using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class DatabaseMaintenances
{
    [DbmMethodAttr(HasPatNum = true)]
    public static string ClaimDeleteWithNoClaimProcs(bool verbose, DbmMode dbmMode, long patNum = 0)
    {
        var log = "";
        string command;
        var patWhere = "";
        if (patNum > 0) patWhere = $"PatNum={SOut.Long(patNum)} AND ";
        switch (dbmMode)
        {
            case DbmMode.Check:
                command = $"SELECT COUNT(*) FROM claim " +
                          $"WHERE {patWhere}NOT EXISTS(SELECT * FROM claimproc WHERE claim.ClaimNum=claimproc.ClaimNum AND IsOverPay=0)";
                var numFound = SIn.Int(Db.GetCount(command));
                if (numFound != 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Claims found with no claimprocs") + ": " + numFound + "\r\n";
                break;
            case DbmMode.Fix:
                var listDbmLogs = new List<DbmLog>();
                var methodName = MethodBase.GetCurrentMethod().Name;
                command = "SELECT claim.ClaimNum FROM claim "
                          + $"WHERE {patWhere}NOT EXISTS(SELECT * FROM claimproc WHERE claim.ClaimNum=claimproc.ClaimNum AND IsOverPay=0)";
                var tableClaimNums = DataCore.GetTable(command);
                var listClaimNums = new List<long>();
                for (var i = 0; i < tableClaimNums.Rows.Count; i++) listClaimNums.Add(SIn.Long(tableClaimNums.Rows[i]["ClaimNum"].ToString()));
                if (listClaimNums.Count > 0)
                {
                    var listEnumPermTypes = GroupPermissions.GetPermsFromCrudAuditPerm(CrudTableAttribute.GetCrudAuditPermForClass(typeof(Claim)));
                    var listSecurityLogs = SecurityLogs.GetFromFKeysAndType(listClaimNums, listEnumPermTypes);
                    Claims.ClearFkey(listClaimNums); //Zero securitylog FKey column for rows to be deleted.
                    //Insert changes to DbmLogs
                    for (var i = 0; i < listSecurityLogs.Count; i++)
                    {
                        var dbmLog = new DbmLog(Security.CurUser.UserNum, listSecurityLogs[i].SecurityLogNum, DbmLogFKeyType.Securitylog, DbmLogActionType.Update,
                            methodName, "Updated FKey from " + listSecurityLogs[i].FKey + " to 0 from ClaimDeleteWithNoClaimProcs.");
                        listDbmLogs.Add(dbmLog);
                    }
                }

                //Orphaned claims do not show in the account module (tested) so we need to delete them because no other way.
                command = @"DELETE FROM claim "
                          + $"WHERE {patWhere}NOT EXISTS(SELECT * FROM claimproc WHERE claim.ClaimNum=claimproc.ClaimNum AND IsOverPay=0)";
                var numberFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listClaimNums.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listClaimNums[i], DbmLogFKeyType.Claim, DbmLogActionType.Delete,
                        methodName, "Deleted claim from ClaimDeleteWithNoClaimProcs");
                    listDbmLogs.Add(dbmLog);
                }

                if (numberFixed != 0 || verbose)
                {
                    DbmLogCrud.InsertMany(listDbmLogs);
                    log += Lans.g("FormDatabaseMaintenance", "Claims deleted due to no claimprocs") + ": " + numberFixed + "\r\n";
                }

                break;
        }

        return log;
    }

    [DbmMethodAttr]
    public static string ComputerPrefDuplicates(bool verbose, DbmMode dbmMode)
    {
        //Min may not be the oldest when using random primary keys, but we have to pick one.
        var command = "SELECT MIN(ComputerPrefNum) ComputerPrefNum, ComputerName "
                      + "FROM computerpref "
                      + "GROUP BY ComputerName "
                      + "HAVING COUNT(*)>1 ";
        var table = DataCore.GetTable(command);
        var log = "";
        switch (dbmMode)
        {
            case DbmMode.Check:
                if (table.Rows.Count > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "ComputerPref duplicate computer name entries found:") + " " + table.Rows.Count + "\r\n";
                break;
            case DbmMode.Fix:
                var numberFixed = 0;
                if (table.Rows.Count > 0)
                {
                    command = "DELETE FROM computerpref WHERE ComputerPrefNum NOT IN ("
                              + string.Join(",", table.Select().Select(x => SOut.Long(SIn.Long(x["ComputerPrefNum"].ToString())))) + ") "
                              + "AND ComputerName IN ("
                              + string.Join(",", table.Select().Select(x => $"'{SOut.String(SIn.String(x["ComputerName"].ToString()))}'")) + ")";
                    numberFixed = (int) Db.NonQ(command);
                }

                if (numberFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "ComputerPref duplicate computer name entries deleted:") + " " + numberFixed + "\r\n";
                break;
        }

        return log;
    }

    [DbmMethodAttr]
    public static string DiseaseWithInvalidDiseaseDef(bool verbose, DbmMode dbmMode)
    {
        
        var log = "";
        var command = @"SELECT DiseaseNum,DiseaseDefNum FROM disease WHERE DiseaseDefNum NOT IN(SELECT DiseaseDefNum FROM diseasedef)";
        var table = DataCore.GetTable(command);
        var numFound = table.Rows.Count;
        switch (dbmMode)
        {
            case DbmMode.Check:
                if (numFound > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Problems with invalid references found") + ": " + numFound + "\r\n";
                break;
            case DbmMode.Fix:
                if (numFound > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Problems with invalid references found") + ": " + numFound + "\r\n";
                if (numFound > 0)
                {
                    //Check to see if there is already a diseasedef called UNKNOWN PROBLEM.
                    command = "SELECT DiseaseDefNum FROM diseasedef WHERE DiseaseName='UNKNOWN PROBLEM'";
                    var diseaseDefNum = SIn.Long(DataCore.GetScalar(command));
                    if (diseaseDefNum == 0)
                    {
                        //Create a new DiseaseDef called UNKNOWN PROBLEM.
                        var diseaseDef = new DiseaseDef();
                        diseaseDef.DiseaseName = "UNKNOWN PROBLEM";
                        diseaseDef.IsHidden = false;
                        diseaseDefNum = DiseaseDefs.Insert(diseaseDef);
                    }

                    //Update the disease table.
                    command = "UPDATE disease SET DiseaseDefNum=" + SOut.Long(diseaseDefNum) + " WHERE DiseaseNum IN("
                              + string.Join(",", table.Select().Select(x => SIn.Long(x["DiseaseNum"].ToString()))) + ")";
                    Db.NonQ(command);
                    log += Lans.g("FormDatabaseMaintenance", "All invalid references have been attached to the problem called") + " UNKNOWN PROBLEM.\r\n";
                }

                break;
        }

        return log;
    }
    
    [DbmMethodAttr]
    public static string EbillMissingDefaultEntry(DbmMode dbmMode)
    {
        var log = "";
        var command = "SELECT COUNT(*) FROM ebill WHERE ClinicNum=0";
        var numFound = SIn.Int(Db.GetCount(command));
        switch (dbmMode)
        {
            case DbmMode.Check:
                if (numFound == 0) log += Lans.g("FormDatabaseMaintenance", "Missing default ebill entry.");
                break;
            case DbmMode.Fix:
                if (numFound == 0)
                {
                    var ebill = new Ebill();
                    ebill.ClinicNum = 0;
                    ebill.ClientAcctNumber = "";
                    ebill.ElectUserName = "";
                    ebill.ElectPassword = "";
                    ebill.PracticeAddress = EbillAddress.PracticePhysical;
                    ebill.RemitAddress = EbillAddress.PracticeBilling;
                    var ebillNum = EbillCrud.Insert(ebill);
                    if (ebillNum > 0) log += Lans.g("FormDatabaseMaintenance", "Default ebill entry inserted.");
                }

                break;
        }

        return log;
    }

    [DbmMethodAttr(HasBreakDown = true, HasPatNum = true)]
    public static string InsSubNumMismatchPlanNum(bool verbose, DbmMode dbmMode, long patNumSpecific = 0)
    {
        
        var log = "";
        //Not going to validate the following tables because they do not have an InsSubNum column: appointmentx2, benefit.
        //This DBM assumes that the inssub table is correct because that's what we're comparing against.
        string command;
        switch (dbmMode)
        {
            case DbmMode.Check:

                #region CHECK

                var numFound = 0;
                var hasBreakDown = false;
                //Can't do the following because no inssubnum: appointmentx2, benefit.
                //Can't do inssub because that's what we're comparing against.  That's the one that's assumed to be correct.
                //claim.PlanNum -----------------------------------------------------------------------------------------------------
                command = "SELECT COUNT(*) FROM claim "
                          + "WHERE PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum) "
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                numFound = SIn.Int(Db.GetCount(command));
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Mismatched claim InsSubNum/PlanNum values") + ": " + numFound + "\r\n";
                    if (numFound > 0) hasBreakDown = true;
                }

                //claim.PlanNum2---------------------------------------------------------------------------------------------------
                command = "SELECT COUNT(*) FROM claim "
                          + "WHERE PlanNum2 != 0 "
                          + "AND PlanNum2 NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum2)"
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                numFound = SIn.Int(Db.GetCount(command));
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Mismatched claim InsSubNum2/PlanNum2 values") + ": " + numFound + "\r\n";
                    if (numFound > 0) hasBreakDown = true;
                }

                //claimproc---------------------------------------------------------------------------------------------------
                command = "SELECT COUNT(*) FROM claimproc "
                          + "WHERE PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claimproc.InsSubNum)"
                          + PatientAndClauseHelper(patNumSpecific, "claimproc");
                numFound = SIn.Int(Db.GetCount(command));
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Mismatched claimproc InsSubNum/PlanNum values") + ": " + numFound + "\r\n";
                    if (numFound > 0) hasBreakDown = true;
                }

                //etrans---------------------------------------------------------------------------------------------------
                command = "SELECT COUNT(*) FROM etrans "
                          + "WHERE PlanNum!=0 AND InsSubNum!=0 AND PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=etrans.InsSubNum)"
                          + PatientAndClauseHelper(patNumSpecific, "etrans");
                numFound = SIn.Int(Db.GetCount(command));
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Mismatched etrans InsSubNum/PlanNum values") + ": " + numFound + "\r\n";
                    if (numFound > 0) hasBreakDown = true;
                }

                //payplan---------------------------------------------------------------------------------------------------
                command = "SELECT COUNT(*) FROM payplan "
                          + "WHERE EXISTS (SELECT PlanNum FROM inssub WHERE inssub.InsSubNum=payplan.InsSubNum AND inssub.PlanNum!=payplan.PlanNum)"
                          + PatientAndClauseHelper(patNumSpecific, "payplan");
                numFound = SIn.Int(Db.GetCount(command));
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Mismatched payplan InsSubNum/PlanNum values") + ": " + numFound + "\r\n";
                    if (numFound > 0) hasBreakDown = true;
                }

                if (hasBreakDown) log += "   " + Lans.g("FormDatabaseMaintenance", "Run Fix or double click to see a break down.");
                break;

            #endregion CHECK

            case DbmMode.Breakdown:

                #region BREAKDOWN

                //In this BREAKDOWN, when user double clicks on this DBM query and show what needs to be fixed/will attempted to be fixed when running Fix.
                //claim.PlanNum -----------------------------------------------------------------------------------------------------
                command = "SELECT claim.PatNum,claim.PlanNum,claim.ClaimNum,claim.DateService FROM claim "
                          + "WHERE PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum) "
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                var tableClaims = DataCore.GetTable(command);
                var listClaimsBad1 = new List<Claim>();
                for (var i = 0; i < tableClaims.Rows.Count; i++)
                {
                    var claim = new Claim();
                    claim.PatNum = SIn.Long(tableClaims.Rows[i]["PatNum"].ToString());
                    claim.PlanNum = SIn.Long(tableClaims.Rows[i]["PlanNum"].ToString());
                    claim.ClaimNum = SIn.Long(tableClaims.Rows[i]["ClaimNum"].ToString());
                    claim.DateService = SIn.DateTime(tableClaims.Rows[i]["DateService"].ToString());
                    listClaimsBad1.Add(claim);
                }

                //claim.PlanNum2---------------------------------------------------------------------------------------------------
                command = "SELECT claim.PatNum,claim.PlanNum2,claim.ClaimNum,claim.DateService FROM claim "
                          + "WHERE PlanNum2 != 0 "
                          + "AND PlanNum2 NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum2)"
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                var tableClaims2 = DataCore.GetTable(command);
                var listClaimsBad2 = new List<Claim>();
                for (var i = 0; i < tableClaims2.Rows.Count; i++)
                {
                    var claim = new Claim();
                    claim.PatNum = SIn.Long(tableClaims2.Rows[i]["PatNum"].ToString());
                    claim.PlanNum2 = SIn.Long(tableClaims2.Rows[i]["PlanNum2"].ToString());
                    claim.ClaimNum = SIn.Long(tableClaims2.Rows[i]["ClaimNum"].ToString());
                    claim.DateService = SIn.DateTime(tableClaims2.Rows[i]["DateService"].ToString());
                    listClaimsBad2.Add(claim);
                }

                //claimproc---------------------------------------------------------------------------------------------------
                command = "SELECT claimproc.PatNum,claimproc.ClaimProcNum,claimproc.InsSubNum,claimproc.ProcNum,claimproc.ClaimNum FROM claimproc "
                          + "WHERE PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claimproc.InsSubNum)"
                          + PatientAndClauseHelper(patNumSpecific, "claimproc");
                var tableClaimProcs = DataCore.GetTable(command);
                var listClaimProcsBad = new List<ClaimProc>();
                for (var i = 0; i < tableClaimProcs.Rows.Count; i++)
                {
                    var claimProc = new ClaimProc();
                    claimProc.PatNum = SIn.Long(tableClaimProcs.Rows[i]["PatNum"].ToString());
                    claimProc.ClaimProcNum = SIn.Long(tableClaimProcs.Rows[i]["ClaimProcNum"].ToString());
                    claimProc.InsSubNum = SIn.Long(tableClaimProcs.Rows[i]["InsSubNum"].ToString());
                    claimProc.ProcNum = SIn.Long(tableClaimProcs.Rows[i]["ProcNum"].ToString());
                    claimProc.ClaimNum = SIn.Long(tableClaimProcs.Rows[i]["ClaimNum"].ToString());
                    listClaimProcsBad.Add(claimProc);
                }

                //etrans---------------------------------------------------------------------------------------------------
                command = "SELECT etrans.PatNum,etrans.PlanNum,etrans.EtransNum,etrans.DateTimeTrans FROM etrans "
                          + "WHERE PlanNum!=0 AND InsSubNum!=0 AND PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=etrans.InsSubNum)"
                          + PatientAndClauseHelper(patNumSpecific, "etrans");
                var tableEtrans = DataCore.GetTable(command);
                var listEtransesBad = new List<Etrans>();
                for (var i = 0; i < tableEtrans.Rows.Count; i++)
                {
                    var etrans = new Etrans();
                    etrans.PatNum = SIn.Long(tableEtrans.Rows[i]["PatNum"].ToString());
                    etrans.PlanNum = SIn.Long(tableEtrans.Rows[i]["PlanNum"].ToString());
                    etrans.EtransNum = SIn.Long(tableEtrans.Rows[i]["EtransNum"].ToString());
                    etrans.DateTimeTrans = SIn.DateTime(tableEtrans.Rows[i]["DateTimeTrans"].ToString());
                    listEtransesBad.Add(etrans);
                }

                //payplan---------------------------------------------------------------------------------------------------
                command = "SELECT payplan.PatNum,payplan.PlanNum,payplan.PayPlanNum FROM payplan "
                          + "WHERE EXISTS (SELECT PlanNum FROM inssub WHERE inssub.InsSubNum=payplan.InsSubNum AND inssub.PlanNum!=payplan.PlanNum)"
                          + PatientAndClauseHelper(patNumSpecific, "payplan");
                var tablePayPlans = DataCore.GetTable(command);
                var listPayPlansBad = new List<PayPlan>();
                for (var i = 0; i < tablePayPlans.Rows.Count; i++)
                {
                    var payPlan = new PayPlan();
                    payPlan.PatNum = SIn.Long(tablePayPlans.Rows[i]["PatNum"].ToString());
                    payPlan.PlanNum = SIn.Long(tablePayPlans.Rows[i]["PlanNum"].ToString());
                    payPlan.PayPlanNum = SIn.Long(tablePayPlans.Rows[i]["PayPlanNum"].ToString());
                    listPayPlansBad.Add(payPlan);
                }

                var listPatNumsDistinct = listClaimsBad1.Select(x => x.PatNum).Distinct()
                    .Union(listClaimsBad2.Select(x => x.PatNum).Distinct())
                    .Union(listClaimProcsBad.Select(x => x.PatNum).Distinct())
                    .Union(listEtransesBad.Select(x => x.PatNum).Distinct())
                    .Union(listPayPlansBad.Select(x => x.PatNum).Distinct())
                    .ToList();
                var listPatientsLim = Patients.GetLimForPats(listPatNumsDistinct);
                numFound = listClaimsBad1.Count +
                           listClaimsBad2.Count +
                           listClaimProcsBad.Count +
                           listEtransesBad.Count +
                           listPayPlansBad.Count;
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Mismatched InsSubNum/PlanNum values") + ": " + numFound + "\r\n";
                    log += Lans.g("FormDatabaseMaintenance", "Patients affected") + ": " + listPatNumsDistinct.Count + "\r\n";
                    for (var i = 0; i < listPatientsLim.Count; i++)
                    {
                        var lineItemDBM = "   " + Lans.g("FormDatabaseMaintenance", "Patient with associated invalid PlanNums") + ":" + listPatientsLim[i].GetNameLF() + " (PatNum:" + listPatientsLim[i].PatNum + ")" + "\r\n";
                        for (var j = 0; j < listClaimsBad1.Count; j++)
                        {
                            if (listClaimsBad1[j].PatNum != listPatientsLim[i].PatNum) continue;
                            lineItemDBM += "    ClaimNum:" + listClaimsBad1[j].ClaimNum + " PlanNum:" + listClaimsBad1[j].PlanNum + " DateService:"
                                           + listClaimsBad1[j].DateService.ToShortDateString() + "\r\n";
                        }

                        for (var j = 0; j < listClaimsBad2.Count; j++)
                        {
                            if (listClaimsBad2[j].PatNum != listPatientsLim[i].PatNum) continue;
                            lineItemDBM += "    ClaimNum:" + listClaimsBad2[j].ClaimNum + " PlanNum2:" + listClaimsBad2[j].PlanNum2 + " DateService:"
                                           + listClaimsBad2[j].DateService.ToShortDateString() + "\r\n";
                        }

                        for (var j = 0; j < listClaimProcsBad.Count; j++)
                        {
                            if (listClaimProcsBad[j].PatNum != listPatientsLim[i].PatNum) continue;
                            lineItemDBM += "    ClaimProcNum:" + listClaimProcsBad[j].ClaimProcNum + " InsSubNum:" + listClaimProcsBad[j].InsSubNum + " ClaimNum:"
                                           + listClaimProcsBad[j].ClaimNum + " ProcNum:" + listClaimProcsBad[j].ProcNum + "\r\n";
                        }

                        for (var j = 0; j < listEtransesBad.Count; j++)
                        {
                            if (listEtransesBad[j].PatNum != listPatientsLim[i].PatNum) continue;
                            lineItemDBM += "    EtransNum:" + listEtransesBad[j].EtransNum + " PlanNum:" + listEtransesBad[j].PlanNum + " DateTimeTrans:"
                                           + listEtransesBad[j].DateTimeTrans.ToShortDateString() + "\r\n";
                        }

                        for (var j = 0; j < listPayPlansBad.Count; j++)
                        {
                            if (listPayPlansBad[j].PatNum != listPatientsLim[i].PatNum) continue;
                            lineItemDBM += "    PayPlanNum:" + listPayPlansBad[j].PayPlanNum + " PlanNum:" + listPayPlansBad[j].PlanNum + "\r\n";
                        }

                        lineItemDBM += "\r\n";
                        log += lineItemDBM;
                    }
                }

                break;

            #endregion BREAKDOWN

            case DbmMode.Fix:

                #region FIX

                var numFixed = 0;
                var listDbmLogs = new List<DbmLog>();
                var methodName = MethodBase.GetCurrentMethod().Name;
                var where = "";

                #region Claim PlanNum

                #region claim.PlanNum (1/4) Mismatch

                where = "WHERE PlanNum != (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum)"
                        + PatientAndClauseHelper(patNumSpecific, "claim");
                command = "SELECT * FROM claim " + where;
                var listClaims = ClaimCrud.SelectMany(command);
                command = "UPDATE claim SET PlanNum=(SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum) " + where;
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listClaims.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listClaims[i].ClaimNum, DbmLogFKeyType.Claim, DbmLogActionType.Update,
                        methodName, "Updated PlanNum from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Mismatched claim InsSubNum/PlanNum fixed") + ": " + numFixed + "\r\n";

                #endregion

                numFixed = 0;

                #region claim.PlanNum (2/4) PlanNum zero, invalid InsSubNum

                //Will leave orphaned claimprocs. No finanicals to check.
                command = "SELECT claim.ClaimNum FROM claim WHERE PlanNum=0 AND ClaimStatus IN ('PreAuth','W','U','H','I') "
                          + "AND NOT EXISTS(SELECT * FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum)"
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                var tableClaimNums = DataCore.GetTable(command);
                var listClaimNums = new List<long>();
                for (var i = 0; i < tableClaimNums.Rows.Count; i++) listClaimNums.Add(SIn.Long(tableClaimNums.Rows[i]["ClaimNum"].ToString()));
                if (listClaimNums.Count > 0)
                {
                    var listEnumPermTypes = GroupPermissions.GetPermsFromCrudAuditPerm(CrudTableAttribute.GetCrudAuditPermForClass(typeof(Claim)));
                    var listSecurityLogs = SecurityLogs.GetFromFKeysAndType(listClaimNums, listEnumPermTypes);
                    Claims.ClearFkey(listClaimNums); //Zero securitylog FKey column for rows to be deleted.
                    for (var i = 0; i < listSecurityLogs.Count; i++)
                    {
                        var dbmLog = new DbmLog(Security.CurUser.UserNum, listSecurityLogs[i].SecurityLogNum, DbmLogFKeyType.Securitylog,
                            DbmLogActionType.Update, methodName, "Set FKey to 0 from InsSubNumMismatchPlanNum.");
                        listDbmLogs.Add(dbmLog);
                    }
                }

                where = "WHERE PlanNum=0 AND ClaimStatus IN('PreAuth','W','U','H','I') AND NOT EXISTS(SELECT * FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum)"
                        + PatientAndClauseHelper(patNumSpecific, "claim");
                command = "SELECT ClaimNum FROM claim " + where;
                listClaimNums = Db.GetListLong(command);
                command = "DELETE FROM claim " + where;
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listClaimNums.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listClaimNums[i], DbmLogFKeyType.Claim, DbmLogActionType.Delete,
                        methodName, "Claim deleted with invalid InsSubNum and PlanNum=0 .");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Claims deleted with invalid InsSubNum and PlanNum=0") + ": " + numFixed + "\r\n";

                #endregion

                numFixed = 0;

                #region claim.PlanNum (3/4) PlanNum invalid, and claim.InsSubNum invalid

                command = "SELECT claim.PatNum,claim.PlanNum,claim.InsSubNum FROM claim "
                          + "WHERE PlanNum NOT IN (SELECT insplan.PlanNum FROM insplan) "
                          + "AND InsSubNum NOT IN (SELECT inssub.InsSubNum FROM inssub) "
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                var table = DataCore.GetTable(command);
                if (table.Rows.Count > 0) log += Lans.g("FormDatabaseMaintenance", "List of patients who will need insurance information reentered:") + "\r\n";
                for (var i = 0; i < table.Rows.Count; i++)
                {
                    //Create simple InsPlans and InsSubs for each claim to replace the missing ones.
                    //make sure a plan does not exist from a previous insert in this loop
                    command = "SELECT COUNT(*) FROM insplan WHERE PlanNum=" + table.Rows[i]["PlanNum"];
                    if (Db.GetCount(command) == "0")
                    {
                        var insPlan = new InsPlan();
                        insPlan.PlanNum = SIn.Long(table.Rows[i]["PlanNum"].ToString()); //reuse the existing FK
                        insPlan.IsHidden = true;
                        insPlan.CarrierNum = Carriers.GetByNameAndPhone("UNKNOWN CARRIER", "", true).CarrierNum;
                        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                        insPlan.SecUserNumEntry = Security.CurUser.UserNum;
                        InsPlans.Insert(insPlan, true);
                        var dbmLog = new DbmLog(Security.CurUser.UserNum, insPlan.PlanNum, DbmLogFKeyType.InsPlan, DbmLogActionType.Insert,
                            methodName, "Inserted new insplan from InsSubNumMismatchPlanNum.");
                        listDbmLogs.Add(dbmLog);
                    }

                    var patNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
                    //make sure an inssub does not exist from a previous insert in this loop
                    command = "SELECT COUNT(*) FROM inssub WHERE InsSubNum=" + table.Rows[i]["InsSubNum"];
                    if (Db.GetCount(command) == "0")
                    {
                        var insSub = new InsSub();
                        insSub.InsSubNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString()); //reuse the existing FK
                        insSub.PlanNum = SIn.Long(table.Rows[i]["PlanNum"].ToString());
                        insSub.Subscriber = patNum; //if this sub was created on a previous loop, this may be some other patient.
                        insSub.SubscriberID = "unknown";
                        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                        insSub.SecUserNumEntry = Security.CurUser.UserNum;
                        InsSubs.Insert(insSub, true);
                        var dbmLog = new DbmLog(Security.CurUser.UserNum, insSub.InsSubNum, DbmLogFKeyType.InsSub, DbmLogActionType.Insert,
                            methodName, "Inserted new inssub from InsSubNumMismatchPlanNum.");
                        listDbmLogs.Add(dbmLog);
                    }

                    var patient = Patients.GetLim(patNum);
                    log += "PatNum: " + patient.PatNum + " - " + Patients.GetNameFL(patient.LName, patient.FName, patient.Preferred, patient.MiddleI) + "\r\n";
                }

                numFixed = table.Rows.Count;
                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Claims with invalid PlanNums and invalid InsSubNums fixed: ") + numFixed + "\r\n";

                #endregion

                numFixed = 0;

                #region claim.PlanNum (4/4) PlanNum valid, but claim.InsSubNum invalid

                command = "SELECT PatNum,PlanNum,InsSubNum FROM claim "
                          + "WHERE PlanNum IN (SELECT insplan.PlanNum FROM insplan) "
                          + "AND InsSubNum NOT IN (SELECT inssub.InsSubNum FROM inssub) GROUP BY InsSubNum"
                          + PatientAndClauseHelper(patNumSpecific, "claim");
                table = DataCore.GetTable(command);
                //Create a dummy inssub and link it to the valid plan.
                for (var i = 0; i < table.Rows.Count; i++)
                {
                    var insSub = new InsSub();
                    insSub.InsSubNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString());
                    insSub.PlanNum = SIn.Long(table.Rows[i]["PlanNum"].ToString());
                    insSub.Subscriber = SIn.Long(table.Rows[i]["PatNum"].ToString());
                    insSub.SubscriberID = "unknown";
                    //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                    insSub.SecUserNumEntry = Security.CurUser.UserNum;
                    InsSubs.Insert(insSub, true);
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, insSub.InsSubNum, DbmLogFKeyType.InsSub, DbmLogActionType.Insert,
                        methodName, "Inserted new inssub from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                numFixed = table.Rows.Count;
                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Claims with invalid InsSubNums and invalid PlanNums fixed: ") + numFixed + "\r\n";

                #endregion

                #endregion

                numFixed = 0;

                #region Claim PlanNum2

                //claim.PlanNum2---------------------------------------------------------------------------------------------------
                where = "WHERE PlanNum2 != 0 AND PlanNum2 NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum2)"
                        + PatientAndClauseHelper(patNumSpecific, "claim");
                command = "SELECT * FROM claim " + where;
                listClaims = ClaimCrud.SelectMany(command);
                command = "UPDATE claim SET PlanNum2=(SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claim.InsSubNum2) " + where;
                //if InsSubNum2 was completely invalid, then PlanNum2 gets set to zero here.
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listClaims.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listClaims[i].ClaimNum, DbmLogFKeyType.Claim,
                        DbmLogActionType.Update, methodName, "Updated PlanNum2 from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Mismatched claim InsSubNum2/PlanNum2 fixed: ") + numFixed + "\r\n";

                #endregion

                numFixed = 0;

                #region ClaimProc

                //claimproc (1/2) If planNum is valid but InsSubNum does not exist, then add a dummy inssub----------------------------------------
                command = "SELECT PatNum,PlanNum,InsSubNum FROM claimproc "
                          + "WHERE PlanNum IN (SELECT insplan.PlanNum FROM insplan) "
                          + PatientAndClauseHelper(patNumSpecific, "claimproc")
                          + "AND InsSubNum NOT IN (SELECT inssub.InsSubNum FROM inssub) GROUP BY InsSubNum";
                table = DataCore.GetTable(command);
                //Create a dummy inssub and link it to the valid plan.
                for (var i = 0; i < table.Rows.Count; i++)
                {
                    var insSub = new InsSub();
                    insSub.InsSubNum = SIn.Long(table.Rows[i]["InsSubNum"].ToString());
                    insSub.PlanNum = SIn.Long(table.Rows[i]["PlanNum"].ToString());
                    insSub.Subscriber = SIn.Long(table.Rows[i]["PatNum"].ToString());
                    insSub.SubscriberID = "unknown";
                    //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                    insSub.SecUserNumEntry = Security.CurUser.UserNum;
                    InsSubs.Insert(insSub, true);
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, insSub.InsSubNum, DbmLogFKeyType.InsSub, DbmLogActionType.Insert,
                        methodName, "Inserted new inssub from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                numFixed = table.Rows.Count;
                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Claims with valid PlanNums and invalid InsSubNums fixed: ") + numFixed + "\r\n";
                numFixed = 0;
                //claimproc (2/2) Mismatch, but InsSubNum is valid
                where = "WHERE PlanNum != (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claimproc.InsSubNum)"
                        + PatientAndClauseHelper(patNumSpecific, "claimproc");
                command = "SELECT * FROM claimproc " + where;
                var listClaimProcs = ClaimProcCrud.SelectMany(command);
                command = "UPDATE claimproc SET PlanNum=(SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=claimproc.InsSubNum) " + where;
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listClaimProcs.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listClaimProcs[i].ClaimProcNum, DbmLogFKeyType.ClaimProc,
                        DbmLogActionType.Update, methodName, "Updated PlanNum from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Mismatched claimproc InsSubNum/PlanNum fixed: ") + numFixed + "\r\n";
                numFixed = 0;
                //claimproc.PlanNum zero, invalid InsSubNum--------------------------------------------------------------------------------
                where = "WHERE PlanNum=0 AND NOT EXISTS(SELECT * FROM inssub WHERE inssub.InsSubNum=claimproc.InsSubNum)"
                        + " AND InsPayAmt=0 AND WriteOff=0" //Make sure this deletion will not affect financials.
                        + " AND Status IN (6,2)" //OK to delete because no claim and just an estimate (6) or preauth (2) claimproc
                        + PatientAndClauseHelper(patNumSpecific, "claimproc");
                command = "SELECT * FROM claimproc " + where;
                listClaimProcs = ClaimProcCrud.SelectMany(command);
                command = "DELETE FROM claimproc " + where;
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listClaimProcs.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listClaimProcs[i].ClaimProcNum, DbmLogFKeyType.ClaimProc,
                        DbmLogActionType.Delete, methodName, "Deleted claimproc from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Claimprocs deleted with invalid InsSubNum and PlanNum=0: ") + numFixed + "\r\n";

                #endregion

                numFixed = 0;

                #region Etrans

                //etrans---------------------------------------------------------------------------------------------------
                where = "WHERE PlanNum!=0 AND InsSubNum!=0 AND PlanNum NOT IN (SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=etrans.InsSubNum)"
                        + PatientAndClauseHelper(patNumSpecific, "etrans");
                command = "SELECT * FROM etrans " + where;
                var listEtranses = EtransCrud.SelectMany(command);
                command = "UPDATE etrans SET PlanNum=(SELECT inssub.PlanNum FROM inssub WHERE inssub.InsSubNum=etrans.InsSubNum) " + where;
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listEtranses.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listEtranses[i].EtransNum, DbmLogFKeyType.Etrans,
                        DbmLogActionType.Update, methodName, "Updated PlanNum from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Mismatched etrans InsSubNum/PlanNum fixed: ") + numFixed + "\r\n";

                #endregion

                numFixed = 0;

                #region PayPlan

                //payplan--------------------------------------------------------------------------------------------------
                where = "WHERE EXISTS (SELECT PlanNum FROM inssub WHERE inssub.InsSubNum=payplan.InsSubNum AND inssub.PlanNum!=payplan.PlanNum)"
                        + PatientAndClauseHelper(patNumSpecific, "payplan");
                command = "SELECT * FROM payplan " + where;
                var listPayPlans = PayPlanCrud.SelectMany(command);
                command = "UPDATE payplan SET PlanNum=(SELECT PlanNum FROM inssub WHERE inssub.InsSubNum=payplan.InsSubNum) " + where;
                numFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listPayPlans.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listPayPlans[i].PayPlanNum, DbmLogFKeyType.PayPlan,
                        DbmLogActionType.Update, methodName, "Updated PlanNum from InsSubNumMismatchPlanNum.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Mismatched payplan InsSubNum/PlanNum fixed: ") + numFixed + "\r\n";

                #endregion

                DbmLogCrud.InsertMany(listDbmLogs);

                #endregion FIX

                break;
        }

        return log;
    }

    [DbmMethodAttr(HasPatNum = true)]
    public static string PaySplitWithInvalidPayNum(bool verbose, DbmMode dbmMode, long patNum = 0)
    {
        var log = "";
        //Get the unique PayNums for orphaned paysplits.
        var command = "SELECT paysplit.PayNum FROM paysplit WHERE NOT EXISTS(SELECT * FROM payment WHERE paysplit.PayNum=payment.PayNum) ";
        if (patNum > 0) command += $"AND paysplit.PatNum={SOut.Long(patNum)} ";
        command += "GROUP BY paysplit.PayNum";
        var listPayNums = Db.GetListLong(command);
        var countFound = 0;
        var listDataRows = new List<DataRow>();
        //Get the all of the data necessary to create dummy payments for ALL payment splits associated to the orphaned PayNums.
        if (!listPayNums.IsNullOrEmpty())
        {
            command = $@"SELECT PayNum,PatNum,DatePay,DateEntry,SUM(SplitAmt) SplitAmt_,COUNT(*) Count_
					FROM paysplit
					WHERE PayNum IN({string.Join(",", listPayNums.Select(x => SOut.Long(x)))})
					GROUP BY PayNum";
            var table = DataCore.GetTable(command);
            listDataRows = table.Select().ToList();
            //The query results were grouped by PayNum so sum the value in the Count_ column for all of the rows returned.
            countFound = (int) listDataRows.Sum(x => SIn.Long(x["Count_"].ToString()));
        }

        switch (dbmMode)
        {
            case DbmMode.Check:
                if (countFound > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Paysplits found with invalid PayNum:") + " " + countFound + "\r\n";
                break;
            case DbmMode.Fix:
                var listDbmLogs = new List<DbmLog>();
                var methodName = MethodBase.GetCurrentMethod().Name;
                var countFixed = 0;
                var hasPayNumZero = false;
                var listDefsPaymentTypes = Defs.GetDefsForCategory(DefCat.PaymentTypes, true);

                #region PayNum Not Zero

                if (listDataRows.Count > 0)
                    for (var i = 0; i < listDataRows.Count; i++)
                    {
                        var payNum = SIn.Long(listDataRows[i]["PayNum"].ToString());
                        if (payNum == 0)
                        {
                            hasPayNumZero = true;
                            continue;
                        }

                        //There's only one place in the program where this is called from.  Date is today, so no need to validate the date.
                        var payment = new Payment();
                        payment.PayType = listDefsPaymentTypes[0].DefNum;
                        payment.DateEntry = SIn.Date(listDataRows[i]["DateEntry"].ToString());
                        payment.PatNum = SIn.Long(listDataRows[i]["PatNum"].ToString());
                        payment.PayDate = SIn.Date(listDataRows[i]["DatePay"].ToString());
                        payment.PayAmt = SIn.Double(listDataRows[i]["SplitAmt_"].ToString());
                        payment.PayNote = "Dummy payment. Original payment entry missing from the database.";
                        payment.PayNum = payNum;
                        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                        payment.SecUserNumEntry = Security.CurUser.UserNum;
                        payment.PaymentSource = CreditCardSource.None;
                        payment.ProcessStatus = ProcessStat.OfficeProcessed;
                        payment.IsCcCompleted = true;
                        Payments.Insert(payment, true);
                        var dbmLog = new DbmLog(Security.CurUser.UserNum, payment.PayNum, DbmLogFKeyType.Payment,
                            DbmLogActionType.Insert, methodName, "Inserted payment from PaySplitWithInvalidPayNum.");
                        listDbmLogs.Add(dbmLog);
                        countFixed += SIn.Int(listDataRows[i]["Count_"].ToString());
                    }

                #endregion

                #region PayNum Zero

                if (hasPayNumZero)
                {
                    //Handling paysplits that have a pay num of 0 separately because we want to create one payment per patient per day
                    command = "SELECT PatNum,DatePay,DateEntry,SUM(SplitAmt) SplitAmt_,COUNT(*) Count_ FROM paysplit WHERE PayNum=0 ";
                    if (patNum > 0) command += $"AND PatNum={SOut.Long(patNum)} ";
                    command += "GROUP BY PatNum,DatePay";
                    var table = DataCore.GetTable(command);
                    for (var i = 0; i < table.Rows.Count; i++)
                    {
                        var payment = new Payment();
                        payment.PayType = listDefsPaymentTypes[0].DefNum;
                        payment.DateEntry = SIn.Date(table.Rows[i]["DateEntry"].ToString());
                        payment.PatNum = SIn.Long(table.Rows[i]["PatNum"].ToString());
                        payment.PayDate = SIn.Date(table.Rows[i]["DatePay"].ToString());
                        payment.PayAmt = SIn.Double(table.Rows[i]["SplitAmt_"].ToString());
                        payment.PayNote = "Dummy payment. Original payment entry number was 0.";
                        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
                        payment.SecUserNumEntry = Security.CurUser.UserNum;
                        payment.PaymentSource = CreditCardSource.None;
                        payment.ProcessStatus = ProcessStat.OfficeProcessed;
                        payment.IsCcCompleted = true;
                        Payments.Insert(payment);
                        var dbmLog = new DbmLog(Security.CurUser.UserNum, payment.PayNum, DbmLogFKeyType.Payment,
                            DbmLogActionType.Insert, methodName, "Inserted payment from PaySplitWithInvalidPayNum.");
                        listDbmLogs.Add(dbmLog);
                        command = "SELECT SplitNum FROM paysplit WHERE PayNum=0 AND PatNum=" + SOut.Long(payment.PatNum) + " AND DatePay=" + SOut.Date(payment.PayDate);
                        var listSplitNums = Db.GetListLong(command);
                        command = $@"UPDATE paysplit SET PayNum={SOut.Long(payment.PayNum)}
								WHERE SplitNum IN({string.Join(",", listSplitNums.Select(x => SOut.Long(x)))})";
                        Db.NonQ(command);
                        for (var j = 0; j < listSplitNums.Count; j++)
                        {
                            var dbmLog2 = new DbmLog(Security.CurUser.UserNum, listSplitNums[j], DbmLogFKeyType.PaySplit,
                                DbmLogActionType.Update, methodName, "Updated PayNum from 0 to " + payment.PayNum + ".");
                            listDbmLogs.Add(dbmLog2);
                        }

                        countFixed += listSplitNums.Count;
                    }
                }

                #endregion

                if (countFixed > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Paysplits found with invalid PayNum fixed:") + " " + countFixed + "\r\n";
                    DbmLogCrud.InsertMany(listDbmLogs);
                }

                break;
        }

        return log;
    }

    [DbmMethodAttr]
    public static string ProcButtonItemsDeleteWithInvalidAutoCode(bool verbose, DbmMode dbmMode)
    {
        var log = "";
        string command;
        switch (dbmMode)
        {
            case DbmMode.Check:
                command = @"SELECT COUNT(*) FROM procbuttonitem WHERE CodeNum=0 AND NOT EXISTS(
						SELECT * FROM autocode WHERE autocode.AutoCodeNum=procbuttonitem.AutoCodeNum)";
                var numFound = SIn.Int(Db.GetCount(command));
                if (numFound > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "ProcButtonItems found with invalid autocode: ") + numFound + "\r\n";
                break;
            case DbmMode.Fix:
                command = @"DELETE FROM procbuttonitem WHERE CodeNum=0 AND NOT EXISTS(
						SELECT * FROM autocode WHERE autocode.AutoCodeNum=procbuttonitem.AutoCodeNum)";
                var numberFixed = (int) Db.NonQ(command);
                if (numberFixed > 0) Signalods.SetInvalid(InvalidType.ProcButtons);
                if (numberFixed > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "ProcButtonItems deleted due to invalid autocode: ") + numberFixed + "\r\n";
                break;
        }

        return log;
    }
    
    [DbmMethodAttr(HasPatNum = true)]
    public static string ProcedurelogCodeNumInvalid(bool verbose, DbmMode dbmMode, long patNum = 0)
    {
        var log = "";
        var command = @"SELECT ProcNum FROM procedurelog WHERE NOT EXISTS(SELECT * FROM procedurecode WHERE procedurecode.CodeNum=procedurelog.CodeNum)";
        if (patNum > 0) command += " AND patNum=" + SOut.String(patNum.ToString()) + " ";
        switch (dbmMode)
        {
            case DbmMode.Check:
                var numFound = Db.GetListLong(command).Count;
                if (numFound > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Procedures found with invalid CodeNum") + ": " + numFound + "\r\n";
                break;
            case DbmMode.Fix:
                var listDbmLogs = new List<DbmLog>();
                var methodName = MethodBase.GetCurrentMethod().Name;
                var listProcNums = Db.GetListLong(command);
                long codeNumBad = 0;
                if (!ProcedureCodes.IsValidCode("~BAD~"))
                {
                    var procedureCode = new ProcedureCode();
                    procedureCode.ProcCode = "~BAD~";
                    procedureCode.Descript = "Invalid procedure";
                    procedureCode.AbbrDesc = "Invalid procedure";
                    procedureCode.ProcCat = Defs.GetByExactNameNeverZero(DefCat.ProcCodeCats, "Never Used");
                    ProcedureCodes.Insert(procedureCode);
                    codeNumBad = procedureCode.CodeNum;
                }
                else
                {
                    codeNumBad = ProcedureCodes.GetCodeNum("~BAD~");
                }

                command = "UPDATE procedurelog SET CodeNum=" + SOut.Long(codeNumBad) + " WHERE NOT EXISTS (SELECT * FROM procedurecode WHERE procedurecode.CodeNum=procedurelog.CodeNum)";
                if (patNum > 0) command += " AND patNum=" + SOut.String(patNum.ToString()) + " ";
                var numberFixed = (int) Db.NonQ(command);
                for (var i = 0; i < listProcNums.Count; i++)
                {
                    var dbmLog = new DbmLog(Security.CurUser.UserNum, listProcNums[i], DbmLogFKeyType.Procedure, DbmLogActionType.Update,
                        methodName, "Procedure fixed with invalid CodeNum from ProcedurelogCodeNumInvalid.");
                    listDbmLogs.Add(dbmLog);
                }

                if (numberFixed > 0 || verbose)
                {
                    DbmLogCrud.InsertMany(listDbmLogs);
                    log += Lans.g("FormDatabaseMaintenance", "Procedures fixed with invalid CodeNum") + ": " + numberFixed + "\r\n";
                }

                break;
        }

        return log;
    }

    [DbmMethodAttr(HasPatNum = true, IsPatDependent = true, HasBreakDown = true)]
    public static string ProcedurelogDeletedWithAttachedIncome(bool verbose, DbmMode dbmMode, long patNum)
    {
        var log = "";
        var family = Patients.GetFamily(patNum);
        var listClaimProcStatuses = ClaimProcs.GetInsPaidStatuses();
        var listProceduresDeleted = Procedures.GetAllForPatsAndStatuses(family.GetPatNums(), ProcStat.D); //Get all deleted procedures.
        var listProcNumsDeleted = listProceduresDeleted.Select(x => x.ProcNum).ToList();
        //Get all entities that are still attached to the deleted procedures.
        var listAdjustmentsDeleted = Adjustments.GetForProcs(listProcNumsDeleted);
        var listPaySplitsDeleted = PaySplits.GetForProcs(listProcNumsDeleted);
        var listClaimProcsDeleted = ClaimProcs.GetForProcs(listProcNumsDeleted).FindAll(x => listClaimProcStatuses.Contains(x.Status));
        var listPayPlanChargesDeleted = PayPlanCharges.GetForProcs(listProcNumsDeleted);
        var listPayPlanLinksDeleted = PayPlanLinks.GetForFKeysAndLinkType(listProcNumsDeleted, PayPlanLinkType.Procedure);
        //Get unique proc nums.
        var listProcNums = listAdjustmentsDeleted.Select(x => x.ProcNum).Distinct().ToList();
        listProcNums.AddRange(listPaySplitsDeleted.Select(x => x.ProcNum).Distinct().ToList());
        listProcNums.AddRange(listClaimProcsDeleted.Select(x => x.ProcNum).Distinct().ToList());
        listProcNums.AddRange(listPayPlanChargesDeleted.Select(x => x.ProcNum).Distinct().ToList());
        listProcNums.AddRange(listPayPlanLinksDeleted.Select(x => x.FKey).Distinct().ToList()); //We can use the FKey here since we only pulled procs.
        var listProcedures = listProceduresDeleted.FindAll(x => listProcNums.Contains(x.ProcNum)); //List of procedures with income.
        var numFound = listProcedures.Count();
        var listProcCodes = ProcedureCodes.GetCodesForCodeNums(listProcedures.Select(x => x.CodeNum).ToList());
        switch (dbmMode)
        {
            case DbmMode.Check:
            case DbmMode.Fix:
                if (numFound > 0 || verbose)
                {
                    log += Lans.g("FormDatabaseMaintenance", "Procedures of status 'Deleted' with attached income:") + " " + numFound + "\r\n";
                    log += "   " + Lans.g("FormDatabaseMaintenance", "Manual fix needed.  Double click to see a break down.") + "\r\n";
                }

                break;
            case DbmMode.Breakdown:
                if (numFound > 0 || verbose) log += Lans.g("FormDatabaseMaintenance", "Deleted procedures with attached income that need to be manually investigated:") + "\r\n";
                //Instead of grabbing these inside the loops, to speed things along do a bulk get for these before entering the nested loops.
                var listClaimsDeletedProcs = Claims.GetClaimsFromClaimNums(listClaimProcsDeleted.Select(x => x.ClaimNum).Distinct().ToList());
                var listPayPlanNums = listPayPlanChargesDeleted.Select(x => x.PayPlanNum).Distinct().ToList();
                listPayPlanNums.AddRange(listPayPlanLinksDeleted.Select(x => x.PayPlanNum).Distinct().ToList());
                var listPayPlansDeleted = PayPlans.GetMany(listPayPlanNums.ToArray());
                var listPaymentsDeleted = Payments.GetPayments(listPaySplitsDeleted.Select(x => x.PayNum).ToList());
                var listInsPlansDeleted = InsPlans.GetByInsSubs(listClaimProcsDeleted.Select(x => x.InsSubNum).ToList());
                var listInsSubsDeleted = InsSubs.GetMany(listClaimProcsDeleted.Select(x => x.InsSubNum).ToList());
                var stringBuilder = new StringBuilder();
                var listPatNumsDist = listProcedures.Select(x => x.PatNum).Distinct().ToList();
                for (var i = 0; i < listPatNumsDist.Count; i++)
                {
                    var patient = Patients.GetPat(listPatNumsDist[i]);
                    var listProceduresByPatNum = listProcedures.FindAll(x => x.PatNum == listPatNumsDist[i]);
                    stringBuilder.Append("PatNum #" + listPatNumsDist[i] + " - " + family.GetNameInFamFL(listPatNumsDist[i]) + " has "
                                         + listProceduresByPatNum.Count() + " deleted procedures:\r\n");
                    for (var j = 0; j < listProceduresByPatNum.Count(); j++)
                    {
                        //These are all the different attached income for the given procedure.
                        var listAdjustments = listAdjustmentsDeleted.FindAll(x => x.ProcNum == listProceduresByPatNum[j].ProcNum);
                        var listPaySplits = listPaySplitsDeleted.FindAll(x => x.ProcNum == listProceduresByPatNum[j].ProcNum);
                        var listClaimProcs = listClaimProcsDeleted.FindAll(x => x.ProcNum == listProceduresByPatNum[j].ProcNum);
                        var listPayPlanCharges = listPayPlanChargesDeleted.FindAll(x => x.ProcNum == listProceduresByPatNum[j].ProcNum);
                        var listPayPlanLinks = listPayPlanLinksDeleted.FindAll(x => x.FKey == listProceduresByPatNum[j].ProcNum);
                        stringBuilder.Append($" ProcNum #{listProceduresByPatNum[j].ProcNum} {listProceduresByPatNum[j].ProcDate.ToShortDateString()} - {Procedures.GetDescription(listProceduresByPatNum[j])}\r\n");
                        for (var k = 0; k < listAdjustments.Count; k++)
                        {
                            //Adjustments
                            var def = Defs.GetDef(DefCat.AdjTypes, listAdjustments[k].AdjType);
                            stringBuilder.Append($"		Adjustment #{listAdjustments[k].AdjNum} - Date: {listAdjustments[k].DateEntry.ToShortDateString()}\r\n");
                            stringBuilder.Append($"			Adjustment Date: {listAdjustments[k].AdjDate.ToShortDateString()}\r\n");
                            stringBuilder.Append($"			Procedure date: {listProceduresByPatNum[j].ProcDate.ToShortDateString()}\r\n");
                            stringBuilder.Append($"			Amount: {listAdjustments[k].AdjAmt:C}\r\n");
                            if (def != null) stringBuilder.Append($"			Adj Type: {def.ItemName}\r\n");
                        }

                        for (var k = 0; k < listPaySplits.Count; k++)
                        {
                            //PaySplits
                            var payment = listPaymentsDeleted.FirstOrDefault(x => x.PayNum == listPaySplits[k].PayNum); //Maybe use this? Since paysplits are hidden behind 2 extra forms
                            Def def = null;
                            stringBuilder.Append($"		PaySplit #{listPaySplits[k].SplitNum} - Date: {listPaySplits[k].DateEntry.ToShortDateString()}\r\n");
                            //Payment Info
                            if (payment != null)
                            {
                                stringBuilder.Append($"			Payment Date: {payment.PayDate.ToShortDateString()}\r\n");
                                stringBuilder.Append($"			Amount: {payment.PayAmt:C}\r\n");
                                def = Defs.GetDef(DefCat.PaymentTypes, payment.PayType);
                                if (def != null) stringBuilder.Append($"			Payment Type: {def.ItemName}\r\n");
                            }

                            //PaySplit Info
                            def = Defs.GetDef(DefCat.PaySplitUnearnedType, listPaySplits[k].UnearnedType);
                            stringBuilder.Append($"			Amount: {listPaySplits[k].SplitAmt:C}\r\n");
                            if (def != null) stringBuilder.Append($"			Unearned Type: {def.ItemName}\r\n");
                        }

                        for (var k = 0; k < listClaimProcs.Count; k++)
                        {
                            //ClaimProcs
                            var claim = listClaimsDeletedProcs.FirstOrDefault(x => x.ClaimNum == listClaimProcs[k].ClaimNum);
                            stringBuilder.Append($"		ClaimProc #{listClaimProcs[k].ClaimProcNum} - Date: {listClaimProcs[k].DateCP.ToShortDateString()}\r\n");
                            //Claim Info
                            if (claim != null)
                            {
                                stringBuilder.Append($"			Claim Status: {claim.ClaimStatus}\r\n");
                                stringBuilder.Append($"			Insurance Plan: {InsPlans.GetDescript(claim.PlanNum, family, listInsPlansDeleted, claim.InsSubNum, listInsSubsDeleted)}\r\n");
                                stringBuilder.Append($"			Date Orig Sent: {claim.DateSentOrig.ToShortDateString()}\r\n");
                            }

                            //ClaimProc Info
                            stringBuilder.Append($"			Status: {listClaimProcs[k].Status.GetDescription()}\r\n");
                            stringBuilder.Append($"			Pay Entry Date: {listClaimProcs[k].DateEntry.ToShortDateString()}\r\n");
                            stringBuilder.Append($"			Payment Date: {listClaimProcs[k].DateCP.ToShortDateString()}\r\n");
                            stringBuilder.Append($"			Procedure Date: {listClaimProcs[k].ProcDate.ToShortDateString()}\r\n");
                            stringBuilder.Append($"			Description: {Procedures.GetDescription(listProceduresByPatNum[j])}\r\n");
                            stringBuilder.Append($"			Billed to Ins: {listClaimProcs[k].FeeBilled:C}\r\n");
                            stringBuilder.Append($"			Ins Est Amount: {listClaimProcs[k].InsEstTotal:C}\r\n");
                            stringBuilder.Append($"			Ins Pay Amount: {listClaimProcs[k].InsPayAmt:C}\r\n");
                        }

                        for (var k = 0; k < listPayPlanCharges.Count; k++)
                        {
                            //PayPlanCharges
                            var payPlan = listPayPlansDeleted.FirstOrDefault(x => x.PayPlanNum == listPayPlanCharges[k].PayPlanNum);
                            //PayPlanCharge
                            stringBuilder.Append($"		PayPlanCharge #{listPayPlanCharges[k].PayPlanNum} - Date: {listPayPlanCharges[k].ChargeDate}\r\n");
                            if (payPlan != null)
                            {
                                stringBuilder.Append($"			Total Amount: {payPlan.CompletedAmt:C}\r\n");
                                stringBuilder.Append($"			Date of First payment: {payPlan.DatePayPlanStart.ToShortDateString()}\r\n");
                                stringBuilder.Append($"			APR: {payPlan.APR}\r\n");
                                stringBuilder.Append($"			Charge Frequency: {payPlan.ChargeFrequency.GetDescription()}\r\n");
                            }
                        }

                        for (var k = 0; k < listPayPlanLinks.Count; k++)
                        {
                            //PayPlanLinks
                            var payPlan = listPayPlansDeleted.FirstOrDefault(x => x.PayPlanNum == listPayPlanLinks[k].PayPlanNum);
                            stringBuilder.Append($"		PayPlanLink #{listPayPlanLinks[k].PayPlanLinkNum} - Date: {listPayPlanLinks[k].SecDateTEntry.ToShortDateString()}\r\n");
                            //PayPlan Info
                            if (payPlan != null)
                            {
                                stringBuilder.Append($"			Total Amount: {payPlan.CompletedAmt:C}\r\n");
                                stringBuilder.Append($"			Date of First payment: {payPlan.DatePayPlanStart.ToShortDateString()}\r\n");
                                stringBuilder.Append($"			APR: {payPlan.APR}\r\n");
                                stringBuilder.Append($"			Charge Frequency: {payPlan.ChargeFrequency.GetDescription()}\r\n");
                                stringBuilder.Append($"			Treatment Planned Mode: {payPlan.DynamicPayPlanTPOption.GetDescription()}\r\n");
                            }

                            //PayPlanLink Info
                            stringBuilder.Append($"			Amount Override: {listPayPlanLinks[k].AmountOverride:C}\r\n");
                            stringBuilder.Append($"			Type: {listPayPlanLinks[k].LinkType.GetDescription(true)}\r\n");
                        }

                        stringBuilder.Append("\r\n");
                    }
                }

                log += stringBuilder.ToString();
                break;
        }

        return log;
    }

    public static List<string> GetDatabaseNames()
    {
        var listRetVals = new List<string>();
        var command = "SHOW DATABASES";
        //if this next step fails, table will simply have 0 rows
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listRetVals.Add(table.Rows[i][0].ToString());
        return listRetVals;
    }

    public static string GetDuplicateClaimProcs()
    {
        var retVal = "";
        var command = @"SELECT LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber, COUNT(*) cnt
FROM claimproc
LEFT JOIN patient ON patient.PatNum=claimproc.PatNum
WHERE ClaimNum > 0
AND ProcNum>0
AND Status!=4/*exclude supplemental*/
GROUP BY LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber 
HAVING cnt>1";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "";
        retVal += "Duplicate claim payments found:\r\n";
        DateTime date;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0) //check for duplicate rows.  We only want to report each claim once.
                if (table.Rows[i]["ClaimNum"].ToString() == table.Rows[i - 1]["ClaimNum"].ToString())
                    continue;

            date = SIn.Date(table.Rows[i]["ProcDate"].ToString());
            retVal += table.Rows[i]["LName"] + ", "
                                             + table.Rows[i]["FName"] + " "
                                             + "(" + table.Rows[i]["PatNum"] + "), "
                                             + date.ToShortDateString() + "\r\n";
        }

        retVal += "\r\n";
        return retVal;
    }

    public static string GetDuplicateSupplementalPayments()
    {
        var retVal = "";
        var command = @"SELECT LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber, COUNT(*) cnt
FROM claimproc
LEFT JOIN patient ON patient.PatNum=claimproc.PatNum
WHERE ClaimNum > 0
AND ProcNum>0
AND Status=4/*only supplemental*/
GROUP BY LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber
HAVING cnt>1";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return "";
        retVal += "Duplicate supplemental payments found (may be false positives):\r\n";
        DateTime date;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0)
                if (table.Rows[i]["ClaimNum"].ToString() == table.Rows[i - 1]["ClaimNum"].ToString())
                    continue;

            date = SIn.Date(table.Rows[i]["ProcDate"].ToString());
            retVal += table.Rows[i]["LName"] + ", "
                                             + table.Rows[i]["FName"] + " "
                                             + "(" + table.Rows[i]["PatNum"] + "), "
                                             + date.ToShortDateString() + "\r\n";
        }

        retVal += "\r\n";
        return retVal;
    }

    public static string GetMissingClaimProcs(string dbOld)
    {
        var retVal = "";
        var command = "SELECT LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber "
                      + "FROM " + dbOld + ".claimproc "
                      + "LEFT JOIN " + dbOld + ".patient ON " + dbOld + ".patient.PatNum=" + dbOld + ".claimproc.PatNum "
                      + "WHERE NOT EXISTS(SELECT * FROM claimproc WHERE claimproc.ClaimProcNum=" + dbOld + ".claimproc.ClaimProcNum) "
                      + "AND ClaimNum > 0 AND ProcNum>0";
        var table = DataCore.GetTable(command);
        double insPayAmt;
        double feeBilled;
        var count = 0;
        for (var i = 0; i < table.Rows.Count; i++)
        {
            insPayAmt = SIn.Double(table.Rows[i]["InsPayAmt"].ToString());
            feeBilled = SIn.Double(table.Rows[i]["FeeBilled"].ToString());
            command = "SELECT COUNT(*) FROM " + dbOld + ".claimproc "
                      + "WHERE ClaimNum= " + table.Rows[i]["ClaimNum"] + " "
                      + "AND ProcNum= " + table.Rows[i]["ProcNum"] + " "
                      + "AND Status= " + table.Rows[i]["Status"] + " "
                      + "AND InsPayAmt= '" + SOut.Double(insPayAmt) + "' "
                      + "AND FeeBilled= '" + SOut.Double(feeBilled) + "' "
                      + "AND LineNumber= " + table.Rows[i]["LineNumber"];
            var result = Db.GetCount(command);
            if (result != "1") //only include in result if there are duplicates in old db.
                count++;
        }

        command = "SELECT ClaimPaymentNum "
                  + "FROM " + dbOld + ".claimpayment "
                  + "WHERE NOT EXISTS(SELECT * FROM claimpayment WHERE claimpayment.ClaimPaymentNum=" + dbOld + ".claimpayment.ClaimPaymentNum) ";
        var table2 = DataCore.GetTable(command);
        if (count == 0 && table2.Rows.Count == 0) return "";
        retVal += "Missing claim payments found: " + count + "\r\n";
        retVal += "Missing claim checks found (probably false positives): " + table2.Rows.Count + "\r\n";
        return retVal;
    }

    public static string FixClaimProcDeleteDuplicates()
    {
        var log = "";
        //command=@"SELECT LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber, COUNT(*) cnt
        //	FROM claimproc
        //	LEFT JOIN patient ON patient.PatNum=claimproc.PatNum
        //	WHERE ClaimNum > 0
        //	AND ProcNum>0
        //	AND Status!=4/*exclude supplemental*/
        //	GROUP BY ClaimNum,ProcNum,Status,InsPayAmt,FeeBilled,LineNumber
        //	HAVING cnt>1";
        //table=DataCore.GetTable(command);
        //long numberFixed=0;
        //double insPayAmt;
        //double feeBilled;
        //for(int i=0;i<table.Rows.Count;i++) {
        //  insPayAmt=PIn.Double(table.Rows[i]["InsPayAmt"].ToString());
        //  feeBilled=PIn.Double(table.Rows[i]["FeeBilled"].ToString());
        //  command="DELETE FROM claimproc "
        //    +"WHERE ClaimNum= "+table.Rows[i]["ClaimNum"].ToString()+" "
        //    +"AND ProcNum= "+table.Rows[i]["ProcNum"].ToString()+" "
        //    +"AND Status= "+table.Rows[i]["Status"].ToString()+" "
        //    +"AND InsPayAmt= '"+POut.Double(insPayAmt)+"' "
        //    +"AND FeeBilled= '"+POut.Double(feeBilled)+"' "
        //    +"AND LineNumber= "+table.Rows[i]["LineNumber"].ToString()+" "
        //    +"AND ClaimProcNum != "+table.Rows[i]["ClaimProcNum"].ToString();
        //  numberFixed+=Db.NonQ(command);
        //}
        //log+="Claimprocs deleted due duplicate entries: "+numberFixed.ToString()+".\r\n";
        return log;
    }
    
    public static string FixMissingClaimProcs()
    {
        var log = "";
        //command="SELECT LName,FName,patient.PatNum,ClaimNum,FeeBilled,Status,ProcNum,ProcDate,ClaimProcNum,InsPayAmt,LineNumber "
        //  +"FROM "+olddb+".claimproc "
        //  +"LEFT JOIN "+olddb+".patient ON "+olddb+".patient.PatNum="+olddb+".claimproc.PatNum "
        //  +"WHERE NOT EXISTS(SELECT * FROM claimproc WHERE claimproc.ClaimProcNum="+olddb+".claimproc.ClaimProcNum) "
        //  +"AND ClaimNum > 0 AND ProcNum>0";
        //table=DataCore.GetTable(command);
        //long numberFixed=0;
        //command="SELECT ValueString FROM "+olddb+".preference WHERE PrefName='DataBaseVersion'";
        //string oldVersString=DataCore.GetScalar(command);
        //Version oldVersion=new Version(oldVersString);
        //if(oldVersion < new Version("6.7.1.0")) {
        //  return "Version of old database is too old to use with the automated tool: "+oldVersString;
        //}
        //double insPayAmt;
        //double feeBilled;
        //for(int i=0;i<table.Rows.Count;i++) {
        //  insPayAmt=PIn.Double(table.Rows[i]["InsPayAmt"].ToString());
        //  feeBilled=PIn.Double(table.Rows[i]["FeeBilled"].ToString());
        //  command="SELECT COUNT(*) FROM "+olddb+".claimproc "
        //    +"WHERE ClaimNum= "+table.Rows[i]["ClaimNum"].ToString()+" "
        //    +"AND ProcNum= "+table.Rows[i]["ProcNum"].ToString()+" "
        //    +"AND Status= "+table.Rows[i]["Status"].ToString()+" "
        //    +"AND InsPayAmt= '"+POut.Double(insPayAmt)+"' "
        //    +"AND FeeBilled= '"+POut.Double(feeBilled)+"' "
        //    +"AND LineNumber= "+table.Rows[i]["LineNumber"].ToString();
        //  string result=Db.GetCount(command);
        //  if(result=="1") {//only include in result if there are duplicates in old db.
        //    continue;
        //  }
        //  command="INSERT INTO claimproc SELECT *";
        //  if(oldVersion < new Version("6.8.1.0")) {
        //    command+=",-1,-1,0";
        //  }
        //  else if(oldVersion < new Version("6.9.1.0")) {
        //    command+=",0";
        //  }
        //  command+=" FROM "+olddb+".claimproc "
        //    +"WHERE "+olddb+".claimproc.ClaimProcNum="+table.Rows[i]["ClaimProcNum"].ToString();
        //  numberFixed+=Db.NonQ(command);
        //}
        //command="SELECT ClaimPaymentNum "
        //  +"FROM "+olddb+".claimpayment "
        //  +"WHERE NOT EXISTS(SELECT * FROM claimpayment WHERE claimpayment.ClaimPaymentNum="+olddb+".claimpayment.ClaimPaymentNum) ";
        //table=DataCore.GetTable(command);
        //long numberFixed2=0;
        //for(int i=0;i<table.Rows.Count;i++) {
        //  command="INSERT INTO claimpayment SELECT * FROM "+olddb+".claimpayment "
        //    +"WHERE "+olddb+".claimpayment.ClaimPaymentNum="+table.Rows[i]["ClaimPaymentNum"].ToString();
        //  numberFixed2+=Db.NonQ(command);
        //}
        //log+="Missing claimprocs added back: "+numberFixed.ToString()+".\r\n";
        //log+="Missing claimpayments added back: "+numberFixed2.ToString()+".\r\n";
        return log;
    }

    private static string PatientAndClauseHelper(long patNum, string tableName)
    {
        //Not running patient specific DBM or a table wasn't specified.
        if (patNum < 1 || string.IsNullOrWhiteSpace(tableName)) return "";
        return " AND " + tableName + ".PatNum=" + SOut.Long(patNum) + " ";
    }

    public static DataTable GetRedundantIndexesTable()
    {
        var dbName = MiscData.GetCurrentDatabase();
        var command = $@"SELECT table1.TABLE_NAME,
				REPLACE(
					CASE WHEN table1.COLS=table2.COLS AND table1.NON_UNIQUE=table2.NON_UNIQUE
						THEN
							CASE WHEN INSTR(REPLACE(table2.INDEX_NAME,'`',''),REPLACE(table1.INDEX_NAME,'`',''))=1
								THEN table2.INDEX_NAME
							WHEN INSTR(REPLACE(table1.INDEX_NAME,'`',''),REPLACE(table2.INDEX_NAME,'`',''))=1
								THEN table1.INDEX_NAME
							ELSE GREATEST(table1.INDEX_NAME,table2.INDEX_NAME)
							END
					WHEN LENGTH(table1.COLS)-LENGTH(REPLACE(table1.COLS,',',''))>LENGTH(table2.COLS)-LENGTH(REPLACE(table2.COLS,',',''))
						THEN table2.INDEX_NAME
					ELSE table1.INDEX_NAME
					END
				,'`','') INDEX_NAME,
				REPLACE(
					CASE WHEN table1.COLS=table2.COLS AND table1.NON_UNIQUE=table2.NON_UNIQUE
						THEN
							CASE WHEN INSTR(REPLACE(table2.INDEX_NAME,'`',''),REPLACE(table1.INDEX_NAME,'`',''))=1
								THEN CONCAT(table2.COLS,IFNULL(CONCAT('(',table2.SUB_PART,')'),''))
							WHEN INSTR(REPLACE(table1.INDEX_NAME,'`',''),REPLACE(table2.INDEX_NAME,'`',''))=1
								THEN CONCAT(table1.COLS,IFNULL(CONCAT('(',table1.SUB_PART,')'),''))
							ELSE
								CASE WHEN table1.INDEX_NAME>table2.INDEX_NAME
									THEN CONCAT(table1.COLS,IFNULL(CONCAT('(',table1.SUB_PART,')'),''))
								ELSE CONCAT(table2.COLS,IFNULL(CONCAT('(',table2.SUB_PART,')'),''))
								END
							END
					WHEN LENGTH(table1.COLS)-LENGTH(REPLACE(table1.COLS,',',''))>LENGTH(table2.COLS)-LENGTH(REPLACE(table2.COLS,',',''))
						THEN CONCAT(table2.COLS,IFNULL(CONCAT('(',table2.SUB_PART,')'),''))
					ELSE CONCAT(table1.COLS,IFNULL(CONCAT('(',table1.SUB_PART,')'),''))
					END
				,'`','') INDEX_COLS,
				REPLACE(
					GROUP_CONCAT(
						DISTINCT CASE WHEN table1.COLS=table2.COLS AND table1.NON_UNIQUE=table2.NON_UNIQUE
							THEN
								CASE WHEN INSTR(REPLACE(table2.INDEX_NAME,'`',''),REPLACE(table1.INDEX_NAME,'`',''))=1
									THEN CONCAT(table1.INDEX_NAME,' (',table1.COLS,')')
								WHEN INSTR(REPLACE(table1.INDEX_NAME,'`',''),REPLACE(table2.INDEX_NAME,'`',''))=1
									THEN CONCAT(table2.INDEX_NAME,' (',table2.COLS,')')
								ELSE
									CASE WHEN table1.INDEX_NAME<table2.INDEX_NAME
										THEN CONCAT(table1.INDEX_NAME,' (',table1.COLS,')')
									ELSE CONCAT(table2.INDEX_NAME,' (',table2.COLS,')')
									END
								END
						WHEN LENGTH(table1.COLS)-LENGTH(REPLACE(table1.COLS,',',''))>LENGTH(table2.COLS)-LENGTH(REPLACE(table2.COLS,',',''))
							THEN CONCAT(table1.INDEX_NAME,' (',table1.COLS,')')
						ELSE CONCAT(table2.INDEX_NAME,' (',table2.COLS,')')
						END
						SEPARATOR '\r\n'
					)
				,'`','') REDUNDANT_OF,
				CASE WHEN table1.COLS=table2.COLS AND table1.NON_UNIQUE=table2.NON_UNIQUE
					THEN
						CASE WHEN INSTR(REPLACE(table2.INDEX_NAME,'`',''),REPLACE(table1.INDEX_NAME,'`',''))=1
							THEN table2.ENGINE
						WHEN INSTR(REPLACE(table1.INDEX_NAME,'`',''),REPLACE(table2.INDEX_NAME,'`',''))=1
							THEN table1.ENGINE
						ELSE
							CASE WHEN table1.INDEX_NAME>table2.INDEX_NAME
								THEN table1.ENGINE
							ELSE table2.ENGINE
							END
						END
				WHEN LENGTH(table1.COLS)-LENGTH(REPLACE(table1.COLS,',',''))>LENGTH(table2.COLS)-LENGTH(REPLACE(table2.COLS,',',''))
					THEN table2.ENGINE
				ELSE table1.ENGINE
				END `ENGINE`
				FROM (
					SELECT s.TABLE_NAME,CONCAT('`',s.INDEX_NAME,'`') AS INDEX_NAME,s.INDEX_TYPE,s.NON_UNIQUE,s.SUB_PART,t.ENGINE,
					GROUP_CONCAT(CONCAT('`',s.COLUMN_NAME,'`') ORDER BY IF(s.INDEX_TYPE='BTREE',s.SEQ_IN_INDEX,0),s.COLUMN_NAME) COLS
					FROM information_schema.STATISTICS s
					INNER JOIN information_schema.TABLES t ON t.TABLE_SCHEMA=s.TABLE_SCHEMA
						AND t.TABLE_NAME=s.TABLE_NAME
					WHERE s.TABLE_SCHEMA='{SOut.String(dbName)}'
					GROUP BY s.TABLE_NAME,s.INDEX_NAME,s.INDEX_TYPE,s.NON_UNIQUE
				) table1
				INNER JOIN (
					SELECT s.TABLE_NAME,CONCAT('`',s.INDEX_NAME,'`') AS INDEX_NAME,s.INDEX_TYPE,s.NON_UNIQUE,s.SUB_PART,t.ENGINE,
					GROUP_CONCAT(CONCAT('`',s.COLUMN_NAME,'`') ORDER BY IF(s.INDEX_TYPE='BTREE',s.SEQ_IN_INDEX,0),s.COLUMN_NAME) COLS
					FROM information_schema.STATISTICS s
					INNER JOIN information_schema.TABLES t ON t.TABLE_SCHEMA=s.TABLE_SCHEMA
						AND t.TABLE_NAME=s.TABLE_NAME
					WHERE s.TABLE_SCHEMA='{SOut.String(dbName)}'
					GROUP BY s.TABLE_NAME,s.INDEX_NAME,s.INDEX_TYPE,s.NON_UNIQUE
				) AS table2
				WHERE table2.TABLE_NAME=table1.TABLE_NAME
				AND table2.INDEX_NAME!=table1.INDEX_NAME
				AND table2.INDEX_TYPE=table1.INDEX_TYPE
				AND (
					(
						table2.COLS=table1.COLS
						AND (
							table1.NON_UNIQUE
							OR table1.NON_UNIQUE=table2.NON_UNIQUE
						)
					)
					OR (
						table1.INDEX_TYPE='BTREE'
						AND INSTR(table2.COLS,table1.COLS)=1
						AND table1.NON_UNIQUE
					)
				)
				GROUP BY table1.TABLE_NAME,INDEX_NAME";
        return DataCore.GetTable(command);
    }

    public static string DropRedundantIndexes(List<DataRow> listDataRows)
    {
        //Incoming DataRows have these columns: "TABLE_NAME","INDEX_NAME","INDEX_COLS","REDUNDANT_OF"
        var hasInnoDbFilePerTable = false;
        using (var table = DataCore.GetTable("SHOW GLOBAL VARIABLES LIKE 'INNODB_FILE_PER_TABLE'"))
        {
            if (table.Rows.Count > 0 && table.Columns.Count > 1) hasInnoDbFilePerTable = SIn.Bool(table.Rows[0][1].ToString());
        }

        var stringBuilderLog = new StringBuilder();
        var dbName = MiscData.GetCurrentDatabase();
        DataConnection.CommandTimeout = 43200; //12 hours, just in case
        var listTableNamesDist = listDataRows.Select(x => x["TABLE_NAME"].ToString()).Distinct().ToList();
        for (var i = 0; i < listTableNamesDist.Count; i++)
        {
            var fullTableName = "`" + SOut.String(dbName) + "`.`" + SOut.String(listTableNamesDist[i]) + "`";
            var listDataRowsMatchTableName = listDataRows.FindAll(x => x["TABLE_NAME"].ToString() == listTableNamesDist[i]);
            if (listDataRowsMatchTableName.Count == 0) continue;
            stringBuilderLog.AppendLine("ALTER TABLE " + fullTableName + " " + string.Join(", ",
                listDataRowsMatchTableName.Select(x => "ADD INDEX " + SOut.String(x["INDEX_NAME"].ToString()) + " (" + SOut.String(x["INDEX_COLS"].ToString()) + ")")) + ";");
            var command = "ALTER TABLE " + fullTableName + " " + string.Join(", ", listDataRowsMatchTableName.Select(x => "DROP INDEX " + SOut.String(x["INDEX_NAME"].ToString()))) + ";";
            //The ENGINE column should be the same for all rows in the list, since it's the table's storage engine. Using .Exists just in case.
            var doOptimize = listDataRowsMatchTableName.Exists(x => x["ENGINE"].ToString().ToLower() == "innodb") && hasInnoDbFilePerTable;
            var optimize = "";
            if (doOptimize)
            {
                //For InnoDb tables with innodb_file_per_table set, optimize table to reclaim hard drive space and reduce .ibd file size
                command += @"
						OPTIMIZE TABLE " + fullTableName + ";";
                optimize = Lans.g("DatabaseMaintenance", "and optimizing") + " ";
            }

            ODEvent.Fire(ODEventType.ProgressBar, Lans.g("DatabaseMaintenance", "Dropping redundant indexes") + " "
                                                                                                              + optimize + Lans.g("DatabaseMaintenance", "table") + " "
                                                                                                              + fullTableName.Replace("`", "") + ".");
            Db.NonQ(command);
        }

        DataConnection.CommandTimeout = 3600; //set back to 1 hour default
        return stringBuilderLog.ToString();
    }
}