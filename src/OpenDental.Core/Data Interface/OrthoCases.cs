using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoCases
{
    #region Insert

    ///<summary>Insert an OrthoCase into the database. Returns OrthoCaseNum.</summary>
    public static long Insert(OrthoCase orthoCase)
    {
        return OrthoCaseCrud.Insert(orthoCase);
    }

    #endregion Insert

    #region Delete

    /////<summary>Deletes an OrthoCase from the database, does not delete all items associated to the ortho case, call DeleteAllAssociated.</summary>
    //public static void Delete(long orthoCaseNum) {
    //	
    //	Crud.OrthoCaseCrud.Delete(orthoCaseNum);
    //}

    ///<summary>Throws exceptions. Deletes the OrthoCase and all items associated to the ortho case.</summary>
    public static void Delete(long orthoCaseNum, OrthoSchedule orthoSchedule = null, OrthoPlanLink orthoPlanLinkSchedule = null
        , List<OrthoProcLink> listOrthoProcLinks = null, OrthoPlanLink orthoPlanLinkPatPayPlan = null)
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

    #endregion Delete

    #region Get Methods

    public static List<OrthoCase> GetPatientData(long patNum)
    {
        return Refresh(patNum);
    }

    ///<summary>Gets one OrthoCase from the db.</summary>
    public static OrthoCase GetOne(long orthoCaseNum)
    {
        return OrthoCaseCrud.SelectOne(orthoCaseNum);
    }

    ///<summary>Gets all Ortho Cases for a patient.</summary>
    public static List<OrthoCase> Refresh(long patNum)
    {
        var command = "SELECT * FROM orthocase WHERE orthocase.PatNum = " + SOut.Long(patNum);
        return OrthoCaseCrud.SelectMany(command);
    }

    ///<summary>Gets all Ortho Cases for a list of OrthoCaseNums.</summary>
    public static List<OrthoCase> GetMany(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums.Count == 0) return new List<OrthoCase>();

        var command = $"SELECT * FROM orthocase WHERE orthocase.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoCaseCrud.SelectMany(command);
    }

    ///<summary>Gets all Ortho Cases from db for a list of PatNums.</summary>
    public static List<OrthoCase> GetManyForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<OrthoCase>();

        var command = $"SELECT * FROM orthocase WHERE orthocase.PatNum IN({string.Join(",", listPatNums)})";
        return OrthoCaseCrud.SelectMany(command);
    }

    ///<summary>Gets a Patients Active OrthoCase. Patient can only have one active OrthoCase so it is OK to return 1.</summary>
    public static OrthoCase GetActiveForPat(long patNum)
    {
        var command = $"SELECT * FROM orthocase WHERE orthocase.PatNum={SOut.Long(patNum)} AND orthocase.IsActive={SOut.Bool(true)}";
        return OrthoCaseCrud.SelectOne(command);
    }

    ///<summary>Gets a list of active ortho cases for several patients. There should only be 1 active ortho case per patient.</summary>
    public static List<OrthoCase> GetActiveForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<OrthoCase>();

        var command = $"SELECT * FROM orthocase WHERE orthocase.IsActive={SOut.Bool(true)} AND orthocase.PatNum IN({string.Join(",", listPatNums)})";
        return OrthoCaseCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Update

    ///<summary>Update only data that is different in newOrthoCase.</summary>
    public static void Update(OrthoCase orthoCaseNew, OrthoCase orthoCaseOld)
    {
        OrthoCaseCrud.Update(orthoCaseNew, orthoCaseOld);
    }

    /// <summary>
    ///     Activates an OrthoCase and its associated OrthoSchedule and OrthoPlanLink. Sets all other OrthoCases for Pat
    ///     inactive.
    ///     Returns the refreshed list of OrthoCases.
    /// </summary>
    public static List<OrthoCase> Activate(OrthoCase orthoCaseToActivate, long patNum)
    {
        var orthoPlanLinkSchedule = OrthoPlanLinks.GetOneForOrthoCaseByType(orthoCaseToActivate.OrthoCaseNum, OrthoPlanLinkType.OrthoSchedule);
        var orthoSchedule = OrthoSchedules.GetOne(orthoPlanLinkSchedule.FKey);
        SetActiveState(orthoCaseToActivate, orthoPlanLinkSchedule, orthoSchedule, true);
        DeactivateOthersForPat(orthoCaseToActivate.OrthoCaseNum, orthoSchedule.OrthoScheduleNum, patNum);
        return Refresh(patNum);
    }

    ///<summary>Set all objects related to orthocases for a patient inactive besides the ones passed in.</summary>
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

    /// <summary>
    ///     Update the IsActive property for the OrthoCase, OrthoSchedule, and OrthoPlanLink between them.
    ///     Old ortho case can be passed in if other fields need to be updated so that update doesn't have to be called twice.
    /// </summary>
    public static void SetActiveState(OrthoCase orthoCase, OrthoPlanLink orthoPlanLinkSchedule, OrthoSchedule orthoSchedule, bool isActive
        , OrthoCase orthoCaseOld = null)
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

    ///<summary>Sets the BandingDate or DebondDate for an OrthoCase.</summary>
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

    //
    //public static void Update(OrthoCase orthoCase) {
    //	
    //	Crud.OrthoCaseCrud.Update(orthoCase);
    //}

    #endregion Update

    #region Misc Methods

    /// <summary>
    ///     Parses comma delimited list of procCodes from the specified OrthoCase proc type preference
    ///     (OrthoBandingCodes, OrthoDebondCodes, OrthoVisitCodes). Returns as list of proc codes.
    /// </summary>
    public static List<string> GetListProcTypeProcCodes(PrefName prefName)
    {
        return PrefC.GetString(prefName).Split(',').Select(x => x.Trim()).ToList();
    }

    ///<summary>Returns true if any of the preferences: OrthoBandingCodes, OrthoDebondCodes, OrthoVisitCodes aren't blank.</summary>
    public static bool HasOrthoCasesEnabled()
    {
        if (PrefC.GetStringSilent(PrefName.OrthoBandingCodes) != "") return true;
        if (PrefC.GetStringSilent(PrefName.OrthoVisitCodes) != "") return true;
        if (PrefC.GetStringSilent(PrefName.OrthoDebondCodes) != "") return true;
        return false;
    }

    /// <summary>
    ///     This fills a list of all OrthoProcLinks, a dictionary of OrthoProcLinks, a dictionary of OrthoCases associated to
    ///     these OrthoProcLinks,
    ///     and a dictionary of OrthoSchedules for the OrthoCases.
    /// </summary>
    public static void GetDataForAllProcLinks(ref List<OrthoProcLink> listOrthoProcLinksAll,
        ref Dictionary<long, OrthoProcLink> dictionaryOrthoProcLinks, ref Dictionary<long, OrthoCase> dictionaryOrthoCases,
        ref Dictionary<long, OrthoSchedule> dictionaryOrthoSchedules)
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

    /// <summary>
    ///     Fills ref parameters for an orthoProcLink, orthoCase, orthoSchedule, and list of orthoProcLinks for the orthoCase.
    ///     These objects are used in several places to call Procedures.ComputeEstimates()
    /// </summary>
    public static void FillOrthoCaseObjectsForProc(long procNum, ref OrthoProcLink orthoProcLink, ref OrthoCase orthoCase, ref OrthoSchedule orthoSchedule,
        ref List<OrthoProcLink> listOrthoProcLinksForOrthoCase, Dictionary<long, OrthoProcLink> dictionaryOrthoProcLinksForProcList,
        Dictionary<long, OrthoCase> dictionaryOrthoCases, Dictionary<long, OrthoSchedule> dictionaryOrthoSchedules, List<OrthoProcLink> listOrthoProcLinksAll)
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

    #endregion Misc Methods
}