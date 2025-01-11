using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenDentBusiness.WebTypes.WebForms;

[Serializable]
[CrudTable(IsMissingInGeneral = true, CrudLocationOverride = @"..\..\..\OpenDentBusiness\WebTypes\WebForms\Crud", NamespaceOverride = "OpenDentBusiness.WebTypes.WebForms.Crud", CrudExcludePrefC = true)]
public class WebForms_SheetField : TableBase
{
    public SheetFieldType FieldType;
    public string FieldName;
    public string FieldValue;
    public float FontSize;
    public string FontName;
    public bool FontIsBold;
    public int XPos;
    public int YPos;
    public int Width;
    public int Height;
    public GrowthBehaviorEnum GrowthBehavior;
    public string RadioButtonValue;
    public string RadioButtonGroup;
    public bool IsRequired;
    public int TabOrder;
    public string ReportableName;
    public HorizontalAlignment TextAlign;
    public Color ItemColor;
    public int TabOrderMobile;
    public string UiLabelMobile;
    public string UiLabelMobileRadioButton;
    public long SheetFieldDefNum;
}