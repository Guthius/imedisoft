#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PhoneNumberCrud
{
    public static PhoneNumber SelectOne(long phoneNumberNum)
    {
        var command = "SELECT * FROM phonenumber "
                      + "WHERE PhoneNumberNum = " + SOut.Long(phoneNumberNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PhoneNumber SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PhoneNumber> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PhoneNumber> TableToList(DataTable table)
    {
        var retVal = new List<PhoneNumber>();
        PhoneNumber phoneNumber;
        foreach (DataRow row in table.Rows)
        {
            phoneNumber = new PhoneNumber();
            phoneNumber.PhoneNumberNum = SIn.Long(row["PhoneNumberNum"].ToString());
            phoneNumber.PatNum = SIn.Long(row["PatNum"].ToString());
            phoneNumber.PhoneNumberVal = SIn.String(row["PhoneNumberVal"].ToString());
            phoneNumber.PhoneNumberDigits = SIn.String(row["PhoneNumberDigits"].ToString());
            phoneNumber.PhoneType = (PhoneType) SIn.Int(row["PhoneType"].ToString());
            retVal.Add(phoneNumber);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PhoneNumber> listPhoneNumbers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PhoneNumber";
        var table = new DataTable(tableName);
        table.Columns.Add("PhoneNumberNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("PhoneNumberVal");
        table.Columns.Add("PhoneNumberDigits");
        table.Columns.Add("PhoneType");
        foreach (var phoneNumber in listPhoneNumbers)
            table.Rows.Add(SOut.Long(phoneNumber.PhoneNumberNum), SOut.Long(phoneNumber.PatNum), phoneNumber.PhoneNumberVal, phoneNumber.PhoneNumberDigits, SOut.Int((int) phoneNumber.PhoneType));
        return table;
    }

    public static long Insert(PhoneNumber phoneNumber)
    {
        return Insert(phoneNumber, false);
    }

    public static long Insert(PhoneNumber phoneNumber, bool useExistingPK)
    {
        var command = "INSERT INTO phonenumber (";

        command += "PatNum,PhoneNumberVal,PhoneNumberDigits,PhoneType) VALUES(";

        command +=
            SOut.Long(phoneNumber.PatNum) + ","
                                          + "'" + SOut.String(phoneNumber.PhoneNumberVal) + "',"
                                          + "'" + SOut.String(phoneNumber.PhoneNumberDigits) + "',"
                                          + SOut.Int((int) phoneNumber.PhoneType) + ")";
        {
            phoneNumber.PhoneNumberNum = Db.NonQ(command, true, "PhoneNumberNum", "phoneNumber");
        }
        return phoneNumber.PhoneNumberNum;
    }

    public static void InsertMany(List<PhoneNumber> listPhoneNumbers)
    {
        InsertMany(listPhoneNumbers, false);
    }

    public static void InsertMany(List<PhoneNumber> listPhoneNumbers, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listPhoneNumbers.Count)
        {
            var phoneNumber = listPhoneNumbers[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO phonenumber (");
                if (useExistingPK) sbCommands.Append("PhoneNumberNum,");
                sbCommands.Append("PatNum,PhoneNumberVal,PhoneNumberDigits,PhoneType) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(phoneNumber.PhoneNumberNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(phoneNumber.PatNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(phoneNumber.PhoneNumberVal) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(phoneNumber.PhoneNumberDigits) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) phoneNumber.PhoneType));
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
                if (index == listPhoneNumbers.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(PhoneNumber phoneNumber)
    {
        return InsertNoCache(phoneNumber, false);
    }

    public static long InsertNoCache(PhoneNumber phoneNumber, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO phonenumber (";
        if (isRandomKeys || useExistingPK) command += "PhoneNumberNum,";
        command += "PatNum,PhoneNumberVal,PhoneNumberDigits,PhoneType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(phoneNumber.PhoneNumberNum) + ",";
        command +=
            SOut.Long(phoneNumber.PatNum) + ","
                                          + "'" + SOut.String(phoneNumber.PhoneNumberVal) + "',"
                                          + "'" + SOut.String(phoneNumber.PhoneNumberDigits) + "',"
                                          + SOut.Int((int) phoneNumber.PhoneType) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            phoneNumber.PhoneNumberNum = Db.NonQ(command, true, "PhoneNumberNum", "phoneNumber");
        return phoneNumber.PhoneNumberNum;
    }

    public static void Update(PhoneNumber phoneNumber)
    {
        var command = "UPDATE phonenumber SET "
                      + "PatNum           =  " + SOut.Long(phoneNumber.PatNum) + ", "
                      + "PhoneNumberVal   = '" + SOut.String(phoneNumber.PhoneNumberVal) + "', "
                      + "PhoneNumberDigits= '" + SOut.String(phoneNumber.PhoneNumberDigits) + "', "
                      + "PhoneType        =  " + SOut.Int((int) phoneNumber.PhoneType) + " "
                      + "WHERE PhoneNumberNum = " + SOut.Long(phoneNumber.PhoneNumberNum);
        Db.NonQ(command);
    }

    public static bool Update(PhoneNumber phoneNumber, PhoneNumber oldPhoneNumber)
    {
        var command = "";
        if (phoneNumber.PatNum != oldPhoneNumber.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(phoneNumber.PatNum) + "";
        }

        if (phoneNumber.PhoneNumberVal != oldPhoneNumber.PhoneNumberVal)
        {
            if (command != "") command += ",";
            command += "PhoneNumberVal = '" + SOut.String(phoneNumber.PhoneNumberVal) + "'";
        }

        if (phoneNumber.PhoneNumberDigits != oldPhoneNumber.PhoneNumberDigits)
        {
            if (command != "") command += ",";
            command += "PhoneNumberDigits = '" + SOut.String(phoneNumber.PhoneNumberDigits) + "'";
        }

        if (phoneNumber.PhoneType != oldPhoneNumber.PhoneType)
        {
            if (command != "") command += ",";
            command += "PhoneType = " + SOut.Int((int) phoneNumber.PhoneType) + "";
        }

        if (command == "") return false;
        command = "UPDATE phonenumber SET " + command
                                            + " WHERE PhoneNumberNum = " + SOut.Long(phoneNumber.PhoneNumberNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PhoneNumber phoneNumber, PhoneNumber oldPhoneNumber)
    {
        if (phoneNumber.PatNum != oldPhoneNumber.PatNum) return true;
        if (phoneNumber.PhoneNumberVal != oldPhoneNumber.PhoneNumberVal) return true;
        if (phoneNumber.PhoneNumberDigits != oldPhoneNumber.PhoneNumberDigits) return true;
        if (phoneNumber.PhoneType != oldPhoneNumber.PhoneType) return true;
        return false;
    }

    public static void Delete(long phoneNumberNum)
    {
        var command = "DELETE FROM phonenumber "
                      + "WHERE PhoneNumberNum = " + SOut.Long(phoneNumberNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPhoneNumberNums)
    {
        if (listPhoneNumberNums == null || listPhoneNumberNums.Count == 0) return;
        var command = "DELETE FROM phonenumber "
                      + "WHERE PhoneNumberNum IN(" + string.Join(",", listPhoneNumberNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}