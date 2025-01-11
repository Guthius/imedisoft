#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsVerifyHistCrud
{
    public static InsVerifyHist SelectOne(long insVerifyHistNum)
    {
        var command = "SELECT * FROM insverifyhist "
                      + "WHERE InsVerifyHistNum = " + SOut.Long(insVerifyHistNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsVerifyHist SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsVerifyHist> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsVerifyHist> TableToList(DataTable table)
    {
        var retVal = new List<InsVerifyHist>();
        InsVerifyHist insVerifyHist;
        foreach (DataRow row in table.Rows)
        {
            insVerifyHist = new InsVerifyHist();
            insVerifyHist.InsVerifyHistNum = SIn.Long(row["InsVerifyHistNum"].ToString());
            insVerifyHist.VerifyUserNum = SIn.Long(row["VerifyUserNum"].ToString());
            insVerifyHist.InsVerifyNum = SIn.Long(row["InsVerifyNum"].ToString());
            insVerifyHist.DateLastVerified = SIn.Date(row["DateLastVerified"].ToString());
            insVerifyHist.UserNum = SIn.Long(row["UserNum"].ToString());
            insVerifyHist.VerifyType = (VerifyTypes) SIn.Int(row["VerifyType"].ToString());
            insVerifyHist.FKey = SIn.Long(row["FKey"].ToString());
            insVerifyHist.DefNum = SIn.Long(row["DefNum"].ToString());
            insVerifyHist.Note = SIn.String(row["Note"].ToString());
            insVerifyHist.DateLastAssigned = SIn.Date(row["DateLastAssigned"].ToString());
            insVerifyHist.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            insVerifyHist.HoursAvailableForVerification = SIn.Double(row["HoursAvailableForVerification"].ToString());
            insVerifyHist.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(insVerifyHist);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsVerifyHist> listInsVerifyHists, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsVerifyHist";
        var table = new DataTable(tableName);
        table.Columns.Add("InsVerifyHistNum");
        table.Columns.Add("VerifyUserNum");
        table.Columns.Add("InsVerifyNum");
        table.Columns.Add("DateLastVerified");
        table.Columns.Add("UserNum");
        table.Columns.Add("VerifyType");
        table.Columns.Add("FKey");
        table.Columns.Add("DefNum");
        table.Columns.Add("Note");
        table.Columns.Add("DateLastAssigned");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("HoursAvailableForVerification");
        table.Columns.Add("SecDateTEdit");
        foreach (var insVerifyHist in listInsVerifyHists)
            table.Rows.Add(SOut.Long(insVerifyHist.InsVerifyHistNum), SOut.Long(insVerifyHist.VerifyUserNum), SOut.Long(insVerifyHist.InsVerifyNum), SOut.DateT(insVerifyHist.DateLastVerified, false), SOut.Long(insVerifyHist.UserNum), SOut.Int((int) insVerifyHist.VerifyType), SOut.Long(insVerifyHist.FKey), SOut.Long(insVerifyHist.DefNum), insVerifyHist.Note, SOut.DateT(insVerifyHist.DateLastAssigned, false), SOut.DateT(insVerifyHist.DateTimeEntry, false), SOut.Double(insVerifyHist.HoursAvailableForVerification), SOut.DateT(insVerifyHist.SecDateTEdit, false));
        return table;
    }

    public static long Insert(InsVerifyHist insVerifyHist)
    {
        return Insert(insVerifyHist, false);
    }

    public static long Insert(InsVerifyHist insVerifyHist, bool useExistingPK)
    {
        var command = "INSERT INTO insverifyhist (";

        command += "VerifyUserNum,InsVerifyNum,DateLastVerified,UserNum,VerifyType,FKey,DefNum,Note,DateLastAssigned,DateTimeEntry,HoursAvailableForVerification) VALUES(";

        command +=
            SOut.Long(insVerifyHist.VerifyUserNum) + ","
                                                   + SOut.Long(insVerifyHist.InsVerifyNum) + ","
                                                   + SOut.Date(insVerifyHist.DateLastVerified) + ","
                                                   + SOut.Long(insVerifyHist.UserNum) + ","
                                                   + SOut.Int((int) insVerifyHist.VerifyType) + ","
                                                   + SOut.Long(insVerifyHist.FKey) + ","
                                                   + SOut.Long(insVerifyHist.DefNum) + ","
                                                   + DbHelper.ParamChar + "paramNote,"
                                                   + SOut.Date(insVerifyHist.DateLastAssigned) + ","
                                                   + SOut.DateT(insVerifyHist.DateTimeEntry) + ","
                                                   + SOut.Double(insVerifyHist.HoursAvailableForVerification) + ")";
        //SecDateTEdit can only be set by MySQL
        if (insVerifyHist.Note == null) insVerifyHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerifyHist.Note));
        {
            insVerifyHist.InsVerifyHistNum = Db.NonQ(command, true, "InsVerifyHistNum", "insVerifyHist", paramNote);
        }
        return insVerifyHist.InsVerifyHistNum;
    }

    public static long InsertNoCache(InsVerifyHist insVerifyHist)
    {
        return InsertNoCache(insVerifyHist, false);
    }

    public static long InsertNoCache(InsVerifyHist insVerifyHist, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insverifyhist (";
        if (isRandomKeys || useExistingPK) command += "InsVerifyHistNum,";
        command += "VerifyUserNum,InsVerifyNum,DateLastVerified,UserNum,VerifyType,FKey,DefNum,Note,DateLastAssigned,DateTimeEntry,HoursAvailableForVerification) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insVerifyHist.InsVerifyHistNum) + ",";
        command +=
            SOut.Long(insVerifyHist.VerifyUserNum) + ","
                                                   + SOut.Long(insVerifyHist.InsVerifyNum) + ","
                                                   + SOut.Date(insVerifyHist.DateLastVerified) + ","
                                                   + SOut.Long(insVerifyHist.UserNum) + ","
                                                   + SOut.Int((int) insVerifyHist.VerifyType) + ","
                                                   + SOut.Long(insVerifyHist.FKey) + ","
                                                   + SOut.Long(insVerifyHist.DefNum) + ","
                                                   + DbHelper.ParamChar + "paramNote,"
                                                   + SOut.Date(insVerifyHist.DateLastAssigned) + ","
                                                   + SOut.DateT(insVerifyHist.DateTimeEntry) + ","
                                                   + SOut.Double(insVerifyHist.HoursAvailableForVerification) + ")";
        //SecDateTEdit can only be set by MySQL
        if (insVerifyHist.Note == null) insVerifyHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerifyHist.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            insVerifyHist.InsVerifyHistNum = Db.NonQ(command, true, "InsVerifyHistNum", "insVerifyHist", paramNote);
        return insVerifyHist.InsVerifyHistNum;
    }

    public static void Update(InsVerifyHist insVerifyHist)
    {
        var command = "UPDATE insverifyhist SET "
                      + "VerifyUserNum                =  " + SOut.Long(insVerifyHist.VerifyUserNum) + ", "
                      + "InsVerifyNum                 =  " + SOut.Long(insVerifyHist.InsVerifyNum) + ", "
                      + "DateLastVerified             =  " + SOut.Date(insVerifyHist.DateLastVerified) + ", "
                      + "UserNum                      =  " + SOut.Long(insVerifyHist.UserNum) + ", "
                      + "VerifyType                   =  " + SOut.Int((int) insVerifyHist.VerifyType) + ", "
                      + "FKey                         =  " + SOut.Long(insVerifyHist.FKey) + ", "
                      + "DefNum                       =  " + SOut.Long(insVerifyHist.DefNum) + ", "
                      + "Note                         =  " + DbHelper.ParamChar + "paramNote, "
                      + "DateLastAssigned             =  " + SOut.Date(insVerifyHist.DateLastAssigned) + ", "
                      + "DateTimeEntry                =  " + SOut.DateT(insVerifyHist.DateTimeEntry) + ", "
                      + "HoursAvailableForVerification=  " + SOut.Double(insVerifyHist.HoursAvailableForVerification) + " "
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE InsVerifyHistNum = " + SOut.Long(insVerifyHist.InsVerifyHistNum);
        if (insVerifyHist.Note == null) insVerifyHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerifyHist.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(InsVerifyHist insVerifyHist, InsVerifyHist oldInsVerifyHist)
    {
        var command = "";
        if (insVerifyHist.VerifyUserNum != oldInsVerifyHist.VerifyUserNum)
        {
            if (command != "") command += ",";
            command += "VerifyUserNum = " + SOut.Long(insVerifyHist.VerifyUserNum) + "";
        }

        if (insVerifyHist.InsVerifyNum != oldInsVerifyHist.InsVerifyNum)
        {
            if (command != "") command += ",";
            command += "InsVerifyNum = " + SOut.Long(insVerifyHist.InsVerifyNum) + "";
        }

        if (insVerifyHist.DateLastVerified.Date != oldInsVerifyHist.DateLastVerified.Date)
        {
            if (command != "") command += ",";
            command += "DateLastVerified = " + SOut.Date(insVerifyHist.DateLastVerified) + "";
        }

        if (insVerifyHist.UserNum != oldInsVerifyHist.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(insVerifyHist.UserNum) + "";
        }

        if (insVerifyHist.VerifyType != oldInsVerifyHist.VerifyType)
        {
            if (command != "") command += ",";
            command += "VerifyType = " + SOut.Int((int) insVerifyHist.VerifyType) + "";
        }

        if (insVerifyHist.FKey != oldInsVerifyHist.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insVerifyHist.FKey) + "";
        }

        if (insVerifyHist.DefNum != oldInsVerifyHist.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(insVerifyHist.DefNum) + "";
        }

        if (insVerifyHist.Note != oldInsVerifyHist.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (insVerifyHist.DateLastAssigned.Date != oldInsVerifyHist.DateLastAssigned.Date)
        {
            if (command != "") command += ",";
            command += "DateLastAssigned = " + SOut.Date(insVerifyHist.DateLastAssigned) + "";
        }

        if (insVerifyHist.DateTimeEntry != oldInsVerifyHist.DateTimeEntry)
        {
            if (command != "") command += ",";
            command += "DateTimeEntry = " + SOut.DateT(insVerifyHist.DateTimeEntry) + "";
        }

        if (insVerifyHist.HoursAvailableForVerification != oldInsVerifyHist.HoursAvailableForVerification)
        {
            if (command != "") command += ",";
            command += "HoursAvailableForVerification = " + SOut.Double(insVerifyHist.HoursAvailableForVerification) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (insVerifyHist.Note == null) insVerifyHist.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(insVerifyHist.Note));
        command = "UPDATE insverifyhist SET " + command
                                              + " WHERE InsVerifyHistNum = " + SOut.Long(insVerifyHist.InsVerifyHistNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(InsVerifyHist insVerifyHist, InsVerifyHist oldInsVerifyHist)
    {
        if (insVerifyHist.VerifyUserNum != oldInsVerifyHist.VerifyUserNum) return true;
        if (insVerifyHist.InsVerifyNum != oldInsVerifyHist.InsVerifyNum) return true;
        if (insVerifyHist.DateLastVerified.Date != oldInsVerifyHist.DateLastVerified.Date) return true;
        if (insVerifyHist.UserNum != oldInsVerifyHist.UserNum) return true;
        if (insVerifyHist.VerifyType != oldInsVerifyHist.VerifyType) return true;
        if (insVerifyHist.FKey != oldInsVerifyHist.FKey) return true;
        if (insVerifyHist.DefNum != oldInsVerifyHist.DefNum) return true;
        if (insVerifyHist.Note != oldInsVerifyHist.Note) return true;
        if (insVerifyHist.DateLastAssigned.Date != oldInsVerifyHist.DateLastAssigned.Date) return true;
        if (insVerifyHist.DateTimeEntry != oldInsVerifyHist.DateTimeEntry) return true;
        if (insVerifyHist.HoursAvailableForVerification != oldInsVerifyHist.HoursAvailableForVerification) return true;
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long insVerifyHistNum)
    {
        var command = "DELETE FROM insverifyhist "
                      + "WHERE InsVerifyHistNum = " + SOut.Long(insVerifyHistNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsVerifyHistNums)
    {
        if (listInsVerifyHistNums == null || listInsVerifyHistNums.Count == 0) return;
        var command = "DELETE FROM insverifyhist "
                      + "WHERE InsVerifyHistNum IN(" + string.Join(",", listInsVerifyHistNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}