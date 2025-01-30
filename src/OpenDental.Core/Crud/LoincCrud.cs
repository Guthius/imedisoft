using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LoincCrud
{
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

    public static void Insert(Loinc loinc)
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
        var paramExternalCopyrightNotice = new OdSqlParameter("paramExternalCopyrightNotice", SOut.StringParam(loinc.ExternalCopyrightNotice));
        {
            loinc.LoincNum = Db.NonQ(command, true, "LoincNum", "loinc", paramExternalCopyrightNotice);
        }
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
        var paramExternalCopyrightNotice = new OdSqlParameter("paramExternalCopyrightNotice", SOut.StringParam(loinc.ExternalCopyrightNotice));
        Db.NonQ(command, paramExternalCopyrightNotice);
    }
}