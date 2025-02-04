using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EFormFieldDefCrud
{
    public static List<EFormFieldDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EFormFieldDef> TableToList(DataTable table)
    {
        var retVal = new List<EFormFieldDef>();
        foreach (DataRow row in table.Rows)
        {
            var eFormFieldDef = new EFormFieldDef
            {
                EFormFieldDefNum = SIn.Long(row["EFormFieldDefNum"].ToString()),
                EFormDefNum = SIn.Long(row["EFormDefNum"].ToString()),
                FieldType = (EnumEFormFieldType) SIn.Int(row["FieldType"].ToString()),
                DbLink = SIn.String(row["DbLink"].ToString()),
                ValueLabel = SIn.String(row["ValueLabel"].ToString()),
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
            retVal.Add(eFormFieldDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EFormFieldDef> listEFormFieldDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EFormFieldDef";
        var table = new DataTable(tableName);
        table.Columns.Add("EFormFieldDefNum");
        table.Columns.Add("EFormDefNum");
        table.Columns.Add("FieldType");
        table.Columns.Add("DbLink");
        table.Columns.Add("ValueLabel");
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
        foreach (var eFormFieldDef in listEFormFieldDefs)
            table.Rows.Add(SOut.Long(eFormFieldDef.EFormFieldDefNum), SOut.Long(eFormFieldDef.EFormDefNum), SOut.Int((int) eFormFieldDef.FieldType), eFormFieldDef.DbLink, eFormFieldDef.ValueLabel, SOut.Int(eFormFieldDef.ItemOrder), eFormFieldDef.PickListVis, eFormFieldDef.PickListDb, SOut.Bool(eFormFieldDef.IsHorizStacking), SOut.Bool(eFormFieldDef.IsTextWrap), SOut.Int(eFormFieldDef.Width), SOut.Int(eFormFieldDef.FontScale), SOut.Bool(eFormFieldDef.IsRequired), eFormFieldDef.ConditionalParent, eFormFieldDef.ConditionalValue, SOut.Int((int) eFormFieldDef.LabelAlign), SOut.Int(eFormFieldDef.SpaceBelow), eFormFieldDef.ReportableName, SOut.Bool(eFormFieldDef.IsLocked), SOut.Int((int) eFormFieldDef.Border), SOut.Bool(eFormFieldDef.IsWidthPercentage), SOut.Int(eFormFieldDef.MinWidth), SOut.Int(eFormFieldDef.WidthLabel), SOut.Int(eFormFieldDef.SpaceToRight));
        return table;
    }

    public static void Delete(long eFormFieldDefNum)
    {
        var command = "DELETE FROM eformfielddef "
                      + "WHERE EFormFieldDefNum = " + SOut.Long(eFormFieldDefNum);
        Db.NonQ(command);
    }
}