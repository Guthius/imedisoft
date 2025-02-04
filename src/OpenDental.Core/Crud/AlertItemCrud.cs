using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AlertItemCrud
{
    public static List<AlertItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AlertItem> TableToList(DataTable table)
    {
        var retVal = new List<AlertItem>();
        foreach (DataRow row in table.Rows)
        {
            var alertItem = new AlertItem
            {
                AlertItemNum = SIn.Long(row["AlertItemNum"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                Type = (AlertType) SIn.Int(row["Type"].ToString()),
                Severity = (SeverityType) SIn.Int(row["Severity"].ToString()),
                Actions = (ActionType) SIn.Int(row["Actions"].ToString()),
                FormToOpen = (FormType) SIn.Int(row["FormToOpen"].ToString()),
                FKey = SIn.Long(row["FKey"].ToString()),
                ItemValue = SIn.String(row["ItemValue"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString())
            };
            retVal.Add(alertItem);
        }

        return retVal;
    }

    public static void Insert(AlertItem alertItem)
    {
        var command = "INSERT INTO alertitem (";

        command += "ClinicNum,Description,Type,Severity,Actions,FormToOpen,FKey,ItemValue,UserNum,SecDateTEntry) VALUES(";

        command +=
            SOut.Long(alertItem.ClinicNum) + ","
                                           + "'" + SOut.String(alertItem.Description) + "',"
                                           + SOut.Int((int) alertItem.Type) + ","
                                           + SOut.Int((int) alertItem.Severity) + ","
                                           + SOut.Int((int) alertItem.Actions) + ","
                                           + SOut.Int((int) alertItem.FormToOpen) + ","
                                           + SOut.Long(alertItem.FKey) + ","
                                           + "'" + SOut.String(alertItem.ItemValue) + "',"
                                           + SOut.Long(alertItem.UserNum) + ","
                                           + "NOW()" + ")";
        {
            alertItem.AlertItemNum = Db.NonQ(command, true, "AlertItemNum", "alertItem");
        }
    }
}