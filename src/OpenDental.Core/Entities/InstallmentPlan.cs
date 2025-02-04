using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InstallmentPlan : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InstallmentPlanNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>Date payment plan agreement was made.</summary>
    public DateTime DateAgreement;

    ///<summary>Date of first payment.</summary>
    public DateTime DateFirstPayment;

    ///<summary>Amount of monthly payment.</summary>
    public double MonthlyPayment;

    ///<summary>Annual Percentage Rate. e.g. 12.</summary>
    public float APR;

    public string Note;

    public InstallmentPlan Copy()
    {
        return (InstallmentPlan) MemberwiseClone();
    }
}