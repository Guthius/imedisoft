#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EServiceShortGuidCrud
{
    public static EServiceShortGuid SelectOne(long eServiceShortGuidNum)
    {
        var command = "SELECT * FROM eserviceshortguid "
                      + "WHERE EServiceShortGuidNum = " + SOut.Long(eServiceShortGuidNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EServiceShortGuid SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EServiceShortGuid> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EServiceShortGuid> TableToList(DataTable table)
    {
        var retVal = new List<EServiceShortGuid>();
        EServiceShortGuid eServiceShortGuid;
        foreach (DataRow row in table.Rows)
        {
            eServiceShortGuid = new EServiceShortGuid();
            eServiceShortGuid.EServiceShortGuidNum = SIn.Long(row["EServiceShortGuidNum"].ToString());
            var eServiceCode = row["EServiceCode"].ToString();
            if (eServiceCode == "")
                eServiceShortGuid.EServiceCode = 0;
            else
                try
                {
                    eServiceShortGuid.EServiceCode = (eServiceCode) Enum.Parse(typeof(eServiceCode), eServiceCode);
                }
                catch
                {
                    eServiceShortGuid.EServiceCode = 0;
                }

            eServiceShortGuid.ShortGuid = SIn.String(row["ShortGuid"].ToString());
            eServiceShortGuid.ShortURL = SIn.String(row["ShortURL"].ToString());
            eServiceShortGuid.FKey = SIn.Long(row["FKey"].ToString());
            var fKeyType = row["FKeyType"].ToString();
            if (fKeyType == "")
                eServiceShortGuid.FKeyType = 0;
            else
                try
                {
                    eServiceShortGuid.FKeyType = (EServiceShortGuidKeyType) Enum.Parse(typeof(EServiceShortGuidKeyType), fKeyType);
                }
                catch
                {
                    eServiceShortGuid.FKeyType = 0;
                }

            eServiceShortGuid.DateTimeExpiration = SIn.DateTime(row["DateTimeExpiration"].ToString());
            eServiceShortGuid.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            retVal.Add(eServiceShortGuid);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EServiceShortGuid> listEServiceShortGuids, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EServiceShortGuid";
        var table = new DataTable(tableName);
        table.Columns.Add("EServiceShortGuidNum");
        table.Columns.Add("EServiceCode");
        table.Columns.Add("ShortGuid");
        table.Columns.Add("ShortURL");
        table.Columns.Add("FKey");
        table.Columns.Add("FKeyType");
        table.Columns.Add("DateTimeExpiration");
        table.Columns.Add("DateTEntry");
        foreach (var eServiceShortGuid in listEServiceShortGuids)
            table.Rows.Add(SOut.Long(eServiceShortGuid.EServiceShortGuidNum), SOut.Int((int) eServiceShortGuid.EServiceCode), eServiceShortGuid.ShortGuid, eServiceShortGuid.ShortURL, SOut.Long(eServiceShortGuid.FKey), SOut.Int((int) eServiceShortGuid.FKeyType), SOut.DateT(eServiceShortGuid.DateTimeExpiration, false), SOut.DateT(eServiceShortGuid.DateTEntry, false));
        return table;
    }

    public static long Insert(EServiceShortGuid eServiceShortGuid)
    {
        return Insert(eServiceShortGuid, false);
    }

    public static long Insert(EServiceShortGuid eServiceShortGuid, bool useExistingPK)
    {
        var command = "INSERT INTO eserviceshortguid (";

        command += "EServiceCode,ShortGuid,ShortURL,FKey,FKeyType,DateTimeExpiration,DateTEntry) VALUES(";

        command +=
            "'" + SOut.String(eServiceShortGuid.EServiceCode.ToString()) + "',"
            + "'" + SOut.String(eServiceShortGuid.ShortGuid) + "',"
            + "'" + SOut.String(eServiceShortGuid.ShortURL) + "',"
            + SOut.Long(eServiceShortGuid.FKey) + ","
            + "'" + SOut.String(eServiceShortGuid.FKeyType.ToString()) + "',"
            + SOut.DateT(eServiceShortGuid.DateTimeExpiration) + ","
            + DbHelper.Now() + ")";
        {
            eServiceShortGuid.EServiceShortGuidNum = Db.NonQ(command, true, "EServiceShortGuidNum", "eServiceShortGuid");
        }
        return eServiceShortGuid.EServiceShortGuidNum;
    }

    public static void InsertMany(List<EServiceShortGuid> listEServiceShortGuids)
    {
        InsertMany(listEServiceShortGuids, false);
    }

    public static void InsertMany(List<EServiceShortGuid> listEServiceShortGuids, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEServiceShortGuids.Count)
        {
            var eServiceShortGuid = listEServiceShortGuids[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO eserviceshortguid (");
                if (useExistingPK) sbCommands.Append("EServiceShortGuidNum,");
                sbCommands.Append("EServiceCode,ShortGuid,ShortURL,FKey,FKeyType,DateTimeExpiration,DateTEntry) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(eServiceShortGuid.EServiceShortGuidNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(eServiceShortGuid.EServiceCode.ToString()) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(eServiceShortGuid.ShortGuid) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(eServiceShortGuid.ShortURL) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(eServiceShortGuid.FKey));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(eServiceShortGuid.FKeyType.ToString()) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(eServiceShortGuid.DateTimeExpiration));
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
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
                if (index == listEServiceShortGuids.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(EServiceShortGuid eServiceShortGuid)
    {
        return InsertNoCache(eServiceShortGuid, false);
    }

    public static long InsertNoCache(EServiceShortGuid eServiceShortGuid, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO eserviceshortguid (";
        if (isRandomKeys || useExistingPK) command += "EServiceShortGuidNum,";
        command += "EServiceCode,ShortGuid,ShortURL,FKey,FKeyType,DateTimeExpiration,DateTEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eServiceShortGuid.EServiceShortGuidNum) + ",";
        command +=
            "'" + SOut.String(eServiceShortGuid.EServiceCode.ToString()) + "',"
            + "'" + SOut.String(eServiceShortGuid.ShortGuid) + "',"
            + "'" + SOut.String(eServiceShortGuid.ShortURL) + "',"
            + SOut.Long(eServiceShortGuid.FKey) + ","
            + "'" + SOut.String(eServiceShortGuid.FKeyType.ToString()) + "',"
            + SOut.DateT(eServiceShortGuid.DateTimeExpiration) + ","
            + DbHelper.Now() + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eServiceShortGuid.EServiceShortGuidNum = Db.NonQ(command, true, "EServiceShortGuidNum", "eServiceShortGuid");
        return eServiceShortGuid.EServiceShortGuidNum;
    }

    public static void Update(EServiceShortGuid eServiceShortGuid)
    {
        var command = "UPDATE eserviceshortguid SET "
                      + "EServiceCode        = '" + SOut.String(eServiceShortGuid.EServiceCode.ToString()) + "', "
                      + "ShortGuid           = '" + SOut.String(eServiceShortGuid.ShortGuid) + "', "
                      + "ShortURL            = '" + SOut.String(eServiceShortGuid.ShortURL) + "', "
                      + "FKey                =  " + SOut.Long(eServiceShortGuid.FKey) + ", "
                      + "FKeyType            = '" + SOut.String(eServiceShortGuid.FKeyType.ToString()) + "', "
                      + "DateTimeExpiration  =  " + SOut.DateT(eServiceShortGuid.DateTimeExpiration) + " "
                      //DateTEntry not allowed to change
                      + "WHERE EServiceShortGuidNum = " + SOut.Long(eServiceShortGuid.EServiceShortGuidNum);
        Db.NonQ(command);
    }

    public static bool Update(EServiceShortGuid eServiceShortGuid, EServiceShortGuid oldEServiceShortGuid)
    {
        var command = "";
        if (eServiceShortGuid.EServiceCode != oldEServiceShortGuid.EServiceCode)
        {
            if (command != "") command += ",";
            command += "EServiceCode = '" + SOut.String(eServiceShortGuid.EServiceCode.ToString()) + "'";
        }

        if (eServiceShortGuid.ShortGuid != oldEServiceShortGuid.ShortGuid)
        {
            if (command != "") command += ",";
            command += "ShortGuid = '" + SOut.String(eServiceShortGuid.ShortGuid) + "'";
        }

        if (eServiceShortGuid.ShortURL != oldEServiceShortGuid.ShortURL)
        {
            if (command != "") command += ",";
            command += "ShortURL = '" + SOut.String(eServiceShortGuid.ShortURL) + "'";
        }

        if (eServiceShortGuid.FKey != oldEServiceShortGuid.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(eServiceShortGuid.FKey) + "";
        }

        if (eServiceShortGuid.FKeyType != oldEServiceShortGuid.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = '" + SOut.String(eServiceShortGuid.FKeyType.ToString()) + "'";
        }

        if (eServiceShortGuid.DateTimeExpiration != oldEServiceShortGuid.DateTimeExpiration)
        {
            if (command != "") command += ",";
            command += "DateTimeExpiration = " + SOut.DateT(eServiceShortGuid.DateTimeExpiration) + "";
        }

        //DateTEntry not allowed to change
        if (command == "") return false;
        command = "UPDATE eserviceshortguid SET " + command
                                                  + " WHERE EServiceShortGuidNum = " + SOut.Long(eServiceShortGuid.EServiceShortGuidNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EServiceShortGuid eServiceShortGuid, EServiceShortGuid oldEServiceShortGuid)
    {
        if (eServiceShortGuid.EServiceCode != oldEServiceShortGuid.EServiceCode) return true;
        if (eServiceShortGuid.ShortGuid != oldEServiceShortGuid.ShortGuid) return true;
        if (eServiceShortGuid.ShortURL != oldEServiceShortGuid.ShortURL) return true;
        if (eServiceShortGuid.FKey != oldEServiceShortGuid.FKey) return true;
        if (eServiceShortGuid.FKeyType != oldEServiceShortGuid.FKeyType) return true;
        if (eServiceShortGuid.DateTimeExpiration != oldEServiceShortGuid.DateTimeExpiration) return true;
        //DateTEntry not allowed to change
        return false;
    }

    public static void Delete(long eServiceShortGuidNum)
    {
        var command = "DELETE FROM eserviceshortguid "
                      + "WHERE EServiceShortGuidNum = " + SOut.Long(eServiceShortGuidNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEServiceShortGuidNums)
    {
        if (listEServiceShortGuidNums == null || listEServiceShortGuidNums.Count == 0) return;
        var command = "DELETE FROM eserviceshortguid "
                      + "WHERE EServiceShortGuidNum IN(" + string.Join(",", listEServiceShortGuidNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}