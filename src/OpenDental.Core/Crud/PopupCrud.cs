using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PopupCrud
{
    public static List<Popup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Popup> TableToList(DataTable table)
    {
        var retVal = new List<Popup>();
        foreach (DataRow row in table.Rows)
        {
            var popup = new Popup
            {
                PopupNum = SIn.Long(row["PopupNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                IsDisabled = SIn.Bool(row["IsDisabled"].ToString()),
                PopupLevel = (EnumPopupLevel) SIn.Int(row["PopupLevel"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString()),
                IsArchived = SIn.Bool(row["IsArchived"].ToString()),
                PopupNumArchive = SIn.Long(row["PopupNumArchive"].ToString()),
                DateTimeDisabled = SIn.DateTime(row["DateTimeDisabled"].ToString())
            };
            retVal.Add(popup);
        }

        return retVal;
    }

    public static long Insert(Popup popup)
    {
        var command = "INSERT INTO popup (";

        command += "PatNum,Description,IsDisabled,PopupLevel,UserNum,DateTimeEntry,IsArchived,PopupNumArchive,DateTimeDisabled) VALUES(";

        command +=
            SOut.Long(popup.PatNum) + ","
                                    + DbHelper.ParamChar + "paramDescription,"
                                    + SOut.Bool(popup.IsDisabled) + ","
                                    + SOut.Int((int) popup.PopupLevel) + ","
                                    + SOut.Long(popup.UserNum) + ","
                                    + "NOW()" + ","
                                    + SOut.Bool(popup.IsArchived) + ","
                                    + SOut.Long(popup.PopupNumArchive) + ","
                                    + SOut.DateTime(popup.DateTimeDisabled) + ")";
        if (popup.Description == null) popup.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(popup.Description));
        {
            popup.PopupNum = Db.NonQ(command, true, "PopupNum", "popup", paramDescription);
        }
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
                      + "DateTimeDisabled=  " + SOut.DateTime(popup.DateTimeDisabled) + " "
                      + "WHERE PopupNum = " + SOut.Long(popup.PopupNum);
        if (popup.Description == null) popup.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(popup.Description));
        Db.NonQ(command, paramDescription);
    }
}