using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HieClinicCrud
{
    public static List<HieClinic> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HieClinic> TableToList(DataTable table)
    {
        var retVal = new List<HieClinic>();
        foreach (DataRow row in table.Rows)
        {
            var hieClinic = new HieClinic
            {
                HieClinicNum = SIn.Long(row["HieClinicNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                SupportedCarrierFlags = (HieCarrierFlags) SIn.Int(row["SupportedCarrierFlags"].ToString()),
                PathExportCCD = SIn.String(row["PathExportCCD"].ToString()),
                TimeOfDayExportCCD = TimeSpan.FromTicks(SIn.Long(row["TimeOfDayExportCCD"].ToString())),
                IsEnabled = SIn.Bool(row["IsEnabled"].ToString())
            };
            retVal.Add(hieClinic);
        }

        return retVal;
    }

    public static void Insert(HieClinic hieClinic)
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

    public static void DeleteMany(List<long> listHieClinicNums)
    {
        if (listHieClinicNums == null || listHieClinicNums.Count == 0) return;
        var command = "DELETE FROM hieclinic "
                      + "WHERE HieClinicNum IN(" + string.Join(",", listHieClinicNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<HieClinic> listNew, List<HieClinic> listDB)
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
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            HieClinic fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            HieClinic fieldDB = null;
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
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}