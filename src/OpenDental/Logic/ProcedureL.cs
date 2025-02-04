using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Logic;

public class ProcedureL
{
    public static void SetCompleteInAppt(Appointment appointment, List<InsPlan> listInsPlans, List<PatPlan> listPatPlans, Patient patient, List<InsSub> listInsSub, bool removeCompletedProcs)
    {
        var procedures = Procedures.SetCompleteInAppt(appointment, listInsPlans, listPatPlans, patient, listInsSub, removeCompletedProcs);
        var procCodes = procedures.Select(x => ProcedureCodes.GetStringProcCode(x.CodeNum)).ToList();

        AutomationL.Trigger(EnumAutomationTrigger.ProcedureComplete, procCodes, appointment.PatNum);

        Procedures.AfterProcsSetComplete(procedures);
    }

    public static string ProcsContainDuplicates(List<Procedure> procedures)
    {
        var hasLongDCodes = false;

        var hL7Def = HL7Defs.GetOneDeepEnabled();
        if (hL7Def is not null)
        {
            hasLongDCodes = hL7Def.HasLongDCodes;
        }

        var info = "";
        var checkedProcedures = new List<Procedure>();
        foreach (var proc in procedures)
        {
            var procedureCode = ProcedureCodes.GetProcCode(proc.CodeNum);

            var procCodeStr = procedureCode.ProcCode;
            if (procCodeStr.Length > 5 && procCodeStr.StartsWith("D") && !hasLongDCodes)
            {
                procCodeStr = procCodeStr.Substring(0, 5);
            }

            foreach (var checkedProcedure in checkedProcedures)
            {
                var procedureCodeDup = ProcedureCodes.GetProcCode(checkedProcedure.CodeNum);
                var procCodeDupStr = procedureCodeDup.ProcCode;
                if (procCodeDupStr.Length > 5 && procCodeDupStr.StartsWith("D") && !hasLongDCodes)
                {
                    procCodeDupStr = procCodeDupStr.Substring(0, 5);
                }

                if (procCodeDupStr != procCodeStr)
                {
                    continue;
                }

                if (checkedProcedure.ToothNum != proc.ToothNum)
                {
                    continue;
                }

                if (checkedProcedure.ToothRange != proc.ToothRange)
                {
                    continue;
                }

                if (checkedProcedure.ProcFee != proc.ProcFee)
                {
                    continue;
                }

                if (checkedProcedure.Surf != proc.Surf)
                {
                    continue;
                }

                if (info != "")
                {
                    info += ", ";
                }

                info += procCodeDupStr;
            }

            checkedProcedures.Add(proc);
        }

        if (info != "")
        {
            info = "Duplicate procedures: " + info;
        }

        return info;
    }

    public static bool DoRemoveCompletedProcs(Appointment appointment, List<Procedure> proceduresForAppt, bool checkForAllProcCompl = false)
    {
        if (proceduresForAppt.Count == 0)
        {
            return false;
        }

        if (checkForAllProcCompl && (appointment.AptStatus != ApptStatus.Complete || proceduresForAppt.All(x => x.ProcStatus == ProcStat.C)))
        {
            return false;
        }

        var proceduresCompletedWithDifferentProv = new List<Procedure>();
        foreach (var procedure in proceduresForAppt)
        {
            if (procedure.ProcStatus != ProcStat.C || procedure.AptNum != appointment.AptNum)
            {
                continue;
            }

            var procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum);
            var provNum = Procedures.GetProvNumFromAppointment(appointment, procedureCode);
            if (provNum != procedure.ProvNum)
            {
                proceduresCompletedWithDifferentProv.Add(procedure);
            }
        }

        if (proceduresCompletedWithDifferentProv.Count == 0)
        {
            return true;
        }

        var procNums = proceduresCompletedWithDifferentProv.Select(x => x.ProcNum).ToList();

        if (PrefC.GetBool(PrefName.ProcProvChangesClaimProcWithClaim))
        {
            var claimProcs = ClaimProcs.RefreshForProcs(procNums);
            if (claimProcs.Any(x => x.Status is ClaimProcStatus.Received or ClaimProcStatus.Supplemental or ClaimProcStatus.CapClaim))
            {
                MsgBox.Show("Procedures",
                    "The appointment provider does not match the provider on at least one procedure that is attached to a claim.\r\n" +
                    "The provider on the procedure(s) cannot be changed.");

                return true;
            }
        }

        var paySplits = PaySplits.GetPaySplitsFromProcs(procNums);
        if (paySplits.Count > 0)
        {
            MsgBox.Show("Procedures",
                "The appointment provider does not match the provider on at least one completed procedure.\r\n" +
                "The procedure provider cannot be changed to match the appointment provider because the paysplit provider would no longer match. " +
                "Any change to the provider on the completed procedure(s) or paysplit(s) will have to be made manually.");

            return true;
        }

        foreach (var procedure in proceduresCompletedWithDifferentProv)
        {
            var permissions = GroupPermissions.SwitchExistingPermissionIfNeeded(EnumPermType.ProcCompleteEdit, procedure);
            
            var dateTimeForPerm = Procedures.GetDateForPermCheck(procedure);
            if (Security.IsGlobalDateLock(permissions, dateTimeForPerm))
            {
                return true;
            }

            if (Security.IsAuthorized(permissions, dateTimeForPerm, suppressMessage: true, suppressLockDateMessage: true))
            {
                continue;
            }

            ODMessageBox.Show(
                "The appointment provider does not match the provider on at least one completed procedure.\r\n" +
                "Not authorized for: " + GroupPermissions.GetDesc(permissions) + "\r\n" +
                "Any change to the provider on the completed procedure(s) will have to be made manually.");

            return true;
        }

        return !MsgBox.Show("Procedures", MsgBoxButtons.YesNo,
            "The appointment provider does not match the provider on at least one completed procedure.\r\n" +
            "Change the provider on the completed procedure(s) to match the provider on the appointment?");
    }

    public static bool IsProcCompleteAttachedToClaim(Procedure procedure, List<ClaimProc> claimProcs, bool silent = false)
    {
        var claimProcsFiltered = claimProcs.FindAll(x => x.ProcNum == procedure.ProcNum);

        if (procedure.ProcStatus != ProcStat.C || !Procedures.IsAttachedToClaim(procedure, claimProcsFiltered, isPreauthIncluded: false))
        {
            return false;
        }

        if (!silent)
        {
            MsgBox.Show("Procedures",
                "This is a completed procedure that is attached to a claim. " +
                "You must remove the procedure from the claim or delete the claim before editing the status.");
        }

        return true;
    }

    public static bool ValidateProvider(List<ClaimProc> claimProcs, long provNumSelected, long provNumForProc)
    {
        return Procedures.ValidateProvider(claimProcs, provNumSelected, provNumForProc, MsgBox.Show);
    }

    public static bool CheckPermissionsAndGlobalLockDate(Procedure procedureOld, Procedure procedureNew, DateTime dateTimeProc, double procFeeOverride = double.MinValue, bool suppressMessage = false)
    {
        return Procedures.CheckPermissionsAndGlobalLockDate(procedureOld, procedureNew, dateTimeProc, Security.CurUser, procFeeOverride, msg =>
        {
            if (!suppressMessage)
            {
                MsgBox.Show(msg);
            }
        });
    }
}