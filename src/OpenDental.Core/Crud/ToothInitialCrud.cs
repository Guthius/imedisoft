using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ToothInitialCrud
{
    public static List<ToothInitial> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToothInitial> TableToList(DataTable table)
    {
        var retVal = new List<ToothInitial>();
        foreach (DataRow row in table.Rows)
        {
            var toothInitial = new ToothInitial
            {
                ToothInitialNum = SIn.Long(row["ToothInitialNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ToothNum = SIn.String(row["ToothNum"].ToString()),
                InitialType = (ToothInitialType) SIn.Int(row["InitialType"].ToString()),
                Movement = SIn.Float(row["Movement"].ToString()),
                DrawingSegment = SIn.String(row["DrawingSegment"].ToString()),
                ColorDraw = Color.FromArgb(SIn.Int(row["ColorDraw"].ToString())),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                DrawText = SIn.String(row["DrawText"].ToString())
            };
            retVal.Add(toothInitial);
        }

        return retVal;
    }

    public static void Insert(ToothInitial toothInitial)
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
                                           + "NOW()" + ","
                                           //SecDateTEdit can only be set by MySQL
                                           + "'" + SOut.String(toothInitial.DrawText) + "')";
        if (toothInitial.DrawingSegment == null) toothInitial.DrawingSegment = "";
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", SOut.StringParam(toothInitial.DrawingSegment));
        {
            toothInitial.ToothInitialNum = Db.NonQ(command, true, "ToothInitialNum", "toothInitial", paramDrawingSegment);
        }
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
        var paramDrawingSegment = new OdSqlParameter("paramDrawingSegment", SOut.StringParam(toothInitial.DrawingSegment));
        Db.NonQ(command, paramDrawingSegment);
    }
}