#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrAmendmentCrud
{
    public static EhrAmendment SelectOne(long ehrAmendmentNum)
    {
        var command = "SELECT * FROM ehramendment "
                      + "WHERE EhrAmendmentNum = " + SOut.Long(ehrAmendmentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrAmendment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrAmendment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrAmendment> TableToList(DataTable table)
    {
        var retVal = new List<EhrAmendment>();
        EhrAmendment ehrAmendment;
        foreach (DataRow row in table.Rows)
        {
            ehrAmendment = new EhrAmendment();
            ehrAmendment.EhrAmendmentNum = SIn.Long(row["EhrAmendmentNum"].ToString());
            ehrAmendment.PatNum = SIn.Long(row["PatNum"].ToString());
            ehrAmendment.IsAccepted = (YN) SIn.Int(row["IsAccepted"].ToString());
            ehrAmendment.Description = SIn.String(row["Description"].ToString());
            ehrAmendment.Source = (AmendmentSource) SIn.Int(row["Source"].ToString());
            ehrAmendment.SourceName = SIn.String(row["SourceName"].ToString());
            ehrAmendment.FileName = SIn.String(row["FileName"].ToString());
            ehrAmendment.RawBase64 = SIn.String(row["RawBase64"].ToString());
            ehrAmendment.DateTRequest = SIn.DateTime(row["DateTRequest"].ToString());
            ehrAmendment.DateTAcceptDeny = SIn.DateTime(row["DateTAcceptDeny"].ToString());
            ehrAmendment.DateTAppend = SIn.DateTime(row["DateTAppend"].ToString());
            retVal.Add(ehrAmendment);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrAmendment> listEhrAmendments, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrAmendment";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrAmendmentNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("IsAccepted");
        table.Columns.Add("Description");
        table.Columns.Add("Source");
        table.Columns.Add("SourceName");
        table.Columns.Add("FileName");
        table.Columns.Add("RawBase64");
        table.Columns.Add("DateTRequest");
        table.Columns.Add("DateTAcceptDeny");
        table.Columns.Add("DateTAppend");
        foreach (var ehrAmendment in listEhrAmendments)
            table.Rows.Add(SOut.Long(ehrAmendment.EhrAmendmentNum), SOut.Long(ehrAmendment.PatNum), SOut.Int((int) ehrAmendment.IsAccepted), ehrAmendment.Description, SOut.Int((int) ehrAmendment.Source), ehrAmendment.SourceName, ehrAmendment.FileName, ehrAmendment.RawBase64, SOut.DateT(ehrAmendment.DateTRequest, false), SOut.DateT(ehrAmendment.DateTAcceptDeny, false), SOut.DateT(ehrAmendment.DateTAppend, false));
        return table;
    }

    public static long Insert(EhrAmendment ehrAmendment)
    {
        return Insert(ehrAmendment, false);
    }

    public static long Insert(EhrAmendment ehrAmendment, bool useExistingPK)
    {
        var command = "INSERT INTO ehramendment (";

        command += "PatNum,IsAccepted,Description,Source,SourceName,FileName,RawBase64,DateTRequest,DateTAcceptDeny,DateTAppend) VALUES(";

        command +=
            SOut.Long(ehrAmendment.PatNum) + ","
                                           + SOut.Int((int) ehrAmendment.IsAccepted) + ","
                                           + DbHelper.ParamChar + "paramDescription,"
                                           + SOut.Int((int) ehrAmendment.Source) + ","
                                           + DbHelper.ParamChar + "paramSourceName,"
                                           + "'" + SOut.String(ehrAmendment.FileName) + "',"
                                           + DbHelper.ParamChar + "paramRawBase64,"
                                           + SOut.DateT(ehrAmendment.DateTRequest) + ","
                                           + SOut.DateT(ehrAmendment.DateTAcceptDeny) + ","
                                           + SOut.DateT(ehrAmendment.DateTAppend) + ")";
        if (ehrAmendment.Description == null) ehrAmendment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(ehrAmendment.Description));
        if (ehrAmendment.SourceName == null) ehrAmendment.SourceName = "";
        var paramSourceName = new OdSqlParameter("paramSourceName", OdDbType.Text, SOut.StringParam(ehrAmendment.SourceName));
        if (ehrAmendment.RawBase64 == null) ehrAmendment.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(ehrAmendment.RawBase64));
        {
            ehrAmendment.EhrAmendmentNum = Db.NonQ(command, true, "EhrAmendmentNum", "ehrAmendment", paramDescription, paramSourceName, paramRawBase64);
        }
        return ehrAmendment.EhrAmendmentNum;
    }

    public static long InsertNoCache(EhrAmendment ehrAmendment)
    {
        return InsertNoCache(ehrAmendment, false);
    }

    public static long InsertNoCache(EhrAmendment ehrAmendment, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehramendment (";
        if (isRandomKeys || useExistingPK) command += "EhrAmendmentNum,";
        command += "PatNum,IsAccepted,Description,Source,SourceName,FileName,RawBase64,DateTRequest,DateTAcceptDeny,DateTAppend) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrAmendment.EhrAmendmentNum) + ",";
        command +=
            SOut.Long(ehrAmendment.PatNum) + ","
                                           + SOut.Int((int) ehrAmendment.IsAccepted) + ","
                                           + DbHelper.ParamChar + "paramDescription,"
                                           + SOut.Int((int) ehrAmendment.Source) + ","
                                           + DbHelper.ParamChar + "paramSourceName,"
                                           + "'" + SOut.String(ehrAmendment.FileName) + "',"
                                           + DbHelper.ParamChar + "paramRawBase64,"
                                           + SOut.DateT(ehrAmendment.DateTRequest) + ","
                                           + SOut.DateT(ehrAmendment.DateTAcceptDeny) + ","
                                           + SOut.DateT(ehrAmendment.DateTAppend) + ")";
        if (ehrAmendment.Description == null) ehrAmendment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(ehrAmendment.Description));
        if (ehrAmendment.SourceName == null) ehrAmendment.SourceName = "";
        var paramSourceName = new OdSqlParameter("paramSourceName", OdDbType.Text, SOut.StringParam(ehrAmendment.SourceName));
        if (ehrAmendment.RawBase64 == null) ehrAmendment.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(ehrAmendment.RawBase64));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription, paramSourceName, paramRawBase64);
        else
            ehrAmendment.EhrAmendmentNum = Db.NonQ(command, true, "EhrAmendmentNum", "ehrAmendment", paramDescription, paramSourceName, paramRawBase64);
        return ehrAmendment.EhrAmendmentNum;
    }

    public static void Update(EhrAmendment ehrAmendment)
    {
        var command = "UPDATE ehramendment SET "
                      + "PatNum         =  " + SOut.Long(ehrAmendment.PatNum) + ", "
                      + "IsAccepted     =  " + SOut.Int((int) ehrAmendment.IsAccepted) + ", "
                      + "Description    =  " + DbHelper.ParamChar + "paramDescription, "
                      + "Source         =  " + SOut.Int((int) ehrAmendment.Source) + ", "
                      + "SourceName     =  " + DbHelper.ParamChar + "paramSourceName, "
                      + "FileName       = '" + SOut.String(ehrAmendment.FileName) + "', "
                      + "RawBase64      =  " + DbHelper.ParamChar + "paramRawBase64, "
                      + "DateTRequest   =  " + SOut.DateT(ehrAmendment.DateTRequest) + ", "
                      + "DateTAcceptDeny=  " + SOut.DateT(ehrAmendment.DateTAcceptDeny) + ", "
                      + "DateTAppend    =  " + SOut.DateT(ehrAmendment.DateTAppend) + " "
                      + "WHERE EhrAmendmentNum = " + SOut.Long(ehrAmendment.EhrAmendmentNum);
        if (ehrAmendment.Description == null) ehrAmendment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(ehrAmendment.Description));
        if (ehrAmendment.SourceName == null) ehrAmendment.SourceName = "";
        var paramSourceName = new OdSqlParameter("paramSourceName", OdDbType.Text, SOut.StringParam(ehrAmendment.SourceName));
        if (ehrAmendment.RawBase64 == null) ehrAmendment.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(ehrAmendment.RawBase64));
        Db.NonQ(command, paramDescription, paramSourceName, paramRawBase64);
    }

    public static bool Update(EhrAmendment ehrAmendment, EhrAmendment oldEhrAmendment)
    {
        var command = "";
        if (ehrAmendment.PatNum != oldEhrAmendment.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrAmendment.PatNum) + "";
        }

        if (ehrAmendment.IsAccepted != oldEhrAmendment.IsAccepted)
        {
            if (command != "") command += ",";
            command += "IsAccepted = " + SOut.Int((int) ehrAmendment.IsAccepted) + "";
        }

        if (ehrAmendment.Description != oldEhrAmendment.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (ehrAmendment.Source != oldEhrAmendment.Source)
        {
            if (command != "") command += ",";
            command += "Source = " + SOut.Int((int) ehrAmendment.Source) + "";
        }

        if (ehrAmendment.SourceName != oldEhrAmendment.SourceName)
        {
            if (command != "") command += ",";
            command += "SourceName = " + DbHelper.ParamChar + "paramSourceName";
        }

        if (ehrAmendment.FileName != oldEhrAmendment.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(ehrAmendment.FileName) + "'";
        }

        if (ehrAmendment.RawBase64 != oldEhrAmendment.RawBase64)
        {
            if (command != "") command += ",";
            command += "RawBase64 = " + DbHelper.ParamChar + "paramRawBase64";
        }

        if (ehrAmendment.DateTRequest != oldEhrAmendment.DateTRequest)
        {
            if (command != "") command += ",";
            command += "DateTRequest = " + SOut.DateT(ehrAmendment.DateTRequest) + "";
        }

        if (ehrAmendment.DateTAcceptDeny != oldEhrAmendment.DateTAcceptDeny)
        {
            if (command != "") command += ",";
            command += "DateTAcceptDeny = " + SOut.DateT(ehrAmendment.DateTAcceptDeny) + "";
        }

        if (ehrAmendment.DateTAppend != oldEhrAmendment.DateTAppend)
        {
            if (command != "") command += ",";
            command += "DateTAppend = " + SOut.DateT(ehrAmendment.DateTAppend) + "";
        }

        if (command == "") return false;
        if (ehrAmendment.Description == null) ehrAmendment.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(ehrAmendment.Description));
        if (ehrAmendment.SourceName == null) ehrAmendment.SourceName = "";
        var paramSourceName = new OdSqlParameter("paramSourceName", OdDbType.Text, SOut.StringParam(ehrAmendment.SourceName));
        if (ehrAmendment.RawBase64 == null) ehrAmendment.RawBase64 = "";
        var paramRawBase64 = new OdSqlParameter("paramRawBase64", OdDbType.Text, SOut.StringParam(ehrAmendment.RawBase64));
        command = "UPDATE ehramendment SET " + command
                                             + " WHERE EhrAmendmentNum = " + SOut.Long(ehrAmendment.EhrAmendmentNum);
        Db.NonQ(command, paramDescription, paramSourceName, paramRawBase64);
        return true;
    }

    public static bool UpdateComparison(EhrAmendment ehrAmendment, EhrAmendment oldEhrAmendment)
    {
        if (ehrAmendment.PatNum != oldEhrAmendment.PatNum) return true;
        if (ehrAmendment.IsAccepted != oldEhrAmendment.IsAccepted) return true;
        if (ehrAmendment.Description != oldEhrAmendment.Description) return true;
        if (ehrAmendment.Source != oldEhrAmendment.Source) return true;
        if (ehrAmendment.SourceName != oldEhrAmendment.SourceName) return true;
        if (ehrAmendment.FileName != oldEhrAmendment.FileName) return true;
        if (ehrAmendment.RawBase64 != oldEhrAmendment.RawBase64) return true;
        if (ehrAmendment.DateTRequest != oldEhrAmendment.DateTRequest) return true;
        if (ehrAmendment.DateTAcceptDeny != oldEhrAmendment.DateTAcceptDeny) return true;
        if (ehrAmendment.DateTAppend != oldEhrAmendment.DateTAppend) return true;
        return false;
    }

    public static void Delete(long ehrAmendmentNum)
    {
        var command = "DELETE FROM ehramendment "
                      + "WHERE EhrAmendmentNum = " + SOut.Long(ehrAmendmentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrAmendmentNums)
    {
        if (listEhrAmendmentNums == null || listEhrAmendmentNums.Count == 0) return;
        var command = "DELETE FROM ehramendment "
                      + "WHERE EhrAmendmentNum IN(" + string.Join(",", listEhrAmendmentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}