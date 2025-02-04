using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ReactivationCrud
{
    public static Reactivation SelectOne(long reactivationNum)
    {
        var command = "SELECT * FROM reactivation "
                      + "WHERE ReactivationNum = " + SOut.Long(reactivationNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Reactivation> TableToList(DataTable table)
    {
        var retVal = new List<Reactivation>();
        foreach (DataRow row in table.Rows)
        {
            var reactivation = new Reactivation
            {
                ReactivationNum = SIn.Long(row["ReactivationNum"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                ReactivationStatus = SIn.Long(row["ReactivationStatus"].ToString()),
                ReactivationNote = SIn.String(row["ReactivationNote"].ToString()),
                DoNotContact = SIn.Bool(row["DoNotContact"].ToString())
            };
            retVal.Add(reactivation);
        }

        return retVal;
    }

    public static void Insert(Reactivation reactivation)
    {
        var command = "INSERT INTO reactivation (";

        command += "PatNum,ReactivationStatus,ReactivationNote,DoNotContact) VALUES(";

        command +=
            SOut.Long(reactivation.PatNum) + ","
                                           + SOut.Long(reactivation.ReactivationStatus) + ","
                                           + DbHelper.ParamChar + "paramReactivationNote,"
                                           + SOut.Bool(reactivation.DoNotContact) + ")";
        if (reactivation.ReactivationNote == null) reactivation.ReactivationNote = "";
        var paramReactivationNote = new OdSqlParameter("paramReactivationNote", SOut.StringParam(reactivation.ReactivationNote));
        {
            reactivation.ReactivationNum = Db.NonQ(command, true, "ReactivationNum", "reactivation", paramReactivationNote);
        }
    }

    public static void Update(Reactivation reactivation)
    {
        var command = "UPDATE reactivation SET "
                      + "PatNum            =  " + SOut.Long(reactivation.PatNum) + ", "
                      + "ReactivationStatus=  " + SOut.Long(reactivation.ReactivationStatus) + ", "
                      + "ReactivationNote  =  " + DbHelper.ParamChar + "paramReactivationNote, "
                      + "DoNotContact      =  " + SOut.Bool(reactivation.DoNotContact) + " "
                      + "WHERE ReactivationNum = " + SOut.Long(reactivation.ReactivationNum);
        if (reactivation.ReactivationNote == null) reactivation.ReactivationNote = "";
        var paramReactivationNote = new OdSqlParameter("paramReactivationNote", SOut.StringParam(reactivation.ReactivationNote));
        Db.NonQ(command, paramReactivationNote);
    }

    public static void Delete(long reactivationNum)
    {
        var command = "DELETE FROM reactivation "
                      + "WHERE ReactivationNum = " + SOut.Long(reactivationNum);
        Db.NonQ(command);
    }
}