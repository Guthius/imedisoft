#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ERoutingActionCrud
{
    public static ERoutingAction SelectOne(long eRoutingActionNum)
    {
        var command = "SELECT * FROM eroutingaction "
                      + "WHERE ERoutingActionNum = " + SOut.Long(eRoutingActionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ERoutingAction SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ERoutingAction> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingAction> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingAction>();
        ERoutingAction eRoutingAction;
        foreach (DataRow row in table.Rows)
        {
            eRoutingAction = new ERoutingAction();
            eRoutingAction.ERoutingActionNum = SIn.Long(row["ERoutingActionNum"].ToString());
            eRoutingAction.ERoutingNum = SIn.Long(row["ERoutingNum"].ToString());
            eRoutingAction.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eRoutingAction.ERoutingActionType = (EnumERoutingActionType) SIn.Int(row["ERoutingActionType"].ToString());
            eRoutingAction.UserNum = SIn.Long(row["UserNum"].ToString());
            eRoutingAction.IsComplete = SIn.Bool(row["IsComplete"].ToString());
            eRoutingAction.DateTimeComplete = SIn.DateTime(row["DateTimeComplete"].ToString());
            eRoutingAction.ForeignKey = SIn.Long(row["ForeignKey"].ToString());
            eRoutingAction.ForeignKeyType = (EnumERoutingFKType) SIn.Int(row["ForeignKeyType"].ToString());
            eRoutingAction.LabelOverride = SIn.String(row["LabelOverride"].ToString());
            retVal.Add(eRoutingAction);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ERoutingAction> listERoutingActions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERoutingAction";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingActionNum");
        table.Columns.Add("ERoutingNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("ERoutingActionType");
        table.Columns.Add("UserNum");
        table.Columns.Add("IsComplete");
        table.Columns.Add("DateTimeComplete");
        table.Columns.Add("ForeignKey");
        table.Columns.Add("ForeignKeyType");
        table.Columns.Add("LabelOverride");
        foreach (var eRoutingAction in listERoutingActions)
            table.Rows.Add(SOut.Long(eRoutingAction.ERoutingActionNum), SOut.Long(eRoutingAction.ERoutingNum), SOut.Int(eRoutingAction.ItemOrder), SOut.Int((int) eRoutingAction.ERoutingActionType), SOut.Long(eRoutingAction.UserNum), SOut.Bool(eRoutingAction.IsComplete), SOut.DateT(eRoutingAction.DateTimeComplete, false), SOut.Long(eRoutingAction.ForeignKey), SOut.Int((int) eRoutingAction.ForeignKeyType), eRoutingAction.LabelOverride);
        return table;
    }

    public static long Insert(ERoutingAction eRoutingAction)
    {
        return Insert(eRoutingAction, false);
    }

    public static long Insert(ERoutingAction eRoutingAction, bool useExistingPK)
    {
        var command = "INSERT INTO eroutingaction (";

        command += "ERoutingNum,ItemOrder,ERoutingActionType,UserNum,IsComplete,DateTimeComplete,ForeignKey,ForeignKeyType,LabelOverride) VALUES(";

        command +=
            SOut.Long(eRoutingAction.ERoutingNum) + ","
                                                  + SOut.Int(eRoutingAction.ItemOrder) + ","
                                                  + SOut.Int((int) eRoutingAction.ERoutingActionType) + ","
                                                  + SOut.Long(eRoutingAction.UserNum) + ","
                                                  + SOut.Bool(eRoutingAction.IsComplete) + ","
                                                  + SOut.DateT(eRoutingAction.DateTimeComplete) + ","
                                                  + SOut.Long(eRoutingAction.ForeignKey) + ","
                                                  + SOut.Int((int) eRoutingAction.ForeignKeyType) + ","
                                                  + "'" + SOut.String(eRoutingAction.LabelOverride) + "')";
        {
            eRoutingAction.ERoutingActionNum = Db.NonQ(command, true, "ERoutingActionNum", "eRoutingAction");
        }
        return eRoutingAction.ERoutingActionNum;
    }

    public static long InsertNoCache(ERoutingAction eRoutingAction)
    {
        return InsertNoCache(eRoutingAction, false);
    }

    public static long InsertNoCache(ERoutingAction eRoutingAction, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eroutingaction (";
        if (isRandomKeys || useExistingPK) command += "ERoutingActionNum,";
        command += "ERoutingNum,ItemOrder,ERoutingActionType,UserNum,IsComplete,DateTimeComplete,ForeignKey,ForeignKeyType,LabelOverride) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eRoutingAction.ERoutingActionNum) + ",";
        command +=
            SOut.Long(eRoutingAction.ERoutingNum) + ","
                                                  + SOut.Int(eRoutingAction.ItemOrder) + ","
                                                  + SOut.Int((int) eRoutingAction.ERoutingActionType) + ","
                                                  + SOut.Long(eRoutingAction.UserNum) + ","
                                                  + SOut.Bool(eRoutingAction.IsComplete) + ","
                                                  + SOut.DateT(eRoutingAction.DateTimeComplete) + ","
                                                  + SOut.Long(eRoutingAction.ForeignKey) + ","
                                                  + SOut.Int((int) eRoutingAction.ForeignKeyType) + ","
                                                  + "'" + SOut.String(eRoutingAction.LabelOverride) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eRoutingAction.ERoutingActionNum = Db.NonQ(command, true, "ERoutingActionNum", "eRoutingAction");
        return eRoutingAction.ERoutingActionNum;
    }

    public static void Update(ERoutingAction eRoutingAction)
    {
        var command = "UPDATE eroutingaction SET "
                      + "ERoutingNum       =  " + SOut.Long(eRoutingAction.ERoutingNum) + ", "
                      + "ItemOrder         =  " + SOut.Int(eRoutingAction.ItemOrder) + ", "
                      + "ERoutingActionType=  " + SOut.Int((int) eRoutingAction.ERoutingActionType) + ", "
                      + "UserNum           =  " + SOut.Long(eRoutingAction.UserNum) + ", "
                      + "IsComplete        =  " + SOut.Bool(eRoutingAction.IsComplete) + ", "
                      + "DateTimeComplete  =  " + SOut.DateT(eRoutingAction.DateTimeComplete) + ", "
                      + "ForeignKey        =  " + SOut.Long(eRoutingAction.ForeignKey) + ", "
                      + "ForeignKeyType    =  " + SOut.Int((int) eRoutingAction.ForeignKeyType) + ", "
                      + "LabelOverride     = '" + SOut.String(eRoutingAction.LabelOverride) + "' "
                      + "WHERE ERoutingActionNum = " + SOut.Long(eRoutingAction.ERoutingActionNum);
        Db.NonQ(command);
    }

    public static bool Update(ERoutingAction eRoutingAction, ERoutingAction oldERoutingAction)
    {
        var command = "";
        if (eRoutingAction.ERoutingNum != oldERoutingAction.ERoutingNum)
        {
            if (command != "") command += ",";
            command += "ERoutingNum = " + SOut.Long(eRoutingAction.ERoutingNum) + "";
        }

        if (eRoutingAction.ItemOrder != oldERoutingAction.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(eRoutingAction.ItemOrder) + "";
        }

        if (eRoutingAction.ERoutingActionType != oldERoutingAction.ERoutingActionType)
        {
            if (command != "") command += ",";
            command += "ERoutingActionType = " + SOut.Int((int) eRoutingAction.ERoutingActionType) + "";
        }

        if (eRoutingAction.UserNum != oldERoutingAction.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(eRoutingAction.UserNum) + "";
        }

        if (eRoutingAction.IsComplete != oldERoutingAction.IsComplete)
        {
            if (command != "") command += ",";
            command += "IsComplete = " + SOut.Bool(eRoutingAction.IsComplete) + "";
        }

        if (eRoutingAction.DateTimeComplete != oldERoutingAction.DateTimeComplete)
        {
            if (command != "") command += ",";
            command += "DateTimeComplete = " + SOut.DateT(eRoutingAction.DateTimeComplete) + "";
        }

        if (eRoutingAction.ForeignKey != oldERoutingAction.ForeignKey)
        {
            if (command != "") command += ",";
            command += "ForeignKey = " + SOut.Long(eRoutingAction.ForeignKey) + "";
        }

        if (eRoutingAction.ForeignKeyType != oldERoutingAction.ForeignKeyType)
        {
            if (command != "") command += ",";
            command += "ForeignKeyType = " + SOut.Int((int) eRoutingAction.ForeignKeyType) + "";
        }

        if (eRoutingAction.LabelOverride != oldERoutingAction.LabelOverride)
        {
            if (command != "") command += ",";
            command += "LabelOverride = '" + SOut.String(eRoutingAction.LabelOverride) + "'";
        }

        if (command == "") return false;
        command = "UPDATE eroutingaction SET " + command
                                               + " WHERE ERoutingActionNum = " + SOut.Long(eRoutingAction.ERoutingActionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ERoutingAction eRoutingAction, ERoutingAction oldERoutingAction)
    {
        if (eRoutingAction.ERoutingNum != oldERoutingAction.ERoutingNum) return true;
        if (eRoutingAction.ItemOrder != oldERoutingAction.ItemOrder) return true;
        if (eRoutingAction.ERoutingActionType != oldERoutingAction.ERoutingActionType) return true;
        if (eRoutingAction.UserNum != oldERoutingAction.UserNum) return true;
        if (eRoutingAction.IsComplete != oldERoutingAction.IsComplete) return true;
        if (eRoutingAction.DateTimeComplete != oldERoutingAction.DateTimeComplete) return true;
        if (eRoutingAction.ForeignKey != oldERoutingAction.ForeignKey) return true;
        if (eRoutingAction.ForeignKeyType != oldERoutingAction.ForeignKeyType) return true;
        if (eRoutingAction.LabelOverride != oldERoutingAction.LabelOverride) return true;
        return false;
    }

    public static void Delete(long eRoutingActionNum)
    {
        var command = "DELETE FROM eroutingaction "
                      + "WHERE ERoutingActionNum = " + SOut.Long(eRoutingActionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listERoutingActionNums)
    {
        if (listERoutingActionNums == null || listERoutingActionNums.Count == 0) return;
        var command = "DELETE FROM eroutingaction "
                      + "WHERE ERoutingActionNum IN(" + string.Join(",", listERoutingActionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}