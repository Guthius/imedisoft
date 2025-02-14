using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OrthoCaseProcedureLinker
{
    public long PatNum;
    public readonly OrthoCase ActiveOrthoCase;
    public readonly List<OrthoProcLink> ListOrthoProcLinks = [];
    public readonly OrthoSchedule OrthoSchedule;

    private readonly Procedure _bandingProcedure;
    private readonly PayPlan _linkedPayPlan;
    private readonly List<PayPlanCharge> _listAllProcPayPlanCreditsForPat = [];
    private readonly List<PayPlanLink> _listAllProcPayPlanLinksForPat = [];
    private readonly OrthoPlanLink _orthoSchedulePlanLink;

    public OrthoCaseProcedureLinker()
    {
    }

    public static OrthoProcLink CreateOrUpdateOrthoProcLink(Procedure procedureOld, Procedure procedure)
    {
        OrthoProcLink orthoProcLink = null;
        if (procedureOld.ProcStatus != ProcStat.C && procedure.ProcStatus == ProcStat.C)
        {
            //If procedure's status has changed from not-Complete to Completed, create new link to return...
            var orthoCaseProcedureLinker = CreateOneForPatient(procedure.PatNum);
            orthoProcLink = orthoCaseProcedureLinker.LinkProcedureToActiveOrthoCaseIfNeeded(procedure);
        }

        if (orthoProcLink == null)
        {
            //...otherwise grab existing link.
            orthoProcLink = OrthoProcLinks.GetByProcNum(procedure.ProcNum);
            if (orthoProcLink != null && procedureOld.ProcDate != procedure.ProcDate)
            {
                //if existing link successfully found and the dates don't match: update the link before returning it.
                OrthoCases.UpdateDatesByLinkedProc(orthoProcLink, procedure);
            }
        }

        return orthoProcLink;
    }

    public static OrthoCaseProcedureLinker CreateOneForPatient(long patNum)
    {
        var listOrthoCaseProcedureLinkers = CreateManyForPatients([patNum]);
        //Creation methods below ensure that list is never null or empty. A patient can only have one active OrthoCase,
        //so the list will only contain that active OrthoCase or a blank OrthoCaseProcedureLinker.
        return listOrthoCaseProcedureLinkers[0];
    }

    public static List<OrthoCaseProcedureLinker> CreateManyForPatients(List<long> listPatNums)
    {
        List<OrthoCaseProcedureLinker> listOrthoCaseProcedureLinkers = [];
        if (!OrthoCases.HasOrthoCasesEnabled())
        {
            return CreateBlankLinkersForPatients(listPatNums);
        }

        //Get data needed for all patients to avoid querying in loops.
        var listActiveOrthoCases = OrthoCases.GetActiveForPats(listPatNums);
        if (listActiveOrthoCases.IsNullOrEmpty())
        {
            return CreateBlankLinkersForPatients(listPatNums);
        }

        //We don't need any other data for patients that don't have active OrthoCases.
        //Use this list of PatNums for further querying to filter on fewer PatNums.
        var listPatNumsForActiveOrthoCases = listActiveOrthoCases.Select(x => x.PatNum).ToList();
        var listOrthoCaseNums = listActiveOrthoCases.Select(x => x.OrthoCaseNum).ToList();
        var listOrthoProcLinks = OrthoProcLinks.GetManyByOrthoCases(listOrthoCaseNums);
        var listBandingProcNums = listOrthoProcLinks
            .Where(x => x.ProcLinkType == OrthoProcType.Banding)
            .Select(x => x.ProcNum)
            .ToList();
        var listBandingProcedures = Procedures.GetManyProc(listBandingProcNums, false);
        var listOrthoPlanLinks = OrthoPlanLinks.GetManyForOrthoCases(listOrthoCaseNums);
        List<long> listOrthoScheduleNums = listOrthoScheduleNums = listOrthoPlanLinks
            .Where(x => x.LinkType == OrthoPlanLinkType.OrthoSchedule)
            .Select(x => x.FKey)
            .ToList();
        var listOrthoSchedules = OrthoSchedules.GetMany(listOrthoScheduleNums);
        var listPayPlans = PayPlans.GetAllPatPayPlansForPats(listPatNumsForActiveOrthoCases);
        var listPayPlanNums = listPayPlans.Select(x => x.PayPlanNum).ToList();
        var listProcPayPlanLinks = PayPlanLinks.GetForPayPlansAndLinkType(listPayPlanNums, PayPlanLinkType.Procedure);
        var listProcPayPlanCredits = PayPlanCharges.GetAllProcCreditsForPayPlans(listPayPlanNums);
        //Create an OrthoCaseProcedureLinker for each patient.
        //Full list of PatNums is used because we need to create blanks for patients without an active OrthoCase.
        for (var i = 0; i < listPatNums.Count; i++)
        {
            var patNum = listPatNums[i];
            var OrthoCaseProcedureLinker = CreateOneForPatient(patNum, listActiveOrthoCases, listOrthoProcLinks, listOrthoPlanLinks,
                listBandingProcedures, listOrthoSchedules, listPayPlans, listProcPayPlanLinks, listProcPayPlanCredits);
            listOrthoCaseProcedureLinkers.Add(OrthoCaseProcedureLinker);
        }

        return listOrthoCaseProcedureLinkers;
    }

    public bool ShouldProcedureLinkToOrthoCase(Procedure procedure, string procCode = null)
    {
        if (!OrthoCases.HasOrthoCasesEnabled())
        {
            return false; //Won't link if the OrthoCase feature is disabled.
        }

        if (ActiveOrthoCase == null)
        {
            return false; //Won't link if patient has no active OrthoCase.
        }

        if (ListOrthoProcLinks.Any(x => x.ProcLinkType == OrthoProcType.Debond))
        {
            return false; //Won't link if active OrthoCase has a completed Debond. Indicates case is closed.
        }

        if (procedure.ProcStatus != ProcStat.C)
        {
            return false; //Won't link incomplete Procedures.
        }

        if (procCode.IsNullOrEmpty())
        {
            procCode = ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode;
        }

        if (!Procedures.IsAnOrthoCaseProcCode(procCode))
        {
            return false; //Won't link Procedures that don't have an OrthoCase Procedure code.
        }

        var doesCaseHaveBanding = (_bandingProcedure != null);
        var doesCaseHaveIncompleteBanding = (doesCaseHaveBanding && _bandingProcedure.ProcStatus != ProcStat.C);
        var isProcedureCompletedBanding = (doesCaseHaveBanding && procedure.ProcNum == _bandingProcedure.ProcNum);
        if (doesCaseHaveIncompleteBanding && !isProcedureCompletedBanding)
        {
            return false; //If OrthoCase has an incomplete banding Procedure linked, other procs can't be linked until it is completed.
        }

        var doesProcedureHaveBandingCode = OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoBandingCodes).Contains(procCode);
        var isProcedureWrongBanding = (doesCaseHaveBanding && !isProcedureCompletedBanding && doesProcedureHaveBandingCode);
        if (isProcedureWrongBanding)
        {
            return false; //Can't have more than one banding Procedure linked to case.
        }

        if (ActiveOrthoCase.IsTransfer && doesProcedureHaveBandingCode)
        {
            return false; //Can't link bandings to transfer cases.
        }

        var listVisitProcNums = ListOrthoProcLinks
            .Where(x => x.ProcLinkType == OrthoProcType.Visit)
            .Select(x => x.ProcNum)
            .ToList();
        if (listVisitProcNums.Contains(procedure.ProcNum))
        {
            return false; //Can't link the same visit more than once
        }

        return true; //Procedure and OrthoCase are eligible for linking.
    }

    public OrthoProcLink LinkProcedureToActiveOrthoCaseIfNeeded(Procedure procedure, bool doUpdateProcedure = false)
    {
        var procCode = ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode;
        if (!ShouldProcedureLinkToOrthoCase(procedure, procCode))
        {
            return null;
        }

        OrthoProcLink orthoProcLink = null;
        var procedureOld = procedure.Copy();
        var orthoCaseOld = ActiveOrthoCase.Copy();
        //If Procedure being set complete is the banding, it is already linked. We just need to update the BandingDate.
        if (_bandingProcedure != null && _bandingProcedure.ProcNum == procedure.ProcNum)
        {
            ActiveOrthoCase.BandingDate = procedure.ProcDate;
            OrthoCases.Update(ActiveOrthoCase, orthoCaseOld);
            orthoProcLink = ListOrthoProcLinks.FirstOrDefault(x => x.ProcNum == procedure.ProcNum);
        }
        //Link visit Procedure
        else if (OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoVisitCodes).Contains(procCode))
        {
            orthoProcLink = new OrthoProcLink
            {
                OrthoCaseNum = ActiveOrthoCase.OrthoCaseNum,
                ProcNum = procedure.ProcNum,
                ProcLinkType = OrthoProcType.Visit,
                SecUserNumEntry = Security.CurUser.UserNum
            };
            OrthoProcLinks.Insert(orthoProcLink);
            ListOrthoProcLinks.Add(orthoProcLink);
        }
        //Link Debond Procedure
        else if (OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoDebondCodes).Contains(procCode))
        {
            orthoProcLink = new OrthoProcLink
            {
                OrthoCaseNum = ActiveOrthoCase.OrthoCaseNum,
                ProcNum = procedure.ProcNum,
                ProcLinkType = OrthoProcType.Debond,
                SecUserNumEntry = Security.CurUser.UserNum
            };
            OrthoProcLinks.Insert(orthoProcLink);
            ListOrthoProcLinks.Add(orthoProcLink);
            ActiveOrthoCase.DebondDate = procedure.ProcDate;
            //Completing debond closes OrthoCase. This will also save our updated DebondDate
            OrthoCases.SetActiveState(ActiveOrthoCase, _orthoSchedulePlanLink, OrthoSchedule, isActive: false, orthoCaseOld);
        }
        else
        {
            return null;
        }

        SetProcFeeForLinkedProc(procedure);
        LinkOrthoCaseProcedureToPayPlanIfNeeded(procedure);
        if (doUpdateProcedure)
        {
            Procedures.Update(procedure, procedureOld, isProcLinkedToOrthoCase: true);
        }

        return orthoProcLink;
    }

    private OrthoCaseProcedureLinker(OrthoCase activeOrthoCase, List<OrthoProcLink> listOrthoProcLinks, OrthoSchedule orthoSchedule, Procedure bandingProcedure, PayPlan linkedPayPlan, List<PayPlanCharge> listAllProcPayPlanCreditsForPat, List<PayPlanLink> listAllProcPayPLanLinksForPat, OrthoPlanLink orthoSchedulePlanLink)
    {
        PatNum = activeOrthoCase.PatNum;
        ActiveOrthoCase = activeOrthoCase;
        ListOrthoProcLinks = listOrthoProcLinks;
        OrthoSchedule = orthoSchedule;
        _bandingProcedure = bandingProcedure;
        _linkedPayPlan = linkedPayPlan;
        _listAllProcPayPlanCreditsForPat = listAllProcPayPlanCreditsForPat;
        _listAllProcPayPlanLinksForPat = listAllProcPayPLanLinksForPat;
        _orthoSchedulePlanLink = orthoSchedulePlanLink;
    }

    private static List<OrthoCaseProcedureLinker> CreateBlankLinkersForPatients(List<long> listPatNums)
    {
        List<OrthoCaseProcedureLinker> listOrthoCaseProcedureLinkers = [];
        for (var i = 0; i < listPatNums.Count(); i++)
        {
            listOrthoCaseProcedureLinkers.Add(new OrthoCaseProcedureLinker {PatNum = listPatNums[i]});
        }

        return listOrthoCaseProcedureLinkers;
    }

    private static OrthoCaseProcedureLinker CreateOneForPatient(long patNum, List<OrthoCase> listActiveOrthoCases, List<OrthoProcLink> listOrthoProcLinks, List<OrthoPlanLink> listOrthoPlanLinks, List<Procedure> listBandingProcedures, List<OrthoSchedule> listOrthoSchedules, List<PayPlan> listPayPlans, List<PayPlanLink> listProcPayPlanLinks, List<PayPlanCharge> listProcPayPlanCharges)
    {
        var activeOrthoCase = listActiveOrthoCases.FirstOrDefault(x => x.PatNum == patNum);
        if (activeOrthoCase == null)
        {
            return new OrthoCaseProcedureLinker() {PatNum = patNum};
        }

        var orthoScheduleLink = listOrthoPlanLinks.FirstOrDefault(x => x.LinkType == OrthoPlanLinkType.OrthoSchedule && x.OrthoCaseNum == activeOrthoCase.OrthoCaseNum);
        if (orthoScheduleLink == null)
        {
            return new OrthoCaseProcedureLinker() {PatNum = patNum};
        }

        var orthoSchedule = listOrthoSchedules.FirstOrDefault(x => x.OrthoScheduleNum == orthoScheduleLink.FKey);
        if (orthoSchedule == null)
        {
            return new OrthoCaseProcedureLinker() {PatNum = patNum};
        }

        var listOrthoProcLinksForCase = listOrthoProcLinks.FindAll(x => x.OrthoCaseNum == activeOrthoCase.OrthoCaseNum);
        var listLinkedProcNums = listOrthoProcLinksForCase.Select(x => x.ProcNum).ToList();
        var bandingProcedure = listBandingProcedures.FirstOrDefault(x => listLinkedProcNums.Contains(x.ProcNum));
        var orthoPayPlanLink = listOrthoPlanLinks.FirstOrDefault(x => x.LinkType == OrthoPlanLinkType.PatPayPlan && x.OrthoCaseNum == activeOrthoCase.OrthoCaseNum);
        PayPlan linkedPayPlan = null;
        List<PayPlan> listPayPlansForPat = [];
        List<PayPlanLink> listProcPayPlanLinksForPat = [];
        List<PayPlanCharge> listProcPayPlanChargesForPat = [];
        if (orthoPayPlanLink != null)
        {
            listPayPlansForPat = listPayPlans.FindAll(x => x.PatNum == patNum);
            linkedPayPlan = listPayPlansForPat.FirstOrDefault(x => x.PayPlanNum == orthoPayPlanLink.FKey);
        }

        if (linkedPayPlan != null)
        {
            //If a payplan is not linked to the OrthoCase, we don't care about other payplan data. Skip to avoid needless searching.
            var listPayPlanNumsForPat = listPayPlansForPat.Select(x => x.PayPlanNum).ToList();
            listProcPayPlanLinksForPat = listProcPayPlanLinks.FindAll(x => listPayPlanNumsForPat.Contains(x.PayPlanNum));
            listProcPayPlanChargesForPat = listProcPayPlanCharges.FindAll(x => listPayPlanNumsForPat.Contains(x.PayPlanNum));
        }

        return new OrthoCaseProcedureLinker(activeOrthoCase, listOrthoProcLinksForCase, orthoSchedule, bandingProcedure,
            linkedPayPlan, listProcPayPlanChargesForPat, listProcPayPlanLinksForPat, orthoScheduleLink);
    }

    private void SetProcFeeForLinkedProc(Procedure procedure)
    {
        double procFee = 0;
        var orthoProcLink = ListOrthoProcLinks.FirstOrDefault(x => x.ProcNum == procedure.ProcNum);
        switch (orthoProcLink.ProcLinkType)
        {
            case OrthoProcType.Banding:
                procFee = OrthoSchedule.BandingAmount;
                break;
            case OrthoProcType.Debond:
                procFee = OrthoSchedule.DebondAmount;
                break;
            case OrthoProcType.Visit:
                var allVisitsAmount = Math.Round((ActiveOrthoCase.Fee - OrthoSchedule.BandingAmount - OrthoSchedule.DebondAmount) * 100) / 100;
                var plannedVisitCount = OrthoSchedules.CalculatePlannedVisitsCount(OrthoSchedule.BandingAmount, OrthoSchedule.DebondAmount
                    , OrthoSchedule.VisitAmount, ActiveOrthoCase.Fee);
                var listVisitProcLinks = ListOrthoProcLinks.FindAll(x => x.ProcLinkType == OrthoProcType.Visit);
                if (listVisitProcLinks.Count == plannedVisitCount)
                {
                    procFee = Math.Round((allVisitsAmount - OrthoSchedule.VisitAmount * (plannedVisitCount - 1)) * 100) / 100;
                }
                else if (listVisitProcLinks.Count < plannedVisitCount)
                {
                    procFee = OrthoSchedule.VisitAmount;
                }

                break;
        }

        procedure.ProcFee = procFee;
        procedure.BaseUnits = 0;
        procedure.UnitQty = 1;
    }

    private bool ShouldProcedureLinkToPayPlan(Procedure procedure)
    {
        if (_linkedPayPlan == null || _linkedPayPlan.IsClosed || _linkedPayPlan.IsLocked)
        {
            return false; //Can't link Procedure if no pay plan is linked to the active OrthoCase, or the pay plan is closed or locked.
        }

        if (_listAllProcPayPlanLinksForPat.Select(x => x.FKey).Contains(procedure.ProcNum))
        {
            return false; //Can't link Procedure if it is already linked to a dynamic pay plan.
        }

        if (_listAllProcPayPlanCreditsForPat.Select(x => x.ProcNum).Contains(procedure.ProcNum))
        {
            return false; //Can't link Procedure if it is already credited on a patient payment plan.
        }

        return true; //Procedure is eligible for linking to the OrthoCase's pay plan.
    }

    private void LinkOrthoCaseProcedureToPayPlanIfNeeded(Procedure procedure)
    {
        if (!ShouldProcedureLinkToPayPlan(procedure))
        {
            return;
        }

        var payPlanLink = new PayPlanLink
        {
            PayPlanNum = _linkedPayPlan.PayPlanNum,
            LinkType = PayPlanLinkType.Procedure,
            FKey = procedure.ProcNum,
        };
        PayPlanLinks.Insert(payPlanLink);
        _listAllProcPayPlanLinksForPat.Add(payPlanLink);
    }
}