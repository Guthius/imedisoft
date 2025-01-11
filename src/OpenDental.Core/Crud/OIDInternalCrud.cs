#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OIDInternalCrud
{
    public static OIDInternal SelectOne(long oIDInternalNum)
    {
        var command = "SELECT * FROM oidinternal "
                      + "WHERE OIDInternalNum = " + SOut.Long(oIDInternalNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OIDInternal SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OIDInternal> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OIDInternal> TableToList(DataTable table)
    {
        var retVal = new List<OIDInternal>();
        OIDInternal oIDInternal;
        foreach (DataRow row in table.Rows)
        {
            oIDInternal = new OIDInternal();
            oIDInternal.OIDInternalNum = SIn.Long(row["OIDInternalNum"].ToString());
            var iDType = row["IDType"].ToString();
            if (iDType == "")
                oIDInternal.IDType = 0;
            else
                try
                {
                    oIDInternal.IDType = (IdentifierType) Enum.Parse(typeof(IdentifierType), iDType);
                }
                catch
                {
                    oIDInternal.IDType = 0;
                }

            oIDInternal.IDRoot = SIn.String(row["IDRoot"].ToString());
            retVal.Add(oIDInternal);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OIDInternal> listOIDInternals, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OIDInternal";
        var table = new DataTable(tableName);
        table.Columns.Add("OIDInternalNum");
        table.Columns.Add("IDType");
        table.Columns.Add("IDRoot");
        foreach (var oIDInternal in listOIDInternals)
            table.Rows.Add(SOut.Long(oIDInternal.OIDInternalNum), SOut.Int((int) oIDInternal.IDType), oIDInternal.IDRoot);
        return table;
    }

    public static long Insert(OIDInternal oIDInternal)
    {
        return Insert(oIDInternal, false);
    }

    public static long Insert(OIDInternal oIDInternal, bool useExistingPK)
    {
        var command = "INSERT INTO oidinternal (";

        command += "IDType,IDRoot) VALUES(";

        command +=
            "'" + SOut.String(oIDInternal.IDType.ToString()) + "',"
            + "'" + SOut.String(oIDInternal.IDRoot) + "')";
        {
            oIDInternal.OIDInternalNum = Db.NonQ(command, true, "OIDInternalNum", "oIDInternal");
        }
        return oIDInternal.OIDInternalNum;
    }

    public static long InsertNoCache(OIDInternal oIDInternal)
    {
        return InsertNoCache(oIDInternal, false);
    }

    public static long InsertNoCache(OIDInternal oIDInternal, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO oidinternal (";
        if (isRandomKeys || useExistingPK) command += "OIDInternalNum,";
        command += "IDType,IDRoot) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(oIDInternal.OIDInternalNum) + ",";
        command +=
            "'" + SOut.String(oIDInternal.IDType.ToString()) + "',"
            + "'" + SOut.String(oIDInternal.IDRoot) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            oIDInternal.OIDInternalNum = Db.NonQ(command, true, "OIDInternalNum", "oIDInternal");
        return oIDInternal.OIDInternalNum;
    }

    public static void Update(OIDInternal oIDInternal)
    {
        var command = "UPDATE oidinternal SET "
                      + "IDType        = '" + SOut.String(oIDInternal.IDType.ToString()) + "', "
                      + "IDRoot        = '" + SOut.String(oIDInternal.IDRoot) + "' "
                      + "WHERE OIDInternalNum = " + SOut.Long(oIDInternal.OIDInternalNum);
        Db.NonQ(command);
    }

    public static bool Update(OIDInternal oIDInternal, OIDInternal oldOIDInternal)
    {
        var command = "";
        if (oIDInternal.IDType != oldOIDInternal.IDType)
        {
            if (command != "") command += ",";
            command += "IDType = '" + SOut.String(oIDInternal.IDType.ToString()) + "'";
        }

        if (oIDInternal.IDRoot != oldOIDInternal.IDRoot)
        {
            if (command != "") command += ",";
            command += "IDRoot = '" + SOut.String(oIDInternal.IDRoot) + "'";
        }

        if (command == "") return false;
        command = "UPDATE oidinternal SET " + command
                                            + " WHERE OIDInternalNum = " + SOut.Long(oIDInternal.OIDInternalNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OIDInternal oIDInternal, OIDInternal oldOIDInternal)
    {
        if (oIDInternal.IDType != oldOIDInternal.IDType) return true;
        if (oIDInternal.IDRoot != oldOIDInternal.IDRoot) return true;
        return false;
    }

    public static void Delete(long oIDInternalNum)
    {
        var command = "DELETE FROM oidinternal "
                      + "WHERE OIDInternalNum = " + SOut.Long(oIDInternalNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOIDInternalNums)
    {
        if (listOIDInternalNums == null || listOIDInternalNums.Count == 0) return;
        var command = "DELETE FROM oidinternal "
                      + "WHERE OIDInternalNum IN(" + string.Join(",", listOIDInternalNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}