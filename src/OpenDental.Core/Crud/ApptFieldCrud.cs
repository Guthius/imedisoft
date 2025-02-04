using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ApptFieldCrud
{
    public static ApptField SelectOne(long apptFieldNum)
    {
        var command = "SELECT * FROM apptfield "
                      + "WHERE ApptFieldNum = " + SOut.Long(apptFieldNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ApptField> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptField> TableToList(DataTable table)
    {
        var retVal = new List<ApptField>();
        foreach (DataRow row in table.Rows)
        {
            var apptField = new ApptField
            {
                AptNum = SIn.Long(row["AptNum"].ToString()),
                FieldName = SIn.String(row["FieldName"].ToString()),
                FieldValue = SIn.String(row["FieldValue"].ToString())
            };
            retVal.Add(apptField);
        }

        return retVal;
    }

    public static void Insert(ApptField apptField)
    {
        var command = "INSERT INTO apptfield (";

        command += "AptNum,FieldName,FieldValue) VALUES(";

        command +=
            SOut.Long(apptField.AptNum) + ","
                                        + "'" + SOut.String(apptField.FieldName) + "',"
                                        + DbHelper.ParamChar + "paramFieldValue)";
        if (apptField.FieldValue == null) apptField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", SOut.StringParam(apptField.FieldValue));
        {
            Db.NonQ(command, true, "ApptFieldNum", "apptField", paramFieldValue);
        }
    }
}