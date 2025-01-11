#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcTPCrud
{
    public static ProcTP SelectOne(long procTPNum)
    {
        var command = "SELECT * FROM proctp "
                      + "WHERE ProcTPNum = " + SOut.Long(procTPNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcTP SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcTP> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcTP> TableToList(DataTable table)
    {
        var retVal = new List<ProcTP>();
        ProcTP procTP;
        foreach (DataRow row in table.Rows)
        {
            procTP = new ProcTP();
            procTP.ProcTPNum = SIn.Long(row["ProcTPNum"].ToString());
            procTP.TreatPlanNum = SIn.Long(row["TreatPlanNum"].ToString());
            procTP.PatNum = SIn.Long(row["PatNum"].ToString());
            procTP.ProcNumOrig = SIn.Long(row["ProcNumOrig"].ToString());
            procTP.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            procTP.Priority = SIn.Long(row["Priority"].ToString());
            procTP.ToothNumTP = SIn.String(row["ToothNumTP"].ToString());
            procTP.Surf = SIn.String(row["Surf"].ToString());
            procTP.ProcCode = SIn.String(row["ProcCode"].ToString());
            procTP.Descript = SIn.String(row["Descript"].ToString());
            procTP.FeeAmt = SIn.Double(row["FeeAmt"].ToString());
            procTP.PriInsAmt = SIn.Double(row["PriInsAmt"].ToString());
            procTP.SecInsAmt = SIn.Double(row["SecInsAmt"].ToString());
            procTP.PatAmt = SIn.Double(row["PatAmt"].ToString());
            procTP.Discount = SIn.Double(row["Discount"].ToString());
            procTP.Prognosis = SIn.String(row["Prognosis"].ToString());
            procTP.Dx = SIn.String(row["Dx"].ToString());
            procTP.ProcAbbr = SIn.String(row["ProcAbbr"].ToString());
            procTP.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            procTP.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            procTP.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            procTP.FeeAllowed = SIn.Double(row["FeeAllowed"].ToString());
            procTP.TaxAmt = SIn.Double(row["TaxAmt"].ToString());
            procTP.ProvNum = SIn.Long(row["ProvNum"].ToString());
            procTP.DateTP = SIn.Date(row["DateTP"].ToString());
            procTP.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            procTP.CatPercUCR = SIn.Double(row["CatPercUCR"].ToString());
            retVal.Add(procTP);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcTP> listProcTPs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcTP";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcTPNum");
        table.Columns.Add("TreatPlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProcNumOrig");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Priority");
        table.Columns.Add("ToothNumTP");
        table.Columns.Add("Surf");
        table.Columns.Add("ProcCode");
        table.Columns.Add("Descript");
        table.Columns.Add("FeeAmt");
        table.Columns.Add("PriInsAmt");
        table.Columns.Add("SecInsAmt");
        table.Columns.Add("PatAmt");
        table.Columns.Add("Discount");
        table.Columns.Add("Prognosis");
        table.Columns.Add("Dx");
        table.Columns.Add("ProcAbbr");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("FeeAllowed");
        table.Columns.Add("TaxAmt");
        table.Columns.Add("ProvNum");
        table.Columns.Add("DateTP");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("CatPercUCR");
        foreach (var procTP in listProcTPs)
            table.Rows.Add(SOut.Long(procTP.ProcTPNum), SOut.Long(procTP.TreatPlanNum), SOut.Long(procTP.PatNum), SOut.Long(procTP.ProcNumOrig), SOut.Int(procTP.ItemOrder), SOut.Long(procTP.Priority), procTP.ToothNumTP, procTP.Surf, procTP.ProcCode, procTP.Descript, SOut.Double(procTP.FeeAmt), SOut.Double(procTP.PriInsAmt), SOut.Double(procTP.SecInsAmt), SOut.Double(procTP.PatAmt), SOut.Double(procTP.Discount), procTP.Prognosis, procTP.Dx, procTP.ProcAbbr, SOut.Long(procTP.SecUserNumEntry), SOut.DateT(procTP.SecDateEntry, false), SOut.DateT(procTP.SecDateTEdit, false), SOut.Double(procTP.FeeAllowed), SOut.Double(procTP.TaxAmt), SOut.Long(procTP.ProvNum), SOut.DateT(procTP.DateTP, false), SOut.Long(procTP.ClinicNum), SOut.Double(procTP.CatPercUCR));
        return table;
    }

    public static long Insert(ProcTP procTP)
    {
        return Insert(procTP, false);
    }

    public static long Insert(ProcTP procTP, bool useExistingPK)
    {
        var command = "INSERT INTO proctp (";

        command += "TreatPlanNum,PatNum,ProcNumOrig,ItemOrder,Priority,ToothNumTP,Surf,ProcCode,Descript,FeeAmt,PriInsAmt,SecInsAmt,PatAmt,Discount,Prognosis,Dx,ProcAbbr,SecUserNumEntry,SecDateEntry,FeeAllowed,TaxAmt,ProvNum,DateTP,ClinicNum,CatPercUCR) VALUES(";

        command +=
            SOut.Long(procTP.TreatPlanNum) + ","
                                           + SOut.Long(procTP.PatNum) + ","
                                           + SOut.Long(procTP.ProcNumOrig) + ","
                                           + SOut.Int(procTP.ItemOrder) + ","
                                           + SOut.Long(procTP.Priority) + ","
                                           + "'" + SOut.String(procTP.ToothNumTP) + "',"
                                           + "'" + SOut.String(procTP.Surf) + "',"
                                           + "'" + SOut.String(procTP.ProcCode) + "',"
                                           + "'" + SOut.String(procTP.Descript) + "',"
                                           + SOut.Double(procTP.FeeAmt) + ","
                                           + SOut.Double(procTP.PriInsAmt) + ","
                                           + SOut.Double(procTP.SecInsAmt) + ","
                                           + SOut.Double(procTP.PatAmt) + ","
                                           + SOut.Double(procTP.Discount) + ","
                                           + "'" + SOut.String(procTP.Prognosis) + "',"
                                           + "'" + SOut.String(procTP.Dx) + "',"
                                           + "'" + SOut.String(procTP.ProcAbbr) + "',"
                                           + SOut.Long(procTP.SecUserNumEntry) + ","
                                           + DbHelper.Now() + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + SOut.Double(procTP.FeeAllowed) + ","
                                           + SOut.Double(procTP.TaxAmt) + ","
                                           + SOut.Long(procTP.ProvNum) + ","
                                           + SOut.Date(procTP.DateTP) + ","
                                           + SOut.Long(procTP.ClinicNum) + ","
                                           + SOut.Double(procTP.CatPercUCR) + ")";
        {
            procTP.ProcTPNum = Db.NonQ(command, true, "ProcTPNum", "procTP");
        }
        return procTP.ProcTPNum;
    }

    public static long InsertNoCache(ProcTP procTP)
    {
        return InsertNoCache(procTP, false);
    }

    public static long InsertNoCache(ProcTP procTP, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO proctp (";
        if (isRandomKeys || useExistingPK) command += "ProcTPNum,";
        command += "TreatPlanNum,PatNum,ProcNumOrig,ItemOrder,Priority,ToothNumTP,Surf,ProcCode,Descript,FeeAmt,PriInsAmt,SecInsAmt,PatAmt,Discount,Prognosis,Dx,ProcAbbr,SecUserNumEntry,SecDateEntry,FeeAllowed,TaxAmt,ProvNum,DateTP,ClinicNum,CatPercUCR) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procTP.ProcTPNum) + ",";
        command +=
            SOut.Long(procTP.TreatPlanNum) + ","
                                           + SOut.Long(procTP.PatNum) + ","
                                           + SOut.Long(procTP.ProcNumOrig) + ","
                                           + SOut.Int(procTP.ItemOrder) + ","
                                           + SOut.Long(procTP.Priority) + ","
                                           + "'" + SOut.String(procTP.ToothNumTP) + "',"
                                           + "'" + SOut.String(procTP.Surf) + "',"
                                           + "'" + SOut.String(procTP.ProcCode) + "',"
                                           + "'" + SOut.String(procTP.Descript) + "',"
                                           + SOut.Double(procTP.FeeAmt) + ","
                                           + SOut.Double(procTP.PriInsAmt) + ","
                                           + SOut.Double(procTP.SecInsAmt) + ","
                                           + SOut.Double(procTP.PatAmt) + ","
                                           + SOut.Double(procTP.Discount) + ","
                                           + "'" + SOut.String(procTP.Prognosis) + "',"
                                           + "'" + SOut.String(procTP.Dx) + "',"
                                           + "'" + SOut.String(procTP.ProcAbbr) + "',"
                                           + SOut.Long(procTP.SecUserNumEntry) + ","
                                           + DbHelper.Now() + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + SOut.Double(procTP.FeeAllowed) + ","
                                           + SOut.Double(procTP.TaxAmt) + ","
                                           + SOut.Long(procTP.ProvNum) + ","
                                           + SOut.Date(procTP.DateTP) + ","
                                           + SOut.Long(procTP.ClinicNum) + ","
                                           + SOut.Double(procTP.CatPercUCR) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procTP.ProcTPNum = Db.NonQ(command, true, "ProcTPNum", "procTP");
        return procTP.ProcTPNum;
    }

    public static void Update(ProcTP procTP)
    {
        var command = "UPDATE proctp SET "
                      + "TreatPlanNum   =  " + SOut.Long(procTP.TreatPlanNum) + ", "
                      + "PatNum         =  " + SOut.Long(procTP.PatNum) + ", "
                      + "ProcNumOrig    =  " + SOut.Long(procTP.ProcNumOrig) + ", "
                      + "ItemOrder      =  " + SOut.Int(procTP.ItemOrder) + ", "
                      + "Priority       =  " + SOut.Long(procTP.Priority) + ", "
                      + "ToothNumTP     = '" + SOut.String(procTP.ToothNumTP) + "', "
                      + "Surf           = '" + SOut.String(procTP.Surf) + "', "
                      + "ProcCode       = '" + SOut.String(procTP.ProcCode) + "', "
                      + "Descript       = '" + SOut.String(procTP.Descript) + "', "
                      + "FeeAmt         =  " + SOut.Double(procTP.FeeAmt) + ", "
                      + "PriInsAmt      =  " + SOut.Double(procTP.PriInsAmt) + ", "
                      + "SecInsAmt      =  " + SOut.Double(procTP.SecInsAmt) + ", "
                      + "PatAmt         =  " + SOut.Double(procTP.PatAmt) + ", "
                      + "Discount       =  " + SOut.Double(procTP.Discount) + ", "
                      + "Prognosis      = '" + SOut.String(procTP.Prognosis) + "', "
                      + "Dx             = '" + SOut.String(procTP.Dx) + "', "
                      + "ProcAbbr       = '" + SOut.String(procTP.ProcAbbr) + "', "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "FeeAllowed     =  " + SOut.Double(procTP.FeeAllowed) + ", "
                      + "TaxAmt         =  " + SOut.Double(procTP.TaxAmt) + ", "
                      + "ProvNum        =  " + SOut.Long(procTP.ProvNum) + ", "
                      + "DateTP         =  " + SOut.Date(procTP.DateTP) + ", "
                      + "ClinicNum      =  " + SOut.Long(procTP.ClinicNum) + ", "
                      + "CatPercUCR     =  " + SOut.Double(procTP.CatPercUCR) + " "
                      + "WHERE ProcTPNum = " + SOut.Long(procTP.ProcTPNum);
        Db.NonQ(command);
    }

    public static bool Update(ProcTP procTP, ProcTP oldProcTP)
    {
        var command = "";
        if (procTP.TreatPlanNum != oldProcTP.TreatPlanNum)
        {
            if (command != "") command += ",";
            command += "TreatPlanNum = " + SOut.Long(procTP.TreatPlanNum) + "";
        }

        if (procTP.PatNum != oldProcTP.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(procTP.PatNum) + "";
        }

        if (procTP.ProcNumOrig != oldProcTP.ProcNumOrig)
        {
            if (command != "") command += ",";
            command += "ProcNumOrig = " + SOut.Long(procTP.ProcNumOrig) + "";
        }

        if (procTP.ItemOrder != oldProcTP.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(procTP.ItemOrder) + "";
        }

        if (procTP.Priority != oldProcTP.Priority)
        {
            if (command != "") command += ",";
            command += "Priority = " + SOut.Long(procTP.Priority) + "";
        }

        if (procTP.ToothNumTP != oldProcTP.ToothNumTP)
        {
            if (command != "") command += ",";
            command += "ToothNumTP = '" + SOut.String(procTP.ToothNumTP) + "'";
        }

        if (procTP.Surf != oldProcTP.Surf)
        {
            if (command != "") command += ",";
            command += "Surf = '" + SOut.String(procTP.Surf) + "'";
        }

        if (procTP.ProcCode != oldProcTP.ProcCode)
        {
            if (command != "") command += ",";
            command += "ProcCode = '" + SOut.String(procTP.ProcCode) + "'";
        }

        if (procTP.Descript != oldProcTP.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(procTP.Descript) + "'";
        }

        if (procTP.FeeAmt != oldProcTP.FeeAmt)
        {
            if (command != "") command += ",";
            command += "FeeAmt = " + SOut.Double(procTP.FeeAmt) + "";
        }

        if (procTP.PriInsAmt != oldProcTP.PriInsAmt)
        {
            if (command != "") command += ",";
            command += "PriInsAmt = " + SOut.Double(procTP.PriInsAmt) + "";
        }

        if (procTP.SecInsAmt != oldProcTP.SecInsAmt)
        {
            if (command != "") command += ",";
            command += "SecInsAmt = " + SOut.Double(procTP.SecInsAmt) + "";
        }

        if (procTP.PatAmt != oldProcTP.PatAmt)
        {
            if (command != "") command += ",";
            command += "PatAmt = " + SOut.Double(procTP.PatAmt) + "";
        }

        if (procTP.Discount != oldProcTP.Discount)
        {
            if (command != "") command += ",";
            command += "Discount = " + SOut.Double(procTP.Discount) + "";
        }

        if (procTP.Prognosis != oldProcTP.Prognosis)
        {
            if (command != "") command += ",";
            command += "Prognosis = '" + SOut.String(procTP.Prognosis) + "'";
        }

        if (procTP.Dx != oldProcTP.Dx)
        {
            if (command != "") command += ",";
            command += "Dx = '" + SOut.String(procTP.Dx) + "'";
        }

        if (procTP.ProcAbbr != oldProcTP.ProcAbbr)
        {
            if (command != "") command += ",";
            command += "ProcAbbr = '" + SOut.String(procTP.ProcAbbr) + "'";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (procTP.FeeAllowed != oldProcTP.FeeAllowed)
        {
            if (command != "") command += ",";
            command += "FeeAllowed = " + SOut.Double(procTP.FeeAllowed) + "";
        }

        if (procTP.TaxAmt != oldProcTP.TaxAmt)
        {
            if (command != "") command += ",";
            command += "TaxAmt = " + SOut.Double(procTP.TaxAmt) + "";
        }

        if (procTP.ProvNum != oldProcTP.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(procTP.ProvNum) + "";
        }

        if (procTP.DateTP.Date != oldProcTP.DateTP.Date)
        {
            if (command != "") command += ",";
            command += "DateTP = " + SOut.Date(procTP.DateTP) + "";
        }

        if (procTP.ClinicNum != oldProcTP.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(procTP.ClinicNum) + "";
        }

        if (procTP.CatPercUCR != oldProcTP.CatPercUCR)
        {
            if (command != "") command += ",";
            command += "CatPercUCR = " + SOut.Double(procTP.CatPercUCR) + "";
        }

        if (command == "") return false;
        command = "UPDATE proctp SET " + command
                                       + " WHERE ProcTPNum = " + SOut.Long(procTP.ProcTPNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProcTP procTP, ProcTP oldProcTP)
    {
        if (procTP.TreatPlanNum != oldProcTP.TreatPlanNum) return true;
        if (procTP.PatNum != oldProcTP.PatNum) return true;
        if (procTP.ProcNumOrig != oldProcTP.ProcNumOrig) return true;
        if (procTP.ItemOrder != oldProcTP.ItemOrder) return true;
        if (procTP.Priority != oldProcTP.Priority) return true;
        if (procTP.ToothNumTP != oldProcTP.ToothNumTP) return true;
        if (procTP.Surf != oldProcTP.Surf) return true;
        if (procTP.ProcCode != oldProcTP.ProcCode) return true;
        if (procTP.Descript != oldProcTP.Descript) return true;
        if (procTP.FeeAmt != oldProcTP.FeeAmt) return true;
        if (procTP.PriInsAmt != oldProcTP.PriInsAmt) return true;
        if (procTP.SecInsAmt != oldProcTP.SecInsAmt) return true;
        if (procTP.PatAmt != oldProcTP.PatAmt) return true;
        if (procTP.Discount != oldProcTP.Discount) return true;
        if (procTP.Prognosis != oldProcTP.Prognosis) return true;
        if (procTP.Dx != oldProcTP.Dx) return true;
        if (procTP.ProcAbbr != oldProcTP.ProcAbbr) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (procTP.FeeAllowed != oldProcTP.FeeAllowed) return true;
        if (procTP.TaxAmt != oldProcTP.TaxAmt) return true;
        if (procTP.ProvNum != oldProcTP.ProvNum) return true;
        if (procTP.DateTP.Date != oldProcTP.DateTP.Date) return true;
        if (procTP.ClinicNum != oldProcTP.ClinicNum) return true;
        if (procTP.CatPercUCR != oldProcTP.CatPercUCR) return true;
        return false;
    }

    public static void Delete(long procTPNum)
    {
        var command = "DELETE FROM proctp "
                      + "WHERE ProcTPNum = " + SOut.Long(procTPNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcTPNums)
    {
        if (listProcTPNums == null || listProcTPNums.Count == 0) return;
        var command = "DELETE FROM proctp "
                      + "WHERE ProcTPNum IN(" + string.Join(",", listProcTPNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}