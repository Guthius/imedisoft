#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RxNormCrud
{
    public static RxNorm SelectOne(long rxNormNum)
    {
        var command = "SELECT * FROM rxnorm "
                      + "WHERE RxNormNum = " + SOut.Long(rxNormNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RxNorm SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RxNorm> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RxNorm> TableToList(DataTable table)
    {
        var retVal = new List<RxNorm>();
        RxNorm rxNorm;
        foreach (DataRow row in table.Rows)
        {
            rxNorm = new RxNorm();
            rxNorm.RxNormNum = SIn.Long(row["RxNormNum"].ToString());
            rxNorm.RxCui = SIn.String(row["RxCui"].ToString());
            rxNorm.MmslCode = SIn.String(row["MmslCode"].ToString());
            rxNorm.Description = SIn.String(row["Description"].ToString());
            retVal.Add(rxNorm);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RxNorm> listRxNorms, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RxNorm";
        var table = new DataTable(tableName);
        table.Columns.Add("RxNormNum");
        table.Columns.Add("RxCui");
        table.Columns.Add("MmslCode");
        table.Columns.Add("Description");
        foreach (var rxNorm in listRxNorms)
            table.Rows.Add(SOut.Long(rxNorm.RxNormNum), rxNorm.RxCui, rxNorm.MmslCode, rxNorm.Description);
        return table;
    }

    public static long Insert(RxNorm rxNorm)
    {
        return Insert(rxNorm, false);
    }

    public static long Insert(RxNorm rxNorm, bool useExistingPK)
    {
        var command = "INSERT INTO rxnorm (";

        command += "RxCui,MmslCode,Description) VALUES(";

        command +=
            "'" + SOut.String(rxNorm.RxCui) + "',"
            + "'" + SOut.String(rxNorm.MmslCode) + "',"
            + DbHelper.ParamChar + "paramDescription)";
        if (rxNorm.Description == null) rxNorm.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(rxNorm.Description));
        {
            rxNorm.RxNormNum = Db.NonQ(command, true, "RxNormNum", "rxNorm", paramDescription);
        }
        return rxNorm.RxNormNum;
    }

    public static long InsertNoCache(RxNorm rxNorm)
    {
        return InsertNoCache(rxNorm, false);
    }

    public static long InsertNoCache(RxNorm rxNorm, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO rxnorm (";
        if (isRandomKeys || useExistingPK) command += "RxNormNum,";
        command += "RxCui,MmslCode,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(rxNorm.RxNormNum) + ",";
        command +=
            "'" + SOut.String(rxNorm.RxCui) + "',"
            + "'" + SOut.String(rxNorm.MmslCode) + "',"
            + DbHelper.ParamChar + "paramDescription)";
        if (rxNorm.Description == null) rxNorm.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(rxNorm.Description));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription);
        else
            rxNorm.RxNormNum = Db.NonQ(command, true, "RxNormNum", "rxNorm", paramDescription);
        return rxNorm.RxNormNum;
    }

    public static void Update(RxNorm rxNorm)
    {
        var command = "UPDATE rxnorm SET "
                      + "RxCui      = '" + SOut.String(rxNorm.RxCui) + "', "
                      + "MmslCode   = '" + SOut.String(rxNorm.MmslCode) + "', "
                      + "Description=  " + DbHelper.ParamChar + "paramDescription "
                      + "WHERE RxNormNum = " + SOut.Long(rxNorm.RxNormNum);
        if (rxNorm.Description == null) rxNorm.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(rxNorm.Description));
        Db.NonQ(command, paramDescription);
    }

    public static bool Update(RxNorm rxNorm, RxNorm oldRxNorm)
    {
        var command = "";
        if (rxNorm.RxCui != oldRxNorm.RxCui)
        {
            if (command != "") command += ",";
            command += "RxCui = '" + SOut.String(rxNorm.RxCui) + "'";
        }

        if (rxNorm.MmslCode != oldRxNorm.MmslCode)
        {
            if (command != "") command += ",";
            command += "MmslCode = '" + SOut.String(rxNorm.MmslCode) + "'";
        }

        if (rxNorm.Description != oldRxNorm.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (command == "") return false;
        if (rxNorm.Description == null) rxNorm.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(rxNorm.Description));
        command = "UPDATE rxnorm SET " + command
                                       + " WHERE RxNormNum = " + SOut.Long(rxNorm.RxNormNum);
        Db.NonQ(command, paramDescription);
        return true;
    }

    public static bool UpdateComparison(RxNorm rxNorm, RxNorm oldRxNorm)
    {
        if (rxNorm.RxCui != oldRxNorm.RxCui) return true;
        if (rxNorm.MmslCode != oldRxNorm.MmslCode) return true;
        if (rxNorm.Description != oldRxNorm.Description) return true;
        return false;
    }

    public static void Delete(long rxNormNum)
    {
        var command = "DELETE FROM rxnorm "
                      + "WHERE RxNormNum = " + SOut.Long(rxNormNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRxNormNums)
    {
        if (listRxNormNums == null || listRxNormNums.Count == 0) return;
        var command = "DELETE FROM rxnorm "
                      + "WHERE RxNormNum IN(" + string.Join(",", listRxNormNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}