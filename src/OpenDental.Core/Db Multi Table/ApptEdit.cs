using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ApptEdit
{
    public static LoadData GetLoadData(Appointment appointment)
    {
        var loadData = new LoadData
        {
            ListProceduresForAppointment = Procedures.GetProcsForApptEdit(appointment)
        };
        
        loadData.ListAppointments = Appointments.GetAppointmentsForProcs(loadData.ListProceduresForAppointment);
        loadData.Family = Patients.GetFamily(appointment.PatNum);
        loadData.ListPatPlans = PatPlans.Refresh(appointment.PatNum);
        loadData.ListInsSubs = InsSubs.RefreshForFam(loadData.Family);
        loadData.ListBenefits = Benefits.Refresh(loadData.ListPatPlans, loadData.ListInsSubs);
        loadData.ListClaimProcHists = ClaimProcs.GetHistList(appointment.PatNum, loadData.ListBenefits, loadData.ListPatPlans, loadData.ListInsPlans, DateTime.Today, loadData.ListInsSubs);
        loadData.ListInsPlans = InsPlans.RefreshForSubList(loadData.ListInsSubs);
        loadData.TableApppointmentFields = Appointments.GetApptFields(appointment.AptNum);
        loadData.TableComms = Appointments.GetCommTable(appointment.PatNum.ToString(), appointment.AptNum);
        loadData.ListLabCases = LabCases.GetForApt(appointment);
        loadData.TablePatients = Appointments.GetPatTable(appointment.PatNum.ToString(), appointment);
        loadData.ListClaimProcs = ClaimProcs.RefreshForProcs(loadData.ListProceduresForAppointment.Select(x => x.ProcNum).ToList());
        loadData.ListAdjustments = Adjustments.GetForProcs(loadData.ListProceduresForAppointment.Select(x => x.ProcNum).ToList());
        
        return loadData;
    }

    public static List<Procedure> QuickAddProcs(Appointment appointment, Patient patient, List<string> listProcedureCodesAndToothNumstoAdd, long provNum, long provHyg, List<InsSub> listInsSubs, List<InsPlan> listInsPlans, List<PatPlan> listPatPlans, List<Benefit> listBenefits)
    {
        Procedures.SetDateFirstVisit(appointment.AptDateTime.Date, 1, patient);
        var listClaimProcs = ClaimProcs.Refresh(appointment.PatNum);
        var listStringProcCodes = new List<string>();
        //Examples: D0220#7,D0220#10,D0220#25
        //D0220,D0220,D0220  
        //D0220,D0220#10,D0220#25
        for (var i = 0; i < listProcedureCodesAndToothNumstoAdd.Count; i++)
        {
            var stringArrayProcedureCodeAndToothNum = listProcedureCodesAndToothNumstoAdd[i].Split('#'); //Example D2102#8 (tooth is in international format)
            listStringProcCodes.Add(stringArrayProcedureCodeAndToothNum[0]); //ProcCode is first item
        }

        var listProcedureCodes = ProcedureCodes.GetProcCodes(listStringProcCodes);
        var listProvNumsTreat = new List<long>();
        listProvNumsTreat.Add(provNum);
        listProvNumsTreat.Add(provHyg); //these were both passed in
        var listSubstitutionLinks = SubstitutionLinks.GetAllForPlans(listInsPlans); //not available in FormApptEdit
        var discountPlanNum = DiscountPlanSubs.GetDiscountPlanNumForPat(patient.PatNum, appointment.AptDateTime);
        var listFees = Fees.GetListFromObjects(listProcedureCodes, listMedicalCodes: null, //no procs to pull medicalCodes from
            listProvNumsTreat, patient.PriProv, patient.SecProv, patient.FeeSched,
            listInsPlans, new List<long>() {appointment.ClinicNum}, listAppointments: null, //procNums for appt already handled above
            listSubstitutionLinks, discountPlanNum);
        //null,listProvNumsTreat,listProcedureCodes.Select(x=>x.ProvNumDefault).ToList(),
        //pat.PriProv,pat.SecProv,pat.FeeSched,listInsPlans,new List<long>(){apt.ClinicNum},listProcCodesToAdd,null);//procnums for appt already handled above
        var listProceduresAdded = new List<Procedure>();
        //Make a copy of apt with provNum and provHyg, in order to maintain behavior of this method prior to using Procedures.ConstructProcedureForAppt
        //provNum and provHyg are sent in and are the selected provs in FormApptEdit, which may be different than the current provs on apt
        var appointmentCur = appointment.Copy();
        appointmentCur.ProvNum = provNum;
        appointmentCur.ProvHyg = provHyg;
        for (var i = 0; i < listProcedureCodesAndToothNumstoAdd.Count; i++)
        {
            var stringArrayProcCodeAndTooth = listProcedureCodesAndToothNumstoAdd[i].Split('#'); //0: ProcCode, 1: ToothNum(if present)
            var procedureCode = listProcedureCodes.Find(x => x.ProcCode == stringArrayProcCodeAndTooth[0]);
            if (procedureCode == null)
            {
                continue; //Bad ProcCode, do not insert.
            }

            var procedure = Procedures.ConstructProcedureForAppt
                (procedureCode.CodeNum, appointmentCur, patient, listPatPlans, listInsPlans, listInsSubs, listFees);
            if (stringArrayProcCodeAndTooth.Length == 2)
            {
                if (!Tooth.IsValidEntry(stringArrayProcCodeAndTooth[1]))
                {
                    //in db in international format
                    continue; //Bad ToothNum, do not insert.
                }

                procedure.ToothNum = Tooth.Parse(stringArrayProcCodeAndTooth[1]);
            }

            Procedures.Insert(procedure); //recall synch not required
            Procedures.ComputeEstimates(procedure, patient.PatNum, ref listClaimProcs, isInitialEntry: true, listInsPlans, listPatPlans, listBenefits,
                histList: null, loopList: null, saveToDb: true, patient.Age, listInsSubs, listClaimProcsAll: null, isClaimProcRemoveNeeded: false,
                useProcDateOnProc: false, listSubstitutionLinks, isForOrtho: false, listFees);
            listProceduresAdded.Add(procedure);
        }

        return new List<Procedure>(listProceduresAdded);
    }
    
    public class LoadData
    {
        public List<Procedure> ListProceduresForAppointment;
        public List<Appointment> ListAppointments;
        public Family Family;
        public List<PatPlan> ListPatPlans;
        public List<Benefit> ListBenefits;
        public List<ClaimProcHist> ListClaimProcHists;
        public List<InsSub> ListInsSubs;
        public List<InsPlan> ListInsPlans;
        public DataTable TableApppointmentFields;
        public DataTable TableComms;
        public List<LabCase> ListLabCases;
        public DataTable TablePatients;
        public List<ClaimProc> ListClaimProcs;
        public List<Adjustment> ListAdjustments;
    }
}