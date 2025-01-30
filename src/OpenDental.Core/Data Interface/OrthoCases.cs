using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OrthoCases
{
    public static long Insert(OrthoCase orthoCase)
    {
        return OrthoCaseCrud.Insert(orthoCase);
    }

    public static void Delete(long orthoCaseNum, OrthoSchedule orthoSchedule = null, OrthoPlanLink orthoPlanLinkSchedule = null, List<OrthoProcLink> listOrthoProcLinks = null, OrthoPlanLink orthoPlanLinkPatPayPlan = null)
    {
        //Get associated objects if they were not passed in.
        if (orthoPlanLinkSchedule == null) orthoPlanLinkSchedule = OrthoPlanLinks.GetOneForOrthoCaseByType(orthoCaseNum, OrthoPlanLinkType.OrthoSchedule);
        if (orthoPlanLinkSchedule != null && orthoSchedule == null) orthoSchedule = OrthoSchedules.GetOne(orthoPlanLinkSchedule.FKey);
        if (listOrthoProcLinks == null) listOrthoProcLinks = OrthoProcLinks.GetManyByOrthoCase(orthoCaseNum);
        if (orthoPlanLinkPatPayPlan == null) orthoPlanLinkPatPayPlan = OrthoPlanLinks.GetOneForOrthoCaseByType(orthoCaseNum, OrthoPlanLinkType.PatPayPlan);
        //Check that all objects are actually associated by primary keys.
        var errorText = "Error: Failed to delete ortho case. Attempted to delete";
        if (orthoPlanLinkSchedule != null && orthoPlanLinkSchedule.OrthoCaseNum != orthoCaseNum)
            throw new ApplicationException(Lans.g(
                "OrthoCases", $"{errorText} an ortho plan link for an ortho schedule that does not belong to the ortho case."));
        if (orthoSchedule != null && orthoSchedule.OrthoScheduleNum != orthoPlanLinkSchedule.FKey) throw new ApplicationException(Lans.g("OrthoCases", $"{errorText} an ortho schedule that does not belong to the ortho case."));
        for (var i = 0; i < listOrthoProcLinks.Count; i++)
            if (listOrthoProcLinks[i].OrthoCaseNum != orthoCaseNum)
                throw new ApplicationException(Lans.g("OrthoCases", $"{errorText} an ortho procedure link that does not belong to the ortho case."));

        if (orthoPlanLinkPatPayPlan != null && orthoPlanLinkPatPayPlan.OrthoCaseNum != orthoCaseNum)
            throw new ApplicationException(Lans.g(
                "Orthocases", $"{errorText} an ortho plan link for a patient payment plan that does not belong to the ortho case."));
        //Delete objects
        OrthoCaseCrud.Delete(orthoCaseNum);
        OrthoScheduleCrud.Delete(orthoSchedule.OrthoScheduleNum);
        OrthoPlanLinkCrud.Delete(orthoPlanLinkSchedule.OrthoPlanLinkNum);
        OrthoProcLinks.DeleteMany(listOrthoProcLinks.Select(x => x.OrthoProcLinkNum).ToList());
        if (orthoPlanLinkPatPayPlan != null) OrthoPlanLinkCrud.Delete(orthoPlanLinkPatPayPlan.OrthoPlanLinkNum);
    }

    public static List<OrthoCase> GetPatientData(long patNum)
    {
        return Refresh(patNum);
    }

    public static OrthoCase GetOne(long orthoCaseNum)
    {
        return OrthoCaseCrud.SelectOne(orthoCaseNum);
    }

    public static List<OrthoCase> Refresh(long patNum)
    {
        var command = "SELECT * FROM orthocase WHERE orthocase.PatNum = " + SOut.Long(patNum);
        return OrthoCaseCrud.SelectMany(command);
    }

    public static List<OrthoCase> GetMany(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums.Count == 0) return new List<OrthoCase>();

        var command = $"SELECT * FROM orthocase WHERE orthocase.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoCaseCrud.SelectMany(command);
    }

    public static List<OrthoCase> GetActiveForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<OrthoCase>();

        var command = $"SELECT * FROM orthocase WHERE orthocase.IsActive={SOut.Bool(true)} AND orthocase.PatNum IN({string.Join(",", listPatNums)})";
        return OrthoCaseCrud.SelectMany(command);
    }

    public static void Update(OrthoCase orthoCaseNew, OrthoCase orthoCaseOld)
    {
        OrthoCaseCrud.Update(orthoCaseNew, orthoCaseOld);
    }

    public static List<OrthoCase> Activate(OrthoCase orthoCaseToActivate, long patNum)
    {
        var orthoPlanLinkSchedule = OrthoPlanLinks.GetOneForOrthoCaseByType(orthoCaseToActivate.OrthoCaseNum, OrthoPlanLinkType.OrthoSchedule);
        var orthoSchedule = OrthoSchedules.GetOne(orthoPlanLinkSchedule.FKey);
        SetActiveState(orthoCaseToActivate, orthoPlanLinkSchedule, orthoSchedule, true);
        DeactivateOthersForPat(orthoCaseToActivate.OrthoCaseNum, orthoSchedule.OrthoScheduleNum, patNum);
        return Refresh(patNum);
    }

    public static void DeactivateOthersForPat(long orthoCaseNumActive, long orthoScheduleNumActive, long patNum)
    {
        //Get all orthocase nums to deactivate.
        var listOrthoCaseNums = Refresh(patNum).Where(x => x.OrthoCaseNum != orthoCaseNumActive).Select(x => x.OrthoCaseNum).ToList();
        if (listOrthoCaseNums.Count <= 0) return;
        //Set all other orthocases inactive besides one being activated
        var command = $@"UPDATE orthocase SET orthocase.IsActive={SOut.Bool(false)}
				WHERE orthocase.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        Db.NonQ(command);
        //Set OrthoPlanLinks inactive
        command = $@"UPDATE orthoplanlink SET orthoplanlink.IsActive={SOut.Bool(false)}
				WHERE orthoplanlink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        Db.NonQ(command);
        //Get All OrthoPlanLinks to deactivate
        var listOrthoScheduleNums =
            OrthoPlanLinks.GetAllForOrthoCasesByType(listOrthoCaseNums, OrthoPlanLinkType.OrthoSchedule).Select(x => x.FKey).ToList();
        if (listOrthoScheduleNums.Count <= 0) return;
        //Set OrthoSchedules inactive
        command = $@"UPDATE orthoschedule SET orthoschedule.IsActive={SOut.Bool(false)}
				WHERE orthoschedule.OrthoScheduleNum IN({string.Join(",", listOrthoScheduleNums)})";
        Db.NonQ(command);
    }

    public static void SetActiveState(OrthoCase orthoCase, OrthoPlanLink orthoPlanLinkSchedule, OrthoSchedule orthoSchedule, bool isActive, OrthoCase orthoCaseOld = null)
    {
        if (orthoCaseOld == null) orthoCaseOld = orthoCase.Copy();
        var orthoScheduleOld = orthoSchedule.Copy();
        var oldScheduleOrthoPlanLink = orthoPlanLinkSchedule.Copy();
        orthoCase.IsActive = isActive;
        orthoSchedule.IsActive = isActive;
        orthoPlanLinkSchedule.IsActive = isActive;
        Update(orthoCase, orthoCaseOld);
        OrthoSchedules.Update(orthoSchedule, orthoScheduleOld);
        OrthoPlanLinks.Update(orthoPlanLinkSchedule, oldScheduleOrthoPlanLink);
    }

    public static void UpdateDatesByLinkedProc(OrthoProcLink orthoProcLink, Procedure procedure)
    {
        if (orthoProcLink.ProcLinkType == OrthoProcType.Visit) return;
        var orthoCase = GetOne(orthoProcLink.OrthoCaseNum);
        var orthoCaseOld = orthoCase.Copy();
        //Update banding date only if banding proc is complete or it is treatment planned and attached to an appointment.
        if ((orthoProcLink.ProcLinkType == OrthoProcType.Banding && procedure.ProcStatus == ProcStat.C) || (procedure.ProcStatus == ProcStat.TP && procedure.AptNum != 0))
            orthoCase.BandingDate = procedure.ProcDate;
        else if (orthoProcLink.ProcLinkType == OrthoProcType.Debond) orthoCase.DebondDate = procedure.ProcDate;
        Update(orthoCase, orthoCaseOld);
    }

    public static List<string> GetListProcTypeProcCodes(PrefName prefName)
    {
        return PrefC.GetString(prefName).Split(',').Select(x => x.Trim()).ToList();
    }

    public static bool HasOrthoCasesEnabled()
    {
        if (PrefC.GetStringSilent(PrefName.OrthoBandingCodes) != "") return true;
        if (PrefC.GetStringSilent(PrefName.OrthoVisitCodes) != "") return true;
        if (PrefC.GetStringSilent(PrefName.OrthoDebondCodes) != "") return true;
        return false;
    }

    public static void GetDataForAllProcLinks(ref List<OrthoProcLink> listOrthoProcLinksAll, ref Dictionary<long, OrthoProcLink> dictionaryOrthoProcLinks, ref Dictionary<long, OrthoCase> dictionaryOrthoCases, ref Dictionary<long, OrthoSchedule> dictionaryOrthoSchedules)
    {
        listOrthoProcLinksAll = OrthoProcLinks.GetAll();
        if (listOrthoProcLinksAll.Count > 0)
        {
            dictionaryOrthoProcLinks = listOrthoProcLinksAll.ToDictionary(x => x.ProcNum, x => x);
            dictionaryOrthoCases = GetMany(dictionaryOrthoProcLinks.Values.Select(x => x.OrthoCaseNum).Distinct().ToList()).ToDictionary(x => x.OrthoCaseNum, x => x);
            var dictionaryOrthoPlanLinksSchedule =
                OrthoPlanLinks.GetAllForOrthoCasesByType(dictionaryOrthoCases.Keys.ToList(), OrthoPlanLinkType.OrthoSchedule).ToDictionary(x => x.FKey, x => x);
            var listOrthoSchedules = OrthoSchedules.GetMany(dictionaryOrthoPlanLinksSchedule.Keys.ToList());
            dictionaryOrthoSchedules = listOrthoSchedules.ToDictionary(x => dictionaryOrthoPlanLinksSchedule[x.OrthoScheduleNum].OrthoCaseNum, x => x);
        }
    }

    public static void FillOrthoCaseObjectsForProc(long procNum, ref OrthoProcLink orthoProcLink, ref OrthoCase orthoCase, ref OrthoSchedule orthoSchedule, ref List<OrthoProcLink> listOrthoProcLinksForOrthoCase, Dictionary<long, OrthoProcLink> dictionaryOrthoProcLinksForProcList, Dictionary<long, OrthoCase> dictionaryOrthoCases, Dictionary<long, OrthoSchedule> dictionaryOrthoSchedules, List<OrthoProcLink> listOrthoProcLinksAll)
    {
        listOrthoProcLinksForOrthoCase = null;
        dictionaryOrthoProcLinksForProcList.TryGetValue(procNum, out orthoProcLink);
        //If proc is linked to an OrthoCase, get other OrthoCase data needed to update estimates.
        if (orthoProcLink != null)
        {
            var orthoCaseNum = orthoProcLink.OrthoCaseNum;
            dictionaryOrthoCases.TryGetValue(orthoCaseNum, out orthoCase);
            dictionaryOrthoSchedules.TryGetValue(orthoCaseNum, out orthoSchedule);
            listOrthoProcLinksForOrthoCase = listOrthoProcLinksAll.Where(x => x.OrthoCaseNum == orthoCaseNum).ToList();
        }
    }
}