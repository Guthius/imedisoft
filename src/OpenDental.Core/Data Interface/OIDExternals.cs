using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OIDExternals
{
    public static List<OIDExternal> GetByInternalIdAndType(long idInternal, IdentifierType idType)
    {
        return OIDExternalCrud.SelectMany("SELECT * FROM oidexternal WHERE IDType='" + idType + "' AND IDInternal=" + idInternal);
    }
}