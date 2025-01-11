#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SmsBlockPhoneCrud
{
    public static SmsBlockPhone SelectOne(long smsBlockPhoneNum)
    {
        var command = "SELECT * FROM smsblockphone "
                      + "WHERE SmsBlockPhoneNum = " + SOut.Long(smsBlockPhoneNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SmsBlockPhone SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SmsBlockPhone> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsBlockPhone> TableToList(DataTable table)
    {
        var retVal = new List<SmsBlockPhone>();
        SmsBlockPhone smsBlockPhone;
        foreach (DataRow row in table.Rows)
        {
            smsBlockPhone = new SmsBlockPhone();
            smsBlockPhone.SmsBlockPhoneNum = SIn.Long(row["SmsBlockPhoneNum"].ToString());
            smsBlockPhone.BlockWirelessNumber = SIn.String(row["BlockWirelessNumber"].ToString());
            retVal.Add(smsBlockPhone);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SmsBlockPhone> listSmsBlockPhones, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SmsBlockPhone";
        var table = new DataTable(tableName);
        table.Columns.Add("SmsBlockPhoneNum");
        table.Columns.Add("BlockWirelessNumber");
        foreach (var smsBlockPhone in listSmsBlockPhones)
            table.Rows.Add(SOut.Long(smsBlockPhone.SmsBlockPhoneNum), smsBlockPhone.BlockWirelessNumber);
        return table;
    }

    public static long Insert(SmsBlockPhone smsBlockPhone)
    {
        return Insert(smsBlockPhone, false);
    }

    public static long Insert(SmsBlockPhone smsBlockPhone, bool useExistingPK)
    {
        var command = "INSERT INTO smsblockphone (";

        command += "BlockWirelessNumber) VALUES(";

        command +=
            "'" + SOut.String(smsBlockPhone.BlockWirelessNumber) + "')";
        {
            smsBlockPhone.SmsBlockPhoneNum = Db.NonQ(command, true, "SmsBlockPhoneNum", "smsBlockPhone");
        }
        return smsBlockPhone.SmsBlockPhoneNum;
    }

    public static long InsertNoCache(SmsBlockPhone smsBlockPhone)
    {
        return InsertNoCache(smsBlockPhone, false);
    }

    public static long InsertNoCache(SmsBlockPhone smsBlockPhone, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO smsblockphone (";
        if (isRandomKeys || useExistingPK) command += "SmsBlockPhoneNum,";
        command += "BlockWirelessNumber) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(smsBlockPhone.SmsBlockPhoneNum) + ",";
        command +=
            "'" + SOut.String(smsBlockPhone.BlockWirelessNumber) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            smsBlockPhone.SmsBlockPhoneNum = Db.NonQ(command, true, "SmsBlockPhoneNum", "smsBlockPhone");
        return smsBlockPhone.SmsBlockPhoneNum;
    }

    public static void Update(SmsBlockPhone smsBlockPhone)
    {
        var command = "UPDATE smsblockphone SET "
                      + "BlockWirelessNumber= '" + SOut.String(smsBlockPhone.BlockWirelessNumber) + "' "
                      + "WHERE SmsBlockPhoneNum = " + SOut.Long(smsBlockPhone.SmsBlockPhoneNum);
        Db.NonQ(command);
    }

    public static bool Update(SmsBlockPhone smsBlockPhone, SmsBlockPhone oldSmsBlockPhone)
    {
        var command = "";
        if (smsBlockPhone.BlockWirelessNumber != oldSmsBlockPhone.BlockWirelessNumber)
        {
            if (command != "") command += ",";
            command += "BlockWirelessNumber = '" + SOut.String(smsBlockPhone.BlockWirelessNumber) + "'";
        }

        if (command == "") return false;
        command = "UPDATE smsblockphone SET " + command
                                              + " WHERE SmsBlockPhoneNum = " + SOut.Long(smsBlockPhone.SmsBlockPhoneNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SmsBlockPhone smsBlockPhone, SmsBlockPhone oldSmsBlockPhone)
    {
        if (smsBlockPhone.BlockWirelessNumber != oldSmsBlockPhone.BlockWirelessNumber) return true;
        return false;
    }

    public static void Delete(long smsBlockPhoneNum)
    {
        var command = "DELETE FROM smsblockphone "
                      + "WHERE SmsBlockPhoneNum = " + SOut.Long(smsBlockPhoneNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSmsBlockPhoneNums)
    {
        if (listSmsBlockPhoneNums == null || listSmsBlockPhoneNums.Count == 0) return;
        var command = "DELETE FROM smsblockphone "
                      + "WHERE SmsBlockPhoneNum IN(" + string.Join(",", listSmsBlockPhoneNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}