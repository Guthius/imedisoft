using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class AccountEntry
{
    public object Tag;
    public DateTime Date;
    public long PriKey;
    public long PatNum;
    public long ProvNum;
    public long ClinicNum;
    public long ProcNum;
    public long AdjNum;
    public long PayPlanChargeNum;
    public PayPlanDebitTypes PayPlanDebitType;
    public long PayPlanNum;
    public long UnearnedType;
    public decimal AmountOriginal;
    public decimal AmountAvailable;
    public decimal AmountEnd;
    public decimal InsPayAmt;
    public decimal IncomeAmt;
    public decimal AdjustedAmt;
    public decimal AdjustmentAmtPos;
    public decimal AdjustmentAmtNeg;
    public readonly List<PayPlanPrincipalApplied> ListPayPlanPrincipalApplieds = [];
    public SplitCollection SplitCollection = [];
    public StringBuilder WarningMsg = new();
    public StringBuilder ErrorMsg = new();

    public string Description
    {
        get
        {
            var description = $"{TagTypeName} #{PriKey}";
            if (GetType() == typeof(PaySplit))
            {
                var paySplit = (PaySplit) Tag;
                if (paySplit.ProcNum > 0)
                {
                    description += $" (ProcNum #{paySplit.ProcNum})";
                }
                else if (paySplit.AdjNum > 0)
                {
                    description += $" (AdjNum #{paySplit.AdjNum})";
                }
                else if (paySplit.PayPlanNum > 0)
                {
                    description += $" (PayPlanNum #{paySplit.PayPlanNum})";
                }
                else if (paySplit.UnearnedType > 0)
                {
                    description += $" (UnearnedType #{paySplit.UnearnedType})";
                }
                else
                {
                    description += $" (unallocated)";
                }
            }

            description += $", PatNum #{PatNum}";
            if (ProvNum > 0)
            {
                description += $", ProvNum #{ProvNum} ({Providers.GetAbbr(ProvNum)})";
            }

            if (ClinicNum > 0)
            {
                description += $", ClinicNum #{ClinicNum} ({Clinics.GetAbbr(ClinicNum)})";
            }

            description += $" on {Date.ToShortDateString()} [AmtOrig: {AmountOriginal:c} AmtEnd: {AmountEnd:c}]";
            return description;
        }
    }

    public string DescriptionForGrid
    {
        get
        {
            if (GetType() == typeof(PaySplit))
            {
                return IsUnearned ? Defs.GetName(DefCat.PaySplitUnearnedType, UnearnedType) : "Unallocated";
            }

            if (GetType() == typeof(Procedure))
            {
                return "Proc: " + Procedures.GetDescription((Procedure) Tag);
            }

            if (Tag.GetType() == typeof(FauxAccountEntry))
            {
                var strFaux = "PayPlanCharge";
                var fauxAccountEntry = (FauxAccountEntry) this;
                if (fauxAccountEntry.AccountEntryProc is {Tag: not null} && fauxAccountEntry.AccountEntryProc.GetType() == typeof(Procedure))
                {
                    strFaux += "\r\nProc: " + Procedures.GetDescription((Procedure) fauxAccountEntry.AccountEntryProc.Tag);
                }
                else if (fauxAccountEntry.IsAdjustment)
                {
                    strFaux += "\r\nAdjustment";
                }
                else if (CompareDecimal.IsGreaterThanZero(fauxAccountEntry.Interest))
                {
                    strFaux += "\r\nInterest";
                }
                else if (ProcNum == 0 && AdjNum == 0)
                {
                    strFaux += "\r\nUnattached";
                }

                return strFaux;
            }
            else
            {
                return TagTypeName;
            }
        }
    }

    public string TagTypeName => GetType()?.Name ?? "[NULL]";

    public bool IsPaySplitAttachedToProd
    {
        get
        {
            var type = GetType();
            if (type == null || type != typeof(PaySplit))
            {
                return false;
            }

            var paySplit = (PaySplit) Tag;
            return paySplit.ProcNum > 0 || paySplit.AdjNum > 0;
        }
    }

    public bool IsUnallocated
    {
        get
        {
            var type = GetType();
            if (type == null || type != typeof(PaySplit))
            {
                return false;
            }

            return ((PaySplit) Tag).IsUnallocated;
        }
    }

    public bool IsUnearned
    {
        get
        {
            var type = GetType();
            if (type == null || type != typeof(PaySplit))
            {
                return false;
            }

            return UnearnedType > 0;
        }
    }

    public double AmountPaid
    {
        get { return SplitCollection.Sum(x => x.SplitAmt); }
    }

    public static decimal GetExplicitlyLinkedProcAmt(AccountEntry accountEntry, bool doConsiderPatPayments = false)
    {
        if (accountEntry.GetType() is null || accountEntry.GetType() != typeof(Procedure))
        {
            return 0;
        }

        //Start with the raw ProcFeeTotal.
        var amountProc = (decimal) ((Procedure) accountEntry.Tag).ProcFeeTotal;
        //Always add any adjustments (which will actually subtract negative adjustments).
        amountProc += accountEntry.AdjustedAmt;
        //Payment Plans remove value from procedures based on how much principal has been applied.
        amountProc -= accountEntry.ListPayPlanPrincipalApplieds.Sum(x => x.PrincipalApplied);
        //Next is to consider insurance payments and then conditionally consider patient payments.
        var amountToSubtract = accountEntry.InsPayAmt;
        if (doConsiderPatPayments)
        {
            amountToSubtract += (decimal) accountEntry.AmountPaid;
        }

        return amountProc - amountToSubtract;
    }

    public new Type GetType()
    {
        return Tag?.GetType();
    }

    public AccountEntry()
    {
    }

    public AccountEntry(PayPlanCharge payPlanCharge)
    {
        Tag = payPlanCharge;
        Date = payPlanCharge.ChargeDate;
        PriKey = payPlanCharge.PayPlanChargeNum;
        AmountOriginal = (decimal) payPlanCharge.Principal + (decimal) payPlanCharge.Interest;
        AmountAvailable = AmountOriginal;
        AmountEnd = AmountOriginal;
        ProvNum = payPlanCharge.ProvNum;
        ClinicNum = payPlanCharge.ClinicNum;
        PatNum = payPlanCharge.PatNum;
        PayPlanChargeNum = payPlanCharge.PayPlanChargeNum;
        PayPlanNum = payPlanCharge.PayPlanNum;
        ProcNum = GetProcNumFromTag();
        AdjNum = GetAdjNumFromTag();
    }

    public AccountEntry(Adjustment adjustment)
    {
        Tag = adjustment;
        Date = adjustment.AdjDate;
        PriKey = adjustment.AdjNum;
        AmountOriginal = (decimal) adjustment.AdjAmt;
        AmountAvailable = AmountOriginal;
        AmountEnd = AmountOriginal;
        ProvNum = adjustment.ProvNum;
        ClinicNum = adjustment.ClinicNum;
        PatNum = adjustment.PatNum;
        ProcNum = GetProcNumFromTag();
        AdjNum = GetAdjNumFromTag();
    }

    public AccountEntry(Procedure proc)
    {
        Tag = proc;
        Date = proc.ProcDate;
        PriKey = proc.ProcNum;
        AmountOriginal = (decimal) proc.ProcFeeTotal;
        AmountAvailable = AmountOriginal;
        AmountEnd = AmountOriginal;
        ProvNum = proc.ProvNum;
        ClinicNum = proc.ClinicNum;
        PatNum = proc.PatNum;
        ProcNum = GetProcNumFromTag();
        AdjNum = GetAdjNumFromTag();
    }

    public AccountEntry(PaySplit paySplit)
    {
        Tag = paySplit;
        Date = paySplit.DatePay;
        PriKey = paySplit.SplitNum;
        AmountOriginal = 0 - (decimal) paySplit.SplitAmt;
        AmountAvailable = AmountOriginal;
        AmountEnd = AmountOriginal;
        ProvNum = paySplit.ProvNum;
        SplitCollection.Add(paySplit);
        ClinicNum = paySplit.ClinicNum;
        PatNum = paySplit.PatNum;
        PayPlanChargeNum = paySplit.PayPlanChargeNum;
        PayPlanDebitType = paySplit.PayPlanDebitType;
        PayPlanNum = paySplit.PayPlanNum;
        ProcNum = GetProcNumFromTag();
        AdjNum = GetAdjNumFromTag();
        UnearnedType = paySplit.UnearnedType;
    }

    public AccountEntry(PayAsTotal payAsTotal)
    {
        Tag = payAsTotal;
        Date = payAsTotal.DateEntry;
        PriKey = 0; //This is not a database object, no primary keys are available 
        AmountOriginal = 0 - (decimal) payAsTotal.SummedInsPayAmt;
        AmountAvailable = AmountOriginal;
        AmountEnd = AmountOriginal;
        ProvNum = payAsTotal.ProvNum;
        ClinicNum = payAsTotal.ClinicNum;
        PatNum = payAsTotal.PatNum;
        ProcNum = GetProcNumFromTag();
        AdjNum = GetAdjNumFromTag();
    }

    public AccountEntry Copy()
    {
        var accountEntry = (AccountEntry) MemberwiseClone();
        accountEntry.SplitCollection = [];
        //Make a deep copy of each split within SplitCollection because it is technically an ICollection and MemberwiseClone does not handle that.
        foreach (var paySplit in SplitCollection)
        {
            accountEntry.SplitCollection.Add(paySplit.Copy());
        }

        accountEntry.ErrorMsg = new StringBuilder(ErrorMsg.ToString());
        accountEntry.WarningMsg = new StringBuilder(WarningMsg.ToString());
        return accountEntry;
    }

    private long GetProcNumFromTag()
    {
        switch (TagTypeName)
        {
            case nameof(Adjustment):
                return ((Adjustment) Tag).ProcNum;

            case nameof(ClaimProc):
                return ((ClaimProc) Tag).ProcNum;

            case nameof(PayPlanCharge):
                var payPlanCharge = Tag as PayPlanCharge;
                var procNum = payPlanCharge.ProcNum;
                if (payPlanCharge.LinkType == PayPlanLinkType.Procedure && payPlanCharge.FKey > 0)
                {
                    procNum = payPlanCharge.FKey;
                }

                return procNum;

            case nameof(PaySplit):
                return ((PaySplit) Tag).ProcNum;

            case nameof(Procedure):
                return ((Procedure) Tag).ProcNum;

            default:
                return 0;
        }
    }

    private long GetAdjNumFromTag()
    {
        switch (TagTypeName)
        {
            case nameof(Adjustment):
                return ((Adjustment) Tag).AdjNum;
            
            case nameof(PaySplit):
                return ((PaySplit) Tag).AdjNum;
            
            case nameof(ClaimProc):
            case nameof(PayPlanCharge):
                var payPlanCharge = Tag as PayPlanCharge;
                long adjNum = 0;
                if (payPlanCharge.LinkType == PayPlanLinkType.Adjustment && payPlanCharge.FKey > 0)
                {
                    adjNum = payPlanCharge.FKey;
                }

                return adjNum;
            
            default:
                return 0;
        }
    }
}

public class PayPlanPrincipalApplied(long payPlanNum, decimal principalApplied)
{
    public readonly long PayPlanNum = payPlanNum;
    public readonly decimal PrincipalApplied = principalApplied;
}

public class FauxAccountEntry : AccountEntry
{
    public long Guarantor;
    public readonly PayPlanChargeType ChargeType;
    public bool IsAdjustment;
    public readonly bool IsDynamic;
    public readonly decimal Principal;
    public decimal PrincipalAdjusted;
    public readonly decimal Interest;
    public AccountEntry AccountEntryProc;
    public readonly bool IsOffset;

    public FauxAccountEntry(PayPlanCharge payPlanCharge, bool isPrincipal)
    {
        IsAdjustment = payPlanCharge.IsCreditAdjustment || payPlanCharge.IsDebitAdjustment;
        if (isPrincipal)
        {
            AmountOriginal = (decimal) payPlanCharge.Principal;
            Principal = (decimal) payPlanCharge.Principal;
            PrincipalAdjusted = Principal;
            
            switch (payPlanCharge.LinkType)
            {
                case PayPlanLinkType.Adjustment:
                    IsDynamic = true;
                    AdjNum = payPlanCharge.FKey;
                    break;
                
                case PayPlanLinkType.Procedure:
                    IsDynamic = true;
                    ProcNum = payPlanCharge.FKey;
                    break;
                
                case PayPlanLinkType.OrthoCase:
                    IsDynamic = true;
                    break;
                
                case PayPlanLinkType.None:
                default:
                    IsDynamic = false;
                    ProcNum = payPlanCharge.ProcNum;
                    if (IsAdjustment)
                    {
                        AdjNum = payPlanCharge.FKey;
                    }

                    break;
            }
        }
        else
        {
            AmountOriginal = (decimal) payPlanCharge.Interest;
            Interest = (decimal) payPlanCharge.Interest;
        }

        AmountEnd = AmountOriginal;
        ChargeType = payPlanCharge.ChargeType;
        if (ChargeType == PayPlanChargeType.Debit)
        {
            if (isPrincipal)
            {
                PayPlanDebitType = PayPlanDebitTypes.Principal;
            }
            else
            {
                PayPlanDebitType = PayPlanDebitTypes.Interest;
                ProcNum = 0;
                AdjNum = 0;
            }
        }

        ClinicNum = payPlanCharge.ClinicNum;
        Date = payPlanCharge.ChargeDate;
        Guarantor = payPlanCharge.Guarantor;
        IsOffset = payPlanCharge.IsOffset;
        PatNum = payPlanCharge.PatNum;
        PayPlanChargeNum = payPlanCharge.ChargeType == PayPlanChargeType.Credit ? 0 : payPlanCharge.PayPlanChargeNum;
        PayPlanNum = payPlanCharge.PayPlanNum;
        PriKey = payPlanCharge.PayPlanChargeNum;
        ProvNum = payPlanCharge.ProvNum;
        Tag = this;
    }

    public FauxAccountEntry(PayPlanProductionEntry payPlanProdEntry)
    {
        IsDynamic = true;
        AdjNum = payPlanProdEntry.GetAdjNum();
        AmountOriginal = CompareDecimal.IsZero(payPlanProdEntry.AmountOverride) ? payPlanProdEntry.AmountOriginal : payPlanProdEntry.AmountOverride;
        AmountEnd = AmountOriginal;
        ClinicNum = payPlanProdEntry.ClinicNum;
        ChargeType = PayPlanChargeType.Credit;
        Date = payPlanProdEntry.ProductionDate;
        Guarantor = 0;
        Interest = 0;
        PatNum = payPlanProdEntry.PatNum;
        PayPlanNum = payPlanProdEntry.LinkedCredit.PayPlanNum;
        PriKey = payPlanProdEntry.PriKey;
        Principal = AmountOriginal;
        PrincipalAdjusted = Principal;
        ProcNum = payPlanProdEntry.GetProcNum();
        ProvNum = payPlanProdEntry.ProvNum;
        IsAdjustment = payPlanProdEntry.LinkType == PayPlanLinkType.Adjustment;
        Tag = this;
    }

    public new FauxAccountEntry Copy()
    {
        var accountEntry = (FauxAccountEntry) MemberwiseClone();
        
        accountEntry.SplitCollection = [];
        
        foreach (var paySplit in SplitCollection)
        {
            accountEntry.SplitCollection.Add(paySplit.Copy());
        }

        return accountEntry;
    }
}