using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class QuickPasteCat : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long QuickPasteCatNum;
    
    public string Description;
    public int ItemOrder;

    ///<summary>Enum:EnumQuickPasteType  Each Category can be set to be the default category for multiple types of notes. Stored as integers separated by commas.</summary>
    public string DefaultForTypes;

    public List<EnumQuickPasteType> ListDefaultForTypes
    {
        get
        {
            return string.IsNullOrEmpty(DefaultForTypes) ? [] : DefaultForTypes.Split(',').Select(x => SIn.Enum<EnumQuickPasteType>(x)).ToList();
        }
    }

    public QuickPasteCat Copy()
    {
        return (QuickPasteCat) MemberwiseClone();
    }
}