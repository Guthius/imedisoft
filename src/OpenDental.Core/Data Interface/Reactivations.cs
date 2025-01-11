using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Reactivations
{
    public static void Insert(Reactivation reactivation)
    {
        ReactivationCrud.Insert(reactivation);
    }
    
    public static void Delete(long reactivationNum)
    {
        ReactivationCrud.Delete(reactivationNum);
    }
    
    public static Reactivation GetOne(long reactivationNum)
    {
        return ReactivationCrud.SelectOne(reactivationNum);
    }

    public static int GetNumReminders(long patNum)
    {
        var commType = Commlogs.GetTypeAuto(CommItemTypeAuto.REACT);
        if (commType == 0) return 0;
        var cmd =
            @"SELECT
					COUNT(*) AS NumReminders
					FROM commlog
					WHERE commlog.CommType=" + SOut.Long(commType) + " " +
            "AND commlog.PatNum=" + SOut.Long(patNum);
        return SIn.Int(DataCore.GetScalar(cmd));
    }

    public static DateTime GetDateLastContacted(long patNum)
    {
        var commType = Commlogs.GetTypeAuto(CommItemTypeAuto.REACT);
        if (commType == 0) return DateTime.MinValue;
        var cmd =
            @"SELECT
					MAX(commlog.CommDateTime) AS DateLastContacted
					FROM commlog
					WHERE commlog.CommType=" + SOut.Long(commType) + " " +
            "AND commlog.PatNum=" + SOut.Long(patNum) + " " +
            "GROUP BY commlog.PatNum";
        return SIn.DateTime(DataCore.GetScalar(cmd));
    }

    ///<summary>Gets the list of patients that need to be on the reactivation list based on the passed in filters.</summary>
    public static DataTable GetReactivationList(DateTime dateSince, DateTime dateStop, bool groupFamilies, bool showDoNotContact, bool isInactiveIncluded
        , long provNum, long clinicNum, long siteNum, long billingType, ReactivationListSort sortBy, RecallListShowNumberReminders showReactivations)
    {
        //Get information we will need to do the query
        var listReactCommLogTypeDefNums = Defs.GetDefsForCategory(DefCat.CommLogTypes, true)
            .FindAll(x => CommItemTypeAuto.REACT.GetDescription(true).Equals(x.ItemValue)).Select(x => x.DefNum).ToList();
        if (listReactCommLogTypeDefNums.Count == 0) return new DataTable();
        var contactInterval = PrefC.GetInt(PrefName.ReactivationContactInterval);
        var listPatStatuses = new List<PatientStatus> {PatientStatus.Patient, PatientStatus.Prospective};
        if (isInactiveIncluded) listPatStatuses.Add(PatientStatus.Inactive);
        var strPatStatuses = string.Join(",", listPatStatuses.Select(x => SOut.Int((int) x)));
        //Get the raw set of patients who should be on the reactivation list
        var cmd =
            $@"SELECT 
						pat.PatNum,
						pat.LName,
						pat.FName,
						pat.MiddleI,
						pat.Preferred,
						pat.Guarantor,
						pat.PatStatus,
						pat.Birthdate,
						pat.PriProv,
						COALESCE(billingtype.ItemName,'') AS BillingType,
						pat.ClinicNum,
						pat.SiteNum,
						pat.PreferRecallMethod,
						'' AS ContactMethod,
						pat.HmPhone,
						pat.WirelessPhone,
						pat.WkPhone,
						{(groupFamilies ? "COALESCE(guarantor.Email,pat.Email,'') AS Email," : "pat.Email,")}
						MAX(proc.ProcDate) AS DateLastProc,
						COALESCE(comm.DateLastContacted,'') AS DateLastContacted,
						COALESCE(comm.ContactedCount,0) AS ContactedCount,
						COALESCE(react.ReactivationNum,0) AS ReactivationNum,
						COALESCE(react.ReactivationStatus,0) AS ReactivationStatus,
						COALESCE(react.DoNotContact,0) as DoNotContact,
						react.ReactivationNote,
						guarantor.PatNum as GuarNum,
						guarantor.LName as GuarLName,
						guarantor.FName as GuarFName
					FROM patient pat
					INNER JOIN procedurelog proc ON pat.PatNum=proc.PatNum AND proc.ProcStatus={SOut.Int((int) ProcStat.C)}
					INNER JOIN procedurecode ON procedurecode.CodeNum=proc.CodeNum AND procedurecode.ProcCode NOT IN ('D9986','D9987')
					LEFT JOIN appointment appt ON pat.PatNum=appt.PatNum AND appt.AptDateTime >= {DbHelper.Curdate()} 
					LEFT JOIN (
						SELECT
							commlog.PatNum,
							MAX(commlog.CommDateTime) AS DateLastContacted,
							COUNT(*) AS ContactedCount
							FROM commlog
							WHERE commlog.CommType IN ({string.Join(",", listReactCommLogTypeDefNums)}) 
							GROUP BY commlog.PatNum
					) comm ON pat.PatNum=comm.PatNum
					LEFT JOIN reactivation react ON pat.PatNum=react.PatNum
					LEFT JOIN definition billingtype ON pat.BillingType=billingtype.DefNum
					INNER JOIN patient guarantor ON pat.Guarantor=guarantor.PatNum
					WHERE pat.PatStatus IN ({strPatStatuses}) ";
        cmd += provNum > 0 ? " AND pat.PriProv=" + SOut.Long(provNum) : "";
        cmd += clinicNum > -1 ? " AND pat.ClinicNum=" + SOut.Long(clinicNum) : ""; //might still want to get the 0 clinic pats
        cmd += siteNum > 0 ? " AND pat.SiteNum=" + SOut.Long(siteNum) : "";
        cmd += billingType > 0 ? " AND pat.BillingType=" + SOut.Long(billingType) : "";
        cmd += showDoNotContact ? "" : " AND (react.DoNotContact IS NULL OR react.DoNotContact=0)";
        cmd += contactInterval > -1 ? " AND (comm.DateLastContacted IS NULL OR comm.DateLastContacted <= " + SOut.DateT(DateTime.Today.AddDays(-contactInterval)) + ") " : "";
        //set number of contact attempts
        var maxReminds = PrefC.GetInt(PrefName.ReactivationCountContactMax);
        if (showReactivations == RecallListShowNumberReminders.SixPlus)
        {
            cmd += " AND ContactedCount>=6 "; //don't need to look at pref this only shows in UI if the prefvalue allows it
        }
        else if (showReactivations == RecallListShowNumberReminders.Zero)
        {
            cmd += " AND (comm.ContactedCount=0 OR comm.ContactedCount IS NULL) ";
        }
        else if (showReactivations != RecallListShowNumberReminders.All)
        {
            var filter = (int) showReactivations - 1;
            //if the contactmax pref is not -1 or 0, and the contactmax is smaller than the requested filter, replace the filter with the contactmax
            cmd += " AND comm.ContactedCount=" + SOut.Int(maxReminds > 0 && maxReminds < filter ? maxReminds : filter) + " ";
        }
        else if (showReactivations == RecallListShowNumberReminders.All)
        {
            //get all but filter on the contactmax
            cmd += " AND (comm.ContactedCount < " + SOut.Int(maxReminds) + " OR comm.ContactedCount IS NULL) ";
        }

        cmd += $@" GROUP BY pat.PatNum 
							HAVING MAX(proc.ProcDate) < {SOut.Date(dateSince)} AND MAX(proc.ProcDate) >= {SOut.Date(dateStop)}
							AND MIN(appt.AptDateTime) IS NULL ";
        //set the sort by
        switch (sortBy)
        {
            case ReactivationListSort.Alphabetical:
                cmd += " ORDER BY " + (groupFamilies ? "guarantor.LName,guarantor.FName,pat.FName" : "pat.LName,pat.FName");
                break;
            case ReactivationListSort.BillingType:
                cmd += " ORDER BY billingtype.ItemName,DateLastContacted" + (groupFamilies ? ",guarantor.LName,guarantor.FName" : "");
                break;
            case ReactivationListSort.LastContacted:
                cmd += " ORDER BY IF(comm.DateLastContacted='' OR comm.DateLastContacted IS NULL,1,0),comm.DateLastContacted" + (groupFamilies ? ",guarantor.LName,guarantor.FName" : "");
                break;
            case ReactivationListSort.LastSeen:
                cmd += " ORDER BY MAX(proc.ProcDate)";
                break;
        }

        var dtReturn = DataCore.GetTable(cmd);
        foreach (DataRow row in dtReturn.Rows)
            //FOR REVIEW: currently, we are displaying PreferRecallMethod, which is what RecallList also does.  Just want to make sure we don't want to use PreferContactMethod
            row["ContactMethod"] = Recalls.GetContactFromMethod(SIn.Enum<ContactMethod>(row["PreferRecallMethod"].ToString()), groupFamilies
                , row["HmPhone"].ToString(), row["WkPhone"].ToString(), row["WirelessPhone"].ToString(), row["Email"].ToString() //guarEmail queried as Email
                , row["Email"].ToString()); //Pat.Email is also "Email"
        return dtReturn;
    }

    /// <summary>
    ///     Follows the format of the Recall addrTable, used in the RecallList to duplicate functionality for
    ///     mailing/emailing patients.
    /// </summary>
    public static DataTable GetAddrTable(List<Patient> listPats, List<Patient> listGuars, bool groupFamilies, ReactivationListSort sortBy)
    {
        var table = Recalls.GetAddrTableStructure();
        var listPatsOrGuars = listPats; //Default to the list of patients passed in.
        //Utilize listGuars if groupFamilies is true so that family members do not get their own row.
        if (groupFamilies)
            //This makes it so that we only return one family address even if the user has passed in every single member of the family.
            listPatsOrGuars = listGuars.FindAll(x => listPats.Select(y => y.Guarantor).Contains(x.PatNum));
        foreach (var pat in listPatsOrGuars)
        {
            var patCur = pat; //Always the guarantor if grouping by family, otherwise a selected patient.
            var guar = listGuars.FirstOrDefault(x => x.PatNum == pat.Guarantor);
            //Only include Patients that were selected, rather than all family members.
            var listSelectedPatsInFam = listPats.Where(x => x.Guarantor == guar.PatNum).ToList();
            if (listSelectedPatsInFam.Count == 1) //Selected patient may not be the guarantor.
                //So use first selected patient because this will result in an individual postcard, which should show the selected patient's name, not the
                //name of the guarantor.
                patCur = listSelectedPatsInFam.First();
            var famList = ""; //If famList is blank, the single patient reactivation email/postcard template will be used.
            var strPatNums = patCur.PatNum.ToString();
            var email = patCur.Email;
            var emailPatNum = patCur.PatNum;
            var language = patCur.Language;
            if (groupFamilies)
            {
                if (listSelectedPatsInFam.Count > 1) famList = string.Join(", ", listSelectedPatsInFam.Select(x => x.FName)); //If famList is set, the family email/postcard template will be used.
                strPatNums = string.Join(",", listSelectedPatsInFam.Select(x => x.PatNum));
                email = guar.Email; //Use guarantor email for single selected patient when grouping by family.
                emailPatNum = guar.PatNum;
                language = guar.Language;
            }

            var row = table.NewRow();
            row["address"] = patCur.Address + (!string.IsNullOrWhiteSpace(patCur.Address2) ? Environment.NewLine + patCur.Address2 : "");
            row["City"] = patCur.City;
            row["clinicNum"] = patCur.ClinicNum;
            row["dateDue"] = DateTime.MinValue; //This isn't used for reactivations, but it's here keep the table the same as recall addrTable
            row["email"] = email;
            row["emailPatNum"] = emailPatNum;
            row["famList"] = famList;
            row["guarLName"] = guar.LName;
            row["numberOfReminders"] = GetNumReminders(patCur.PatNum);
            row["patientNameF"] = patCur.GetNameFirstOrPreferred();
            row["patientNameFL"] = patCur.GetNameFLnoPref();
            row["patNums"] = strPatNums;
            row["State"] = patCur.State;
            row["Zip"] = patCur.Zip;
            row["Language"] = language; //This isn't used for reactivations, but it's here to keep the table the same as recall addrTable
            table.Rows.Add(row);
        }

        return table;
    }
    
    public static void Update(Reactivation reactivation)
    {
        ReactivationCrud.Update(reactivation);
    }

    public static void UpdateStatus(long reactivationNum, long statusDefNum)
    {
        var cmd = "UPDATE reactivation SET ReactivationStatus=" + (statusDefNum) + " WHERE ReactivationNum=" + (reactivationNum);
        Db.NonQ(cmd);
    }
}


public enum ReactivationListSort
{
    
    LastContacted,

    
    BillingType,

    
    Alphabetical,

    
    LastSeen
}