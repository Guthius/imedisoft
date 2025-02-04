using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TrojanQueries
{
    public static DateTime GetMaxProcedureDate(long PatNum)
    {
        var command = $@"SELECT MAX(ProcDate) FROM procedurelog,patient
				WHERE patient.PatNum=procedurelog.PatNum
				AND procedurelog.ProcStatus={(int) ProcStat.C}
				AND patient.Guarantor={PatNum}";
        return SIn.Date(DataCore.GetScalar(command));
    }

    public static DateTime GetMaxPaymentDate(long PatNum)
    {
        var command = $@"SELECT MAX(DatePay) FROM paysplit,patient
				WHERE patient.PatNum=paysplit.PatNum
				AND patient.Guarantor={PatNum}";
        return SIn.Date(DataCore.GetScalar(command));
    }

    public static int GetUniqueFileNum()
    {
        var progNum = Programs.GetProgramNum(ProgramName.TrojanExpressCollect);
        var fileNum = SIn.Int(ProgramProperties.GetValFromDb(progNum, "PreviousFileNumber"), false) + 1;
        while (ProgramProperties.SetProperty(progNum, "PreviousFileNumber", fileNum.ToString()) < 1)
        {
            fileNum++;
        }

        return fileNum;
    }

    public static DataTable GetPendingDeletionTable(Collection<string[]> deletePatientRecords)
    {
        var whereTrojanID = "";
        for (var i = 0; i < deletePatientRecords.Count; i++)
        {
            if (i > 0)
            {
                whereTrojanID += "OR ";
            }

            whereTrojanID += "i.TrojanID='" + deletePatientRecords[i][0] + "' ";
        }

        var command = "SELECT DISTINCT " +
                      "p.FName," +
                      "p.LName," +
                      "p.FName," +
                      "p.LName," +
                      "p.SSN," +
                      "p.Birthdate," +
                      "i.GroupNum," +
                      "s.SubscriberID," +
                      "i.TrojanID," +
                      "CASE i.EmployerNum WHEN 0 THEN '' ELSE e.EmpName END," +
                      "CASE i.EmployerNum WHEN 0 THEN '' ELSE e.Phone END," +
                      "c.CarrierName," +
                      "c.Phone " +
                      "FROM patient p,insplan i,employer e,carrier c,inssub s " +
                      "WHERE p.PatNum=s.Subscriber AND " +
                      "(" + whereTrojanID + ") AND " +
                      "i.CarrierNum=c.CarrierNum AND " +
                      "s.PlanNum=i.PlanNum AND " +
                      "(i.EmployerNum=e.EmployerNum OR i.EmployerNum=0) AND " +
                      "(SELECT COUNT(*) FROM patplan a WHERE a.PatNum=p.PatNum AND a.InsSubNum=s.InsSubNum) > 0 " +
                      "ORDER BY i.TrojanID,p.LName,p.FName";
        return DataCore.GetTable(command);
    }

    public static DataTable GetPendingDeletionTableTrojan(Collection<string[]> deleteTrojanRecords)
    {
        var whereTrojanID = "";
        for (var i = 0; i < deleteTrojanRecords.Count; i++)
        {
            if (i > 0)
            {
                whereTrojanID += "OR ";
            }

            whereTrojanID += "i.TrojanID='" + deleteTrojanRecords[i][0] + "' ";
        }

        var command = "SELECT DISTINCT " +
                      "p.FName," +
                      "p.LName," +
                      "p.FName," +
                      "p.LName," +
                      "p.SSN," +
                      "p.Birthdate," +
                      "i.GroupNum," +
                      "s.SubscriberID," +
                      "i.TrojanID," +
                      "CASE i.EmployerNum WHEN 0 THEN '' ELSE e.EmpName END," +
                      "CASE i.EmployerNum WHEN 0 THEN '' ELSE e.Phone END," +
                      "c.CarrierName," +
                      "c.Phone " +
                      "FROM patient p,insplan i,employer e,carrier c,inssub s " +
                      "WHERE p.PatNum=s.Subscriber AND " +
                      "(" + whereTrojanID + ") AND " +
                      "i.CarrierNum=c.CarrierNum AND " +
                      "s.PlanNum=i.PlanNum AND " +
                      "(i.EmployerNum=e.EmployerNum OR i.EmployerNum=0) AND " +
                      "(SELECT COUNT(*) FROM patplan a WHERE a.PatNum=p.PatNum AND a.InsSubNum=s.InsSubNum) > 0 " +
                      "ORDER BY i.TrojanID,p.LName,p.FName";
        return DataCore.GetTable(command);
    }

    public static InsPlan GetPlanWithTrojanID(string trojanID)
    {
        var command = "SELECT * FROM insplan WHERE TrojanID = '" + SOut.String(trojanID) + "'";
        return InsPlanCrud.SelectOne(command);
    }

    public static void UpdatePlan(TrojanObject troj, long planNum, bool updateBenefits)
    {
        var employerNum = Employers.GetEmployerNum(troj.ENAME);
        string command;
        //for(int i=0;i<planNums.Count;i++) {
        command = "UPDATE insplan SET "
                  + "EmployerNum=" + employerNum + ", "
                  + "GroupName='" + SOut.String(troj.PLANDESC) + "', "
                  + "GroupNum='" + SOut.String(troj.POLICYNO) + "', "
                  + "CarrierNum= " + troj.CarrierNum + " "
                  + "WHERE PlanNum=" + planNum;
        Db.NonQ(command);
        command = $@"UPDATE insbluebook SET
				insbluebook.GroupNum='{SOut.String(troj.POLICYNO)}',
				insbluebook.CarrierNum={troj.CarrierNum}
				WHERE insbluebook.PlanNum={planNum}";
        Db.NonQ(command);
        command = "UPDATE inssub SET "
                  + "BenefitNotes='" + SOut.String(troj.BenefitNotes) + "' "
                  + "WHERE PlanNum=" + planNum;
        Db.NonQ(command);
        if (updateBenefits)
        {
            //clear benefits
            command = "DELETE FROM benefit WHERE PlanNum=" + planNum;
            Db.NonQ(command);
            //benefitList
            for (var j = 0; j < troj.BenefitList.Count; j++)
            {
                troj.BenefitList[j].PlanNum = planNum;
                Benefits.Insert(troj.BenefitList[j]);
            }

            InsPlans.ComputeEstimatesForTrojanPlan(planNum);
        }
    }
}

[Serializable]
public class TrojanObject
{
    public string TROJANID;

    ///<summary>Employer name</summary>
    public string ENAME;

    ///<summary>GroupName</summary>
    public string PLANDESC;

    ///<summary>Carrier phone</summary>
    public string ELIGPHONE;

    ///<summary>GroupNum</summary>
    public string POLICYNO;

    ///<summary>Accepts eclaims</summary>
    public bool ECLAIMS;

    ///<summary>ElectID</summary>
    public string PAYERID;

    ///<summary>CarrierName</summary>
    public string MAILTO;

    ///<summary>Address</summary>
    public string MAILTOST;

    ///<summary>City</summary>
    public string MAILCITYONLY;

    ///<summary>State</summary>
    public string MAILSTATEONLY;

    ///<summary>Zip</summary>
    public string MAILZIPONLY;

    ///<summary>The only thing that will be missing from these benefits is the PlanNum.</summary>
    public List<Benefit> BenefitList;

    ///<summary>This can be filled at some point based on the carrier fields.</summary>
    public long CarrierNum;

    public string BenefitNotes;
    public string PlanNote;
    public int MonthRenewal;
}