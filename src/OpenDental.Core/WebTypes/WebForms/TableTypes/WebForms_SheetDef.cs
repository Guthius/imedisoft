using System;

namespace OpenDentBusiness.WebTypes.WebForms;

[Serializable]
[CrudTable(CrudLocationOverride = @"..\..\..\OpenDentBusiness\WebTypes\WebForms\Crud", NamespaceOverride = "OpenDentBusiness.WebTypes.WebForms.Crud", CrudExcludePrefC = true)]
public class WebForms_SheetDef : TableBase
{
    public long WebSheetDefID;
    public string Description;
    public long SheetDefNum;
}