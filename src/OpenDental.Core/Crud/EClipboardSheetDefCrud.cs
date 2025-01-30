using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EClipboardSheetDefCrud
{
    public static List<EClipboardSheetDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EClipboardSheetDef> TableToList(DataTable table)
    {
        var retVal = new List<EClipboardSheetDef>();
        EClipboardSheetDef eClipboardSheetDef;
        foreach (DataRow row in table.Rows)
        {
            eClipboardSheetDef = new EClipboardSheetDef();
            eClipboardSheetDef.EClipboardSheetDefNum = SIn.Long(row["EClipboardSheetDefNum"].ToString());
            eClipboardSheetDef.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
            eClipboardSheetDef.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            eClipboardSheetDef.ResubmitInterval = TimeSpan.FromTicks(SIn.Long(row["ResubmitInterval"].ToString()));
            eClipboardSheetDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eClipboardSheetDef.PrefillStatus = (PrefillStatuses) SIn.Int(row["PrefillStatus"].ToString());
            eClipboardSheetDef.MinAge = SIn.Int(row["MinAge"].ToString());
            eClipboardSheetDef.MaxAge = SIn.Int(row["MaxAge"].ToString());
            eClipboardSheetDef.IgnoreSheetDefNums = SIn.String(row["IgnoreSheetDefNums"].ToString());
            eClipboardSheetDef.PrefillStatusOverride = SIn.Long(row["PrefillStatusOverride"].ToString());
            eClipboardSheetDef.EFormDefNum = SIn.Long(row["EFormDefNum"].ToString());
            eClipboardSheetDef.Frequency = (EnumEClipFreq) SIn.Int(row["Frequency"].ToString());
            retVal.Add(eClipboardSheetDef);
        }

        return retVal;
    }

    public static void Insert(EClipboardSheetDef eClipboardSheetDef)
    {
        var command = "INSERT INTO eclipboardsheetdef (";

        command += "SheetDefNum,ClinicNum,ResubmitInterval,ItemOrder,PrefillStatus,MinAge,MaxAge,IgnoreSheetDefNums,PrefillStatusOverride,EFormDefNum,Frequency) VALUES(";

        command +=
            SOut.Long(eClipboardSheetDef.SheetDefNum) + ","
                                                      + SOut.Long(eClipboardSheetDef.ClinicNum) + ","
                                                      + "'" + SOut.Long(eClipboardSheetDef.ResubmitInterval.Ticks) + "',"
                                                      + SOut.Int(eClipboardSheetDef.ItemOrder) + ","
                                                      + SOut.Int((int) eClipboardSheetDef.PrefillStatus) + ","
                                                      + SOut.Int(eClipboardSheetDef.MinAge) + ","
                                                      + SOut.Int(eClipboardSheetDef.MaxAge) + ","
                                                      + DbHelper.ParamChar + "paramIgnoreSheetDefNums,"
                                                      + SOut.Long(eClipboardSheetDef.PrefillStatusOverride) + ","
                                                      + SOut.Long(eClipboardSheetDef.EFormDefNum) + ","
                                                      + SOut.Int((int) eClipboardSheetDef.Frequency) + ")";
        if (eClipboardSheetDef.IgnoreSheetDefNums == null) eClipboardSheetDef.IgnoreSheetDefNums = "";
        var paramIgnoreSheetDefNums = new OdSqlParameter("paramIgnoreSheetDefNums", SOut.StringParam(eClipboardSheetDef.IgnoreSheetDefNums));
        {
            eClipboardSheetDef.EClipboardSheetDefNum = Db.NonQ(command, true, "EClipboardSheetDefNum", "eClipboardSheetDef", paramIgnoreSheetDefNums);
        }
    }

    public static void Update(EClipboardSheetDef eClipboardSheetDef)
    {
        var command = "UPDATE eclipboardsheetdef SET "
                      + "SheetDefNum          =  " + SOut.Long(eClipboardSheetDef.SheetDefNum) + ", "
                      + "ClinicNum            =  " + SOut.Long(eClipboardSheetDef.ClinicNum) + ", "
                      + "ResubmitInterval     =  " + SOut.Long(eClipboardSheetDef.ResubmitInterval.Ticks) + ", "
                      + "ItemOrder            =  " + SOut.Int(eClipboardSheetDef.ItemOrder) + ", "
                      + "PrefillStatus        =  " + SOut.Int((int) eClipboardSheetDef.PrefillStatus) + ", "
                      + "MinAge               =  " + SOut.Int(eClipboardSheetDef.MinAge) + ", "
                      + "MaxAge               =  " + SOut.Int(eClipboardSheetDef.MaxAge) + ", "
                      + "IgnoreSheetDefNums   =  " + DbHelper.ParamChar + "paramIgnoreSheetDefNums, "
                      + "PrefillStatusOverride=  " + SOut.Long(eClipboardSheetDef.PrefillStatusOverride) + ", "
                      + "EFormDefNum          =  " + SOut.Long(eClipboardSheetDef.EFormDefNum) + ", "
                      + "Frequency            =  " + SOut.Int((int) eClipboardSheetDef.Frequency) + " "
                      + "WHERE EClipboardSheetDefNum = " + SOut.Long(eClipboardSheetDef.EClipboardSheetDefNum);
        if (eClipboardSheetDef.IgnoreSheetDefNums == null) eClipboardSheetDef.IgnoreSheetDefNums = "";
        var paramIgnoreSheetDefNums = new OdSqlParameter("paramIgnoreSheetDefNums", SOut.StringParam(eClipboardSheetDef.IgnoreSheetDefNums));
        Db.NonQ(command, paramIgnoreSheetDefNums);
    }

    public static bool Update(EClipboardSheetDef eClipboardSheetDef, EClipboardSheetDef oldEClipboardSheetDef)
    {
        var command = "";
        if (eClipboardSheetDef.SheetDefNum != oldEClipboardSheetDef.SheetDefNum)
        {
            if (command != "") command += ",";
            command += "SheetDefNum = " + SOut.Long(eClipboardSheetDef.SheetDefNum) + "";
        }

        if (eClipboardSheetDef.ClinicNum != oldEClipboardSheetDef.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(eClipboardSheetDef.ClinicNum) + "";
        }

        if (eClipboardSheetDef.ResubmitInterval != oldEClipboardSheetDef.ResubmitInterval)
        {
            if (command != "") command += ",";
            command += "ResubmitInterval = '" + SOut.Long(eClipboardSheetDef.ResubmitInterval.Ticks) + "'";
        }

        if (eClipboardSheetDef.ItemOrder != oldEClipboardSheetDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(eClipboardSheetDef.ItemOrder) + "";
        }

        if (eClipboardSheetDef.PrefillStatus != oldEClipboardSheetDef.PrefillStatus)
        {
            if (command != "") command += ",";
            command += "PrefillStatus = " + SOut.Int((int) eClipboardSheetDef.PrefillStatus) + "";
        }

        if (eClipboardSheetDef.MinAge != oldEClipboardSheetDef.MinAge)
        {
            if (command != "") command += ",";
            command += "MinAge = " + SOut.Int(eClipboardSheetDef.MinAge) + "";
        }

        if (eClipboardSheetDef.MaxAge != oldEClipboardSheetDef.MaxAge)
        {
            if (command != "") command += ",";
            command += "MaxAge = " + SOut.Int(eClipboardSheetDef.MaxAge) + "";
        }

        if (eClipboardSheetDef.IgnoreSheetDefNums != oldEClipboardSheetDef.IgnoreSheetDefNums)
        {
            if (command != "") command += ",";
            command += "IgnoreSheetDefNums = " + DbHelper.ParamChar + "paramIgnoreSheetDefNums";
        }

        if (eClipboardSheetDef.PrefillStatusOverride != oldEClipboardSheetDef.PrefillStatusOverride)
        {
            if (command != "") command += ",";
            command += "PrefillStatusOverride = " + SOut.Long(eClipboardSheetDef.PrefillStatusOverride) + "";
        }

        if (eClipboardSheetDef.EFormDefNum != oldEClipboardSheetDef.EFormDefNum)
        {
            if (command != "") command += ",";
            command += "EFormDefNum = " + SOut.Long(eClipboardSheetDef.EFormDefNum) + "";
        }

        if (eClipboardSheetDef.Frequency != oldEClipboardSheetDef.Frequency)
        {
            if (command != "") command += ",";
            command += "Frequency = " + SOut.Int((int) eClipboardSheetDef.Frequency) + "";
        }

        if (command == "") return false;
        if (eClipboardSheetDef.IgnoreSheetDefNums == null) eClipboardSheetDef.IgnoreSheetDefNums = "";
        var paramIgnoreSheetDefNums = new OdSqlParameter("paramIgnoreSheetDefNums", SOut.StringParam(eClipboardSheetDef.IgnoreSheetDefNums));
        command = "UPDATE eclipboardsheetdef SET " + command
                                                   + " WHERE EClipboardSheetDefNum = " + SOut.Long(eClipboardSheetDef.EClipboardSheetDefNum);
        Db.NonQ(command, paramIgnoreSheetDefNums);
        return true;
    }

    public static void DeleteMany(List<long> listEClipboardSheetDefNums)
    {
        if (listEClipboardSheetDefNums == null || listEClipboardSheetDefNums.Count == 0) return;
        var command = "DELETE FROM eclipboardsheetdef "
                      + "WHERE EClipboardSheetDefNum IN(" + string.Join(",", listEClipboardSheetDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static void Sync(List<EClipboardSheetDef> listNew, List<EClipboardSheetDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<EClipboardSheetDef>();
        var listUpdNew = new List<EClipboardSheetDef>();
        var listUpdDB = new List<EClipboardSheetDef>();
        var listDel = new List<EClipboardSheetDef>();
        listNew.Sort((x, y) => { return x.EClipboardSheetDefNum.CompareTo(y.EClipboardSheetDefNum); });
        listDB.Sort((x, y) => { return x.EClipboardSheetDefNum.CompareTo(y.EClipboardSheetDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        EClipboardSheetDef fieldNew;
        EClipboardSheetDef fieldDB;
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

            if (fieldNew.EClipboardSheetDefNum < fieldDB.EClipboardSheetDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.EClipboardSheetDefNum > fieldDB.EClipboardSheetDefNum)
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

        DeleteMany(listDel.Select(x => x.EClipboardSheetDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return;
    }
}