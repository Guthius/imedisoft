#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ERoutingActionDefCrud
{
    public static ERoutingActionDef SelectOne(long eRoutingActionDefNum)
    {
        var command = "SELECT * FROM eroutingactiondef "
                      + "WHERE ERoutingActionDefNum = " + SOut.Long(eRoutingActionDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ERoutingActionDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ERoutingActionDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingActionDef> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingActionDef>();
        ERoutingActionDef eRoutingActionDef;
        foreach (DataRow row in table.Rows)
        {
            eRoutingActionDef = new ERoutingActionDef();
            eRoutingActionDef.ERoutingActionDefNum = SIn.Long(row["ERoutingActionDefNum"].ToString());
            eRoutingActionDef.ERoutingDefNum = SIn.Long(row["ERoutingDefNum"].ToString());
            eRoutingActionDef.ERoutingActionType = (EnumERoutingActionType) SIn.Int(row["ERoutingActionType"].ToString());
            eRoutingActionDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eRoutingActionDef.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            eRoutingActionDef.DateTLastModified = SIn.DateTime(row["DateTLastModified"].ToString());
            eRoutingActionDef.ForeignKey = SIn.Long(row["ForeignKey"].ToString());
            eRoutingActionDef.ForeignKeyType = (EnumERoutingDefFKType) SIn.Int(row["ForeignKeyType"].ToString());
            eRoutingActionDef.LabelOverride = SIn.String(row["LabelOverride"].ToString());
            retVal.Add(eRoutingActionDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ERoutingActionDef> listERoutingActionDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERoutingActionDef";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingActionDefNum");
        table.Columns.Add("ERoutingDefNum");
        table.Columns.Add("ERoutingActionType");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("DateTLastModified");
        table.Columns.Add("ForeignKey");
        table.Columns.Add("ForeignKeyType");
        table.Columns.Add("LabelOverride");
        foreach (var eRoutingActionDef in listERoutingActionDefs)
            table.Rows.Add(SOut.Long(eRoutingActionDef.ERoutingActionDefNum), SOut.Long(eRoutingActionDef.ERoutingDefNum), SOut.Int((int) eRoutingActionDef.ERoutingActionType), SOut.Int(eRoutingActionDef.ItemOrder), SOut.DateT(eRoutingActionDef.SecDateTEntry, false), SOut.DateT(eRoutingActionDef.DateTLastModified, false), SOut.Long(eRoutingActionDef.ForeignKey), SOut.Int((int) eRoutingActionDef.ForeignKeyType), eRoutingActionDef.LabelOverride);
        return table;
    }

    public static long Insert(ERoutingActionDef eRoutingActionDef)
    {
        return Insert(eRoutingActionDef, false);
    }

    public static long Insert(ERoutingActionDef eRoutingActionDef, bool useExistingPK)
    {
        var command = "INSERT INTO eroutingactiondef (";

        command += "ERoutingDefNum,ERoutingActionType,ItemOrder,SecDateTEntry,DateTLastModified,ForeignKey,ForeignKeyType,LabelOverride) VALUES(";

        command +=
            SOut.Long(eRoutingActionDef.ERoutingDefNum) + ","
                                                        + SOut.Int((int) eRoutingActionDef.ERoutingActionType) + ","
                                                        + SOut.Int(eRoutingActionDef.ItemOrder) + ","
                                                        + DbHelper.Now() + ","
                                                        + SOut.DateT(eRoutingActionDef.DateTLastModified) + ","
                                                        + SOut.Long(eRoutingActionDef.ForeignKey) + ","
                                                        + SOut.Int((int) eRoutingActionDef.ForeignKeyType) + ","
                                                        + "'" + SOut.String(eRoutingActionDef.LabelOverride) + "')";
        {
            eRoutingActionDef.ERoutingActionDefNum = Db.NonQ(command, true, "ERoutingActionDefNum", "eRoutingActionDef");
        }
        return eRoutingActionDef.ERoutingActionDefNum;
    }

    public static long InsertNoCache(ERoutingActionDef eRoutingActionDef)
    {
        return InsertNoCache(eRoutingActionDef, false);
    }

    public static long InsertNoCache(ERoutingActionDef eRoutingActionDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eroutingactiondef (";
        if (isRandomKeys || useExistingPK) command += "ERoutingActionDefNum,";
        command += "ERoutingDefNum,ERoutingActionType,ItemOrder,SecDateTEntry,DateTLastModified,ForeignKey,ForeignKeyType,LabelOverride) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eRoutingActionDef.ERoutingActionDefNum) + ",";
        command +=
            SOut.Long(eRoutingActionDef.ERoutingDefNum) + ","
                                                        + SOut.Int((int) eRoutingActionDef.ERoutingActionType) + ","
                                                        + SOut.Int(eRoutingActionDef.ItemOrder) + ","
                                                        + DbHelper.Now() + ","
                                                        + SOut.DateT(eRoutingActionDef.DateTLastModified) + ","
                                                        + SOut.Long(eRoutingActionDef.ForeignKey) + ","
                                                        + SOut.Int((int) eRoutingActionDef.ForeignKeyType) + ","
                                                        + "'" + SOut.String(eRoutingActionDef.LabelOverride) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eRoutingActionDef.ERoutingActionDefNum = Db.NonQ(command, true, "ERoutingActionDefNum", "eRoutingActionDef");
        return eRoutingActionDef.ERoutingActionDefNum;
    }

    public static void Update(ERoutingActionDef eRoutingActionDef)
    {
        var command = "UPDATE eroutingactiondef SET "
                      + "ERoutingDefNum      =  " + SOut.Long(eRoutingActionDef.ERoutingDefNum) + ", "
                      + "ERoutingActionType  =  " + SOut.Int((int) eRoutingActionDef.ERoutingActionType) + ", "
                      + "ItemOrder           =  " + SOut.Int(eRoutingActionDef.ItemOrder) + ", "
                      //SecDateTEntry not allowed to change
                      + "DateTLastModified   =  " + SOut.DateT(eRoutingActionDef.DateTLastModified) + ", "
                      + "ForeignKey          =  " + SOut.Long(eRoutingActionDef.ForeignKey) + ", "
                      + "ForeignKeyType      =  " + SOut.Int((int) eRoutingActionDef.ForeignKeyType) + ", "
                      + "LabelOverride       = '" + SOut.String(eRoutingActionDef.LabelOverride) + "' "
                      + "WHERE ERoutingActionDefNum = " + SOut.Long(eRoutingActionDef.ERoutingActionDefNum);
        Db.NonQ(command);
    }

    public static bool Update(ERoutingActionDef eRoutingActionDef, ERoutingActionDef oldERoutingActionDef)
    {
        var command = "";
        if (eRoutingActionDef.ERoutingDefNum != oldERoutingActionDef.ERoutingDefNum)
        {
            if (command != "") command += ",";
            command += "ERoutingDefNum = " + SOut.Long(eRoutingActionDef.ERoutingDefNum) + "";
        }

        if (eRoutingActionDef.ERoutingActionType != oldERoutingActionDef.ERoutingActionType)
        {
            if (command != "") command += ",";
            command += "ERoutingActionType = " + SOut.Int((int) eRoutingActionDef.ERoutingActionType) + "";
        }

        if (eRoutingActionDef.ItemOrder != oldERoutingActionDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(eRoutingActionDef.ItemOrder) + "";
        }

        //SecDateTEntry not allowed to change
        if (eRoutingActionDef.DateTLastModified != oldERoutingActionDef.DateTLastModified)
        {
            if (command != "") command += ",";
            command += "DateTLastModified = " + SOut.DateT(eRoutingActionDef.DateTLastModified) + "";
        }

        if (eRoutingActionDef.ForeignKey != oldERoutingActionDef.ForeignKey)
        {
            if (command != "") command += ",";
            command += "ForeignKey = " + SOut.Long(eRoutingActionDef.ForeignKey) + "";
        }

        if (eRoutingActionDef.ForeignKeyType != oldERoutingActionDef.ForeignKeyType)
        {
            if (command != "") command += ",";
            command += "ForeignKeyType = " + SOut.Int((int) eRoutingActionDef.ForeignKeyType) + "";
        }

        if (eRoutingActionDef.LabelOverride != oldERoutingActionDef.LabelOverride)
        {
            if (command != "") command += ",";
            command += "LabelOverride = '" + SOut.String(eRoutingActionDef.LabelOverride) + "'";
        }

        if (command == "") return false;
        command = "UPDATE eroutingactiondef SET " + command
                                                  + " WHERE ERoutingActionDefNum = " + SOut.Long(eRoutingActionDef.ERoutingActionDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ERoutingActionDef eRoutingActionDef, ERoutingActionDef oldERoutingActionDef)
    {
        if (eRoutingActionDef.ERoutingDefNum != oldERoutingActionDef.ERoutingDefNum) return true;
        if (eRoutingActionDef.ERoutingActionType != oldERoutingActionDef.ERoutingActionType) return true;
        if (eRoutingActionDef.ItemOrder != oldERoutingActionDef.ItemOrder) return true;
        //SecDateTEntry not allowed to change
        if (eRoutingActionDef.DateTLastModified != oldERoutingActionDef.DateTLastModified) return true;
        if (eRoutingActionDef.ForeignKey != oldERoutingActionDef.ForeignKey) return true;
        if (eRoutingActionDef.ForeignKeyType != oldERoutingActionDef.ForeignKeyType) return true;
        if (eRoutingActionDef.LabelOverride != oldERoutingActionDef.LabelOverride) return true;
        return false;
    }

    public static void Delete(long eRoutingActionDefNum)
    {
        var command = "DELETE FROM eroutingactiondef "
                      + "WHERE ERoutingActionDefNum = " + SOut.Long(eRoutingActionDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listERoutingActionDefNums)
    {
        if (listERoutingActionDefNums == null || listERoutingActionDefNums.Count == 0) return;
        var command = "DELETE FROM eroutingactiondef "
                      + "WHERE ERoutingActionDefNum IN(" + string.Join(",", listERoutingActionDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}