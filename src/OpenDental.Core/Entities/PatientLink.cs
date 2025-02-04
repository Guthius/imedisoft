using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PatientLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PatientLinkNum;

    ///<summary>FK to patient.PatNum. The patient that is linked from.
    ///For a Merge type, this is that patient that was merged from.
    ///For a Clone type, this is the original or master patient.</summary>
    public long PatNumFrom;

    ///<summary>FK to patient.PatNum, unless LinkType=PaySimple. The patient that is linked to.
    ///For a Merge type, this is that patient that was merged into.
    ///For a Clone type, this represents the clone that was made from the PatNumFrom patient.</summary>
    public long PatNumTo;

    public PatientLinkType LinkType;

    ///<summary>The time the link was created.</summary>
    public DateTime DateTimeLink;
}

public enum PatientLinkType
{
    Undefined,

    /// <summary>
    /// The two patients have been merged into each other.
    /// </summary>
    Merge,

    /// <summary>
    /// A clone has been made of the From patient.
    /// PatNumFrom is the original or master and PatNumTo is the clone.
    /// </summary>
    Clone,

    /// <summary>
    /// The PatNumFrom column will hold the ID for PaySimple.
    /// This should not be used in OpenDental to get a patient.
    /// </summary>
    PaySimple,
}