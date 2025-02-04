using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EFormFieldCrud
{
    public static List<EFormField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EFormField> TableToList(DataTable table)
    {
        var retVal = new List<EFormField>();
        foreach (DataRow row in table.Rows)
        {
            var eFormField = new EFormField
            {
                EFormFieldNum = SIn.Long(row["EFormFieldNum"].ToString()),
                EFormNum = SIn.Long(row["EFormNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                FieldType = (EnumEFormFieldType) SIn.Int(row["FieldType"].ToString()),
                DbLink = SIn.String(row["DbLink"].ToString()),
                ValueLabel = SIn.String(row["ValueLabel"].ToString()),
                ValueString = SIn.String(row["ValueString"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                PickListVis = SIn.String(row["PickListVis"].ToString()),
                PickListDb = SIn.String(row["PickListDb"].ToString()),
                IsHorizStacking = SIn.Bool(row["IsHorizStacking"].ToString()),
                IsTextWrap = SIn.Bool(row["IsTextWrap"].ToString()),
                Width = SIn.Int(row["Width"].ToString()),
                FontScale = SIn.Int(row["FontScale"].ToString()),
                IsRequired = SIn.Bool(row["IsRequired"].ToString()),
                ConditionalParent = SIn.String(row["ConditionalParent"].ToString()),
                ConditionalValue = SIn.String(row["ConditionalValue"].ToString()),
                LabelAlign = (EnumEFormLabelAlign) SIn.Int(row["LabelAlign"].ToString()),
                SpaceBelow = SIn.Int(row["SpaceBelow"].ToString()),
                ReportableName = SIn.String(row["ReportableName"].ToString()),
                IsLocked = SIn.Bool(row["IsLocked"].ToString()),
                Border = (EnumEFormBorder) SIn.Int(row["Border"].ToString()),
                IsWidthPercentage = SIn.Bool(row["IsWidthPercentage"].ToString()),
                MinWidth = SIn.Int(row["MinWidth"].ToString()),
                WidthLabel = SIn.Int(row["WidthLabel"].ToString()),
                SpaceToRight = SIn.Int(row["SpaceToRight"].ToString())
            };
            retVal.Add(eFormField);
        }

        return retVal;
    }

    public static void Insert(EFormField eFormField)
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
        var paramValueLabel = new OdSqlParameter("paramValueLabel", SOut.StringParam(eFormField.ValueLabel));
        if (eFormField.ValueString == null) eFormField.ValueString = "";
        var paramValueString = new OdSqlParameter("paramValueString", SOut.StringParam(eFormField.ValueString));
        {
            eFormField.EFormFieldNum = Db.NonQ(command, true, "EFormFieldNum", "eFormField", paramValueLabel, paramValueString);
        }
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
}