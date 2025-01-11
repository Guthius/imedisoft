#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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

    public static InstallmentPlan SelectOne(string command)
    {
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

    public static DataTable ListToTable(List<InstallmentPlan> listInstallmentPlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InstallmentPlan";
        var table = new DataTable(tableName);
        table.Columns.Add("InstallmentPlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateAgreement");
        table.Columns.Add("DateFirstPayment");
        table.Columns.Add("MonthlyPayment");
        table.Columns.Add("APR");
        table.Columns.Add("Note");
        foreach (var installmentPlan in listInstallmentPlans)
            table.Rows.Add(SOut.Long(installmentPlan.InstallmentPlanNum), SOut.Long(installmentPlan.PatNum), SOut.DateT(installmentPlan.DateAgreement, false), SOut.DateT(installmentPlan.DateFirstPayment, false), SOut.Double(installmentPlan.MonthlyPayment), SOut.Float(installmentPlan.APR), installmentPlan.Note);
        return table;
    }

    public static long Insert(InstallmentPlan installmentPlan)
    {
        return Insert(installmentPlan, false);
    }

    public static long Insert(InstallmentPlan installmentPlan, bool useExistingPK)
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
        return installmentPlan.InstallmentPlanNum;
    }

    public static long InsertNoCache(InstallmentPlan installmentPlan)
    {
        return InsertNoCache(installmentPlan, false);
    }

    public static long InsertNoCache(InstallmentPlan installmentPlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO installmentplan (";
        if (isRandomKeys || useExistingPK) command += "InstallmentPlanNum,";
        command += "PatNum,DateAgreement,DateFirstPayment,MonthlyPayment,APR,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(installmentPlan.InstallmentPlanNum) + ",";
        command +=
            SOut.Long(installmentPlan.PatNum) + ","
                                              + SOut.Date(installmentPlan.DateAgreement) + ","
                                              + SOut.Date(installmentPlan.DateFirstPayment) + ","
                                              + SOut.Double(installmentPlan.MonthlyPayment) + ","
                                              + SOut.Float(installmentPlan.APR) + ","
                                              + "'" + SOut.String(installmentPlan.Note) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            installmentPlan.InstallmentPlanNum = Db.NonQ(command, true, "InstallmentPlanNum", "installmentPlan");
        return installmentPlan.InstallmentPlanNum;
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

    public static bool Update(InstallmentPlan installmentPlan, InstallmentPlan oldInstallmentPlan)
    {
        var command = "";
        if (installmentPlan.PatNum != oldInstallmentPlan.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(installmentPlan.PatNum) + "";
        }

        if (installmentPlan.DateAgreement.Date != oldInstallmentPlan.DateAgreement.Date)
        {
            if (command != "") command += ",";
            command += "DateAgreement = " + SOut.Date(installmentPlan.DateAgreement) + "";
        }

        if (installmentPlan.DateFirstPayment.Date != oldInstallmentPlan.DateFirstPayment.Date)
        {
            if (command != "") command += ",";
            command += "DateFirstPayment = " + SOut.Date(installmentPlan.DateFirstPayment) + "";
        }

        if (installmentPlan.MonthlyPayment != oldInstallmentPlan.MonthlyPayment)
        {
            if (command != "") command += ",";
            command += "MonthlyPayment = " + SOut.Double(installmentPlan.MonthlyPayment) + "";
        }

        if (installmentPlan.APR != oldInstallmentPlan.APR)
        {
            if (command != "") command += ",";
            command += "APR = " + SOut.Float(installmentPlan.APR) + "";
        }

        if (installmentPlan.Note != oldInstallmentPlan.Note)
        {
            if (command != "") command += ",";
            command += "Note = '" + SOut.String(installmentPlan.Note) + "'";
        }

        if (command == "") return false;
        command = "UPDATE installmentplan SET " + command
                                                + " WHERE InstallmentPlanNum = " + SOut.Long(installmentPlan.InstallmentPlanNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InstallmentPlan installmentPlan, InstallmentPlan oldInstallmentPlan)
    {
        if (installmentPlan.PatNum != oldInstallmentPlan.PatNum) return true;
        if (installmentPlan.DateAgreement.Date != oldInstallmentPlan.DateAgreement.Date) return true;
        if (installmentPlan.DateFirstPayment.Date != oldInstallmentPlan.DateFirstPayment.Date) return true;
        if (installmentPlan.MonthlyPayment != oldInstallmentPlan.MonthlyPayment) return true;
        if (installmentPlan.APR != oldInstallmentPlan.APR) return true;
        if (installmentPlan.Note != oldInstallmentPlan.Note) return true;
        return false;
    }

    public static void Delete(long installmentPlanNum)
    {
        var command = "DELETE FROM installmentplan "
                      + "WHERE InstallmentPlanNum = " + SOut.Long(installmentPlanNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInstallmentPlanNums)
    {
        if (listInstallmentPlanNums == null || listInstallmentPlanNums.Count == 0) return;
        var command = "DELETE FROM installmentplan "
                      + "WHERE InstallmentPlanNum IN(" + string.Join(",", listInstallmentPlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}