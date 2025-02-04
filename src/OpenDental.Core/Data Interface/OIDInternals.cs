using System;
using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OIDInternals
{
    private static long _customerPatNum;

    public static OIDInternal GetForType(IdentifierType identifierType)
    {
        InsertMissingValues();

        return OIDInternalCrud.SelectOne("SELECT * FROM oidinternal WHERE IDType='" + identifierType + "'");
    }

    public static void InsertMissingValues()
    {
        var identifierTypes = new List<IdentifierType>();

        var oidInternals = OIDInternalCrud.SelectMany("SELECT * FROM oidinternal");
        foreach (var oidInternal in oidInternals)
        {
            identifierTypes.Add(oidInternal.IDType);
        }

        for (var i = 0; i < Enum.GetValues(typeof(IdentifierType)).Length; i++)
        {
            if (identifierTypes.Contains((IdentifierType) i))
            {
                continue;
            }

            DataCore.NonQ("INSERT INTO oidinternal (IDType,IDRoot) VALUES('" + (IdentifierType) i + "','')", false);
        }
    }
}