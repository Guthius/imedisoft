#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EFormDefCrud
{
    public static EFormDef SelectOne(long eFormDefNum)
    {
        var command = "SELECT * FROM eformdef "
                      + "WHERE EFormDefNum = " + SOut.Long(eFormDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EFormDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EFormDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EFormDef> TableToList(DataTable table)
    {
        var retVal = new List<EFormDef>();
        EFormDef eFormDef;
        foreach (DataRow row in table.Rows)
        {
            eFormDef = new EFormDef();
            eFormDef.EFormDefNum = SIn.Long(row["EFormDefNum"].ToString());
            eFormDef.FormType = (EnumEFormType) SIn.Int(row["FormType"].ToString());
            eFormDef.Description = SIn.String(row["Description"].ToString());
            eFormDef.DateTCreated = SIn.DateTime(row["DateTCreated"].ToString());
            eFormDef.IsInternalHidden = SIn.Bool(row["IsInternalHidden"].ToString());
            eFormDef.MaxWidth = SIn.Int(row["MaxWidth"].ToString());
            eFormDef.RevID = SIn.Int(row["RevID"].ToString());
            eFormDef.ShowLabelsBold = SIn.Bool(row["ShowLabelsBold"].ToString());
            eFormDef.SpaceBelowEachField = SIn.Int(row["SpaceBelowEachField"].ToString());
            eFormDef.SpaceToRightEachField = SIn.Int(row["SpaceToRightEachField"].ToString());
            eFormDef.SaveImageCategory = SIn.Long(row["SaveImageCategory"].ToString());
            retVal.Add(eFormDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EFormDef> listEFormDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EFormDef";
        var table = new DataTable(tableName);
        table.Columns.Add("EFormDefNum");
        table.Columns.Add("FormType");
        table.Columns.Add("Description");
        table.Columns.Add("DateTCreated");
        table.Columns.Add("IsInternalHidden");
        table.Columns.Add("MaxWidth");
        table.Columns.Add("RevID");
        table.Columns.Add("ShowLabelsBold");
        table.Columns.Add("SpaceBelowEachField");
        table.Columns.Add("SpaceToRightEachField");
        table.Columns.Add("SaveImageCategory");
        foreach (var eFormDef in listEFormDefs)
            table.Rows.Add(SOut.Long(eFormDef.EFormDefNum), SOut.Int((int) eFormDef.FormType), eFormDef.Description, SOut.DateT(eFormDef.DateTCreated, false), SOut.Bool(eFormDef.IsInternalHidden), SOut.Int(eFormDef.MaxWidth), SOut.Int(eFormDef.RevID), SOut.Bool(eFormDef.ShowLabelsBold), SOut.Int(eFormDef.SpaceBelowEachField), SOut.Int(eFormDef.SpaceToRightEachField), SOut.Long(eFormDef.SaveImageCategory));
        return table;
    }

    public static long Insert(EFormDef eFormDef)
    {
        return Insert(eFormDef, false);
    }

    public static long Insert(EFormDef eFormDef, bool useExistingPK)
    {
        var command = "INSERT INTO eformdef (";

        command += "FormType,Description,DateTCreated,IsInternalHidden,MaxWidth,RevID,ShowLabelsBold,SpaceBelowEachField,SpaceToRightEachField,SaveImageCategory) VALUES(";

        command +=
            SOut.Int((int) eFormDef.FormType) + ","
                                              + "'" + SOut.String(eFormDef.Description) + "',"
                                              + SOut.DateT(eFormDef.DateTCreated) + ","
                                              + SOut.Bool(eFormDef.IsInternalHidden) + ","
                                              + SOut.Int(eFormDef.MaxWidth) + ","
                                              + SOut.Int(eFormDef.RevID) + ","
                                              + SOut.Bool(eFormDef.ShowLabelsBold) + ","
                                              + SOut.Int(eFormDef.SpaceBelowEachField) + ","
                                              + SOut.Int(eFormDef.SpaceToRightEachField) + ","
                                              + SOut.Long(eFormDef.SaveImageCategory) + ")";
        {
            eFormDef.EFormDefNum = Db.NonQ(command, true, "EFormDefNum", "eFormDef");
        }
        return eFormDef.EFormDefNum;
    }

    public static long InsertNoCache(EFormDef eFormDef)
    {
        return InsertNoCache(eFormDef, false);
    }

    public static long InsertNoCache(EFormDef eFormDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eformdef (";
        if (isRandomKeys || useExistingPK) command += "EFormDefNum,";
        command += "FormType,Description,DateTCreated,IsInternalHidden,MaxWidth,RevID,ShowLabelsBold,SpaceBelowEachField,SpaceToRightEachField,SaveImageCategory) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eFormDef.EFormDefNum) + ",";
        command +=
            SOut.Int((int) eFormDef.FormType) + ","
                                              + "'" + SOut.String(eFormDef.Description) + "',"
                                              + SOut.DateT(eFormDef.DateTCreated) + ","
                                              + SOut.Bool(eFormDef.IsInternalHidden) + ","
                                              + SOut.Int(eFormDef.MaxWidth) + ","
                                              + SOut.Int(eFormDef.RevID) + ","
                                              + SOut.Bool(eFormDef.ShowLabelsBold) + ","
                                              + SOut.Int(eFormDef.SpaceBelowEachField) + ","
                                              + SOut.Int(eFormDef.SpaceToRightEachField) + ","
                                              + SOut.Long(eFormDef.SaveImageCategory) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eFormDef.EFormDefNum = Db.NonQ(command, true, "EFormDefNum", "eFormDef");
        return eFormDef.EFormDefNum;
    }

    public static void Update(EFormDef eFormDef)
    {
        var command = "UPDATE eformdef SET "
                      + "FormType             =  " + SOut.Int((int) eFormDef.FormType) + ", "
                      + "Description          = '" + SOut.String(eFormDef.Description) + "', "
                      + "DateTCreated         =  " + SOut.DateT(eFormDef.DateTCreated) + ", "
                      + "IsInternalHidden     =  " + SOut.Bool(eFormDef.IsInternalHidden) + ", "
                      + "MaxWidth             =  " + SOut.Int(eFormDef.MaxWidth) + ", "
                      + "RevID                =  " + SOut.Int(eFormDef.RevID) + ", "
                      + "ShowLabelsBold       =  " + SOut.Bool(eFormDef.ShowLabelsBold) + ", "
                      + "SpaceBelowEachField  =  " + SOut.Int(eFormDef.SpaceBelowEachField) + ", "
                      + "SpaceToRightEachField=  " + SOut.Int(eFormDef.SpaceToRightEachField) + ", "
                      + "SaveImageCategory    =  " + SOut.Long(eFormDef.SaveImageCategory) + " "
                      + "WHERE EFormDefNum = " + SOut.Long(eFormDef.EFormDefNum);
        Db.NonQ(command);
    }

    public static bool Update(EFormDef eFormDef, EFormDef oldEFormDef)
    {
        var command = "";
        if (eFormDef.FormType != oldEFormDef.FormType)
        {
            if (command != "") command += ",";
            command += "FormType = " + SOut.Int((int) eFormDef.FormType) + "";
        }

        if (eFormDef.Description != oldEFormDef.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(eFormDef.Description) + "'";
        }

        if (eFormDef.DateTCreated != oldEFormDef.DateTCreated)
        {
            if (command != "") command += ",";
            command += "DateTCreated = " + SOut.DateT(eFormDef.DateTCreated) + "";
        }

        if (eFormDef.IsInternalHidden != oldEFormDef.IsInternalHidden)
        {
            if (command != "") command += ",";
            command += "IsInternalHidden = " + SOut.Bool(eFormDef.IsInternalHidden) + "";
        }

        if (eFormDef.MaxWidth != oldEFormDef.MaxWidth)
        {
            if (command != "") command += ",";
            command += "MaxWidth = " + SOut.Int(eFormDef.MaxWidth) + "";
        }

        if (eFormDef.RevID != oldEFormDef.RevID)
        {
            if (command != "") command += ",";
            command += "RevID = " + SOut.Int(eFormDef.RevID) + "";
        }

        if (eFormDef.ShowLabelsBold != oldEFormDef.ShowLabelsBold)
        {
            if (command != "") command += ",";
            command += "ShowLabelsBold = " + SOut.Bool(eFormDef.ShowLabelsBold) + "";
        }

        if (eFormDef.SpaceBelowEachField != oldEFormDef.SpaceBelowEachField)
        {
            if (command != "") command += ",";
            command += "SpaceBelowEachField = " + SOut.Int(eFormDef.SpaceBelowEachField) + "";
        }

        if (eFormDef.SpaceToRightEachField != oldEFormDef.SpaceToRightEachField)
        {
            if (command != "") command += ",";
            command += "SpaceToRightEachField = " + SOut.Int(eFormDef.SpaceToRightEachField) + "";
        }

        if (eFormDef.SaveImageCategory != oldEFormDef.SaveImageCategory)
        {
            if (command != "") command += ",";
            command += "SaveImageCategory = " + SOut.Long(eFormDef.SaveImageCategory) + "";
        }

        if (command == "") return false;
        command = "UPDATE eformdef SET " + command
                                         + " WHERE EFormDefNum = " + SOut.Long(eFormDef.EFormDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EFormDef eFormDef, EFormDef oldEFormDef)
    {
        if (eFormDef.FormType != oldEFormDef.FormType) return true;
        if (eFormDef.Description != oldEFormDef.Description) return true;
        if (eFormDef.DateTCreated != oldEFormDef.DateTCreated) return true;
        if (eFormDef.IsInternalHidden != oldEFormDef.IsInternalHidden) return true;
        if (eFormDef.MaxWidth != oldEFormDef.MaxWidth) return true;
        if (eFormDef.RevID != oldEFormDef.RevID) return true;
        if (eFormDef.ShowLabelsBold != oldEFormDef.ShowLabelsBold) return true;
        if (eFormDef.SpaceBelowEachField != oldEFormDef.SpaceBelowEachField) return true;
        if (eFormDef.SpaceToRightEachField != oldEFormDef.SpaceToRightEachField) return true;
        if (eFormDef.SaveImageCategory != oldEFormDef.SaveImageCategory) return true;
        return false;
    }

    public static void Delete(long eFormDefNum)
    {
        var command = "DELETE FROM eformdef "
                      + "WHERE EFormDefNum = " + SOut.Long(eFormDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEFormDefNums)
    {
        if (listEFormDefNums == null || listEFormDefNums.Count == 0) return;
        var command = "DELETE FROM eformdef "
                      + "WHERE EFormDefNum IN(" + string.Join(",", listEFormDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}