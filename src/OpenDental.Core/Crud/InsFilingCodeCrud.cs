#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class InsFilingCodeCrud
{
    public static InsFilingCode SelectOne(long insFilingCodeNum)
    {
        var command = "SELECT * FROM insfilingcode "
                      + "WHERE InsFilingCodeNum = " + SOut.Long(insFilingCodeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static InsFilingCode SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsFilingCode> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsFilingCode> TableToList(DataTable table)
    {
        var retVal = new List<InsFilingCode>();
        InsFilingCode insFilingCode;
        foreach (DataRow row in table.Rows)
        {
            insFilingCode = new InsFilingCode();
            insFilingCode.InsFilingCodeNum = SIn.Long(row["InsFilingCodeNum"].ToString());
            insFilingCode.Descript = SIn.String(row["Descript"].ToString());
            insFilingCode.EclaimCode = SIn.String(row["EclaimCode"].ToString());
            insFilingCode.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            insFilingCode.GroupType = SIn.Long(row["GroupType"].ToString());
            insFilingCode.ExcludeOtherCoverageOnPriClaims = SIn.Bool(row["ExcludeOtherCoverageOnPriClaims"].ToString());
            retVal.Add(insFilingCode);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<InsFilingCode> listInsFilingCodes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "InsFilingCode";
        var table = new DataTable(tableName);
        table.Columns.Add("InsFilingCodeNum");
        table.Columns.Add("Descript");
        table.Columns.Add("EclaimCode");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("GroupType");
        table.Columns.Add("ExcludeOtherCoverageOnPriClaims");
        foreach (var insFilingCode in listInsFilingCodes)
            table.Rows.Add(SOut.Long(insFilingCode.InsFilingCodeNum), insFilingCode.Descript, insFilingCode.EclaimCode, SOut.Int(insFilingCode.ItemOrder), SOut.Long(insFilingCode.GroupType), SOut.Bool(insFilingCode.ExcludeOtherCoverageOnPriClaims));
        return table;
    }

    public static long Insert(InsFilingCode insFilingCode)
    {
        return Insert(insFilingCode, false);
    }

    public static long Insert(InsFilingCode insFilingCode, bool useExistingPK)
    {
        var command = "INSERT INTO insfilingcode (";

        command += "Descript,EclaimCode,ItemOrder,GroupType,ExcludeOtherCoverageOnPriClaims) VALUES(";

        command +=
            "'" + SOut.String(insFilingCode.Descript) + "',"
            + "'" + SOut.String(insFilingCode.EclaimCode) + "',"
            + SOut.Int(insFilingCode.ItemOrder) + ","
            + SOut.Long(insFilingCode.GroupType) + ","
            + SOut.Bool(insFilingCode.ExcludeOtherCoverageOnPriClaims) + ")";
        {
            insFilingCode.InsFilingCodeNum = Db.NonQ(command, true, "InsFilingCodeNum", "insFilingCode");
        }
        return insFilingCode.InsFilingCodeNum;
    }

    public static long InsertNoCache(InsFilingCode insFilingCode)
    {
        return InsertNoCache(insFilingCode, false);
    }

    public static long InsertNoCache(InsFilingCode insFilingCode, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO insfilingcode (";
        if (isRandomKeys || useExistingPK) command += "InsFilingCodeNum,";
        command += "Descript,EclaimCode,ItemOrder,GroupType,ExcludeOtherCoverageOnPriClaims) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(insFilingCode.InsFilingCodeNum) + ",";
        command +=
            "'" + SOut.String(insFilingCode.Descript) + "',"
            + "'" + SOut.String(insFilingCode.EclaimCode) + "',"
            + SOut.Int(insFilingCode.ItemOrder) + ","
            + SOut.Long(insFilingCode.GroupType) + ","
            + SOut.Bool(insFilingCode.ExcludeOtherCoverageOnPriClaims) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            insFilingCode.InsFilingCodeNum = Db.NonQ(command, true, "InsFilingCodeNum", "insFilingCode");
        return insFilingCode.InsFilingCodeNum;
    }

    public static void Update(InsFilingCode insFilingCode)
    {
        var command = "UPDATE insfilingcode SET "
                      + "Descript                       = '" + SOut.String(insFilingCode.Descript) + "', "
                      + "EclaimCode                     = '" + SOut.String(insFilingCode.EclaimCode) + "', "
                      + "ItemOrder                      =  " + SOut.Int(insFilingCode.ItemOrder) + ", "
                      + "GroupType                      =  " + SOut.Long(insFilingCode.GroupType) + ", "
                      + "ExcludeOtherCoverageOnPriClaims=  " + SOut.Bool(insFilingCode.ExcludeOtherCoverageOnPriClaims) + " "
                      + "WHERE InsFilingCodeNum = " + SOut.Long(insFilingCode.InsFilingCodeNum);
        Db.NonQ(command);
    }

    public static bool Update(InsFilingCode insFilingCode, InsFilingCode oldInsFilingCode)
    {
        var command = "";
        if (insFilingCode.Descript != oldInsFilingCode.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(insFilingCode.Descript) + "'";
        }

        if (insFilingCode.EclaimCode != oldInsFilingCode.EclaimCode)
        {
            if (command != "") command += ",";
            command += "EclaimCode = '" + SOut.String(insFilingCode.EclaimCode) + "'";
        }

        if (insFilingCode.ItemOrder != oldInsFilingCode.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(insFilingCode.ItemOrder) + "";
        }

        if (insFilingCode.GroupType != oldInsFilingCode.GroupType)
        {
            if (command != "") command += ",";
            command += "GroupType = " + SOut.Long(insFilingCode.GroupType) + "";
        }

        if (insFilingCode.ExcludeOtherCoverageOnPriClaims != oldInsFilingCode.ExcludeOtherCoverageOnPriClaims)
        {
            if (command != "") command += ",";
            command += "ExcludeOtherCoverageOnPriClaims = " + SOut.Bool(insFilingCode.ExcludeOtherCoverageOnPriClaims) + "";
        }

        if (command == "") return false;
        command = "UPDATE insfilingcode SET " + command
                                              + " WHERE InsFilingCodeNum = " + SOut.Long(insFilingCode.InsFilingCodeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(InsFilingCode insFilingCode, InsFilingCode oldInsFilingCode)
    {
        if (insFilingCode.Descript != oldInsFilingCode.Descript) return true;
        if (insFilingCode.EclaimCode != oldInsFilingCode.EclaimCode) return true;
        if (insFilingCode.ItemOrder != oldInsFilingCode.ItemOrder) return true;
        if (insFilingCode.GroupType != oldInsFilingCode.GroupType) return true;
        if (insFilingCode.ExcludeOtherCoverageOnPriClaims != oldInsFilingCode.ExcludeOtherCoverageOnPriClaims) return true;
        return false;
    }

    public static void Delete(long insFilingCodeNum)
    {
        var command = "DELETE FROM insfilingcode "
                      + "WHERE InsFilingCodeNum = " + SOut.Long(insFilingCodeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listInsFilingCodeNums)
    {
        if (listInsFilingCodeNums == null || listInsFilingCodeNums.Count == 0) return;
        var command = "DELETE FROM insfilingcode "
                      + "WHERE InsFilingCodeNum IN(" + string.Join(",", listInsFilingCodeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}