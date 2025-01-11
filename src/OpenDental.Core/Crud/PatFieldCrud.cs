#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatFieldCrud
{
    public static PatField SelectOne(long patFieldNum)
    {
        var command = "SELECT * FROM patfield "
                      + "WHERE PatFieldNum = " + SOut.Long(patFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatField SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatField> TableToList(DataTable table)
    {
        var retVal = new List<PatField>();
        PatField patField;
        foreach (DataRow row in table.Rows)
        {
            patField = new PatField();
            patField.PatFieldNum = SIn.Long(row["PatFieldNum"].ToString());
            patField.PatNum = SIn.Long(row["PatNum"].ToString());
            patField.FieldName = SIn.String(row["FieldName"].ToString());
            patField.FieldValue = SIn.String(row["FieldValue"].ToString());
            patField.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            patField.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            patField.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(patField);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatField> listPatFields, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatField";
        var table = new DataTable(tableName);
        table.Columns.Add("PatFieldNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("FieldName");
        table.Columns.Add("FieldValue");
        table.Columns.Add("SecUserNumEntry");
        table.Columns.Add("SecDateEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var patField in listPatFields)
            table.Rows.Add(SOut.Long(patField.PatFieldNum), SOut.Long(patField.PatNum), patField.FieldName, patField.FieldValue, SOut.Long(patField.SecUserNumEntry), SOut.DateT(patField.SecDateEntry, false), SOut.DateT(patField.SecDateTEdit, false));
        return table;
    }

    public static long Insert(PatField patField)
    {
        return Insert(patField, false);
    }

    public static long Insert(PatField patField, bool useExistingPK)
    {
        var command = "INSERT INTO patfield (";

        command += "PatNum,FieldName,FieldValue,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            SOut.Long(patField.PatNum) + ","
                                       + "'" + SOut.String(patField.FieldName) + "',"
                                       + DbHelper.ParamChar + "paramFieldValue,"
                                       + SOut.Long(patField.SecUserNumEntry) + ","
                                       + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (patField.FieldValue == null) patField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringNote(patField.FieldValue));
        {
            patField.PatFieldNum = Db.NonQ(command, true, "PatFieldNum", "patField", paramFieldValue);
        }
        return patField.PatFieldNum;
    }

    public static long InsertNoCache(PatField patField)
    {
        return InsertNoCache(patField, false);
    }

    public static long InsertNoCache(PatField patField, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patfield (";
        if (isRandomKeys || useExistingPK) command += "PatFieldNum,";
        command += "PatNum,FieldName,FieldValue,SecUserNumEntry,SecDateEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patField.PatFieldNum) + ",";
        command +=
            SOut.Long(patField.PatNum) + ","
                                       + "'" + SOut.String(patField.FieldName) + "',"
                                       + DbHelper.ParamChar + "paramFieldValue,"
                                       + SOut.Long(patField.SecUserNumEntry) + ","
                                       + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (patField.FieldValue == null) patField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringNote(patField.FieldValue));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramFieldValue);
        else
            patField.PatFieldNum = Db.NonQ(command, true, "PatFieldNum", "patField", paramFieldValue);
        return patField.PatFieldNum;
    }

    public static void Update(PatField patField)
    {
        var command = "UPDATE patfield SET "
                      + "PatNum         =  " + SOut.Long(patField.PatNum) + ", "
                      + "FieldName      = '" + SOut.String(patField.FieldName) + "', "
                      + "FieldValue     =  " + DbHelper.ParamChar + "paramFieldValue "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE PatFieldNum = " + SOut.Long(patField.PatFieldNum);
        if (patField.FieldValue == null) patField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringNote(patField.FieldValue));
        Db.NonQ(command, paramFieldValue);
    }

    public static bool Update(PatField patField, PatField oldPatField)
    {
        var command = "";
        if (patField.PatNum != oldPatField.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(patField.PatNum) + "";
        }

        if (patField.FieldName != oldPatField.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(patField.FieldName) + "'";
        }

        if (patField.FieldValue != oldPatField.FieldValue)
        {
            if (command != "") command += ",";
            command += "FieldValue = " + DbHelper.ParamChar + "paramFieldValue";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        if (patField.FieldValue == null) patField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringNote(patField.FieldValue));
        command = "UPDATE patfield SET " + command
                                         + " WHERE PatFieldNum = " + SOut.Long(patField.PatFieldNum);
        Db.NonQ(command, paramFieldValue);
        return true;
    }

    public static bool UpdateComparison(PatField patField, PatField oldPatField)
    {
        if (patField.PatNum != oldPatField.PatNum) return true;
        if (patField.FieldName != oldPatField.FieldName) return true;
        if (patField.FieldValue != oldPatField.FieldValue) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long patFieldNum)
    {
        var command = "DELETE FROM patfield "
                      + "WHERE PatFieldNum = " + SOut.Long(patFieldNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatFieldNums)
    {
        if (listPatFieldNums == null || listPatFieldNums.Count == 0) return;
        var command = "DELETE FROM patfield "
                      + "WHERE PatFieldNum IN(" + string.Join(",", listPatFieldNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}