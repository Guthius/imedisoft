#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrNotPerformedCrud
{
    public static EhrNotPerformed SelectOne(long ehrNotPerformedNum)
    {
        var command = "SELECT * FROM ehrnotperformed "
                      + "WHERE EhrNotPerformedNum = " + SOut.Long(ehrNotPerformedNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrNotPerformed SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrNotPerformed> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrNotPerformed> TableToList(DataTable table)
    {
        var retVal = new List<EhrNotPerformed>();
        EhrNotPerformed ehrNotPerformed;
        foreach (DataRow row in table.Rows)
        {
            ehrNotPerformed = new EhrNotPerformed();
            ehrNotPerformed.EhrNotPerformedNum = SIn.Long(row["EhrNotPerformedNum"].ToString());
            ehrNotPerformed.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrNotPerformed.ProvNum = SIn.Long(row["ProvNum"].ToString());
            ehrNotPerformed.CodeValue = SIn.String(row["CodeValue"].ToString());
            ehrNotPerformed.CodeSystem = SIn.String(row["CodeSystem"].ToString());
            ehrNotPerformed.CodeValueReason = SIn.String(row["CodeValueReason"].ToString());
            ehrNotPerformed.CodeSystemReason = SIn.String(row["CodeSystemReason"].ToString());
            ehrNotPerformed.Note = SIn.String(row["Note"].ToString());
            ehrNotPerformed.DateEntry = SIn.Date(row["DateEntry"].ToString());
            retVal.Add(ehrNotPerformed);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrNotPerformed> listEhrNotPerformeds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrNotPerformed";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrNotPerformedNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("CodeValue");
        table.Columns.Add("CodeSystem");
        table.Columns.Add("CodeValueReason");
        table.Columns.Add("CodeSystemReason");
        table.Columns.Add("Note");
        table.Columns.Add("DateEntry");
        foreach (var ehrNotPerformed in listEhrNotPerformeds)
            table.Rows.Add(SOut.Long(ehrNotPerformed.EhrNotPerformedNum), SOut.Long(ehrNotPerformed.PatNum), SOut.Long(ehrNotPerformed.ProvNum), ehrNotPerformed.CodeValue, ehrNotPerformed.CodeSystem, ehrNotPerformed.CodeValueReason, ehrNotPerformed.CodeSystemReason, ehrNotPerformed.Note, SOut.DateT(ehrNotPerformed.DateEntry, false));
        return table;
    }

    public static long Insert(EhrNotPerformed ehrNotPerformed)
    {
        return Insert(ehrNotPerformed, false);
    }

    public static long Insert(EhrNotPerformed ehrNotPerformed, bool useExistingPK)
    {
        var command = "INSERT INTO ehrnotperformed (";

        command += "PatNum,ProvNum,CodeValue,CodeSystem,CodeValueReason,CodeSystemReason,Note,DateEntry) VALUES(";

        command +=
            SOut.Long(ehrNotPerformed.PatNum) + ","
                                              + SOut.Long(ehrNotPerformed.ProvNum) + ","
                                              + "'" + SOut.String(ehrNotPerformed.CodeValue) + "',"
                                              + "'" + SOut.String(ehrNotPerformed.CodeSystem) + "',"
                                              + "'" + SOut.String(ehrNotPerformed.CodeValueReason) + "',"
                                              + "'" + SOut.String(ehrNotPerformed.CodeSystemReason) + "',"
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.Date(ehrNotPerformed.DateEntry) + ")";
        if (ehrNotPerformed.Note == null) ehrNotPerformed.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(ehrNotPerformed.Note));
        {
            ehrNotPerformed.EhrNotPerformedNum = Db.NonQ(command, true, "EhrNotPerformedNum", "ehrNotPerformed", paramNote);
        }
        return ehrNotPerformed.EhrNotPerformedNum;
    }

    public static long InsertNoCache(EhrNotPerformed ehrNotPerformed)
    {
        return InsertNoCache(ehrNotPerformed, false);
    }

    public static long InsertNoCache(EhrNotPerformed ehrNotPerformed, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrnotperformed (";
        if (isRandomKeys || useExistingPK) command += "EhrNotPerformedNum,";
        command += "PatNum,ProvNum,CodeValue,CodeSystem,CodeValueReason,CodeSystemReason,Note,DateEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrNotPerformed.EhrNotPerformedNum) + ",";
        command +=
            SOut.Long(ehrNotPerformed.PatNum) + ","
                                              + SOut.Long(ehrNotPerformed.ProvNum) + ","
                                              + "'" + SOut.String(ehrNotPerformed.CodeValue) + "',"
                                              + "'" + SOut.String(ehrNotPerformed.CodeSystem) + "',"
                                              + "'" + SOut.String(ehrNotPerformed.CodeValueReason) + "',"
                                              + "'" + SOut.String(ehrNotPerformed.CodeSystemReason) + "',"
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.Date(ehrNotPerformed.DateEntry) + ")";
        if (ehrNotPerformed.Note == null) ehrNotPerformed.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(ehrNotPerformed.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            ehrNotPerformed.EhrNotPerformedNum = Db.NonQ(command, true, "EhrNotPerformedNum", "ehrNotPerformed", paramNote);
        return ehrNotPerformed.EhrNotPerformedNum;
    }

    public static void Update(EhrNotPerformed ehrNotPerformed)
    {
        var command = "UPDATE ehrnotperformed SET "
                      + "PatNum            =  " + SOut.Long(ehrNotPerformed.PatNum) + ", "
                      + "ProvNum           =  " + SOut.Long(ehrNotPerformed.ProvNum) + ", "
                      + "CodeValue         = '" + SOut.String(ehrNotPerformed.CodeValue) + "', "
                      + "CodeSystem        = '" + SOut.String(ehrNotPerformed.CodeSystem) + "', "
                      + "CodeValueReason   = '" + SOut.String(ehrNotPerformed.CodeValueReason) + "', "
                      + "CodeSystemReason  = '" + SOut.String(ehrNotPerformed.CodeSystemReason) + "', "
                      + "Note              =  " + DbHelper.ParamChar + "paramNote, "
                      + "DateEntry         =  " + SOut.Date(ehrNotPerformed.DateEntry) + " "
                      + "WHERE EhrNotPerformedNum = " + SOut.Long(ehrNotPerformed.EhrNotPerformedNum);
        if (ehrNotPerformed.Note == null) ehrNotPerformed.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(ehrNotPerformed.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(EhrNotPerformed ehrNotPerformed, EhrNotPerformed oldEhrNotPerformed)
    {
        var command = "";
        if (ehrNotPerformed.PatNum != oldEhrNotPerformed.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrNotPerformed.PatNum) + "";
        }

        if (ehrNotPerformed.ProvNum != oldEhrNotPerformed.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(ehrNotPerformed.ProvNum) + "";
        }

        if (ehrNotPerformed.CodeValue != oldEhrNotPerformed.CodeValue)
        {
            if (command != "") command += ",";
            command += "CodeValue = '" + SOut.String(ehrNotPerformed.CodeValue) + "'";
        }

        if (ehrNotPerformed.CodeSystem != oldEhrNotPerformed.CodeSystem)
        {
            if (command != "") command += ",";
            command += "CodeSystem = '" + SOut.String(ehrNotPerformed.CodeSystem) + "'";
        }

        if (ehrNotPerformed.CodeValueReason != oldEhrNotPerformed.CodeValueReason)
        {
            if (command != "") command += ",";
            command += "CodeValueReason = '" + SOut.String(ehrNotPerformed.CodeValueReason) + "'";
        }

        if (ehrNotPerformed.CodeSystemReason != oldEhrNotPerformed.CodeSystemReason)
        {
            if (command != "") command += ",";
            command += "CodeSystemReason = '" + SOut.String(ehrNotPerformed.CodeSystemReason) + "'";
        }

        if (ehrNotPerformed.Note != oldEhrNotPerformed.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (ehrNotPerformed.DateEntry.Date != oldEhrNotPerformed.DateEntry.Date)
        {
            if (command != "") command += ",";
            command += "DateEntry = " + SOut.Date(ehrNotPerformed.DateEntry) + "";
        }

        if (command == "") return false;
        if (ehrNotPerformed.Note == null) ehrNotPerformed.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(ehrNotPerformed.Note));
        command = "UPDATE ehrnotperformed SET " + command
                                                + " WHERE EhrNotPerformedNum = " + SOut.Long(ehrNotPerformed.EhrNotPerformedNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(EhrNotPerformed ehrNotPerformed, EhrNotPerformed oldEhrNotPerformed)
    {
        if (ehrNotPerformed.PatNum != oldEhrNotPerformed.PatNum) return true;
        if (ehrNotPerformed.ProvNum != oldEhrNotPerformed.ProvNum) return true;
        if (ehrNotPerformed.CodeValue != oldEhrNotPerformed.CodeValue) return true;
        if (ehrNotPerformed.CodeSystem != oldEhrNotPerformed.CodeSystem) return true;
        if (ehrNotPerformed.CodeValueReason != oldEhrNotPerformed.CodeValueReason) return true;
        if (ehrNotPerformed.CodeSystemReason != oldEhrNotPerformed.CodeSystemReason) return true;
        if (ehrNotPerformed.Note != oldEhrNotPerformed.Note) return true;
        if (ehrNotPerformed.DateEntry.Date != oldEhrNotPerformed.DateEntry.Date) return true;
        return false;
    }

    public static void Delete(long ehrNotPerformedNum)
    {
        var command = "DELETE FROM ehrnotperformed "
                      + "WHERE EhrNotPerformedNum = " + SOut.Long(ehrNotPerformedNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrNotPerformedNums)
    {
        if (listEhrNotPerformedNums == null || listEhrNotPerformedNums.Count == 0) return;
        var command = "DELETE FROM ehrnotperformed "
                      + "WHERE EhrNotPerformedNum IN(" + string.Join(",", listEhrNotPerformedNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}