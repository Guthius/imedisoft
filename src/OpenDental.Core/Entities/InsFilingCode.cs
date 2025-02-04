using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsFilingCode : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InsFilingCodeNum;

    public string Descript;

    ///<summary>Code for electronic claim.</summary>
    public string EclaimCode;

    public int ItemOrder;

    ///<summary>FK to definition.DefNum.  Reporting Group.</summary>
    public long GroupType;

    ///<summary>If set to true, and the patient's secondary insurance plan uses this insfilingcode, the secondary insurance plan will 
    ///not be populated on primary e-claims or paper claims.</summary>
    public bool ExcludeOtherCoverageOnPriClaims;

    public InsFilingCode Clone()
    {
        return (InsFilingCode) MemberwiseClone();
    }
}