#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ERoutingDefCrud
{
    public static ERoutingDef SelectOne(long eRoutingDefNum)
    {
        var command = "SELECT * FROM eroutingdef "
                      + "WHERE ERoutingDefNum = " + SOut.Long(eRoutingDefNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ERoutingDef SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ERoutingDef> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERoutingDef> TableToList(DataTable table)
    {
        var retVal = new List<ERoutingDef>();
        ERoutingDef eRoutingDef;
        foreach (DataRow row in table.Rows)
        {
            eRoutingDef = new ERoutingDef();
            eRoutingDef.ERoutingDefNum = SIn.Long(row["ERoutingDefNum"].ToString());
            eRoutingDef.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            eRoutingDef.Description = SIn.String(row["Description"].ToString());
            eRoutingDef.UserNumCreated = SIn.Long(row["UserNumCreated"].ToString());
            eRoutingDef.UserNumModified = SIn.Long(row["UserNumModified"].ToString());
            eRoutingDef.SecDateTEntered = SIn.DateTime(row["SecDateTEntered"].ToString());
            eRoutingDef.DateLastModified = SIn.DateTime(row["DateLastModified"].ToString());
            retVal.Add(eRoutingDef);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ERoutingDef> listERoutingDefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERoutingDef";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingDefNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("Description");
        table.Columns.Add("UserNumCreated");
        table.Columns.Add("UserNumModified");
        table.Columns.Add("SecDateTEntered");
        table.Columns.Add("DateLastModified");
        foreach (var eRoutingDef in listERoutingDefs)
            table.Rows.Add(SOut.Long(eRoutingDef.ERoutingDefNum), SOut.Long(eRoutingDef.ClinicNum), eRoutingDef.Description, SOut.Long(eRoutingDef.UserNumCreated), SOut.Long(eRoutingDef.UserNumModified), SOut.DateT(eRoutingDef.SecDateTEntered, false), SOut.DateT(eRoutingDef.DateLastModified, false));
        return table;
    }

    public static long Insert(ERoutingDef eRoutingDef)
    {
        return Insert(eRoutingDef, false);
    }

    public static long Insert(ERoutingDef eRoutingDef, bool useExistingPK)
    {
        var command = "INSERT INTO eroutingdef (";

        command += "ClinicNum,Description,UserNumCreated,UserNumModified,SecDateTEntered,DateLastModified) VALUES(";

        command +=
            SOut.Long(eRoutingDef.ClinicNum) + ","
                                             + "'" + SOut.String(eRoutingDef.Description) + "',"
                                             + SOut.Long(eRoutingDef.UserNumCreated) + ","
                                             + SOut.Long(eRoutingDef.UserNumModified) + ","
                                             + DbHelper.Now() + ","
                                             + SOut.DateT(eRoutingDef.DateLastModified) + ")";
        {
            eRoutingDef.ERoutingDefNum = Db.NonQ(command, true, "ERoutingDefNum", "eRoutingDef");
        }
        return eRoutingDef.ERoutingDefNum;
    }

    public static long InsertNoCache(ERoutingDef eRoutingDef)
    {
        return InsertNoCache(eRoutingDef, false);
    }

    public static long InsertNoCache(ERoutingDef eRoutingDef, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eroutingdef (";
        if (isRandomKeys || useExistingPK) command += "ERoutingDefNum,";
        command += "ClinicNum,Description,UserNumCreated,UserNumModified,SecDateTEntered,DateLastModified) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eRoutingDef.ERoutingDefNum) + ",";
        command +=
            SOut.Long(eRoutingDef.ClinicNum) + ","
                                             + "'" + SOut.String(eRoutingDef.Description) + "',"
                                             + SOut.Long(eRoutingDef.UserNumCreated) + ","
                                             + SOut.Long(eRoutingDef.UserNumModified) + ","
                                             + DbHelper.Now() + ","
                                             + SOut.DateT(eRoutingDef.DateLastModified) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eRoutingDef.ERoutingDefNum = Db.NonQ(command, true, "ERoutingDefNum", "eRoutingDef");
        return eRoutingDef.ERoutingDefNum;
    }

    public static void Update(ERoutingDef eRoutingDef)
    {
        var command = "UPDATE eroutingdef SET "
                      + "ClinicNum       =  " + SOut.Long(eRoutingDef.ClinicNum) + ", "
                      + "Description     = '" + SOut.String(eRoutingDef.Description) + "', "
                      + "UserNumCreated  =  " + SOut.Long(eRoutingDef.UserNumCreated) + ", "
                      + "UserNumModified =  " + SOut.Long(eRoutingDef.UserNumModified) + ", "
                      //SecDateTEntered not allowed to change
                      + "DateLastModified=  " + SOut.DateT(eRoutingDef.DateLastModified) + " "
                      + "WHERE ERoutingDefNum = " + SOut.Long(eRoutingDef.ERoutingDefNum);
        Db.NonQ(command);
    }

    public static bool Update(ERoutingDef eRoutingDef, ERoutingDef oldERoutingDef)
    {
        var command = "";
        if (eRoutingDef.ClinicNum != oldERoutingDef.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(eRoutingDef.ClinicNum) + "";
        }

        if (eRoutingDef.Description != oldERoutingDef.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(eRoutingDef.Description) + "'";
        }

        if (eRoutingDef.UserNumCreated != oldERoutingDef.UserNumCreated)
        {
            if (command != "") command += ",";
            command += "UserNumCreated = " + SOut.Long(eRoutingDef.UserNumCreated) + "";
        }

        if (eRoutingDef.UserNumModified != oldERoutingDef.UserNumModified)
        {
            if (command != "") command += ",";
            command += "UserNumModified = " + SOut.Long(eRoutingDef.UserNumModified) + "";
        }

        //SecDateTEntered not allowed to change
        if (eRoutingDef.DateLastModified != oldERoutingDef.DateLastModified)
        {
            if (command != "") command += ",";
            command += "DateLastModified = " + SOut.DateT(eRoutingDef.DateLastModified) + "";
        }

        if (command == "") return false;
        command = "UPDATE eroutingdef SET " + command
                                            + " WHERE ERoutingDefNum = " + SOut.Long(eRoutingDef.ERoutingDefNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ERoutingDef eRoutingDef, ERoutingDef oldERoutingDef)
    {
        if (eRoutingDef.ClinicNum != oldERoutingDef.ClinicNum) return true;
        if (eRoutingDef.Description != oldERoutingDef.Description) return true;
        if (eRoutingDef.UserNumCreated != oldERoutingDef.UserNumCreated) return true;
        if (eRoutingDef.UserNumModified != oldERoutingDef.UserNumModified) return true;
        //SecDateTEntered not allowed to change
        if (eRoutingDef.DateLastModified != oldERoutingDef.DateLastModified) return true;
        return false;
    }

    public static void Delete(long eRoutingDefNum)
    {
        var command = "DELETE FROM eroutingdef "
                      + "WHERE ERoutingDefNum = " + SOut.Long(eRoutingDefNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listERoutingDefNums)
    {
        if (listERoutingDefNums == null || listERoutingDefNums.Count == 0) return;
        var command = "DELETE FROM eroutingdef "
                      + "WHERE ERoutingDefNum IN(" + string.Join(",", listERoutingDefNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}