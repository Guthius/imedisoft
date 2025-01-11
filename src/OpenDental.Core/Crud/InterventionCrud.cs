#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InterventionCrud
{
    public static Intervention SelectOne(long interventionNum)
    {
        var command = "SELECT * FROM intervention "
                      + "WHERE InterventionNum = " + SOut.Long(interventionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Intervention SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Intervention> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Intervention> TableToList(DataTable table)
    {
        var retVal = new List<Intervention>();
        Intervention intervention;
        foreach (DataRow row in table.Rows)
        {
            intervention = new Intervention();
            intervention.InterventionNum = SIn.Long(row["InterventionNum"].ToString());
            intervention.PatNum = SIn.Long(row["PatNum"].ToString());
            intervention.ProvNum = SIn.Long(row["ProvNum"].ToString());
            intervention.CodeValue = SIn.String(row["CodeValue"].ToString());
            intervention.CodeSystem = SIn.String(row["CodeSystem"].ToString());
            intervention.Note = SIn.String(row["Note"].ToString());
            intervention.DateEntry = SIn.Date(row["DateEntry"].ToString());
            intervention.CodeSet = (InterventionCodeSet) SIn.Int(row["CodeSet"].ToString());
            intervention.IsPatDeclined = SIn.Bool(row["IsPatDeclined"].ToString());
            retVal.Add(intervention);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Intervention> listInterventions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Intervention";
        var table = new DataTable(tableName);
        table.Columns.Add("InterventionNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("CodeValue");
        table.Columns.Add("CodeSystem");
        table.Columns.Add("Note");
        table.Columns.Add("DateEntry");
        table.Columns.Add("CodeSet");
        table.Columns.Add("IsPatDeclined");
        foreach (var intervention in listInterventions)
            table.Rows.Add(SOut.Long(intervention.InterventionNum), SOut.Long(intervention.PatNum), SOut.Long(intervention.ProvNum), intervention.CodeValue, intervention.CodeSystem, intervention.Note, SOut.DateT(intervention.DateEntry, false), SOut.Int((int) intervention.CodeSet), SOut.Bool(intervention.IsPatDeclined));
        return table;
    }

    public static long Insert(Intervention intervention)
    {
        return Insert(intervention, false);
    }

    public static long Insert(Intervention intervention, bool useExistingPK)
    {
        var command = "INSERT INTO intervention (";

        command += "PatNum,ProvNum,CodeValue,CodeSystem,Note,DateEntry,CodeSet,IsPatDeclined) VALUES(";

        command +=
            SOut.Long(intervention.PatNum) + ","
                                           + SOut.Long(intervention.ProvNum) + ","
                                           + "'" + SOut.String(intervention.CodeValue) + "',"
                                           + "'" + SOut.String(intervention.CodeSystem) + "',"
                                           + DbHelper.ParamChar + "paramNote,"
                                           + SOut.Date(intervention.DateEntry) + ","
                                           + SOut.Int((int) intervention.CodeSet) + ","
                                           + SOut.Bool(intervention.IsPatDeclined) + ")";
        if (intervention.Note == null) intervention.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(intervention.Note));
        {
            intervention.InterventionNum = Db.NonQ(command, true, "InterventionNum", "intervention", paramNote);
        }
        return intervention.InterventionNum;
    }

    public static long InsertNoCache(Intervention intervention)
    {
        return InsertNoCache(intervention, false);
    }

    public static long InsertNoCache(Intervention intervention, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO intervention (";
        if (isRandomKeys || useExistingPK) command += "InterventionNum,";
        command += "PatNum,ProvNum,CodeValue,CodeSystem,Note,DateEntry,CodeSet,IsPatDeclined) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(intervention.InterventionNum) + ",";
        command +=
            SOut.Long(intervention.PatNum) + ","
                                           + SOut.Long(intervention.ProvNum) + ","
                                           + "'" + SOut.String(intervention.CodeValue) + "',"
                                           + "'" + SOut.String(intervention.CodeSystem) + "',"
                                           + DbHelper.ParamChar + "paramNote,"
                                           + SOut.Date(intervention.DateEntry) + ","
                                           + SOut.Int((int) intervention.CodeSet) + ","
                                           + SOut.Bool(intervention.IsPatDeclined) + ")";
        if (intervention.Note == null) intervention.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(intervention.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            intervention.InterventionNum = Db.NonQ(command, true, "InterventionNum", "intervention", paramNote);
        return intervention.InterventionNum;
    }

    public static void Update(Intervention intervention)
    {
        var command = "UPDATE intervention SET "
                      + "PatNum         =  " + SOut.Long(intervention.PatNum) + ", "
                      + "ProvNum        =  " + SOut.Long(intervention.ProvNum) + ", "
                      + "CodeValue      = '" + SOut.String(intervention.CodeValue) + "', "
                      + "CodeSystem     = '" + SOut.String(intervention.CodeSystem) + "', "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote, "
                      + "DateEntry      =  " + SOut.Date(intervention.DateEntry) + ", "
                      + "CodeSet        =  " + SOut.Int((int) intervention.CodeSet) + ", "
                      + "IsPatDeclined  =  " + SOut.Bool(intervention.IsPatDeclined) + " "
                      + "WHERE InterventionNum = " + SOut.Long(intervention.InterventionNum);
        if (intervention.Note == null) intervention.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(intervention.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Intervention intervention, Intervention oldIntervention)
    {
        var command = "";
        if (intervention.PatNum != oldIntervention.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(intervention.PatNum) + "";
        }

        if (intervention.ProvNum != oldIntervention.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(intervention.ProvNum) + "";
        }

        if (intervention.CodeValue != oldIntervention.CodeValue)
        {
            if (command != "") command += ",";
            command += "CodeValue = '" + SOut.String(intervention.CodeValue) + "'";
        }

        if (intervention.CodeSystem != oldIntervention.CodeSystem)
        {
            if (command != "") command += ",";
            command += "CodeSystem = '" + SOut.String(intervention.CodeSystem) + "'";
        }

        if (intervention.Note != oldIntervention.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (intervention.DateEntry.Date != oldIntervention.DateEntry.Date)
        {
            if (command != "") command += ",";
            command += "DateEntry = " + SOut.Date(intervention.DateEntry) + "";
        }

        if (intervention.CodeSet != oldIntervention.CodeSet)
        {
            if (command != "") command += ",";
            command += "CodeSet = " + SOut.Int((int) intervention.CodeSet) + "";
        }

        if (intervention.IsPatDeclined != oldIntervention.IsPatDeclined)
        {
            if (command != "") command += ",";
            command += "IsPatDeclined = " + SOut.Bool(intervention.IsPatDeclined) + "";
        }

        if (command == "") return false;
        if (intervention.Note == null) intervention.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(intervention.Note));
        command = "UPDATE intervention SET " + command
                                             + " WHERE InterventionNum = " + SOut.Long(intervention.InterventionNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Intervention intervention, Intervention oldIntervention)
    {
        if (intervention.PatNum != oldIntervention.PatNum) return true;
        if (intervention.ProvNum != oldIntervention.ProvNum) return true;
        if (intervention.CodeValue != oldIntervention.CodeValue) return true;
        if (intervention.CodeSystem != oldIntervention.CodeSystem) return true;
        if (intervention.Note != oldIntervention.Note) return true;
        if (intervention.DateEntry.Date != oldIntervention.DateEntry.Date) return true;
        if (intervention.CodeSet != oldIntervention.CodeSet) return true;
        if (intervention.IsPatDeclined != oldIntervention.IsPatDeclined) return true;
        return false;
    }

    public static void Delete(long interventionNum)
    {
        var command = "DELETE FROM intervention "
                      + "WHERE InterventionNum = " + SOut.Long(interventionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInterventionNums)
    {
        if (listInterventionNums == null || listInterventionNums.Count == 0) return;
        var command = "DELETE FROM intervention "
                      + "WHERE InterventionNum IN(" + string.Join(",", listInterventionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}