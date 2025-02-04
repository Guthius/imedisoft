using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DashboardAR : TableBase
{
    ///<summary>This date will always be the last day of a month.</summary>
    public DateTime DateCalc;

    ///<summary>Bal_0_30+Bal_31_60+Bal_61_90+BalOver90 for all patients.  This should also exactly equal BalTotal for all patients with positive amounts.  Negative BalTotals are credits, not A/R.</summary>
    public double BalTotal;

    ///<summary>Sum of all InsEst for all patients for the month.</summary>
    public double InsEst;
}