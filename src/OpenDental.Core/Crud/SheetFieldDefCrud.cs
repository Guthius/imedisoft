#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SheetFieldDefCrud
{
    public static SheetFieldDef SelectOne(long sheetFieldDefNum)
    {
        var command = "SELECT * FROM sheetfielddef "
                      + "WHERE SheetFieldDefNum = " + SOut.Long(sheetFieldDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SheetFieldDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SheetFieldDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SheetFieldDef> TableToList(DataTable table)
    {
        var retVal = new List<SheetFieldDef>();
        SheetFieldDef sheetFieldDef;
        foreach (DataRow row in table.Rows)
        {
            sheetFieldDef = new SheetFieldDef();
            sheetFieldDef.SheetFieldDefNum = SIn.Long(row["SheetFieldDefNum"].ToString());
            sheetFieldDef.SheetDefNum = SIn.Long(row["SheetDefNum"].ToString());
            sheetFieldDef.FieldType = (SheetFieldType) SIn.Int(row["FieldType"].ToString());
            sheetFieldDef.FieldName = SIn.String(row["FieldName"].ToString());
            sheetFieldDef.FieldValue = SIn.String(row["FieldValue"].ToString());
            sheetFieldDef.FontSize = SIn.Float(row["FontSize"].ToString());
            sheetFieldDef.FontName = SIn.String(row["FontName"].ToString());
            sheetFieldDef.FontIsBold = SIn.Bool(row["FontIsBold"].ToString());
            sheetFieldDef.XPos = SIn.Int(row["XPos"].ToString());
            sheetFieldDef.YPos = SIn.Int(row["YPos"].ToString());
            sheetFieldDef.Width = SIn.Int(row["Width"].ToString());
            sheetFieldDef.Height = SIn.Int(row["Height"].ToString());
            sheetFieldDef.GrowthBehavior = (GrowthBehaviorEnum) SIn.Int(row["GrowthBehavior"].ToString());
            sheetFieldDef.RadioButtonValue = SIn.String(row["RadioButtonValue"].ToString());
            sheetFieldDef.RadioButtonGroup = SIn.String(row["RadioButtonGroup"].ToString());
            sheetFieldDef.IsRequired = SIn.Bool(row["IsRequired"].ToString());
            sheetFieldDef.TabOrder = SIn.Int(row["TabOrder"].ToString());
            sheetFieldDef.ReportableName = SIn.String(row["ReportableName"].ToString());
            sheetFieldDef.TextAlign = (HorizontalAlignment) SIn.Int(row["TextAlign"].ToString());
            sheetFieldDef.IsPaymentOption = SIn.Bool(row["IsPaymentOption"].ToString());
            sheetFieldDef.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            sheetFieldDef.ItemColor = Color.FromArgb(SIn.Int(row["ItemColor"].ToString()));
            sheetFieldDef.TabOrderMobile = SIn.Int(row["TabOrderMobile"].ToString());
            sheetFieldDef.UiLabelMobile = SIn.String(row["UiLabelMobile"].ToString());
            sheetFieldDef.UiLabelMobileRadioButton = SIn.String(row["UiLabelMobileRadioButton"].ToString());
            sheetFieldDef.LayoutMode = (SheetFieldLayoutMode) SIn.Int(row["LayoutMode"].ToString());
            sheetFieldDef.Language = SIn.String(row["Language"].ToString());
            sheetFieldDef.CanElectronicallySign = SIn.Bool(row["CanElectronicallySign"].ToString());
            sheetFieldDef.IsSigProvRestricted = SIn.Bool(row["IsSigProvRestricted"].ToString());
            retVal.Add(sheetFieldDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SheetFieldDef> listSheetFieldDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SheetFieldDef";
        var table = new DataTable(tableName);
        table.Columns.Add("SheetFieldDefNum");
        table.Columns.Add("SheetDefNum");
        table.Columns.Add("FieldType");
        table.Columns.Add("FieldName");
        table.Columns.Add("FieldValue");
        table.Columns.Add("FontSize");
        table.Columns.Add("FontName");
        table.Columns.Add("FontIsBold");
        table.Columns.Add("XPos");
        table.Columns.Add("YPos");
        table.Columns.Add("Width");
        table.Columns.Add("Height");
        table.Columns.Add("GrowthBehavior");
        table.Columns.Add("RadioButtonValue");
        table.Columns.Add("RadioButtonGroup");
        table.Columns.Add("IsRequired");
        table.Columns.Add("TabOrder");
        table.Columns.Add("ReportableName");
        table.Columns.Add("TextAlign");
        table.Columns.Add("IsPaymentOption");
        table.Columns.Add("IsLocked");
        table.Columns.Add("ItemColor");
        table.Columns.Add("TabOrderMobile");
        table.Columns.Add("UiLabelMobile");
        table.Columns.Add("UiLabelMobileRadioButton");
        table.Columns.Add("LayoutMode");
        table.Columns.Add("Language");
        table.Columns.Add("CanElectronicallySign");
        table.Columns.Add("IsSigProvRestricted");
        foreach (var sheetFieldDef in listSheetFieldDefs)
            table.Rows.Add(SOut.Long(sheetFieldDef.SheetFieldDefNum), SOut.Long(sheetFieldDef.SheetDefNum), SOut.Int((int) sheetFieldDef.FieldType), sheetFieldDef.FieldName, sheetFieldDef.FieldValue, SOut.Float(sheetFieldDef.FontSize), sheetFieldDef.FontName, SOut.Bool(sheetFieldDef.FontIsBold), SOut.Int(sheetFieldDef.XPos), SOut.Int(sheetFieldDef.YPos), SOut.Int(sheetFieldDef.Width), SOut.Int(sheetFieldDef.Height), SOut.Int((int) sheetFieldDef.GrowthBehavior), sheetFieldDef.RadioButtonValue, sheetFieldDef.RadioButtonGroup, SOut.Bool(sheetFieldDef.IsRequired), SOut.Int(sheetFieldDef.TabOrder), sheetFieldDef.ReportableName, SOut.Int((int) sheetFieldDef.TextAlign), SOut.Bool(sheetFieldDef.IsPaymentOption), SOut.Bool(sheetFieldDef.IsLocked), SOut.Int(sheetFieldDef.ItemColor.ToArgb()), SOut.Int(sheetFieldDef.TabOrderMobile), sheetFieldDef.UiLabelMobile, sheetFieldDef.UiLabelMobileRadioButton, SOut.Int((int) sheetFieldDef.LayoutMode), sheetFieldDef.Language, SOut.Bool(sheetFieldDef.CanElectronicallySign), SOut.Bool(sheetFieldDef.IsSigProvRestricted));
        return table;
    }

    public static long Insert(SheetFieldDef sheetFieldDef)
    {
        return Insert(sheetFieldDef, false);
    }

    public static long Insert(SheetFieldDef sheetFieldDef, bool useExistingPK)
    {
        var command = "INSERT INTO sheetfielddef (";

        command += "SheetDefNum,FieldType,FieldName,FieldValue,FontSize,FontName,FontIsBold,XPos,YPos,Width,Height,GrowthBehavior,RadioButtonValue,RadioButtonGroup,IsRequired,TabOrder,ReportableName,TextAlign,IsPaymentOption,IsLocked,ItemColor,TabOrderMobile,UiLabelMobile,UiLabelMobileRadioButton,LayoutMode,Language,CanElectronicallySign,IsSigProvRestricted) VALUES(";

        command +=
            SOut.Long(sheetFieldDef.SheetDefNum) + ","
                                                 + SOut.Int((int) sheetFieldDef.FieldType) + ","
                                                 + "'" + SOut.String(sheetFieldDef.FieldName) + "',"
                                                 + DbHelper.ParamChar + "paramFieldValue,"
                                                 + SOut.Float(sheetFieldDef.FontSize) + ","
                                                 + "'" + SOut.String(sheetFieldDef.FontName) + "',"
                                                 + SOut.Bool(sheetFieldDef.FontIsBold) + ","
                                                 + SOut.Int(sheetFieldDef.XPos) + ","
                                                 + SOut.Int(sheetFieldDef.YPos) + ","
                                                 + SOut.Int(sheetFieldDef.Width) + ","
                                                 + SOut.Int(sheetFieldDef.Height) + ","
                                                 + SOut.Int((int) sheetFieldDef.GrowthBehavior) + ","
                                                 + "'" + SOut.String(sheetFieldDef.RadioButtonValue) + "',"
                                                 + "'" + SOut.String(sheetFieldDef.RadioButtonGroup) + "',"
                                                 + SOut.Bool(sheetFieldDef.IsRequired) + ","
                                                 + SOut.Int(sheetFieldDef.TabOrder) + ","
                                                 + "'" + SOut.String(sheetFieldDef.ReportableName) + "',"
                                                 + SOut.Int((int) sheetFieldDef.TextAlign) + ","
                                                 + SOut.Bool(sheetFieldDef.IsPaymentOption) + ","
                                                 + SOut.Bool(sheetFieldDef.IsLocked) + ","
                                                 + SOut.Int(sheetFieldDef.ItemColor.ToArgb()) + ","
                                                 + SOut.Int(sheetFieldDef.TabOrderMobile) + ","
                                                 + DbHelper.ParamChar + "paramUiLabelMobile,"
                                                 + DbHelper.ParamChar + "paramUiLabelMobileRadioButton,"
                                                 + SOut.Int((int) sheetFieldDef.LayoutMode) + ","
                                                 + "'" + SOut.String(sheetFieldDef.Language) + "',"
                                                 + SOut.Bool(sheetFieldDef.CanElectronicallySign) + ","
                                                 + SOut.Bool(sheetFieldDef.IsSigProvRestricted) + ")";
        if (sheetFieldDef.FieldValue == null) sheetFieldDef.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetFieldDef.FieldValue));
        if (sheetFieldDef.UiLabelMobile == null) sheetFieldDef.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobile));
        if (sheetFieldDef.UiLabelMobileRadioButton == null) sheetFieldDef.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobileRadioButton));
        {
            sheetFieldDef.SheetFieldDefNum = Db.NonQ(command, true, "SheetFieldDefNum", "sheetFieldDef", paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        }
        return sheetFieldDef.SheetFieldDefNum;
    }

    public static long InsertNoCache(SheetFieldDef sheetFieldDef)
    {
        return InsertNoCache(sheetFieldDef, false);
    }

    public static long InsertNoCache(SheetFieldDef sheetFieldDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sheetfielddef (";
        if (isRandomKeys || useExistingPK) command += "SheetFieldDefNum,";
        command += "SheetDefNum,FieldType,FieldName,FieldValue,FontSize,FontName,FontIsBold,XPos,YPos,Width,Height,GrowthBehavior,RadioButtonValue,RadioButtonGroup,IsRequired,TabOrder,ReportableName,TextAlign,IsPaymentOption,IsLocked,ItemColor,TabOrderMobile,UiLabelMobile,UiLabelMobileRadioButton,LayoutMode,Language,CanElectronicallySign,IsSigProvRestricted) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sheetFieldDef.SheetFieldDefNum) + ",";
        command +=
            SOut.Long(sheetFieldDef.SheetDefNum) + ","
                                                 + SOut.Int((int) sheetFieldDef.FieldType) + ","
                                                 + "'" + SOut.String(sheetFieldDef.FieldName) + "',"
                                                 + DbHelper.ParamChar + "paramFieldValue,"
                                                 + SOut.Float(sheetFieldDef.FontSize) + ","
                                                 + "'" + SOut.String(sheetFieldDef.FontName) + "',"
                                                 + SOut.Bool(sheetFieldDef.FontIsBold) + ","
                                                 + SOut.Int(sheetFieldDef.XPos) + ","
                                                 + SOut.Int(sheetFieldDef.YPos) + ","
                                                 + SOut.Int(sheetFieldDef.Width) + ","
                                                 + SOut.Int(sheetFieldDef.Height) + ","
                                                 + SOut.Int((int) sheetFieldDef.GrowthBehavior) + ","
                                                 + "'" + SOut.String(sheetFieldDef.RadioButtonValue) + "',"
                                                 + "'" + SOut.String(sheetFieldDef.RadioButtonGroup) + "',"
                                                 + SOut.Bool(sheetFieldDef.IsRequired) + ","
                                                 + SOut.Int(sheetFieldDef.TabOrder) + ","
                                                 + "'" + SOut.String(sheetFieldDef.ReportableName) + "',"
                                                 + SOut.Int((int) sheetFieldDef.TextAlign) + ","
                                                 + SOut.Bool(sheetFieldDef.IsPaymentOption) + ","
                                                 + SOut.Bool(sheetFieldDef.IsLocked) + ","
                                                 + SOut.Int(sheetFieldDef.ItemColor.ToArgb()) + ","
                                                 + SOut.Int(sheetFieldDef.TabOrderMobile) + ","
                                                 + DbHelper.ParamChar + "paramUiLabelMobile,"
                                                 + DbHelper.ParamChar + "paramUiLabelMobileRadioButton,"
                                                 + SOut.Int((int) sheetFieldDef.LayoutMode) + ","
                                                 + "'" + SOut.String(sheetFieldDef.Language) + "',"
                                                 + SOut.Bool(sheetFieldDef.CanElectronicallySign) + ","
                                                 + SOut.Bool(sheetFieldDef.IsSigProvRestricted) + ")";
        if (sheetFieldDef.FieldValue == null) sheetFieldDef.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetFieldDef.FieldValue));
        if (sheetFieldDef.UiLabelMobile == null) sheetFieldDef.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobile));
        if (sheetFieldDef.UiLabelMobileRadioButton == null) sheetFieldDef.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobileRadioButton));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        else
            sheetFieldDef.SheetFieldDefNum = Db.NonQ(command, true, "SheetFieldDefNum", "sheetFieldDef", paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        return sheetFieldDef.SheetFieldDefNum;
    }

    public static void Update(SheetFieldDef sheetFieldDef)
    {
        var command = "UPDATE sheetfielddef SET "
                      + "SheetDefNum             =  " + SOut.Long(sheetFieldDef.SheetDefNum) + ", "
                      + "FieldType               =  " + SOut.Int((int) sheetFieldDef.FieldType) + ", "
                      + "FieldName               = '" + SOut.String(sheetFieldDef.FieldName) + "', "
                      + "FieldValue              =  " + DbHelper.ParamChar + "paramFieldValue, "
                      + "FontSize                =  " + SOut.Float(sheetFieldDef.FontSize) + ", "
                      + "FontName                = '" + SOut.String(sheetFieldDef.FontName) + "', "
                      + "FontIsBold              =  " + SOut.Bool(sheetFieldDef.FontIsBold) + ", "
                      + "XPos                    =  " + SOut.Int(sheetFieldDef.XPos) + ", "
                      + "YPos                    =  " + SOut.Int(sheetFieldDef.YPos) + ", "
                      + "Width                   =  " + SOut.Int(sheetFieldDef.Width) + ", "
                      + "Height                  =  " + SOut.Int(sheetFieldDef.Height) + ", "
                      + "GrowthBehavior          =  " + SOut.Int((int) sheetFieldDef.GrowthBehavior) + ", "
                      + "RadioButtonValue        = '" + SOut.String(sheetFieldDef.RadioButtonValue) + "', "
                      + "RadioButtonGroup        = '" + SOut.String(sheetFieldDef.RadioButtonGroup) + "', "
                      + "IsRequired              =  " + SOut.Bool(sheetFieldDef.IsRequired) + ", "
                      + "TabOrder                =  " + SOut.Int(sheetFieldDef.TabOrder) + ", "
                      + "ReportableName          = '" + SOut.String(sheetFieldDef.ReportableName) + "', "
                      + "TextAlign               =  " + SOut.Int((int) sheetFieldDef.TextAlign) + ", "
                      + "IsPaymentOption         =  " + SOut.Bool(sheetFieldDef.IsPaymentOption) + ", "
                      + "IsLocked                =  " + SOut.Bool(sheetFieldDef.IsLocked) + ", "
                      + "ItemColor               =  " + SOut.Int(sheetFieldDef.ItemColor.ToArgb()) + ", "
                      + "TabOrderMobile          =  " + SOut.Int(sheetFieldDef.TabOrderMobile) + ", "
                      + "UiLabelMobile           =  " + DbHelper.ParamChar + "paramUiLabelMobile, "
                      + "UiLabelMobileRadioButton=  " + DbHelper.ParamChar + "paramUiLabelMobileRadioButton, "
                      + "LayoutMode              =  " + SOut.Int((int) sheetFieldDef.LayoutMode) + ", "
                      + "Language                = '" + SOut.String(sheetFieldDef.Language) + "', "
                      + "CanElectronicallySign   =  " + SOut.Bool(sheetFieldDef.CanElectronicallySign) + ", "
                      + "IsSigProvRestricted     =  " + SOut.Bool(sheetFieldDef.IsSigProvRestricted) + " "
                      + "WHERE SheetFieldDefNum = " + SOut.Long(sheetFieldDef.SheetFieldDefNum);
        if (sheetFieldDef.FieldValue == null) sheetFieldDef.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetFieldDef.FieldValue));
        if (sheetFieldDef.UiLabelMobile == null) sheetFieldDef.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobile));
        if (sheetFieldDef.UiLabelMobileRadioButton == null) sheetFieldDef.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobileRadioButton));
        Db.NonQ(command, paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
    }

    public static bool Update(SheetFieldDef sheetFieldDef, SheetFieldDef oldSheetFieldDef)
    {
        var command = "";
        if (sheetFieldDef.SheetDefNum != oldSheetFieldDef.SheetDefNum)
        {
            if (command != "") command += ",";
            command += "SheetDefNum = " + SOut.Long(sheetFieldDef.SheetDefNum) + "";
        }

        if (sheetFieldDef.FieldType != oldSheetFieldDef.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) sheetFieldDef.FieldType) + "";
        }

        if (sheetFieldDef.FieldName != oldSheetFieldDef.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(sheetFieldDef.FieldName) + "'";
        }

        if (sheetFieldDef.FieldValue != oldSheetFieldDef.FieldValue)
        {
            if (command != "") command += ",";
            command += "FieldValue = " + DbHelper.ParamChar + "paramFieldValue";
        }

        if (sheetFieldDef.FontSize != oldSheetFieldDef.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(sheetFieldDef.FontSize) + "";
        }

        if (sheetFieldDef.FontName != oldSheetFieldDef.FontName)
        {
            if (command != "") command += ",";
            command += "FontName = '" + SOut.String(sheetFieldDef.FontName) + "'";
        }

        if (sheetFieldDef.FontIsBold != oldSheetFieldDef.FontIsBold)
        {
            if (command != "") command += ",";
            command += "FontIsBold = " + SOut.Bool(sheetFieldDef.FontIsBold) + "";
        }

        if (sheetFieldDef.XPos != oldSheetFieldDef.XPos)
        {
            if (command != "") command += ",";
            command += "XPos = " + SOut.Int(sheetFieldDef.XPos) + "";
        }

        if (sheetFieldDef.YPos != oldSheetFieldDef.YPos)
        {
            if (command != "") command += ",";
            command += "YPos = " + SOut.Int(sheetFieldDef.YPos) + "";
        }

        if (sheetFieldDef.Width != oldSheetFieldDef.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(sheetFieldDef.Width) + "";
        }

        if (sheetFieldDef.Height != oldSheetFieldDef.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(sheetFieldDef.Height) + "";
        }

        if (sheetFieldDef.GrowthBehavior != oldSheetFieldDef.GrowthBehavior)
        {
            if (command != "") command += ",";
            command += "GrowthBehavior = " + SOut.Int((int) sheetFieldDef.GrowthBehavior) + "";
        }

        if (sheetFieldDef.RadioButtonValue != oldSheetFieldDef.RadioButtonValue)
        {
            if (command != "") command += ",";
            command += "RadioButtonValue = '" + SOut.String(sheetFieldDef.RadioButtonValue) + "'";
        }

        if (sheetFieldDef.RadioButtonGroup != oldSheetFieldDef.RadioButtonGroup)
        {
            if (command != "") command += ",";
            command += "RadioButtonGroup = '" + SOut.String(sheetFieldDef.RadioButtonGroup) + "'";
        }

        if (sheetFieldDef.IsRequired != oldSheetFieldDef.IsRequired)
        {
            if (command != "") command += ",";
            command += "IsRequired = " + SOut.Bool(sheetFieldDef.IsRequired) + "";
        }

        if (sheetFieldDef.TabOrder != oldSheetFieldDef.TabOrder)
        {
            if (command != "") command += ",";
            command += "TabOrder = " + SOut.Int(sheetFieldDef.TabOrder) + "";
        }

        if (sheetFieldDef.ReportableName != oldSheetFieldDef.ReportableName)
        {
            if (command != "") command += ",";
            command += "ReportableName = '" + SOut.String(sheetFieldDef.ReportableName) + "'";
        }

        if (sheetFieldDef.TextAlign != oldSheetFieldDef.TextAlign)
        {
            if (command != "") command += ",";
            command += "TextAlign = " + SOut.Int((int) sheetFieldDef.TextAlign) + "";
        }

        if (sheetFieldDef.IsPaymentOption != oldSheetFieldDef.IsPaymentOption)
        {
            if (command != "") command += ",";
            command += "IsPaymentOption = " + SOut.Bool(sheetFieldDef.IsPaymentOption) + "";
        }

        if (sheetFieldDef.IsLocked != oldSheetFieldDef.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(sheetFieldDef.IsLocked) + "";
        }

        if (sheetFieldDef.ItemColor != oldSheetFieldDef.ItemColor)
        {
            if (command != "") command += ",";
            command += "ItemColor = " + SOut.Int(sheetFieldDef.ItemColor.ToArgb()) + "";
        }

        if (sheetFieldDef.TabOrderMobile != oldSheetFieldDef.TabOrderMobile)
        {
            if (command != "") command += ",";
            command += "TabOrderMobile = " + SOut.Int(sheetFieldDef.TabOrderMobile) + "";
        }

        if (sheetFieldDef.UiLabelMobile != oldSheetFieldDef.UiLabelMobile)
        {
            if (command != "") command += ",";
            command += "UiLabelMobile = " + DbHelper.ParamChar + "paramUiLabelMobile";
        }

        if (sheetFieldDef.UiLabelMobileRadioButton != oldSheetFieldDef.UiLabelMobileRadioButton)
        {
            if (command != "") command += ",";
            command += "UiLabelMobileRadioButton = " + DbHelper.ParamChar + "paramUiLabelMobileRadioButton";
        }

        if (sheetFieldDef.LayoutMode != oldSheetFieldDef.LayoutMode)
        {
            if (command != "") command += ",";
            command += "LayoutMode = " + SOut.Int((int) sheetFieldDef.LayoutMode) + "";
        }

        if (sheetFieldDef.Language != oldSheetFieldDef.Language)
        {
            if (command != "") command += ",";
            command += "Language = '" + SOut.String(sheetFieldDef.Language) + "'";
        }

        if (sheetFieldDef.CanElectronicallySign != oldSheetFieldDef.CanElectronicallySign)
        {
            if (command != "") command += ",";
            command += "CanElectronicallySign = " + SOut.Bool(sheetFieldDef.CanElectronicallySign) + "";
        }

        if (sheetFieldDef.IsSigProvRestricted != oldSheetFieldDef.IsSigProvRestricted)
        {
            if (command != "") command += ",";
            command += "IsSigProvRestricted = " + SOut.Bool(sheetFieldDef.IsSigProvRestricted) + "";
        }

        if (command == "") return false;
        if (sheetFieldDef.FieldValue == null) sheetFieldDef.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetFieldDef.FieldValue));
        if (sheetFieldDef.UiLabelMobile == null) sheetFieldDef.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobile));
        if (sheetFieldDef.UiLabelMobileRadioButton == null) sheetFieldDef.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetFieldDef.UiLabelMobileRadioButton));
        command = "UPDATE sheetfielddef SET " + command
                                              + " WHERE SheetFieldDefNum = " + SOut.Long(sheetFieldDef.SheetFieldDefNum);
        Db.NonQ(command, paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        return true;
    }

    public static bool UpdateComparison(SheetFieldDef sheetFieldDef, SheetFieldDef oldSheetFieldDef)
    {
        if (sheetFieldDef.SheetDefNum != oldSheetFieldDef.SheetDefNum) return true;
        if (sheetFieldDef.FieldType != oldSheetFieldDef.FieldType) return true;
        if (sheetFieldDef.FieldName != oldSheetFieldDef.FieldName) return true;
        if (sheetFieldDef.FieldValue != oldSheetFieldDef.FieldValue) return true;
        if (sheetFieldDef.FontSize != oldSheetFieldDef.FontSize) return true;
        if (sheetFieldDef.FontName != oldSheetFieldDef.FontName) return true;
        if (sheetFieldDef.FontIsBold != oldSheetFieldDef.FontIsBold) return true;
        if (sheetFieldDef.XPos != oldSheetFieldDef.XPos) return true;
        if (sheetFieldDef.YPos != oldSheetFieldDef.YPos) return true;
        if (sheetFieldDef.Width != oldSheetFieldDef.Width) return true;
        if (sheetFieldDef.Height != oldSheetFieldDef.Height) return true;
        if (sheetFieldDef.GrowthBehavior != oldSheetFieldDef.GrowthBehavior) return true;
        if (sheetFieldDef.RadioButtonValue != oldSheetFieldDef.RadioButtonValue) return true;
        if (sheetFieldDef.RadioButtonGroup != oldSheetFieldDef.RadioButtonGroup) return true;
        if (sheetFieldDef.IsRequired != oldSheetFieldDef.IsRequired) return true;
        if (sheetFieldDef.TabOrder != oldSheetFieldDef.TabOrder) return true;
        if (sheetFieldDef.ReportableName != oldSheetFieldDef.ReportableName) return true;
        if (sheetFieldDef.TextAlign != oldSheetFieldDef.TextAlign) return true;
        if (sheetFieldDef.IsPaymentOption != oldSheetFieldDef.IsPaymentOption) return true;
        if (sheetFieldDef.IsLocked != oldSheetFieldDef.IsLocked) return true;
        if (sheetFieldDef.ItemColor != oldSheetFieldDef.ItemColor) return true;
        if (sheetFieldDef.TabOrderMobile != oldSheetFieldDef.TabOrderMobile) return true;
        if (sheetFieldDef.UiLabelMobile != oldSheetFieldDef.UiLabelMobile) return true;
        if (sheetFieldDef.UiLabelMobileRadioButton != oldSheetFieldDef.UiLabelMobileRadioButton) return true;
        if (sheetFieldDef.LayoutMode != oldSheetFieldDef.LayoutMode) return true;
        if (sheetFieldDef.Language != oldSheetFieldDef.Language) return true;
        if (sheetFieldDef.CanElectronicallySign != oldSheetFieldDef.CanElectronicallySign) return true;
        if (sheetFieldDef.IsSigProvRestricted != oldSheetFieldDef.IsSigProvRestricted) return true;
        return false;
    }

    public static void Delete(long sheetFieldDefNum)
    {
        var command = "DELETE FROM sheetfielddef "
                      + "WHERE SheetFieldDefNum = " + SOut.Long(sheetFieldDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSheetFieldDefNums)
    {
        if (listSheetFieldDefNums == null || listSheetFieldDefNums.Count == 0) return;
        var command = "DELETE FROM sheetfielddef "
                      + "WHERE SheetFieldDefNum IN(" + string.Join(",", listSheetFieldDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<SheetFieldDef> listNew, List<SheetFieldDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<SheetFieldDef>();
        var listUpdNew = new List<SheetFieldDef>();
        var listUpdDB = new List<SheetFieldDef>();
        var listDel = new List<SheetFieldDef>();
        listNew.Sort((x, y) => { return x.SheetFieldDefNum.CompareTo(y.SheetFieldDefNum); });
        listDB.Sort((x, y) => { return x.SheetFieldDefNum.CompareTo(y.SheetFieldDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        SheetFieldDef fieldNew;
        SheetFieldDef fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.SheetFieldDefNum < fieldDB.SheetFieldDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SheetFieldDefNum > fieldDB.SheetFieldDefNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.SheetFieldDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}