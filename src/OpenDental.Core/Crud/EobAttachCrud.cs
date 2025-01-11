#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EobAttachCrud
{
    public static EobAttach SelectOne(long eobAttachNum)
    {
        var command = "SELECT * FROM eobattach "
                      + "WHERE EobAttachNum = " + SOut.Long(eobAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EobAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EobAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EobAttach> TableToList(DataTable table)
    {
        var retVal = new List<EobAttach>();
        EobAttach eobAttach;
        foreach (DataRow row in table.Rows)
        {
            eobAttach = new EobAttach();
            eobAttach.EobAttachNum = SIn.Long(row["EobAttachNum"].ToString());
            eobAttach.ClaimPaymentNum = SIn.Long(row["ClaimPaymentNum"].ToString());
            eobAttach.DateTCreated = SIn.DateTime(row["DateTCreated"].ToString());
            eobAttach.FileName = SIn.String(row["FileName"].ToString());
            eobAttach.RawBase64 = SIn.String(row["RawBase64"].ToString());
            retVal.Add(eobAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EobAttach> listEobAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EobAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("EobAttachNum");
        table.Columns.Add("ClaimPaymentNum");
        table.Columns.Add("DateTCreated");
        table.Columns.Add("FileName");
        table.Columns.Add("RawBase64");
        foreach (var eobAttach in listEobAttachs)
            table.Rows.Add(SOut.Long(eobAttach.EobAttachNum), SOut.Long(eobAttach.ClaimPaymentNum), SOut.DateT(eobAttach.DateTCreated, false), eobAttach.FileName, eobAttach.RawBase64);
        return table;
    }

    public static long Insert(EobAttach eobAttach)
    {
        return Insert(eobAttach, false);
    }

    public static long Insert(EobAttach eobAttach, bool useExistingPK)
    {
        var command = "INSERT INTO eobattach (";

        command += "ClaimPaymentNum,DateTCreated,FileName,RawBase64) VALUES(";

        command +=
            SOut.Long(eobAttach.ClaimPaymentNum) + ","
                                                 + SOut.DateT(eobAttach.DateTCreated) + ","
                                                 + "'" + SOut.String(eobAttach.FileName) + "',"
                                                 + DbHelper.ParamChar + "paramRawBase64)";
        if (eobAttach.RawBase64 == null) eobAttach.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(eobAttach.RawBase64));
        {
            eobAttach.EobAttachNum = Db.NonQ(command, true, "EobAttachNum", "eobAttach", paramRawBase64);
        }
        return eobAttach.EobAttachNum;
    }

    public static long InsertNoCache(EobAttach eobAttach)
    {
        return InsertNoCache(eobAttach, false);
    }

    public static long InsertNoCache(EobAttach eobAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eobattach (";
        if (isRandomKeys || useExistingPK) command += "EobAttachNum,";
        command += "ClaimPaymentNum,DateTCreated,FileName,RawBase64) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eobAttach.EobAttachNum) + ",";
        command +=
            SOut.Long(eobAttach.ClaimPaymentNum) + ","
                                                 + SOut.DateT(eobAttach.DateTCreated) + ","
                                                 + "'" + SOut.String(eobAttach.FileName) + "',"
                                                 + DbHelper.ParamChar + "paramRawBase64)";
        if (eobAttach.RawBase64 == null) eobAttach.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(eobAttach.RawBase64));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramRawBase64);
        else
            eobAttach.EobAttachNum = Db.NonQ(command, true, "EobAttachNum", "eobAttach", paramRawBase64);
        return eobAttach.EobAttachNum;
    }

    public static void Update(EobAttach eobAttach)
    {
        var command = "UPDATE eobattach SET "
                      + "ClaimPaymentNum=  " + SOut.Long(eobAttach.ClaimPaymentNum) + ", "
                      + "DateTCreated   =  " + SOut.DateT(eobAttach.DateTCreated) + ", "
                      + "FileName       = '" + SOut.String(eobAttach.FileName) + "', "
                      + "RawBase64      =  " + DbHelper.ParamChar + "paramRawBase64 "
                      + "WHERE EobAttachNum = " + SOut.Long(eobAttach.EobAttachNum);
        if (eobAttach.RawBase64 == null) eobAttach.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(eobAttach.RawBase64));
        Db.NonQ(command, paramRawBase64);
    }

    public static bool Update(EobAttach eobAttach, EobAttach oldEobAttach)
    {
        var command = "";
        if (eobAttach.ClaimPaymentNum != oldEobAttach.ClaimPaymentNum)
        {
            if (command != "") command += ",";
            command += "ClaimPaymentNum = " + SOut.Long(eobAttach.ClaimPaymentNum) + "";
        }

        if (eobAttach.DateTCreated != oldEobAttach.DateTCreated)
        {
            if (command != "") command += ",";
            command += "DateTCreated = " + SOut.DateT(eobAttach.DateTCreated) + "";
        }

        if (eobAttach.FileName != oldEobAttach.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(eobAttach.FileName) + "'";
        }

        if (eobAttach.RawBase64 != oldEobAttach.RawBase64)
        {
            if (command != "") command += ",";
            command += "RawBase64 = " + DbHelper.ParamChar + "paramRawBase64";
        }

        if (command == "") return false;
        if (eobAttach.RawBase64 == null) eobAttach.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(eobAttach.RawBase64));
        command = "UPDATE eobattach SET " + command
                                          + " WHERE EobAttachNum = " + SOut.Long(eobAttach.EobAttachNum);
        Db.NonQ(command, paramRawBase64);
        return true;
    }

    public static bool UpdateComparison(EobAttach eobAttach, EobAttach oldEobAttach)
    {
        if (eobAttach.ClaimPaymentNum != oldEobAttach.ClaimPaymentNum) return true;
        if (eobAttach.DateTCreated != oldEobAttach.DateTCreated) return true;
        if (eobAttach.FileName != oldEobAttach.FileName) return true;
        if (eobAttach.RawBase64 != oldEobAttach.RawBase64) return true;
        return false;
    }

    public static void Delete(long eobAttachNum)
    {
        var command = "DELETE FROM eobattach "
                      + "WHERE EobAttachNum = " + SOut.Long(eobAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEobAttachNums)
    {
        if (listEobAttachNums == null || listEobAttachNums.Count == 0) return;
        var command = "DELETE FROM eobattach "
                      + "WHERE EobAttachNum IN(" + string.Join(",", listEobAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}