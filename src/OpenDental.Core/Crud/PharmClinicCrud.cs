#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PharmClinicCrud
{
    public static PharmClinic SelectOne(long pharmClinicNum)
    {
        var command = "SELECT * FROM pharmclinic "
                      + "WHERE PharmClinicNum = " + SOut.Long(pharmClinicNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PharmClinic SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PharmClinic> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PharmClinic> TableToList(DataTable table)
    {
        var retVal = new List<PharmClinic>();
        PharmClinic pharmClinic;
        foreach (DataRow row in table.Rows)
        {
            pharmClinic = new PharmClinic();
            pharmClinic.PharmClinicNum = SIn.Long(row["PharmClinicNum"].ToString());
            pharmClinic.PharmacyNum = SIn.Long(row["PharmacyNum"].ToString());
            pharmClinic.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            retVal.Add(pharmClinic);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PharmClinic> listPharmClinics, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PharmClinic";
        var table = new DataTable(tableName);
        table.Columns.Add("PharmClinicNum");
        table.Columns.Add("PharmacyNum");
        table.Columns.Add("ClinicNum");
        foreach (var pharmClinic in listPharmClinics)
            table.Rows.Add(SOut.Long(pharmClinic.PharmClinicNum), SOut.Long(pharmClinic.PharmacyNum), SOut.Long(pharmClinic.ClinicNum));
        return table;
    }

    public static long Insert(PharmClinic pharmClinic)
    {
        return Insert(pharmClinic, false);
    }

    public static long Insert(PharmClinic pharmClinic, bool useExistingPK)
    {
        var command = "INSERT INTO pharmclinic (";

        command += "PharmacyNum,ClinicNum) VALUES(";

        command +=
            SOut.Long(pharmClinic.PharmacyNum) + ","
                                               + SOut.Long(pharmClinic.ClinicNum) + ")";
        {
            pharmClinic.PharmClinicNum = Db.NonQ(command, true, "PharmClinicNum", "pharmClinic");
        }
        return pharmClinic.PharmClinicNum;
    }

    public static long InsertNoCache(PharmClinic pharmClinic)
    {
        return InsertNoCache(pharmClinic, false);
    }

    public static long InsertNoCache(PharmClinic pharmClinic, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO pharmclinic (";
        if (isRandomKeys || useExistingPK) command += "PharmClinicNum,";
        command += "PharmacyNum,ClinicNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(pharmClinic.PharmClinicNum) + ",";
        command +=
            SOut.Long(pharmClinic.PharmacyNum) + ","
                                               + SOut.Long(pharmClinic.ClinicNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            pharmClinic.PharmClinicNum = Db.NonQ(command, true, "PharmClinicNum", "pharmClinic");
        return pharmClinic.PharmClinicNum;
    }

    public static void Update(PharmClinic pharmClinic)
    {
        var command = "UPDATE pharmclinic SET "
                      + "PharmacyNum   =  " + SOut.Long(pharmClinic.PharmacyNum) + ", "
                      + "ClinicNum     =  " + SOut.Long(pharmClinic.ClinicNum) + " "
                      + "WHERE PharmClinicNum = " + SOut.Long(pharmClinic.PharmClinicNum);
        Db.NonQ(command);
    }

    public static bool Update(PharmClinic pharmClinic, PharmClinic oldPharmClinic)
    {
        var command = "";
        if (pharmClinic.PharmacyNum != oldPharmClinic.PharmacyNum)
        {
            if (command != "") command += ",";
            command += "PharmacyNum = " + SOut.Long(pharmClinic.PharmacyNum) + "";
        }

        if (pharmClinic.ClinicNum != oldPharmClinic.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(pharmClinic.ClinicNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE pharmclinic SET " + command
                                            + " WHERE PharmClinicNum = " + SOut.Long(pharmClinic.PharmClinicNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PharmClinic pharmClinic, PharmClinic oldPharmClinic)
    {
        if (pharmClinic.PharmacyNum != oldPharmClinic.PharmacyNum) return true;
        if (pharmClinic.ClinicNum != oldPharmClinic.ClinicNum) return true;
        return false;
    }

    public static void Delete(long pharmClinicNum)
    {
        var command = "DELETE FROM pharmclinic "
                      + "WHERE PharmClinicNum = " + SOut.Long(pharmClinicNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPharmClinicNums)
    {
        if (listPharmClinicNums == null || listPharmClinicNums.Count == 0) return;
        var command = "DELETE FROM pharmclinic "
                      + "WHERE PharmClinicNum IN(" + string.Join(",", listPharmClinicNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<PharmClinic> listNew, List<PharmClinic> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<PharmClinic>();
        var listUpdNew = new List<PharmClinic>();
        var listUpdDB = new List<PharmClinic>();
        var listDel = new List<PharmClinic>();
        listNew.Sort((x, y) => { return x.PharmClinicNum.CompareTo(y.PharmClinicNum); });
        listDB.Sort((x, y) => { return x.PharmClinicNum.CompareTo(y.PharmClinicNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        PharmClinic fieldNew;
        PharmClinic fieldDB;
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

            if (fieldNew.PharmClinicNum < fieldDB.PharmClinicNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.PharmClinicNum > fieldDB.PharmClinicNum)
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

        DeleteMany(listDel.Select(x => x.PharmClinicNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}