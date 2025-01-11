#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabClinicalInfoCrud
{
    public static EhrLabClinicalInfo SelectOne(long ehrLabClinicalInfoNum)
    {
        var command = "SELECT * FROM ehrlabclinicalinfo "
                      + "WHERE EhrLabClinicalInfoNum = " + SOut.Long(ehrLabClinicalInfoNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabClinicalInfo SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabClinicalInfo> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabClinicalInfo> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabClinicalInfo>();
        EhrLabClinicalInfo ehrLabClinicalInfo;
        foreach (DataRow row in table.Rows)
        {
            ehrLabClinicalInfo = new EhrLabClinicalInfo();
            ehrLabClinicalInfo.EhrLabClinicalInfoNum = SIn.Long(row["EhrLabClinicalInfoNum"].ToString());
            ehrLabClinicalInfo.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLabClinicalInfo.ClinicalInfoID = SIn.String(row["ClinicalInfoID"].ToString());
            ehrLabClinicalInfo.ClinicalInfoText = SIn.String(row["ClinicalInfoText"].ToString());
            ehrLabClinicalInfo.ClinicalInfoCodeSystemName = SIn.String(row["ClinicalInfoCodeSystemName"].ToString());
            ehrLabClinicalInfo.ClinicalInfoIDAlt = SIn.String(row["ClinicalInfoIDAlt"].ToString());
            ehrLabClinicalInfo.ClinicalInfoTextAlt = SIn.String(row["ClinicalInfoTextAlt"].ToString());
            ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt = SIn.String(row["ClinicalInfoCodeSystemNameAlt"].ToString());
            ehrLabClinicalInfo.ClinicalInfoTextOriginal = SIn.String(row["ClinicalInfoTextOriginal"].ToString());
            retVal.Add(ehrLabClinicalInfo);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabClinicalInfo> listEhrLabClinicalInfos, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabClinicalInfo";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabClinicalInfoNum");
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("ClinicalInfoID");
        table.Columns.Add("ClinicalInfoText");
        table.Columns.Add("ClinicalInfoCodeSystemName");
        table.Columns.Add("ClinicalInfoIDAlt");
        table.Columns.Add("ClinicalInfoTextAlt");
        table.Columns.Add("ClinicalInfoCodeSystemNameAlt");
        table.Columns.Add("ClinicalInfoTextOriginal");
        foreach (var ehrLabClinicalInfo in listEhrLabClinicalInfos)
            table.Rows.Add(SOut.Long(ehrLabClinicalInfo.EhrLabClinicalInfoNum), SOut.Long(ehrLabClinicalInfo.EhrLabNum), ehrLabClinicalInfo.ClinicalInfoID, ehrLabClinicalInfo.ClinicalInfoText, ehrLabClinicalInfo.ClinicalInfoCodeSystemName, ehrLabClinicalInfo.ClinicalInfoIDAlt, ehrLabClinicalInfo.ClinicalInfoTextAlt, ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt, ehrLabClinicalInfo.ClinicalInfoTextOriginal);
        return table;
    }

    public static long Insert(EhrLabClinicalInfo ehrLabClinicalInfo)
    {
        return Insert(ehrLabClinicalInfo, false);
    }

    public static long Insert(EhrLabClinicalInfo ehrLabClinicalInfo, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabclinicalinfo (";

        command += "EhrLabNum,ClinicalInfoID,ClinicalInfoText,ClinicalInfoCodeSystemName,ClinicalInfoIDAlt,ClinicalInfoTextAlt,ClinicalInfoCodeSystemNameAlt,ClinicalInfoTextOriginal) VALUES(";

        command +=
            SOut.Long(ehrLabClinicalInfo.EhrLabNum) + ","
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoID) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoText) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemName) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoIDAlt) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextAlt) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextOriginal) + "')";
        {
            ehrLabClinicalInfo.EhrLabClinicalInfoNum = Db.NonQ(command, true, "EhrLabClinicalInfoNum", "ehrLabClinicalInfo");
        }
        return ehrLabClinicalInfo.EhrLabClinicalInfoNum;
    }

    public static long InsertNoCache(EhrLabClinicalInfo ehrLabClinicalInfo)
    {
        return InsertNoCache(ehrLabClinicalInfo, false);
    }

    public static long InsertNoCache(EhrLabClinicalInfo ehrLabClinicalInfo, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabclinicalinfo (";
        if (isRandomKeys || useExistingPK) command += "EhrLabClinicalInfoNum,";
        command += "EhrLabNum,ClinicalInfoID,ClinicalInfoText,ClinicalInfoCodeSystemName,ClinicalInfoIDAlt,ClinicalInfoTextAlt,ClinicalInfoCodeSystemNameAlt,ClinicalInfoTextOriginal) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabClinicalInfo.EhrLabClinicalInfoNum) + ",";
        command +=
            SOut.Long(ehrLabClinicalInfo.EhrLabNum) + ","
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoID) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoText) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemName) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoIDAlt) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextAlt) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt) + "',"
                                                    + "'" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextOriginal) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabClinicalInfo.EhrLabClinicalInfoNum = Db.NonQ(command, true, "EhrLabClinicalInfoNum", "ehrLabClinicalInfo");
        return ehrLabClinicalInfo.EhrLabClinicalInfoNum;
    }

    public static void Update(EhrLabClinicalInfo ehrLabClinicalInfo)
    {
        var command = "UPDATE ehrlabclinicalinfo SET "
                      + "EhrLabNum                    =  " + SOut.Long(ehrLabClinicalInfo.EhrLabNum) + ", "
                      + "ClinicalInfoID               = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoID) + "', "
                      + "ClinicalInfoText             = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoText) + "', "
                      + "ClinicalInfoCodeSystemName   = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemName) + "', "
                      + "ClinicalInfoIDAlt            = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoIDAlt) + "', "
                      + "ClinicalInfoTextAlt          = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextAlt) + "', "
                      + "ClinicalInfoCodeSystemNameAlt= '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt) + "', "
                      + "ClinicalInfoTextOriginal     = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextOriginal) + "' "
                      + "WHERE EhrLabClinicalInfoNum = " + SOut.Long(ehrLabClinicalInfo.EhrLabClinicalInfoNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabClinicalInfo ehrLabClinicalInfo, EhrLabClinicalInfo oldEhrLabClinicalInfo)
    {
        var command = "";
        if (ehrLabClinicalInfo.EhrLabNum != oldEhrLabClinicalInfo.EhrLabNum)
        {
            if (command != "") command += ",";
            command += "EhrLabNum = " + SOut.Long(ehrLabClinicalInfo.EhrLabNum) + "";
        }

        if (ehrLabClinicalInfo.ClinicalInfoID != oldEhrLabClinicalInfo.ClinicalInfoID)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoID = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoID) + "'";
        }

        if (ehrLabClinicalInfo.ClinicalInfoText != oldEhrLabClinicalInfo.ClinicalInfoText)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoText = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoText) + "'";
        }

        if (ehrLabClinicalInfo.ClinicalInfoCodeSystemName != oldEhrLabClinicalInfo.ClinicalInfoCodeSystemName)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoCodeSystemName = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemName) + "'";
        }

        if (ehrLabClinicalInfo.ClinicalInfoIDAlt != oldEhrLabClinicalInfo.ClinicalInfoIDAlt)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoIDAlt = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoIDAlt) + "'";
        }

        if (ehrLabClinicalInfo.ClinicalInfoTextAlt != oldEhrLabClinicalInfo.ClinicalInfoTextAlt)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoTextAlt = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextAlt) + "'";
        }

        if (ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt != oldEhrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoCodeSystemNameAlt = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt) + "'";
        }

        if (ehrLabClinicalInfo.ClinicalInfoTextOriginal != oldEhrLabClinicalInfo.ClinicalInfoTextOriginal)
        {
            if (command != "") command += ",";
            command += "ClinicalInfoTextOriginal = '" + SOut.String(ehrLabClinicalInfo.ClinicalInfoTextOriginal) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabclinicalinfo SET " + command
                                                   + " WHERE EhrLabClinicalInfoNum = " + SOut.Long(ehrLabClinicalInfo.EhrLabClinicalInfoNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabClinicalInfo ehrLabClinicalInfo, EhrLabClinicalInfo oldEhrLabClinicalInfo)
    {
        if (ehrLabClinicalInfo.EhrLabNum != oldEhrLabClinicalInfo.EhrLabNum) return true;
        if (ehrLabClinicalInfo.ClinicalInfoID != oldEhrLabClinicalInfo.ClinicalInfoID) return true;
        if (ehrLabClinicalInfo.ClinicalInfoText != oldEhrLabClinicalInfo.ClinicalInfoText) return true;
        if (ehrLabClinicalInfo.ClinicalInfoCodeSystemName != oldEhrLabClinicalInfo.ClinicalInfoCodeSystemName) return true;
        if (ehrLabClinicalInfo.ClinicalInfoIDAlt != oldEhrLabClinicalInfo.ClinicalInfoIDAlt) return true;
        if (ehrLabClinicalInfo.ClinicalInfoTextAlt != oldEhrLabClinicalInfo.ClinicalInfoTextAlt) return true;
        if (ehrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt != oldEhrLabClinicalInfo.ClinicalInfoCodeSystemNameAlt) return true;
        if (ehrLabClinicalInfo.ClinicalInfoTextOriginal != oldEhrLabClinicalInfo.ClinicalInfoTextOriginal) return true;
        return false;
    }

    public static void Delete(long ehrLabClinicalInfoNum)
    {
        var command = "DELETE FROM ehrlabclinicalinfo "
                      + "WHERE EhrLabClinicalInfoNum = " + SOut.Long(ehrLabClinicalInfoNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabClinicalInfoNums)
    {
        if (listEhrLabClinicalInfoNums == null || listEhrLabClinicalInfoNums.Count == 0) return;
        var command = "DELETE FROM ehrlabclinicalinfo "
                      + "WHERE EhrLabClinicalInfoNum IN(" + string.Join(",", listEhrLabClinicalInfoNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}