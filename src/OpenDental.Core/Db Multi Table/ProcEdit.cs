using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcEdit
{
    public static LoadData GetLoadData(Procedure proc, Patient pat, Family fam)
    {
        var data = new LoadData
        {
            ListPatPlans = PatPlans.Refresh(pat.PatNum)
        };
        
        if (!PatPlans.IsPatPlanListValid(data.ListPatPlans))
        {
            data.ListPatPlans = PatPlans.Refresh(pat.PatNum);
        }

        data.ListInsSubs = InsSubs.RefreshForFam(fam);
        data.ListInsPlans = InsPlans.RefreshForSubList(data.ListInsSubs);
        data.ListClaims = Claims.Refresh(pat.PatNum);
        data.ListClaimProcsForProc = ClaimProcs.RefreshForProc(proc.ProcNum);
        data.ListBenefits = Benefits.Refresh(data.ListPatPlans, data.ListInsSubs);
        data.ListRefAttaches = RefAttaches.RefreshFiltered(proc.PatNum, false, proc.ProcNum);
        data.ListPaySplitsForProc = PaySplits.GetForProcs(ListTools.FromSingle(proc.ProcNum));
        data.ListPaymentsForProc = Payments.GetPayments(data.ListPaySplitsForProc.Select(x => x.PayNum).ToList());
        data.ArrAdjustments = Adjustments.Refresh(proc.PatNum);
        data.OrthoProcedureLink = OrthoProcLinks.GetByProcNum(proc.ProcNum);
        return data;
    }
    
    public class LoadData
    {
        public List<InsSub> ListInsSubs;
        public List<InsPlan> ListInsPlans;
        public List<Claim> ListClaims;
        public List<ClaimProc> ListClaimProcsForProc;
        public List<PatPlan> ListPatPlans;
        public List<Benefit> ListBenefits;
        public List<RefAttach> ListRefAttaches;
        public List<PaySplit> ListPaySplitsForProc;
        public List<Payment> ListPaymentsForProc;
        public Adjustment[] ArrAdjustments;
        public OrthoProcLink OrthoProcedureLink;
    }
}