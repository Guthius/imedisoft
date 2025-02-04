using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<EForm> TableToList(DataTable table)
    {
        var retVal = new List<EForm>();
        foreach (DataRow row in table.Rows)
        {
            var eForm = new EForm
            {
                EFormNum = SIn.Long(row["EFormNum"].ToString()),
                FormType = (EnumEFormType) SIn.Int(row["FormType"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                DateTimeShown = SIn.DateTime(row["DateTimeShown"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                DateTEdited = SIn.DateTime(row["DateTEdited"].ToString()),
                MaxWidth = SIn.Int(row["MaxWidth"].ToString()),
                EFormDefNum = SIn.Long(row["EFormDefNum"].ToString()),
                Status = (EnumEFormStatus) SIn.Int(row["Status"].ToString()),
                RevID = SIn.Int(row["RevID"].ToString()),
                ShowLabelsBold = SIn.Bool(row["ShowLabelsBold"].ToString()),
                SpaceBelowEachField = SIn.Int(row["SpaceBelowEachField"].ToString()),
                SpaceToRightEachField = SIn.Int(row["SpaceToRightEachField"].ToString()),
                SaveImageCategory = SIn.Long(row["SaveImageCategory"].ToString())
            };
            retVal.Add(eForm);
        }

        return retVal;
    }

    public static void Insert(EForm eForm)
    {
        var command = "INSERT INTO eform (";

        command += "FormType,PatNum,DateTimeShown,Description,DateTEdited,MaxWidth,EFormDefNum,Status,RevID,ShowLabelsBold,SpaceBelowEachField,SpaceToRightEachField,SaveImageCategory) VALUES(";

        command +=
            SOut.Int((int) eForm.FormType) + ","
                                           + SOut.Long(eForm.PatNum) + ","
                                           + SOut.DateTime(eForm.DateTimeShown) + ","
                                           + "'" + SOut.String(eForm.Description) + "',"
                                           + SOut.DateTime(eForm.DateTEdited) + ","
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
    }

    public static void Update(EForm eForm)
    {
        var command = "UPDATE eform SET "
                      + "FormType             =  " + SOut.Int((int) eForm.FormType) + ", "
                      + "PatNum               =  " + SOut.Long(eForm.PatNum) + ", "
                      + "DateTimeShown        =  " + SOut.DateTime(eForm.DateTimeShown) + ", "
                      + "Description          = '" + SOut.String(eForm.Description) + "', "
                      + "DateTEdited          =  " + SOut.DateTime(eForm.DateTEdited) + ", "
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

    public static void Delete(long eFormNum)
    {
        var command = "DELETE FROM eform "
                      + "WHERE EFormNum = " + SOut.Long(eFormNum);
        Db.NonQ(command);
    }
}