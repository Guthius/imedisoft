using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class LetterMergeFieldCrud
{
    public static List<LetterMergeField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LetterMergeField> TableToList(DataTable table)
    {
        var retVal = new List<LetterMergeField>();
        foreach (DataRow row in table.Rows)
        {
            var letterMergeField = new LetterMergeField
            {
                FieldNum = SIn.Long(row["FieldNum"].ToString()),
                LetterMergeNum = SIn.Long(row["LetterMergeNum"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString())
            };
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

    public static void Insert(LetterMergeField letterMergeField)
    {
        var command = "INSERT INTO lettermergefield (";

        command += "LetterMergeNum,FieldName) VALUES(";

        command +=
            SOut.Long(letterMergeField.LetterMergeNum) + ","
                                                       + "'" + SOut.String(letterMergeField.FieldName) + "')";
        {
            letterMergeField.FieldNum = Db.NonQ(command, true, "FieldNum", "letterMergeField");
        }
    }
}