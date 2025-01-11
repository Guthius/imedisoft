#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EServiceSignalCrud
{
    public static EServiceSignal SelectOne(long eServiceSignalNum)
    {
        var command = "SELECT * FROM eservicesignal "
                      + "WHERE EServiceSignalNum = " + SOut.Long(eServiceSignalNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EServiceSignal SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EServiceSignal> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EServiceSignal> TableToList(DataTable table)
    {
        var retVal = new List<EServiceSignal>();
        EServiceSignal eServiceSignal;
        foreach (DataRow row in table.Rows)
        {
            eServiceSignal = new EServiceSignal();
            eServiceSignal.EServiceSignalNum = SIn.Long(row["EServiceSignalNum"].ToString());
            eServiceSignal.ServiceCode = SIn.Int(row["ServiceCode"].ToString());
            eServiceSignal.ReasonCategory = SIn.Int(row["ReasonCategory"].ToString());
            eServiceSignal.ReasonCode = SIn.Int(row["ReasonCode"].ToString());
            eServiceSignal.Severity = (eServiceSignalSeverity) SIn.Int(row["Severity"].ToString());
            eServiceSignal.Description = SIn.String(row["Description"].ToString());
            eServiceSignal.SigDateTime = SIn.DateTime(row["SigDateTime"].ToString());
            eServiceSignal.Tag = SIn.String(row["Tag"].ToString());
            eServiceSignal.IsProcessed = SIn.Bool(row["IsProcessed"].ToString());
            retVal.Add(eServiceSignal);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EServiceSignal> listEServiceSignals, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EServiceSignal";
        var table = new DataTable(tableName);
        table.Columns.Add("EServiceSignalNum");
        table.Columns.Add("ServiceCode");
        table.Columns.Add("ReasonCategory");
        table.Columns.Add("ReasonCode");
        table.Columns.Add("Severity");
        table.Columns.Add("Description");
        table.Columns.Add("SigDateTime");
        table.Columns.Add("Tag");
        table.Columns.Add("IsProcessed");
        foreach (var eServiceSignal in listEServiceSignals)
            table.Rows.Add(SOut.Long(eServiceSignal.EServiceSignalNum), SOut.Int(eServiceSignal.ServiceCode), SOut.Int(eServiceSignal.ReasonCategory), SOut.Int(eServiceSignal.ReasonCode), SOut.Int((int) eServiceSignal.Severity), eServiceSignal.Description, SOut.DateT(eServiceSignal.SigDateTime, false), eServiceSignal.Tag, SOut.Bool(eServiceSignal.IsProcessed));
        return table;
    }

    public static long Insert(EServiceSignal eServiceSignal)
    {
        return Insert(eServiceSignal, false);
    }

    public static long Insert(EServiceSignal eServiceSignal, bool useExistingPK)
    {
        var command = "INSERT INTO eservicesignal (";

        command += "ServiceCode,ReasonCategory,ReasonCode,Severity,Description,SigDateTime,Tag,IsProcessed) VALUES(";

        command +=
            SOut.Int(eServiceSignal.ServiceCode) + ","
                                                 + SOut.Int(eServiceSignal.ReasonCategory) + ","
                                                 + SOut.Int(eServiceSignal.ReasonCode) + ","
                                                 + SOut.Int((int) eServiceSignal.Severity) + ","
                                                 + DbHelper.ParamChar + "paramDescription,"
                                                 + SOut.DateT(eServiceSignal.SigDateTime) + ","
                                                 + DbHelper.ParamChar + "paramTag,"
                                                 + SOut.Bool(eServiceSignal.IsProcessed) + ")";
        if (eServiceSignal.Description == null) eServiceSignal.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(eServiceSignal.Description));
        if (eServiceSignal.Tag == null) eServiceSignal.Tag = "";
        var paramTag = new OdSqlParameter("paramTag", OdDbType.Text, SOut.StringParam(eServiceSignal.Tag));
        {
            eServiceSignal.EServiceSignalNum = Db.NonQ(command, true, "EServiceSignalNum", "eServiceSignal", paramDescription, paramTag);
        }
        return eServiceSignal.EServiceSignalNum;
    }

    public static long InsertNoCache(EServiceSignal eServiceSignal)
    {
        return InsertNoCache(eServiceSignal, false);
    }

    public static long InsertNoCache(EServiceSignal eServiceSignal, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eservicesignal (";
        if (isRandomKeys || useExistingPK) command += "EServiceSignalNum,";
        command += "ServiceCode,ReasonCategory,ReasonCode,Severity,Description,SigDateTime,Tag,IsProcessed) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eServiceSignal.EServiceSignalNum) + ",";
        command +=
            SOut.Int(eServiceSignal.ServiceCode) + ","
                                                 + SOut.Int(eServiceSignal.ReasonCategory) + ","
                                                 + SOut.Int(eServiceSignal.ReasonCode) + ","
                                                 + SOut.Int((int) eServiceSignal.Severity) + ","
                                                 + DbHelper.ParamChar + "paramDescription,"
                                                 + SOut.DateT(eServiceSignal.SigDateTime) + ","
                                                 + DbHelper.ParamChar + "paramTag,"
                                                 + SOut.Bool(eServiceSignal.IsProcessed) + ")";
        if (eServiceSignal.Description == null) eServiceSignal.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(eServiceSignal.Description));
        if (eServiceSignal.Tag == null) eServiceSignal.Tag = "";
        var paramTag = new OdSqlParameter("paramTag", OdDbType.Text, SOut.StringParam(eServiceSignal.Tag));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription, paramTag);
        else
            eServiceSignal.EServiceSignalNum = Db.NonQ(command, true, "EServiceSignalNum", "eServiceSignal", paramDescription, paramTag);
        return eServiceSignal.EServiceSignalNum;
    }

    public static void Update(EServiceSignal eServiceSignal)
    {
        var command = "UPDATE eservicesignal SET "
                      + "ServiceCode      =  " + SOut.Int(eServiceSignal.ServiceCode) + ", "
                      + "ReasonCategory   =  " + SOut.Int(eServiceSignal.ReasonCategory) + ", "
                      + "ReasonCode       =  " + SOut.Int(eServiceSignal.ReasonCode) + ", "
                      + "Severity         =  " + SOut.Int((int) eServiceSignal.Severity) + ", "
                      + "Description      =  " + DbHelper.ParamChar + "paramDescription, "
                      + "SigDateTime      =  " + SOut.DateT(eServiceSignal.SigDateTime) + ", "
                      + "Tag              =  " + DbHelper.ParamChar + "paramTag, "
                      + "IsProcessed      =  " + SOut.Bool(eServiceSignal.IsProcessed) + " "
                      + "WHERE EServiceSignalNum = " + SOut.Long(eServiceSignal.EServiceSignalNum);
        if (eServiceSignal.Description == null) eServiceSignal.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(eServiceSignal.Description));
        if (eServiceSignal.Tag == null) eServiceSignal.Tag = "";
        var paramTag = new OdSqlParameter("paramTag", OdDbType.Text, SOut.StringParam(eServiceSignal.Tag));
        Db.NonQ(command, paramDescription, paramTag);
    }

    public static bool Update(EServiceSignal eServiceSignal, EServiceSignal oldEServiceSignal)
    {
        var command = "";
        if (eServiceSignal.ServiceCode != oldEServiceSignal.ServiceCode)
        {
            if (command != "") command += ",";
            command += "ServiceCode = " + SOut.Int(eServiceSignal.ServiceCode) + "";
        }

        if (eServiceSignal.ReasonCategory != oldEServiceSignal.ReasonCategory)
        {
            if (command != "") command += ",";
            command += "ReasonCategory = " + SOut.Int(eServiceSignal.ReasonCategory) + "";
        }

        if (eServiceSignal.ReasonCode != oldEServiceSignal.ReasonCode)
        {
            if (command != "") command += ",";
            command += "ReasonCode = " + SOut.Int(eServiceSignal.ReasonCode) + "";
        }

        if (eServiceSignal.Severity != oldEServiceSignal.Severity)
        {
            if (command != "") command += ",";
            command += "Severity = " + SOut.Int((int) eServiceSignal.Severity) + "";
        }

        if (eServiceSignal.Description != oldEServiceSignal.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (eServiceSignal.SigDateTime != oldEServiceSignal.SigDateTime)
        {
            if (command != "") command += ",";
            command += "SigDateTime = " + SOut.DateT(eServiceSignal.SigDateTime) + "";
        }

        if (eServiceSignal.Tag != oldEServiceSignal.Tag)
        {
            if (command != "") command += ",";
            command += "Tag = " + DbHelper.ParamChar + "paramTag";
        }

        if (eServiceSignal.IsProcessed != oldEServiceSignal.IsProcessed)
        {
            if (command != "") command += ",";
            command += "IsProcessed = " + SOut.Bool(eServiceSignal.IsProcessed) + "";
        }

        if (command == "") return false;
        if (eServiceSignal.Description == null) eServiceSignal.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(eServiceSignal.Description));
        if (eServiceSignal.Tag == null) eServiceSignal.Tag = "";
        var paramTag = new OdSqlParameter("paramTag", OdDbType.Text, SOut.StringParam(eServiceSignal.Tag));
        command = "UPDATE eservicesignal SET " + command
                                               + " WHERE EServiceSignalNum = " + SOut.Long(eServiceSignal.EServiceSignalNum);
        Db.NonQ(command, paramDescription, paramTag);
        return true;
    }

    public static bool UpdateComparison(EServiceSignal eServiceSignal, EServiceSignal oldEServiceSignal)
    {
        if (eServiceSignal.ServiceCode != oldEServiceSignal.ServiceCode) return true;
        if (eServiceSignal.ReasonCategory != oldEServiceSignal.ReasonCategory) return true;
        if (eServiceSignal.ReasonCode != oldEServiceSignal.ReasonCode) return true;
        if (eServiceSignal.Severity != oldEServiceSignal.Severity) return true;
        if (eServiceSignal.Description != oldEServiceSignal.Description) return true;
        if (eServiceSignal.SigDateTime != oldEServiceSignal.SigDateTime) return true;
        if (eServiceSignal.Tag != oldEServiceSignal.Tag) return true;
        if (eServiceSignal.IsProcessed != oldEServiceSignal.IsProcessed) return true;
        return false;
    }

    public static void Delete(long eServiceSignalNum)
    {
        var command = "DELETE FROM eservicesignal "
                      + "WHERE EServiceSignalNum = " + SOut.Long(eServiceSignalNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEServiceSignalNums)
    {
        if (listEServiceSignalNums == null || listEServiceSignalNums.Count == 0) return;
        var command = "DELETE FROM eservicesignal "
                      + "WHERE EServiceSignalNum IN(" + string.Join(",", listEServiceSignalNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}