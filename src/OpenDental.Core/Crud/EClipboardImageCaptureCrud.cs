#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EClipboardImageCaptureCrud
{
    public static EClipboardImageCapture SelectOne(long eClipboardImageCaptureNum)
    {
        var command = "SELECT * FROM eclipboardimagecapture "
                      + "WHERE EClipboardImageCaptureNum = " + SOut.Long(eClipboardImageCaptureNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EClipboardImageCapture SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EClipboardImageCapture> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EClipboardImageCapture> TableToList(DataTable table)
    {
        var retVal = new List<EClipboardImageCapture>();
        EClipboardImageCapture eClipboardImageCapture;
        foreach (DataRow row in table.Rows)
        {
            eClipboardImageCapture = new EClipboardImageCapture();
            eClipboardImageCapture.EClipboardImageCaptureNum = SIn.Long(row["EClipboardImageCaptureNum"].ToString());
            eClipboardImageCapture.PatNum = SIn.Long(row["PatNum"].ToString());
            eClipboardImageCapture.DefNum = SIn.Long(row["DefNum"].ToString());
            eClipboardImageCapture.IsSelfPortrait = SIn.Bool(row["IsSelfPortrait"].ToString());
            eClipboardImageCapture.DateTimeUpserted = SIn.DateTime(row["DateTimeUpserted"].ToString());
            eClipboardImageCapture.DocNum = SIn.Long(row["DocNum"].ToString());
            eClipboardImageCapture.OcrCaptureType = (EnumOcrCaptureType) SIn.Int(row["OcrCaptureType"].ToString());
            retVal.Add(eClipboardImageCapture);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EClipboardImageCapture> listEClipboardImageCaptures, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EClipboardImageCapture";
        var table = new DataTable(tableName);
        table.Columns.Add("EClipboardImageCaptureNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DefNum");
        table.Columns.Add("IsSelfPortrait");
        table.Columns.Add("DateTimeUpserted");
        table.Columns.Add("DocNum");
        table.Columns.Add("OcrCaptureType");
        foreach (var eClipboardImageCapture in listEClipboardImageCaptures)
            table.Rows.Add(SOut.Long(eClipboardImageCapture.EClipboardImageCaptureNum), SOut.Long(eClipboardImageCapture.PatNum), SOut.Long(eClipboardImageCapture.DefNum), SOut.Bool(eClipboardImageCapture.IsSelfPortrait), SOut.DateT(eClipboardImageCapture.DateTimeUpserted, false), SOut.Long(eClipboardImageCapture.DocNum), SOut.Int((int) eClipboardImageCapture.OcrCaptureType));
        return table;
    }

    public static long Insert(EClipboardImageCapture eClipboardImageCapture)
    {
        return Insert(eClipboardImageCapture, false);
    }

    public static long Insert(EClipboardImageCapture eClipboardImageCapture, bool useExistingPK)
    {
        var command = "INSERT INTO eclipboardimagecapture (";

        command += "PatNum,DefNum,IsSelfPortrait,DateTimeUpserted,DocNum,OcrCaptureType) VALUES(";

        command +=
            SOut.Long(eClipboardImageCapture.PatNum) + ","
                                                     + SOut.Long(eClipboardImageCapture.DefNum) + ","
                                                     + SOut.Bool(eClipboardImageCapture.IsSelfPortrait) + ","
                                                     + SOut.DateT(eClipboardImageCapture.DateTimeUpserted) + ","
                                                     + SOut.Long(eClipboardImageCapture.DocNum) + ","
                                                     + SOut.Int((int) eClipboardImageCapture.OcrCaptureType) + ")";
        {
            eClipboardImageCapture.EClipboardImageCaptureNum = Db.NonQ(command, true, "EClipboardImageCaptureNum", "eClipboardImageCapture");
        }
        return eClipboardImageCapture.EClipboardImageCaptureNum;
    }

    public static long InsertNoCache(EClipboardImageCapture eClipboardImageCapture)
    {
        return InsertNoCache(eClipboardImageCapture, false);
    }

    public static long InsertNoCache(EClipboardImageCapture eClipboardImageCapture, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eclipboardimagecapture (";
        if (isRandomKeys || useExistingPK) command += "EClipboardImageCaptureNum,";
        command += "PatNum,DefNum,IsSelfPortrait,DateTimeUpserted,DocNum,OcrCaptureType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eClipboardImageCapture.EClipboardImageCaptureNum) + ",";
        command +=
            SOut.Long(eClipboardImageCapture.PatNum) + ","
                                                     + SOut.Long(eClipboardImageCapture.DefNum) + ","
                                                     + SOut.Bool(eClipboardImageCapture.IsSelfPortrait) + ","
                                                     + SOut.DateT(eClipboardImageCapture.DateTimeUpserted) + ","
                                                     + SOut.Long(eClipboardImageCapture.DocNum) + ","
                                                     + SOut.Int((int) eClipboardImageCapture.OcrCaptureType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eClipboardImageCapture.EClipboardImageCaptureNum = Db.NonQ(command, true, "EClipboardImageCaptureNum", "eClipboardImageCapture");
        return eClipboardImageCapture.EClipboardImageCaptureNum;
    }

    public static void Update(EClipboardImageCapture eClipboardImageCapture)
    {
        var command = "UPDATE eclipboardimagecapture SET "
                      + "PatNum                   =  " + SOut.Long(eClipboardImageCapture.PatNum) + ", "
                      + "DefNum                   =  " + SOut.Long(eClipboardImageCapture.DefNum) + ", "
                      + "IsSelfPortrait           =  " + SOut.Bool(eClipboardImageCapture.IsSelfPortrait) + ", "
                      + "DateTimeUpserted         =  " + SOut.DateT(eClipboardImageCapture.DateTimeUpserted) + ", "
                      + "DocNum                   =  " + SOut.Long(eClipboardImageCapture.DocNum) + ", "
                      + "OcrCaptureType           =  " + SOut.Int((int) eClipboardImageCapture.OcrCaptureType) + " "
                      + "WHERE EClipboardImageCaptureNum = " + SOut.Long(eClipboardImageCapture.EClipboardImageCaptureNum);
        Db.NonQ(command);
    }

    public static bool Update(EClipboardImageCapture eClipboardImageCapture, EClipboardImageCapture oldEClipboardImageCapture)
    {
        var command = "";
        if (eClipboardImageCapture.PatNum != oldEClipboardImageCapture.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(eClipboardImageCapture.PatNum) + "";
        }

        if (eClipboardImageCapture.DefNum != oldEClipboardImageCapture.DefNum)
        {
            if (command != "") command += ",";
            command += "DefNum = " + SOut.Long(eClipboardImageCapture.DefNum) + "";
        }

        if (eClipboardImageCapture.IsSelfPortrait != oldEClipboardImageCapture.IsSelfPortrait)
        {
            if (command != "") command += ",";
            command += "IsSelfPortrait = " + SOut.Bool(eClipboardImageCapture.IsSelfPortrait) + "";
        }

        if (eClipboardImageCapture.DateTimeUpserted != oldEClipboardImageCapture.DateTimeUpserted)
        {
            if (command != "") command += ",";
            command += "DateTimeUpserted = " + SOut.DateT(eClipboardImageCapture.DateTimeUpserted) + "";
        }

        if (eClipboardImageCapture.DocNum != oldEClipboardImageCapture.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(eClipboardImageCapture.DocNum) + "";
        }

        if (eClipboardImageCapture.OcrCaptureType != oldEClipboardImageCapture.OcrCaptureType)
        {
            if (command != "") command += ",";
            command += "OcrCaptureType = " + SOut.Int((int) eClipboardImageCapture.OcrCaptureType) + "";
        }

        if (command == "") return false;
        command = "UPDATE eclipboardimagecapture SET " + command
                                                       + " WHERE EClipboardImageCaptureNum = " + SOut.Long(eClipboardImageCapture.EClipboardImageCaptureNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EClipboardImageCapture eClipboardImageCapture, EClipboardImageCapture oldEClipboardImageCapture)
    {
        if (eClipboardImageCapture.PatNum != oldEClipboardImageCapture.PatNum) return true;
        if (eClipboardImageCapture.DefNum != oldEClipboardImageCapture.DefNum) return true;
        if (eClipboardImageCapture.IsSelfPortrait != oldEClipboardImageCapture.IsSelfPortrait) return true;
        if (eClipboardImageCapture.DateTimeUpserted != oldEClipboardImageCapture.DateTimeUpserted) return true;
        if (eClipboardImageCapture.DocNum != oldEClipboardImageCapture.DocNum) return true;
        if (eClipboardImageCapture.OcrCaptureType != oldEClipboardImageCapture.OcrCaptureType) return true;
        return false;
    }

    public static void Delete(long eClipboardImageCaptureNum)
    {
        var command = "DELETE FROM eclipboardimagecapture "
                      + "WHERE EClipboardImageCaptureNum = " + SOut.Long(eClipboardImageCaptureNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEClipboardImageCaptureNums)
    {
        if (listEClipboardImageCaptureNums == null || listEClipboardImageCaptureNums.Count == 0) return;
        var command = "DELETE FROM eclipboardimagecapture "
                      + "WHERE EClipboardImageCaptureNum IN(" + string.Join(",", listEClipboardImageCaptureNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}