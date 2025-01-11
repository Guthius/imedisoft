#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HieClinicCrud
{
    public static HieClinic SelectOne(long hieClinicNum)
    {
        var command = "SELECT * FROM hieclinic "
                      + "WHERE HieClinicNum = " + SOut.Long(hieClinicNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HieClinic SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HieClinic> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HieClinic> TableToList(DataTable table)
    {
        var retVal = new List<HieClinic>();
        HieClinic hieClinic;
        foreach (DataRow row in table.Rows)
        {
            hieClinic = new HieClinic();
            hieClinic.HieClinicNum = SIn.Long(row["HieClinicNum"].ToString());
            hieClinic.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            hieClinic.SupportedCarrierFlags = (HieCarrierFlags) SIn.Int(row["SupportedCarrierFlags"].ToString());
            hieClinic.PathExportCCD = SIn.String(row["PathExportCCD"].ToString());
            hieClinic.TimeOfDayExportCCD = TimeSpan.FromTicks(SIn.Long(row["TimeOfDayExportCCD"].ToString()));
            hieClinic.IsEnabled = SIn.Bool(row["IsEnabled"].ToString());
            retVal.Add(hieClinic);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HieClinic> listHieClinics, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HieClinic";
        var table = new DataTable(tableName);
        table.Columns.Add("HieClinicNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SupportedCarrierFlags");
        table.Columns.Add("PathExportCCD");
        table.Columns.Add("TimeOfDayExportCCD");
        table.Columns.Add("IsEnabled");
        foreach (var hieClinic in listHieClinics)
            table.Rows.Add(SOut.Long(hieClinic.HieClinicNum), SOut.Long(hieClinic.ClinicNum), SOut.Int((int) hieClinic.SupportedCarrierFlags), hieClinic.PathExportCCD, SOut.Long(hieClinic.TimeOfDayExportCCD.Ticks), SOut.Bool(hieClinic.IsEnabled));
        return table;
    }

    public static long Insert(HieClinic hieClinic)
    {
        return Insert(hieClinic, false);
    }

    public static long Insert(HieClinic hieClinic, bool useExistingPK)
    {
        var command = "INSERT INTO hieclinic (";

        command += "ClinicNum,SupportedCarrierFlags,PathExportCCD,TimeOfDayExportCCD,IsEnabled) VALUES(";

        command +=
            SOut.Long(hieClinic.ClinicNum) + ","
                                           + SOut.Int((int) hieClinic.SupportedCarrierFlags) + ","
                                           + "'" + SOut.String(hieClinic.PathExportCCD) + "',"
                                           + "'" + SOut.Long(hieClinic.TimeOfDayExportCCD.Ticks) + "',"
                                           + SOut.Bool(hieClinic.IsEnabled) + ")";
        {
            hieClinic.HieClinicNum = Db.NonQ(command, true, "HieClinicNum", "hieClinic");
        }
        return hieClinic.HieClinicNum;
    }

    public static long InsertNoCache(HieClinic hieClinic)
    {
        return InsertNoCache(hieClinic, false);
    }

    public static long InsertNoCache(HieClinic hieClinic, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hieclinic (";
        if (isRandomKeys || useExistingPK) command += "HieClinicNum,";
        command += "ClinicNum,SupportedCarrierFlags,PathExportCCD,TimeOfDayExportCCD,IsEnabled) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hieClinic.HieClinicNum) + ",";
        command +=
            SOut.Long(hieClinic.ClinicNum) + ","
                                           + SOut.Int((int) hieClinic.SupportedCarrierFlags) + ","
                                           + "'" + SOut.String(hieClinic.PathExportCCD) + "',"
                                           + "'" + SOut.Long(hieClinic.TimeOfDayExportCCD.Ticks) + "',"
                                           + SOut.Bool(hieClinic.IsEnabled) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            hieClinic.HieClinicNum = Db.NonQ(command, true, "HieClinicNum", "hieClinic");
        return hieClinic.HieClinicNum;
    }

    public static void Update(HieClinic hieClinic)
    {
        var command = "UPDATE hieclinic SET "
                      + "ClinicNum            =  " + SOut.Long(hieClinic.ClinicNum) + ", "
                      + "SupportedCarrierFlags=  " + SOut.Int((int) hieClinic.SupportedCarrierFlags) + ", "
                      + "PathExportCCD        = '" + SOut.String(hieClinic.PathExportCCD) + "', "
                      + "TimeOfDayExportCCD   =  " + SOut.Long(hieClinic.TimeOfDayExportCCD.Ticks) + ", "
                      + "IsEnabled            =  " + SOut.Bool(hieClinic.IsEnabled) + " "
                      + "WHERE HieClinicNum = " + SOut.Long(hieClinic.HieClinicNum);
        Db.NonQ(command);
    }

    public static bool Update(HieClinic hieClinic, HieClinic oldHieClinic)
    {
        var command = "";
        if (hieClinic.ClinicNum != oldHieClinic.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(hieClinic.ClinicNum) + "";
        }

        if (hieClinic.SupportedCarrierFlags != oldHieClinic.SupportedCarrierFlags)
        {
            if (command != "") command += ",";
            command += "SupportedCarrierFlags = " + SOut.Int((int) hieClinic.SupportedCarrierFlags) + "";
        }

        if (hieClinic.PathExportCCD != oldHieClinic.PathExportCCD)
        {
            if (command != "") command += ",";
            command += "PathExportCCD = '" + SOut.String(hieClinic.PathExportCCD) + "'";
        }

        if (hieClinic.TimeOfDayExportCCD != oldHieClinic.TimeOfDayExportCCD)
        {
            if (command != "") command += ",";
            command += "TimeOfDayExportCCD = '" + SOut.Long(hieClinic.TimeOfDayExportCCD.Ticks) + "'";
        }

        if (hieClinic.IsEnabled != oldHieClinic.IsEnabled)
        {
            if (command != "") command += ",";
            command += "IsEnabled = " + SOut.Bool(hieClinic.IsEnabled) + "";
        }

        if (command == "") return false;
        command = "UPDATE hieclinic SET " + command
                                          + " WHERE HieClinicNum = " + SOut.Long(hieClinic.HieClinicNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(HieClinic hieClinic, HieClinic oldHieClinic)
    {
        if (hieClinic.ClinicNum != oldHieClinic.ClinicNum) return true;
        if (hieClinic.SupportedCarrierFlags != oldHieClinic.SupportedCarrierFlags) return true;
        if (hieClinic.PathExportCCD != oldHieClinic.PathExportCCD) return true;
        if (hieClinic.TimeOfDayExportCCD != oldHieClinic.TimeOfDayExportCCD) return true;
        if (hieClinic.IsEnabled != oldHieClinic.IsEnabled) return true;
        return false;
    }

    public static void Delete(long hieClinicNum)
    {
        var command = "DELETE FROM hieclinic "
                      + "WHERE HieClinicNum = " + SOut.Long(hieClinicNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHieClinicNums)
    {
        if (listHieClinicNums == null || listHieClinicNums.Count == 0) return;
        var command = "DELETE FROM hieclinic "
                      + "WHERE HieClinicNum IN(" + string.Join(",", listHieClinicNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<HieClinic> listNew, List<HieClinic> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<HieClinic>();
        var listUpdNew = new List<HieClinic>();
        var listUpdDB = new List<HieClinic>();
        var listDel = new List<HieClinic>();
        listNew.Sort((x, y) => { return x.HieClinicNum.CompareTo(y.HieClinicNum); });
        listDB.Sort((x, y) => { return x.HieClinicNum.CompareTo(y.HieClinicNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        HieClinic fieldNew;
        HieClinic fieldDB;
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

            if (fieldNew.HieClinicNum < fieldDB.HieClinicNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.HieClinicNum > fieldDB.HieClinicNum)
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

        DeleteMany(listDel.Select(x => x.HieClinicNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}