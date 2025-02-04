using System;
using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Provider : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProvNum;

    public string Abbr;
    public int ItemOrder;
    public string LName;
    public string FName;

    ///<summary>Middle inital or name.</summary>
    public string MI;

    ///<summary>eg. DMD or DDS.</summary>
    public string Suffix;

    ///<summary>FK to feesched.FeeSchedNum.</summary>
    public long FeeSched;

    ///<summary>FK to definition.DefNum.</summary>
    public long Specialty;

    ///<summary>or TIN.  No punctuation</summary>
    public string SSN;

    ///<summary>DEPRECATED. Can include punctuation</summary>
    public string StateLicense;

    ///<summary>DEPRECATED.  DEANum can be found in the providerclinic table.</summary>
    public string DEANum;

    ///<summary>True if hygienist.</summary>
    public bool IsSecondary;

    ///<summary>Color that shows in appointments</summary>
    public Color ProvColor;

    ///<summary>If true, provider will not show on any lists</summary>
    public bool IsHidden;

    ///<summary>True if the SSN field is actually a Tax ID Num</summary>
    public bool UsingTIN;

    ///<summary>Signature on file.</summary>
    public bool SigOnFile;
    
    public string MedicaidID;

    ///<summary>Color that shows in appointments as outline when highlighted.</summary>
    public Color OutlineColor;

    ///<summary>US NPI, and Canadian CDA provider number.</summary>
    public string NationalProvID;

    ///<summary>Canadian field required for e-claims.  Assigned by CDA.  It's OK to have multiple providers with the same OfficeNum.  Max length should be 4.</summary>
    public string CanadianOfficeNum;

    ///<summary>If none of the supplied taxonomies works.  This will show on claims.</summary>
    public string TaxonomyCodeOverride;

    ///<summary>For Canada. Set to true if CDA Net provider.</summary>
    public bool IsCDAnet;

    ///<summary>DEPRECATED. Provider medical State ID.</summary>
    public string StateRxID;

    ///<summary>Default is false because most providers are persons.  But some dummy providers used for practices or billing entities are not persons.  This is needed on 837s.</summary>
    public bool IsNotPerson;

    ///<summary>DEPRECATED. The state abbreviation where the state license number in the StateLicense field is legally registered.</summary>
    public string StateWhereLicensed;

    ///<summary>FK to provider.ProvNum</summary>
    public long ProvNumBillingOverride;
    
    public ProviderStatus ProvStatus;

    ///<summary>Determines whether the provider will show on standard reports.</summary>
    public bool IsHiddenReport;

    ///<summary>Indicates if the provider should only be scheduled in a certain way (e.g. Root canals only)</summary>
    public string SchedNote;

    /// <summary>The birthdate of the provider.</summary>
    public DateTime Birthdate;

    /// <summary>The hourly production goal amount of the provider.</summary>
    public double HourlyProdGoalAmt;

    ///<summary>The date that the provider's term ends. This can be used to prevent appointments from being scheduled, appointments from being 
    ///marked complete, prescriptions from being prescribed, and claims from being sent.</summary>
    public DateTime DateTerm;

    ///<summary>The preferred name of the provider, shows what will be displayed to patients in eClipboard.</summary>
    public string PreferredName;

    public Provider Copy()
    {
        return (Provider) MemberwiseClone();
    }

    public string GetLongDesc()
    {
        if (ProvNum == 0)
        {
            return Abbr;
        }

        var retval = Abbr + "- " + LName + ", " + FName;

        if (IsHidden)
        {
            retval += " (hidden)";
        }

        return retval;
    }
    
    public string GetAbbr()
    {
        var retval = Abbr;
        if (IsHidden)
        {
            retval += " (hidden)";
        }

        return retval;
    }
    
    public string GetFormalName()
    {
        var retVal = FName + " " + MI;
        if (MI.Length == 1)
        {
            retVal += ".";
        }

        if (MI != "")
        {
            retVal += " ";
        }

        retVal += LName;
        if (Suffix != "")
        {
            retVal += ", " + Suffix;
        }

        return retVal;
    }
}

public enum ProviderStatus
{
    Active,
    Deleted
}