using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RequiredFieldCrud
{
    public static List<RequiredField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RequiredField> TableToList(DataTable table)
    {
        var retVal = new List<RequiredField>();
        foreach (DataRow row in table.Rows)
        {
            var requiredField = new RequiredField
            {
                RequiredFieldNum = SIn.Long(row["RequiredFieldNum"].ToString()),
                FieldType = (RequiredFieldType) SIn.Int(row["FieldType"].ToString())
            };
            var fieldName = row["FieldName"].ToString();
            if (fieldName == "")
                requiredField.FieldName = 0;
            else
                try
                {
                    requiredField.FieldName = (RequiredFieldName) Enum.Parse(typeof(RequiredFieldName), fieldName);
                }
                catch
                {
                    requiredField.FieldName = 0;
                }

            retVal.Add(requiredField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RequiredField> listRequiredFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RequiredField";
        var table = new DataTable(tableName);
        table.Columns.Add("RequiredFieldNum");
        table.Columns.Add("FieldType");
        table.Columns.Add("FieldName");
        foreach (var requiredField in listRequiredFields)
            table.Rows.Add(SOut.Long(requiredField.RequiredFieldNum), SOut.Int((int) requiredField.FieldType), SOut.Int((int) requiredField.FieldName));
        return table;
    }

    public static void Insert(RequiredField requiredField)
    {
        var command = "INSERT INTO requiredfield (";

        command += "FieldType,FieldName) VALUES(";

        command +=
            SOut.Int((int) requiredField.FieldType) + ","
                                                    + "'" + SOut.String(requiredField.FieldName.ToString()) + "')";
        {
            requiredField.RequiredFieldNum = Db.NonQ(command, true, "RequiredFieldNum", "requiredField");
        }
    }

    public static void Delete(long requiredFieldNum)
    {
        var command = "DELETE FROM requiredfield "
                      + "WHERE RequiredFieldNum = " + SOut.Long(requiredFieldNum);
        Db.NonQ(command);
    }
}