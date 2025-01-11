#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcMultiVisitCrud
{
    public static ProcMultiVisit SelectOne(long procMultiVisitNum)
    {
        var command = "SELECT * FROM procmultivisit "
                      + "WHERE ProcMultiVisitNum = " + SOut.Long(procMultiVisitNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcMultiVisit SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcMultiVisit> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcMultiVisit> TableToList(DataTable table)
    {
        var retVal = new List<ProcMultiVisit>();
        ProcMultiVisit procMultiVisit;
        foreach (DataRow row in table.Rows)
        {
            procMultiVisit = new ProcMultiVisit();
            procMultiVisit.ProcMultiVisitNum = SIn.Long(row["ProcMultiVisitNum"].ToString());
            procMultiVisit.GroupProcMultiVisitNum = SIn.Long(row["GroupProcMultiVisitNum"].ToString());
            procMultiVisit.ProcNum = SIn.Long(row["ProcNum"].ToString());
            procMultiVisit.ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString());
            procMultiVisit.IsInProcess = SIn.Bool(row["IsInProcess"].ToString());
            procMultiVisit.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            procMultiVisit.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            procMultiVisit.PatNum = SIn.Long(row["PatNum"].ToString());
            retVal.Add(procMultiVisit);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcMultiVisit> listProcMultiVisits, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcMultiVisit";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcMultiVisitNum");
        table.Columns.Add("GroupProcMultiVisitNum");
        table.Columns.Add("ProcNum");
        table.Columns.Add("ProcStatus");
        table.Columns.Add("IsInProcess");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("PatNum");
        foreach (var procMultiVisit in listProcMultiVisits)
            table.Rows.Add(SOut.Long(procMultiVisit.ProcMultiVisitNum), SOut.Long(procMultiVisit.GroupProcMultiVisitNum), SOut.Long(procMultiVisit.ProcNum), SOut.Int((int) procMultiVisit.ProcStatus), SOut.Bool(procMultiVisit.IsInProcess), SOut.DateT(procMultiVisit.SecDateTEntry, false), SOut.DateT(procMultiVisit.SecDateTEdit, false), SOut.Long(procMultiVisit.PatNum));
        return table;
    }

    public static long Insert(ProcMultiVisit procMultiVisit)
    {
        return Insert(procMultiVisit, false);
    }

    public static long Insert(ProcMultiVisit procMultiVisit, bool useExistingPK)
    {
        var command = "INSERT INTO procmultivisit (";

        command += "GroupProcMultiVisitNum,ProcNum,ProcStatus,IsInProcess,SecDateTEntry,PatNum) VALUES(";

        command +=
            SOut.Long(procMultiVisit.GroupProcMultiVisitNum) + ","
                                                             + SOut.Long(procMultiVisit.ProcNum) + ","
                                                             + SOut.Int((int) procMultiVisit.ProcStatus) + ","
                                                             + SOut.Bool(procMultiVisit.IsInProcess) + ","
                                                             + DbHelper.Now() + ","
                                                             //SecDateTEdit can only be set by MySQL
                                                             + SOut.Long(procMultiVisit.PatNum) + ")";
        {
            procMultiVisit.ProcMultiVisitNum = Db.NonQ(command, true, "ProcMultiVisitNum", "procMultiVisit");
        }
        return procMultiVisit.ProcMultiVisitNum;
    }

    public static long InsertNoCache(ProcMultiVisit procMultiVisit)
    {
        return InsertNoCache(procMultiVisit, false);
    }

    public static long InsertNoCache(ProcMultiVisit procMultiVisit, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procmultivisit (";
        if (isRandomKeys || useExistingPK) command += "ProcMultiVisitNum,";
        command += "GroupProcMultiVisitNum,ProcNum,ProcStatus,IsInProcess,SecDateTEntry,PatNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procMultiVisit.ProcMultiVisitNum) + ",";
        command +=
            SOut.Long(procMultiVisit.GroupProcMultiVisitNum) + ","
                                                             + SOut.Long(procMultiVisit.ProcNum) + ","
                                                             + SOut.Int((int) procMultiVisit.ProcStatus) + ","
                                                             + SOut.Bool(procMultiVisit.IsInProcess) + ","
                                                             + DbHelper.Now() + ","
                                                             //SecDateTEdit can only be set by MySQL
                                                             + SOut.Long(procMultiVisit.PatNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procMultiVisit.ProcMultiVisitNum = Db.NonQ(command, true, "ProcMultiVisitNum", "procMultiVisit");
        return procMultiVisit.ProcMultiVisitNum;
    }

    public static void Update(ProcMultiVisit procMultiVisit)
    {
        var command = "UPDATE procmultivisit SET "
                      + "GroupProcMultiVisitNum=  " + SOut.Long(procMultiVisit.GroupProcMultiVisitNum) + ", "
                      + "ProcNum               =  " + SOut.Long(procMultiVisit.ProcNum) + ", "
                      + "ProcStatus            =  " + SOut.Int((int) procMultiVisit.ProcStatus) + ", "
                      + "IsInProcess           =  " + SOut.Bool(procMultiVisit.IsInProcess) + ", "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "PatNum                =  " + SOut.Long(procMultiVisit.PatNum) + " "
                      + "WHERE ProcMultiVisitNum = " + SOut.Long(procMultiVisit.ProcMultiVisitNum);
        Db.NonQ(command);
    }

    public static bool Update(ProcMultiVisit procMultiVisit, ProcMultiVisit oldProcMultiVisit)
    {
        var command = "";
        if (procMultiVisit.GroupProcMultiVisitNum != oldProcMultiVisit.GroupProcMultiVisitNum)
        {
            if (command != "") command += ",";
            command += "GroupProcMultiVisitNum = " + SOut.Long(procMultiVisit.GroupProcMultiVisitNum) + "";
        }

        if (procMultiVisit.ProcNum != oldProcMultiVisit.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(procMultiVisit.ProcNum) + "";
        }

        if (procMultiVisit.ProcStatus != oldProcMultiVisit.ProcStatus)
        {
            if (command != "") command += ",";
            command += "ProcStatus = " + SOut.Int((int) procMultiVisit.ProcStatus) + "";
        }

        if (procMultiVisit.IsInProcess != oldProcMultiVisit.IsInProcess)
        {
            if (command != "") command += ",";
            command += "IsInProcess = " + SOut.Bool(procMultiVisit.IsInProcess) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (procMultiVisit.PatNum != oldProcMultiVisit.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(procMultiVisit.PatNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE procmultivisit SET " + command
                                               + " WHERE ProcMultiVisitNum = " + SOut.Long(procMultiVisit.ProcMultiVisitNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProcMultiVisit procMultiVisit, ProcMultiVisit oldProcMultiVisit)
    {
        if (procMultiVisit.GroupProcMultiVisitNum != oldProcMultiVisit.GroupProcMultiVisitNum) return true;
        if (procMultiVisit.ProcNum != oldProcMultiVisit.ProcNum) return true;
        if (procMultiVisit.ProcStatus != oldProcMultiVisit.ProcStatus) return true;
        if (procMultiVisit.IsInProcess != oldProcMultiVisit.IsInProcess) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (procMultiVisit.PatNum != oldProcMultiVisit.PatNum) return true;
        return false;
    }

    public static void Delete(long procMultiVisitNum)
    {
        var command = "DELETE FROM procmultivisit "
                      + "WHERE ProcMultiVisitNum = " + SOut.Long(procMultiVisitNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcMultiVisitNums)
    {
        if (listProcMultiVisitNums == null || listProcMultiVisitNums.Count == 0) return;
        var command = "DELETE FROM procmultivisit "
                      + "WHERE ProcMultiVisitNum IN(" + string.Join(",", listProcMultiVisitNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}