#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LabCaseCrud
{
    public static LabCase SelectOne(long labCaseNum)
    {
        var command = "SELECT * FROM labcase "
                      + "WHERE LabCaseNum = " + SOut.Long(labCaseNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LabCase SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LabCase> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LabCase> TableToList(DataTable table)
    {
        var retVal = new List<LabCase>();
        LabCase labCase;
        foreach (DataRow row in table.Rows)
        {
            labCase = new LabCase();
            labCase.LabCaseNum = SIn.Long(row["LabCaseNum"].ToString());
            labCase.PatNum = SIn.Long(row["PatNum"].ToString());
            labCase.LaboratoryNum = SIn.Long(row["LaboratoryNum"].ToString());
            labCase.AptNum = SIn.Long(row["AptNum"].ToString());
            labCase.PlannedAptNum = SIn.Long(row["PlannedAptNum"].ToString());
            labCase.DateTimeDue = SIn.DateTime(row["DateTimeDue"].ToString());
            labCase.DateTimeCreated = SIn.DateTime(row["DateTimeCreated"].ToString());
            labCase.DateTimeSent = SIn.DateTime(row["DateTimeSent"].ToString());
            labCase.DateTimeRecd = SIn.DateTime(row["DateTimeRecd"].ToString());
            labCase.DateTimeChecked = SIn.DateTime(row["DateTimeChecked"].ToString());
            labCase.ProvNum = SIn.Long(row["ProvNum"].ToString());
            labCase.Instructions = SIn.String(row["Instructions"].ToString());
            labCase.LabFee = SIn.Double(row["LabFee"].ToString());
            labCase.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            labCase.InvoiceNum = SIn.String(row["InvoiceNum"].ToString());
            retVal.Add(labCase);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LabCase> listLabCases, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LabCase";
        var table = new DataTable(tableName);
        table.Columns.Add("LabCaseNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("LaboratoryNum");
        table.Columns.Add("AptNum");
        table.Columns.Add("PlannedAptNum");
        table.Columns.Add("DateTimeDue");
        table.Columns.Add("DateTimeCreated");
        table.Columns.Add("DateTimeSent");
        table.Columns.Add("DateTimeRecd");
        table.Columns.Add("DateTimeChecked");
        table.Columns.Add("ProvNum");
        table.Columns.Add("Instructions");
        table.Columns.Add("LabFee");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("InvoiceNum");
        foreach (var labCase in listLabCases)
            table.Rows.Add(SOut.Long(labCase.LabCaseNum), SOut.Long(labCase.PatNum), SOut.Long(labCase.LaboratoryNum), SOut.Long(labCase.AptNum), SOut.Long(labCase.PlannedAptNum), SOut.DateT(labCase.DateTimeDue, false), SOut.DateT(labCase.DateTimeCreated, false), SOut.DateT(labCase.DateTimeSent, false), SOut.DateT(labCase.DateTimeRecd, false), SOut.DateT(labCase.DateTimeChecked, false), SOut.Long(labCase.ProvNum), labCase.Instructions, SOut.Double(labCase.LabFee), SOut.DateT(labCase.DateTStamp, false), labCase.InvoiceNum);
        return table;
    }

    public static long Insert(LabCase labCase)
    {
        return Insert(labCase, false);
    }

    public static long Insert(LabCase labCase, bool useExistingPK)
    {
        var command = "INSERT INTO labcase (";

        command += "PatNum,LaboratoryNum,AptNum,PlannedAptNum,DateTimeDue,DateTimeCreated,DateTimeSent,DateTimeRecd,DateTimeChecked,ProvNum,Instructions,LabFee,InvoiceNum) VALUES(";

        command +=
            SOut.Long(labCase.PatNum) + ","
                                      + SOut.Long(labCase.LaboratoryNum) + ","
                                      + SOut.Long(labCase.AptNum) + ","
                                      + SOut.Long(labCase.PlannedAptNum) + ","
                                      + SOut.DateT(labCase.DateTimeDue) + ","
                                      + SOut.DateT(labCase.DateTimeCreated) + ","
                                      + SOut.DateT(labCase.DateTimeSent) + ","
                                      + SOut.DateT(labCase.DateTimeRecd) + ","
                                      + SOut.DateT(labCase.DateTimeChecked) + ","
                                      + SOut.Long(labCase.ProvNum) + ","
                                      + DbHelper.ParamChar + "paramInstructions,"
                                      + SOut.Double(labCase.LabFee) + ","
                                      //DateTStamp can only be set by MySQL
                                      + "'" + SOut.String(labCase.InvoiceNum) + "')";
        if (labCase.Instructions == null) labCase.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(labCase.Instructions));
        {
            labCase.LabCaseNum = Db.NonQ(command, true, "LabCaseNum", "labCase", paramInstructions);
        }
        return labCase.LabCaseNum;
    }

    public static long InsertNoCache(LabCase labCase)
    {
        return InsertNoCache(labCase, false);
    }

    public static long InsertNoCache(LabCase labCase, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO labcase (";
        if (isRandomKeys || useExistingPK) command += "LabCaseNum,";
        command += "PatNum,LaboratoryNum,AptNum,PlannedAptNum,DateTimeDue,DateTimeCreated,DateTimeSent,DateTimeRecd,DateTimeChecked,ProvNum,Instructions,LabFee,InvoiceNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(labCase.LabCaseNum) + ",";
        command +=
            SOut.Long(labCase.PatNum) + ","
                                      + SOut.Long(labCase.LaboratoryNum) + ","
                                      + SOut.Long(labCase.AptNum) + ","
                                      + SOut.Long(labCase.PlannedAptNum) + ","
                                      + SOut.DateT(labCase.DateTimeDue) + ","
                                      + SOut.DateT(labCase.DateTimeCreated) + ","
                                      + SOut.DateT(labCase.DateTimeSent) + ","
                                      + SOut.DateT(labCase.DateTimeRecd) + ","
                                      + SOut.DateT(labCase.DateTimeChecked) + ","
                                      + SOut.Long(labCase.ProvNum) + ","
                                      + DbHelper.ParamChar + "paramInstructions,"
                                      + SOut.Double(labCase.LabFee) + ","
                                      //DateTStamp can only be set by MySQL
                                      + "'" + SOut.String(labCase.InvoiceNum) + "')";
        if (labCase.Instructions == null) labCase.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(labCase.Instructions));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramInstructions);
        else
            labCase.LabCaseNum = Db.NonQ(command, true, "LabCaseNum", "labCase", paramInstructions);
        return labCase.LabCaseNum;
    }

    public static void Update(LabCase labCase)
    {
        var command = "UPDATE labcase SET "
                      + "PatNum         =  " + SOut.Long(labCase.PatNum) + ", "
                      + "LaboratoryNum  =  " + SOut.Long(labCase.LaboratoryNum) + ", "
                      + "AptNum         =  " + SOut.Long(labCase.AptNum) + ", "
                      + "PlannedAptNum  =  " + SOut.Long(labCase.PlannedAptNum) + ", "
                      + "DateTimeDue    =  " + SOut.DateT(labCase.DateTimeDue) + ", "
                      + "DateTimeCreated=  " + SOut.DateT(labCase.DateTimeCreated) + ", "
                      + "DateTimeSent   =  " + SOut.DateT(labCase.DateTimeSent) + ", "
                      + "DateTimeRecd   =  " + SOut.DateT(labCase.DateTimeRecd) + ", "
                      + "DateTimeChecked=  " + SOut.DateT(labCase.DateTimeChecked) + ", "
                      + "ProvNum        =  " + SOut.Long(labCase.ProvNum) + ", "
                      + "Instructions   =  " + DbHelper.ParamChar + "paramInstructions, "
                      + "LabFee         =  " + SOut.Double(labCase.LabFee) + ", "
                      //DateTStamp can only be set by MySQL
                      + "InvoiceNum     = '" + SOut.String(labCase.InvoiceNum) + "' "
                      + "WHERE LabCaseNum = " + SOut.Long(labCase.LabCaseNum);
        if (labCase.Instructions == null) labCase.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(labCase.Instructions));
        Db.NonQ(command, paramInstructions);
    }

    public static bool Update(LabCase labCase, LabCase oldLabCase)
    {
        var command = "";
        if (labCase.PatNum != oldLabCase.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(labCase.PatNum) + "";
        }

        if (labCase.LaboratoryNum != oldLabCase.LaboratoryNum)
        {
            if (command != "") command += ",";
            command += "LaboratoryNum = " + SOut.Long(labCase.LaboratoryNum) + "";
        }

        if (labCase.AptNum != oldLabCase.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(labCase.AptNum) + "";
        }

        if (labCase.PlannedAptNum != oldLabCase.PlannedAptNum)
        {
            if (command != "") command += ",";
            command += "PlannedAptNum = " + SOut.Long(labCase.PlannedAptNum) + "";
        }

        if (labCase.DateTimeDue != oldLabCase.DateTimeDue)
        {
            if (command != "") command += ",";
            command += "DateTimeDue = " + SOut.DateT(labCase.DateTimeDue) + "";
        }

        if (labCase.DateTimeCreated != oldLabCase.DateTimeCreated)
        {
            if (command != "") command += ",";
            command += "DateTimeCreated = " + SOut.DateT(labCase.DateTimeCreated) + "";
        }

        if (labCase.DateTimeSent != oldLabCase.DateTimeSent)
        {
            if (command != "") command += ",";
            command += "DateTimeSent = " + SOut.DateT(labCase.DateTimeSent) + "";
        }

        if (labCase.DateTimeRecd != oldLabCase.DateTimeRecd)
        {
            if (command != "") command += ",";
            command += "DateTimeRecd = " + SOut.DateT(labCase.DateTimeRecd) + "";
        }

        if (labCase.DateTimeChecked != oldLabCase.DateTimeChecked)
        {
            if (command != "") command += ",";
            command += "DateTimeChecked = " + SOut.DateT(labCase.DateTimeChecked) + "";
        }

        if (labCase.ProvNum != oldLabCase.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(labCase.ProvNum) + "";
        }

        if (labCase.Instructions != oldLabCase.Instructions)
        {
            if (command != "") command += ",";
            command += "Instructions = " + DbHelper.ParamChar + "paramInstructions";
        }

        if (labCase.LabFee != oldLabCase.LabFee)
        {
            if (command != "") command += ",";
            command += "LabFee = " + SOut.Double(labCase.LabFee) + "";
        }

        //DateTStamp can only be set by MySQL
        if (labCase.InvoiceNum != oldLabCase.InvoiceNum)
        {
            if (command != "") command += ",";
            command += "InvoiceNum = '" + SOut.String(labCase.InvoiceNum) + "'";
        }

        if (command == "") return false;
        if (labCase.Instructions == null) labCase.Instructions = "";
        var paramInstructions = new OdSqlParameter("paramInstructions", OdDbType.Text, SOut.StringParam(labCase.Instructions));
        command = "UPDATE labcase SET " + command
                                        + " WHERE LabCaseNum = " + SOut.Long(labCase.LabCaseNum);
        Db.NonQ(command, paramInstructions);
        return true;
    }

    public static bool UpdateComparison(LabCase labCase, LabCase oldLabCase)
    {
        if (labCase.PatNum != oldLabCase.PatNum) return true;
        if (labCase.LaboratoryNum != oldLabCase.LaboratoryNum) return true;
        if (labCase.AptNum != oldLabCase.AptNum) return true;
        if (labCase.PlannedAptNum != oldLabCase.PlannedAptNum) return true;
        if (labCase.DateTimeDue != oldLabCase.DateTimeDue) return true;
        if (labCase.DateTimeCreated != oldLabCase.DateTimeCreated) return true;
        if (labCase.DateTimeSent != oldLabCase.DateTimeSent) return true;
        if (labCase.DateTimeRecd != oldLabCase.DateTimeRecd) return true;
        if (labCase.DateTimeChecked != oldLabCase.DateTimeChecked) return true;
        if (labCase.ProvNum != oldLabCase.ProvNum) return true;
        if (labCase.Instructions != oldLabCase.Instructions) return true;
        if (labCase.LabFee != oldLabCase.LabFee) return true;
        //DateTStamp can only be set by MySQL
        if (labCase.InvoiceNum != oldLabCase.InvoiceNum) return true;
        return false;
    }

    public static void Delete(long labCaseNum)
    {
        var command = "DELETE FROM labcase "
                      + "WHERE LabCaseNum = " + SOut.Long(labCaseNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLabCaseNums)
    {
        if (listLabCaseNums == null || listLabCaseNums.Count == 0) return;
        var command = "DELETE FROM labcase "
                      + "WHERE LabCaseNum IN(" + string.Join(",", listLabCaseNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}