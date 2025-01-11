using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CvxCrud
{
    public static Cvx SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Cvx> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Cvx> TableToList(DataTable table)
    {
        var retVal = new List<Cvx>();
        Cvx cvx;
        foreach (DataRow row in table.Rows)
        {
            cvx = new Cvx();
            cvx.CvxNum = SIn.Long(row["CvxNum"].ToString());
            cvx.CvxCode = SIn.String(row["CvxCode"].ToString());
            cvx.Description = SIn.String(row["Description"].ToString());
            cvx.IsActive = SIn.String(row["IsActive"].ToString());
            retVal.Add(cvx);
        }

        return retVal;
    }

    public static long Insert(Cvx cvx)
    {
        var command = "INSERT INTO cvx (";

        command += "CvxCode,Description,IsActive) VALUES(";

        command +=
            "'" + SOut.String(cvx.CvxCode) + "',"
            + "'" + SOut.String(cvx.Description) + "',"
            + "'" + SOut.String(cvx.IsActive) + "')";
        {
            cvx.CvxNum = Db.NonQ(command, true, "CvxNum", "cvx");
        }
        return cvx.CvxNum;
    }

    public static void Update(Cvx cvx)
    {
        var command = "UPDATE cvx SET "
                      + "CvxCode    = '" + SOut.String(cvx.CvxCode) + "', "
                      + "Description= '" + SOut.String(cvx.Description) + "', "
                      + "IsActive   = '" + SOut.String(cvx.IsActive) + "' "
                      + "WHERE CvxNum = " + SOut.Long(cvx.CvxNum);
        Db.NonQ(command);
    }
}