using System;
using CodeBase;

namespace OpenDentBusiness;

public class AutoCommActives
{
    public static bool IsForEmail(CommType commType)
    {
        var contactMethod = EnumTools.GetAttributeOrDefault<CommTypeAttribute>(commType).ContactMethod;
        return contactMethod == ContactMethod.Email;
    }

    public static bool IsForEmail(string commTypeStr)
    {
        if (!Enum.TryParse(commTypeStr, out CommType commType)) return false;

        return IsForEmail(commType);
    }
}