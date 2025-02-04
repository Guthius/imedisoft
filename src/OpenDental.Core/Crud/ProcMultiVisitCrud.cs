using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcMultiVisitCrud
{
    public static List<ProcMultiVisit> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcMultiVisit> TableToList(DataTable table)
    {
        var retVal = new List<ProcMultiVisit>();
        foreach (DataRow row in table.Rows)
        {
            var procMultiVisit = new ProcMultiVisit
            {
                ProcMultiVisitNum = SIn.Long(row["ProcMultiVisitNum"].ToString()),
                GroupProcMultiVisitNum = SIn.Long(row["GroupProcMultiVisitNum"].ToString()),
                ProcNum = SIn.Long(row["ProcNum"].ToString()),
                ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString()),
                IsInProcess = SIn.Bool(row["IsInProcess"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString())
            };
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
            table.Rows.Add(SOut.Long(procMultiVisit.ProcMultiVisitNum), SOut.Long(procMultiVisit.GroupProcMultiVisitNum), SOut.Long(procMultiVisit.ProcNum), SOut.Int((int) procMultiVisit.ProcStatus), SOut.Bool(procMultiVisit.IsInProcess), SOut.DateTime(procMultiVisit.SecDateTEntry, false), SOut.DateTime(procMultiVisit.SecDateTEdit, false), SOut.Long(procMultiVisit.PatNum));
        return table;
    }

    public static long Insert(ProcMultiVisit procMultiVisit)
    {
        var command = "INSERT INTO procmultivisit (";

        command += "GroupProcMultiVisitNum,ProcNum,ProcStatus,IsInProcess,SecDateTEntry,PatNum) VALUES(";

        command +=
            SOut.Long(procMultiVisit.GroupProcMultiVisitNum) + ","
                                                             + SOut.Long(procMultiVisit.ProcNum) + ","
                                                             + SOut.Int((int) procMultiVisit.ProcStatus) + ","
                                                             + SOut.Bool(procMultiVisit.IsInProcess) + ","
                                                             + "NOW()" + ","
                                                             //SecDateTEdit can only be set by MySQL
                                                             + SOut.Long(procMultiVisit.PatNum) + ")";
        {
            procMultiVisit.ProcMultiVisitNum = Db.NonQ(command, true, "ProcMultiVisitNum", "procMultiVisit");
        }
        return procMultiVisit.ProcMultiVisitNum;
    }

    public static void Update(ProcMultiVisit procMultiVisit, ProcMultiVisit oldProcMultiVisit)
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

        if (command == "") return;
        command = "UPDATE procmultivisit SET " + command
                                               + " WHERE ProcMultiVisitNum = " + SOut.Long(procMultiVisit.ProcMultiVisitNum);
        Db.NonQ(command);
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
}