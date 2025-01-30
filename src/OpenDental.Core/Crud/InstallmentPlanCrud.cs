using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InstallmentPlanCrud
{
    public static InstallmentPlan SelectOne(long installmentPlanNum)
    {
        var command = "SELECT * FROM installmentplan "
                      + "WHERE InstallmentPlanNum = " + SOut.Long(installmentPlanNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InstallmentPlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InstallmentPlan> TableToList(DataTable table)
    {
        var retVal = new List<InstallmentPlan>();
        InstallmentPlan installmentPlan;
        foreach (DataRow row in table.Rows)
        {
            installmentPlan = new InstallmentPlan();
            installmentPlan.InstallmentPlanNum = SIn.Long(row["InstallmentPlanNum"].ToString());
            installmentPlan.PatNum = SIn.Long(row["PatNum"].ToString());
            installmentPlan.DateAgreement = SIn.Date(row["DateAgreement"].ToString());
            installmentPlan.DateFirstPayment = SIn.Date(row["DateFirstPayment"].ToString());
            installmentPlan.MonthlyPayment = SIn.Double(row["MonthlyPayment"].ToString());
            installmentPlan.APR = SIn.Float(row["APR"].ToString());
            installmentPlan.Note = SIn.String(row["Note"].ToString());
            retVal.Add(installmentPlan);
        }

        return retVal;
    }

    public static void Insert(InstallmentPlan installmentPlan)
    {
        var command = "INSERT INTO installmentplan (";

        command += "PatNum,DateAgreement,DateFirstPayment,MonthlyPayment,APR,Note) VALUES(";

        command +=
            SOut.Long(installmentPlan.PatNum) + ","
                                              + SOut.Date(installmentPlan.DateAgreement) + ","
                                              + SOut.Date(installmentPlan.DateFirstPayment) + ","
                                              + SOut.Double(installmentPlan.MonthlyPayment) + ","
                                              + SOut.Float(installmentPlan.APR) + ","
                                              + "'" + SOut.String(installmentPlan.Note) + "')";
        {
            installmentPlan.InstallmentPlanNum = Db.NonQ(command, true, "InstallmentPlanNum", "installmentPlan");
        }
    }

    public static void Update(InstallmentPlan installmentPlan)
    {
        var command = "UPDATE installmentplan SET "
                      + "PatNum            =  " + SOut.Long(installmentPlan.PatNum) + ", "
                      + "DateAgreement     =  " + SOut.Date(installmentPlan.DateAgreement) + ", "
                      + "DateFirstPayment  =  " + SOut.Date(installmentPlan.DateFirstPayment) + ", "
                      + "MonthlyPayment    =  " + SOut.Double(installmentPlan.MonthlyPayment) + ", "
                      + "APR               =  " + SOut.Float(installmentPlan.APR) + ", "
                      + "Note              = '" + SOut.String(installmentPlan.Note) + "' "
                      + "WHERE InstallmentPlanNum = " + SOut.Long(installmentPlan.InstallmentPlanNum);
        Db.NonQ(command);
    }
}