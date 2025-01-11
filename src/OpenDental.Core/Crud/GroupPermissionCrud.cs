#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class GroupPermissionCrud
{
    public static GroupPermission SelectOne(long groupPermNum)
    {
        var command = "SELECT * FROM grouppermission "
                      + "WHERE GroupPermNum = " + SOut.Long(groupPermNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static GroupPermission SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<GroupPermission> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<GroupPermission> TableToList(DataTable table)
    {
        var retVal = new List<GroupPermission>();
        GroupPermission groupPermission;
        foreach (DataRow row in table.Rows)
        {
            groupPermission = new GroupPermission();
            groupPermission.GroupPermNum = SIn.Long(row["GroupPermNum"].ToString());
            groupPermission.NewerDate = SIn.Date(row["NewerDate"].ToString());
            groupPermission.NewerDays = SIn.Int(row["NewerDays"].ToString());
            groupPermission.UserGroupNum = SIn.Long(row["UserGroupNum"].ToString());
            groupPermission.PermType = (EnumPermType) SIn.Int(row["PermType"].ToString());
            groupPermission.FKey = SIn.Long(row["FKey"].ToString());
            retVal.Add(groupPermission);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<GroupPermission> listGroupPermissions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "GroupPermission";
        var table = new DataTable(tableName);
        table.Columns.Add("GroupPermNum");
        table.Columns.Add("NewerDate");
        table.Columns.Add("NewerDays");
        table.Columns.Add("UserGroupNum");
        table.Columns.Add("PermType");
        table.Columns.Add("FKey");
        foreach (var groupPermission in listGroupPermissions)
            table.Rows.Add(SOut.Long(groupPermission.GroupPermNum), SOut.DateT(groupPermission.NewerDate, false), SOut.Int(groupPermission.NewerDays), SOut.Long(groupPermission.UserGroupNum), SOut.Int((int) groupPermission.PermType), SOut.Long(groupPermission.FKey));
        return table;
    }

    public static long Insert(GroupPermission groupPermission)
    {
        return Insert(groupPermission, false);
    }

    public static long Insert(GroupPermission groupPermission, bool useExistingPK)
    {
        var command = "INSERT INTO grouppermission (";

        command += "NewerDate,NewerDays,UserGroupNum,PermType,FKey) VALUES(";

        command +=
            SOut.Date(groupPermission.NewerDate) + ","
                                                 + SOut.Int(groupPermission.NewerDays) + ","
                                                 + SOut.Long(groupPermission.UserGroupNum) + ","
                                                 + SOut.Int((int) groupPermission.PermType) + ","
                                                 + SOut.Long(groupPermission.FKey) + ")";
        {
            groupPermission.GroupPermNum = Db.NonQ(command, true, "GroupPermNum", "groupPermission");
        }
        return groupPermission.GroupPermNum;
    }

    public static long InsertNoCache(GroupPermission groupPermission)
    {
        return InsertNoCache(groupPermission, false);
    }

    public static long InsertNoCache(GroupPermission groupPermission, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO grouppermission (";
        if (isRandomKeys || useExistingPK) command += "GroupPermNum,";
        command += "NewerDate,NewerDays,UserGroupNum,PermType,FKey) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(groupPermission.GroupPermNum) + ",";
        command +=
            SOut.Date(groupPermission.NewerDate) + ","
                                                 + SOut.Int(groupPermission.NewerDays) + ","
                                                 + SOut.Long(groupPermission.UserGroupNum) + ","
                                                 + SOut.Int((int) groupPermission.PermType) + ","
                                                 + SOut.Long(groupPermission.FKey) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            groupPermission.GroupPermNum = Db.NonQ(command, true, "GroupPermNum", "groupPermission");
        return groupPermission.GroupPermNum;
    }

    public static void Update(GroupPermission groupPermission)
    {
        var command = "UPDATE grouppermission SET "
                      + "NewerDate   =  " + SOut.Date(groupPermission.NewerDate) + ", "
                      + "NewerDays   =  " + SOut.Int(groupPermission.NewerDays) + ", "
                      + "UserGroupNum=  " + SOut.Long(groupPermission.UserGroupNum) + ", "
                      + "PermType    =  " + SOut.Int((int) groupPermission.PermType) + ", "
                      + "FKey        =  " + SOut.Long(groupPermission.FKey) + " "
                      + "WHERE GroupPermNum = " + SOut.Long(groupPermission.GroupPermNum);
        Db.NonQ(command);
    }

    public static bool Update(GroupPermission groupPermission, GroupPermission oldGroupPermission)
    {
        var command = "";
        if (groupPermission.NewerDate.Date != oldGroupPermission.NewerDate.Date)
        {
            if (command != "") command += ",";
            command += "NewerDate = " + SOut.Date(groupPermission.NewerDate) + "";
        }

        if (groupPermission.NewerDays != oldGroupPermission.NewerDays)
        {
            if (command != "") command += ",";
            command += "NewerDays = " + SOut.Int(groupPermission.NewerDays) + "";
        }

        if (groupPermission.UserGroupNum != oldGroupPermission.UserGroupNum)
        {
            if (command != "") command += ",";
            command += "UserGroupNum = " + SOut.Long(groupPermission.UserGroupNum) + "";
        }

        if (groupPermission.PermType != oldGroupPermission.PermType)
        {
            if (command != "") command += ",";
            command += "PermType = " + SOut.Int((int) groupPermission.PermType) + "";
        }

        if (groupPermission.FKey != oldGroupPermission.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(groupPermission.FKey) + "";
        }

        if (command == "") return false;
        command = "UPDATE grouppermission SET " + command
                                                + " WHERE GroupPermNum = " + SOut.Long(groupPermission.GroupPermNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(GroupPermission groupPermission, GroupPermission oldGroupPermission)
    {
        if (groupPermission.NewerDate.Date != oldGroupPermission.NewerDate.Date) return true;
        if (groupPermission.NewerDays != oldGroupPermission.NewerDays) return true;
        if (groupPermission.UserGroupNum != oldGroupPermission.UserGroupNum) return true;
        if (groupPermission.PermType != oldGroupPermission.PermType) return true;
        if (groupPermission.FKey != oldGroupPermission.FKey) return true;
        return false;
    }

    public static void Delete(long groupPermNum)
    {
        var command = "DELETE FROM grouppermission "
                      + "WHERE GroupPermNum = " + SOut.Long(groupPermNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listGroupPermNums)
    {
        if (listGroupPermNums == null || listGroupPermNums.Count == 0) return;
        var command = "DELETE FROM grouppermission "
                      + "WHERE GroupPermNum IN(" + string.Join(",", listGroupPermNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<GroupPermission> listNew, List<GroupPermission> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<GroupPermission>();
        var listUpdNew = new List<GroupPermission>();
        var listUpdDB = new List<GroupPermission>();
        var listDel = new List<GroupPermission>();
        listNew.Sort((x, y) => { return x.GroupPermNum.CompareTo(y.GroupPermNum); });
        listDB.Sort((x, y) => { return x.GroupPermNum.CompareTo(y.GroupPermNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        GroupPermission fieldNew;
        GroupPermission fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.GroupPermNum < fieldDB.GroupPermNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.GroupPermNum > fieldDB.GroupPermNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.GroupPermNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}