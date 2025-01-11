#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrCarePlanCrud
{
    public static EhrCarePlan SelectOne(long ehrCarePlanNum)
    {
        var command = "SELECT * FROM ehrcareplan "
                      + "WHERE EhrCarePlanNum = " + SOut.Long(ehrCarePlanNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrCarePlan SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrCarePlan> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrCarePlan> TableToList(DataTable table)
    {
        var retVal = new List<EhrCarePlan>();
        EhrCarePlan ehrCarePlan;
        foreach (DataRow row in table.Rows)
        {
            ehrCarePlan = new EhrCarePlan();
            ehrCarePlan.EhrCarePlanNum = SIn.Long(row["EhrCarePlanNum"].ToString());
            ehrCarePlan.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrCarePlan.SnomedEducation = SIn.String(row["SnomedEducation"].ToString());
            ehrCarePlan.Instructions = SIn.String(row["Instructions"].ToString());
            ehrCarePlan.DatePlanned = SIn.Date(row["DatePlanned"].ToString());
            retVal.Add(ehrCarePlan);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrCarePlan> listEhrCarePlans, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrCarePlan";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrCarePlanNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("SnomedEducation");
        table.Columns.Add("Instructions");
        table.Columns.Add("DatePlanned");
        foreach (var ehrCarePlan in listEhrCarePlans)
            table.Rows.Add(SOut.Long(ehrCarePlan.EhrCarePlanNum), SOut.Long(ehrCarePlan.PatNum), ehrCarePlan.SnomedEducation, ehrCarePlan.Instructions, SOut.DateT(ehrCarePlan.DatePlanned, false));
        return table;
    }

    public static long Insert(EhrCarePlan ehrCarePlan)
    {
        return Insert(ehrCarePlan, false);
    }

    public static long Insert(EhrCarePlan ehrCarePlan, bool useExistingPK)
    {
        var command = "INSERT INTO ehrcareplan (";

        command += "PatNum,SnomedEducation,Instructions,DatePlanned) VALUES(";

        command +=
            SOut.Long(ehrCarePlan.PatNum) + ","
                                          + "'" + SOut.String(ehrCarePlan.SnomedEducation) + "',"
                                          + "'" + SOut.String(ehrCarePlan.Instructions) + "',"
                                          + SOut.Date(ehrCarePlan.DatePlanned) + ")";
        {
            ehrCarePlan.EhrCarePlanNum = Db.NonQ(command, true, "EhrCarePlanNum", "ehrCarePlan");
        }
        return ehrCarePlan.EhrCarePlanNum;
    }

    public static long InsertNoCache(EhrCarePlan ehrCarePlan)
    {
        return InsertNoCache(ehrCarePlan, false);
    }

    public static long InsertNoCache(EhrCarePlan ehrCarePlan, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrcareplan (";
        if (isRandomKeys || useExistingPK) command += "EhrCarePlanNum,";
        command += "PatNum,SnomedEducation,Instructions,DatePlanned) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrCarePlan.EhrCarePlanNum) + ",";
        command +=
            SOut.Long(ehrCarePlan.PatNum) + ","
                                          + "'" + SOut.String(ehrCarePlan.SnomedEducation) + "',"
                                          + "'" + SOut.String(ehrCarePlan.Instructions) + "',"
                                          + SOut.Date(ehrCarePlan.DatePlanned) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrCarePlan.EhrCarePlanNum = Db.NonQ(command, true, "EhrCarePlanNum", "ehrCarePlan");
        return ehrCarePlan.EhrCarePlanNum;
    }

    public static void Update(EhrCarePlan ehrCarePlan)
    {
        var command = "UPDATE ehrcareplan SET "
                      + "PatNum         =  " + SOut.Long(ehrCarePlan.PatNum) + ", "
                      + "SnomedEducation= '" + SOut.String(ehrCarePlan.SnomedEducation) + "', "
                      + "Instructions   = '" + SOut.String(ehrCarePlan.Instructions) + "', "
                      + "DatePlanned    =  " + SOut.Date(ehrCarePlan.DatePlanned) + " "
                      + "WHERE EhrCarePlanNum = " + SOut.Long(ehrCarePlan.EhrCarePlanNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrCarePlan ehrCarePlan, EhrCarePlan oldEhrCarePlan)
    {
        var command = "";
        if (ehrCarePlan.PatNum != oldEhrCarePlan.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrCarePlan.PatNum) + "";
        }

        if (ehrCarePlan.SnomedEducation != oldEhrCarePlan.SnomedEducation)
        {
            if (command != "") command += ",";
            command += "SnomedEducation = '" + SOut.String(ehrCarePlan.SnomedEducation) + "'";
        }

        if (ehrCarePlan.Instructions != oldEhrCarePlan.Instructions)
        {
            if (command != "") command += ",";
            command += "Instructions = '" + SOut.String(ehrCarePlan.Instructions) + "'";
        }

        if (ehrCarePlan.DatePlanned.Date != oldEhrCarePlan.DatePlanned.Date)
        {
            if (command != "") command += ",";
            command += "DatePlanned = " + SOut.Date(ehrCarePlan.DatePlanned) + "";
        }

        if (command == "") return false;
        command = "UPDATE ehrcareplan SET " + command
                                            + " WHERE EhrCarePlanNum = " + SOut.Long(ehrCarePlan.EhrCarePlanNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrCarePlan ehrCarePlan, EhrCarePlan oldEhrCarePlan)
    {
        if (ehrCarePlan.PatNum != oldEhrCarePlan.PatNum) return true;
        if (ehrCarePlan.SnomedEducation != oldEhrCarePlan.SnomedEducation) return true;
        if (ehrCarePlan.Instructions != oldEhrCarePlan.Instructions) return true;
        if (ehrCarePlan.DatePlanned.Date != oldEhrCarePlan.DatePlanned.Date) return true;
        return false;
    }

    public static void Delete(long ehrCarePlanNum)
    {
        var command = "DELETE FROM ehrcareplan "
                      + "WHERE EhrCarePlanNum = " + SOut.Long(ehrCarePlanNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrCarePlanNums)
    {
        if (listEhrCarePlanNums == null || listEhrCarePlanNums.Count == 0) return;
        var command = "DELETE FROM ehrcareplan "
                      + "WHERE EhrCarePlanNum IN(" + string.Join(",", listEhrCarePlanNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}