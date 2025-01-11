#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DispSupplyCrud
{
    public static DispSupply SelectOne(long dispSupplyNum)
    {
        var command = "SELECT * FROM dispsupply "
                      + "WHERE DispSupplyNum = " + SOut.Long(dispSupplyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DispSupply SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DispSupply> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DispSupply> TableToList(DataTable table)
    {
        var retVal = new List<DispSupply>();
        DispSupply dispSupply;
        foreach (DataRow row in table.Rows)
        {
            dispSupply = new DispSupply();
            dispSupply.DispSupplyNum = SIn.Long(row["DispSupplyNum"].ToString());
            dispSupply.SupplyNum = SIn.Long(row["SupplyNum"].ToString());
            dispSupply.ProvNum = SIn.Long(row["ProvNum"].ToString());
            dispSupply.DateDispensed = SIn.Date(row["DateDispensed"].ToString());
            dispSupply.DispQuantity = SIn.Float(row["DispQuantity"].ToString());
            dispSupply.Note = SIn.String(row["Note"].ToString());
            retVal.Add(dispSupply);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DispSupply> listDispSupplys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DispSupply";
        var table = new DataTable(tableName);
        table.Columns.Add("DispSupplyNum");
        table.Columns.Add("SupplyNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("DateDispensed");
        table.Columns.Add("DispQuantity");
        table.Columns.Add("Note");
        foreach (var dispSupply in listDispSupplys)
            table.Rows.Add(SOut.Long(dispSupply.DispSupplyNum), SOut.Long(dispSupply.SupplyNum), SOut.Long(dispSupply.ProvNum), SOut.DateT(dispSupply.DateDispensed, false), SOut.Float(dispSupply.DispQuantity), dispSupply.Note);
        return table;
    }

    public static long Insert(DispSupply dispSupply)
    {
        return Insert(dispSupply, false);
    }

    public static long Insert(DispSupply dispSupply, bool useExistingPK)
    {
        var command = "INSERT INTO dispsupply (";

        command += "SupplyNum,ProvNum,DateDispensed,DispQuantity,Note) VALUES(";

        command +=
            SOut.Long(dispSupply.SupplyNum) + ","
                                            + SOut.Long(dispSupply.ProvNum) + ","
                                            + SOut.Date(dispSupply.DateDispensed) + ","
                                            + SOut.Float(dispSupply.DispQuantity) + ","
                                            + DbHelper.ParamChar + "paramNote)";
        if (dispSupply.Note == null) dispSupply.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(dispSupply.Note));
        {
            dispSupply.DispSupplyNum = Db.NonQ(command, true, "DispSupplyNum", "dispSupply", paramNote);
        }
        return dispSupply.DispSupplyNum;
    }

    public static long InsertNoCache(DispSupply dispSupply)
    {
        return InsertNoCache(dispSupply, false);
    }

    public static long InsertNoCache(DispSupply dispSupply, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dispsupply (";
        if (isRandomKeys || useExistingPK) command += "DispSupplyNum,";
        command += "SupplyNum,ProvNum,DateDispensed,DispQuantity,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dispSupply.DispSupplyNum) + ",";
        command +=
            SOut.Long(dispSupply.SupplyNum) + ","
                                            + SOut.Long(dispSupply.ProvNum) + ","
                                            + SOut.Date(dispSupply.DateDispensed) + ","
                                            + SOut.Float(dispSupply.DispQuantity) + ","
                                            + DbHelper.ParamChar + "paramNote)";
        if (dispSupply.Note == null) dispSupply.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(dispSupply.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            dispSupply.DispSupplyNum = Db.NonQ(command, true, "DispSupplyNum", "dispSupply", paramNote);
        return dispSupply.DispSupplyNum;
    }

    public static void Update(DispSupply dispSupply)
    {
        var command = "UPDATE dispsupply SET "
                      + "SupplyNum    =  " + SOut.Long(dispSupply.SupplyNum) + ", "
                      + "ProvNum      =  " + SOut.Long(dispSupply.ProvNum) + ", "
                      + "DateDispensed=  " + SOut.Date(dispSupply.DateDispensed) + ", "
                      + "DispQuantity =  " + SOut.Float(dispSupply.DispQuantity) + ", "
                      + "Note         =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE DispSupplyNum = " + SOut.Long(dispSupply.DispSupplyNum);
        if (dispSupply.Note == null) dispSupply.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(dispSupply.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(DispSupply dispSupply, DispSupply oldDispSupply)
    {
        var command = "";
        if (dispSupply.SupplyNum != oldDispSupply.SupplyNum)
        {
            if (command != "") command += ",";
            command += "SupplyNum = " + SOut.Long(dispSupply.SupplyNum) + "";
        }

        if (dispSupply.ProvNum != oldDispSupply.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(dispSupply.ProvNum) + "";
        }

        if (dispSupply.DateDispensed.Date != oldDispSupply.DateDispensed.Date)
        {
            if (command != "") command += ",";
            command += "DateDispensed = " + SOut.Date(dispSupply.DateDispensed) + "";
        }

        if (dispSupply.DispQuantity != oldDispSupply.DispQuantity)
        {
            if (command != "") command += ",";
            command += "DispQuantity = " + SOut.Float(dispSupply.DispQuantity) + "";
        }

        if (dispSupply.Note != oldDispSupply.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (dispSupply.Note == null) dispSupply.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringNote(dispSupply.Note));
        command = "UPDATE dispsupply SET " + command
                                           + " WHERE DispSupplyNum = " + SOut.Long(dispSupply.DispSupplyNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(DispSupply dispSupply, DispSupply oldDispSupply)
    {
        if (dispSupply.SupplyNum != oldDispSupply.SupplyNum) return true;
        if (dispSupply.ProvNum != oldDispSupply.ProvNum) return true;
        if (dispSupply.DateDispensed.Date != oldDispSupply.DateDispensed.Date) return true;
        if (dispSupply.DispQuantity != oldDispSupply.DispQuantity) return true;
        if (dispSupply.Note != oldDispSupply.Note) return true;
        return false;
    }

    public static void Delete(long dispSupplyNum)
    {
        var command = "DELETE FROM dispsupply "
                      + "WHERE DispSupplyNum = " + SOut.Long(dispSupplyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDispSupplyNums)
    {
        if (listDispSupplyNums == null || listDispSupplyNums.Count == 0) return;
        var command = "DELETE FROM dispsupply "
                      + "WHERE DispSupplyNum IN(" + string.Join(",", listDispSupplyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}