using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OIDExternalCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var oIDExternal = new OIDExternal
            {
                OIDExternalNum = SIn.Long(row["OIDExternalNum"].ToString())
            };
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

    public static void Insert(OIDExternal oIDExternal)
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
}