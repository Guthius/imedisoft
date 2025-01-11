#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EFormCrud
{
    public static EForm SelectOne(long eFormNum)
    {
        var command = "SELECT * FROM eform "
                      + "WHERE EFormNum = " + SOut.Long(eFormNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EForm SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EForm> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EForm> TableToList(DataTable table)
    {
        var retVal = new List<EForm>();
        EForm eForm;
        foreach (DataRow row in table.Rows)
        {
            eForm = new EForm();
            eForm.EFormNum = SIn.Long(row["EFormNum"].ToString());
            eForm.FormType = (EnumEFormType) SIn.Int(row["FormType"].ToString());
            eForm.PatNum = SIn.Long(row["PatNum"].ToString());
            eForm.DateTimeShown = SIn.DateTime(row["DateTimeShown"].ToString());
            eForm.Description = SIn.String(row["Description"].ToString());
            eForm.DateTEdited = SIn.DateTime(row["DateTEdited"].ToString());
            eForm.MaxWidth = SIn.Int(row["MaxWidth"].ToString());
            eForm.EFormDefNum = SIn.Long(row["EFormDefNum"].ToString());
            eForm.Status = (EnumEFormStatus) SIn.Int(row["Status"].ToString());
            eForm.RevID = SIn.Int(row["RevID"].ToString());
            eForm.ShowLabelsBold = SIn.Bool(row["ShowLabelsBold"].ToString());
            eForm.SpaceBelowEachField = SIn.Int(row["SpaceBelowEachField"].ToString());
            eForm.SpaceToRightEachField = SIn.Int(row["SpaceToRightEachField"].ToString());
            eForm.SaveImageCategory = SIn.Long(row["SaveImageCategory"].ToString());
            retVal.Add(eForm);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EForm> listEForms, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EForm";
        var table = new DataTable(tableName);
        table.Columns.Add("EFormNum");
        table.Columns.Add("FormType");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateTimeShown");
        table.Columns.Add("Description");
        table.Columns.Add("DateTEdited");
        table.Columns.Add("MaxWidth");
        table.Columns.Add("EFormDefNum");
        table.Columns.Add("Status");
        table.Columns.Add("RevID");
        table.Columns.Add("ShowLabelsBold");
        table.Columns.Add("SpaceBelowEachField");
        table.Columns.Add("SpaceToRightEachField");
        table.Columns.Add("SaveImageCategory");
        foreach (var eForm in listEForms)
            table.Rows.Add(SOut.Long(eForm.EFormNum), SOut.Int((int) eForm.FormType), SOut.Long(eForm.PatNum), SOut.DateT(eForm.DateTimeShown, false), eForm.Description, SOut.DateT(eForm.DateTEdited, false), SOut.Int(eForm.MaxWidth), SOut.Long(eForm.EFormDefNum), SOut.Int((int) eForm.Status), SOut.Int(eForm.RevID), SOut.Bool(eForm.ShowLabelsBold), SOut.Int(eForm.SpaceBelowEachField), SOut.Int(eForm.SpaceToRightEachField), SOut.Long(eForm.SaveImageCategory));
        return table;
    }

    public static long Insert(EForm eForm)
    {
        return Insert(eForm, false);
    }

    public static long Insert(EForm eForm, bool useExistingPK)
    {
        var command = "INSERT INTO eform (";

        command += "FormType,PatNum,DateTimeShown,Description,DateTEdited,MaxWidth,EFormDefNum,Status,RevID,ShowLabelsBold,SpaceBelowEachField,SpaceToRightEachField,SaveImageCategory) VALUES(";

        command +=
            SOut.Int((int) eForm.FormType) + ","
                                           + SOut.Long(eForm.PatNum) + ","
                                           + SOut.DateT(eForm.DateTimeShown) + ","
                                           + "'" + SOut.String(eForm.Description) + "',"
                                           + SOut.DateT(eForm.DateTEdited) + ","
                                           + SOut.Int(eForm.MaxWidth) + ","
                                           + SOut.Long(eForm.EFormDefNum) + ","
                                           + SOut.Int((int) eForm.Status) + ","
                                           + SOut.Int(eForm.RevID) + ","
                                           + SOut.Bool(eForm.ShowLabelsBold) + ","
                                           + SOut.Int(eForm.SpaceBelowEachField) + ","
                                           + SOut.Int(eForm.SpaceToRightEachField) + ","
                                           + SOut.Long(eForm.SaveImageCategory) + ")";
        {
            eForm.EFormNum = Db.NonQ(command, true, "EFormNum", "eForm");
        }
        return eForm.EFormNum;
    }

    public static long InsertNoCache(EForm eForm)
    {
        return InsertNoCache(eForm, false);
    }

    public static long InsertNoCache(EForm eForm, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eform (";
        if (isRandomKeys || useExistingPK) command += "EFormNum,";
        command += "FormType,PatNum,DateTimeShown,Description,DateTEdited,MaxWidth,EFormDefNum,Status,RevID,ShowLabelsBold,SpaceBelowEachField,SpaceToRightEachField,SaveImageCategory) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eForm.EFormNum) + ",";
        command +=
            SOut.Int((int) eForm.FormType) + ","
                                           + SOut.Long(eForm.PatNum) + ","
                                           + SOut.DateT(eForm.DateTimeShown) + ","
                                           + "'" + SOut.String(eForm.Description) + "',"
                                           + SOut.DateT(eForm.DateTEdited) + ","
                                           + SOut.Int(eForm.MaxWidth) + ","
                                           + SOut.Long(eForm.EFormDefNum) + ","
                                           + SOut.Int((int) eForm.Status) + ","
                                           + SOut.Int(eForm.RevID) + ","
                                           + SOut.Bool(eForm.ShowLabelsBold) + ","
                                           + SOut.Int(eForm.SpaceBelowEachField) + ","
                                           + SOut.Int(eForm.SpaceToRightEachField) + ","
                                           + SOut.Long(eForm.SaveImageCategory) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eForm.EFormNum = Db.NonQ(command, true, "EFormNum", "eForm");
        return eForm.EFormNum;
    }

    public static void Update(EForm eForm)
    {
        var command = "UPDATE eform SET "
                      + "FormType             =  " + SOut.Int((int) eForm.FormType) + ", "
                      + "PatNum               =  " + SOut.Long(eForm.PatNum) + ", "
                      + "DateTimeShown        =  " + SOut.DateT(eForm.DateTimeShown) + ", "
                      + "Description          = '" + SOut.String(eForm.Description) + "', "
                      + "DateTEdited          =  " + SOut.DateT(eForm.DateTEdited) + ", "
                      + "MaxWidth             =  " + SOut.Int(eForm.MaxWidth) + ", "
                      + "EFormDefNum          =  " + SOut.Long(eForm.EFormDefNum) + ", "
                      + "Status               =  " + SOut.Int((int) eForm.Status) + ", "
                      + "RevID                =  " + SOut.Int(eForm.RevID) + ", "
                      + "ShowLabelsBold       =  " + SOut.Bool(eForm.ShowLabelsBold) + ", "
                      + "SpaceBelowEachField  =  " + SOut.Int(eForm.SpaceBelowEachField) + ", "
                      + "SpaceToRightEachField=  " + SOut.Int(eForm.SpaceToRightEachField) + ", "
                      + "SaveImageCategory    =  " + SOut.Long(eForm.SaveImageCategory) + " "
                      + "WHERE EFormNum = " + SOut.Long(eForm.EFormNum);
        Db.NonQ(command);
    }

    public static bool Update(EForm eForm, EForm oldEForm)
    {
        var command = "";
        if (eForm.FormType != oldEForm.FormType)
        {
            if (command != "") command += ",";
            command += "FormType = " + SOut.Int((int) eForm.FormType) + "";
        }

        if (eForm.PatNum != oldEForm.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(eForm.PatNum) + "";
        }

        if (eForm.DateTimeShown != oldEForm.DateTimeShown)
        {
            if (command != "") command += ",";
            command += "DateTimeShown = " + SOut.DateT(eForm.DateTimeShown) + "";
        }

        if (eForm.Description != oldEForm.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(eForm.Description) + "'";
        }

        if (eForm.DateTEdited != oldEForm.DateTEdited)
        {
            if (command != "") command += ",";
            command += "DateTEdited = " + SOut.DateT(eForm.DateTEdited) + "";
        }

        if (eForm.MaxWidth != oldEForm.MaxWidth)
        {
            if (command != "") command += ",";
            command += "MaxWidth = " + SOut.Int(eForm.MaxWidth) + "";
        }

        if (eForm.EFormDefNum != oldEForm.EFormDefNum)
        {
            if (command != "") command += ",";
            command += "EFormDefNum = " + SOut.Long(eForm.EFormDefNum) + "";
        }

        if (eForm.Status != oldEForm.Status)
        {
            if (command != "") command += ",";
            command += "Status = " + SOut.Int((int) eForm.Status) + "";
        }

        if (eForm.RevID != oldEForm.RevID)
        {
            if (command != "") command += ",";
            command += "RevID = " + SOut.Int(eForm.RevID) + "";
        }

        if (eForm.ShowLabelsBold != oldEForm.ShowLabelsBold)
        {
            if (command != "") command += ",";
            command += "ShowLabelsBold = " + SOut.Bool(eForm.ShowLabelsBold) + "";
        }

        if (eForm.SpaceBelowEachField != oldEForm.SpaceBelowEachField)
        {
            if (command != "") command += ",";
            command += "SpaceBelowEachField = " + SOut.Int(eForm.SpaceBelowEachField) + "";
        }

        if (eForm.SpaceToRightEachField != oldEForm.SpaceToRightEachField)
        {
            if (command != "") command += ",";
            command += "SpaceToRightEachField = " + SOut.Int(eForm.SpaceToRightEachField) + "";
        }

        if (eForm.SaveImageCategory != oldEForm.SaveImageCategory)
        {
            if (command != "") command += ",";
            command += "SaveImageCategory = " + SOut.Long(eForm.SaveImageCategory) + "";
        }

        if (command == "") return false;
        command = "UPDATE eform SET " + command
                                      + " WHERE EFormNum = " + SOut.Long(eForm.EFormNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EForm eForm, EForm oldEForm)
    {
        if (eForm.FormType != oldEForm.FormType) return true;
        if (eForm.PatNum != oldEForm.PatNum) return true;
        if (eForm.DateTimeShown != oldEForm.DateTimeShown) return true;
        if (eForm.Description != oldEForm.Description) return true;
        if (eForm.DateTEdited != oldEForm.DateTEdited) return true;
        if (eForm.MaxWidth != oldEForm.MaxWidth) return true;
        if (eForm.EFormDefNum != oldEForm.EFormDefNum) return true;
        if (eForm.Status != oldEForm.Status) return true;
        if (eForm.RevID != oldEForm.RevID) return true;
        if (eForm.ShowLabelsBold != oldEForm.ShowLabelsBold) return true;
        if (eForm.SpaceBelowEachField != oldEForm.SpaceBelowEachField) return true;
        if (eForm.SpaceToRightEachField != oldEForm.SpaceToRightEachField) return true;
        if (eForm.SaveImageCategory != oldEForm.SaveImageCategory) return true;
        return false;
    }

    public static void Delete(long eFormNum)
    {
        var command = "DELETE FROM eform "
                      + "WHERE EFormNum = " + SOut.Long(eFormNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEFormNums)
    {
        if (listEFormNums == null || listEFormNums.Count == 0) return;
        var command = "DELETE FROM eform "
                      + "WHERE EFormNum IN(" + string.Join(",", listEFormNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}