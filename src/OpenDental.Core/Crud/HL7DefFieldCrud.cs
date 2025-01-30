using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HL7DefFieldCrud
{
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

    public static void Insert(HL7DefField hL7DefField)
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
        var paramFixedText = new OdSqlParameter("paramFixedText", SOut.StringParam(hL7DefField.FixedText));
        {
            hL7DefField.HL7DefFieldNum = Db.NonQ(command, true, "HL7DefFieldNum", "hL7DefField", paramFixedText);
        }
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
        var paramFixedText = new OdSqlParameter("paramFixedText", SOut.StringParam(hL7DefField.FixedText));
        Db.NonQ(command, paramFixedText);
    }
}