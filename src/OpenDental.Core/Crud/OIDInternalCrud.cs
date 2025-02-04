using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OIDInternalCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var oIDInternal = new OIDInternal
            {
                OIDInternalNum = SIn.Long(row["OIDInternalNum"].ToString())
            };
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
}