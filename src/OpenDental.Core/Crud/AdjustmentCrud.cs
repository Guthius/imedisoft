using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AdjustmentCrud
{
    public static Adjustment SelectOne(long adjNum)
    {
        var command = "SELECT * FROM adjustment "
                      + "WHERE AdjNum = " + SOut.Long(adjNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;

        return list[0];
    }

    public static List<Adjustment> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Adjustment> TableToList(DataTable table)
    {
        var retVal = new List<Adjustment>();
        Adjustment adjustment;
        foreach (DataRow row in table.Rows)
        {
            adjustment = new Adjustment();
            adjustment.AdjNum = SIn.Long(row["AdjNum"].ToString());
            adjustment.AdjDate = SIn.Date(row["AdjDate"].ToString());
            adjustment.AdjAmt = SIn.Double(row["AdjAmt"].ToString());
            adjustment.PatNum = SIn.Long(row["PatNum"].ToString());
            adjustment.AdjType = SIn.Long(row["AdjType"].ToString());
            adjustment.ProvNum = SIn.Long(row["ProvNum"].ToString());
            adjustment.AdjNote = SIn.String(row["AdjNote"].ToString());
            adjustment.ProcDate = SIn.Date(row["ProcDate"].ToString());
            adjustment.ProcNum = SIn.Long(row["ProcNum"].ToString());
            adjustment.DateEntry = SIn.Date(row["DateEntry"].ToString());
            adjustment.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            adjustment.StatementNum = SIn.Long(row["StatementNum"].ToString());
            adjustment.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            adjustment.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            adjustment.TaxTransID = SIn.Long(row["TaxTransID"].ToString());
            retVal.Add(adjustment);
        }

        return retVal;
    }

    public static long Insert(Adjustment adjustment)
    {
        var command = "INSERT INTO adjustment (";

        command += "AdjDate,AdjAmt,PatNum,AdjType,ProvNum,AdjNote,ProcDate,ProcNum,DateEntry,ClinicNum,StatementNum,SecUserNumEntry,TaxTransID) VALUES(";

        command +=
            SOut.Date(adjustment.AdjDate) + ","
                                          + SOut.Double(adjustment.AdjAmt) + ","
                                          + SOut.Long(adjustment.PatNum) + ","
                                          + SOut.Long(adjustment.AdjType) + ","
                                          + SOut.Long(adjustment.ProvNum) + ","
                                          + DbHelper.ParamChar + "paramAdjNote,"
                                          + SOut.Date(adjustment.ProcDate) + ","
                                          + SOut.Long(adjustment.ProcNum) + ","
                                          + DbHelper.Now() + ","
                                          + SOut.Long(adjustment.ClinicNum) + ","
                                          + SOut.Long(adjustment.StatementNum) + ","
                                          + SOut.Long(adjustment.SecUserNumEntry) + ","
                                          //SecDateTEdit can only be set by MySQL
                                          + SOut.Long(adjustment.TaxTransID) + ")";
        if (adjustment.AdjNote == null) adjustment.AdjNote = "";

        var paramAdjNote = new OdSqlParameter("paramAdjNote", OdDbType.Text, SOut.StringNote(adjustment.AdjNote));
        {
            adjustment.AdjNum = Db.NonQ(command, true, "AdjNum", "adjustment", paramAdjNote);
        }
        return adjustment.AdjNum;
    }

    public static void Update(Adjustment adjustment)
    {
        var command = "UPDATE adjustment SET "
                      + "AdjDate        =  " + SOut.Date(adjustment.AdjDate) + ", "
                      + "AdjAmt         =  " + SOut.Double(adjustment.AdjAmt) + ", "
                      + "PatNum         =  " + SOut.Long(adjustment.PatNum) + ", "
                      + "AdjType        =  " + SOut.Long(adjustment.AdjType) + ", "
                      + "ProvNum        =  " + SOut.Long(adjustment.ProvNum) + ", "
                      + "AdjNote        =  " + DbHelper.ParamChar + "paramAdjNote, "
                      + "ProcDate       =  " + SOut.Date(adjustment.ProcDate) + ", "
                      + "ProcNum        =  " + SOut.Long(adjustment.ProcNum) + ", "
                      //DateEntry not allowed to change
                      + "ClinicNum      =  " + SOut.Long(adjustment.ClinicNum) + ", "
                      + "StatementNum   =  " + SOut.Long(adjustment.StatementNum) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateTEdit can only be set by MySQL
                      + "TaxTransID     =  " + SOut.Long(adjustment.TaxTransID) + " "
                      + "WHERE AdjNum = " + SOut.Long(adjustment.AdjNum);
        if (adjustment.AdjNote == null) adjustment.AdjNote = "";

        var paramAdjNote = new OdSqlParameter("paramAdjNote", OdDbType.Text, SOut.StringNote(adjustment.AdjNote));
        Db.NonQ(command, paramAdjNote);
    }

    public static void Delete(long adjNum)
    {
        var command = "DELETE FROM adjustment "
                      + "WHERE AdjNum = " + SOut.Long(adjNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listAdjNums)
    {
        if (listAdjNums == null || listAdjNums.Count == 0) return;

        var command = "DELETE FROM adjustment "
                      + "WHERE AdjNum IN(" + string.Join(",", listAdjNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}