using System;
using System.Drawing;

namespace OpenDentBusiness.WebTypes.WebForms;

[Serializable]
[CrudTable(CrudLocationOverride = @"..\..\..\OpenDentBusiness\WebTypes\WebForms\Crud", NamespaceOverride = "OpenDentBusiness.WebTypes.WebForms.Crud", CrudExcludePrefC = true)]
public class WebForms_Preference : TableBase
{
    public Color ColorBorder;
    public string CultureName;
    public bool DisableSignatures;

    public WebForms_Preference Copy()
    {
        return (WebForms_Preference) MemberwiseClone();
    }
}