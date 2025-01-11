#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcGroupItemCrud
{
    public static ProcGroupItem SelectOne(long procGroupItemNum)
    {
        var command = "SELECT * FROM procgroupitem "
                      + "WHERE ProcGroupItemNum = " + SOut.Long(procGroupItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcGroupItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcGroupItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcGroupItem> TableToList(DataTable table)
    {
        var retVal = new List<ProcGroupItem>();
        ProcGroupItem procGroupItem;
        foreach (DataRow row in table.Rows)
        {
            procGroupItem = new ProcGroupItem();
            procGroupItem.ProcGroupItemNum = SIn.Long(row["ProcGroupItemNum"].ToString());
            procGroupItem.ProcNum = SIn.Long(row["ProcNum"].ToString());
            procGroupItem.GroupNum = SIn.Long(row["GroupNum"].ToString());
            retVal.Add(procGroupItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcGroupItem> listProcGroupItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcGroupItem";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcGroupItemNum");
        table.Columns.Add("ProcNum");
        table.Columns.Add("GroupNum");
        foreach (var procGroupItem in listProcGroupItems)
            table.Rows.Add(SOut.Long(procGroupItem.ProcGroupItemNum), SOut.Long(procGroupItem.ProcNum), SOut.Long(procGroupItem.GroupNum));
        return table;
    }

    public static long Insert(ProcGroupItem procGroupItem)
    {
        return Insert(procGroupItem, false);
    }

    public static long Insert(ProcGroupItem procGroupItem, bool useExistingPK)
    {
        var command = "INSERT INTO procgroupitem (";

        command += "ProcNum,GroupNum) VALUES(";

        command +=
            SOut.Long(procGroupItem.ProcNum) + ","
                                             + SOut.Long(procGroupItem.GroupNum) + ")";
        {
            procGroupItem.ProcGroupItemNum = Db.NonQ(command, true, "ProcGroupItemNum", "procGroupItem");
        }
        return procGroupItem.ProcGroupItemNum;
    }

    public static void InsertMany(List<ProcGroupItem> listProcGroupItems)
    {
        InsertMany(listProcGroupItems, false);
    }

    public static void InsertMany(List<ProcGroupItem> listProcGroupItems, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listProcGroupItems.Count)
        {
            var procGroupItem = listProcGroupItems[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO procgroupitem (");
                if (useExistingPK) sbCommands.Append("ProcGroupItemNum,");
                sbCommands.Append("ProcNum,GroupNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(procGroupItem.ProcGroupItemNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(procGroupItem.ProcNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(procGroupItem.GroupNum));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listProcGroupItems.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(ProcGroupItem procGroupItem)
    {
        return InsertNoCache(procGroupItem, false);
    }

    public static long InsertNoCache(ProcGroupItem procGroupItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procgroupitem (";
        if (isRandomKeys || useExistingPK) command += "ProcGroupItemNum,";
        command += "ProcNum,GroupNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procGroupItem.ProcGroupItemNum) + ",";
        command +=
            SOut.Long(procGroupItem.ProcNum) + ","
                                             + SOut.Long(procGroupItem.GroupNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procGroupItem.ProcGroupItemNum = Db.NonQ(command, true, "ProcGroupItemNum", "procGroupItem");
        return procGroupItem.ProcGroupItemNum;
    }

    public static void Update(ProcGroupItem procGroupItem)
    {
        var command = "UPDATE procgroupitem SET "
                      + "ProcNum         =  " + SOut.Long(procGroupItem.ProcNum) + ", "
                      + "GroupNum        =  " + SOut.Long(procGroupItem.GroupNum) + " "
                      + "WHERE ProcGroupItemNum = " + SOut.Long(procGroupItem.ProcGroupItemNum);
        Db.NonQ(command);
    }

    public static bool Update(ProcGroupItem procGroupItem, ProcGroupItem oldProcGroupItem)
    {
        var command = "";
        if (procGroupItem.ProcNum != oldProcGroupItem.ProcNum)
        {
            if (command != "") command += ",";
            command += "ProcNum = " + SOut.Long(procGroupItem.ProcNum) + "";
        }

        if (procGroupItem.GroupNum != oldProcGroupItem.GroupNum)
        {
            if (command != "") command += ",";
            command += "GroupNum = " + SOut.Long(procGroupItem.GroupNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE procgroupitem SET " + command
                                              + " WHERE ProcGroupItemNum = " + SOut.Long(procGroupItem.ProcGroupItemNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProcGroupItem procGroupItem, ProcGroupItem oldProcGroupItem)
    {
        if (procGroupItem.ProcNum != oldProcGroupItem.ProcNum) return true;
        if (procGroupItem.GroupNum != oldProcGroupItem.GroupNum) return true;
        return false;
    }

    public static void Delete(long procGroupItemNum)
    {
        var command = "DELETE FROM procgroupitem "
                      + "WHERE ProcGroupItemNum = " + SOut.Long(procGroupItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcGroupItemNums)
    {
        if (listProcGroupItemNums == null || listProcGroupItemNums.Count == 0) return;
        var command = "DELETE FROM procgroupitem "
                      + "WHERE ProcGroupItemNum IN(" + string.Join(",", listProcGroupItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}