using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        ApptField apptField;
        foreach (DataRow row in table.Rows)
        {
            apptField = new ApptField();
            apptField.ApptFieldNum = SIn.Long(row["ApptFieldNum"].ToString());
            apptField.AptNum = SIn.Long(row["AptNum"].ToString());
            apptField.FieldName = SIn.String(row["FieldName"].ToString());
            apptField.FieldValue = SIn.String(row["FieldValue"].ToString());
            retVal.Add(apptField);
        }

        return retVal;
    }

    public static long Insert(ApptField apptField)
    {
        var command = "INSERT INTO apptfield (";

        command += "AptNum,FieldName,FieldValue) VALUES(";

        command +=
            SOut.Long(apptField.AptNum) + ","
                                        + "'" + SOut.String(apptField.FieldName) + "',"
                                        + DbHelper.ParamChar + "paramFieldValue)";
        if (apptField.FieldValue == null) apptField.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(apptField.FieldValue));
        {
            apptField.ApptFieldNum = Db.NonQ(command, true, "ApptFieldNum", "apptField", paramFieldValue);
        }
        return apptField.ApptFieldNum;
    }
}