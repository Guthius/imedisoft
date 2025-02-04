using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DisplayFieldCrud
{
    public static List<DisplayField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DisplayField> TableToList(DataTable table)
    {
        var retVal = new List<DisplayField>();
        foreach (DataRow row in table.Rows)
        {
            var displayField = new DisplayField
            {
                DisplayFieldNum = SIn.Long(row["DisplayFieldNum"].ToString()),
                InternalName = SIn.String(row["InternalName"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                ColumnWidth = SIn.Int(row["ColumnWidth"].ToString()),
                Category = (DisplayFieldCategory) SIn.Int(row["Category"].ToString()),
                ChartViewNum = SIn.Long(row["ChartViewNum"].ToString()),
                PickList = SIn.String(row["PickList"].ToString()),
                DescriptionOverride = SIn.String(row["DescriptionOverride"].ToString())
            };
            retVal.Add(displayField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DisplayField> listDisplayFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DisplayField";
        var table = new DataTable(tableName);
        table.Columns.Add("DisplayFieldNum");
        table.Columns.Add("InternalName");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Description");
        table.Columns.Add("ColumnWidth");
        table.Columns.Add("Category");
        table.Columns.Add("ChartViewNum");
        table.Columns.Add("PickList");
        table.Columns.Add("DescriptionOverride");
        foreach (var displayField in listDisplayFields)
            table.Rows.Add(SOut.Long(displayField.DisplayFieldNum), displayField.InternalName, SOut.Int(displayField.ItemOrder), displayField.Description, SOut.Int(displayField.ColumnWidth), SOut.Int((int) displayField.Category), SOut.Long(displayField.ChartViewNum), displayField.PickList, displayField.DescriptionOverride);
        return table;
    }

    public static void Insert(DisplayField displayField)
    {
        var command = "INSERT INTO displayfield (";

        command += "InternalName,ItemOrder,Description,ColumnWidth,Category,ChartViewNum,PickList,DescriptionOverride) VALUES(";

        command +=
            "'" + SOut.String(displayField.InternalName) + "',"
            + SOut.Int(displayField.ItemOrder) + ","
            + "'" + SOut.String(displayField.Description) + "',"
            + SOut.Int(displayField.ColumnWidth) + ","
            + SOut.Int((int) displayField.Category) + ","
            + SOut.Long(displayField.ChartViewNum) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + "'" + SOut.String(displayField.DescriptionOverride) + "')";
        if (displayField.PickList == null) displayField.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", SOut.StringParam(displayField.PickList));
        {
            displayField.DisplayFieldNum = Db.NonQ(command, true, "DisplayFieldNum", "displayField", paramPickList);
        }
    }

    public static void Update(DisplayField displayField)
    {
        var command = "UPDATE displayfield SET "
                      + "InternalName       = '" + SOut.String(displayField.InternalName) + "', "
                      + "ItemOrder          =  " + SOut.Int(displayField.ItemOrder) + ", "
                      + "Description        = '" + SOut.String(displayField.Description) + "', "
                      + "ColumnWidth        =  " + SOut.Int(displayField.ColumnWidth) + ", "
                      + "Category           =  " + SOut.Int((int) displayField.Category) + ", "
                      + "ChartViewNum       =  " + SOut.Long(displayField.ChartViewNum) + ", "
                      + "PickList           =  " + DbHelper.ParamChar + "paramPickList, "
                      + "DescriptionOverride= '" + SOut.String(displayField.DescriptionOverride) + "' "
                      + "WHERE DisplayFieldNum = " + SOut.Long(displayField.DisplayFieldNum);
        if (displayField.PickList == null) displayField.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", SOut.StringParam(displayField.PickList));
        Db.NonQ(command, paramPickList);
    }
}