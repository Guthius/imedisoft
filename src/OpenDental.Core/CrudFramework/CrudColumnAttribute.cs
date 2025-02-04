using System;

namespace OpenDentBusiness;

[AttributeUsage(AttributeTargets.Field)]
public class CrudColumnAttribute : Attribute
{
    public bool IsPriKey { get; set; }
    public bool IsNotDbColumn { get; set; }
}