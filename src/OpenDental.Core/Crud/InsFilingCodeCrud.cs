using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsFilingCodeCrud
{
    public static List<InsFilingCode> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsFilingCode> TableToList(DataTable table)
    {
        var retVal = new List<InsFilingCode>();
        foreach (DataRow row in table.Rows)
        {
            var insFilingCode = new InsFilingCode
            {
                InsFilingCodeNum = SIn.Long(row["InsFilingCodeNum"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                EclaimCode = SIn.String(row["EclaimCode"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                GroupType = SIn.Long(row["GroupType"].ToString()),
                ExcludeOtherCoverageOnPriClaims = SIn.Bool(row["ExcludeOtherCoverageOnPriClaims"].ToString())
            };
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

    public static void Delete(long insFilingCodeNum)
    {
        var command = "DELETE FROM insfilingcode "
                      + "WHERE InsFilingCodeNum = " + SOut.Long(insFilingCodeNum);
        Db.NonQ(command);
    }
}