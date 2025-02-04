using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DiseaseDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DiseaseDefNum;
    
    public string DiseaseName;
    public int ItemOrder;
    public bool IsHidden;

    ///<summary>The last date and time this row was altered.  Not user editable.</summary>
    public DateTime DateTStamp;

    ///<summary>FK to icd9.Icd9Code.  Example: 250.00 for diabetes.  User not allowed to enter any string anymore, must pick one from the Icd9Code table.  Some may exist in the databases without linking to a valid Icd9Code table entry if the ConvertDatabase could not find the user typed string in the list of valid Icd9Codes.</summary>
    public string ICD9Code;

    ///<summary>FK to snomed.SnomedCode.  Example: 230572002 for diabetic neuropathy.  User not allowed to enter any string anymore, must pick from the Snomed table.  Some may exist in the databases without linking to a valid Snomed table entry if the ConvertDatabase could find the user typed string in the list of valid SnomedCodes.</summary>
    public string SnomedCode;

    ///<summary>FK to icd10.Icd10Code.  Example: E10.1 for 'Type 1 diabetes mellitus with ketoacidosis'. User not allowed to enter any string anymore, must pick one from the Icd10Code table.</summary>
    public string Icd10Code;


    public DiseaseDef Copy()
    {
        return (DiseaseDef) MemberwiseClone();
    }
}