#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsVerifyCrud
{
    public static InsVerify SelectOne(long insVerifyNum)
    {
        var command = "SELECT * FROM insverify "
                      + "WHERE InsVerifyNum = " + SOut.Long(insVerifyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsVerify SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsVerify> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsVerify> TableToList(DataTable table)
    {
        var retVal = new List<InsVerify>();
        InsVerify insVerify;
        foreach (DataRow row in table.Rows)
        {
            insVerify = new InsVerify();
            insVerify.InsVerifyNum = SIn.Long(row["InsVerifyNum"].ToString());
            insVerify.DateLastVerified = SIn.Date(row["DateLastVerified"].ToString());
            insVerify.UserNum = SIn.Long(row["UserNum"].ToString());
            insVerify.VerifyType = (VerifyTypes) SIn.Int(row["VerifyType"].ToString());
            insVerify.FKey = SIn.Long(row["FKey"].ToString());
            insVerify.DefNum = SIn.Long(row["DefNum"].ToString());
            insVerify.DateLastAssigned = SIn.Date(row["DateLastAssigned"].ToString());
            insVerify.Note = SIn.String(row["Note"].ToString());
            insVerify.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            insVerify.HoursAvailableForVerification = SIn.Double(row["HoursAvailableForVerification"].ToString());
            insVerify.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(insVerify);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsVerify> listInsVerifys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsVerify";
        var table = new DataTable(tableName);
        table.Columns.Add("InsVerifyNum");
        table.Columns.Add("DateLastVerified");
        table.Columns.Add("UserNum");
        table.Columns.Add("VerifyType");
        table.Columns.Add("FKey");
        table.Columns.Add("DefNum");
        table.Columns.Add("DateLastAssigned");
        table.Columns.Add("Note");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("HoursAvailableForVerification");
        table.Columns.Add("SecDateTEdit");
        foreach (var insVerify in listInsVerifys)
            table.Rows.Add(SOut.Long(insVerify.InsVerifyNum), SOut.DateT(insVerify.DateLastVerified, false), SOut.Long(insVerify.UserNum), SOut.Int((int) insVerify.VerifyType), SOut.Long(insVerify.FKey), SOut.Long(insVerify.DefNum), SOut.DateT(insVerify.DateLastAssigned, false), insVerify.Note, SOut.DateT(insVerify.DateTimeEntry, false), SOut.Double(insVerify.HoursAvailableForVerification), SOut.DateT(insVerify.SecDateTEdit, false));
        return table;
    }

    public static long Insert(InsVerify insVerify)
    {
        return Insert(insVerify, false);
    }

    public static long Insert(InsVerify insVerify, bool useExistingPK)
    {
        var command = "INSERT INTO insverify (";

        command += "DateLastVerified,UserNum,VerifyType,FKey,DefNum,DateLastAssigned,Note,DateTimeEntry,HoursAvailableForVerification) VALUES(";

        command +=
            SOut.Date(insVerify.DateLastVerified) + ","
                                                  + SOut.Long(insVerify.UserNum) + ","
                                                  + SOut.Int((int) insVerify.VerifyType) + ","
                                                  + SOut.Long(insVerify.FKey) + ","
                                                  + SOut.Long(insVerify.DefNum) + ","
                                                  + SOut.Date(insVerify.DateLastAssigned) + ","
                                                  + DbHelper.ParamChar + "paramNote,"
                                                  + DbHelper.Now() + ","
                                                  + SOut.Double(insVerify.HoursAvailableForVerification) + ")";
        //SecDateTEdit can only be set by MySQL
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerify.Note));
        {
            insVerify.InsVerifyNum = Db.NonQ(command, true, "InsVerifyNum", "insVerify", paramNote);
        }
        return insVerify.InsVerifyNum;
    }

    public static long InsertNoCache(InsVerify insVerify)
    {
        return InsertNoCache(insVerify, false);
    }

    public static long InsertNoCache(InsVerify insVerify, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insverify (";
        if (isRandomKeys || useExistingPK) command += "InsVerifyNum,";
        command += "DateLastVerified,UserNum,VerifyType,FKey,DefNum,DateLastAssigned,Note,DateTimeEntry,HoursAvailableForVerification) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insVerify.InsVerifyNum) + ",";
        command +=
            SOut.Date(insVerify.DateLastVerified) + ","
                                                  + SOut.Long(insVerify.UserNum) + ","
                                                  + SOut.Int((int) insVerify.VerifyType) + ","
                                                  + SOut.Long(insVerify.FKey) + ","
                                                  + SOut.Long(insVerify.DefNum) + ","
                                                  + SOut.Date(insVerify.DateLastAssigned) + ","
                                                  + DbHelper.ParamChar + "paramNote,"
                                                  + DbHelper.Now() + ","
                                                  + SOut.Double(insVerify.HoursAvailableForVerification) + ")";
        //SecDateTEdit can only be set by MySQL
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerify.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            insVerify.InsVerifyNum = Db.NonQ(command, true, "InsVerifyNum", "insVerify", paramNote);
        return insVerify.InsVerifyNum;
    }

    public static void Update(InsVerify insVerify)
    {
        var command = "UPDATE insverify SET "
                      + "DateLastVerified             =  " + SOut.Date(insVerify.DateLastVerified) + ", "
                      + "UserNum                      =  " + SOut.Long(insVerify.UserNum) + ", "
                      + "VerifyType                   =  " + SOut.Int((int) insVerify.VerifyType) + ", "
                      + "FKey                         =  " + SOut.Long(insVerify.FKey) + ", "
                      + "DefNum                       =  " + SOut.Long(insVerify.DefNum) + ", "
                      + "DateLastAssigned             =  " + SOut.Date(insVerify.DateLastAssigned) + ", "
                      + "Note                         =  " + DbHelper.ParamChar + "paramNote, "
                      //DateTimeEntry not allowed to change
                      + "HoursAvailableForVerification=  " + SOut.Double(insVerify.HoursAvailableForVerification) + " "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE InsVerifyNum = " + SOut.Long(insVerify.InsVerifyNum);
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerify.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(InsVerify insVerify, InsVerify oldInsVerify)
    {
        var command = "";
        if (insVerify.DateLastVerified.Date != oldInsVerify.DateLastVerified.Date)
        {
            if (command != "") command += ",";
            command += "DateLastVerified = " + SOut.Date(insVerify.DateLastVerified) + "";
        }

        if (insVerify.UserNum != oldInsVerify.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(insVerify.UserNum) + "";
        }

        if (insVerify.VerifyType != oldInsVerify.VerifyType)
        {
            if (command != "") command += ",";
            command += "VerifyType = " + SOut.Int((int) insVerify.VerifyType) + "";
        }

        if (insVerify.FKey != oldInsVerify.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insVerify.FKey) + "";
        }

        if (insVerify.DefNum != oldInsVerify.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(insVerify.DefNum) + "";
        }

        if (insVerify.DateLastAssigned.Date != oldInsVerify.DateLastAssigned.Date)
        {
            if (command != "") command += ",";
            command += "DateLastAssigned = " + SOut.Date(insVerify.DateLastAssigned) + "";
        }

        if (insVerify.Note != oldInsVerify.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        //DateTimeEntry not allowed to change
        if (insVerify.HoursAvailableForVerification != oldInsVerify.HoursAvailableForVerification)
        {
            if (command != "") command += ",";
            command += "HoursAvailableForVerification = " + SOut.Double(insVerify.HoursAvailableForVerification) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (insVerify.Note == null) insVerify.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerify.Note));
        command = "UPDATE insverify SET " + command
                                          + " WHERE InsVerifyNum = " + SOut.Long(insVerify.InsVerifyNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(InsVerify insVerify, InsVerify oldInsVerify)
    {
        if (insVerify.DateLastVerified.Date != oldInsVerify.DateLastVerified.Date) return true;
        if (insVerify.UserNum != oldInsVerify.UserNum) return true;
        if (insVerify.VerifyType != oldInsVerify.VerifyType) return true;
        if (insVerify.FKey != oldInsVerify.FKey) return true;
        if (insVerify.DefNum != oldInsVerify.DefNum) return true;
        if (insVerify.DateLastAssigned.Date != oldInsVerify.DateLastAssigned.Date) return true;
        if (insVerify.Note != oldInsVerify.Note) return true;
        //DateTimeEntry not allowed to change
        if (insVerify.HoursAvailableForVerification != oldInsVerify.HoursAvailableForVerification) return true;
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long insVerifyNum)
    {
        var command = "DELETE FROM insverify "
                      + "WHERE InsVerifyNum = " + SOut.Long(insVerifyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsVerifyNums)
    {
        if (listInsVerifyNums == null || listInsVerifyNums.Count == 0) return;
        var command = "DELETE FROM insverify "
                      + "WHERE InsVerifyNum IN(" + string.Join(",", listInsVerifyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}