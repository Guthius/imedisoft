using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ClaimEdit
{
    public static LoadData GetLoadData(Patient pat, Family fam, Claim claim)
    {
        LoadData data = new LoadData();
        data.ListPatPlans = PatPlans.Refresh(pat.PatNum);
        data.ListInsSubs = InsSubs.RefreshForFam(fam);
        data.ListInsPlans = InsPlans.RefreshForSubList(data.ListInsSubs);
        data.ListClaimProcs = ClaimProcs.Refresh(pat.PatNum);
        data.ListProcs = Procedures.Refresh(pat.PatNum);
        data.ListClaimValCodes = ClaimValCodeLogs.GetForClaim(claim.ClaimNum);
        data.ClaimCondCodeLogCur = ClaimCondCodeLogs.GetByClaimNum(claim.ClaimNum);
        data.TablePayments = ClaimPayments.GetForClaim(claim.ClaimNum);
        data.TablePayments.TableName = "ClaimPayments";
        data.ListToothInitials = ToothInitials.GetPatientData(pat.PatNum);
        data.ListCustomStatusEntries = ClaimTrackings.RefreshForClaim(ClaimTrackingType.StatusHistory, claim.ClaimNum);
        data.DoShowPatResp = PrefC.GetBool(PrefName.ClaimEditShowPatResponsibility);
        return data;
    }

    public static UpdateData UpdateClaim(Claim claimCur, List<ClaimValCodeLog> listClaimValCodes, ClaimCondCodeLog claimCondCodeLog, List<Procedure> listProcsToUpdatePlaceOfService, Patient pat, bool doMakeSecLog, EnumPermType permissionToLog)
    {
        UpdateData data = new UpdateData();
        Claims.Update(claimCur);
        if (listClaimValCodes != null)
        {
            ClaimValCodeLogs.UpdateList(listClaimValCodes);
        }

        if (claimCondCodeLog != null)
        {
            if (claimCondCodeLog.IsNew)
            {
                ClaimCondCodeLogs.Insert(claimCondCodeLog);
            }
            else
            {
                ClaimCondCodeLogs.Update(claimCondCodeLog);
            }
        }

        foreach (Procedure proc in listProcsToUpdatePlaceOfService)
        {
            Procedure oldProc = proc.Copy();
            proc.PlaceService = claimCur.PlaceService;
            Procedures.Update(proc, oldProc);
        }

        if (doMakeSecLog)
        {
            SecurityLogs.MakeLogEntry(permissionToLog, claimCur.PatNum,
                pat.GetNameLF() + ", Date of service: " + claimCur.DateService.ToShortDateString());
        }

        data.ListSendQueueItems = Claims.GetQueueList(claimCur.ClaimNum, claimCur.ClinicNum, 0);
        return data;
    }
        
    public class UpdateData
    {
        public ClaimSendQueueItem[] ListSendQueueItems;
    }
        
    public class LoadData
    {
        public List<PatPlan> ListPatPlans;
        public List<InsSub> ListInsSubs;
        public List<InsPlan> ListInsPlans;
        public List<ClaimProc> ListClaimProcs;
        public List<Procedure> ListProcs;
        public List<ClaimValCodeLog> ListClaimValCodes;
        public ClaimCondCodeLog ClaimCondCodeLogCur;
        public DataTable TablePayments;
        public List<ToothInitial> ListToothInitials;
        public List<ClaimTracking> ListCustomStatusEntries;
        public bool DoShowPatResp;
    }
}