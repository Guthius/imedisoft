#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SheetFieldCrud
{
    public static SheetField SelectOne(long sheetFieldNum)
    {
        var command = "SELECT * FROM sheetfield "
                      + "WHERE SheetFieldNum = " + SOut.Long(sheetFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SheetField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SheetField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SheetField> TableToList(DataTable table)
    {
        var retVal = new List<SheetField>();
        SheetField sheetField;
        foreach (DataRow row in table.Rows)
        {
            sheetField = new SheetField();
            sheetField.SheetFieldNum = SIn.Long(row["SheetFieldNum"].ToString());
            sheetField.SheetNum = SIn.Long(row["SheetNum"].ToString());
            sheetField.FieldType = (SheetFieldType) SIn.Int(row["FieldType"].ToString());
            sheetField.FieldName = SIn.String(row["FieldName"].ToString());
            sheetField.FieldValue = SIn.String(row["FieldValue"].ToString());
            sheetField.FontSize = SIn.Float(row["FontSize"].ToString());
            sheetField.FontName = SIn.String(row["FontName"].ToString());
            sheetField.FontIsBold = SIn.Bool(row["FontIsBold"].ToString());
            sheetField.XPos = SIn.Int(row["XPos"].ToString());
            sheetField.YPos = SIn.Int(row["YPos"].ToString());
            sheetField.Width = SIn.Int(row["Width"].ToString());
            sheetField.Height = SIn.Int(row["Height"].ToString());
            sheetField.GrowthBehavior = (GrowthBehaviorEnum) SIn.Int(row["GrowthBehavior"].ToString());
            sheetField.RadioButtonValue = SIn.String(row["RadioButtonValue"].ToString());
            sheetField.RadioButtonGroup = SIn.String(row["RadioButtonGroup"].ToString());
            sheetField.IsRequired = SIn.Bool(row["IsRequired"].ToString());
            sheetField.TabOrder = SIn.Int(row["TabOrder"].ToString());
            sheetField.ReportableName = SIn.String(row["ReportableName"].ToString());
            sheetField.TextAlign = (HorizontalAlignment) SIn.Int(row["TextAlign"].ToString());
            sheetField.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            sheetField.ItemColor = Color.FromArgb(SIn.Int(row["ItemColor"].ToString()));
            sheetField.DateTimeSig = SIn.DateTime(row["DateTimeSig"].ToString());
            sheetField.TabOrderMobile = SIn.Int(row["TabOrderMobile"].ToString());
            sheetField.UiLabelMobile = SIn.String(row["UiLabelMobile"].ToString());
            sheetField.UiLabelMobileRadioButton = SIn.String(row["UiLabelMobileRadioButton"].ToString());
            sheetField.SheetFieldDefNum = SIn.Long(row["SheetFieldDefNum"].ToString());
            sheetField.CanElectronicallySign = SIn.Bool(row["CanElectronicallySign"].ToString());
            sheetField.IsSigProvRestricted = SIn.Bool(row["IsSigProvRestricted"].ToString());
            retVal.Add(sheetField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SheetField> listSheetFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SheetField";
        var table = new DataTable(tableName);
        table.Columns.Add("SheetFieldNum");
        table.Columns.Add("SheetNum");
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
        table.Columns.Add("IsLocked");
        table.Columns.Add("ItemColor");
        table.Columns.Add("DateTimeSig");
        table.Columns.Add("TabOrderMobile");
        table.Columns.Add("UiLabelMobile");
        table.Columns.Add("UiLabelMobileRadioButton");
        table.Columns.Add("SheetFieldDefNum");
        table.Columns.Add("CanElectronicallySign");
        table.Columns.Add("IsSigProvRestricted");
        foreach (var sheetField in listSheetFields)
            table.Rows.Add(SOut.Long(sheetField.SheetFieldNum), SOut.Long(sheetField.SheetNum), SOut.Int((int) sheetField.FieldType), sheetField.FieldName, sheetField.FieldValue, SOut.Float(sheetField.FontSize), sheetField.FontName, SOut.Bool(sheetField.FontIsBold), SOut.Int(sheetField.XPos), SOut.Int(sheetField.YPos), SOut.Int(sheetField.Width), SOut.Int(sheetField.Height), SOut.Int((int) sheetField.GrowthBehavior), sheetField.RadioButtonValue, sheetField.RadioButtonGroup, SOut.Bool(sheetField.IsRequired), SOut.Int(sheetField.TabOrder), sheetField.ReportableName, SOut.Int((int) sheetField.TextAlign), SOut.Bool(sheetField.IsLocked), SOut.Int(sheetField.ItemColor.ToArgb()), SOut.DateT(sheetField.DateTimeSig, false), SOut.Int(sheetField.TabOrderMobile), sheetField.UiLabelMobile, sheetField.UiLabelMobileRadioButton, SOut.Long(sheetField.SheetFieldDefNum), SOut.Bool(sheetField.CanElectronicallySign), SOut.Bool(sheetField.IsSigProvRestricted));
        return table;
    }

    public static long Insert(SheetField sheetField)
    {
        return Insert(sheetField, false);
    }

    public static long Insert(SheetField sheetField, bool useExistingPK)
    {
        var command = "INSERT INTO sheetfield (";

        command += "SheetNum,FieldType,FieldName,FieldValue,FontSize,FontName,FontIsBold,XPos,YPos,Width,Height,GrowthBehavior,RadioButtonValue,RadioButtonGroup,IsRequired,TabOrder,ReportableName,TextAlign,IsLocked,ItemColor,DateTimeSig,TabOrderMobile,UiLabelMobile,UiLabelMobileRadioButton,SheetFieldDefNum,CanElectronicallySign,IsSigProvRestricted) VALUES(";

        command +=
            SOut.Long(sheetField.SheetNum) + ","
                                           + SOut.Int((int) sheetField.FieldType) + ","
                                           + "'" + SOut.String(sheetField.FieldName) + "',"
                                           + DbHelper.ParamChar + "paramFieldValue,"
                                           + SOut.Float(sheetField.FontSize) + ","
                                           + "'" + SOut.String(sheetField.FontName) + "',"
                                           + SOut.Bool(sheetField.FontIsBold) + ","
                                           + SOut.Int(sheetField.XPos) + ","
                                           + SOut.Int(sheetField.YPos) + ","
                                           + SOut.Int(sheetField.Width) + ","
                                           + SOut.Int(sheetField.Height) + ","
                                           + SOut.Int((int) sheetField.GrowthBehavior) + ","
                                           + "'" + SOut.String(sheetField.RadioButtonValue) + "',"
                                           + "'" + SOut.String(sheetField.RadioButtonGroup) + "',"
                                           + SOut.Bool(sheetField.IsRequired) + ","
                                           + SOut.Int(sheetField.TabOrder) + ","
                                           + "'" + SOut.String(sheetField.ReportableName) + "',"
                                           + SOut.Int((int) sheetField.TextAlign) + ","
                                           + SOut.Bool(sheetField.IsLocked) + ","
                                           + SOut.Int(sheetField.ItemColor.ToArgb()) + ","
                                           + SOut.DateT(sheetField.DateTimeSig) + ","
                                           + SOut.Int(sheetField.TabOrderMobile) + ","
                                           + DbHelper.ParamChar + "paramUiLabelMobile,"
                                           + DbHelper.ParamChar + "paramUiLabelMobileRadioButton,"
                                           + SOut.Long(sheetField.SheetFieldDefNum) + ","
                                           + SOut.Bool(sheetField.CanElectronicallySign) + ","
                                           + SOut.Bool(sheetField.IsSigProvRestricted) + ")";
        if (sheetField.FieldValue == null) sheetField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetField.FieldValue));
        if (sheetField.UiLabelMobile == null) sheetField.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobile));
        if (sheetField.UiLabelMobileRadioButton == null) sheetField.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobileRadioButton));
        {
            sheetField.SheetFieldNum = Db.NonQ(command, true, "SheetFieldNum", "sheetField", paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        }
        return sheetField.SheetFieldNum;
    }

    public static void InsertMany(List<SheetField> listSheetFields)
    {
        InsertMany(listSheetFields, false);
    }

    public static void InsertMany(List<SheetField> listSheetFields, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSheetFields.Count)
        {
            var sheetField = listSheetFields[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO sheetfield (");
                if (useExistingPK) sbCommands.Append("SheetFieldNum,");
                sbCommands.Append("SheetNum,FieldType,FieldName,FieldValue,FontSize,FontName,FontIsBold,XPos,YPos,Width,Height,GrowthBehavior,RadioButtonValue,RadioButtonGroup,IsRequired,TabOrder,ReportableName,TextAlign,IsLocked,ItemColor,DateTimeSig,TabOrderMobile,UiLabelMobile,UiLabelMobileRadioButton,SheetFieldDefNum,CanElectronicallySign,IsSigProvRestricted) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(sheetField.SheetFieldNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(sheetField.SheetNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) sheetField.FieldType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.FieldName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.FieldValue) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Float(sheetField.FontSize));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.FontName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(sheetField.FontIsBold));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.XPos));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.YPos));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.Width));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.Height));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) sheetField.GrowthBehavior));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.RadioButtonValue) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.RadioButtonGroup) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(sheetField.IsRequired));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.TabOrder));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.ReportableName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) sheetField.TextAlign));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(sheetField.IsLocked));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.ItemColor.ToArgb()));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(sheetField.DateTimeSig));
            sbRow.Append(",");
            sbRow.Append(SOut.Int(sheetField.TabOrderMobile));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.UiLabelMobile) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(sheetField.UiLabelMobileRadioButton) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(sheetField.SheetFieldDefNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(sheetField.CanElectronicallySign));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(sheetField.IsSigProvRestricted));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listSheetFields.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(SheetField sheetField)
    {
        return InsertNoCache(sheetField, false);
    }

    public static long InsertNoCache(SheetField sheetField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sheetfield (";
        if (isRandomKeys || useExistingPK) command += "SheetFieldNum,";
        command += "SheetNum,FieldType,FieldName,FieldValue,FontSize,FontName,FontIsBold,XPos,YPos,Width,Height,GrowthBehavior,RadioButtonValue,RadioButtonGroup,IsRequired,TabOrder,ReportableName,TextAlign,IsLocked,ItemColor,DateTimeSig,TabOrderMobile,UiLabelMobile,UiLabelMobileRadioButton,SheetFieldDefNum,CanElectronicallySign,IsSigProvRestricted) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sheetField.SheetFieldNum) + ",";
        command +=
            SOut.Long(sheetField.SheetNum) + ","
                                           + SOut.Int((int) sheetField.FieldType) + ","
                                           + "'" + SOut.String(sheetField.FieldName) + "',"
                                           + DbHelper.ParamChar + "paramFieldValue,"
                                           + SOut.Float(sheetField.FontSize) + ","
                                           + "'" + SOut.String(sheetField.FontName) + "',"
                                           + SOut.Bool(sheetField.FontIsBold) + ","
                                           + SOut.Int(sheetField.XPos) + ","
                                           + SOut.Int(sheetField.YPos) + ","
                                           + SOut.Int(sheetField.Width) + ","
                                           + SOut.Int(sheetField.Height) + ","
                                           + SOut.Int((int) sheetField.GrowthBehavior) + ","
                                           + "'" + SOut.String(sheetField.RadioButtonValue) + "',"
                                           + "'" + SOut.String(sheetField.RadioButtonGroup) + "',"
                                           + SOut.Bool(sheetField.IsRequired) + ","
                                           + SOut.Int(sheetField.TabOrder) + ","
                                           + "'" + SOut.String(sheetField.ReportableName) + "',"
                                           + SOut.Int((int) sheetField.TextAlign) + ","
                                           + SOut.Bool(sheetField.IsLocked) + ","
                                           + SOut.Int(sheetField.ItemColor.ToArgb()) + ","
                                           + SOut.DateT(sheetField.DateTimeSig) + ","
                                           + SOut.Int(sheetField.TabOrderMobile) + ","
                                           + DbHelper.ParamChar + "paramUiLabelMobile,"
                                           + DbHelper.ParamChar + "paramUiLabelMobileRadioButton,"
                                           + SOut.Long(sheetField.SheetFieldDefNum) + ","
                                           + SOut.Bool(sheetField.CanElectronicallySign) + ","
                                           + SOut.Bool(sheetField.IsSigProvRestricted) + ")";
        if (sheetField.FieldValue == null) sheetField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetField.FieldValue));
        if (sheetField.UiLabelMobile == null) sheetField.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobile));
        if (sheetField.UiLabelMobileRadioButton == null) sheetField.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobileRadioButton));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        else
            sheetField.SheetFieldNum = Db.NonQ(command, true, "SheetFieldNum", "sheetField", paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        return sheetField.SheetFieldNum;
    }

    public static void Update(SheetField sheetField)
    {
        var command = "UPDATE sheetfield SET "
                      + "SheetNum                =  " + SOut.Long(sheetField.SheetNum) + ", "
                      + "FieldType               =  " + SOut.Int((int) sheetField.FieldType) + ", "
                      + "FieldName               = '" + SOut.String(sheetField.FieldName) + "', "
                      + "FieldValue              =  " + DbHelper.ParamChar + "paramFieldValue, "
                      + "FontSize                =  " + SOut.Float(sheetField.FontSize) + ", "
                      + "FontName                = '" + SOut.String(sheetField.FontName) + "', "
                      + "FontIsBold              =  " + SOut.Bool(sheetField.FontIsBold) + ", "
                      + "XPos                    =  " + SOut.Int(sheetField.XPos) + ", "
                      + "YPos                    =  " + SOut.Int(sheetField.YPos) + ", "
                      + "Width                   =  " + SOut.Int(sheetField.Width) + ", "
                      + "Height                  =  " + SOut.Int(sheetField.Height) + ", "
                      + "GrowthBehavior          =  " + SOut.Int((int) sheetField.GrowthBehavior) + ", "
                      + "RadioButtonValue        = '" + SOut.String(sheetField.RadioButtonValue) + "', "
                      + "RadioButtonGroup        = '" + SOut.String(sheetField.RadioButtonGroup) + "', "
                      + "IsRequired              =  " + SOut.Bool(sheetField.IsRequired) + ", "
                      + "TabOrder                =  " + SOut.Int(sheetField.TabOrder) + ", "
                      + "ReportableName          = '" + SOut.String(sheetField.ReportableName) + "', "
                      + "TextAlign               =  " + SOut.Int((int) sheetField.TextAlign) + ", "
                      + "IsLocked                =  " + SOut.Bool(sheetField.IsLocked) + ", "
                      + "ItemColor               =  " + SOut.Int(sheetField.ItemColor.ToArgb()) + ", "
                      + "DateTimeSig             =  " + SOut.DateT(sheetField.DateTimeSig) + ", "
                      + "TabOrderMobile          =  " + SOut.Int(sheetField.TabOrderMobile) + ", "
                      + "UiLabelMobile           =  " + DbHelper.ParamChar + "paramUiLabelMobile, "
                      + "UiLabelMobileRadioButton=  " + DbHelper.ParamChar + "paramUiLabelMobileRadioButton, "
                      + "SheetFieldDefNum        =  " + SOut.Long(sheetField.SheetFieldDefNum) + ", "
                      + "CanElectronicallySign   =  " + SOut.Bool(sheetField.CanElectronicallySign) + ", "
                      + "IsSigProvRestricted     =  " + SOut.Bool(sheetField.IsSigProvRestricted) + " "
                      + "WHERE SheetFieldNum = " + SOut.Long(sheetField.SheetFieldNum);
        if (sheetField.FieldValue == null) sheetField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetField.FieldValue));
        if (sheetField.UiLabelMobile == null) sheetField.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobile));
        if (sheetField.UiLabelMobileRadioButton == null) sheetField.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobileRadioButton));
        Db.NonQ(command, paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
    }

    public static bool Update(SheetField sheetField, SheetField oldSheetField)
    {
        var command = "";
        if (sheetField.SheetNum != oldSheetField.SheetNum)
        {
            if (command != "") command += ",";
            command += "SheetNum = " + SOut.Long(sheetField.SheetNum) + "";
        }

        if (sheetField.FieldType != oldSheetField.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) sheetField.FieldType) + "";
        }

        if (sheetField.FieldName != oldSheetField.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(sheetField.FieldName) + "'";
        }

        if (sheetField.FieldValue != oldSheetField.FieldValue)
        {
            if (command != "") command += ",";
            command += "FieldValue = " + DbHelper.ParamChar + "paramFieldValue";
        }

        if (sheetField.FontSize != oldSheetField.FontSize)
        {
            if (command != "") command += ",";
            command += "FontSize = " + SOut.Float(sheetField.FontSize) + "";
        }

        if (sheetField.FontName != oldSheetField.FontName)
        {
            if (command != "") command += ",";
            command += "FontName = '" + SOut.String(sheetField.FontName) + "'";
        }

        if (sheetField.FontIsBold != oldSheetField.FontIsBold)
        {
            if (command != "") command += ",";
            command += "FontIsBold = " + SOut.Bool(sheetField.FontIsBold) + "";
        }

        if (sheetField.XPos != oldSheetField.XPos)
        {
            if (command != "") command += ",";
            command += "XPos = " + SOut.Int(sheetField.XPos) + "";
        }

        if (sheetField.YPos != oldSheetField.YPos)
        {
            if (command != "") command += ",";
            command += "YPos = " + SOut.Int(sheetField.YPos) + "";
        }

        if (sheetField.Width != oldSheetField.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(sheetField.Width) + "";
        }

        if (sheetField.Height != oldSheetField.Height)
        {
            if (command != "") command += ",";
            command += "Height = " + SOut.Int(sheetField.Height) + "";
        }

        if (sheetField.GrowthBehavior != oldSheetField.GrowthBehavior)
        {
            if (command != "") command += ",";
            command += "GrowthBehavior = " + SOut.Int((int) sheetField.GrowthBehavior) + "";
        }

        if (sheetField.RadioButtonValue != oldSheetField.RadioButtonValue)
        {
            if (command != "") command += ",";
            command += "RadioButtonValue = '" + SOut.String(sheetField.RadioButtonValue) + "'";
        }

        if (sheetField.RadioButtonGroup != oldSheetField.RadioButtonGroup)
        {
            if (command != "") command += ",";
            command += "RadioButtonGroup = '" + SOut.String(sheetField.RadioButtonGroup) + "'";
        }

        if (sheetField.IsRequired != oldSheetField.IsRequired)
        {
            if (command != "") command += ",";
            command += "IsRequired = " + SOut.Bool(sheetField.IsRequired) + "";
        }

        if (sheetField.TabOrder != oldSheetField.TabOrder)
        {
            if (command != "") command += ",";
            command += "TabOrder = " + SOut.Int(sheetField.TabOrder) + "";
        }

        if (sheetField.ReportableName != oldSheetField.ReportableName)
        {
            if (command != "") command += ",";
            command += "ReportableName = '" + SOut.String(sheetField.ReportableName) + "'";
        }

        if (sheetField.TextAlign != oldSheetField.TextAlign)
        {
            if (command != "") command += ",";
            command += "TextAlign = " + SOut.Int((int) sheetField.TextAlign) + "";
        }

        if (sheetField.IsLocked != oldSheetField.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(sheetField.IsLocked) + "";
        }

        if (sheetField.ItemColor != oldSheetField.ItemColor)
        {
            if (command != "") command += ",";
            command += "ItemColor = " + SOut.Int(sheetField.ItemColor.ToArgb()) + "";
        }

        if (sheetField.DateTimeSig != oldSheetField.DateTimeSig)
        {
            if (command != "") command += ",";
            command += "DateTimeSig = " + SOut.DateT(sheetField.DateTimeSig) + "";
        }

        if (sheetField.TabOrderMobile != oldSheetField.TabOrderMobile)
        {
            if (command != "") command += ",";
            command += "TabOrderMobile = " + SOut.Int(sheetField.TabOrderMobile) + "";
        }

        if (sheetField.UiLabelMobile != oldSheetField.UiLabelMobile)
        {
            if (command != "") command += ",";
            command += "UiLabelMobile = " + DbHelper.ParamChar + "paramUiLabelMobile";
        }

        if (sheetField.UiLabelMobileRadioButton != oldSheetField.UiLabelMobileRadioButton)
        {
            if (command != "") command += ",";
            command += "UiLabelMobileRadioButton = " + DbHelper.ParamChar + "paramUiLabelMobileRadioButton";
        }

        if (sheetField.SheetFieldDefNum != oldSheetField.SheetFieldDefNum)
        {
            if (command != "") command += ",";
            command += "SheetFieldDefNum = " + SOut.Long(sheetField.SheetFieldDefNum) + "";
        }

        if (sheetField.CanElectronicallySign != oldSheetField.CanElectronicallySign)
        {
            if (command != "") command += ",";
            command += "CanElectronicallySign = " + SOut.Bool(sheetField.CanElectronicallySign) + "";
        }

        if (sheetField.IsSigProvRestricted != oldSheetField.IsSigProvRestricted)
        {
            if (command != "") command += ",";
            command += "IsSigProvRestricted = " + SOut.Bool(sheetField.IsSigProvRestricted) + "";
        }

        if (command == "") return false;
        if (sheetField.FieldValue == null) sheetField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(sheetField.FieldValue));
        if (sheetField.UiLabelMobile == null) sheetField.UiLabelMobile = "";
        var paramUiLabelMobile = new OdSqlParameter("paramUiLabelMobile", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobile));
        if (sheetField.UiLabelMobileRadioButton == null) sheetField.UiLabelMobileRadioButton = "";
        var paramUiLabelMobileRadioButton = new OdSqlParameter("paramUiLabelMobileRadioButton", OdDbType.Text, SOut.StringParam(sheetField.UiLabelMobileRadioButton));
        command = "UPDATE sheetfield SET " + command
                                           + " WHERE SheetFieldNum = " + SOut.Long(sheetField.SheetFieldNum);
        Db.NonQ(command, paramFieldValue, paramUiLabelMobile, paramUiLabelMobileRadioButton);
        return true;
    }

    public static bool UpdateComparison(SheetField sheetField, SheetField oldSheetField)
    {
        if (sheetField.SheetNum != oldSheetField.SheetNum) return true;
        if (sheetField.FieldType != oldSheetField.FieldType) return true;
        if (sheetField.FieldName != oldSheetField.FieldName) return true;
        if (sheetField.FieldValue != oldSheetField.FieldValue) return true;
        if (sheetField.FontSize != oldSheetField.FontSize) return true;
        if (sheetField.FontName != oldSheetField.FontName) return true;
        if (sheetField.FontIsBold != oldSheetField.FontIsBold) return true;
        if (sheetField.XPos != oldSheetField.XPos) return true;
        if (sheetField.YPos != oldSheetField.YPos) return true;
        if (sheetField.Width != oldSheetField.Width) return true;
        if (sheetField.Height != oldSheetField.Height) return true;
        if (sheetField.GrowthBehavior != oldSheetField.GrowthBehavior) return true;
        if (sheetField.RadioButtonValue != oldSheetField.RadioButtonValue) return true;
        if (sheetField.RadioButtonGroup != oldSheetField.RadioButtonGroup) return true;
        if (sheetField.IsRequired != oldSheetField.IsRequired) return true;
        if (sheetField.TabOrder != oldSheetField.TabOrder) return true;
        if (sheetField.ReportableName != oldSheetField.ReportableName) return true;
        if (sheetField.TextAlign != oldSheetField.TextAlign) return true;
        if (sheetField.IsLocked != oldSheetField.IsLocked) return true;
        if (sheetField.ItemColor != oldSheetField.ItemColor) return true;
        if (sheetField.DateTimeSig != oldSheetField.DateTimeSig) return true;
        if (sheetField.TabOrderMobile != oldSheetField.TabOrderMobile) return true;
        if (sheetField.UiLabelMobile != oldSheetField.UiLabelMobile) return true;
        if (sheetField.UiLabelMobileRadioButton != oldSheetField.UiLabelMobileRadioButton) return true;
        if (sheetField.SheetFieldDefNum != oldSheetField.SheetFieldDefNum) return true;
        if (sheetField.CanElectronicallySign != oldSheetField.CanElectronicallySign) return true;
        if (sheetField.IsSigProvRestricted != oldSheetField.IsSigProvRestricted) return true;
        return false;
    }

    public static void Delete(long sheetFieldNum)
    {
        var command = "DELETE FROM sheetfield "
                      + "WHERE SheetFieldNum = " + SOut.Long(sheetFieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSheetFieldNums)
    {
        if (listSheetFieldNums == null || listSheetFieldNums.Count == 0) return;
        var command = "DELETE FROM sheetfield "
                      + "WHERE SheetFieldNum IN(" + string.Join(",", listSheetFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<SheetField> listNew, List<SheetField> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<SheetField>();
        var listUpdNew = new List<SheetField>();
        var listUpdDB = new List<SheetField>();
        var listDel = new List<SheetField>();
        listNew.Sort((x, y) => { return x.SheetFieldNum.CompareTo(y.SheetFieldNum); });
        listDB.Sort((x, y) => { return x.SheetFieldNum.CompareTo(y.SheetFieldNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        SheetField fieldNew;
        SheetField fieldDB;
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

            if (fieldNew.SheetFieldNum < fieldDB.SheetFieldNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SheetFieldNum > fieldDB.SheetFieldNum)
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

        DeleteMany(listDel.Select(x => x.SheetFieldNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}