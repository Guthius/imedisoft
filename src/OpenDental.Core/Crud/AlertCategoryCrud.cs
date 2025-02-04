using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AlertCategoryCrud
{
    public static List<AlertCategory> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AlertCategory> TableToList(DataTable table)
    {
        var retVal = new List<AlertCategory>();
        foreach (DataRow row in table.Rows)
        {
            var alertCategory = new AlertCategory
            {
                AlertCategoryNum = SIn.Long(row["AlertCategoryNum"].ToString()),
                IsHQCategory = SIn.Bool(row["IsHQCategory"].ToString()),
                InternalName = SIn.String(row["InternalName"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(alertCategory);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AlertCategory> listAlertCategorys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AlertCategory";
        var table = new DataTable(tableName);
        table.Columns.Add("AlertCategoryNum");
        table.Columns.Add("IsHQCategory");
        table.Columns.Add("InternalName");
        table.Columns.Add("Description");
        foreach (var alertCategory in listAlertCategorys)
            table.Rows.Add(SOut.Long(alertCategory.AlertCategoryNum), SOut.Bool(alertCategory.IsHQCategory), alertCategory.InternalName, alertCategory.Description);
        return table;
    }

    public static long Insert(AlertCategory alertCategory)
    {
        var command = "INSERT INTO alertcategory (";

        command += "IsHQCategory,InternalName,Description) VALUES(";

        command +=
            SOut.Bool(alertCategory.IsHQCategory) + ","
                                                  + "'" + SOut.String(alertCategory.InternalName) + "',"
                                                  + "'" + SOut.String(alertCategory.Description) + "')";
        {
            alertCategory.AlertCategoryNum = Db.NonQ(command, true, "AlertCategoryNum", "alertCategory");
        }
        return alertCategory.AlertCategoryNum;
    }

    public static void Update(AlertCategory alertCategory)
    {
        var command = "UPDATE alertcategory SET "
                      + "IsHQCategory    =  " + SOut.Bool(alertCategory.IsHQCategory) + ", "
                      + "InternalName    = '" + SOut.String(alertCategory.InternalName) + "', "
                      + "Description     = '" + SOut.String(alertCategory.Description) + "' "
                      + "WHERE AlertCategoryNum = " + SOut.Long(alertCategory.AlertCategoryNum);
        Db.NonQ(command);
    }

    public static void Delete(long alertCategoryNum)
    {
        var command = "DELETE FROM alertcategory "
                      + "WHERE AlertCategoryNum = " + SOut.Long(alertCategoryNum);
        Db.NonQ(command);
    }
}