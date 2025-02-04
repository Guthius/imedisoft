using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoProcLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoProcLinkNum;

    ///<summary>FK to orthocase.OrthoCaseNum.  </summary>
    public long OrthoCaseNum;

    ///<summary>FK to procedurelog.ProcNum </summary>
    public long ProcNum;

    ///<summary>DateTime proclink was added. Not editable by user. </summary>
    public DateTime SecDateTEntry;

    ///<summary>FK to userod.UserNum. User that added the proc link. </summary>
    public long SecUserNumEntry;

    ///<summary>Enum:OrthoProcType Indicates what type of procedure is being associated to Ortho Case in link.</summary>
    public OrthoProcType ProcLinkType;

    public OrthoProcLink Copy()
    {
        return (OrthoProcLink) MemberwiseClone();
    }
}

///<summary>A procedures type as it relates to an Ortho Case</summary>
public enum OrthoProcType
{
    /// <summary>
    /// Procedure for putting appliance on.
    /// </summary>
    Banding,

    /// <summary>
    /// Procedure for removing appliance.
    /// </summary>
    Debond,

    /// <summary>
    /// All maintenance visits between Banding and Debond procedures.
    /// </summary>
    Visit
}