using System;

namespace OpenDentBusiness.WebTypes.WebForms;

[Serializable]
[CrudTable(IsMissingInGeneral = true, HasBatchWriteMethods = true, CrudLocationOverride = @"..\..\..\OpenDentBusiness\WebTypes\WebForms\Crud", NamespaceOverride = "OpenDentBusiness.WebTypes.WebForms.Crud", CrudExcludePrefC = true)]
public class WebForms_SheetFieldDef : TableBase;