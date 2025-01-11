#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EFormFieldDefCrud
{
    public static EFormFieldDef SelectOne(long eFormFieldDefNum)
    {
        var command = "SELECT * FROM eformfielddef "
                      + "WHERE EFormFieldDefNum = " + SOut.Long(eFormFieldDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EFormFieldDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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

    public static long Insert(EFormFieldDef eFormFieldDef)
    {
        return Insert(eFormFieldDef, false);
    }

    public static long Insert(EFormFieldDef eFormFieldDef, bool useExistingPK)
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
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormFieldDef.ValueLabel));
        {
            eFormFieldDef.EFormFieldDefNum = Db.NonQ(command, true, "EFormFieldDefNum", "eFormFieldDef", paramValueLabel);
        }
        return eFormFieldDef.EFormFieldDefNum;
    }

    public static long InsertNoCache(EFormFieldDef eFormFieldDef)
    {
        return InsertNoCache(eFormFieldDef, false);
    }

    public static long InsertNoCache(EFormFieldDef eFormFieldDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eformfielddef (";
        if (isRandomKeys || useExistingPK) command += "EFormFieldDefNum,";
        command += "EFormDefNum,FieldType,DbLink,ValueLabel,ItemOrder,PickListVis,PickListDb,IsHorizStacking,IsTextWrap,Width,FontScale,IsRequired,ConditionalParent,ConditionalValue,LabelAlign,SpaceBelow,ReportableName,IsLocked,Border,IsWidthPercentage,MinWidth,WidthLabel,SpaceToRight) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eFormFieldDef.EFormFieldDefNum) + ",";
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
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormFieldDef.ValueLabel));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramValueLabel);
        else
            eFormFieldDef.EFormFieldDefNum = Db.NonQ(command, true, "EFormFieldDefNum", "eFormFieldDef", paramValueLabel);
        return eFormFieldDef.EFormFieldDefNum;
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
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormFieldDef.ValueLabel));
        Db.NonQ(command, paramValueLabel);
    }

    public static bool Update(EFormFieldDef eFormFieldDef, EFormFieldDef oldEFormFieldDef)
    {
        var command = "";
        if (eFormFieldDef.EFormDefNum != oldEFormFieldDef.EFormDefNum)
        {
            if (command != "") command += ",";
            command += "EFormDefNum = " + SOut.Long(eFormFieldDef.EFormDefNum) + "";
        }

        if (eFormFieldDef.FieldType != oldEFormFieldDef.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) eFormFieldDef.FieldType) + "";
        }

        if (eFormFieldDef.DbLink != oldEFormFieldDef.DbLink)
        {
            if (command != "") command += ",";
            command += "DbLink = '" + SOut.String(eFormFieldDef.DbLink) + "'";
        }

        if (eFormFieldDef.ValueLabel != oldEFormFieldDef.ValueLabel)
        {
            if (command != "") command += ",";
            command += "ValueLabel = " + DbHelper.ParamChar + "paramValueLabel";
        }

        if (eFormFieldDef.ItemOrder != oldEFormFieldDef.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(eFormFieldDef.ItemOrder) + "";
        }

        if (eFormFieldDef.PickListVis != oldEFormFieldDef.PickListVis)
        {
            if (command != "") command += ",";
            command += "PickListVis = '" + SOut.String(eFormFieldDef.PickListVis) + "'";
        }

        if (eFormFieldDef.PickListDb != oldEFormFieldDef.PickListDb)
        {
            if (command != "") command += ",";
            command += "PickListDb = '" + SOut.String(eFormFieldDef.PickListDb) + "'";
        }

        if (eFormFieldDef.IsHorizStacking != oldEFormFieldDef.IsHorizStacking)
        {
            if (command != "") command += ",";
            command += "IsHorizStacking = " + SOut.Bool(eFormFieldDef.IsHorizStacking) + "";
        }

        if (eFormFieldDef.IsTextWrap != oldEFormFieldDef.IsTextWrap)
        {
            if (command != "") command += ",";
            command += "IsTextWrap = " + SOut.Bool(eFormFieldDef.IsTextWrap) + "";
        }

        if (eFormFieldDef.Width != oldEFormFieldDef.Width)
        {
            if (command != "") command += ",";
            command += "Width = " + SOut.Int(eFormFieldDef.Width) + "";
        }

        if (eFormFieldDef.FontScale != oldEFormFieldDef.FontScale)
        {
            if (command != "") command += ",";
            command += "FontScale = " + SOut.Int(eFormFieldDef.FontScale) + "";
        }

        if (eFormFieldDef.IsRequired != oldEFormFieldDef.IsRequired)
        {
            if (command != "") command += ",";
            command += "IsRequired = " + SOut.Bool(eFormFieldDef.IsRequired) + "";
        }

        if (eFormFieldDef.ConditionalParent != oldEFormFieldDef.ConditionalParent)
        {
            if (command != "") command += ",";
            command += "ConditionalParent = '" + SOut.String(eFormFieldDef.ConditionalParent) + "'";
        }

        if (eFormFieldDef.ConditionalValue != oldEFormFieldDef.ConditionalValue)
        {
            if (command != "") command += ",";
            command += "ConditionalValue = '" + SOut.String(eFormFieldDef.ConditionalValue) + "'";
        }

        if (eFormFieldDef.LabelAlign != oldEFormFieldDef.LabelAlign)
        {
            if (command != "") command += ",";
            command += "LabelAlign = " + SOut.Int((int) eFormFieldDef.LabelAlign) + "";
        }

        if (eFormFieldDef.SpaceBelow != oldEFormFieldDef.SpaceBelow)
        {
            if (command != "") command += ",";
            command += "SpaceBelow = " + SOut.Int(eFormFieldDef.SpaceBelow) + "";
        }

        if (eFormFieldDef.ReportableName != oldEFormFieldDef.ReportableName)
        {
            if (command != "") command += ",";
            command += "ReportableName = '" + SOut.String(eFormFieldDef.ReportableName) + "'";
        }

        if (eFormFieldDef.IsLocked != oldEFormFieldDef.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(eFormFieldDef.IsLocked) + "";
        }

        if (eFormFieldDef.Border != oldEFormFieldDef.Border)
        {
            if (command != "") command += ",";
            command += "Border = " + SOut.Int((int) eFormFieldDef.Border) + "";
        }

        if (eFormFieldDef.IsWidthPercentage != oldEFormFieldDef.IsWidthPercentage)
        {
            if (command != "") command += ",";
            command += "IsWidthPercentage = " + SOut.Bool(eFormFieldDef.IsWidthPercentage) + "";
        }

        if (eFormFieldDef.MinWidth != oldEFormFieldDef.MinWidth)
        {
            if (command != "") command += ",";
            command += "MinWidth = " + SOut.Int(eFormFieldDef.MinWidth) + "";
        }

        if (eFormFieldDef.WidthLabel != oldEFormFieldDef.WidthLabel)
        {
            if (command != "") command += ",";
            command += "WidthLabel = " + SOut.Int(eFormFieldDef.WidthLabel) + "";
        }

        if (eFormFieldDef.SpaceToRight != oldEFormFieldDef.SpaceToRight)
        {
            if (command != "") command += ",";
            command += "SpaceToRight = " + SOut.Int(eFormFieldDef.SpaceToRight) + "";
        }

        if (command == "") return false;
        if (eFormFieldDef.ValueLabel == null) eFormFieldDef.ValueLabel = "";
        var paramValueLabel = new OdSqlParameter("paramValueLabel", OdDbType.Text, SOut.StringParam(eFormFieldDef.ValueLabel));
        command = "UPDATE eformfielddef SET " + command
                                              + " WHERE EFormFieldDefNum = " + SOut.Long(eFormFieldDef.EFormFieldDefNum);
        Db.NonQ(command, paramValueLabel);
        return true;
    }

    public static bool UpdateComparison(EFormFieldDef eFormFieldDef, EFormFieldDef oldEFormFieldDef)
    {
        if (eFormFieldDef.EFormDefNum != oldEFormFieldDef.EFormDefNum) return true;
        if (eFormFieldDef.FieldType != oldEFormFieldDef.FieldType) return true;
        if (eFormFieldDef.DbLink != oldEFormFieldDef.DbLink) return true;
        if (eFormFieldDef.ValueLabel != oldEFormFieldDef.ValueLabel) return true;
        if (eFormFieldDef.ItemOrder != oldEFormFieldDef.ItemOrder) return true;
        if (eFormFieldDef.PickListVis != oldEFormFieldDef.PickListVis) return true;
        if (eFormFieldDef.PickListDb != oldEFormFieldDef.PickListDb) return true;
        if (eFormFieldDef.IsHorizStacking != oldEFormFieldDef.IsHorizStacking) return true;
        if (eFormFieldDef.IsTextWrap != oldEFormFieldDef.IsTextWrap) return true;
        if (eFormFieldDef.Width != oldEFormFieldDef.Width) return true;
        if (eFormFieldDef.FontScale != oldEFormFieldDef.FontScale) return true;
        if (eFormFieldDef.IsRequired != oldEFormFieldDef.IsRequired) return true;
        if (eFormFieldDef.ConditionalParent != oldEFormFieldDef.ConditionalParent) return true;
        if (eFormFieldDef.ConditionalValue != oldEFormFieldDef.ConditionalValue) return true;
        if (eFormFieldDef.LabelAlign != oldEFormFieldDef.LabelAlign) return true;
        if (eFormFieldDef.SpaceBelow != oldEFormFieldDef.SpaceBelow) return true;
        if (eFormFieldDef.ReportableName != oldEFormFieldDef.ReportableName) return true;
        if (eFormFieldDef.IsLocked != oldEFormFieldDef.IsLocked) return true;
        if (eFormFieldDef.Border != oldEFormFieldDef.Border) return true;
        if (eFormFieldDef.IsWidthPercentage != oldEFormFieldDef.IsWidthPercentage) return true;
        if (eFormFieldDef.MinWidth != oldEFormFieldDef.MinWidth) return true;
        if (eFormFieldDef.WidthLabel != oldEFormFieldDef.WidthLabel) return true;
        if (eFormFieldDef.SpaceToRight != oldEFormFieldDef.SpaceToRight) return true;
        return false;
    }

    public static void Delete(long eFormFieldDefNum)
    {
        var command = "DELETE FROM eformfielddef "
                      + "WHERE EFormFieldDefNum = " + SOut.Long(eFormFieldDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEFormFieldDefNums)
    {
        if (listEFormFieldDefNums == null || listEFormFieldDefNums.Count == 0) return;
        var command = "DELETE FROM eformfielddef "
                      + "WHERE EFormFieldDefNum IN(" + string.Join(",", listEFormFieldDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<EFormFieldDef> listNew, List<EFormFieldDef> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<EFormFieldDef>();
        var listUpdNew = new List<EFormFieldDef>();
        var listUpdDB = new List<EFormFieldDef>();
        var listDel = new List<EFormFieldDef>();
        listNew.Sort((x, y) => { return x.EFormFieldDefNum.CompareTo(y.EFormFieldDefNum); });
        listDB.Sort((x, y) => { return x.EFormFieldDefNum.CompareTo(y.EFormFieldDefNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        EFormFieldDef fieldNew;
        EFormFieldDef fieldDB;
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

            if (fieldNew.EFormFieldDefNum < fieldDB.EFormFieldDefNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.EFormFieldDefNum > fieldDB.EFormFieldDefNum)
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

        DeleteMany(listDel.Select(x => x.EFormFieldDefNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}