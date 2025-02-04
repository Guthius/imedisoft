using System;
using System.Collections.Generic;
using System.Linq;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TreatPlan : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TreatPlanNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>The date of the treatment plan</summary>
    public DateTime DateTP;

    public string Heading;

    ///<summary>A note specific to this treatment plan that shows at the bottom.</summary>
    public string Note;

    ///<summary>The encrypted and bound signature in base64 format.  The signature is bound to the concatenation of the tp Note, DateTP, and to each proctp Descript and PatAmt.</summary>
    public string Signature;

    ///<summary>True if the signature is in Topaz format rather than OD format.</summary>
    public bool SigIsTopaz;

    ///<summary>FK to patient.PatNum. Can be 0.  The patient responsible for approving the treatment.  Public health field not visible to everyone else.</summary>
    public long ResponsParty;

    ///<summary>FK to document.DocNum. Can be 0.  If signed, this is the pdf document of the TP at time of signing. See PrefName.TreatPlanSaveSignedToPdf</summary>
    public long DocNum;

    ///<summary>Enum:TreatPlanStatus Determines the type of treatment plan this is. 0 - Saved, 1 - Active, 2 - Inactive.</summary>
    public TreatPlanStatus TPStatus;

    ///<summary>FK to userod.UserNum.  Set to the user logged in when the row was inserted at SecDateEntry date and time.</summary>
    public long SecUserNumEntry;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual date of entry.</summary>
    public DateTime SecDateEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program
    ///updates.  Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    ///<summary>FK to userod.UserNum. The user that will present the treatment plan. 
    ///Defaults to the user that entered the treatment plan, but can be changed with the TreatPlanPresenterEdit permission.</summary>
    public long UserNumPresenter;

    ///<summary>Enum:TreatPlanType Determines the type of insurance this treatment plan was saved with.  Used for displaying proper information when loading.</summary>
    public TreatPlanType TPType;

    ///<summary>The encrypted and bound signature in base64 format.  The signature is bound to the concatenation of the tp Note, DateTP, and to each proctp Descript and PatAmt.</summary>
    public string SignaturePractice;

    ///<summary>The date of the treatment plan is signed.</summary>
    public DateTime DateTSigned;

    ///<summary>The date of the treatment plan is signed by the office.</summary>
    public DateTime DateTPracticeSigned;

    ///<summary>The typed name of the person who signed the treatplan.</summary>
    public string SignatureText;

    ///<summary>The typed name of the person who signed the practice signature.</summary>
    public string SignaturePracticeText;

    ///<summary>FK to MobileAppDevice.</summary>
    public long MobileAppDeviceNum;

    ///<summary>Used to pass the list of ProcTPs in memory with the TreatPlan.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public List<ProcTP> ListProcTPs = [];
    
    public TreatPlan Copy()
    {
        var newTP = (TreatPlan) MemberwiseClone();
        newTP.ListProcTPs = ListProcTPs.Select(x => x.Copy()).ToList();
        return newTP;
    }
}

///<summary>0 - Saved, 1 - Active, 2 - Inactive </summary>
public enum TreatPlanStatus
{
    ///<summary>0 - Saved treatment plans. Prior to version 15.4.1 all treatment plans were considered archived. Archived TPs are linked to ProcTPs.</summary>
    Saved = 0,

    ///<summary>1 - Current active TP. There should be only one Active TP per patient. This is a TP linked directly to procedures via the TreatPlanAttach table.</summary>
    Active = 1,

    ///<summary>2 - Current inactive TP. This is a TP linked directly to procedures via the TreatPlanAttach table.</summary>
    Inactive = 2
}

///<summary>0 - Insurance, 1 - Discount Plan</summary>
public enum TreatPlanType
{
    ///<summary>0 - Treatment plan saved for regular insurance.</summary>
    Insurance = 0,

    ///<summary>1 - Treatment plan saved for discount plan.</summary>
    Discount = 1
}