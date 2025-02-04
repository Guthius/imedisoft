using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AllergyDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AllergyDefNum;


    public string Description;
    public bool IsHidden;

    ///<summary>Enum:SnomedAllergy SNOMED Allergy Type Code.  Only used to create CCD in FormSummaryOfCare.</summary>
    public SnomedAllergy SnomedType;

    ///<summary>FK to medication.MedicationNum.  Optional, only used with CCD messages.</summary>
    public long MedicationNum;

    public string UniiCode;
}

public enum SnomedAllergy
{
    ///<summary>0-No SNOMED allergy type code has been assigned.</summary>
    None,

    ///<summary>1-Allergy to substance (disorder), code number 418038007.</summary>
    AllergyToSubstance,

    ///<summary>2-Drug allergy (disorder), code number 416098002.</summary>
    DrugAllergy,

    ///<summary>3-Drug intolerance (disorder), code number 59037007.</summary>
    DrugIntolerance,

    ///<summary>4-Food allergy (disorder), code number 414285001.</summary>
    FoodAllergy,

    ///<summary>5-Food intolerance (disorder), code number 235719002.</summary>
    FoodIntolerance,

    ///<summary>6-Propensity to adverse reactions (disorder), code number 420134006.</summary>
    AdverseReactions,

    ///<summary>7-Propensity to adverse reactions to drug (disorder), code number 419511003</summary>
    AdverseReactionsToDrug,

    ///<summary>8-Propensity to adverse reactions to food (disorder), code number 418471000.</summary>
    AdverseReactionsToFood,

    ///<summary>9-Propensity to adverse reactions to substance (disorder), code number 419199007.</summary>
    AdverseReactionsToSubstance
}