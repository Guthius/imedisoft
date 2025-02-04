using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<PatField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatField> TableToList(DataTable table)
    {
        var retVal = new List<PatField>();
        foreach (DataRow row in table.Rows)
        {
            var patField = new PatField
            {
                PatFieldNum = SIn.Long(row["PatFieldNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString()),
                FieldValue = SIn.String(row["FieldValue"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateEntry = SIn.Date(row["SecDateEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString())
            };
            retVal.Add(patField);
        }

        return retVal;
    }

    public static void Insert(PatField patField)
    {
        var command = "INSERT INTO patfield (";

        command += "PatNum,FieldName,FieldValue,SecUserNumEntry,SecDateEntry) VALUES(";

        command +=
            SOut.Long(patField.PatNum) + ","
                                       + "'" + SOut.String(patField.FieldName) + "',"
                                       + DbHelper.ParamChar + "paramFieldValue,"
                                       + SOut.Long(patField.SecUserNumEntry) + ","
                                       + "NOW()" + ")";
        //SecDateTEdit can only be set by MySQL
        if (patField.FieldValue == null) patField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", SOut.StringNote(patField.FieldValue));
        {
            patField.PatFieldNum = Db.NonQ(command, true, "PatFieldNum", "patField", paramFieldValue);
        }
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
        var paramFieldValue = new OdSqlParameter("paramFieldValue", SOut.StringNote(patField.FieldValue));
        Db.NonQ(command, paramFieldValue);
    }
}