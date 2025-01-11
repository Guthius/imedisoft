#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PopupCrud
{
    public static Popup SelectOne(long popupNum)
    {
        var command = "SELECT * FROM popup "
                      + "WHERE PopupNum = " + SOut.Long(popupNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Popup SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Popup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Popup> TableToList(DataTable table)
    {
        var retVal = new List<Popup>();
        Popup popup;
        foreach (DataRow row in table.Rows)
        {
            popup = new Popup();
            popup.PopupNum = SIn.Long(row["PopupNum"].ToString());
            popup.PatNum = SIn.Long(row["PatNum"].ToString());
            popup.Description = SIn.String(row["Description"].ToString());
            popup.IsDisabled = SIn.Bool(row["IsDisabled"].ToString());
            popup.PopupLevel = (EnumPopupLevel) SIn.Int(row["PopupLevel"].ToString());
            popup.UserNum = SIn.Long(row["UserNum"].ToString());
            popup.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            popup.IsArchived = SIn.Bool(row["IsArchived"].ToString());
            popup.PopupNumArchive = SIn.Long(row["PopupNumArchive"].ToString());
            popup.DateTimeDisabled = SIn.DateTime(row["DateTimeDisabled"].ToString());
            retVal.Add(popup);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Popup> listPopups, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Popup";
        var table = new DataTable(tableName);
        table.Columns.Add("PopupNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("Description");
        table.Columns.Add("IsDisabled");
        table.Columns.Add("PopupLevel");
        table.Columns.Add("UserNum");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("IsArchived");
        table.Columns.Add("PopupNumArchive");
        table.Columns.Add("DateTimeDisabled");
        foreach (var popup in listPopups)
            table.Rows.Add(SOut.Long(popup.PopupNum), SOut.Long(popup.PatNum), popup.Description, SOut.Bool(popup.IsDisabled), SOut.Int((int) popup.PopupLevel), SOut.Long(popup.UserNum), SOut.DateT(popup.DateTimeEntry, false), SOut.Bool(popup.IsArchived), SOut.Long(popup.PopupNumArchive), SOut.DateT(popup.DateTimeDisabled, false));
        return table;
    }

    public static long Insert(Popup popup)
    {
        return Insert(popup, false);
    }

    public static long Insert(Popup popup, bool useExistingPK)
    {
        var command = "INSERT INTO popup (";

        command += "PatNum,Description,IsDisabled,PopupLevel,UserNum,DateTimeEntry,IsArchived,PopupNumArchive,DateTimeDisabled) VALUES(";

        command +=
            SOut.Long(popup.PatNum) + ","
                                    + DbHelper.ParamChar + "paramDescription,"
                                    + SOut.Bool(popup.IsDisabled) + ","
                                    + SOut.Int((int) popup.PopupLevel) + ","
                                    + SOut.Long(popup.UserNum) + ","
                                    + DbHelper.Now() + ","
                                    + SOut.Bool(popup.IsArchived) + ","
                                    + SOut.Long(popup.PopupNumArchive) + ","
                                    + SOut.DateT(popup.DateTimeDisabled) + ")";
        if (popup.Description == null) popup.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(popup.Description));
        {
            popup.PopupNum = Db.NonQ(command, true, "PopupNum", "popup", paramDescription);
        }
        return popup.PopupNum;
    }

    public static long InsertNoCache(Popup popup)
    {
        return InsertNoCache(popup, false);
    }

    public static long InsertNoCache(Popup popup, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO popup (";
        if (isRandomKeys || useExistingPK) command += "PopupNum,";
        command += "PatNum,Description,IsDisabled,PopupLevel,UserNum,DateTimeEntry,IsArchived,PopupNumArchive,DateTimeDisabled) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(popup.PopupNum) + ",";
        command +=
            SOut.Long(popup.PatNum) + ","
                                    + DbHelper.ParamChar + "paramDescription,"
                                    + SOut.Bool(popup.IsDisabled) + ","
                                    + SOut.Int((int) popup.PopupLevel) + ","
                                    + SOut.Long(popup.UserNum) + ","
                                    + DbHelper.Now() + ","
                                    + SOut.Bool(popup.IsArchived) + ","
                                    + SOut.Long(popup.PopupNumArchive) + ","
                                    + SOut.DateT(popup.DateTimeDisabled) + ")";
        if (popup.Description == null) popup.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(popup.Description));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription);
        else
            popup.PopupNum = Db.NonQ(command, true, "PopupNum", "popup", paramDescription);
        return popup.PopupNum;
    }

    public static void Update(Popup popup)
    {
        var command = "UPDATE popup SET "
                      + "PatNum          =  " + SOut.Long(popup.PatNum) + ", "
                      + "Description     =  " + DbHelper.ParamChar + "paramDescription, "
                      + "IsDisabled      =  " + SOut.Bool(popup.IsDisabled) + ", "
                      + "PopupLevel      =  " + SOut.Int((int) popup.PopupLevel) + ", "
                      + "UserNum         =  " + SOut.Long(popup.UserNum) + ", "
                      //DateTimeEntry not allowed to change
                      + "IsArchived      =  " + SOut.Bool(popup.IsArchived) + ", "
                      + "PopupNumArchive =  " + SOut.Long(popup.PopupNumArchive) + ", "
                      + "DateTimeDisabled=  " + SOut.DateT(popup.DateTimeDisabled) + " "
                      + "WHERE PopupNum = " + SOut.Long(popup.PopupNum);
        if (popup.Description == null) popup.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(popup.Description));
        Db.NonQ(command, paramDescription);
    }

    public static bool Update(Popup popup, Popup oldPopup)
    {
        var command = "";
        if (popup.PatNum != oldPopup.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(popup.PatNum) + "";
        }

        if (popup.Description != oldPopup.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (popup.IsDisabled != oldPopup.IsDisabled)
        {
            if (command != "") command += ",";
            command += "IsDisabled = " + SOut.Bool(popup.IsDisabled) + "";
        }

        if (popup.PopupLevel != oldPopup.PopupLevel)
        {
            if (command != "") command += ",";
            command += "PopupLevel = " + SOut.Int((int) popup.PopupLevel) + "";
        }

        if (popup.UserNum != oldPopup.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(popup.UserNum) + "";
        }

        //DateTimeEntry not allowed to change
        if (popup.IsArchived != oldPopup.IsArchived)
        {
            if (command != "") command += ",";
            command += "IsArchived = " + SOut.Bool(popup.IsArchived) + "";
        }

        if (popup.PopupNumArchive != oldPopup.PopupNumArchive)
        {
            if (command != "") command += ",";
            command += "PopupNumArchive = " + SOut.Long(popup.PopupNumArchive) + "";
        }

        if (popup.DateTimeDisabled != oldPopup.DateTimeDisabled)
        {
            if (command != "") command += ",";
            command += "DateTimeDisabled = " + SOut.DateT(popup.DateTimeDisabled) + "";
        }

        if (command == "") return false;
        if (popup.Description == null) popup.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(popup.Description));
        command = "UPDATE popup SET " + command
                                      + " WHERE PopupNum = " + SOut.Long(popup.PopupNum);
        Db.NonQ(command, paramDescription);
        return true;
    }

    public static bool UpdateComparison(Popup popup, Popup oldPopup)
    {
        if (popup.PatNum != oldPopup.PatNum) return true;
        if (popup.Description != oldPopup.Description) return true;
        if (popup.IsDisabled != oldPopup.IsDisabled) return true;
        if (popup.PopupLevel != oldPopup.PopupLevel) return true;
        if (popup.UserNum != oldPopup.UserNum) return true;
        //DateTimeEntry not allowed to change
        if (popup.IsArchived != oldPopup.IsArchived) return true;
        if (popup.PopupNumArchive != oldPopup.PopupNumArchive) return true;
        if (popup.DateTimeDisabled != oldPopup.DateTimeDisabled) return true;
        return false;
    }


    public static void Delete(long popupNum)
    {
        var command = "DELETE FROM popup "
                      + "WHERE PopupNum = " + SOut.Long(popupNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listPopupNums)
    {
        if (listPopupNums == null || listPopupNums.Count == 0) return;
        var command = "DELETE FROM popup "
                      + "WHERE PopupNum IN(" + string.Join(",", listPopupNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}