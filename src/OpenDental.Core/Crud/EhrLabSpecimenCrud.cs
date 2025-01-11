#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabSpecimenCrud
{
    public static EhrLabSpecimen SelectOne(long ehrLabSpecimenNum)
    {
        var command = "SELECT * FROM ehrlabspecimen "
                      + "WHERE EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimenNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabSpecimen SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabSpecimen> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabSpecimen> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabSpecimen>();
        EhrLabSpecimen ehrLabSpecimen;
        foreach (DataRow row in table.Rows)
        {
            ehrLabSpecimen = new EhrLabSpecimen();
            ehrLabSpecimen.EhrLabSpecimenNum = SIn.Long(row["EhrLabSpecimenNum"].ToString());
            ehrLabSpecimen.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLabSpecimen.SetIdSPM = SIn.Long(row["SetIdSPM"].ToString());
            ehrLabSpecimen.SpecimenTypeID = SIn.String(row["SpecimenTypeID"].ToString());
            ehrLabSpecimen.SpecimenTypeText = SIn.String(row["SpecimenTypeText"].ToString());
            ehrLabSpecimen.SpecimenTypeCodeSystemName = SIn.String(row["SpecimenTypeCodeSystemName"].ToString());
            ehrLabSpecimen.SpecimenTypeIDAlt = SIn.String(row["SpecimenTypeIDAlt"].ToString());
            ehrLabSpecimen.SpecimenTypeTextAlt = SIn.String(row["SpecimenTypeTextAlt"].ToString());
            ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt = SIn.String(row["SpecimenTypeCodeSystemNameAlt"].ToString());
            ehrLabSpecimen.SpecimenTypeTextOriginal = SIn.String(row["SpecimenTypeTextOriginal"].ToString());
            ehrLabSpecimen.CollectionDateTimeStart = SIn.String(row["CollectionDateTimeStart"].ToString());
            ehrLabSpecimen.CollectionDateTimeEnd = SIn.String(row["CollectionDateTimeEnd"].ToString());
            retVal.Add(ehrLabSpecimen);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabSpecimen> listEhrLabSpecimens, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabSpecimen";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabSpecimenNum");
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("SetIdSPM");
        table.Columns.Add("SpecimenTypeID");
        table.Columns.Add("SpecimenTypeText");
        table.Columns.Add("SpecimenTypeCodeSystemName");
        table.Columns.Add("SpecimenTypeIDAlt");
        table.Columns.Add("SpecimenTypeTextAlt");
        table.Columns.Add("SpecimenTypeCodeSystemNameAlt");
        table.Columns.Add("SpecimenTypeTextOriginal");
        table.Columns.Add("CollectionDateTimeStart");
        table.Columns.Add("CollectionDateTimeEnd");
        foreach (var ehrLabSpecimen in listEhrLabSpecimens)
            table.Rows.Add(SOut.Long(ehrLabSpecimen.EhrLabSpecimenNum), SOut.Long(ehrLabSpecimen.EhrLabNum), SOut.Long(ehrLabSpecimen.SetIdSPM), ehrLabSpecimen.SpecimenTypeID, ehrLabSpecimen.SpecimenTypeText, ehrLabSpecimen.SpecimenTypeCodeSystemName, ehrLabSpecimen.SpecimenTypeIDAlt, ehrLabSpecimen.SpecimenTypeTextAlt, ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt, ehrLabSpecimen.SpecimenTypeTextOriginal, ehrLabSpecimen.CollectionDateTimeStart, ehrLabSpecimen.CollectionDateTimeEnd);
        return table;
    }

    public static long Insert(EhrLabSpecimen ehrLabSpecimen)
    {
        return Insert(ehrLabSpecimen, false);
    }

    public static long Insert(EhrLabSpecimen ehrLabSpecimen, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabspecimen (";

        command += "EhrLabNum,SetIdSPM,SpecimenTypeID,SpecimenTypeText,SpecimenTypeCodeSystemName,SpecimenTypeIDAlt,SpecimenTypeTextAlt,SpecimenTypeCodeSystemNameAlt,SpecimenTypeTextOriginal,CollectionDateTimeStart,CollectionDateTimeEnd) VALUES(";

        command +=
            SOut.Long(ehrLabSpecimen.EhrLabNum) + ","
                                                + SOut.Long(ehrLabSpecimen.SetIdSPM) + ","
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeID) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeText) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemName) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeIDAlt) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeTextAlt) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeTextOriginal) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.CollectionDateTimeStart) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.CollectionDateTimeEnd) + "')";
        {
            ehrLabSpecimen.EhrLabSpecimenNum = Db.NonQ(command, true, "EhrLabSpecimenNum", "ehrLabSpecimen");
        }
        return ehrLabSpecimen.EhrLabSpecimenNum;
    }

    public static long InsertNoCache(EhrLabSpecimen ehrLabSpecimen)
    {
        return InsertNoCache(ehrLabSpecimen, false);
    }

    public static long InsertNoCache(EhrLabSpecimen ehrLabSpecimen, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabspecimen (";
        if (isRandomKeys || useExistingPK) command += "EhrLabSpecimenNum,";
        command += "EhrLabNum,SetIdSPM,SpecimenTypeID,SpecimenTypeText,SpecimenTypeCodeSystemName,SpecimenTypeIDAlt,SpecimenTypeTextAlt,SpecimenTypeCodeSystemNameAlt,SpecimenTypeTextOriginal,CollectionDateTimeStart,CollectionDateTimeEnd) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabSpecimen.EhrLabSpecimenNum) + ",";
        command +=
            SOut.Long(ehrLabSpecimen.EhrLabNum) + ","
                                                + SOut.Long(ehrLabSpecimen.SetIdSPM) + ","
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeID) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeText) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemName) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeIDAlt) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeTextAlt) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.SpecimenTypeTextOriginal) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.CollectionDateTimeStart) + "',"
                                                + "'" + SOut.String(ehrLabSpecimen.CollectionDateTimeEnd) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabSpecimen.EhrLabSpecimenNum = Db.NonQ(command, true, "EhrLabSpecimenNum", "ehrLabSpecimen");
        return ehrLabSpecimen.EhrLabSpecimenNum;
    }

    public static void Update(EhrLabSpecimen ehrLabSpecimen)
    {
        var command = "UPDATE ehrlabspecimen SET "
                      + "EhrLabNum                    =  " + SOut.Long(ehrLabSpecimen.EhrLabNum) + ", "
                      + "SetIdSPM                     =  " + SOut.Long(ehrLabSpecimen.SetIdSPM) + ", "
                      + "SpecimenTypeID               = '" + SOut.String(ehrLabSpecimen.SpecimenTypeID) + "', "
                      + "SpecimenTypeText             = '" + SOut.String(ehrLabSpecimen.SpecimenTypeText) + "', "
                      + "SpecimenTypeCodeSystemName   = '" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemName) + "', "
                      + "SpecimenTypeIDAlt            = '" + SOut.String(ehrLabSpecimen.SpecimenTypeIDAlt) + "', "
                      + "SpecimenTypeTextAlt          = '" + SOut.String(ehrLabSpecimen.SpecimenTypeTextAlt) + "', "
                      + "SpecimenTypeCodeSystemNameAlt= '" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt) + "', "
                      + "SpecimenTypeTextOriginal     = '" + SOut.String(ehrLabSpecimen.SpecimenTypeTextOriginal) + "', "
                      + "CollectionDateTimeStart      = '" + SOut.String(ehrLabSpecimen.CollectionDateTimeStart) + "', "
                      + "CollectionDateTimeEnd        = '" + SOut.String(ehrLabSpecimen.CollectionDateTimeEnd) + "' "
                      + "WHERE EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimen.EhrLabSpecimenNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabSpecimen ehrLabSpecimen, EhrLabSpecimen oldEhrLabSpecimen)
    {
        var command = "";
        if (ehrLabSpecimen.EhrLabNum != oldEhrLabSpecimen.EhrLabNum)
        {
            if (command != "") command += ",";
            command += "EhrLabNum = " + SOut.Long(ehrLabSpecimen.EhrLabNum) + "";
        }

        if (ehrLabSpecimen.SetIdSPM != oldEhrLabSpecimen.SetIdSPM)
        {
            if (command != "") command += ",";
            command += "SetIdSPM = " + SOut.Long(ehrLabSpecimen.SetIdSPM) + "";
        }

        if (ehrLabSpecimen.SpecimenTypeID != oldEhrLabSpecimen.SpecimenTypeID)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeID = '" + SOut.String(ehrLabSpecimen.SpecimenTypeID) + "'";
        }

        if (ehrLabSpecimen.SpecimenTypeText != oldEhrLabSpecimen.SpecimenTypeText)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeText = '" + SOut.String(ehrLabSpecimen.SpecimenTypeText) + "'";
        }

        if (ehrLabSpecimen.SpecimenTypeCodeSystemName != oldEhrLabSpecimen.SpecimenTypeCodeSystemName)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeCodeSystemName = '" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemName) + "'";
        }

        if (ehrLabSpecimen.SpecimenTypeIDAlt != oldEhrLabSpecimen.SpecimenTypeIDAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeIDAlt = '" + SOut.String(ehrLabSpecimen.SpecimenTypeIDAlt) + "'";
        }

        if (ehrLabSpecimen.SpecimenTypeTextAlt != oldEhrLabSpecimen.SpecimenTypeTextAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeTextAlt = '" + SOut.String(ehrLabSpecimen.SpecimenTypeTextAlt) + "'";
        }

        if (ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt != oldEhrLabSpecimen.SpecimenTypeCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeCodeSystemNameAlt = '" + SOut.String(ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt) + "'";
        }

        if (ehrLabSpecimen.SpecimenTypeTextOriginal != oldEhrLabSpecimen.SpecimenTypeTextOriginal)
        {
            if (command != "") command += ",";
            command += "SpecimenTypeTextOriginal = '" + SOut.String(ehrLabSpecimen.SpecimenTypeTextOriginal) + "'";
        }

        if (ehrLabSpecimen.CollectionDateTimeStart != oldEhrLabSpecimen.CollectionDateTimeStart)
        {
            if (command != "") command += ",";
            command += "CollectionDateTimeStart = '" + SOut.String(ehrLabSpecimen.CollectionDateTimeStart) + "'";
        }

        if (ehrLabSpecimen.CollectionDateTimeEnd != oldEhrLabSpecimen.CollectionDateTimeEnd)
        {
            if (command != "") command += ",";
            command += "CollectionDateTimeEnd = '" + SOut.String(ehrLabSpecimen.CollectionDateTimeEnd) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabspecimen SET " + command
                                               + " WHERE EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimen.EhrLabSpecimenNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabSpecimen ehrLabSpecimen, EhrLabSpecimen oldEhrLabSpecimen)
    {
        if (ehrLabSpecimen.EhrLabNum != oldEhrLabSpecimen.EhrLabNum) return true;
        if (ehrLabSpecimen.SetIdSPM != oldEhrLabSpecimen.SetIdSPM) return true;
        if (ehrLabSpecimen.SpecimenTypeID != oldEhrLabSpecimen.SpecimenTypeID) return true;
        if (ehrLabSpecimen.SpecimenTypeText != oldEhrLabSpecimen.SpecimenTypeText) return true;
        if (ehrLabSpecimen.SpecimenTypeCodeSystemName != oldEhrLabSpecimen.SpecimenTypeCodeSystemName) return true;
        if (ehrLabSpecimen.SpecimenTypeIDAlt != oldEhrLabSpecimen.SpecimenTypeIDAlt) return true;
        if (ehrLabSpecimen.SpecimenTypeTextAlt != oldEhrLabSpecimen.SpecimenTypeTextAlt) return true;
        if (ehrLabSpecimen.SpecimenTypeCodeSystemNameAlt != oldEhrLabSpecimen.SpecimenTypeCodeSystemNameAlt) return true;
        if (ehrLabSpecimen.SpecimenTypeTextOriginal != oldEhrLabSpecimen.SpecimenTypeTextOriginal) return true;
        if (ehrLabSpecimen.CollectionDateTimeStart != oldEhrLabSpecimen.CollectionDateTimeStart) return true;
        if (ehrLabSpecimen.CollectionDateTimeEnd != oldEhrLabSpecimen.CollectionDateTimeEnd) return true;
        return false;
    }

    public static void Delete(long ehrLabSpecimenNum)
    {
        var command = "DELETE FROM ehrlabspecimen "
                      + "WHERE EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimenNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabSpecimenNums)
    {
        if (listEhrLabSpecimenNums == null || listEhrLabSpecimenNums.Count == 0) return;
        var command = "DELETE FROM ehrlabspecimen "
                      + "WHERE EhrLabSpecimenNum IN(" + string.Join(",", listEhrLabSpecimenNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}