#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DrugUnitCrud
{
    public static DrugUnit SelectOne(long drugUnitNum)
    {
        var command = "SELECT * FROM drugunit "
                      + "WHERE DrugUnitNum = " + SOut.Long(drugUnitNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DrugUnit SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DrugUnit> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DrugUnit> TableToList(DataTable table)
    {
        var retVal = new List<DrugUnit>();
        DrugUnit drugUnit;
        foreach (DataRow row in table.Rows)
        {
            drugUnit = new DrugUnit();
            drugUnit.DrugUnitNum = SIn.Long(row["DrugUnitNum"].ToString());
            drugUnit.UnitIdentifier = SIn.String(row["UnitIdentifier"].ToString());
            drugUnit.UnitText = SIn.String(row["UnitText"].ToString());
            retVal.Add(drugUnit);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DrugUnit> listDrugUnits, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DrugUnit";
        var table = new DataTable(tableName);
        table.Columns.Add("DrugUnitNum");
        table.Columns.Add("UnitIdentifier");
        table.Columns.Add("UnitText");
        foreach (var drugUnit in listDrugUnits)
            table.Rows.Add(SOut.Long(drugUnit.DrugUnitNum), drugUnit.UnitIdentifier, drugUnit.UnitText);
        return table;
    }

    public static long Insert(DrugUnit drugUnit)
    {
        return Insert(drugUnit, false);
    }

    public static long Insert(DrugUnit drugUnit, bool useExistingPK)
    {
        var command = "INSERT INTO drugunit (";

        command += "UnitIdentifier,UnitText) VALUES(";

        command +=
            "'" + SOut.String(drugUnit.UnitIdentifier) + "',"
            + "'" + SOut.String(drugUnit.UnitText) + "')";
        {
            drugUnit.DrugUnitNum = Db.NonQ(command, true, "DrugUnitNum", "drugUnit");
        }
        return drugUnit.DrugUnitNum;
    }

    public static long InsertNoCache(DrugUnit drugUnit)
    {
        return InsertNoCache(drugUnit, false);
    }

    public static long InsertNoCache(DrugUnit drugUnit, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO drugunit (";
        if (isRandomKeys || useExistingPK) command += "DrugUnitNum,";
        command += "UnitIdentifier,UnitText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(drugUnit.DrugUnitNum) + ",";
        command +=
            "'" + SOut.String(drugUnit.UnitIdentifier) + "',"
            + "'" + SOut.String(drugUnit.UnitText) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            drugUnit.DrugUnitNum = Db.NonQ(command, true, "DrugUnitNum", "drugUnit");
        return drugUnit.DrugUnitNum;
    }

    public static void Update(DrugUnit drugUnit)
    {
        var command = "UPDATE drugunit SET "
                      + "UnitIdentifier= '" + SOut.String(drugUnit.UnitIdentifier) + "', "
                      + "UnitText      = '" + SOut.String(drugUnit.UnitText) + "' "
                      + "WHERE DrugUnitNum = " + SOut.Long(drugUnit.DrugUnitNum);
        Db.NonQ(command);
    }

    public static bool Update(DrugUnit drugUnit, DrugUnit oldDrugUnit)
    {
        var command = "";
        if (drugUnit.UnitIdentifier != oldDrugUnit.UnitIdentifier)
        {
            if (command != "") command += ",";
            command += "UnitIdentifier = '" + SOut.String(drugUnit.UnitIdentifier) + "'";
        }

        if (drugUnit.UnitText != oldDrugUnit.UnitText)
        {
            if (command != "") command += ",";
            command += "UnitText = '" + SOut.String(drugUnit.UnitText) + "'";
        }

        if (command == "") return false;
        command = "UPDATE drugunit SET " + command
                                         + " WHERE DrugUnitNum = " + SOut.Long(drugUnit.DrugUnitNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DrugUnit drugUnit, DrugUnit oldDrugUnit)
    {
        if (drugUnit.UnitIdentifier != oldDrugUnit.UnitIdentifier) return true;
        if (drugUnit.UnitText != oldDrugUnit.UnitText) return true;
        return false;
    }

    public static void Delete(long drugUnitNum)
    {
        var command = "DELETE FROM drugunit "
                      + "WHERE DrugUnitNum = " + SOut.Long(drugUnitNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDrugUnitNums)
    {
        if (listDrugUnitNums == null || listDrugUnitNums.Count == 0) return;
        var command = "DELETE FROM drugunit "
                      + "WHERE DrugUnitNum IN(" + string.Join(",", listDrugUnitNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}