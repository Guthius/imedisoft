using System;
using System.Collections.Generic;

namespace OpenDentBusiness.WebTypes.WebForms;

[Serializable]
[CrudTable(CrudLocationOverride = @"..\..\..\OpenDentBusiness\WebTypes\WebForms\Crud", NamespaceOverride = "OpenDentBusiness.WebTypes.WebForms.Crud", CrudExcludePrefC = true)]
public class WebForms_Sheet : TableBase
{
    public long SheetID;
    public string Description;
    public SheetTypeEnum SheetType;
    public DateTime DateTimeSheet;
    public float FontSize;
    public string FontName;
    public int Width;
    public int Height;
    public bool IsLandscape;
    public long ClinicNum;
    public bool HasMobileLayout;
    public long SheetDefNum;
    public int RevID;
    public string EServiceLogGuid = "";
    public List<WebForms_SheetField> SheetFields;
}