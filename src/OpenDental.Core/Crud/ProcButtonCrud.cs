using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcButtonCrud
{
    public static List<ProcButton> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcButton> TableToList(DataTable table)
    {
        var retVal = new List<ProcButton>();
        foreach (DataRow row in table.Rows)
        {
            var procButton = new ProcButton
            {
                ProcButtonNum = SIn.Long(row["ProcButtonNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                ItemOrder = SIn.Int(row["ItemOrder"].ToString()),
                Category = SIn.Long(row["Category"].ToString()),
                ButtonImage = SIn.String(row["ButtonImage"].ToString()),
                IsMultiVisit = SIn.Bool(row["IsMultiVisit"].ToString())
            };
            retVal.Add(procButton);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcButton> listProcButtons, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcButton";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcButtonNum");
        table.Columns.Add("Description");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("Category");
        table.Columns.Add("ButtonImage");
        table.Columns.Add("IsMultiVisit");
        foreach (var procButton in listProcButtons)
            table.Rows.Add(SOut.Long(procButton.ProcButtonNum), procButton.Description, SOut.Int(procButton.ItemOrder), SOut.Long(procButton.Category), procButton.ButtonImage, SOut.Bool(procButton.IsMultiVisit));
        return table;
    }

    public static void Insert(ProcButton procButton)
    {
        var command = "INSERT INTO procbutton (";

        command += "Description,ItemOrder,Category,ButtonImage,IsMultiVisit) VALUES(";

        command +=
            "'" + SOut.String(procButton.Description) + "',"
            + SOut.Int(procButton.ItemOrder) + ","
            + SOut.Long(procButton.Category) + ","
            + DbHelper.ParamChar + "paramButtonImage,"
            + SOut.Bool(procButton.IsMultiVisit) + ")";
        if (procButton.ButtonImage == null) procButton.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", SOut.StringParam(procButton.ButtonImage));
        {
            procButton.ProcButtonNum = Db.NonQ(command, true, "ProcButtonNum", "procButton", paramButtonImage);
        }
    }

    public static void Update(ProcButton procButton)
    {
        var command = "UPDATE procbutton SET "
                      + "Description  = '" + SOut.String(procButton.Description) + "', "
                      + "ItemOrder    =  " + SOut.Int(procButton.ItemOrder) + ", "
                      + "Category     =  " + SOut.Long(procButton.Category) + ", "
                      + "ButtonImage  =  " + DbHelper.ParamChar + "paramButtonImage, "
                      + "IsMultiVisit =  " + SOut.Bool(procButton.IsMultiVisit) + " "
                      + "WHERE ProcButtonNum = " + SOut.Long(procButton.ProcButtonNum);
        if (procButton.ButtonImage == null) procButton.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", SOut.StringParam(procButton.ButtonImage));
        Db.NonQ(command, paramButtonImage);
    }
}