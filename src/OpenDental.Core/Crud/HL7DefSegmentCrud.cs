#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HL7DefSegmentCrud
{
    public static HL7DefSegment SelectOne(long hL7DefSegmentNum)
    {
        var command = "SELECT * FROM hl7defsegment "
                      + "WHERE HL7DefSegmentNum = " + SOut.Long(hL7DefSegmentNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HL7DefSegment SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HL7DefSegment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HL7DefSegment> TableToList(DataTable table)
    {
        var retVal = new List<HL7DefSegment>();
        HL7DefSegment hL7DefSegment;
        foreach (DataRow row in table.Rows)
        {
            hL7DefSegment = new HL7DefSegment();
            hL7DefSegment.HL7DefSegmentNum = SIn.Long(row["HL7DefSegmentNum"].ToString());
            hL7DefSegment.HL7DefMessageNum = SIn.Long(row["HL7DefMessageNum"].ToString());
            hL7DefSegment.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            hL7DefSegment.CanRepeat = SIn.Bool(row["CanRepeat"].ToString());
            hL7DefSegment.IsOptional = SIn.Bool(row["IsOptional"].ToString());
            var segmentName = row["SegmentName"].ToString();
            if (segmentName == "")
                hL7DefSegment.SegmentName = 0;
            else
                try
                {
                    hL7DefSegment.SegmentName = (SegmentNameHL7) Enum.Parse(typeof(SegmentNameHL7), segmentName);
                }
                catch
                {
                    hL7DefSegment.SegmentName = 0;
                }

            hL7DefSegment.Note = SIn.String(row["Note"].ToString());
            retVal.Add(hL7DefSegment);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HL7DefSegment> listHL7DefSegments, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HL7DefSegment";
        var table = new DataTable(tableName);
        table.Columns.Add("HL7DefSegmentNum");
        table.Columns.Add("HL7DefMessageNum");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("CanRepeat");
        table.Columns.Add("IsOptional");
        table.Columns.Add("SegmentName");
        table.Columns.Add("Note");
        foreach (var hL7DefSegment in listHL7DefSegments)
            table.Rows.Add(SOut.Long(hL7DefSegment.HL7DefSegmentNum), SOut.Long(hL7DefSegment.HL7DefMessageNum), SOut.Int(hL7DefSegment.ItemOrder), SOut.Bool(hL7DefSegment.CanRepeat), SOut.Bool(hL7DefSegment.IsOptional), SOut.Int((int) hL7DefSegment.SegmentName), hL7DefSegment.Note);
        return table;
    }

    public static long Insert(HL7DefSegment hL7DefSegment)
    {
        return Insert(hL7DefSegment, false);
    }

    public static long Insert(HL7DefSegment hL7DefSegment, bool useExistingPK)
    {
        var command = "INSERT INTO hl7defsegment (";

        command += "HL7DefMessageNum,ItemOrder,CanRepeat,IsOptional,SegmentName,Note) VALUES(";

        command +=
            SOut.Long(hL7DefSegment.HL7DefMessageNum) + ","
                                                      + SOut.Int(hL7DefSegment.ItemOrder) + ","
                                                      + SOut.Bool(hL7DefSegment.CanRepeat) + ","
                                                      + SOut.Bool(hL7DefSegment.IsOptional) + ","
                                                      + "'" + SOut.String(hL7DefSegment.SegmentName.ToString()) + "',"
                                                      + DbHelper.ParamChar + "paramNote)";
        if (hL7DefSegment.Note == null) hL7DefSegment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefSegment.Note));
        {
            hL7DefSegment.HL7DefSegmentNum = Db.NonQ(command, true, "HL7DefSegmentNum", "hL7DefSegment", paramNote);
        }
        return hL7DefSegment.HL7DefSegmentNum;
    }

    public static long InsertNoCache(HL7DefSegment hL7DefSegment)
    {
        return InsertNoCache(hL7DefSegment, false);
    }

    public static long InsertNoCache(HL7DefSegment hL7DefSegment, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hl7defsegment (";
        if (isRandomKeys || useExistingPK) command += "HL7DefSegmentNum,";
        command += "HL7DefMessageNum,ItemOrder,CanRepeat,IsOptional,SegmentName,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hL7DefSegment.HL7DefSegmentNum) + ",";
        command +=
            SOut.Long(hL7DefSegment.HL7DefMessageNum) + ","
                                                      + SOut.Int(hL7DefSegment.ItemOrder) + ","
                                                      + SOut.Bool(hL7DefSegment.CanRepeat) + ","
                                                      + SOut.Bool(hL7DefSegment.IsOptional) + ","
                                                      + "'" + SOut.String(hL7DefSegment.SegmentName.ToString()) + "',"
                                                      + DbHelper.ParamChar + "paramNote)";
        if (hL7DefSegment.Note == null) hL7DefSegment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefSegment.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            hL7DefSegment.HL7DefSegmentNum = Db.NonQ(command, true, "HL7DefSegmentNum", "hL7DefSegment", paramNote);
        return hL7DefSegment.HL7DefSegmentNum;
    }

    public static void Update(HL7DefSegment hL7DefSegment)
    {
        var command = "UPDATE hl7defsegment SET "
                      + "HL7DefMessageNum=  " + SOut.Long(hL7DefSegment.HL7DefMessageNum) + ", "
                      + "ItemOrder       =  " + SOut.Int(hL7DefSegment.ItemOrder) + ", "
                      + "CanRepeat       =  " + SOut.Bool(hL7DefSegment.CanRepeat) + ", "
                      + "IsOptional      =  " + SOut.Bool(hL7DefSegment.IsOptional) + ", "
                      + "SegmentName     = '" + SOut.String(hL7DefSegment.SegmentName.ToString()) + "', "
                      + "Note            =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE HL7DefSegmentNum = " + SOut.Long(hL7DefSegment.HL7DefSegmentNum);
        if (hL7DefSegment.Note == null) hL7DefSegment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefSegment.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(HL7DefSegment hL7DefSegment, HL7DefSegment oldHL7DefSegment)
    {
        var command = "";
        if (hL7DefSegment.HL7DefMessageNum != oldHL7DefSegment.HL7DefMessageNum)
        {
            if (command != "") command += ",";
            command += "HL7DefMessageNum = " + SOut.Long(hL7DefSegment.HL7DefMessageNum) + "";
        }

        if (hL7DefSegment.ItemOrder != oldHL7DefSegment.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(hL7DefSegment.ItemOrder) + "";
        }

        if (hL7DefSegment.CanRepeat != oldHL7DefSegment.CanRepeat)
        {
            if (command != "") command += ",";
            command += "CanRepeat = " + SOut.Bool(hL7DefSegment.CanRepeat) + "";
        }

        if (hL7DefSegment.IsOptional != oldHL7DefSegment.IsOptional)
        {
            if (command != "") command += ",";
            command += "IsOptional = " + SOut.Bool(hL7DefSegment.IsOptional) + "";
        }

        if (hL7DefSegment.SegmentName != oldHL7DefSegment.SegmentName)
        {
            if (command != "") command += ",";
            command += "SegmentName = '" + SOut.String(hL7DefSegment.SegmentName.ToString()) + "'";
        }

        if (hL7DefSegment.Note != oldHL7DefSegment.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (hL7DefSegment.Note == null) hL7DefSegment.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(hL7DefSegment.Note));
        command = "UPDATE hl7defsegment SET " + command
                                              + " WHERE HL7DefSegmentNum = " + SOut.Long(hL7DefSegment.HL7DefSegmentNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(HL7DefSegment hL7DefSegment, HL7DefSegment oldHL7DefSegment)
    {
        if (hL7DefSegment.HL7DefMessageNum != oldHL7DefSegment.HL7DefMessageNum) return true;
        if (hL7DefSegment.ItemOrder != oldHL7DefSegment.ItemOrder) return true;
        if (hL7DefSegment.CanRepeat != oldHL7DefSegment.CanRepeat) return true;
        if (hL7DefSegment.IsOptional != oldHL7DefSegment.IsOptional) return true;
        if (hL7DefSegment.SegmentName != oldHL7DefSegment.SegmentName) return true;
        if (hL7DefSegment.Note != oldHL7DefSegment.Note) return true;
        return false;
    }

    public static void Delete(long hL7DefSegmentNum)
    {
        var command = "DELETE FROM hl7defsegment "
                      + "WHERE HL7DefSegmentNum = " + SOut.Long(hL7DefSegmentNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHL7DefSegmentNums)
    {
        if (listHL7DefSegmentNums == null || listHL7DefSegmentNums.Count == 0) return;
        var command = "DELETE FROM hl7defsegment "
                      + "WHERE HL7DefSegmentNum IN(" + string.Join(",", listHL7DefSegmentNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}