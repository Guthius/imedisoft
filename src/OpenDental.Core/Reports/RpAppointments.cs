using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness {
	public class RpAppointments {
		///<summary>If not using clinics then supply an empty list of clinicNums.</summary>
		public static DataTable GetAppointmentTable(DateTime dateStart,DateTime dateEnd,List<long> listProvNums,List<long> listClinicNums,
			bool hasClinicsEnabled,bool isShowRecall,bool isShowNewPat,bool isShowASAP,bool isShowExistingPat,SortAndFilterBy sortBy,List<ApptStatus> listApptStatusesToExclude,
			List<long> listConfirmationStatuses,string formSender) 
		{
			//Appointment status conditions
			var whereApptStatus="";
			if(listApptStatusesToExclude.Count > 0) {
				whereApptStatus+=" appointment.AptStatus NOT IN ("+string.Join(",",listApptStatusesToExclude.Select(x => SOut.Int((int)x)))+") AND ";
			}
			//Provider Conditions
			var whereProv="";
			if(listProvNums.Count > 0) {
				whereProv+=" (appointment.ProvNum IN("+string.Join(",",listProvNums)+") "
					+" OR appointment.ProvHyg IN("+string.Join(",",listProvNums)+")) AND ";
			}
			//Clinic Conditions
			var whereClinics="";
			if(hasClinicsEnabled && listClinicNums.Count > 0) {
				whereClinics+=" appointment.ClinicNum IN("+string.Join(",",listClinicNums)+") AND ";
			}
			//Appointment confirmation conditions
			var whereConfStatus="";
			if(listConfirmationStatuses.Count > 0) {
				whereConfStatus+=" appointment.Confirmed IN ("+string.Join(",",listConfirmationStatuses)+") AND ";
			}
			//Query
			var command = @"SELECT ";
			if(sortBy==SortAndFilterBy.SecDateTEntry) {
				command+="appointment.SecDateTEntry,";
			}
			command += 
				@"appointment.AptDateTime,
				patient.PatNum,
				TRIM(CONCAT(CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),CASE WHEN LENGTH(patient.Preferred) > 0 THEN CONCAT(CONCAT('(',patient.Preferred),') ') ELSE '' END),patient.FName), ' '),patient.MiddleI)) PatName,
				patient.Birthdate,
				appointment.AptDateTime,
				LENGTH(appointment.Pattern)*5 AptLength,
				appointment.ProcDescript,
				patient.HmPhone,
				patient.WkPhone,
				patient.WirelessPhone,
				COALESCE(clinic.Description,'"+SOut.String(Lans.g("formSender","Unassigned"))+@"') ClinicDesc,
				appointment.SecDateTEntry AS 'DateTimeCreated',
				appointment.Confirmed,
				appointment.Note,
				appointment.AptNum
				FROM appointment
				INNER JOIN patient ON appointment.PatNum=patient.PatNum "+
				@" LEFT JOIN clinic ON appointment.ClinicNum=clinic.ClinicNum 
				WHERE "
				+whereApptStatus
				+whereProv
				+whereClinics
				+whereConfStatus;
			if(sortBy==SortAndFilterBy.SecDateTEntry) {
				command+=" "+DbHelper.BetweenDates("appointment.SecDateTEntry",dateStart,dateEnd)
					+" ORDER BY appointment.ClinicNum,appointment.SecDateTEntry,PatName";
			}
			else if(sortBy==SortAndFilterBy.AptDateTime) {
				command+=" appointment.AptDateTime BETWEEN "+SOut.Date(dateStart)+" AND "+SOut.Date(dateEnd.AddDays(1))
					+" ORDER BY appointment.ClinicNum,appointment.AptDateTime,PatName";
			}
			var table=DataCore.GetTable(command);
			return table;
		}

		///<summary>Set the date that RpAppointments.GetAppointmentTable will use to sort and filter results.</summary>
		public enum SortAndFilterBy {
			///<summary>Sort and filter on appointment.SecDateTEntry</summary>
			SecDateTEntry,
			///<summary>Sort and filter on appointment.AptDateTime</summary>
			AptDateTime,
		}

	}

}
