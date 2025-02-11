using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CDT;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Core.Features.Providers;
using OpenDentBusiness.AutoComm;
using OpenDentBusiness.HL7;
using OpenDentBusiness.WebTypes;

namespace OpenDentBusiness;

public class Patients
{
    public const string LanguageDeclinedToSpecify = "Declined to Specify";

    public static string[] StringArrayPatNumForeignKeys
    {
        get
        {
            var stringArrayPatNumForeignKeys = new[]
            {
                "adjustment.PatNum",
                "allergy.PatNum",
                "appointment.PatNum",
                "apptgeneralmessagesent.PatNum",
                "apptremindersent.PatNum",
                "apptthankyousent.PatNum",
                "apptnewpatthankyousent.PatNum",
                "asapcomm.PatNum",
                "carecreditwebresponse.PatNum",
                "claim.PatNum",
                "claimproc.PatNum",
                "clinicerx.PatNum",
                "commlog.PatNum",
                "commoptout.PatNum",
                "confirmationrequest.PatNum",
                "creditcard.PatNum",
                "custrefentry.PatNumCust",
                "custrefentry.PatNumRef",
                //"custreference.PatNum",  //This is handled below.  We do not want to change patnum, the references form only shows entries for active patients.
                //"discountplansub.PatNum", //This is handled below. We want patients to keep their original discount plans if they have them, and only add under certain conditions.
                "disease.PatNum",
                //"document.PatNum",  //This is handled below when images are stored in the database and on the client side for images stored in the AtoZ folder due to the middle tier.
                "eclipboardimagecapture.PatNum",
                "ehramendment.PatNum",
                "ehrcareplan.PatNum",
                "ehrlab.PatNum",
                "ehrmeasureevent.PatNum",
                "ehrnotperformed.PatNum",
                //"ehrpatient.PatNum",  //This is handled below.  We do not want to change patnum here because there can only be one entry per patient.
                "ehrprovkey.PatNum",
                "ehrquarterlykey.PatNum",
                "ehrsummaryccd.PatNum",
                "emailmessage.PatNum",
                "emailmessage.PatNumSubj",
                "emailsecure.PatNum",
                "encounter.PatNum",
                "erxlog.PatNum",
                "eservicelog.PatNum",
                "etrans.PatNum",
                //"famaging.PatNum", //Taken care of down below as this should be the guarantor of the patient being merged into
                "familyhealth.PatNum",
                "erouting.PatNum",
                //formpat.FormPatNum IS NOT a PatNum so it is should not be merged.  It is the primary key.
                "formpat.PatNum",
                "guardian.PatNumChild", //This may create duplicate entries for a single patient and guardian
                "guardian.PatNumGuardian", //This may create duplicate entries for a single patient and guardian
                "hiequeue.PatNum",
                "histappointment.PatNum",
                "hl7msg.PatNum",
                "inssub.Subscriber",
                "installmentplan.PatNum",
                "intervention.PatNum",
                "labcase.PatNum",
                "labpanel.PatNum",
                "medicalorder.PatNum",
                //medicationpat.MedicationPatNum IS NOT a PatNum so it is should not be merged.  It is the primary key.
                "medicationpat.PatNum",
                "medlab.PatNum",
                "mount.PatNum",
                "mobileappdevice.PatNum",
                "mobiledatabyte.PatNum",
                "msgtopaysent.PatNum",
                "orthocase.PatNum",
                "orthohardware.PatNum",
                //"orthochartlog.PatNum",//this wouldn't affect a merge
                //"orthochart.PatNum",//Taken care of by orthochartrow
                "orthochartrow.PatNum",
                //"oidexternal.IDInternal",  //TODO:  Deal with these elegantly below, not always a patnum
                //"patfield.PatNum", //Taken care of below
                "patrestriction.PatNum",
                "patient.ResponsParty",
                //"patient.PatNum"  //We do not want to change patnum
                //"patient.Guarantor"  //This is taken care of below
                //"patient.PatNumCloneFrom", //We do not want to change this for historical purposes
                "patient.SuperFamily", //The patfrom guarantor was changed, so this should be updated
                //"patientlink.PatNumFrom",//We want to leave the link history unchanged so that audit entries display correctly. If we start using this table for other types of linkage besides merges, then we might need to include this column.
                //"patientlink.PatNumTo",//^^Ditto
                //"patientnote.PatNum"	//The patientnote table is ignored because only one record can exist for each patient.  The record in 'patFrom' remains so it can be accessed again if needed.
                "patientportalinvite.PatNum",
                //"patientrace.PatNum", //The patientrace table is ignored because we don't want duplicate races.  We could merge them but we would have to add specific code to stop duplicate races being inserted.
                "patplan.PatNum",
                "payconnectresponseweb.PatNum",
                "payment.PatNum",
                "payortype.PatNum",
                "payplan.Guarantor", //Treated as a patnum, because it is actually a guarantor for the payment plan, and not a patient guarantor.
                "payplan.PatNum",
                "payplancharge.Guarantor", //Treated as a patnum, because it is actually a guarantor for the payment plan, and not a patient guarantor.
                "payplancharge.PatNum",
                "paysplit.PatNum",
                "perioexam.PatNum",
                "phonenumber.PatNum",
                "popup.PatNum",
                "procedurelog.PatNum",
                //"procmultivisit.PatNum",
                "procnote.PatNum",
                "proctp.PatNum",
                "promotionlog.PatNum",
                "providererx.PatNum", //For non-HQ this should always be 0.
                //question.FormPatNum IS NOT a PatNum so it is should not be merged.  It is a FKey to FormPat.FormPatNum
                "question.PatNum",
                "reactivation.PatNum",
                //"recall.PatNum",  //We do not merge recall entries because it would cause duplicate recall entries.  Instead, update current recall entries.
                "recurringcharge.PatNum",
                "refattach.PatNum",
                //"referral.PatNum",  //This is synched with the new information below.
                "registrationkey.PatNum",
                "repeatcharge.PatNum",
                "reqstudent.PatNum",
                "rxpat.PatNum",
                //"screen.ScreenPatNum", //IS NOT a PatNum so it is should not be merged.  FKey to screenpat.ScreenPatNum.
                "screenpat.PatNum",
                //screenpat.ScreenPatNum IS NOT a PatNum so it is should not be merged.  It is a primary key.
                //"securitylog.FKey",  //This would only matter when the FKey pointed to a PatNum.  Currently this is only for the PatientPortal permission
                //  which per Allen is not needed to be merged. 11/06/2015.
                //"securitylog.PatNum",//Changing the PatNum of a securitylog record will cause it to show a red (untrusted) in the audit trail.
                //  Best to preserve history in the securitylog and leave the corresponding PatNums static.
                "sheet.PatNum",
                "smsfrommobile.PatNum",
                "smstomobile.PatNum",
                "statement.PatNum",
                //task.KeyNum,  //Taken care of in a seperate step, because it is not always a patnum.
                //taskhist.KeyNum,  //Taken care of in a seperate step, because it is not always a patnum.
                "terminalactive.PatNum",
                "toothinitial.PatNum",
                "treatplan.PatNum",
                "treatplan.ResponsParty",
                "treatplanparam.PatNum",
                //"tsitranslog.PatNum", //Taken care of down below as this should be the guarantor of the patient being merged into
                //vaccineobs.VaccinePatNum IS NOT a PatNum so it is should not be merged. It is the FK to the vaccinepat.VaccinePatNum.
                "vaccinepat.PatNum",
                //vaccinepat.VaccinePatNum IS NOT a PatNum so it is should not be merged. It is the primary key.
                "vitalsign.PatNum",
                "webschedrecall.PatNum",
                "xchargetransaction.PatNum",
                "xwebresponse.PatNum"
            };
            return stringArrayPatNumForeignKeys;
        }
    }

    public static Patient CreateNewPatient(string lName, string fName, DateTime birthDate, long priProv, long clinicNum, string securityLogMsg, LogSources logSource = LogSources.None, string email = "", string hmPhone = "", string wirelessPhone = "", PatientStatus patStatus = PatientStatus.Patient, long patNum = 0, bool setTxtOk = false)
    {
        var patient = new Patient();
        patient.LName = CreateNewPatientNameHelper(lName);
        patient.FName = CreateNewPatientNameHelper(fName);
        var doUseExistingPK = false;
        if (patNum > 0)
        {
            patient.PatNum = patNum;
            doUseExistingPK = true;
        }

        patient.Birthdate = birthDate;
        patient.PatStatus = patStatus;
        patient.BillingType = SIn.Long(ClinicPrefs.GetPrefValue(PrefName.PracticeDefaultBillType, clinicNum));
        patient.PriProv = priProv;
        patient.Gender = PatientGender.Unknown;
        patient.ClinicNum = clinicNum;
        patient.Email = email;
        patient.HmPhone = TelephoneNumbers.AutoFormat(hmPhone);
        patient.WirelessPhone = TelephoneNumbers.AutoFormat(wirelessPhone);
        if (setTxtOk && !wirelessPhone.IsNullOrEmpty()) patient.TxtMsgOk = YN.Yes;
        Insert(patient);
        SecurityLogs.MakeLogEntry(EnumPermType.PatientCreate, patient.PatNum, securityLogMsg, logSource);
        var custRef = new CustReference();
        custRef.PatNum = patient.PatNum;
        CustReferences.Insert(custRef);
        var PatOld = patient.Copy();
        patient.Guarantor = patient.PatNum;
        Update(patient, PatOld);
        return patient;
    }

    private static string CreateNewPatientNameHelper(string name)
    {
        if (name.Length == 1) return name.ToUpper();

        if (name.Length > 1) //eg Sp
            return name.Substring(0, 1).ToUpper() + name.Substring(1);

        return "";
    }

    public static Family GetFamily(long patNum)
    {
        return GetFamilies([patNum]).FirstOrDefault() ?? new Family();
    }

    public static List<Family> GetFamilies(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return [];
        var command = @"SELECT DISTINCT f.*,CASE WHEN f.Guarantor != f.PatNum THEN 1 ELSE 0 END AS IsNotGuar 
				FROM patient p
				INNER JOIN patient f ON f.Guarantor=p.Guarantor
				WHERE p.PatNum IN (" + string.Join(",", listPatNums.Select(x => (x))) + @")
				ORDER BY IsNotGuar, f.Birthdate";
        var listFamilies = new List<Family>();
        var listPatients = PatientCrud.SelectMany(command);
        foreach (var patient in listPatients) patient.Age = DateToAge(patient.Birthdate);
        var dictFamilyPatients = listPatients.GroupBy(x => x.Guarantor)
            .ToDictionary(y => y.Key, y => y.ToList());
        foreach (var kvp in dictFamilyPatients)
        {
            var family = new Family();
            family.ListPats = kvp.Value.ToArray();
            listFamilies.Add(family);
        }

        return listFamilies;
    }

    public static List<Patient> GetPatientData(long patNum)
    {
        var command = @"SELECT DISTINCT f.*,CASE WHEN f.Guarantor != f.PatNum THEN 1 ELSE 0 END AS IsNotGuar 
				FROM patient p
				INNER JOIN patient f ON f.Guarantor=p.Guarantor
				WHERE p.PatNum =" + (patNum)
                                  + " ORDER BY IsNotGuar, f.Birthdate";
        var listPatients = PatientCrud.SelectMany(command);
        for (var i = 0; i < listPatients.Count; i++) listPatients[i].Age = DateToAge(listPatients[i].Birthdate);
        return listPatients;
    }

    public static List<Patient> GetForFeeSched(long feeSchedNum)
    {
        var command = "SELECT * FROM patient WHERE FeeSched=" + (feeSchedNum);
        return PatientCrud.SelectMany(command);
    }

    public static Patient GetPat(long patNum)
    {
        if (patNum == 0) return null;

        var pat = PatientCrud.SelectOne(patNum);
        if (pat == null) return null; //used in eCW bridge
        pat.Age = DateToAge(pat.Birthdate);
        return pat;
    }

    public static Patient GetPatByChartNumber(string chartNumber)
    {
        if (chartNumber == "") return null;
        var command = "SELECT * FROM patient WHERE ChartNumber='" + SOut.String(chartNumber) + "'";
        Patient pat = null;
        try
        {
            pat = PatientCrud.SelectOne(command);
        }
        catch
        {
        }

        if (pat == null) return null;
        pat.Age = DateToAge(pat.Birthdate);
        return pat;
    }

    public static List<long> GetAllFamilyPatNums(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return [];
        var command = "SELECT patient.PatNum FROM patient "
                      + "INNER JOIN ("
                      + "SELECT DISTINCT Guarantor FROM patient WHERE PatNum IN (" + string.Join(",", listPatNums) + ")"
                      + ") guarnums ON guarnums.Guarantor=patient.Guarantor "
                      + "WHERE patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deleted);
        return Db.GetListLong(command);
    }

    public static List<long> GetAllFamilyPatNumsForGuars(List<long> listGuarNums)
    {
        if (listGuarNums == null || listGuarNums.Count < 1) return [];
        var command = "SELECT PatNum FROM patient WHERE Guarantor IN (" + string.Join(",", listGuarNums) + ")";
        return Db.GetListLong(command);
    }

    public static List<Patient> GetAllPatientsForGuarantor(long guarantorNum)
    {
        var command = "SELECT * FROM patient WHERE Guarantor=" + (guarantorNum);
        return PatientCrud.SelectMany(command);
    }

    public static List<long> GetAllFamilyPatNumsForSuperFam(List<long> listSuperFamNums)
    {
        listSuperFamNums?.RemoveAll(x => x <= 0); //if list is not null, remove all nums <= 0
        if ((listSuperFamNums?.Count ?? 0) == 0) //if list is null or has no nums > 0, return new list
            return [];

        var command = "SELECT PatNum FROM patient WHERE SuperFamily IN (" + string.Join(",", listSuperFamNums.Distinct()) + ")";
        return Db.GetListLong(command);
    }

    public static long Insert(Patient pat)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        pat.SecUserNumEntry = Security.CurUser.UserNum;
        pat.PatNum = PatientCrud.Insert(pat);
        pat.SecurityHash = HashFields(pat);
        PatientCrud.Update(pat);
        if (PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable)) PhoneNumbers.SyncPat(pat);
        return pat.PatNum;
    }

    public static bool Update(Patient patient, Patient oldPatient)
    {
        if (IsPatientHashValid(oldPatient)) patient.SecurityHash = HashFields(patient);
        var retval = PatientCrud.Update(patient, oldPatient);
        if (PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable)
            && (patient.HmPhone != oldPatient.HmPhone
                || patient.WkPhone != oldPatient.WkPhone
                || patient.WirelessPhone != oldPatient.WirelessPhone))
            PhoneNumbers.SyncPat(patient);
        return retval;
    }

    public static void Delete(Patient pat)
    {
        var command = "UPDATE patient SET PatStatus=" + ((int) PatientStatus.Deleted) + ", "
                      + "Guarantor=PatNum "
                      + "WHERE PatNum =" + pat.PatNum;
        Db.NonQ(command);
        //no need to call PhoneNumbers.SyncPat since only the status and guar are changed here
    }

    public static DataTable GetPtDataTable(PtTableSearchParams ptSearchArgs)
    {
        var exactMatchSnippet = GetExactMatchSnippet(ptSearchArgs);
        var phonedigits = StringTools.StripNonDigits(ptSearchArgs.Phone);
        var regexp = "";
        for (var i = 0; i < phonedigits.Length; i++)
        {
            if (i != 0) regexp += "[^0-9]*"; //zero or more intervening digits that are not numbers
            if (i == 3)
            {
                //If there is more than three digits and the first digit is 1, make it optional.
                if (phonedigits.StartsWith("1"))
                    regexp = "1?" + regexp.Substring(1);
                else
                    regexp = "1?[^0-9]*" + regexp; //add a leading 1 so that 1-800 numbers can show up simply by typing in 800 followed by the number.
            }

            regexp += phonedigits[i];
        }

        regexp = SOut.String(regexp); //just escape necessary characters here one time
        //Replaces spaces and punctation with wildcards because users should be able to type the following example and match certain addresses:
        //Search term: "4145 S Court St" should match "4145 S. Court St." in the database.
        var strAddress = Regex.Replace(ptSearchArgs.Address, @"[�\-.,:;_""'/\\)(#\s&]", "%");
        var phDigitsTrimmed = phonedigits.TrimStart('0', '1');
        //a single digit search is faster using REGEXP, so only use the phonenumber table if the pref is set and the phDigitsTrimmed length>1
        var usePhonenumTable = PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable) && phDigitsTrimmed.Length > 0;
        var useExactMatch = PrefC.GetBool(PrefName.EnterpriseExactMatchPhone);
        var exactMatchPhoneDigits = PrefC.GetInt(PrefName.EnterpriseExactMatchPhoneNumDigits);
        var phoneNumSearch = "";
        var likeQueryString = $@"
					AND phonenumber.PhoneNumberDigits LIKE '{SOut.String(phDigitsTrimmed)}%'";
        if (usePhonenumTable)
        {
            if (useExactMatch && phDigitsTrimmed.Length == exactMatchPhoneDigits)
                phoneNumSearch = $@"
					AND phonenumber.PhoneNumberDigits = '{SOut.String(phDigitsTrimmed)}'";
            else
                phoneNumSearch = likeQueryString;
        }

        var lNameMobileQueryString = "";
        var fNameMobileQueryString = "";
        if (ptSearchArgs.IsFromMobile)
        {
            lNameMobileQueryString += string.IsNullOrEmpty(ptSearchArgs.LName)
                ? ""
                : @$"AND (
				patient.LName LIKE '%{ptSearchArgs.LName}%' OR patient.FName LIKE '%{ptSearchArgs.LName}%' OR patient.Preferred LIKE '%{ptSearchArgs.LName}%')";
            fNameMobileQueryString += string.IsNullOrEmpty(ptSearchArgs.FName)
                ? ""
                : @$"AND (
				patient.LName LIKE '%{ptSearchArgs.FName}%' OR patient.FName LIKE '%{ptSearchArgs.FName}%' OR patient.Preferred LIKE '%{ptSearchArgs.FName}%')";
        }

        var command = $@"SELECT DISTINCT patient.PatNum,patient.LName,patient.FName,patient.MiddleI,patient.Preferred,patient.Birthdate,patient.SSN,
				patient.HmPhone,patient.WkPhone,patient.Address,patient.Address2,patient.PatStatus,patient.BillingType,patient.ChartNumber,patient.City,patient.State,patient.Zip,
				patient.PriProv,patient.SiteNum,patient.Email,patient.Country,patient.ClinicNum,patient.SecProv,patient.WirelessPhone,patient.TxtMsgOk,patient.DateFirstVisit,patient.MedUrgNote,patient.CreditType,patient.Ward,patient.AdmitDate,
				{exactMatchSnippet} isExactMatch,"
                      //using this sub-select instead of joining these two tables because the runtime is much better this way
                      //Example: large db with joins single clinic 19.8 sec, all clinics 32.484 sec; with sub-select single clinic 0.054 sec, all clinics 0.007 sec
                      + (!ptSearchArgs.HasSpecialty
                          ? "''"
                          : $@"
				(
					SELECT definition.ItemName FROM definition
					INNER JOIN deflink ON definition.DefNum=deflink.DefNum
					WHERE deflink.LinkType={SOut.Int((int) DefLinkType.Patient)}
					AND deflink.FKey=patient.PatNum
					AND definition.Category={SOut.Int((int) DefCat.ClinicSpecialty)}
					LIMIT 1
				)") + @" Specialty," //always include Specialty column, only populate if displaying specialty field
                      + (!false ? "" :
                          ptSearchArgs.Phone.IsNullOrEmpty() ? "'' OtherPhone, " : @"GROUP_CONCAT(DISTINCT phonenumber.PhoneNumberVal) AS OtherPhone,")
                      + (!false ? "" :
                          ptSearchArgs.RegKey.IsNullOrEmpty() ? "'' RegKey, " : @"registrationkey.RegKey,")
                      + (string.IsNullOrEmpty(ptSearchArgs.InvoiceNumber) ? "'' " : "statement.") + @"StatementNum
				FROM patient"
                      + (!usePhonenumTable
                          ? ""
                          : @"
				INNER JOIN phonenumber ON phonenumber.PatNum=patient.PatNum")
                      + (!false ? "" :
                          ptSearchArgs.Phone.IsNullOrEmpty() ? "" : $@"{(usePhonenumTable ? "" : $@"
				LEFT JOIN phonenumber ON phonenumber.PatNum=patient.PatNum
					AND phonenumber.PhoneType={SOut.Int((int) PhoneType.Other)}
				{(string.IsNullOrEmpty(regexp) ? "" : $@"
					AND phonenumber.PhoneNumberVal REGEXP '{regexp}'")}")}")
                      + (!false ? "" :
                          ptSearchArgs.RegKey.IsNullOrEmpty() ? "" : @"
				LEFT JOIN registrationkey ON patient.PatNum=registrationkey.PatNum")
                      + (string.IsNullOrEmpty(ptSearchArgs.SubscriberId)
                          ? ""
                          : @"
				LEFT JOIN patplan ON patplan.PatNum=patient.PatNum
				LEFT JOIN inssub ON patplan.InsSubNum=inssub.InsSubNum")
                      + (string.IsNullOrEmpty(ptSearchArgs.InvoiceNumber)
                          ? ""
                          : @"
				LEFT JOIN statement ON statement.PatNum=patient.PatNum AND statement.IsInvoice") + $@"
				WHERE patient.PatStatus NOT IN({SOut.Int((int) PatientStatus.Deleted)}"
                      + (!ptSearchArgs.HideInactive ? "" : $@",{SOut.Int((int) PatientStatus.Inactive)}")
                      + (ptSearchArgs.ShowArchived ? "" : $@",{SOut.Int((int) PatientStatus.Archived)},{SOut.Int((int) PatientStatus.Deceased)}") + ")"
                      + (ptSearchArgs.ShowMerged
                          ? ""
                          : $@"
				AND patient.PatNum NOT IN (
					SELECT DISTINCT pl1.PatNumFrom 
					FROM patientlink pl1
					LEFT JOIN patientlink pl2 ON pl2.PatNumTo=pl1.PatNumFrom
						AND	pl2.DateTimeLink > pl1.DateTimeLink
						AND pl2.LinkType={SOut.Int((int) PatientLinkType.Merge)}
					WHERE pl2.PatNumTo IS NULL
					AND pl1.LinkType={SOut.Int((int) PatientLinkType.Merge)}
				) ")
                      + (ptSearchArgs.IsFromMobile ? lNameMobileQueryString :
                          string.IsNullOrEmpty(ptSearchArgs.LName) ? "" : $@"
				AND (
					patient.LName LIKE '{(ptSearchArgs.DoLimit ? "" : "%") + ptSearchArgs.LName}%'{(!false ? "" : $@"
					OR patient.Preferred LIKE '{(ptSearchArgs.DoLimit ? "" : "%") + ptSearchArgs.LName}%'")}
				) ")
                      + (ptSearchArgs.IsFromMobile ? fNameMobileQueryString :
                          string.IsNullOrEmpty(ptSearchArgs.FName) ? "" : $@"
				AND (
					patient.FName LIKE '{ptSearchArgs.FName}%'"
                                                                          //Nathan has approved the preferred name search for first name only. It is not intended to work with last name for our customers.
                                                                          + $@"{(!false && !PrefC.GetBool(PrefName.PatientSelectUseFNameForPreferred) ? "" : $@"
					OR patient.Preferred LIKE '{ptSearchArgs.FName}%'")}
				) ")
                      + (string.IsNullOrEmpty(regexp) || usePhonenumTable
                          ? ""
                          : $@"
				AND (
					patient.HmPhone REGEXP '{regexp}'
					OR patient.WkPhone REGEXP '{regexp}'
					OR patient.WirelessPhone REGEXP '{regexp}'{(!false ? "" : $@"
					OR phonenumber.PhoneNumberVal REGEXP '{regexp}'")}
				) ")
                      + phoneNumSearch
                      + (string.IsNullOrEmpty(strAddress)
                          ? ""
                          : $@"
				AND (
					patient.Address LIKE '%{strAddress}%'
				)")
                      + (string.IsNullOrEmpty(ptSearchArgs.City)
                          ? ""
                          : $@"
				AND patient.City LIKE '{ptSearchArgs.City}%'") //LIKE is case insensitive in mysql.
                      + (string.IsNullOrEmpty(ptSearchArgs.State)
                          ? ""
                          : $@"
				AND patient.State LIKE '{ptSearchArgs.State}%'") //LIKE is case insensitive in mysql.
                      + (string.IsNullOrEmpty(ptSearchArgs.Ssn)
                          ? ""
                          : $@"
				AND patient.SSN LIKE '{ptSearchArgs.Ssn}%'") //LIKE is case insensitive in mysql.
                      + (string.IsNullOrEmpty(ptSearchArgs.ChartNumber)
                          ? ""
                          : $@"
				AND patient.ChartNumber LIKE '{ptSearchArgs.ChartNumber}%'") //LIKE is case insensitive in mysql.
                      + (string.IsNullOrEmpty(ptSearchArgs.Email)
                          ? ""
                          : $@"
				AND patient.Email LIKE '%{ptSearchArgs.Email}%'") //LIKE is case insensitive in mysql.
                      + (string.IsNullOrEmpty(ptSearchArgs.Country)
                          ? ""
                          : $@"
				AND patient.Country LIKE '%{ptSearchArgs.Country}%'") //LIKE is case insensitive in mysql.
                      + (string.IsNullOrEmpty(ptSearchArgs.RegKey)
                          ? ""
                          : $@"
				AND registrationkey.RegKey LIKE '%{ptSearchArgs.RegKey}%'") //LIKE is case insensitive in mysql.
                      + (ptSearchArgs.BillingType == 0
                          ? ""
                          : $@"
				AND patient.BillingType={(ptSearchArgs.BillingType)}")
                      + (!ptSearchArgs.GuarOnly
                          ? ""
                          : @"
				AND patient.PatNum=patient.Guarantor")
                      + (ptSearchArgs.SiteNum == 0
                          ? ""
                          : $@"
				AND patient.SiteNum={ptSearchArgs.SiteNum}")
                      + (string.IsNullOrEmpty(ptSearchArgs.SubscriberId)
                          ? ""
                          : $@"
				AND inssub.SubscriberId LIKE '{ptSearchArgs.SubscriberId}%'") //LIKE is case insensitive in mysql.
                      + (ptSearchArgs.Birthdate.Year < 1880 || ptSearchArgs.Birthdate.Year > 2100
                          ? ""
                          : $@"
				AND patient.Birthdate={SOut.Date(ptSearchArgs.Birthdate)}")
                      //Only include patients who are assigned to the clinic and also patients who are not assigned to any clinic
                      + (string.IsNullOrEmpty(ptSearchArgs.ClinicNums)
                          ? ""
                          : $@"
				AND (
					patient.ClinicNum IN (0,{ptSearchArgs.ClinicNums})
					OR EXISTS (
						SELECT 1 FROM appointment
						WHERE ClinicNum IN ({ptSearchArgs.ClinicNums})
						AND patient.PatNum=appointment.PatNum
					)
				)")
                      //jordan I don't think unassigned patients should be included.  They will usually search by name anyway
                      + (string.IsNullOrEmpty(ptSearchArgs.ClinicName)
                          ? ""
                          : $@"
				AND (
					EXISTS(
						SELECT 1 FROM clinic
						WHERE clinic.Abbr LIKE '{ptSearchArgs.ClinicName}%'
						AND patient.ClinicNum=clinic.ClinicNum
					)
				)")
                      //Do a mathematical comparison for the patNumStr.
                      + $@"
				{DbHelper.LongBetween("patient.PatNum", ptSearchArgs.PatNumStr, ptSearchArgs.IsFromMobile)}"
                      //Do a mathematical comparison for the invoiceNumber.
                      + $@"
				{DbHelper.LongBetween("statement.StatementNum", ptSearchArgs.InvoiceNumber)}"
                      //NOTE: This filter will superceed all filters set above.  Negate all filters above and select pats based solely on being in explicitPatNums
                      + (ptSearchArgs.ListExplicitPatNums.IsNullOrEmpty()
                          ? ""
                          : $@"
				AND FALSE
				OR patient.PatNum IN ({string.Join(",", ptSearchArgs.ListExplicitPatNums)})")
                      + @"GROUP BY patient.PatNum";
        if (!usePhonenumTable)
        {
            command += $@"
				ORDER BY {(ptSearchArgs.InitialPatNum == 0 || !ptSearchArgs.DoLimit ? "" : $@"patient.PatNum={ptSearchArgs.InitialPatNum} DESC,")}"
                       + $@"isExactMatch DESC,patient.LName,patient.FName";
            if (ptSearchArgs.DoLimit)
            {
                command = DbHelper.LimitOrderBy(command, 40);
                if (ptSearchArgs.PageNum > 1)
                    //This code is for the paging system in patient select. It takes the current page number minus 1 and multiplies that value by 40. For page 2 it would be (2-1) * 40 aka 40, so we would skip the first 40 patients and grab the next 40.
                    command += $@" OFFSET {(ptSearchArgs.PageNum - 1) * 40}";
            }
        }
        else if (ptSearchArgs.DoLimit)
        {
            command = DbHelper.LimitOrderBy(command, 41);
            if (ptSearchArgs.PageNum > 1) command += $@" OFFSET {(ptSearchArgs.PageNum - 1) * 41}";
        }

        //Will run on the Read-Only server. if not set up, automatically runs on the current server.
        var table = DataCore.GetTable(command);
        if (usePhonenumTable && useExactMatch && phDigitsTrimmed.Length == exactMatchPhoneDigits && table.Rows.Count == 0)
        {
            command = command.Replace(phoneNumSearch, likeQueryString);
            table = DataCore.GetTable(command);
        }

        var arrayRows = table.Select();
        if (usePhonenumTable)
        {
            if ((ptSearchArgs.InitialPatNum > 0 && ptSearchArgs.DoLimit) || table.Rows.Count < 41)
                arrayRows = table.Select().OrderByDescending(x => x["PatNum"].ToString() == ptSearchArgs.InitialPatNum.ToString())
                    .ThenByDescending(x => x["isExactMatch"].ToString() == "1")
                    .ThenBy(x => x["LName"].ToString())
                    .ThenBy(x => x["FName"].ToString()).ToArray();
            else if (ptSearchArgs.DoLimit && arrayRows.Length > 40) arrayRows = arrayRows.Take(40).ToArray();
        }

        var listPatNumStrs = table.Select().Select(x => x["PatNum"].ToString()).ToList();
        var dictNextLastApts = new Dictionary<string, Tuple<DateTime, DateTime>>();
        if (table.Rows.Count > 0)
            if ((ptSearchArgs.HasNextLastVisit && ptSearchArgs.DoLimit) || PrefC.GetBool(PrefName.OmhNy))
            {
                command = $@"SELECT PatNum,
						COALESCE(MIN(CASE WHEN AptStatus={SOut.Int((int) ApptStatus.Scheduled)} AND AptDateTime>={"NOW()"}
							THEN AptDateTime END),{SOut.DateTime(DateTime.MinValue)}) NextVisit,
						COALESCE(MAX(CASE WHEN AptStatus={SOut.Int((int) ApptStatus.Complete)} AND AptDateTime<={"NOW()"}
							THEN AptDateTime END),{SOut.DateTime(DateTime.MinValue)}) LastVisit
						FROM appointment 
						WHERE AptStatus IN({SOut.Int((int) ApptStatus.Scheduled)},{SOut.Int((int) ApptStatus.Complete)})
						AND PatNum IN ({string.Join(",", listPatNumStrs)})
						GROUP BY PatNum";
                dictNextLastApts = DataCore.GetTable(command).Select()
                    .ToDictionary(x => x["PatNum"].ToString(), x => Tuple.Create(SIn.DateTime(x["NextVisit"].ToString()), SIn.DateTime(x["LastVisit"].ToString())));
            }

        var listPatNums = new List<long>();
        if (DisplayFields.IsInUse(DisplayFieldCategory.PatientSelect, "DischargeDate"))
        {
            listPatNums = listPatNumStrs.Select(x => SIn.Long(x)).ToList();
        }

        var PtDataTable = table.Clone(); //does not copy any data
        PtDataTable.TableName = "table";
        PtDataTable.Columns.Add("age");
        PtDataTable.Columns.Add("clinic");
        PtDataTable.Columns.Add("site");
        //lastVisit and nextVisit are not part of PtDataTable and need to be added manually from the corresponding dictionary.
        PtDataTable.Columns.Add("lastVisit");
        PtDataTable.Columns.Add("nextVisit");
        //DischargeDate is not a part of the table and must be added manually by grabbing the ehrpatient data.
        PtDataTable.Columns.Add("DischargeDate");
        var listRecalls = new List<Recall>(); //only for OmhNy
        var listPatNumsWithCompletedProcs = new List<long>(); //only for OmhNy
        //RecallPastDue is not a part of the table and must be added manually by grabbing the recall data.
        var listProceduresComplete = new List<Procedure>(); //only for OmhNy
        long codeNumAdmissionExam = -1; //only for OmhNy
        if (PrefC.GetBool(PrefName.OmhNy))
        {
            codeNumAdmissionExam = ProcedureCodes.GetProcCode("D0122").CodeNum; //only for OmhNy
            if (listPatNums.Count == 0) listPatNums = listPatNumStrs.Select(x => SIn.Long(x)).ToList();
            listRecalls = Recalls.GetList(listPatNums);
            listProceduresComplete = Procedures.GetCompleteForProcCodeNum(listPatNums, codeNumAdmissionExam);
            PtDataTable.Columns.Add("RecallPastDue");
        }

        PtDataTable.Columns.OfType<DataColumn>().ForEach(x => x.DataType = typeof(string));
        DataRow r;
        DateTime date;
        foreach (var dRow in arrayRows)
        {
            r = PtDataTable.NewRow();
            r["PatNum"] = dRow["PatNum"].ToString();
            r["LName"] = dRow["LName"].ToString();
            r["FName"] = dRow["FName"].ToString();
            r["MiddleI"] = dRow["MiddleI"].ToString();
            r["Preferred"] = dRow["Preferred"].ToString();
            date = SIn.Date(dRow["Birthdate"].ToString());
            if (date.Year > 1880)
            {
                r["age"] = DateToAge(date);
                r["Birthdate"] = date.ToShortDateString();
            }
            else
            {
                r["age"] = "";
                r["Birthdate"] = "";
            }

            r["SSN"] = dRow["SSN"].ToString();
            r["HmPhone"] = dRow["HmPhone"].ToString();
            r["WkPhone"] = dRow["WkPhone"].ToString();
            r["Address"] = dRow["Address"].ToString();
            r["Address2"] = dRow["Address2"].ToString();
            r["PatStatus"] = ((PatientStatus) SIn.Int(dRow["PatStatus"].ToString())).ToString();
            r["BillingType"] = Defs.GetName(DefCat.BillingTypes, SIn.Long(dRow["BillingType"].ToString()));
            r["ChartNumber"] = dRow["ChartNumber"].ToString();
            r["City"] = dRow["City"].ToString();
            r["State"] = dRow["State"].ToString();
            r["Zip"] = dRow["Zip"].ToString();
            r["PriProv"] = Providers.GetAbbr(SIn.Long(dRow["PriProv"].ToString()));
            r["site"] = Sites.GetDescription(SIn.Long(dRow["SiteNum"].ToString()));
            r["Email"] = dRow["Email"].ToString();
            r["Country"] = dRow["Country"].ToString();
            var clinicNum = SIn.Long(dRow["ClinicNum"].ToString());
            r["ClinicNum"] = clinicNum;
            r["clinic"] = Clinics.GetAbbr(clinicNum);
            if (false)
            {
                //if for OD HQ
                r["OtherPhone"] = dRow["OtherPhone"].ToString();
                r["RegKey"] = dRow["RegKey"].ToString();
            }

            r["StatementNum"] = dRow["StatementNum"].ToString();
            r["WirelessPhone"] = dRow["WirelessPhone"].ToString();
            r["SecProv"] = Providers.GetAbbr(SIn.Long(dRow["SecProv"].ToString()));
            r["nextVisit"] = "";
            r["lastVisit"] = "";
            if (dictNextLastApts.TryGetValue(dRow["PatNum"].ToString(), out var tupleNextLastVisitDates))
            {
                date = tupleNextLastVisitDates.Item1;
                if (date.Year > 1880) //if the date is valid
                    r["nextVisit"] = date.ToShortDateString();
                date = tupleNextLastVisitDates.Item2;
                if (date.Year > 1880) //if the date is valid
                    r["lastVisit"] = date.ToShortDateString();
            }

            r["isExactMatch"] = dRow["isExactMatch"].ToString();
            r["Specialty"] = dRow["Specialty"].ToString();
            r["TxtMsgOk"] = dRow["TxtMsgOk"].ToString();
            r["DateFirstVisit"] = dRow["DateFirstVisit"].ToString();
            r["CreditType"] = dRow["CreditType"].ToString();
            r["MedurgNote"] = dRow["MedUrgNote"].ToString();
            r["Ward"] = dRow["Ward"].ToString();
            var dateAdmit = SIn.Date(dRow["AdmitDate"].ToString());
            var dateDischarge = DateTime.MinValue;
            if (dateAdmit.Year > 1880) r["AdmitDate"] = dateAdmit.ToShortDateString();
            var patNum = SIn.Long(r["PatNum"].ToString());
            #region New York Mental Health

            if (PrefC.GetBool(PrefName.OmhNy))
            {
                var listRecallsPat = listRecalls
                    .FindAll(x => x.PatNum == patNum)
                    .OrderBy(x => x.RecallTypeNum).ToList(); //at their request, not sure why
                var description = "ORANGE";
                var hasCompletedAdmissionProc = listProceduresComplete.Exists(x => x.PatNum == patNum && dateDischarge <= dateAdmit && x.DateComplete >= dateAdmit && x.CodeNum == codeNumAdmissionExam); //Currently admitted and has an admission exam completed after being admitted
                if (hasCompletedAdmissionProc || dateDischarge > dateAdmit) //Has completed procedure or is currently Discharged
                    description = "BLACK";
                //Recall that has a due date within 14 days or less... or recall is overdue
                var recallPastDue = listRecallsPat.Find(x => x.DateDue.Year > 1880
                                                             && x.DateDue.AddDays(-14) <= DateTime.Today
                                                             && RecallTypes.GetDescription(x.RecallTypeNum).In("PROPHY", "CHILD PROPHY", "ANNUAL EXAM", "6 MONTH EXAM", "PANO X-RAY", "PERIO SRP(UR)", "PERIO SRP(UL)", "PERIO SRP(LR)", "PERIO SRP(LL)"));
                if (recallPastDue != null && hasCompletedAdmissionProc) description = RecallTypes.GetDescription(recallPastDue.RecallTypeNum);
                r["RecallPastDue"] = description;
            }

            #endregion New York Mental Health

            PtDataTable.Rows.Add(r);
        }

        return PtDataTable;
    }

    private static string GetExactMatchSnippet(PtTableSearchParams args)
    {
        var listClauses = new List<string>();
        listClauses.Add(string.IsNullOrEmpty(args.LName) ? "" : "(patient.LName='" + args.LName + "')");
        listClauses.Add(string.IsNullOrEmpty(args.FName) ? "" : "(patient.FName='" + args.FName + "')");
        listClauses.Add(string.IsNullOrEmpty(args.Phone)
            ? ""
            : "(patient.WirelessPhone='" + args.Phone + "' OR patient.HmPhone='" + args.Phone + "' OR patient.WkPhone='" + args.Phone + "'"
              + (!false ? "" : " OR phonenumber.PhoneNumberVal='" + args.Phone + "'") //Join
              + ")");
        listClauses.Add(string.IsNullOrEmpty(args.Address) ? "" : "(patient.Address='" + args.Address + "')");
        listClauses.Add(string.IsNullOrEmpty(args.City) ? "" : "(patient.City='" + args.City + "')");
        listClauses.Add(string.IsNullOrEmpty(args.State) ? "" : "(patient.State='" + args.State + "')");
        listClauses.Add(string.IsNullOrEmpty(args.Ssn) ? "" : "(patient.Ssn='" + args.Ssn + "')");
        listClauses.Add(string.IsNullOrEmpty(args.PatNumStr) ? "" : "(patient.PatNum='" + args.PatNumStr + "')");
        listClauses.Add(string.IsNullOrEmpty(args.ChartNumber) ? "" : "(patient.ChartNumber='" + args.ChartNumber + "')");
        listClauses.Add(string.IsNullOrEmpty(args.SubscriberId) ? "" : "(inssub.SubscriberId='" + args.SubscriberId + "')"); //Join
        listClauses.Add(string.IsNullOrEmpty(args.Email) ? "" : "(patient.Email='" + args.Email + "')");
        listClauses.Add(string.IsNullOrEmpty(args.Country) ? "" : "(patient.Country='" + args.Country + "')");
        listClauses.Add(string.IsNullOrEmpty(args.RegKey) || !false ? "" : "(registrationkey.RegKey='" + args.RegKey + "')"); //Join
        listClauses.Add(args.Birthdate.Year < 1880 ? "" : "(patient.Birthdate=" + SOut.Date(args.Birthdate) + ")");
        listClauses.Add(string.IsNullOrEmpty(args.InvoiceNumber) ? "" : "(statement.StatementNum='" + args.InvoiceNumber + "')");
        listClauses.RemoveAll(string.IsNullOrEmpty);
        if (listClauses.Count > 0) return "(" + string.Join(" AND ", listClauses) + ")";
        return "'0'";
    }

    public static Patient[] GetMultPats(List<long> patNums)
    {
        var table = new DataTable();
        if (patNums.Count > 0)
        {
            var command = "SELECT * FROM patient WHERE PatNum IN (" + string.Join(",", patNums) + ") ";
            table = DataCore.GetTable(command);
        }

        var multPats = PatientCrud.TableToList(table).ToArray();
        return multPats;
    }

    public static Patient GetOnePat(Patient[] multPats, long patNum)
    {
        for (var i = 0; i < multPats.Length; i++)
            if (multPats[i].PatNum == patNum)
                return multPats[i];

        return new Patient();
    }

    public static Patient GetLim(long patNum)
    {
        if (patNum == 0) return new Patient();
        var command =
            "SELECT PatNum,LName,FName,MiddleI,Preferred,CreditType,Guarantor,HasIns,SSN,Birthdate,ClinicNum "
            + "FROM patient "
            + "WHERE PatNum = '" + patNum + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return new Patient();
        var Lim = new Patient();
        Lim.PatNum = SIn.Long(table.Rows[0][0].ToString());
        Lim.LName = SIn.String(table.Rows[0][1].ToString());
        Lim.FName = SIn.String(table.Rows[0][2].ToString());
        Lim.MiddleI = SIn.String(table.Rows[0][3].ToString());
        Lim.Preferred = SIn.String(table.Rows[0][4].ToString());
        Lim.CreditType = SIn.String(table.Rows[0][5].ToString());
        Lim.Guarantor = SIn.Long(table.Rows[0][6].ToString());
        Lim.HasIns = SIn.String(table.Rows[0][7].ToString());
        Lim.SSN = SIn.String(table.Rows[0][8].ToString());
        Lim.Birthdate = SIn.DateTime(table.Rows[0][9].ToString());
        Lim.ClinicNum = SIn.Long(table.Rows[0][10].ToString());
        return Lim;
    }

    public static List<PatientFor834Import> GetAllPatsFor834Imports()
    {
        var retVal = new List<PatientFor834Import>();
        var command = "SELECT PatNum,LName,FName,BirthDate,PatStatus FROM patient";
        var table = DataCore.GetTable(command);
        foreach (DataRow row in table.Rows)
        {
            var patLim = new PatientFor834Import();
            patLim.PatNum = SIn.Long(row["PatNum"].ToString());
            patLim.LName = SIn.String(row["LName"].ToString());
            patLim.FName = SIn.String(row["FName"].ToString());
            patLim.Birthdate = SIn.Date(row["Birthdate"].ToString());
            patLim.PatStatus = SIn.Enum<PatientStatus>(row["PatStatus"].ToString());
            retVal.Add(patLim);
        }

        return retVal;
    }

    public static List<Patient> GetLimForPats(List<long> listPatNums, bool doIncludeClinicNum = false)
    {
        if (listPatNums == null || listPatNums.Count < 1) return [];

        var retVal = new List<Patient>();
        var command = "SELECT PatNum,LName,FName,MiddleI,Preferred,CreditType,Guarantor,HasIns,SSN" + (doIncludeClinicNum ? ",ClinicNum" : "") + " "
                      + "FROM patient "
                      + "WHERE PatNum IN (" + string.Join(",", listPatNums) + ")";
        var table = DataCore.GetTable(command);
        foreach (DataRow row in table.Rows)
        {
            var patLim = new Patient();
            patLim.PatNum = SIn.Long(row["PatNum"].ToString());
            patLim.LName = SIn.String(row["LName"].ToString());
            patLim.FName = SIn.String(row["FName"].ToString());
            patLim.MiddleI = SIn.String(row["MiddleI"].ToString());
            patLim.Preferred = SIn.String(row["Preferred"].ToString());
            patLim.CreditType = SIn.String(row["CreditType"].ToString());
            patLim.Guarantor = SIn.Long(row["Guarantor"].ToString());
            patLim.HasIns = SIn.String(row["HasIns"].ToString());
            patLim.SSN = SIn.String(row["SSN"].ToString());
            if (doIncludeClinicNum) patLim.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            retVal.Add(patLim);
        }

        return retVal;
    }

    public static DataTable GetPaymentStartingBalances(long guarNum, long excludePayNum)
    {
        return GetPaymentStartingBalances(guarNum, excludePayNum, false);
    }

    public static DataTable GetPaymentStartingBalances(long guarNum, long excludePayNum, bool groupByProv)
    {
        //This method no longer uses a temporary table due to the problems it was causing replication users.
        //The in-memory table name was left as "tempfambal" for nostalgic purposes because veteran engineers know exactly where to look when "tempfambal" is mentioned.
        //This query will be using UNION ALLs so that duplicate-row removal does not occur. 
        var command = @"
					SELECT tempfambal.PatNum,tempfambal.ProvNum,
						tempfambal.ClinicNum,ROUND(SUM(tempfambal.AmtBal),3) StartBal,
						ROUND(SUM(tempfambal.AmtBal-tempfambal.InsEst),3) AfterIns,patient.FName,patient.Preferred,0.0 EndBal,
						CASE WHEN patient.Guarantor!=patient.PatNum THEN 1 ELSE 0 END IsNotGuar,patient.Birthdate,tempfambal.UnearnedType
					FROM(
						/*Completed procedures*/
						(SELECT patient.PatNum,procedurelog.ProvNum,procedurelog.ClinicNum,
						SUM(procedurelog.ProcFee*(procedurelog.UnitQty+procedurelog.BaseUnits)) AmtBal,0 InsEst,0 UnearnedType
						FROM procedurelog,patient
						WHERE patient.PatNum=procedurelog.PatNum
						AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + @"
						AND patient.Guarantor=" + (guarNum) + @"
						GROUP BY patient.PatNum,procedurelog.ProvNum,procedurelog.ClinicNum)
					UNION ALL			
						/*Received insurance payments*/
						(SELECT patient.PatNum,claimproc.ProvNum,claimproc.ClinicNum,-SUM(claimproc.InsPayAmt)-SUM(claimproc.Writeoff) AmtBal,0 InsEst,0 UnearnedType
						FROM claimproc,patient
						WHERE patient.PatNum=claimproc.PatNum
						AND (claimproc.Status=" + SOut.Int((int) ClaimProcStatus.Received) + @" 
							OR claimproc.Status=" + SOut.Int((int) ClaimProcStatus.Supplemental) + @" 
							OR claimproc.Status=" + SOut.Int((int) ClaimProcStatus.CapClaim) + @" 
							OR claimproc.Status=" + SOut.Int((int) ClaimProcStatus.CapComplete) + @")
						AND patient.Guarantor=" + (guarNum) + @"
						AND claimproc.PayPlanNum = 0
						GROUP BY patient.PatNum,claimproc.ProvNum,claimproc.ClinicNum)
					UNION ALL
						/*Insurance estimates*/
						(SELECT patient.PatNum,claimproc.ProvNum,claimproc.ClinicNum,0 AmtBal,SUM(claimproc.InsPayEst)+SUM(claimproc.Writeoff) InsEst,0 UnearnedType
						FROM claimproc,patient
						WHERE patient.PatNum=claimproc.PatNum
						AND claimproc.Status=" + SOut.Int((int) ClaimProcStatus.NotReceived) + @"
						AND patient.Guarantor=" + (guarNum) + @"
						GROUP BY patient.PatNum,claimproc.ProvNum,claimproc.ClinicNum)
					UNION ALL
						/*Adjustments*/
						(SELECT patient.PatNum,adjustment.ProvNum,adjustment.ClinicNum,SUM(adjustment.AdjAmt) AmtBal,0 InsEst,0 UnearnedType
						FROM adjustment,patient
						WHERE patient.PatNum=adjustment.PatNum
						AND patient.Guarantor=" + (guarNum) + @"
						GROUP BY patient.PatNum,adjustment.ProvNum,adjustment.ClinicNum)
					UNION ALL
						/*Patient payments*/
						(SELECT patient.PatNum,paysplit.ProvNum,paysplit.ClinicNum,-SUM(SplitAmt) AmtBal,0 InsEst,paysplit.UnearnedType
						FROM paysplit,patient
						WHERE patient.PatNum=paysplit.PatNum
						AND paysplit.PayNum!=" + (excludePayNum) + @"
						AND patient.Guarantor=" + (guarNum);
        if (PrefC.GetInt(PrefName.PayPlansVersion) == 1) //for payplans v1, exclude paysplits attached to payplans
            command += @"
						AND paysplit.PayPlanNum=0 ";
        command += @"
						GROUP BY patient.PatNum,paysplit.ProvNum,paysplit.ClinicNum)
					UNION ALL	
						(SELECT patient.PatNum,payplancharge.ProvNum,payplancharge.ClinicNum,-payplan.CompletedAmt ";
        if (PrefC.GetInt(PrefName.PayPlansVersion) == 2)
            command += "+ SUM(CASE WHEN payplancharge.ChargeType=" + SOut.Int((int) PayPlanChargeType.Debit) + @"
						AND payplancharge.ChargeDate <= CURDATE() THEN payplancharge.Principal + payplancharge.Interest ELSE 0 END) ";
        command += @"AmtBal,0 InsEst,0 UnearnedType
						FROM payplancharge
						INNER JOIN payplan ON payplan.PayPlanNum=payplancharge.PayPlanNum
						INNER JOIN patient ON patient.PatNum=payplancharge.PatNum AND patient.Guarantor=" + (guarNum) + @"
						GROUP BY payplan.PayPlanNum,payplan.CompletedAmt,patient.PatNum,payplancharge.ProvNum,payplancharge.ClinicNum)
					) tempfambal,patient
					WHERE tempfambal.PatNum=patient.PatNum 
					GROUP BY tempfambal.PatNum,tempfambal.ProvNum,";
        if (!groupByProv) command += @"tempfambal.ClinicNum,";
        command += @"patient.FName,patient.Preferred";
        //Probably an unnecessary MySQL / Oracle split but I didn't want to affect the old GROUP BY functionality for MySQL just be Oracle is lame.
        command += @"
					HAVING ((StartBal>0.005 OR StartBal<-0.005) OR (AfterIns>0.005 OR AfterIns<-0.005))
					ORDER BY IsNotGuar,Birthdate,ProvNum,FName,Preferred";
        return DataCore.GetTable(command);
    }

    public static void ChangeGuarantorToCur(Family Fam, Patient patientOld)
    {
        //Move famfinurgnote to current patient:
        var patient = patientOld.Copy();
        patient.FamFinUrgNote = Fam.ListPats[0].FamFinUrgNote;
        Update(patient, patientOld);
        //Clear FamFinUrgNote from old guarantor:
        var patientGuarantor = Fam.ListPats[0];
        var patientGuarantorOld = patientGuarantor.Copy();
        patientGuarantor.FamFinUrgNote = "";
        Update(patientGuarantor, patientGuarantorOld);
        //Move family financial note to current patient:
        var command = "SELECT FamFinancial FROM patientnote "
                      + "WHERE PatNum = " + (patient.Guarantor);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 1)
        {
            command = "UPDATE patientnote SET "
                      + "FamFinancial = '" + SOut.String(table.Rows[0][0].ToString()) + "' "
                      + "WHERE PatNum = " + (patient.PatNum);
            Db.NonQ(command);
        }

        command = "UPDATE patientnote SET FamFinancial = '' "
                  + "WHERE PatNum = " + (patient.Guarantor);
        Db.NonQ(command);
        //change guarantor of all family members:
        var listPatients = GetAllPatientsForGuarantor(patientOld.Guarantor);
        for (var i = 0; i < listPatients.Count; i++)
        {
            patient = listPatients[i].Copy();
            listPatients[i].Guarantor = patientOld.PatNum;
            Update(listPatients[i], patient); //Guarantor is used in SecurityHash, Update will handle rehashing
        }
    }

    public static void CombineGuarantors(Family Fam, Patient Pat)
    {
        var command = "";
        //concat cur notes with guarantor notes. Requires rehashing.
        var listPatients = GetAllPatientsForGuarantor(Pat.Guarantor);
        for (var i = 0; i < listPatients.Count; i++)
        {
            var patientNew = listPatients[i].Copy();
            patientNew.FamFinUrgNote = Fam.ListPats[0].FamFinUrgNote + Pat.FamFinUrgNote;
            Update(patientNew, listPatients[i]);
        }

        //delete cur notes. Requires rehashing.
        var patientOld = Pat.Copy();
        Pat.FamFinUrgNote = "";
        Update(Pat, patientOld);
        //concat family financial notes
        var PatientNoteCur = PatientNotes.Refresh(Pat.PatNum, Pat.Guarantor);
        //patientnote table must have been refreshed for this to work.
        //Makes sure there are entries for patient and for guarantor.
        //Also, PatientNotes.cur.FamFinancial will now have the guar info in it.
        var strGuar = PatientNoteCur.FamFinancial;
        command =
            "SELECT famfinancial "
            + "FROM patientnote WHERE patnum ='" + (Pat.PatNum) + "'";
        //MessageBox.Show(string command);
        var table = DataCore.GetTable(command);
        var strCur = SIn.String(table.Rows[0][0].ToString());
        command =
            "UPDATE patientnote SET "
            + "famfinancial = '" + SOut.String(strGuar + strCur) + "' "
            + "WHERE patnum = '" + Pat.Guarantor + "'";
        Db.NonQ(command);
        //delete cur financial notes
        command =
            "UPDATE patientnote SET "
            + "famfinancial = ''"
            + "WHERE patnum = '" + Pat.PatNum + "'";
        Db.NonQ(command);
    }
    
    public static Dictionary<long, string> GetPatientNames(List<long> listPatNums)
    {
        return GetLimForPats(listPatNums)
            .ToDictionary(x => x.PatNum, x => x.GetNameLF());
    }

    public static List<PatientName> GetPatientNameList(List<long> listPatNums)
    {
        var listPatients = GetLimForPats(listPatNums);
        var listPatientNames = new List<PatientName>();
        for (var i = 0; i < listPatients.Count; ++i)
        {
            var patientName = new PatientName();
            patientName.PatNum = listPatients[i].PatNum;
            patientName.Name = listPatients[i].GetNameLF();
            listPatientNames.Add(patientName);
        }

        return listPatientNames;
    }

    public static Dictionary<long, string> GetDictAllPatientNames()
    {
        var table = GetAllPatientNamesTable();
        var dict = new Dictionary<long, string>();
        long patnum;
        string lname, fname, middlei, preferred;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            patnum = SIn.Long(table.Rows[i][0].ToString());
            lname = SIn.String(table.Rows[i][1].ToString());
            fname = SIn.String(table.Rows[i][2].ToString());
            middlei = SIn.String(table.Rows[i][3].ToString());
            preferred = SIn.String(table.Rows[i][4].ToString());
            dict.Add(patnum, GetNameLF(lname, fname, preferred, middlei));
        }

        return dict;
    }

    public static DataTable GetAllPatientNamesTable()
    {
        var command = "SELECT patnum,lname,fname,middlei,preferred "
                      + "FROM patient";
        var table = DataCore.GetTable(command);
        return table;
    }

    public static bool SuperFamHasSameAddrPhone(Patient pat, bool isArchivedIncluded)
    {
        var command = "SELECT COUNT(*) FROM patient WHERE SuperFamily=" + (pat.SuperFamily) + " "
                      + "AND (HmPhone!='" + SOut.String(pat.HmPhone) + "' "
                      + "OR Address!='" + SOut.String(pat.Address) + "' "
                      + "OR Address2!='" + SOut.String(pat.Address2) + "' "
                      + "OR City!='" + SOut.String(pat.City) + "' "
                      + "OR State!='" + SOut.String(pat.State) + "' "
                      + "OR Country!='" + SOut.String(pat.Country) + "' "
                      + "OR Zip!='" + SOut.String(pat.Zip) + "') ";
        if (!isArchivedIncluded) command += "AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Archived);
        if (SIn.Int(Db.GetCount(command)) == 0) //Everybody in the superfamily has the same information
            return true;
        return false; //At least one patient in the superfamily has different information
    }

    public static void UpdateAddressForFam(Patient pat, bool isSuperFam, bool isAuthArchivedEdit)
    {
        var strWhere = "";
        if (isSuperFam)
            strWhere += " WHERE SuperFamily = " + (pat.SuperFamily);
        else
            strWhere += " WHERE Guarantor = " + (pat.Guarantor);
        if (!isAuthArchivedEdit) strWhere += " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Archived);
        //Get the list of patients before the changes.
        var strSelect = "SELECT * FROM patient " + strWhere;
        var listPatsOld = PatientCrud.SelectMany(strSelect);
        var command = "UPDATE patient SET "
                      + "Address = '" + SOut.String(pat.Address) + "'"
                      + ",Address2 = '" + SOut.String(pat.Address2) + "'"
                      + ",City = '" + SOut.String(pat.City) + "'"
                      + ",State = '" + SOut.String(pat.State) + "'"
                      + ",Country = '" + SOut.String(pat.Country) + "'"
                      + ",Zip = '" + SOut.String(pat.Zip) + "'"
                      + ",HmPhone = '" + SOut.String(pat.HmPhone) + "'"
                      + strWhere;
        Db.NonQ(command);
        //Get the list of patients after the changes
        var listPatsNew = PatientCrud.SelectMany(strSelect);
        //Add securitylog entries for any changes made.
        var didPhoneChange = false;
        foreach (var patOld in listPatsOld)
        {
            var patNew = listPatsNew.FirstOrDefault(x => x.PatNum == patOld.PatNum);
            if (patNew == null) continue; //This shouldn't happen.
            didPhoneChange |= patOld.HmPhone != patNew.HmPhone;
            InsertAddressChangeSecurityLogEntry(patOld, patNew);
        }

        if (didPhoneChange && PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable)) PhoneNumbers.SyncPats(listPatsNew);
    }

    public static void InsertAddressChangeSecurityLogEntry(Patient patOld, Patient patCur)
    {
        var secLogText = new StringBuilder();
        secLogText.Append(SecurityLogEntryHelper(patOld.PatStatus.GetDescription(), patCur.PatStatus.GetDescription(), "status"));
        secLogText.Append(SecurityLogEntryHelper(patOld.LName, patCur.LName, "last name"));
        secLogText.Append(SecurityLogEntryHelper(patOld.FName, patCur.FName, "first name"));
        secLogText.Append(SecurityLogEntryHelper(patOld.WkPhone, patCur.WkPhone, "work phone"));
        secLogText.Append(SecurityLogEntryHelper(patOld.WirelessPhone, patCur.WirelessPhone, "wireless phone"));
        secLogText.Append(SecurityLogEntryHelper(patOld.HmPhone, patCur.HmPhone, "home phone"));
        secLogText.Append(SecurityLogEntryHelper(patOld.Address, patCur.Address, "address"));
        secLogText.Append(SecurityLogEntryHelper(patOld.Address2, patCur.Address2, "address 2"));
        secLogText.Append(SecurityLogEntryHelper(patOld.City, patCur.City, "city"));
        secLogText.Append(SecurityLogEntryHelper(patOld.State, patCur.State, "state"));
        secLogText.Append(SecurityLogEntryHelper(patOld.Country, patCur.Country, "country"));
        secLogText.Append(SecurityLogEntryHelper(patOld.Zip, patCur.Zip, "zip code"));
        secLogText.Append(SecurityLogEntryHelper(patOld.TxtMsgOk.ToString(), patCur.TxtMsgOk.ToString(), "Text OK"));
        secLogText.Append(SecurityLogEntryHelper(patOld.Email, patCur.Email, "Email"));
        if (secLogText.ToString() != "") SecurityLogs.MakeLogEntry(EnumPermType.PatientEdit, patCur.PatNum, secLogText.ToString());
    }

    public static void InsertBillTypeChangeSecurityLogEntry(Patient patOld, Patient patCur)
    {
        var strLog = SecurityLogEntryHelper(Defs.GetName(DefCat.BillingTypes, patOld.BillingType), Defs.GetName(DefCat.BillingTypes, patCur.BillingType),
            "billing type");
        if (string.IsNullOrEmpty(strLog)) return;
        SecurityLogs.MakeLogEntry(EnumPermType.PatientBillingEdit, patCur.PatNum, strLog);
    }

    public static void InsertPrimaryProviderChangeSecurityLogEntry(Patient patOld, Patient patCur)
    {
        var strLog = SecurityLogEntryHelper(patOld.PriProv == 0 ? "'blank'" : Providers.GetLongDesc(patOld.PriProv),
            patCur.PriProv == 0 ? "'blank'" : Providers.GetLongDesc(patCur.PriProv),
            "Primary Provider");
        if (string.IsNullOrEmpty(strLog)) return;
        SecurityLogs.MakeLogEntry(EnumPermType.PatPriProvEdit, patCur.PatNum, strLog);
    }

    private static string SecurityLogEntryHelper(string oldVal, string newVal, string textInLog)
    {
        if (oldVal != newVal) return "Patient " + textInLog + " changed from '" + oldVal + "' to '" + newVal + "'\r\n";
        return "";
    }

    public static void UpdateBillingProviderForFam(Patient pat, bool isAuthPriProvEdit, bool isAuthArchivedEdit)
    {
        var strWhere = " WHERE Guarantor = " + (pat.Guarantor);
        //Get the list of patients before the changes.
        var strSelect = "SELECT * FROM patient " + strWhere;
        var listPatsOld = PatientCrud.SelectMany(strSelect);
        var command = "UPDATE patient SET "
                      + "credittype      = '" + SOut.String(pat.CreditType) + "',";
        if (isAuthPriProvEdit) command += "priprov = " + (pat.PriProv) + ",";
        command +=
            "secprov         = " + (pat.SecProv) + ","
            + "feesched        = " + (pat.FeeSched) + ","
            + "billingtype     = " + (pat.BillingType) + " "
            + strWhere;
        if (!isAuthArchivedEdit) command += " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Archived);
        Db.NonQ(command);
        //Get the list of patients after the changes
        var listPatsNew = PatientCrud.SelectMany(strSelect);
        foreach (var patOld in listPatsOld)
        {
            var patNew = listPatsNew.FirstOrDefault(x => x.PatNum == patOld.PatNum);
            if (patNew == null) continue; //This shouldn't happen.
            InsertBillTypeChangeSecurityLogEntry(patOld, patNew);
            InsertPrimaryProviderChangeSecurityLogEntry(patOld, patNew);
        }
    }

    public static void UpdateArriveEarlyForFam(Patient pat, bool isAuthArchivedEdit)
    {
        var command = "UPDATE patient SET "
                      + "AskToArriveEarly = '" + SOut.Int(pat.AskToArriveEarly) + "'"
                      + " WHERE guarantor = '" + (pat.Guarantor) + "'";
        if (!isAuthArchivedEdit) command += " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Archived);
        var table = DataCore.GetTable(command);
    }

    public static void UpdateNotesForFam(Patient pat, bool isAuthArchivedEdit)
    {
        var command = "UPDATE patient SET "
                      + "addrnote = '" + SOut.String(pat.AddrNote) + "'"
                      + " WHERE guarantor = '" + (pat.Guarantor) + "'";
        if (!isAuthArchivedEdit) command += " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Archived);
        Db.NonQ(command);
    }

    public static void UpdateEmailPhoneForFam(Patient pat, bool isAuthArchivedEdit)
    {
        var strWhere = " WHERE Guarantor = " + (pat.Guarantor);
        //Get the list of patients before the changes.
        var strSelect = "SELECT * FROM patient " + strWhere;
        var listPatsOld = PatientCrud.SelectMany(strSelect);
        var command = "UPDATE patient SET "
                      + "Email='" + SOut.String(pat.Email) + "'"
                      + ",WirelessPhone='" + SOut.String(pat.WirelessPhone) + "'"
                      + ",WkPhone='" + SOut.String(pat.WkPhone) + "'"
                      + ",TxtMsgOk='" + SOut.Int((int) pat.TxtMsgOk) + "'"
                      + strWhere;
        if (!isAuthArchivedEdit) command += " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Archived);
        Db.NonQ(command);
        if (PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable)) PhoneNumbers.SyncPats(GetFamily(pat.PatNum).ListPats.ToList());
        //Get the list of patients after the changes
        var listPatsNew = PatientCrud.SelectMany(strSelect);
        foreach (var patOld in listPatsOld)
        {
            var patNew = listPatsNew.FirstOrDefault(x => x.PatNum == patOld.PatNum);
            if (patNew == null) continue; //This shouldn't happen.
            InsertAddressChangeSecurityLogEntry(patOld, patNew);
        }
    }

    public static void UpdateRecalls(Patient patNew, Patient patOld, string sender)
    {
        //if patient is inactive, deceased, etc., then disable any recalls
        if (patNew.PatStatus == PatientStatus.Archived
            || patNew.PatStatus == PatientStatus.Deceased
            || patNew.PatStatus == PatientStatus.Inactive
            || patNew.PatStatus == PatientStatus.NonPatient
            || patNew.PatStatus == PatientStatus.Prospective)
        {
            var recalls = Recalls.GetList(patNew.PatNum);
            for (var i = 0; i < recalls.Count; i++)
                if (!recalls[i].IsDisabled || recalls[i].DateDue.Year > 1880)
                {
                    recalls[i].IsDisabled = true;
                    recalls[i].DateDue = DateTime.MinValue;
                    Recalls.Update(recalls[i]);
                    SecurityLogs.MakeLogEntry(EnumPermType.RecallEdit, recalls[i].PatNum, "Recall disabled from the " + sender + ".");
                }
        }
        //if patient was re-activated, then re-enable any recalls
        else if (patNew.PatStatus != patOld.PatStatus && patNew.PatStatus == PatientStatus.Patient)
        {
            //if changed patstatus, and new status is Patient
            var recalls = Recalls.GetList(patNew.PatNum);
            if (recalls.Count == 0) return; //This patient does not have any recalls to 're-activate'.
            for (var i = 0; i < recalls.Count; i++)
                if (recalls[i].IsDisabled)
                {
                    recalls[i].IsDisabled = false;
                    Recalls.Update(recalls[i]);
                    SecurityLogs.MakeLogEntry(EnumPermType.RecallEdit, recalls[i].PatNum, "Recall re-enabled from the " + sender + ".");
                }

            Recalls.Synch(patNew.PatNum);
        }
    }

    public static List<PatAging> GetAgingList(string age, DateTime lastStatement, List<long> billingNums, bool excludeAddr, bool excludeNeg, double excludeLessThan, bool excludeInactive, bool ignoreInPerson, List<long> clinicNums, bool isSuperStatements, bool isSinglePatient, List<long> listPendingInsPatNums, List<long> listUnsentPatNums, Dictionary<long, List<PatAgingTransaction>> dictPatAgingTransactions, bool excludeNoTil = false, bool excludeNotBilledSince = false, bool isFinanceBilling = false, List<long> listPatNumsToExclude = null)
    {
        var listPatStatusExclude = new List<int>();
        listPatStatusExclude.Add((int) PatientStatus.Deleted); //Always hide deleted.
        if (excludeInactive) listPatStatusExclude.Add((int) PatientStatus.Inactive);
        var guarOrPat = "";
        if (isSinglePatient)
            guarOrPat = "pat";
        else
            guarOrPat = "guar";
        var listWhereAnds = new List<string>();
        var strMinusIns = "";
        if (!PrefC.GetBool(PrefName.BalancesDontSubtractIns)) strMinusIns = "-guar.InsEst";
        var strBalExclude = "(ROUND(guar.BalTotal" + strMinusIns + ",3) >= ROUND(" + SOut.Double(excludeLessThan) + ",3) OR (guar.PayPlanDue > 0 AND guar.PayPlanDue >= ROUND(" + SOut.Double(excludeLessThan) + ",3))";
        if (!excludeNeg) //include credits
            strBalExclude += " OR ROUND(guar.BalTotal" + strMinusIns + ",3) < 0";
        strBalExclude += ")";
        listWhereAnds.Add(strBalExclude);
        if (!isFinanceBilling)
            switch (age)
            {
                //age 0 means include all
                case "30":
                    listWhereAnds.Add("(guar.Bal_31_60>0 OR guar.Bal_61_90>0 OR guar.BalOver90>0 OR guar.PayPlanDue>0)");
                    break;
                case "60":
                    listWhereAnds.Add("(guar.Bal_61_90>0 OR guar.BalOver90>0 OR guar.PayPlanDue>0)");
                    break;
                case "90":
                    listWhereAnds.Add("(guar.BalOver90>0 OR guar.PayPlanDue>0)");
                    break;
            }
        else
            listWhereAnds.Add("(guar.Bal_0_30 + guar.Bal_31_60 + guar.Bal_61_90 + guar.BalOver90 - guar.InsEst > '0.005')");

        if (billingNums.Count > 0) //if billingNums.Count==0, then we'll include all billing types
            listWhereAnds.Add("guar.BillingType IN (" + string.Join(",", billingNums.Select(x => (x))) + ")");
        if (excludeAddr) listWhereAnds.Add("guar.Zip!=''");
        if (excludeNoTil) listWhereAnds.Add("guar.HasSignedTil");
        if (clinicNums.Count > 0) listWhereAnds.Add("guar.ClinicNum IN (" + string.Join(",", clinicNums.Select(x => (x))) + ")");
        listWhereAnds.Add("(guar.PatStatus!=" + SOut.Int((int) PatientStatus.Archived) + " OR ROUND(guar.BalTotal,3) != 0)"); //Hide archived patients with PatBal=0.
        if (!listPatNumsToExclude.IsNullOrEmpty()) listWhereAnds.Add("pat.PatNum NOT IN (" + string.Join(",", listPatNumsToExclude) + ")");
        var command = "";
        command = "SELECT " + guarOrPat + ".PatNum," + guarOrPat + ".FName," + guarOrPat + ".MiddleI," + guarOrPat + ".Preferred," + guarOrPat + ".LName," + guarOrPat + ".ClinicNum,guar.SuperFamily,"
                  + "guar.HasSuperBilling,guar.BillingType,"
                  + "guar.Bal_0_30,guar.Bal_31_60,guar.Bal_61_90,guar.BalOver90,guar.BalTotal,guar.InsEst,guar.PayPlanDue,"
                  + "COALESCE(MAX(statement.DateSent),'0001-01-01') AS lastStatement,guar.PriProv,guar.Zip,guar.PatStatus,guar.HasSignedTil "
                  + "FROM patient guar "
                  + "INNER JOIN patient pat ON guar.PatNum=pat.Guarantor AND pat.PatStatus NOT IN (" + string.Join(",", listPatStatusExclude) + ") "
                  + "LEFT JOIN statement ON " + guarOrPat + ".PatNum=statement.PatNum "
                  + (ignoreInPerson ? "AND statement.Mode_!=" + SOut.Int((int) StatementMode.InPerson) + " " : "")
                  + "WHERE " + string.Join(" AND ", listWhereAnds) + " "
                  + "GROUP BY " + guarOrPat + ".PatNum "
                  + "ORDER BY " + guarOrPat + ".LName," + guarOrPat + ".FName ";
        var table = DataCore.GetTable(command);
        var agingList = new List<PatAging>();
        if (table.Rows.Count < 1) return agingList;
        var listDataRowsFromTable = table.Select().ToList();
        if (isSuperStatements)
        {
            var listSuperNums = listDataRowsFromTable //get all of the super heads that have members with balances.
                .FindAll(x => x["HasSuperBilling"].ToString() == "1" && x["SuperFamily"].ToString() != "0")
                .Select(x => SIn.Long(x["SuperFamily"].ToString())).Distinct().ToList();
            var listPatNums = listDataRowsFromTable.Select(x => SIn.Long(x["PatNum"].ToString())).ToList();
            //the super family heads that did not have balances, but whose members had balances.
            var listSuperFamilyNumsNeeded = listSuperNums.FindAll(x => !listPatNums.Contains(x));
            if (listSuperFamilyNumsNeeded.Count > 0)
            {
                //get our super family heads that did not have balances but whose members have balances.
                command = "SELECT " + guarOrPat + ".PatNum," + guarOrPat + ".FName," + guarOrPat + ".MiddleI," + guarOrPat + ".Preferred," + guarOrPat + ".LName," + guarOrPat + ".ClinicNum,guar.SuperFamily,"
                          + "guar.HasSuperBilling,guar.BillingType,"
                          + "guar.Bal_0_30,guar.Bal_31_60,guar.Bal_61_90,guar.BalOver90,guar.BalTotal,guar.InsEst,guar.PayPlanDue,"
                          + "COALESCE(MAX(statement.DateSent),'0001-01-01') AS lastStatement "
                          + "FROM patient guar "
                          + "LEFT JOIN statement ON " + guarOrPat + ".PatNum=statement.PatNum "
                          + "WHERE " + guarOrPat + ".PatNum IN(" + string.Join(",", listSuperFamilyNumsNeeded.Select(x => (x))) + ") "
                          + "GROUP BY " + guarOrPat + ".PatNum "
                          + "ORDER BY " + guarOrPat + ".LName," + guarOrPat + ".FName ";
                var superHeadTable = DataCore.GetTable(command);
                if (superHeadTable.Rows.Count > 0)
                {
                    table.Merge(superHeadTable);
                    var combinedView = table.DefaultView;
                    combinedView.Sort = "LName,FName";
                    table = combinedView.ToTable();
                }
            }
        }

        var listSuperFamNums = listDataRowsFromTable.Select(x => x["SuperFamily"].ToString()).Where(x => x != "0").Distinct().ToList();
        //Create a dictionary for each super family head member and create a PatAging object that will represent the entire super family.
        var dictSuperFamPatAging = new Dictionary<long, PatAging>();
        if (listSuperFamNums.Count > 0)
        {
            command = "SELECT supe.PatNum,supe.LName,supe.FName,supe.MiddleI,supe.Preferred,supe.SuperFamily,supe.BillingType,supe.ClinicNum,"
                      + "COALESCE(MAX(statement.DateSent),'0001-01-01') lastSuperStatement "
                      + "FROM patient supe "
                      + "LEFT JOIN statement ON supe.PatNum=statement.SuperFamily "
                      + (ignoreInPerson ? "AND statement.Mode_!=" + SOut.Int((int) StatementMode.InPerson) + " " : "")
                      + "WHERE supe.PatNum=supe.SuperFamily "
                      + "AND supe.HasSuperBilling=1 "
                      + "GROUP BY supe.PatNum "
                      + "ORDER BY NULL";
            dictSuperFamPatAging = DataCore.GetTable(command).Select().Where(x => listSuperFamNums.Contains(x["PatNum"].ToString()))
                .ToDictionary(x => SIn.Long(x["PatNum"].ToString()), x => new PatAging
                {
                    PatNum = SIn.Long(x["PatNum"].ToString()),
                    DateLastStatement = SIn.Date(x["lastSuperStatement"].ToString()),
                    SuperFamily = SIn.Long(x["SuperFamily"].ToString()),
                    HasSuperBilling = true, //query only returns super heads who do have super billing
                    PatName = GetNameLF(SIn.String(x["LName"].ToString()), SIn.String(x["FName"].ToString()), SIn.String(x["Preferred"].ToString()),
                        SIn.String(x["MiddleI"].ToString())),
                    BillingType = SIn.Long(x["BillingType"].ToString()),
                    ClinicNum = SIn.Long(x["ClinicNum"].ToString())
                });
        }

        //Only worry about looping through the entire data table for super family shenanigans if there are any super family head members present.
        if (dictSuperFamPatAging.Count > 0 && isSuperStatements)
        {
            PatAging patAgingCur;
            //Now that we know all of the super family heads, loop through all the other patients that showed up in the outstanding aging list
            //and add each super family memeber's aging to their corresponding super family head PatAging entry in the dictionary.
            foreach (DataRow rCur in table.Rows)
            {
                if (rCur["HasSuperBilling"].ToString() != "1" || rCur["SuperFamily"].ToString() == "0") continue;
                if (!dictSuperFamPatAging.TryGetValue(SIn.Long(rCur["SuperFamily"].ToString()), out patAgingCur)) continue; //super head must not have super billing
                patAgingCur.Bal_0_30 += SIn.Double(rCur["Bal_0_30"].ToString());
                patAgingCur.Bal_31_60 += SIn.Double(rCur["Bal_31_60"].ToString());
                patAgingCur.Bal_61_90 += SIn.Double(rCur["Bal_61_90"].ToString());
                patAgingCur.BalOver90 += SIn.Double(rCur["BalOver90"].ToString());
                patAgingCur.BalTotal += SIn.Double(rCur["BalTotal"].ToString());
                patAgingCur.InsEst += SIn.Double(rCur["InsEst"].ToString());
                patAgingCur.AmountDue = patAgingCur.BalTotal - patAgingCur.InsEst;
                patAgingCur.PayPlanDue += SIn.Double(rCur["PayPlanDue"].ToString());
            }
        }

        PatAging patage;
        DateTime dateLastStatement;
        DateTime maxDate;
        foreach (DataRow rowCur in table.Rows)
        {
            patage = new PatAging();
            patage.PatNum = SIn.Long(rowCur["PatNum"].ToString());
            patage.SuperFamily = SIn.Long(rowCur["SuperFamily"].ToString());
            patage.HasSuperBilling = SIn.Bool(rowCur["HasSuperBilling"].ToString());
            SIn.Bool(rowCur["HasSignedTil"].ToString());
            patage.ClinicNum = SIn.Long(rowCur["ClinicNum"].ToString());
            dateLastStatement = DateTime.MinValue;
            patage.PriProv = SIn.Long(rowCur["PriProv"].ToString());
            patage.Zip = SIn.String(rowCur["Zip"].ToString());
            SIn.Enum<PatientStatus>(rowCur["PatStatus"].ToString());
            PatAging superPat;
            if (patage.HasSuperBilling && dictSuperFamPatAging.TryGetValue(patage.SuperFamily, out superPat)) dateLastStatement = superPat.DateLastStatement;
            //If pat HasSuperBilling and super head has received a super statement, dateLastStatement will be the more recent date of the last super 
            //statement or the last family statement, regardless of the isSuperStatements flag.  If the guar is not in a super family with super billing, 
            //dateLastStatement will be the date of their last family statement (not-super statement).
            dateLastStatement = new[] {dateLastStatement, SIn.Date(rowCur["lastStatement"].ToString())}.Max();
            maxDate = DateTime.MinValue;
            //dict will not contain any values if 'includeChanged' is false
            if (dictPatAgingTransactions.TryGetValue(patage.PatNum, out var listPatAgingTransactions)) maxDate = AgingData.GetDateLastTrans(listPatAgingTransactions, dateLastStatement);
            if ((!isFinanceBilling && dateLastStatement.Date >= new[] {lastStatement.AddDays(1), maxDate}.Max().Date)
                || listPendingInsPatNums.Contains(patage.PatNum) //list only filled if excluding pending ins
                || listUnsentPatNums.Contains(patage.PatNum) //list only filled if excluding unsent procs
                || (isSuperStatements && patage.HasSuperBilling && patage.PatNum != patage.SuperFamily) //included in super statement and not the super head, skip
                || (dateLastStatement.Date < lastStatement.Date && excludeNotBilledSince)) //exclude account not billed since date
                continue; //this patient is excluded, skip
            //if not generating super statements OR this guar doesn't have super billing OR this guar's super family num isn't in the dictionary
            //then this guar will get a non-super statement (regular single family statement)
            if (!isSuperStatements || !patage.HasSuperBilling || !dictSuperFamPatAging.TryGetValue(patage.SuperFamily, out patage))
            {
                patage.Bal_0_30 = SIn.Double(rowCur["Bal_0_30"].ToString());
                patage.Bal_31_60 = SIn.Double(rowCur["Bal_31_60"].ToString());
                patage.Bal_61_90 = SIn.Double(rowCur["Bal_61_90"].ToString());
                patage.BalOver90 = SIn.Double(rowCur["BalOver90"].ToString());
                patage.BalTotal = SIn.Double(rowCur["BalTotal"].ToString());
                patage.InsEst = SIn.Double(rowCur["InsEst"].ToString());
                patage.AmountDue = patage.BalTotal - patage.InsEst;
                patage.PayPlanDue = SIn.Double(rowCur["PayPlanDue"].ToString());
                patage.DateLastStatement = SIn.Date(rowCur["lastStatement"].ToString());
                patage.PatName = GetNameLF(SIn.String(rowCur["LName"].ToString()), SIn.String(rowCur["FName"].ToString()),
                    SIn.String(rowCur["Preferred"].ToString()), SIn.String(rowCur["MiddleI"].ToString()));
                patage.BillingType = SIn.Long(rowCur["BillingType"].ToString());
            }

            agingList.Add(patage);
        }

        return agingList;
    }

    public static List<PatAging> GetAgingListSimple(List<long> listBillingTypeNums, List<long> listGuarantors, bool doIncludeZeroBalance = false, bool isGuarsOnly = false, List<long> listClinicNums = null, bool doIncludeSuperFamilyHeads = false)
    {
        var listWhereAnds = new List<string>();
        if (!doIncludeZeroBalance) listWhereAnds.Add("Bal_0_30 + Bal_31_60 + Bal_61_90 + BalOver90 - InsEst > '0.005'"); //more that 1/2 cent
        if (!listBillingTypeNums.IsNullOrEmpty()) listWhereAnds.Add("BillingType IN (" + string.Join(",", listBillingTypeNums) + ")");
        if (!listGuarantors.IsNullOrEmpty()) listWhereAnds.Add("PatNum IN (" + string.Join(",", listGuarantors) + ")");
        if (!listClinicNums.IsNullOrEmpty()) listWhereAnds.Add("ClinicNum IN (" + string.Join(",", listClinicNums) + ")");
        if (isGuarsOnly) listWhereAnds.Add("PatNum=Guarantor");
        var whereClause = "";
        if (listWhereAnds.Count > 0) whereClause = "WHERE (" + string.Join(" AND ", listWhereAnds) + ") ";
        if (doIncludeSuperFamilyHeads)
        {
            if (string.IsNullOrEmpty(whereClause))
                whereClause += "WHERE ";
            else
                whereClause += "OR ";
            whereClause += "PatNum=SuperFamily ";
        }

        var command = $@"SELECT PatNum,Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,BalTotal,InsEst,LName,FName,MiddleI,Preferred,PriProv,BillingType,
				Guarantor,SuperFamily,HasSuperBilling,ClinicNum
				FROM patient
				{whereClause}
				ORDER BY LName,FName";
        return DataCore.GetList(command, x =>
            new PatAging
            {
                PatNum = SIn.Long(x["PatNum"].ToString()),
                Bal_0_30 = SIn.Double(x["Bal_0_30"].ToString()),
                Bal_31_60 = SIn.Double(x["Bal_31_60"].ToString()),
                Bal_61_90 = SIn.Double(x["Bal_61_90"].ToString()),
                BalOver90 = SIn.Double(x["BalOver90"].ToString()),
                BalTotal = SIn.Double(x["BalTotal"].ToString()),
                InsEst = SIn.Double(x["InsEst"].ToString()),
                PatName = GetNameLF(SIn.String(x["LName"].ToString()), SIn.String(x["FName"].ToString()), SIn.String(x["Preferred"].ToString()),
                    SIn.String(x["MiddleI"].ToString())),
                AmountDue = SIn.Double(x["BalTotal"].ToString()) - SIn.Double(x["InsEst"].ToString()),
                PriProv = SIn.Long(x["PriProv"].ToString()),
                BillingType = SIn.Long(x["BillingType"].ToString()),
                Guarantor = SIn.Long(x["Guarantor"].ToString()),
                SuperFamily = SIn.Long(x["SuperFamily"].ToString()),
                HasSuperBilling = SIn.Bool(x["HasSuperBilling"].ToString()),
                ClinicNum = SIn.Long(x["ClinicNum"].ToString())
            });
    }

    public static List<PatAging> GetAgingListFromGuarNums(List<long> listGuarNums)
    {
        if (listGuarNums.Count < 1) return [];
        var command = "SELECT PatNum,Guarantor,LName,FName,MiddleI,PriProv,BillingType,ClinicNum,Bal_0_30,Bal_31_60,Bal_61_90,BalOver90,BalTotal,InsEst "
                      + "FROM patient "
                      + "WHERE patient.PatNum IN (" + string.Join(",", listGuarNums.Select(x => (x))) + ") "
                      + "AND patient.Guarantor=patient.PatNum";
        var listPatAgings = DataCore.GetTable(command).Select().Select(x => new PatAging
        {
            PatNum = SIn.Long(x["PatNum"].ToString()),
            Guarantor = SIn.Long(x["Guarantor"].ToString()),
            PatName = SIn.String(x["LName"].ToString()) + ", " + SIn.String(x["FName"].ToString()) + " " + SIn.String(x["MiddleI"].ToString()),
            PriProv = SIn.Long(x["PriProv"].ToString()),
            BillingType = SIn.Long(x["BillingType"].ToString()),
            ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
            Bal_0_30 = SIn.Double(x["Bal_0_30"].ToString()),
            Bal_31_60 = SIn.Double(x["Bal_31_60"].ToString()),
            Bal_61_90 = SIn.Double(x["Bal_61_90"].ToString()),
            BalOver90 = SIn.Double(x["BalOver90"].ToString()),
            BalTotal = SIn.Double(x["BalTotal"].ToString()),
            InsEst = SIn.Double(x["InsEst"].ToString()),
            AmountDue = SIn.Double(x["BalTotal"].ToString()) - SIn.Double(x["InsEst"].ToString()),
            ListTsiLogs = []
        }).ToList();
        if (listPatAgings.Count == 0) return listPatAgings;
        var dictPatNumListTrans = TsiTransLogs.SelectMany(listGuarNums)
            .GroupBy(x => x.PatNum)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.TransDateTime).ToList());
        foreach (var patAgingCur in listPatAgings)
            if (!dictPatNumListTrans.TryGetValue(patAgingCur.Guarantor, out patAgingCur.ListTsiLogs))
                patAgingCur.ListTsiLogs = [];

        return listPatAgings;
    }

    public static string GetNextChartNum()
    {
        var command = "SELECT ChartNumber from patient WHERE "
                      + DbHelper.Regexp("ChartNumber", "^[0-9]+$") + " " //matches any positive number of digits
                      + "ORDER BY (chartnumber+0) DESC"; //1/13/05 by Keyush Shaw-added 0.
        command = DbHelper.LimitOrderBy(command, 1);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) //no existing chart numbers
            return "1";
        var lastChartNum = SIn.String(table.Rows[0][0].ToString());
        //or could add more match conditions
        try
        {
            return (Convert.ToInt64(lastChartNum) + 1).ToString();
        }
        catch
        {
            throw new ApplicationException(lastChartNum + " is an existing ChartNumber.  It's too big to convert to a long int, so it's not possible to add one to automatically increment.");
        }
    }

    public static string ChartNumUsedBy(string chartNum, long excludePatNum)
    {
        var command = "SELECT LName,FName from patient WHERE "
                      + "ChartNumber = '" + SOut.String(chartNum)
                      + "' AND PatNum != '" + excludePatNum + "'";
        var table = DataCore.GetTable(command);
        var retVal = "";
        if (table.Rows.Count != 0) //found duplicate chart number
            retVal = SIn.String(table.Rows[0][1].ToString()) + " " + SIn.String(table.Rows[0][0].ToString());
        return retVal;
    }

    public static int GetNumberPatients()
    {
        var command = "SELECT Count(*) FROM patient";
        var table = DataCore.GetTable(command);
        return SIn.Int(table.Rows[0][0].ToString());
    }

    public static void SetHasIns(long patNum)
    {
        var command = "SELECT patient.HasIns,COUNT(patplan.PatNum) FROM patient "
                      + "LEFT JOIN patplan ON patplan.PatNum=patient.PatNum"
                      + " WHERE patient.PatNum=" + (patNum)
                      + " GROUP BY patplan.PatNum,patient.HasIns";
        var table = DataCore.GetTable(command);
        var newVal = "";
        if (table.Rows[0][1].ToString() != "0") newVal = "I";
        if (newVal != table.Rows[0][0].ToString())
        {
            command = "UPDATE patient SET HasIns='" + SOut.String(newVal)
                                                    + "' WHERE PatNum=" + (patNum);
            Db.NonQ(command);
        }
    }

    public static long GetProvNum(Patient pat)
    {
        var retval = pat.PriProv;
        if (retval == 0) retval = PrefC.GetLong(PrefName.PracticeDefaultProv);
        if (retval == 0) retval = Providers.GetFirstOrDefault(x => true, true)?.Id ?? 0;
        return retval;
    }

    public static long GetProvNum(long patNum)
    {
        return GetProvNum(GetPat(patNum));
    }

    public static long[] GetAllPatNums(bool hasDeleted)
    {
        var command = "";
        if (hasDeleted)
            command = "SELECT PatNum FROM patient";
        else
            command = "SELECT PatNum FROM patient WHERE patient.PatStatus!=4";
        var dt = DataCore.GetTable(command);
        var patnums = new long[dt.Rows.Count];
        for (var i = 0; i < patnums.Length; i++) patnums[i] = SIn.Long(dt.Rows[i]["PatNum"].ToString());
        return patnums;
    }

    public static int DateToAge(DateTime date)
    {
        if (date.Year < 1880) return 0;
        if (date.Month < DateTime.Now.Month) //birthday in previous month
            return DateTime.Now.Year - date.Year;
        if (date.Month == DateTime.Now.Month && date.Day <= DateTime.Now.Day) //birthday in this month
            return DateTime.Now.Year - date.Year;
        return DateTime.Now.Year - date.Year - 1;
    }

    public static int DateToAge(DateTime birthdate, DateTime asofDate)
    {
        if (birthdate.Year < 1880)
            return 0;
        if (birthdate.Month < asofDate.Month) //birthday in previous month
            return asofDate.Year - birthdate.Year;
        if (birthdate.Month == asofDate.Month && birthdate.Day <= asofDate.Day) //birthday in this month
            return asofDate.Year - birthdate.Year;
        return asofDate.Year - birthdate.Year - 1;
    }

    public static string AgeToString(int age)
    {
        if (age == 0) return "";

        return age.ToString();
    }

    public static void ReformatAllPhoneNumbers()
    {
        string oldTel;
        string newTel;
        string idNum;
        var command = "select * from patient";
        var table = DataCore.GetTable(command);
        var listPatNumsForPhNumSync = new List<long>();
        var doSyncPhNumTable = PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var patNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
            idNum = SIn.String(table.Rows[i][0].ToString());
            //home
            oldTel = SIn.String(table.Rows[i][15].ToString());
            newTel = TelephoneNumbers.ReFormat(oldTel);
            if (oldTel != newTel)
            {
                command = "UPDATE patient SET hmphone = '"
                          + SOut.String(newTel) + "' WHERE patNum = '" + idNum + "'";
                Db.NonQ(command);
                if (doSyncPhNumTable) listPatNumsForPhNumSync.Add(patNum);
            }

            //wk:
            oldTel = SIn.String(table.Rows[i][16].ToString());
            newTel = TelephoneNumbers.ReFormat(oldTel);
            if (oldTel != newTel)
            {
                command = "UPDATE patient SET wkphone = '"
                          + SOut.String(newTel) + "' WHERE patNum = '" + idNum + "'";
                Db.NonQ(command);
                if (doSyncPhNumTable) listPatNumsForPhNumSync.Add(patNum);
            }

            //wireless
            oldTel = SIn.String(table.Rows[i][17].ToString());
            newTel = TelephoneNumbers.ReFormat(oldTel);
            if (oldTel != newTel)
            {
                command = "UPDATE patient SET wirelessphone = '"
                          + SOut.String(newTel) + "' WHERE patNum = '" + idNum + "'";
                Db.NonQ(command);
                if (doSyncPhNumTable) listPatNumsForPhNumSync.Add(patNum);
            }
        }

        if (doSyncPhNumTable && listPatNumsForPhNumSync.Count > 0) PhoneNumbers.SyncPats(GetMultPats(listPatNumsForPhNumSync.Distinct().ToList()).ToList());
        command = "SELECT * from carrier";
        table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            idNum = SIn.String(table.Rows[i][0].ToString());
            //ph
            oldTel = SIn.String(table.Rows[i][7].ToString());
            newTel = TelephoneNumbers.ReFormat(oldTel);
            if (oldTel != newTel)
            {
                command = "UPDATE carrier SET Phone = '"
                          + SOut.String(newTel) + "' WHERE CarrierNum = '" + idNum + "'";
                Db.NonQ(command);
            }
        }

        command = "SELECT PatNum,ICEPhone FROM patientnote";
        table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            idNum = SIn.String(table.Rows[i]["PatNum"].ToString());
            oldTel = SIn.String(table.Rows[i]["ICEPhone"].ToString());
            newTel = TelephoneNumbers.ReFormat(oldTel);
            if (oldTel != newTel)
            {
                command = "UPDATE patientnote SET ICEPhone='" + SOut.String(newTel) + "' WHERE PatNum=" + idNum;
                Db.NonQ(command);
            }
        }
    }

    public static DataTable GetGuarantorInfo(long PatientID)
    {
        var command = @"SELECT FName,MiddleI,LName,Guarantor,Address,
								Address2,City,State,Zip,Email,EstBalance,
								BalTotal,Bal_0_30,Bal_31_60,Bal_61_90,BalOver90
						FROM Patient Where Patnum=" + PatientID +
                      " AND patnum=guarantor";
        return DataCore.GetTable(command);
    }

    public static long GetPatNumByNameAndBirthday(string lName, string fName, DateTime birthdate)
    {
        var command = "SELECT PatNum FROM patient WHERE "
                      + "LName='" + SOut.String(lName) + "' "
                      + "AND FName='" + SOut.String(fName) + "' "
                      + "AND Birthdate=" + SOut.Date(birthdate) + " "
                      + "AND PatStatus!=" + SOut.Int((int) PatientStatus.Archived) + " " //Not Archived
                      + "AND PatStatus!=" + SOut.Int((int) PatientStatus.Deleted); //Not Deleted
        return SIn.Long(DataCore.GetScalar(command));
    }

    public static List<Patient> GetListByName(string lName, string fName, long PatNum)
    {
        var command = $@"SELECT * FROM patient
				WHERE PatNum!={(PatNum)}
				AND PatStatus!={SOut.Int((int) PatientStatus.Deleted)}
				AND FName='{SOut.String(fName)}'
				AND LName='{SOut.String(lName)}'";
        return PatientCrud.SelectMany(command);
    }

    public static void UpdateFamilyBillingType(long billingType, long Guarantor)
    {
        var command = "UPDATE patient SET BillingType=" + (billingType) +
                      " WHERE Guarantor=" + (Guarantor);
        Db.NonQ(command);
    }

    public static void UpdateAllFamilyBillingTypes(long billingType, List<long> listGuarNums)
    {
        if (listGuarNums.Count < 1) return;
        var command = "UPDATE patient SET BillingType=" + (billingType) + " "
                      + "WHERE Guarantor IN (" + string.Join(",", listGuarNums.Select(x => (x))) + ")";
        Db.NonQ(command);
    }

    public static string GetEligibilityDisplayName(long patId)
    {
        var command = @"SELECT FName,LName," + DbHelper.DateFormatColumn("birthdate", "%m/%d/%Y") + " BirthDate,Gender "
                      + "FROM patient WHERE patient.PatNum=" + (patId);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "Patient(???) is Eligible";
        return SIn.String(table.Rows[0][1].ToString()) + ", " + SIn.String(table.Rows[0][0].ToString()) + " is Eligible";
    }

    public static bool IsTrophyFolderInUse(string folderName)
    {
        var command = "SELECT COUNT(*) FROM patient WHERE TrophyFolder LIKE '%" + SOut.String(folderName) + "%'";
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    public static bool IsBillingTypeInUse(long defNum)
    {
        var command = "SELECT COUNT(*) FROM patient WHERE BillingType=" + (defNum) + " AND PatStatus!=" + SOut.Int((int) PatientStatus.Deleted);
        if (Db.GetCount(command) != "0") return true;
        command = "SELECT COUNT(*) FROM insplan WHERE BillingType=" + (defNum);
        if (Db.GetCount(command) != "0") return true;
        //check any prefs that are FK's to the definition.DefNum column and warn if a pref is using the def
        if (new[]
            {
                PrefName.TransworldPaidInFullBillingType, PrefName.ApptEConfirmStatusSent, PrefName.ApptEConfirmStatusAccepted,
                PrefName.ApptEConfirmStatusDeclined, PrefName.ApptEConfirmStatusSendFailed, PrefName.ApptConfirmExcludeEConfirm,
                PrefName.ApptConfirmExcludeERemind, PrefName.ApptConfirmExcludeESend, PrefName.ApptConfirmExcludeEThankYou,
                PrefName.ApptConfirmExcludeNewPatThankYou, PrefName.BrokenAppointmentAdjustmentType, PrefName.ConfirmStatusEmailed,
                PrefName.ConfirmStatusTextMessaged, PrefName.PrepaymentUnearnedType, PrefName.SalesTaxAdjustmentType
            }
            .Select(x => PrefC.GetString(x))
            .SelectMany(x => x.Split(',').Select(y => SIn.Long(y, false)).Where(y => y > 0)) //some prefs are comma delimited lists of longs. SelectMany will return a single list of longs
            .Any(x => x == defNum))
            return true;
        return false;
    }

    public static bool IsValidSSN(string ssn, out string formattedSSN)
    {
        if (Regex.IsMatch(ssn, @"^\d{9}$")) //if just 9 numbers, reformat with dashes.
            ssn = ssn.Substring(0, 3) + "-" + ssn.Substring(3, 2) + "-" + ssn.Substring(5, 4);
        formattedSSN = ssn;
        return Regex.IsMatch(formattedSSN, @"^\d\d\d-\d\d-\d\d\d\d$");
    }

    public static string SSNRemoveDashes(string ssn)
    {
        if (CultureInfo.CurrentCulture.Name == "en-US")
            if (Regex.IsMatch(ssn, @"^\d\d\d-\d\d-\d\d\d\d$"))
                return ssn.Replace("-", "");

        return ssn; //other cultures
    }

    public static bool MergeTwoPatients(long patTo, long patFrom)
    {
        if (patTo == patFrom)
            //Do not merge the same patient onto itself.
            return true;
        //We need to test patfields before doing anything else because the user may wish to cancel and abort the merge.
        var patToFields = PatFields.Refresh(patTo);
        var patFromFields = PatFields.Refresh(patFrom);
        var patFieldsToDelete = new List<PatField>();
        var patFieldsToUpdate = new List<PatField>();
        for (var i = 0; i < patFromFields.Length; i++)
        {
            var hasMatch = false;
            for (var j = 0; j < patToFields.Length; j++)
                //Check patient fields that are the same to see if they have different values.
                if (patFromFields[i].FieldName == patToFields[j].FieldName)
                {
                    hasMatch = true;
                    if (patFromFields[i].FieldValue != patToFields[j].FieldValue)
                    {
                        //Get input from user on which value to use.
                        var result = MessageBox.Show("The two patients being merged have different values set for the patient field:\r\n\"" + patFromFields[i].FieldName + "\"\r\n\r\n"
                                                     + "The merge into patient has the value: \"" + patToFields[j].FieldValue + "\"\r\n"
                                                     + "The merge from patient has the value: \"" + patFromFields[i].FieldValue + "\"\r\n\r\n"
                                                     + "Would you like to overwrite the merge into value with the merge from value?\r\n(Cancel will abort the merge)", "Warning", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            //User chose to use the merge from patient field info.
                            patFromFields[i].PatNum = patTo;
                            patFieldsToUpdate.Add(patFromFields[i]);
                            patFieldsToDelete.Add(patToFields[j]);
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            return false; //Completely cancels the entire merge.  No changes have been made at this point.
                        }
                    }
                }

            if (!hasMatch)
            {
                //The patient field does not exist in the merge into account.
                patFromFields[i].PatNum = patTo;
                patFieldsToUpdate.Add(patFromFields[i]);
            }
        }

        var isMergeSuccessful = false;
        var retryCount = 5;
        while (--retryCount >= 0 && !isMergeSuccessful)
            try
            {
                isMergeSuccessful = MergeTwoPatientPointOfNoReturn(patTo, patFrom, patFieldsToDelete, patFieldsToUpdate);
            }
            catch (Exception ex)
            {
                if (retryCount <= 0) throw; //Throw exception after retrying 5 times.
            }

        return isMergeSuccessful;
    }

    public static bool MergeTwoPatientPointOfNoReturn(long patTo, long patFrom, List<PatField> patFieldsToDelete, List<PatField> patFieldsToUpdate)
    {
        var command = "";
        //Update and remove all patfields that were added to the list above.
        for (var i = 0; i < patFieldsToDelete.Count; i++) PatFields.Delete(patFieldsToDelete[i]);
        for (var j = 0; j < patFieldsToUpdate.Count; j++) PatFields.Update(patFieldsToUpdate[j]);
        var patientFrom = GetPat(patFrom);
        var patientTo = GetPat(patTo);
        //CustReference.  We need to combine patient from and patient into entries to have the into patient information from both.
        var custRefFrom = CustReferences.GetOneByPatNum(patientFrom.PatNum);
        var custRefTo = CustReferences.GetOneByPatNum(patientTo.PatNum);
        if (custRefFrom != null && custRefTo != null)
        {
            //If either of these are null, do nothing.  This is an internal only table so we didn't bother fixing it/warning users here.
            var newCustRef = new CustReference();
            newCustRef.CustReferenceNum = custRefTo.CustReferenceNum; //Use same primary key so we can update.
            newCustRef.PatNum = patientTo.PatNum;
            if (custRefTo.DateMostRecent > custRefFrom.DateMostRecent)
                newCustRef.DateMostRecent = custRefTo.DateMostRecent; //Use the most recent date.
            else
                newCustRef.DateMostRecent = custRefFrom.DateMostRecent; //Use the most recent date.
            if (custRefTo.Note == "")
                newCustRef.Note = custRefFrom.Note;
            else if (custRefFrom.Note == "")
                newCustRef.Note = custRefTo.Note;
            else //Both entries have a note
                newCustRef.Note = custRefTo.Note + " | " + custRefFrom.Note; /*Combine in a | delimited string*/
            newCustRef.IsBadRef = custRefFrom.IsBadRef || custRefTo.IsBadRef; //If either entry is a bad reference, count as a bad reference.
            CustReferences.Update(newCustRef); //Overwrites the old custRefTo entry.
        }
        
        //If the 'patFrom' had any ties to guardians, they should be deleted to prevent duplicate entries.
        command = "DELETE FROM guardian"
                  + " WHERE PatNumChild=" + (patFrom)
                  + " OR PatNumGuardian=" + (patFrom);
        Db.NonQ(command);
        //Merge patient notes prior to updating the patient table, otherwise the wrong notes might bet set.
        PatientNotes.Merge(patientFrom, patientTo);
        //Update all guarantor foreign keys to change them from 'patFrom' to 
        //the guarantor of 'patTo'. This will effectively move all 'patFrom' family members 
        //to the family defined by 'patTo' in the case that 'patFrom' is a guarantor. If
        //the guarantor for 'patTo' is 'patFrom' then set the guarantor for the family instead to
        //'patTo'. If 'patFrom' is not a guarantor, then this command will have no effect and is
        //thus safe to always be run.
        var newGuarantor = patFrom == patientTo.Guarantor ? patientTo.PatNum : patientTo.Guarantor;
        var listPatients = GetAllPatientsForGuarantor(patFrom);
        for (var i = 0; i < listPatients.Count; i++)
        {
            var patientNew = listPatients[i].Copy();
            patientNew.Guarantor = newGuarantor; //Changing guarantor requires rehashing
            Update(patientNew, listPatients[i]);
        }

        //Update tables where the PatNum should be changed to the guarantor of the patient being merged into.
        //Only accomplishes anything if the patFrom is a guarantor. Otherwise, no effect.
        string[] listGuarantorToGuarantor = ["famaging", "tsitranslog"];
        for (var i = 0; i < listGuarantorToGuarantor.Length; i++)
        {
            command = "UPDATE " + SOut.String(listGuarantorToGuarantor[i]) + " "
                      + "SET PatNum=" + (newGuarantor) + " "
                      + "WHERE PatNum=" + (patFrom);
            Db.NonQ(command);
        }

        //At this point, the 'patFrom' is a regular patient and is absoloutely not a guarantor.
        //Now modify all PatNum foreign keys from 'patFrom' to 'patTo' to complete the majority of the
        //merge of the records between the two accounts.			
        for (var i = 0; i < StringArrayPatNumForeignKeys.Length; i++)
        {
            var tableAndKeyName = StringArrayPatNumForeignKeys[i].Split('.');
            command = "UPDATE " + tableAndKeyName[0]
                                + " SET " + tableAndKeyName[1] + "=" + (patTo)
                                + " WHERE " + tableAndKeyName[1] + "=" + (patFrom);
            Db.NonQ(command);
        }

        //We have changed the PatNum in the paysplit table.
        //The PatNum field is included in the generation of the PaySplit.SecurityHash field.
        //Rehash all their paysplits to match the new PatNum.
        var listPaySplits = PaySplits.GetForPats([patTo]);
        for (var i = 0; i < listPaySplits.Count; i++)
        {
            listPaySplits[i].SecurityHash = PaySplits.HashFields(listPaySplits[i]);
            PaySplits.Update(listPaySplits[i]);
        }

        //Update the 'HasIns' column, and handle discount plans.
        //If the combined patient has insurance, we want to make sure we update 'HasIns' and 
        //remove any discount plan. If the combined patient has no insurance, only merge in 
        //the 'patFrom' discount plan if 'patTo' doesn't have a discount plan already.
        if (PatPlans.GetPatPlansForPat(patTo).Count > 0)
        {
            //If the merged patient has insurance
            //Set HasIns to true
            command = "UPDATE patient "
                      + "SET HasIns='I' "
                      + "WHERE PatNum=" + (patTo);
            Db.NonQ(command);
            //Remove discount plans if necessary
            if (DiscountPlanSubs.HasDiscountPlan(patTo)) DiscountPlanSubs.DeleteForPatient(patTo);
        }
        else if (!DiscountPlanSubs.HasDiscountPlan(patTo))
        {
            //If patTo has no discount plan or insurance
            //Set the discount plan if there isn't one already
            command = "UPDATE discountplansub "
                      + "SET PatNum=" + (patTo) + " "
                      + "WHERE PatNum=" + (patFrom);
            Db.NonQ(command);
        }

        //Clean up any remaining discount plans on the old patient.
        if (DiscountPlanSubs.HasDiscountPlan(patFrom)) DiscountPlanSubs.DeleteForPatient(patFrom);
        //We have to move over the tasks belonging to the 'patFrom' patient in a seperate step because
        //the KeyNum field of the task table might be a foreign key to something other than a patnum,
        //including possibly an appointment number.
        command = "UPDATE task "
                  + "SET KeyNum=" + (patTo) + " "
                  + "WHERE KeyNum=" + (patFrom) + " AND ObjectType=" + (int) TaskObjectType.Patient;
        Db.NonQ(command);
        //We have to move over the tasks belonging to the 'patFrom' patient in a seperate step because the KeyNum field of the taskhist table might be 
        //  a foreign key to something other than a patnum, including possibly an appointment number.
        command = "UPDATE taskhist "
                  + "SET KeyNum=" + (patTo) + " "
                  + "WHERE KeyNum=" + (patFrom) + " AND ObjectType=" + (int) TaskObjectType.Patient;
        Db.NonQ(command);
        //Mark the patient where data was pulled from as archived unless the patient is already marked as deceased.
        //We need to have the patient marked either archived or deceased so that it is hidden by default, and
        //we also need the customer to be able to access the account again in case a particular table gets missed
        //in the merge tool after an update to Open Dental. This will allow our customers to remerge the missing
        //data after a bug fix is released. 
        command = "UPDATE patient "
                  + "SET PatStatus=" + (int) PatientStatus.Archived + " "
                  + "WHERE PatNum=" + (patFrom) + " "
                  + "AND PatStatus!=" + (int) PatientStatus.Deceased;
        Db.NonQ(command);
        //Set remove PatFrom from the superfamily if they currently belong in one by setting patient.SuperFamily to 0.
        if (patientFrom.SuperFamily != 0)
        {
            command = "UPDATE patient SET patient.SuperFamily=0 WHERE patient.PatNum=" + (patFrom) + ";";
            Db.NonQ(command);
        }

        //Update patplan.Ordinal in case multiple patplans wound up with the same Ordinal
        var listPatPlans = PatPlans.GetPatPlansForPat(patTo).OrderBy(x => x.Ordinal).ToList();
        var patPlanPrimary = listPatPlans.FirstOrDefault();
        if (patPlanPrimary != null) PatPlans.SetOrdinal(patPlanPrimary.PatPlanNum, patPlanPrimary.Ordinal); //Will reset all other Ordinals to consecutive values.
        //This updates the referrals with the new patient information from the merge.
        var referral = Referrals.GetFirstOrDefault(x => x.PatNum == patFrom);
        if (referral != null)
        {
            referral.PatNum = patientTo.PatNum;
            referral.LName = patientTo.LName;
            referral.FName = patientTo.FName;
            referral.MName = patientTo.MiddleI;
            referral.Address = patientTo.Address;
            referral.Address2 = patientTo.Address2;
            referral.City = patientTo.City;
            referral.ST = patientTo.State;
            referral.SSN = patientTo.SSN;
            referral.Zip = patientTo.Zip;
            referral.Telephone = TelephoneNumbers.FormatNumbersExactTen(patientTo.HmPhone);
            referral.EMail = patientTo.Email;
            Referrals.Update(referral);
            Referrals.RefreshCache();
        }

        Recalls.Synch(patTo); //Update patient's recalls now that merge is completed.
        //Delete all Clone PatientLinks for this patient, it has been merged into another patient.
        PatientLinks.DeletePatNumTos(patFrom, PatientLinkType.Clone);
        //Delete any existing PatientLinks between these two patients of type Clone.
        PatientLinks.DeleteCloneBetweenToAndFrom(patFrom, patTo);
        //Create a link from the from patient to the to patient.
        var patLink = new PatientLink();
        patLink.PatNumFrom = patFrom;
        patLink.PatNumTo = patTo;
        patLink.LinkType = PatientLinkType.Merge;
        PatientLinks.Insert(patLink);
        //Update any remaining Clones PatientLinks and set their PatNumFrom fields to the merged into patnum. The clones will now be cloned from the merged into patient.
        PatientLinks.UpdateFromPatientClonesAfterMerge(patFrom, patTo);
        return true;
    }

    public static PronounPreferred GetPronoun(PatientGender patientGender, PronounPreferred pronounPreferred)
    {
        if (PrefC.GetBool(PrefName.ShowPreferredPronounsForPats))
            if (pronounPreferred != PronounPreferred.None)
                //they specified a pronoun override
                return pronounPreferred;

        //base pronoun on gender
        switch (patientGender)
        {
            case PatientGender.Male:
                return PronounPreferred.HeHim;
            case PatientGender.Female:
                return PronounPreferred.SheHer;
            case PatientGender.Other:
                return PronounPreferred.TheyThem;
            case PatientGender.Unknown:
            default:
                return PronounPreferred.None;
        }
    }

    public static string GetNameLF(string LName, string FName, string Preferred, string MiddleI)
    {
        var retVal = "";
        retVal += LName;
        if (FName != "" || MiddleI != "" || Preferred != "") retVal += ",";
        if (Preferred != "") retVal += " '" + Preferred + "'";
        if (FName != "")
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += FName;
        }

        if (MiddleI != "")
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += MiddleI;
        }

        return retVal;
    }

    public static string GetNameLF(long patNum)
    {
        var pat = GetLim(patNum);
        return GetNameLF(pat);
    }

    public static string GetNameLF(Patient pat)
    {
        var retVal = "";
        retVal += pat.LName;
        if (pat.FName != "" || pat.MiddleI != "" || pat.Preferred != "") retVal += ",";
        if (pat.Preferred != "") retVal += " '" + pat.Preferred + "'";
        if (pat.FName != "")
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += pat.FName;
        }

        if (pat.MiddleI != "")
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += pat.MiddleI;
        }

        return retVal;
    }

    public static string GetNameLFnoPref(string LName, string FName, string MiddleI)
    {
        return GetNameLF(LName, FName, "", MiddleI);
    }

    public static string GetNameFL(long patNum)
    {
        if (patNum == 0) return "";
        var pat = GetLim(patNum);
        if (pat == null) return "";
        return GetNameFL(pat.LName, pat.FName, pat.Preferred, pat.MiddleI);
    }

    public static string GetNameFL(string LName, string FName, string Preferred, string MiddleI)
    {
        var retVal = "";
        if (FName != "") retVal += FName;
        if (!string.IsNullOrWhiteSpace(Preferred))
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += "'" + Preferred + "'";
        }

        if (!string.IsNullOrWhiteSpace(MiddleI))
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += MiddleI;
        }

        retVal = AddSpaceIfNeeded(retVal);
        retVal += LName;
        return retVal;
    }

    public static string GetNameFLnoPref(string LName, string FName, string MiddleI)
    {
        var retVal = "";
        retVal += FName;
        if (!string.IsNullOrWhiteSpace(MiddleI))
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += MiddleI;
        }

        retVal = AddSpaceIfNeeded(retVal);
        retVal += LName;
        return retVal;
    }

    public static string GetNameFirstOrPrefL(string LName, string FName, string Preferred)
    {
        var retVal = "";
        if (Preferred == "")
            retVal += FName;
        else
            retVal += Preferred;
        retVal = AddSpaceIfNeeded(retVal);
        retVal += LName;
        return retVal;
    }

    public static string GetNameFirstOrPrefML(string LName, string FName, string Preferred, string MiddleI)
    {
        var retVal = "";
        if (Preferred == "")
        {
            retVal += FName;
        }
        else
        {
            retVal += Preferred;
            ;
        }

        if (!string.IsNullOrWhiteSpace(MiddleI))
        {
            retVal = AddSpaceIfNeeded(retVal);
            retVal += MiddleI + ".";
        }

        retVal = AddSpaceIfNeeded(retVal);
        retVal += LName;
        return retVal;
    }

    public static string GetNameFLFormal(string LName, string FName, string MiddleI, string Title)
    {
        return string.Join(" ", new[] {Title, FName, MiddleI, LName}.Where(x => !string.IsNullOrEmpty(x))); //returns "" if all strings are null or empty.
    }

    public static string GetNameFirst(string FName, string Preferred)
    {
        var retVal = FName;
        if (Preferred != "") retVal += " '" + Preferred + "'";
        return retVal;
    }

    public static string GetNameFirstOrPreferred(string nameFirst, string namePreferred)
    {
        if (string.IsNullOrWhiteSpace(namePreferred)) return nameFirst ?? "";
        return namePreferred;
    }

    public static string GetNameFirstOrPreferredOrLast(string FName, string Preferred, string LName)
    {
        if (FName != "") return FName;
        if (Preferred != "") return Preferred;
        return LName;
    }

    private static string AddSpaceIfNeeded(string name)
    {
        if (name != "") return name + " ";
        return name;
    }

    public static string GetSalutation(string Salutation, string Preferred, string FName)
    {
        if (Salutation != "") return Salutation;
        if (Preferred != "") return Preferred;
        return FName;
    }

    public static string GetAddressFull(string address, string address2, string city, string state, string zip)
    {
        var retVal = address;
        if (address2 != "") retVal += "\r\n" + address2;
        retVal += "\r\n" + city + ", " + state + " " + zip;
        return retVal;
    }

    public static void ChangePrimaryProviders(long provNumFrom, long provNumTo)
    {
        var command = "UPDATE patient SET PriProv=" + (provNumTo) + " WHERE PriProv=" + (provNumFrom);
        Db.NonQ(command);
    }

    public static void ChangeSecondaryProviders(long provNumFrom, long provNumTo)
    {
        var command = "UPDATE patient "
                      + "SET SecProv = '" + provNumTo + "' "
                      + "WHERE SecProv = '" + provNumFrom + "'";
        Db.NonQ(command);
    }

    public static DataTable GetPatNumsByPriProvs(List<long> listProvNums)
    {
        if (listProvNums == null || listProvNums.Count == 0) return new DataTable();
        var command = "SELECT PatNum,PriProv FROM patient WHERE PriProv IN (" + string.Join(",", listProvNums) + ")";
        return DataCore.GetTable(command);
    }

    public static List<long> GetPatNumsByClinic(long clinicNum, bool getAllStatuses = false)
    {
        var command = "SELECT PatNum FROM patient WHERE ClinicNum=" + (clinicNum);
        if (!getAllStatuses)
            command += " AND PatStatus NOT IN (" + SOut.Int((int) PatientStatus.Deleted) + "," + SOut.Int((int) PatientStatus.Archived) + ","
                       + SOut.Int((int) PatientStatus.Deceased) + "," + SOut.Int((int) PatientStatus.NonPatient) + ") ";
        return Db.GetListLong(command);
    }

    public static void ChangeClinicsForAll(long clinicNumFrom, long clinicNumTo)
    {
        var command = "UPDATE patient SET ClinicNum=" + (clinicNumTo) + " WHERE ClinicNum=" + (clinicNumFrom);
        Db.NonQ(command);
    }

    public static void UpdateProv(long patNum, long provNumNew)
    {
        var command = "UPDATE patient SET PriProv =" + (provNumNew) + " WHERE PatNum = " + (patNum);
        Db.NonQ(command);
    }

    public static DataTable GetSuperFamProcAdjustsPPCharges(long superFamily)
    {
        var listPatients = GetBySuperFamily(superFamily);
        var listPatNums = listPatients.Where(x =>
                (x.PatNum == x.Guarantor && x.HasSuperBilling)
                || (x.PatNum != x.Guarantor && listPatients.Exists(y => y.PatNum == x.Guarantor && y.HasSuperBilling)))
            .Select(x => x.PatNum).ToList();
        var command = "SELECT * FROM ("
                      + "SELECT procedurelog.ProcNum AS 'PriKey', procedurelog.ProcDate AS 'Date', procedurelog.PatNum AS 'PatNum', procedurelog.ProvNum AS 'Prov' "
                      + ",procedurelog.ProcFee AS 'Amount', procedurelog.CodeNum AS 'Code', procedurelog.ToothNum AS 'Tooth', '' AS 'AdjType', '' AS 'ChargeType'"
                      + ", " + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " AS 'PatName'"
                      + "FROM procedurelog "
                      + "INNER JOIN patient ON procedurelog.PatNum=patient.PatNum "
                      + "WHERE procedurelog.PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND StatementNum=0 "
                      + "AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                      + "UNION ALL "
                      + "SELECT adjustment.AdjNum AS 'PriKey', adjustment.AdjDate AS 'Date', adjustment.PatNum AS 'PatNum', adjustment.ProvNum AS 'Prov'"
                      + ", adjustment.AdjAmt AS 'Amount', '' AS 'Code', '' AS 'Tooth', adjustment.AdjType AS 'AdjType', '' AS 'ChargeType'"
                      + ", " + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " AS 'PatName'"
                      + "FROM adjustment "
                      + "INNER JOIN patient ON adjustment.PatNum=patient.PatNum "
                      + "WHERE adjustment.PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND StatementNum=0 "
                      + "UNION ALL "
                      + "SELECT payplancharge.PayPlanChargeNum AS 'PriKey', payplancharge.ChargeDate AS 'Date', payplancharge.PatNum AS 'PatNum'"
                      + ",payplancharge.ProvNum AS 'Prov', payplancharge.Principal+payplancharge.Interest AS 'Amount', '' AS 'Code', '' AS 'Tooth'"
                      + ",'' AS 'AdjType', payplancharge.ChargeType AS 'ChargeType'," + DbHelper.Concat("patient.LName", "', '", "patient.FName") + " AS 'PatName'"
                      + "FROM payplancharge "
                      + "INNER JOIN patient ON payplancharge.PatNum=patient.PatNum "
                      + "WHERE payplancharge.PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND payplancharge.ChargeType=" + SOut.Int((int) PayPlanChargeType.Debit) + " "
                      + "AND StatementNum=0 "
                      + "AND " + SOut.Bool(PrefC.GetInt(PrefName.PayPlansVersion) == (int) PayPlanVersions.AgeCreditsAndDebits) + " "
                      + "AND payplancharge.ChargeDate<ADDDATE(NOW(), INTERVAL 3 MONTH) " //Only show payplan charges less than 3 mos into the future
                      + ") procadj ORDER BY procadj.Date DESC";
        return DataCore.GetTable(command);
    }

    public static List<Patient> GetBySuperFamily(long SuperFamilyNum)
    {
        if (SuperFamilyNum == 0) return []; //return empty list

        var command = "SELECT * FROM patient WHERE SuperFamily=" + (SuperFamilyNum)
                                                                 + " AND patient.PatStatus!=" + SOut.Int((int) PatientStatus.Deleted);
        return PatientCrud.SelectMany(command);
    }

    public static List<Patient> GetSuperFamilyGuarantors(long SuperFamilyNum)
    {
        if (SuperFamilyNum == 0) return []; //return empty list
        //Should also work in Oracle.
        //this query was taking 2.5 seconds on a large database
        //string command = "SELECT DISTINCT * FROM patient WHERE PatNum IN (SELECT Guarantor FROM patient WHERE SuperFamily="+POut.Long(SuperFamilyNum)+") "
        //	+"AND PatStatus!="+POut.Int((int)PatientStatus.Deleted);
        //optimized to 0.001 second runtime on same db
        var command = "SELECT DISTINCT * FROM patient WHERE SuperFamily=" + (SuperFamilyNum)
                                                                          + " AND PatStatus!=" + SOut.Int((int) PatientStatus.Deleted) + " AND PatNum=Guarantor";
        return PatientCrud.TableToList(DataCore.GetTable(command));
    }

    public static void AssignToSuperfamily(long guarantor, long superFamilyNum)
    {
        var command = "UPDATE patient SET SuperFamily=" + (superFamilyNum) + ", HasSuperBilling=1 WHERE Guarantor=" + (guarantor);
        Db.NonQ(command);
    }

    public static void MoveSuperFamily(long oldSuperFamilyNum, long newSuperFamilyNum)
    {
        if (oldSuperFamilyNum == 0) return;
        var command = "UPDATE patient SET SuperFamily=" + newSuperFamilyNum + " WHERE SuperFamily=" + oldSuperFamilyNum;
        Db.NonQ(command);
    }

    public static void DisbandSuperFamily(long SuperFamilyNum)
    {
        if (SuperFamilyNum == 0) return;
        var command = "UPDATE patient SET SuperFamily=0 WHERE SuperFamily=" + (SuperFamilyNum);
        Db.NonQ(command);
    }

    public static List<Patient> GetPatientsForPhi(long patNum)
    {
        var listPatNums = GetPatNumsForPhi(patNum);
        //If there are duplicates in listPatNums, then they will be removed because of the IN statement in the query below.
        var command = $@"SELECT * 
				FROM patient
				WHERE PatStatus IN ({SOut.Enum(PatientStatus.Patient)},{SOut.Enum(PatientStatus.NonPatient)},{SOut.Enum(PatientStatus.Inactive)})
				AND PatNum IN ({string.Join(",", listPatNums)})";
        return PatientCrud.SelectMany(command);
    }

    public static List<long> GetPatNumsForPhi(long patNum)
    {
        var listPatNums = new List<long>();
        listPatNums.Add(patNum);
        var command = "";
        if (PrefC.GetBool(PrefName.FamPhiAccess))
        {
            //Include guarantor's family if pref is set.
            //Include any patient where this PatNum is the Guarantor.
            command = "SELECT PatNum FROM patient WHERE Guarantor = " + (patNum);
            var tablePatientsG = DataCore.GetTable(command);
            for (var i = 0; i < tablePatientsG.Rows.Count; i++) listPatNums.Add(SIn.Long(tablePatientsG.Rows[i]["PatNum"].ToString()));
        }

        //Include any patient where the given patient is the responsible party.
        command = "SELECT PatNum FROM patient WHERE ResponsParty = " + (patNum);
        var tablePatientsR = DataCore.GetTable(command);
        for (var i = 0; i < tablePatientsR.Rows.Count; i++) listPatNums.Add(SIn.Long(tablePatientsR.Rows[i]["PatNum"].ToString()));
        //Include any patient where this patient is the guardian.
        command = "SELECT PatNum FROM patient "
                  + "WHERE PatNum IN (SELECT guardian.PatNumChild FROM guardian WHERE guardian.IsGuardian = 1 AND guardian.PatNumGuardian=" + (patNum) + ") ";
        var tablePatientsD = DataCore.GetTable(command);
        for (var i = 0; i < tablePatientsD.Rows.Count; i++) listPatNums.Add(SIn.Long(tablePatientsD.Rows[i]["PatNum"].ToString()));
        return listPatNums.Distinct().ToList();
    }

    public static List<Patient> GetPatsByEmailAddress(string emailAddress)
    {
        var command = "SELECT * FROM patient WHERE Email LIKE '%" + SOut.String(emailAddress) + "%' "
                      + "AND PatStatus!=" + SOut.Enum(PatientStatus.Archived) + " AND PatStatus!=" + SOut.Enum(PatientStatus.Deleted);
        return PatientCrud.SelectMany(command);
    }

    public static List<Patient> GetAssociatedPatients(long patNum)
    {
        //patients associated to payment plans of which any member of this family is the guarantor UNION patients in the family
        var command = "SELECT pplans.PatNum,pplans.LName,pplans.FName,pplans.MiddleI,pplans.Preferred,pplans.CreditType,pplans.Guarantor,pplans.HasIns,pplans.SSN "
                      + "FROM patient pat "
                      + "LEFT JOIN patient fam ON fam.Guarantor = pat.Guarantor "
                      + "LEFT JOIN payplan ON payplan.Guarantor = fam.PatNum "
                      + "LEFT JOIN patient pplans ON pplans.PatNum = payplan.PatNum "
                      + "WHERE pat.PatNum = " + (patNum) + " "
                      + "AND payplan.IsClosed = 0 "
                      + "GROUP BY pplans.PatNum,pplans.LName,pplans.FName,pplans.MiddleI,pplans.Preferred,pplans.CreditType,pplans.Guarantor,pplans.HasIns,pplans.SSN ";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return [];
        var listPatLims = new List<Patient>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var Lim = new Patient();
            Lim.PatNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
            Lim.LName = SIn.String(table.Rows[i]["LName"].ToString());
            Lim.FName = SIn.String(table.Rows[i]["FName"].ToString());
            Lim.MiddleI = SIn.String(table.Rows[i]["MiddleI"].ToString());
            Lim.Preferred = SIn.String(table.Rows[i]["Preferred"].ToString());
            Lim.CreditType = SIn.String(table.Rows[i]["CreditType"].ToString());
            Lim.Guarantor = SIn.Long(table.Rows[i]["Guarantor"].ToString());
            Lim.HasIns = SIn.String(table.Rows[i]["HasIns"].ToString());
            Lim.SSN = SIn.String(table.Rows[i]["SSN"].ToString());
            listPatLims.Add(Lim);
        }

        return listPatLims;
    }

    public static List<PatComm> GetPatComms(List<long> patNums, ClinicDto clinic, bool isGetFamily = true)
    {
        var retVal = new List<PatComm>();
        if (patNums.Count <= 0) return retVal;
        string command;
        var patNumsSearch = new List<long>(patNums);
        if (isGetFamily)
        {
            command = "SELECT Guarantor FROM patient WHERE PatNum IN (" + string.Join(",", patNumsSearch.Distinct()) + ")";
            patNumsSearch = patNumsSearch.Union(Db.GetListLong(command)).ToList(); //combines and removes duplicates.
        }

        command = "SELECT PatNum, PatStatus, PreferConfirmMethod, PreferContactMethod, PreferRecallMethod, PreferContactConfidential, "
                  + "TxtMsgOk,HmPhone,WkPhone,WirelessPhone,Email,FName,LName,Preferred,Guarantor,Language,Birthdate,Premed,";
        if (clinic != null) command += clinic.Id;
        command += " ClinicNum FROM patient WHERE PatNum IN (" + string.Join(",", patNumsSearch.Distinct()) + ") ";
        var isUnknownNo = PrefC.GetBool(PrefName.TextMsgOkStatusTreatAsNo);
        List<ClinicDto> listAllClinics;
        if (clinic == null)
            listAllClinics = Clinics.GetDeepCopy().Concat([Clinics.GetPracticeAsClinicZero()]).ToList();
        else
            listAllClinics = [clinic];
        var dictEmailValidForClinics = listAllClinics
            .ToDictionary(x => x.Id.ToString(), x => EmailAddresses.GetFirstOrDefault(y => y.EmailAddressNum == x.EmailAddressId) != null);
        var dictTextingEnabledForClinics = listAllClinics.ToDictionary(x => x.Id.ToString(), x => Clinics.IsTextingEnabled(x.Id));
        var curCulture = StringTools.TruncateBeginning(CultureInfo.CurrentCulture.Name, 2);
        var dictClinicCountryCodes = listAllClinics.ToDictionary(x => x.Id.ToString(), x => SmsPhones.GetFirstOrDefault(y => y.ClinicNum == x.Id)?.CountryCode ?? "");
        bool isEmailValidForClinic;
        bool isTextingEnabledForClinic;
        string clinicCountryCode;
        var listPatComms = DataCore.GetTable(command).Select()
            .Select(x => new PatComm(
                x,
                dictEmailValidForClinics.TryGetValue(x["ClinicNum"].ToString(), out isEmailValidForClinic) ? isEmailValidForClinic : false,
                dictTextingEnabledForClinics.TryGetValue(x["ClinicNum"].ToString(), out isTextingEnabledForClinic) ? isTextingEnabledForClinic : false,
                isUnknownNo,
                curCulture,
                dictClinicCountryCodes.TryGetValue(x["ClinicNum"].ToString(), out clinicCountryCode) ? clinicCountryCode : ""
            )).ToList();
        listPatComms = AppendCommOptOuts(listPatComms);
        return listPatComms;
    }

    private static List<PatComm> AppendCommOptOuts(List<PatComm> listPatComms)
    {
        var dictOptOuts = CommOptOuts.GetForPats(listPatComms.Select(x => x.PatNum).ToList())
            .GroupBy(x => x.PatNum)
            .ToDictionary(x => x.Key, y => y.First());
        foreach (var patComm in listPatComms)
            if (dictOptOuts.ContainsKey(patComm.PatNum))
                patComm.CommOptOut = dictOptOuts[patComm.PatNum];

        return listPatComms;
    }

    public static List<PatComm> GetPatComms(List<Patient> listPats)
    {
        var isUnknownNo = PrefC.GetBool(PrefName.TextMsgOkStatusTreatAsNo);
        var curCulture = StringTools.TruncateBeginning(CultureInfo.CurrentCulture.Name, 2);
        var listPatComms = new List<PatComm>();
        foreach (var pat in listPats)
        {
            var clinic = Clinics.GetFirstOrDefault(x => x.Id == pat.ClinicNum) ?? Clinics.GetPracticeAsClinicZero();
            var isEmailValidForClinic = EmailAddresses.GetFirstOrDefault(x => x.EmailAddressNum == clinic.EmailAddressId) != null;
            var isTextingEnabledForClinic = Clinics.IsTextingEnabled(clinic.Id);
            var countryCodePhone = SmsPhones.GetFirstOrDefault(x => x.ClinicNum == clinic.Id)?.CountryCode ?? "";
            listPatComms.Add(new PatComm(pat, isEmailValidForClinic, isTextingEnabledForClinic, isUnknownNo, curCulture, countryCodePhone));
        }

        listPatComms = AppendCommOptOuts(listPatComms);
        return listPatComms;
    }

    public static List<long> GetPatNumMaxForGroups(int numPerGroup, List<PatientStatus> listPatStatuses)
    {
        var retval = new List<long>();
        if (numPerGroup < 1) return retval;
        var whereClause = "";
        if (listPatStatuses != null && listPatStatuses.Count > 0) whereClause = "WHERE PatStatus IN(" + string.Join(",", listPatStatuses.Select(x => SOut.Int((int) x))) + ") ";
        string command;
        long groupMaxPatNum = 0;
        var groupNum = 0;
        do
        {
            if (groupNum > 0) //after first loop, groupMaxPatNum will be set and guaranteed to be >0
                retval.Add(groupMaxPatNum);
            command = "SELECT MAX(PatNum) FROM (SELECT PatNum FROM patient " + whereClause + "ORDER BY PatNum LIMIT " + groupNum + "," + numPerGroup + ") patNumGroup";
            groupMaxPatNum = Db.GetLong(command);
            groupNum += numPerGroup;
        } while (groupMaxPatNum > 0);

        return retval;
    }

    public static List<Patient> GetPatsToChangeStatus(PatientStatus patStatus, DateTime fromDate, bool doIncludeTPProc, bool doIncludeCompletedProc, bool doIncludeAppointments, List<long> listClinicNums)
    {
        var whereClause = "WHERE PatStatus=" + SOut.Int((int) patStatus) + " AND (";
        //A selectedClinicNum of -2 corresponds to clincs not enabled or all clinics
        if (!listClinicNums.IsNullOrEmpty()) whereClause += "ClinicNum IN (" + string.Join(",", listClinicNums.Select(x => (x))) + ") ) AND (";
        if (doIncludeTPProc || doIncludeCompletedProc)
        {
            //TP or completed proc in date range.
            var procstatus = doIncludeTPProc && doIncludeCompletedProc ? "(1,2)" : doIncludeTPProc ? "(1)" : "(2)";
            whereClause += "PatNum NOT IN ("
                           + "SELECT DISTINCT PatNum "
                           + "FROM procedurelog "
                           + "WHERE ProcStatus IN " + procstatus + " "
                           + "AND ProcDate > " + SOut.DateTime(fromDate, true) +
                           ") ";
            if (doIncludeAppointments) //Appt in date range.
                whereClause += "AND ";
        }

        if (doIncludeAppointments) //Appt in date range.
            whereClause += "PatNum NOT IN ("
                           + "SELECT DISTINCT PatNum "
                           + "FROM appointment "
                           + "WHERE AptDateTime > " + SOut.DateTime(fromDate, true)
                           //ONly grabi "valid" appointments so they can be filtered out of the list of inactive patients in the Patient Status Setter tool
                           + " AND AptStatus IN ("
                           + SOut.Enum(ApptStatus.Scheduled) + ","
                           + SOut.Enum(ApptStatus.Complete) + ","
                           + SOut.Enum(ApptStatus.ASAP)
                           + ")"
                           + ")";
        var command = "SELECT PatNum,PatStatus,FName,LName,MiddleI,BirthDate,ClinicNum FROM patient " + whereClause + ") ORDER BY PatNum";
        var table = DataCore.GetTable(command);
        var listPatients = new List<Patient>();
        foreach (DataRow row in table.Rows)
        {
            var patCur = new Patient();
            patCur.PatNum = SIn.Long(row["PatNum"].ToString());
            patCur.PatStatus = (PatientStatus) SIn.Long(row["PatStatus"].ToString());
            patCur.FName = SIn.String(row["FName"].ToString());
            patCur.LName = SIn.String(row["LName"].ToString());
            patCur.Preferred = "";
            patCur.MiddleI = SIn.String(row["MiddleI"].ToString());
            patCur.Birthdate = SIn.Date(row["BirthDate"].ToString());
            patCur.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            listPatients.Add(patCur);
        }

        return listPatients;
    }

    public static string SSNFormatHelper(string patSSN, bool doMask)
    {
        if (string.IsNullOrEmpty(patSSN)) return "";
        //Always display the last four characters.
        if (patSSN.Length <= 4) return patSSN;
        var stringSSN = patSSN;
        //Turn all but the last four characters into x's if the return value should be masked.
        if (doMask) stringSSN = stringSSN.Substring(stringSSN.Length - 4, 4).PadLeft(stringSSN.Length, 'x');
        //Apply the US SSN format of xxx-xx-xxxx when the culture is US, and only containted digits before masking.
        if (CultureInfo.CurrentCulture.Name.EndsWith("US") && stringSSN.Length == 9 && patSSN.All(char.IsDigit)) stringSSN = stringSSN.Substring(0, 3) + "-" + stringSSN.Substring(3, 2) + "-" + stringSSN.Substring(5, 4);
        return stringSSN;
    }

    public static string DOBFormatHelper(DateTime patBirthdate, bool doMask)
    {
        if (patBirthdate.Year < 1880) return ""; //In most places anything older than this (usually minval) is just shown as blank.  Don't bother masking.
        var retval = patBirthdate.ToShortDateString(); //This will take care of localization formatting for us. 
        if (doMask) retval = Regex.Replace(retval, "[0-9]", "x"); //Keep seperator characters, replace all numbers with x
        return retval;
    }

    public static string HashFields(Patient patient)
    {
        var unhashedText = patient.PatNum.ToString();
        try
        {
            return Class1.CreateSaltedHash(unhashedText, true);
        }
        catch (Exception ex)
        {
            return ex.GetType().Name;
        }
    }

    public static bool IsPatientHashValid(Patient patient)
    {
        if (patient == null) return true;
        if (patient.SecurityHash == null) //When a patient is first created through middle tier and not yet refreshed from db, this can be null and should not show a warning triangle.
            return true;
        //We are not checking the date. All patient get hashed.
        if (patient.SecurityHash == HashFields(patient)) return true;
        return false;
    }

    public static List<PatientStatus> GetPatientStatuses(Patient patient)
    {
        var listPatientStatuses = new List<PatientStatus>();
        var stringArrayNames = Enum.GetNames(typeof(PatientStatus));
        for (var i = 0; i < stringArrayNames.Length; i++)
        {
            if ((PatientStatus) i == PatientStatus.Deleted && patient.PatStatus != PatientStatus.Deleted) continue; //Only display 'Deleted' if patient is 'Deleted'.  Shouldn't happen, but has been observed.
            listPatientStatuses.Add((PatientStatus) i);
        }

        return listPatientStatuses;
    }

    public static List<string> GetRelationships(Family family, List<Guardian> listGuardians)
    {
        var listRelationships = new List<string>();
        for (var i = 0; i < listGuardians.Count; i++)
            listRelationships.Add(family.GetNameInFamFirst(listGuardians[i].PatNumGuardian) + " "
                                                                                            + Guardians.GetGuardianRelationshipStr(listGuardians[i].Relationship));
        return listRelationships;
    }

    public static List<string> GetLanguages(Patient patient)
    {
        var listLanguages = new List<string>();
        if (PrefC.GetString(PrefName.LanguagesUsedByPatients) != "")
        {
            var stringArrayLanguages = PrefC.GetString(PrefName.LanguagesUsedByPatients).Split(',');
            for (var i = 0; i < stringArrayLanguages.Length; i++)
            {
                if (stringArrayLanguages[i] == "") continue;
                listLanguages.Add(stringArrayLanguages[i]);
            }
        }

        if (!string.IsNullOrWhiteSpace(patient.Language) && !listLanguages.Contains(patient.Language)) listLanguages.Add(patient.Language);
        return listLanguages;
    }

    public static List<string> GetMultiRaces()
    {
        var listRaces = new List<string>();
        listRaces.Add("None");
        listRaces.Add("AfricanAmerican");
        listRaces.Add("AmericanIndian");
        listRaces.Add("Asian");
        listRaces.Add("DeclinedToSpecify");
        listRaces.Add("HawaiiOrPacIsland");
        listRaces.Add("Other");
        listRaces.Add("White");
        return listRaces;
    }

    public static List<string> GetEthinicities()
    {
        var listEthnicities = new List<string>();
        listEthnicities.Add("None");
        listEthnicities.Add("DeclinedToSpecify");
        listEthnicities.Add("Not Hispanic");
        listEthnicities.Add("Hispanic");
        return listEthnicities;
    }

    public static List<string> GetCanadianEligibilityCodes()
    {
        var listCanadianEligibilityCodes = new List<string>();
        listCanadianEligibilityCodes.Add("0 - Please Choose");
        listCanadianEligibilityCodes.Add("1 - Full-time student");
        listCanadianEligibilityCodes.Add("2 - Disabled");
        listCanadianEligibilityCodes.Add("3 - Disabled student");
        listCanadianEligibilityCodes.Add("4 - Code not applicable");
        return listCanadianEligibilityCodes;
    }

    public static long GetEmployerNumForPatient(Patient patient, string employerName)
    {
        if (patient.EmployerNum == 0)
        {
            //no employer was previously entered.
            if (employerName == "") return patient.EmployerNum; //no change
            return Employers.GetEmployerNum(employerName);
        }

        //an employer was previously entered
        if (employerName == "") return 0;
        //if text has changed
        if (Employers.GetName(patient.EmployerNum) != employerName) return Employers.GetEmployerNum(employerName);
        return patient.EmployerNum; //no change
    }

    public static Result ValidatePatientEdit(Patient patient, Patient patientOld, bool isNew, string site)
    {
        var result = new Result {IsSuccess = false};
        var dateTimeDeceased = DateTime.MinValue;
        if (string.IsNullOrEmpty(patient.LName))
        {
            result.Msg = "Last Name must be entered.";
            return result;
        }

        //see if chartNum is a duplicate
        if (!PrefC.GetBool(PrefName.OmhNy) && !string.IsNullOrEmpty(patient.ChartNumber))
        {
            //the patNum will be 0 for new
            var usedBy = ChartNumUsedBy(patient.ChartNumber, patient.PatNum);
            if (!string.IsNullOrEmpty(usedBy))
            {
                result.Msg = "This chart number is already in use by:";
                result.Msg2 = " " + usedBy;
                return result;
            }
        }

        if (!string.IsNullOrEmpty(patient.County) && !Counties.DoesExist(patient.County))
        {
            result.Msg = "County name invalid. The County entered is not present in the list of Counties. Please add the new County.";
            return result;
        }
        
        if (!string.IsNullOrEmpty(site) && site != Sites.GetDescription(patient.SiteNum) && Sites.FindMatchSiteNum(site) == -1)
        {
            result.Msg = "Invalid Site description.";
            return result;
        }

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
            if (patient.CanadianEligibilityCode == 1 //FT student
                && string.IsNullOrEmpty(patient.SchoolName) && patient.Birthdate.AddYears(18) <= DateTime.Today)
            {
                result.Msg = "School should be entered if full-time student and patient is 18 or older.";
                return result;
            }

        //Don't allow changing status from Archived if this is a merged patient.
        if (patientOld.PatStatus != patient.PatStatus && patientOld.PatStatus == PatientStatus.Archived &&
            PatientLinks.WasPatientMerged(patientOld.PatNum))
        {
            result.Msg = "Not allowed to change the status of a merged patient.";
            return result;
        }

        if (isNew && true)
            if (!PrefC.GetBool(PrefName.ClinicAllowPatientsAtHeadquarters) && patient.ClinicNum == 0)
            {
                result.Msg = "Current settings for clinics do not allow patients to be added to the 'Unassigned' clinic. Please select a clinic.";
                return result;
            }

        result.IsSuccess = true;
        return result;
    }

    public static Result SavePatientEdit(Userod userod, Patient patient, Patient patientOld, PatientNote patientNote, Family family, bool isNew, bool restrictSched, bool arriveEarlySame, bool addressSame, bool addressSameSuperFamily, bool billProvSame, bool notesSame, bool emailPhoneSame, DefLink defLink, long specialtyDefNum, List<PatientRace> listPatientRaces, CommOptOut commOptOut)
    {
        var result = new Result();
        Update(patient, patientOld);
        PatientNotes.Update(patientNote, patient.Guarantor);
        PatientRaces.Reconcile(patient.PatNum, listPatientRaces); //Insert, Update, Delete if needed.
        CommOptOuts.Upsert(commOptOut);
        var strPatPriProvDesc = Providers.GetLongDesc(patient.PriProv);
        InsertPrimaryProviderChangeSecurityLogEntry(patientOld, patient);
        var isApptSchedRestricted = PatRestrictions.IsRestricted(patient.PatNum, PatRestrict.ApptSchedule);
        if (restrictSched)
            PatRestrictions.Upsert(patient.PatNum, PatRestrict.ApptSchedule); //will only insert if one does not already exist in the db.
        else
            PatRestrictions.RemovePatRestriction(patient.PatNum, PatRestrict.ApptSchedule);
        PatRestrictions.InsertPatRestrictApptChangeSecurityLog(patient.PatNum, isApptSchedRestricted, PatRestrictions.IsRestricted(patient.PatNum, PatRestrict.ApptSchedule));

        #region 'Same' Checkboxes

        var isAuthArchivedEdit = Security.IsAuthorized(EnumPermType.ArchivedPatientEdit, MiscData.GetNowDateTime(), true, true, userod);
        if (arriveEarlySame) UpdateArriveEarlyForFam(patient, isAuthArchivedEdit);
        //Only family checked.
        if (addressSame && !addressSameSuperFamily)
            //might want to include a mechanism for comparing fields to be overwritten
            UpdateAddressForFam(patient, false, isAuthArchivedEdit);
        //SuperFamily is checked, family could be checked or unchecked.
        else if (addressSameSuperFamily) UpdateAddressForFam(patient, true, isAuthArchivedEdit);
        if (billProvSame)
        {
            var listPatientsForPriProvEdit = family.ListPats.ToList().FindAll(x => x.PatNum != patient.PatNum && x.PriProv != patient.PriProv);
            if (!isAuthArchivedEdit) //Remove Archived patients if not allowed to edit so we don't create a log for them.
                listPatientsForPriProvEdit.RemoveAll(x => x.PatStatus == PatientStatus.Archived);
            //true if any family member has a different PriProv and the user is authorized for PriProvEdit
            var isChangePriProvs = listPatientsForPriProvEdit.Count > 0 && Security.IsAuthorized(EnumPermType.PatPriProvEdit, DateTime.MinValue, true, true);
            UpdateBillingProviderForFam(patient, isChangePriProvs, isAuthArchivedEdit); //if user is not authorized this will not update PriProvs for fam
        }

        if (notesSame) UpdateNotesForFam(patient, isAuthArchivedEdit);
        if (emailPhoneSame) UpdateEmailPhoneForFam(patient, isAuthArchivedEdit);

        #endregion 'Same' Checkboxes

        if (patient.BillingType != patientOld.BillingType) InsertBillTypeChangeSecurityLogEntry(patientOld, patient);
        //If this patient is also a referral source,
        //keep address info synched:
        var referral = Referrals.GetFirstOrDefault(x => x.PatNum == patient.PatNum);
        if (referral != null)
        {
            referral.LName = patient.LName;
            referral.FName = patient.FName;
            referral.MName = patient.MiddleI;
            referral.Address = patient.Address;
            referral.Address2 = patient.Address2;
            referral.City = patient.City;
            referral.ST = patient.State;
            referral.SSN = patient.SSN;
            referral.Zip = patient.Zip;
            referral.Telephone = TelephoneNumbers.FormatNumbersExactTen(patient.HmPhone);
            referral.EMail = patient.Email;
            Referrals.Update(referral);
            Referrals.RefreshCache();
        }

        //if patient is inactive, deceased, etc., then disable any recalls
        UpdateRecalls(patient, patientOld, "Edit Patient Window");
        //If there is an existing HL7 def enabled, send an ADT message if there is an outbound ADT message defined
        if (HL7Defs.IsExistingHL7Enabled())
        {
            //new patients get the A04 ADT, updating existing patients we send an A08
            MessageHL7 messageHL7 = null;
            if (isNew)
                messageHL7 = MessageConstructor.GenerateADT(patient, GetPat(patient.Guarantor), EventTypeHL7.A04);
            else
                messageHL7 = MessageConstructor.GenerateADT(patient, GetPat(patient.Guarantor), EventTypeHL7.A08);
            //Will be null if there is no outbound ADT message defined, so do nothing
            if (messageHL7 != null)
            {
                var hl7Msg = new HL7Msg();
                hl7Msg.AptNum = 0;
                hl7Msg.HL7Status = HL7MessageStatus.OutPending; //it will be marked outSent by the HL7 service.
                hl7Msg.MsgText = messageHL7.ToString();
                hl7Msg.PatNum = patient.PatNum;
                HL7Msgs.Insert(hl7Msg);
                if ( /* ODBuild.IsDebug() */ false) result.Msg = messageHL7.ToString();
            }
        }

        if (HieClinics.IsEnabled()) HieQueues.Insert(new HieQueue(patient.PatNum));
        if (defLink != null)
        {
            if (specialtyDefNum == 0)
            {
                DefLinks.Delete(defLink.DefLinkNum);
            }
            else
            {
                defLink.DefNum = specialtyDefNum;
                DefLinks.Update(defLink);
            }
        }
        else if (specialtyDefNum != 0)
        {
            //if the patient does not have a specialty and "Unspecified" is not selected. 
            var defLinkNew = new DefLink();
            defLinkNew.DefNum = specialtyDefNum;
            defLinkNew.FKey = patient.PatNum;
            defLinkNew.LinkType = DefLinkType.Patient;
            DefLinks.Insert(defLinkNew);
        }

        if (!isNew)
        {
            InsertAddressChangeSecurityLogEntry(patientOld, patient);
            ODEvent.Fire(ODEventType.Patient, patient);
        }

        result.IsSuccess = true;
        return result;
    }

    public static bool DoPromptForOptInSms(Patient patient, Patient patientOld)
    {
        if (!Clinics.IsTextingEnabled(patient.ClinicNum)) return false; //Office doesn't use texting.
        if (!ClinicPrefs.GetBool(PrefName.ShortCodeOptInOnApptComplete, patient.ClinicNum)) return false; //Office has turned off this prompt.
        if (patient.TxtMsgOk != YN.Yes || string.IsNullOrWhiteSpace(PhoneNumbers.RemoveNonDigitsAndTrimStart(patient.WirelessPhone))) return false; //Not set to YES or no phone number, so no need to send a test message.
        var phoneOldNormalized = PhoneNumbers.RemoveNonDigitsAndTrimStart(patientOld.WirelessPhone);
        var phoneNewNormalized = PhoneNumbers.RemoveNonDigitsAndTrimStart(patient.WirelessPhone);
        if (phoneOldNormalized == phoneNewNormalized && patientOld.TxtMsgOk == YN.Yes) return false; //Phone number hasn't changed and TxtMsgOK was already YES => No changes, no need to prompt.
        return true;
    }

    public static bool DoSendOptOutText(Patient patient, Patient patientOld)
    {
        if (!Clinics.IsTextingEnabled(patient.ClinicNum)) return false; //Office doesn't use texting.
        if (patient.TxtMsgOk != YN.No || string.IsNullOrWhiteSpace(PhoneNumbers.RemoveNonDigitsAndTrimStart(patient.WirelessPhone))) return false; //TxtMsgOk not set to NO or no phone number, so no need to send a text message.
        //we are not checking if phone number changed like in optin because we do not want to send an opt out notification if a patient was previously notified that they were opted out on their old number
        if (patientOld.TxtMsgOk == YN.Unknown || patientOld.TxtMsgOk == YN.No) return false; //TxtMsgOK was already 'No' or not set => No behavioral changes, no need to prompt.
        return PrefC.GetBool(PrefName.TextOptOutSendNotification);
    }

    public static Result SetDefaultRelationships(Patient patient, Family family, PatientPosition patientPositionCur)
    {
        var result = new Result {IsSuccess = false};
        var listPatientsAdults = new List<Patient>();
        var listPatientsChildren = new List<Patient>();
        PatientPosition patientPosition;
        for (var p = 0; p < family.ListPats.Length; p++)
        {
            if (family.ListPats[p].PatNum == patient.PatNum)
                patientPosition = patientPositionCur;
            else
                patientPosition = family.ListPats[p].Position;
            if (patientPosition == PatientPosition.Child)
                listPatientsChildren.Add(family.ListPats[p]);
            else
                listPatientsAdults.Add(family.ListPats[p]);
        }

        Patient patientEldestMaleAdult = null;
        Patient patientEldestFemaleAdult = null;
        for (var i = 0; i < listPatientsAdults.Count; i++)
        {
            if (listPatientsAdults[i].Gender == PatientGender.Male
                && (patientEldestMaleAdult == null || listPatientsAdults[i].Age > patientEldestMaleAdult.Age))
                patientEldestMaleAdult = listPatientsAdults[i];
            if (listPatientsAdults[i].Gender == PatientGender.Female
                && (patientEldestFemaleAdult == null || listPatientsAdults[i].Age > patientEldestFemaleAdult.Age))
                patientEldestFemaleAdult = listPatientsAdults[i];
            //Do not do anything for the other genders.
        }

        if (listPatientsAdults.Count < 1)
        {
            result.Msg = "No adults found.\r\nFamily relationships will not be changed.";
            return result;
        }

        if (listPatientsChildren.Count < 1)
        {
            result.Msg = "No children found.\r\nFamily relationships will not be changed.";
            return result;
        }

        if (patientEldestFemaleAdult == null && patientEldestMaleAdult == null)
        {
            result.Msg = "No male or female adults found.\r\nFamily relationships will not be changed.";
            return result;
        }

        if (Guardians.ExistForFamily(patient.Guarantor))
            //delete all guardians for the family, original family relationships are saved on load so this can be undone if the user presses cancel.
            Guardians.DeleteForFamily(patient.Guarantor);
        for (var i = 0; i < listPatientsChildren.Count; i++)
        {
            if (patientEldestFemaleAdult != null)
            {
                //Create Parent=>Child relationship
                var guardianMother = new Guardian();
                guardianMother.PatNumChild = patientEldestFemaleAdult.PatNum;
                guardianMother.PatNumGuardian = listPatientsChildren[i].PatNum;
                guardianMother.Relationship = GuardianRelationship.Child;
                Guardians.Insert(guardianMother);
                //Create Child=>Parent relationship
                var guardianChild = new Guardian();
                guardianChild.PatNumChild = listPatientsChildren[i].PatNum;
                guardianChild.PatNumGuardian = patientEldestFemaleAdult.PatNum;
                guardianChild.Relationship = GuardianRelationship.Mother;
                guardianChild.IsGuardian = true;
                Guardians.Insert(guardianChild);
            }

            if (patientEldestMaleAdult != null)
            {
                //Create Parent=>Child relationship
                var guardianFather = new Guardian();
                guardianFather.PatNumChild = patientEldestMaleAdult.PatNum;
                guardianFather.PatNumGuardian = listPatientsChildren[i].PatNum;
                guardianFather.Relationship = GuardianRelationship.Child;
                Guardians.Insert(guardianFather);
                //Create Child=>Parent relationship
                var guardianChild = new Guardian();
                guardianChild.PatNumChild = listPatientsChildren[i].PatNum;
                guardianChild.PatNumGuardian = patientEldestMaleAdult.PatNum;
                guardianChild.Relationship = GuardianRelationship.Father;
                guardianChild.IsGuardian = true;
                Guardians.Insert(guardianChild);
            }
        }

        result.IsSuccess = true;
        return result;
    }
    
    public class PatientName
    {
        public string Name;
        public long PatNum;
    }

    public static List<long> GetClonePatNumsAll(long patNum)
    {
        var patNumOriginal = patNum;
        //Figure out if the patNum passed in is in fact the original patient and if it isn't, go get it from the database.
        if (PatientLinks.IsPatientAClone(patNum)) patNumOriginal = PatientLinks.GetOriginalPatNumFromClone(patNum);
        return PatientLinks.GetPatNumsLinkedFromRecursive(patNumOriginal, PatientLinkType.Clone);
    }

    public static Def GetPatientSpecialtyDef(long patNum)
    {
        var command = "SELECT DefNum FROM deflink WHERE LinkType=" + SOut.Int((int) DefLinkType.Patient) + " AND FKey=" + (patNum);
        var defNum = Db.GetLong(command);
        return Defs.GetDef(DefCat.ClinicSpecialty, defNum);
    }

    public static Patient GetOriginalPatientForClone(Patient patCur)
    {
        if (patCur == null || !IsPatientAClone(patCur.PatNum)) return patCur;
        //Go get the master or original patient from the database for the clone patient passed in.
        return GetPat(PatientLinks.GetOriginalPatNumFromClone(patCur.PatNum));
    }

    public static List<Patient> GetPatientsByPhone(string phoneNumber, string countryCode, List<PhoneType> listPhoneTypes = null)
    {
        phoneNumber ??= ""; //Avoid any null reference exceptions.
        //Default to search for the three main phone types.
        if (listPhoneTypes.IsNullOrEmpty()) listPhoneTypes = [PhoneType.WirelessPhone, PhoneType.HmPhone, PhoneType.WkPhone];
        List<Patient> listPats;
        /* Example output:
         * SELECT patient.* FROM patient WHERE patient.PatStatus NOT IN (3,4) AND (patient.WirelessPhone REGEXP <phonenumber> OR patient.HmPhone REGEXP <phonenumber>...)
         * SELECT patient.* FROM patient INNER JOIN phonenumber ON phonenumber.PatNum=patient.PatNum WHERE patient.PatStatus NOT IN (3,4) AND (phonenumber.PhoneNumberDigits=<phonenumber>)
         */
        try
        {
            var select = "SELECT patient.* FROM patient ";
            var join = "";
            var where = $"WHERE patient.PatStatus NOT IN ({SOut.Int((int) PatientStatus.Archived)},{SOut.Int((int) PatientStatus.Deleted)}) ";
            if (PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable))
            {
                //Search the PhoneNumber table
                join = "INNER JOIN phonenumber ON phonenumber.PatNum=patient.PatNum ";
                where += $"AND ({GetPhoneNumberWhereClause(phoneNumber, listPhoneTypes)}) ";
            }
            else
            {
                //Search the Patient table only			
                where += $"AND ({GetPatientPhoneRegexpClause(phoneNumber, countryCode, listPhoneTypes)}) ";
            }

            var groupby = "GROUP BY patient.PatNum";
            var command = select + join + where + groupby;
            listPats = PatientCrud.SelectMany(command);
        }
        catch
        {
            //should only happen if phone number is blank, if so, return empty list below, with appropriate structure 
            listPats = [];
        }

        return listPats;
    }

    private static string GetPhoneNumberWhereClause(string phoneNumber, List<PhoneType> listPhoneTypes)
    {
        var strPhoneDigits = PhoneNumbers.RemoveNonDigitsAndTrimStart(phoneNumber); //Digits only, strip leading 0/1
        var where = $"phonenumber.PhoneNumberDigits='{SOut.String(strPhoneDigits)}' "; //PhoneNumber.PhoneNumberDigits exactly equals RemoveDigitsAndTrimStart()
        var strPhoneTypes = string.Join(",", listPhoneTypes.Select(x => SOut.Int((int) x)));
        where += $"AND phonenumber.PhoneType IN ({strPhoneTypes})"; //For these PhoneTypes
        return where;
    }

    private static string GetPatientPhoneRegexpClause(string phoneNumber, string countryCode, List<PhoneType> listPhoneTypes)
    {
        var listPhoneFields = listPhoneTypes.Select(x => x switch
        {
            PhoneType.WirelessPhone => nameof(Patient.WirelessPhone),
            PhoneType.HmPhone => nameof(Patient.HmPhone),
            PhoneType.WkPhone => nameof(Patient.WkPhone),
            _ => null //No other fields available
        }).Where(x => x != null).ToList();
        var phoneRegexp = ConvertPhoneToRegexp(phoneNumber, countryCode);
        //DO NOT POut THIS REGEX. They have been cleaned for use in this function by ConvertPhoneToRegexp.
        return string.Join(" OR ", listPhoneFields.Select(x => DbHelper.Regexp(SOut.String($"patient.{x}"), phoneRegexp)));
    }

    private static string ConvertPhoneToRegexp(string phoneRaw, string countryCode)
    {
        //Strip all non-numeric characters just in case.
        var retVal = new string(phoneRaw.Where(x => char.IsDigit(x)).ToArray());
        var prefix = "";
        var wildcard = "[^0-9]*"; //any quantity of any non-digit character
        switch (countryCode.ToUpper())
        {
            case "US":
            case "CA":
                //Number prefixed with a country and not prefixed with a country code should both be prefixed with a country code.
                //EG: Both of the following should yield the same 11-digit string... 80012345678, 180012345678 == 180012345678.
                if (retVal.Length == 11 && retVal[0] == '1')
                {
                    //We have an 11-digit number coming in that starts with a 1
                    //Prefix with {0,1} in order to make country code optional.
                    prefix = retVal[0] + "{0,1}" + wildcard;
                    //Remove the first char, which we just included in the prefix above.
                    retVal = retVal.Substring(1);
                }

                break;
        }

        if (string.IsNullOrEmpty(retVal)) throw new Exception("Phone number cannot be blank.");
        //Add back the optional prefix from above and converto a RegEx.
        //Ex. for 1(503)363-5432
        //[^0-9]*1{0,1}[^0-9]*5[^0-9]*0[^0-9]*3[^0-9]*3[^0-9]*6[^0-9]*3[^0-9]*5[^0-9]*4[^0-9]*3[^0-9]*2[^0-9]*
        retVal = wildcard + prefix + string.Join(wildcard, retVal.ToArray()) + wildcard;
        return retVal;
    }

    public static List<PatAging> GetAgingList(long clinicNum = 0)
    {
        var collectionBillType = Defs.GetDefsForCategory(DefCat.BillingTypes, true).FirstOrDefault(x => x.ItemValue.ToLower() == "c")?.DefNum ?? 0;
        var guarAndClinicNum = "";
        var guarClinicJoin = "";
        var guarGroupBy = "GROUP BY p.Guarantor";
        if (true)
        {
            guarAndClinicNum = $@"
					AND guar.ClinicNum={(clinicNum)}";
            guarClinicJoin = $@"
				INNER JOIN patient guar ON p.Guarantor=guar.PatNum{guarAndClinicNum}";
            guarGroupBy = "GROUP BY guar.PatNum";
        }

        var command = $@"SELECT guar.PatNum,guar.Bal_0_30,guar.Bal_31_60,guar.Bal_61_90,guar.BalOver90,guar.BalTotal,guar.InsEst,
				guar.BalTotal-guar.InsEst AS $pat,guar.PayPlanDue,guar.LName,guar.FName,guar.Preferred,guar.MiddleI,guar.PriProv,guar.BillingType,
				guar.ClinicNum,guar.Address,guar.City,guar.State,guar.Zip,guar.Birthdate
				FROM patient guar
				WHERE guar.PatNum=guar.Guarantor{guarAndClinicNum}
				AND guar.PatStatus!={SOut.Int((int) PatientStatus.Deleted)}
				AND (
					guar.PatStatus!={SOut.Int((int) PatientStatus.Archived)}
					OR ABS(guar.BalTotal)>0.005{(collectionBillType == 0 ? "" : $@"
					OR guar.BillingType={(collectionBillType)}")}
				)";
        var dictAll = new Dictionary<long, PatAging>();
        using var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return [];
        foreach (DataRow row in table.Rows)
        {
            var patNum = SIn.Long(row["PatNum"].ToString());
            dictAll[patNum] = new PatAging
            {
                PatNum = patNum,
                Guarantor = patNum,
                Bal_0_30 = SIn.Double(row["Bal_0_30"].ToString()),
                Bal_31_60 = SIn.Double(row["Bal_31_60"].ToString()),
                Bal_61_90 = SIn.Double(row["Bal_61_90"].ToString()),
                BalOver90 = SIn.Double(row["BalOver90"].ToString()),
                BalTotal = SIn.Double(row["BalTotal"].ToString()),
                InsEst = SIn.Double(row["InsEst"].ToString()),
                AmountDue = SIn.Double(row["$pat"].ToString()),
                PayPlanDue = SIn.Double(row["PayPlanDue"].ToString()),
                PatName = GetNameLF(SIn.String(row["LName"].ToString()), SIn.String(row["FName"].ToString()),
                    SIn.String(row["Preferred"].ToString()), SIn.String(row["MiddleI"].ToString())),
                PriProv = SIn.Long(row["PriProv"].ToString()),
                BillingType = SIn.Long(row["BillingType"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                Address = SIn.String(row["Address"].ToString()),
                City = SIn.String(row["City"].ToString()),
                State = SIn.String(row["State"].ToString()),
                Zip = SIn.String(row["Zip"].ToString()),
                Birthdate = SIn.Date(row["Birthdate"].ToString()),
                //the following values will be set below, if applicable
                ListTsiLogs = [],
                DateLastPay = DateTime.MinValue,
                HasInsPending = false,
                DateLastProc = DateTime.MinValue,
                HasUnsentProcs = false,
                DateBalBegan = DateTime.MinValue
            };
        }

        TsiTransLogs.SelectMany(dictAll.Keys.ToList())
            .GroupBy(x => x.PatNum)
            .ForEach(x => dictAll[x.Key].ListTsiLogs = x.OrderByDescending(y => y.TransDateTime).ToList());
        command = $@"SELECT p.Guarantor,MAX(p.DatePay) DateLastPay
				FROM (
					SELECT patient.Guarantor,MAX(paysplit.DatePay) DatePay
					FROM paysplit
					INNER JOIN payment ON payment.PayNum=paysplit.PayNum
					INNER JOIN patient ON paysplit.PatNum=patient.PatNum
					WHERE payment.PayType!=0
					GROUP BY paysplit.PayNum,patient.Guarantor
					HAVING SUM(paysplit.SplitAmt)!=0
					ORDER BY NULL
				) p{guarClinicJoin}
				{guarGroupBy}
				ORDER BY NULL";
        using var tableDateLastPay = DataCore.GetTable(command);
        foreach (DataRow row in tableDateLastPay.Rows)
        {
            var guarNum = SIn.Long(row["Guarantor"].ToString());
            if (!dictAll.ContainsKey(guarNum)) continue;
            dictAll[guarNum].DateLastPay = SIn.Date(row["DateLastPay"].ToString());
        }

        command = $@"SELECT DISTINCT p.Guarantor
				FROM patient p{guarClinicJoin}
				INNER JOIN claim ON p.PatNum=claim.PatNum
					AND claim.ClaimStatus IN ('U','H','I','W','S')
					AND claim.ClaimType IN ('P','S','Other')";
        Db.GetListLong(command).FindAll(x => dictAll.ContainsKey(x)).ForEach(x => dictAll[x].HasInsPending = true);
        command = $@"SELECT p.Guarantor,MAX(procedurelog.ProcDate) MaxProcDate
				FROM patient p{guarClinicJoin}
				INNER JOIN procedurelog ON procedurelog.PatNum=p.PatNum
				WHERE procedurelog.ProcFee>0
				AND procedurelog.ProcStatus=2
				{guarGroupBy}
				ORDER BY NULL";
        using var tableMaxProcDate = DataCore.GetTable(command);
        foreach (DataRow row in tableMaxProcDate.Rows)
        {
            var guarNum = SIn.Long(row["Guarantor"].ToString());
            if (!dictAll.ContainsKey(guarNum)) continue;
            dictAll[guarNum].DateLastProc = SIn.Date(row["MaxProcDate"].ToString());
        }

        command = $@"SELECT DISTINCT p.Guarantor
				FROM patient p{guarClinicJoin}
				INNER JOIN procedurelog ON procedurelog.PatNum=p.PatNum
				INNER JOIN claimproc ON claimproc.ProcNum=procedurelog.ProcNum
				WHERE procedurelog.ProcFee>0
				AND procedurelog.ProcStatus=2
				AND procedurelog.ProcDate>CURDATE()-INTERVAL 6 MONTH
				AND claimproc.NoBillIns=0
				AND claimproc.Status=6";
        Db.GetListLong(command).FindAll(x => dictAll.ContainsKey(x)).ForEach(x => dictAll[x].HasUnsentProcs = true);
        return dictAll.Values.ToList();
    }

    public static void SetDateBalBegan(long clinicNum, ref List<PatAging> listPatAgingAll, ref List<ClinicBalBegans> listClinicBalBegans)
    {
        var dictAll = listPatAgingAll.ToDictionary(x => x.PatNum);
        if (!listClinicBalBegans.Any(x => x.ClinicNum == clinicNum)) listClinicBalBegans.Add(new ClinicBalBegans(clinicNum, Ledgers.GetDateBalanceBegan(clinicNum))); //uses today's date, doesn't consider super families
        var dictDateBals = listClinicBalBegans.First(x => x.ClinicNum == clinicNum).DictGuarDateBals; //guaranteed to contain clinicNum from above
        foreach (var patNum in dictAll.Keys)
        {
            if (!dictDateBals.ContainsKey(patNum)) continue;
            dictAll[patNum].DateBalBegan = dictDateBals[patNum];
        }
    }

    public static List<long> GetListCollectionGuarNums(bool doIncludeSuspended = true)
    {
        var listBillTypes = Defs.GetDefsForCategory(DefCat.BillingTypes, true).FindAll(x => x.ItemValue.ToLower() == "c");
        var listSuspendedGuarNums = new List<long>();
        if (doIncludeSuspended) listSuspendedGuarNums = TsiTransLogs.GetSuspendedGuarNums();
        if (listBillTypes.Count == 0) return listSuspendedGuarNums; //no collection billing type, return suspended guar nums, could be empty
        var command = "SELECT patient.Guarantor "
                      + "FROM patient "
                      + "WHERE patient.PatNum=patient.Guarantor "
                      + "AND patient.BillingType IN (" + string.Join(",", listBillTypes.Select(x => (x.DefNum))) + ")";
        return Db.GetListLong(command).Union(listSuspendedGuarNums).ToList();
    }

    public static bool IsGuarCollections(long guarNum, bool includeSuspended = true)
    {
        if (includeSuspended && TsiTransLogs.IsGuarSuspended(guarNum)) return true;
        var billTypeColl = Defs.GetDefsForCategory(DefCat.BillingTypes, true).FirstOrDefault(x => x.ItemValue.ToLower() == "c");
        if (billTypeColl == null) return false; //if not suspended and no billing type marked as collection billing type, return false, guar not a collection guar
        var command = "SELECT 1 isGuarCollection "
                      + "FROM patient "
                      + "WHERE PatNum=" + (guarNum) + " "
                      + "AND PatNum=Guarantor "
                      + "AND BillingType=" + (billTypeColl.DefNum) + " "
                      + DbHelper.LimitAnd(1);
        return SIn.Bool(DataCore.GetScalar(command));
    }

    public static List<long> GetGuarantorsForPatNums(List<long> listPatNums)
    {
        if (listPatNums.IsNullOrEmpty()) return [];

        //If two patients in the same family are passed in, it will still only return that families guarantor once.
        var command = "SELECT DISTINCT Guarantor FROM patient WHERE PatNum IN (" + string.Join(",", listPatNums.Select(x => (x))) + ") ";
        return Db.GetListLong(command);
    }

    public static List<long> GetGuarantorsWithFinancialDataChangedAfterDate(DateTime dateChanged)
    {
        //Find all of the PatNums that have had financial data changed since the specified date (at midnight).
        //Utilize POut.Date instead of POut.DateT so that the time portion is excluded thus allowing the changes on the specified date to be returned.
        //E.g. an adjustment with a SecDateTEdit of 2016-04-19 17:38:36 will be returned when SecDateTEdit > '2016-04-19' is used within the WHERE clause.
        //Also, utilize UNION instead of UNION ALL because we do not want duplicates to be returned.
        var command = $@"SELECT DISTINCT PatNum FROM adjustment WHERE SecDateTEdit > {SOut.Date(dateChanged)}
				UNION
				SELECT DISTINCT PatNum FROM claimproc WHERE SecDateTEdit > {SOut.Date(dateChanged)}
				UNION
				SELECT DISTINCT PatNum FROM payplancharge WHERE SecDateTEdit > {SOut.Date(dateChanged)}
				UNION
				SELECT DISTINCT PatNum FROM paysplit WHERE SecDateTEdit > {SOut.Date(dateChanged)}
				UNION
				SELECT DISTINCT PatNum FROM procedurelog WHERE DateTStamp > {SOut.Date(dateChanged)}";
        var listPatNums = Db.GetListLong(command);
        //Return a list of the guarantors for the PatNums.
        return GetGuarantorsForPatNums(listPatNums);
    }

    public static List<long> GetAllGuarantors()
    {
        var command = $"SELECT DISTINCT Guarantor FROM patient WHERE patient.PatStatus!={SOut.Int((int) PatientStatus.Deleted)}";
        return Db.GetListLong(command);
    }

    public static Patient GetGuarForPat(long patNum)
    {
        if (patNum == 0) return null;

        var command = $@"SELECT * FROM patient guar
				WHERE guar.PatNum=(SELECT patient.Guarantor FROM patient WHERE patient.PatNum={(patNum)})";
        return PatientCrud.SelectOne(command);
    }

    public static Patient CreateCloneAndSynch(Patient patient, Family familyCur = null, List<InsPlan> listInsPlans = null, List<InsSub> listInsSubs = null, List<Benefit> listBenefits = null, long primaryProvNum = 0, long clinicNum = 0)
    {
        var patientSynch = CreateClone(patient, primaryProvNum, clinicNum);
        SynchCloneWithPatient(patient, patientSynch, familyCur, listInsPlans, listInsSubs, listBenefits);
        return patientSynch;
    }

    public static Patient CreateClone(Patient patient, long primaryProvNum = 0, long clinicNum = 0)
    {
        var patientSynch = new Patient();
        patientSynch.LName = patient.LName.ToUpper();
        patientSynch.FName = patient.FName.ToUpper();
        patientSynch.MiddleI = patient.MiddleI.ToUpper();
        patientSynch.Birthdate = patient.Birthdate;
        //PriPro is intentionally not synched so the clone can be assigned to a different provider for tracking production.
        if (primaryProvNum == 0) primaryProvNum = PrefC.GetLong(PrefName.PracticeDefaultProv);
        patientSynch.PriProv = primaryProvNum;
        patientSynch.ClinicNum = clinicNum;
        Insert(patientSynch);
        SecurityLogs.MakeLogEntry(EnumPermType.PatientCreate, patientSynch.PatNum, Lans.g("ContrFamily", "Created from Family Module Clones Add button."));
        PatientLinks.Insert(new PatientLink
        {
            PatNumFrom = patient.PatNum,
            PatNumTo = patientSynch.PatNum,
            LinkType = PatientLinkType.Clone
        });

        #region Family / Super Family

        //Go get the clone from the database so that fields will be refreshed to their non-null values, i.e. '' instead of null
        patientSynch = GetPat(patientSynch.PatNum);
        var patientSynchOld = patientSynch.Copy();
        //Now that the clone has been inserted and has a primary key we can consider what family and/or super family the clone should be part of.
        if (PrefC.GetBool(PrefName.CloneCreateSuperFamily))
        {
            //Put the clone into their own family.
            patientSynch.Guarantor = patientSynch.PatNum;
            //But then put the clone into the super family of the master (creating one if the master isn't already part of a super family).
            var superFamilyNum = patient.SuperFamily;
            if (superFamilyNum < 1)
            {
                //Forcefully create a new super family, make the master patient the super family head, and then put the clone into that super family.
                AssignToSuperfamily(patient.Guarantor, patient.Guarantor); //Moves other family members into the super family as well.
                superFamilyNum = patient.Guarantor;
            }

            //Do the guts of what AssignToSuperfamily() would have done but for our patientSynch object so that we save a db call.
            patientSynch.HasSuperBilling = true;
            patientSynch.SuperFamily = superFamilyNum;
        }
        else
        {
            //The preference to force using super families is off so we will only put the clone into a super family if the original is part of one.
            patientSynch.Guarantor = patient.Guarantor;
            patientSynch.SuperFamily = patient.SuperFamily;
        }

        //Save any family or super family changes to the database.  Other family members would have already been affected by this point.
        Update(patientSynch, patientSynchOld);

        #endregion

        return patientSynch;
    }

    public static string SynchClonesWithPatient(Patient patient, Family familyCur = null, List<InsPlan> listInsPlans = null, List<InsSub> listInsSubs = null, List<Benefit> listBenefits = null, List<PatPlan> listPatPlans = null)
    {
        var stringBuilder = new StringBuilder();
        //Get all clones for the patient passed in and then synch each one and return a string regarding what happened during the synch.
        var patNumOriginal = patient.PatNum;
        //Figure out if the patNum passed in is in fact the original patient and if it isn't, go get it from the database.
        if (PatientLinks.IsPatientAClone(patient.PatNum)) patNumOriginal = PatientLinks.GetOriginalPatNumFromClone(patient.PatNum);
        //Now that we know the PatNum of the original or master patient we can get all corresponding clones.
        var listPatNumClones = PatientLinks.GetPatNumsLinkedFromRecursive(patNumOriginal, PatientLinkType.Clone);
        //We now have every single clone PatNum but need to remove the one that is associated to patient so that we don't synch it.
        listPatNumClones.RemoveAll(x => x == patient.PatNum);
        var arraySynchPatients = GetMultPats(listPatNumClones);
        //Loop through all remaining clones and synch them with the patient that was passed in.
        foreach (var patientSynch in arraySynchPatients)
        {
            var changes = SynchCloneWithPatient(patient, patientSynch, familyCur, listInsPlans, listInsSubs, listBenefits, listPatPlans);
            if (!string.IsNullOrWhiteSpace(changes))
                stringBuilder.AppendLine(Lans.g("ContrFamily", "The following changes were made to the patient")
                                         + " " + patientSynch.PatNum + " - " + GetNameFL(patientSynch.LName, patientSynch.FName, patientSynch.Preferred, patientSynch.MiddleI)
                                         + ":\r\n" + changes);
        }

        return stringBuilder.ToString();
    }

    public static string SynchCloneWithPatient(Patient patient, Patient patientSynch, Family familyCur = null, List<InsPlan> listInsPlans = null, List<InsSub> listInsSubs = null, List<Benefit> listBenefits = null, List<PatPlan> listPatPlans = null, List<PatPlan> listPatPlansForSynch = null)
    {
        var patCloneOld = patientSynch.Copy();
        var patientCloneDemoChanges = SynchCloneDemographics(patient, patientSynch);
        if (Update(patientSynch, patCloneOld)
            && PrefC.GetBool(PrefName.PatientPhoneUsePhonenumberTable)
            && patientCloneDemoChanges.ListFieldsCleared
                .Union(patientCloneDemoChanges.ListFieldsUpdated.Select(y => y.FieldName))
                .Any(x => x.In("Home Phone", "Wireless Phone", "Work Phone")))
            PhoneNumbers.SyncPat(patientSynch);
        InsertBillTypeChangeSecurityLogEntry(patCloneOld, patientSynch);
        var strDataUpdated = "";
        var strChngFrom = " " + Lans.g("ContrFamily", "changed from") + " ";
        var strChngTo = " " + Lans.g("ContrFamily", "to") + " ";
        var strBlank = Lans.g("ContrFamily", "blank");
        foreach (var patientCloneField in patientCloneDemoChanges.ListFieldsUpdated)
        {
            strDataUpdated += Lans.g("ContrFamily", patientCloneField.FieldName) + strChngFrom;
            strDataUpdated += string.IsNullOrEmpty(patientCloneField.OldValue) ? strBlank : patientCloneField.OldValue;
            strDataUpdated += strChngTo;
            strDataUpdated += string.IsNullOrEmpty(patientCloneField.NewValue) ? strBlank : patientCloneField.NewValue;
            strDataUpdated += "\r\n";
        }

        if (familyCur == null) familyCur = GetFamily(patient.PatNum);
        if (listInsSubs == null) listInsSubs = InsSubs.RefreshForFam(familyCur);
        if (listInsPlans == null) listInsPlans = InsPlans.RefreshForSubList(listInsSubs);
        if (listBenefits == null) listBenefits = Benefits.Refresh(PatPlans.Refresh(patient.PatNum), listInsSubs);
        var patientClonePatPlanChanges = SynchClonePatPlans(patient, patientSynch, familyCur, listInsPlans, listInsSubs, listBenefits
            , listPatPlans, listPatPlansForSynch);
        strDataUpdated += patientClonePatPlanChanges.StrDataUpdated;
        return strDataUpdated;
    }

    private static PatientCloneDemographicChanges SynchCloneDemographics(Patient patient, Patient patientSynch)
    {
        var patientCloneDemoChanges = new PatientCloneDemographicChanges();
        //We allow users to synch clones to clones now.  Therefore, we need to always go to the database and figure out the PatNum of the original.
        var patNumOriginal = patient.PatNum;
        if (PatientLinks.IsPatientAClone(patient.PatNum))
            //The patient that is going to synch its settings to patientSynch must be a clone, go get the PatNum of the original patient.
            patNumOriginal = PatientLinks.GetOriginalPatNumFromClone(patient.PatNum);
        var isSynchTheMaster = patientSynch.PatNum == patNumOriginal;

        #region Synch Clone Data - Patient Demographics

        patientCloneDemoChanges.ListFieldsUpdated = [];
        patientCloneDemoChanges.ListFieldsCleared = [];
        if (patientSynch.FName.ToLower() != patient.FName.ToLower())
        {
            if (patientSynch.FName != "" && patient.FName == "") patientCloneDemoChanges.ListFieldsCleared.Add("First Name");
            var fName = patient.FName.ToUpper();
            if (isSynchTheMaster) //We are synching a clone to the master, do NOT update the master's field to all caps.
                fName = StringTools.ToUpperFirstOnly(patient.FName);
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("First Name", patientSynch.FName, fName));
            patientSynch.FName = fName;
        }

        if (patientSynch.LName.ToLower() != patient.LName.ToLower())
        {
            if (patientSynch.LName != "" && patient.LName == "") patientCloneDemoChanges.ListFieldsCleared.Add("Last Name");
            var lName = patient.LName.ToUpper();
            if (isSynchTheMaster) //We are synching a clone to the master, do NOT update the master's field to all caps.
                lName = StringTools.ToUpperFirstOnly(patient.LName);
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Last Name", patientSynch.LName, lName));
            patientSynch.LName = lName;
        }

        if (patientSynch.Title != patient.Title)
        {
            if (patientSynch.Title != "" && patient.Title == "") patientCloneDemoChanges.ListFieldsCleared.Add("Title");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Title", patientSynch.Title, patient.Title));
            patientSynch.Title = patient.Title;
        }

        if (patientSynch.Preferred.ToLower() != patient.Preferred.ToLower())
        {
            if (patientSynch.Preferred != "" && patient.Preferred == "") patientCloneDemoChanges.ListFieldsCleared.Add("Preferred Name");
            var preferred = patient.Preferred.ToUpper();
            if (isSynchTheMaster) //We are synching a clone to the master, do NOT update the master's field to all caps.
                preferred = StringTools.ToUpperFirstOnly(patient.Preferred);
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Preferred Name", patientSynch.Preferred, preferred));
            patientSynch.Preferred = preferred;
        }

        if (patientSynch.MiddleI.ToLower() != patient.MiddleI.ToLower())
        {
            if (patientSynch.MiddleI != "" && patient.MiddleI == "") patientCloneDemoChanges.ListFieldsCleared.Add("Middle Initial");
            var middleI = patient.MiddleI.ToUpper();
            if (isSynchTheMaster) //We are synching a clone to the master, do NOT update the master's field to all caps.
                middleI = StringTools.ToUpperFirstOnly(patient.MiddleI);
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Middle Initial", patientSynch.MiddleI, middleI));
            patientSynch.MiddleI = middleI;
        }

        if (patientSynch.Birthdate != patient.Birthdate)
        {
            if (patientSynch.Birthdate.Year > 1880 && patient.Birthdate.Year < 1880) patientCloneDemoChanges.ListFieldsCleared.Add("Birthdate");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Birthdate", patientSynch.Birthdate.ToShortDateString(), patient.Birthdate.ToShortDateString()));
            patientSynch.Birthdate = patient.Birthdate;
        }

        //As of v17.2, it is desirable to allow patient clones to be in different families... whatever.
        //if(patientSynch.Guarantor!=patient.Guarantor) {
        //	Patient patCloneGuar=Patients.GetPat(patientSynch.Guarantor);
        //	Patient patNonCloneGuar=Patients.GetPat(patient.Guarantor);
        //	string strPatCloneGuarName="";
        //	string strPatNonCloneGuarName="";
        //	if(patCloneGuar!=null) {
        //		strPatCloneGuarName=Patients.GetNameFL(patCloneGuar.LName,patCloneGuar.FName,patCloneGuar.Preferred,patCloneGuar.MiddleI);
        //	}
        //	if(patNonCloneGuar!=null) {
        //		strPatNonCloneGuarName=Patients.GetNameFL(patNonCloneGuar.LName,patNonCloneGuar.FName,patNonCloneGuar.Preferred,patNonCloneGuar.MiddleI);
        //	}
        //	patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Guarantor",patientSynch.Guarantor.ToString()+" - "+strPatCloneGuarName,patient.Guarantor.ToString()+" - "+strPatNonCloneGuarName));
        //	patientSynch.Guarantor=patient.Guarantor;
        //}
        if (patientSynch.ResponsParty != patient.ResponsParty)
        {
            var patCloneRespPart = GetPat(patientSynch.ResponsParty);
            var patNonCloneRespPart = GetPat(patient.ResponsParty);
            var strPatCloneRespPartName = "";
            var strPatNonCloneRespPartName = "";
            if (patCloneRespPart != null) strPatCloneRespPartName = GetNameFL(patCloneRespPart.LName, patCloneRespPart.FName, patCloneRespPart.Preferred, patCloneRespPart.MiddleI);
            if (patNonCloneRespPart != null) strPatNonCloneRespPartName = GetNameFL(patNonCloneRespPart.LName, patNonCloneRespPart.FName, patNonCloneRespPart.Preferred, patNonCloneRespPart.MiddleI);
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Responsible Party", patientSynch.ResponsParty + " - " + strPatCloneRespPartName, patient.ResponsParty + " - " + strPatNonCloneRespPartName));
            patientSynch.ResponsParty = patient.ResponsParty;
        }

        //As of v17.2, it is desirable to allow patient clones to be in different super families... whatever.
        //if(patientSynch.SuperFamily!=patient.SuperFamily) {
        //	Patient patCloneSupFam=Patients.GetPat(patientSynch.SuperFamily);
        //	Patient patNonCloneSupFam=Patients.GetPat(patient.SuperFamily);
        //	string strPatCloneSupFamName="";
        //	string strPatNonCloneSupFamName="";
        //	if(patCloneSupFam!=null) {
        //		strPatCloneSupFamName=Patients.GetNameFL(patCloneSupFam.LName,patCloneSupFam.FName,patCloneSupFam.Preferred,patCloneSupFam.MiddleI);
        //	}
        //	if(patNonCloneSupFam!=null) {
        //		strPatNonCloneSupFamName=Patients.GetNameFL(patNonCloneSupFam.LName,patNonCloneSupFam.FName,patNonCloneSupFam.Preferred,patNonCloneSupFam.MiddleI);
        //	}
        //	patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Super Family",patientSynch.SuperFamily.ToString()+" - "+strPatCloneSupFamName,patient.SuperFamily.ToString()+" - "+strPatNonCloneSupFamName ));
        //	patientSynch.SuperFamily=patient.SuperFamily;
        //}
        if (patientSynch.PatStatus != patient.PatStatus)
        {
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Patient Status", patientSynch.PatStatus.ToString(), patient.PatStatus.ToString()));
            patientSynch.PatStatus = patient.PatStatus;
        }

        if (patientSynch.Gender != patient.Gender)
        {
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Gender", patientSynch.Gender.ToString(), patient.Gender.ToString()));
            patientSynch.Gender = patient.Gender;
        }

        if (patientSynch.Language != patient.Language)
        {
            var strPatCloneLang = "";
            var strPatNonCloneLang = "";
            try
            {
                strPatCloneLang = MiscUtils.GetCultureFromThreeLetter(patientSynch.Language).DisplayName;
            }
            catch
            {
                strPatCloneLang = patientSynch.Language;
            }

            try
            {
                strPatNonCloneLang = MiscUtils.GetCultureFromThreeLetter(patient.Language).DisplayName;
            }
            catch
            {
                strPatNonCloneLang = patient.Language;
            }

            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Language", strPatCloneLang, strPatNonCloneLang));
            patientSynch.Language = patient.Language;
        }

        if (patientSynch.SSN != patient.SSN)
        {
            if (patientSynch.SSN != "" && patient.SSN == "") patientCloneDemoChanges.ListFieldsCleared.Add("SSN");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("SSN", patientSynch.SSN, patient.SSN));
            patientSynch.SSN = patient.SSN;
        }

        if (patientSynch.Position != patient.Position)
        {
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Position", patientSynch.Position.ToString(), patient.Position.ToString()));
            patientSynch.Position = patient.Position;
        }

        if (patientSynch.Address != patient.Address)
        {
            if (patientSynch.Address != "" && patient.Address == "") patientCloneDemoChanges.ListFieldsCleared.Add("Address");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Address", patientSynch.Address, patient.Address));
            patientSynch.Address = patient.Address;
        }

        if (patientSynch.Address2 != patient.Address2)
        {
            if (patientSynch.Address2 != "" && patient.Address2 == "") patientCloneDemoChanges.ListFieldsCleared.Add("Address2");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Address2", patientSynch.Address2, patient.Address2));
            patientSynch.Address2 = patient.Address2;
        }

        if (patientSynch.City != patient.City)
        {
            if (patientSynch.City != "" && patient.City == "") patientCloneDemoChanges.ListFieldsCleared.Add("City");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("City", patientSynch.City, patient.City));
            patientSynch.City = patient.City;
        }

        if (patientSynch.State != patient.State)
        {
            if (patientSynch.State != "" && patient.State == "") patientCloneDemoChanges.ListFieldsCleared.Add("State");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("State", patientSynch.State, patient.State));
            patientSynch.State = patient.State;
        }

        if (patientSynch.Zip != patient.Zip)
        {
            if (patientSynch.Zip != "" && patient.Zip == "") patientCloneDemoChanges.ListFieldsCleared.Add("Zip");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Zip", patientSynch.Zip, patient.Zip));
            patientSynch.Zip = patient.Zip;
        }

        if (patientSynch.County != patient.County)
        {
            if (patientSynch.County != "" && patient.County == "") patientCloneDemoChanges.ListFieldsCleared.Add("County");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("County", patientSynch.County, patient.County));
            patientSynch.County = patient.County;
        }

        if (patientSynch.AddrNote != patient.AddrNote)
        {
            if (patientSynch.AddrNote != "" && patient.AddrNote == "") patientCloneDemoChanges.ListFieldsCleared.Add("Address Note");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Address Note", patientSynch.AddrNote, patient.AddrNote));
            patientSynch.AddrNote = patient.AddrNote;
        }

        if (patientSynch.HmPhone != patient.HmPhone)
        {
            if (patientSynch.HmPhone != "" && patient.HmPhone == "") patientCloneDemoChanges.ListFieldsCleared.Add("Home Phone");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Home Phone", patientSynch.HmPhone, patient.HmPhone));
            patientSynch.HmPhone = patient.HmPhone;
        }

        if (patientSynch.WirelessPhone != patient.WirelessPhone)
        {
            if (patientSynch.WirelessPhone != "" && patient.WirelessPhone == "") patientCloneDemoChanges.ListFieldsCleared.Add("Wireless Phone");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Wireless Phone", patientSynch.WirelessPhone, patient.WirelessPhone));
            patientSynch.WirelessPhone = patient.WirelessPhone;
        }

        if (patientSynch.WkPhone != patient.WkPhone)
        {
            if (patientSynch.WkPhone != "" && patient.WkPhone == "") patientCloneDemoChanges.ListFieldsCleared.Add("Work Phone");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Work Phone", patientSynch.WkPhone, patient.WkPhone));
            patientSynch.WkPhone = patient.WkPhone;
        }

        if (patientSynch.Email != patient.Email)
        {
            if (patientSynch.Email != "" && patient.Email == "") patientCloneDemoChanges.ListFieldsCleared.Add("Email");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Email", patientSynch.Email, patient.Email));
            patientSynch.Email = patient.Email;
        }

        if (patientSynch.TxtMsgOk != patient.TxtMsgOk)
        {
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("TxtMsgOk", patientSynch.TxtMsgOk.ToString(), patient.TxtMsgOk.ToString()));
            patientSynch.TxtMsgOk = patient.TxtMsgOk;
        }

        if (patientSynch.BillingType != patient.BillingType)
        {
            var defCloneBillingType = Defs.GetDef(DefCat.BillingTypes, patientSynch.BillingType);
            var defNonCloneBillingType = Defs.GetDef(DefCat.BillingTypes, patient.BillingType);
            var cloneBillType = defCloneBillingType == null ? "" : defCloneBillingType.ItemName;
            var nonCloneBillType = defNonCloneBillingType == null ? "" : defNonCloneBillingType.ItemName;
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Billing Type", cloneBillType, nonCloneBillType));
            patientSynch.BillingType = patient.BillingType;
        }

        if (patientSynch.FeeSched != patient.FeeSched)
        {
            var cloneFeeSchedObj = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == patientSynch.FeeSched);
            var nonCloneFeeSchedObj = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == patient.FeeSched);
            var cloneFeeSched = cloneFeeSchedObj == null ? "" : cloneFeeSchedObj.Description;
            var nonCloneFeeSched = nonCloneFeeSchedObj == null ? "" : nonCloneFeeSchedObj.Description;
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Fee Schedule", cloneFeeSched, nonCloneFeeSched));
            patientSynch.FeeSched = patient.FeeSched;
        }

        if (patientSynch.CreditType != patient.CreditType)
        {
            if (patientSynch.CreditType != "" && patient.CreditType == "") patientCloneDemoChanges.ListFieldsCleared.Add("Credit Type");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Credit Type", patientSynch.CreditType, patient.CreditType));
            patientSynch.CreditType = patient.CreditType;
        }

        if (patientSynch.MedicaidID != patient.MedicaidID)
        {
            if (patientSynch.MedicaidID != "" && patient.MedicaidID == "") patientCloneDemoChanges.ListFieldsCleared.Add("Medicaid ID");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Medicaid ID", patientSynch.MedicaidID, patient.MedicaidID));
            patientSynch.MedicaidID = patient.MedicaidID;
        }

        if (patientSynch.MedUrgNote != patient.MedUrgNote)
        {
            if (patientSynch.MedUrgNote != "" && patient.MedUrgNote == "") patientCloneDemoChanges.ListFieldsCleared.Add("Medical Urgent Note");
            patientCloneDemoChanges.ListFieldsUpdated.Add(new PatientCloneField("Medical Urgent Note", patientSynch.MedUrgNote, patient.MedUrgNote));
            patientSynch.MedUrgNote = patient.MedUrgNote;
        }

        #endregion Synch Clone Data - Patient Demographics

        return patientCloneDemoChanges;
    }

    public static PatientClonePatPlanChanges SynchClonePatPlans(Patient patient, Patient patientSynch, Family familyCur, List<InsPlan> listInsPlans, List<InsSub> listInsSubs, List<Benefit> listBenefits, List<PatPlan> listPatPlans = null, List<PatPlan> listPatPlansForSynch = null)
    {
        //TODO: correct all messages so that they don't refer to "the clone" or "the original".
        var patientClonePatPlanChanges = new PatientClonePatPlanChanges();

        #region Synch Clone Data - PatPlans

        patientClonePatPlanChanges.PatPlansChanged = false;
        patientClonePatPlanChanges.PatPlansInserted = false;
        patientClonePatPlanChanges.StrDataUpdated = "";
        if (listPatPlans == null) listPatPlans = PatPlans.Refresh(patient.PatNum); //ordered by ordinal
        if (listPatPlansForSynch == null) listPatPlansForSynch = PatPlans.Refresh(patientSynch.PatNum); //ordered by ordinal
        var claimList = Claims.Refresh(patientSynch.PatNum); //used to determine if the patplan we are going to drop is attached to a claim with today's date
        for (var i = claimList.Count - 1; i > -1; i--)
        {
            //remove any claims that do not have a date of today, we are only concerned with claims with today's date
            if (claimList[i].DateService == DateTime.Today) continue;
            claimList.RemoveAt(i);
        }

        //if the clone has more patplans than the non-clone, drop the additional patplans
        //we will compute all estimates for the clone after all of the patplan adding/dropping/rearranging
        for (var i = listPatPlansForSynch.Count - 1; i > listPatPlans.Count - 1; i--)
        {
            var insSubCloneCur = InsSubs.GetOne(listPatPlansForSynch[i].InsSubNum);
            //we will drop the clone's patplan because the clone has more patplans than the non-clone
            //before we can drop the plan, we have to make sure there is not a claim with today's date
            var isAttachedToClaim = false;
            for (var j = 0; j < claimList.Count; j++)
            {
                //claimList will only contain claims with DateService=Today
                if (claimList[j].PlanNum != insSubCloneCur.PlanNum) //different insplan
                    continue;
                patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "Insurance Plans do not match.  "
                                                                                   + "Due to a claim with today's date we cannot sync the plans, the issue must be corrected manually on the following plan")
                                                             + ": " + InsPlans.GetDescript(insSubCloneCur.PlanNum, familyCur, listInsPlans, listPatPlansForSynch[i].InsSubNum, listInsSubs) + ".\r\n";
                isAttachedToClaim = true;
                break;
            }

            if (isAttachedToClaim) //we will continue trying to drop non-clone additional plans, but only if no claim for today exists
                continue;
            patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The following insurance plan was dropped due to it not existing with the same ordinal on the original patient") + ": "
                                                                                                                                                                                                + InsPlans.GetDescript(insSubCloneCur.PlanNum, familyCur, listInsPlans, listPatPlansForSynch[i].InsSubNum, listInsSubs) + ".\r\n";
            patientClonePatPlanChanges.PatPlansChanged = true;
            PatPlans.DeleteNonContiguous(listPatPlansForSynch[i].PatPlanNum);
            listPatPlansForSynch.RemoveAt(i);
        }

        for (var i = 0; i < listPatPlans.Count; i++)
        {
            var insSubNonCloneCur = InsSubs.GetOne(listPatPlans[i].InsSubNum);
            var insPlanNonCloneDescriptCur = InsPlans.GetDescript(insSubNonCloneCur.PlanNum, familyCur, listInsPlans, listPatPlans[i].InsSubNum, listInsSubs);
            if (listPatPlansForSynch.Count < i + 1)
            {
                //if there is not a PatPlan at this ordinal position for the clone, add a new one that is an exact copy, with correct PatNum of course
                var patPlanNew = listPatPlans[i].Copy();
                patPlanNew.PatNum = patientSynch.PatNum;
                PatPlans.Insert(patPlanNew);
                patientClonePatPlanChanges.PatPlansInserted = true;
                patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The following insurance was added") + ": " + insPlanNonCloneDescriptCur + ".\r\n";
                patientClonePatPlanChanges.PatPlansChanged = true;
                continue;
            }

            var insSubCloneCur = InsSubs.GetOne(listPatPlansForSynch[i].InsSubNum);
            var insPlanCloneDescriptCur = InsPlans.GetDescript(insSubCloneCur.PlanNum, familyCur, listInsPlans, listPatPlansForSynch[i].InsSubNum, listInsSubs);
            if (listPatPlans[i].InsSubNum != listPatPlansForSynch[i].InsSubNum)
            {
                //both pats have a patplan at this ordinal, but the clone's is pointing to a different inssub
                //we will drop the clone's patplan and add the non-clone's patplan
                //before we can drop the plan, we have to make sure there is not a claim with today's date
                var isAttachedToClaim = false;
                for (var j = 0; j < claimList.Count; j++)
                {
                    //claimList will only contain claims with DateService=Today
                    if (claimList[j].PlanNum != insSubCloneCur.PlanNum) //different insplan
                        continue;
                    patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "Insurance Plans do not match.  "
                                                                                       + "Due to a claim with today's date we cannot sync the plans, the issue must be corrected manually on the following plan")
                                                                 + ": " + insPlanCloneDescriptCur + ".\r\n";
                    isAttachedToClaim = true;
                    break;
                }

                if (isAttachedToClaim) //if we cannot change this plan to match the non-clone's plan at the same ordinal, we will synch the rest of the plans and let the user know to fix manually
                    continue;
                patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The following plan was updated to match the selected patient's plan") + ": " + insPlanCloneDescriptCur + ".\r\n";
                patientClonePatPlanChanges.PatPlansChanged = true;
                PatPlans.DeleteNonContiguous(listPatPlansForSynch[i].PatPlanNum); //we use the NonContiguous version because we are going to insert into this same ordinal, compute estimates will happen at the end of all the changes
                var patPlanCopy = listPatPlans[i].Copy();
                patPlanCopy.PatNum = patientSynch.PatNum;
                PatPlans.Insert(patPlanCopy);
            }
            else
            {
                //both clone and non-clone have the same patplan.InsSubNum at this position in their list, just make sure all data in the patplans match
                if (listPatPlans[i].Ordinal != listPatPlansForSynch[i].Ordinal)
                {
                    patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The ordinal of the insurance plan") + " " + insPlanCloneDescriptCur + " "
                                                                 + Lans.g("ContrFamily", "was updated to") + " " + listPatPlans[i].Ordinal + ".\r\n";
                    patientClonePatPlanChanges.PatPlansChanged = true;
                    listPatPlansForSynch[i].Ordinal = listPatPlans[i].Ordinal;
                }

                if (listPatPlans[i].IsPending != listPatPlansForSynch[i].IsPending)
                {
                    patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The pending status of the insurance plan") + " " + insPlanCloneDescriptCur + " "
                                                                 + Lans.g("ContrFamily", "was updated to") + " " + listPatPlans[i].IsPending + ".\r\n";
                    patientClonePatPlanChanges.PatPlansChanged = true;
                    listPatPlansForSynch[i].IsPending = listPatPlans[i].IsPending;
                }

                if (listPatPlans[i].Relationship != listPatPlansForSynch[i].Relationship)
                {
                    patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The relationship to the subscriber of the insurance plan") + " " + insPlanCloneDescriptCur + " "
                                                                 + Lans.g("ContrFamily", "was updated to") + " " + listPatPlans[i].Relationship + ".\r\n";
                    patientClonePatPlanChanges.PatPlansChanged = true;
                    listPatPlansForSynch[i].Relationship = listPatPlans[i].Relationship;
                }

                if (listPatPlans[i].PatID != listPatPlansForSynch[i].PatID)
                {
                    patientClonePatPlanChanges.StrDataUpdated += Lans.g("ContrFamily", "The patient ID of the insurance plan") + " " + insPlanCloneDescriptCur + " "
                                                                 + Lans.g("ContrFamily", "was updated to") + " " + listPatPlans[i].PatID + ".\r\n";
                    patientClonePatPlanChanges.PatPlansChanged = true;
                    listPatPlansForSynch[i].PatID = listPatPlans[i].PatID;
                }

                PatPlans.Update(listPatPlansForSynch[i]);
            }
        }

        if (patientClonePatPlanChanges.PatPlansInserted) SecurityLogs.MakeLogEntry(EnumPermType.PatPlanCreate, 0, Lans.g("ContrFamily", "One or more PatPlans created via Sync Clone tool."));
        if (patientClonePatPlanChanges.PatPlansChanged)
        {
            //compute all estimates for clone after making changes to the patplans
            var claimProcs = ClaimProcs.Refresh(patientSynch.PatNum);
            var procs = Procedures.Refresh(patientSynch.PatNum);
            listPatPlansForSynch = PatPlans.Refresh(patientSynch.PatNum);
            listInsSubs = InsSubs.RefreshForFam(familyCur);
            listInsPlans = InsPlans.RefreshForSubList(listInsSubs);
            listBenefits = Benefits.Refresh(listPatPlansForSynch, listInsSubs);
            Procedures.ComputeEstimatesForAll(patientSynch.PatNum, claimProcs, procs, listInsPlans, listPatPlansForSynch, listBenefits, patientSynch.Age, listInsSubs);
            SetHasIns(patientSynch.PatNum);
        }

        #endregion Synch Clone Data - PatPlans

        return patientClonePatPlanChanges;
    }

    public static bool IsPatientAClone(long patNum)
    {
        return PatientLinks.IsPatientAClone(patNum);
    }

    public static bool IsPatientACloneOrOriginal(long patNum)
    {
        return PatientLinks.IsPatientACloneOrOriginal(patNum);
    }

    public static bool ArePatientsClonesOfEachOther(long patNum1, long patNum2)
    {
        return PatientLinks.ArePatientsClonesOfEachOther(patNum1, patNum2);
    }

    public static string ReplacePatient(string message, Patient pat, bool isHtmlEmail = false)
    {
        if (pat == null) return message;
        //Use patient's preferred name if they have one, otherwise default to first name.
        var strPatPreferredOrFName = pat.FName;
        if (!string.IsNullOrWhiteSpace(pat.Preferred)) strPatPreferredOrFName = pat.Preferred;
        var template = new StringBuilder(message);
        ReplaceTags.ReplaceOneTag(template, "[FName]", pat.FName, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[LName]", pat.LName, isHtmlEmail);
        var lNameLetter = "";
        if (pat.LName?.Length > 0) lNameLetter = pat.LName.Substring(0, 1).ToUpper();
        ReplaceTags.ReplaceOneTag(template, "[LNameLetter]", lNameLetter, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[NameF]", pat.FName, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[NameFL]", GetNameFL(pat.LName, pat.FName, pat.Preferred, pat.MiddleI), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[NameLF]", GetNameLF(pat), isHtmlEmail);
        ;
        ReplaceTags.ReplaceOneTag(template, "[NamePreferredOrFirst]", strPatPreferredOrFName, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[PatNum]", pat.PatNum.ToString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[ChartNumber]", pat.ChartNumber, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[HmPhone]", pat.HmPhone, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[WkPhone]", pat.WkPhone, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Gender]", pat.Gender.ToString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Email]", pat.Email, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[ProvNum]", pat.PriProv.ToString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[ClinicNum]", pat.ClinicNum.ToString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[WirelessPhone]", pat.WirelessPhone, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Birthdate]", pat.Birthdate.ToShortDateString(), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Birthdate_yyyyMMdd]", pat.Birthdate.ToString("yyyyMMdd"), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[SSN]", pat.SSN, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Address]", pat.Address, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Address2]", pat.Address2, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[City]", pat.City, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[State]", pat.State, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[Zip]", pat.Zip, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[MonthlyCardsOnFile]", CreditCards.GetMonthlyCardsOnFile(pat.PatNum), isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[PatientTitle]", pat.Title, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[PatientMiddleInitial]", pat.MiddleI, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[PatientPreferredName]", pat.Preferred, isHtmlEmail);
        ReplaceTags.ReplaceOneTag(template, "[PrimaryProviderNameFLSuffix]", Providers.GetFormalName(pat.PriProv), isHtmlEmail);
        if (message.Contains("[ReferredFromProvNameFL]"))
        {
            var patRef = Referrals.GetReferralForPat(pat.PatNum);
            if (patRef != null)
                ReplaceTags.ReplaceOneTag(template, "[ReferredFromProvNameFL]", GetNameFL(patRef.LName, patRef.FName, "", ""), isHtmlEmail);
            else
                ReplaceTags.ReplaceOneTag(template, "[ReferredFromProvNameFL]", "", isHtmlEmail);
        }

        if (message.Contains("[PatientGenderMF]"))
        {
            if (pat.Gender == PatientGender.Male)
                ReplaceTags.ReplaceOneTag(template, "[PatientGenderMF]", "M", isHtmlEmail);
            else if (pat.Gender == PatientGender.Female)
                ReplaceTags.ReplaceOneTag(template, "[PatientGenderMF]", "F", isHtmlEmail);
            else
                ReplaceTags.ReplaceOneTag(template, "[PatientGenderMF]", "", isHtmlEmail);
        }

        return template.ToString();
    }

    public static string ReplaceGuarantor(string message, Patient pat)
    {
        if (pat == null) return message;
        var guar = GetPat(pat.Guarantor);
        if (guar == null) return message;
        var retVal = message;
        retVal = retVal.Replace("[GuarantorPatNum]", guar.PatNum.ToString());
        retVal = retVal.Replace("[GuarantorTitle]", guar.Title);
        retVal = retVal.Replace("[GuarantorNameF]", guar.FName);
        retVal = retVal.Replace("[GuarantorNameL]", guar.LName);
        retVal = retVal.Replace("[GuarantorMiddleInitial]", guar.MiddleI);
        retVal = retVal.Replace("[GuarantorHmPhone]", guar.HmPhone);
        retVal = retVal.Replace("[GuarantorWkPhone]", guar.WkPhone);
        retVal = retVal.Replace("[GuarantorAddress]", guar.Address);
        retVal = retVal.Replace("[GuarantorAddress2]", guar.Address2);
        retVal = retVal.Replace("[GuarantorCity]", guar.City);
        retVal = retVal.Replace("[GuarantorState]", guar.State);
        retVal = retVal.Replace("[GuarantorZip]", guar.Zip);
        return retVal;
    }

    public static bool IsFieldPHI(string field)
    {
        return ListPHIFields.Select(x => x.ToLower()).Contains(field.ToLower());
    }

    public static List<string> ListPHIFields =>
    [
        "[LName]",
        "[NameFL]",
        "[NameLF]",
        "[WirelessPhone]",
        "[HmPhone]",
        "[WkPhone]",
        "[Birthdate]",
        "[Birthdate_yyyyMMdd]",
        "[SSN]",
        "[Address]",
        "[Address2]",
        "[City]",
        "[Zip]"
    ];
}

public class PatientEditSameForFamily
{
    public bool AddressSameForFamily;
    public bool AddressSameForSuperFamily;
    public bool AddressSameForSuperFamilyVisible;
    public bool AddrPhoneNotesSameForFamily;
    public bool ArriveEarlySameForFamily;
    public bool BillingProviderSameForFamily;
    public bool EmailPhoneSameForFamily;

    public PatientEditSameForFamily(Patient patient, Family family)
    {
        AddressSameForFamily = true;
        BillingProviderSameForFamily = true;
        AddrPhoneNotesSameForFamily = true;
        EmailPhoneSameForFamily = true;
        ArriveEarlySameForFamily = true;
        AddressSameForSuperFamily = false;
        AddressSameForSuperFamilyVisible = false;
        var isAuthArchivedEdit = Security.IsAuthorized(EnumPermType.ArchivedPatientEdit, true);
        var listPatientsFamily = family.ListPats.ToList();
        if (!isAuthArchivedEdit)
            //Exclude Archived pats if user does not have permission.  If user doesn't have the permission and all non-Archived patients pass the
            //the conditions, but there is an Archived patient who does not, the check will still be true.  The Archived will not be updated on OK.
            listPatientsFamily = listPatientsFamily.Where(x => x.PatStatus != PatientStatus.Archived).ToList();
        //for the comparison logic to work, any nulls must be converted to ""
        if (patient.HmPhone is null) patient.HmPhone = "";
        if (patient.Address is null) patient.Address = "";
        if (patient.Address2 is null) patient.Address2 = "";
        if (patient.City is null) patient.City = "";
        if (patient.State is null) patient.State = "";
        if (patient.Zip is null) patient.Zip = "";
        if (patient.Country is null) patient.Country = "";
        if (patient.CreditType is null) patient.CreditType = "";
        if (patient.AddrNote is null) patient.AddrNote = "";
        if (patient.WirelessPhone is null) patient.WirelessPhone = "";
        if (patient.WkPhone is null) patient.WkPhone = "";
        if (patient.Email is null) patient.Email = "";
        for (var i = 0; i < listPatientsFamily.Count; i++)
        {
            if (patient.HmPhone != listPatientsFamily[i].HmPhone
                || patient.Address != listPatientsFamily[i].Address
                || patient.Address2 != listPatientsFamily[i].Address2
                || patient.City != listPatientsFamily[i].City
                || patient.State != listPatientsFamily[i].State
                || patient.Zip != listPatientsFamily[i].Zip
                || patient.Country != listPatientsFamily[i].Country)
                AddressSameForFamily = false;
            if (patient.CreditType != listPatientsFamily[i].CreditType
                || patient.BillingType != listPatientsFamily[i].BillingType
                || patient.PriProv != listPatientsFamily[i].PriProv
                || patient.SecProv != listPatientsFamily[i].SecProv
                || patient.FeeSched != listPatientsFamily[i].FeeSched)
                BillingProviderSameForFamily = false;
            if (patient.AddrNote != listPatientsFamily[i].AddrNote) AddrPhoneNotesSameForFamily = false;
            if (patient.WirelessPhone != listPatientsFamily[i].WirelessPhone
                || patient.WkPhone != listPatientsFamily[i].WkPhone
                || patient.Email != listPatientsFamily[i].Email)
                EmailPhoneSameForFamily = false;
            if (patient.AskToArriveEarly != listPatientsFamily[i].AskToArriveEarly) ArriveEarlySameForFamily = false;
        }

        if (PrefC.GetBool(PrefName.SameForFamilyCheckboxesUnchecked))
        {
            AddressSameForFamily = false;
            BillingProviderSameForFamily = false;
            AddrPhoneNotesSameForFamily = false;
            EmailPhoneSameForFamily = false;
        }

        //SuperFamilies is enabled, Syncing SuperFam Info is enabled, and this is the superfamily head.  Show the sync checkbox.
        if (PrefC.GetBool(PrefName.ShowFeatureSuperfamilies)
            && PrefC.GetBool(PrefName.PatientAllSuperFamilySync)
            && patient.SuperFamily != 0
            && patient.PatNum == patient.SuperFamily) //Has to be the Super Head.
        {
            AddressSameForSuperFamilyVisible = true;
            //Check all superfam members for any with differing information
            AddressSameForSuperFamily = Patients.SuperFamHasSameAddrPhone(patient, isAuthArchivedEdit);
        }
    }
}

public class PatientCloneDemographicChanges
{
    public List<string> ListFieldsCleared;
    public List<PatientCloneField> ListFieldsUpdated;
}

public class PatientClonePatPlanChanges
{
    public bool PatPlansChanged;
    public bool PatPlansInserted;
    public string StrDataUpdated;
}

public class PatientCloneField(string fieldName, string oldValue, string newValue)
{
    public string FieldName = fieldName;
    public string NewValue = newValue;
    public string OldValue = oldValue;
}

public class PatComm : WebBase
{
    public long ClinicNum;
    public CommOptOut CommOptOut = new();
    public string Email;
    public string FName;
    public long Guarantor;
    public bool IsEmailAnOption;
    public bool IsSmsAnOption;
    public bool IsSmsPhoneFormatOk;
    public bool IsTextingEnabledForClinic;
    public string Language;
    public string LName;
    public long PatNum;
    public PatientStatus PatStatus;
    public string PreferredName;
    public bool Premed;
    public string SmsPhone;
    public YN TxtMsgOk;
    public string WirelessPhone;

    public PatComm(Patient pat, bool isEmailValidForClinic, bool isTextingEnabledForClinic, bool isUnknownNo, string curCulture, string smsPhoneCountryCode)
    {
        PatNum = pat.PatNum;
        PatStatus = pat.PatStatus;
        TxtMsgOk = pat.TxtMsgOk;
        WirelessPhone = pat.WirelessPhone;
        Email = GetEmailAddresses(pat.Email);
        FName = pat.FName;
        LName = pat.LName;
        PreferredName = pat.Preferred;
        Guarantor = pat.Guarantor;
        ClinicNum = pat.ClinicNum;
        Language = pat.Language;
        Premed = pat.Premed;
        SetSmsEmailFields(isEmailValidForClinic, isTextingEnabledForClinic, isUnknownNo, curCulture, smsPhoneCountryCode);
    }

    public PatComm(DataRow dataRow, bool isEmailValidForClinic, bool isTextingEnabledForClinic, bool isUnknownNo, string curCulture, string smsPhoneCountryCode)
    {
        PatNum = SIn.Long(dataRow["PatNum"].ToString());
        PatStatus = (PatientStatus) SIn.Int(dataRow["PatStatus"].ToString());
        TxtMsgOk = (YN) SIn.Int(dataRow["TxtMsgOk"].ToString());
        SIn.String(dataRow["HmPhone"].ToString());
        SIn.String(dataRow["WkPhone"].ToString());
        WirelessPhone = SIn.String(dataRow["WirelessPhone"].ToString());
        Email = GetEmailAddresses(SIn.String(dataRow["Email"].ToString()));
        FName = SIn.String(dataRow["FName"].ToString());
        PreferredName = SIn.String(dataRow["Preferred"].ToString());
        LName = SIn.String(dataRow["LName"].ToString());
        Guarantor = SIn.Long(dataRow["Guarantor"].ToString());
        ClinicNum = SIn.Long(dataRow["ClinicNum"].ToString());
        Language = SIn.String(dataRow["Language"].ToString());
        SIn.Date(dataRow["Birthdate"].ToString());
        Premed = SIn.Bool(dataRow["Premed"].ToString());
        SetSmsEmailFields(isEmailValidForClinic, isTextingEnabledForClinic, isUnknownNo, curCulture, smsPhoneCountryCode);
    }

    public static string GetEmailAddresses(string emailAddresses)
    {
        return string.Join($"{EmailAddresses.AddressDelimiters.First()}", EmailAddresses.GetValidAddresses(emailAddresses));
    }

    private void SetSmsEmailFields(bool isEmailValidForClinic, bool isTextingEnabledForClinic, bool isUnknownNo, string curCulture, string smsPhoneCountryCode)
    {
        IsSmsPhoneFormatOk = false;
        if (TxtMsgOk == YN.No || (isUnknownNo && TxtMsgOk == YN.Unknown))
        {
            SmsPhone = "";
        }
        else
        {
            //Previously chose between WirelessPhone,HmPhone,WkPhone. Now chooses WirelessPhone or nothing at all.
            SmsPhone = WirelessPhone?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(SmsPhone))
                SmsPhone = "";
            else
                try
                {
                    SmsPhone = SmsToMobiles.ConvertPhoneToInternational(SmsPhone, curCulture, smsPhoneCountryCode);
                    IsSmsPhoneFormatOk = true;
                }
                catch (Exception e)
                {
                    //Formatting for sms failed to set to empty so we don't try to use it.
                    SmsPhone = "";
                }
        }

        IsSmsAnOption =
            //SmsPhone is in proper format for sms send.
            IsSmsPhoneFormatOk
            //Sms is allowed by practice and patient.
            && (TxtMsgOk == YN.Yes || (TxtMsgOk == YN.Unknown && !isUnknownNo))
            //Patient has a valid phone number.
            && !string.IsNullOrWhiteSpace(SmsPhone)
            //Patient is not deceased
            && PatStatus != PatientStatus.Deceased
            //Clinic linked to this PatComm supports texting.
            && isTextingEnabledForClinic;
        IsTextingEnabledForClinic = isTextingEnabledForClinic;
        IsEmailAnOption =
            //Patient has a valid email.
            EmailAddresses.GetValidAddresses(Email).Count > 0
            //Patient is not deceased
            && PatStatus != PatientStatus.Deceased
            //Clinic linked to this PatComm has a valid email.
            && isEmailValidForClinic;
    }

    public string GetReasonCantText(CommOptOutType type = 0)
    {
        if (!IsTextingEnabledForClinic) return "Not sending text because texting is not enabled for this clinic.";
        if (TxtMsgOk == YN.No || (TxtMsgOk == YN.Unknown && PrefC.GetBool(PrefName.TextMsgOkStatusTreatAsNo))) return "Not sending text because this patient is set to not receive texts.";
        if (string.IsNullOrEmpty(WirelessPhone)) return "Not sending text because this patient does not have a wireless phone entered.";
        if (!IsSmsPhoneFormatOk) return "Not sending text because this patient's wireless phone is not valid.";
        if (!IsSmsAnOption) return "Not sending text because this patient is not able to receive texts.";
        if (CommOptOut.IsOptedOut(CommOptOutMode.Text, type)) return "Not sending text because this patient opted out of receiving automated text messages.";
        return "";
    }

    public static string BuildConfirmMessage(ContactMethod contactMethod, Patient pat, Appointment appt)
    {
        var template = contactMethod switch
        {
            ContactMethod.Email => PrefC.GetString(PrefName.ConfirmEmailMessage),
            ContactMethod.TextMessage => PrefC.GetString(PrefName.ConfirmTextMessage),
            ContactMethod.Mail => PrefC.GetString(PrefName.ConfirmPostcardMessage),
            _ => PrefC.GetString(PrefName.ConfirmTextMessage)
        };
        return BuildAppointmentMessage(pat, appt, template);
    }

    public static string BuildAppointmentMessage(Patient pat, Appointment appt, string template = "[NameF]:  [date] at [time]", bool isEmail = false)
    {
        var dateTime = appt.AptDateTime;
        if (appt.DateTimeAskedToArrive.Year > 1880) dateTime = appt.DateTimeAskedToArrive;
        if (pat != null)
        {
            var name = Patients.GetNameFirstOrPreferred(pat.FName, pat.Preferred);
            ClinicDto clinic = null; //Null clinic will default to practice name in TagReplacer.
            if (appt.ClinicNum > 0)
                clinic = Clinics.GetClinic(appt.ClinicNum);
            else if (pat.ClinicNum > 0) clinic = Clinics.GetClinic(pat.ClinicNum);
            var tagReplacer = new TagReplacer();
            var autoCommObj = new AutoCommObj();
            autoCommObj.NameF = pat.FName;
            autoCommObj.NamePreferredOrFirst = name;
            autoCommObj.ProvNum = appt.ProvNum;
            template = tagReplacer.ReplaceTags(template, autoCommObj, clinic, isEmail);
        }

        template = template.Replace("[date]", dateTime.ToString(PrefC.PatientCommunicationDateFormat)); //[date] and [time] aren't considered in ReplaceTags
        template = template.Replace("[time]", dateTime.ToString(PrefC.PatientCommunicationTimeFormat));
        return template;
    }

    public string GetFirstOrPreferred()
    {
        return Patients.GetNameFirstOrPreferred(FName, PreferredName);
    }

    public bool IsPatientShortCodeEligible(long clinicNum)
    {
        return IsSmsAnOption //Patient set to receive sms.
               && IsAnyShortCodeServiceEnabled(clinicNum); //At least one Short Code eService is enabled
    }

    public static bool IsAnyShortCodeServiceEnabled(long clinicNum)
    {
        return EnumTools.GetFlags((ShortCodeTypeFlag) ClinicPrefs.GetLong(PrefName.ShortCodeApptReminderTypes, clinicNum))
            .Any(x => EnumTools.GetAttributeOrDefault<ShortCodeAttribute>(x).IsServiceEnabled(clinicNum));
    }
}

public class PatAging
{
    public string Address;
    public double AmountDue;
    public double Bal_0_30;
    public double Bal_31_60;
    public double Bal_61_90;
    public double BalOver90;
    public double BalTotal;
    public long BillingType;
    public DateTime Birthdate;
    public string City;
    public long ClinicNum;
    public DateTime DateBalBegan;
    public DateTime DateLastPay;
    public DateTime DateLastProc;
    public DateTime DateLastStatement;
    public long Guarantor;
    public bool HasInsPending;
    public bool HasSuperBilling;
    public bool HasUnsentProcs;
    public double InsEst;
    public List<TsiTransLog> ListTsiLogs;
    public string PatName;
    public long PatNum;
    public double PayPlanDue;
    public long PriProv;
    public string State;
    public long SuperFamily;
    public string Zip;
}

public class ClinicBalBegans(long clinicNum, Dictionary<long, DateTime> dictGuarDateBals)
{
    public long ClinicNum = clinicNum;
    public Dictionary<long, DateTime> DictGuarDateBals = dictGuarDateBals;
}

[Serializable]
public class PtTableSearchParams
{
    public string Address = "";
    public long BillingType;
    public DateTime Birthdate;
    public string ChartNumber;
    public string City;
    public string ClinicName;
    public string ClinicNums;
    public string Country;
    public bool DoLimit;
    public string Email;
    public string FName;
    public bool GuarOnly;
    public bool HasNextLastVisit;
    public bool HasSpecialty;
    public bool HideInactive;
    public long InitialPatNum;
    public string InvoiceNumber = "";
    public bool IsFromMobile;
    public List<long> ListExplicitPatNums;
    public string LName;
    public int PageNum;
    public string PatNumStr = "";
    public string Phone = "";
    public string RegKey;
    public bool ShowArchived;
    public bool ShowMerged;
    public long SiteNum;
    public string Ssn;
    public string State;
    public string SubscriberId;
    
    public PtTableSearchParams()
    {
    }

    public PtTableSearchParams(bool doLimit, string lname, string fname, string phone, string address, bool hideInactive, string city,
        string state, string ssn, string patNumStr, string chartNumber, long billingType, bool guarOnly, bool showArchived, DateTime birthdate, long siteNum,
        string subscriberId, string email, string country, string regKey, string clinicNums, string clinicName, string invoiceNumber, List<long> listExplicitPatNums = null,
        long initialPatNum = 0, bool showMerged = false, bool hasSpecialty = false, bool hasNextLastVisit = false, bool isFromMobile = false, int pageNum = 1)
    {
        DoLimit = doLimit; //bool
        LName = SOut.String(lname);
        FName = SOut.String(fname);
        Phone = SOut.String(TelephoneNumbers.AutoFormat(phone));
        Address = SOut.String(address);
        HideInactive = hideInactive; //bool
        City = SOut.String(city);
        State = SOut.String(state);
        Ssn = SOut.String(ssn);
        PatNumStr = SOut.String(patNumStr);
        ChartNumber = SOut.String(chartNumber);
        BillingType = billingType; //long
        GuarOnly = guarOnly; //bool
        ShowArchived = showArchived; //bool
        Birthdate = birthdate; //date
        SiteNum = siteNum; //long
        SubscriberId = SOut.String(subscriberId);
        Email = SOut.String(email);
        Country = SOut.String(country);
        RegKey = SOut.String(new string(regKey.Where(x => char.IsLetterOrDigit(x)).ToArray()));
        ClinicNums = SOut.String(clinicNums);
        ClinicName = SOut.String(clinicName);
        InvoiceNumber = SOut.String(invoiceNumber);
        InitialPatNum = initialPatNum; //long
        ShowMerged = showMerged; //bool
        HasSpecialty = hasSpecialty; //bool
        HasNextLastVisit = hasNextLastVisit; //bool
        ListExplicitPatNums = listExplicitPatNums ?? []; //List<long>
        IsFromMobile = isFromMobile; //bool
        PageNum = pageNum; //int
    }


    public PtTableSearchParams Copy()
    {
        var retval = (PtTableSearchParams) MemberwiseClone();
        retval.ListExplicitPatNums = ListExplicitPatNums.ToList();
        return retval;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is PtTableSearchParams)) return false;
        var pTSPOther = obj as PtTableSearchParams;
        if (DoLimit != pTSPOther.DoLimit
            || LName != pTSPOther.LName
            || FName != pTSPOther.FName
            || Phone != pTSPOther.Phone
            || Address != pTSPOther.Address
            || HideInactive != pTSPOther.HideInactive
            || City != pTSPOther.City
            || State != pTSPOther.State
            || Ssn != pTSPOther.Ssn
            || PatNumStr != pTSPOther.PatNumStr
            || ChartNumber != pTSPOther.ChartNumber
            || BillingType != pTSPOther.BillingType
            || GuarOnly != pTSPOther.GuarOnly
            || ShowArchived != pTSPOther.ShowArchived
            || Birthdate != pTSPOther.Birthdate
            || SiteNum != pTSPOther.SiteNum
            || SubscriberId != pTSPOther.SubscriberId
            || Email != pTSPOther.Email
            || Country != pTSPOther.Country
            || RegKey != pTSPOther.RegKey
            || ClinicNums != pTSPOther.ClinicNums
            || ClinicName != pTSPOther.ClinicName
            || InvoiceNumber != pTSPOther.InvoiceNumber
            || InitialPatNum != pTSPOther.InitialPatNum
            || ShowMerged != pTSPOther.ShowMerged
            || HasSpecialty != pTSPOther.HasSpecialty
            || HasNextLastVisit != pTSPOther.HasNextLastVisit
            || ListExplicitPatNums == null != (pTSPOther.ListExplicitPatNums == null)
            || (ListExplicitPatNums != null && pTSPOther.ListExplicitPatNums != null
                                            && (ListExplicitPatNums.Except(pTSPOther.ListExplicitPatNums).Any() || pTSPOther.ListExplicitPatNums.Except(ListExplicitPatNums).Any()))
            || IsFromMobile != pTSPOther.IsFromMobile)
            return false;
        return true;
    }

    public override int GetHashCode()
    {
        //Always return the same value (0 is acceptable). This will defer to the Equals override as the tie-breaker, which is what we want in this case.
        return 0;
    }
}

public class PatientFor834Import : IComparable
{
    private static readonly List<PatientStatus> _listPatientStatuses = [PatientStatus.Patient, PatientStatus.Inactive, PatientStatus.NonPatient, PatientStatus.Prospective];
    public DateTime Birthdate;
    public string FName;
    public string LName;
    public long PatNum;
    public PatientStatus PatStatus;

    public PatientFor834Import()
    {
    }

    public PatientFor834Import(Patient patient)
    {
        PatNum = patient.PatNum;
        FName = patient.FName;
        LName = patient.LName;
        Birthdate = patient.Birthdate;
        PatStatus = patient.PatStatus;
    }

    public int CompareTo(object patOther)
    {
        var p1 = this;
        var p2 = (PatientFor834Import) patOther;
        var lname1 = p1.LName == null ? "" : p1.LName;
        var lname2 = p2.LName == null ? "" : p2.LName;
        var comp = lname1.Trim().ToLower().CompareTo(lname2.Trim().ToLower());
        if (comp != 0) return comp;
        var fname1 = p1.FName == null ? "" : p1.FName;
        var fname2 = p2.FName == null ? "" : p2.FName;
        comp = fname1.Trim().ToLower().CompareTo(fname2.Trim().ToLower());
        if (comp != 0) return comp;
        return p1.Birthdate.Date.CompareTo(p2.Birthdate.Date);
    }

    public static List<PatientFor834Import> GetPatientLimitedsByNameAndBirthday(Patient patient, List<PatientFor834Import> listSortedPatients)
    {
        var pat = new PatientFor834Import(patient);
        if (pat.LName.Trim().ToLower().Length == 0 || pat.FName.Trim().ToLower().Length == 0 || pat.Birthdate.Year < 1880)
            //We do not allow a match unless Last Name, First Name, AND birthdate are specified.  Otherwise at match could be meaningless.
            return [];
        var patIdx = listSortedPatients.BinarySearch(pat); //If there are multiple matches, then this will only return one of the indexes randomly.
        if (patIdx < 0)
            //No matches found.
            return [];
        //The matched indicies will all be consecutive and will include the returned index from the binary search, because the list is sorted.
        var beginIdx = patIdx;
        for (var i = patIdx - 1; i >= 0 && pat.CompareTo(listSortedPatients[i]) == 0; i--) beginIdx = i;
        var endIdx = patIdx;
        for (var i = patIdx + 1; i < listSortedPatients.Count && pat.CompareTo(listSortedPatients[i]) == 0; i++) endIdx = i;
        var listPatientMatches = new List<PatientFor834Import>();
        for (var i = beginIdx; i <= endIdx; i++) listPatientMatches.Add(listSortedPatients[i]);
        return listPatientMatches;
    }
    
    public static void FilterMatchingList(ref List<PatientFor834Import> listPatientMatches)
    {
        if (listPatientMatches.Where(x => _listPatientStatuses.Contains(x.PatStatus)).ToList().Count == 0) return;
        if (listPatientMatches.Count > 1)
            listPatientMatches = listPatientMatches
                .Where(x =>
                    !(x.PatStatus == PatientStatus.Archived
                      || x.PatStatus == PatientStatus.Deceased)
                ).ToList();
    }
}