#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HL7DefFieldCrud
{
    public static HL7DefField SelectOne(long hL7DefFieldNum)
    {
        var command = "SELECT * FROM hl7deffield "
                      + "WHERE HL7DefFieldNum = " + SOut.Long(hL7DefFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HL7DefField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HL7DefField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HL7DefField> TableToList(DataTable table)
    {
        var retVal = new List<HL7DefField>();
        HL7DefField hL7DefField;
        foreach (DataRow row in table.Rows)
        {
            hL7DefField = new HL7DefField();
            hL7DefField.HL7DefFieldNum = SIn.Long(row["HL7DefFieldNum"].ToString());
            hL7DefField.HL7DefSegmentNum = SIn.Long(row["HL7DefSegmentNum"].ToString());
            hL7DefField.OrdinalPos = SIn.Int(row["OrdinalPos"].ToString());
            hL7DefField.TableId = SIn.String(row["TableId"].ToString());
            var dataType = row["DataType"].ToString();
            if (dataType == "")
                hL7DefField.DataType = 0;
            else
                try
                {
                    hL7DefField.DataType = (DataTypeHL7) Enum.Parse(typeof(DataTypeHL7), dataType);
                }
                catch
                {
                    hL7DefField.DataType = 0;
                }

            hL7DefField.FieldName = SIn.String(row["FieldName"].ToString());
            hL7DefField.FixedText = SIn.String(row["FixedText"].ToString());
            retVal.Add(hL7DefField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HL7DefField> listHL7DefFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HL7DefField";
        var table = new DataTable(tableName);
        table.Columns.Add("HL7DefFieldNum");
        table.Columns.Add("HL7DefSegmentNum");
        table.Columns.Add("OrdinalPos");
        table.Columns.Add("TableId");
        table.Columns.Add("DataType");
        table.Columns.Add("FieldName");
        table.Columns.Add("FixedText");
        foreach (var hL7DefField in listHL7DefFields)
            table.Rows.Add(SOut.Long(hL7DefField.HL7DefFieldNum), SOut.Long(hL7DefField.HL7DefSegmentNum), SOut.Int(hL7DefField.OrdinalPos), hL7DefField.TableId, SOut.Int((int) hL7DefField.DataType), hL7DefField.FieldName, hL7DefField.FixedText);
        return table;
    }

    public static long Insert(HL7DefField hL7DefField)
    {
        return Insert(hL7DefField, false);
    }

    public static long Insert(HL7DefField hL7DefField, bool useExistingPK)
    {
        var command = "INSERT INTO hl7deffield (";

        command += "HL7DefSegmentNum,OrdinalPos,TableId,DataType,FieldName,FixedText) VALUES(";

        command +=
            SOut.Long(hL7DefField.HL7DefSegmentNum) + ","
                                                    + SOut.Int(hL7DefField.OrdinalPos) + ","
                                                    + "'" + SOut.String(hL7DefField.TableId) + "',"
                                                    + "'" + SOut.String(hL7DefField.DataType.ToString()) + "',"
                                                    + "'" + SOut.String(hL7DefField.FieldName) + "',"
                                                    + DbHelper.ParamChar + "paramFixedText)";
        if (hL7DefField.FixedText == null) hL7DefField.FixedText = "";
        var paramFixedText = new OdSqlParameter("paramFixedText", OdDbType.Text, SOut.StringParam(hL7DefField.FixedText));
        {
            hL7DefField.HL7DefFieldNum = Db.NonQ(command, true, "HL7DefFieldNum", "hL7DefField", paramFixedText);
        }
        return hL7DefField.HL7DefFieldNum;
    }

    public static long InsertNoCache(HL7DefField hL7DefField)
    {
        return InsertNoCache(hL7DefField, false);
    }

    public static long InsertNoCache(HL7DefField hL7DefField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hl7deffield (";
        if (isRandomKeys || useExistingPK) command += "HL7DefFieldNum,";
        command += "HL7DefSegmentNum,OrdinalPos,TableId,DataType,FieldName,FixedText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hL7DefField.HL7DefFieldNum) + ",";
        command +=
            SOut.Long(hL7DefField.HL7DefSegmentNum) + ","
                                                    + SOut.Int(hL7DefField.OrdinalPos) + ","
                                                    + "'" + SOut.String(hL7DefField.TableId) + "',"
                                                    + "'" + SOut.String(hL7DefField.DataType.ToString()) + "',"
                                                    + "'" + SOut.String(hL7DefField.FieldName) + "',"
                                                    + DbHelper.ParamChar + "paramFixedText)";
        if (hL7DefField.FixedText == null) hL7DefField.FixedText = "";
        var paramFixedText = new OdSqlParameter("paramFixedText", OdDbType.Text, SOut.StringParam(hL7DefField.FixedText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramFixedText);
        else
            hL7DefField.HL7DefFieldNum = Db.NonQ(command, true, "HL7DefFieldNum", "hL7DefField", paramFixedText);
        return hL7DefField.HL7DefFieldNum;
    }

    public static void Update(HL7DefField hL7DefField)
    {
        var command = "UPDATE hl7deffield SET "
                      + "HL7DefSegmentNum=  " + SOut.Long(hL7DefField.HL7DefSegmentNum) + ", "
                      + "OrdinalPos      =  " + SOut.Int(hL7DefField.OrdinalPos) + ", "
                      + "TableId         = '" + SOut.String(hL7DefField.TableId) + "', "
                      + "DataType        = '" + SOut.String(hL7DefField.DataType.ToString()) + "', "
                      + "FieldName       = '" + SOut.String(hL7DefField.FieldName) + "', "
                      + "FixedText       =  " + DbHelper.ParamChar + "paramFixedText "
                      + "WHERE HL7DefFieldNum = " + SOut.Long(hL7DefField.HL7DefFieldNum);
        if (hL7DefField.FixedText == null) hL7DefField.FixedText = "";
        var paramFixedText = new OdSqlParameter("paramFixedText", OdDbType.Text, SOut.StringParam(hL7DefField.FixedText));
        Db.NonQ(command, paramFixedText);
    }

    public static bool Update(HL7DefField hL7DefField, HL7DefField oldHL7DefField)
    {
        var command = "";
        if (hL7DefField.HL7DefSegmentNum != oldHL7DefField.HL7DefSegmentNum)
        {
            if (command != "") command += ",";
            command += "HL7DefSegmentNum = " + SOut.Long(hL7DefField.HL7DefSegmentNum) + "";
        }

        if (hL7DefField.OrdinalPos != oldHL7DefField.OrdinalPos)
        {
            if (command != "") command += ",";
            command += "OrdinalPos = " + SOut.Int(hL7DefField.OrdinalPos) + "";
        }

        if (hL7DefField.TableId != oldHL7DefField.TableId)
        {
            if (command != "") command += ",";
            command += "TableId = '" + SOut.String(hL7DefField.TableId) + "'";
        }

        if (hL7DefField.DataType != oldHL7DefField.DataType)
        {
            if (command != "") command += ",";
            command += "DataType = '" + SOut.String(hL7DefField.DataType.ToString()) + "'";
        }

        if (hL7DefField.FieldName != oldHL7DefField.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(hL7DefField.FieldName) + "'";
        }

        if (hL7DefField.FixedText != oldHL7DefField.FixedText)
        {
            if (command != "") command += ",";
            command += "FixedText = " + DbHelper.ParamChar + "paramFixedText";
        }

        if (command == "") return false;
        if (hL7DefField.FixedText == null) hL7DefField.FixedText = "";
        var paramFixedText = new OdSqlParameter("paramFixedText", OdDbType.Text, SOut.StringParam(hL7DefField.FixedText));
        command = "UPDATE hl7deffield SET " + command
                                            + " WHERE HL7DefFieldNum = " + SOut.Long(hL7DefField.HL7DefFieldNum);
        Db.NonQ(command, paramFixedText);
        return true;
    }

    public static bool UpdateComparison(HL7DefField hL7DefField, HL7DefField oldHL7DefField)
    {
        if (hL7DefField.HL7DefSegmentNum != oldHL7DefField.HL7DefSegmentNum) return true;
        if (hL7DefField.OrdinalPos != oldHL7DefField.OrdinalPos) return true;
        if (hL7DefField.TableId != oldHL7DefField.TableId) return true;
        if (hL7DefField.DataType != oldHL7DefField.DataType) return true;
        if (hL7DefField.FieldName != oldHL7DefField.FieldName) return true;
        if (hL7DefField.FixedText != oldHL7DefField.FixedText) return true;
        return false;
    }

    public static void Delete(long hL7DefFieldNum)
    {
        var command = "DELETE FROM hl7deffield "
                      + "WHERE HL7DefFieldNum = " + SOut.Long(hL7DefFieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHL7DefFieldNums)
    {
        if (listHL7DefFieldNums == null || listHL7DefFieldNums.Count == 0) return;
        var command = "DELETE FROM hl7deffield "
                      + "WHERE HL7DefFieldNum IN(" + string.Join(",", listHL7DefFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}