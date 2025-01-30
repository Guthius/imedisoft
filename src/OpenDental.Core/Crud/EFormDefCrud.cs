using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EFormDefCrud
{
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
            table.Rows.Add(SOut.Long(eFormDef.EFormDefNum), SOut.Int((int) eFormDef.FormType), eFormDef.Description, SOut.DateTime(eFormDef.DateTCreated, false), SOut.Bool(eFormDef.IsInternalHidden), SOut.Int(eFormDef.MaxWidth), SOut.Int(eFormDef.RevID), SOut.Bool(eFormDef.ShowLabelsBold), SOut.Int(eFormDef.SpaceBelowEachField), SOut.Int(eFormDef.SpaceToRightEachField), SOut.Long(eFormDef.SaveImageCategory));
        return table;
    }

    public static long Insert(EFormDef eFormDef)
    {
        var command = "INSERT INTO eformdef (";

        command += "FormType,Description,DateTCreated,IsInternalHidden,MaxWidth,RevID,ShowLabelsBold,SpaceBelowEachField,SpaceToRightEachField,SaveImageCategory) VALUES(";

        command +=
            SOut.Int((int) eFormDef.FormType) + ","
                                              + "'" + SOut.String(eFormDef.Description) + "',"
                                              + SOut.DateTime(eFormDef.DateTCreated) + ","
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

    public static void Update(EFormDef eFormDef)
    {
        var command = "UPDATE eformdef SET "
                      + "FormType             =  " + SOut.Int((int) eFormDef.FormType) + ", "
                      + "Description          = '" + SOut.String(eFormDef.Description) + "', "
                      + "DateTCreated         =  " + SOut.DateTime(eFormDef.DateTCreated) + ", "
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

    public static void Delete(long eFormDefNum)
    {
        var command = "DELETE FROM eformdef "
                      + "WHERE EFormDefNum = " + SOut.Long(eFormDefNum);
        Db.NonQ(command);
    }
}