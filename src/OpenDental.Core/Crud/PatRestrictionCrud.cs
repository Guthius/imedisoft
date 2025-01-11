#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatRestrictionCrud
{
    public static PatRestriction SelectOne(long patRestrictionNum)
    {
        var command = "SELECT * FROM patrestriction "
                      + "WHERE PatRestrictionNum = " + SOut.Long(patRestrictionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatRestriction SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatRestriction> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatRestriction> TableToList(DataTable table)
    {
        var retVal = new List<PatRestriction>();
        PatRestriction patRestriction;
        foreach (DataRow row in table.Rows)
        {
            patRestriction = new PatRestriction();
            patRestriction.PatRestrictionNum = SIn.Long(row["PatRestrictionNum"].ToString());
            patRestriction.PatNum = SIn.Long(row["PatNum"].ToString());
            patRestriction.PatRestrictType = (PatRestrict) SIn.Int(row["PatRestrictType"].ToString());
            retVal.Add(patRestriction);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatRestriction> listPatRestrictions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatRestriction";
        var table = new DataTable(tableName);
        table.Columns.Add("PatRestrictionNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("PatRestrictType");
        foreach (var patRestriction in listPatRestrictions)
            table.Rows.Add(SOut.Long(patRestriction.PatRestrictionNum), SOut.Long(patRestriction.PatNum), SOut.Int((int) patRestriction.PatRestrictType));
        return table;
    }

    public static long Insert(PatRestriction patRestriction)
    {
        return Insert(patRestriction, false);
    }

    public static long Insert(PatRestriction patRestriction, bool useExistingPK)
    {
        var command = "INSERT INTO patrestriction (";

        command += "PatNum,PatRestrictType) VALUES(";

        command +=
            SOut.Long(patRestriction.PatNum) + ","
                                             + SOut.Int((int) patRestriction.PatRestrictType) + ")";
        {
            patRestriction.PatRestrictionNum = Db.NonQ(command, true, "PatRestrictionNum", "patRestriction");
        }
        return patRestriction.PatRestrictionNum;
    }

    public static long InsertNoCache(PatRestriction patRestriction)
    {
        return InsertNoCache(patRestriction, false);
    }

    public static long InsertNoCache(PatRestriction patRestriction, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patrestriction (";
        if (isRandomKeys || useExistingPK) command += "PatRestrictionNum,";
        command += "PatNum,PatRestrictType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patRestriction.PatRestrictionNum) + ",";
        command +=
            SOut.Long(patRestriction.PatNum) + ","
                                             + SOut.Int((int) patRestriction.PatRestrictType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            patRestriction.PatRestrictionNum = Db.NonQ(command, true, "PatRestrictionNum", "patRestriction");
        return patRestriction.PatRestrictionNum;
    }

    public static void Update(PatRestriction patRestriction)
    {
        var command = "UPDATE patrestriction SET "
                      + "PatNum           =  " + SOut.Long(patRestriction.PatNum) + ", "
                      + "PatRestrictType  =  " + SOut.Int((int) patRestriction.PatRestrictType) + " "
                      + "WHERE PatRestrictionNum = " + SOut.Long(patRestriction.PatRestrictionNum);
        Db.NonQ(command);
    }

    public static bool Update(PatRestriction patRestriction, PatRestriction oldPatRestriction)
    {
        var command = "";
        if (patRestriction.PatNum != oldPatRestriction.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(patRestriction.PatNum) + "";
        }

        if (patRestriction.PatRestrictType != oldPatRestriction.PatRestrictType)
        {
            if (command != "") command += ",";
            command += "PatRestrictType = " + SOut.Int((int) patRestriction.PatRestrictType) + "";
        }

        if (command == "") return false;
        command = "UPDATE patrestriction SET " + command
                                               + " WHERE PatRestrictionNum = " + SOut.Long(patRestriction.PatRestrictionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PatRestriction patRestriction, PatRestriction oldPatRestriction)
    {
        if (patRestriction.PatNum != oldPatRestriction.PatNum) return true;
        if (patRestriction.PatRestrictType != oldPatRestriction.PatRestrictType) return true;
        return false;
    }

    public static void Delete(long patRestrictionNum)
    {
        var command = "DELETE FROM patrestriction "
                      + "WHERE PatRestrictionNum = " + SOut.Long(patRestrictionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatRestrictionNums)
    {
        if (listPatRestrictionNums == null || listPatRestrictionNums.Count == 0) return;
        var command = "DELETE FROM patrestriction "
                      + "WHERE PatRestrictionNum IN(" + string.Join(",", listPatRestrictionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}