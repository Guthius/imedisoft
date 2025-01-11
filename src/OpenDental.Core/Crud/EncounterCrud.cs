#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EncounterCrud
{
    public static Encounter SelectOne(long encounterNum)
    {
        var command = "SELECT * FROM encounter "
                      + "WHERE EncounterNum = " + SOut.Long(encounterNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Encounter SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Encounter> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Encounter> TableToList(DataTable table)
    {
        var retVal = new List<Encounter>();
        Encounter encounter;
        foreach (DataRow row in table.Rows)
        {
            encounter = new Encounter();
            encounter.EncounterNum = SIn.Long(row["EncounterNum"].ToString());
            encounter.PatNum = SIn.Long(row["PatNum"].ToString());
            encounter.ProvNum = SIn.Long(row["ProvNum"].ToString());
            encounter.CodeValue = SIn.String(row["CodeValue"].ToString());
            encounter.CodeSystem = SIn.String(row["CodeSystem"].ToString());
            encounter.Note = SIn.String(row["Note"].ToString());
            encounter.DateEncounter = SIn.Date(row["DateEncounter"].ToString());
            retVal.Add(encounter);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Encounter> listEncounters, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Encounter";
        var table = new DataTable(tableName);
        table.Columns.Add("EncounterNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("CodeValue");
        table.Columns.Add("CodeSystem");
        table.Columns.Add("Note");
        table.Columns.Add("DateEncounter");
        foreach (var encounter in listEncounters)
            table.Rows.Add(SOut.Long(encounter.EncounterNum), SOut.Long(encounter.PatNum), SOut.Long(encounter.ProvNum), encounter.CodeValue, encounter.CodeSystem, encounter.Note, SOut.DateT(encounter.DateEncounter, false));
        return table;
    }

    public static long Insert(Encounter encounter)
    {
        return Insert(encounter, false);
    }

    public static long Insert(Encounter encounter, bool useExistingPK)
    {
        var command = "INSERT INTO encounter (";

        command += "PatNum,ProvNum,CodeValue,CodeSystem,Note,DateEncounter) VALUES(";

        command +=
            SOut.Long(encounter.PatNum) + ","
                                        + SOut.Long(encounter.ProvNum) + ","
                                        + "'" + SOut.String(encounter.CodeValue) + "',"
                                        + "'" + SOut.String(encounter.CodeSystem) + "',"
                                        + DbHelper.ParamChar + "paramNote,"
                                        + SOut.Date(encounter.DateEncounter) + ")";
        if (encounter.Note == null) encounter.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(encounter.Note));
        {
            encounter.EncounterNum = Db.NonQ(command, true, "EncounterNum", "encounter", paramNote);
        }
        return encounter.EncounterNum;
    }

    public static long InsertNoCache(Encounter encounter)
    {
        return InsertNoCache(encounter, false);
    }

    public static long InsertNoCache(Encounter encounter, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO encounter (";
        if (isRandomKeys || useExistingPK) command += "EncounterNum,";
        command += "PatNum,ProvNum,CodeValue,CodeSystem,Note,DateEncounter) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(encounter.EncounterNum) + ",";
        command +=
            SOut.Long(encounter.PatNum) + ","
                                        + SOut.Long(encounter.ProvNum) + ","
                                        + "'" + SOut.String(encounter.CodeValue) + "',"
                                        + "'" + SOut.String(encounter.CodeSystem) + "',"
                                        + DbHelper.ParamChar + "paramNote,"
                                        + SOut.Date(encounter.DateEncounter) + ")";
        if (encounter.Note == null) encounter.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(encounter.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            encounter.EncounterNum = Db.NonQ(command, true, "EncounterNum", "encounter", paramNote);
        return encounter.EncounterNum;
    }

    public static void Update(Encounter encounter)
    {
        var command = "UPDATE encounter SET "
                      + "PatNum       =  " + SOut.Long(encounter.PatNum) + ", "
                      + "ProvNum      =  " + SOut.Long(encounter.ProvNum) + ", "
                      + "CodeValue    = '" + SOut.String(encounter.CodeValue) + "', "
                      + "CodeSystem   = '" + SOut.String(encounter.CodeSystem) + "', "
                      + "Note         =  " + DbHelper.ParamChar + "paramNote, "
                      + "DateEncounter=  " + SOut.Date(encounter.DateEncounter) + " "
                      + "WHERE EncounterNum = " + SOut.Long(encounter.EncounterNum);
        if (encounter.Note == null) encounter.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(encounter.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(Encounter encounter, Encounter oldEncounter)
    {
        var command = "";
        if (encounter.PatNum != oldEncounter.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(encounter.PatNum) + "";
        }

        if (encounter.ProvNum != oldEncounter.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(encounter.ProvNum) + "";
        }

        if (encounter.CodeValue != oldEncounter.CodeValue)
        {
            if (command != "") command += ",";
            command += "CodeValue = '" + SOut.String(encounter.CodeValue) + "'";
        }

        if (encounter.CodeSystem != oldEncounter.CodeSystem)
        {
            if (command != "") command += ",";
            command += "CodeSystem = '" + SOut.String(encounter.CodeSystem) + "'";
        }

        if (encounter.Note != oldEncounter.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (encounter.DateEncounter.Date != oldEncounter.DateEncounter.Date)
        {
            if (command != "") command += ",";
            command += "DateEncounter = " + SOut.Date(encounter.DateEncounter) + "";
        }

        if (command == "") return false;
        if (encounter.Note == null) encounter.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(encounter.Note));
        command = "UPDATE encounter SET " + command
                                          + " WHERE EncounterNum = " + SOut.Long(encounter.EncounterNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(Encounter encounter, Encounter oldEncounter)
    {
        if (encounter.PatNum != oldEncounter.PatNum) return true;
        if (encounter.ProvNum != oldEncounter.ProvNum) return true;
        if (encounter.CodeValue != oldEncounter.CodeValue) return true;
        if (encounter.CodeSystem != oldEncounter.CodeSystem) return true;
        if (encounter.Note != oldEncounter.Note) return true;
        if (encounter.DateEncounter.Date != oldEncounter.DateEncounter.Date) return true;
        return false;
    }

    public static void Delete(long encounterNum)
    {
        var command = "DELETE FROM encounter "
                      + "WHERE EncounterNum = " + SOut.Long(encounterNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEncounterNums)
    {
        if (listEncounterNums == null || listEncounterNums.Count == 0) return;
        var command = "DELETE FROM encounter "
                      + "WHERE EncounterNum IN(" + string.Join(",", listEncounterNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}