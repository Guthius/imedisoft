#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EFormFieldCrud
{
    public static EFormField SelectOne(long eFormFieldNum)
    {
        var command = "SELECT * FROM eformfield "
                      + "WHERE EFormFieldNum = " + SOut.Long(eFormFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EFormField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EFormField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EFormField> TableToList(DataTable table)
    {
        var retVal = new List<EFormField>();
        EFormField eFormField;
        foreach (DataRow row in table.Rows)
        {
            eFormField = new EFormField();
            eFormField.EFormFieldNum = SIn.Long(row["EFormFieldNum"].ToString());
            eFormField.EFormNum = SIn.Long(row["EFormNum"].ToString());
            eFormField.PatNum = SIn.Long(row["PatNum"].ToString());
            eFormField.FieldType = (EnumEFormFieldType) SIn.Int(row["FieldType"].ToString());
            eFormField.DbLink = SIn.String(row["DbLink"].ToString());
            eFormField.ValueLabel = SIn.String(row["ValueLabel"].ToString());
            eFormField.ValueString = SIn.String(row["ValueString"].ToString());
            eFormField.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eFormField.PickListVis = SIn.String(row["PickListVis"].ToString());
            eFormField.PickListDb = SIn.String(row["PickListDb"].ToString());
            eFormField.IsHorizStacking = SIn.Bool(row["IsHorizStacking"].ToString());
            eFormField.IsTextWrap = SIn.Bool(row["IsTextWrap"].ToString());
            eFormField.Width = SIn.Int(row["Width"].ToString());
            eFormField.FontScale = SIn.Int(row["FontScale"].ToString());
            eFormField.IsRequired = SIn.Bool(row["IsRequired"].ToString());
            eFormField.ConditionalParent = SIn.String(row["ConditionalParent"].ToString());
            eFormField.ConditionalValue = SIn.String(row["ConditionalValue"].ToString());
            eFormField.LabelAlign = (EnumEFormLabelAlign) SIn.Int(row["LabelAlign"].ToString());
            eFormField.SpaceBelow = SIn.Int(row["SpaceBelow"].ToString());
            eFormField.ReportableName = SIn.String(row["ReportableName"].ToString());
            eFormField.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            eFormField.Border = (EnumEFormBorder) SIn.Int(row["Border"].ToString());
            eFormField.IsWidthPercentage = SIn.Bool(row["IsWidthPercentage"].ToString());
            eFormField.MinWidth = SIn.Int(row["MinWidth"].ToString());
            eFormField.WidthLabel = SIn.Int(row["WidthLabel"].ToString());
            eFormField.SpaceToRight = SIn.Int(row["SpaceToRight"].ToString());
            retVal.Add(eFormField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EFormField> listEFormFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EFormField";
        var table = new DataTable(tableName);
        table.Columns.Add("EFormFieldNum");
        table.Columns.Add("EFormNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("FieldType");
        table.Columns.Add("DbLink");
        table.Columns.Add("ValueLabel");
        table.Columns.Add("ValueString");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("PickListVis");
        table.Columns.Add("PickListDb");
        table.Columns.Add("IsHorizStacking");
        table.Columns.Add("IsTextWrap");
        table.Columns.Add("Width");
        table.Columns.Add("FontScale");
        table.Columns.Add("IsRequired");
        table.Columns.Add("ConditionalParent");
        table.Columns.Add("ConditionalValue");
        table.Columns.Add("LabelAlign");
        table.Columns.Add("SpaceBelow");
        table.Columns.Add("ReportableName");
        table.Columns.Add("IsLocked");
        table.Columns.Add("Border");
        table.Columns.Add("IsWidthPercentage");
        table.Columns.Add("MinWidth");
        table.Columns.Add("WidthLabel");
        table.Columns.Add("SpaceToRight");
        foreach (var eFormField in listEFormFields)
            table.Rows.Add(SOut.Long(eFormField.EFormFieldNum), SOut.Long(eFormField.EFormNum), SOut.Long(eFormField.PatNum), SOut.Int((int) eFormField.FieldType), eFormField.DbLink, eFormField.ValueLabel, eFormField.ValueString, SOut.Int(eFormField.ItemOrder), eFormField.PickListVis, eFormField.PickListDb, SOut.Bool(eFormField.IsHorizStacking), SOut.Bool(eFormField.IsTextWrap), SOut.Int(eFormField.Width), SOut.Int(eFormField.FontScale), SOut.Bool(eFormField.IsRequired), eFormField.ConditionalParent, eFormField.ConditionalValue, SOut.Int((int) eFormField.LabelAlign), SOut.Int(eFormField.SpaceBelow), eFormField.ReportableName, SOut.Bool(eFormField.IsLocked), SOut.Int((int) eFormField.Border), SOut.Bool(eFormField.IsWidthPercentage), SOut.Int(eFormField.MinWidth), SOut.Int(eFormField.WidthLabel), SOut.Int(eFormField.SpaceToRight));
        return table;
    }

    public static long Insert(EFormField eFormField)
    {
        return Insert(eFormField, false);
    }

    public static long Insert(EFormField eFormField, bool useExistingPK)
    {
        var command = "INSERT INTO eformfield (";

        command += "EFormNum,PatNum,FieldType,DbLink,ValueLabel,ValueString,ItemOrder,PickListVis,PickListDb,IsHorizStacking,IsTextWrap,Width,FontScale,IsRequired,ConditionalParent,ConditionalValue,LabelAlign,SpaceBelow,ReportableName,IsLocked,Border,IsWidthPercentage,MinWidth,WidthLabel,SpaceToRight) VALUES(";

        command +=
            SOut.Long(eFormField.EFormNum) + ","
                                           + SOut.Long(eFormField.PatNum) + ","
                                           + SOut.Int((int) eFormField.FieldType) + ","
                                           + "'" + SOut.String(eFormField.DbLink) + "',"
                                           + DbHelper.ParamChar + "paramValueLabel,"
                                           + DbHelper.ParamChar + "paramValueString,"
                                           + SOut.Int(eFormField.ItemOrder) + ","
                                           + "'" + SOut.String(eFormField.PickListVis) + "',"
                                           + "'" + SOut.String(eFormField.PickListDb) + "',"
                                           + SOut.Bool(eFormField.IsHorizStacking) + ","
                                           + SOut.Bool(eFormField.IsTextWrap) + ","
                                           + SOut.Int(eFormField.Width) + ","
                                           + SOut.Int(eFormField.FontScale) + ","
                                           + SOut.Bool(eFormField.IsRequired) + ","
                                           + "'" + SOut.String(eFormField.ConditionalParent) + "',"
                                           + "'" + SOut.String(eFormField.ConditionalValue) + "',"
                                           + SOut.Int((int) eFormField.LabelAlign) + ","
                                           + SOut.Int(eFormField.SpaceBelow) + ","
                                           + "'" + SOut.String(eFormField.ReportableName) + "',"
                                           + SOut.Bool(eFormField.IsLocked) + ","
                                           + SOut.Int((int) eFormField.Border) + ","
                                           + SOut.Bool(eFormField.IsWidthPercentage) + ","
                                           + SOut.Int(eFormField.MinWidth) + ","
                                           + SOut.Int(eFormField.WidthLabel) + ","
                                           + SOut.Int(eFormField.SpaceToRight) + ")";
        if (eFormField.ValueLabel == null) eFormField.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormField.ValueLabel));
        if (eFormField.ValueString == null) eFormField.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(eFormField.ValueString));
        {
            eFormField.EFormFieldNum = Db.NonQ(command, true, "EFormFieldNum", "eFormField", paramValueLabel, paramValueString);
        }
        return eFormField.EFormFieldNum;
    }

    public static long InsertNoCache(EFormField eFormField)
    {
        return InsertNoCache(eFormField, false);
    }

    public static long InsertNoCache(EFormField eFormField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eformfield (";
        if (isRandomKeys || useExistingPK) command += "EFormFieldNum,";
        command += "EFormNum,PatNum,FieldType,DbLink,ValueLabel,ValueString,ItemOrder,PickListVis,PickListDb,IsHorizStacking,IsTextWrap,Width,FontScale,IsRequired,ConditionalParent,ConditionalValue,LabelAlign,SpaceBelow,ReportableName,IsLocked,Border,IsWidthPercentage,MinWidth,WidthLabel,SpaceToRight) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eFormField.EFormFieldNum) + ",";
        command +=
            SOut.Long(eFormField.EFormNum) + ","
                                           + SOut.Long(eFormField.PatNum) + ","
                                           + SOut.Int((int) eFormField.FieldType) + ","
                                           + "'" + SOut.String(eFormField.DbLink) + "',"
                                           + DbHelper.ParamChar + "paramValueLabel,"
                                           + DbHelper.ParamChar + "paramValueString,"
                                           + SOut.Int(eFormField.ItemOrder) + ","
                                           + "'" + SOut.String(eFormField.PickListVis) + "',"
                                           + "'" + SOut.String(eFormField.PickListDb) + "',"
                                           + SOut.Bool(eFormField.IsHorizStacking) + ","
                                           + SOut.Bool(eFormField.IsTextWrap) + ","
                                           + SOut.Int(eFormField.Width) + ","
                                           + SOut.Int(eFormField.FontScale) + ","
                                           + SOut.Bool(eFormField.IsRequired) + ","
                                           + "'" + SOut.String(eFormField.ConditionalParent) + "',"
                                           + "'" + SOut.String(eFormField.ConditionalValue) + "',"
                                           + SOut.Int((int) eFormField.LabelAlign) + ","
                                           + SOut.Int(eFormField.SpaceBelow) + ","
                                           + "'" + SOut.String(eFormField.ReportableName) + "',"
                                           + SOut.Bool(eFormField.IsLocked) + ","
                                           + SOut.Int((int) eFormField.Border) + ","
                                           + SOut.Bool(eFormField.IsWidthPercentage) + ","
                                           + SOut.Int(eFormField.MinWidth) + ","
                                           + SOut.Int(eFormField.WidthLabel) + ","
                                           + SOut.Int(eFormField.SpaceToRight) + ")";
        if (eFormField.ValueLabel == null) eFormField.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormField.ValueLabel));
        if (eFormField.ValueString == null) eFormField.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(eFormField.ValueString));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramValueLabel, paramValueString);
        else
            eFormField.EFormFieldNum = Db.NonQ(command, true, "EFormFieldNum", "eFormField", paramValueLabel, paramValueString);
        return eFormField.EFormFieldNum;
    }

    public static void Update(EFormField eFormField)
    {
        var command = "UPDATE eformfield SET "
                      + "EFormNum         =  " + SOut.Long(eFormField.EFormNum) + ", "
                      + "PatNum           =  " + SOut.Long(eFormField.PatNum) + ", "
                      + "FieldType        =  " + SOut.Int((int) eFormField.FieldType) + ", "
                      + "DbLink           = '" + SOut.String(eFormField.DbLink) + "', "
                      + "ValueLabel       =  " + DbHelper.ParamChar + "paramValueLabel, "
                      + "ValueString      =  " + DbHelper.ParamChar + "paramValueString, "
                      + "ItemOrder        =  " + SOut.Int(eFormField.ItemOrder) + ", "
                      + "PickListVis      = '" + SOut.String(eFormField.PickListVis) + "', "
                      + "PickListDb       = '" + SOut.String(eFormField.PickListDb) + "', "
                      + "IsHorizStacking  =  " + SOut.Bool(eFormField.IsHorizStacking) + ", "
                      + "IsTextWrap       =  " + SOut.Bool(eFormField.IsTextWrap) + ", "
                      + "Width            =  " + SOut.Int(eFormField.Width) + ", "
                      + "FontScale        =  " + SOut.Int(eFormField.FontScale) + ", "
                      + "IsRequired       =  " + SOut.Bool(eFormField.IsRequired) + ", "
                      + "ConditionalParent= '" + SOut.String(eFormField.ConditionalParent) + "', "
                      + "ConditionalValue = '" + SOut.String(eFormField.ConditionalValue) + "', "
                      + "LabelAlign       =  " + SOut.Int((int) eFormField.LabelAlign) + ", "
                      + "SpaceBelow       =  " + SOut.Int(eFormField.SpaceBelow) + ", "
                      + "ReportableName   = '" + SOut.String(eFormField.ReportableName) + "', "
                      + "IsLocked         =  " + SOut.Bool(eFormField.IsLocked) + ", "
                      + "Border           =  " + SOut.Int((int) eFormField.Border) + ", "
                      + "IsWidthPercentage=  " + SOut.Bool(eFormField.IsWidthPercentage) + ", "
                      + "MinWidth         =  " + SOut.Int(eFormField.MinWidth) + ", "
                      + "WidthLabel       =  " + SOut.Int(eFormField.WidthLabel) + ", "
                      + "SpaceToRight     =  " + SOut.Int(eFormField.SpaceToRight) + " "
                      + "WHERE EFormFieldNum = " + SOut.Long(eFormField.EFormFieldNum);
        if (eFormField.ValueLabel == null) eFormField.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormField.ValueLabel));
        if (eFormField.ValueString == null) eFormField.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(eFormField.ValueString));
        Db.NonQ(command, paramValueLabel, paramValueString);
    }

    public static bool Update(EFormField eFormField, EFormField oldEFormField)
    {
        var command = "";
        if (eFormField.EFormNum != oldEFormField.EFormNum)
        {
            if (command != "") command += ",";
            command += "EFormNum = " + SOut.Long(eFormField.EFormNum) + "";
        }

        if (eFormField.PatNum != oldEFormField.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(eFormField.PatNum) + "";
        }

        if (eFormField.FieldType != oldEFormField.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) eFormField.FieldType) + "";
        }

        if (eFormField.DbLink != oldEFormField.DbLink)
        {
            if (command != "") command += ",";
            command += "DbLink = '" + SOut.String(eFormField.DbLink) + "'";
        }

        if (eFormField.ValueLabel != oldEFormField.ValueLabel)
        {
            if (command != "") command += ",";
            command += "ValueLabel = " + DbHelper.ParamChar + "paramValueLabel";
        }

        if (eFormField.ValueString != oldEFormField.ValueString)
        {
            if (command != "") command += ",";
            command += "ValueString = " + DbHelper.ParamChar + "paramValueString";
        }

        if (eFormField.ItemOrder != oldEFormField.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(eFormField.ItemOrder) + "";
        }

        if (eFormField.PickListVis != oldEFormField.PickListVis)
        {
            if (command != "") command += ",";
            command += "PickListVis = '" + SOut.String(eFormField.PickListVis) + "'";
        }

        if (eFormField.PickListDb != oldEFormField.PickListDb)
        {
            if (command != "") command += ",";
            command += "PickListDb = '" + SOut.String(eFormField.PickListDb) + "'";
        }

        if (eFormField.IsHorizStacking != oldEFormField.IsHorizStacking)
        {
            if (command != "") command += ",";
            command += "IsHorizStacking = " + SOut.Bool(eFormField.IsHorizStacking) + "";
        }

        if (eFormField.IsTextWrap != oldEFormField.IsTextWrap)
        {
            if (command != "") command += ",";
            command += "IsTextWrap = " + SOut.Bool(eFormField.IsTextWrap) + "";
        }

        if (eFormField.Width != oldEFormField.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(eFormField.Width) + "";
        }

        if (eFormField.FontScale != oldEFormField.FontScale)
        {
            if (command != "") command += ",";
            command += "FontScale = " + SOut.Int(eFormField.FontScale) + "";
        }

        if (eFormField.IsRequired != oldEFormField.IsRequired)
        {
            if (command != "") command += ",";
            command += "IsRequired = " + SOut.Bool(eFormField.IsRequired) + "";
        }

        if (eFormField.ConditionalParent != oldEFormField.ConditionalParent)
        {
            if (command != "") command += ",";
            command += "ConditionalParent = '" + SOut.String(eFormField.ConditionalParent) + "'";
        }

        if (eFormField.ConditionalValue != oldEFormField.ConditionalValue)
        {
            if (command != "") command += ",";
            command += "ConditionalValue = '" + SOut.String(eFormField.ConditionalValue) + "'";
        }

        if (eFormField.LabelAlign != oldEFormField.LabelAlign)
        {
            if (command != "") command += ",";
            command += "LabelAlign = " + SOut.Int((int) eFormField.LabelAlign) + "";
        }

        if (eFormField.SpaceBelow != oldEFormField.SpaceBelow)
        {
            if (command != "") command += ",";
            command += "SpaceBelow = " + SOut.Int(eFormField.SpaceBelow) + "";
        }

        if (eFormField.ReportableName != oldEFormField.ReportableName)
        {
            if (command != "") command += ",";
            command += "ReportableName = '" + SOut.String(eFormField.ReportableName) + "'";
        }

        if (eFormField.IsLocked != oldEFormField.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(eFormField.IsLocked) + "";
        }

        if (eFormField.Border != oldEFormField.Border)
        {
            if (command != "") command += ",";
            command += "Border = " + SOut.Int((int) eFormField.Border) + "";
        }

        if (eFormField.IsWidthPercentage != oldEFormField.IsWidthPercentage)
        {
            if (command != "") command += ",";
            command += "IsWidthPercentage = " + SOut.Bool(eFormField.IsWidthPercentage) + "";
        }

        if (eFormField.MinWidth != oldEFormField.MinWidth)
        {
            if (command != "") command += ",";
            command += "MinWidth = " + SOut.Int(eFormField.MinWidth) + "";
        }

        if (eFormField.WidthLabel != oldEFormField.WidthLabel)
        {
            if (command != "") command += ",";
            command += "WidthLabel = " + SOut.Int(eFormField.WidthLabel) + "";
        }

        if (eFormField.SpaceToRight != oldEFormField.SpaceToRight)
        {
            if (command != "") command += ",";
            command += "SpaceToRight = " + SOut.Int(eFormField.SpaceToRight) + "";
        }

        if (command == "") return false;
        if (eFormField.ValueLabel == null) eFormField.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormField.ValueLabel));
        if (eFormField.ValueString == null) eFormField.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", OdDbType.Text, SOut.StringParam(eFormField.ValueString));
        command = "UPDATE eformfield SET " + command
                                           + " WHERE EFormFieldNum = " + SOut.Long(eFormField.EFormFieldNum);
        Db.NonQ(command, paramValueLabel, paramValueString);
        return true;
    }

    public static bool UpdateComparison(EFormField eFormField, EFormField oldEFormField)
    {
        if (eFormField.EFormNum != oldEFormField.EFormNum) return true;
        if (eFormField.PatNum != oldEFormField.PatNum) return true;
        if (eFormField.FieldType != oldEFormField.FieldType) return true;
        if (eFormField.DbLink != oldEFormField.DbLink) return true;
        if (eFormField.ValueLabel != oldEFormField.ValueLabel) return true;
        if (eFormField.ValueString != oldEFormField.ValueString) return true;
        if (eFormField.ItemOrder != oldEFormField.ItemOrder) return true;
        if (eFormField.PickListVis != oldEFormField.PickListVis) return true;
        if (eFormField.PickListDb != oldEFormField.PickListDb) return true;
        if (eFormField.IsHorizStacking != oldEFormField.IsHorizStacking) return true;
        if (eFormField.IsTextWrap != oldEFormField.IsTextWrap) return true;
        if (eFormField.Width != oldEFormField.Width) return true;
        if (eFormField.FontScale != oldEFormField.FontScale) return true;
        if (eFormField.IsRequired != oldEFormField.IsRequired) return true;
        if (eFormField.ConditionalParent != oldEFormField.ConditionalParent) return true;
        if (eFormField.ConditionalValue != oldEFormField.ConditionalValue) return true;
        if (eFormField.LabelAlign != oldEFormField.LabelAlign) return true;
        if (eFormField.SpaceBelow != oldEFormField.SpaceBelow) return true;
        if (eFormField.ReportableName != oldEFormField.ReportableName) return true;
        if (eFormField.IsLocked != oldEFormField.IsLocked) return true;
        if (eFormField.Border != oldEFormField.Border) return true;
        if (eFormField.IsWidthPercentage != oldEFormField.IsWidthPercentage) return true;
        if (eFormField.MinWidth != oldEFormField.MinWidth) return true;
        if (eFormField.WidthLabel != oldEFormField.WidthLabel) return true;
        if (eFormField.SpaceToRight != oldEFormField.SpaceToRight) return true;
        return false;
    }

    public static void Delete(long eFormFieldNum)
    {
        var command = "DELETE FROM eformfield "
                      + "WHERE EFormFieldNum = " + SOut.Long(eFormFieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEFormFieldNums)
    {
        if (listEFormFieldNums == null || listEFormFieldNums.Count == 0) return;
        var command = "DELETE FROM eformfield "
                      + "WHERE EFormFieldNum IN(" + string.Join(",", listEFormFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}