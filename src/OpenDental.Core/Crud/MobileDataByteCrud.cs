#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MobileDataByteCrud
{
    public static MobileDataByte SelectOne(long mobileDataByteNum)
    {
        var command = "SELECT * FROM mobiledatabyte "
                      + "WHERE MobileDataByteNum = " + SOut.Long(mobileDataByteNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MobileDataByte SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MobileDataByte> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MobileDataByte> TableToList(DataTable table)
    {
        var retVal = new List<MobileDataByte>();
        MobileDataByte mobileDataByte;
        foreach (DataRow row in table.Rows)
        {
            mobileDataByte = new MobileDataByte();
            mobileDataByte.MobileDataByteNum = SIn.Long(row["MobileDataByteNum"].ToString());
            mobileDataByte.RawBase64Data = SIn.String(row["RawBase64Data"].ToString());
            mobileDataByte.RawBase64Code = SIn.String(row["RawBase64Code"].ToString());
            mobileDataByte.RawBase64Tag = SIn.String(row["RawBase64Tag"].ToString());
            mobileDataByte.PatNum = SIn.Long(row["PatNum"].ToString());
            mobileDataByte.ActionType = (eActionType) SIn.Int(row["ActionType"].ToString());
            mobileDataByte.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            mobileDataByte.DateTimeExpires = SIn.DateTime(row["DateTimeExpires"].ToString());
            retVal.Add(mobileDataByte);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MobileDataByte> listMobileDataBytes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MobileDataByte";
        var table = new DataTable(tableName);
        table.Columns.Add("MobileDataByteNum");
        table.Columns.Add("RawBase64Data");
        table.Columns.Add("RawBase64Code");
        table.Columns.Add("RawBase64Tag");
        table.Columns.Add("PatNum");
        table.Columns.Add("ActionType");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimeExpires");
        foreach (var mobileDataByte in listMobileDataBytes)
            table.Rows.Add(SOut.Long(mobileDataByte.MobileDataByteNum), mobileDataByte.RawBase64Data, mobileDataByte.RawBase64Code, mobileDataByte.RawBase64Tag, SOut.Long(mobileDataByte.PatNum), SOut.Int((int) mobileDataByte.ActionType), SOut.DateT(mobileDataByte.DateTimeEntry, false), SOut.DateT(mobileDataByte.DateTimeExpires, false));
        return table;
    }

    public static long Insert(MobileDataByte mobileDataByte)
    {
        return Insert(mobileDataByte, false);
    }

    public static long Insert(MobileDataByte mobileDataByte, bool useExistingPK)
    {
        var command = "INSERT INTO mobiledatabyte (";

        command += "RawBase64Data,RawBase64Code,RawBase64Tag,PatNum,ActionType,DateTimeEntry,DateTimeExpires) VALUES(";

        command +=
            DbHelper.ParamChar + "paramRawBase64Data,"
                               + DbHelper.ParamChar + "paramRawBase64Code,"
                               + DbHelper.ParamChar + "paramRawBase64Tag,"
                               + SOut.Long(mobileDataByte.PatNum) + ","
                               + SOut.Int((int) mobileDataByte.ActionType) + ","
                               + DbHelper.Now() + ","
                               + SOut.DateT(mobileDataByte.DateTimeExpires) + ")";
        if (mobileDataByte.RawBase64Data == null) mobileDataByte.RawBase64Data = "";
        var paramRawBase64Data = new OdSqlParameter("paramRawBase64Data", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Data));
        if (mobileDataByte.RawBase64Code == null) mobileDataByte.RawBase64Code = "";
        var paramRawBase64Code = new OdSqlParameter("paramRawBase64Code", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Code));
        if (mobileDataByte.RawBase64Tag == null) mobileDataByte.RawBase64Tag = "";
        var paramRawBase64Tag = new OdSqlParameter("paramRawBase64Tag", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Tag));
        {
            mobileDataByte.MobileDataByteNum = Db.NonQ(command, true, "MobileDataByteNum", "mobileDataByte", paramRawBase64Data, paramRawBase64Code, paramRawBase64Tag);
        }
        return mobileDataByte.MobileDataByteNum;
    }

    public static long InsertNoCache(MobileDataByte mobileDataByte)
    {
        return InsertNoCache(mobileDataByte, false);
    }

    public static long InsertNoCache(MobileDataByte mobileDataByte, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mobiledatabyte (";
        if (isRandomKeys || useExistingPK) command += "MobileDataByteNum,";
        command += "RawBase64Data,RawBase64Code,RawBase64Tag,PatNum,ActionType,DateTimeEntry,DateTimeExpires) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mobileDataByte.MobileDataByteNum) + ",";
        command +=
            DbHelper.ParamChar + "paramRawBase64Data,"
                               + DbHelper.ParamChar + "paramRawBase64Code,"
                               + DbHelper.ParamChar + "paramRawBase64Tag,"
                               + SOut.Long(mobileDataByte.PatNum) + ","
                               + SOut.Int((int) mobileDataByte.ActionType) + ","
                               + DbHelper.Now() + ","
                               + SOut.DateT(mobileDataByte.DateTimeExpires) + ")";
        if (mobileDataByte.RawBase64Data == null) mobileDataByte.RawBase64Data = "";
        var paramRawBase64Data = new OdSqlParameter("paramRawBase64Data", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Data));
        if (mobileDataByte.RawBase64Code == null) mobileDataByte.RawBase64Code = "";
        var paramRawBase64Code = new OdSqlParameter("paramRawBase64Code", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Code));
        if (mobileDataByte.RawBase64Tag == null) mobileDataByte.RawBase64Tag = "";
        var paramRawBase64Tag = new OdSqlParameter("paramRawBase64Tag", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Tag));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramRawBase64Data, paramRawBase64Code, paramRawBase64Tag);
        else
            mobileDataByte.MobileDataByteNum = Db.NonQ(command, true, "MobileDataByteNum", "mobileDataByte", paramRawBase64Data, paramRawBase64Code, paramRawBase64Tag);
        return mobileDataByte.MobileDataByteNum;
    }

    public static void Update(MobileDataByte mobileDataByte)
    {
        var command = "UPDATE mobiledatabyte SET "
                      + "RawBase64Data    =  " + DbHelper.ParamChar + "paramRawBase64Data, "
                      + "RawBase64Code    =  " + DbHelper.ParamChar + "paramRawBase64Code, "
                      + "RawBase64Tag     =  " + DbHelper.ParamChar + "paramRawBase64Tag, "
                      + "PatNum           =  " + SOut.Long(mobileDataByte.PatNum) + ", "
                      + "ActionType       =  " + SOut.Int((int) mobileDataByte.ActionType) + ", "
                      //DateTimeEntry not allowed to change
                      + "DateTimeExpires  =  " + SOut.DateT(mobileDataByte.DateTimeExpires) + " "
                      + "WHERE MobileDataByteNum = " + SOut.Long(mobileDataByte.MobileDataByteNum);
        if (mobileDataByte.RawBase64Data == null) mobileDataByte.RawBase64Data = "";
        var paramRawBase64Data = new OdSqlParameter("paramRawBase64Data", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Data));
        if (mobileDataByte.RawBase64Code == null) mobileDataByte.RawBase64Code = "";
        var paramRawBase64Code = new OdSqlParameter("paramRawBase64Code", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Code));
        if (mobileDataByte.RawBase64Tag == null) mobileDataByte.RawBase64Tag = "";
        var paramRawBase64Tag = new OdSqlParameter("paramRawBase64Tag", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Tag));
        Db.NonQ(command, paramRawBase64Data, paramRawBase64Code, paramRawBase64Tag);
    }

    public static bool Update(MobileDataByte mobileDataByte, MobileDataByte oldMobileDataByte)
    {
        var command = "";
        if (mobileDataByte.RawBase64Data != oldMobileDataByte.RawBase64Data)
        {
            if (command != "") command += ",";
            command += "RawBase64Data = " + DbHelper.ParamChar + "paramRawBase64Data";
        }

        if (mobileDataByte.RawBase64Code != oldMobileDataByte.RawBase64Code)
        {
            if (command != "") command += ",";
            command += "RawBase64Code = " + DbHelper.ParamChar + "paramRawBase64Code";
        }

        if (mobileDataByte.RawBase64Tag != oldMobileDataByte.RawBase64Tag)
        {
            if (command != "") command += ",";
            command += "RawBase64Tag = " + DbHelper.ParamChar + "paramRawBase64Tag";
        }

        if (mobileDataByte.PatNum != oldMobileDataByte.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(mobileDataByte.PatNum) + "";
        }

        if (mobileDataByte.ActionType != oldMobileDataByte.ActionType)
        {
            if (command != "") command += ",";
            command += "ActionType = " + SOut.Int((int) mobileDataByte.ActionType) + "";
        }

        //DateTimeEntry not allowed to change
        if (mobileDataByte.DateTimeExpires != oldMobileDataByte.DateTimeExpires)
        {
            if (command != "") command += ",";
            command += "DateTimeExpires = " + SOut.DateT(mobileDataByte.DateTimeExpires) + "";
        }

        if (command == "") return false;
        if (mobileDataByte.RawBase64Data == null) mobileDataByte.RawBase64Data = "";
        var paramRawBase64Data = new OdSqlParameter("paramRawBase64Data", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Data));
        if (mobileDataByte.RawBase64Code == null) mobileDataByte.RawBase64Code = "";
        var paramRawBase64Code = new OdSqlParameter("paramRawBase64Code", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Code));
        if (mobileDataByte.RawBase64Tag == null) mobileDataByte.RawBase64Tag = "";
        var paramRawBase64Tag = new OdSqlParameter("paramRawBase64Tag", OdDbType.Text, SOut.StringParam(mobileDataByte.RawBase64Tag));
        command = "UPDATE mobiledatabyte SET " + command
                                               + " WHERE MobileDataByteNum = " + SOut.Long(mobileDataByte.MobileDataByteNum);
        Db.NonQ(command, paramRawBase64Data, paramRawBase64Code, paramRawBase64Tag);
        return true;
    }

    public static bool UpdateComparison(MobileDataByte mobileDataByte, MobileDataByte oldMobileDataByte)
    {
        if (mobileDataByte.RawBase64Data != oldMobileDataByte.RawBase64Data) return true;
        if (mobileDataByte.RawBase64Code != oldMobileDataByte.RawBase64Code) return true;
        if (mobileDataByte.RawBase64Tag != oldMobileDataByte.RawBase64Tag) return true;
        if (mobileDataByte.PatNum != oldMobileDataByte.PatNum) return true;
        if (mobileDataByte.ActionType != oldMobileDataByte.ActionType) return true;
        //DateTimeEntry not allowed to change
        if (mobileDataByte.DateTimeExpires != oldMobileDataByte.DateTimeExpires) return true;
        return false;
    }

    public static void Delete(long mobileDataByteNum)
    {
        var command = "DELETE FROM mobiledatabyte "
                      + "WHERE MobileDataByteNum = " + SOut.Long(mobileDataByteNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMobileDataByteNums)
    {
        if (listMobileDataByteNums == null || listMobileDataByteNums.Count == 0) return;
        var command = "DELETE FROM mobiledatabyte "
                      + "WHERE MobileDataByteNum IN(" + string.Join(",", listMobileDataByteNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}