#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LoincCrud
{
    public static Loinc SelectOne(long loincNum)
    {
        var command = "SELECT * FROM loinc "
                      + "WHERE LoincNum = " + SOut.Long(loincNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Loinc SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Loinc> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Loinc> TableToList(DataTable table)
    {
        var retVal = new List<Loinc>();
        Loinc loinc;
        foreach (DataRow row in table.Rows)
        {
            loinc = new Loinc();
            loinc.LoincNum = SIn.Long(row["LoincNum"].ToString());
            loinc.LoincCode = SIn.String(row["LoincCode"].ToString());
            loinc.Component = SIn.String(row["Component"].ToString());
            loinc.PropertyObserved = SIn.String(row["PropertyObserved"].ToString());
            loinc.TimeAspct = SIn.String(row["TimeAspct"].ToString());
            loinc.SystemMeasured = SIn.String(row["SystemMeasured"].ToString());
            loinc.ScaleType = SIn.String(row["ScaleType"].ToString());
            loinc.MethodType = SIn.String(row["MethodType"].ToString());
            loinc.StatusOfCode = SIn.String(row["StatusOfCode"].ToString());
            loinc.NameShort = SIn.String(row["NameShort"].ToString());
            loinc.ClassType = SIn.String(row["ClassType"].ToString());
            loinc.UnitsRequired = SIn.Bool(row["UnitsRequired"].ToString());
            loinc.OrderObs = SIn.String(row["OrderObs"].ToString());
            loinc.HL7FieldSubfieldID = SIn.String(row["HL7FieldSubfieldID"].ToString());
            loinc.ExternalCopyrightNotice = SIn.String(row["ExternalCopyrightNotice"].ToString());
            loinc.NameLongCommon = SIn.String(row["NameLongCommon"].ToString());
            loinc.UnitsUCUM = SIn.String(row["UnitsUCUM"].ToString());
            loinc.RankCommonTests = SIn.Int(row["RankCommonTests"].ToString());
            loinc.RankCommonOrders = SIn.Int(row["RankCommonOrders"].ToString());
            retVal.Add(loinc);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Loinc> listLoincs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Loinc";
        var table = new DataTable(tableName);
        table.Columns.Add("LoincNum");
        table.Columns.Add("LoincCode");
        table.Columns.Add("Component");
        table.Columns.Add("PropertyObserved");
        table.Columns.Add("TimeAspct");
        table.Columns.Add("SystemMeasured");
        table.Columns.Add("ScaleType");
        table.Columns.Add("MethodType");
        table.Columns.Add("StatusOfCode");
        table.Columns.Add("NameShort");
        table.Columns.Add("ClassType");
        table.Columns.Add("UnitsRequired");
        table.Columns.Add("OrderObs");
        table.Columns.Add("HL7FieldSubfieldID");
        table.Columns.Add("ExternalCopyrightNotice");
        table.Columns.Add("NameLongCommon");
        table.Columns.Add("UnitsUCUM");
        table.Columns.Add("RankCommonTests");
        table.Columns.Add("RankCommonOrders");
        foreach (var loinc in listLoincs)
            table.Rows.Add(SOut.Long(loinc.LoincNum), loinc.LoincCode, loinc.Component, loinc.PropertyObserved, loinc.TimeAspct, loinc.SystemMeasured, loinc.ScaleType, loinc.MethodType, loinc.StatusOfCode, loinc.NameShort, loinc.ClassType, SOut.Bool(loinc.UnitsRequired), loinc.OrderObs, loinc.HL7FieldSubfieldID, loinc.ExternalCopyrightNotice, loinc.NameLongCommon, loinc.UnitsUCUM, SOut.Int(loinc.RankCommonTests), SOut.Int(loinc.RankCommonOrders));
        return table;
    }

    public static long Insert(Loinc loinc)
    {
        return Insert(loinc, false);
    }

    public static long Insert(Loinc loinc, bool useExistingPK)
    {
        var command = "INSERT INTO loinc (";

        command += "LoincCode,Component,PropertyObserved,TimeAspct,SystemMeasured,ScaleType,MethodType,StatusOfCode,NameShort,ClassType,UnitsRequired,OrderObs,HL7FieldSubfieldID,ExternalCopyrightNotice,NameLongCommon,UnitsUCUM,RankCommonTests,RankCommonOrders) VALUES(";

        command +=
            "'" + SOut.String(loinc.LoincCode) + "',"
            + "'" + SOut.String(loinc.Component) + "',"
            + "'" + SOut.String(loinc.PropertyObserved) + "',"
            + "'" + SOut.String(loinc.TimeAspct) + "',"
            + "'" + SOut.String(loinc.SystemMeasured) + "',"
            + "'" + SOut.String(loinc.ScaleType) + "',"
            + "'" + SOut.String(loinc.MethodType) + "',"
            + "'" + SOut.String(loinc.StatusOfCode) + "',"
            + "'" + SOut.String(loinc.NameShort) + "',"
            + "'" + SOut.String(loinc.ClassType) + "',"
            + SOut.Bool(loinc.UnitsRequired) + ","
            + "'" + SOut.String(loinc.OrderObs) + "',"
            + "'" + SOut.String(loinc.HL7FieldSubfieldID) + "',"
            + DbHelper.ParamChar + "paramExternalCopyrightNotice,"
            + "'" + SOut.String(loinc.NameLongCommon) + "',"
            + "'" + SOut.String(loinc.UnitsUCUM) + "',"
            + SOut.Int(loinc.RankCommonTests) + ","
            + SOut.Int(loinc.RankCommonOrders) + ")";
        if (loinc.ExternalCopyrightNotice == null) loinc.ExternalCopyrightNotice = "";
        var paramExternalCopyrightNotice = new OdSqlParameter("paramExternalCopyrightNotice", OdDbType.Text, SOut.StringParam(loinc.ExternalCopyrightNotice));
        {
            loinc.LoincNum = Db.NonQ(command, true, "LoincNum", "loinc", paramExternalCopyrightNotice);
        }
        return loinc.LoincNum;
    }

    public static long InsertNoCache(Loinc loinc)
    {
        return InsertNoCache(loinc, false);
    }

    public static long InsertNoCache(Loinc loinc, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO loinc (";
        if (isRandomKeys || useExistingPK) command += "LoincNum,";
        command += "LoincCode,Component,PropertyObserved,TimeAspct,SystemMeasured,ScaleType,MethodType,StatusOfCode,NameShort,ClassType,UnitsRequired,OrderObs,HL7FieldSubfieldID,ExternalCopyrightNotice,NameLongCommon,UnitsUCUM,RankCommonTests,RankCommonOrders) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(loinc.LoincNum) + ",";
        command +=
            "'" + SOut.String(loinc.LoincCode) + "',"
            + "'" + SOut.String(loinc.Component) + "',"
            + "'" + SOut.String(loinc.PropertyObserved) + "',"
            + "'" + SOut.String(loinc.TimeAspct) + "',"
            + "'" + SOut.String(loinc.SystemMeasured) + "',"
            + "'" + SOut.String(loinc.ScaleType) + "',"
            + "'" + SOut.String(loinc.MethodType) + "',"
            + "'" + SOut.String(loinc.StatusOfCode) + "',"
            + "'" + SOut.String(loinc.NameShort) + "',"
            + "'" + SOut.String(loinc.ClassType) + "',"
            + SOut.Bool(loinc.UnitsRequired) + ","
            + "'" + SOut.String(loinc.OrderObs) + "',"
            + "'" + SOut.String(loinc.HL7FieldSubfieldID) + "',"
            + DbHelper.ParamChar + "paramExternalCopyrightNotice,"
            + "'" + SOut.String(loinc.NameLongCommon) + "',"
            + "'" + SOut.String(loinc.UnitsUCUM) + "',"
            + SOut.Int(loinc.RankCommonTests) + ","
            + SOut.Int(loinc.RankCommonOrders) + ")";
        if (loinc.ExternalCopyrightNotice == null) loinc.ExternalCopyrightNotice = "";
        var paramExternalCopyrightNotice = new OdSqlParameter("paramExternalCopyrightNotice", OdDbType.Text, SOut.StringParam(loinc.ExternalCopyrightNotice));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramExternalCopyrightNotice);
        else
            loinc.LoincNum = Db.NonQ(command, true, "LoincNum", "loinc", paramExternalCopyrightNotice);
        return loinc.LoincNum;
    }

    public static void Update(Loinc loinc)
    {
        var command = "UPDATE loinc SET "
                      + "LoincCode              = '" + SOut.String(loinc.LoincCode) + "', "
                      + "Component              = '" + SOut.String(loinc.Component) + "', "
                      + "PropertyObserved       = '" + SOut.String(loinc.PropertyObserved) + "', "
                      + "TimeAspct              = '" + SOut.String(loinc.TimeAspct) + "', "
                      + "SystemMeasured         = '" + SOut.String(loinc.SystemMeasured) + "', "
                      + "ScaleType              = '" + SOut.String(loinc.ScaleType) + "', "
                      + "MethodType             = '" + SOut.String(loinc.MethodType) + "', "
                      + "StatusOfCode           = '" + SOut.String(loinc.StatusOfCode) + "', "
                      + "NameShort              = '" + SOut.String(loinc.NameShort) + "', "
                      + "ClassType              = '" + SOut.String(loinc.ClassType) + "', "
                      + "UnitsRequired          =  " + SOut.Bool(loinc.UnitsRequired) + ", "
                      + "OrderObs               = '" + SOut.String(loinc.OrderObs) + "', "
                      + "HL7FieldSubfieldID     = '" + SOut.String(loinc.HL7FieldSubfieldID) + "', "
                      + "ExternalCopyrightNotice=  " + DbHelper.ParamChar + "paramExternalCopyrightNotice, "
                      + "NameLongCommon         = '" + SOut.String(loinc.NameLongCommon) + "', "
                      + "UnitsUCUM              = '" + SOut.String(loinc.UnitsUCUM) + "', "
                      + "RankCommonTests        =  " + SOut.Int(loinc.RankCommonTests) + ", "
                      + "RankCommonOrders       =  " + SOut.Int(loinc.RankCommonOrders) + " "
                      + "WHERE LoincNum = " + SOut.Long(loinc.LoincNum);
        if (loinc.ExternalCopyrightNotice == null) loinc.ExternalCopyrightNotice = "";
        var paramExternalCopyrightNotice = new OdSqlParameter("paramExternalCopyrightNotice", OdDbType.Text, SOut.StringParam(loinc.ExternalCopyrightNotice));
        Db.NonQ(command, paramExternalCopyrightNotice);
    }

    public static bool Update(Loinc loinc, Loinc oldLoinc)
    {
        var command = "";
        if (loinc.LoincCode != oldLoinc.LoincCode)
        {
            if (command != "") command += ",";
            command += "LoincCode = '" + SOut.String(loinc.LoincCode) + "'";
        }

        if (loinc.Component != oldLoinc.Component)
        {
            if (command != "") command += ",";
            command += "Component = '" + SOut.String(loinc.Component) + "'";
        }

        if (loinc.PropertyObserved != oldLoinc.PropertyObserved)
        {
            if (command != "") command += ",";
            command += "PropertyObserved = '" + SOut.String(loinc.PropertyObserved) + "'";
        }

        if (loinc.TimeAspct != oldLoinc.TimeAspct)
        {
            if (command != "") command += ",";
            command += "TimeAspct = '" + SOut.String(loinc.TimeAspct) + "'";
        }

        if (loinc.SystemMeasured != oldLoinc.SystemMeasured)
        {
            if (command != "") command += ",";
            command += "SystemMeasured = '" + SOut.String(loinc.SystemMeasured) + "'";
        }

        if (loinc.ScaleType != oldLoinc.ScaleType)
        {
            if (command != "") command += ",";
            command += "ScaleType = '" + SOut.String(loinc.ScaleType) + "'";
        }

        if (loinc.MethodType != oldLoinc.MethodType)
        {
            if (command != "") command += ",";
            command += "MethodType = '" + SOut.String(loinc.MethodType) + "'";
        }

        if (loinc.StatusOfCode != oldLoinc.StatusOfCode)
        {
            if (command != "") command += ",";
            command += "StatusOfCode = '" + SOut.String(loinc.StatusOfCode) + "'";
        }

        if (loinc.NameShort != oldLoinc.NameShort)
        {
            if (command != "") command += ",";
            command += "NameShort = '" + SOut.String(loinc.NameShort) + "'";
        }

        if (loinc.ClassType != oldLoinc.ClassType)
        {
            if (command != "") command += ",";
            command += "ClassType = '" + SOut.String(loinc.ClassType) + "'";
        }

        if (loinc.UnitsRequired != oldLoinc.UnitsRequired)
        {
            if (command != "") command += ",";
            command += "UnitsRequired = " + SOut.Bool(loinc.UnitsRequired) + "";
        }

        if (loinc.OrderObs != oldLoinc.OrderObs)
        {
            if (command != "") command += ",";
            command += "OrderObs = '" + SOut.String(loinc.OrderObs) + "'";
        }

        if (loinc.HL7FieldSubfieldID != oldLoinc.HL7FieldSubfieldID)
        {
            if (command != "") command += ",";
            command += "HL7FieldSubfieldID = '" + SOut.String(loinc.HL7FieldSubfieldID) + "'";
        }

        if (loinc.ExternalCopyrightNotice != oldLoinc.ExternalCopyrightNotice)
        {
            if (command != "") command += ",";
            command += "ExternalCopyrightNotice = " + DbHelper.ParamChar + "paramExternalCopyrightNotice";
        }

        if (loinc.NameLongCommon != oldLoinc.NameLongCommon)
        {
            if (command != "") command += ",";
            command += "NameLongCommon = '" + SOut.String(loinc.NameLongCommon) + "'";
        }

        if (loinc.UnitsUCUM != oldLoinc.UnitsUCUM)
        {
            if (command != "") command += ",";
            command += "UnitsUCUM = '" + SOut.String(loinc.UnitsUCUM) + "'";
        }

        if (loinc.RankCommonTests != oldLoinc.RankCommonTests)
        {
            if (command != "") command += ",";
            command += "RankCommonTests = " + SOut.Int(loinc.RankCommonTests) + "";
        }

        if (loinc.RankCommonOrders != oldLoinc.RankCommonOrders)
        {
            if (command != "") command += ",";
            command += "RankCommonOrders = " + SOut.Int(loinc.RankCommonOrders) + "";
        }

        if (command == "") return false;
        if (loinc.ExternalCopyrightNotice == null) loinc.ExternalCopyrightNotice = "";
        var paramExternalCopyrightNotice = new OdSqlParameter("paramExternalCopyrightNotice", OdDbType.Text, SOut.StringParam(loinc.ExternalCopyrightNotice));
        command = "UPDATE loinc SET " + command
                                      + " WHERE LoincNum = " + SOut.Long(loinc.LoincNum);
        Db.NonQ(command, paramExternalCopyrightNotice);
        return true;
    }

    public static bool UpdateComparison(Loinc loinc, Loinc oldLoinc)
    {
        if (loinc.LoincCode != oldLoinc.LoincCode) return true;
        if (loinc.Component != oldLoinc.Component) return true;
        if (loinc.PropertyObserved != oldLoinc.PropertyObserved) return true;
        if (loinc.TimeAspct != oldLoinc.TimeAspct) return true;
        if (loinc.SystemMeasured != oldLoinc.SystemMeasured) return true;
        if (loinc.ScaleType != oldLoinc.ScaleType) return true;
        if (loinc.MethodType != oldLoinc.MethodType) return true;
        if (loinc.StatusOfCode != oldLoinc.StatusOfCode) return true;
        if (loinc.NameShort != oldLoinc.NameShort) return true;
        if (loinc.ClassType != oldLoinc.ClassType) return true;
        if (loinc.UnitsRequired != oldLoinc.UnitsRequired) return true;
        if (loinc.OrderObs != oldLoinc.OrderObs) return true;
        if (loinc.HL7FieldSubfieldID != oldLoinc.HL7FieldSubfieldID) return true;
        if (loinc.ExternalCopyrightNotice != oldLoinc.ExternalCopyrightNotice) return true;
        if (loinc.NameLongCommon != oldLoinc.NameLongCommon) return true;
        if (loinc.UnitsUCUM != oldLoinc.UnitsUCUM) return true;
        if (loinc.RankCommonTests != oldLoinc.RankCommonTests) return true;
        if (loinc.RankCommonOrders != oldLoinc.RankCommonOrders) return true;
        return false;
    }

    public static void Delete(long loincNum)
    {
        var command = "DELETE FROM loinc "
                      + "WHERE LoincNum = " + SOut.Long(loincNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLoincNums)
    {
        if (listLoincNums == null || listLoincNums.Count == 0) return;
        var command = "DELETE FROM loinc "
                      + "WHERE LoincNum IN(" + string.Join(",", listLoincNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}