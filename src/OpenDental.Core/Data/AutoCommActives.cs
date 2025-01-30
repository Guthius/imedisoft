using System;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AutoCommActives
{
    public static bool IsForEmail(CommType commType)
    {
        return EnumTools.GetAttributeOrDefault<CommTypeAttribute>(commType).ContactMethod == ContactMethod.Email;
    }

    public static bool IsForEmail(string commTypeStr)
    {
        return Enum.TryParse(commTypeStr, out CommType commType) && IsForEmail(commType);
    }
}