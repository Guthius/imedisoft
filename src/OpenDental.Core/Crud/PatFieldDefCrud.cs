using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatFieldDefCrud
{
    public static List<PatFieldDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatFieldDef> TableToList(DataTable table)
    {
        var retVal = new List<PatFieldDef>();
        foreach (DataRow row in table.Rows)
        {
            var patFieldDef = new PatFieldDef
            {
                PatFieldDefNum = SIn.Long(row["PatFieldDefNum"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString()),
                FieldType = (PatFieldType) SIn.Int(row["FieldType"].ToString()),
                PickList = SIn.String(row["PickList"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString())
            };
            retVal.Add(patFieldDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatFieldDef> listPatFieldDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatFieldDef";
        var table = new DataTable(tableName);
        table.Columns.Add("PatFieldDefNum");
        table.Columns.Add("FieldName");
        table.Columns.Add("FieldType");
        table.Columns.Add("PickList");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsHidden");
        foreach (var patFieldDef in listPatFieldDefs)
            table.Rows.Add(SOut.Long(patFieldDef.PatFieldDefNum), patFieldDef.FieldName, SOut.Int((int) patFieldDef.FieldType), patFieldDef.PickList, SOut.Int(patFieldDef.ItemOrder), SOut.Bool(patFieldDef.IsHidden));
        return table;
    }

    public static void Insert(PatFieldDef patFieldDef)
    {
        var command = "INSERT INTO patfielddef (";

        command += "FieldName,FieldType,PickList,ItemOrder,IsHidden) VALUES(";

        command +=
            "'" + SOut.String(patFieldDef.FieldName) + "',"
            + SOut.Int((int) patFieldDef.FieldType) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + SOut.Int(patFieldDef.ItemOrder) + ","
            + SOut.Bool(patFieldDef.IsHidden) + ")";
        if (patFieldDef.PickList == null) patFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", SOut.StringParam(patFieldDef.PickList));
        {
            patFieldDef.PatFieldDefNum = Db.NonQ(command, true, "PatFieldDefNum", "patFieldDef", paramPickList);
        }
    }

    public static void Update(PatFieldDef patFieldDef)
    {
        var command = "UPDATE patfielddef SET "
                      + "FieldName     = '" + SOut.String(patFieldDef.FieldName) + "', "
                      + "FieldType     =  " + SOut.Int((int) patFieldDef.FieldType) + ", "
                      + "PickList      =  " + DbHelper.ParamChar + "paramPickList, "
                      + "ItemOrder     =  " + SOut.Int(patFieldDef.ItemOrder) + ", "
                      + "IsHidden      =  " + SOut.Bool(patFieldDef.IsHidden) + " "
                      + "WHERE PatFieldDefNum = " + SOut.Long(patFieldDef.PatFieldDefNum);
        if (patFieldDef.PickList == null) patFieldDef.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", SOut.StringParam(patFieldDef.PickList));
        Db.NonQ(command, paramPickList);
    }
}