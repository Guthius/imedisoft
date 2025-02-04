using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UserClinicCrud
{
    public static List<UserClinic> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserClinic> TableToList(DataTable table)
    {
        var retVal = new List<UserClinic>();
        foreach (DataRow row in table.Rows)
        {
            var userClinic = new UserClinic
            {
                UserClinicNum = SIn.Long(row["UserClinicNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString())
            };
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

    public static void Insert(UserClinic userClinic)
    {
        var command = "INSERT INTO userclinic (";

        command += "UserNum,ClinicNum) VALUES(";

        command +=
            SOut.Long(userClinic.UserNum) + ","
                                          + SOut.Long(userClinic.ClinicNum) + ")";
        {
            userClinic.UserClinicNum = Db.NonQ(command, true, "UserClinicNum", "userClinic");
        }
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
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            UserClinic fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            UserClinic fieldDB = null;
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