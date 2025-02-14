using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OIDExternalCrud
{
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
}