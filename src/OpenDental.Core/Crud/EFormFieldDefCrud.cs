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
        EFormFieldDef eFormFieldDef;
        foreach (DataRow row in table.Rows)
        {
            eFormFieldDef = new EFormFieldDef();
            eFormFieldDef.EFormFieldDefNum = SIn.Long(row["EFormFieldDefNum"].ToString());
            eFormFieldDef.EFormDefNum = SIn.Long(row["EFormDefNum"].ToString());
            eFormFieldDef.FieldType = (EnumEFormFieldType) SIn.Int(row["FieldType"].ToString());
            eFormFieldDef.DbLink = SIn.String(row["DbLink"].ToString());
            eFormFieldDef.ValueLabel = SIn.String(row["ValueLabel"].ToString());
            eFormFieldDef.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            eFormFieldDef.PickListVis = SIn.String(row["PickListVis"].ToString());
            eFormFieldDef.PickListDb = SIn.String(row["PickListDb"].ToString());
            eFormFieldDef.IsHorizStacking = SIn.Bool(row["IsHorizStacking"].ToString());
            eFormFieldDef.IsTextWrap = SIn.Bool(row["IsTextWrap"].ToString());
            eFormFieldDef.Width = SIn.Int(row["Width"].ToString());
            eFormFieldDef.FontScale = SIn.Int(row["FontScale"].ToString());
            eFormFieldDef.IsRequired = SIn.Bool(row["IsRequired"].ToString());
            eFormFieldDef.ConditionalParent = SIn.String(row["ConditionalParent"].ToString());
            eFormFieldDef.ConditionalValue = SIn.String(row["ConditionalValue"].ToString());
            eFormFieldDef.LabelAlign = (EnumEFormLabelAlign) SIn.Int(row["LabelAlign"].ToString());
            eFormFieldDef.SpaceBelow = SIn.Int(row["SpaceBelow"].ToString());
            eFormFieldDef.ReportableName = SIn.String(row["ReportableName"].ToString());
            eFormFieldDef.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            eFormFieldDef.Border = (EnumEFormBorder) SIn.Int(row["Border"].ToString());
            eFormFieldDef.IsWidthPercentage = SIn.Bool(row["IsWidthPercentage"].ToString());
            eFormFieldDef.MinWidth = SIn.Int(row["MinWidth"].ToString());
            eFormFieldDef.WidthLabel = SIn.Int(row["WidthLabel"].ToString());
            eFormFieldDef.SpaceToRight = SIn.Int(row["SpaceToRight"].ToString());
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

    public static void Insert(EFormFieldDef eFormFieldDef)
    {
        var command = "INSERT INTO eformfielddef (";

        command += "EFormDefNum,FieldType,DbLink,ValueLabel,ItemOrder,PickListVis,PickListDb,IsHorizStacking,IsTextWrap,Width,FontScale,IsRequired,ConditionalParent,ConditionalValue,LabelAlign,SpaceBelow,ReportableName,IsLocked,Border,IsWidthPercentage,MinWidth,WidthLabel,SpaceToRight) VALUES(";

        command +=
            SOut.Long(eFormFieldDef.EFormDefNum) + ","
                                                 + SOut.Int((int) eFormFieldDef.FieldType) + ","
                                                 + "'" + SOut.String(eFormFieldDef.DbLink) + "',"
                                                 + DbHelper.ParamChar + "paramValueLabel,"
                                                 + SOut.Int(eFormFieldDef.ItemOrder) + ","
                                                 + "'" + SOut.String(eFormFieldDef.PickListVis) + "',"
                                                 + "'" + SOut.String(eFormFieldDef.PickListDb) + "',"
                                                 + SOut.Bool(eFormFieldDef.IsHorizStacking) + ","
                                                 + SOut.Bool(eFormFieldDef.IsTextWrap) + ","
                                                 + SOut.Int(eFormFieldDef.Width) + ","
                                                 + SOut.Int(eFormFieldDef.FontScale) + ","
                                                 + SOut.Bool(eFormFieldDef.IsRequired) + ","
                                                 + "'" + SOut.String(eFormFieldDef.ConditionalParent) + "',"
                                                 + "'" + SOut.String(eFormFieldDef.ConditionalValue) + "',"
                                                 + SOut.Int((int) eFormFieldDef.LabelAlign) + ","
                                                 + SOut.Int(eFormFieldDef.SpaceBelow) + ","
                                                 + "'" + SOut.String(eFormFieldDef.ReportableName) + "',"
                                                 + SOut.Bool(eFormFieldDef.IsLocked) + ","
                                                 + SOut.Int((int) eFormFieldDef.Border) + ","
                                                 + SOut.Bool(eFormFieldDef.IsWidthPercentage) + ","
                                                 + SOut.Int(eFormFieldDef.MinWidth) + ","
                                                 + SOut.Int(eFormFieldDef.WidthLabel) + ","
                                                 + SOut.Int(eFormFieldDef.SpaceToRight) + ")";
        if (eFormFieldDef.ValueLabel == null) eFormFieldDef.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", SOut.StringParam(eFormFieldDef.ValueLabel));
        {
            eFormFieldDef.EFormFieldDefNum = Db.NonQ(command, true, "EFormFieldDefNum", "eFormFieldDef", paramValueLabel);
        }
    }

    public static void Update(EFormFieldDef eFormFieldDef)
    {
        var command = "UPDATE eformfielddef SET "
                      + "EFormDefNum      =  " + SOut.Long(eFormFieldDef.EFormDefNum) + ", "
                      + "FieldType        =  " + SOut.Int((int) eFormFieldDef.FieldType) + ", "
                      + "DbLink           = '" + SOut.String(eFormFieldDef.DbLink) + "', "
                      + "ValueLabel       =  " + DbHelper.ParamChar + "paramValueLabel, "
                      + "ItemOrder        =  " + SOut.Int(eFormFieldDef.ItemOrder) + ", "
                      + "PickListVis      = '" + SOut.String(eFormFieldDef.PickListVis) + "', "
                      + "PickListDb       = '" + SOut.String(eFormFieldDef.PickListDb) + "', "
                      + "IsHorizStacking  =  " + SOut.Bool(eFormFieldDef.IsHorizStacking) + ", "
                      + "IsTextWrap       =  " + SOut.Bool(eFormFieldDef.IsTextWrap) + ", "
                      + "Width            =  " + SOut.Int(eFormFieldDef.Width) + ", "
                      + "FontScale        =  " + SOut.Int(eFormFieldDef.FontScale) + ", "
                      + "IsRequired       =  " + SOut.Bool(eFormFieldDef.IsRequired) + ", "
                      + "ConditionalParent= '" + SOut.String(eFormFieldDef.ConditionalParent) + "', "
                      + "ConditionalValue = '" + SOut.String(eFormFieldDef.ConditionalValue) + "', "
                      + "LabelAlign       =  " + SOut.Int((int) eFormFieldDef.LabelAlign) + ", "
                      + "SpaceBelow       =  " + SOut.Int(eFormFieldDef.SpaceBelow) + ", "
                      + "ReportableName   = '" + SOut.String(eFormFieldDef.ReportableName) + "', "
                      + "IsLocked         =  " + SOut.Bool(eFormFieldDef.IsLocked) + ", "
                      + "Border           =  " + SOut.Int((int) eFormFieldDef.Border) + ", "
                      + "IsWidthPercentage=  " + SOut.Bool(eFormFieldDef.IsWidthPercentage) + ", "
                      + "MinWidth         =  " + SOut.Int(eFormFieldDef.MinWidth) + ", "
                      + "WidthLabel       =  " + SOut.Int(eFormFieldDef.WidthLabel) + ", "
                      + "SpaceToRight     =  " + SOut.Int(eFormFieldDef.SpaceToRight) + " "
                      + "WHERE EFormFieldDefNum = " + SOut.Long(eFormFieldDef.EFormFieldDefNum);
        if (eFormFieldDef.ValueLabel == null) eFormFieldDef.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", SOut.StringParam(eFormFieldDef.ValueLabel));
        Db.NonQ(command, paramValueLabel);
    }

    public static void Delete(long eFormFieldDefNum)
    {
        var command = "DELETE FROM eformfielddef "
                      + "WHERE EFormFieldDefNum = " + SOut.Long(eFormFieldDefNum);
        Db.NonQ(command);
    }
}