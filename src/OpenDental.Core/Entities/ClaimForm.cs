using System.Collections.Generic;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClaimForm : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClaimFormNum;
    
    public string Description;
    public bool IsHidden;
    public string FontName = "";
    public float FontSize;

    ///<summary>Deprecated as of version 17.2. Internal claimforms have been moved over to XML files in OpenDentBusiness.Properties.Resources.</summary>
    public string UniqueID = "";

    ///<summary>Set to false to not print images.  This removes the background for printing on premade forms.</summary>
    public bool PrintImages;
    
    public int OffsetX;
    public int OffsetY;
    public int Width = 850;
    public int Height = 1100;

    ///<summary>This is not a database column.  It is list of all claimformItems that are attached to this ClaimForm.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public List<ClaimFormItem> Items = [];

    ///<summary>This is not a database column. If this claimform is internal, it cannot be edited.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public bool IsInternal;

    public ClaimForm Copy()
    {
        var cf = (ClaimForm) MemberwiseClone();
        List<ClaimFormItem> claimFormItemCopies = [];
        foreach (var t in cf.Items)
        {
            claimFormItemCopies.Add(t.Copy());
        }

        cf.Items = claimFormItemCopies;
        return cf;
    }
}