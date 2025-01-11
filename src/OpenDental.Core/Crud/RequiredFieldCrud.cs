#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RequiredFieldCrud
{
    public static RequiredField SelectOne(long requiredFieldNum)
    {
        var command = "SELECT * FROM requiredfield "
                      + "WHERE RequiredFieldNum = " + SOut.Long(requiredFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RequiredField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RequiredField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RequiredField> TableToList(DataTable table)
    {
        var retVal = new List<RequiredField>();
        RequiredField requiredField;
        foreach (DataRow row in table.Rows)
        {
            requiredField = new RequiredField();
            requiredField.RequiredFieldNum = SIn.Long(row["RequiredFieldNum"].ToString());
            requiredField.FieldType = (RequiredFieldType) SIn.Int(row["FieldType"].ToString());
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

    public static long Insert(RequiredField requiredField)
    {
        return Insert(requiredField, false);
    }

    public static long Insert(RequiredField requiredField, bool useExistingPK)
    {
        var command = "INSERT INTO requiredfield (";

        command += "FieldType,FieldName) VALUES(";

        command +=
            SOut.Int((int) requiredField.FieldType) + ","
                                                    + "'" + SOut.String(requiredField.FieldName.ToString()) + "')";
        {
            requiredField.RequiredFieldNum = Db.NonQ(command, true, "RequiredFieldNum", "requiredField");
        }
        return requiredField.RequiredFieldNum;
    }

    public static long InsertNoCache(RequiredField requiredField)
    {
        return InsertNoCache(requiredField, false);
    }

    public static long InsertNoCache(RequiredField requiredField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO requiredfield (";
        if (isRandomKeys || useExistingPK) command += "RequiredFieldNum,";
        command += "FieldType,FieldName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(requiredField.RequiredFieldNum) + ",";
        command +=
            SOut.Int((int) requiredField.FieldType) + ","
                                                    + "'" + SOut.String(requiredField.FieldName.ToString()) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            requiredField.RequiredFieldNum = Db.NonQ(command, true, "RequiredFieldNum", "requiredField");
        return requiredField.RequiredFieldNum;
    }

    public static void Update(RequiredField requiredField)
    {
        var command = "UPDATE requiredfield SET "
                      + "FieldType       =  " + SOut.Int((int) requiredField.FieldType) + ", "
                      + "FieldName       = '" + SOut.String(requiredField.FieldName.ToString()) + "' "
                      + "WHERE RequiredFieldNum = " + SOut.Long(requiredField.RequiredFieldNum);
        Db.NonQ(command);
    }

    public static bool Update(RequiredField requiredField, RequiredField oldRequiredField)
    {
        var command = "";
        if (requiredField.FieldType != oldRequiredField.FieldType)
        {
            if (command != "") command += ",";
            command += "FieldType = " + SOut.Int((int) requiredField.FieldType) + "";
        }

        if (requiredField.FieldName != oldRequiredField.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(requiredField.FieldName.ToString()) + "'";
        }

        if (command == "") return false;
        command = "UPDATE requiredfield SET " + command
                                              + " WHERE RequiredFieldNum = " + SOut.Long(requiredField.RequiredFieldNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(RequiredField requiredField, RequiredField oldRequiredField)
    {
        if (requiredField.FieldType != oldRequiredField.FieldType) return true;
        if (requiredField.FieldName != oldRequiredField.FieldName) return true;
        return false;
    }

    public static void Delete(long requiredFieldNum)
    {
        var command = "DELETE FROM requiredfield "
                      + "WHERE RequiredFieldNum = " + SOut.Long(requiredFieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRequiredFieldNums)
    {
        if (listRequiredFieldNums == null || listRequiredFieldNums.Count == 0) return;
        var command = "DELETE FROM requiredfield "
                      + "WHERE RequiredFieldNum IN(" + string.Join(",", listRequiredFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}