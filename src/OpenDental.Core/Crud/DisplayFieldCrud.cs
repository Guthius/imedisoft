#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DisplayFieldCrud
{
    public static DisplayField SelectOne(long displayFieldNum)
    {
        var command = "SELECT * FROM displayfield "
                      + "WHERE DisplayFieldNum = " + SOut.Long(displayFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DisplayField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DisplayField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DisplayField> TableToList(DataTable table)
    {
        var retVal = new List<DisplayField>();
        DisplayField displayField;
        foreach (DataRow row in table.Rows)
        {
            displayField = new DisplayField();
            displayField.DisplayFieldNum = SIn.Long(row["DisplayFieldNum"].ToString());
            displayField.InternalName = SIn.String(row["InternalName"].ToString());
            displayField.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            displayField.Description = SIn.String(row["Description"].ToString());
            displayField.ColumnWidth = SIn.Int(row["ColumnWidth"].ToString());
            displayField.Category = (DisplayFieldCategory) SIn.Int(row["Category"].ToString());
            displayField.ChartViewNum = SIn.Long(row["ChartViewNum"].ToString());
            displayField.PickList = SIn.String(row["PickList"].ToString());
            displayField.DescriptionOverride = SIn.String(row["DescriptionOverride"].ToString());
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

    public static long Insert(DisplayField displayField)
    {
        return Insert(displayField, false);
    }

    public static long Insert(DisplayField displayField, bool useExistingPK)
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
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(displayField.PickList));
        {
            displayField.DisplayFieldNum = Db.NonQ(command, true, "DisplayFieldNum", "displayField", paramPickList);
        }
        return displayField.DisplayFieldNum;
    }

    public static long InsertNoCache(DisplayField displayField)
    {
        return InsertNoCache(displayField, false);
    }

    public static long InsertNoCache(DisplayField displayField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO displayfield (";
        if (isRandomKeys || useExistingPK) command += "DisplayFieldNum,";
        command += "InternalName,ItemOrder,Description,ColumnWidth,Category,ChartViewNum,PickList,DescriptionOverride) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(displayField.DisplayFieldNum) + ",";
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
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(displayField.PickList));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPickList);
        else
            displayField.DisplayFieldNum = Db.NonQ(command, true, "DisplayFieldNum", "displayField", paramPickList);
        return displayField.DisplayFieldNum;
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
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(displayField.PickList));
        Db.NonQ(command, paramPickList);
    }

    public static bool Update(DisplayField displayField, DisplayField oldDisplayField)
    {
        var command = "";
        if (displayField.InternalName != oldDisplayField.InternalName)
        {
            if (command != "") command += ",";
            command += "InternalName = '" + SOut.String(displayField.InternalName) + "'";
        }

        if (displayField.ItemOrder != oldDisplayField.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(displayField.ItemOrder) + "";
        }

        if (displayField.Description != oldDisplayField.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(displayField.Description) + "'";
        }

        if (displayField.ColumnWidth != oldDisplayField.ColumnWidth)
        {
            if (command != "") command += ",";
            command += "ColumnWidth = " + SOut.Int(displayField.ColumnWidth) + "";
        }

        if (displayField.Category != oldDisplayField.Category)
        {
            if (command != "") command += ",";
            command += "Category = " + SOut.Int((int) displayField.Category) + "";
        }

        if (displayField.ChartViewNum != oldDisplayField.ChartViewNum)
        {
            if (command != "") command += ",";
            command += "ChartViewNum = " + SOut.Long(displayField.ChartViewNum) + "";
        }

        if (displayField.PickList != oldDisplayField.PickList)
        {
            if (command != "") command += ",";
            command += "PickList = " + DbHelper.ParamChar + "paramPickList";
        }

        if (displayField.DescriptionOverride != oldDisplayField.DescriptionOverride)
        {
            if (command != "") command += ",";
            command += "DescriptionOverride = '" + SOut.String(displayField.DescriptionOverride) + "'";
        }

        if (command == "") return false;
        if (displayField.PickList == null) displayField.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(displayField.PickList));
        command = "UPDATE displayfield SET " + command
                                             + " WHERE DisplayFieldNum = " + SOut.Long(displayField.DisplayFieldNum);
        Db.NonQ(command, paramPickList);
        return true;
    }

    public static bool UpdateComparison(DisplayField displayField, DisplayField oldDisplayField)
    {
        if (displayField.InternalName != oldDisplayField.InternalName) return true;
        if (displayField.ItemOrder != oldDisplayField.ItemOrder) return true;
        if (displayField.Description != oldDisplayField.Description) return true;
        if (displayField.ColumnWidth != oldDisplayField.ColumnWidth) return true;
        if (displayField.Category != oldDisplayField.Category) return true;
        if (displayField.ChartViewNum != oldDisplayField.ChartViewNum) return true;
        if (displayField.PickList != oldDisplayField.PickList) return true;
        if (displayField.DescriptionOverride != oldDisplayField.DescriptionOverride) return true;
        return false;
    }

    public static void Delete(long displayFieldNum)
    {
        var command = "DELETE FROM displayfield "
                      + "WHERE DisplayFieldNum = " + SOut.Long(displayFieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDisplayFieldNums)
    {
        if (listDisplayFieldNums == null || listDisplayFieldNums.Count == 0) return;
        var command = "DELETE FROM displayfield "
                      + "WHERE DisplayFieldNum IN(" + string.Join(",", listDisplayFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}