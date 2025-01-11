#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserClinicCrud
{
    public static UserClinic SelectOne(long userClinicNum)
    {
        var command = "SELECT * FROM userclinic "
                      + "WHERE UserClinicNum = " + SOut.Long(userClinicNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UserClinic SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserClinic> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserClinic> TableToList(DataTable table)
    {
        var retVal = new List<UserClinic>();
        UserClinic userClinic;
        foreach (DataRow row in table.Rows)
        {
            userClinic = new UserClinic();
            userClinic.UserClinicNum = SIn.Long(row["UserClinicNum"].ToString());
            userClinic.UserNum = SIn.Long(row["UserNum"].ToString());
            userClinic.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            retVal.Add(userClinic);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserClinic> listUserClinics, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserClinic";
        var table = new DataTable(tableName);
        table.Columns.Add("UserClinicNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("ClinicNum");
        foreach (var userClinic in listUserClinics)
            table.Rows.Add(SOut.Long(userClinic.UserClinicNum), SOut.Long(userClinic.UserNum), SOut.Long(userClinic.ClinicNum));
        return table;
    }

    public static long Insert(UserClinic userClinic)
    {
        return Insert(userClinic, false);
    }

    public static long Insert(UserClinic userClinic, bool useExistingPK)
    {
        var command = "INSERT INTO userclinic (";

        command += "UserNum,ClinicNum) VALUES(";

        command +=
            SOut.Long(userClinic.UserNum) + ","
                                          + SOut.Long(userClinic.ClinicNum) + ")";
        {
            userClinic.UserClinicNum = Db.NonQ(command, true, "UserClinicNum", "userClinic");
        }
        return userClinic.UserClinicNum;
    }

    public static long InsertNoCache(UserClinic userClinic)
    {
        return InsertNoCache(userClinic, false);
    }

    public static long InsertNoCache(UserClinic userClinic, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO userclinic (";
        if (isRandomKeys || useExistingPK) command += "UserClinicNum,";
        command += "UserNum,ClinicNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userClinic.UserClinicNum) + ",";
        command +=
            SOut.Long(userClinic.UserNum) + ","
                                          + SOut.Long(userClinic.ClinicNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            userClinic.UserClinicNum = Db.NonQ(command, true, "UserClinicNum", "userClinic");
        return userClinic.UserClinicNum;
    }

    public static void Update(UserClinic userClinic)
    {
        var command = "UPDATE userclinic SET "
                      + "UserNum      =  " + SOut.Long(userClinic.UserNum) + ", "
                      + "ClinicNum    =  " + SOut.Long(userClinic.ClinicNum) + " "
                      + "WHERE UserClinicNum = " + SOut.Long(userClinic.UserClinicNum);
        Db.NonQ(command);
    }

    public static bool Update(UserClinic userClinic, UserClinic oldUserClinic)
    {
        var command = "";
        if (userClinic.UserNum != oldUserClinic.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(userClinic.UserNum) + "";
        }

        if (userClinic.ClinicNum != oldUserClinic.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(userClinic.ClinicNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE userclinic SET " + command
                                           + " WHERE UserClinicNum = " + SOut.Long(userClinic.UserClinicNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(UserClinic userClinic, UserClinic oldUserClinic)
    {
        if (userClinic.UserNum != oldUserClinic.UserNum) return true;
        if (userClinic.ClinicNum != oldUserClinic.ClinicNum) return true;
        return false;
    }

    public static void Delete(long userClinicNum)
    {
        var command = "DELETE FROM userclinic "
                      + "WHERE UserClinicNum = " + SOut.Long(userClinicNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserClinicNums)
    {
        if (listUserClinicNums == null || listUserClinicNums.Count == 0) return;
        var command = "DELETE FROM userclinic "
                      + "WHERE UserClinicNum IN(" + string.Join(",", listUserClinicNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<UserClinic> listNew, List<UserClinic> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<UserClinic>();
        var listUpdNew = new List<UserClinic>();
        var listUpdDB = new List<UserClinic>();
        var listDel = new List<UserClinic>();
        listNew.Sort((x, y) => { return x.UserClinicNum.CompareTo(y.UserClinicNum); });
        listDB.Sort((x, y) => { return x.UserClinicNum.CompareTo(y.UserClinicNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        UserClinic fieldNew;
        UserClinic fieldDB;
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

            if (fieldNew.UserClinicNum < fieldDB.UserClinicNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.UserClinicNum > fieldDB.UserClinicNum)
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

        DeleteMany(listDel.Select(x => x.UserClinicNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}