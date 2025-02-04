using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenDentBusiness.SheetFramework;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Features.Providers.Dtos;

namespace OpenDentBusiness;

public class SheetFiller {

	///<summary>Gets some data from the database and fills the fields. Input should only be new sheets.
	///dataSet should be prefilled with AccountModules.GetAccount() prior to calling this method when filling fields for statements.
	///Returns empty string if no errors were encountered.  To avoid queries, pass in staticTextData with initialized fields; null fields will 
	///be handled by running the appropriate query.</summary>
	public static string FillFields(Sheet sheet,DataSet dataSet=null,Statement stmt=null,Patient patient=null,Family family=null
		,StaticTextData staticTextData=null,long refAttachProvNum=0) 
	{
		foreach(var param in sheet.Parameters){
			if(param.IsRequired && param.ParamValue==null){
				throw new ApplicationException(Lans.g("Sheet","Parameter not specified for sheet")+": "+param.ParamName);
			}
		}
		Provider provider=null;
		Referral refer=null;
		Deposit deposit=null;
		switch(sheet.SheetType) {
			case SheetTypeEnum.LabelPatient:
				var patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForLabelPatient(sheet,patient);
				break;
			case SheetTypeEnum.LabelCarrier:
				var carrier=Carriers.GetCarrier((long)GetParamByName(sheet,"CarrierNum").ParamValue);
				FillFieldsForLabelCarrier(sheet,carrier);
				break;
			case SheetTypeEnum.LabelReferral:
				Referrals.TryGetReferral((long)GetParamByName(sheet,"ReferralNum").ParamValue,out refer);
				FillFieldsForLabelReferral(sheet,refer);
				break;
			case SheetTypeEnum.ReferralSlip:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				Referrals.TryGetReferral((long)GetParamByName(sheet,"ReferralNum").ParamValue,out refer);
				FillFieldsForReferralSlip(sheet,patient,refer,refAttachProvNum);
				break;
			case SheetTypeEnum.LabelAppointment:
				var appt=Appointments.GetOneApt((long)GetParamByName(sheet,"AptNum").ParamValue);
				patNum=appt.PatNum;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForLabelAppointment(sheet,appt,patient);
				break;
			case SheetTypeEnum.Consent:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForConsent(sheet,patient);
				break;
			case SheetTypeEnum.PatientLetter:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForPatientLetter(sheet,patient);
				break;
			case SheetTypeEnum.ReferralLetter:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				Referrals.TryGetReferral((long)GetParamByName(sheet,"ReferralNum").ParamValue,out refer);
				FillFieldsForReferralLetter(sheet,patient,refer);
				break;
			case SheetTypeEnum.PatientForm:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForPatientForm(sheet,patient);
				break;
			case SheetTypeEnum.RoutingSlip:
				var apt=Appointments.GetOneApt((long)GetParamByName(sheet,"AptNum").ParamValue);
				if(apt==null) {
					return Lans.g("SheetFiller","Appointment no longer exists.");
				}
				patNum=apt.PatNum;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForRoutingSlip(sheet,patient,apt);
				break;
			case SheetTypeEnum.MedicalHistory:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForMedicalHistory(sheet,patient);
				break;
			case SheetTypeEnum.LabSlip:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				var lab=LabCases.GetOne((long)GetParamByName(sheet,"LabCaseNum").ParamValue);
				FillFieldsForLabCase(sheet,patient,lab);
				break;
			case SheetTypeEnum.ExamSheet:
				patNum=(long)GetParamByName(sheet,"PatNum").ParamValue;
				patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
				FillFieldsForExamSheet(sheet,patient);
				break;
			case SheetTypeEnum.DepositSlip:
				deposit=Deposits.GetOne((long)GetParamByName(sheet,"DepositNum").ParamValue);
				FillFieldsForDepositSlip(sheet,deposit);
				break;
			case SheetTypeEnum.Statement:
				patient=(patient==null || patient.PatNum!=sheet.PatNum ? Patients.GetPat(sheet.PatNum) : patient);
				FillFieldsForStatement(sheet,stmt,dataSet,patient,family?.Guarantor);
				break;
			case SheetTypeEnum.MedLabResults:
				patient=(patient==null || patient.PatNum!=sheet.PatNum ? Patients.GetPat(sheet.PatNum) : patient);
				break;
			case SheetTypeEnum.TreatmentPlan:
				patient=(patient==null || patient.PatNum!=sheet.PatNum ? Patients.GetPat(sheet.PatNum) : patient);
				FillFieldsForTreatPlan(sheet,patient);
				break;
			case SheetTypeEnum.PaymentPlan:
				patient=(patient==null || patient.PatNum!=sheet.PatNum ? Patients.GetPat(sheet.PatNum) : patient);
				FillFieldsForPaymentPlan(sheet,patient);
				break;
			case SheetTypeEnum.ERA:
				FillFieldsForERA(sheet);
				break;
			case SheetTypeEnum.ERAGridHeader:
				FillFieldsForERAGridHeader(sheet);
				break;
			case SheetTypeEnum.PatientDashboardWidget:
				patient=(patient==null || patient.PatNum!=sheet.PatNum ? Patients.GetPat(sheet.PatNum) : patient);
				break;
		}
		var listEnumStaticTextFields=GetAllStaticTextFieldsForSheet(sheet);
		var staticTextFieldDependency=StaticTextData.GetStaticTextDependencies(listEnumStaticTextFields);
		var sheetParameterAptNum=GetParamByName(sheet,"AptNum");
		long aptNum=0;
		if(sheetParameterAptNum!=null && sheetParameterAptNum.ParamValue!=null) {
			aptNum=SIn.Long(sheetParameterAptNum.ParamValue.ToString(),throwExceptions:false);
		}
		var listStaticTextReplacements=GetStaticTextReplacements(listEnumStaticTextFields,patient,family,staticTextData,staticTextFieldDependency,aptNum,sheet.SheetType);
		ReplaceStaticTextFieldsInSheet(listStaticTextReplacements,sheet,patient,family);
		FillPatientImages(sheet,patient,staticTextData);
		return "";
	}

	public static List<EnumStaticTextField> GetAllStaticTextFieldsForSheet(Sheet sheet){
		var listEnumStaticTextFields=new List<EnumStaticTextField>();
		for(var i=0;i<sheet.SheetFields.Count;i++) {
			if(sheet.SheetFields[i].FieldType!=SheetFieldType.StaticText) {
				continue;
			}
			var pattern = @"\["//beginning square bracket
			              +@"("//beginning of group
			              +@"\w+"//one or more word characters (letters, digits, or underscores)
			              +@")"//end of group
			              +@"\]";//ending square bracket
			//group 0 is the entire match, including []
			//group 1 is just the part inside (), so without []
			var regex=new Regex(pattern);
			var matchCollection=regex.Matches(sheet.SheetFields[i].FieldValue);
			for(var m=0;m<matchCollection.Count;m++){
				EnumStaticTextField enumStaticTextField;
				try{
					enumStaticTextField=(EnumStaticTextField)Enum.Parse(typeof(EnumStaticTextField),matchCollection[m].Groups[1].Value);
				}
				catch{
					continue;
				}
				listEnumStaticTextFields.Add(enumStaticTextField);
			}
		}
		return listEnumStaticTextFields;
	}

	private static SheetParameter GetParamByName(Sheet sheet,string paramName){
		foreach(var param in sheet.Parameters){
			if(param.ParamName==paramName){
				return param;
			}
		}
		return null;
	}

	public static List<long> GetListProcCodeNumsForStaticText(out List<long> listCodeNumsPano,out List<long> listCodeNumsExam,out List<long> listCodeNumsProphy,out List<long> listCodeNumsPerio,
		out List<long> listCodeNumsBW,out List<long> listCodeNumsSRP)
	{
		//Fill each out parameter with the CodeNums associated to the corresponding fixed code group.
		listCodeNumsPano=ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.PanoFMX);
		listCodeNumsExam=ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.Exam);
		listCodeNumsProphy=ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.Prophy);
		listCodeNumsBW=ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.BW);
		listCodeNumsPerio=ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.Perio);
		listCodeNumsSRP=ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.SRP);
		//Fill listProcCodeNums with all of the code nums found.
		var listProcCodeNums=new List<long>();
		listProcCodeNums.AddRange(listCodeNumsPano);
		listProcCodeNums.AddRange(listCodeNumsBW);
		listProcCodeNums.AddRange(listCodeNumsExam);
		listProcCodeNums.AddRange(listCodeNumsPerio);
		listProcCodeNums.AddRange(listCodeNumsProphy);
		listProcCodeNums.AddRange(listCodeNumsSRP);
		return listProcCodeNums;
	}

	///<summary>Pat can be null sometimes.  For example, in deposit slip.</summary>
	public static List<StaticTextReplacement> GetStaticTextReplacements(List<EnumStaticTextField> listEnumStaticTextFields,Patient patient,Family family,StaticTextData staticTextData,StaticTextFieldDependency staticTextFieldDependency,long aptNum,SheetTypeEnum sheetType=SheetTypeEnum.None) {
		#region Instantiate Strings
		var activeProblems="";
		var address="";
		var activeAllergies="";
		var apptDateMonthSpelled="";
		var apptModNote="";//This is the Appointment Module Note in the Appointments for window.  http://www.opendental.com/manual/apptsched.html
		var apptProcs="";
		var apptProvNameFormal="";
		var apptsAllFuture="";
		var birthdate="";
		var carrierAddress="";
		var carrier2Address="";
		var carrierCityStZip="";
		var carrier2CityStZip="";
		var carrierName="";
		var carrier2Name="";
		var clinicPatDescription="";
		var clinicPatAddress="";
		var clinicPatCityStZip="";
		var clinicPatPhone="";
		var clinicCurDescription="";
		var clinicCurAddress="";
		var clinicCurCityStZip="";
		var clinicCurPhone="";
		var dateFirstVisit="";
		var dateOfLastSavedTP="";
		var dateRecallDue="";
		var dateTimeLastAppt="";
		var dateLastAppt="";
		var dateLastBW="";
		var dateLastExam="";
		var dateLastPerio="";
		var dateLastPanoFMX="";
		var dateLastProphy="";
		var dateLastSrp="";
		var dueForBWYN="";
		var dueForPanoYN="";
		var famPopups="";
		var famRecallDue="";
		var pronounHeSheThey="";
		var pronounheshethey="";
		var pronounHimHerThem="";
		var pronounhimherthem="";
		var pronounHimselfHerselfThemself="";
		var pronounhimselfherselfthemself="";
		var pronounHisHerTheir="";
		var pronounhishertheir="";
		var pronounHisHersTheirs="";
		var pronounhisherstheirs="";
		var guarantorHmPhone="";
		var guarantorNameF="";
		var guarantorNameFL="";
		var guarantorNameL="";
		var guarantorNamePref="";
		var guarantorNameLF="";
		var guarantorWirelessPhone="";
		var guarantorWkPhone="";
		var insAnnualMax="";
		var insDeductible="";
		var insDeductibleUsed="";
		var insEmployer="";
		var insFeeSchedule="";
		var insPending="";
		var insPercentages="";
		var insPlanGroupNumber="";
		var insPlanGroupName="";
		var insPlanNote="";
		var insSubNote="";
		var insSubBirthDate="";
		var insRemaining="";
		var insUsed="";
		var insFreqBW="";
		var insFreqExams="";
		var insFreqPanoFMX="";
		var insType=""; //(ppo, etc)
		var ins2AnnualMax="";
		var ins2Deductible="";
		var ins2DeductibleUsed="";
		var ins2Employer="";
		var ins2FreqBW="";
		var ins2FreqExams="";
		var ins2FreqPanoFMX="";
		var ins2PlanGroupNumber="";
		var ins2PlanGroupName="";
		var ins2Pending="";
		var ins2Percentages="";
		var ins2Remaining="";
		var ins2Used="";
		var medicalSummary="";
		var currentMedications="";
		var namePreferredOrFirst="";
		var nextSchedApptDateT="";
		var nextSchedApptDate="";
		var nextSchedApptsFam="";
		var patientPortalCredentials="";
		var phone="";
		var plannedAppointmentInfo="";
		var premedicateYN="";
		var recallInterval="";
		var recallScheduledYN="";
		var referredFrom=""; //(just one)
		var referredTo=""; //(typically Drs. could be multiline. Include date)
		var serviceNote="";
		var subscriberId="";
		var subscriberNameFL="";
		var subscriber2NameFL="";
		var tpResponsPartyAddress="";
		var tpResponsPartyCityStZip="";
		var tpResponsPartyNameFL="";
		var treatmentNote="";
		var treatmentPlanProcs="";
		var treatmentPlanProcsPriority="";
		#endregion
		ProviderDto providerPri=null;
		PatientNote patientNote=null;
		#region Patient Fields
		if(patient!=null) {
			//Use patient's preferred name if they have one, otherwise default to first name.
			namePreferredOrFirst=patient.FName;
			if(!string.IsNullOrWhiteSpace(patient.Preferred)) {
				namePreferredOrFirst=patient.Preferred;
			}
			#region Procedure CodeNums
			//Procedure CodeNums-------------------------------------------------------------------------------------------------------------
			var listProcCodeNums=GetListProcCodeNumsForStaticText(out var listCodeNumsPano,out var listCodeNumsExam,out var listCodeNumsProphy,
				out var listCodeNumsPerio,out var listCodeNumsBW,out var listCodeNumsSRP);
			#endregion
			family=family??Patients.GetFamily(patient.PatNum);
			staticTextData=StaticTextData.GetStaticTextData(staticTextFieldDependency,patient,family,listProcCodeNums,staticTextData);
			premedicateYN=Lans.g("All","No");
			recallScheduledYN=Lans.g("All","No");
			dueForBWYN=Lans.g("All","No");
			dueForPanoYN=Lans.g("All","No");
			if(patient.Premed) {
				premedicateYN=Lans.g("All","Yes");
			}
			patientNote=staticTextData.PatNote;
			medicalSummary=patientNote.Medical;
			treatmentNote=patientNote.Treatment;
			apptModNote=patient.ApptModNote;
			#region Gender
			#region Male
			var pronounHe="He";
			var pronounHim="Him";
			var pronounHimself="Himself";
			var pronounHis="His";
			#endregion
			#region Female
			var pronounShe="She";
			var pronounHer="Her";
			var pronounHerself="Herself";
			var pronounHers="Hers";
			#endregion
			#region Intersex
			var pronounThey="They";
			var pronounThem="Them";
			var pronounThemself="Themself";
			var pronounTheir="Their";
			var pronounTheirs="Theirs";
			#endregion
			#region Unknown
			var pronounUnknown="The patient";
			var pronounUnknowns="The patient's";
			#endregion
			var pronounPreferred=Patients.GetPronoun(patient.Gender,patientNote.Pronoun);
			switch(pronounPreferred) {
				case PronounPreferred.HeHim:
					pronounHeSheThey=Lans.g("PatientInfo",pronounHe);
					pronounheshethey=Lans.g("PatientInfo",pronounHe.ToLower());
					pronounHimHerThem=Lans.g("PatientInfo",pronounHim);
					pronounhimherthem=Lans.g("PatientInfo",pronounHim.ToLower());
					pronounHimselfHerselfThemself=Lans.g("PatientInfo",pronounHimself);
					pronounhimselfherselfthemself=Lans.g("PatientInfo",pronounHimself.ToLower());
					pronounHisHerTheir=Lans.g("PatientInfo",pronounHis);
					pronounhishertheir=Lans.g("PatientInfo",pronounHis.ToLower());
					pronounHisHersTheirs=Lans.g("PatientInfo",pronounHis);
					pronounhisherstheirs=Lans.g("PatientInfo",pronounHis.ToLower());
					break;
				case PronounPreferred.SheHer:
					pronounHeSheThey=Lans.g("PatientInfo",pronounShe);
					pronounheshethey=Lans.g("PatientInfo",pronounShe.ToLower());
					pronounHimHerThem=Lans.g("PatientInfo",pronounHer);
					pronounhimherthem=Lans.g("PatientInfo",pronounHer.ToLower());
					pronounHimselfHerselfThemself=Lans.g("PatientInfo",pronounHerself);
					pronounhimselfherselfthemself=Lans.g("PatientInfo",pronounHerself.ToLower());
					pronounHisHerTheir=Lans.g("PatientInfo",pronounHer);
					pronounhishertheir=Lans.g("PatientInfo",pronounHer.ToLower());
					pronounHisHersTheirs=Lans.g("PatientInfo",pronounHers);
					pronounhisherstheirs=Lans.g("PatientInfo",pronounHers.ToLower());
					break;
				case PronounPreferred.TheyThem:
					pronounHeSheThey=Lans.g("PatientInfo",pronounThey);
					pronounheshethey=Lans.g("PatientInfo",pronounThey.ToLower());
					pronounHimHerThem=Lans.g("PatientInfo",pronounThem);
					pronounhimherthem=Lans.g("PatientInfo",pronounThem.ToLower());
					pronounHimselfHerselfThemself=Lans.g("PatientInfo",pronounThemself);
					pronounhimselfherselfthemself=Lans.g("PatientInfo",pronounThemself.ToLower());
					pronounHisHerTheir=Lans.g("PatientInfo",pronounTheir);
					pronounhishertheir=Lans.g("PatientInfo",pronounTheir.ToLower());
					pronounHisHersTheirs=Lans.g("PatientInfo",pronounTheirs);
					pronounhisherstheirs=Lans.g("PatientInfo",pronounTheirs.ToLower());
					break;
				case PronounPreferred.None:
					pronounHeSheThey=Lans.g("PatientInfo",pronounUnknown);
					pronounheshethey=Lans.g("PatientInfo",pronounUnknown.ToLower());
					pronounHimHerThem=Lans.g("PatientInfo",pronounUnknown);
					pronounhimherthem=Lans.g("PatientInfo",pronounUnknown.ToLower());
					pronounHimselfHerselfThemself=Lans.g("PatientInfo",pronounUnknown);
					pronounhimselfherselfthemself=Lans.g("PatientInfo",pronounUnknown.ToLower());
					pronounHisHerTheir=Lans.g("PatientInfo",pronounUnknowns);
					pronounhishertheir=Lans.g("PatientInfo",pronounUnknowns.ToLower());
					pronounHisHersTheirs=Lans.g("PatientInfo",pronounUnknowns);
					pronounhisherstheirs=Lans.g("PatientInfo",pronounUnknowns.ToLower());
					break;
			}
			#endregion
			#region Guarantor
			var guar=family.Guarantor??Patients.GetPat(patient.Guarantor);
			if(guar!=null) {
				guarantorNameF=guar.FName;
				guarantorNameFL=guar.GetNameFL();
				guarantorNameL=guar.LName;
				guarantorNameLF=guar.GetNameLF();
				guarantorNamePref=guar.Preferred;
				guarantorHmPhone=guar.HmPhone;
				guarantorWirelessPhone=guar.WirelessPhone;
				guarantorWkPhone=guar.WkPhone;
			}
			#endregion
			#region Address
			address=patient.Address;
			if(patient.Address2!="") {
				address+=", "+patient.Address2;
			}
			#endregion
			#region Birthdate
			birthdate=patient.Birthdate.ToShortDateString();
			if(patient.Birthdate.Year<1880) {
				birthdate="";
			}
			#endregion
			#region Date First Visit
			dateFirstVisit=patient.DateFirstVisit.ToShortDateString();
			if(patient.DateFirstVisit.Year<1880) {
				dateFirstVisit="";
			}
			#endregion
			#region Treatment Plan Procs
			//todo some day: move this section down to TP section
			List<Procedure> procsList=null;//there is another variable that does the same thing. Carefully combine them.
			if(listEnumStaticTextFields.Contains(EnumStaticTextField.treatmentPlanProcs)
			   || listEnumStaticTextFields.Contains(EnumStaticTextField.plannedAppointmentInfo)
			   || listEnumStaticTextFields.Contains(EnumStaticTextField.treatmentPlanProcsPriority))
			{
				procsList=staticTextData.ListProceduresPat;
				if(staticTextData.ListSelectedTpProcs.Count>0) {
					procsList=staticTextData.ListSelectedTpProcs;
				}
				if(listEnumStaticTextFields.Contains(EnumStaticTextField.treatmentPlanProcs) || listEnumStaticTextFields.Contains(EnumStaticTextField.treatmentPlanProcsPriority)){
					var listProceduresTPs=Procedures.GetListTPandTPi(procsList);//sorted by priority, then toothnum
					for(var i=0;i<listProceduresTPs.Count;i++) {
						if(listProceduresTPs[i].ProcStatus!=ProcStat.TP) {
							continue;
						}
						if(treatmentPlanProcs!="") {
							treatmentPlanProcs+="\r\n";
							treatmentPlanProcsPriority+="\r\n";
						}
						//Figure out what the procedure description will be like.
						var procDescript=ProcedureCodes.GetStringProcCode(listProceduresTPs[i].CodeNum)+", "
						                                                                               +Procedures.GetDescription(listProceduresTPs[i])+", "
						                                                                               +listProceduresTPs[i].ProcFee.ToString("c");
						//Get the procedure's priority.
						var priority=Defs.GetName(DefCat.TxPriorities,listProceduresTPs[i].Priority);
						if(priority=="") {
							priority=Lans.g("TreatmentPlans","No priority");
						}
						//Set the corresponding static field text.
						treatmentPlanProcsPriority+=priority+", "+procDescript;
						treatmentPlanProcs+=procDescript;
					}
				}
			}
			#endregion
			serviceNote=patientNote.Service;
			#region Referrals
			var RefAttachList=staticTextData.ListRefAttaches;
			var tempReferralFrom = Referrals.GetReferralForPat(patient.PatNum,RefAttachList);
			if(tempReferralFrom!=null) {
				if(tempReferralFrom.IsDoctor) {
					referredFrom+=tempReferralFrom.FName+" "+tempReferralFrom.LName+" "+tempReferralFrom.Title+" : "
					              +Defs.GetName(DefCat.ProviderSpecialties,tempReferralFrom.Specialty);
				}
				else {
					referredFrom+=tempReferralFrom.FName+" "+tempReferralFrom.LName;
				}
			}
			for(var i=0;i<RefAttachList.Count;i++) {
				if(RefAttachList[i].RefType!=ReferralType.RefTo) {
					continue;
				}
				Referral tempRef;
				if(Referrals.TryGetReferral(RefAttachList[i].ReferralNum,out tempRef)) {
					if(tempRef.IsDoctor) {
						referredTo+=tempRef.FName+" "+tempRef.LName+" "+tempRef.Title+" : "+Defs.GetName(DefCat.ProviderSpecialties,tempRef.Specialty)+" "
						            +RefAttachList[i].RefDate.ToShortDateString()+"\r\n";
					}
					else {
						referredTo+=tempRef.FName+" "+tempRef.LName+" "+RefAttachList[i].RefDate.ToShortDateString()+"\r\n";
					}
				}
			}
			#endregion
			#region Insurance
			//Insurance-------------------------------------------------------------------------------------------------------------------
			var listPatPlans=staticTextData.ListPatPlans;
			var listInsSubs=staticTextData.ListInsSubs;
			var listInsPlans=staticTextData.ListInsPlans;
			if(!PatPlans.IsPatPlanListValid(listPatPlans,listInsSubs:listInsSubs,listInsPlans:listInsPlans)) {
				//need to validate due to call to GetHistList below
				listPatPlans=PatPlans.Refresh(patient.PatNum);
			}
			var ordinal=PatPlans.GetOrdinal(PriSecMed.Primary,listPatPlans,listInsPlans,listInsSubs);
			if(ordinal==0) { //No primary dental plan. See if they have a medical plan instead.
				ordinal=PatPlans.GetOrdinal(PriSecMed.Medical,listPatPlans,listInsPlans,listInsSubs);
			}
			var subNum=PatPlans.GetInsSubNum(listPatPlans,ordinal);
			var patPlanNum=PatPlans.GetPatPlanNum(subNum,listPatPlans);
			var sub=InsSubs.GetSub(subNum,listInsSubs);
			Patient subscriber;
			if(sub.Subscriber==patient.PatNum) {
				subscriber=patient;
			}
			else if(guar!=null && sub.Subscriber==guar.PatNum) {
				subscriber=guar;
			}
			else {
				subscriber=Patients.GetPat(sub.Subscriber);
			}
			if(subscriber!=null) {
				insSubBirthDate=subscriber.Birthdate.ToShortDateString();
			}
			InsPlan plan=null;
			if(sub!=null) {
				plan=InsPlans.GetPlan(sub.PlanNum,listInsPlans);
				insSubNote=sub.SubscNote;
			}
			Carrier carrier=null;
			var benefitList=staticTextData.ListBenefits;
			var histList=staticTextData.HistList;
			double doubAnnualMax;
			double doubDeductible;
			double doubDeductibleUsed;
			double doubPending;
			double doubRemain;
			double doubUsed;
			if(plan!=null) {
				insFeeSchedule=FeeScheds.GetDescription(plan.FeeSched);
				insPlanGroupName=plan.GroupName;
				insPlanGroupNumber=plan.GroupNum;
				insPlanNote=plan.PlanNote;
				carrier=Carriers.GetCarrier(plan.CarrierNum);
				carrierName=carrier.CarrierName;
				carrierAddress=carrier.Address;
				if(carrier.Address2!="") {
					carrierAddress+=", "+carrier.Address2;
				}
				carrierCityStZip=carrier.City+", "+carrier.State+"  "+carrier.Zip;
				subscriberId=sub.SubscriberID;
				if(subscriber!=null) {
					subscriberNameFL=subscriber.GetNameFL();
				}
				doubAnnualMax=Benefits.GetAnnualMaxDisplay(benefitList,plan.PlanNum,patPlanNum,false);
				doubRemain=-1;
				if(doubAnnualMax!=-1) {
					insAnnualMax=doubAnnualMax.ToString("c");
					doubRemain=doubAnnualMax;
				}
				doubDeductible=Benefits.GetDeductGeneralDisplay(benefitList,plan.PlanNum,patPlanNum,BenefitCoverageLevel.Individual);
				if(doubDeductible!=-1) {
					insDeductible=doubDeductible.ToString("c");
				}
				doubDeductibleUsed=InsPlans.GetDedUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,listInsPlans,BenefitCoverageLevel.Individual,patient.PatNum);
				if(doubDeductibleUsed!=-1) {
					insDeductibleUsed=doubDeductibleUsed.ToString("c");
				}
				doubPending=InsPlans.GetPendingDisplay(histList,DateTime.Today,plan,patPlanNum,-1,patient.PatNum,subNum,benefitList);
				if(doubPending!=-1) {
					insPending=doubPending.ToString("c");
					if(doubRemain!=-1) {
						doubRemain-=doubPending;
					}
				}
				doubUsed=InsPlans.GetInsUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,listInsPlans,benefitList,patient.PatNum,subNum);
				if(doubUsed!=-1) {
					insUsed=doubUsed.ToString("c");
					if(doubRemain!=-1) {
						doubRemain-=doubUsed;
					}
				}
				if(doubRemain!=-1) {
					insRemaining=doubRemain.ToString("c");
				}
				for(var j=0;j<benefitList.Count;j++) {
					if(benefitList[j].PlanNum != plan.PlanNum) {
						continue;
					}
					if(benefitList[j].BenefitType != InsBenefitType.CoInsurance) {
						continue;
					}
					if(insPercentages!="") {
						insPercentages+=",  ";
					}
					insPercentages+=CovCats.GetDesc(benefitList[j].CovCatNum)+" "+benefitList[j].Percent.ToString()+"%";
				}
				insFreqBW=Benefits.GetFrequencyDisplay(FrequencyType.BW,benefitList,plan.PlanNum);
				insFreqExams=Benefits.GetFrequencyDisplay(FrequencyType.Exam,benefitList,plan.PlanNum);
				insFreqPanoFMX=Benefits.GetFrequencyDisplay(FrequencyType.PanoFMX,benefitList,plan.PlanNum);
				switch(plan.PlanType) {//(ppo, etc)
					case "p":
						insType=Lans.g("InsurancePlans","PPO Percentage");
						break;
					case "f":
						insType=Lans.g("InsurancePlans","Medicaid or Flat Copay");
						break;
					case "c":
						insType=Lans.g("InsurancePlans","Capitation");
						break;
					case "":
						insType=Lans.g("InsurancePlans","Category Percentage");
						break;
				}
				insEmployer=Employers.GetEmployer(plan.EmployerNum).EmpName; //blank if no Employer listed
			}
			subNum=PatPlans.GetInsSubNum(listPatPlans,PatPlans.GetOrdinal(PriSecMed.Secondary,listPatPlans,listInsPlans,listInsSubs));
			patPlanNum=PatPlans.GetPatPlanNum(subNum,listPatPlans);
			sub=InsSubs.GetSub(subNum,listInsSubs);
			if(sub!=null) {
				plan=InsPlans.GetPlan(sub.PlanNum,listInsPlans);
			}
			if(plan!=null) { //secondary insurance
				ins2PlanGroupName=plan.GroupName;
				ins2PlanGroupNumber=plan.GroupNum;
				carrier=Carriers.GetCarrier(plan.CarrierNum);
				carrier2Name=carrier.CarrierName;
				carrier2Address=carrier.Address;
				if(carrier.Address2!="") {
					carrier2Address+=", "+carrier.Address2;
				}
				carrier2CityStZip=carrier.City+", "+carrier.State+"  "+carrier.Zip;
				//subscriberId=plan.SubscriberID;
				subscriber2NameFL=Patients.GetLim(sub.Subscriber).GetNameFL();
				doubAnnualMax=Benefits.GetAnnualMaxDisplay(benefitList,plan.PlanNum,patPlanNum,false);
				doubRemain=-1;
				if(doubAnnualMax!=-1) {
					ins2AnnualMax=doubAnnualMax.ToString("c");
					doubRemain=doubAnnualMax;
				}
				doubDeductible=Benefits.GetDeductGeneralDisplay(benefitList,plan.PlanNum,patPlanNum,BenefitCoverageLevel.Individual);
				if(doubDeductible!=-1) {
					ins2Deductible=doubDeductible.ToString("c");
				}
				doubDeductibleUsed=InsPlans.GetDedUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,listInsPlans,BenefitCoverageLevel.Individual,patient.PatNum);
				if(doubDeductibleUsed!=-1) {
					ins2DeductibleUsed=doubDeductibleUsed.ToString("c");
				}
				doubPending=InsPlans.GetPendingDisplay(histList,DateTime.Today,plan,patPlanNum,-1,patient.PatNum,subNum,benefitList);
				if(doubPending!=-1) {
					ins2Pending=doubPending.ToString("c");
					if(doubRemain!=-1) {
						doubRemain-=doubPending;
					}
				}
				doubUsed=InsPlans.GetInsUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,listInsPlans,benefitList,patient.PatNum,subNum);
				if(doubUsed!=-1) {
					ins2Used=doubUsed.ToString("c");
					if(doubRemain!=-1) {
						doubRemain-=doubUsed;
					}
				}
				if(doubRemain!=-1) {
					ins2Remaining=doubRemain.ToString("c");
				}
				for(var j=0;j<benefitList.Count;j++) {
					if(benefitList[j].PlanNum != plan.PlanNum) {
						continue;
					}
					if(benefitList[j].BenefitType != InsBenefitType.CoInsurance) {
						continue;
					}
					if(ins2Percentages!="") {
						ins2Percentages+=",  ";
					}
					ins2Percentages+=CovCats.GetDesc(benefitList[j].CovCatNum)+" "+benefitList[j].Percent.ToString()+"%";
				}
				ins2FreqBW=Benefits.GetFrequencyDisplay(FrequencyType.BW,benefitList,plan.PlanNum);
				ins2FreqExams=Benefits.GetFrequencyDisplay(FrequencyType.Exam,benefitList,plan.PlanNum);
				ins2FreqPanoFMX=Benefits.GetFrequencyDisplay(FrequencyType.PanoFMX,benefitList,plan.PlanNum);
				ins2Employer=Employers.GetEmployer(plan.EmployerNum).EmpName;//blank if no Employer listed
			}
			#endregion
			#region Treatment Plan
			//Treatment plan-----------------------------------------------------------------------------------------------------------
			var treatPlanList=staticTextData.ListTreatPlans;
			TreatPlan treatPlan=null;
			if(treatPlanList.Count>0) {
				treatPlan=treatPlanList[treatPlanList.Count-1].Copy();
				dateOfLastSavedTP=treatPlan.DateTP.ToShortDateString();
				var patRespParty=Patients.GetPat(treatPlan.ResponsParty);
				if(patRespParty!=null) {
					tpResponsPartyAddress=patRespParty.Address;
					if(patRespParty.Address2!="") {
						tpResponsPartyAddress+=", "+patRespParty.Address2;
					}
					tpResponsPartyCityStZip=patRespParty.City+", "+patRespParty.State+"  "+patRespParty.Zip;
					tpResponsPartyNameFL=patRespParty.GetNameFL();
				}
			}
			#endregion
			#region Procedure Log
			//Procedure Log-------------------------------------------------------------------------------------------------------------
			var proceduresList=staticTextData.ListProceduresSome;
			var dBW=DateTime.MinValue;
			var dExam=DateTime.MinValue;
			var dPerio=DateTime.MinValue;
			var dPanoFMX=DateTime.MinValue;
			var dProphy=DateTime.MinValue;
			var dSRP=DateTime.MinValue;
			for(var i=0;i<proceduresList.Count;i++) {
				var proc = proceduresList[i];//cache Proc to speed up process
				if(proc.ProcStatus!=ProcStat.C
				   && proc.ProcStatus!=ProcStat.EC
				   && proc.ProcStatus!=ProcStat.EO) {
					continue;//only look at completed or existing procedures
				}
				if(listCodeNumsBW.Contains(proc.CodeNum) && proc.ProcDate>dBW){ //newest
					dBW=proc.ProcDate;
					dateLastBW=proc.ProcDate.ToShortDateString();
				}
				if(listCodeNumsExam.Contains(proc.CodeNum) && proc.ProcDate>dExam) //newest
				{
					dExam=proc.ProcDate;
					dateLastExam=proc.ProcDate.ToShortDateString();
				}
				if(listCodeNumsPerio.Contains(proc.CodeNum)//Periodontal Maintenance 
				   && proc.ProcDate>dPerio)//newest 
				{
					dPerio=proc.ProcDate;
					dateLastPerio=proc.ProcDate.ToShortDateString();
				}
				if((listCodeNumsPano.Contains(proc.CodeNum))//panoramic film
				   && proc.ProcDate>dPanoFMX) //newest
				{
					dPanoFMX=proc.ProcDate;
					dateLastPanoFMX=proc.ProcDate.ToShortDateString();
				}
				if(listCodeNumsProphy.Contains(proc.CodeNum) && proc.ProcDate>dProphy) //newest
				{
					dProphy=proc.ProcDate;
					dateLastProphy=proc.ProcDate.ToShortDateString();
				}
				if(listCodeNumsSRP.Contains(proc.CodeNum) && proc.ProcDate>dSRP) {
					dSRP=proc.ProcDate;
					dateLastSrp=proc.ProcDate.ToShortDateString();
				}
			}
			#endregion
			#region Recall
			//Recall--------------------------------------------------------------------------------------------------------------------
			var listRecalls=staticTextData.ListRecallsForFam.FindAll(x => x.PatNum==patient.PatNum);
			for(var i=0;i<listRecalls.Count;i++) {
				//don't care about recalls in the future, or without a due date as these recalls won't even show in the recall list.
				if(listRecalls[i].DateDue>DateTime.Today || listRecalls[i].DateDue.Year<1880) { 
					continue;
				}
				if(listRecalls[i].IsDisabled) { //don't care about recalls that are disabled.
					continue;
				}
				if(listRecalls[i].DisableUntilDate>DateTime.Today) { //don't care about recalls that are disabled until the future
					continue;
				}
				if(listRecalls[i].DisableUntilBalance>0 && listRecalls[i].DisableUntilBalance<(family.ListPats[0].BalTotal-family.ListPats[0].InsEst)) { 
					//don't care about recalls if they are disabled due to family balance
					continue;
				}
				var listProcCodes=RecallTypes.GetProcs(listRecalls[i].RecallTypeNum);
				for(var j=0;j<listProcCodes.Count;j++) {
					if(listProcCodes[j]=="D0210"//intraoral - complete series (including bitewings) 				   
					   ||listProcCodes[j]=="D0270"//bitewing - single film													   
					   ||listProcCodes[j]=="D0272"//bitewings - two films														   
					   ||listProcCodes[j]=="D0274"//bitewings - four films													   
					   ||listProcCodes[j]=="D0277"//vertical bitewings - 7 to 8 films											   
					   ||listProcCodes[j]=="D0273")//bitewings - three films
					{
						dueForBWYN=Lans.g("All","Yes");
					}
					if(listProcCodes[j]=="D0210"//intraoral - complete series (including bitewings)				   
					   ||listProcCodes[j]=="D0330")//panoramic film
					{
						dueForPanoYN=Lans.g("All","Yes");
					}
				}
			}
			var recall=Recalls.GetRecallProphyOrPerio(patient.PatNum,listRecalls: staticTextData.ListRecallsForFam);
			if(recall!=null && !recall.IsDisabled) {
				if(recall.DateDue.Year>1880) {
					dateRecallDue=recall.DateDue.ToShortDateString();
				}
				recallInterval=recall.RecallInterval.ToString();
				if(recall.DateScheduled>=DateTime.Today) {
					recallScheduledYN=Lans.g("All","Yes");
				}
			}
			for(var i=0;i<family.ListPats.Length;i++) {
				recall=Recalls.GetRecallProphyOrPerio(family.ListPats[i].PatNum,true,listRecalls: staticTextData.ListRecallsForFam);
				if(recall==null || recall.IsDisabled || recall.DateDue==DateTime.MinValue || recall.DateDue>=DateTime.Today) {
					continue;
				}
				if(famRecallDue!="") {
					famRecallDue+="\r\n";
				}
				famRecallDue+=family.ListPats[i].FName+", "+recall.DateDue.ToShortDateString()+" "+RecallTypes.GetDescription(recall.RecallTypeNum);
			}
			#endregion
			#region Appointments
			//Appointments--------------------------------------------------------------------------------------------------------------
			var apptList=staticTextData.ListAppts;
			var apptFutureList=staticTextData.ListFutureApptsForFam.FindAll(x => x.PatNum==patient.PatNum);
			for(var i=0;i<apptList.Count;i++) {
				if(apptList[i].AptStatus != ApptStatus.Scheduled
				   && apptList[i].AptStatus != ApptStatus.Complete
				   && apptList[i].AptStatus != ApptStatus.None) {
					continue;
				}
				if(apptList[i].AptDateTime < DateTime.Now) {
					//this will happen repeatedly up until the most recent.
					dateTimeLastAppt=apptList[i].AptDateTime.ToShortDateString()+"  "+apptList[i].AptDateTime.ToShortTimeString();
					dateLastAppt=apptList[i].AptDateTime.ToShortDateString();
				}
				else {//after now
					if(nextSchedApptDateT=="") {//only the first one found
						nextSchedApptDateT=apptList[i].AptDateTime.ToShortDateString()+"  "+apptList[i].AptDateTime.ToShortTimeString();
						nextSchedApptDate=apptList[i].AptDateTime.ToShortDateString();
						break;//we're done with the list now.
					}
				}
			}
			for(var i=0;i<apptFutureList.Count;i++) {//cannot be combined in loop above because of the break in the loop.
				apptsAllFuture+=apptFutureList[i].AptDateTime.ToShortDateString()+" "+apptFutureList[i].AptDateTime.ToShortTimeString()+" : "+apptFutureList[i].ProcDescript+"\r\n";
			}
			for(var i=0;i<family.ListPats.Length;i++) {
				var futAptsList=staticTextData.ListFutureApptsForFam.FindAll(x => x.PatNum==family.ListPats[i].PatNum);
				if(futAptsList.Count>0) {//just gets one future appt for each person
					nextSchedApptsFam+=family.ListPats[i].FName+": "+futAptsList[0].AptDateTime.ToShortDateString()+" "+futAptsList[0].AptDateTime.ToShortTimeString()+" : "+futAptsList[0].ProcDescript+"\r\n";
				}
			}
			if(listEnumStaticTextFields.Contains(EnumStaticTextField.plannedAppointmentInfo)){
				var plannedAppt=staticTextData.ListPlannedAppts?.OrderBy(x => x.ItemOrderPlanned)?.FirstOrDefault();
				for(var i=0;i<apptList.Count;i++) {
					if(plannedAppt!=null && apptList[i].AptNum==plannedAppt.AptNum) {
						plannedAppointmentInfo=Lans.g("Appointments","Procedures:")+" ";
						plannedAppointmentInfo+=apptList[i].ProcDescript+"\r\n";
						var minutesTotal=apptList[i].Pattern.Length*5;
						var hours=minutesTotal/60;//automatically rounds down
						var minutes=minutesTotal-hours*60;
						plannedAppointmentInfo+=Lans.g("Appointments","Appt Length:")+" ";
						if(hours>0) {
							plannedAppointmentInfo+=hours.ToString()+" "+Lans.g("Appointments","hours")+", ";
						}
						plannedAppointmentInfo+=minutes.ToString()+" "+Lans.g("Appointments","min")+"\r\n";
					}
				}
			}
			if(aptNum>0){
				var appointment=staticTextData.ListAppts.FirstOrDefault(x => x.AptNum==aptNum);
				if(appointment!=null) {
					if(appointment.AptDateTime.Year > 1880) {
						//Month spelled followed by day and year. E.g. April 28, 2018
						apptDateMonthSpelled=appointment.AptDateTime.ToString("MMMM dd, yyyy");
					}
					if(!staticTextData.ListProceduresPat.IsNullOrEmpty()) {
						var listProcedures=staticTextData.ListProceduresPat.FindAll(x => x.AptNum==appointment.AptNum);
						apptProcs=string.Join("\r\n",listProcedures.Select(x => Procedures.GetDescriptionForLetter(x)));
					}
					var provider=Providers.GetById(appointment.ProvNum);
					if(provider!=null) {
						apptProvNameFormal=provider.FormalName;//Matches the format of priProvNameFormal.
					}
				}
			}
			#endregion
			#region Clinic
			providerPri=Providers.GetById(Patients.GetProvNum(patient));
			if(providerPri==null) {//Rare, temporary fix. We have previously seen this issue happen when a provider is removed from the DB somehow.
				providerPri=new ProviderDto() {//If Provider is missing this will throw a UE where we attempt to use GetFullName(...)
					FirstName="",
					LastName="",
					MiddleName="",//This is the lynch pin to ensuring GetFullName(...) does not fail when looking for a missing Provider.
				};
			}
			//Pat Clinic-------------------------------------------------------------------------------------------------------------
			var clinic=Clinics.GetClinic(patient.ClinicNum);
			clinicPatDescription=clinic.Description;
			clinicPatAddress=clinic.AddressLine1;
			if(clinic.AddressLine2!="") {
				clinicPatAddress+=", "+clinic.AddressLine2;
			}
			clinicPatCityStZip=clinic.City+", "+clinic.State+"  "+clinic.Zip;
			phone=clinic.PhoneNumber;
			clinicPatPhone=TelephoneNumbers.ReFormat(phone);
			#endregion
			#region Diseases/Allergies
			var listDiseases=staticTextData.ListDiseases;
			for(var i=0;i<listDiseases.Count;i++) {
				if(activeProblems!="") {
					activeProblems+=", ";
				}
				activeProblems+=DiseaseDefs.GetName(listDiseases[i].DiseaseDefNum);
			}
			var listAllergies=staticTextData.ListAllergies;
			for(var i=0;i<listAllergies.Count;i++) {
				if(activeAllergies!="") {
					activeAllergies+=", ";
				}
				try {//unknown methods can cause the allergy to be deleted sometimes
					activeAllergies+=AllergyDefs.GetDescription(listAllergies[i].AllergyDefNum);
				}
				catch {
					continue;
				}
			}
			#endregion
			#region Medication
			var listMedicationPats=staticTextData.ListMedicationPats;
			foreach(var medPat in listMedicationPats) {
				//Default to using the MedicationPat description (e.g. an eRx from NewCrop that is not in the Medication table).
				var medDescript=medPat.MedDescript??"";
				//Prefer the description associated to the Medication object if available.
				var medCur=Medications.GetMedication(medPat.MedicationNum);
				if(medCur!=null) {
					medDescript=medCur.MedName;
					if(medCur.MedicationNum!=medCur.GenericNum) {
						var medGeneric=Medications.GetMedication(medCur.GenericNum);
						medDescript+=(medGeneric==null) ? "" : " ("+medGeneric.MedName+")";
					}
				}
				currentMedications+=((currentMedications=="") ? "" : ", ")+medDescript;
			}
			#endregion
			#region Popups
			//patient, family & superfam popups
			var listFamPopups=staticTextData.ListFamPopups;
			for(var i=0;i<listFamPopups.Count;i++) {
				if(listFamPopups[i].DateTimeDisabled!=DateTime.MinValue && listFamPopups[i].DateTimeDisabled < DateTime.Now) { //If disabled
					continue;
				}
				if(famPopups!="") {
					famPopups+=", ";
				}
				famPopups+=listFamPopups[i].Description;
			}
			#endregion
		}//End of if(pat!=null)
		if(patient!=null || sheetType==SheetTypeEnum.DepositSlip) {
			//Populate current clinic fields if a patient exists or if the sheet is a Deposit Slip.
			var clinicCur=Clinics.GetClinic(Clinics.ClinicNum);
			clinicCurDescription=clinicCur.Description;
			clinicCurAddress=clinicCur.AddressLine1;
			if(clinicCur.AddressLine2!="") {
				clinicCurAddress+=", "+clinicCur.AddressLine2;
			}
			clinicCurCityStZip=clinicCur.City+", "+clinicCur.State+"  "+clinicCur.Zip;
			phone=clinicCur.PhoneNumber;
			clinicCurPhone=TelephoneNumbers.ReFormat(phone);
		}
		#endregion
		var listStaticTextReplacements=new List<StaticTextReplacement>();
		for(var i=0;i<listEnumStaticTextFields.Count;i++){
			var staticTextReplacement=new StaticTextReplacement();
			staticTextReplacement.StaticTextField=listEnumStaticTextFields[i];
			switch(listEnumStaticTextFields[i]) {
				case EnumStaticTextField.dateToday: staticTextReplacement.NewValue=DateTime.Today.ToShortDateString(); break;
				case EnumStaticTextField.dateTodayLong: staticTextReplacement.NewValue=DateTime.Today.ToLongDateString(); break;
				case EnumStaticTextField.practiceTitle: staticTextReplacement.NewValue=PrefC.GetString(PrefName.PracticeTitle); break;
			}
			if(patient==null && sheetType!=SheetTypeEnum.DepositSlip) {
				listStaticTextReplacements.Add(staticTextReplacement);
				continue;
			}
			switch(listEnumStaticTextFields[i]){
				case EnumStaticTextField.activeAllergies: staticTextReplacement.NewValue=activeAllergies; break;
				case EnumStaticTextField.activeProblems: staticTextReplacement.NewValue=activeProblems; break;
				case EnumStaticTextField.address: staticTextReplacement.NewValue=address; break;
				case EnumStaticTextField.apptsAllFuture: staticTextReplacement.NewValue=apptsAllFuture.TrimEnd(); break;
				case EnumStaticTextField.apptDateMonthSpelled: staticTextReplacement.NewValue=apptDateMonthSpelled; break;
				case EnumStaticTextField.apptModNote: staticTextReplacement.NewValue=apptModNote; break;
				case EnumStaticTextField.apptProcs: staticTextReplacement.NewValue=apptProcs; break;
				case EnumStaticTextField.apptProvNameFormal: staticTextReplacement.NewValue=apptProvNameFormal; break;
				case EnumStaticTextField.age: staticTextReplacement.NewValue=Patients.AgeToString(patient.Age); break;
				case EnumStaticTextField.balTotal: staticTextReplacement.NewValue=family.ListPats[0].BalTotal.ToString("c"); break;
				case EnumStaticTextField.bal_0_30: staticTextReplacement.NewValue=family.ListPats[0].Bal_0_30.ToString("c"); break;
				case EnumStaticTextField.bal_31_60: staticTextReplacement.NewValue=family.ListPats[0].Bal_31_60.ToString("c"); break;
				case EnumStaticTextField.bal_61_90: staticTextReplacement.NewValue=family.ListPats[0].Bal_61_90.ToString("c"); break;
				case EnumStaticTextField.balOver90: staticTextReplacement.NewValue=family.ListPats[0].BalOver90.ToString("c"); break;
				case EnumStaticTextField.balInsEst: staticTextReplacement.NewValue=family.ListPats[0].InsEst.ToString("c"); break;
				case EnumStaticTextField.balTotalMinusInsEst: staticTextReplacement.NewValue=(family.ListPats[0].BalTotal-family.ListPats[0].InsEst).ToString("c"); break;
				case EnumStaticTextField.BillingType: staticTextReplacement.NewValue=Defs.GetName(DefCat.BillingTypes,patient.BillingType); break;
				case EnumStaticTextField.Birthdate: staticTextReplacement.NewValue=birthdate; break;
				case EnumStaticTextField.carrierName: staticTextReplacement.NewValue=carrierName; break;
				case EnumStaticTextField.carrier2Name: staticTextReplacement.NewValue=carrier2Name; break;
				case EnumStaticTextField.ChartNumber: staticTextReplacement.NewValue=patient.ChartNumber; break;
				case EnumStaticTextField.carrierAddress: staticTextReplacement.NewValue=carrierAddress; break;
				case EnumStaticTextField.carrier2Address: staticTextReplacement.NewValue=carrier2Address; break;
				case EnumStaticTextField.carrierCityStZip: staticTextReplacement.NewValue=carrierCityStZip; break;
				case EnumStaticTextField.carrier2CityStZip: staticTextReplacement.NewValue=carrier2CityStZip; break;
				case EnumStaticTextField.cityStateZip: staticTextReplacement.NewValue=patient.City+", "+patient.State+"  "+patient.Zip; break;
				case EnumStaticTextField.clinicDescription: staticTextReplacement.NewValue=clinicPatDescription; break;
				case EnumStaticTextField.clinicAddress: staticTextReplacement.NewValue=clinicPatAddress; break;
				case EnumStaticTextField.clinicCityStZip: staticTextReplacement.NewValue=clinicPatCityStZip; break;
				case EnumStaticTextField.clinicPhone: staticTextReplacement.NewValue=clinicPatPhone; break;
				case EnumStaticTextField.clinicPatDescription: staticTextReplacement.NewValue=clinicPatDescription; break;
				case EnumStaticTextField.clinicPatAddress: staticTextReplacement.NewValue=clinicPatAddress; break;
				case EnumStaticTextField.clinicPatCityStZip: staticTextReplacement.NewValue=clinicPatCityStZip; break;
				case EnumStaticTextField.clinicPatPhone: staticTextReplacement.NewValue=clinicPatPhone; break;
				case EnumStaticTextField.clinicCurDescription: staticTextReplacement.NewValue=clinicCurDescription; break;
				case EnumStaticTextField.clinicCurAddress: staticTextReplacement.NewValue=clinicCurAddress; break;
				case EnumStaticTextField.clinicCurCityStZip: staticTextReplacement.NewValue=clinicCurCityStZip; break;
				case EnumStaticTextField.clinicCurPhone: staticTextReplacement.NewValue=clinicCurPhone; break;
				case EnumStaticTextField.currentMedications: staticTextReplacement.NewValue=currentMedications; break;
				case EnumStaticTextField.DateFirstVisit: staticTextReplacement.NewValue=dateFirstVisit; break;
				case EnumStaticTextField.dateLastAppt: staticTextReplacement.NewValue=dateLastAppt; break;
				case EnumStaticTextField.dateLastBW: staticTextReplacement.NewValue=dateLastBW; break;
				case EnumStaticTextField.dateLastExam: staticTextReplacement.NewValue=dateLastExam; break;
				case EnumStaticTextField.dateLastPerio: staticTextReplacement.NewValue=dateLastPerio; break;
				case EnumStaticTextField.dateLastPanoFMX: staticTextReplacement.NewValue=dateLastPanoFMX; break;
				case EnumStaticTextField.dateLastProphy: staticTextReplacement.NewValue=dateLastProphy; break;
				case EnumStaticTextField.dateLastSrp: staticTextReplacement.NewValue=dateLastSrp; break;
				case EnumStaticTextField.dateOfLastSavedTP: staticTextReplacement.NewValue=dateOfLastSavedTP; break;
				case EnumStaticTextField.dateRecallDue: staticTextReplacement.NewValue=dateRecallDue; break;
				case EnumStaticTextField.dateTimeLastAppt: staticTextReplacement.NewValue=dateTimeLastAppt; break;
				case EnumStaticTextField.dueForBWYN: staticTextReplacement.NewValue=dueForBWYN; break;
				case EnumStaticTextField.dueForPanoYN: staticTextReplacement.NewValue=dueForPanoYN; break;
				case EnumStaticTextField.Email: staticTextReplacement.NewValue=patient.Email; break;
				case EnumStaticTextField.famFinNote: staticTextReplacement.NewValue=patientNote.FamFinancial; break;
				case EnumStaticTextField.famFinUrgNote: staticTextReplacement.NewValue=family.ListPats[0].FamFinUrgNote; break;
				case EnumStaticTextField.famRecallDue: staticTextReplacement.NewValue=famRecallDue; break;
				case EnumStaticTextField.guarantorHmPhone: staticTextReplacement.NewValue=guarantorHmPhone; break;
				case EnumStaticTextField.guarantorNameF: staticTextReplacement.NewValue=guarantorNameF; break;
				case EnumStaticTextField.guarantorNameFL: staticTextReplacement.NewValue=guarantorNameFL; break;
				case EnumStaticTextField.guarantorNameL: staticTextReplacement.NewValue=guarantorNameL; break;
				case EnumStaticTextField.guarantorNamePref: staticTextReplacement.NewValue=guarantorNamePref; break;
				case EnumStaticTextField.guarantorNameLF: staticTextReplacement.NewValue=guarantorNameLF; break;
				case EnumStaticTextField.guarantorWirelessPhone: staticTextReplacement.NewValue=guarantorWirelessPhone; break;
				case EnumStaticTextField.guarantorWkPhone: staticTextReplacement.NewValue=guarantorWkPhone; break;
				case EnumStaticTextField.gender: staticTextReplacement.NewValue=Lans.g("enumPatientGender",patient.Gender.ToString()); break;
				case EnumStaticTextField.genderHeShe: staticTextReplacement.NewValue=pronounHeSheThey; break;
				case EnumStaticTextField.genderheshe: staticTextReplacement.NewValue=pronounheshethey; break;
				case EnumStaticTextField.genderHimHer: staticTextReplacement.NewValue=pronounHimHerThem; break;
				case EnumStaticTextField.genderhimher: staticTextReplacement.NewValue=pronounhimherthem; break;
				case EnumStaticTextField.genderHimselfHerself: staticTextReplacement.NewValue=pronounHimselfHerselfThemself; break;
				case EnumStaticTextField.genderhimselfherself: staticTextReplacement.NewValue=pronounhimselfherselfthemself; break;
				case EnumStaticTextField.genderHisHer: staticTextReplacement.NewValue=pronounHisHerTheir; break;
				case EnumStaticTextField.genderhisher: staticTextReplacement.NewValue=pronounhishertheir; break;
				case EnumStaticTextField.genderHisHers: staticTextReplacement.NewValue=pronounHisHersTheirs; break;
				case EnumStaticTextField.genderhishers: staticTextReplacement.NewValue=pronounhisherstheirs; break;
				case EnumStaticTextField.HmPhone: staticTextReplacement.NewValue=patient.HmPhone; break;
				case EnumStaticTextField.insAnnualMax: staticTextReplacement.NewValue=insAnnualMax; break;
				case EnumStaticTextField.insDeductible: staticTextReplacement.NewValue=insDeductible; break;
				case EnumStaticTextField.insDeductibleUsed: staticTextReplacement.NewValue=insDeductibleUsed; break;
				case EnumStaticTextField.insEmployer: staticTextReplacement.NewValue=insEmployer; break;
				case EnumStaticTextField.insFeeSchedule: staticTextReplacement.NewValue=insFeeSchedule; break;
				case EnumStaticTextField.insFreqBW: staticTextReplacement.NewValue=insFreqBW.TrimEnd(); break;
				case EnumStaticTextField.insFreqExams: staticTextReplacement.NewValue=insFreqExams.TrimEnd(); break;
				case EnumStaticTextField.insFreqPanoFMX: staticTextReplacement.NewValue=insFreqPanoFMX.TrimEnd(); break;
				case EnumStaticTextField.insPending: staticTextReplacement.NewValue=insPending; break;
				case EnumStaticTextField.insPercentages: staticTextReplacement.NewValue=insPercentages; break;
				case EnumStaticTextField.insPlanGroupNumber: staticTextReplacement.NewValue=insPlanGroupNumber; break;
				case EnumStaticTextField.insPlanGroupName: staticTextReplacement.NewValue=insPlanGroupName; break;
				case EnumStaticTextField.insPlanNote: staticTextReplacement.NewValue=insPlanNote; break;
				case EnumStaticTextField.insType: staticTextReplacement.NewValue=insType; break;
				case EnumStaticTextField.insSubBirthDate: staticTextReplacement.NewValue=insSubBirthDate; break;
				case EnumStaticTextField.insSubNote: staticTextReplacement.NewValue=insSubNote; break;
				case EnumStaticTextField.insRemaining: staticTextReplacement.NewValue=insRemaining; break;
				case EnumStaticTextField.insUsed: staticTextReplacement.NewValue=insUsed; break;
				case EnumStaticTextField.ins2AnnualMax: staticTextReplacement.NewValue=ins2AnnualMax; break;
				case EnumStaticTextField.ins2Deductible: staticTextReplacement.NewValue=ins2Deductible; break;
				case EnumStaticTextField.ins2DeductibleUsed: staticTextReplacement.NewValue=ins2DeductibleUsed; break;
				case EnumStaticTextField.ins2Employer: staticTextReplacement.NewValue=ins2Employer; break;
				case EnumStaticTextField.ins2FreqBW: staticTextReplacement.NewValue=ins2FreqBW.TrimEnd(); break;
				case EnumStaticTextField.ins2FreqExams: staticTextReplacement.NewValue=ins2FreqExams.TrimEnd(); break;
				case EnumStaticTextField.ins2FreqPanoFMX: staticTextReplacement.NewValue=ins2FreqPanoFMX.TrimEnd(); break;
				case EnumStaticTextField.ins2PlanGroupNumber: staticTextReplacement.NewValue=ins2PlanGroupNumber; break;
				case EnumStaticTextField.ins2PlanGroupName: staticTextReplacement.NewValue=ins2PlanGroupName; break;
				case EnumStaticTextField.ins2Pending: staticTextReplacement.NewValue=ins2Pending; break;
				case EnumStaticTextField.ins2Percentages: staticTextReplacement.NewValue=ins2Percentages; break;
				case EnumStaticTextField.ins2Remaining: staticTextReplacement.NewValue=ins2Remaining; break;
				case EnumStaticTextField.ins2Used: staticTextReplacement.NewValue=ins2Used; break;
				case EnumStaticTextField.medicalSummary: staticTextReplacement.NewValue=medicalSummary; break;
				case EnumStaticTextField.MedUrgNote: staticTextReplacement.NewValue=patient.MedUrgNote; break;
				case EnumStaticTextField.nameF: staticTextReplacement.NewValue=patient.FName; break;
				case EnumStaticTextField.nameFL: staticTextReplacement.NewValue=patient.GetNameFL(); break;
				case EnumStaticTextField.nameFLFormal: staticTextReplacement.NewValue=patient.GetNameFLFormal(); break;
				case EnumStaticTextField.nameL: staticTextReplacement.NewValue=patient.LName; break;
				case EnumStaticTextField.nameLF: staticTextReplacement.NewValue=patient.GetNameLF(); break;
				case EnumStaticTextField.nameMI: staticTextReplacement.NewValue=patient.MiddleI; break;
				case EnumStaticTextField.namePref: staticTextReplacement.NewValue=patient.Preferred; break;
				case EnumStaticTextField.namePreferredOrFirst: staticTextReplacement.NewValue=namePreferredOrFirst; break;
				case EnumStaticTextField.nextSchedApptDate: staticTextReplacement.NewValue=nextSchedApptDate; break;
				case EnumStaticTextField.nextSchedApptDateT: staticTextReplacement.NewValue=nextSchedApptDateT; break;
				case EnumStaticTextField.nextSchedApptsFam: staticTextReplacement.NewValue=nextSchedApptsFam.TrimEnd(); break;
				case EnumStaticTextField.PatNum: staticTextReplacement.NewValue=patient.PatNum.ToString(); break;
				case EnumStaticTextField.famPopups: staticTextReplacement.NewValue=famPopups; break;
				case EnumStaticTextField.patientPortalCredentials: staticTextReplacement.NewValue=patientPortalCredentials; break;
				case EnumStaticTextField.plannedAppointmentInfo: staticTextReplacement.NewValue=plannedAppointmentInfo; break;
				case EnumStaticTextField.premedicateYN: staticTextReplacement.NewValue=premedicateYN; break;
				case EnumStaticTextField.priProvNameFormal: staticTextReplacement.NewValue=providerPri.FormalName; break;
				case EnumStaticTextField.recallInterval: staticTextReplacement.NewValue=recallInterval; break;
				case EnumStaticTextField.recallScheduledYN: staticTextReplacement.NewValue=recallScheduledYN; break;
				case EnumStaticTextField.referredFrom: staticTextReplacement.NewValue=referredFrom; break;
				case EnumStaticTextField.referredTo: staticTextReplacement.NewValue=referredTo.TrimEnd(); break;
				case EnumStaticTextField.salutation: staticTextReplacement.NewValue=patient.GetSalutation(); break;
				case EnumStaticTextField.serviceNote: staticTextReplacement.NewValue=serviceNote; break;
				case EnumStaticTextField.siteDescription: staticTextReplacement.NewValue=Sites.GetDescription(patient.SiteNum); break;
				case EnumStaticTextField.SSN: staticTextReplacement.NewValue=patient.SSN; break;
				case EnumStaticTextField.subscriberID: staticTextReplacement.NewValue=subscriberId; break;
				case EnumStaticTextField.subscriberNameFL: staticTextReplacement.NewValue=subscriberNameFL; break;
				case EnumStaticTextField.subscriber2NameFL: staticTextReplacement.NewValue=subscriber2NameFL; break;
				case EnumStaticTextField.timeNow: staticTextReplacement.NewValue=DateTime.Now.ToShortTimeString(); break;
				case EnumStaticTextField.tpResponsPartyAddress: staticTextReplacement.NewValue=tpResponsPartyAddress; break;
				case EnumStaticTextField.tpResponsPartyCityStZip: staticTextReplacement.NewValue=tpResponsPartyCityStZip; break;
				case EnumStaticTextField.tpResponsPartyNameFL: staticTextReplacement.NewValue=tpResponsPartyNameFL; break;
				case EnumStaticTextField.treatmentNote: staticTextReplacement.NewValue=treatmentNote; break;
				case EnumStaticTextField.treatmentPlanProcs: staticTextReplacement.NewValue=treatmentPlanProcs; break;
				case EnumStaticTextField.treatmentPlanProcsPriority: staticTextReplacement.NewValue=treatmentPlanProcsPriority; break;
				case EnumStaticTextField.WirelessPhone: staticTextReplacement.NewValue=patient.WirelessPhone; break;
				case EnumStaticTextField.WkPhone: staticTextReplacement.NewValue=patient.WkPhone; break;
			}
			listStaticTextReplacements.Add(staticTextReplacement);
		}
		return listStaticTextReplacements;
	}

	///<summary>Takes a list of replacements and actually performs the replacement within all the Sheet fields.</summary>
	private static void ReplaceStaticTextFieldsInSheet(List<StaticTextReplacement> listStaticTextReplacements,Sheet sheet,Patient patient,Family family){
		for(var f=0;f<sheet.SheetFields.Count;f++){
			if(sheet.SheetFields[f].FieldType!=SheetFieldType.StaticText) {
				continue;
			}
			for(var i=0;i<listStaticTextReplacements.Count;i++){
				sheet.SheetFields[f].FieldValue=sheet.SheetFields[f].FieldValue.Replace(
					"["+listStaticTextReplacements[i].StaticTextField.ToString()+"]",
					listStaticTextReplacements[i].NewValue);
			}
		}
		#region Fill Exam Sheet Fields
		//Fill Exam Sheet Fields----------------------------------------------------------------------------------------------
		//Example: ExamSheet:MyExamSheet;MyField
		if((sheet.SheetType==SheetTypeEnum.PatientLetter || sheet.SheetType==SheetTypeEnum.ReferralSlip || sheet.SheetType==SheetTypeEnum.ReferralLetter) && patient!=null) {
			for(var f=0;f<sheet.SheetFields.Count;f++){
				if(sheet.SheetFields[f].FieldType!=SheetFieldType.StaticText) {
					continue;
				}
				var fieldValue=sheet.SheetFields[f].FieldValue;
				var rgx=@"\[ExamSheet\:([^;]+);([^\]]+)\]";
				var match=Regex.Match(fieldValue,rgx);
				while(match.Success) {
					var examSheetDescript=match.Result("$1");
					var fieldName=match.Result("$2");
					var examFields=SheetFields.GetFieldFromExamSheet(patient.PatNum,examSheetDescript,fieldName);//Either a list of fields (if radio button) or single field
					if(examFields==null || examFields.Count==0) {
						match=match.NextMatch();
						continue;
					}
					if(examFields[0].RadioButtonGroup!="") {//a user defined 'misc' radio button check box, find the selected item and replace with reportable name
						for(var i=0;i<examFields.Count;i++) {
							if(examFields[i].FieldValue=="X") {
								fieldValue=fieldValue.Replace(match.Value,examFields[i].ReportableName);//each radio button in the group has a different reportable name.
								break;
							}
						}
					}
					else if(examFields[0].ReportableName!="") {//not a radio button, so either user defined single misc check boxes, combobox, or misc input field with reportable name
						if(examFields[0].FieldType==SheetFieldType.ComboBox && match.Value.Contains(examFields[0].ReportableName)) {
							fieldValue=fieldValue.Replace(match.Value,examFields[0].FieldValue.Substring(0,examFields[0].FieldValue.IndexOf(';')));
						}
						else {
							fieldValue=fieldValue.Replace(match.Value,examFields[0].FieldValue);//checkboxes from exam sheets will show as X or blank on letter.
						}
					}
					else if(examFields[0].FieldName!="" && examFields[0].FieldName!="misc") {//internally defined
						if(examFields[0].RadioButtonValue=="") {//internally defined field, not part of a radio button group
							fieldValue=fieldValue.Replace(match.Value,examFields[0].FieldValue);//checkbox or input
						}
						else {//internally defined radio button, look for one selected
							for(var i=0;i<examFields.Count;i++) {
								if(examFields[i].FieldValue=="X") {
									fieldValue=fieldValue.Replace(match.Value,examFields[i].RadioButtonValue);
									break;
								}
							}
						}
					}
					match=match.NextMatch();
				}//while
				sheet.SheetFields[f].FieldValue=fieldValue;
			}
		}
		#endregion
		object[] parameters={patient,sheet,listStaticTextReplacements};
	}

	///<summary>For new sheets only. Sets sheetField.FieldValue=DocumentNum(FK) or "MountNum:###" based on documentCategoryNum (stored in SheetField.FieldName) and pat.PatNum. Example: if DocCategory=132 (PatImages) then FieldValue would be set equal to the DocNum or MountNum of the most recent PatImage in the patient's image folder.  If there are no images in the patient's folder, FieldValue will be blank.</summary>
	private static void FillPatientImages(Sheet sheet,Patient patient,StaticTextData staticTextData=null){
		if(patient is null){
			return;
		}
		var listDocuments=new List<Document>();
		if(staticTextData!=null && staticTextData.ListDocuments!=null){
			listDocuments=staticTextData.ListDocuments;
		}
		else{
			listDocuments=Documents.GetPatientData(patient.PatNum);
		}
		var listMounts=Mounts.GetPatientData(patient.PatNum);
		for(var i=0;i<sheet.SheetFields.Count;i++){
			if(sheet.SheetFields[i].FieldType!=SheetFieldType.PatImage){
				continue;
			}
			sheet.SheetFields[i].FieldValue="";
			var categoryNum=SIn.Long(sheet.SheetFields[i].FieldName);
			var document=listDocuments.FindAll(x=>x.DocCategory==categoryNum && x.MountItemNum==0).LastOrDefault();
			var mount=listMounts.FindAll(x=>x.DocCategory==categoryNum).LastOrDefault();
			if(document!=null && mount!=null){
				if(document.DateCreated>mount.DateCreated){
					sheet.SheetFields[i].FieldValue=document.DocNum.ToString();
				}
				else{
					sheet.SheetFields[i].FieldValue="MountNum:"+mount.MountNum.ToString();
				}
			}
			else if(document!=null){
				sheet.SheetFields[i].FieldValue=document.DocNum.ToString();
			}
			else if(mount!=null){
				sheet.SheetFields[i].FieldValue="MountNum:"+mount.MountNum.ToString();
			}
		}
	}

	private static void FillFieldsForLabelPatient(Sheet sheet,Patient pat){
		foreach(var field in sheet.SheetFields){
			switch(field.FieldName){
				case "nameFL":
					field.FieldValue=pat.GetNameFLFormal();
					break;
				case "nameLF":
					field.FieldValue=pat.GetNameLF();
					break;
				case "address":
					field.FieldValue=pat.Address;
					if(pat.Address2!=""){
						field.FieldValue+="\r\n"+pat.Address2;
					}
					break;
				case "cityStateZip":
					field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
					break;
				case "ChartNumber":
					field.FieldValue=pat.ChartNumber;
					break;
				case "PatNum":
					field.FieldValue=pat.PatNum.ToString();
					break;
				case "dateTime.Today":
					field.FieldValue=DateTime.Today.ToShortDateString();
					break;
				case "birthdate":
					//only a temporary workaround:
					field.FieldValue="BD: "+pat.Birthdate.ToShortDateString();
					break;
				case "priProvName":
					field.FieldValue=Providers.GetLongDesc(pat.PriProv);
					break;
				case "text":
					//If the user sets a Label Text as their patient label, then the "text" param will be null.
					//We will handle this case by manually setting the field value to name and address here.
					var paramCur=GetParamByName(sheet,"text");
					if(paramCur==null) {
						field.FieldValue=pat.FName+" "+pat.LName+"\r\n"+pat.Address+"\r\n"+pat.City+", "+pat.State+" "+pat.Zip+"\r\n";
					}
					else {
						field.FieldValue=paramCur.ParamValue.ToString();
					}
					break;
			}
		}

			
	}

	private static void FillFieldsForLabelCarrier(Sheet sheet,Carrier carrier) {
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "CarrierName":
					field.FieldValue=carrier.CarrierName;
					break;
				case "address":
					field.FieldValue=carrier.Address;
					if(carrier.Address2!="") {
						field.FieldValue+="\r\n"+carrier.Address2;
					}
					break;
				case "cityStateZip":
					field.FieldValue=carrier.City+", "+carrier.State+" "+carrier.Zip;
					break;
			}
		}
	}

	private static void FillFieldsForLabelReferral(Sheet sheet,Referral refer) {
		if(refer==null) {
			return;
		}
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "nameFL":
					field.FieldValue=Referrals.GetNameFL(refer.ReferralNum);
					break;
				case "address":
					field.FieldValue=refer.Address;
					if(refer.Address2!="") {
						field.FieldValue+="\r\n"+refer.Address2;
					}
					break;
				case "cityStateZip":
					field.FieldValue=refer.City+", "+refer.ST+" "+refer.Zip;
					break;
			}
		}
	}

	private static void FillFieldsForReferralSlip(Sheet sheet,Patient pat,Referral refer,long refAttachProvNum){
		var listPatientFields=new List<SheetField>();
		var listReferralFields=new List<SheetField>();
		#region misc fields
		foreach(var field in sheet.SheetFields) {
			if(field.FieldName.StartsWith("patient.")) {
				listPatientFields.Add(field);
				continue;
			}
			if(field.FieldName.StartsWith("referral.")) {
				listReferralFields.Add(field);
				continue;
			}
			//All other fields are considered misc so put them through the switch case.
			switch(field.FieldName) {
				case "dateTime.Today":
					field.FieldValue=DateTime.Today.ToShortDateString();
					break;
			}
		}
		#endregion
		#region referral fields
		if(refer!=null) {
			foreach(var field in listReferralFields) {
				switch(field.FieldName) {
					case "referral.nameFL":
						field.FieldValue=Referrals.GetNameFL(refer.ReferralNum);
						break;
					case "referral.address":
						field.FieldValue=refer.Address;
						if(refer.Address2!="") {
							field.FieldValue+="\r\n"+refer.Address2;
						}
						break;
					case "referral.cityStateZip":
						field.FieldValue=refer.City+", "+refer.ST+" "+refer.Zip;
						break;
					case "referral.phone":
						field.FieldValue="";
						if(refer.Telephone.Length==10){
							field.FieldValue=TelephoneNumbers.ReFormat(refer.Telephone);
						}
						break;
					case "referral.phone2":
						field.FieldValue=refer.Phone2;
						break;
				}
			}
		}
		#endregion
		#region patient fields
		if(pat!=null) {
			foreach(var field in listPatientFields) {
				switch(field.FieldName) {
					case "patient.nameFL":
						field.FieldValue=pat.GetNameFL();
						break;
					case "patient.WkPhone":
						field.FieldValue=pat.WkPhone;
						break;
					case "patient.HmPhone":
						field.FieldValue=pat.HmPhone;
						break;
					case "patient.WirelessPhone":
						field.FieldValue=pat.WirelessPhone;
						break;
					case "patient.address":
						field.FieldValue=pat.Address;
						if(pat.Address2!="") {
							field.FieldValue+="\r\n"+pat.Address2;
						}
						break;
					case "patient.cityStateZip":
						field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						break;
					case "patient.provider":
						field.FieldValue=Providers.GetById(Patients.GetProvNum(pat)).FormalName;//Use the patient's default provider.
						if(refAttachProvNum>0){
							field.FieldValue=Providers.GetById(refAttachProvNum).FormalName;//Referral type is "To", use selected referral's associated provider instead.
						}
						break;
					//case "notes"://an input field
				}
			}
		}
		#endregion
	}

	private static void FillFieldsForLabelAppointment(Sheet sheet,Appointment appt,Patient pat) {
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "nameFL":
					field.FieldValue=pat.GetNameFirstOrPrefL();
					break;
				case "nameLF":
					field.FieldValue=pat.GetNameLF();
					break;
				case "weekdayDateTime":
					field.FieldValue=appt.AptDateTime.ToString("ddd")+"   "
					                                                 +appt.AptDateTime.ToShortDateString()+"  "
					                                                 +appt.AptDateTime.ToShortTimeString();//  h:mm tt");
					break;
				case "length":
					var minutesTotal=appt.Pattern.Length*5;
					var hours=minutesTotal/60;//automatically rounds down
					var minutes=minutesTotal-hours*60;
					field.FieldValue="";
					if(hours>0){
						field.FieldValue=hours.ToString()+" hours, ";
					}
					field.FieldValue+=minutes.ToString()+" min";
					break;
			}
		}
	}

	private static void FillFieldsForConsent(Sheet sheet,Patient pat) {
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "patient.nameFL":
					field.FieldValue=pat.GetNameFL();
					break;
				case "dateTime.Today":
					field.FieldValue=DateTime.Today.ToShortDateString();
					break;
			}
		}
	}

	private static void FillFieldsForPatientLetter(Sheet sheet,Patient pat) {
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "PracticeTitle":
					field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
					break;
				case "PracticeAddress":
					field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
					if(PrefC.GetString(PrefName.PracticeAddress2) != ""){
						field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
					}
					break;
				case "practiceCityStateZip":
					field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "
					                                                       +PrefC.GetString(PrefName.PracticeST)+"  "
					                                                       +PrefC.GetString(PrefName.PracticeZip);
					break;
				case "patient.nameFL":
					field.FieldValue=pat.GetNameFLFormal();
					break;
				case "patient.address":
					field.FieldValue=pat.Address;
					if(pat.Address2!="") {
						field.FieldValue+="\r\n"+pat.Address2;
					}
					break;
				case "patient.cityStateZip":
					field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
					break;
				case "today.DayDate":
					field.FieldValue=DateTime.Today.ToString("dddd")+", "+DateTime.Today.ToShortDateString();
					break;
				case "patient.salutation":
					field.FieldValue="Dear "+pat.GetSalutation()+":";
					break;
				case "patient.priProvNameFL":
					field.FieldValue=Providers.GetFormalName(pat.PriProv);
					break;
			}
		}
	}

	private static void FillFieldsForReferralLetter(Sheet sheet,Patient pat,Referral refer) {
		var listPatientFields=new List<SheetField>();
		var listReferralFields=new List<SheetField>();
		#region misc fields
		foreach(var field in sheet.SheetFields) {
			if(field.FieldName.StartsWith("patient.")) {
				listPatientFields.Add(field);
				continue;
			}
			if(field.FieldName.StartsWith("referral.")) {
				listReferralFields.Add(field);
				continue;
			}
			switch(field.FieldName) {
				case "PracticeTitle":
					field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
					break;
				case "PracticeAddress":
					field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
					if(PrefC.GetString(PrefName.PracticeAddress2) != ""){
						field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
					}
					break;
				case "PracticePhoneNumber":
					var practicePhone=PrefC.GetString(PrefName.PracticePhone);
					field.FieldValue=practicePhone;
					if(practicePhone.Length==10) {
						field.FieldValue=TelephoneNumbers.ReFormat(practicePhone);
					}
					break;
				case "practiceCityStateZip":
					field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "
					                                                       +PrefC.GetString(PrefName.PracticeST)+"  "
					                                                       +PrefC.GetString(PrefName.PracticeZip);
					break;
				case "today.DayDate":
					field.FieldValue=DateTime.Today.ToString("dddd")+", "+DateTime.Today.ToShortDateString();
					break;
			}
		}
		#endregion
		#region referral fields
		if(refer!=null) {
			foreach(var field in listReferralFields) {
				switch(field.FieldName) {
					case "referral.phone":
						field.FieldValue="";
						if(refer.Telephone.Length==10) {
							field.FieldValue=TelephoneNumbers.ReFormat(refer.Telephone);
						}
						break;
					case "referral.phone2":
						field.FieldValue=refer.Phone2;
						break;
					case "referral.nameFL":
						field.FieldValue=Referrals.GetNameFL(refer.ReferralNum);
						break;
					case "referral.nameL":
						field.FieldValue=refer.LName;
						break;
					case "referral.address":
						field.FieldValue=refer.Address;
						if(refer.Address2!="") {
							field.FieldValue+="\r\n"+refer.Address2;
						}
						break;
					case "referral.cityStateZip":
						field.FieldValue=refer.City+", "+refer.ST+" "+refer.Zip;
						break;
					case "referral.salutation":
						field.FieldValue="Dear "+refer.FName+":";
						break;
				}
			}
		}
		#endregion
		#region patient fields
		if(pat!=null) {
			foreach(var field in listPatientFields) {
				switch(field.FieldName) {
					case "patient.nameFL":
						field.FieldValue=pat.GetNameFL();
						break;
					case "patient.Birthdate":
						field.FieldValue=pat.Birthdate.ToShortDateString();
						break;
					case "patient.priProvNameFL":
						field.FieldValue=Providers.GetFormalName(pat.PriProv);
						break;
				}
			}
		}
		#endregion
	}

	private static void FillFieldsForPatientForm(Sheet sheet,Patient pat) {
		var fam=Patients.GetFamily(pat.PatNum);
		var patPlanList=PatPlans.Refresh(pat.PatNum);
		if(!PatPlans.IsPatPlanListValid(patPlanList)) {
			patPlanList=PatPlans.Refresh(pat.PatNum);
		}
		var subList=InsSubs.RefreshForFam(fam);
		var planList=InsPlans.RefreshForSubList(subList);
		InsPlan insplan1=null;
		InsSub sub1=null;
		Carrier carrier1=null;
		if(patPlanList.Count>0){
			sub1=InsSubs.GetSub(patPlanList[0].InsSubNum,subList);
			insplan1=InsPlans.GetPlan(sub1.PlanNum,planList);
			carrier1=Carriers.GetCarrier(insplan1.CarrierNum);
		}
		InsPlan insplan2=null;
		InsSub sub2=null;
		Carrier carrier2=null;
		if(patPlanList.Count>1) {
			sub2=InsSubs.GetSub(patPlanList[1].InsSubNum,subList);
			insplan2=InsPlans.GetPlan(sub2.PlanNum,planList);
			carrier2=Carriers.GetCarrier(insplan2.CarrierNum);
		}
		var patCurNote=PatientNotes.Refresh(pat.PatNum,pat.Guarantor);
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "Address":
					field.FieldValue=pat.Address;
					break;
				case "Address2":
					field.FieldValue=pat.Address2;
					break;
				case "addressAndHmPhoneIsSameEntireFamily":
					var isSame=true;
					for(var i=0;i<fam.ListPats.Length;i++){
						if(pat.HmPhone!=fam.ListPats[i].HmPhone
						   || pat.Address!=fam.ListPats[i].Address
						   || pat.Address2!=fam.ListPats[i].Address2
						   || pat.City!=fam.ListPats[i].City
						   || pat.State!=fam.ListPats[i].State
						   || pat.Zip!=fam.ListPats[i].Zip)
						{
							isSame=false;
							break;
						}
					}
					if(isSame) {
						field.FieldValue="X";
					}
					break;
				case "Birthdate":
					field.FieldValue=pat.Birthdate.ToShortDateString();
					break;
				case "City":
					field.FieldValue=pat.City;
					break;
				case "Email":
					field.FieldValue=pat.Email;
					break;
				case "FName":
					field.FieldValue=pat.FName;
					break;
				case "Gender":
					if(field.RadioButtonValue==pat.Gender.ToString()) {
						field.FieldValue="X";
					}
					break;
				case "HmPhone":
					field.FieldValue=pat.HmPhone;
					break;
				case "ICEName":
					field.FieldValue=patCurNote.ICEName;
					break;
				case "ICEPhone":
					field.FieldValue=patCurNote.ICEPhone;
					break;
				case "ins1CarrierName":
					if(carrier1!=null){
						field.FieldValue=carrier1.CarrierName;
					}
					break;
				case "ins1CarrierPhone":
					if(carrier1!=null) {
						field.FieldValue=carrier1.Phone;
					}
					break;
				case "ins1EmployerName":
					if(insplan1!=null) {
						field.FieldValue=Employers.GetName(insplan1.EmployerNum);
					}
					break;
				case "ins1GroupName":
					if(insplan1!=null) {
						field.FieldValue=insplan1.GroupName;
					}
					break;
				case "ins1GroupNum":
					if(insplan1!=null) {
						field.FieldValue=insplan1.GroupNum;
					}
					break;
				case "ins1Relat":
					if(patPlanList.Count>0 && patPlanList[0].Relationship.ToString()==field.RadioButtonValue) {
						field.FieldValue="X";
					}
					break;
				case "ins1SubscriberID":
					if(insplan1!=null) {
						field.FieldValue=sub1.SubscriberID;
					}
					break;
				case "ins1SubscriberNameF":
					if(insplan1!=null) {
						field.FieldValue=fam.GetNameInFamFirst(sub1.Subscriber);
					}
					break;
				case "ins2CarrierName":
					if(carrier2!=null) {
						field.FieldValue=carrier2.CarrierName;
					}
					break;
				case "ins2CarrierPhone":
					if(carrier2!=null) {
						field.FieldValue=carrier2.Phone;
					}
					break;
				case "ins2EmployerName":
					if(insplan2!=null) {
						field.FieldValue=Employers.GetName(insplan2.EmployerNum);
					}
					break;
				case "ins2GroupName":
					if(insplan2!=null) {
						field.FieldValue=insplan2.GroupName;
					}
					break;
				case "ins2GroupNum":
					if(insplan2!=null) {
						field.FieldValue=insplan2.GroupNum;
					}
					break;
				case "ins2Relat":
					if(patPlanList.Count>1 && patPlanList[1].Relationship.ToString()==field.RadioButtonValue) {
						field.FieldValue="X";
					}
					break;
				case "ins2SubscriberID":
					if(insplan2!=null) {
						field.FieldValue=sub2.SubscriberID;
					}
					break;
				case "ins2SubscriberNameF":
					if(insplan2!=null) {
						field.FieldValue=fam.GetNameInFamFirst(sub2.Subscriber);
					}
					break;
				case "LName":
					field.FieldValue=pat.LName;
					break;
				case "MiddleI":
					field.FieldValue=pat.MiddleI;
					break;
				case "Position":
					if(pat.Position.ToString()==field.RadioButtonValue) {
						field.FieldValue="X";
					}
					break;
				case "PreferConfirmMethod":
					if(pat.PreferConfirmMethod.ToString()==field.RadioButtonValue) {
						field.FieldValue="X";
					}
					break;
				case "PreferContactMethod":
					if(pat.PreferContactMethod.ToString()==field.RadioButtonValue) {
						field.FieldValue="X";
					}
					break;
				case "PreferRecallMethod":
					if(pat.PreferRecallMethod.ToString()==field.RadioButtonValue) {
						field.FieldValue="X";
					}
					break;
				case "Preferred":
					field.FieldValue=pat.Preferred;
					break;
				case "referredFrom":
					var referral=Referrals.GetReferralForPat(pat.PatNum);
					if(referral!=null){
						field.FieldValue=Referrals.GetNameFL(referral.ReferralNum);
					}
					break;
				case "SSN":
					if(CultureInfo.CurrentCulture.Name=="en-US" && pat.SSN.Length==9){//and length exactly 9 (no data gets lost in formatting)
						field.FieldValue=pat.SSN.Substring(0,3)+"-"+pat.SSN.Substring(3,2)+"-"+pat.SSN.Substring(5,4);
					}
					else {
						field.FieldValue=pat.SSN;
					}
					break;
				case "State":
					field.FieldValue=pat.State;
					break;
				case "StudentStatus":
					if(pat.StudentStatus=="F" && field.RadioButtonValue=="Fulltime") {
						field.FieldValue="X";
					}
					if(pat.StudentStatus=="N" && field.RadioButtonValue=="Nonstudent") {
						field.FieldValue="X";
					}
					if(pat.StudentStatus=="P" && field.RadioButtonValue=="Parttime") {
						field.FieldValue="X";
					}

					break;
				case "WirelessPhone":
					field.FieldValue=pat.WirelessPhone;
					break;
				case "wirelessCarrier":
					field.FieldValue="";//not implemented
					break;
				case "WkPhone":
					field.FieldValue=pat.WkPhone;
					break;
				case "Zip":
					field.FieldValue=pat.Zip;
					break;
			}
		}
	}

	private static void FillFieldsForRoutingSlip(Sheet sheet,Patient pat,Appointment apt) {
		var fam=Patients.GetFamily(apt.PatNum);
		var labForApt=LabCases.GetForApt(apt.AptNum).FirstOrDefault();
		var referral=Referrals.GetReferralForPat(apt.PatNum);
		string str;
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "appt.timeDate":
					field.FieldValue=apt.AptDateTime.ToShortTimeString()+"  "+apt.AptDateTime.ToShortDateString();
					break;
				case "appt.length":
					field.FieldValue=(apt.Pattern.Length*5).ToString()+" "+Lans.g("SheetRoutingSlip","minutes");
					break;
				case "appt.providers":
					str=Providers.GetLongDesc(apt.ProvNum);
					if(apt.ProvHyg!=0){
						str+="\r\n"+Providers.GetLongDesc(apt.ProvHyg);
					}
					field.FieldValue=str;
					break;
				case "appt.procedures":
					str="";
					var procs=Procedures.GetProcsForSingle(apt.AptNum,false);
					var isOnlyTP=true;
					for(var i=0;i<procs.Count;i++) {
						if(procs[i].ProcStatus!=ProcStat.TP) {
							isOnlyTP=false;
							break;
						}
					}
					if(isOnlyTP) {
						var listProceduresTPs=Procedures.GetListTPandTPi(procs);//this sorts.  Doesn't work unless all are TP.
						for(var i=0;i<listProceduresTPs.Count;i++) {
							if(i>0) {
								str+="\r\n";
							}
							str+=Procedures.GetDescription(listProceduresTPs[i]);
						}
					}
					else {
						for(var i=0;i<procs.Count;i++) {
							if(i>0) {
								str+="\r\n";
							}
							str+=Procedures.GetDescription(procs[i]);
						}
					}
					field.FieldValue=str;
					break;
				case "appt.Note":
					field.FieldValue=apt.Note;
					break;
				case "appt.estPatientPortion":
					field.FieldValue=Appointments.GetEstPatientPortion(apt).ToString("c");
					break;
				case "otherFamilyMembers":
					str="";
					for(var i=0;i<fam.ListPats.Length;i++) {
						if(fam.ListPats[i].PatNum==pat.PatNum) {
							continue;
						}
						if(fam.ListPats[i].PatStatus==PatientStatus.Archived
						   || fam.ListPats[i].PatStatus==PatientStatus.Deceased) {
							//Prospective patients will show.
							continue;
						}
						if(str!="") {
							str+="\r\n";
						}
						str+=fam.ListPats[i].GetNameFL();
						if(fam.ListPats[i].Age>0){
							str+=",   "+fam.ListPats[i].Age.ToString();
						}
					}
					field.FieldValue=str;
					break;
				case "labName":
					if(labForApt!=null) {
						field.FieldValue=Laboratories.GetOne(labForApt.LaboratoryNum).Description;
					}
					break;
				case "dateLabSent":
					if(labForApt!=null) {
						if(labForApt.DateTimeSent==DateTime.MinValue) {
							field.FieldValue="Not Sent";
						}
						else {
							field.FieldValue=labForApt.DateTimeSent.ToShortDateString();
						}
					}
					break;
				case "dateLabReceived":
					if(labForApt!=null) {
						if(labForApt.DateTimeRecd==DateTime.MinValue) {
							field.FieldValue="Not Received";
						}
						else {
							field.FieldValue=labForApt.DateTimeRecd.ToShortDateString();
						}
					}
					break;
				case "referral.address":
					field.FieldValue="";
					if(referral!=null) {
						field.FieldValue+=referral.Address;
						if(referral.Address2!="") {
							field.FieldValue+="\r\n"+referral.Address2;
						}
					}
					break;
				case "referral.cityStateZip":
					field.FieldValue="";
					if(referral!=null) {
						field.FieldValue+=referral.City;
						field.FieldValue+=(field.FieldValue!="" && referral.ST!="" ? ", " : "");
						field.FieldValue+=referral.ST;
						field.FieldValue+=(field.FieldValue!="" ? " ":"")+referral.Zip;
					}
					break;
				case "referral.FLName":
					field.FieldValue="";
					if(referral!=null) {
						field.FieldValue+=Patients.GetNameFL(referral.LName,referral.FName,"","");
					}
					break;
				case "referral.LName":
					field.FieldValue="";
					if(referral!=null) {
						field.FieldValue=referral.LName;
					}
					break;
			}
		}
	}

	private static void FillFieldsForMedicalHistory(Sheet sheet,Patient pat) {
		var inputMedList=new List<SheetField>();
		var patCurNote=PatientNotes.Refresh(pat.PatNum,pat.Guarantor);
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "Birthdate":
					field.FieldValue=pat.Birthdate.ToShortDateString();
					continue;
				case "FName":
					field.FieldValue=pat.FName;
					continue;
				case "ICEName":
					field.FieldValue=patCurNote.ICEName;
					continue;
				case "ICEPhone":
					field.FieldValue=patCurNote.ICEPhone;
					continue;
				case "LName":
					field.FieldValue=pat.LName;
					continue;
			}
			if(field.FieldType==SheetFieldType.CheckBox) {
				if(field.FieldName.StartsWith("allergy:")) {//"allergy:Pen"
					var allergies=Allergies.GetAll(pat.PatNum,true);
					for(var i=0;i<allergies.Count;i++) {
						if(AllergyDefs.GetDescription(allergies[i].AllergyDefNum)==field.FieldName.Remove(0,8)) {
							if(allergies[i].StatusIsActive && field.RadioButtonValue=="Y") {
								field.FieldValue="X";
							}
							else if(!allergies[i].StatusIsActive && field.RadioButtonValue=="N") {
								field.FieldValue="X";
							}
							break;
						}
					}
				}
				else if(field.FieldName.StartsWith("problem:")) {//"problem:Hepatitis B"
					var diseases=Diseases.Refresh(pat.PatNum,false);
					for(var i=0;i<diseases.Count;i++) {
						if(DiseaseDefs.GetName(diseases[i].DiseaseDefNum)==field.FieldName.Remove(0,8)) {
							if(diseases[i].ProbStatus==ProblemStatus.Active && field.RadioButtonValue=="Y") {
								field.FieldValue="X";
							}
							else if(diseases[i].ProbStatus!=ProblemStatus.Active && field.RadioButtonValue=="N") {
								field.FieldValue="X";
							}
							break;
						}
					}
				}
			}
			else if(field.FieldType==SheetFieldType.InputField && field.FieldName.StartsWith("inputMed")) {
				inputMedList.Add(field);
			}
		}
		//Special logic for checkMed and inputMed.
		if(inputMedList.Count>0) {
			inputMedList.Sort(CompareSheetFieldNames);
			//Loop through the patients medications and fill in the input fields.
			var medPatList=MedicationPats.Refresh(pat.PatNum,false);
			for(var i=0;i<medPatList.Count;i++) {
				if(i==inputMedList.Count) {
					break;//Pat has more medications than inputMed fields on sheet.
				}
				if(medPatList[i].MedicationNum==0) {
					inputMedList[i].FieldValue=medPatList[i].MedDescript;
				}
				else {
					inputMedList[i].FieldValue=Medications.GetDescription(medPatList[i].MedicationNum);
				}
				inputMedList[i].FieldType=SheetFieldType.OutputText;//Don't try to import as a new medication.
			}
		}
	}

	private static void FillFieldsForLabCase(Sheet sheet,Patient pat,LabCase labcase) {
		var lab=Laboratories.GetOne(labcase.LaboratoryNum);//might possibly be null
		var prov=Providers.GetById(labcase.ProvNum);
		var appt=Appointments.GetOneApt(labcase.AptNum);//might be null
		long clinicNum=0;
		if(true) {//Only get clinic specific information if clinics are turned on.
			clinicNum=pat.ClinicNum;
		}
		var provClinic=ProviderClinics.GetOneOrDefault(prov.Id,clinicNum);
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "lab.Description":
					if(lab!=null){
						field.FieldValue=lab.Description;
					}
					break;
				case "lab.Phone":
					if(lab!=null){
						field.FieldValue=lab.Phone;
					}
					break;
				case "lab.Notes":
					if(lab!=null){
						field.FieldValue=lab.Notes;
					}
					break;
				case "lab.WirelessPhone":
					if(lab!=null){
						field.FieldValue=lab.WirelessPhone;
					}
					break;
				case "lab.Address":
					if(lab!=null){
						field.FieldValue=lab.Address;
					}
					break;
				case "lab.CityStZip":
					if(lab!=null){
						field.FieldValue=lab.City+", "+lab.State+" "+lab.Zip;
					}
					break;
				case "lab.Email":
					if(lab!=null){
						field.FieldValue=lab.Email;
					}
					break;
				case "appt.DateTime":
					if(appt!=null) {
						field.FieldValue=appt.AptDateTime.ToShortDateString()+"  "+appt.AptDateTime.ToShortTimeString();
					}
					break;
				case "labcase.DateTimeDue":
					field.FieldValue=labcase.DateTimeDue.ToShortDateString()+"  "+labcase.DateTimeDue.ToShortTimeString();
					break;
				case "labcase.DateTimeCreated":
					field.FieldValue=labcase.DateTimeCreated.ToShortDateString()+"  "+labcase.DateTimeCreated.ToShortTimeString();
					break;
				case "labcase.Instructions":
					field.FieldValue=labcase.Instructions;
					break;
				case "prov.nameFormal":
					field.FieldValue=prov.FormalName;
					break;
				case "prov.stateLicence":
					field.FieldValue=(provClinic==null ? "" : provClinic.StateLicense);
					break;
				case "labcase.LabCaseNum":
					field.FieldValue=labcase.LabCaseNum.ToString();
					break;
			}
		}
	}

	private static void FillFieldsForExamSheet(Sheet sheet,Patient pat) {
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "Birthdate":
					field.FieldValue=pat.Birthdate.ToShortDateString();
					break;
				case "FName":
					field.FieldValue=pat.FName;
					break;
				case "Gender":
					if(field.RadioButtonValue==pat.Gender.ToString()) {
						field.FieldValue="X";
					}
					break;
				case "LName":
					field.FieldValue=pat.LName;
					break;
				case "MiddleI":
					field.FieldValue=pat.MiddleI;
					break;
				case "patient.priProvNameFL":
					field.FieldValue=Providers.GetFormalName(pat.PriProv);
					break;
				case "Preferred":
					field.FieldValue=pat.Preferred;
					break;
				case "Race":
					if(field.RadioButtonValue==PatientRaces.GetPatientRaceOldFromPatientRaces(pat.PatNum).ToString()) { //==pat.Race.ToString()) {
						field.FieldValue="X";
					}
					break;
				case "sheet.DateTimeSheet":
					field.FieldValue=sheet.DateTimeSheet.ToString();
					break;
			}
		}
	}

	private static void FillFieldsForDepositSlip(Sheet sheet,Deposit deposit) {
		var PatPayList=Payments.GetForDeposit(deposit.DepositNum);
		var ClaimPayList=ClaimPayments.GetForDeposit(deposit.DepositNum);
		//Stores all deposit list items. Used for depositList sheetfield item.
		var depositList=new List<string[]> ();
		//Stores the list of deposit items used to display itemized deposit items, not summary. It will not include deposit items that have the 
		//PayType=PrefC.AccountingCashPaymentType and the sheet has the sheetfield item of cashSumTotal.
		//Used for checkNumber(..) and depositItem(..) sheet fields 
		var listDepositItems=new List<string[]> ();
		//true if the sheet has a sheetfield named "cashSumTotal". Used to get a list of depositItems. 
		var hasCashSumTotal=sheet.GetSheetFieldByName("cashSumTotal")!=null;
		var colSize=new int[] {11,33,15,14,0};
		for(var i=0;i<PatPayList.Count;i++){
			var amount=PatPayList[i].PayAmt.ToString("F");
			colSize[4]=Math.Max(colSize[4],amount.Length);
		}
		for(var i=0;i<ClaimPayList.Length;i++){
			var amount=ClaimPayList[i].CheckAmt.ToString("F");
			colSize[4]=Math.Max(colSize[4],amount.Length);
		}
		decimal cashSumTotal=0;
		foreach(var payCur in PatPayList) { 
			var depositItem=new string[5];
			var date=payCur.PayDate.ToShortDateString();
			if(date.Length>colSize[0]){
				date=date.Substring(0,colSize[0]);
			}
			depositItem[0]=date.PadRight(colSize[0],' ')+" ";
			var pat=Patients.GetPat(payCur.PatNum);
			var name=pat.GetNameLF();
			if(name.Length>colSize[1]){
				name=name.Substring(0,colSize[1]);
			}
			depositItem[1]=name.PadRight(colSize[1],' ')+" ";
			var checkNum=payCur.CheckNum;
			if(checkNum.Length>colSize[2]){
				checkNum=checkNum.Substring(0,colSize[2]);
			}
			depositItem[2]=checkNum.PadRight(colSize[2],' ')+" ";
			var bankBranch=payCur.BankBranch;
			if(bankBranch.Length>colSize[3]){
				bankBranch=bankBranch.Substring(0,colSize[3]);
			}
			depositItem[3]=bankBranch.PadRight(colSize[3],' ')+" ";
			depositItem[4]=payCur.PayAmt.ToString("F").PadLeft(colSize[4],' ');
			depositList.Add(depositItem);
			if(hasCashSumTotal && payCur.PayType==PrefC.GetLong(PrefName.AccountingCashPaymentType)) {
				cashSumTotal+=(decimal)payCur.PayAmt;
				continue;
			}
			listDepositItems.Add(depositItem);
		}
		foreach(var claimPayCur in ClaimPayList) {
			var depositItem=new string[5];
			var date=claimPayCur.CheckDate.ToShortDateString();
			if(date.Length>colSize[0]){
				date=date.Substring(0,colSize[0]);
			}
			depositItem[0]=date.PadRight(colSize[0],' ')+" ";
			var name=claimPayCur.CarrierName;
			if(name.Length>colSize[1]){
				name=name.Substring(0,colSize[1]);
			}
			depositItem[1]=name.PadRight(colSize[1],' ')+" ";
			var checkNum=claimPayCur.CheckNum;
			if(checkNum.Length>colSize[2]){
				checkNum=checkNum.Substring(0,colSize[2]);
			}
			depositItem[2]=checkNum.PadRight(colSize[2],' ')+" ";
			var bankBranch=claimPayCur.BankBranch;
			if(bankBranch.Length>colSize[3]){
				bankBranch=bankBranch.Substring(0,colSize[3]);
			}
			depositItem[3]=bankBranch.PadRight(colSize[3],' ')+" ";
			depositItem[4]=claimPayCur.CheckAmt.ToString("F").PadLeft(colSize[4],' ');
			depositList.Add(depositItem);
			if(hasCashSumTotal && claimPayCur.PayType==PrefC.GetLong(PrefName.AccountingCashPaymentType)) {
				continue;
			}
			listDepositItems.Add(depositItem);
		}
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "cashSumTotal":
					field.FieldValue=cashSumTotal.ToString("n").PadLeft(12);
					break;
				case "checkNumber01":
					if(listDepositItems.Count>=1) {
						field.FieldValue=listDepositItems[0][2].PadLeft(15);
					}
					break;
				case "checkNumber02":
					if(listDepositItems.Count>=2) {
						field.FieldValue=listDepositItems[1][2].PadLeft(15);
					}
					break;
				case "checkNumber03":
					if(listDepositItems.Count>=3) {
						field.FieldValue=listDepositItems[2][2].PadLeft(15);
					}
					break;
				case "checkNumber04":
					if(listDepositItems.Count>=4) {
						field.FieldValue=listDepositItems[3][2].PadLeft(15);
					}
					break;
				case "checkNumber05":
					if(listDepositItems.Count>=5) {
						field.FieldValue=listDepositItems[4][2].PadLeft(15);
					}
					break;
				case "checkNumber06":
					if(listDepositItems.Count>=6) {
						field.FieldValue=listDepositItems[5][2].PadLeft(15);
					}
					break;
				case "checkNumber07":
					if(listDepositItems.Count>=7) {
						field.FieldValue=listDepositItems[6][2].PadLeft(15);
					}
					break;
				case "checkNumber08":
					if(listDepositItems.Count>=8) {
						field.FieldValue=listDepositItems[7][2].PadLeft(15);
					}
					break;
				case "checkNumber09":
					if(listDepositItems.Count>=9) {
						field.FieldValue=listDepositItems[8][2].PadLeft(15);
					}
					break;
				case "checkNumber10":
					if(listDepositItems.Count>=10) {
						field.FieldValue=listDepositItems[9][2].PadLeft(15);
					}
					break;
				case "checkNumber11":
					if(listDepositItems.Count>=11) {
						field.FieldValue=listDepositItems[10][2].PadLeft(15);
					}
					break;
				case "checkNumber12":
					if(listDepositItems.Count>=12) {
						field.FieldValue=listDepositItems[11][2].PadLeft(15);
					}
					break;
				case "checkNumber13":
					if(listDepositItems.Count>=13) {
						field.FieldValue=listDepositItems[12][2].PadLeft(15);
					}
					break;
				case "checkNumber14":
					if(listDepositItems.Count>=14) {
						field.FieldValue=listDepositItems[13][2].PadLeft(15);
					}
					break;
				case "checkNumber15":
					if(listDepositItems.Count>=15) {
						field.FieldValue=listDepositItems[14][2].PadLeft(15);
					}
					break;
				case "checkNumber16":
					if(listDepositItems.Count>=16) {
						field.FieldValue=listDepositItems[15][2].PadLeft(15);
					}
					break;
				case "checkNumber17":
					if(listDepositItems.Count>=17) {
						field.FieldValue=listDepositItems[16][2].PadLeft(15);
					}
					break;
				case "checkNumber18":
					if(listDepositItems.Count>=18) {
						field.FieldValue=listDepositItems[17][2].PadLeft(15);
					}
					break;
				case "deposit.BankAccountInfo":
					field.FieldValue=deposit.BankAccountInfo;
					break;
				case "deposit.DateDeposit":
					field.FieldValue=deposit.DateDeposit.ToShortDateString();
					break;
				case "depositList":
					var depositListB=new StringBuilder(Lans.g("Deposits","Date").PadRight(12)+Lans.g("Deposits","Name").PadRight(34)
					                                                                         +Lans.g("Deposits","Check Number").PadRight(16)+Lans.g("Deposits","Bank-Branch").PadRight(15)+Lans.g("Deposits","Amount")+Environment.NewLine);
					for(var i=0;i<depositList.Count;i++){
						if(i>0){
							depositListB.Append(Environment.NewLine);
						}
						for(var j=0;j<5;j++){
							depositListB.Append(depositList[i][j]);
						}
					}
					field.FieldValue=depositListB.ToString();
					break;
				case "depositTotal":
					decimal total=0;
					for(var i=0;i<PatPayList.Count;i++){
						total+=(decimal)PatPayList[i].PayAmt;
					}
					for(var i=0;i<ClaimPayList.Length;i++){
						total+=(decimal)ClaimPayList[i].CheckAmt;
					}
					field.FieldValue=total.ToString("n").PadLeft(12,' ');
					break;
				case "depositItemCount":
					field.FieldValue=(listDepositItems.Count+(cashSumTotal>0?1:0)).ToString().PadLeft(2,'0');
					break;
				case "depositItem01":
					if(listDepositItems.Count>=1){
						field.FieldValue=listDepositItems[0][4].PadLeft(12,' ');
					}
					break;
				case "depositItem02":
					if(listDepositItems.Count>=2){
						field.FieldValue=listDepositItems[1][4].PadLeft(12,' ');
					}
					break;
				case "depositItem03":
					if(listDepositItems.Count>=3){
						field.FieldValue=listDepositItems[2][4].PadLeft(12,' ');
					}
					break;
				case "depositItem04":
					if(listDepositItems.Count>=4){
						field.FieldValue=listDepositItems[3][4].PadLeft(12,' ');
					}
					break;
				case "depositItem05":
					if(listDepositItems.Count>=5){
						field.FieldValue=listDepositItems[4][4].PadLeft(12,' ');
					}
					break;
				case "depositItem06":
					if(listDepositItems.Count>=6){
						field.FieldValue=listDepositItems[5][4].PadLeft(12,' ');
					}
					break;
				case "depositItem07":
					if(listDepositItems.Count>=7){
						field.FieldValue=listDepositItems[6][4].PadLeft(12,' ');
					}
					break;
				case "depositItem08":
					if(listDepositItems.Count>=8){
						field.FieldValue=listDepositItems[7][4].PadLeft(12,' ');
					}
					break;
				case "depositItem09":
					if(listDepositItems.Count>=9){
						field.FieldValue=listDepositItems[8][4].PadLeft(12,' ');
					}
					break;
				case "depositItem10":
					if(listDepositItems.Count>=10){
						field.FieldValue=listDepositItems[9][4].PadLeft(12,' ');
					}
					break;
				case "depositItem11":
					if(listDepositItems.Count>=11){
						field.FieldValue=listDepositItems[10][4].PadLeft(12,' ');
					}
					break;
				case "depositItem12":
					if(listDepositItems.Count>=12){
						field.FieldValue=listDepositItems[11][4].PadLeft(12,' ');
					}
					break;
				case "depositItem13":
					if(listDepositItems.Count>=13){
						field.FieldValue=listDepositItems[12][4].PadLeft(12,' ');
					}
					break;
				case "depositItem14":
					if(listDepositItems.Count>=14){
						field.FieldValue=listDepositItems[13][4].PadLeft(12,' ');
					}
					break;
				case "depositItem15":
					if(listDepositItems.Count>=15){
						field.FieldValue=listDepositItems[14][4].PadLeft(12,' ');
					}
					break;
				case "depositItem16":
					if(listDepositItems.Count>=16){
						field.FieldValue=listDepositItems[15][4].PadLeft(12,' ');
					}
					break;
				case "depositItem17":
					if(listDepositItems.Count>=17){
						field.FieldValue=listDepositItems[16][4].PadLeft(12,' ');
					}
					break;
				case "depositItem18":
					if(listDepositItems.Count>=18){
						field.FieldValue=listDepositItems[17][4].PadLeft(12,' ');
					}
					break;
			}
		}
	}

	private static void FillFieldsForStatement(Sheet sheet,Statement statement,DataSet dataSet,Patient patient,Patient patientGuar=null) {
		long patNum;
		if(statement.SuperFamily!=0) {//Superfamily statement
			patNum=statement.SuperFamily;
		}
		else {
			patNum=statement.PatNum;
		}
		if(dataSet==null) {
			//Expect a DataSet to exist and passed into this method but in case its null, get the DataSet
			dataSet=AccountModules.GetStatementDataSet(statement,false,false);
		}
		if(dataSet!=null) {
			//Update statement.BalTotal and statement.InsEst in memory and DB, values will then be used to fill out the sheet
			Statements.CalcBalTotalInsEst(statement,dataSet);
			Statements.Update(statement);
		}
		patient=(patient==null || patient.PatNum!=patNum ? Patients.GetPat(patNum) : patient);
		patientGuar=(patientGuar==null || patientGuar.PatNum!=patient.Guarantor ? Patients.GetPat(patient.Guarantor) : patientGuar);
		var tableAppt=dataSet.Tables["appts"];
		var stringArrayTotInsBalVals=CalcStatementVals(sheet,statement,dataSet,patient,patientGuar);
		var totInsBalLabs=totInsBalLabsHelper(sheet,statement);
		if(tableAppt==null){
			tableAppt=new DataTable();
		}
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "accountNumber":
					#region Account Number
					field.FieldValue=Lans.g("Statements","Account Number")+" ";
					if(PrefC.GetBool(PrefName.StatementAccountsUseChartNumber)) {
						field.FieldValue+=patientGuar.ChartNumber;
					}
					else {
						field.FieldValue+=patientGuar.PatNum;
					}
					#endregion
					break;
				case "futureAppointments":
					#region Future Appointments
					if(!statement.IsReceipt && !statement.IsInvoice) {
						if(tableAppt.Rows.Count>0) {
							field.FieldValue=Lans.g("Statements","Scheduled Appointments:");
						}
						for(var i=0;i<tableAppt.Rows.Count;i++) {
							field.FieldValue+="\r\n"+tableAppt.Rows[i]["descript"].ToString();
						}
					}
					#endregion
					break;
				case "statement.NoteBold":
					field.FieldValue=statement.NoteBold;
					if(field.FieldValue==null) {
						field.FieldValue="";
					}
					break;
				case "statement.Note":
					field.FieldValue=statement.Note;
					if(field.FieldValue==null) {
						field.FieldValue="";
					}
					break;
				case "totalLabel":
					field.FieldValue=totInsBalLabs[0];
					break;
				case "insEstLabel":
					field.FieldValue=totInsBalLabs[1];
					break;
				case "balanceLabel":
					field.FieldValue=totInsBalLabs[2];
					break;
				case "invoicePaymentLabel": //only for invoices
					field.FieldValue=totInsBalLabs[3];
					break;
				case "invoiceTotalLabel"://only for invoices
					field.FieldValue=totInsBalLabs[4];
					break;
				case "totalValue":
					field.FieldValue=statement.BalTotal.ToString("c");
					break;
				case "insEstValue":
					field.FieldValue=statement.InsEst.ToString("c");
					break;
				case "balanceValue":
					field.FieldValue=stringArrayTotInsBalVals[0];
					break;
				case "amountDueValue":
					try {
						field.FieldValue=SIn.Double(SheetDataTableUtil.GetDataTableForGridType(sheet,dataSet,"StatementEnclosed",statement,null).Rows[0][0].ToString()).ToString("C");
					}
					catch {
						field.FieldValue=0.ToString("C");
					}
					break;
				case "payPlanAmtDueValue":
					//Don't fill payPlanAmtDueValue field for limited statements.
					if(statement.StatementType==StmtType.LimitedStatement) {
						field.FieldValue="N/A";
						break;
					}
					var tableMisc=dataSet.Tables["misc"];
					if(tableMisc==null){
						tableMisc=new DataTable();	
					}
					var payPlanDue=tableMisc.Select().Where(x => x["descript"].ToString()=="payPlanDue").Sum(x => SIn.Double(x["value"].ToString()));
					field.FieldValue=payPlanDue.ToString("c");
					break;
				case "invoicePaymentValue"://only for invoices
					field.FieldValue=stringArrayTotInsBalVals[1];
					break;
				case "invoiceTotalValue"://only for invoices
					field.FieldValue=stringArrayTotInsBalVals[2];
					break;
				case "statementReceiptInvoice":
					#region Sta/Rec/Inv
					if(statement.IsInvoice) {
						if(CultureInfo.CurrentCulture.Name=="en-NZ" || CultureInfo.CurrentCulture.Name=="en-AU") {//New Zealand and Australia
							field.FieldValue=Lans.g("Statements","TAX INVOICE");
						}
						else {
							field.FieldValue=Lans.g("Statements","INVOICE")+" #"+statement.StatementNum.ToString();
						}
					}
					else if(statement.IsReceipt) {
						field.FieldValue=Lans.g("Statements","RECEIPT");
						if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
							field.FieldValue+=" #"+statement.StatementNum.ToString();
						}
					}
					else {
						field.FieldValue=Lans.g("Statements","STATEMENT");
						if(statement.StatementType==StmtType.LimitedStatement) {
							field.FieldValue+=" ("+Lans.g("Statements","Limited")+")";
						}
					}
					#endregion
					break;
				case "returnAddress":
					#region ReturnAddress
					if(!PrefC.GetBool(PrefName.StatementShowReturnAddress)) {
						field.FieldValue="";
						break;
					}
					#region Practice Address
					if(true && Clinics.GetCount() > 0 //if using clinics
					        && Clinics.GetClinic(patientGuar.ClinicNum)!=null)//and this patient assigned to a clinic
					{
						var clinic=Clinics.GetClinic(patientGuar.ClinicNum);
						field.FieldValue=clinic.Description+"\r\n";
						if(CultureInfo.CurrentCulture.Name=="en-AU") {//Australia
							var defaultProv=Providers.GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
							field.FieldValue+="ABN: "+defaultProv.NationalProviderId+"\r\n";
						}
						if(CultureInfo.CurrentCulture.Name=="en-NZ") {//New Zealand
							var defaultProv=Providers.GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
							field.FieldValue+="GST: "+defaultProv.Ssn+"\r\n";
						}
						field.FieldValue+=clinic.AddressLine1+"\r\n";
						if(clinic.AddressLine2!="") {
							field.FieldValue+=clinic.AddressLine2+"\r\n";
						}
						if(CultureInfo.CurrentCulture.Name.EndsWith("CH")) {//CH is for switzerland. eg de-CH
							field.FieldValue+=clinic.Zip+" "+clinic.City+"\r\n";
						}
						else if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
							field.FieldValue+=clinic.City+" "+clinic.Zip+"\r\n";
						}
						else {
							field.FieldValue+=clinic.City+", "+clinic.State+" "+clinic.Zip+"\r\n";
						}
						if(clinic.PhoneNumber.Length==10) {
							field.FieldValue+=TelephoneNumbers.ReFormat(clinic.PhoneNumber)+"\r\n";
						}
						else {
							field.FieldValue+=clinic.PhoneNumber+"\r\n";
						}
					}
					else {//no clinics
						field.FieldValue=PrefC.GetString(PrefName.PracticeTitle)+"\r\n";
						if(CultureInfo.CurrentCulture.Name=="en-AU") {//Australia
							var defaultProv=Providers.GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
							field.FieldValue+="ABN: "+defaultProv.NationalProviderId+"\r\n";
						}
						if(CultureInfo.CurrentCulture.Name=="en-NZ") {//New Zealand
							var defaultProv=Providers.GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
							field.FieldValue+="GST: "+defaultProv.Ssn+"\r\n";
						}
						field.FieldValue+=PrefC.GetString(PrefName.PracticeAddress)+"\r\n";
						if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
							field.FieldValue+=PrefC.GetString(PrefName.PracticeAddress2)+"\r\n";
						}
						if(CultureInfo.CurrentCulture.Name.EndsWith("CH")) {//CH is for switzerland. eg de-CH
							field.FieldValue+=PrefC.GetString(PrefName.PracticeZip)+" "+PrefC.GetString(PrefName.PracticeCity)+"\r\n";
						}
						else if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
							field.FieldValue+=PrefC.GetString(PrefName.PracticeCity)+" "+PrefC.GetString(PrefName.PracticeZip)+"\r\n";
						}
						else {
							field.FieldValue+=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip)+"\r\n";
						}
						var practicePhone=PrefC.GetString(PrefName.PracticePhone);
						if(practicePhone.Length==10) {
							field.FieldValue+=TelephoneNumbers.ReFormat(practicePhone)+"\r\n";
						}
						else {
							field.FieldValue+=practicePhone+"\r\n";
						}
					}
					#endregion
					#endregion
					break;
				case "billingAddress":
					#region BillingAddress
					if(statement.SinglePatient){
						field.FieldValue=patient.GetNameFLnoPref()+"\r\n";
					}
					else{
						field.FieldValue=patientGuar.GetNameFLFormal()+"\r\n";
					}
					field.FieldValue+=patientGuar.Address+"\r\n";
					if(patientGuar.Address2!="") {
						field.FieldValue+=patientGuar.Address2+"\r\n";
					}
					if(CultureInfo.CurrentCulture.Name.EndsWith("CH")) {//CH is for switzerland. eg de-CH
						field.FieldValue+=(patientGuar.Zip+" "+patientGuar.City).Trim();//no line break
					}
					else if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
						field.FieldValue+=(patientGuar.City+" "+patientGuar.Zip).Trim();//no line break
					}
					else {
						field.FieldValue+=((patientGuar.City+", "+patientGuar.State).Trim(new[] { ',',' ' })+" "+patientGuar.Zip).Trim();//no line break
					}
					if(!string.IsNullOrWhiteSpace(patientGuar.Country)) {
						if(CultureInfo.CurrentCulture.Name.EndsWith("CH")||CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//if Singapore or Switzerland
							if(!string.IsNullOrWhiteSpace(patientGuar.City+patientGuar.Zip)) {//and either city or zip are not blank, add line break
								field.FieldValue+="\r\n";
							}
						}
						else {//all other cultures
							if(!string.IsNullOrWhiteSpace(patientGuar.City+patientGuar.State+patientGuar.Zip)) {//any field, city, state or zip contain data, add line break
								field.FieldValue+="\r\n";
							}
						}
						field.FieldValue+=patientGuar.Country;
					}
					#endregion
					break;
				case "practiceTitle":
					field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
					break;
				case "statementIsCopy":
					field.FieldValue=(statement.IsInvoiceCopy?Lans.g("Statements","COPY"):"");
					break;
				case "statementIsTaxReceipt":
					//if(!CultureInfo.CurrentCulture.Name.EndsWith("CA")) { field.FieldValue=""; break; }
					field.FieldValue=(statement.IsReceipt?Lans.g("Statements","KEEP THIS RECEIPT FOR INCOME TAX PURPOSES"):"");
					break;
				case "practiceAddress":
					field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
					if(PrefC.GetString(PrefName.PracticeAddress2) != "") {
						field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
					}
					break;
				case "practiceCityStateZip":
					field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "
					                                                       +PrefC.GetString(PrefName.PracticeST)+"  "
					                                                       +PrefC.GetString(PrefName.PracticeZip);
					break;
				case "statement.DateSent":
					field.FieldValue=statement.DateSent.ToShortDateString();
					break;
				case "patient.salutation":
					field.FieldValue="Dear "+patient.GetSalutation()+":";
					break;
				case "patient.priProvNameFL":
					field.FieldValue=Providers.GetFormalName(patient.PriProv);
					break;
				case "ProviderLegendAUS":
					#region ProviderLegendAUS
					if(CultureInfo.CurrentCulture.Name!="en-AU") {//English (Australia)
						field.FieldValue="";
						break;
					}
					Providers.RefreshCache();
					var listProviders=Providers.GetDeepCopy(true);
					field.FieldValue="PROVIDERS:"+"\r\n";
					for(var i=0;i<listProviders.Count;i++) {//All non-hidden providers are added to the legend.
						var prov=listProviders[i];
						var suffix="";
						if(prov.Suffix.Trim()!=""){
							suffix=", "+prov.Suffix.Trim();
						}
						field.FieldValue+=prov.Abbr+" - "+prov.FirstName+" "+prov.LastName+suffix+" - "+prov.MedicaidId+"\r\n";
					}
					#endregion
					break;
				case "invoicePayPlanValue":
					field.FieldValue=stringArrayTotInsBalVals[3];
					break;
				case "invoicePayPlanLabel":
					field.FieldValue=totInsBalLabs[5];
					break;
				case "StatementNum":
					field.FieldValue+=" #"+statement.StatementNum.ToString();
					break;
			}
		}
	}

	private static void FillFieldsForTreatPlan(Sheet sheet,Patient pat) {
		var treatPlan=(TreatPlan)SheetParameter.GetParamByName(sheet.Parameters,"TreatPlan").ParamValue;
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "Heading":
					field.FieldValue=treatPlan.Heading;
					break;
				case "DateTSigned":
					field.FieldValue=(treatPlan.DateTSigned.Year > 1880 ? treatPlan.DateTSigned.ToString() : "");
					break;
				case "DateTPracticeSigned":
					field.FieldValue=(treatPlan.DateTPracticeSigned.Year > 1880 ? treatPlan.DateTPracticeSigned.ToString() : "");
					break;
				case "defaultHeading":
					var value="";
					ClinicDto clinic;
					if(pat.ClinicNum==0 || !true) {
						clinic=Clinics.GetPracticeAsClinicZero();
					}
					else {
						clinic=Clinics.GetClinic(pat.ClinicNum);
					}
					value=clinic.Description;
					if(clinic.PhoneNumber.Length==10) {
						value+="\r\n"+TelephoneNumbers.ReFormat(clinic.PhoneNumber);
					}
					else {
						value+="\r\n"+clinic.PhoneNumber;
					}
					value+="\r\n"+pat.GetNameFLFormal()+", DOB "+pat.Birthdate.ToShortDateString();
					if(treatPlan.ResponsParty!=0) {
						value+="\r\n"+Lans.g("ContrTreat","Responsible Party")+": "+Patients.GetLim(treatPlan.ResponsParty).GetNameFL();
					}
					if(treatPlan.TPStatus==TreatPlanStatus.Saved) {
						value+="\r\n"+treatPlan.DateTP.ToShortDateString();
					}
					else {//active or inactive TP
						value+="\r\n"+DateTime.Today.ToShortDateString();
					}
					field.FieldValue=value;
					break;
				case "Note":
					field.FieldValue=treatPlan.Note;
					break;
				case "SignatureText":
					field.FieldValue=treatPlan.SignatureText;
					break;
				case "SignaturePracticeText":
					field.FieldValue=treatPlan.SignaturePracticeText;
					break;
				case "tpPatPortionEst":
					var tpFees=treatPlan.ListProcTPs.Sum(x => (decimal)x.PatAmt);
					field.FieldValue=tpFees.ToString("f");
					break;
			}
		}
	}

	///<summary>Returns 6 label strings: Total, Insurance, Balance. These labels change based on various settings.</summary>
	private static string[] totInsBalLabsHelper(Sheet sheet,Statement statement) {
		var sLine1="";//Total
		var sLine2="";//InsExt
		var sLine3="";//Balance
		var sLine4="";//InvoicePayments
		var sLine5="";//InvoiceBalRem
		var sLine6="";//Invoice Pay Plan Charges
		if(statement.IsInvoice) {//invoices can't be superstatements
			sLine1=Lans.g("Statements","Procedures:");
			sLine2=Lans.g("Statements","Adjustments:");
			sLine6=Lans.g("Statements","Pay Plan Charges:");
			sLine3=Lans.g("Statements","Total:");
			if(PrefC.GetBool(PrefName.InvoicePaymentsGridShowNetProd)) {
				sLine4=Lans.g("Statements","Payments & WriteOffs");
			}
			else {
				sLine4=Lans.g("Statements","Payments:");
			}
			sLine5=Lans.g("Statements","Balance Remaining:");
		}
		else if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
			if(statement.SuperFamily!=0) {
				sLine1=Lans.g("Statements","Sum of Balances:");
			}
			else {
				sLine1=Lans.g("Statements","Balance:");
			}
			//sLine2=Lans.g("Statements","Ins Pending:");
			//sLine3=Lans.g("Statements","After Ins:");
		}
		else {//this is more common
			if(statement.SuperFamily!=0) {
				sLine1=Lans.g("Statements","Sum of Totals:");
				sLine2=Lans.g("Statements","-Sum of Ins Estimates:");
				sLine3=Lans.g("Statements","=Sum of Balances:");
			}
			else {
				sLine1=Lans.g("Statements","Total:");
				sLine2=Lans.g("Statements","-Ins Estimate:");
				sLine3=Lans.g("Statements","=Balance:");
			}
		}
		//sLine4 and sLine5 are only used in invoices
		return new string[] { sLine1,sLine2,sLine3,sLine4,sLine5,sLine6 };
	}

	///<summary>Returns 4 values that will be displayed on statements. Does not include the two big ones: BalTotal and InsEst. 3 of the 4 numbers are not even used for most statements. Returns Balance, InvoicePayments, InvoiceBalanceRemaining, and Invoice Pay Plan Charges. Set statement.BalTotal and statement.InsEst using Statements.CalcTotInsBalVals() before calling this method. These values change based on various settings. </summary>
	private static string[] CalcStatementVals(Sheet sheet,Statement statement,DataSet dataSet,Patient patient,Patient patientGuar) {
		var sLine3="";//Balance
		var sLine4="";//InvoicePayments
		var sLine5="";//InvoiceBalanceRemaining
		var sLine6="";//Invoice Pay Plan Charges
		DataTable tableAcct;
		var tableMisc=dataSet.Tables["misc"];
		if(tableMisc==null) {
			tableMisc=new DataTable();
		}
		if(statement.IsInvoice) {
			double amtPayPlan=0;
			string tableName;
			for(var i=0;i<dataSet.Tables.Count;i++) {
				tableAcct=dataSet.Tables[i];
				tableName=tableAcct.TableName;
				if(!tableName.StartsWith("account")) {
					continue;
				}
				for(var p=0;p<tableAcct.Rows.Count;p++) {
					//The procedure and adjustment amounts have already been caclulated and stored in statement.BalTotal and InsEst respectively, but the pay plan amount still needs to be calculated.
					if(tableAcct.Rows[p]["PayPlanChargeNum"].ToString()!="0") {
						amtPayPlan+=SIn.Double(tableAcct.Rows[p]["chargesDouble"].ToString());
					}
				}
			}
			var paymentValue=SheetDataTableUtil.GetDataTableForGridType(sheet,dataSet,"StatementInvoicePayment",statement,null).Select()
				.Sum(x => SIn.Double(x["amt"].ToString()));
			sLine3+=(statement.BalTotal+statement.InsEst+amtPayPlan).ToString("c");
			sLine4+=paymentValue.ToString("c");
			sLine5+=(paymentValue+statement.BalTotal+statement.InsEst+amtPayPlan).ToString("c");
			sLine6+=amtPayPlan.ToString("c");
		}
		else if(statement.StatementType==StmtType.LimitedStatement) {
			if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {

			}
			else {
				sLine3+=(statement.BalTotal-statement.InsEst).ToString("c");
			}
		}
		else if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
			//
		}
		else {//more common
			if(statement.SinglePatient) {
				double amtPatInsEst=0;
				for(var m=0;m<tableMisc.Rows.Count;m++) {
					if(tableMisc.Rows[m]["descript"].ToString()=="patInsEst") {
						amtPatInsEst=SIn.Double(tableMisc.Rows[m]["value"].ToString());
					}
				}
				sLine3+=(statement.BalTotal-statement.InsEst).ToString("c");
			}
			else {
				if(statement.SuperFamily!=0) {//Superfam statement
					sLine3+=(statement.BalTotal-statement.InsEst).ToString("c");
				}
				else {
					sLine3+=(statement.BalTotal-statement.InsEst).ToString("c");
				}
			}
		}
		//sLine4, sLine5 and sLine6 are only used in invoices
		return new string[] { sLine3,sLine4,sLine5,sLine6 };
	}

	private static int CompareSheetFieldNames(SheetField input1,SheetField input2) {
		if(input1.IsRequired && !input2.IsRequired) {
			return -1;
		}
		if(!input1.IsRequired && input2.IsRequired) {
			return 1;
		}
		if(Convert.ToInt32(input1.FieldName.Remove(0,8)) < Convert.ToInt32(input2.FieldName.Remove(0,8))) {
			return -1;
		}
		if(Convert.ToInt32(input1.FieldName.Remove(0,8)) > Convert.ToInt32(input2.FieldName.Remove(0,8))) {
			return 1;
		}
		return 0;
	}

	private static void FillFieldsForPaymentPlan(Sheet sheet,Patient pat) {
		var payPlan=(PayPlan)SheetParameter.GetParamByName(sheet.Parameters,"payplan").ParamValue;
		var principal=GetParamByName(sheet,"Principal");
		var totFinCharge=GetParamByName(sheet,"totalFinanceCharge");
		var totCostLoan=GetParamByName(sheet,"totalCostOfLoan");
		var PatGuar=Patients.GetPat(pat.Guarantor);
		var listCreditCard=CreditCards.GetForPayPlan(payPlan.PayPlanNum);
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "PracticeTitle":
					field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
					break;
				case "dateToday":
					field.FieldValue=DateTime.Today.ToShortDateString();
					break;
				case "nameLF":
					field.FieldValue=pat.GetNameLF();
					break;
				case "guarantor":
					field.FieldValue=PatGuar.GetNameLFnoPref();
					break;
				case "Principal":
					field.FieldValue=principal.ParamValue.ToString();
					break;
				case "DateOfAgreement":
					field.FieldValue=payPlan.PayPlanDate.ToShortDateString();
					break;
				case "APR":
					field.FieldValue=payPlan.APR.ToString("f1");
					break;
				case "totalFinanceCharge":
					field.FieldValue=((double)totFinCharge.ParamValue).ToString("n");
					break;
				case "totalCostOfLoan":
					field.FieldValue=totCostLoan.ParamValue.ToString();
					break;
				case "Note":
					field.FieldValue=payPlan.Note;
					break;
				case "ccNumberMaskedWithExp":
					field.FieldValue=listCreditCard.IsNullOrEmpty() ? "" : 
						string.Join(", ",listCreditCard.Select(x => $"{x.CCNumberMasked} Exp {x.CCExpiration.ToString("MM/yy")}"));
					break;
				case "TermsAndConditions":
					field.FieldValue=PayPlans.GetTermsAndConditionsString(payPlan);
					break;
			}
		}

	}
		
	private static void FillFieldsForERA(Sheet sheet) {
		var era=(X835)GetParamByName(sheet,"ERA").ParamValue;
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "PayerName":
					field.FieldValue=era.PayerName;
					break;
				case "PayerID":
					field.FieldValue=era.PayerId;
					break;
				case "PayerAddress":
					field.FieldValue=era.PayerAddress;
					break;
				case "PayerCity":
					field.FieldValue=era.PayerCity;
					break;
				case "PayerState":
					field.FieldValue=era.PayerState;
					break;
				case "PayerZip":
					field.FieldValue=era.PayerZip;
					break;
				case "PayerContactInfo":
					field.FieldValue=era.GetPayerContactInfo();
					break;
				case "PayeeName":
					field.FieldValue=era.PayeeName;
					break;
				case "PayeeId":
					field.FieldValue=era.PayeeId;
					break;
				case "TransHandlingDesc":
					field.FieldValue=era.TransactionHandlingDescript;
					break;
				case "PaymentMethod":
					field.FieldValue=era.PayMethodDescript;
					break;
				case "AcctNumEndingIn":
					field.FieldValue=era.AccountNumReceiving;
					break;
				case "Check#":
					field.FieldValue=era.TransRefNum;
					break;
				case "DateEffective":
					field.FieldValue=era.DateEffective.ToShortDateString();
					break;
				case "InsPaid":
					field.FieldValue=era.InsPaid.ToString("C2");
					break;
			}
		}
	}
		
	private static void FillFieldsForERAGridHeader(Sheet sheet) {
		var eraClaimPaid=(Hx835_Claim)GetParamByName(sheet,"EraClaimPaid").ParamValue;
		foreach(var field in sheet.SheetFields) {
			switch(field.FieldName) {
				case "Subscriber":
					field.FieldValue=eraClaimPaid.SubscriberName.ToString();
					break;
				case "Patient":
					field.FieldValue=eraClaimPaid.PatientName.ToString();
					break;
				case "ClaimIdentifier":
					field.FieldValue=eraClaimPaid.ClaimTrackingNumber.ToString();
					break;
				case "PayorControlNum":
					field.FieldValue=eraClaimPaid.PayerControlNumber.ToString();
					break;
				case "Status":
					field.FieldValue=eraClaimPaid.StatusCodeDescript.ToString();
					break;
				case "DateService":
					field.FieldValue=eraClaimPaid.DateServiceStart.ToShortDateString();
					break;
				case "ClaimFee":
					field.FieldValue=eraClaimPaid.ClaimFee.ToString("C2");
					break;
				case "InsPaid":
					field.FieldValue=eraClaimPaid.InsPaid.ToString("C2");
					break;
				case "PatientResponsibility":
					field.FieldValue=eraClaimPaid.PatientRespAmt.ToString("C2");
					break;
				case "DatePayerReceived":
					var value="";
					if(eraClaimPaid.DatePayerReceived!=DateTime.MinValue) {
						value=eraClaimPaid.DatePayerReceived.ToShortDateString();
					}
					field.FieldValue=value;
					break;
				case "ClaimIndexNum":
					var claimIndex=SIn.Int(GetParamByName(sheet,"ClaimIndexNum").ParamValue.ToString());
					if(claimIndex!=0) {//Is 0 when IsSingleClaim parameter is true.
						field.FieldValue=claimIndex.ToString();
					}
					break;
			}
		}
	}
}