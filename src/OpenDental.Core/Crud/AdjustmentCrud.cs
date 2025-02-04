using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var adjustment = new Adjustment
            {
                AdjNum = SIn.Long(row["AdjNum"].ToString()),
                AdjDate = SIn.Date(row["AdjDate"].ToString()),
                AdjAmt = SIn.Double(row["AdjAmt"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                AdjType = SIn.Long(row["AdjType"].ToString()),
                ProvNum = SIn.Long(row["ProvNum"].ToString()),
                AdjNote = SIn.String(row["AdjNote"].ToString()),
                ProcDate = SIn.Date(row["ProcDate"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                DateEntry = SIn.Date(row["DateEntry"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                StatementNum = SIn.Long(row["StatementNum"].ToString()),
                SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                TaxTransID = SIn.Long(row["TaxTransID"].ToString())
            };
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
                                          + "NOW()" + ","
                                          + SOut.Long(adjustment.ClinicNum) + ","
                                          + SOut.Long(adjustment.StatementNum) + ","
                                          + SOut.Long(adjustment.SecUserNumEntry) + ","
                                          //SecDateTEdit can only be set by MySQL
                                          + SOut.Long(adjustment.TaxTransID) + ")";
        if (adjustment.AdjNote == null) adjustment.AdjNote = "";

        var paramAdjNote = new OdSqlParameter("paramAdjNote", SOut.StringNote(adjustment.AdjNote));
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

        var paramAdjNote = new OdSqlParameter("paramAdjNote", SOut.StringNote(adjustment.AdjNote));
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