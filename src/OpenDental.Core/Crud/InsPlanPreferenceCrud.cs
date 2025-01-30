using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class InsPlanPreferenceCrud
{
    public static InsPlanPreference SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<InsPlanPreference> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<InsPlanPreference> TableToList(DataTable table)
    {
        var retVal = new List<InsPlanPreference>();
        InsPlanPreference insPlanPreference;
        foreach (DataRow row in table.Rows)
        {
            insPlanPreference = new InsPlanPreference();
            insPlanPreference.InsPlanPrefNum = SIn.Long(row["InsPlanPrefNum"].ToString());
            insPlanPreference.PlanNum = SIn.Long(row["PlanNum"].ToString());
            insPlanPreference.FKey = SIn.Long(row["FKey"].ToString());
            insPlanPreference.FKeyType = (InsPlanPrefFKeyType) SIn.Int(row["FKeyType"].ToString());
            insPlanPreference.ValueString = SIn.String(row["ValueString"].ToString());
            retVal.Add(insPlanPreference);
        }

        return retVal;
    }

    public static void Insert(InsPlanPreference insPlanPreference)
    {
        var command = "INSERT INTO insplanpreference (";

        command += "PlanNum,FKey,FKeyType,ValueString) VALUES(";

        command +=
            SOut.Long(insPlanPreference.PlanNum) + ","
                                                 + SOut.Long(insPlanPreference.FKey) + ","
                                                 + SOut.Int((int) insPlanPreference.FKeyType) + ","
                                                 + DbHelper.ParamChar + "paramValueString)";
        if (insPlanPreference.ValueString == null) insPlanPreference.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", SOut.StringParam(insPlanPreference.ValueString));
        {
            insPlanPreference.InsPlanPrefNum = Db.NonQ(command, true, "InsPlanPrefNum", "insPlanPreference", paramValueString);
        }
    }

    public static void Update(InsPlanPreference insPlanPreference, InsPlanPreference oldInsPlanPreference)
    {
        var command = "";
        if (insPlanPreference.PlanNum != oldInsPlanPreference.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(insPlanPreference.PlanNum) + "";
        }

        if (insPlanPreference.FKey != oldInsPlanPreference.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(insPlanPreference.FKey) + "";
        }

        if (insPlanPreference.FKeyType != oldInsPlanPreference.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = " + SOut.Int((int) insPlanPreference.FKeyType) + "";
        }

        if (insPlanPreference.ValueString != oldInsPlanPreference.ValueString)
        {
            if (command != "") command += ",";
            command += "ValueString = " + DbHelper.ParamChar + "paramValueString";
        }

        if (command == "") return;
        if (insPlanPreference.ValueString == null) insPlanPreference.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", SOut.StringParam(insPlanPreference.ValueString));
        command = "UPDATE insplanpreference SET " + command
                                                  + " WHERE InsPlanPrefNum = " + SOut.Long(insPlanPreference.InsPlanPrefNum);
        Db.NonQ(command, paramValueString);
    }
}