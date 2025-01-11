#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class FormPatCrud
{
    public static FormPat SelectOne(long formPatNum)
    {
        var command = "SELECT * FROM formpat "
                      + "WHERE FormPatNum = " + SOut.Long(formPatNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static FormPat SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<FormPat> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<FormPat> TableToList(DataTable table)
    {
        var retVal = new List<FormPat>();
        FormPat formPat;
        foreach (DataRow row in table.Rows)
        {
            formPat = new FormPat();
            formPat.FormPatNum = SIn.Long(row["FormPatNum"].ToString());
            formPat.PatNum = SIn.Long(row["PatNum"].ToString());
            formPat.FormDateTime = SIn.DateTime(row["FormDateTime"].ToString());
            retVal.Add(formPat);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<FormPat> listFormPats, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "FormPat";
        var table = new DataTable(tableName);
        table.Columns.Add("FormPatNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("FormDateTime");
        foreach (var formPat in listFormPats)
            table.Rows.Add(SOut.Long(formPat.FormPatNum), SOut.Long(formPat.PatNum), SOut.DateT(formPat.FormDateTime, false));
        return table;
    }

    public static long Insert(FormPat formPat)
    {
        return Insert(formPat, false);
    }

    public static long Insert(FormPat formPat, bool useExistingPK)
    {
        var command = "INSERT INTO formpat (";

        command += "PatNum,FormDateTime) VALUES(";

        command +=
            SOut.Long(formPat.PatNum) + ","
                                      + SOut.DateT(formPat.FormDateTime) + ")";
        {
            formPat.FormPatNum = Db.NonQ(command, true, "FormPatNum", "formPat");
        }
        return formPat.FormPatNum;
    }

    public static long InsertNoCache(FormPat formPat)
    {
        return InsertNoCache(formPat, false);
    }

    public static long InsertNoCache(FormPat formPat, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO formpat (";
        if (isRandomKeys || useExistingPK) command += "FormPatNum,";
        command += "PatNum,FormDateTime) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(formPat.FormPatNum) + ",";
        command +=
            SOut.Long(formPat.PatNum) + ","
                                      + SOut.DateT(formPat.FormDateTime) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            formPat.FormPatNum = Db.NonQ(command, true, "FormPatNum", "formPat");
        return formPat.FormPatNum;
    }

    public static void Update(FormPat formPat)
    {
        var command = "UPDATE formpat SET "
                      + "PatNum      =  " + SOut.Long(formPat.PatNum) + ", "
                      + "FormDateTime=  " + SOut.DateT(formPat.FormDateTime) + " "
                      + "WHERE FormPatNum = " + SOut.Long(formPat.FormPatNum);
        Db.NonQ(command);
    }

    public static bool Update(FormPat formPat, FormPat oldFormPat)
    {
        var command = "";
        if (formPat.PatNum != oldFormPat.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(formPat.PatNum) + "";
        }

        if (formPat.FormDateTime != oldFormPat.FormDateTime)
        {
            if (command != "") command += ",";
            command += "FormDateTime = " + SOut.DateT(formPat.FormDateTime) + "";
        }

        if (command == "") return false;
        command = "UPDATE formpat SET " + command
                                        + " WHERE FormPatNum = " + SOut.Long(formPat.FormPatNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(FormPat formPat, FormPat oldFormPat)
    {
        if (formPat.PatNum != oldFormPat.PatNum) return true;
        if (formPat.FormDateTime != oldFormPat.FormDateTime) return true;
        return false;
    }

    public static void Delete(long formPatNum)
    {
        var command = "DELETE FROM formpat "
                      + "WHERE FormPatNum = " + SOut.Long(formPatNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFormPatNums)
    {
        if (listFormPatNums == null || listFormPatNums.Count == 0) return;
        var command = "DELETE FROM formpat "
                      + "WHERE FormPatNum IN(" + string.Join(",", listFormPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}