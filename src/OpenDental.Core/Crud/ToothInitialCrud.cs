#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ToothInitialCrud
{
    public static ToothInitial SelectOne(long toothInitialNum)
    {
        var command = "SELECT * FROM toothinitial "
                      + "WHERE ToothInitialNum = " + SOut.Long(toothInitialNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ToothInitial SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ToothInitial> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToothInitial> TableToList(DataTable table)
    {
        var retVal = new List<ToothInitial>();
        ToothInitial toothInitial;
        foreach (DataRow row in table.Rows)
        {
            toothInitial = new ToothInitial();
            toothInitial.ToothInitialNum = SIn.Long(row["ToothInitialNum"].ToString());
            toothInitial.PatNum = SIn.Long(row["PatNum"].ToString());
            toothInitial.ToothNum = SIn.String(row["ToothNum"].ToString());
            toothInitial.InitialType = (ToothInitialType) SIn.Int(row["InitialType"].ToString());
            toothInitial.Movement = SIn.Float(row["Movement"].ToString());
            toothInitial.DrawingSegment = SIn.String(row["DrawingSegment"].ToString());
            toothInitial.ColorDraw = Color.FromArgb(SIn.Int(row["ColorDraw"].ToString()));
            toothInitial.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            toothInitial.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            toothInitial.DrawText = SIn.String(row["DrawText"].ToString());
            retVal.Add(toothInitial);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ToothInitial> listToothInitials, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ToothInitial";
        var table = new DataTable(tableName);
        table.Columns.Add("ToothInitialNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("ToothNum");
        table.Columns.Add("InitialType");
        table.Columns.Add("Movement");
        table.Columns.Add("DrawingSegment");
        table.Columns.Add("ColorDraw");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("DrawText");
        foreach (var toothInitial in listToothInitials)
            table.Rows.Add(SOut.Long(toothInitial.ToothInitialNum), SOut.Long(toothInitial.PatNum), toothInitial.ToothNum, SOut.Int((int) toothInitial.InitialType), SOut.Float(toothInitial.Movement), toothInitial.DrawingSegment, SOut.Int(toothInitial.ColorDraw.ToArgb()), SOut.DateT(toothInitial.SecDateTEntry, false), SOut.DateT(toothInitial.SecDateTEdit, false), toothInitial.DrawText);
        return table;
    }

    public static long Insert(ToothInitial toothInitial)
    {
        return Insert(toothInitial, false);
    }

    public static long Insert(ToothInitial toothInitial, bool useExistingPK)
    {
        var command = "INSERT INTO toothinitial (";

        command += "PatNum,ToothNum,InitialType,Movement,DrawingSegment,ColorDraw,SecDateTEntry,DrawText) VALUES(";

        command +=
            SOut.Long(toothInitial.PatNum) + ","
                                           + "'" + SOut.String(toothInitial.ToothNum) + "',"
                                           + SOut.Int((int) toothInitial.InitialType) + ","
                                           + SOut.Float(toothInitial.Movement) + ","
                                           + DbHelper.ParamChar + "paramDrawingSegment,"
                                           + SOut.Int(toothInitial.ColorDraw.ToArgb()) + ","
                                           + DbHelper.Now() + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + "'" + SOut.String(toothInitial.DrawText) + "')";
        if (toothInitial.DrawingSegment == null) toothInitial.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(toothInitial.DrawingSegment));
        {
            toothInitial.ToothInitialNum = Db.NonQ(command, true, "ToothInitialNum", "toothInitial", paramDrawingSegment);
        }
        return toothInitial.ToothInitialNum;
    }

    public static long InsertNoCache(ToothInitial toothInitial)
    {
        return InsertNoCache(toothInitial, false);
    }

    public static long InsertNoCache(ToothInitial toothInitial, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO toothinitial (";
        if (isRandomKeys || useExistingPK) command += "ToothInitialNum,";
        command += "PatNum,ToothNum,InitialType,Movement,DrawingSegment,ColorDraw,SecDateTEntry,DrawText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(toothInitial.ToothInitialNum) + ",";
        command +=
            SOut.Long(toothInitial.PatNum) + ","
                                           + "'" + SOut.String(toothInitial.ToothNum) + "',"
                                           + SOut.Int((int) toothInitial.InitialType) + ","
                                           + SOut.Float(toothInitial.Movement) + ","
                                           + DbHelper.ParamChar + "paramDrawingSegment,"
                                           + SOut.Int(toothInitial.ColorDraw.ToArgb()) + ","
                                           + DbHelper.Now() + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + "'" + SOut.String(toothInitial.DrawText) + "')";
        if (toothInitial.DrawingSegment == null) toothInitial.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(toothInitial.DrawingSegment));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDrawingSegment);
        else
            toothInitial.ToothInitialNum = Db.NonQ(command, true, "ToothInitialNum", "toothInitial", paramDrawingSegment);
        return toothInitial.ToothInitialNum;
    }

    public static void Update(ToothInitial toothInitial)
    {
        var command = "UPDATE toothinitial SET "
                      + "PatNum         =  " + SOut.Long(toothInitial.PatNum) + ", "
                      + "ToothNum       = '" + SOut.String(toothInitial.ToothNum) + "', "
                      + "InitialType    =  " + SOut.Int((int) toothInitial.InitialType) + ", "
                      + "Movement       =  " + SOut.Float(toothInitial.Movement) + ", "
                      + "DrawingSegment =  " + DbHelper.ParamChar + "paramDrawingSegment, "
                      + "ColorDraw      =  " + SOut.Int(toothInitial.ColorDraw.ToArgb()) + ", "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "DrawText       = '" + SOut.String(toothInitial.DrawText) + "' "
                      + "WHERE ToothInitialNum = " + SOut.Long(toothInitial.ToothInitialNum);
        if (toothInitial.DrawingSegment == null) toothInitial.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(toothInitial.DrawingSegment));
        Db.NonQ(command, paramDrawingSegment);
    }

    public static bool Update(ToothInitial toothInitial, ToothInitial oldToothInitial)
    {
        var command = "";
        if (toothInitial.PatNum != oldToothInitial.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(toothInitial.PatNum) + "";
        }

        if (toothInitial.ToothNum != oldToothInitial.ToothNum)
        {
            if (command != "") command += ",";
            command += "ToothNum = '" + SOut.String(toothInitial.ToothNum) + "'";
        }

        if (toothInitial.InitialType != oldToothInitial.InitialType)
        {
            if (command != "") command += ",";
            command += "InitialType = " + SOut.Int((int) toothInitial.InitialType) + "";
        }

        if (toothInitial.Movement != oldToothInitial.Movement)
        {
            if (command != "") command += ",";
            command += "Movement = " + SOut.Float(toothInitial.Movement) + "";
        }

        if (toothInitial.DrawingSegment != oldToothInitial.DrawingSegment)
        {
            if (command != "") command += ",";
            command += "DrawingSegment = " + DbHelper.ParamChar + "paramDrawingSegment";
        }

        if (toothInitial.ColorDraw != oldToothInitial.ColorDraw)
        {
            if (command != "") command += ",";
            command += "ColorDraw = " + SOut.Int(toothInitial.ColorDraw.ToArgb()) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (toothInitial.DrawText != oldToothInitial.DrawText)
        {
            if (command != "") command += ",";
            command += "DrawText = '" + SOut.String(toothInitial.DrawText) + "'";
        }

        if (command == "") return false;
        if (toothInitial.DrawingSegment == null) toothInitial.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", OdDbType.Text, SOut.StringParam(toothInitial.DrawingSegment));
        command = "UPDATE toothinitial SET " + command
                                             + " WHERE ToothInitialNum = " + SOut.Long(toothInitial.ToothInitialNum);
        Db.NonQ(command, paramDrawingSegment);
        return true;
    }

    public static bool UpdateComparison(ToothInitial toothInitial, ToothInitial oldToothInitial)
    {
        if (toothInitial.PatNum != oldToothInitial.PatNum) return true;
        if (toothInitial.ToothNum != oldToothInitial.ToothNum) return true;
        if (toothInitial.InitialType != oldToothInitial.InitialType) return true;
        if (toothInitial.Movement != oldToothInitial.Movement) return true;
        if (toothInitial.DrawingSegment != oldToothInitial.DrawingSegment) return true;
        if (toothInitial.ColorDraw != oldToothInitial.ColorDraw) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (toothInitial.DrawText != oldToothInitial.DrawText) return true;
        return false;
    }

    public static void Delete(long toothInitialNum)
    {
        var command = "DELETE FROM toothinitial "
                      + "WHERE ToothInitialNum = " + SOut.Long(toothInitialNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listToothInitialNums)
    {
        if (listToothInitialNums == null || listToothInitialNums.Count == 0) return;
        var command = "DELETE FROM toothinitial "
                      + "WHERE ToothInitialNum IN(" + string.Join(",", listToothInitialNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}