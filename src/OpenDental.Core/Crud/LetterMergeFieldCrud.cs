#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LetterMergeFieldCrud
{
    public static LetterMergeField SelectOne(long fieldNum)
    {
        var command = "SELECT * FROM lettermergefield "
                      + "WHERE FieldNum = " + SOut.Long(fieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LetterMergeField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LetterMergeField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LetterMergeField> TableToList(DataTable table)
    {
        var retVal = new List<LetterMergeField>();
        LetterMergeField letterMergeField;
        foreach (DataRow row in table.Rows)
        {
            letterMergeField = new LetterMergeField();
            letterMergeField.FieldNum = SIn.Long(row["FieldNum"].ToString());
            letterMergeField.LetterMergeNum = SIn.Long(row["LetterMergeNum"].ToString());
            letterMergeField.FieldName = SIn.String(row["FieldName"].ToString());
            retVal.Add(letterMergeField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LetterMergeField> listLetterMergeFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LetterMergeField";
        var table = new DataTable(tableName);
        table.Columns.Add("FieldNum");
        table.Columns.Add("LetterMergeNum");
        table.Columns.Add("FieldName");
        foreach (var letterMergeField in listLetterMergeFields)
            table.Rows.Add(SOut.Long(letterMergeField.FieldNum), SOut.Long(letterMergeField.LetterMergeNum), letterMergeField.FieldName);
        return table;
    }

    public static long Insert(LetterMergeField letterMergeField)
    {
        return Insert(letterMergeField, false);
    }

    public static long Insert(LetterMergeField letterMergeField, bool useExistingPK)
    {
        var command = "INSERT INTO lettermergefield (";

        command += "LetterMergeNum,FieldName) VALUES(";

        command +=
            SOut.Long(letterMergeField.LetterMergeNum) + ","
                                                       + "'" + SOut.String(letterMergeField.FieldName) + "')";
        {
            letterMergeField.FieldNum = Db.NonQ(command, true, "FieldNum", "letterMergeField");
        }
        return letterMergeField.FieldNum;
    }

    public static long InsertNoCache(LetterMergeField letterMergeField)
    {
        return InsertNoCache(letterMergeField, false);
    }

    public static long InsertNoCache(LetterMergeField letterMergeField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO lettermergefield (";
        if (isRandomKeys || useExistingPK) command += "FieldNum,";
        command += "LetterMergeNum,FieldName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(letterMergeField.FieldNum) + ",";
        command +=
            SOut.Long(letterMergeField.LetterMergeNum) + ","
                                                       + "'" + SOut.String(letterMergeField.FieldName) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            letterMergeField.FieldNum = Db.NonQ(command, true, "FieldNum", "letterMergeField");
        return letterMergeField.FieldNum;
    }

    public static void Update(LetterMergeField letterMergeField)
    {
        var command = "UPDATE lettermergefield SET "
                      + "LetterMergeNum=  " + SOut.Long(letterMergeField.LetterMergeNum) + ", "
                      + "FieldName     = '" + SOut.String(letterMergeField.FieldName) + "' "
                      + "WHERE FieldNum = " + SOut.Long(letterMergeField.FieldNum);
        Db.NonQ(command);
    }

    public static bool Update(LetterMergeField letterMergeField, LetterMergeField oldLetterMergeField)
    {
        var command = "";
        if (letterMergeField.LetterMergeNum != oldLetterMergeField.LetterMergeNum)
        {
            if (command != "") command += ",";
            command += "LetterMergeNum = " + SOut.Long(letterMergeField.LetterMergeNum) + "";
        }

        if (letterMergeField.FieldName != oldLetterMergeField.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(letterMergeField.FieldName) + "'";
        }

        if (command == "") return false;
        command = "UPDATE lettermergefield SET " + command
                                                 + " WHERE FieldNum = " + SOut.Long(letterMergeField.FieldNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(LetterMergeField letterMergeField, LetterMergeField oldLetterMergeField)
    {
        if (letterMergeField.LetterMergeNum != oldLetterMergeField.LetterMergeNum) return true;
        if (letterMergeField.FieldName != oldLetterMergeField.FieldName) return true;
        return false;
    }

    public static void Delete(long fieldNum)
    {
        var command = "DELETE FROM lettermergefield "
                      + "WHERE FieldNum = " + SOut.Long(fieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listFieldNums)
    {
        if (listFieldNums == null || listFieldNums.Count == 0) return;
        var command = "DELETE FROM lettermergefield "
                      + "WHERE FieldNum IN(" + string.Join(",", listFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}