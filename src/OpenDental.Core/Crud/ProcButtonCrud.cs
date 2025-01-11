#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcButtonCrud
{
    public static ProcButton SelectOne(long procButtonNum)
    {
        var command = "SELECT * FROM procbutton "
                      + "WHERE ProcButtonNum = " + SOut.Long(procButtonNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcButton SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcButton> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcButton> TableToList(DataTable table)
    {
        var retVal = new List<ProcButton>();
        ProcButton procButton;
        foreach (DataRow row in table.Rows)
        {
            procButton = new ProcButton();
            procButton.ProcButtonNum = SIn.Long(row["ProcButtonNum"].ToString());
            procButton.Description = SIn.String(row["Description"].ToString());
            procButton.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            procButton.Category = SIn.Long(row["Category"].ToString());
            procButton.ButtonImage = SIn.String(row["ButtonImage"].ToString());
            procButton.IsMultiVisit = SIn.Bool(row["IsMultiVisit"].ToString());
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

    public static long Insert(ProcButton procButton)
    {
        return Insert(procButton, false);
    }

    public static long Insert(ProcButton procButton, bool useExistingPK)
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
        var paramButtonImage = new OdSqlParameter("paramButtonImage", OdDbType.Text, SOut.StringParam(procButton.ButtonImage));
        {
            procButton.ProcButtonNum = Db.NonQ(command, true, "ProcButtonNum", "procButton", paramButtonImage);
        }
        return procButton.ProcButtonNum;
    }

    public static long InsertNoCache(ProcButton procButton)
    {
        return InsertNoCache(procButton, false);
    }

    public static long InsertNoCache(ProcButton procButton, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procbutton (";
        if (isRandomKeys || useExistingPK) command += "ProcButtonNum,";
        command += "Description,ItemOrder,Category,ButtonImage,IsMultiVisit) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procButton.ProcButtonNum) + ",";
        command +=
            "'" + SOut.String(procButton.Description) + "',"
            + SOut.Int(procButton.ItemOrder) + ","
            + SOut.Long(procButton.Category) + ","
            + DbHelper.ParamChar + "paramButtonImage,"
            + SOut.Bool(procButton.IsMultiVisit) + ")";
        if (procButton.ButtonImage == null) procButton.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", OdDbType.Text, SOut.StringParam(procButton.ButtonImage));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramButtonImage);
        else
            procButton.ProcButtonNum = Db.NonQ(command, true, "ProcButtonNum", "procButton", paramButtonImage);
        return procButton.ProcButtonNum;
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
        var paramButtonImage = new OdSqlParameter("paramButtonImage", OdDbType.Text, SOut.StringParam(procButton.ButtonImage));
        Db.NonQ(command, paramButtonImage);
    }

    public static bool Update(ProcButton procButton, ProcButton oldProcButton)
    {
        var command = "";
        if (procButton.Description != oldProcButton.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(procButton.Description) + "'";
        }

        if (procButton.ItemOrder != oldProcButton.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(procButton.ItemOrder) + "";
        }

        if (procButton.Category != oldProcButton.Category)
        {
            if (command != "") command += ",";
            command += "Category = " + SOut.Long(procButton.Category) + "";
        }

        if (procButton.ButtonImage != oldProcButton.ButtonImage)
        {
            if (command != "") command += ",";
            command += "ButtonImage = " + DbHelper.ParamChar + "paramButtonImage";
        }

        if (procButton.IsMultiVisit != oldProcButton.IsMultiVisit)
        {
            if (command != "") command += ",";
            command += "IsMultiVisit = " + SOut.Bool(procButton.IsMultiVisit) + "";
        }

        if (command == "") return false;
        if (procButton.ButtonImage == null) procButton.ButtonImage = "";
        var paramButtonImage = new OdSqlParameter("paramButtonImage", OdDbType.Text, SOut.StringParam(procButton.ButtonImage));
        command = "UPDATE procbutton SET " + command
                                           + " WHERE ProcButtonNum = " + SOut.Long(procButton.ProcButtonNum);
        Db.NonQ(command, paramButtonImage);
        return true;
    }

    public static bool UpdateComparison(ProcButton procButton, ProcButton oldProcButton)
    {
        if (procButton.Description != oldProcButton.Description) return true;
        if (procButton.ItemOrder != oldProcButton.ItemOrder) return true;
        if (procButton.Category != oldProcButton.Category) return true;
        if (procButton.ButtonImage != oldProcButton.ButtonImage) return true;
        if (procButton.IsMultiVisit != oldProcButton.IsMultiVisit) return true;
        return false;
    }

    public static void Delete(long procButtonNum)
    {
        var command = "DELETE FROM procbutton "
                      + "WHERE ProcButtonNum = " + SOut.Long(procButtonNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcButtonNums)
    {
        if (listProcButtonNums == null || listProcButtonNums.Count == 0) return;
        var command = "DELETE FROM procbutton "
                      + "WHERE ProcButtonNum IN(" + string.Join(",", listProcButtonNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}