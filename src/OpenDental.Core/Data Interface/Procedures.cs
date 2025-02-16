using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;

namespace OpenDentBusiness;

public class Procedures
{
    public const string AutoNotePromptRegex = @"\[Prompt:""[a-zA-Z_0-9 ]+""\]";

    private static Queue<DataTable> _queueDataTables;

    private static readonly object _lockObjQueueThread = new();

    private static bool _isQueueDone;

    private const int ROWS_BATCH_MAX_SIZE = 10000;
    private const int UPDATE_PROCNUM_IN_MAX_SIZE = 1000;

    private static ODThread _odThreadQueueData;
    
    private static List<long> _listProcNumsMaxForGroups;

    private static int _totCount;
    
    public static List<Procedure> GetForPlanned(long patNum, long plannedAptNum)
    {
        if (patNum == 0 || plannedAptNum == 0) return [];
        var command = "SELECT * FROM procedurelog WHERE PatNum=" + patNum
                                                                 + " AND PlannedAptNum=" + plannedAptNum
                                                                 + " AND ProcStatus !=" + SOut.Int((int) ProcStat.D); //don't include deleted
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetAllTp(long clinicNum = -1)
    {
        var command = "SELECT * FROM procedurelog WHERE procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.TP);
        if (clinicNum >= 0) command += " AND procedurelog.ClinicNum=" + clinicNum;
        return DataCore.GetList(command, ProcedureCrud.RowToObj);
    }

    public static List<Procedure> GetAllForPatsAndStatuses(List<long> listPatNums, params ProcStat[] arrayProcStats)
    {
        if (listPatNums.IsNullOrEmpty() || arrayProcStats.IsNullOrEmpty()) return [];

        var command = "SELECT * FROM procedurelog "
                      + "WHERE procedurelog.PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND procedurelog.ProcStatus IN (" + string.Join(",", arrayProcStats.Select(x => SOut.Enum(x))) + ") ";
        return DataCore.GetList(command, ProcedureCrud.RowToObj);
    }

    public static List<Procedure> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM procedurelog WHERE PatNum=" + patNum
                                                                 + " AND ProcStatus !=" + SOut.Int((int) ProcStat.D) //don't include deleted
                                                                 + " ORDER BY ProcDate";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> Refresh(long patNum)
    {
        var command = "SELECT * FROM procedurelog WHERE PatNum=" + patNum
                                                                 + " AND ProcStatus !=" + SOut.Int((int) ProcStat.D) //don't include deleted
                                                                 + " ORDER BY ProcDate";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> RefreshForStatus(long patNum, ProcStat procStatus, bool isNotOnApt = true)
    {
        var command = "SELECT * FROM procedurelog WHERE PatNum=" + patNum + " "
                      + "AND ProcStatus =" + SOut.Int((int) procStatus) + " "
                      + (isNotOnApt ? "AND AptNum=0" : "");
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> RefreshForProcCodeNums(long patNum, List<long> listProcCodeNums)
    {
        if (listProcCodeNums == null || listProcCodeNums.Count == 0) return [];
        var command = "SELECT * FROM procedurelog WHERE PatNum=" + patNum + " " +
                      "AND CodeNum IN (" + string.Join(",", listProcCodeNums) + ") " +
                      "AND ProcStatus !=" + SOut.Int((int) ProcStat.D) + " " + //don't include deleted
                      "ORDER BY ProcDate";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetCompleteForProcCodeNum(List<long> listPatNums, long codeNum)
    {
        if (listPatNums == null || listPatNums.Count == 0) return [];

        var command = "SELECT * FROM procedurelog WHERE PatNum IN" + " (" + string.Join(",", listPatNums) + ") " +
                      "AND CodeNum=" + codeNum + " " +
                      "AND ProcStatus =" + SOut.Int((int) ProcStat.C) + " " + //include Complete
                      "ORDER BY ProcDate";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetCompleteForPats(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return [];
        return GetProceduresForStatusHelper(ProcStat.C, listPatNums);
    }

    public static List<Procedure> GetTpForPats(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count < 1) return [];
        return GetProceduresForStatusHelper(ProcStat.TP, listPatNums);
    }

    private static List<Procedure> GetProceduresForStatusHelper(ProcStat stat, List<long> listPatNums)
    {
        //No RemotingRole check; private method
        var strQueryFilter = $@"procedurelog.PatNum IN ({string.Join(",", listPatNums)})";
        var command = $"SELECT PayPlanNum FROM payplan WHERE Guarantor IN ({string.Join(",", listPatNums)})";
        var listGuarPayPlanNums = Db.GetListLong(command);
        if (!listGuarPayPlanNums.IsNullOrEmpty())
        {
            //Get all procedures from both patient and dynamic payment plans where the given patients are the guarantor.
            //Purposefully includes payment plans that are inside or outside of the family.
            command = $@"SELECT ProcNum FROM payplancharge 
					WHERE PayPlanNum IN ({string.Join(",", listGuarPayPlanNums)}) 
					AND ProcNum!=0
						UNION 
					SELECT FKey ProcNum FROM payplanlink 
					WHERE PayPlanNum IN ({string.Join(",", listGuarPayPlanNums)}) 
					AND LinkType={SOut.Int((int) PayPlanLinkType.Procedure)}
					AND FKey!=0";
            var listProcNums = Db.GetListLong(command);
            if (!listProcNums.IsNullOrEmpty()) strQueryFilter += $" OR procedurelog.ProcNum IN({string.Join(",", listProcNums)})";
        }

        command = $@" SELECT procedurelog.*
				FROM procedurelog
				WHERE ProcStatus = {SOut.Int((int) stat)}
				AND ({strQueryFilter})
				ORDER BY ProcDate ";
        var listProcedures = DataCore.GetList(command, ProcedureCrud.RowToObj);
        listProcedures.Sort(ProcedureLogic.CompareProcedures);
        return listProcedures;
    }

    public static List<Procedure> GetForProcTPs(List<ProcTP> listProcTP, params ProcStat[] procStats)
    {
        if (listProcTP.Count == 0) return [];
        var command = "SELECT ProcNum,CodeNum,AptNum,ProcDate,ClinicNum,ProcStatus,ProcFee,BaseUnits,UnitQty FROM procedurelog "
                      + "WHERE procedurelog.ProcNum IN (" + string.Join(",", listProcTP.Select(x => x.ProcNumOrig).ToList()) + ") "
                      + "AND procedurelog.ProcStatus IN (" + string.Join(",", procStats.Select(x => (int) x)) + ")";
        var table = DataCore.GetTable(command);
        var listProcs = new List<Procedure>();
        foreach (DataRow row in table.Rows)
        {
            var proc = new Procedure();
            proc.ProcNum = SIn.Long(row["ProcNum"].ToString());
            proc.CodeNum = SIn.Long(row["CodeNum"].ToString());
            proc.AptNum = SIn.Long(row["AptNum"].ToString());
            proc.ProcDate = SIn.Date(row["ProcDate"].ToString());
            proc.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            proc.ProcStatus = (ProcStat) SIn.Int(row["ProcStatus"].ToString());
            proc.ProcFee = SIn.Double(row["ProcFee"].ToString());
            proc.BaseUnits = SIn.Int(row["BaseUnits"].ToString());
            proc.UnitQty = SIn.Int(row["UnitQty"].ToString());
            listProcs.Add(proc);
        }

        return listProcs;
    }

    public static List<RpUnearnedIncome.UnearnedProc> GetRemainingProcsForFamilies(List<long> listGuarantorNums)
    {
        if (listGuarantorNums.Count == 0) return [];

        var listAllFamilyPatNums = Patients.GetAllFamilyPatNums(listGuarantorNums);
        /*given a list of families, get all procedures with a remaining pat port for those families.*/
        var command = @"
			SELECT patient.Guarantor,
			(procedurelog.ProcFee *(procedurelog.BaseUnits + procedurelog.UnitQty)) + COALESCE(adj.AdjAmt,0)
				- (COALESCE(cp.WriteOff,0) + COALESCE(cp.InsPay,0) + COALESCE(cp.InsEst,0) + COALESCE(patpay.Amt,0)) RemAmt,
			procedurelog.* 
			FROM procedurelog
			LEFT JOIN (
				SELECT claimproc.ProcNum, 
				SUM(CASE WHEN claimproc.Status IN ("
                      + SOut.Int((int) ClaimProcStatus.NotReceived) + ","
                      + SOut.Int((int) ClaimProcStatus.Received) + ","
                      + SOut.Int((int) ClaimProcStatus.Supplemental) + ","
                      + SOut.Int((int) ClaimProcStatus.CapComplete)
                      + @") THEN claimproc.WriteOff END) AS WriteOff,
				SUM(CASE WHEN claimproc.Status IN ("
                      + SOut.Int((int) ClaimProcStatus.Received) + ","
                      + SOut.Int((int) ClaimProcStatus.Supplemental)
                      + @") THEN claimproc.InsPayAmt END) AS InsPay,
				SUM(CASE WHEN claimproc.Status = " + SOut.Int((int) ClaimProcStatus.NotReceived) + @" THEN claimproc.InsPayEst END) AS InsEst
				FROM claimproc
				WHERE claimproc.Status IN ("
                      + SOut.Int((int) ClaimProcStatus.NotReceived) + ","
                      + SOut.Int((int) ClaimProcStatus.Received) + ","
                      + SOut.Int((int) ClaimProcStatus.Supplemental) + ","
                      + SOut.Int((int) ClaimProcStatus.CapComplete)
                      + @")
				AND claimproc.PatNum IN (" + string.Join(",", listAllFamilyPatNums.Select(x => x)) + @")
				AND claimproc.ProcNum != 0
				GROUP BY claimproc.ProcNum
			)cp ON cp.ProcNum = procedurelog.ProcNum
			LEFT JOIN (
				SELECT adjustment.ProcNum, SUM(adjustment.AdjAmt) AdjAmt
				FROM adjustment
				WHERE adjustment.PatNum IN (" + string.Join(",", listAllFamilyPatNums.Select(x => x)) + @")
				AND adjustment.ProcNum != 0
				GROUP BY adjustment.ProcNum
			)adj ON adj.ProcNum = procedurelog.ProcNum
			LEFT JOIN (
				SELECT paysplit.ProcNum, SUM(paysplit.SplitAmt) Amt
				FROM paysplit
				WHERE paysplit.PatNum IN (" + string.Join(",", listAllFamilyPatNums.Select(x => x)) + @")
				AND paysplit.ProcNum != 0
				GROUP BY paysplit.ProcNum
			)patpay ON patpay.ProcNum = procedurelog.ProcNum
			INNER JOIN patient ON patient.PatNum = procedurelog.PatNum
			WHERE procedurelog.ProcStatus = " + SOut.Int((int) ProcStat.C) + @"
			AND procedurelog.PatNum IN (" + string.Join(",", listAllFamilyPatNums.Select(x => x)) + @")
			AND (procedurelog.ProcFee *(procedurelog.BaseUnits + procedurelog.UnitQty)) + COALESCE(adj.AdjAmt,0)
				- (COALESCE(cp.WriteOff,0) + COALESCE(cp.InsPay,0) + COALESCE(cp.InsEst,0) + COALESCE(patpay.Amt,0)) > 0.005";
        var table = DataCore.GetTable(command);
        var retVal = new List<RpUnearnedIncome.UnearnedProc>();
        var listProcs = ProcedureCrud.TableToList(table);
        for (var i = 0; i < listProcs.Count; i++)
            retVal.Add(new RpUnearnedIncome.UnearnedProc(listProcs[i], SIn.Long(table.Rows[i]["Guarantor"].ToString())
                , SIn.Decimal(table.Rows[i]["RemAmt"].ToString())));
        return retVal;
    }

    public static Procedure GetOneProc(long procNum, bool includeNote)
    {
        //Doing this before remoting role check because Middle Tier can't serialize a Procedure with ProcStatus=0.
        if (procNum == 0) return new Procedure();

        var proc = ProcedureCrud.SelectOne(procNum);
        if (proc == null) return new Procedure(); //This will throw if Middle Tier. Haven't come up with a good solution yet.
        if (!includeNote) return proc;
        var command = "SELECT * FROM procnote WHERE ProcNum=" + procNum + " ORDER BY EntryDateTime DESC";
        DbHelper.LimitOrderBy(command, 1);
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return proc;
        proc.UserNum = SIn.Long(table.Rows[0]["UserNum"].ToString());
        proc.Note = SIn.String(table.Rows[0]["Note"].ToString());
        proc.SigIsTopaz = SIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
        proc.Signature = SIn.String(table.Rows[0]["Signature"].ToString());
        return proc;
    }

    public static List<Procedure> GetManyProc(List<long> listProcNums, bool includeNote)
    {
        if (listProcNums == null || listProcNums.Count == 0) return [];

        var command = "";
        if (!includeNote)
        {
            command = "SELECT * FROM procedurelog WHERE ProcNum IN (" + string.Join(",", listProcNums) + ")";
            return ProcedureCrud.SelectMany(command);
        }

        command = "SELECT procedurelog.*"
                  + ",procnoterow.UserNum NoteUserNum,procnoterow.Note NoteNote,procnoterow.SigIsTopaz NoteSigIsTopaz,procnoterow.Signature NoteSignature "
                  + "FROM procedurelog "
                  + "LEFT JOIN (SELECT ProcNum,MAX(EntryDateTime) EntryDateTime FROM procnote WHERE procnote.ProcNum IN (" + string.Join(",", listProcNums) + ") GROUP BY ProcNum) procnotemax ON procnotemax.ProcNum=procedurelog.ProcNum "
                  + "LEFT JOIN procnote procnoterow ON procnoterow.ProcNum=procedurelog.ProcNum AND procnoterow.EntryDateTime=procnotemax.EntryDateTime "
                  + "WHERE procedurelog.ProcNum IN (" + string.Join(",", listProcNums) + ")";
        //ProcNote stuff
        var table = DataCore.GetTable(command);
        var listProcs = ProcedureCrud.TableToList(table);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (table.Rows[i]["NoteNote"].ToString() == "") continue;
            listProcs[i].UserNum = SIn.Long(table.Rows[i]["NoteUserNum"].ToString());
            listProcs[i].Note = SIn.String(table.Rows[i]["NoteNote"].ToString());
            listProcs[i].SigIsTopaz = SIn.Bool(table.Rows[i]["NoteSigIsTopaz"].ToString());
            listProcs[i].Signature = SIn.String(table.Rows[i]["NoteSignature"].ToString());
        }

        return listProcs;
    }

    public static List<Procedure> GetProcsForSingle(long aptNum, bool isPlanned)
    {
        string command;
        if (isPlanned)
            command = "SELECT * from procedurelog WHERE PlannedAptNum = '" + aptNum + "'";
        else
            command = "SELECT * from procedurelog WHERE AptNum = '" + aptNum + "'";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetProcsForApptEdit(Appointment appt)
    {
        var command = "SELECT procedurelog.* FROM procedurelog "
                      + "WHERE procedurelog.PatNum=" + appt.PatNum + " "
                      + "AND (procedurelog.ProcStatus=" + (int) ProcStat.TP + " ";
        if (appt.AptNum != 0)
        {
            //Filling grid for a new appt
            command += "OR ";
            if (appt.AptStatus == ApptStatus.Planned)
                command += "procedurelog.PlannedAptNum=" + appt.AptNum + " ";
            else //Scheduled
                command += "procedurelog.AptNum=" + appt.AptNum + " ";
        }

        if (appt.AptStatus == ApptStatus.Scheduled || appt.AptStatus == ApptStatus.Complete
                                                   || appt.AptStatus == ApptStatus.Broken)
            command += "OR (procedurelog.AptNum=0 AND procedurelog.ProcStatus=" + (int) ProcStat.C + " AND "
                       + "DATE(procedurelog.ProcDate)=" + SOut.Date(appt.AptDateTime) + ") ";
        command += ") AND procedurelog.ProcStatus != " + (int) ProcStat.D;
        var result = ProcedureCrud.SelectMany(command);
        for (var i = 0; i < result.Count; i++)
        {
            command = "SELECT * FROM procnote WHERE ProcNum=" + result[i].ProcNum + " ORDER BY EntryDateTime DESC LIMIT 1";
            var table = DataCore.GetTable(command);
            if (table.Rows.Count == 0) continue;
            result[i].UserNum = SIn.Long(table.Rows[0]["UserNum"].ToString());
            result[i].Note = SIn.String(table.Rows[0]["Note"].ToString());
            result[i].SigIsTopaz = SIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
            result[i].Signature = SIn.String(table.Rows[0]["Signature"].ToString());
        }

        ProcedureLogic.SortProcedures(result);
        return result;
    }

    public static List<Procedure> GetProcsForPatByDate(long patNum, DateTime date)
    {
        var command = "SELECT * FROM procedurelog "
                      + "WHERE PatNum=" + patNum + " "
                      + "AND (ProcDate=" + SOut.Date(date) + " OR DateEntryC=" + SOut.Date(date) + ") "
                      + "AND ProcStatus!=" + SOut.Int((int) ProcStat.D); //exclude deleted procs
        var result = ProcedureCrud.SelectMany(command);
        for (var i = 0; i < result.Count; i++)
        {
            command = "SELECT * FROM procnote WHERE ProcNum=" + result[i].ProcNum + " ORDER BY EntryDateTime DESC";
            command = DbHelper.LimitOrderBy(command, 1);
            var table = DataCore.GetTable(command);
            if (table.Rows.Count == 0) continue;
            result[i].UserNum = SIn.Long(table.Rows[0]["UserNum"].ToString());
            result[i].Note = SIn.String(table.Rows[0]["Note"].ToString());
            result[i].SigIsTopaz = SIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
            result[i].Signature = SIn.String(table.Rows[0]["Signature"].ToString());
        }

        return result;
    }

    public static List<Procedure> GetProcsForClaimNum(long claimNum)
    {
        var command = "SELECT * FROM procedurelog " +
                      "INNER JOIN claimproc " +
                      "ON claimproc.ProcNum=procedurelog.ProcNum " +
                      "WHERE ClaimNum=" + claimNum;
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetProcsFromClaimProcs(List<ClaimProc> listClaimProc)
    {
        if (listClaimProc.Count == 0) return [];
        var listProcNums = listClaimProc.Select(x => x.ProcNum).ToList();
        var command = "SELECT * FROM procedurelog WHERE ProcNum IN (" + string.Join(",", listProcNums) + ")";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<ProcQueued> GetProcQueuedsFromClaimProcQueueds(List<ClaimProcs.ClaimProcQueued> listClaimProcQueueds)
    {
        if (listClaimProcQueueds.IsNullOrEmpty()) return [];
        var listProcNums = listClaimProcQueueds.Select(x => x.ProcNum).ToList();
        var command = "SELECT ProcNum,CodeNum,IcdVersion,DiagnosticCode,DiagnosticCode2,DiagnosticCode3,DiagnosticCode4" +
                      " FROM procedurelog WHERE ProcNum IN (" + string.Join(",", listProcNums) + ")";
        var table = DataCore.GetTable(command);
        var listProcForIcds = new List<ProcQueued>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var dataRow = table.Rows[i];
            var procForIcd = new ProcQueued();
            procForIcd.ProcNum = SIn.Long(dataRow["ProcNum"].ToString());
            procForIcd.CodeNum = SIn.Long(dataRow["CodeNum"].ToString());
            procForIcd.IcdVersion = SIn.Int(dataRow["IcdVersion"].ToString());
            procForIcd.DiagnosticCode = SIn.String(dataRow["DiagnosticCode"].ToString());
            procForIcd.DiagnosticCode2 = SIn.String(dataRow["DiagnosticCode2"].ToString());
            procForIcd.DiagnosticCode3 = SIn.String(dataRow["DiagnosticCode3"].ToString());
            procForIcd.DiagnosticCode4 = SIn.String(dataRow["DiagnosticCode4"].ToString());
            listProcForIcds.Add(procForIcd);
        }

        return listProcForIcds;
    }

    public class ProcQueued
    {
        public long CodeNum;
        public string DiagnosticCode;
        public string DiagnosticCode2;
        public string DiagnosticCode3;
        public string DiagnosticCode4;
        public int IcdVersion;
        public long ProcNum;
    }

    public static List<Procedure> GetProcsNonCpoeAttachedToApptsForProv(long provNum)
    {
        if (provNum == 0) return [];
        var command = "SELECT procedurelog.* "
                      + "FROM procedurelog "
                      + "INNER JOIN appointment ON procedurelog.AptNum=appointment.AptNum "
                      + "INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
                      + "WHERE procedurecode.IsRadiology=1 "
                      + "AND appointment.AptStatus=" + SOut.Int((int) ApptStatus.Scheduled) + " "
                      + "AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.TP) + " "
                      + "AND procedurelog.IsCpoe=0 "
                      + "AND procedurelog.ProvNum=" + provNum + " "
                      + "AND DATE(appointment.AptDateTime) >= CURDATE() "
                      + "ORDER BY appointment.AptDateTime";
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetProcsByStatusForPat(long patNum, params ProcStat[] procStatuses)
    {
        if (procStatuses == null || procStatuses.Length == 0) return [];
        var command = "SELECT * FROM procedurelog WHERE PatNum=" + patNum + " AND ProcStatus IN (" + string.Join(",", procStatuses.Select(x => (int) x)) + ")";
        return ProcedureCrud.SelectMany(command);
    }

    public static string GetRecentProcDateString(long patNum, DateTime aptDate, string procCodeRange)
    {
        if (aptDate.Year < 1880) aptDate = DateTime.Today;
        string code1;
        string code2;
        if (procCodeRange.Contains("-"))
        {
            var codeSplit = procCodeRange.Split('-');
            code1 = codeSplit[0].Trim();
            code2 = codeSplit[1].Trim();
        }
        else
        {
            code1 = procCodeRange.Trim();
            code2 = procCodeRange.Trim();
        }

        var command = "SELECT MAX(ProcDate) FROM procedurelog "
                      + "LEFT JOIN procedurecode ON procedurecode.CodeNum=procedurelog.CodeNum "
                      + "WHERE PatNum=" + patNum + " "
                      //+"AND CodeNum="+POut.Long(codeNum)+" "
                      + "AND ProcDate < " + SOut.Date(aptDate) + " "
                      + "AND (ProcStatus =" + SOut.Int((int) ProcStat.C) + " "
                      + "OR ProcStatus =" + SOut.Int((int) ProcStat.EC) + " "
                      + "OR ProcStatus =" + SOut.Int((int) ProcStat.EO) + ") "
                      + "AND procedurecode.ProcCode >= '" + SOut.String(code1) + "' "
                      + "AND procedurecode.ProcCode <= '" + SOut.String(code2) + "' ";
        var date = SIn.Date(DataCore.GetScalar(command));
        if (date.Year < 1880) return "";
        return date.ToString("M/yy");
    }

    public static List<Procedure> GetProcsMultApts(List<long> listAptNums)
    {
        if (listAptNums.IsNullOrEmpty()) return [];

        var strAptNums = "";
        for (var i = 0; i < listAptNums.Count; i++)
        {
            if (i > 0) strAptNums += " OR";
            strAptNums += " (AptNum=" + listAptNums[i];
            strAptNums += " OR PlannedAptNum=" + listAptNums[i] + ")";
        }

        var command = "SELECT * FROM procedurelog WHERE" + strAptNums;
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetProcsOneApt(long myAptNum, List<Procedure> procsMultApts, bool isForPlanned = false)
    {
        var listProcedures = new List<Procedure>();
        for (var i = 0; i < procsMultApts.Count; i++)
            if (isForPlanned && procsMultApts[i].PlannedAptNum == myAptNum)
                //If proc is attached to this planned appt only
                listProcedures.Add(procsMultApts[i].Copy());
            else if (procsMultApts[i].AptNum == myAptNum)
                //If proc is attached to this appointment
                listProcedures.Add(procsMultApts[i].Copy());

        return listProcedures;
    }

    public static List<Procedure> GetProcsForFormProcBandingSelect(long patNum)
    {
        if (PrefC.GetString(PrefName.OrthoBandingCodes) == "") return [];
        var listBandingProcedures = OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoBandingCodes);
        var command = $@"SELECT procedurelog.* FROM procedurelog
				JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum
				WHERE procedurelog.PatNum={patNum}
				AND procedurelog.ProcStatus={SOut.Int((int) ProcStat.TP)}
				AND procedurecode.ProcCode IN({string.Join(",", listBandingProcedures.Select(x => "'" + SOut.String(x) + "'").ToList())})";
        return ProcedureCrud.SelectMany(command);
    }

    public static Procedure GetProcFromList(List<Procedure> listProcs, long procNum)
    {
        return listProcs.FirstOrDefault(x => x.ProcNum == procNum) ?? new Procedure();
    }

    public static List<Procedure> GetCompletedForDateRange(DateTime dateStart, DateTime dateStop, List<long> listProcCodeNums = null, List<long> listPatNums = null, bool includeNote = false, bool includeGroupNote = false)
    {
        var command = "";
        var whereClause = "WHERE procedurelog.ProcStatus IN(" + SOut.Int((int) ProcStat.C);
        if (includeGroupNote) whereClause += "," + SOut.Int((int) ProcStat.EC);
        whereClause += ") AND procedurelog.ProcDate>=" + SOut.Date(dateStart) + " AND procedurelog.ProcDate<=" + SOut.Date(dateStop);
        if (listProcCodeNums != null && listProcCodeNums.Count > 0) whereClause += " AND procedurelog.CodeNum IN (" + string.Join(",", listProcCodeNums) + ")";
        if (listPatNums != null && listPatNums.Count > 0) whereClause += " AND procedurelog.PatNum IN (" + string.Join(",", listPatNums) + ")";
        command = "SELECT * FROM procedurelog " + whereClause;
        var listProcs = ProcedureCrud.SelectMany(command);
        if (!includeNote || listProcs.Count == 0) return listProcs;
        //ProcNote stuff
        command = "SELECT procnote.ProcNum,procnote.UserNum,procnote.Note,procnote.SigIsTopaz,procnote.Signature "
                  + "FROM procnote "
                  + "INNER JOIN ("
                  + "SELECT procnote.ProcNum,MAX(procnote.EntryDateTime) EntryDateTime "
                  + "FROM procnote "
                  + "WHERE procnote.ProcNum IN(" + string.Join(",", listProcs.Select(x => x.ProcNum)) + ") "
                  + "GROUP BY procnote.ProcNum"
                  + ") procnotemax ON procnote.ProcNum=procnotemax.ProcNum AND procnote.EntryDateTime=procnotemax.EntryDateTime";
        var dictProcNoteRows = DataCore.GetTable(command).Select().ToDictionary(x => SIn.Long(x["ProcNum"].ToString()));
        if (dictProcNoteRows.Count == 0) //no notes for the procs, just return the list of procs
            return listProcs;
        foreach (var proc in listProcs)
        {
            DataRow row;
            if (!dictProcNoteRows.TryGetValue(proc.ProcNum, out row) || string.IsNullOrEmpty(row["Note"].ToString())) continue;
            proc.UserNum = SIn.Long(row["UserNum"].ToString());
            proc.Note = SIn.String(row["Note"].ToString());
            proc.SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString());
            proc.Signature = SIn.String(row["Signature"].ToString());
        }

        return listProcs;
    }

    public static List<Procedure> GetCompletedByDateCompleteForDateRange(DateTime dateStart, DateTime dateStop)
    {
        var command = "SELECT * FROM procedurelog WHERE ProcStatus=" + SOut.Int((int) ProcStat.C)
                                                                     + " AND DateComplete>=" + SOut.Date(dateStart)
                                                                     + " AND DateComplete<=" + SOut.Date(dateStop);
        return ProcedureCrud.SelectMany(command);
    }

    public static double GetProcFee(Patient pat, List<PatPlan> listPatPlans, List<InsSub> listInsSubs, List<InsPlan> listInsPlans, Procedure procedure, List<Benefit> listBenefits = null, List<Fee> listFees = null)
    {
        //Do not change proc fee for completed procedures before today
        if (procedure.DateComplete.Year > 1880 && procedure.DateComplete < DateTime.Today && procedure.ProcStatus == ProcStat.C) return procedure.ProcFee;
        double procFeeRet;
        InsPlan insPlanPrimary = null;
        var patPlanPrimary = listPatPlans.Find(x => x.Ordinal == 1);
        if (patPlanPrimary != null)
        {
            //This will return a new InsSub if one is not found in the list or the DB.
            var insSubPrimary = InsSubs.GetSub(patPlanPrimary.InsSubNum, listInsSubs);
            //This will be null if insSubPrimary.PlanNum is 0 due to a insSubPrimary being a new InsSub.
            insPlanPrimary = InsPlans.GetPlan(insSubPrimary.PlanNum, listInsPlans);
        }

        //Get fee schedule and fee amount for medical or dental.
        if (PrefC.GetBool(PrefName.MedicalFeeUsedForNewProcs) && !string.IsNullOrEmpty(procedure.MedicalCode))
        {
            var feeSch = FeeScheds.GetMedFeeSched(pat, listInsPlans, listPatPlans, listInsSubs, procedure.ProvNum);
            procFeeRet = Fees.GetAmount0(ProcedureCodes.GetProcCode(procedure.MedicalCode).CodeNum, feeSch, procedure.ClinicNum, procedure.ProvNum, listFees);
        }
        else
        {
            var feeSch = FeeScheds.GetFeeSched(pat, listInsPlans, listPatPlans, listInsSubs, procedure.ProvNum);
            procFeeRet = Fees.GetAmount0(procedure.CodeNum, feeSch, procedure.ClinicNum, procedure.ProvNum, listFees);
        }

        if (insPlanPrimary != null && insPlanPrimary.PlanType == "p")
        {
            //PPO
            var ucrFee = Fees.GetAmount0(procedure.CodeNum, Providers.GetById(Patients.GetProvNum(pat)).FeeScheduleId??0, procedure.ClinicNum, procedure.ProvNum, listFees);
            if (procFeeRet < ucrFee || PrefC.GetBool(PrefName.InsPpoAlwaysUseUcrFee)) procFeeRet = ucrFee;
        }

        var priPatPlan = listPatPlans.Find(x => x.Ordinal == 1);
        if (priPatPlan == null) return procFeeRet;
        var priInsPlan = listInsPlans.Find(x => x.PlanNum == listInsSubs.Find(y => y.InsSubNum == priPatPlan.InsSubNum)?.PlanNum);
        if (priInsPlan != null && InsPlans.UsesUcrFeeForExclusions(priInsPlan.ExclusionFeeRule))
        {
            //Getting benefits here may cause slowness in places that get the fee many times 
            //(i.e. the Update Fees button in the TP module, or the Quick Add Procs button in FormApptEdit).
            //In places that are slow because of this (loops, etc) we should pass in listBenefits to avoid the chatty database calls.
            if (listBenefits == null) listBenefits = Benefits.GetForPlanOrPatPlan(priInsPlan.PlanNum, priPatPlan.PatPlanNum);
            if (Benefits.IsExcluded(ProcedureCodes.GetStringProcCode(procedure.CodeNum), listBenefits, priInsPlan.PlanNum, priPatPlan.PatPlanNum)
                || Benefits.GetPercent(ProcedureCodes.GetStringProcCode(procedure.CodeNum), priInsPlan.PlanType, priInsPlan.PlanNum, priPatPlan.PatPlanNum,
                    listBenefits) == 0)
                //Get the fee from the provider's fee schedule (ucr fee)
                procFeeRet = Fees.GetAmount0(procedure.CodeNum, Providers.GetById(Patients.GetProvNum(pat)).FeeScheduleId??0, procedure.ClinicNum, procedure.ProvNum, listFees);
        }

        return procFeeRet;
    }

    public static DataTable GetTablePatProvUsed(List<long> listPatNums)
    {
        var command = "SELECT PatNum,procedurelog.ProvNum,COUNT(ProcNum) procCount "
                      + "FROM procedurelog "
                      + "INNER JOIN provider ON procedurelog.ProvNum=provider.ProvNum AND provider.IsHidden=0 AND provider.IsSecondary=0 AND provider.IsNotPerson=0 "
                      + "WHERE PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                      + "GROUP BY procedurelog.ProvNum,PatNum" + " "
                      + "ORDER BY procCount DESC";
        return DataCore.GetTable(command);
    }

    public static long GetProvNumFromAppointment(Appointment apt, ProcedureCode procCode)
    {
        long provNum;
        if (procCode.ProvNumDefault != 0) //Override provider for procedures with a default provider
            provNum = procCode.ProvNumDefault;
        else if (apt.ProvHyg == 0 || !procCode.IsHygiene) //either no hygiene prov on the appt or the proc is not a hygiene proc
            provNum = apt.ProvNum;
        else //appointment has a hygiene prov and the proc IsHygiene
            provNum = apt.ProvHyg;
        return provNum;
    }

    public static List<Procedure> GetCanadianExtractedTeeth(List<Procedure> procList)
    {
        var extracted = new List<Procedure>();
        ProcedureCode procCode;
        foreach (var proc in procList)
        {
            if (!proc.ProcStatus.In(ProcStat.C, ProcStat.EC, ProcStat.EO)) continue;
            if (!Tooth.IsValidDB(proc.ToothNum)) continue;
            if (Tooth.IsSuperNum(proc.ToothNum)) continue;
            if (Tooth.IsPrimary(proc.ToothNum)) continue;
            if (proc.ProcDate.Year < 1880) continue;
            procCode = ProcedureCodes.GetProcCode(proc.CodeNum);
            if (procCode.TreatArea != TreatmentArea.Tooth) continue;
            if (procCode.PaintType != ToothPaintingType.Extraction) continue;
            //Canadian claims only allow 1 extraction procedure per tooth.
            //If there are duplicates found, the CDA only wants the most recent extraction procedure.
            var procedureInList = extracted.FirstOrDefault(x => x.ToothNum == proc.ToothNum);
            if (procedureInList != null)
            {
                if (procedureInList.DateTStamp > proc.DateTStamp) continue; //The current proc is not the most recent extraction for the toothnum.
                extracted.Remove(procedureInList);
            }

            extracted.Add(proc.Copy());
        }

        return extracted.OrderByDescending(x => x.DateTStamp).ToList();
    }

    public static List<Procedure> GetCanadianLabFees(long procNumLab, List<Procedure> procList)
    {
        var retVal = new List<Procedure>();
        if (procNumLab == 0) //Ignore regular procedures.
            return retVal;
        for (var i = 0; i < procList.Count; i++)
            if (procList[i].ProcNumLab == procNumLab)
                retVal.Add(procList[i]);

        return retVal;
    }

    public static List<Procedure> GetCanadianLabFees(List<long> listProcNums)
    {
        if (listProcNums.Count == 0) return [];
        return ProcedureCrud.SelectMany("SELECT * FROM procedurelog WHERE ProcStatus<>" + SOut.Int((int) ProcStat.D) + " AND ProcNumLab IN (" + string.Join(",", listProcNums) + ")");
    }

    public static List<Procedure> GetCanadianLabFees(long procNum)
    {
        if (procNum == 0) //By Total payment rows do not have labs.
            return [];
        var command = "SELECT * FROM procedurelog WHERE ProcStatus<>" + SOut.Int((int) ProcStat.D) + " AND ProcNumLab=" + procNum;
        return ProcedureCrud.SelectMany(command);
    }

    public static List<Procedure> GetProcsAttachedToFutureAppt(List<long> listPatNums, List<long> listCodeNums)
    {
        if (listPatNums.Count == 0 || listCodeNums.Count == 0) return [];
        var command = "SELECT procedurelog.* "
                      + "FROM procedurelog "
                      + "INNER JOIN appointment ON appointment.AptNum=procedurelog.AptNum "
                      + "WHERE appointment.PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND procedurelog.CodeNum IN (" + string.Join(",", listCodeNums) + ") "
                      + "AND procedurelog.ProcStatus=" + SOut.Int((int) ProcStat.TP) + " "
                      //All appts today or later
                      + "AND " + DbHelper.DateTConditionColumn("appointment.AptDateTime", ConditionOperator.GreaterThanOrEqual, MiscData.GetNowDateTime()) + " "
                      + "AND appointment.AptStatus=" + SOut.Int((int) ApptStatus.Scheduled);
        return ProcedureCrud.SelectMany(command);
    }

    public static List<long> GetProcNumMaxForGroups(int numPerGroup, List<ProcStat> listProcStatuses, long clinicNum)
    {
        _totCount = 0;
        var retval = new List<long>();
        if (numPerGroup < 1) return retval;
        var listWhereClauses = new List<string>();
        if (listProcStatuses != null && listProcStatuses.Count > 0) listWhereClauses.Add("ProcStatus IN(" + string.Join(",", listProcStatuses.Select(x => SOut.Int((int) x))) + ")");
        if (true && clinicNum > -1) listWhereClauses.Add("ClinicNum=" + clinicNum);
        var whereClause = "";
        if (listWhereClauses.Count > 0) whereClause = "WHERE " + string.Join(" AND ", listWhereClauses) + " ";
        var command = "SET @row=0,@maxProcNum=0;"
                      + "SELECT procNum,@row totalCount FROM ("
                      + "SELECT @row:=@row+1 rowNum,@maxProcNum:=ProcNum procNum FROM ("
                      + "SELECT ProcNum FROM procedurelog " + whereClause + "ORDER BY ProcNum"
                      + ") a"
                      + ") b "
                      + "WHERE procNum=@maxProcNum OR rowNum%" + numPerGroup + "=0";
        var tableCur = DataCore.GetTable(command);
        if (tableCur.Rows.Count > 0)
        {
            _totCount = SIn.Int(tableCur.Rows[0]["totalCount"].ToString());
            retval = tableCur.Select().Select(x => SIn.Long(x["procNum"].ToString())).ToList();
        }

        return retval;
    }

    public static List<Procedure> GetListTPandTPi(List<Procedure> procList, List<TreatPlanAttach> listTreatPlanAttaches = null)
    {
        return SortListByTreatPlanPriority(procList.FindAll(x => x.ProcStatus == ProcStat.TP || x.ProcStatus == ProcStat.TPi), listTreatPlanAttaches);
    }
    
    public static long GetClinicNum(long procNum)
    {
        var command = "SELECT ClinicNum FROM procedurelog WHERE ProcNum=" + procNum;
        return SIn.Long(DataCore.GetScalar(command));
    }

    public static Procedure GetMostRecentSRP(List<Procedure> listProcedures)
    {
        var listSRPCodeNums = ProcedureCodes.GetCodeNumsForCodeGroupFixed(EnumCodeGroupFixed.SRP);
        var listSRPProceduresOrdered = listProcedures
            .FindAll(x => x.ProcStatus.In(ProcStat.EO, ProcStat.C) && x.CodeNum.In(listSRPCodeNums.ToArray()))
            .OrderBy(x => x.ProcDate).ToList();
        //Most recent will be at the end of the list
        return listSRPProceduresOrdered.LastOrDefault();
    }

    public static List<string> GetUniqueDiagnosticCodes(List<Procedure> listProcs, bool isPadded)
    {
        return GetUniqueDiagnosticCodes(listProcs, isPadded, []);
    }

    public static List<string> GetUniqueDiagnosticCodes(List<Procedure> listProcs, bool isPadded, List<byte> listDiagnosticVersions)
    {
        var listDiagnosticCodes = new List<string>();
        listDiagnosticVersions.Clear();
        for (var i = 0; i < listProcs.Count; i++)
        {
            //Ensure that the principal diagnosis is first in the list.
            var proc = listProcs[i];
            if (proc.IcdVersion == 9) continue;
            if (proc.IsPrincDiag && proc.DiagnosticCode != "")
            {
                listDiagnosticCodes.Add(proc.DiagnosticCode);
                listDiagnosticVersions.Add(proc.IcdVersion);
                break;
            }
        }

        for (var i = 0; i < listProcs.Count; i++)
        {
            var proc = listProcs[i];
            if (proc.IcdVersion == 9) //don't return icd9 codes.
                continue;
            if (proc.DiagnosticCode != "" && !ExistsDiagnosticCode(listDiagnosticCodes, listDiagnosticVersions, proc.DiagnosticCode, proc.IcdVersion))
            {
                listDiagnosticCodes.Add(proc.DiagnosticCode);
                listDiagnosticVersions.Add(proc.IcdVersion);
            }

            if (proc.DiagnosticCode2 != "" && !ExistsDiagnosticCode(listDiagnosticCodes, listDiagnosticVersions, proc.DiagnosticCode2, proc.IcdVersion))
            {
                listDiagnosticCodes.Add(proc.DiagnosticCode2);
                listDiagnosticVersions.Add(proc.IcdVersion);
            }

            if (proc.DiagnosticCode3 != "" && !ExistsDiagnosticCode(listDiagnosticCodes, listDiagnosticVersions, proc.DiagnosticCode3, proc.IcdVersion))
            {
                listDiagnosticCodes.Add(proc.DiagnosticCode3);
                listDiagnosticVersions.Add(proc.IcdVersion);
            }

            if (proc.DiagnosticCode4 != "" && !ExistsDiagnosticCode(listDiagnosticCodes, listDiagnosticVersions, proc.DiagnosticCode4, proc.IcdVersion))
            {
                listDiagnosticCodes.Add(proc.DiagnosticCode4);
                listDiagnosticVersions.Add(proc.IcdVersion);
            }
        }

        while (isPadded && listDiagnosticCodes.Count < 12)
        {
            //Pad to at least 12 items.  Simplifies claim printing logic.
            listDiagnosticCodes.Add("");
            listDiagnosticVersions.Add(0);
        }

        return listDiagnosticCodes;
    }

    public static DataTable GetReferred(DateTime dateFrom, DateTime dateTo, bool complete)
    {
        var command =
            "SELECT procedurelog.CodeNum,procedurelog.PatNum,LName,FName,MName,RefDate,DateProcComplete,refattach.Note,RefToStatus "
            + "FROM procedurelog "
            + "JOIN refattach ON procedurelog.ProcNum=refattach.ProcNum "
            + "JOIN referral ON refattach.ReferralNum=referral.ReferralNum "
            + "WHERE RefDate>=" + SOut.Date(dateFrom) + " "
            + "AND RefDate<=" + SOut.Date(dateTo) + " ";
        if (!complete) command += "AND DateProcComplete=" + SOut.Date(DateTime.MinValue) + " ";
        command += "ORDER BY RefDate";
        return DataCore.GetTable(command);
    }

    public static List<Procedure> GetCompletedForDateRangeLimited(DateTime dateStart, DateTime dateStop, List<long> listClinicNums)
    {
        var command = "SELECT ProcNum,ProcFee,UnitQty,BaseUnits,ClinicNum,CodeNum,ProcDate "
                      + "FROM procedurelog WHERE ProcStatus=" + SOut.Int((int) ProcStat.C) + " "
                      + "AND ProcDate>=" + SOut.Date(dateStart) + " AND ProcDate<=" + SOut.Date(dateStop);
        if (listClinicNums != null && listClinicNums.Count > 0) command += " AND ClinicNum IN (" + string.Join(",", listClinicNums) + ")";
        var table = DataCore.GetTable(command);
        var listProcsLim = new List<Procedure>();
        foreach (DataRow row in table.Rows)
        {
            var proc = new Procedure();
            proc.ProcNum = SIn.Long(row["ProcNum"].ToString());
            proc.ProcFee = SIn.Double(row["ProcFee"].ToString());
            proc.UnitQty = SIn.Int(row["UnitQty"].ToString());
            proc.BaseUnits = SIn.Int(row["BaseUnits"].ToString());
            proc.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            proc.CodeNum = SIn.Long(row["CodeNum"].ToString());
            proc.ProcDate = SIn.Date(row["ProcDate"].ToString());
            listProcsLim.Add(proc);
        }

        return listProcsLim;
    }

    public static long Insert(Procedure procedure, bool skipDiscountPlanAdjustment = false)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        procedure.SecUserNumEntry = Security.CurUser.UserNum;
        procedure.DiscountPlanAmt = GetDiscountAmountForDiscountPlanAndValidate(procedure);
        if (procedure.ProcStatus == ProcStat.C)
            procedure.DateComplete = DateTime.Today;
        else //In case someone tried to programmatically set the DateComplete when they shouldn't have.
            procedure.DateComplete = DateTime.MinValue;
        ProcedureCrud.Insert(procedure);
        //Do the sales tax adjustments first because we might be changing the proc note if an error occurs.
        if (procedure.ProcStatus == ProcStat.C && !skipDiscountPlanAdjustment) Adjustments.CreateAdjustmentForDiscountPlan(procedure); //Inserting completed procedure.
        if (procedure.Note != "")
        {
            var note = new ProcNote();
            note.PatNum = procedure.PatNum;
            note.ProcNum = procedure.ProcNum;
            note.UserNum = procedure.UserNum;
            note.Note = procedure.Note;
            ProcNotes.Insert(note);
        }

        return procedure.ProcNum;
    }
    
    public static bool ExceedsFreqLimitation;
    public static bool ExceedsAnnualMax;

    public static double GetDiscountAmountForDiscountPlanAndValidate(Procedure procedure, DiscountPlanSub discountPlanSub = null, DiscountPlan discountPlan = null, double runningTotal = 0, List<Procedure> listAddHistProcs = null)
    {
        ExceedsFreqLimitation = false;
        ExceedsAnnualMax = false;
        if (discountPlanSub == null)
        {
            discountPlanSub = DiscountPlanSubs.GetSubForPat(procedure.PatNum);
            if (discountPlanSub == null) //Patient isn't a discountPlanSub
                return 0;
        }

        if (discountPlan == null)
        {
            discountPlan = DiscountPlans.GetPlan(discountPlanSub.DiscountPlanNum);
            if (discountPlan == null) //Patient doesn't have a discountPlan
                return 0;
        }

        var dateEffective = DiscountPlanSubs.GetAnnualMaxDateEffective(discountPlanSub.DateEffective);
        var dateTerm = DiscountPlanSubs.GetAnnualMaxDateTerm(discountPlanSub.DateTerm);
        if (procedure.ProcDate < dateEffective || procedure.ProcDate > dateTerm) return 0;
        var dateEffectiveFinal = DiscountPlanSubs.GetDateEffectiveForAnnualDateRangeSegment(procedure.ProcDate, dateEffective, dateTerm);
        var dateTermFinal = DiscountPlanSubs.GetDateTermForAnnualDateRangeSegment(procedure.ProcDate, dateEffective, dateTerm);
        //Check if the frequency limitation is exceeded
        var discountPlanFrequencyLimits = "";
        try
        {
            if (discountPlanSub != null)
                discountPlanFrequencyLimits = DiscountPlans.CheckDiscountFrequencyAndValidateDiscountPlanSub(ListTools.FromSingle(procedure), procedure.PatNum,
                    procedure.ProcDate, discountPlanSub, listAddHistProcs);
        }
        catch (Exception e)
        {
            //Just Swollow it, the following if statement actually will handle things
            discountPlanFrequencyLimits = e.Message; //Set this to something so at least we don't try applying a Fee.
        }

        if (!string.IsNullOrEmpty(discountPlanFrequencyLimits))
        {
            ExceedsFreqLimitation = true;
            return 0;
        }

        var dictDiscountFees = DiscountPlans.GetFeeSchedNumsByPatNums(ListTools.FromSingle(procedure.PatNum));
        if (dictDiscountFees.Count == 0) //Only do this if the patient has a discount plan.
            return 0;
        return GetDiscountAmountForDiscountPlan(procedure, discountPlanSub.PatNum, dateEffectiveFinal, dateTermFinal, dictDiscountFees.First().Value, discountPlan, runningTotal, listAddHistProcs);
    }

    public static double GetDiscountAmountForDiscountPlan(Procedure procedure, long patNum, DateTime dateStart, DateTime dateStop, long feeSchedNum, DiscountPlan discountPlan = null, double runningTotal = 0, List<Procedure> listAddHistProcs = null)
    {
        var procFee = Fees.GetFee(procedure.CodeNum, feeSchedNum, procedure.ClinicNum, procedure.ProvNum);
        if (procFee == null)
        {
            //No fee for discount plan's feesched
            var procProv = Providers.GetById(procedure.ProvNum);
            procFee = Fees.GetFee(procedure.CodeNum, procProv.FeeScheduleId??0, procedure.ClinicNum, procProv.Id);
            if (procFee == null)
            {
                //No fee for discount plan's feesched and proc's provider
                var pat = Patients.GetPat(procedure.PatNum);
                var patProv = Providers.GetById(pat.PriProv);
                procFee = Fees.GetFee(procedure.CodeNum, patProv.FeeScheduleId??0, procedure.ClinicNum, patProv.Id);
                if (procFee == null)
                {
                    //No fee for pat's pri prov feesched and pat's pri prov
                    patProv = Providers.GetById(PrefC.GetLong(PrefName.PracticeDefaultProv));
                    procFee = Fees.GetFee(procedure.CodeNum, patProv.FeeScheduleId??0, procedure.ClinicNum, patProv.Id);
                }
            }
        }

        //double procFeeAmt=(procFee == null) ? procedure.ProcFeeTotal : procFee.Amount;
        if (procFee == null || procFee.Amount == -1) //If there is no procfee from the fee sched, there will be no discount
            return 0;
        //Check if the annual max is exceeded
        var procFeeAmt = procFee.Amount;
        var estimatedDiscountAmt = procedure.ProcFeeTotal - procFeeAmt * procedure.Quantity;
        if (estimatedDiscountAmt <= 0) //ProcFeeAmount would be issuing a refund
            return 0;
        if (runningTotal == 0) runningTotal = Adjustments.GetTotForPatByType(patNum, discountPlan.DefNum, dateStart, dateStop, procedure.ProcNum);
        if (discountPlan.AnnualMax == -1) return estimatedDiscountAmt;
        if (runningTotal > discountPlan.AnnualMax)
        {
            //Discount would exceed annual limit
            //_annualMaxMessage=Lans.g("Procedures","Exceeds Annual Max.");
            ExceedsAnnualMax = true;
            return 0;
        }

        if (runningTotal + estimatedDiscountAmt > discountPlan.AnnualMax)
        {
            //Discount is partially applied up to annualMax
            //_annualMaxMessage=Lans.g("Procedures","Partially exceeds Annual Max. Applying:")+$" {discountPlan.AnnualMax-runningTotal:C}";
            ExceedsAnnualMax = true;
            return discountPlan.AnnualMax - runningTotal;
        }

        return estimatedDiscountAmt;
    }
    
    public static void FormProcEditUpdate(Procedure procNew, Procedure procOld, ProcedureCode procCode, bool isProcLinkedToOrthoCase, bool isNew = false, string procTeethStr = "")
    {
        if (!CultureInfo.CurrentCulture.Name.EndsWith("CA") || procOld.ProcNumLab == 0) //Canadian. en-CA or fr-CA
            Update(procNew, procOld, isProcLinkedToOrthoCase: isProcLinkedToOrthoCase); //Do not update Canadian labs here, because they are handled in SetCanadianLabFeesCompleteForProc below.

        Appointments.UpdateProcDescriptionForAppt(procNew, procOld); //Does nothing if procNew is not associated to an appt or planned appt.
        DiscountChangeSecLogEntry(procNew, procOld); //Does nothing if procNew.Discount is the same as procOld.Discount.
        UpdateTpProcPriority(procNew); //Only updates/goes to DB if procNew.ProcStatus is TP.
        if (!ProcWasSetComplete(procNew, procOld, procCode))
        {
            //Does nothing if procOld.ProcStatus is C or if procNew.ProcStatus is not C.
            if (procNew.ProcStatus == ProcStat.C && isNew)
            {
                //if new procedure is complete
                LogProcComplCreate(procNew.PatNum, procNew, procNew.ToothNum);
            }
            else if (procOld.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC) && !isNew)
            {
                #region SecurityLog for editing a previously completed proc

                var logText = procCode.ProcCode + " (" + procOld.ProcStatus + "), ";
                if (procOld.ProcStatus != procNew.ProcStatus)
                {
                    //Status changed.
                    var statusText = logText + Lans.g("Procedures", " changed from ") + procOld.ProcStatus + Lans.g("Procedures", " to ") + procNew.ProcStatus;
                    SecurityLogs.MakeLogEntry(EnumPermType.ProcCompleteStatusEdit, procNew.PatNum, statusText);
                }

                if (!string.IsNullOrEmpty(procTeethStr)) logText += Lans.g("Procedures", "Teeth") + ": " + procTeethStr + ", ";
                logText += Lans.g("Procedures", "Fee") + ": " + procNew.ProcFee.ToString("F") + ", " + procCode.Descript + ". ";
                if (procNew.ProvNum != procOld.ProvNum && procOld.ProcStatus == ProcStat.C) logText += Lans.g("Procedures", "Provider was changed from") + " " + Providers.GetAbbr(procOld.ProvNum) + " " + Lans.g("Procedures", "to") + " " + Providers.GetAbbr(procNew.ProvNum) + ".";
                if (procOld.ProcStatus.In(ProcStat.EO, ProcStat.EC))
                {
                    SecurityLogs.MakeLogEntry(EnumPermType.ProcExistingEdit, procNew.PatNum, logText);
                }
                else
                {
                    //completed procedures, adds changes to securitylog if certain fields edited
                    logText += SecurityLogs.AppendProcCompleteEditSecurityLog(procNew, procOld);
                    SecurityLogs.MakeLogEntry(EnumPermType.ProcCompleteEdit, procNew.PatNum, logText);
                }
            }

            #endregion
        }

        SetToothInitialForCompExtraction(procNew);

        #region Canadian lab helpers

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Canada
            if (procNew.ProcStatus == ProcStat.C) //Canadian lab fees complete
                SetCanadianLabFeesCompleteForProc(procNew);
            else //Canadian lab fees not complete
                SetCanadianLabFeesStatusForProc(procNew);
        }

        #endregion
    }

    public static void Update(Procedure procedure, Procedure oldProcedure, bool isPaySplit = false, bool isProcLinkedToOrthoCase = false)
    {
        //Setting any procedure to TP, get a tax estimate only if the procedure amount is changing and the procedure is taxable
        if (procedure.ProcStatus == ProcStat.TP)
            //Status changed and is attached to appointment (do not care about planned appointments since that is what treatment planning is for).
            if (procedure.ProcStatus != oldProcedure.ProcStatus && procedure.AptNum != 0)
                foreach (var appt in Appointments.GetAppointmentsForProcs([procedure]))
                    //only 0,1, or 2 of these
                    //Detach the procedure from completed appointment.
                    if (appt.AptStatus == ApptStatus.Complete && procedure.AptNum == appt.AptNum)
                    {
                        procedure.AptNum = 0;
                        break; //If there is another appointment in the loop, then it must be a planned appointment which we do not care about here.
                    }

        //Setting a procedure to complete
        if (oldProcedure.ProcStatus != ProcStat.C && procedure.ProcStatus == ProcStat.C && !isProcLinkedToOrthoCase)
        {
            procedure.DiscountPlanAmt = Adjustments.CreateAdjustmentForDiscountPlan(procedure);
            var hasDiscountAdjustment = Adjustments.GetListForProc(procedure.ProcNum)
                .Exists(x => x.AdjType == PrefC.GetLong(PrefName.TreatPlanDiscountAdjustmentType)
                             && CompareDouble.IsEqual(x.AdjAmt, -procedure.Discount));
            if (!CompareDouble.IsZero(procedure.Discount) && !hasDiscountAdjustment)
                //Discounted Procedure and has no matching discount adjustment
                Adjustments.CreateAdjustmentForDiscount(procedure);
        }

        //If this is a complete procedure but either the date or the fee is changing, we need to try to update the transaction in AvaTax
        if (procedure.ProcStatus == ProcStat.C &&
            (oldProcedure.ProcFee != procedure.ProcFee || oldProcedure.ProcDate.Date != procedure.ProcDate.Date || oldProcedure.ProcStatus != ProcStat.C))
        {
        }

        //Setting a completed procedure to TP.
        if (oldProcedure.ProcStatus == ProcStat.C && procedure.ProcStatus != ProcStat.C) Adjustments.DeleteForProcedure(procedure.ProcNum);
        if (procedure.ProcStatus == ProcStat.C && procedure.DateComplete.Year < 1880 && oldProcedure.ProcStatus != ProcStat.C)
            procedure.DateComplete = DateTime.Today;
        else if (procedure.ProcStatus != ProcStat.C && procedure.DateComplete.Date == DateTime.Today.Date) procedure.DateComplete = DateTime.MinValue; //db only field used by one customer and this is how they requested it.  PatNum #19191
        if (isPaySplit) PaySplits.UpdateAttachedPaySplits(procedure);
        var hasChanged = ProcedureCrud.Update(procedure, oldProcedure);
        //Setting the procedure to complete OR treatment planned. Alternatively, if setting a proc TP / Completed to NOT be TP / Completed.
        if ((oldProcedure.ProcStatus != ProcStat.C && new List<ProcStat> {ProcStat.C, ProcStat.TP}.Contains(procedure.ProcStatus))
            || (procedure.ProcStatus != ProcStat.C && new List<ProcStat> {ProcStat.C, ProcStat.TP}.Contains(oldProcedure.ProcStatus)))
            PayPlanCharges.UpdateAttachedPayPlanCharges(procedure); //does nothing if there are none.
        if (hasChanged && oldProcedure.ProcStatus == ProcStat.TP && procedure.ProcStatus == ProcStat.C)
        {
            //check for any tp prepayments and make transfer if valid
            var listUnearnedSplitsForProc = PaySplits.GetPaySplitsFromProc(procedure.ProcNum, true);
            Payments.CreateTransferForTpProcs(procedure, listUnearnedSplitsForProc);
        }

        if (procedure.ProcStatus != oldProcedure.ProcStatus) ProcMultiVisits.UpdateGroupForProc(procedure.ProcNum, procedure.ProcStatus);
        if (procedure.Note != oldProcedure.Note
            || procedure.UserNum != oldProcedure.UserNum
            || procedure.SigIsTopaz != oldProcedure.SigIsTopaz
            || procedure.Signature != oldProcedure.Signature)
        {
            var note = new ProcNote();
            note.PatNum = procedure.PatNum;
            note.ProcNum = procedure.ProcNum;
            note.UserNum = procedure.UserNum;
            note.Note = procedure.Note;
            note.SigIsTopaz = procedure.SigIsTopaz;
            note.Signature = procedure.Signature;
            ProcNotes.Insert(note);
        }
    }

    public static void UpdateAptNum(long procNum, long newAptNum)
    {
        UpdateAptNums([procNum], newAptNum);
    }

    public static void UpdateAptNums(List<long> listProcNums, long newAptNum, bool isPlannedAptNum = false)
    {
        if (listProcNums == null || listProcNums.Count == 0) return;

        var command = "UPDATE procedurelog "
                      + "SET " + (isPlannedAptNum ? "PlannedAptNum =" : "AptNum =") + newAptNum + " "
                      + "WHERE ProcNum IN (" + string.Join(",", listProcNums.Select(x => x)) + ")";
        Db.NonQ(command);
    }

    public static void UpdateCpoeForProc(long procNum, bool isCpoe)
    {
        UpdateCpoeForProcs([procNum], isCpoe);
    }

    public static void UpdateCpoeForProcs(List<long> listProcNums, bool isCpoe)
    {
        if (listProcNums == null || listProcNums.Count < 1) return;
        var command = "UPDATE procedurelog SET IsCpoe = " + SOut.Bool(isCpoe)
                                                          + " WHERE ProcNum IN (" + string.Join(",", listProcNums) + ")";
        Db.NonQ(command);
    }

    public static void SetDateFirstVisit(DateTime visitDate, int situation, Patient pat)
    {
        if (situation == 1)
            if (pat.DateFirstVisit.Year > 1880)
                return; //a date has already been set.

        if (situation == 2)
            if (pat.DateFirstVisit.Year > 1880 && pat.DateFirstVisit < DateTime.Now.AddDays(-7))
                return; //a date has already been set.

        var command = "SELECT COUNT(*) from procedurelog "
                      + "INNER JOIN procedurecode on procedurecode.CodeNum = procedurelog.CodeNum "
                      + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                      + "WHERE PatNum = '" + pat.PatNum + "' "
                      + "AND ProcStatus = '2'";
        var table = DataCore.GetTable(command);
        if (SIn.Long(table.Rows[0][0].ToString()) > 0) return; //there are already completed procs (for all situations)
        if (situation == 2)
        {
            //ask user first?
        }

        if (situation == 3)
            command = "UPDATE patient SET DateFirstVisit =" + SOut.Date(new DateTime(0001, 01, 01))
                                                            + " WHERE PatNum ='"
                                                            + pat.PatNum + "'";
        else
            command = "UPDATE patient SET DateFirstVisit ="
                      + SOut.Date(visitDate) + " WHERE PatNum ='"
                      + pat.PatNum + "'";
        //MessageBox.Show(cmd.CommandText);
        //dcon.NonQ(command);
        Db.NonQ(command);
    }

    public static void SetProvidersInAppointment(Appointment apt, List<Procedure> listProcOrig, bool isUpdatingFees, ProcFeeHelper procFeeHelper)
    {
        var listProcNew = new List<Procedure>();
        Procedure changedProc;
        OrthoProcLink orthoProcLink;
        var dictOrthoProcLinks =
            OrthoProcLinks.GetManyForProcs(listProcOrig.Select(x => x.ProcNum).ToList()).ToDictionary(y => y.ProcNum, y => y);
        for (var i = 0; i < listProcOrig.Count; i++)
        {
            changedProc = listProcOrig[i].Copy();
            listProcNew.Add(changedProc);
            if (!IsProcComplEditAuthorized(changedProc)) continue;
            changedProc = ChangeProcInAppointment(apt, changedProc);
            if (listProcOrig[i].ProcDate.Date != changedProc.ProcDate.Date && changedProc.ProcStatus == ProcStat.TP)
            {
                dictOrthoProcLinks.TryGetValue(listProcOrig[i].ProcNum, out orthoProcLink);
                if (orthoProcLink != null && orthoProcLink.ProcLinkType == OrthoProcType.Banding) OrthoCases.UpdateDatesByLinkedProc(orthoProcLink, changedProc);
            }
        }

        if (isUpdatingFees)
            foreach (var procCur in listProcNew)
            {
                if (!IsProcComplEditAuthorized(procCur)) continue;
                procFeeHelper.FillData();
                procCur.ProcFee = GetProcFee(procFeeHelper.Pat, procFeeHelper.ListPatPlans, procFeeHelper.ListInsSubs, procFeeHelper.ListInsPlans,
                    procCur, procFeeHelper.ListBenefitsPrimary, procFeeHelper.ListFees);
            }

        Sync(listProcNew, listProcOrig);
    }

    public static void SetCanadianLabFeesCompleteForProc(Procedure proc)
    {
        //If this gets run on a lab fee itself, nothing will happen because result will be zero procs.
        var command = "SELECT * FROM procedurelog WHERE ProcNumLab=" + proc.ProcNum + " AND ProcStatus!=" + SOut.Int((int) ProcStat.D);
        var labFeesForProc = ProcedureCrud.SelectMany(command);
        if (proc.ProcNumLab == 0)
        {
            //Regular procedure, not a lab.
            for (var i = 0; i < labFeesForProc.Count; i++)
            {
                var labFeeNew = labFeesForProc[i];
                var labFeeOld = labFeeNew.Copy();
                labFeeNew.AptNum = proc.AptNum;
                labFeeNew.CanadianTypeCodes = proc.CanadianTypeCodes;
                labFeeNew.ClinicNum = proc.ClinicNum;
                labFeeNew.DateEntryC = proc.DateEntryC;
                labFeeNew.PlaceService = proc.PlaceService;
                labFeeNew.ProcDate = proc.ProcDate;
                labFeeNew.ProcStatus = ProcStat.C;
                if (labFeeNew.ProvNum == 0) //Shouldn't happen
                    labFeeNew.ProvNum = proc.ProvNum;
                labFeeNew.SiteNum = proc.SiteNum;
                labFeeNew.UserNum = proc.UserNum;
                Update(labFeeNew, labFeeOld);
            }
        }
        else
        {
            //Lab fee.  Set complete, set the parent procedure as well as any other lab fees complete.
            command = "SELECT * FROM procedurelog WHERE ProcNum=" + proc.ProcNumLab + " AND ProcStatus!=" + SOut.Int((int) ProcStat.D);
            var procParent = ProcedureCrud.SelectOne(command);
            SetCanadianLabFeesCompleteForProc(procParent);
            var parentProcNew = procParent;
            var parentProcOld = procParent.Copy();
            parentProcNew.ProcStatus = ProcStat.C;
            Update(parentProcNew, parentProcOld);
        }
    }

    public static void SetCanadianLabFeesStatusForProc(Procedure proc)
    {
        //If this gets run on a lab fee itself, nothing will happen because result will be zero procs.
        var command = "SELECT * FROM procedurelog WHERE ProcNumLab=" + proc.ProcNum + " AND ProcStatus!=" + SOut.Int((int) ProcStat.D);
        var labFeesForProc = ProcedureCrud.SelectMany(command);
        if (proc.ProcNumLab == 0)
        {
            //Regular procedure, not a lab.
            for (var i = 0; i < labFeesForProc.Count; i++)
            {
                var labFeeNew = labFeesForProc[i];
                var labFeeOld = labFeeNew.Copy();
                labFeeNew.ProcStatus = proc.ProcStatus;
                Update(labFeeNew, labFeeOld);
            }
        }
        else
        {
            //Lab fee.  If lab is set back to any status other than complete, set the parent procedure as well as any other lab fees back to that status.
            command = "SELECT * FROM procedurelog WHERE ProcNum=" + proc.ProcNumLab + " AND ProcStatus!=" + SOut.Int((int) ProcStat.D);
            var procParent = ProcedureCrud.SelectOne(command);
            var parentProcNew = procParent;
            var parentProcOld = procParent.Copy();
            parentProcNew.ProcStatus = proc.ProcStatus;
            SetCanadianLabFeesStatusForProc(parentProcNew);
            Update(parentProcNew, parentProcOld);
        }
    }
    
    public static void Lock(DateTime date1, DateTime date2)
    {
        var command = "UPDATE procedurelog SET IsLocked=1 "
                      + "WHERE (ProcStatus=" + SOut.Int((int) ProcStat.C) + " " //completed
                      + "OR CodeNum=" + ProcedureCodes.GetCodeNum(ProcedureCodes.GroupProcCode) + ") " //or group note
                      + "AND ProcDate >= " + SOut.Date(date1) + " "
                      + "AND ProcDate <= " + SOut.Date(date2);
        Db.NonQ(command);
    }

    public static void Sync(List<Procedure> listNew, List<Procedure> listOld)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        ProcedureCrud.Sync(listNew, listOld, Security.CurUser.UserNum);
    }

    public static void SetTPActive(long patNum, List<long> listProcNums)
    {
        var command = "UPDATE procedurelog SET ProcStatus=" + SOut.Int((int) ProcStat.TPi) + " WHERE PatNum=" + patNum + " " +
                      "AND ProcStatus=" + SOut.Int((int) ProcStat.TP) + " ";
        if (listProcNums.Count == 0)
        {
            Db.NonQ(command);
            return; //no procedures left on active plan
        }

        command += "AND ProcNum NOT IN (" + string.Join(",", listProcNums) + ") ";
        Db.NonQ(command);
        command = "UPDATE procedurelog SET ProcStatus=" + SOut.Int((int) ProcStat.TP) + " WHERE PatNum=" + patNum + " " +
                  "AND ProcStatus=" + SOut.Int((int) ProcStat.TPi) + " AND ProcNum IN (" + string.Join(",", listProcNums) + ") ";
        Db.NonQ(command);
    }

    public static bool UpdateProcsInApptHelper(List<Procedure> listProcsForAppt, Patient pat, Appointment AptCur, Appointment AptOld, List<InsPlan> PlanList, List<InsSub> SubList, List<int> listProcSelectedIndices, bool removeCompletedProcs, bool doUpdateProcFees = false, LogSources logSource = LogSources.None)
    {
        if (AptCur.AptStatus != ApptStatus.Complete)
        {
            //appt is not set complete, just update necessary fields like ProvNum, ProcDate, and ClinicNum
            ProcFeeHelper procFeeHelper = null;
            OrthoProcLink orthoProcLink;
            var dictOrthoProcLinks =
                OrthoProcLinks.GetManyForProcs(listProcsForAppt.Select(x => x.ProcNum).ToList()).ToDictionary(y => y.ProcNum, y => y);
            foreach (var index in listProcSelectedIndices)
            {
                //We only want to change the fields that just changed.  We don't want to undo any changes that are being made outside this window.  Note
                //that if we make any other changes to this proc that are not in this section we should consolidate update statements.
                var procCur = listProcsForAppt[index];
                var procOld = procCur.Copy();
                var perm = GroupPermissions.SwitchExistingPermissionIfNeeded(EnumPermType.ProcCompleteEdit, procOld);
                var dateForPerm = GetDateForPermCheck(procOld);
                if (procOld.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC) && !Security.IsAuthorized(perm, dateForPerm, true)) continue;
                ChangeProcInAppointment(AptCur, procCur); //Doesn't update DB
                dictOrthoProcLinks.TryGetValue(procCur.ProcNum, out orthoProcLink);
                if (doUpdateProcFees && orthoProcLink == null)
                {
                    //Can't update fees for procedures linked to orthocases.
                    procFeeHelper = procFeeHelper ?? new ProcFeeHelper(pat, null, null, SubList, PlanList, null);
                    procFeeHelper.FillData();
                    procCur.ProcFee = GetProcFee(procFeeHelper.Pat, procFeeHelper.ListPatPlans, procFeeHelper.ListInsSubs, procFeeHelper.ListInsPlans,
                        procCur, procFeeHelper.ListBenefitsPrimary, procFeeHelper.ListFees);
                }

                Update(procCur, procOld); //Update fields if needed.
                //If proc is linked as a TP'd banding to an OrthoCase, update the OrthoCase.
                if (procCur.ProcStatus == ProcStat.TP && orthoProcLink != null && orthoProcLink.ProcLinkType == OrthoProcType.Banding) OrthoCases.UpdateDatesByLinkedProc(orthoProcLink, procCur);
            }
        }
        else if (listProcSelectedIndices.Any(x => listProcsForAppt[x].ProcStatus != ProcStat.C))
        {
            //if appointment is marked complete and any procedures are not, then set the remaining procedures complete.
            var listPatPlans = PatPlans.Refresh(AptCur.PatNum);
            SetCompleteInAppt(AptCur, PlanList, listPatPlans, pat, SubList, removeCompletedProcs);
            if (AptOld.AptStatus == ApptStatus.Complete) //seperate log entry for completed appointments
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentCompleteEdit, pat.PatNum, AptCur.AptDateTime.ToShortDateString()
                                                                                            + ", " + AptCur.ProcDescript + ", Procedures automatically set complete due to appt being set complete", AptCur.AptNum, logSource,
                    AptOld.DateTStamp);
            else
                SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit, pat.PatNum, AptCur.AptDateTime.ToShortDateString()
                                                                                    + ", " + AptCur.ProcDescript + ", Procedures automatically set complete due to appt being set complete", AptCur.AptNum, logSource,
                    AptOld.DateTStamp);
            return true;
        }

        return false;
    }
    
    public static void UpdateDiscountPlanAmt(long procNum, double newDiscountPlanAmt)
    {
        var command = "UPDATE procedurelog SET DiscountPlanAmt = " + SOut.Double(newDiscountPlanAmt)
                                                                   + " WHERE ProcNum = " + procNum;
        Db.NonQ(command);
    }

    public static void UpdateDiscountPlanAmts(List<Procedure> listProcs)
    {
        UpdateDiscountPlanAmts(listProcs.Select(x => new DiscountPlanProc(x)).ToList());
    }

    public static void UpdateDiscountPlanAmts(List<DiscountPlanProc> listProcs)
    {
        listProcs.ForEach(x => UpdateDiscountPlanAmt(x.ProcNum, x.DiscountPlanAmt));
    }
    
    public static void Delete(long procNum, bool forceDelete = false, bool hideGraphics = false)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) DeleteCanadianLabFeesForProcCode(procNum); //Deletes lab fees attached to current procedures.
        string command;
        if (forceDelete)
        {
            //Delete referral attaches
            command = "DELETE FROM refattach WHERE ProcNum=" + procNum;
            Db.NonQ(command);
            //Remove the procedure from the pay split
            command = "UPDATE paysplit SET ProcNum=0 WHERE ProcNum=" + procNum;
            Db.NonQ(command);
            //Claimprocs deleted below
        }
        else
        {
            ValidateDelete(procNum);
        }

        //delete adjustments, audit logs added from Adjustments.DeleteForProcedure()
        Adjustments.DeleteForProcedure(procNum);
        //delete claimprocs
        command = "DELETE from claimproc WHERE ProcNum = '" + procNum + "'";
        Db.NonQ(command);
        //detach procedure labs
        command = "UPDATE procedurelog SET ProcNumLab=0 WHERE ProcNumLab='" + procNum + "'";
        Db.NonQ(command);
        PayPlanCharges.DeleteForProc(procNum);
        //delete and update procmultivisits
        ProcMultiVisits.UpdateGroupForProc(procNum, ProcStat.D);
        command = "SELECT AptNum,PlannedAptNum,DateComplete FROM procedurelog WHERE ProcNum = " + procNum;
        var table = DataCore.GetTable(command);
        var dateComplete = SIn.Date(table.Rows[0]["DateComplete"].ToString());
        var aptNum = SIn.Long(table.Rows[0]["AptNum"].ToString());
        var plannedAptNum = SIn.Long(table.Rows[0]["PlannedAptNum"].ToString());
        //set the procedure deleted-----------------------------------------------------------------------------------------
        command = "UPDATE procedurelog SET ProcStatus = " + SOut.Int((int) ProcStat.D) + ", "
                  + "AptNum=0, "
                  + "PlannedAptNum=0";
        if (dateComplete.Date == DateTime.Today.Date) command += ", DateComplete=" + SOut.Date(DateTime.MinValue);
        if (hideGraphics) command += ", HideGraphics=1";
        command += " WHERE ProcNum=" + procNum;
        Db.NonQ(command);
        //resynch appointment description-------------------------------------------------------------------------------------
        if (aptNum != 0)
        {
            var apt = Appointments.GetOneApt(aptNum);
            var aptOld = apt.Copy();
            Appointments.SetProcDescript(apt);
            Appointments.Update(apt, aptOld);
        }

        if (plannedAptNum != 0)
        {
            var plannedApt = Appointments.GetOneApt(plannedAptNum);
            var plannedAptOld = plannedApt.Copy();
            Appointments.SetProcDescript(plannedApt);
            Appointments.Update(plannedApt, plannedAptOld);
        }
    }

    public static void TryDeleteLab(Procedure procLab)
    {
        try
        {
            Delete(procLab.ProcNum);
        }
        catch (Exception ex)
        {
            //The lab procedure could be attached to a claim or adjustment. If so, it seems harmless to keep the procedure around with a $0 fee.
            if (CompareDouble.IsEqual(procLab.ProcFee, 0)) return;
            var procOld = procLab.Copy();
            procLab.ProcFee = 0;
            Update(procLab, procOld);
        }
    }

    public static void DeleteCanadianLabFeesForProcCode(long procNum)
    {
        var command = "SELECT * FROM procedurelog WHERE ProcNumLab=" + procNum + " AND ProcStatus!=" + SOut.Int((int) ProcStat.D);
        var labFeeProcs = ProcedureCrud.SelectMany(command);
        for (var i = 0; i < labFeeProcs.Count; i++) Delete(labFeeProcs[i].ProcNum);
    }
    
    public static void DiscountChangeSecLogEntry(Procedure procNew, Procedure procOld)
    {
        if (procNew.Discount == procOld.Discount) return;
        var message = Lans.g("Procedures", "Discount created or changed from Proc Edit window for procedure")
                      + ": " + ProcedureCodes.GetProcCode(procNew.CodeNum).ProcCode + "  " + Lans.g("Procedures", "Dated")
                      + ": " + procNew.ProcDate.ToShortDateString() + "  " + Lans.g("Procedures", "With a Fee of") + ": " + procNew.ProcFee.ToString("c") + ".  "
                      + Lans.g("Procedures", "Changed the discount value from") + " " + procOld.Discount.ToString("c") + " " + Lans.g("Procedures", "to") + " "
                      + procNew.Discount.ToString("c");
        SecurityLogs.MakeLogEntry(EnumPermType.TreatPlanDiscountEdit, procNew.PatNum, message);
    }
    
    public static void UpdateTpProcPriority(Procedure proc)
    {
        if (proc.ProcStatus != ProcStat.TP) return;
        //if proc is TP status, update priority on any TreatPlanAttach objects if they are attaching this proc to the active TP
        var activePlan = TreatPlans.GetActiveForPat(proc.PatNum);
        if (activePlan == null) return;
        var listTpAttaches = TreatPlanAttaches.GetAllForTreatPlan(activePlan.TreatPlanNum);
        //should only be 0 or one TPAttach on this TP with this ProcNum
        listTpAttaches.FindAll(x => x.ProcNum == proc.ProcNum).ForEach(x => x.Priority = proc.Priority);
        TreatPlanAttaches.Sync(listTpAttaches, activePlan.TreatPlanNum);
    }

    public static bool ProcWasSetComplete(Procedure procNew, Procedure procOld, ProcedureCode procCode)
    {
        if (procOld.ProcStatus == ProcStat.C || procNew.ProcStatus != ProcStat.C) return false;
        //Auto-insert default encounter -------------------------------------------------------------------------------------------------------------
        Encounters.InsertDefaultEncounter(procNew.PatNum, procNew.ProvNum, procNew.ProcDate);
        //OrthoProcedures ---------------------------------------------------------------------------------------------------------------------------
        SetOrthoProcComplete(procNew, procCode);
        //if status was changed to complete
        LogProcComplCreate(procNew.PatNum, procNew, procNew.ToothNum);
        return true;
    }

    public static void SetToothInitialForCompExtraction(Procedure proc)
    {
        if (!proc.ProcStatus.In(ProcStat.C, ProcStat.EC, ProcStat.EO)
            || ProcedureCodes.GetProcCode(proc.CodeNum).PaintType != ToothPaintingType.Extraction)
            return;
        ToothInitials.SetValue(proc.PatNum, proc.ToothNum, ToothInitialType.Missing);
    }

    public static List<long> GetForInvoice(long statementNum)
    {
        if (statementNum == 0) return [];
        var command = "SELECT ProcNum FROM procedurelog WHERE procedurelog.StatementNum = " + statementNum;
        return Db.GetListLong(command);
    }

    public static void ValidateDelete(long procNum)
    {
        //Test to see if the procedure is attached to a claim (excluding pre-auths)
        var command = "SELECT COUNT(*) FROM claimproc WHERE ProcNum=" + procNum
                                                                      + " AND ClaimNum > 0 AND Status!=" + SOut.Int((int) ClaimProcStatus.Preauth);
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to a claim."));
        //Test to see if any payment at all has been received for this proc
        command = "SELECT COUNT(*) FROM claimproc WHERE ProcNum=" + procNum
                                                                  + " AND InsPayAmt > 0 AND Status IN (" + SOut.Int((int) ClaimProcStatus.Received) + "," + SOut.Int((int) ClaimProcStatus.Supplemental) + ","
                                                                  + SOut.Int((int) ClaimProcStatus.CapClaim) + "," + SOut.Int((int) ClaimProcStatus.CapComplete) + ")";
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to an insurance payment."));
        //Test to see if any referrals exist for this proc
        command = "SELECT COUNT(*) FROM refattach WHERE ProcNum=" + procNum;
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure with referrals attached."));
        //Test to see if any paysplits are attached to this proc
        command = "SELECT COUNT(*) FROM paysplit WHERE ProcNum=" + procNum;
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to a patient payment."));
        command = "SELECT COUNT(*) FROM adjustment WHERE ProcNum=" + procNum;
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to an adjustment."));
        command = "SELECT COUNT(*) FROM rxpat WHERE ProcNum=" + procNum;
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to a prescription."));
        command = $"SELECT COUNT(*) FROM payplancharge WHERE payplancharge.ProcNum={procNum}";
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to a payment plan."));
        command = $"SELECT COUNT(*) FROM payplanlink WHERE payplanlink.FKey={procNum} " +
                  $"AND payplanlink.LinkType={SOut.Int((int) PayPlanLinkType.Procedure)}";
        if (Db.GetCount(command) != "0") throw new Exception(Lans.g("Procedures", "Not allowed to delete a procedure that is attached to a payment plan."));
    }

    public static Procedure CreateOrthoAutoProcsForPat(long patNum, long codeNum, long provNum, long clinicNum, DateTime procDate)
    {
        var procedure = new Procedure();
        procedure.PatNum = patNum;
        procedure.CodeNum = codeNum;
        procedure.ProvNum = provNum;
        procedure.ClinicNum = clinicNum;
        procedure.ProcStatus = ProcStat.C;
        procedure.Surf = "";
        procedure.ToothNum = "";
        procedure.UserNum = Security.CurUser.UserNum;
        procedure.ProcDate = procDate;
        procedure.DateEntryC = DateTime.Today;
        procedure.SecDateEntry = DateTime.Today;
        procedure.ProcFee = 0;
        procedure.PlaceService = Clinics.GetPlaceService(clinicNum);
        procedure.ProcNum = Insert(procedure);
        return procedure;
    }

    public static void CreateSalesTaxProc(long patNum, List<Procedure> listProcs)
    {
        var listProcNums = listProcs.Select(x => x.ProcNum).ToList();
        var listClaimProcs = ClaimProcs.RefreshForProcs(listProcNums);
        var procedureSalesTax = new Procedure();
        procedureSalesTax.PatNum = patNum;
        procedureSalesTax.ProcStatus = ProcStat.C;
        procedureSalesTax.Surf = "";
        procedureSalesTax.ToothNum = "";
        procedureSalesTax.UserNum = Security.CurUser.UserNum;
        procedureSalesTax.ProcDate = DateTime.Today;
        //Need to be smart about ProvNum if we want to be able to make a claim.
        var provNumSalesTax = PrefC.GetLong(PrefName.SalesTaxDefaultProvider);
        if (provNumSalesTax == 0) provNumSalesTax = Patients.GetProvNum(patNum);
        procedureSalesTax.ProvNum = provNumSalesTax;
        var procCodeSalesTax = PrefC.GetString(PrefName.SalesTaxProcCode);
        procedureSalesTax.CodeNum = ProcedureCodes.GetCodeNum(procCodeSalesTax);
        procedureSalesTax.DateTP = DateTime.Today;
        procedureSalesTax.DateEntryC = DateTime.Today;
        procedureSalesTax.PlaceService = Clinics.GetPlaceService(Clinics.ClinicNum);
        for (var i = 0; i < listProcs.Count; i++)
        {
            var salesTax = ClaimProcs.ComputeSalesTax(listProcs[i], listClaimProcs, false);
            procedureSalesTax.ProcFee += salesTax;
        }

        Insert(procedureSalesTax);
    }

    public static bool AreAnyComplete(long patNum)
    {
        var command = "SELECT COUNT(*) FROM procedurelog "
                      + "INNER JOIN procedurecode on procedurecode.CodeNum = procedurelog.CodeNum "
                      + "AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
                      + "WHERE PatNum=" + patNum
                      + " AND ProcStatus=2";
        var table = DataCore.GetTable(command);
        if (table.Rows[0][0].ToString() == "0") return false;

        return true;
    }

    public static bool WillBeMissing(string toothNum, long patNum)
    {
        //first, check for missing teeth
        var command = "SELECT COUNT(*) FROM toothinitial "
                      + "WHERE ToothNum='" + toothNum + "' "
                      + "AND PatNum=" + patNum
                      + " AND InitialType=0"; //missing
        var table = DataCore.GetTable(command);
        if (table.Rows[0][0].ToString() != "0") return true;
        //then, check for a planned extraction
        command = "SELECT COUNT(*) FROM procedurelog,procedurecode "
                  + "WHERE procedurelog.CodeNum=procedurecode.CodeNum "
                  + "AND procedurelog.ToothNum='" + toothNum + "' "
                  + "AND procedurelog.PatNum=" + patNum + " "
                  + "AND procedurelog.ProcStatus <> " + SOut.Int((int) ProcStat.D) + " " //Not deleted procedures
                  + "AND procedurelog.ProcStatus <> " + SOut.Int((int) ProcStat.TPi) + " " //Not inactive treatment planned procedures
                  + "AND procedurecode.PaintType=1"; //extraction
        table = DataCore.GetTable(command);
        if (table.Rows[0][0].ToString() != "0") return true;
        return false;
    }

    public static bool NoBillIns(Procedure proc, List<ClaimProc> claimProcList, long planNum)
    {
        if (proc == null) return false;
        for (var i = 0; i < claimProcList.Count; i++)
            if (claimProcList[i].ProcNum == proc.ProcNum
                && claimProcList[i].PlanNum == planNum
                && claimProcList[i].NoBillIns)
                return true;

        return false;
    }

    public static bool IsAttachedToClaim(Procedure proc, List<ClaimProc> claimProcList, bool isPreauthIncluded = true)
    {
        for (var i = 0; i < claimProcList.Count; i++)
            if (claimProcList[i].ProcNum == proc.ProcNum
                && claimProcList[i].ClaimNum > 0
                && (claimProcList[i].Status == ClaimProcStatus.CapClaim
                    || claimProcList[i].Status == ClaimProcStatus.NotReceived
                    || (claimProcList[i].Status == ClaimProcStatus.Preauth && isPreauthIncluded)
                    || claimProcList[i].Status == ClaimProcStatus.Received
                    || claimProcList[i].Status == ClaimProcStatus.Supplemental
                ))
                return true;

        return false;
    }

    public static DateTime GetOldestClaimDate(List<ClaimProc> claimProcsForProc, bool includePreAuth = true)
    {
        Claim claim;
        var retVal = DateTime.Today;
        for (var i = 0; i < claimProcsForProc.Count; i++)
        {
            if (claimProcsForProc[i].ClaimNum == 0) continue;
            if (claimProcsForProc[i].Status == ClaimProcStatus.CapClaim
                || claimProcsForProc[i].Status == ClaimProcStatus.NotReceived
                || (claimProcsForProc[i].Status == ClaimProcStatus.Preauth && includePreAuth)
                || claimProcsForProc[i].Status == ClaimProcStatus.Received
                || claimProcsForProc[i].Status == ClaimProcStatus.Supplemental
               )
            {
                claim = Claims.GetClaim(claimProcsForProc[i].ClaimNum);
                if (claim == null) //Another user probably deleted this claim just now.
                    continue;
                if (claim.DateSent < retVal) retVal = claim.DateSent;
            }
        }

        return retVal;
    }

    public static DateTime GetOldestPreAuth(List<ClaimProc> claimProcsForProc)
    {
        Claim claim;
        var retVal = DateTime.Today;
        for (var i = 0; i < claimProcsForProc.Count; i++)
            if (claimProcsForProc[i].ClaimNum != 0 && claimProcsForProc[i].Status == ClaimProcStatus.Preauth)
            {
                claim = Claims.GetClaim(claimProcsForProc[i].ClaimNum);
                if (claim != null && claim.DateSent < retVal) retVal = claim.DateSent;
            }

        return retVal;
    }

    public static DateTime GetDateForPermCheck(Procedure proc, DateTime dateOverride = default)
    {
        var date = dateOverride == DateTime.MinValue ? proc.ProcDate : dateOverride;
        if (proc.ProcStatus.In(ProcStat.EO, ProcStat.EC, ProcStat.TP, ProcStat.TPi)) date = proc.DateEntryC;
        return date;
    }

    public static bool IsAttachedToClaim(List<Procedure> procList, List<ClaimProc> claimprocList)
    {
        for (var j = 0; j < procList.Count; j++)
            if (IsAttachedToClaim(procList[j], claimprocList))
                return true;

        return false;
    }

    public static bool IsAttachedToClaim(long procNum)
    {
        var command = "SELECT COUNT(*) FROM claimproc "
                      + "WHERE ProcNum=" + procNum + " "
                      + "AND ClaimNum>0";
        var table = DataCore.GetTable(command);
        if (table.Rows[0][0].ToString() == "0") return false;
        return true;
    }

    public static bool IsAlreadyAttachedToClaim(Procedure proc, List<ClaimProc> claimProcList, long insSubNum)
    {
        for (var i = 0; i < claimProcList.Count; i++)
            if (claimProcList[i].ProcNum == proc.ProcNum
                && claimProcList[i].InsSubNum == insSubNum
                && claimProcList[i].ClaimNum > 0
                && claimProcList[i].Status != ClaimProcStatus.Preauth)
                return true;

        return false;
    }

    public static bool IsReferralAttached(long referralNum)
    {
        var command = "SELECT COUNT(*) FROM procedurelog WHERE OrderingReferralNum=" + referralNum;
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    public static bool NeedsSent(long procNum, long insSubNum, List<ClaimProc> listClaimProcs)
    {
        for (var i = 0; i < listClaimProcs.Count; i++)
            if (listClaimProcs[i].ProcNum == procNum
                && !listClaimProcs[i].NoBillIns
                && listClaimProcs[i].InsSubNum == insSubNum
                && listClaimProcs[i].Status == ClaimProcStatus.Estimate)
                return true;

        return false;
    }

    public static ClaimProc GetClaimProcEstimate(long procNum, List<ClaimProc> claimProcList, InsPlan plan, long insSubNum)
    {
        //bool matchOfWrongType=false;
        for (var i = 0; i < claimProcList.Count; i++)
            if (claimProcList[i].ProcNum == procNum
                && !claimProcList[i].NoBillIns
                && claimProcList[i].PlanNum == plan.PlanNum
                && claimProcList[i].InsSubNum == insSubNum)
            {
                if (plan.PlanType == "c")
                {
                    if (claimProcList[i].Status == ClaimProcStatus.CapComplete) return claimProcList[i];
                }
                else
                {
                    //any type except capitation
                    if (claimProcList[i].Status == ClaimProcStatus.Estimate) return claimProcList[i];
                }
            }

        return null;
    }

    public static string ConvertProcToString(long codeNum, string surf, string toothNum, bool forAccount)
    {
        var code = ProcedureCodes.GetProcCode(codeNum);
        var strLine = GetToothAndSurfForCodeNum(codeNum, surf, toothNum, !forAccount);
        if (!forAccount)
            strLine += " " + code.AbbrDesc;
        else if (code.LaymanTerm != "")
            strLine += " " + code.LaymanTerm;
        else
            strLine += " " + code.Descript;
        return strLine;
    }
    
    private static string GetToothAndSurfForCodeNum(long codeNum, string surf, string toothNum, bool hasToothNum)
    {
        var strLine = "";
        var code = ProcedureCodes.GetProcCode(codeNum);
        switch (code.TreatArea)
        {
            case TreatmentArea.Surf:
                if (hasToothNum) strLine += "#" + Tooth.Display(toothNum) + "-"; //"#12-"
                strLine += Tooth.SurfTidyFromDbToDisplay(surf, toothNum); //"MOD-"
                break;
            case TreatmentArea.Tooth:
                if (hasToothNum) strLine += "#" + Tooth.Display(toothNum) + "-"; //"#12-"
                break;
            case TreatmentArea.Quad:
                strLine += surf + "-"; //"UL-"
                break;
            case TreatmentArea.Sextant:
                strLine += "S" + Tooth.GetSextant(surf, (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers)) + "-"; //"S2-"
                break;
            case TreatmentArea.Arch:
                strLine += surf + "-"; //"U-"
                break;
            case TreatmentArea.ToothRange:
                //strLine+=table.Rows[j][13].ToString()+" ";//don't show range
                break;
        } //end switch

        return strLine;
    }

    public static string GetDescription(Procedure proc, bool forAccount = false)
    {
        return ConvertProcToString(proc.CodeNum, proc.Surf, proc.ToothNum, forAccount);
    }

    public static string GetDescriptionForLetter(Procedure proc)
    {
        var code = ProcedureCodes.GetProcCode(proc.CodeNum);
        string retVal;
        if (code.LaymanTerm != "")
            retVal = code.LaymanTerm;
        else
            retVal = code.Descript;
        var toothSurf = GetToothAndSurfForCodeNum(code.CodeNum, proc.Surf, proc.ToothNum, true).TrimEnd('-');
        if (!string.IsNullOrWhiteSpace(toothSurf)) retVal += " " + toothSurf;
        return retVal;
    }

    public static Procedure ChangeProcInAppointment(Appointment apt, Procedure proc)
    {
        if (!IsProcComplEditAuthorized(proc))
            //This check is redundant but helps future calls to not miss the security check.
            return proc; //Don't make any changes to procedure.
        if (proc.ProcStatus != ProcStat.C)
        {
            var procCode = ProcedureCodes.GetProcCode(proc.CodeNum);
            proc.ProvNum = GetProvNumFromAppointment(apt, procCode);
        }

        proc.ClinicNum = apt.ClinicNum;
        if (proc.ProcStatus == ProcStat.TP && apt.AptDateTime != DateTime.MinValue)
            if ((proc.PlannedAptNum == apt.AptNum && proc.AptNum == 0) || apt.AptStatus != ApptStatus.Planned)
                proc.ProcDate = apt.AptDateTime;

        return proc;
    }

    private static bool IsProcComplAuthorized(EnumPermType perm, Procedure proc, bool includeCodeNumAndFee = false)
    {
        if (!proc.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC)) return true; //Don't check security if the procedure isn't completed (or EO/EC).
        var dateForPerm = GetDateForPermCheck(proc);
        perm = GroupPermissions.SwitchExistingPermissionIfNeeded(perm, proc);
        if (includeCodeNumAndFee) return Security.IsAuthorized(perm, dateForPerm, proc.CodeNum, proc.ProcFee);

        return Security.IsAuthorized(perm, dateForPerm, true);
    }

    public static bool IsProcComplDeleteAuthorized(Procedure proc, bool includeCodeNumAndFee = false)
    {
        return IsProcComplAuthorized(EnumPermType.ProcCompleteStatusEdit, proc, includeCodeNumAndFee);
    }

    public static bool IsProcComplEditAuthorized(Procedure proc, bool includeCodeNumAndFee = false)
    {
        return IsProcComplAuthorized(EnumPermType.ProcCompleteEdit, proc, includeCodeNumAndFee);
    }

    public static bool ShouldFeesChange(List<Procedure> listNewProcs, List<Procedure> listOldProcs, ref string promptText, ProcFeeHelper procFeeHelper)
    {
        //this method was called FeeUpdatePromptHelper
        switch (PrefC.GetInt(PrefName.ProcFeeUpdatePrompt))
        {
            case 0: //No prompt, don't change fee
                return false;
            case 1: //No prompt, always change fee
                //No prompt or check required. Equivilent to clicking "Yes" in some sense. Returns true.
                return true;
            case 2: //Prompt if patient portion would be different
                var listClaimProcs = ClaimProcs.GetForProcs(listNewProcs.Select(x => x.ProcNum).ToList());
                var listAdjustments = Adjustments.GetForProcs(listNewProcs.Select(x => x.ProcNum).ToList());
                foreach (var proc in listNewProcs)
                {
                    var procOld = listOldProcs.FirstOrDefault(x => x.ProcNum == proc.ProcNum);
                    if (procOld == null) continue;
                    procFeeHelper.FillData();
                    proc.ProcFee = GetProcFee(procFeeHelper.Pat, procFeeHelper.ListPatPlans, procFeeHelper.ListInsSubs, procFeeHelper.ListInsPlans, proc, procFeeHelper.ListBenefitsPrimary, procFeeHelper.ListFees);
                    var procOldPatPortion = ClaimProcs.GetPatPortion(procOld, listClaimProcs, listAdjustments);
                    var orthoProcLink = OrthoCaseProcedureLinker.CreateOrUpdateOrthoProcLink(procOld, proc);
                    ComputeEstimates(proc, procFeeHelper.Pat.PatNum, ref listClaimProcs, false, procFeeHelper.ListInsPlans,
                        procFeeHelper.ListPatPlans, procFeeHelper.ListBenefitsPrimary, null, null, false, procFeeHelper.Pat.Age,
                        procFeeHelper.ListInsSubs, null, false, false, null,
                        false, procFeeHelper.ListFees, null, orthoProcLink);
                    var procCurPatPortion = ClaimProcs.GetPatPortion(proc, listClaimProcs, listAdjustments);
                    if (procCurPatPortion != procOldPatPortion)
                    {
                        promptText = Lans.g(nameof(Procedures), "The procedure's newly selected provider will change the fee.  Would you like to update the procedure's fee to the newly selected provider's fee?");
                        return true;
                    }
                }

                return false;
            case 3: //Prompt if procedure fee amount is different per our manual
                for (var i = 0; i < listNewProcs.Count; i++)
                {
                    var proc = listNewProcs[i];
                    var procOld = listOldProcs.FirstOrDefault(x => x.ProcNum == proc.ProcNum);
                    if (procOld == null) // New procedure, so fee can't be changed since it didn't exist before
                        continue;
                    procFeeHelper.FillData();
                    proc.ProcFee = GetProcFee(procFeeHelper.Pat, procFeeHelper.ListPatPlans, procFeeHelper.ListInsSubs, procFeeHelper.ListInsPlans, proc, procFeeHelper.ListBenefitsPrimary, procFeeHelper.ListFees);
                    if (proc.ProcFee != procOld.ProcFee)
                    {
                        promptText = Lans.g(nameof(Procedures), "Would you like to update procedure fee amounts to the newly selected provider's fees?");
                        return true;
                    }
                }

                return false;
        }

        return false;
    }

    public static long GlobalUpdateFees(List<Fee> listFeesHQ, long clinicNumGlobal, string progressText)
    {
        //There are three parts to this:
        //1. Spawn multiple threads to queue up DataTables, each with 10,000 procedures.
        //2. In the main thread, process the DataTables and calculate new fees.
        //This requires going to the db for fees. The old way was to use the global fee cache.
        //The new way does something very similar, but with local lists instead of global fee cache.
        //This isn't better/worse/faster/slower than the old way.  It's just part of removing global fee cache. 
        //At the beginning, we get all fees with ClinicNum of zero.
        //Then, with each new clinic, we get all fees with matching ClinicNum.  Takes a few seconds.
        //This may fail (and always did fail) above a certain number (thousands?) of fee schedules because local machine will run out of memory, etc.
        //We will need a copy of such a database to determine new strategy.
        //3. On multiple threads, update the fees on procedures in blocks of 1000 identical fees.
        if (clinicNumGlobal == -1) throw new ApplicationException("Must specify ClinicNum or 0 for HQ.");
        if (_odThreadQueueData != null) throw new ApplicationException("Global update fees tool is already running.");
        ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper("Getting table of fees to update..."
            , progressBarEventType: ProgBarEventType.TextMsg));
        var s = new Stopwatch();
        if (/* ODBuild.IsDebug() */ false) s.Start();

        #region Create Thread Queue Data

        _odThreadQueueData = new ODThread(QueueDataBatches, clinicNumGlobal);
        _odThreadQueueData.Name = "GlobalUpdateFeesQueueDataThread";
        _odThreadQueueData.AddExceptionHandler(ex => { _isQueueDone = true; });
        _isQueueDone = false;
        lock (_lockObjQueueThread)
        {
            _queueDataTables = new Queue<DataTable>();
        }

        _listProcNumsMaxForGroups = GetProcNumMaxForGroups(ROWS_BATCH_MAX_SIZE, [ProcStat.TP], clinicNumGlobal);
        if (_totCount == 0 || _listProcNumsMaxForGroups.Count == 0)
        {
            //not likely to happen, this would mean there are 0 TP procedures in the db, nothing to do
            _odThreadQueueData = null;
            return 0;
        }

        _odThreadQueueData.Start();

        #endregion Create Queue Batch Data Thread

        #region Get Medical Fee Sched Dict

        var isMedFeeUsedForNewProcs = PrefC.GetBool(PrefName.MedicalFeeUsedForNewProcs);
        var dictPatNumMedFeeSchedNum = new Dictionary<long, long>();
        if (isMedFeeUsedForNewProcs)
        {
            var command = "SELECT patplan.PatNum,MAX(insplan.FeeSched) medFeeSched "
                          + "FROM patplan "
                          + "INNER JOIN ("
                          + "SELECT patplan.PatNum,MIN(patplan.Ordinal) Ordinal "
                          + "FROM patplan "
                          + "INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
                          + "INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum AND insplan.IsMedical "
                          + "GROUP BY patplan.PatNum ";
            command += "ORDER BY NULL";
            command += ") medplan ON medplan.PatNum=patplan.PatNum AND medplan.Ordinal=patplan.Ordinal "
                       + "INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
                       + "INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum "
                       + "GROUP BY patplan.PatNum ";
            command += "ORDER BY NULL";
            dictPatNumMedFeeSchedNum = DataCore.GetTable(command).Select()
                .ToDictionary(x => SIn.Long(x["PatNum"].ToString()), x => SIn.Long(x["medFeeSched"].ToString()));
        }

        #endregion Get Medical Fee Sched Dict

        #region Get Variables Used By All Batches

        var rowSkippedCount = 0; //used to update progress bar
        var procFeesUpdatedCount = 0; //used to report number of fees updated to calling form
        var isInsPpoAlwaysUseUcrFee = PrefC.GetBool(PrefName.InsPpoAlwaysUseUcrFee);
        Lookup<FeeKey2, Fee> lookupFeesByCodeAndSched = null;
        var listFeesHQandClinic = Fees.GetByClinicNum(clinicNumGlobal); //could be empty for some clinics that don't use overrides
        listFeesHQandClinic.AddRange(listFeesHQ);
        lookupFeesByCodeAndSched = (Lookup<FeeKey2, Fee>) listFeesHQandClinic.ToLookup(x => new FeeKey2(x.CodeNum, x.FeeSched));
        //lookup will make it very fast to look up the fees we need.
        var practDefaultProvNum = PrefC.GetLong(PrefName.PracticeDefaultProv);
        var practDefaultProvFeeSched = Providers.GetFirstOrDefault(x => x.Id == practDefaultProvNum)?.FeeScheduleId ?? 0; //default to 0 if prov is not found
        var firstNonHiddenProvFeeSched = Providers.GetFirstOrDefault(x => !x.IsHidden)?.FeeScheduleId ?? 0; //default to 0 if all provs hidden (not likely to happen)
        var dictProvFeeSched = Providers.GetDeepCopy().ToDictionary(x => x.Id, x => x.FeeScheduleId??0);
        //dictionary of fee key linked to a list of lists of longs in order to keep each update statement limited to updating 1000 procedures per query
        var dictFeeListCodes = new Dictionary<double, List<List<long>>>();
        var table = new DataTable();
        long batchNumber = 0;
        var listBenefitsAll = new List<Benefit>();
        var listPatPlanNums = new List<long>();
        var listPlanNums = new List<long>();

        #endregion Get Variables Used By All Batches

        try
        {
            while (!_isQueueDone || _queueDataTables.Count > 0)
            {
                //if batch thread is done and queue is empty, loop is finished
                if (_queueDataTables.Count == 0)
                    //queueBatchThread must not be finished gathering batches but the queue is empty, give the batch thread time to catch up
                    continue;
                try
                {
                    lock (_lockObjQueueThread)
                    {
                        table = _queueDataTables.Dequeue();
                        if (/* ODBuild.IsDebug() */ false) Console.WriteLine("Main thread, dequeue batch, queue count: " + _queueDataTables.Count);
                    }
                }
                catch (Exception ex)
                {
                    //queue must be empty even though we just checked it before entering the while loop, just loop again and wait if necessary
                    continue;
                }

                batchNumber++;
                double currentRowCount = 0; //keeps track of both rows skipped and unskipped
                listPatPlanNums = table.Select().Select(x => SIn.Long(x["patPlanNum"].ToString())).ToList();
                listPlanNums = table.Select().Select(x => SIn.Long(x["planNum"].ToString())).ToList();
                listBenefitsAll = Benefits.GetAllForPlanNumsAndPatPlanNums(listPlanNums, listPatPlanNums);
                foreach (DataRow rowCur in table.Rows)
                {
                    #region Get Variables from DataRow

                    var codeNum = SIn.Long(rowCur["CodeNum"].ToString());
                    var clinicNum = SIn.Long(rowCur["ClinicNum"].ToString());
                    var procProvNum = SIn.Long(rowCur["ProvNum"].ToString());
                    var patPriProv = SIn.Long(rowCur["PriProv"].ToString());
                    var patFeeSched = SIn.Long(rowCur["patFeeSched"].ToString());
                    var medCodeNum = SIn.Long(rowCur["medCodeNum"].ToString());
                    var procNum = SIn.Long(rowCur["ProcNum"].ToString());
                    var procNumLab = SIn.Long(rowCur["ProcNumLab"].ToString());
                    var procFeeCur = SIn.Double(rowCur["ProcFee"].ToString());
                    var patNum = SIn.Long(rowCur["PatNum"].ToString());
                    var patPriPlanFeeSchedNum = SIn.Long(rowCur["planFeeSched"].ToString());
                    var patPlanNum = SIn.Long(rowCur["patPlanNum"].ToString());
                    var planNum = SIn.Long(rowCur["planNum"].ToString());
                    var planType = SIn.String(rowCur["planType"].ToString());
                    var exclusionRule = SIn.Enum<ExclusionRule>(rowCur["exclusionFeeRule"].ToString());
                    var percentComplete = Math.Ceiling(currentRowCount / table.Rows.Count * 100);
                    long feeSchedCur = 0;
                    double newFee;
                    if (CultureInfo.CurrentCulture.Name.EndsWith("CA")
                        && procNumLab != 0)
                    {
                        currentRowCount++;
                        ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(progressText, (int) percentComplete + "%"
                            , (int) percentComplete, 100, ProgBarStyle.Blocks, "Clinic"
                            , labelTop: "Batch " + batchNumber + "/" + _listProcNumsMaxForGroups.Count));
                        continue; //The proc fee for a lab is derived from the lab fee on the parent procedure.
                    }

                    #endregion Get Variables from DataRow

                    #region Med Fee Used and Proc Has Med CodeNum

                    if (isMedFeeUsedForNewProcs && medCodeNum > 0)
                        if (dictPatNumMedFeeSchedNum.TryGetValue(patNum, out feeSchedCur))
                        {
                            //use med plan fee sched first
                            if (feeSchedCur == 0 && patFeeSched > 0)
                                //if med plan fee sched is 0, use pat fee sched second
                                feeSchedCur = patFeeSched;
                            if (feeSchedCur == 0 && patPriProv > 0)
                                //if no pat fee sched, use pat pri prov fee sched third
                                dictProvFeeSched.TryGetValue(patPriProv, out feeSchedCur);
                            if (feeSchedCur == 0)
                                //if no pat pri prov fee sched, use first non-hidden prov fee sched last
                                feeSchedCur = firstNonHiddenProvFeeSched;
                        }

                    #endregion Med Fee Used and Proc Has Med CodeNum

                    #region Dental Fee Sched

                    if (feeSchedCur == 0) //not using med plan fees or no med plan for pat or no med codeNum for proc, basically no fee sched found yet
                        feeSchedCur = patPriPlanFeeSchedNum; //use pri plan fee sched first
                    if (feeSchedCur == 0 && patFeeSched > 0) //no pri plan fee sched, use pat fee sched second
                        feeSchedCur = patFeeSched;
                    if (feeSchedCur == 0 && procProvNum > 0) //no pat fee sched, use proc prov fee sched third
                        dictProvFeeSched.TryGetValue(procProvNum, out feeSchedCur);
                    if (feeSchedCur == 0 && patPriProv > 0) //no proc prov fee sched, use pat pri prov fee sched last
                        dictProvFeeSched.TryGetValue(patPriProv, out feeSchedCur);

                    #endregion Dental Fee Sched

                    if (isMedFeeUsedForNewProcs && medCodeNum > 0)
                    {
                        var listFeesForCodeAndSched = lookupFeesByCodeAndSched[new FeeKey2(medCodeNum, feeSchedCur)].ToList();
                        newFee = Fees.GetAmount0(medCodeNum, feeSchedCur, clinicNum, procProvNum, listFeesForCodeAndSched);
                    }
                    else
                    {
                        var listFeesForCodeAndSched = lookupFeesByCodeAndSched[new FeeKey2(codeNum, feeSchedCur)].ToList();
                        newFee = Fees.GetAmount0(codeNum, feeSchedCur, clinicNum, procProvNum, listFeesForCodeAndSched);
                    }

                    #region PPO Plan, Might Use UCR Fee

                    if (planType == "p")
                    {
                        //PPO plan, might use UCR fee
                        feeSchedCur = 0;
                        if (patPriProv > 0) //use pat pri prov fee sched first
                            dictProvFeeSched.TryGetValue(patPriProv, out feeSchedCur);
                        if (feeSchedCur == 0 && practDefaultProvFeeSched > 0) //no pat pri prov fee sched, use practice default prov fee sched second
                            feeSchedCur = practDefaultProvFeeSched;
                        if (feeSchedCur == 0) //no practice default prov fee sched, use first non-hidden prov fee sched last
                            feeSchedCur = firstNonHiddenProvFeeSched;
                        double ucrFee = 0;
                        var listFeesForCodeAndSched = lookupFeesByCodeAndSched[new FeeKey2(codeNum, feeSchedCur)].ToList();
                        ucrFee = Fees.GetAmount0(codeNum, feeSchedCur, clinicNum, procProvNum, listFeesForCodeAndSched);
                        if (newFee < ucrFee || isInsPpoAlwaysUseUcrFee) newFee = ucrFee;
                    }

                    #endregion PPO Plan, Might Use UCR Fee

                    #region Exclusions

                    if (InsPlans.UsesUcrFeeForExclusions(exclusionRule))
                    {
                        var listBenefits = listBenefitsAll.FindAll(x => x.PatPlanNum == patPlanNum || x.PlanNum == planNum);
                        if (!listBenefits.IsNullOrEmpty() && (Benefits.IsExcluded(ProcedureCodes.GetStringProcCode(codeNum), listBenefits, planNum, patPlanNum)
                                                              || Benefits.GetPercent(ProcedureCodes.GetStringProcCode(codeNum), planType, planNum, patPlanNum, listBenefits) == 0))
                        {
                            //Get the fee from the provider's fee schedule (ucr fee)
                            dictProvFeeSched.TryGetValue(patPriProv, out feeSchedCur);
                            var listFeesForCodeAndSched = lookupFeesByCodeAndSched[new FeeKey2(codeNum, feeSchedCur)].ToList();
                            newFee = Fees.GetAmount0(codeNum, feeSchedCur, clinicNum, patPriProv, listFeesForCodeAndSched);
                        }
                    }

                    #endregion Exclusions

                    if (CompareDouble.IsEqual(newFee, procFeeCur))
                    {
                        rowSkippedCount++;
                        currentRowCount++;
                        ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(progressText, (int) percentComplete + "%"
                            , (int) percentComplete, 100, ProgBarStyle.Blocks, "Clinic"
                            , labelTop: "Batch " + batchNumber + "/" + _listProcNumsMaxForGroups.Count));
                        continue;
                    }

                    if (!dictFeeListCodes.ContainsKey(newFee)) dictFeeListCodes[newFee] = [new(UPDATE_PROCNUM_IN_MAX_SIZE)];
                    if (dictFeeListCodes[newFee].Last().Count >= UPDATE_PROCNUM_IN_MAX_SIZE) dictFeeListCodes[newFee].Add(new List<long>(UPDATE_PROCNUM_IN_MAX_SIZE));
                    dictFeeListCodes[newFee].Last().Add(procNum);
                    currentRowCount++;
                    //update batch label for progress bar
                    ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(progressText, (int) percentComplete + "%"
                        , (int) percentComplete, 100, ProgBarStyle.Blocks, "Clinic",
                        labelTop: "Batch " + batchNumber + "/" + _listProcNumsMaxForGroups.Count));
                } //end of foreach loop. Done with one batch of procedures.

                ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(
                    "Batch " + batchNumber + "/" + _listProcNumsMaxForGroups.Count + " fee process completed"
                    , progressBarEventType: ProgBarEventType.TextMsg));
            } //end of while loop. Done with all batches of procedures.

            ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(progressText, 100 + "%"
                , 100, 100, ProgBarStyle.Blocks, "Clinic", labelTop: "Batch " + batchNumber + "/" + _listProcNumsMaxForGroups.Count));
            ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(
                "Procedure fees processed " + progressText + " " + _totCount + "/" + _totCount
                , progressBarEventType: ProgBarEventType.TextMsg));
            if (dictFeeListCodes.Count == 0) return 0; //no procedure fees updated, all skipped
            ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper(
                "Updating fees...", progressBarEventType: ProgBarEventType.TextMsg));

            #region Create List of Actions

            var listActions = dictFeeListCodes.SelectMany(x => x.Value.Select(y => new Action(() =>
            {
                var command = "UPDATE procedurelog SET ProcFee=" + SOut.Double(x.Key) + " WHERE ProcNum IN (" + string.Join(",", y) + ")";
                var s1 = new Stopwatch();
                if (/* ODBuild.IsDebug() */ false) s1.Start();
                Db.NonQ(command);
                if (/* ODBuild.IsDebug() */ false)
                {
                    s1.Stop();
                    Console.WriteLine("Updated " + y.Count + " procedures, runtime: " + s1.Elapsed.TotalSeconds + " sec");
                }

                procFeesUpdatedCount += y.Count;
            }))).ToList();

            #endregion Create List of Actions

            ODThread.RunParallel(listActions, TimeSpan.FromMinutes(30)
                , onException: ex =>
                {
                    //Notify the user what went wrong via the text box.
                    ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper("Error updating ProcFee: " + ex.Message
                        , progressBarEventType: ProgBarEventType.TextMsg));
                }); //each group of actions gets X minutes.
            ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper("Fees Updated Successfully",
                progressBarEventType: ProgBarEventType.TextMsg));
        } //end of try
        catch (Exception ex)
        {
        }
        finally
        {
            _odThreadQueueData?.QuitAsync();
            _odThreadQueueData = null;
        }

        if (/* ODBuild.IsDebug() */ false)
        {
            s.Stop();
            Console.WriteLine("Runtime: " + s.Elapsed.Minutes + " min " + (s.Elapsed.TotalSeconds - s.Elapsed.Minutes * 60) + " sec");
        }

        return procFeesUpdatedCount;
    }

    private static void QueueDataBatches(ODThread odThread)
    {
        var s = new Stopwatch();
        if (/* ODBuild.IsDebug() */ false) s.Start();
        try
        {
            var isMedFeeUsedForNewProcs = PrefC.GetBool(PrefName.MedicalFeeUsedForNewProcs);
            var clinicNumGlobal = (long) odThread.Parameters[0];
            var listQueries = new List<string>();
            for (var i = 0; i < _listProcNumsMaxForGroups.Count; i++)
            {
                #region Get ProcNum Range and ClinicNum Where Clauses

                var listWhereAnds = new List<string>();
                if (i > 0) listWhereAnds.Add("procedurelog.ProcNum>" + _listProcNumsMaxForGroups[i - 1]);
                if (i < _listProcNumsMaxForGroups.Count - 1) listWhereAnds.Add("procedurelog.ProcNum<=" + _listProcNumsMaxForGroups[i]);
                if (true && clinicNumGlobal > -1) //only add clinic restriction if a clinic was passed in. Defaults to -1.
                    listWhereAnds.Add("procedurelog.ClinicNum=" + clinicNumGlobal);

                #endregion Get ProcNum Range and ClinicNum Where Clauses

                #region Get Query String

                //Get all TP procedures in this batch's ProcNum range.
                var command = "SELECT procedurelog.PatNum,procedurelog.ProcNum,procedurelog.CodeNum,procedurelog.ClinicNum,procedurelog.ProvNum,"
                              + "procedurelog.ProcFee,patient.PriProv,patient.FeeSched patFeeSched,procedurelog.ProcNumLab,"
                              + (isMedFeeUsedForNewProcs ? "COALESCE(procedurecode.CodeNum,0) medCodeNum," : "0 medCodeNum,")
                              + "IFNULL(MAX(CASE WHEN p.PatNum IS NOT NULL THEN insplan.PlanType ELSE '' END),'') PlanType," //greater of "",c,f,or p. p is what we care about.
                              + "IFNULL(MAX(CASE WHEN patplan.Ordinal=1 THEN insplan.FeeSched ELSE 0 END),0) planFeeSched, " //handles multiple primary patplans
                              + "IFNULL(patplan.PatPlanNum,0) patPlanNum, "
                              + "IFNULL(insplan.PlanNum,0) planNum, "
                              + "IFNULL(insplan.ExclusionFeeRule,0) exclusionFeeRule "
                              + "FROM procedurelog "
                              + "INNER JOIN patient ON patient.PatNum=procedurelog.PatNum "
                              + "LEFT JOIN ("
                              + "SELECT patplan.PatNum,MIN(patplan.Ordinal) minOrdinal "
                              + "FROM patplan "
                              + "INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
                              + "INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum " //AND !insplan.IsMedical "
                              + "GROUP BY patplan.PatNum"
                              + ") p ON patient.PatNum=p.PatNum "
                              + "LEFT JOIN patplan ON patplan.PatNum=patient.PatNum AND (patplan.Ordinal=p.MinOrdinal OR patplan.Ordinal=1) "
                              + "LEFT JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
                              + "LEFT JOIN insplan ON insplan.PlanNum=inssub.PlanNum "
                              + (isMedFeeUsedForNewProcs ? "LEFT JOIN procedurecode ON procedurecode.ProcCode=procedurelog.MedicalCode " : "")
                              + "WHERE procedurelog.ProcStatus IN (" + SOut.Int((int) ProcStat.TP) + "," + SOut.Int((int) ProcStat.TPi) + ") "
                              + (listWhereAnds.Count > 0 ? "AND " + string.Join(" AND ", listWhereAnds) + " " : "")
                              + "GROUP BY procedurelog.ProcNum " //because sometimes a pat can have more than one primary patplan and DBM only has "manual fix needed" for this problem
                              + "ORDER BY NULL"; //an old mysql trick to improve performance. Trick is obsolete as of 5.6.
                listQueries.Add(command);

                #endregion Get Query String
            }

            #region Create List of Actions

            var listActions = listQueries.Select(x => new Action(() =>
            {
                var s1 = new Stopwatch();
                if (/* ODBuild.IsDebug() */ false) s1.Start();
                var table = DataCore.GetTable(x);
                if (/* ODBuild.IsDebug() */ false) s1.Stop();
                if (table.Rows.Count > 0)
                {
                    while (_queueDataTables.Count > 5)
                        //wait until queue is at reasonable size before queueing more. We don't want to hold more than 5 datatables in memory at once. 
                        Thread.Sleep(1);
                    lock (_lockObjQueueThread)
                    {
                        _queueDataTables.Enqueue(table);
                        if (/* ODBuild.IsDebug() */ false) Console.WriteLine(odThread.Name + " - enqueue batch, queue count: " + _queueDataTables.Count + ", runtime: " + s1.Elapsed.TotalSeconds + " sec");
                    }
                }
            })).ToList();

            #endregion Create List of Actions

            ODThread.RunParallel(listActions, TimeSpan.FromMinutes(30), onException: ex =>
            {
                //Notify the user what went wrong via the text box.
                ODEvent.Fire(ODEventType.FeeSched, new ProgressBarHelper("Error getting TP procedures batch: " + ex.Message
                    , progressBarEventType: ProgBarEventType.TextMsg));
            });
        }
        catch (Exception ex)
        {
        }
        finally
        {
            //always make sure to notify the main thread that the thread is done so the main thread doesn't wait for eternity
            _isQueueDone = true;
            if (/* ODBuild.IsDebug() */ false)
            {
                s.Stop();
                Console.WriteLine(odThread.Name + " - Done, enqueue total count: " + _listProcNumsMaxForGroups.Count
                                  + ", thread runtime: " + s.Elapsed.Minutes + " min " + (s.Elapsed.TotalSeconds - s.Elapsed.Minutes * 60) + " sec");
            }
        }
    }

    public static List<Procedure> SortListByTreatPlanPriority(List<Procedure> listProcs, List<TreatPlanAttach> listTreatPlanAttaches = null)
    {
        return SortListByTreatPlanPriority(listProcs, PrefC.GetBool(PrefName.TreatPlanSortByTooth), listTreatPlanAttaches);
    }

    public static List<Procedure> SortListByTreatPlanPriority(List<Procedure> listProcs, bool isTreatPlanSortByTooth, List<TreatPlanAttach> listTreatPlanAttaches = null)
    {
        var dictPriorities = new Dictionary<long, int>();
        var dictProcNumPriority = new Dictionary<long, int>();
        var hasTreatPlanAttaches = false;
        //Check to see if a list of TreatPlanAttaches was passed in.  If so, use the treatplanattach priorities instead of the priority of the procedure that's in the database.
        //This is used for multiple active treatment plans where we need to display a sorted priority even though that procedure might not have the same priority in another
        //saved treatment plan and thus would have a different priority in the database.
        if (listTreatPlanAttaches != null)
        {
            hasTreatPlanAttaches = true;
            dictPriorities = Defs.GetDefsForCategory(DefCat.TxPriorities).ToDictionary(x => x.DefNum, x => x.ItemOrder);
            listTreatPlanAttaches.ForEach(x => dictProcNumPriority[x.ProcNum] = x.Priority == 0 ? -1 : dictPriorities[x.Priority]);
        }

        var listLabProcs = listProcs.Where(x => x.ProcNumLab != 0).Select(x => x.Copy()).ToList(); //Canadian Lab Procs
        var listLabProcNums = listLabProcs.Select(x => x.ProcNum).ToList();
        listProcs.RemoveAll(x => listLabProcNums.Contains(x.ProcNum)); //Remove all labs from this list if any.  Labs are always below their parent proc.
        //Procedure code is purposefully not included in the sorting of this list.  It will ruin PrefName.TreatPlanSortByTooth sorting.
        var listOrderedProcs = listProcs
            .OrderBy(x => (hasTreatPlanAttaches ? dictProcNumPriority[x.ProcNum] : x.PriorityOrder) < 0)
            .ThenBy(x => hasTreatPlanAttaches ? dictProcNumPriority[x.ProcNum] : x.PriorityOrder)
            //.ThenBy(x => isTreatPlanSortByTooth ? x.ToothRange : 0)
            .ThenBy(x => isTreatPlanSortByTooth ? Tooth.ToInt(x.ToothNum) : 0) //Sorting by a constant causes nothing to happen.
            .ThenBy(x => x.ProcDate)
            .ThenBy(x => x.ProcNum) //This is necessary in case isTreatPlanSortByTooth is false, and to break any ties between the other sorting methods.
            .ToList();
        var listParentProcNums = listLabProcs.Select(x => x.ProcNumLab).Distinct().ToList(); //There can be up to 2 lab procs for each parent proc.
        for (var i = listOrderedProcs.Count - 1; i >= 0; i--)
        {
            //Loop backward so we can insert as we go without affecting the index.
            if (!listParentProcNums.Contains(listOrderedProcs[i].ProcNum)) continue; //Not a parent proc.
            listOrderedProcs.InsertRange(i + 1, listLabProcs.Where(x => x.ProcNumLab == listOrderedProcs[i].ProcNum).ToList()); //Insert labs below parent proc.
        }

        return listOrderedProcs;
    }

    public static string CheckFrequency(List<Procedure> procList, long patNum, DateTime aptDateTime)
    {
        if (procList == null) throw new ArgumentException("Invalid procedure list passed in.", "procList");
        var pat = Patients.GetPat(patNum);
        if (pat == null) throw new ArgumentException("Patient not found in database.", "patNum");
        if (aptDateTime == null) throw new ArgumentException("Appointment Date not present.", "aptDateTime");
        var procListNew = procList.Select(x => x.Copy()).ToList(); //Because we're modifying the procedures in this method
        var frequencyConflicts = "";
        var listPatPlans = PatPlans.GetPatPlansForPat(patNum);
        if (!PatPlans.IsPatPlanListValid(listPatPlans))
            //need to validate due to call to GetHistList below
            listPatPlans = PatPlans.Refresh(patNum);
        if (listPatPlans.Count < 1) return "";
        var listInsSubs = InsSubs.GetMany(listPatPlans.Select(x => x.InsSubNum).ToList());
        var listInsPlans = InsPlans.GetByInsSubs(listInsSubs.Select(x => x.InsSubNum).ToList());
        var listBenefits = Benefits.Refresh(listPatPlans, listInsSubs);
        listBenefits.AddRange(Benefits.GetForPatPlansAndProcs(listPatPlans.Select(x => x.PatPlanNum).ToList(), procListNew.Select(x => x.CodeNum).ToList()));
        var listClaimProcsHist = ClaimProcs.GetHistList(patNum, listBenefits, listPatPlans, listInsPlans, DateTime.Now, listInsSubs);
        var listClaimProcsForProcs = ClaimProcs.GetForProcs(procListNew.Select(x => x.ProcNum).ToList());
        if (aptDateTime != DateTime.MinValue)
        {
            foreach (var proc in procListNew) proc.ProcDate = aptDateTime.Date;
            foreach (var claimProc in listClaimProcsForProcs) claimProc.ProcDate = aptDateTime.Date;
        }

        //Get data for any OrthoCases that may be linked to procs in procListNew
        var listOrthoCases = OrthoCases.Refresh(patNum);
        var listOrthoProcLinksAll = OrthoProcLinks.GetManyByOrthoCases(listOrthoCases.Select(x => x.OrthoCaseNum).ToList());
        var listProcNums = procListNew.Select(x => x.ProcNum).ToList();
        var listOrthoProcLinks = listOrthoProcLinksAll.FindAll(x => listProcNums.Contains(x.ProcNum));
        var listOrthoSchedules = new List<OrthoSchedule>();
        if (listOrthoProcLinks.Count > 0)
        {
            var listSchedulePlanLinksFKey = OrthoPlanLinks.GetAllForOrthoCasesByType(listOrthoCases.Select(x => x.OrthoCaseNum).ToList(), OrthoPlanLinkType.OrthoSchedule).Select(x => x.FKey).ToList();
            listOrthoSchedules = OrthoSchedules.GetMany(listSchedulePlanLinksFKey);
        }

        var listSubstLinks = SubstitutionLinks.GetAllForPlans(listInsPlans);
        var blueBookEstimateData = new BlueBookEstimateData(listInsPlans, listInsSubs, listPatPlans, procListNew, listSubstLinks);
        for (var i = 0; i < procListNew.Count; i++)
        {
            OrthoCase orthoCase = null;
            OrthoSchedule orthoSchedule = null;
            List<OrthoProcLink> listOrthoProcLinksForOrthoCase = null;
            var orthoProcLink = listOrthoProcLinks.Find(x => x.ProcNum == procListNew[i].ProcNum);
            if (orthoProcLink != null)
            {
                var orthoCaseNum = orthoProcLink.OrthoCaseNum;
                orthoCase = listOrthoCases.Find(x => x.OrthoCaseNum == orthoCaseNum);
                orthoSchedule = listOrthoSchedules.Find(x => x.OrthoScheduleNum == orthoCaseNum);
                listOrthoProcLinksForOrthoCase = listOrthoProcLinksAll.FindAll(x => x.OrthoCaseNum == orthoCaseNum);
            }

            ComputeEstimates(procListNew[i], pat.PatNum, ref listClaimProcsForProcs, false, listInsPlans, listPatPlans, listBenefits, listClaimProcsHist, null, false, pat.Age
                , listInsSubs, listSubstLinks: listSubstLinks, orthoProcLink: orthoProcLink, orthoCase: orthoCase, orthoSchedule: orthoSchedule
                , listOrthoProcLinksForOrthoCase: listOrthoProcLinksForOrthoCase, blueBookEstimateData: blueBookEstimateData);
            var claimProc = listClaimProcsForProcs.Find(x => x.ProcNum == procListNew[i].ProcNum);
            if (claimProc != null && !string.IsNullOrEmpty(claimProc.EstimateNote) && claimProc.EstimateNote.Contains("Frequency Limitation"))
            {
                if (frequencyConflicts != "") frequencyConflicts += "\r\n";
                frequencyConflicts += ProcedureCodes.GetStringProcCode(procListNew[i].CodeNum);
            }
        }

        return frequencyConflicts;
    }
    
    public static void ComputeEstimates(Procedure proc, long patNum, List<ClaimProc> claimProcs, bool isInitialEntry, List<InsPlan> planList, List<PatPlan> patPlans, List<Benefit> benefitList, int patientAge, List<InsSub> subList, OrthoProcLink orthoProcLink = null, OrthoCase orthoCase = null, OrthoSchedule orthoSchedule = null, List<OrthoProcLink> listOrthoProcLinksForOrthoCase = null, List<Fee> listFees = null, BlueBookEstimateData blueBookEstimateData = null)
    {
        ComputeEstimates(proc, patNum, ref claimProcs, isInitialEntry, planList, patPlans, benefitList,
            null, null, true,
            patientAge, subList,
            null, false, false, null, false, //null,false,false,ListSubstLinks,false
            listFees, null, orthoProcLink, orthoCase, orthoSchedule, listOrthoProcLinksForOrthoCase, blueBookEstimateData);
    }

    public static void ComputeEstimates(
        Procedure proc,
        long patNum,
        ref List<ClaimProc> claimProcs,
        bool isInitialEntry,
        List<InsPlan> planList,
        List<PatPlan> patPlans,
        List<Benefit> benefitList,
        List<ClaimProcHist> histList,
        List<ClaimProcHist> loopList,
        bool saveToDb, //can default to null,null,true
        int patientAge,
        List<InsSub> subList,
        List<ClaimProc> listClaimProcsAll = null,
        bool isClaimProcRemoveNeeded = false,
        bool useProcDateOnProc = false,
        List<SubstitutionLink> listSubstLinks = null,
        bool isForOrtho = false,
        List<Fee> listFees = null,
        Lookup<FeeKey2, Fee> lookupFees = null,
        OrthoProcLink orthoProcLink = null,
        OrthoCase orthoCase = null,
        OrthoSchedule orthoSchedule = null,
        List<OrthoProcLink> listOrthoProcLinksForOrthoCase = null,
        BlueBookEstimateData blueBookEstimateData = null,
        List<long> listApptNums = null)
    {
        if (PrefC.GetBool(PrefName.EnterpriseHygProcUsePriProvFee) && ProcedureCodes.GetProcCode(proc.CodeNum).IsHygiene)
        {
            if (listApptNums == null)
            {
                listApptNums =
                [
                    proc.AptNum,
                    proc.PlannedAptNum
                ];
                listApptNums = listApptNums.FindAll(x => x != 0).Distinct().ToList();
            }

            proc = proc.Copy();
            Appointment appointment = null;
            if (listApptNums.Contains(proc.AptNum))
            {
                appointment = Appointments.GetOneApt(proc.AptNum);
                if (appointment != null) proc.ProvNum = appointment.ProvNum;
            }
            else if (listApptNums.Contains(proc.PlannedAptNum))
            {
                appointment = Appointments.GetOneApt(proc.PlannedAptNum);
                if (appointment != null) proc.ProvNum = appointment.ProvNum;
            }
        }

        //If an orthoCase or an orthProcLink was passed in with the other orthocase related data left null, we must get that data.
        if (orthoProcLink != null || orthoCase != null)
        {
            long orthoCaseNum = 0;
            if (orthoProcLink != null)
            {
                orthoCaseNum = orthoProcLink.OrthoCaseNum;
                if (orthoCase == null) orthoCase = OrthoCases.GetOne(orthoProcLink.OrthoCaseNum);
            }
            else if (orthoCase != null)
            {
                orthoCaseNum = orthoCase.OrthoCaseNum;
                if (orthoProcLink == null) orthoProcLink = OrthoProcLinks.GetByProcNum(proc.ProcNum);
            }

            if (orthoSchedule == null)
            {
                var orthoSchedulePlanLink = OrthoPlanLinks.GetOneForOrthoCaseByType(orthoCaseNum, OrthoPlanLinkType.OrthoSchedule);
                orthoSchedule = OrthoSchedules.GetOne(orthoSchedulePlanLink.FKey);
            }

            if (listOrthoProcLinksForOrthoCase == null) listOrthoProcLinksForOrthoCase = OrthoProcLinks.GetManyByOrthoCase(orthoCaseNum);
        }

        if (listClaimProcsAll == null) listClaimProcsAll = claimProcs;
        var isHistorical = false;
        if (proc.ProcDate < DateTime.Today && proc.ProcStatus == ProcStat.C)
        {
            isHistorical = true; //Don't automatically create an estimate for completed procedures, especially if they are older than today.  Very important after a conversion from another software.
            //Special logic in place only for capitation plans:

            var listInsSubsForPatPlanActive = subList.FindAll(x => patPlans.Any(y => x.InsSubNum == y.InsSubNum));
            var listInsPlansForPatActive = planList.FindAll(x => listInsSubsForPatPlanActive.Any(y => x.PlanNum == y.PlanNum));
            if (listInsPlansForPatActive.Any(x => x.PlanType == "c") //11/19/2012 js We had a specific complaint where changing plan type to capitation automatically added WOs to historical procs.
                && !listClaimProcsAll.Any(x => x.ProcNum == proc.ProcNum
                                               && x.Status.In(ClaimProcStatus.CapClaim, ClaimProcStatus.CapComplete, ClaimProcStatus.CapEstimate)))
                //If there are any capitation plans but no capitation claimproc.statuses then return.
                //04/02/2013 Jason- To relax this filter for offices that enter treatment a few days after it's done, we will see if any capitation statuses exist.
                return; //There are no capitation claimprocs for this procedure, therefore we don't want to touch/damage this proc.
        }

        //first test to see if each estimate matches an existing patPlan (current coverage),
        //delete any other estimates
        var listDeletedClaimProcNums = new List<long>();
        for (var i = 0; i < claimProcs.Count; i++)
        {
            if (claimProcs[i].ProcNum != proc.ProcNum) continue;
            if (claimProcs[i].PlanNum == 0) continue;
            if (claimProcs[i].Status != ClaimProcStatus.Estimate && claimProcs[i].Status != ClaimProcStatus.CapEstimate) continue;
            var planIsCurrent = false;
            for (var p = 0; p < patPlans.Count; p++)
                if (patPlans[p].InsSubNum == claimProcs[i].InsSubNum
                    && InsSubs.GetSub(patPlans[p].InsSubNum, subList).PlanNum == claimProcs[i].PlanNum)
                {
                    planIsCurrent = true;
                    break;
                }

            //If claimProc estimate is for a plan that is not current, delete it
            if (!planIsCurrent)
            {
                if (saveToDb)
                    ClaimProcs.Delete(claimProcs[i]);
                else
                    claimProcs[i].DoDelete = true;
                listDeletedClaimProcNums.Add(claimProcs[i].ClaimProcNum);
            }
        }

        if (isClaimProcRemoveNeeded)
            //Remove all claimProcs which were deleted.
            //This prevents Canadian lab procedures from generating claimProcs for deleted parent claimProcs. 
            claimProcs.RemoveAll(x => listDeletedClaimProcNums.Contains(x.ClaimProcNum));
        InsPlan planCur;
        InsSub subCur;
        //bool estExists;
        var cpAdded = false;
        //loop through all patPlans (current coverage), and add any missing estimates
        for (var p = 0; p < patPlans.Count; p++)
        {
            //typically, loop will only have length of 1 or 2
            //Don't automatically create an estimate for completed procedures, especially if they are older than today.
            //However, we have an optional preference for users that knowingly accept this danger and have a workflow that requires this.
            if (isHistorical && !PrefC.GetBool(PrefName.ClaimProcsAllowedToBackdate) && !isForOrtho) break;

            if (proc.ProcNumLab != 0) break; //Do not add estimates for labs, they are handeld by their parent procedure.
            var patPlanCur = patPlans[p];
            //test to see if estimate exists
            if (listClaimProcsAll.Any(x => x.ProcNum == proc.ProcNum
                                           && x.PlanNum != 0
                                           && x.InsSubNum == patPlanCur.InsSubNum
                                           && !x.Status.In(ClaimProcStatus.CapClaim, ClaimProcStatus.Preauth, ClaimProcStatus.Supplemental)))
                continue; //estimate exists
            //estimate is missing, so add it.
            subCur = InsSubs.GetSub(patPlanCur.InsSubNum, subList);
            planCur = InsPlans.GetPlan(subCur.PlanNum, planList);
            if (planCur == null) //subCur can never be null) {//??
                continue; //??
            var cp = new ClaimProc();
            cp.ProcNum = proc.ProcNum;
            cp.PatNum = patNum;
            cp.ProvNum = proc.ProvNum;
            if (planCur.PlanType == "c")
            {
                if (proc.ProcStatus == ProcStat.C)
                    cp.Status = ClaimProcStatus.CapComplete;
                else
                    cp.Status = ClaimProcStatus.CapEstimate; //this may be changed below
            }
            else
            {
                cp.Status = ClaimProcStatus.Estimate;
            }

            cp.PlanNum = planCur.PlanNum;
            cp.InsSubNum = subCur.InsSubNum;
            //Capitation procedures are not usually attached to a claim.
            //In order for Aging to calculate properly the ProcDate (Date Completed) and DateCP (Payment Date) must be the same.
            //If the following line of code changes, then we need to preserve this existing behavior specifically for CapComplete.
            cp.DateCP = proc.ProcDate;
            cp.AllowedOverride = -1;
            cp.PercentOverride = -1;
            cp.NoBillIns = InsPlanPreferences.NoBillIns(ProcedureCodes.GetProcCode(proc.CodeNum), planCur);
            cp.PaidOtherIns = -1;
            cp.CopayOverride = -1;
            cp.ProcDate = proc.ProcDate;
            cp.BaseEst = 0;
            cp.InsEstTotal = 0;
            cp.InsEstTotalOverride = -1;
            cp.DedEst = -1;
            cp.DedEstOverride = -1;
            cp.PaidOtherInsOverride = -1;
            cp.WriteOffEst = -1;
            cp.WriteOffEstOverride = -1;
            //ComputeBaseEst will fill AllowedOverride,Percentage,CopayAmt,BaseEst
            if (saveToDb)
                ClaimProcs.Insert(cp);
            else
                claimProcs.Add(cp); //this newly added cp has no ClaimProcNum and is not yet in the db.
            cpAdded = true;
        }

        //if any were added, refresh the list
        if (cpAdded && saveToDb) //no need to refresh the list if !saveToDb, because list already made current.
            claimProcs = ClaimProcs.Refresh(patNum);
        if (orthoProcLink != null)
        {
            //For procs linked to Orthocases,estimates are calculated based off of orthocase info, not insurance info.
            //Need all claimprocs for pat, including any that were added but not inserted yet.
            if (cpAdded && saveToDb)
            {
                listClaimProcsAll = claimProcs;
            }
            else
            {
                listClaimProcsAll = ClaimProcs.Refresh(patNum);
                listClaimProcsAll.AddRange(claimProcs.Where(x => x.ClaimProcNum == 0).ToList()); //Add any that are made but not in DB yet.
            }

            ClaimProcs.ComputeEstimatesByOrthoCase(proc, orthoProcLink, orthoCase, orthoSchedule, saveToDb, listClaimProcsAll, claimProcs, patPlans
                , listOrthoProcLinksForOrthoCase);
            return;
        }

        double paidOtherInsEstTotal = 0;
        double paidOtherInsBaseEst = 0;
        double writeOffEstOtherIns = 0;
        if (listSubstLinks == null) listSubstLinks = SubstitutionLinks.GetAllForPlans(planList);
        if (listFees != null && lookupFees == null)
            //we are just dealing with a very short list of fees, so this doesn't cost anything.
            lookupFees = (Lookup<FeeKey2, Fee>) listFees.ToLookup(x => new FeeKey2(x.CodeNum, x.FeeSched));
        if (blueBookEstimateData == null) blueBookEstimateData = new BlueBookEstimateData(planList, subList, patPlans, [proc], listSubstLinks);
        //because secondary claimproc might come before primary claimproc in the list, we cannot simply loop through the claimprocs
        ComputeForOrdinal(1, claimProcs, proc, planList, isInitialEntry, ref paidOtherInsEstTotal, ref paidOtherInsBaseEst, ref writeOffEstOtherIns,
            patPlans, benefitList, histList, loopList, saveToDb, patientAge, subList, listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
        ComputeForOrdinal(2, claimProcs, proc, planList, isInitialEntry, ref paidOtherInsEstTotal, ref paidOtherInsBaseEst, ref writeOffEstOtherIns,
            patPlans, benefitList, histList, loopList, saveToDb, patientAge, subList, listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
        ComputeForOrdinal(3, claimProcs, proc, planList, isInitialEntry, ref paidOtherInsEstTotal, ref paidOtherInsBaseEst, ref writeOffEstOtherIns,
            patPlans, benefitList, histList, loopList, saveToDb, patientAge, subList, listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
        ComputeForOrdinal(4, claimProcs, proc, planList, isInitialEntry, ref paidOtherInsEstTotal, ref paidOtherInsBaseEst, ref writeOffEstOtherIns,
            patPlans, benefitList, histList, loopList, saveToDb, patientAge, subList, listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
        //At this point, for a PPO with secondary, the sum of all estimates plus primary writeoff might be greater than fee.
        if (patPlans.Count > 1)
        {
            subCur = InsSubs.GetSub(patPlans[0].InsSubNum, subList);
            planCur = InsPlans.GetPlan(subCur.PlanNum, planList);
            if (planCur.PlanType == "p")
            {
                //claimProcs=ClaimProcs.Refresh(patNum);
                //ClaimProc priClaimProc=null;
                var priClaimProcIdx = -1;
                double sumPay = 0; //Either actual or estimate
                for (var i = 0; i < claimProcs.Count; i++)
                {
                    if (claimProcs[i].ProcNum != proc.ProcNum) continue;
                    if (claimProcs[i].Status == ClaimProcStatus.Received && !PrefC.GetBool(PrefName.InsEstRecalcReceived)) continue;
                    if (claimProcs[i].Status.In(ClaimProcStatus.Adjustment, ClaimProcStatus.CapClaim, ClaimProcStatus.CapComplete, ClaimProcStatus.CapEstimate,
                            ClaimProcStatus.Preauth, ClaimProcStatus.InsHist))
                        continue;
                    if (claimProcs[i].PlanNum == planCur.PlanNum && claimProcs[i].WriteOffEst > 0) priClaimProcIdx = i;
                    if (claimProcs[i].Status == ClaimProcStatus.Received
                        || claimProcs[i].Status == ClaimProcStatus.Supplemental)
                        sumPay += claimProcs[i].InsPayAmt;
                    if (claimProcs[i].Status == ClaimProcStatus.Estimate)
                    {
                        if (!CompareDouble.IsEqual(claimProcs[i].InsEstTotalOverride, -1))
                            sumPay += claimProcs[i].InsEstTotalOverride;
                        else
                            sumPay += claimProcs[i].InsEstTotal;
                    }

                    if (claimProcs[i].Status == ClaimProcStatus.NotReceived) sumPay += claimProcs[i].InsPayEst;
                }

                //Alter primary WO if needed.
                if (priClaimProcIdx != -1)
                {
                    var procFee = proc.ProcFeeTotal;
                    if (sumPay + claimProcs[priClaimProcIdx].WriteOffEst > procFee)
                    {
                        var writeOffEst = procFee - sumPay;
                        if (writeOffEst < 0) writeOffEst = 0;
                        claimProcs[priClaimProcIdx].WriteOffEst = writeOffEst;
                        if (saveToDb) ClaimProcs.Update(claimProcs[priClaimProcIdx]);
                    }
                }
            }
        }

        #region Sales Tax

        //At the end of recalculating writeoffs, we need to update sales tax estimate for treatment planned procs.
        if (proc.ProcStatus == ProcStat.TP) proc.TaxAmt = ClaimProcs.ComputeSalesTax(proc, claimProcs, true);

        #endregion
    }

    private static void ComputeForOrdinal(int ordinal, List<ClaimProc> claimProcs, Procedure proc, List<InsPlan> planList, bool isInitialEntry, ref double paidOtherInsEstTotal, ref double paidOtherInsBaseEst, ref double writeOffEstOtherIns, List<PatPlan> patPlans, List<Benefit> benefitList, List<ClaimProcHist> histList, List<ClaimProcHist> loopList, bool saveToDb, int patientAge, List<InsSub> listInsSubs, List<SubstitutionLink> listSubstLinks, bool useProcDateOnProc, Lookup<FeeKey2, Fee> lookupFees, BlueBookEstimateData blueBookEstimateData)
    {
        InsPlan PlanCur;
        PatPlan patplan;
        var procCode = ProcedureCodes.GetProcCode(proc.CodeNum);
        for (var i = 0; i < claimProcs.Count; i++)
        {
            var hasEstimateCalculation = true;
            if (claimProcs[i].Status == ClaimProcStatus.Received && !PrefC.GetBool(PrefName.InsEstRecalcReceived))
                //Do not recalculate insurance estimates on received claimprocs.  However, we still need to include the insurance payments in the 
                //calculation of any secondary, tertiary, etc insurance plans for each procedure.  In this case, we want to make sure writeOffEstOtherIns,
                //paidOtherInsEstTotal, and paidOtherInsBaseEst include the Received claimproc estimates/payments, while still not changing the received 
                //claimproc estimates due to Frequency Limitations or Waiting Periods (calculated in this method, rather than ClaimProcs.ComputeBaseEst()).
                hasEstimateCalculation = false;
            if (claimProcs[i].ProcNum != proc.ProcNum) continue;
            PlanCur = InsPlans.GetPlan(claimProcs[i].PlanNum, planList);
            if (PlanCur == null) continue; //in older versions it still did a couple of small things even if plan was null, but don't know why
            //example:cap estimate changed to cap complete, and if estimate, then provnum set
            //but I don't see how PlanCur could ever be null
            patplan = PatPlans.GetFromList(patPlans, claimProcs[i].InsSubNum);
            //capitation estimates are always forced to follow the status of the procedure
            if (PlanCur.PlanType == "c"
                && (claimProcs[i].Status == ClaimProcStatus.CapComplete || claimProcs[i].Status == ClaimProcStatus.CapEstimate))
            {
                if (isInitialEntry)
                {
                    //this will be switched to CapComplete further down if applicable.
                    //This makes ComputeBaseEst work properly on new cap procs w status Complete
                    claimProcs[i].Status = ClaimProcStatus.CapEstimate;
                }
                else if (proc.ProcStatus == ProcStat.C)
                {
                    claimProcs[i].Status = ClaimProcStatus.CapComplete;
                    //Capitation procedures are not usually attached to a claim.
                    //In order for Aging to calculate properly the ProcDate (Date Completed) and DateCP (Payment Date) must be the same.
                    claimProcs[i].DateCP = proc.ProcDate;
                }
                else
                {
                    claimProcs[i].Status = ClaimProcStatus.CapEstimate;
                }
            }

            //ignored: adjustment,InsHist
            //ComputeBaseEst automatically skips: capComplete,Preauth,capClaim,Supplemental
            //does recalc est on: CapEstimate,Estimate,NotReceived,Received
            //the cp is altered within ComputeBaseEst, but not saved unless cp is associated
            //to a lab then the sibling labs claimProcs are updated in CanadianLabBaseEstHelper(...)
            if (patplan == null)
            {
                //the plan for this claimproc was dropped 
                if (ordinal != 4) //only process on the fourth round
                    continue;
                ClaimProcs.ComputeBaseEst(claimProcs[i], proc, PlanCur, 0,
                    benefitList, histList, loopList, patPlans, 0, 0, patientAge, 0, planList, listInsSubs, listSubstLinks, useProcDateOnProc, lookupFees,
                    blueBookEstimateData);
            }
            else if (patplan.Ordinal == 1)
            {
                if (ordinal != 1) continue;
                ClaimProcs.ComputeBaseEst(claimProcs[i], proc, PlanCur, patplan.PatPlanNum,
                    benefitList, histList, loopList, patPlans, paidOtherInsEstTotal, paidOtherInsBaseEst, patientAge, writeOffEstOtherIns, planList, listInsSubs,
                    listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
            }
            else if (patplan.Ordinal == 2)
            {
                if (ordinal != 2) continue;
                ClaimProcs.ComputeBaseEst(claimProcs[i], proc, PlanCur, patplan.PatPlanNum,
                    benefitList, histList, loopList, patPlans, paidOtherInsEstTotal, paidOtherInsBaseEst, patientAge, writeOffEstOtherIns, planList, listInsSubs,
                    listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
            }
            else if (patplan.Ordinal == 3)
            {
                if (ordinal != 3) continue;
                ClaimProcs.ComputeBaseEst(claimProcs[i], proc, PlanCur, patplan.PatPlanNum,
                    benefitList, histList, loopList, patPlans, paidOtherInsEstTotal, paidOtherInsBaseEst, patientAge, writeOffEstOtherIns, planList, listInsSubs,
                    listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
            }
            else
            {
                //patplan.Ordinal is 4 or greater.  Estimate won't be accurate if more than 4 insurances.
                if (ordinal != 4) continue;
                ClaimProcs.ComputeBaseEst(claimProcs[i], proc, PlanCur, patplan.PatPlanNum,
                    benefitList, histList, loopList, patPlans, paidOtherInsEstTotal, paidOtherInsBaseEst, patientAge, writeOffEstOtherIns, planList, listInsSubs,
                    listSubstLinks, useProcDateOnProc, lookupFees, blueBookEstimateData);
            }

            var hasMetFrequencyLimit = HasMetFrequencyLimitation(claimProcs[i], histList, benefitList, proc, procCode, PlanCur, planList, patplan, loopList, listInsSubs);
            //Only sync the ProcDate if attached to a claim.
            //We know that all procedures associated to a claim or multi-visit group are complete when a claim has been created.
            //If the procedures are in a multi-visit group that is in process, it will need to default to today.
            //Once attached, date cannot be edited from procedure edit window, unless user clicks "Edit Anyway".
            if (claimProcs[i].ClaimNum == 0)
            {
                claimProcs[i].ProcDate = proc.ProcDate;
                var procMultiVisit = ProcMultiVisits.GetFirstOrDefault(x => x.ProcNum == proc.ProcNum);
                if (procMultiVisit != null)
                {
                    var listProcMultiVisits = ProcMultiVisits.GetWhere(x => x.GroupProcMultiVisitNum == procMultiVisit.GroupProcMultiVisitNum);
                    if (ProcMultiVisits.IsGroupInProcess(listProcMultiVisits)) claimProcs[i].ProcDate = DateTime.Today; //Default to today if Multi Visit Group is in Process
                }
            }

            claimProcs[i].ClinicNum = proc.ClinicNum;
            //Wish we could do this, but it might change history.  It's needed when changing a completed proc to a different provider.
            //Can't do it here, though, because some people intentionally set provider different on claimprocs.
            //claimProcs[i].ProvNum=proc.ProvNum;
            if (isInitialEntry
                && claimProcs[i].Status == ClaimProcStatus.CapEstimate
                && proc.ProcStatus == ProcStat.C)
            {
                claimProcs[i].Status = ClaimProcStatus.CapComplete;
                //Capitation procedures are not usually attached to a claim.
                //In order for Aging to calculate properly the ProcDate (Date Completed) and DateCP (Payment Date) must be the same.
                claimProcs[i].DateCP = proc.ProcDate;
            }

            //prov only updated if still an estimate
            if (claimProcs[i].Status == ClaimProcStatus.Estimate
                || claimProcs[i].Status == ClaimProcStatus.CapEstimate)
                claimProcs[i].ProvNum = proc.ProvNum;
            if (hasMetFrequencyLimit && hasEstimateCalculation)
            {
                claimProcs[i].BaseEst = 0;
                claimProcs[i].InsEstTotal = 0;
                claimProcs[i].DedEst = 0;
                claimProcs[i].DedEstOverride = -1;
                claimProcs[i].DedApplied = 0;
                if (claimProcs[i].EstimateNote != "") claimProcs[i].EstimateNote += ", ";
                claimProcs[i].EstimateNote += Lans.g("Procedures", "Frequency Limitation");
            }

            #region Waiting Periods

            //Determine if there is a waiting period associated to this procedure, if there is and it falls within the bounds of the waiting
            //period zero out the claimproc. Otherwise, continue through
            //get all WaitingPeriod Benefits
            var listWaitBenefits = benefitList.FindAll(x => x.BenefitType == InsBenefitType.WaitingPeriod
                                                            && (x.PlanNum == PlanCur.PlanNum || (patplan != null && x.PatPlanNum == patplan.PatPlanNum))
                                                            && (procCode.CodeNum == x.CodeNum || CodeGroups.IsProcInCodeGroup(procCode.ProcCode, x.CodeGroupNum) || CovSpans.IsCodeInSpans(procCode.ProcCode, CovSpans.GetForCat(x.CovCatNum))));
            //Check to see if we have any WaitingPeriod Benefits
            if (listWaitBenefits.Count > 0 && hasEstimateCalculation)
            {
                //WaitingPeriods were found, loop through them to see if they apply
                var hasProcCodeWaitPeriod = false;
                var hasCodeGroupWaitPeriod = false;
                var dateClaimProc = claimProcs[i].ProcDate;
                if (claimProcs[i].ProcDate < DateTime.Today)
                    //If claimproc.procdate is in the past then use today's date instead.
                    dateClaimProc = DateTime.Today;
                //first loop through to see if we have a matching proccode wait period. Apply it, then skip checking for category wait period
                foreach (var ben in listWaitBenefits)
                    if (procCode.CodeNum == ben.CodeNum)
                    {
                        hasProcCodeWaitPeriod = true;
                        var dateCutoff = new DateTime();
                        var inssubCur = InsSubs.GetSub(claimProcs[i].InsSubNum, listInsSubs);
                        if (ben.QuantityQualifier == BenefitQuantity.Months)
                            dateCutoff = inssubCur.DateEffective.AddMonths(ben.Quantity);
                        else if (ben.QuantityQualifier == BenefitQuantity.Years)
                            //Years
                            dateCutoff = inssubCur.DateEffective.AddYears(ben.Quantity);

                        //If still within waiting period, zero out the claim and add a note
                        if (dateClaimProc < dateCutoff.Date && dateClaimProc >= inssubCur.DateEffective.Date)
                        {
                            claimProcs[i].BaseEst = 0;
                            claimProcs[i].InsEstTotal = 0;
                            claimProcs[i].DedEst = 0;
                            claimProcs[i].DedEstOverride = -1;
                            claimProcs[i].DedApplied = 0;
                            if (claimProcs[i].EstimateNote != "") claimProcs[i].EstimateNote += ", ";
                            claimProcs[i].EstimateNote += Lans.g("Procedures", "Waiting Period");
                            break;
                        }
                    }

                if (!hasProcCodeWaitPeriod)
                    for (var b = 0; b < listWaitBenefits.Count; b++)
                    {
                        var isBenefitRelevant = CodeGroups.IsProcInCodeGroup(procCode.ProcCode, listWaitBenefits[b].CodeGroupNum);
                        if (!isBenefitRelevant) continue;
                        hasCodeGroupWaitPeriod = true;
                        var dateCutoff = new DateTime();
                        var inssubCur = InsSubs.GetSub(claimProcs[i].InsSubNum, listInsSubs);
                        if (listWaitBenefits[b].QuantityQualifier == BenefitQuantity.Months)
                            dateCutoff = inssubCur.DateEffective.AddMonths(listWaitBenefits[b].Quantity);
                        else if (listWaitBenefits[b].QuantityQualifier == BenefitQuantity.Years) dateCutoff = inssubCur.DateEffective.AddYears(listWaitBenefits[b].Quantity);

                        //If still within waiting period, zero out the claim and add a note
                        if (dateClaimProc < dateCutoff.Date && dateClaimProc >= inssubCur.DateEffective.Date)
                        {
                            claimProcs[i].BaseEst = 0;
                            claimProcs[i].InsEstTotal = 0;
                            claimProcs[i].DedEst = 0;
                            claimProcs[i].DedEstOverride = -1;
                            claimProcs[i].DedApplied = 0;
                            if (claimProcs[i].EstimateNote != "") claimProcs[i].EstimateNote += ", ";
                            claimProcs[i].EstimateNote += Lans.g("Procedures", "Waiting Period");
                            break;
                        }
                    }

                if (!hasProcCodeWaitPeriod && !hasCodeGroupWaitPeriod)
                    foreach (var ben in listWaitBenefits)
                    {
                        var dateCutoff = new DateTime();
                        var inssubCur = InsSubs.GetSub(claimProcs[i].InsSubNum, listInsSubs);
                        if (ben.QuantityQualifier == BenefitQuantity.Months)
                            dateCutoff = inssubCur.DateEffective.AddMonths(ben.Quantity);
                        else if (ben.QuantityQualifier == BenefitQuantity.Years)
                            //Years
                            dateCutoff = inssubCur.DateEffective.AddYears(ben.Quantity);

                        //If still within waiting period, zero out the claim and add a note
                        if (dateClaimProc < dateCutoff.Date && dateClaimProc >= inssubCur.DateEffective.Date)
                        {
                            claimProcs[i].BaseEst = 0;
                            claimProcs[i].InsEstTotal = 0;
                            claimProcs[i].DedEst = 0;
                            claimProcs[i].DedEstOverride = -1;
                            claimProcs[i].DedApplied = 0;
                            if (claimProcs[i].EstimateNote != "") claimProcs[i].EstimateNote += ", ";
                            claimProcs[i].EstimateNote += Lans.g("Procedures", "Waiting Period");
                            break;
                        }
                    }
            }

            #endregion

            if (hasMetFrequencyLimit && InsPlans.DoZeroOutWriteOffOnOtherLimitation(PlanCur) && ClaimProcs.GetEstimatedStatuses().Contains(claimProcs[i].Status)) claimProcs[i].WriteOffEst = 0;
            //If patplan.Ordinal is 1, 2, or 3 and the claim proc in question is not a preauth.
            //There is no such thing as having "paid by other ins" when dealing with preauths.
            if (patplan != null && new[] {1, 2, 3}.Contains(patplan.Ordinal) && claimProcs[i].Status != ClaimProcStatus.Preauth)
            {
                if (claimProcs[i].Status == ClaimProcStatus.Received || claimProcs[i].Status == ClaimProcStatus.Supplemental)
                {
                    paidOtherInsEstTotal += claimProcs[i].InsPayAmt;
                    paidOtherInsBaseEst += claimProcs[i].InsPayAmt;
                    writeOffEstOtherIns += claimProcs[i].WriteOff;
                }
                else
                {
                    if (claimProcs[i].InsEstTotalOverride != -1)
                        paidOtherInsEstTotal += claimProcs[i].InsEstTotalOverride;
                    else
                        paidOtherInsEstTotal += claimProcs[i].InsEstTotal;
                    paidOtherInsBaseEst += claimProcs[i].BaseEst;
                    writeOffEstOtherIns += ClaimProcs.GetWriteOffEstimate(claimProcs[i]);
                }
            }

            //Calculations done, copy over estimates from InsEstTotal into InsPayEst.  
            //This was already done in ComputeBaseEst but frequencies could have changed it
            //This could potentially be limited to claimprocs status Recieved or NotReceived, but there likely is no harm in doing it for all claimprocs.
            if (!CompareDouble.IsEqual(claimProcs[i].InsEstTotalOverride, -1))
                claimProcs[i].InsPayEst = claimProcs[i].InsEstTotalOverride;
            else
                claimProcs[i].InsPayEst = claimProcs[i].InsEstTotal;
            if (saveToDb)
            {
                //Only creates a log if blue book was used to calculate the estimate for this claimproc and the estimate has changed since last log.
                var blueBookLog = blueBookEstimateData.CreateInsBlueBookLog(claimProcs[i]);
                if (blueBookLog != null) InsBlueBookLogs.Insert(blueBookLog);
                ClaimProcs.Update(claimProcs[i]);
            }
        }
    }

    public static bool HasMetFrequencyLimitation(ClaimProc claimProc, List<ClaimProcHist> histList, List<Benefit> listBenefits, Procedure procedure, ProcedureCode procedureCode, InsPlan planCur, List<InsPlan> listInsPlans, PatPlan patPlan = null, List<ClaimProcHist> loopList = null, List<InsSub> listInsSubs = null)
    {
        if (histList == null || listBenefits == null || !PrefC.GetBool(PrefName.InsChecksFrequency) || procedure.ProcDate.Year < 1880 || claimProc.NoBillIns) return false;
        var listClaimProcHists = new List<ClaimProcHist>(histList);
        if (loopList != null) listClaimProcHists.AddRange(loopList);
        //Procedures not billed to insurance do not affect frequency limitations because the carrier is the entity which enforces the limit.
        listClaimProcHists.RemoveAll(x => x.NoBillIns);
        //In case we are recalculating the estimate for a procedure already attached to a claim, we need to make sure the histList does not include
        //the claim proc we are currently recalculating.
        listClaimProcHists.RemoveAll(x => x.ProcNum == claimProc.ProcNum && x.ClaimNum == claimProc.ClaimNum);
        long patPlanNum = 0;
        if (patPlan != null) patPlanNum = patPlan.PatPlanNum;
        var arrayClaimProcStatuses = new[]
        {
            ClaimProcStatus.Received, ClaimProcStatus.InsHist, ClaimProcStatus.NotReceived, ClaimProcStatus.Estimate, ClaimProcStatus.Supplemental
        };
        var listMatchingFrequencyBenefits = new List<BenefitProcCodes>(); //List of all frequency benefits matcihng the given procedure.
        for (var i = 0; i < listBenefits.Count; i++)
        {
            if (listBenefits[i].CodeNum == 0 && listBenefits[i].CodeGroupNum == 0)
                //Frequency limitation benefits are required to have a CodeNum or CodeGroupNum set.
                //Without either of these set, we do not know which procedure codes to check the frequency of.
                continue;
            if (listBenefits[i].PlanNum == 0 && listBenefits[i].PatPlanNum != patPlanNum) continue;
            if (listBenefits[i].PatPlanNum == 0 && listBenefits[i].PlanNum != planCur.PlanNum) continue;
            if (listBenefits[i].PlanNum != planCur.PlanNum && listBenefits[i].PatPlanNum != patPlanNum && patPlanNum != 0) continue; //Only look at frequency limitation benefits for the current plan, not all plans.
            if (!Benefits.IsFrequencyLimitation(listBenefits[i])) continue;
            //Figure out what procedure codes this frequency limitation benefit is associated with.
            var listProcCodes = new List<string>();
            var listCodeNums = new List<long>();
            if (listBenefits[i].CodeNum > 0)
            {
                listCodeNums.Add(listBenefits[i].CodeNum);
                listProcCodes.Add(ProcedureCodes.GetStringProcCode(listBenefits[i].CodeNum));
            }
            else if (listBenefits[i].CodeGroupNum > 0)
            {
                //Get the CodeGroup associated with this benefit.
                var codeGroup = CodeGroups.GetOne(listBenefits[i].CodeGroupNum);
                //Get all of the ProcCodes associated with the CodeGroup.
                var listProcedureCodes = ProcedureCodes.GetWhere(x => ProcedureCodes.IsCodeInList(x.ProcCode, codeGroup.ProcCodes));
                listProcCodes.AddRange(listProcedureCodes.Select(x => x.ProcCode));
                listCodeNums.AddRange(listProcedureCodes.Select(x => x.CodeNum));
            }

            //Remove empty entries that get added for invalid CodeNums.
            listProcCodes.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            //Remove all invalid CodeNums that get added due to invalid ProcCode strings.
            listCodeNums.RemoveAll(x => x == 0);
            //Skip this frequency limitation benefit if it is not associated with any valid procedure codes or has nothing to do with the current procedure being considered.
            if (listCodeNums.IsNullOrEmpty() || !listCodeNums.Contains(procedure.CodeNum)) continue;
            listMatchingFrequencyBenefits.Add(new BenefitProcCodes {Benefit = listBenefits[i], ListProcCodes = listProcCodes});
        }

        var listPatientOverrides = listMatchingFrequencyBenefits.FindAll(x => x.Benefit.PatPlanNum != 0);
        if (listPatientOverrides.Count > 0) listMatchingFrequencyBenefits = listPatientOverrides;
        listBenefits = listMatchingFrequencyBenefits.Select(x => x.Benefit).ToList();
        for (var i = 0; i < listBenefits.Count; i++)
        {
            //Find all relevant ClaimProcHist entries for this claimproc, benefit, procedure, and procedure code.
            var claimProcHistList = listClaimProcHists.FindAll(x => x.PatNum == procedure.PatNum
                                                                    && x.Status.In(arrayClaimProcStatuses)
                                                                    && x.InsSubNum == claimProc.InsSubNum
                                                                    && listMatchingFrequencyBenefits[i].ListProcCodes.Contains(x.StrProcCode)
                                                                    && IsSameProcedureArea(x.ToothNum, procedure.ToothNum, x.ToothRange, procedure.ToothRange, x.Surf, procedure.Surf, procedureCode.TreatArea, listBenefits[i].TreatArea));
            int quantityLimit;
            DateTime dateLowerBound;
            DateTime dateUpperBound;
            var isLowerBoundInclusive = true;
            if (listBenefits[i].QuantityQualifier == BenefitQuantity.Months || listBenefits[i].QuantityQualifier == BenefitQuantity.Years)
            {
                quantityLimit = 1;
                if (listBenefits[i].QuantityQualifier == BenefitQuantity.Months)
                    dateLowerBound = DateTimeOD.CalculateForEndOfMonthOffset(procedure.ProcDate, listBenefits[i].Quantity);
                else //Years
                    dateLowerBound = DateTimeOD.CalculateForEndOfMonthOffset(procedure.ProcDate, listBenefits[i].Quantity * 12);
                dateUpperBound = DateTime.MaxValue; //Preserve old behavior by having no upper bound.
            }
            else if (listBenefits[i].QuantityQualifier == BenefitQuantity.NumberOfServices)
            {
                quantityLimit = listBenefits[i].Quantity;
                //Calculate datePast based on the TimePeriod of the benefit.
                if (listBenefits[i].TimePeriod == BenefitTimePeriod.NumberInLast12Months)
                {
                    dateLowerBound = DateTimeOD.CalculateForEndOfMonthOffset(procedure.ProcDate, 12); //Exactly 12 months in the past.
                    //For NumberInLast12Months, we do not include procedure exactly 1 year ago. This is why our lower bound is not inclusive.
                    isLowerBoundInclusive = false;
                }
                else
                {
                    //CalendarYear or ServiceYear
                    //CalendarYear will always utilize January 1st.
                    dateLowerBound = new DateTime(procedure.ProcDate.Year, 1, 1);
                    //ServiceYear will be dictated by the value of MonthRenew on the corresponding insurance plan.
                    if (listBenefits[i].TimePeriod == BenefitTimePeriod.ServiceYear)
                    {
                        //Figure out the most accurate InsPlan to consider for this benefit.
                        var insPlan = listInsPlans.Find(x => x.PlanNum == listBenefits[i].PlanNum);
                        //If benefit is an override, use PatPlanNum to get the PlanNum to match to if InsSubs were provided.
                        if (!listInsSubs.IsNullOrEmpty() && patPlanNum != 0 && listBenefits[i].PatPlanNum == patPlanNum)
                        {
                            var insSub = listInsSubs.Find(x => x.InsSubNum == patPlan.InsSubNum);
                            if (insSub == null) continue; //The calling method did not provide the correct InsSubs.
                            insPlan = listInsPlans.Find(x => x.PlanNum == insSub.PlanNum);
                        }

                        if (insPlan == null) continue; //The MonthRenew value cannot be determined.
                        dateLowerBound = new DateTime(procedure.ProcDate.Year, Math.Max(insPlan.MonthRenew, (byte) 1), 1);
                        if (procedure.ProcDate.Date <= dateLowerBound && dateLowerBound > DateTime.Today) dateLowerBound = dateLowerBound.AddYears(-1);
                    }
                }

                dateUpperBound = dateLowerBound.AddYears(1);
            }
            else
            {
                //Unsupported QuantityQualifier
                continue;
            }

            //Each procedure has the potential to have multiple claimprocs associated with it.
            //Only count ClaimProcHist entries once per procedure.
            var quantityHist = claimProcHistList.Where(x => x.ProcDate.Between(dateLowerBound, dateUpperBound, isLowerBoundInclusive))
                .Where(x => x.Amount != 0 || x.Status == ClaimProcStatus.InsHist) //Unpaid procs do not count against quantity, except InsHist because amounts are always 0
                .DistinctBy(x => x.ProcNum)
                .Count();
            if (quantityHist >= quantityLimit) return true; //Frequency limitation has been met.
        }

        //All of the frequency limitation benefits have been considered and none of them were pertinent to the procedure being considered.
        return false;
    }

    public static bool IsSameProcedureArea(string histToothNum, string procCurToothNum, string histToothRangeStr, string procCurToothRangeStr, string histSurf, string procCurSurf, TreatmentArea procCurTreatArea, TreatmentArea benTreatArea = TreatmentArea.None)
    {
        //Procedures like exams and BW's do not ever specify a toothnum, toothrange, or surface.
        if (string.IsNullOrEmpty(histToothNum)
            && string.IsNullOrEmpty(procCurToothNum)
            && string.IsNullOrEmpty(histToothRangeStr)
            && string.IsNullOrEmpty(procCurToothRangeStr)
            && string.IsNullOrEmpty(histSurf)
            && string.IsNullOrEmpty(procCurSurf))
            return true;
        if (benTreatArea == TreatmentArea.Mouth) return true;

        var histToothRange = histToothRangeStr?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];
        var procCurToothRange = procCurToothRangeStr?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (benTreatArea == TreatmentArea.ToothRange)
        {
            if (histToothRange.Length == 0 && histSurf == "U")
                histToothRange = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"]; //See Tooth.IsMaxillary().
            else if (histToothRange.Length == 0 && histSurf == "L")
                histToothRange = ["17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32"];
            else if (procCurToothRange.Length == 0 && procCurSurf == "U")
                procCurToothRange = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"]; //See Tooth.IsMaxillary().
            else if (procCurToothRange.Length == 0 && procCurSurf == "L") procCurToothRange = ["17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32"];
        }

        var hasToothRangeOverlap = histToothRange.Intersect(procCurToothRange).Count() > 0;
        var hasCurToothInHistRange = histToothRange.Any(y => y != "" && y == procCurToothNum);
        var hasHistToothInCurRange = !string.IsNullOrWhiteSpace(histToothNum) && procCurToothRange.Contains(histToothNum);
        var hasSameToothNum = !string.IsNullOrWhiteSpace(procCurToothNum) && histToothNum == procCurToothNum;
        if (benTreatArea == TreatmentArea.Arch)
        {
            if (string.IsNullOrEmpty(histSurf) && histToothRange.Length > 0) histSurf = Tooth.IsMaxillary(histToothRange[0]) ? "U" : "L";
            if (string.IsNullOrEmpty(procCurSurf) && procCurToothRange.Length > 0) procCurSurf = Tooth.IsMaxillary(procCurToothRange[0]) ? "U" : "L";
        }

        var hasSameSurface = histSurf == procCurSurf;
        if (benTreatArea == TreatmentArea.Arch) return hasSameSurface;

        if (benTreatArea == TreatmentArea.Tooth) return hasSameToothNum;

        if (benTreatArea == TreatmentArea.ToothRange) return hasToothRangeOverlap;
        if (hasSameSurface
            && (hasSameToothNum
                || hasHistToothInCurRange
                || hasCurToothInHistRange
                || hasToothRangeOverlap
                || procCurTreatArea.In(TreatmentArea.Arch, TreatmentArea.Quad, TreatmentArea.Sextant)))
            return true;
        return false;
    }

    public static void ComputeEstimatesForAll(long patNum, List<ClaimProc> claimProcs, List<Procedure> procs, List<InsPlan> planList, List<PatPlan> patPlans, List<Benefit> benefitList, int patientAge, List<InsSub> subList, List<ClaimProc> listClaimProcsAll = null, bool isClaimProcRemoveNeeded = false, List<SubstitutionLink> listSubstLinks = null, List<Fee> listFees = null)
    {
        //Get data for any OrthoCases that may be linked to procs in procs list
        List<long> listApptNums = null;
        if (PrefC.GetBool(PrefName.EnterpriseHygProcUsePriProvFee))
        {
            listApptNums = [];
            for (var i = 0; i < procs.Count; i++)
            {
                listApptNums.Add(procs[i].AptNum);
                listApptNums.Add(procs[i].PlannedAptNum);
            }

            listApptNums = listApptNums.FindAll(x => x != 0).Distinct().ToList();
        }

        var listOrthoCases = OrthoCases.Refresh(patNum);
        var listOrthoProcLinksAllForPat = OrthoProcLinks.GetManyByOrthoCases(listOrthoCases.Select(x => x.OrthoCaseNum).ToList());
        var listProcNums = procs.Select(x => x.ProcNum).ToList();
        var listOrthoProcLinks = listOrthoProcLinksAllForPat.FindAll(x => listProcNums.Contains(x.ProcNum));
        var listOrthoSchedules = new List<OrthoSchedule>();
        if (listOrthoProcLinks.Count > 0)
        {
            var listSchedulePlanLinksFKey = OrthoPlanLinks.GetAllForOrthoCasesByType(listOrthoCases.Select(x => x.OrthoCaseNum).ToList(), OrthoPlanLinkType.OrthoSchedule).Select(x => x.FKey).ToList();
            listOrthoSchedules = OrthoSchedules.GetMany(listSchedulePlanLinksFKey);
        }

        var blueBookEstimateData = new BlueBookEstimateData(planList, subList, patPlans, procs, listSubstLinks);
        for (var i = 0; i < procs.Count; i++)
        {
            if (procs[i].ProcNumLab != 0)
                //Labs have their estimates calculated through their parents. Without this claimProcs have duplicated for a lab in the past.
                continue;
            var orthoProcLink = listOrthoProcLinks.Find(x => x.ProcNum == procs[i].ProcNum);
            OrthoCase orthoCase = null;
            OrthoSchedule orthoSchedule = null;
            List<OrthoProcLink> listOrthoProcLinksForOrthoCase = null;
            if (orthoProcLink != null)
            {
                var orthoCaseNum = orthoProcLink.OrthoCaseNum;
                orthoCase = listOrthoCases.Find(x => x.OrthoCaseNum == orthoCaseNum);
                orthoSchedule = listOrthoSchedules.Find(x => x.OrthoScheduleNum == orthoCaseNum);
                listOrthoProcLinksForOrthoCase = listOrthoProcLinksAllForPat.FindAll(x => x.OrthoCaseNum == orthoCaseNum);
            }

            ComputeEstimates(procs[i], patNum, ref claimProcs, false, planList, patPlans, benefitList,
                null, null, true,
                patientAge, subList,
                listClaimProcsAll, isClaimProcRemoveNeeded, false, listSubstLinks, false,
                listFees, null,
                orthoProcLink, orthoCase, orthoSchedule, listOrthoProcLinksForOrthoCase, blueBookEstimateData, listApptNums);
        }
    }

    public static List<Procedure> SetCompleteInApptInList(Appointment apt, List<InsPlan> planList, List<PatPlan> patPlans, Patient patient, List<Procedure> listProcsForAppt, List<InsSub> subList, Userod curUser)
    {
        if (listProcsForAppt.Count == 0) return listProcsForAppt; //Nothing to do.
        var claimProcList = ClaimProcs.Refresh(apt.PatNum);
        var benefitList = Benefits.Refresh(patPlans, subList);
        var histList = ClaimProcs.GetHistList(apt.PatNum, benefitList, patPlans, planList, -1, apt.AptDateTime, subList);
        var loopList = new List<ClaimProcHist>();
        //most recent note will be first in list.
        var command = "SELECT * FROM procnote "
                      + "WHERE ProcNum IN(" + string.Join(",", listProcsForAppt.Select(x => x.ProcNum)) + ") "
                      + "ORDER BY EntryDateTime DESC";
        var rawNotes = DataCore.GetTable(command);
        var listSubstLinks = SubstitutionLinks.GetAllForPlans(planList);
        var listProcedureCodes = new List<ProcedureCode>();
        for (var i = 0; i < listProcsForAppt.Count; i++)
        {
            var procedureCode = ProcedureCodes.GetProcCode(listProcsForAppt[i].CodeNum);
            listProcedureCodes.Add(procedureCode); //null and dups ok
        }

        var discountPlanNum = DiscountPlanSubs.GetDiscountPlanNumForPat(patient.PatNum, apt.AptDateTime);
        var listFees = Fees.GetListFromObjects(listProcedureCodes, listProcsForAppt.Select(x => x.MedicalCode).ToList(),
            listProcsForAppt.Select(x => x.ProvNum).ToList(), patient.PriProv, patient.SecProv, patient.FeeSched, planList,
            listProcsForAppt.Select(x => x.ClinicNum).ToList(), [apt], listSubstLinks, discountPlanNum);
        ProcedureCode procCode;
        Procedure procOld;
        var encounterProvNums = new List<long>(); //for auto-inserting default encounters
        //Get a list of any claimnums associated to procedures in the appoitments
        var listClaimNumsForApptProcs = claimProcList.Where(x => x.ClaimNum > 0 && listProcsForAppt.Select(y => y.ProcNum).Contains(x.ProcNum))
            .Select(x => x.ClaimNum).ToList();
        var histListForApptProcs = histList.Where(x => listClaimNumsForApptProcs.Contains(x.ClaimNum)).ToList();
        var orthoCaseProcedureLinker = OrthoCaseProcedureLinker.CreateOneForPatient(patient.PatNum);
        var blueBookEstimateData = new BlueBookEstimateData(planList, subList, patPlans, listProcsForAppt, listSubstLinks);
        foreach (var procCur in listProcsForAppt)
        {
            //Should only be procs for this appointment
            //attach the note, if it exists.
            foreach (DataRow row in rawNotes.Rows)
            {
                if (procCur.ProcNum.ToString() != row["ProcNum"].ToString()) continue;
                procCur.UserNum = SIn.Long(row["UserNum"].ToString());
                procCur.Note = SIn.String(row["Note"].ToString());
                procCur.SigIsTopaz = SIn.Bool(row["SigIsTopaz"].ToString());
                procCur.Signature = SIn.String(row["Signature"].ToString());
                break; //out of note loop.
            }

            procOld = procCur.Copy();
            procCode = ProcedureCodes.GetProcCode(procCur.CodeNum);
            if (procCode.PaintType == ToothPaintingType.Extraction) //if an extraction, then mark previous procs hidden
                //SetHideGraphical(procCur);//might not matter anymore
                ToothInitials.SetValue(apt.PatNum, procCur.ToothNum, ToothInitialType.Missing);
            procCur.ProcStatus = ProcStat.C;
            if (procOld.ProcStatus != ProcStat.C)
            {
                procCur.ProcDate = apt.AptDateTime.Date; //only change date to match appt if not already complete.
                if (procCur.ProcDate.Year < 1880) procCur.ProcDate = MiscData.GetNowDateTime().Date; //Change procdate to today if the appointment date was invalid
                procCur.DateEntryC = DateTime.Now; //this triggers it to set to server time NOW().
                if (procCur.DiagnosticCode == "")
                {
                    SetDiagnosticCodesToDefault(procCur, procCode);
                    procCur.IcdVersion = PrefC.GetByte(PrefName.DxIcdVersion);
                }
            }

            procCur.ClinicNum = apt.ClinicNum;
            procCur.SiteNum = patient.SiteNum;
            procCur.PlaceService = Clinics.GetPlaceService(procCur.ClinicNum);
            procCur.ProvNum = GetProvNumFromAppointment(apt, procCode);
            //if procedure was already complete, then don't add more notes.
            if (procOld.ProcStatus != ProcStat.C)
            {
                var procNoteDefault = ProcCodeNotes.GetNote(procCur.ProvNum, procCur.CodeNum, procCur.ProcStatus);
                if (procCur.Note != "" && procNoteDefault != "") procCur.Note += "\r\n"; //add a new line if there was already a ProcNote on the procedure.
                procCur.Note += procNoteDefault;
            }

            if (Userods.IsUserCpoe())
                //Only change the status of IsCpoe to true.  Never set it back to false for any reason.  Once true, always true.
                procCur.IsCpoe = true;
            var orthoProcLink = orthoCaseProcedureLinker.LinkProcedureToActiveOrthoCaseIfNeeded(procCur);
            SetOrthoProcComplete(procCur, procCode);
            if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canada
                SetCanadianLabFeesCompleteForProc(procCur);
            var isProcLinkedToOrthoCase = orthoProcLink != null;
            if (procCur.ProcNumLab == 0) //skip because SetCanadianLabFeesCompleteForProc() called update already
                Update(procCur, procOld, isProcLinkedToOrthoCase: isProcLinkedToOrthoCase); //Updates payplan charges for the procedure if it went from any status to complete.
            if (isProcLinkedToOrthoCase) //If proc was linked to orthoCase, Pass ortho case objects to ComputeEstimates.
                ComputeEstimates(procCur, apt.PatNum, ref claimProcList, false, planList, patPlans, benefitList,
                    histList, loopList, true,
                    patient.Age, subList,
                    null, false, false, listSubstLinks, false,
                    listFees, null,
                    orthoProcLink, orthoCaseProcedureLinker.ActiveOrthoCase, orthoCaseProcedureLinker.OrthoSchedule, orthoCaseProcedureLinker.ListOrthoProcLinks, blueBookEstimateData);
            else //Otherwise, call without orthocase objects.
                ComputeEstimates(procCur, apt.PatNum, ref claimProcList, false, planList, patPlans, benefitList,
                    histList, loopList, true,
                    patient.Age, subList,
                    null, false, false, listSubstLinks, false,
                    listFees, blueBookEstimateData: blueBookEstimateData);
            var listClaimProcHistToAdd = ClaimProcs.GetHistForProc(claimProcList, procCur, procCur.CodeNum);
            loopList.AddRange(listClaimProcHistToAdd);
            //Remove any from the histList that were just added to the loopList. This is needed so we don't count the estimated amounts twice (histList and loopList)
            histList.RemoveAll(x => x.ProcNum == procCur.ProcNum
                                    && listClaimProcHistToAdd.Any(y => y.PatNum == x.PatNum && y.PlanNum == x.PlanNum && y.InsSubNum == x.InsSubNum));
            ClaimProcs.SetProvForProc(procCur, claimProcList);
            //Add provnum to list to create an encounter later. Done to limit calls to DB from Encounters.InsertDefaultEncounter().
            if (procOld.ProcStatus != ProcStat.C) //check for distinct later.
                encounterProvNums.Add(procCur.ProvNum);
        }

        //Auto-insert default encounters for the providers that did work on this appointment
        encounterProvNums.Distinct().ToList().ForEach(x => Encounters.InsertDefaultEncounter(apt.PatNum, x, apt.AptDateTime));
        Recalls.Synch(apt.PatNum);
        return listProcsForAppt;
        //Patient pt=Patients.GetPat(apt.PatNum);
        //jsparks-See notes within this method:
        //Reporting.Allocators.AllocatorCollection.CallAll_Allocators(pt.Guarantor);
    }

    private static bool ExistsDiagnosticCode(List<string> listDiagCodes, List<byte> listDiagVersions, string diagnosticCode, byte diagnosticVersion)
    {
        for (var i = 0; i < listDiagCodes.Count; i++)
            if (listDiagCodes[i] == diagnosticCode && listDiagVersions[i] == diagnosticVersion)
                return true;

        return false;
    }

    public static DateTime GetFirstOrthoProcDate(PatientNote patNoteCur)
    {
        if (patNoteCur.DateOrthoPlacementOverride != DateTime.MinValue)
            //if an override is set, use that.
            return patNoteCur.DateOrthoPlacementOverride;
        var firstOrthoProcDate = DateTime.MinValue;
        var listOrthoProcNums = ProcedureCodes.GetOrthoBandingCodeNums();
        //otherwise, use the proc of one of the codes specified in the pref.
        var proc = GetProcsByStatusForPat(patNoteCur.PatNum, ProcStat.C)
            .Where(x => listOrthoProcNums.Contains(x.CodeNum))
            .OrderBy(x => x.ProcDate) //Earliest ortho placement first.
            .FirstOrDefault();
        if (proc != null) firstOrthoProcDate = proc.ProcDate;
        return firstOrthoProcDate;
    }

    public static void SetOrthoProcComplete(Procedure procCur, ProcedureCode procCode)
    {
        if (procCode == null || procCur == null) //this should never happen unless they have some corruption
            return;
        if (PrefC.GetString(PrefName.OrthoDebondCodes).Contains(procCode.ProcCode) && PrefC.GetBool(PrefName.OrthoDebondProcCompletedSetsMonthsTreat))
        {
            SetAutoOrthoMonthsTreat(procCur); //Check if completed code is debond and needs to update Months Treatment.
            return;
        }

        var listOrthoPlacementCodeNums = ProcedureCodes.GetOrthoBandingCodeNums();
        if (listOrthoPlacementCodeNums.Count > 0)
        {
            if (!listOrthoPlacementCodeNums.Contains(procCode.CodeNum)) return;
        }
        else if (!procCode.ProcCode.StartsWith("D8080") && !procCode.ProcCode.StartsWith("D8090"))
        {
            return;
        }

        var listPatPlans = PatPlans.GetPatPlansForPat(procCur.PatNum);
        foreach (var patPlanCur in listPatPlans)
        {
            if (patPlanCur.OrthoAutoNextClaimDate.Date != DateTime.MinValue.Date) continue;
            var insPlanCur = InsPlans.GetByInsSubs([patPlanCur.InsSubNum]).FirstOrDefault();
            if (insPlanCur == null || insPlanCur.OrthoType != OrthoClaimType.InitialPlusPeriodic) continue;
            var waitDays = TimeSpan.FromDays(0);
            waitDays = TimeSpan.FromDays(insPlanCur.OrthoAutoClaimDaysWait);
            var procWaitDays = procCur.ProcDate + waitDays;
            procWaitDays = procWaitDays.AddMonths(1); //Always push the next claim date out a month, even after the "wait days" preference.
            patPlanCur.OrthoAutoNextClaimDate = new DateTime(procWaitDays.Year, procWaitDays.Month, 1);
            PatPlans.Update(patPlanCur);
        }

        if (procCur.ProcStatus == ProcStat.C)
        {
            //Only make this change if Complete procedure.(Currently, will always be true, but for safety).
            var patCur = Patients.GetLim(procCur.PatNum); //GetLim because we just need pat.Guarantor.
            var patNoteCur = PatientNotes.Refresh(patCur.PatNum, patCur.Guarantor); //Inserts PatientNote rows if one does not exists for PatNum AND Guarantor.
            //First time completing an Ortho placement procedure, so we don't have an override in place yet. Any subsequent Ortho procs will use the same
            //override as the first Ortho proc.
            var defaultMonths = PrefC.GetByte(PrefName.OrthoDefaultMonthsTreat);
            //Only set the override if one has not already been set.
            if (patNoteCur.OrthoMonthsTreatOverride == -1)
            {
                //Set OrthoMonthsTreatOverride to PrefName.OrthoDefaultMonthsTreat, so we don't overwrite it if the practice default changes later.
                patNoteCur.OrthoMonthsTreatOverride = defaultMonths; //Use current practice default.
                PatientNotes.Update(patNoteCur, patCur.Guarantor);
            }
        }
    }

    private static void SetAutoOrthoMonthsTreat(Procedure procCur)
    {
        var patCur = Patients.GetLim(procCur.PatNum);
        var patNoteCur = PatientNotes.Refresh(patCur.PatNum, patCur.Guarantor);
        var firstOrthoProcDate = GetFirstOrthoProcDate(patNoteCur);
        if (firstOrthoProcDate == DateTime.MinValue) firstOrthoProcDate = procCur.ProcDate;
        var dateSpan = new DateSpan(firstOrthoProcDate, procCur.ProcDate);
        var totalMonthsDiff = dateSpan.YearsDiff * 12 + dateSpan.MonthsDiff + (dateSpan.DaysDiff < 15 ? 0 : 1);
        patNoteCur.OrthoMonthsTreatOverride = totalMonthsDiff; //Setting the patient notes OrthoMonthsTreatOverride will set the Tx Total Months.
        PatientNotes.Update(patNoteCur, patCur.Guarantor);
    }

    public static string GetClaimDescript(ClaimProc claimProcCur, ProcedureCode procCodeSent, ProcedureCode procCodeCur, InsPlan planCur = null)
    {
        var descript = procCodeSent.Descript;
        if (PrefC.GetBool(PrefName.ClaimPrintProcChartedDesc))
        {
            if (planCur == null) planCur = InsPlans.GetPlan(claimProcCur.PlanNum, null);
            //If the proccode was not overridden by a alternate code or a medical code,
            //then use the orignal procedure code description instead of the codesent description.
            if ((procCodeCur.AlternateCode1 == "" || !planCur.UseAltCode)
                && (procCodeCur.MedicalCode == "" || !planCur.IsMedical))
                descript = procCodeCur.Descript;
        }

        return descript;
    }

    public static void ProcsAptNumHelper(List<Procedure> listProcs, Appointment AptCur, List<Appointment> listAppointments, List<int> listSelectedRows, List<long> listProcNumsAttachedStart, bool isAptPlanned = false, LogSources logSource = LogSources.None)
    {
        if (listProcs == null || AptCur == null || listAppointments == null || listSelectedRows == null || listProcNumsAttachedStart == null) return;
        for (var i = 0; i < listProcs.Count; i++)
        {
            var proc = listProcs[i];
            var procOld = proc.Copy();
            var isAttaching = listSelectedRows.Contains(i);
            var isDetaching = !isAttaching;
            var isAttachedStart = listProcNumsAttachedStart.Contains(proc.ProcNum);
            var isDetachedStart = !isAttachedStart;
            if (isDetaching && isAptPlanned && proc.PlannedAptNum == AptCur.AptNum)
            {
                //Detatching from this planned appointment.
                proc.PlannedAptNum = 0;
            }
            else if (isDetaching && !isAptPlanned && proc.AptNum == AptCur.AptNum)
            {
                //Detatching from this appointment.
                proc.AptNum = 0;
            }
            else if (isDetachedStart && isAttaching && isAptPlanned)
            {
                //Attaching to this planned appointment.
                if (proc.PlannedAptNum != 0 && proc.PlannedAptNum != AptCur.AptNum)
                {
                    //Currently attached to another planned appointment.
                    var apptOldPlanned = listAppointments.FirstOrDefault(x => x.AptNum == proc.PlannedAptNum && x.AptStatus == ApptStatus.Planned);
                    var apptOldPlannedDateStr = apptOldPlanned == null ? "[INVALID #" + proc.PlannedAptNum + "]" : apptOldPlanned.AptDateTime.ToShortDateString();
                    //Add securityLog to planned appointment.
                    SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit, AptCur.PatNum, Lans.g("AppointmentEdit", "Procedure") + " "
                                                                                                                                  + ProcedureCodes.GetProcCode(proc.CodeNum).AbbrDesc + " " + Lans.g("AppointmentEdit", "moved from planned appointment created on") + " "
                                                                                                                                  + apptOldPlannedDateStr + " " + Lans.g("AppointmentEdit", "to planned appointment created on") + " "
                                                                                                                                  + AptCur.AptDateTime.ToShortDateString(), AptCur.AptNum, logSource, AptCur.DateTStamp);
                    //Add securityLog to previously planned appointment.
                    SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit, apptOldPlanned?.PatNum ?? AptCur.PatNum, Lans.g("AppointmentEdit", "Procedure") + " "
                                                                                                                                                            + ProcedureCodes.GetProcCode(proc.CodeNum).AbbrDesc + " " + Lans.g("AppointmentEdit", "moved from planned appointment created on") + " "
                                                                                                                                                            + apptOldPlannedDateStr + " " + Lans.g("AppointmentEdit", "to planned appointment created on") + " "
                                                                                                                                                            + apptOldPlannedDateStr, apptOldPlanned?.AptNum ?? 0, logSource, apptOldPlanned?.DateTStamp ?? default);
                    UpdateOtherApptDesc(proc, AptCur, isAptPlanned, listAppointments, listProcs);
                }

                proc.PlannedAptNum = AptCur.AptNum;
            }
            else if (isDetachedStart && isAttaching && !isAptPlanned)
            {
                //Attaching to this appointment.
                if (proc.AptNum != 0 && proc.AptNum != AptCur.AptNum)
                {
                    //Currently attached to another appointment.
                    var apptOld = listAppointments.FirstOrDefault(x => x.AptNum == proc.AptNum);
                    var apptOldDateStr = apptOld == null ? "[INVALID #" + proc.AptNum + "]" : apptOld.AptDateTime.ToShortDateString();
                    //Add securityLog to appointment.
                    SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit, AptCur.PatNum, Lans.g("AppointmentEdit", "Procedure") + " "
                                                                                                                                  + ProcedureCodes.GetProcCode(proc.CodeNum).AbbrDesc + " " + Lans.g("AppointmentEdit", "moved from appointment on") + " " + apptOldDateStr
                                                                                                                                  + " " + Lans.g("AppointmentEdit", "to appointment on") + " " + AptCur.AptDateTime, AptCur.AptNum, logSource, AptCur.DateTStamp);
                    //Add securityLog to previous appointment.
                    SecurityLogs.MakeLogEntry(EnumPermType.AppointmentEdit, apptOld?.PatNum ?? AptCur.PatNum, Lans.g("AppointmentEdit", "Procedure") + " "
                                                                                                                                                     + ProcedureCodes.GetProcCode(proc.CodeNum).AbbrDesc + " " + Lans.g("AppointmentEdit", "moved from appointment on") + " " + apptOldDateStr
                                                                                                                                                     + " " + Lans.g("AppointmentEdit", "to appointment on") + " " + apptOldDateStr, apptOld?.AptNum ?? 0, logSource, apptOld?.DateTStamp ?? default);
                    UpdateOtherApptDesc(proc, AptCur, isAptPlanned, listAppointments, listProcs);
                }

                proc.AptNum = AptCur.AptNum;
            }
            else
            {
                continue; //No changes were made to the current procedure.
            }

            Update(proc, procOld); //Update above changes to db.
        }
    }

    public static void UpdateOtherApptDesc(Procedure proc, Appointment appt, bool isApptPlanned, List<Appointment> listAppts, List<Procedure> listProcsForAppt)
    {
        Appointment apptPrevious;
        if (isApptPlanned)
        {
            apptPrevious = listAppts.FirstOrDefault(x => x.AptNum == proc.PlannedAptNum);
            proc.PlannedAptNum = appt.AptNum;
        }
        else
        {
            apptPrevious = listAppts.FirstOrDefault(x => x.AptNum == proc.AptNum);
            proc.AptNum = appt.AptNum;
        }

        if (apptPrevious != null)
            //apptPrevious gets updated in memory which causes listAppts to contain the changes.
            Appointments.SetProcDescript(apptPrevious, listProcsForAppt);
    }

    public static void LogProcComplCreate(long patNum, Procedure procCur, string toothNums)
    {
        if (procCur == null) return; //Nothing to do.  Should never happen.
        var procCode = ProcedureCodes.GetProcCode(procCur.CodeNum);
        var logText = procCode.ProcCode + ", ";
        if (toothNums != null && toothNums.Trim() != "") logText += Lans.g("Procedures", "Teeth") + ": " + toothNums + ", ";
        logText += Lans.g("Procedures", "Fee") + ": " + procCur.ProcFee.ToString("F") + ", " + procCode.Descript;
        SecurityLogs.MakeLogEntry(EnumPermType.ProcComplCreate, patNum, logText);
    }

    private static void LogProcComplEdit(Procedure proc, Procedure procOld, List<ProcedureCode> listProcedureCodes = null)
    {
        var procCode = ProcedureCodes.GetProcCode(proc.CodeNum, listProcedureCodes);
        var logText = Lans.g("Procedures", "Completed procedure") + " " + procCode.ProcCode + " "
                      + Lans.g("Procedures", "edited by setting appointment complete.");
        if (proc.ProvNum != procOld.ProvNum)
            logText += " " + Lans.g("Procedures", "Provider was changed from") + " " + Providers.GetAbbr(procOld.ProvNum) + " " + Lans.g("Procedures", "to") + " " +
                       Providers.GetAbbr(proc.ProvNum) + ".";
        SecurityLogs.MakeLogEntry(EnumPermType.ProcCompleteEdit, proc.PatNum, logText, proc.ProcNum, LogSources.None, procOld.DateTStamp);
    }

    public static List<Procedure> SetCompleteInAppt(Appointment apt, List<InsPlan> PlanList, List<PatPlan> patPlans, Patient patient, List<InsSub> subList, bool removeCompletedProcs)
    {
        //Get all procs attached to the appointment and go through the set complete logic.
        //We must go through all procedures. Remove completed procs if removeCompletedProcs is set to true. We don't want to change completed procedures 
        //unless user wants/has permissions.The permission check should be done before calling this method.
        var listProcsInAppt = GetProcsForSingle(apt.AptNum, false);
        if (removeCompletedProcs)
            //Remove already completed procedures.
            listProcsInAppt.RemoveAll(x => x.ProcStatus == ProcStat.C);
        if (listProcsInAppt.Count == 0) return listProcsInAppt; //Nothing to do.
        var listProcsOld = listProcsInAppt.Select(x => x.Copy()).ToList();
        listProcsInAppt = SetCompleteInApptInList(apt, PlanList, patPlans, patient, listProcsInAppt, subList, Security.CurUser);
        var listProcsCompleted = listProcsInAppt.FindAll(x => listProcsOld.Any(y => y.ProcNum == x.ProcNum && y.ProcStatus != ProcStat.C));
        listProcsCompleted.ForEach(x => LogProcComplCreate(apt.PatNum, x, x.ToothNum));
        var listProcsAlreadyComplete = listProcsInAppt.FindAll(x => listProcsOld.Any(y => y.ProcNum == x.ProcNum && y.ProcStatus == ProcStat.C));
        foreach (var proc in listProcsAlreadyComplete)
        {
            var procOld = listProcsOld.FirstOrDefault(x => x.ProcNum == proc.ProcNum);
            LogProcComplEdit(proc, procOld);
        }

        return listProcsInAppt;
    }

    public static Procedure ConstructProcedureForAppt(long codeNum, Appointment appt, Patient pat, List<PatPlan> listPatPlans, List<InsPlan> listInsPlans, List<InsSub> listInsSubs, List<Fee> listFees = null)
    {
        var proc = new Procedure();
        proc.CodeNum = codeNum;
        proc.PatNum = appt.PatNum;
        proc.ProcDate = DateTime.Today;
        proc.DateTP = proc.ProcDate;
        proc.ToothRange = "";
        //surf
        proc.Priority = 0;
        proc.ProcStatus = ProcStat.TP;
        var procCodeCur = ProcedureCodes.GetProcCode(proc.CodeNum);

        #region ProvNum

        proc.ProvNum = appt.ProvNum;
        if (procCodeCur.ProvNumDefault != 0) //Override provider for procedures with a default provider
            //This provider might be restricted to a different clinic than this user.
            proc.ProvNum = procCodeCur.ProvNumDefault;
        else if (procCodeCur.IsHygiene && appt.ProvHyg != 0) proc.ProvNum = appt.ProvHyg;

        #endregion ProvNum

        proc.ClinicNum = appt.ClinicNum;
        proc.MedicalCode = procCodeCur.MedicalCode;
        proc.ProcFee = GetProcFee(pat, listPatPlans, listInsSubs, listInsPlans, proc, listFees: listFees);
        proc.Note = ProcCodeNotes.GetNote(proc.ProvNum, proc.CodeNum, proc.ProcStatus); //get the TP note.
        //dx
        //nextaptnum
        proc.DateEntryC = DateTime.Now;
        proc.BaseUnits = procCodeCur.BaseUnits;
        proc.SiteNum = pat.SiteNum;
        proc.RevCode = procCodeCur.RevenueCodeDefault;
        SetDiagnosticCodesToDefault(proc, procCodeCur);
        proc.PlaceService = Clinics.GetPlaceService(proc.ClinicNum);
        if (Userods.IsUserCpoe())
            //This procedure is considered CPOE because the provider is the one that has added it.
            proc.IsCpoe = true;
        return proc;
    }
    
    public static bool IsAnOrthoCaseProcCode(string procCode)
    {
        var orthoProcCodes = OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoBandingCodes);
        orthoProcCodes.AddRange(OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoVisitCodes));
        orthoProcCodes.AddRange(OrthoCases.GetListProcTypeProcCodes(PrefName.OrthoDebondCodes));
        return orthoProcCodes.Contains(procCode);
    }

    public static bool EntriesAreValid(string textNotes, bool isSigChangedAndNotBlank, string textTimeStart, string textTimeEnd, int unityQty, long provNumSelected,
        string textMedicalCode, bool isTextDrugNdcNotBlank, string textDrugQty, ref DateTime textDate, bool isNew, double procFee, bool isQuickAdd,
        bool isCheckTypeCodeNonXChecked, bool isCheckTypeCodeXChecked, int listProsthSelectedIndex, bool isQuadrantSelected,
        bool hasOrthoProcLink, bool isTextDateOriginalProsthBlank, int comboDrugUnitSelectedIndex,
        string textSurfaces, string textTooth, bool hasSextantSelection, bool isArchSelected,
        Procedure procedure, Procedure procedureOld, ref ProcedureCode procedureCode, string translationSource, Userod user,
        Action<string> actionOnFailure, Func<string, bool> funcYesNoPrompt, Action actionOnProcedureCodeFailure,
        ref List<ClaimProc> listClaimProcsForProc)
    {
        #region Note

        var hasAutoNotePrompt = Regex.IsMatch(textNotes, AutoNotePromptRegex);
        //If ProcNoteSigsBlockedAutoNoteIncomplete is true, do not allow the user to save a changed signature if there are still autonote prompts.
        if (isSigChangedAndNotBlank && hasAutoNotePrompt && PrefC.GetBool(PrefName.ProcNoteSigsBlockedAutoNoteIncomplete))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Remaining auto note prompts must be completed to sign this note. Use Edit Auto Note to resume."));
            return false;
        }

        //There have been 2 or 3 cases where a customer entered a note with thousands of new lines and when OD tries to display such a note in the chart, a GDI exception occurs because the progress notes grid is very tall and takes up too much video memory. To help prevent this issue, we block the user from entering any note where there are 50 or more consecutive new lines anywhere in the note. Any number of new lines less than 50 are considered to be intentional.
        var tooManyNewLines = new StringBuilder();
        for (var i = 0; i < 50; i++) tooManyNewLines.Append("\r\n");
        if (textNotes.Contains(tooManyNewLines.ToString()))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "The notes contain 50 or more consecutive blank lines. Probably unintentional and must be fixed."));
            return false;
        }

        #endregion

        #region textTimeStart, textTimeEnd validation

        if (!AreTimesValid(textTimeStart, textTimeEnd, actionOnFailure)) return false;

        #endregion

        #region textUnitQty validation

        if (!IsQuantityValid(unityQty, actionOnFailure)) return false;

        #endregion

        #region Provider UI

        if (provNumSelected == 0)
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "You must select a provider first."));
            return false;
        }

        #endregion

        if (procedureCode.TreatArea == TreatmentArea.Surf && (textSurfaces == "" || !ValidateToothValue(textTooth, ref textSurfaces)))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Please fix tooth number or surfaces first."));
            return false;
        }

        if (procedureCode.TreatArea == TreatmentArea.Sextant && !ValidateSextant(hasSextantSelection))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Please fix sextant first."));
            return false;
        }

        if (procedureCode.TreatArea == TreatmentArea.Arch && !ValidateArch(isArchSelected))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Please fix arch first."));
            return false;
        }

        if (procedureCode.TreatArea == TreatmentArea.Tooth && !Tooth.IsValidEntry(textTooth))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Please fix tooth number first."));
            return false;
        }

        #region Medical Code

        if (textMedicalCode != "" && !ProcedureCodes.GetContainsKey(textMedicalCode))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Invalid medical code.  It must refer to an existing procedure code."));
            return false;
        }

        #endregion

        #region Drug UI

        if (isTextDrugNdcNotBlank)
            if (comboDrugUnitSelectedIndex == (int) EnumProcDrugUnit.None || textDrugQty == "")
                if (!funcYesNoPrompt.Invoke(Lans.g(translationSource, "Drug quantity and unit are not entered.  Continue anyway?")))
                    return false;

        if (textDrugQty != "")
            try
            {
                float.Parse(textDrugQty);
            }
            catch
            {
                actionOnFailure.Invoke(Lans.g(translationSource, "Please fix drug qty first."));
                return false;
            }

        #endregion

        #region Procedure Status

        //If user is trying to change status to complete and using eCW.
        if (procedure.ProcStatus == ProcStat.C && (isNew || procedureOld.ProcStatus != ProcStat.C) && false)
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Procedures cannot be set complete in this window.  Set the procedure complete by setting the appointment complete."));
            return false;
        }

        if (procedure.ProcStatus == ProcStat.C && textDate.Date > DateTime.Today.Date && !PrefC.GetBool(PrefName.FutureTransDatesAllowed))
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "Completed procedures cannot have future dates."));
            return false;
        }

        if (procedureOld.ProcStatus != ProcStat.C && procedure.ProcStatus == ProcStat.C)
        {
            //if status was changed to complete
            if (procedure.AptNum != 0)
            {
                //if attached to an appointment
                var apt = Appointments.GetOneApt(procedure.AptNum);
                if (apt.AptDateTime.Date > MiscData.GetNowDateTime().Date)
                {
                    //if appointment is in the future
                    actionOnFailure.Invoke(Lans.g(translationSource, "Not allowed because procedure is attached to a future appointment with a date of ")
                                           + apt.AptDateTime.ToShortDateString());
                    return false;
                }

                if (apt.AptDateTime.Year >= 1880) textDate = apt.AptDateTime;
            }

            if (!isQuickAdd && !Security.IsAuthorized(EnumPermType.ProcComplCreate, textDate, true, true, user, 0, -1, 0, 0, actionOnFailure)) //use the new date
                return false;
        }
        else if (!isQuickAdd && isNew && procedure.ProcStatus == ProcStat.C)
        {
            //if new procedure is complete
            if (!Security.IsAuthorized(EnumPermType.ProcComplCreate, textDate, true, true, user, procedure.CodeNum, procFee, 0, 0, actionOnFailure)) return false;
        }
        else if (!isNew)
        {
            //an old procedure
            if (procedureOld.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC)) //that was already complete
                if (!CheckPermissionsAndGlobalLockDate(procedureOld, procedure, textDate, user, procFee, actionOnFailure))
                    return false;
        }

        #endregion

        #region Canada and Prosthesis

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            //Canadian. en-CA or fr-CA
            if (isCheckTypeCodeXChecked)
                if (isCheckTypeCodeNonXChecked)
                {
                    actionOnFailure.Invoke(Lans.g(translationSource, "If type code 'none' is checked, no other type codes may be checked."));
                    return false;
                }

            if (procedureCode.IsProsth && !isCheckTypeCodeNonXChecked && !isCheckTypeCodeXChecked)
                if (!funcYesNoPrompt.Invoke(Lans.g(translationSource, "At least one type code should be checked for prosthesis.  Continue anyway?")))
                    return false;
        }
        else
        {
            if (procedureCode.IsProsth)
                if (listProsthSelectedIndex == 0 || (listProsthSelectedIndex == 2 && isTextDateOriginalProsthBlank))
                    if (!funcYesNoPrompt.Invoke(Lans.g(translationSource, "Prosthesis date not entered. Continue anyway?")))
                        return false;
        }

        #endregion

        #region Quadrant

        if (procedureCode.TreatArea == TreatmentArea.Quad)
            if (!isQuadrantSelected)
            {
                actionOnFailure.Invoke(Lans.g(translationSource, "Please select a quadrant."));
                return false;
            }

        #endregion

        #region Provider

        listClaimProcsForProc = ClaimProcs.GetForProc(ClaimProcs.Refresh(procedure.PatNum), procedure.ProcNum); //update for accuracy
        if (!ValidateProvider(listClaimProcsForProc, provNumSelected, procedureOld.ProvNum, actionOnFailure)) return false;

        #endregion

        //Block if proc is linked to ortho case and user tries to set status from complete to any other status.
        if (hasOrthoProcLink && procedureOld.ProcStatus == ProcStat.C && procedure.ProcStatus != ProcStat.C)
        {
            actionOnFailure.Invoke(Lans.g(translationSource, "The status of a completed procedure that is attached to an ortho case cannot be changed. " +
                                                             "Detach the procedure from the ortho case or delete the ortho case first."));
            return false;
        }

        //Once upon a time we used to use the textProc.Text field to validate that the procedure was valid. Sometimes, that text field would not have a value, or have some strange messed up value that did not coorespond to a procedure code. We no longer need to check that, so we are checking the procedure object itself. All this to say, this check will likely never return false, but just in case, here it is. 
        try
        {
            ProcedureCodes.GetStringProcCode(procedure.CodeNum, doThrowIfMissing: true);
        }
        catch (ApplicationException ae)
        {
            actionOnProcedureCodeFailure.Invoke();
            return false;
        }

        //double check that nothing has happed with the _procedureCode object and that it still reflects a valid code. If it doesn't use the CodeNum from _procedure to get a valid procedureCode object.
        try
        {
            ProcedureCodes.GetStringProcCode(procedureCode.CodeNum, doThrowIfMissing: true);
            if (procedure.CodeNum != procedureCode.CodeNum) procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum); //_procedureCode is valid, but for some reason has fallen out of sync with the _procedure object. Force them back in sync before saving.
        }
        catch (Exception e)
        {
            procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum);
            //no need to return false here since we fixed the issue.
        }

        return true;
    }

    public static bool IsQuantityValid(int quantity, Action<string> actionOnFailure = null)
    {
        if (quantity < 1)
        {
            actionOnFailure?.Invoke(Lans.g("Procedures", "Qty not valid.  Typical value is 1."));
            return false;
        }

        return true;
    }

    public static bool AreTimesValid(string timeStart, string timeEnd, Action<string> actionOnFailure = null)
    {
        if (PrefC.GetBool(PrefName.ShowFeatureMedicalInsurance))
        {
            if (!ValidateTime(timeStart))
            {
                actionOnFailure?.Invoke(Lans.g("Procedures", "Start time is invalid."));
                return false;
            }

            if (!ValidateTime(timeEnd))
            {
                actionOnFailure?.Invoke(Lans.g("Procedures", "End time is invalid."));
                return false;
            }
        }
        else
        {
            if (timeStart != "")
                try
                {
                    DateTime.Parse(timeStart);
                }
                catch
                {
                    actionOnFailure?.Invoke(Lans.g("Procedures", "Start time is invalid."));
                    return false;
                }
        }

        return true;
    }

    public static bool ValidateTime(string time)
    {
        var militaryTime = time;
        if (militaryTime == "") return true;
        if (militaryTime.Length < 4) militaryTime = militaryTime.PadLeft(4, '0');
        //Test if user typed in military time. Ex: 0830 or 1536
        try
        {
            var hour = SIn.Int(militaryTime.Substring(0, 2));
            var minute = SIn.Int(militaryTime.Substring(2, 2));
            if (hour > 23) return false;
            if (minute > 59) return false;
            return true;
        }
        catch
        {
        }

        //Test typical DateTime format. Ex: 1:00 PM
        try
        {
            DateTime.Parse(time);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool ValidateProvider(List<ClaimProc> listClaimProcsForProc, long selectedProvNum, long provNumForProc, Action<string> actionOnFailure = null)
    {
        //validate for provider change
        if (provNumForProc != selectedProvNum && PrefC.GetBool(PrefName.ProcProvChangesClaimProcWithClaim))
            //if selected prov is null (no selection made), no change will happen to the provider
            if (listClaimProcsForProc.Any(x => x.Status.In(ClaimProcStatus.Received, ClaimProcStatus.Supplemental, ClaimProcStatus.CapClaim)))
            {
                actionOnFailure?.Invoke(Lans.g("Procedures", "The provider cannot be changed when this procedure is attached to a claim."));
                return false;
            }

        return true;
    }

    public static bool CheckPermissionsAndGlobalLockDate(Procedure procOld, Procedure procNew, DateTime procDate, Userod user, double procFeeOverride = double.MinValue, Action<string> actionNotAuthorized = null)
    {
        if (!procOld.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC)) //that was already complete
            return true;
        var perm = GroupPermissions.SwitchExistingPermissionIfNeeded(EnumPermType.ProcCompleteStatusEdit, procOld);
        var dateToUseProcOld = GetDateForPermCheck(procOld);
        if (procOld.ProcStatus != procNew.ProcStatus
            && !Security.IsAuthorized(perm, dateToUseProcOld, true, true, user, 0, -1, 0, 0, actionNotAuthorized))
            //block old date
            return false;
        if (procNew.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC))
        {
            var dateToUseProcCur = GetDateForPermCheck(procNew, procDate);
            if (procOld.ProcStatus != procNew.ProcStatus)
            {
                if (procOld.ProcStatus == ProcStat.C)
                    perm = EnumPermType.ProcCompleteStatusEdit;
                else if (procNew.ProcStatus == ProcStat.C)
                    perm = EnumPermType.ProcComplCreate;
                else
                    perm = EnumPermType.ProcExistingEdit;
                if (!Security.IsAuthorized(perm, dateToUseProcCur, true, true, user, 0, -1, 0, 0, actionNotAuthorized)) //block new date, too
                    return false;
            }

            var procFee = procNew.ProcFee;
            if (procFeeOverride != double.MinValue) procFee = procFeeOverride;
            if (procOld.ProcDate != procDate //If user changed the procedure date
                || !CompareDouble.IsEqual(procOld.ProcFee, procFee) //If user changed the procedure fee
                || procOld.CodeNum != procNew.CodeNum) //If user changed the procedure code
            {
                perm = GroupPermissions.SwitchExistingPermissionIfNeeded(EnumPermType.ProcCompleteEdit, procNew);
                if (!Security.IsAuthorized(perm, dateToUseProcCur, true, true, user, procNew.CodeNum, procFee, 0, 0, actionNotAuthorized)) return false;
            }
        }

        return true;
    }

    public static bool ValidateToothValue(string toothNumLabel, ref string toothSurface, Action<string> actionOnFailure = null)
    {
        if (toothNumLabel == "")
        {
            toothSurface = Tooth.SurfTidyForDisplay(toothSurface, "");
        }
        else
        {
            if (!Tooth.IsValidEntry(toothNumLabel)) return false;
            toothSurface = Tooth.SurfTidyForDisplay(toothSurface, Tooth.Parse(toothNumLabel));
        }

        if (toothSurface == "")
        {
            actionOnFailure?.Invoke(Lans.g(nameof(Procedures), "No surfaces selected."));
            return false;
        }

        return true;
    }

    public static bool ValidateSextant(bool hasSextantSelection, Action<string> actionOnFailure = null)
    {
        if (hasSextantSelection) return true;
        actionOnFailure?.Invoke(Lans.g(nameof(Procedures), "Please select a sextant treatment area."));
        return false;
    }

    public static bool ValidateArch(bool isArchSelected, Action<string> actionOnFailure = null)
    {
        if (isArchSelected) return true;
        actionOnFailure?.Invoke(Lans.g(nameof(Procedures), "Please select a arch treatment area."));
        return false;
    }

    public static void UpdateProcedureFields(Procedure procedure, Patient patient, string textMedicalCodeText, double discount, Snomed snomedBodySite, bool checkIcdVersionChecked, List<string> diagnosticCodes, bool checkIsPrincDiagChecked, long selectedProvOrderNum, Referral referralOrdering, string textCodeMod1, string textCodeMod2, string textCodeMod3, string textCodeMod4, int unitQty, ProcUnitQtyType unitQtyType, string revCode, EnumProcDrugUnit drugUnit, float drugQty, ProcUrgency procUrgency, long selectedProvNum, long clinicNum)
    {
        procedure.PatNum = patient.PatNum;
        procedure.MedicalCode = textMedicalCodeText;
        procedure.Discount = discount;
        if (snomedBodySite == null)
            procedure.SnomedBodySite = "";
        else
            procedure.SnomedBodySite = snomedBodySite.SnomedCode;
        procedure.IcdVersion = 9;
        if (checkIcdVersionChecked) procedure.IcdVersion = 10;
        procedure.DiagnosticCode = "";
        procedure.DiagnosticCode2 = "";
        procedure.DiagnosticCode3 = "";
        procedure.DiagnosticCode4 = "";
        if (diagnosticCodes.Count > 0) procedure.DiagnosticCode = diagnosticCodes[0];
        if (diagnosticCodes.Count > 1) procedure.DiagnosticCode2 = diagnosticCodes[1];
        if (diagnosticCodes.Count > 2) procedure.DiagnosticCode3 = diagnosticCodes[2];
        if (diagnosticCodes.Count > 3) procedure.DiagnosticCode4 = diagnosticCodes[3];
        procedure.IsPrincDiag = checkIsPrincDiagChecked;
        procedure.ProvOrderOverride = selectedProvOrderNum;
        if (referralOrdering == null)
            procedure.OrderingReferralNum = 0;
        else
            procedure.OrderingReferralNum = referralOrdering.ReferralNum;
        procedure.CodeMod1 = textCodeMod1;
        procedure.CodeMod2 = textCodeMod2;
        procedure.CodeMod3 = textCodeMod3;
        procedure.CodeMod4 = textCodeMod4;
        procedure.UnitQty = unitQty;
        procedure.UnitQtyType = unitQtyType;
        procedure.RevCode = revCode;
        procedure.DrugUnit = drugUnit;
        procedure.DrugQty = drugQty;
        procedure.Urgency = procUrgency;
        procedure.ProvNum = selectedProvNum;
        procedure.ClinicNum = clinicNum;
    }
    
    public static bool VerifyCompletedProcStatusChange(List<PaySplit> listPaySplitsForProc, Procedure procedure, Procedure procedureOld, string translationSource, Func<string, bool> funcYesNoPrompt, Action<string> actionOnFailure = null)
    {
        double sumPaySplits = 0;
        for (var i = 0; i < listPaySplitsForProc.Count; i++) sumPaySplits += listPaySplitsForProc[i].SplitAmt;
        if (procedureOld.ProcStatus == ProcStat.C && procedure.ProcStatus != ProcStat.C)
        {
            //Proc was complete but was changed.
            if (Adjustments.GetForProc(procedure.ProcNum, Adjustments.Refresh(procedure.PatNum)).Count != 0
                && !funcYesNoPrompt.Invoke(Lans.g(translationSource, "This procedure has adjustments attached to it. Changing the status from completed will delete any adjustments for the procedure. Continue?")))
                return false;
            if (sumPaySplits != 0)
            {
                actionOnFailure?.Invoke(Lans.g(translationSource, "Not allowed to modify the status of a procedure that has payments attached to it. Detach payments from the procedure first."));
                return false;
            }
        }
        else if (procedureOld.ProcStatus != ProcStat.C && procedure.ProcStatus == ProcStat.C)
        {
            //Proc set complete.
            if (ProcedureCodes.AreAnyProcCodesHidden(procedure.CodeNum))
            {
                actionOnFailure?.Invoke($"{Lans.g(translationSource, "Procedure cannot be set complete because it is in a hidden category")}: {ProcedureCodes.GetProcCode(procedure.CodeNum).ProcCode}");
                return false;
            }

            procedure.DateEntryC = DateTime.Now; //this triggers it to set to server time NOW().
            if (procedure.DiagnosticCode == "")
            {
                var procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum);
                SetDiagnosticCodesToDefault(procedure, procedureCode);
                procedure.IcdVersion = PrefC.GetByte(PrefName.DxIcdVersion);
            }
        }

        return true;
    }
    
    public static bool VerifyProviderChange(Procedure procedure, Procedure procedureOld, List<Adjustment> listAdjusts, out bool hasSplitProvChanged, out bool hasAdjProvChanged, string translationSource, Func<string, bool> funcYesNoPrompt, Action<string> actionNotAuthorized = null)
    {
        hasSplitProvChanged = false;
        hasAdjProvChanged = false;
        if (procedure.ProvNum != procedureOld.ProvNum)
        {
            if (PaySplits.IsPaySplitAttached(procedure.ProcNum))
            {
                var listPaySplit = PaySplits.GetPaySplitsFromProc(procedure.ProcNum);
                foreach (var paySplit in listPaySplit)
                {
                    if (!Security.IsAuthorized(EnumPermType.PaymentEdit, Payments.GetPayment(paySplit.PayNum).PayDate, actionNotAuthorized: actionNotAuthorized)) return false;
                    if (procedure.ProvNum != paySplit.ProvNum) hasSplitProvChanged = true;
                }

                if (hasSplitProvChanged
                    && !funcYesNoPrompt.Invoke(Lans.g(translationSource, "The provider for the associated payment splits will be changed to match the provider on the procedure. Continue?")))
                    return false;
            }

            foreach (var adjust in listAdjusts)
            {
                if (!Security.IsAuthorized(EnumPermType.AdjustmentEdit, adjust.AdjDate, actionNotAuthorized: actionNotAuthorized)) return false;
                if (procedure.ProvNum != adjust.ProvNum && PrefC.GetInt(PrefName.RigorousAdjustments) == (int) RigorousAdjustments.EnforceFully) hasAdjProvChanged = true;
            }

            if (hasAdjProvChanged
                && !funcYesNoPrompt.Invoke(Lans.g(translationSource, "The provider for the associated adjustments will be changed to match the provider on the procedure. Continue?")))
                return false;
        }

        return true;
    }

    public static void SetMiscDateAndTimeEditFields(Procedure procedure, string textDateTP, DateTime procDate, string textTimeStart, string textTimeEnd)
    {
        // textDateTP.Text is blank upon load if date in DB is before 1/1/1880. We don't want to update this if the DateTP box is left blank.
        if (procedure.DateTP.Year > 1880 || textDateTP != "") procedure.DateTP = SIn.Date(textDateTP);
        procedure.ProcDate = procDate;
        var dateT = SIn.DateTime(textTimeStart);
        procedure.ProcTime = new TimeSpan(dateT.Hour, dateT.Minute, 0);
        if (PrefC.GetBool(PrefName.ShowFeatureMedicalInsurance))
        {
            dateT = ParseTime(textTimeStart);
            procedure.ProcTime = new TimeSpan(dateT.Hour, dateT.Minute, 0);
            dateT = ParseTime(textTimeEnd);
            procedure.ProcTimeEnd = new TimeSpan(dateT.Hour, dateT.Minute, 0);
        }
    }

    public static DateTime ParseTime(string time)
    {
        var militaryTime = time;
        var dTime = DateTime.MinValue;
        if (militaryTime == "") return dTime;
        if (militaryTime.Length < 4) militaryTime = militaryTime.PadLeft(4, '0');
        //Test if user typed in military time. Ex: 0830 or 1536
        try
        {
            var hour = SIn.Int(militaryTime.Substring(0, 2));
            var minute = SIn.Int(militaryTime.Substring(2, 2));
            dTime = new DateTime(1, 1, 1, hour, minute, 0);
            return dTime;
        }
        catch
        {
        }

        //Test if user typed in a typical DateTime format. Ex: 1:00 PM
        try
        {
            return DateTime.Parse(time);
        }
        catch
        {
        }

        return dTime;
    }

    public static bool SetAndValidateToothData(ProcedureCode procedureCode, Procedure procedure, string textTooth, string textSurfaces, List<int> listBoxTeethSelectedIndices, List<int> listBoxTeeth2SelectedIndices, List<string> listPriTeeth, string translationSource, Action<string> actionOnFailure = null)
    {
        if (procedureCode.TreatArea == TreatmentArea.None
            || procedureCode.TreatArea == TreatmentArea.Mouth)
        {
            procedure.Surf = "";
            procedure.ToothNum = "";
        }

        if (procedureCode.TreatArea == TreatmentArea.Surf)
        {
            procedure.ToothNum = Tooth.Parse(textTooth);
            procedure.Surf = Tooth.SurfTidyFromDisplayToDb(textSurfaces, procedure.ToothNum);
        }

        if (procedureCode.TreatArea == TreatmentArea.Tooth)
        {
            procedure.Surf = "";
            procedure.ToothNum = Tooth.Parse(textTooth);
        }

        if (procedureCode.TreatArea == TreatmentArea.Quad)
            //surf set when radio pushed
            procedure.ToothNum = "";
        if (procedureCode.TreatArea == TreatmentArea.Sextant)
            //surf taken care of when radio pushed
            procedure.ToothNum = "";
        if (procedureCode.TreatArea == TreatmentArea.Arch)
            //taken care of when radio pushed
            procedure.ToothNum = "";
        if (procedureCode.TreatArea == TreatmentArea.ToothRange
            || procedureCode.AreaAlsoToothRange)
        {
            if (listBoxTeethSelectedIndices.Count < 1 && listBoxTeeth2SelectedIndices.Count < 1)
            {
                actionOnFailure?.Invoke(Lans.g(translationSource, "Must pick at least 1 tooth"));
                return false;
            }

            var listSelectedToothNums = new List<string>();
            //Store selected teeth in Maxillary/Upper Arch.
            foreach (var index in listBoxTeethSelectedIndices) listSelectedToothNums.Add((index + 1).ToString());
            //Store selected teeth in Mandibular/Lower Arch.
            foreach (var index in listBoxTeeth2SelectedIndices) listSelectedToothNums.Add((32 - index).ToString());
            //Identify selected teeth which are primary and convert from permanent tooth num to primary tooth num for storage into database.
            for (var j = 0; j < listSelectedToothNums.Count; j++)
                if (listPriTeeth.Contains(listSelectedToothNums[j]))
                    listSelectedToothNums[j] = Tooth.PermToPri(listSelectedToothNums[j]);

            procedure.ToothRange = string.Join(",", listSelectedToothNums);
            procedure.ToothNum = "";
            if (procedureCode.AreaAlsoToothRange)
            {
                //arch or quad stored in surf
            }
            else
            {
                procedure.Surf = "";
            }
        }

        return true;
    }

    public static void SetNote(Procedure procedure, Procedure procedureOld, string textNotes)
    {
        //Status taken care of when list pushed
        procedure.Note = textNotes;
        //Larger offices have trouble with doctors editing specific procedure notes at the same time.
        //One of our customers paid for custom programming that will merge the two notes together in a specific fashion if there was concurrency issues.
        //A specific preference was added because this functionality is so custom.  Typical users can just use the Chart View Audit mode for this info.
        if (procedureOld.ProcNum > 0 && PrefC.GetBool(PrefName.ProcNoteConcurrencyMerge))
        {
            //Go to the database to get the most recent version of the current procedure's note and check it against ProcOld.Note to see if they differ.
            var listProcNotes = ProcNotes.GetProcNotesForProc(procedureOld.ProcNum)
                .OrderByDescending(x => x.EntryDateTime)
                .ThenBy(x => x.ProcNoteNum) //Just in case two notes were entered at the "same time" (current version of MySQL can't handle milliseconds)
                .ToList();
            //If there are notes for the current procedure, get the most recent note and compare it to ProcOld.Note.
            //If the current database note differs from the ProcOld.Note then there was a concurrency issue and we have to merge the db note.
            if (listProcNotes.Count > 0 && procedureOld.Note != listProcNotes[0].Note)
            {
                //Manipulate ProcCur.Note to include the most recent note in its entirety with some custom information required by job #2484
                //Use DateTime.Now because the ProcNote won't get inserted until farther down in this method but we have to do this manipulation before sig.
                procedure.Note = DateTime.Now + "  " + Userods.GetName(procedure.UserNum) + "\r\n" + procedure.Note;
                //Now we need to append the old note from the database in the same format.
                procedure.Note += "\r\n------------------------------------------------------\r\n"
                                  + listProcNotes[0].EntryDateTime + "  " + Userods.GetName(listProcNotes[0].UserNum)
                                  + "\r\n" + listProcNotes[0].Note;
            }
        }
    }

    public static void SetCanadianEditFields(Procedure procedure, bool checkTypeCodeAChecked, bool checkTypeCodeBChecked, bool checkTypeCodeCChecked, bool checkTypeCodeEChecked, bool checkTypeCodeLChecked, bool checkTypeCodeSChecked, bool checkTypeCodeXChecked)
    {
        procedure.CanadianTypeCodes = "";
        if (checkTypeCodeAChecked) procedure.CanadianTypeCodes += "A";
        if (checkTypeCodeBChecked) procedure.CanadianTypeCodes += "B";
        if (checkTypeCodeCChecked) procedure.CanadianTypeCodes += "C";
        if (checkTypeCodeEChecked) procedure.CanadianTypeCodes += "E";
        if (checkTypeCodeLChecked) procedure.CanadianTypeCodes += "L";
        if (checkTypeCodeSChecked) procedure.CanadianTypeCodes += "S";
        if (checkTypeCodeXChecked) procedure.CanadianTypeCodes += "X";
    }

    public static void CanadianLabHelper(string textCanadaLabFee1Text, bool isCanadianLab, List<Procedure> listCanadaLabFees, Procedure procedure, string textCanadaLabFee2Text)
    {
        double canadaLabFee1 = 0;
        if (textCanadaLabFee1Text != "") canadaLabFee1 = SIn.Double(textCanadaLabFee1Text);
        if (canadaLabFee1 == 0)
        {
            if (!isCanadianLab && listCanadaLabFees.Count > 0) //Don't worry about deleting child lab fees if we are editing a lab fee. No such concept.
                TryDeleteLab(listCanadaLabFees[0]);
        }
        else
        {
            //canadaLabFee1!=0
            if (listCanadaLabFees.Count > 0)
            {
                //Retain the old lab code if present.
                var labFee1Old = listCanadaLabFees[0].Copy();
                listCanadaLabFees[0].ProcFee = canadaLabFee1;
                Update(listCanadaLabFees[0], labFee1Old);
            }
            else
            {
                CanadaLabProcEditHelper(procedure, canadaLabFee1);
            }
        }

        double canadaLabFee2 = 0;
        if (textCanadaLabFee2Text != "") canadaLabFee2 = SIn.Double(textCanadaLabFee2Text);
        if (canadaLabFee2 == 0)
        {
            if (!isCanadianLab && listCanadaLabFees.Count > 1) //Don't worry about deleting child lab fees if we are editing a lab fee. No such concept.
                TryDeleteLab(listCanadaLabFees[1]);
        }
        else
        {
            //canadaLabFee2!=0
            if (listCanadaLabFees.Count > 1)
            {
                //Retain the old lab code if present.
                var labFee2Old = listCanadaLabFees[1].Copy();
                listCanadaLabFees[1].ProcFee = canadaLabFee2;
                Update(listCanadaLabFees[1], labFee2Old);
            }
            else
            {
                CanadaLabProcEditHelper(procedure, canadaLabFee2);
            }
        }
    }

    public static void CanadaLabProcEditHelper(Procedure procedure, double labFeeAmt)
    {
        if (!CultureInfo.CurrentCulture.Name.EndsWith("CA")) return; //We don't care if we're not Canadian.
        var labCode = ProcedureCodes.GetProcCode("99111");
        var labFee = new Procedure();
        labFee.PatNum = procedure.PatNum;
        labFee.ProcDate = procedure.ProcDate;
        labFee.ProcFee = labFeeAmt;
        labFee.ProcStatus = procedure.ProcStatus;
        labFee.DateEntryC = DateTime.Now;
        labFee.ClinicNum = procedure.ClinicNum;
        labFee.ProcNumLab = procedure.ProcNum;
        labFee.CodeNum = labCode.CodeNum;
        //Not sure if Place of Service is required for canadian labs. (I don't see any reason why this would/could/should break anything.)
        labFee.PlaceService = Clinics.GetPlaceService(procedure.ClinicNum);
        if (labFee.CodeNum == 0)
        {
            //Code does not exist.
            var code99111 = new ProcedureCode();
            code99111.IsCanadianLab = true;
            code99111.ProcCode = "99111";
            code99111.Descript = "+L Commercial Laboratory Procedures";
            code99111.AbbrDesc = "Lab Fee";
            code99111.ProcCat = Defs.GetByExactNameNeverZero(DefCat.ProcCodeCats, "Adjunctive General Services");
            ProcedureCodes.Insert(code99111);
            labFee.CodeNum = code99111.CodeNum;
            ProcedureCodes.RefreshCache();
        }

        labFee.ProvNum = procedure.ProvNum;
        if (labCode.ProvNumDefault != 0) labFee.ProvNum = labCode.ProvNumDefault;
        Insert(labFee);
    }

    public static void SetProsthEditFields(ProcedureCode procedureCode, Procedure procedure, int listProsthSelectedIndex, DateTime dateOriginalProsth, bool checkIsDateProsthEstChecked)
    {
        if (procedureCode.IsProsth)
        {
            switch (listProsthSelectedIndex)
            {
                case 0:
                    procedure.Prosthesis = "";
                    break;
                case 1:
                    procedure.Prosthesis = "I";
                    break;
                case 2:
                    procedure.Prosthesis = "R";
                    break;
            }

            procedure.DateOriginalProsth = dateOriginalProsth;
            procedure.IsDateProsthEst = checkIsDateProsthEstChecked;
        }
        else
        {
            procedure.Prosthesis = "";
            procedure.DateOriginalProsth = DateTime.MinValue;
            procedure.IsDateProsthEst = false;
        }
    }

    public static bool TryAutoCodesPrompt(ref Procedure procedure, Procedure procedureOld, ProcedureCode procedureCode, bool isMandibular, Patient patient, ref List<ClaimProc> listClaimProcsForProc, Func<long, Procedure> funcPromptFormACLI, Userod userod = null)
    {
        var perm = GroupPermissions.SwitchExistingPermissionIfNeeded(EnumPermType.ProcCompleteEdit, procedure);
        var dateForPerm = GetDateForPermCheck(procedure);
        bool isAuthorized;
        if (userod == null)
            isAuthorized = Security.IsAuthorized(perm, dateForPerm, true);
        else
            isAuthorized = Security.IsAuthorized(perm, dateForPerm, true, true, userod);
        if (!procedureOld.ProcStatus.In(ProcStat.C, ProcStat.EO, ProcStat.EC) || isAuthorized)
        {
            //Only check auto codes if the procedure is not complete or the user has permission to edit completed procedures.
            var codeNumRecommended = AutoCodeItems.GetRecommendedCodeNum(procedure, procedureCode, patient, isMandibular, listClaimProcsForProc);
            if (procedure.CodeNum != codeNumRecommended)
            {
                var proc = funcPromptFormACLI(codeNumRecommended);
                if (proc == null) //null if preference requires user to use suggested auto-code and they chose to edit the procedure
                    return false;
                procedure = proc;
                listClaimProcsForProc = ClaimProcs.RefreshForProc(procedure.ProcNum); //funPromptFormACLI may have added claimprocs.
            }
        }

        return true;
    }

    public static void TryValidateProcFee(Procedure procedure, Procedure procedureOld, Patient patient, List<Fee> listFees, List<PatPlan> listPatPlans, List<InsSub> listInsSubs, List<InsPlan> listInsPlans, List<Benefit> listBenefits, Func<string, bool> funcYesNoPrompt)
    {
        if (procedure.ProvNum != procedureOld.ProvNum
            && procedure.ProcFee == procedureOld.ProcFee)
        {
            var promptText = "";
            var procFeeHelper = new ProcFeeHelper(patient, listFees, listPatPlans, listInsSubs, listInsPlans, listBenefits);
            var isUpdatingFee = ShouldFeesChange([procedure.Copy()], [procedureOld.Copy()],
                ref promptText, procFeeHelper
            );
            if (isUpdatingFee)
            {
                //Made it past the pref check.
                if (promptText != "" && !funcYesNoPrompt.Invoke(promptText)) isUpdatingFee = false;
                if (isUpdatingFee)
                    procedure.ProcFee = GetProcFee(patient, listPatPlans, listInsSubs, listInsPlans, procedure, listBenefits, listFees
                    );
            }
        }
    }

    public static void AfterProcsSetComplete(List<Procedure> listProcedures)
    {
        if (PrefC.GetBool(PrefName.SalesTaxDoAutomate))
            for (var i = 0; i < listProcedures.Count; i++)
                Adjustments.AddSalesTaxIfNoneExists(listProcedures[i]);
    }

    public static string GetSignatureKeyData(Procedure procedure, string updatedNote = null)
    {
        var keyData = procedure.Note;
        if (updatedNote != null) keyData = updatedNote;
        keyData += procedure.UserNum.ToString();
        keyData = keyData.Replace("\r\n", "\n"); //We need all newlines to be the same, a mix of /r/n and /n can invalidate the procedure signature.
        return keyData;
    }

    public static void SetDiagnosticCodesToDefault(Procedure procedure, ProcedureCode procedureCode)
    {
        if (string.IsNullOrEmpty(procedureCode.DiagnosticCodes))
        {
            procedure.DiagnosticCode = PrefC.GetString(PrefName.ICD9DefaultForNewProcs);
        }
        else
        {
            var listDiagnosticCodes = procedureCode.DiagnosticCodes.Split(',').ToList();
            procedure.DiagnosticCode = listDiagnosticCodes[0];
            if (listDiagnosticCodes.Count > 1) procedure.DiagnosticCode2 = listDiagnosticCodes[1];
            if (listDiagnosticCodes.Count > 2) procedure.DiagnosticCode3 = listDiagnosticCodes[2];
            if (listDiagnosticCodes.Count > 3) procedure.DiagnosticCode4 = listDiagnosticCodes[3];
        }
    }

    public static Result DeleteProcedure(Procedure procedure, Procedure procedureOld)
    {
        var result = new Result {IsSuccess = false};
        var listClaimProcs = ClaimProcs.RefreshForProc(procedure.ProcNum);
        var orthoProcLink = OrthoProcLinks.GetByProcNum(procedure.ProcNum);
        if (procedureOld.ProcStatus == ProcStat.TP || procedureOld.ProcStatus == ProcStat.TPi || procedureOld.ProcStatus == ProcStat.C)
        {
            var claimProcPreAuth = listClaimProcs.Where(x => x.ProcNum == procedureOld.ProcNum && x.ClaimNum != 0 && x.Status == ClaimProcStatus.Preauth).FirstOrDefault();
            if (claimProcPreAuth != null && ClaimProcs.RefreshForClaim(claimProcPreAuth.ClaimNum).GroupBy(x => x.ProcNum).Count() == 1)
            {
                result.Msg = "Not allowed to delete the last procedure from a preauthorization. The entire preauthorization would have to be deleted.";
                return result;
            }
        }

        if (orthoProcLink != null)
        {
            result.Msg = "Not allowed to delete a procedure that is linked to an ortho case. " +
                         "Detach the procedure from the ortho case or delete the ortho case first.";
            return result;
        }

        if (PrefC.GetBool(PrefName.ApptsRequireProc))
        {
            var listAppointmentsEmpty = Appointments.GetApptsGoingToBeEmpty([procedure]);
            if (listAppointmentsEmpty.Count > 0)
            {
                result.Msg = "Not allowed to delete the last procedure from an appointment.";
                return result;
            }
        }

        if (procedure.AptNum != 0 || procedure.PlannedAptNum != 0)
        {
            //If the procedure is attached to an appointment
            var res = Appointments.CheckRequiredProcForApptType([procedure]);
            if (res != "")
            {
                result.Msg = res;
                return result;
            }
        }

        try
        {
            Delete(procedure.ProcNum); //also deletes the claimProcs and adjustments. Might throw exception.
            Recalls.Synch(procedure.PatNum); //needs to be moved into Procedures.Delete
        }
        catch (Exception ex)
        {
            result.Msg = ex.Message;
            return result;
        }

        var permissions = EnumPermType.ProcDelete;
        var tag = "";
        switch (procedureOld.ProcStatus)
        {
            case ProcStat.C:
                permissions = EnumPermType.ProcCompleteStatusEdit;
                tag = ", Deleted";
                break;
            case ProcStat.EO:
            case ProcStat.EC:
                permissions = EnumPermType.ProcExistingEdit;
                tag = ", Deleted";
                break;
        }

        SecurityLogs.MakeLogEntry(permissions, procedureOld.PatNum,
            ProcedureCodes.GetProcCode(procedureOld.CodeNum).ProcCode + " (" + procedureOld.ProcStatus + "), " + procedureOld.ProcFee.ToString("c") + tag);
        result.IsSuccess = true;
        return result;
    }

    public static bool HasPermissionsToEditProcWithClaim(List<ClaimProc> listClaimProcs, List<Claim> listClaims, long userNum = 0, bool suppressMessage = false)
    {
        var hasSentOrRecPreauth = false;
        var hasSentOrRecClaim = false;
        var dateOldestClaim = GetOldestClaimDate(listClaimProcs, false);
        var dateOldestPreAuth = GetOldestPreAuth(listClaimProcs);
        var listPermissionss = new List<EnumPermType>();
        var listClaimProcClaimNums = listClaimProcs.Select(x => x.ClaimNum).ToList();
        var listClaimsSentOrReceived = listClaims.Where(x => x.ClaimStatus == "R" || x.ClaimStatus == "S").ToList();
        for (var i = 0; i < listClaimsSentOrReceived.Count; i++)
        {
            var claim = listClaimsSentOrReceived[i];
            if (listClaimProcClaimNums.Contains(claim.ClaimNum))
            {
                hasSentOrRecPreauth |= claim.ClaimType == "PreAuth";
                hasSentOrRecClaim |= claim.ClaimType != "PreAuth";
            }
        }

        if (hasSentOrRecPreauth) listPermissionss.Add(EnumPermType.PreAuthSentEdit);
        if (hasSentOrRecClaim) listPermissionss.Add(EnumPermType.ClaimSentEdit);
        var isAllowed = true;
        for (var i = 0; i < listPermissionss.Count(); i++)
            if (listPermissionss[i] == EnumPermType.PreAuthSentEdit)
            {
                if (userNum == 0)
                    //userNum was not passed in so use the curUser object in the Sercurity class
                    isAllowed &= Security.IsAuthorized(listPermissionss[i], dateOldestPreAuth, suppressMessage);
                else
                    isAllowed &= Security.IsAuthorized(listPermissionss[i], dateOldestPreAuth, suppressMessage, false, Userods.GetUser(userNum));
            }
            else
            {
                if (userNum == 0)
                    //userNum was not passed in so use the curUser object in the Sercurity class
                    isAllowed &= Security.IsAuthorized(listPermissionss[i], dateOldestClaim, suppressMessage);
                else
                    isAllowed &= Security.IsAuthorized(listPermissionss[i], dateOldestClaim, suppressMessage, false, Userods.GetUser(userNum));
            }

        return isAllowed;
    }
    
    private static Procedure CreateProcedureForInsHist(Patient patient, DateTime date, PrefName prefName)
    {
        //Create new EO procedure. Default to the patient's clinic, primary provider, and the first code in the InsHistPref
        var procedureCode = ProcedureCodes.GetByInsHistPref(prefName);
        var procedure = new Procedure();
        procedure.CodeNum = procedureCode.CodeNum;
        procedure.PatNum = patient.PatNum;
        procedure.ProcDate = date;
        procedure.DateTP = date;
        procedure.ProcStatus = ProcStat.EO;
        procedure.ProvNum = patient.PriProv;
        procedure.BaseUnits = procedureCode.BaseUnits;
        procedure.RevCode = procedureCode.RevenueCodeDefault;
        if (procedure.ProvNum == 0) procedure.ProvNum = patient.SecProv;
        if (procedure.ProvNum == 0) procedure.ProvNum = PrefC.GetLong(PrefName.PracticeDefaultProv);
        procedure.ClinicNum = patient.ClinicNum;
        procedure.SiteNum = patient.SiteNum;
        SetDiagnosticCodesToDefault(procedure, procedureCode);
        procedure.PlaceService = Clinics.GetPlaceService(procedure.ClinicNum);
        procedure.Surf = "";
        if (prefName == PrefName.InsHistPerioLLCodes)
            procedure.Surf = "LL";
        else if (prefName == PrefName.InsHistPerioLRCodes)
            procedure.Surf = "LR";
        else if (prefName == PrefName.InsHistPerioURCodes)
            procedure.Surf = "UR";
        else if (prefName == PrefName.InsHistPerioULCodes) procedure.Surf = "UL";
        return procedure;
    }

    public static Procedure GetMostRecentInsHistProc(List<Procedure> listProcedures, List<long> listCodeNumsInsHist, PrefName prefName)
    {
        var listProceduresHistCodeNum = listProcedures.FindAll(x => listCodeNumsInsHist.Contains(x.CodeNum));
        List<Procedure> listProceduresFiltered;
        //For Perio procs, we also need to look at the surface a procedure was done on.
        switch (prefName)
        {
            case PrefName.InsHistPerioURCodes:
                listProceduresFiltered = listProceduresHistCodeNum.FindAll(x => x.Surf.Contains("UR"));
                break;
            case PrefName.InsHistPerioULCodes:
                listProceduresFiltered = listProceduresHistCodeNum.FindAll(x => x.Surf.Contains("UL"));
                break;
            case PrefName.InsHistPerioLRCodes:
                listProceduresFiltered = listProceduresHistCodeNum.FindAll(x => x.Surf.Contains("LR"));
                break;
            case PrefName.InsHistPerioLLCodes:
                listProceduresFiltered = listProceduresHistCodeNum.FindAll(x => x.Surf.Contains("LL"));
                break;
            default: //No change in the list.
                listProceduresFiltered = listProceduresHistCodeNum;
                break;
        }

        if (listProceduresFiltered.Count == 0) return null;
        //Returns the most recent procedure
        return listProceduresFiltered.OrderBy(x => x.ProcDate).LastOrDefault();
    }

    public static void InsertOrUpdateInsHistProcedure(Patient patient, PrefName prefName, DateTime date, long planNum, long insSubNum, Procedure procedure, List<ClaimProc> listClaimProcsForProc)
    {
        //Add a new EO procedure if the proc is null or proc does not have a Status of EO and the new date is greater. 
        if (procedure == null || (procedure.ProcStatus != ProcStat.EO && date.Date > procedure.ProcDate.Date))
        {
            procedure = CreateProcedureForInsHist(patient, date, prefName);
            Insert(procedure);
            ClaimProcs.InsertClaimProcForInsHist(procedure, planNum, insSubNum);
            Recalls.Synch(patient.PatNum); //A new EO procedure was added, run recall sync.
        }
        //Only Update the date if new date is different than the ProcDate and proc has a Status of EO.
        else if (procedure.ProcStatus == ProcStat.EO && date.Date != procedure.ProcDate.Date)
        {
            var procedureOld = procedure.Copy();
            procedure.ProcDate = date;
            Update(procedure, procedureOld);
            //We need to update claimprocs or create new claimprocs if we made changes to the EO procedure in this form.
            if (listClaimProcsForProc.Count > 0)
                ClaimProcs.UpdateClaimProcForInsHist(listClaimProcsForProc.FindAll(x => x.ProcDate != date || x.Status != ClaimProcStatus.InsHist), date, insSubNum);
            else
                ClaimProcs.InsertClaimProcForInsHist(procedure, planNum, insSubNum);
        }
    }
}

public enum CreditCalcType
{
    IncludeAll,
    AllocatedOnly,
    ExcludeAll
}

public class BenefitProcCodes
{
    public Benefit Benefit;
    public List<string> ListProcCodes;
}