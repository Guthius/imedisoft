using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcFeeHelper(long patNum)
{
    public Patient Pat;
    public List<Fee> ListFees;
    public List<PatPlan> ListPatPlans;
    public List<InsSub> ListInsSubs;
    public List<InsPlan> ListInsPlans;
    public List<Benefit> ListBenefitsPrimary;

    public ProcFeeHelper(Patient pat, List<Fee> listFees, List<PatPlan> listPatPlans, List<InsSub> listInsSubs, List<InsPlan> listInsPlans, List<Benefit> listBenefits) : this(pat.PatNum)
    {
        Pat = pat;
        ListFees = listFees;
        ListPatPlans = listPatPlans;
        ListInsSubs = listInsSubs;
        ListInsPlans = listInsPlans;
        ListBenefitsPrimary = listBenefits;
    }

    public void FillData()
    {
        if (Pat != null && ListPatPlans != null && ListInsSubs != null && ListInsPlans != null && ListBenefitsPrimary != null)
        {
            return;
        }

        var procFeeHelper = GetData(patNum, this);
        
        Pat = procFeeHelper.Pat;
        ListPatPlans = procFeeHelper.ListPatPlans;
        ListInsSubs = procFeeHelper.ListInsSubs;
        ListInsPlans = procFeeHelper.ListInsPlans;
        ListBenefitsPrimary = procFeeHelper.ListBenefitsPrimary;
    }

    public static ProcFeeHelper GetData(long patNum, ProcFeeHelper procFeeHelper)
    {
        procFeeHelper ??= new ProcFeeHelper(patNum);
        procFeeHelper.Pat ??= Patients.GetPat(patNum);
        procFeeHelper.ListPatPlans ??= PatPlans.GetPatPlansForPat(patNum);
        procFeeHelper.ListInsSubs ??= InsSubs.GetMany(procFeeHelper.ListPatPlans.Select(x => x.InsSubNum).ToList());
        procFeeHelper.ListInsPlans ??= InsPlans.GetPlans(procFeeHelper.ListInsSubs.Select(x => x.PlanNum).ToList());
        
        if (procFeeHelper.ListPatPlans.Count > 0)
        {
            var priPatPlan = procFeeHelper.ListPatPlans[0];
            var priInsSub = InsSubs.GetSub(priPatPlan.InsSubNum, procFeeHelper.ListInsSubs);
            var priInsPlan = InsPlans.GetPlan(priInsSub.PlanNum, procFeeHelper.ListInsPlans);
            
            procFeeHelper.ListBenefitsPrimary ??= Benefits.GetForPlanOrPatPlan(priInsPlan.PlanNum, priPatPlan.PatPlanNum);
        }
        else
        {
            procFeeHelper.ListBenefitsPrimary = [];
        }

        return procFeeHelper;
    }
}