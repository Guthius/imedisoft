using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class InsVerifies
{
    public static InsVerify GetOneByFKey(long fkey, VerifyTypes verifyType)
    {
        //In some cases, insverify can have more than one row per plan. Using ORDER BY and LIMIT to ensure we get latest DateLastVerified if there are multiple rows for one plan (JobNum:53236)
        var command = "SELECT * FROM insverify WHERE FKey=" + fkey + " AND VerifyType=" + SOut.Int((int) verifyType) + " ORDER BY DateLastVerified DESC LIMIT 1";
        return InsVerifyCrud.SelectOne(command);
    }

    public static void Upsert(long fKey, VerifyTypes verifyType)
    {
        var insVerifyExists = GetOneByFKey(fKey, verifyType);
        if (insVerifyExists == null)
        {
            if (verifyType == VerifyTypes.InsuranceBenefit)
            {
                InsertForPlanNum(fKey);
                return;
            }

            InsertForPatPlanNum(fKey);
            return;
        }

        var insVerify = new InsVerify();
        insVerify.InsVerifyNum = insVerifyExists.InsVerifyNum;
        insVerify.VerifyType = verifyType;
        insVerify.FKey = fKey;
        InsVerifyCrud.Update(insVerify, insVerifyExists);
    }
    
    public static void Update(InsVerify insVerify)
    {
        InsVerifyCrud.Update(insVerify);
    }

    public static void InsertForPatPlanNum(long patPlanNum)
    {
        var insVerify = new InsVerify();
        insVerify.VerifyType = VerifyTypes.PatientEnrollment;
        insVerify.FKey = patPlanNum;
        InsVerifyCrud.Insert(insVerify);
    }

    public static void InsertForPlanNum(long planNum)
    {
        var insVerify = new InsVerify();
        insVerify.VerifyType = VerifyTypes.InsuranceBenefit;
        insVerify.FKey = planNum;
        InsVerifyCrud.Insert(insVerify);
    }

    public static void DeleteByFKey(long fkey, VerifyTypes verifyType)
    {
        var command = "DELETE FROM insverify WHERE FKey=" + fkey + " AND VerifyType=" + SOut.Int((int) verifyType);
        Db.NonQ(command);
    }

    public static List<long> GetAllInsVerifyUserNums()
    {
        var command = "SELECT DISTINCT UserNum FROM insverify";
        return Db.GetListLong(command);
    }

    public static List<InsVerifyGridObject> GetVerifyGridList(DateTime dateStartStandard, DateTime dateEndStandard, DateTime datePatEligibilityLastVerifiedStandard, DateTime datePlanBenefitsLastVerifiedStandard, List<long> listClinicNums, List<long> listRegionDefNums, long statusDefNum, long userNum, string carrierName, bool excludePatVerifyWhenNoIns, bool excludePatClones, DateTime dateStartMedicaid, DateTime dateEndMedicaid, DateTime datePatEligibilityLastVerifiedMedicaid, DateTime datePlanBenefitsLastVerifiedMedicaid, InsVerifyListType insVerifyListType)
    {
        var listInsFilingCodeNums = InsFilingCodes.GetAll().Select(x => x.InsFilingCodeNum).ToList();
        var listInsVerifyMedicaidFilingCodes = PrefC.GetString(PrefName.InsVerifyMedicaidFilingCodes).Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        //Construct two lists of InsFilingCodeNums; one for Medicaid CodeNums (contained in the InsVerifyMedicaidFilingCodes pref), and one for the rest of the CodeNums.
        var listInsFilingCodeNumsMedicaid = listInsVerifyMedicaidFilingCodes.Select(x => SIn.Long(x, false)).ToList();
        var listInsFilingCodeNumsStandard = listInsFilingCodeNums.FindAll(x => !listInsFilingCodeNumsMedicaid.Contains(x));
        listInsFilingCodeNumsStandard.Add(0); //Add 0 to include InsPlans with no filing code.
        var listInsVerifyGridObjects = new List<InsVerifyGridObject>();
        //If our list type is Both or Standard, and we have a populated list of Standard CodeNums, grab all of the relevant InsVerifyGridObjects using the db.
        if ((insVerifyListType == InsVerifyListType.Both || insVerifyListType == InsVerifyListType.Standard) && listInsFilingCodeNumsStandard.Count > 0)
        {
            //The list will always be populated.
            var command = GetVerifyGridListQuery(dateStartStandard, dateEndStandard, datePatEligibilityLastVerifiedStandard, datePlanBenefitsLastVerifiedStandard,
                listClinicNums, listRegionDefNums, statusDefNum, userNum, carrierName, excludePatVerifyWhenNoIns, excludePatClones, listInsFilingCodeNumsStandard);
            var table = DataCore.GetTable(command);
            var listInsVerifyGridObjectsStandard = TableToListInsVerifyGridObjects(table, false);
            listInsVerifyGridObjects.AddRange(listInsVerifyGridObjectsStandard);
        }

        //If our list type is Both or Medicaid, and we have a populated list of Medicaid CodeNums, grab all of the relevant InsVerifyGridObjects using the db.
        if ((insVerifyListType == InsVerifyListType.Both || insVerifyListType == InsVerifyListType.Medicaid) && listInsFilingCodeNumsMedicaid.Count > 0)
        {
            var command = GetVerifyGridListQuery(dateStartMedicaid, dateEndMedicaid, datePatEligibilityLastVerifiedMedicaid, datePlanBenefitsLastVerifiedMedicaid,
                listClinicNums, listRegionDefNums, statusDefNum, userNum, carrierName, excludePatVerifyWhenNoIns, excludePatClones, listInsFilingCodeNumsMedicaid);
            var table = DataCore.GetTable(command);
            var listInsVerifyGridObjectsMedicaid = TableToListInsVerifyGridObjects(table, true);
            listInsVerifyGridObjects.AddRange(listInsVerifyGridObjectsMedicaid);
        }

        //Return all of the relevant InsVerifyGridObjects we collected.
        return listInsVerifyGridObjects;
    }

    private static string GetVerifyGridListQuery(DateTime dateStart, DateTime dateEnd, DateTime datePatEligibilityLastVerified, DateTime datePlanBenefitsLastVerified, List<long> listClinicNums, List<long> listRegionDefNums, long defNumStatus, long userNum, string carrierName, bool excludePatVerifyWhenNoIns, bool excludePatClones, List<long> listInsFilingCodeNums)
    {
        //clinicJoin should only be used if the passed in clinicNum is a value other than 0 (Unassigned).
        var whereClinic = "";
        if (listClinicNums.Contains(-1))
        {
            //All clinics
            whereClinic = "AND (clinic.IsInsVerifyExcluded=0 OR clinic.ClinicNum IS NULL) ";
            if (!listRegionDefNums.Contains(0) && !listRegionDefNums.Contains(-1) && listRegionDefNums.Count > 0) //Specific region
                whereClinic += " AND clinic.Region IN(" + string.Join(",", listRegionDefNums.Select(x => x)) + ") ";
        }
        else if (listClinicNums.Contains(0))
        {
            //Unassigned clinics
            whereClinic = "AND clinic.ClinicNum IS NULL ";
            if (listClinicNums.Count(x => x != 0) > 0)
            {
                //Also has specific clinics selected
                whereClinic = "AND (clinic.ClinicNum IS NULL OR ";
                whereClinic += "(clinic.IsInsVerifyExcluded=0 AND clinic.ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => x)) + ") ";
                if (!listRegionDefNums.Contains(0) && !listRegionDefNums.Contains(-1) && listRegionDefNums.Count > 0) //Specific region
                    whereClinic += " AND clinic.Region IN(" + string.Join(",", listRegionDefNums.Select(x => x)) + ") ";
                whereClinic += ")) ";
            }
        }
        else if (listClinicNums.Count > 0)
        {
            //Specific Clinic
            whereClinic = "AND clinic.IsInsVerifyExcluded=0 AND clinic.ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => x)) + ") ";
            if (!listRegionDefNums.Contains(0) && !listRegionDefNums.Contains(-1) && listRegionDefNums.Count > 0) //Specific region
                whereClinic += " AND clinic.Region IN(" + string.Join(",", listRegionDefNums.Select(x => x)) + ") ";
        }

        var checkBenefitYear = PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear);
        var checkPatEnrollmentYear = PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear);
        var checkBenefitAndPatEnrollmentYearOn = checkBenefitYear && checkPatEnrollmentYear;
        var checkBenefitAndPatEnrollmentYearOff = !checkBenefitYear && !checkPatEnrollmentYear;
        var mainQuery = @"
				SELECT insverify.*,
				patient.LName,patient.FName,patient.Preferred,appointment.PatNum,appointment.AptNum,appointment.AptDateTime,patplan.PatPlanNum,insplan.PlanNum,carrier.CarrierName,
				COALESCE(clinic.Abbr,'None') AS ClinicName,appointment.ClinicNum,inssub.InsSubNum,carrier.CarrierNum
				FROM appointment 
				LEFT JOIN clinic ON clinic.ClinicNum=appointment.ClinicNum 
				INNER JOIN patient ON patient.PatNum=appointment.PatNum 
				INNER JOIN patplan ON patplan.PatNum=appointment.PatNum 
				INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum 
				INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum 
					" + (excludePatVerifyWhenNoIns ? "AND insplan.HideFromVerifyList=0" : "") + @"
					AND insplan.FilingCode IN(" + SOut.String(string.Join(",", listInsFilingCodeNums)) + @")
				INNER JOIN carrier ON carrier.CarrierNum=insplan.CarrierNum 
					" + (string.IsNullOrEmpty(carrierName) ? "" : "AND carrier.CarrierName LIKE '%" + SOut.String(carrierName) + "%'") + @"
				" + (excludePatClones
            ? "LEFT JOIN patientlink ON patientlink.PatNumTo=patient.PatNum AND patientlink.LinkType="
              + SOut.Int((int) PatientLinkType.Clone) + " "
            : "");
        var insVerifyJoin1 = @"INNER JOIN insverify ON 
					(insverify.VerifyType=" + SOut.Int((int) VerifyTypes.InsuranceBenefit) + @" 
					AND insverify.FKey=insplan.PlanNum 
					AND (insverify.DateLastVerified<" + SOut.Date(datePlanBenefitsLastVerified) + @"
						" + (checkBenefitYear
            ? @"OR (insverify.DateLastVerified<DATE_FORMAT(appointment.AptDateTime,CONCAT('%Y-',LPAD(insplan.MonthRenew,2,'0'),'-01')) 
							AND DATE_FORMAT(appointment.AptDateTime,CONCAT('%Y-',LPAD(MonthRenew,2,'0'),'-01'))<=DATE(appointment.AptDateTime))"
            : "") + @") 
					" + (excludePatVerifyWhenNoIns ? "" : "AND insplan.HideFromVerifyList=0") + @") ";
        var insVerifyJoin2 = @"INNER JOIN insverify ON 
					(insverify.VerifyType=" + SOut.Int((int) VerifyTypes.PatientEnrollment) + @"
					AND insverify.FKey=patplan.PatPlanNum
					AND (insverify.DateLastVerified<" + SOut.Date(datePatEligibilityLastVerified) + @"
						" + (checkPatEnrollmentYear
            ? @"OR (insverify.DateLastVerified<DATE_FORMAT(appointment.AptDateTime,CONCAT('%Y-',LPAD(MonthRenew,2,'0'),'-01')) 
							AND DATE_FORMAT(appointment.AptDateTime,CONCAT('%Y-',LPAD(MonthRenew,2,'0'),'-01'))<=DATE(appointment.AptDateTime))"
            : "") + @"))	";
        var whereClause = @"
				WHERE appointment.AptDateTime BETWEEN DATE(" + SOut.Date(dateStart) + ") AND DATE(" + SOut.Date(dateEnd.AddDays(1)) + @") 
				AND appointment.AptStatus IN (" + SOut.Int((int) ApptStatus.Scheduled) + "," + SOut.Int((int) ApptStatus.Complete) + @")
				" + (userNum == -1 ? "" : "AND insverify.UserNum=" + userNum) + @"
				" + (defNumStatus < 1 ? "" : "AND insverify.DefNum=" + defNumStatus) + @"
				" + (excludePatClones ? "AND patientlink.PatNumTo IS NULL" : "") + @"
				" + whereClinic;
        //Previously we joined the insverify table using a large OR clause. This caused MySQL to not be able to use any index on the insverify table.
        //Now we run two unioned queries, each with a different join clause for the insverify table, so that MySQL can use insverify.FKKey as an index.
        var command =
            mainQuery +
            insVerifyJoin1 +
            whereClause + @"
				UNION ALL
				" +
            mainQuery +
            insVerifyJoin2 +
            whereClause;
        return "SELECT * FROM (" + command + ") AS iv GROUP BY PatPlanNum,FKey HAVING MAX(DateLastVerified) ORDER BY AptDateTime";
    }

    private static List<InsVerifyGridObject> TableToListInsVerifyGridObjects(DataTable table, bool isForMedicaid)
    {
        var listInsVerifies = InsVerifyCrud.TableToList(table);
        var checkBenefitYear = PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear);
        var checkPatEnrollmentYear = PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear);
        var checkBenefitAndPatEnrollmentYearOn = checkBenefitYear && checkPatEnrollmentYear;
        var checkBenefitAndPatEnrollmentYearOff = !checkBenefitYear && !checkPatEnrollmentYear;
        var listInsVerifyGridObjects = new List<InsVerifyGridObject>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var insVerify = listInsVerifies[i].Clone();
            insVerify.PatNum = SIn.Long(row["PatNum"].ToString());
            insVerify.PlanNum = SIn.Long(row["PlanNum"].ToString());
            insVerify.PatPlanNum = SIn.Long(row["PatPlanNum"].ToString());
            insVerify.ClinicName = SIn.String(row["ClinicName"].ToString());
            var patName = SIn.String(row["LName"].ToString())
                          + ", ";
            if (SIn.String(row["Preferred"].ToString()) != "") patName += "'" + SIn.String(row["Preferred"].ToString()) + "' ";
            patName += SIn.String(row["FName"].ToString());
            insVerify.PatientName = patName;
            insVerify.CarrierName = SIn.String(row["CarrierName"].ToString());
            insVerify.AppointmentDateTime = SIn.DateTime(row["AptDateTime"].ToString());
            insVerify.AptNum = SIn.Long(row["AptNum"].ToString());
            insVerify.ClinicNum = SIn.Long(row["ClinicNum"].ToString()); //Non DB column. Used in OpenDentalService.
            insVerify.InsSubNum = SIn.Long(row["InsSubNum"].ToString()); //Non DB column. Used in OpenDentalService.
            insVerify.CarrierNum = SIn.Long(row["CarrierNum"].ToString()); //Non DB column. Used in OpenDentalService.
            if (insVerify.VerifyType == VerifyTypes.InsuranceBenefit)
            {
                var insVerifyGridObjectPlanExists = listInsVerifyGridObjects.Find(x => x.InsVerifyPlan != null && x.InsVerifyPlan.PlanNum == insVerify.PlanNum);
                if (insVerifyGridObjectPlanExists != null) continue;
                var insVerifyGridObjectExists = listInsVerifyGridObjects.Find(x => x.InsVerifyPat != null
                                                                                   && x.InsVerifyPat.PatPlanNum == insVerify.PatPlanNum
                                                                                   && x.InsVerifyPat.PlanNum == insVerify.PlanNum
                                                                                   && x.InsVerifyPat.Note == insVerify.Note
                                                                                   && x.InsVerifyPat.DefNum == insVerify.DefNum
                                                                                   && x.InsVerifyPlan == null);
                if ((checkBenefitAndPatEnrollmentYearOn || checkBenefitAndPatEnrollmentYearOff) && insVerifyGridObjectExists != null)
                {
                    //Both prefs on/off means combine pat/ins rows.
                    insVerifyGridObjectExists.InsVerifyPlan = insVerify;
                }
                else
                {
                    var insVerifyGridObject = new InsVerifyGridObject();
                    insVerifyGridObject.InsVerifyPlan = insVerify;
                    insVerifyGridObject.IsForMedicaidPlan = isForMedicaid;
                    listInsVerifyGridObjects.Add(insVerifyGridObject);
                }

                continue;
            }

            if (insVerify.VerifyType != VerifyTypes.PatientEnrollment) continue;
            var insVerifyGridObjectPatExists = listInsVerifyGridObjects.Find(x => x.InsVerifyPat != null && x.InsVerifyPat.PatPlanNum == insVerify.PatPlanNum);
            if (insVerifyGridObjectPatExists != null) continue;
            var insVerifyGridObjectObjExists = listInsVerifyGridObjects.Find(x => x.InsVerifyPlan != null
                                                                                  && x.InsVerifyPlan.PlanNum == insVerify.PlanNum
                                                                                  && x.InsVerifyPlan.Note == insVerify.Note
                                                                                  && x.InsVerifyPlan.DefNum == insVerify.DefNum
                                                                                  && x.InsVerifyPat == null);
            if ((checkBenefitAndPatEnrollmentYearOn || checkBenefitAndPatEnrollmentYearOff) && insVerifyGridObjectObjExists != null)
            {
                //Both prefs on/off means combine pat/ins rows.
                insVerifyGridObjectObjExists.InsVerifyPat = insVerify;
            }
            else
            {
                var insVerifyGridObject = new InsVerifyGridObject();
                insVerifyGridObject.InsVerifyPat = insVerify;
                insVerifyGridObject.IsForMedicaidPlan = isForMedicaid;
                listInsVerifyGridObjects.Add(insVerifyGridObject);
            }
        }

        return listInsVerifyGridObjects;
    }

    public static void CleanupInsVerifyRows(DateTime dateStart, DateTime dateEndStandard, DateTime dateEndMedicaid)
    {
        //Nathan OK'd the necessity for a complex update query like this to avoid looping through update statements.  This will be changed to a crud update method sometime in the future.
        var command = "";
        var listInsVerifyNums = Db.GetListLong(GetInsVerifyCleanupQuery(dateStart, dateEndStandard, dateEndMedicaid));
        if (listInsVerifyNums.Count == 0) return;
        command = "UPDATE insverify "
                  + "SET insverify.DateLastAssigned='0001-01-01', "
                  + "insverify.DefNum=0, "
                  + "insverify.Note='', "
                  + "insverify.UserNum=0 "
                  + "WHERE insverify.InsVerifyNum IN (" + string.Join(",", listInsVerifyNums) + ")";
        Db.NonQ(command);
    }

    private static string GetInsVerifyCleanupQuery(DateTime dateStart, DateTime dateEndStandard, DateTime dateEndMedicaid)
    {
        var listInsFilingCodeNums = InsFilingCodes.GetAll().Select(x => x.InsFilingCodeNum).ToList();
        var listInsVerifyMedicaidFilingCodes = PrefC.GetString(PrefName.InsVerifyMedicaidFilingCodes).Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        //Construct two lists of InsFilingCodeNums; one for Medicaid CodeNums (contained in the InsVerifyMedicaidFilingCodes pref), and one for the rest of the CodeNums.
        var listInsFilingCodeNumsMedicaid = listInsVerifyMedicaidFilingCodes.Select(x => SIn.Long(x, false)).ToList();
        var listInsFilingCodeNumsStandard = listInsFilingCodeNums.FindAll(x => !listInsFilingCodeNumsMedicaid.Contains(x));
        listInsFilingCodeNumsStandard.Add(0); //Add 0 to include InsPlans with no filing code.
        var insFilingCodeNumsMedicaid = string.Join(",", listInsFilingCodeNumsMedicaid);
        var insFilingCodeNumsStandard = string.Join(",", listInsFilingCodeNumsStandard);
        var command = @"SELECT InsVerifyNum
				FROM (
					SELECT InsVerifyNum,patplan.PatNum,insplan.FilingCode
					FROM patplan
					INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum
					INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum
						AND insplan.HideFromVerifyList=0
					INNER JOIN insverify ON VerifyType=" + SOut.Int((int) VerifyTypes.InsuranceBenefit) + @"
						AND insverify.FKey=insplan.PlanNum
					WHERE insverify.DateLastAssigned>'0001-01-01'
					AND insverify.DateLastAssigned<" + SOut.Date(DateTime.Today.AddDays(-30)) + @"
				
					UNION
					
					SELECT InsVerifyNum,patplan.PatNum,insplan.FilingCode
					FROM patplan
					INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum
					INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum
					INNER JOIN insverify ON VerifyType=" + SOut.Int((int) VerifyTypes.PatientEnrollment) + @"
						AND insverify.FKey=patplan.PatPlanNum
					WHERE insverify.DateLastAssigned>'0001-01-01'
					AND insverify.DateLastAssigned<" + SOut.Date(DateTime.Today.AddDays(-30)) + @"
				) insverifies
				LEFT JOIN appointment ON appointment.PatNum=insverifies.PatNum
					AND appointment.AptStatus IN (" + SOut.Int((int) ApptStatus.Scheduled) + "," + SOut.Int((int) ApptStatus.Complete) + @")
					AND (((DATE(appointment.AptDateTime) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEndStandard) + @") AND insverifies.FilingCode IN(" + insFilingCodeNumsStandard + @"))"; //insFilingCodeNumsStandard will always contain zero.
        if (insFilingCodeNumsMedicaid != "") //Only add this bit if the list isn't empty, since calling empty IN() statements cause errors.
            command += @"
					OR
					((DATE(appointment.AptDateTime) BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEndMedicaid) + @") AND insverifies.FilingCode IN(" + insFilingCodeNumsMedicaid + @"))";
        command += @")
				GROUP BY insverifies.InsVerifyNum
				HAVING MAX(appointment.AptNum) IS NULL";
        return command;
    }

    public static InsVerify SetTimeAvailableForVerify(InsVerify insVerify, PlanToVerify planToVerify, int appointmentScheduledDays, int insBenefitEligibilityDays, int patientEnrollmentDays)
    {
        //DateAppointmentScheduled-DateAppointmentCreated
        DateTime dateTimeLatestApptScheduling;
        //DateAppointmentScheduled-appointmentScheduledDays (default is 7)
        DateTime dateTimeUntilAppt;
        //DateTime the appointment takes place.
        DateTime dateTimeAppointment;
        //DateTime the patient was scheduled to be re-verified
        DateTime dateTimeScheduledReVerify;
        //DateTime verification last took place.  Will be 01/01/0001 if verification has never happened.
        DateTime dateTimeLastVerified;
        //DateTime the insurance renewal month rolls over.
        //The month the renewal takes place is stored in the database.  If the month is 0, then it is actually january. 
        //It is the first day of the given month at midnight, or (Month#)/01/(year) @ 00:00AM. 
        //Set to max val by default in case the PrefName.InsVerifyFutureDateBenefitYear=false.
        //Since max val is later in time than the appointment time, it will be ignored.
        var dateBenifitRenewalNeeded = DateTime.MaxValue;

        #region Appointment Dates

        dateTimeAppointment = insVerify.AppointmentDateTime;
        dateTimeUntilAppt = insVerify.AppointmentDateTime.AddDays(-appointmentScheduledDays);
        //Calculate when the appointment was put into it's current time slot.
        //This will be the earliest datetime where the scheduled appointment time is what it is now
        var listHistAppointments = HistAppointments.GetForApt(insVerify.AptNum);
        listHistAppointments.RemoveAll(x => x.AptDateTime.Date != insVerify.AppointmentDateTime.Date);
        listHistAppointments = listHistAppointments.Where(x => x.AptStatus == ApptStatus.Scheduled).OrderBy(x => x.AptDateTime).ToList();
        if (listHistAppointments.Count > 0)
            //If the appointment was moved to the current date after the (Apt.DateTime-appointmentScheduledDays),
            //we only had (Apt.DateTime-listHistAppt.First().HistDateTstamp) days instead of (appointmentScheduledDays)
            dateTimeLatestApptScheduling = listHistAppointments.First().HistDateTStamp;
        else
            //Just in case there's no history for an appointment for some reason.
            //Shouldn't happen because a log entry is created when the appointment is created.
            //Use the date the appointment was created.  This is better than nothing and should never happen anyways.
            dateTimeLatestApptScheduling = Appointments.GetOneApt(insVerify.AptNum).SecDateTEntry;

        #endregion Appointment Dates

        #region Insurance Verification

        dateTimeLastVerified = insVerify.DateLastVerified;
        //Add defined number of days to date last verified to calculate when the next verification date should have started.
        if (planToVerify == PlanToVerify.InsuranceBenefits)
        {
            if (insVerify.DateLastVerified == DateTime.MinValue) //If it's the min value, the insurance has never been verified.
                dateTimeScheduledReVerify = insVerify.DateTimeEntry;
            else
                dateTimeScheduledReVerify = insVerify.DateLastVerified.AddDays(insBenefitEligibilityDays);
        }
        else
        {
            //PlanToVerify.PatientEligibility
            if (insVerify.DateLastVerified == DateTime.MinValue)
                dateTimeScheduledReVerify = insVerify.DateTimeEntry;
            else
                dateTimeScheduledReVerify = insVerify.DateLastVerified.AddDays(patientEnrollmentDays);
        }

        #endregion insurance verification

        #region Benifit Renewal

        if (PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear) || PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear))
        {
            var insPlan = InsPlans.GetPlan(insVerify.PlanNum, null);
            //Setup the month renew dates.  Need all 3 years in case the appointment verify window crosses over a year
            //e.g. Appt verify date: 12/30/2016 and Appt Date: 1/6/2017
            var dateTimeOldestRenewal = new DateTime(DateTime.Now.Year - 1, Math.Max((byte) 1, insPlan.MonthRenew), 1);
            var dateTimeMiddleRenewal = new DateTime(DateTime.Now.Year, Math.Max((byte) 1, insPlan.MonthRenew), 1);
            var dateTimeNewestRenewal = new DateTime(DateTime.Now.Year + 1, Math.Max((byte) 1, insPlan.MonthRenew), 1);
            //We want to find the date closest to the appointment date without going past it.
            if (dateTimeMiddleRenewal > dateTimeAppointment)
            {
                dateBenifitRenewalNeeded = dateTimeOldestRenewal;
            }
            else
            {
                if (dateTimeNewestRenewal > dateTimeAppointment)
                    dateBenifitRenewalNeeded = dateTimeMiddleRenewal;
                else
                    dateBenifitRenewalNeeded = dateTimeNewestRenewal;
            }
        }

        #endregion Benifit Renewal

        var dateTimeAbleToVerify = VerifyDateCalulation(dateTimeUntilAppt, dateTimeLatestApptScheduling, dateTimeScheduledReVerify, dateBenifitRenewalNeeded);
        insVerify.HoursAvailableForVerification = insVerify.AppointmentDateTime.Subtract(dateTimeAbleToVerify).TotalHours;
        return insVerify;
    }

    public static DateTime VerifyDateCalulation(DateTime dateTimeDaysUntilAppt, DateTime dateTimeApptLastScheduled, DateTime dateTimeVerificationExpired, DateTime dateBenefitRenewalNeeded)
    {
        //The date and time that the insurance verification has expired.  If it expired due to a benefit renewal, the time portion will assume midnight.
        var dateTimeVerificationFirstNeeded = new DateTime(Math.Min(dateTimeVerificationExpired.Ticks, dateBenefitRenewalNeeded.Ticks));
        //The date and time that the patient associated to the patient was put on the verification list (this would happen for either plan or benefit insverify types)
        //To show on the verification list, an appointment must be made, and a verification must have expired.
        //Because of this, we get the most recent requirement.  This ensures the exact moment both requirements were present.
        var dateTimeShowInVerificationList = new DateTime(Math.Max(dateTimeApptLastScheduled.Ticks, dateTimeVerificationFirstNeeded.Ticks));
        //The final requirement to show on the verification list is that the appointment needs to be X days away or sooner.
        //Because of this, we compare the X days and the exact date and time that the appointment requirements were met, and take the most recent one, since both need to be met.
        return new DateTime(Math.Max(dateTimeDaysUntilAppt.Ticks, dateTimeShowInVerificationList.Ticks));
    }
}

public enum PlanToVerify
{
    ///<summary>Used when we neet to verify both PatientEligibility and InsuranceBenefits.</summary>
    Both,

    ///<summary>Used when we need to verify that a specific patient is covered by a specific insurance.</summary>
    PatientEligibility,

    ///<summary>Used when we need to verify an insurance plan and insurance plan benefits.</summary>
    InsuranceBenefits
}

///<summary>Enum used to determine which list we want information for in regards to the Insurance Verification List form.</summary>
public enum InsVerifyListType
{
    ///<summary>Used when we want InsVerify information for both Standard and Medicaid.</summary>
    Both,

    ///<summary>Used when we want InsVerify information for just the Standard list.</summary>
    Standard,

    ///<summary>Used when we want InsVerify information for just the Medicaid list.</summary>
    Medicaid
}