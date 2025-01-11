#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserodApptViewCrud
{
    public static UserodApptView SelectOne(long userodApptViewNum)
    {
        var command = "SELECT * FROM userodapptview "
                      + "WHERE UserodApptViewNum = " + SOut.Long(userodApptViewNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UserodApptView SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserodApptView> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserodApptView> TableToList(DataTable table)
    {
        var retVal = new List<UserodApptView>();
        UserodApptView userodApptView;
        foreach (DataRow row in table.Rows)
        {
            userodApptView = new UserodApptView();
            userodApptView.UserodApptViewNum = SIn.Long(row["UserodApptViewNum"].ToString());
            userodApptView.UserNum = SIn.Long(row["UserNum"].ToString());
            userodApptView.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            userodApptView.ApptViewNum = SIn.Long(row["ApptViewNum"].ToString());
            retVal.Add(userodApptView);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserodApptView> listUserodApptViews, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserodApptView";
        var table = new DataTable(tableName);
        table.Columns.Add("UserodApptViewNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ApptViewNum");
        foreach (var userodApptView in listUserodApptViews)
            table.Rows.Add(SOut.Long(userodApptView.UserodApptViewNum), SOut.Long(userodApptView.UserNum), SOut.Long(userodApptView.ClinicNum), SOut.Long(userodApptView.ApptViewNum));
        return table;
    }

    public static long Insert(UserodApptView userodApptView)
    {
        return Insert(userodApptView, false);
    }

    public static long Insert(UserodApptView userodApptView, bool useExistingPK)
    {
        var command = "INSERT INTO userodapptview (";

        command += "UserNum,ClinicNum,ApptViewNum) VALUES(";

        command +=
            SOut.Long(userodApptView.UserNum) + ","
                                              + SOut.Long(userodApptView.ClinicNum) + ","
                                              + SOut.Long(userodApptView.ApptViewNum) + ")";
        {
            userodApptView.UserodApptViewNum = Db.NonQ(command, true, "UserodApptViewNum", "userodApptView");
        }
        return userodApptView.UserodApptViewNum;
    }

    public static long InsertNoCache(UserodApptView userodApptView)
    {
        return InsertNoCache(userodApptView, false);
    }

    public static long InsertNoCache(UserodApptView userodApptView, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO userodapptview (";
        if (isRandomKeys || useExistingPK) command += "UserodApptViewNum,";
        command += "UserNum,ClinicNum,ApptViewNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userodApptView.UserodApptViewNum) + ",";
        command +=
            SOut.Long(userodApptView.UserNum) + ","
                                              + SOut.Long(userodApptView.ClinicNum) + ","
                                              + SOut.Long(userodApptView.ApptViewNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            userodApptView.UserodApptViewNum = Db.NonQ(command, true, "UserodApptViewNum", "userodApptView");
        return userodApptView.UserodApptViewNum;
    }

    public static void Update(UserodApptView userodApptView)
    {
        var command = "UPDATE userodapptview SET "
                      + "UserNum          =  " + SOut.Long(userodApptView.UserNum) + ", "
                      + "ClinicNum        =  " + SOut.Long(userodApptView.ClinicNum) + ", "
                      + "ApptViewNum      =  " + SOut.Long(userodApptView.ApptViewNum) + " "
                      + "WHERE UserodApptViewNum = " + SOut.Long(userodApptView.UserodApptViewNum);
        Db.NonQ(command);
    }

    public static bool Update(UserodApptView userodApptView, UserodApptView oldUserodApptView)
    {
        var command = "";
        if (userodApptView.UserNum != oldUserodApptView.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(userodApptView.UserNum) + "";
        }

        if (userodApptView.ClinicNum != oldUserodApptView.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(userodApptView.ClinicNum) + "";
        }

        if (userodApptView.ApptViewNum != oldUserodApptView.ApptViewNum)
        {
            if (command != "") command += ",";
            command += "ApptViewNum = " + SOut.Long(userodApptView.ApptViewNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE userodapptview SET " + command
                                               + " WHERE UserodApptViewNum = " + SOut.Long(userodApptView.UserodApptViewNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(UserodApptView userodApptView, UserodApptView oldUserodApptView)
    {
        if (userodApptView.UserNum != oldUserodApptView.UserNum) return true;
        if (userodApptView.ClinicNum != oldUserodApptView.ClinicNum) return true;
        if (userodApptView.ApptViewNum != oldUserodApptView.ApptViewNum) return true;
        return false;
    }

    public static void Delete(long userodApptViewNum)
    {
        var command = "DELETE FROM userodapptview "
                      + "WHERE UserodApptViewNum = " + SOut.Long(userodApptViewNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserodApptViewNums)
    {
        if (listUserodApptViewNums == null || listUserodApptViewNums.Count == 0) return;
        var command = "DELETE FROM userodapptview "
                      + "WHERE UserodApptViewNum IN(" + string.Join(",", listUserodApptViewNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}