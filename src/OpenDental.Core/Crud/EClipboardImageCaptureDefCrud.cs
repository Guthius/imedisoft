#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EClipboardImageCaptureDefCrud
{
    public static EClipboardImageCaptureDef SelectOne(long eClipboardImageCaptureDefNum)
    {
        var command = "SELECT * FROM eclipboardimagecapturedef "
                      + "WHERE EClipboardImageCaptureDefNum = " + SOut.Long(eClipboardImageCaptureDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EClipboardImageCaptureDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EClipboardImageCaptureDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EClipboardImageCaptureDef> TableToList(DataTable table)
    {
        var retVal = new List<EClipboardImageCaptureDef>();
        EClipboardImageCaptureDef eClipboardImageCaptureDef;
        foreach (DataRow row in table.Rows)
        {
            eClipboardImageCaptureDef = new EClipboardImageCaptureDef();
            eClipboardImageCaptureDef.EClipboardImageCaptureDefNum = SIn.Long(row["EClipboardImageCaptureDefNum"].ToString());
            eClipboardImageCaptureDef.DefNum = SIn.Long(row["DefNum"].ToString());
            eClipboardImageCaptureDef.IsSelfPortrait = SIn.Bool(row["IsSelfPortrait"].ToString());
            eClipboardImageCaptureDef.FrequencyDays = SIn.Int(row["FrequencyDays"].ToString());
            eClipboardImageCaptureDef.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            eClipboardImageCaptureDef.OcrCaptureType = (EnumOcrCaptureType) SIn.Int(row["OcrCaptureType"].ToString());
            eClipboardImageCaptureDef.Frequency = (EnumEClipFreq) SIn.Int(row["Frequency"].ToString());
            eClipboardImageCaptureDef.ResubmitInterval = TimeSpan.FromTicks(SIn.Long(row["ResubmitInterval"].ToString()));
            retVal.Add(eClipboardImageCaptureDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EClipboardImageCaptureDef> listEClipboardImageCaptureDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EClipboardImageCaptureDef";
        var table = new DataTable(tableName);
        table.Columns.Add("EClipboardImageCaptureDefNum");
        table.Columns.Add("DefNum");
        table.Columns.Add("IsSelfPortrait");
        table.Columns.Add("FrequencyDays");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("OcrCaptureType");
        table.Columns.Add("Frequency");
        table.Columns.Add("ResubmitInterval");
        foreach (var eClipboardImageCaptureDef in listEClipboardImageCaptureDefs)
            table.Rows.Add(SOut.Long(eClipboardImageCaptureDef.EClipboardImageCaptureDefNum), SOut.Long(eClipboardImageCaptureDef.DefNum), SOut.Bool(eClipboardImageCaptureDef.IsSelfPortrait), SOut.Int(eClipboardImageCaptureDef.FrequencyDays), SOut.Long(eClipboardImageCaptureDef.ClinicNum), SOut.Int((int) eClipboardImageCaptureDef.OcrCaptureType), SOut.Int((int) eClipboardImageCaptureDef.Frequency), SOut.Long(eClipboardImageCaptureDef.ResubmitInterval.Ticks));
        return table;
    }

    public static long Insert(EClipboardImageCaptureDef eClipboardImageCaptureDef)
    {
        return Insert(eClipboardImageCaptureDef, false);
    }

    public static long Insert(EClipboardImageCaptureDef eClipboardImageCaptureDef, bool useExistingPK)
    {
        var command = "INSERT INTO eclipboardimagecapturedef (";

        command += "DefNum,IsSelfPortrait,FrequencyDays,ClinicNum,OcrCaptureType,Frequency,ResubmitInterval) VALUES(";

        command +=
            SOut.Long(eClipboardImageCaptureDef.DefNum) + ","
                                                        + SOut.Bool(eClipboardImageCaptureDef.IsSelfPortrait) + ","
                                                        + SOut.Int(eClipboardImageCaptureDef.FrequencyDays) + ","
                                                        + SOut.Long(eClipboardImageCaptureDef.ClinicNum) + ","
                                                        + SOut.Int((int) eClipboardImageCaptureDef.OcrCaptureType) + ","
                                                        + SOut.Int((int) eClipboardImageCaptureDef.Frequency) + ","
                                                        + "'" + SOut.Long(eClipboardImageCaptureDef.ResubmitInterval.Ticks) + "')";
        {
            eClipboardImageCaptureDef.EClipboardImageCaptureDefNum = Db.NonQ(command, true, "EClipboardImageCaptureDefNum", "eClipboardImageCaptureDef");
        }
        return eClipboardImageCaptureDef.EClipboardImageCaptureDefNum;
    }

    public static long InsertNoCache(EClipboardImageCaptureDef eClipboardImageCaptureDef)
    {
        return InsertNoCache(eClipboardImageCaptureDef, false);
    }

    public static long InsertNoCache(EClipboardImageCaptureDef eClipboardImageCaptureDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eclipboardimagecapturedef (";
        if (isRandomKeys || useExistingPK) command += "EClipboardImageCaptureDefNum,";
        command += "DefNum,IsSelfPortrait,FrequencyDays,ClinicNum,OcrCaptureType,Frequency,ResubmitInterval) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eClipboardImageCaptureDef.EClipboardImageCaptureDefNum) + ",";
        command +=
            SOut.Long(eClipboardImageCaptureDef.DefNum) + ","
                                                        + SOut.Bool(eClipboardImageCaptureDef.IsSelfPortrait) + ","
                                                        + SOut.Int(eClipboardImageCaptureDef.FrequencyDays) + ","
                                                        + SOut.Long(eClipboardImageCaptureDef.ClinicNum) + ","
                                                        + SOut.Int((int) eClipboardImageCaptureDef.OcrCaptureType) + ","
                                                        + SOut.Int((int) eClipboardImageCaptureDef.Frequency) + ","
                                                        + "'" + SOut.Long(eClipboardImageCaptureDef.ResubmitInterval.Ticks) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eClipboardImageCaptureDef.EClipboardImageCaptureDefNum = Db.NonQ(command, true, "EClipboardImageCaptureDefNum", "eClipboardImageCaptureDef");
        return eClipboardImageCaptureDef.EClipboardImageCaptureDefNum;
    }

    public static void Update(EClipboardImageCaptureDef eClipboardImageCaptureDef)
    {
        var command = "UPDATE eclipboardimagecapturedef SET "
                      + "DefNum                      =  " + SOut.Long(eClipboardImageCaptureDef.DefNum) + ", "
                      + "IsSelfPortrait              =  " + SOut.Bool(eClipboardImageCaptureDef.IsSelfPortrait) + ", "
                      + "FrequencyDays               =  " + SOut.Int(eClipboardImageCaptureDef.FrequencyDays) + ", "
                      + "ClinicNum                   =  " + SOut.Long(eClipboardImageCaptureDef.ClinicNum) + ", "
                      + "OcrCaptureType              =  " + SOut.Int((int) eClipboardImageCaptureDef.OcrCaptureType) + ", "
                      + "Frequency                   =  " + SOut.Int((int) eClipboardImageCaptureDef.Frequency) + ", "
                      + "ResubmitInterval            =  " + SOut.Long(eClipboardImageCaptureDef.ResubmitInterval.Ticks) + " "
                      + "WHERE EClipboardImageCaptureDefNum = " + SOut.Long(eClipboardImageCaptureDef.EClipboardImageCaptureDefNum);
        Db.NonQ(command);
    }

    public static bool Update(EClipboardImageCaptureDef eClipboardImageCaptureDef, EClipboardImageCaptureDef oldEClipboardImageCaptureDef)
    {
        var command = "";
        if (eClipboardImageCaptureDef.DefNum != oldEClipboardImageCaptureDef.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(eClipboardImageCaptureDef.DefNum) + "";
        }

        if (eClipboardImageCaptureDef.IsSelfPortrait != oldEClipboardImageCaptureDef.IsSelfPortrait)
        {
            if (command != "") command += ",";
            command += "IsSelfPortrait = " + SOut.Bool(eClipboardImageCaptureDef.IsSelfPortrait) + "";
        }

        if (eClipboardImageCaptureDef.FrequencyDays != oldEClipboardImageCaptureDef.FrequencyDays)
        {
            if (command != "") command += ",";
            command += "FrequencyDays = " + SOut.Int(eClipboardImageCaptureDef.FrequencyDays) + "";
        }

        if (eClipboardImageCaptureDef.ClinicNum != oldEClipboardImageCaptureDef.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(eClipboardImageCaptureDef.ClinicNum) + "";
        }

        if (eClipboardImageCaptureDef.OcrCaptureType != oldEClipboardImageCaptureDef.OcrCaptureType)
        {
            if (command != "") command += ",";
            command += "OcrCaptureType = " + SOut.Int((int) eClipboardImageCaptureDef.OcrCaptureType) + "";
        }

        if (eClipboardImageCaptureDef.Frequency != oldEClipboardImageCaptureDef.Frequency)
        {
            if (command != "") command += ",";
            command += "Frequency = " + SOut.Int((int) eClipboardImageCaptureDef.Frequency) + "";
        }

        if (eClipboardImageCaptureDef.ResubmitInterval != oldEClipboardImageCaptureDef.ResubmitInterval)
        {
            if (command != "") command += ",";
            command += "ResubmitInterval = '" + SOut.Long(eClipboardImageCaptureDef.ResubmitInterval.Ticks) + "'";
        }

        if (command == "") return false;
        command = "UPDATE eclipboardimagecapturedef SET " + command
                                                          + " WHERE EClipboardImageCaptureDefNum = " + SOut.Long(eClipboardImageCaptureDef.EClipboardImageCaptureDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EClipboardImageCaptureDef eClipboardImageCaptureDef, EClipboardImageCaptureDef oldEClipboardImageCaptureDef)
    {
        if (eClipboardImageCaptureDef.DefNum != oldEClipboardImageCaptureDef.DefNum) return true;
        if (eClipboardImageCaptureDef.IsSelfPortrait != oldEClipboardImageCaptureDef.IsSelfPortrait) return true;
        if (eClipboardImageCaptureDef.FrequencyDays != oldEClipboardImageCaptureDef.FrequencyDays) return true;
        if (eClipboardImageCaptureDef.ClinicNum != oldEClipboardImageCaptureDef.ClinicNum) return true;
        if (eClipboardImageCaptureDef.OcrCaptureType != oldEClipboardImageCaptureDef.OcrCaptureType) return true;
        if (eClipboardImageCaptureDef.Frequency != oldEClipboardImageCaptureDef.Frequency) return true;
        if (eClipboardImageCaptureDef.ResubmitInterval != oldEClipboardImageCaptureDef.ResubmitInterval) return true;
        return false;
    }

    public static void Delete(long eClipboardImageCaptureDefNum)
    {
        var command = "DELETE FROM eclipboardimagecapturedef "
                      + "WHERE EClipboardImageCaptureDefNum = " + SOut.Long(eClipboardImageCaptureDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEClipboardImageCaptureDefNums)
    {
        if (listEClipboardImageCaptureDefNums == null || listEClipboardImageCaptureDefNums.Count == 0) return;
        var command = "DELETE FROM eclipboardimagecapturedef "
                      + "WHERE EClipboardImageCaptureDefNum IN(" + string.Join(",", listEClipboardImageCaptureDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<EClipboardImageCaptureDef> listNew, List<EClipboardImageCaptureDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<EClipboardImageCaptureDef>();
        var listUpdNew = new List<EClipboardImageCaptureDef>();
        var listUpdDB = new List<EClipboardImageCaptureDef>();
        var listDel = new List<EClipboardImageCaptureDef>();
        listNew.Sort((x, y) => { return x.EClipboardImageCaptureDefNum.CompareTo(y.EClipboardImageCaptureDefNum); });
        listDB.Sort((x, y) => { return x.EClipboardImageCaptureDefNum.CompareTo(y.EClipboardImageCaptureDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        EClipboardImageCaptureDef fieldNew;
        EClipboardImageCaptureDef fieldDB;
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

            if (fieldNew.EClipboardImageCaptureDefNum < fieldDB.EClipboardImageCaptureDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.EClipboardImageCaptureDefNum > fieldDB.EClipboardImageCaptureDefNum)
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

        DeleteMany(listDel.Select(x => x.EClipboardImageCaptureDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}