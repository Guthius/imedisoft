#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PayorTypeCrud
{
    public static PayorType SelectOne(long payorTypeNum)
    {
        var command = "SELECT * FROM payortype "
                      + "WHERE PayorTypeNum = " + SOut.Long(payorTypeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PayorType SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PayorType> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PayorType> TableToList(DataTable table)
    {
        var retVal = new List<PayorType>();
        PayorType payorType;
        foreach (DataRow row in table.Rows)
        {
            payorType = new PayorType();
            payorType.PayorTypeNum = SIn.Long(row["PayorTypeNum"].ToString());
            payorType.PatNum = SIn.Long(row["PatNum"].ToString());
            payorType.DateStart = SIn.Date(row["DateStart"].ToString());
            payorType.SopCode = SIn.String(row["SopCode"].ToString());
            payorType.Note = SIn.String(row["Note"].ToString());
            retVal.Add(payorType);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PayorType> listPayorTypes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PayorType";
        var table = new DataTable(tableName);
        table.Columns.Add("PayorTypeNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateStart");
        table.Columns.Add("SopCode");
        table.Columns.Add("Note");
        foreach (var payorType in listPayorTypes)
            table.Rows.Add(SOut.Long(payorType.PayorTypeNum), SOut.Long(payorType.PatNum), SOut.DateT(payorType.DateStart, false), payorType.SopCode, payorType.Note);
        return table;
    }

    public static long Insert(PayorType payorType)
    {
        return Insert(payorType, false);
    }

    public static long Insert(PayorType payorType, bool useExistingPK)
    {
        var command = "INSERT INTO payortype (";

        command += "PatNum,DateStart,SopCode,Note) VALUES(";

        command +=
            SOut.Long(payorType.PatNum) + ","
                                        + SOut.Date(payorType.DateStart) + ","
                                        + "'" + SOut.String(payorType.SopCode) + "',"
                                        + DbHelper.ParamChar + "paramNote)";
        if (payorType.Note == null) payorType.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payorType.Note));
        {
            payorType.PayorTypeNum = Db.NonQ(command, true, "PayorTypeNum", "payorType", paramNote);
        }
        return payorType.PayorTypeNum;
    }

    public static long InsertNoCache(PayorType payorType)
    {
        return InsertNoCache(payorType, false);
    }

    public static long InsertNoCache(PayorType payorType, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO payortype (";
        if (isRandomKeys || useExistingPK) command += "PayorTypeNum,";
        command += "PatNum,DateStart,SopCode,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(payorType.PayorTypeNum) + ",";
        command +=
            SOut.Long(payorType.PatNum) + ","
                                        + SOut.Date(payorType.DateStart) + ","
                                        + "'" + SOut.String(payorType.SopCode) + "',"
                                        + DbHelper.ParamChar + "paramNote)";
        if (payorType.Note == null) payorType.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payorType.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            payorType.PayorTypeNum = Db.NonQ(command, true, "PayorTypeNum", "payorType", paramNote);
        return payorType.PayorTypeNum;
    }

    public static void Update(PayorType payorType)
    {
        var command = "UPDATE payortype SET "
                      + "PatNum      =  " + SOut.Long(payorType.PatNum) + ", "
                      + "DateStart   =  " + SOut.Date(payorType.DateStart) + ", "
                      + "SopCode     = '" + SOut.String(payorType.SopCode) + "', "
                      + "Note        =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE PayorTypeNum = " + SOut.Long(payorType.PayorTypeNum);
        if (payorType.Note == null) payorType.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payorType.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(PayorType payorType, PayorType oldPayorType)
    {
        var command = "";
        if (payorType.PatNum != oldPayorType.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(payorType.PatNum) + "";
        }

        if (payorType.DateStart.Date != oldPayorType.DateStart.Date)
        {
            if (command != "") command += ",";
            command += "DateStart = " + SOut.Date(payorType.DateStart) + "";
        }

        if (payorType.SopCode != oldPayorType.SopCode)
        {
            if (command != "") command += ",";
            command += "SopCode = '" + SOut.String(payorType.SopCode) + "'";
        }

        if (payorType.Note != oldPayorType.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (payorType.Note == null) payorType.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(payorType.Note));
        command = "UPDATE payortype SET " + command
                                          + " WHERE PayorTypeNum = " + SOut.Long(payorType.PayorTypeNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(PayorType payorType, PayorType oldPayorType)
    {
        if (payorType.PatNum != oldPayorType.PatNum) return true;
        if (payorType.DateStart.Date != oldPayorType.DateStart.Date) return true;
        if (payorType.SopCode != oldPayorType.SopCode) return true;
        if (payorType.Note != oldPayorType.Note) return true;
        return false;
    }

    public static void Delete(long payorTypeNum)
    {
        var command = "DELETE FROM payortype "
                      + "WHERE PayorTypeNum = " + SOut.Long(payorTypeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPayorTypeNums)
    {
        if (listPayorTypeNums == null || listPayorTypeNums.Count == 0) return;
        var command = "DELETE FROM payortype "
                      + "WHERE PayorTypeNum IN(" + string.Join(",", listPayorTypeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}