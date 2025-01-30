using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class GuardianCrud
{
    public static List<Guardian> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Guardian> TableToList(DataTable table)
    {
        var retVal = new List<Guardian>();
        Guardian guardian;
        foreach (DataRow row in table.Rows)
        {
            guardian = new Guardian();
            guardian.GuardianNum = SIn.Long(row["GuardianNum"].ToString());
            guardian.PatNumChild = SIn.Long(row["PatNumChild"].ToString());
            guardian.PatNumGuardian = SIn.Long(row["PatNumGuardian"].ToString());
            guardian.Relationship = (GuardianRelationship) SIn.Int(row["Relationship"].ToString());
            guardian.IsGuardian = SIn.Bool(row["IsGuardian"].ToString());
            retVal.Add(guardian);
        }

        return retVal;
    }

    public static void Insert(Guardian guardian)
    {
        var command = "INSERT INTO guardian (";

        command += "PatNumChild,PatNumGuardian,Relationship,IsGuardian) VALUES(";

        command +=
            SOut.Long(guardian.PatNumChild) + ","
                                            + SOut.Long(guardian.PatNumGuardian) + ","
                                            + SOut.Int((int) guardian.Relationship) + ","
                                            + SOut.Bool(guardian.IsGuardian) + ")";
        {
            guardian.GuardianNum = Db.NonQ(command, true, "GuardianNum", "guardian");
        }
    }

    public static void Update(Guardian guardian)
    {
        var command = "UPDATE guardian SET "
                      + "PatNumChild   =  " + SOut.Long(guardian.PatNumChild) + ", "
                      + "PatNumGuardian=  " + SOut.Long(guardian.PatNumGuardian) + ", "
                      + "Relationship  =  " + SOut.Int((int) guardian.Relationship) + ", "
                      + "IsGuardian    =  " + SOut.Bool(guardian.IsGuardian) + " "
                      + "WHERE GuardianNum = " + SOut.Long(guardian.GuardianNum);
        Db.NonQ(command);
    }

    public static bool Update(Guardian guardian, Guardian oldGuardian)
    {
        var command = "";
        if (guardian.PatNumChild != oldGuardian.PatNumChild)
        {
            if (command != "") command += ",";
            command += "PatNumChild = " + SOut.Long(guardian.PatNumChild) + "";
        }

        if (guardian.PatNumGuardian != oldGuardian.PatNumGuardian)
        {
            if (command != "") command += ",";
            command += "PatNumGuardian = " + SOut.Long(guardian.PatNumGuardian) + "";
        }

        if (guardian.Relationship != oldGuardian.Relationship)
        {
            if (command != "") command += ",";
            command += "Relationship = " + SOut.Int((int) guardian.Relationship) + "";
        }

        if (guardian.IsGuardian != oldGuardian.IsGuardian)
        {
            if (command != "") command += ",";
            command += "IsGuardian = " + SOut.Bool(guardian.IsGuardian) + "";
        }

        if (command == "") return false;
        command = "UPDATE guardian SET " + command
                                         + " WHERE GuardianNum = " + SOut.Long(guardian.GuardianNum);
        Db.NonQ(command);
        return true;
    }

    public static void Delete(long guardianNum)
    {
        var command = "DELETE FROM guardian "
                      + "WHERE GuardianNum = " + SOut.Long(guardianNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listGuardianNums)
    {
        if (listGuardianNums == null || listGuardianNums.Count == 0) return;
        var command = "DELETE FROM guardian "
                      + "WHERE GuardianNum IN(" + string.Join(",", listGuardianNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<Guardian> listNew, List<Guardian> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Guardian>();
        var listUpdNew = new List<Guardian>();
        var listUpdDB = new List<Guardian>();
        var listDel = new List<Guardian>();
        listNew.Sort((x, y) => { return x.GuardianNum.CompareTo(y.GuardianNum); });
        listDB.Sort((x, y) => { return x.GuardianNum.CompareTo(y.GuardianNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        Guardian fieldNew;
        Guardian fieldDB;
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

            if (fieldNew.GuardianNum < fieldDB.GuardianNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.GuardianNum > fieldDB.GuardianNum)
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

        DeleteMany(listDel.Select(x => x.GuardianNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}