#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReactivationCrud
{
    public static Reactivation SelectOne(long reactivationNum)
    {
        var command = "SELECT * FROM reactivation "
                      + "WHERE ReactivationNum = " + SOut.Long(reactivationNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Reactivation SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Reactivation> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Reactivation> TableToList(DataTable table)
    {
        var retVal = new List<Reactivation>();
        Reactivation reactivation;
        foreach (DataRow row in table.Rows)
        {
            reactivation = new Reactivation();
            reactivation.ReactivationNum = SIn.Long(row["ReactivationNum"].ToString());
            reactivation.PatNum = SIn.Long(row["PatNum"].ToString());
            reactivation.ReactivationStatus = SIn.Long(row["ReactivationStatus"].ToString());
            reactivation.ReactivationNote = SIn.String(row["ReactivationNote"].ToString());
            reactivation.DoNotContact = SIn.Bool(row["DoNotContact"].ToString());
            retVal.Add(reactivation);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Reactivation> listReactivations, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Reactivation";
        var table = new DataTable(tableName);
        table.Columns.Add("ReactivationNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ReactivationStatus");
        table.Columns.Add("ReactivationNote");
        table.Columns.Add("DoNotContact");
        foreach (var reactivation in listReactivations)
            table.Rows.Add(SOut.Long(reactivation.ReactivationNum), SOut.Long(reactivation.PatNum), SOut.Long(reactivation.ReactivationStatus), reactivation.ReactivationNote, SOut.Bool(reactivation.DoNotContact));
        return table;
    }

    public static long Insert(Reactivation reactivation)
    {
        return Insert(reactivation, false);
    }

    public static long Insert(Reactivation reactivation, bool useExistingPK)
    {
        var command = "INSERT INTO reactivation (";

        command += "PatNum,ReactivationStatus,ReactivationNote,DoNotContact) VALUES(";

        command +=
            SOut.Long(reactivation.PatNum) + ","
                                           + SOut.Long(reactivation.ReactivationStatus) + ","
                                           + DbHelper.ParamChar + "paramReactivationNote,"
                                           + SOut.Bool(reactivation.DoNotContact) + ")";
        if (reactivation.ReactivationNote == null) reactivation.ReactivationNote = "";
        var paramReactivationNote = new OdSqlParameter("paramReactivationNote", OdDbType.Text, SOut.StringParam(reactivation.ReactivationNote));
        {
            reactivation.ReactivationNum = Db.NonQ(command, true, "ReactivationNum", "reactivation", paramReactivationNote);
        }
        return reactivation.ReactivationNum;
    }

    public static long InsertNoCache(Reactivation reactivation)
    {
        return InsertNoCache(reactivation, false);
    }

    public static long InsertNoCache(Reactivation reactivation, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO reactivation (";
        if (isRandomKeys || useExistingPK) command += "ReactivationNum,";
        command += "PatNum,ReactivationStatus,ReactivationNote,DoNotContact) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(reactivation.ReactivationNum) + ",";
        command +=
            SOut.Long(reactivation.PatNum) + ","
                                           + SOut.Long(reactivation.ReactivationStatus) + ","
                                           + DbHelper.ParamChar + "paramReactivationNote,"
                                           + SOut.Bool(reactivation.DoNotContact) + ")";
        if (reactivation.ReactivationNote == null) reactivation.ReactivationNote = "";
        var paramReactivationNote = new OdSqlParameter("paramReactivationNote", OdDbType.Text, SOut.StringParam(reactivation.ReactivationNote));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramReactivationNote);
        else
            reactivation.ReactivationNum = Db.NonQ(command, true, "ReactivationNum", "reactivation", paramReactivationNote);
        return reactivation.ReactivationNum;
    }

    public static void Update(Reactivation reactivation)
    {
        var command = "UPDATE reactivation SET "
                      + "PatNum            =  " + SOut.Long(reactivation.PatNum) + ", "
                      + "ReactivationStatus=  " + SOut.Long(reactivation.ReactivationStatus) + ", "
                      + "ReactivationNote  =  " + DbHelper.ParamChar + "paramReactivationNote, "
                      + "DoNotContact      =  " + SOut.Bool(reactivation.DoNotContact) + " "
                      + "WHERE ReactivationNum = " + SOut.Long(reactivation.ReactivationNum);
        if (reactivation.ReactivationNote == null) reactivation.ReactivationNote = "";
        var paramReactivationNote = new OdSqlParameter("paramReactivationNote", OdDbType.Text, SOut.StringParam(reactivation.ReactivationNote));
        Db.NonQ(command, paramReactivationNote);
    }

    public static bool Update(Reactivation reactivation, Reactivation oldReactivation)
    {
        var command = "";
        if (reactivation.PatNum != oldReactivation.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(reactivation.PatNum) + "";
        }

        if (reactivation.ReactivationStatus != oldReactivation.ReactivationStatus)
        {
            if (command != "") command += ",";
            command += "ReactivationStatus = " + SOut.Long(reactivation.ReactivationStatus) + "";
        }

        if (reactivation.ReactivationNote != oldReactivation.ReactivationNote)
        {
            if (command != "") command += ",";
            command += "ReactivationNote = " + DbHelper.ParamChar + "paramReactivationNote";
        }

        if (reactivation.DoNotContact != oldReactivation.DoNotContact)
        {
            if (command != "") command += ",";
            command += "DoNotContact = " + SOut.Bool(reactivation.DoNotContact) + "";
        }

        if (command == "") return false;
        if (reactivation.ReactivationNote == null) reactivation.ReactivationNote = "";
        var paramReactivationNote = new OdSqlParameter("paramReactivationNote", OdDbType.Text, SOut.StringParam(reactivation.ReactivationNote));
        command = "UPDATE reactivation SET " + command
                                             + " WHERE ReactivationNum = " + SOut.Long(reactivation.ReactivationNum);
        Db.NonQ(command, paramReactivationNote);
        return true;
    }

    public static bool UpdateComparison(Reactivation reactivation, Reactivation oldReactivation)
    {
        if (reactivation.PatNum != oldReactivation.PatNum) return true;
        if (reactivation.ReactivationStatus != oldReactivation.ReactivationStatus) return true;
        if (reactivation.ReactivationNote != oldReactivation.ReactivationNote) return true;
        if (reactivation.DoNotContact != oldReactivation.DoNotContact) return true;
        return false;
    }

    public static void Delete(long reactivationNum)
    {
        var command = "DELETE FROM reactivation "
                      + "WHERE ReactivationNum = " + SOut.Long(reactivationNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReactivationNums)
    {
        if (listReactivationNums == null || listReactivationNums.Count == 0) return;
        var command = "DELETE FROM reactivation "
                      + "WHERE ReactivationNum IN(" + string.Join(",", listReactivationNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}