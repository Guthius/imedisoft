using System;
using System.Collections.Generic;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EFormDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EFormDefNum;

    ///<summary>Enum:EnumEFormType 0=PatientForm, 1=MedicalHistory, 2=Consent. This doesn't actually do anything, and all fields are available for all types, but that might eventually change if more types are added.</summary>
    public EnumEFormType FormType;

    public string Description;

    ///<summary>The date and time when the EFormDef was created. Not editable by the user in the UI.</summary>
    public DateTime DateTCreated;

    ///<summary>Deprecated.</summary>
    public bool IsInternalHidden;

    ///<summary>Required. Can be any value between 50 and 1000. On wide screens, this limits the width of the form. This is needed on pretty much anything other than a phone. Makes it look consistent across devices and prevents useless white space. Default 450.</summary>
    public int MaxWidth;

    ///<summary>Revision ID. Gets updated any time an eForm field is added or deleted from an eFormDef (this includes any time a translation is changed). Used to determine in conjunction with PrefillStatus for eClipboardSheetDef to determine whether to show a patient a new form or have them update their last filled out form. Must match up with EForm RevID to show a previously filled out form.</summary>
    public int RevID;

    ///<summary>If true, then this form will show labels at 95% and slightly bold. This looks good, but some users might not want it for certain forms, so it's an option. This applies to text, date, radiobuttons, and sigBox. It does not apply to types label, checkbox, or medicationList.</summary>
    public bool ShowLabelsBold;

    ///<summary>The amount of space below each field. Overrides the global default and can be overridden by field.SpaceBelow. -1 indicates to use default. That way, 0 means 0 space.</summary>
    public int SpaceBelowEachField;

    ///<summary>The amount of space to the right of each field. Overrides the global default and can be overridden by field.SpaceToRight. -1 indicates to use default. That way, 0 means 0 space.</summary>
    public int SpaceToRightEachField;

    ///<summary>FK to definition.DefNum. There is a global setting to save forms to the image category which has ItemVal set to "U". We completely ignore that and it will only work for sheets. If this is 0, it will not save to images. Copied to EForm child.</summary>
    public long SaveImageCategory;

    ///<Summary>This is needed for serialization/deserialization of internal EForms. We also leave this list attached to internal EForms for a while for convenience.</Summary>
    [CrudColumn(IsNotDbColumn = true)]
    public List<EFormFieldDef> ListEFormFieldDefs;

    ///<summary>Not a db field.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public bool IsDeleted;

    public EFormDef Copy()
    {
        return (EFormDef) MemberwiseClone();
    }
}