#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SecurityLogHashCrud
{
    public static SecurityLogHash SelectOne(long securityLogHashNum)
    {
        var command = "SELECT * FROM securityloghash "
                      + "WHERE SecurityLogHashNum = " + SOut.Long(securityLogHashNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SecurityLogHash SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SecurityLogHash> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SecurityLogHash> TableToList(DataTable table)
    {
        var retVal = new List<SecurityLogHash>();
        SecurityLogHash securityLogHash;
        foreach (DataRow row in table.Rows)
        {
            securityLogHash = new SecurityLogHash();
            securityLogHash.SecurityLogHashNum = SIn.Long(row["SecurityLogHashNum"].ToString());
            securityLogHash.SecurityLogNum = SIn.Long(row["SecurityLogNum"].ToString());
            securityLogHash.LogHash = SIn.String(row["LogHash"].ToString());
            retVal.Add(securityLogHash);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SecurityLogHash> listSecurityLogHashs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SecurityLogHash";
        var table = new DataTable(tableName);
        table.Columns.Add("SecurityLogHashNum");
        table.Columns.Add("SecurityLogNum");
        table.Columns.Add("LogHash");
        foreach (var securityLogHash in listSecurityLogHashs)
            table.Rows.Add(SOut.Long(securityLogHash.SecurityLogHashNum), SOut.Long(securityLogHash.SecurityLogNum), securityLogHash.LogHash);
        return table;
    }

    public static long Insert(SecurityLogHash securityLogHash)
    {
        return Insert(securityLogHash, false);
    }

    public static long Insert(SecurityLogHash securityLogHash, bool useExistingPK)
    {
        var command = "INSERT INTO securityloghash (";

        command += "SecurityLogNum,LogHash) VALUES(";

        command +=
            SOut.Long(securityLogHash.SecurityLogNum) + ","
                                                      + "'" + SOut.String(securityLogHash.LogHash) + "')";
        {
            securityLogHash.SecurityLogHashNum = Db.NonQ(command, true, "SecurityLogHashNum", "securityLogHash");
        }
        return securityLogHash.SecurityLogHashNum;
    }

    public static void InsertMany(List<SecurityLogHash> listSecurityLogHashs)
    {
        InsertMany(listSecurityLogHashs, false);
    }

    public static void InsertMany(List<SecurityLogHash> listSecurityLogHashs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listSecurityLogHashs.Count)
        {
            var securityLogHash = listSecurityLogHashs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO securityloghash (");
                if (useExistingPK) sbCommands.Append("SecurityLogHashNum,");
                sbCommands.Append("SecurityLogNum,LogHash) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(securityLogHash.SecurityLogHashNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(securityLogHash.SecurityLogNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(securityLogHash.LogHash) + "'");
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
                if (index == listSecurityLogHashs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(SecurityLogHash securityLogHash)
    {
        return InsertNoCache(securityLogHash, false);
    }

    public static long InsertNoCache(SecurityLogHash securityLogHash, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO securityloghash (";
        if (isRandomKeys || useExistingPK) command += "SecurityLogHashNum,";
        command += "SecurityLogNum,LogHash) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(securityLogHash.SecurityLogHashNum) + ",";
        command +=
            SOut.Long(securityLogHash.SecurityLogNum) + ","
                                                      + "'" + SOut.String(securityLogHash.LogHash) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            securityLogHash.SecurityLogHashNum = Db.NonQ(command, true, "SecurityLogHashNum", "securityLogHash");
        return securityLogHash.SecurityLogHashNum;
    }

    public static void Update(SecurityLogHash securityLogHash)
    {
        var command = "UPDATE securityloghash SET "
                      + "SecurityLogNum    =  " + SOut.Long(securityLogHash.SecurityLogNum) + ", "
                      + "LogHash           = '" + SOut.String(securityLogHash.LogHash) + "' "
                      + "WHERE SecurityLogHashNum = " + SOut.Long(securityLogHash.SecurityLogHashNum);
        Db.NonQ(command);
    }

    public static bool Update(SecurityLogHash securityLogHash, SecurityLogHash oldSecurityLogHash)
    {
        var command = "";
        if (securityLogHash.SecurityLogNum != oldSecurityLogHash.SecurityLogNum)
        {
            if (command != "") command += ",";
            command += "SecurityLogNum = " + SOut.Long(securityLogHash.SecurityLogNum) + "";
        }

        if (securityLogHash.LogHash != oldSecurityLogHash.LogHash)
        {
            if (command != "") command += ",";
            command += "LogHash = '" + SOut.String(securityLogHash.LogHash) + "'";
        }

        if (command == "") return false;
        command = "UPDATE securityloghash SET " + command
                                                + " WHERE SecurityLogHashNum = " + SOut.Long(securityLogHash.SecurityLogHashNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SecurityLogHash securityLogHash, SecurityLogHash oldSecurityLogHash)
    {
        if (securityLogHash.SecurityLogNum != oldSecurityLogHash.SecurityLogNum) return true;
        if (securityLogHash.LogHash != oldSecurityLogHash.LogHash) return true;
        return false;
    }

    public static void Delete(long securityLogHashNum)
    {
        var command = "DELETE FROM securityloghash "
                      + "WHERE SecurityLogHashNum = " + SOut.Long(securityLogHashNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSecurityLogHashNums)
    {
        if (listSecurityLogHashNums == null || listSecurityLogHashNums.Count == 0) return;
        var command = "DELETE FROM securityloghash "
                      + "WHERE SecurityLogHashNum IN(" + string.Join(",", listSecurityLogHashNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}