#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabSpecimenConditionCrud
{
    public static EhrLabSpecimenCondition SelectOne(long ehrLabSpecimenConditionNum)
    {
        var command = "SELECT * FROM ehrlabspecimencondition "
                      + "WHERE EhrLabSpecimenConditionNum = " + SOut.Long(ehrLabSpecimenConditionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabSpecimenCondition SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabSpecimenCondition> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabSpecimenCondition> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabSpecimenCondition>();
        EhrLabSpecimenCondition ehrLabSpecimenCondition;
        foreach (DataRow row in table.Rows)
        {
            ehrLabSpecimenCondition = new EhrLabSpecimenCondition();
            ehrLabSpecimenCondition.EhrLabSpecimenConditionNum = SIn.Long(row["EhrLabSpecimenConditionNum"].ToString());
            ehrLabSpecimenCondition.EhrLabSpecimenNum = SIn.Long(row["EhrLabSpecimenNum"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionID = SIn.String(row["SpecimenConditionID"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionText = SIn.String(row["SpecimenConditionText"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionCodeSystemName = SIn.String(row["SpecimenConditionCodeSystemName"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionIDAlt = SIn.String(row["SpecimenConditionIDAlt"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionTextAlt = SIn.String(row["SpecimenConditionTextAlt"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt = SIn.String(row["SpecimenConditionCodeSystemNameAlt"].ToString());
            ehrLabSpecimenCondition.SpecimenConditionTextOriginal = SIn.String(row["SpecimenConditionTextOriginal"].ToString());
            retVal.Add(ehrLabSpecimenCondition);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabSpecimenCondition> listEhrLabSpecimenConditions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabSpecimenCondition";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabSpecimenConditionNum");
        table.Columns.Add("EhrLabSpecimenNum");
        table.Columns.Add("SpecimenConditionID");
        table.Columns.Add("SpecimenConditionText");
        table.Columns.Add("SpecimenConditionCodeSystemName");
        table.Columns.Add("SpecimenConditionIDAlt");
        table.Columns.Add("SpecimenConditionTextAlt");
        table.Columns.Add("SpecimenConditionCodeSystemNameAlt");
        table.Columns.Add("SpecimenConditionTextOriginal");
        foreach (var ehrLabSpecimenCondition in listEhrLabSpecimenConditions)
            table.Rows.Add(SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenConditionNum), SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenNum), ehrLabSpecimenCondition.SpecimenConditionID, ehrLabSpecimenCondition.SpecimenConditionText, ehrLabSpecimenCondition.SpecimenConditionCodeSystemName, ehrLabSpecimenCondition.SpecimenConditionIDAlt, ehrLabSpecimenCondition.SpecimenConditionTextAlt, ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt, ehrLabSpecimenCondition.SpecimenConditionTextOriginal);
        return table;
    }

    public static long Insert(EhrLabSpecimenCondition ehrLabSpecimenCondition)
    {
        return Insert(ehrLabSpecimenCondition, false);
    }

    public static long Insert(EhrLabSpecimenCondition ehrLabSpecimenCondition, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabspecimencondition (";

        command += "EhrLabSpecimenNum,SpecimenConditionID,SpecimenConditionText,SpecimenConditionCodeSystemName,SpecimenConditionIDAlt,SpecimenConditionTextAlt,SpecimenConditionCodeSystemNameAlt,SpecimenConditionTextOriginal) VALUES(";

        command +=
            SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenNum) + ","
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionID) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionText) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemName) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionIDAlt) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextAlt) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextOriginal) + "')";
        {
            ehrLabSpecimenCondition.EhrLabSpecimenConditionNum = Db.NonQ(command, true, "EhrLabSpecimenConditionNum", "ehrLabSpecimenCondition");
        }
        return ehrLabSpecimenCondition.EhrLabSpecimenConditionNum;
    }

    public static long InsertNoCache(EhrLabSpecimenCondition ehrLabSpecimenCondition)
    {
        return InsertNoCache(ehrLabSpecimenCondition, false);
    }

    public static long InsertNoCache(EhrLabSpecimenCondition ehrLabSpecimenCondition, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabspecimencondition (";
        if (isRandomKeys || useExistingPK) command += "EhrLabSpecimenConditionNum,";
        command += "EhrLabSpecimenNum,SpecimenConditionID,SpecimenConditionText,SpecimenConditionCodeSystemName,SpecimenConditionIDAlt,SpecimenConditionTextAlt,SpecimenConditionCodeSystemNameAlt,SpecimenConditionTextOriginal) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenConditionNum) + ",";
        command +=
            SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenNum) + ","
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionID) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionText) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemName) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionIDAlt) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextAlt) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt) + "',"
                                                                 + "'" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextOriginal) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabSpecimenCondition.EhrLabSpecimenConditionNum = Db.NonQ(command, true, "EhrLabSpecimenConditionNum", "ehrLabSpecimenCondition");
        return ehrLabSpecimenCondition.EhrLabSpecimenConditionNum;
    }

    public static void Update(EhrLabSpecimenCondition ehrLabSpecimenCondition)
    {
        var command = "UPDATE ehrlabspecimencondition SET "
                      + "EhrLabSpecimenNum                 =  " + SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenNum) + ", "
                      + "SpecimenConditionID               = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionID) + "', "
                      + "SpecimenConditionText             = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionText) + "', "
                      + "SpecimenConditionCodeSystemName   = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemName) + "', "
                      + "SpecimenConditionIDAlt            = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionIDAlt) + "', "
                      + "SpecimenConditionTextAlt          = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextAlt) + "', "
                      + "SpecimenConditionCodeSystemNameAlt= '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt) + "', "
                      + "SpecimenConditionTextOriginal     = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextOriginal) + "' "
                      + "WHERE EhrLabSpecimenConditionNum = " + SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenConditionNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabSpecimenCondition ehrLabSpecimenCondition, EhrLabSpecimenCondition oldEhrLabSpecimenCondition)
    {
        var command = "";
        if (ehrLabSpecimenCondition.EhrLabSpecimenNum != oldEhrLabSpecimenCondition.EhrLabSpecimenNum)
        {
            if (command != "") command += ",";
            command += "EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenNum) + "";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionID != oldEhrLabSpecimenCondition.SpecimenConditionID)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionID = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionID) + "'";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionText != oldEhrLabSpecimenCondition.SpecimenConditionText)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionText = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionText) + "'";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionCodeSystemName != oldEhrLabSpecimenCondition.SpecimenConditionCodeSystemName)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionCodeSystemName = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemName) + "'";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionIDAlt != oldEhrLabSpecimenCondition.SpecimenConditionIDAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionIDAlt = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionIDAlt) + "'";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionTextAlt != oldEhrLabSpecimenCondition.SpecimenConditionTextAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionTextAlt = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextAlt) + "'";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt != oldEhrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionCodeSystemNameAlt = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt) + "'";
        }

        if (ehrLabSpecimenCondition.SpecimenConditionTextOriginal != oldEhrLabSpecimenCondition.SpecimenConditionTextOriginal)
        {
            if (command != "") command += ",";
            command += "SpecimenConditionTextOriginal = '" + SOut.String(ehrLabSpecimenCondition.SpecimenConditionTextOriginal) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabspecimencondition SET " + command
                                                        + " WHERE EhrLabSpecimenConditionNum = " + SOut.Long(ehrLabSpecimenCondition.EhrLabSpecimenConditionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabSpecimenCondition ehrLabSpecimenCondition, EhrLabSpecimenCondition oldEhrLabSpecimenCondition)
    {
        if (ehrLabSpecimenCondition.EhrLabSpecimenNum != oldEhrLabSpecimenCondition.EhrLabSpecimenNum) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionID != oldEhrLabSpecimenCondition.SpecimenConditionID) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionText != oldEhrLabSpecimenCondition.SpecimenConditionText) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionCodeSystemName != oldEhrLabSpecimenCondition.SpecimenConditionCodeSystemName) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionIDAlt != oldEhrLabSpecimenCondition.SpecimenConditionIDAlt) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionTextAlt != oldEhrLabSpecimenCondition.SpecimenConditionTextAlt) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt != oldEhrLabSpecimenCondition.SpecimenConditionCodeSystemNameAlt) return true;
        if (ehrLabSpecimenCondition.SpecimenConditionTextOriginal != oldEhrLabSpecimenCondition.SpecimenConditionTextOriginal) return true;
        return false;
    }

    public static void Delete(long ehrLabSpecimenConditionNum)
    {
        var command = "DELETE FROM ehrlabspecimencondition "
                      + "WHERE EhrLabSpecimenConditionNum = " + SOut.Long(ehrLabSpecimenConditionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabSpecimenConditionNums)
    {
        if (listEhrLabSpecimenConditionNums == null || listEhrLabSpecimenConditionNums.Count == 0) return;
        var command = "DELETE FROM ehrlabspecimencondition "
                      + "WHERE EhrLabSpecimenConditionNum IN(" + string.Join(",", listEhrLabSpecimenConditionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}