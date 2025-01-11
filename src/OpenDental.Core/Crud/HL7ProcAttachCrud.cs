#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HL7ProcAttachCrud
{
    public static HL7ProcAttach SelectOne(long hL7ProcAttachNum)
    {
        var command = "SELECT * FROM hl7procattach "
                      + "WHERE HL7ProcAttachNum = " + SOut.Long(hL7ProcAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HL7ProcAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HL7ProcAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HL7ProcAttach> TableToList(DataTable table)
    {
        var retVal = new List<HL7ProcAttach>();
        HL7ProcAttach hL7ProcAttach;
        foreach (DataRow row in table.Rows)
        {
            hL7ProcAttach = new HL7ProcAttach();
            hL7ProcAttach.HL7ProcAttachNum = SIn.Long(row["HL7ProcAttachNum"].ToString());
            hL7ProcAttach.HL7MsgNum = SIn.Long(row["HL7MsgNum"].ToString());
            hL7ProcAttach.ProcNum = SIn.Long(row["ProcNum"].ToString());
            retVal.Add(hL7ProcAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HL7ProcAttach> listHL7ProcAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HL7ProcAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("HL7ProcAttachNum");
        table.Columns.Add("HL7MsgNum");
        table.Columns.Add("ProcNum");
        foreach (var hL7ProcAttach in listHL7ProcAttachs)
            table.Rows.Add(SOut.Long(hL7ProcAttach.HL7ProcAttachNum), SOut.Long(hL7ProcAttach.HL7MsgNum), SOut.Long(hL7ProcAttach.ProcNum));
        return table;
    }

    public static long Insert(HL7ProcAttach hL7ProcAttach)
    {
        return Insert(hL7ProcAttach, false);
    }

    public static long Insert(HL7ProcAttach hL7ProcAttach, bool useExistingPK)
    {
        var command = "INSERT INTO hl7procattach (";

        command += "HL7MsgNum,ProcNum) VALUES(";

        command +=
            SOut.Long(hL7ProcAttach.HL7MsgNum) + ","
                                               + SOut.Long(hL7ProcAttach.ProcNum) + ")";
        {
            hL7ProcAttach.HL7ProcAttachNum = Db.NonQ(command, true, "HL7ProcAttachNum", "hL7ProcAttach");
        }
        return hL7ProcAttach.HL7ProcAttachNum;
    }

    public static long InsertNoCache(HL7ProcAttach hL7ProcAttach)
    {
        return InsertNoCache(hL7ProcAttach, false);
    }

    public static long InsertNoCache(HL7ProcAttach hL7ProcAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hl7procattach (";
        if (isRandomKeys || useExistingPK) command += "HL7ProcAttachNum,";
        command += "HL7MsgNum,ProcNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hL7ProcAttach.HL7ProcAttachNum) + ",";
        command +=
            SOut.Long(hL7ProcAttach.HL7MsgNum) + ","
                                               + SOut.Long(hL7ProcAttach.ProcNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            hL7ProcAttach.HL7ProcAttachNum = Db.NonQ(command, true, "HL7ProcAttachNum", "hL7ProcAttach");
        return hL7ProcAttach.HL7ProcAttachNum;
    }

    public static void Update(HL7ProcAttach hL7ProcAttach)
    {
        var command = "UPDATE hl7procattach SET "
                      + "HL7MsgNum       =  " + SOut.Long(hL7ProcAttach.HL7MsgNum) + ", "
                      + "ProcNum         =  " + SOut.Long(hL7ProcAttach.ProcNum) + " "
                      + "WHERE HL7ProcAttachNum = " + SOut.Long(hL7ProcAttach.HL7ProcAttachNum);
        Db.NonQ(command);
    }

    public static bool Update(HL7ProcAttach hL7ProcAttach, HL7ProcAttach oldHL7ProcAttach)
    {
        var command = "";
        if (hL7ProcAttach.HL7MsgNum != oldHL7ProcAttach.HL7MsgNum)
        {
            if (command != "") command += ",";
            command += "HL7MsgNum = " + SOut.Long(hL7ProcAttach.HL7MsgNum) + "";
        }

        if (hL7ProcAttach.ProcNum != oldHL7ProcAttach.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(hL7ProcAttach.ProcNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE hl7procattach SET " + command
                                              + " WHERE HL7ProcAttachNum = " + SOut.Long(hL7ProcAttach.HL7ProcAttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(HL7ProcAttach hL7ProcAttach, HL7ProcAttach oldHL7ProcAttach)
    {
        if (hL7ProcAttach.HL7MsgNum != oldHL7ProcAttach.HL7MsgNum) return true;
        if (hL7ProcAttach.ProcNum != oldHL7ProcAttach.ProcNum) return true;
        return false;
    }

    public static void Delete(long hL7ProcAttachNum)
    {
        var command = "DELETE FROM hl7procattach "
                      + "WHERE HL7ProcAttachNum = " + SOut.Long(hL7ProcAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHL7ProcAttachNums)
    {
        if (listHL7ProcAttachNums == null || listHL7ProcAttachNums.Count == 0) return;
        var command = "DELETE FROM hl7procattach "
                      + "WHERE HL7ProcAttachNum IN(" + string.Join(",", listHL7ProcAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}