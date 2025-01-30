using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OIDExternals
{
    public static void Insert(OIDExternal oidExternal)
    {
        OIDExternalCrud.Insert(oidExternal);
    }

    public static void Update(OIDExternal oidExternal)
    {
        OIDExternalCrud.Update(oidExternal);
    }

    public static OIDExternal GetByRootAndExtension(string rootExternal, string iDExternal)
    {
        return OIDExternalCrud.SelectOne("SELECT * FROM oidexternal WHERE rootExternal='" + SOut.String(rootExternal) + "' AND IDExternal='" + SOut.String(iDExternal) + "'");
    }

    public static List<OIDExternal> GetByInternalIdAndType(long idInternal, IdentifierType idType)
    {
        return OIDExternalCrud.SelectMany("SELECT * FROM oidexternal WHERE IDType='" + idType + "' AND IDInternal=" + idInternal);
    }

    public static OIDExternal GetByPartialRootExternal(string rootExternalPartial)
    {
        return OIDExternalCrud.SelectOne("SELECT * FROM oidexternal WHERE rootExternal LIKE '" + SOut.String(rootExternalPartial) + "%' AND IDType='" + IdentifierType.Root + "'");
    }

    public static OIDExternal GetOidExternal(string rootExternal, long idInternal, IdentifierType idType)
    {
        return OIDExternalCrud.SelectOne("SELECT * FROM oidexternal WHERE rootExternal='" + SOut.String(rootExternal) + "' AND IDType='" + idType + "' AND IDInternal=" + idInternal);
    }
}