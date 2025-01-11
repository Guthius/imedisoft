#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OIDExternalCrud
{
    public static OIDExternal SelectOne(long oIDExternalNum)
    {
        var command = "SELECT * FROM oidexternal "
                      + "WHERE OIDExternalNum = " + SOut.Long(oIDExternalNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OIDExternal SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OIDExternal> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OIDExternal> TableToList(DataTable table)
    {
        var retVal = new List<OIDExternal>();
        OIDExternal oIDExternal;
        foreach (DataRow row in table.Rows)
        {
            oIDExternal = new OIDExternal();
            oIDExternal.OIDExternalNum = SIn.Long(row["OIDExternalNum"].ToString());
            var iDType = row["IDType"].ToString();
            if (iDType == "")
                oIDExternal.IDType = 0;
            else
                try
                {
                    oIDExternal.IDType = (IdentifierType) Enum.Parse(typeof(IdentifierType), iDType);
                }
                catch
                {
                    oIDExternal.IDType = 0;
                }

            oIDExternal.IDInternal = SIn.Long(row["IDInternal"].ToString());
            oIDExternal.IDExternal = SIn.String(row["IDExternal"].ToString());
            oIDExternal.rootExternal = SIn.String(row["rootExternal"].ToString());
            retVal.Add(oIDExternal);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OIDExternal> listOIDExternals, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OIDExternal";
        var table = new DataTable(tableName);
        table.Columns.Add("OIDExternalNum");
        table.Columns.Add("IDType");
        table.Columns.Add("IDInternal");
        table.Columns.Add("IDExternal");
        table.Columns.Add("rootExternal");
        foreach (var oIDExternal in listOIDExternals)
            table.Rows.Add(SOut.Long(oIDExternal.OIDExternalNum), SOut.Int((int) oIDExternal.IDType), SOut.Long(oIDExternal.IDInternal), oIDExternal.IDExternal, oIDExternal.rootExternal);
        return table;
    }

    public static long Insert(OIDExternal oIDExternal)
    {
        return Insert(oIDExternal, false);
    }

    public static long Insert(OIDExternal oIDExternal, bool useExistingPK)
    {
        var command = "INSERT INTO oidexternal (";

        command += "IDType,IDInternal,IDExternal,rootExternal) VALUES(";

        command +=
            "'" + SOut.String(oIDExternal.IDType.ToString()) + "',"
            + SOut.Long(oIDExternal.IDInternal) + ","
            + "'" + SOut.String(oIDExternal.IDExternal) + "',"
            + "'" + SOut.String(oIDExternal.rootExternal) + "')";
        {
            oIDExternal.OIDExternalNum = Db.NonQ(command, true, "OIDExternalNum", "oIDExternal");
        }
        return oIDExternal.OIDExternalNum;
    }

    public static long InsertNoCache(OIDExternal oIDExternal)
    {
        return InsertNoCache(oIDExternal, false);
    }

    public static long InsertNoCache(OIDExternal oIDExternal, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO oidexternal (";
        if (isRandomKeys || useExistingPK) command += "OIDExternalNum,";
        command += "IDType,IDInternal,IDExternal,rootExternal) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(oIDExternal.OIDExternalNum) + ",";
        command +=
            "'" + SOut.String(oIDExternal.IDType.ToString()) + "',"
            + SOut.Long(oIDExternal.IDInternal) + ","
            + "'" + SOut.String(oIDExternal.IDExternal) + "',"
            + "'" + SOut.String(oIDExternal.rootExternal) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            oIDExternal.OIDExternalNum = Db.NonQ(command, true, "OIDExternalNum", "oIDExternal");
        return oIDExternal.OIDExternalNum;
    }

    public static void Update(OIDExternal oIDExternal)
    {
        var command = "UPDATE oidexternal SET "
                      + "IDType        = '" + SOut.String(oIDExternal.IDType.ToString()) + "', "
                      + "IDInternal    =  " + SOut.Long(oIDExternal.IDInternal) + ", "
                      + "IDExternal    = '" + SOut.String(oIDExternal.IDExternal) + "', "
                      + "rootExternal  = '" + SOut.String(oIDExternal.rootExternal) + "' "
                      + "WHERE OIDExternalNum = " + SOut.Long(oIDExternal.OIDExternalNum);
        Db.NonQ(command);
    }

    public static bool Update(OIDExternal oIDExternal, OIDExternal oldOIDExternal)
    {
        var command = "";
        if (oIDExternal.IDType != oldOIDExternal.IDType)
        {
            if (command != "") command += ",";
            command += "IDType = '" + SOut.String(oIDExternal.IDType.ToString()) + "'";
        }

        if (oIDExternal.IDInternal != oldOIDExternal.IDInternal)
        {
            if (command != "") command += ",";
            command += "IDInternal = " + SOut.Long(oIDExternal.IDInternal) + "";
        }

        if (oIDExternal.IDExternal != oldOIDExternal.IDExternal)
        {
            if (command != "") command += ",";
            command += "IDExternal = '" + SOut.String(oIDExternal.IDExternal) + "'";
        }

        if (oIDExternal.rootExternal != oldOIDExternal.rootExternal)
        {
            if (command != "") command += ",";
            command += "rootExternal = '" + SOut.String(oIDExternal.rootExternal) + "'";
        }

        if (command == "") return false;
        command = "UPDATE oidexternal SET " + command
                                            + " WHERE OIDExternalNum = " + SOut.Long(oIDExternal.OIDExternalNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OIDExternal oIDExternal, OIDExternal oldOIDExternal)
    {
        if (oIDExternal.IDType != oldOIDExternal.IDType) return true;
        if (oIDExternal.IDInternal != oldOIDExternal.IDInternal) return true;
        if (oIDExternal.IDExternal != oldOIDExternal.IDExternal) return true;
        if (oIDExternal.rootExternal != oldOIDExternal.rootExternal) return true;
        return false;
    }

    public static void Delete(long oIDExternalNum)
    {
        var command = "DELETE FROM oidexternal "
                      + "WHERE OIDExternalNum = " + SOut.Long(oIDExternalNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOIDExternalNums)
    {
        if (listOIDExternalNums == null || listOIDExternalNums.Count == 0) return;
        var command = "DELETE FROM oidexternal "
                      + "WHERE OIDExternalNum IN(" + string.Join(",", listOIDExternalNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}