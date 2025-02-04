using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness {
	public class RpProcOverpaid {
		public static DataTable GetOverPaidProcs(long patNum,List<long> listProvNums,List<long> listClinics,DateTime dateStart,DateTime dateEnd,
			bool isOnlyShowingInsOrWoOverpaid=false) {
			var listHiddenUnearnedDefNums=Defs.GetDefsNoCache(DefCat.PaySplitUnearnedType).FindAll(x => !string.IsNullOrEmpty(x.ItemValue)).Select(x => x.DefNum).ToList();
			#region Completed Procs
			var command="SELECT ";
			if(PrefC.GetBool(PrefName.ReportsShowPatNum)) {
				command+=DbHelper.Concat("CAST(patient.PatNum AS CHAR)","'-'","patient.LName","', '","patient.FName","' '","patient.MiddleI");
			}
			else {
				command+=DbHelper.Concat("patient.LName","', '","patient.FName","' '","patient.MiddleI");
			}
			command+=@" AS 'patientName',
				procedurelog.ProcDate,
				procedurecode.ProcCode,
				procedurelog.ToothNum,
				provider.Abbr,
				(procedurelog.ProcFee*(procedurelog.UnitQty+procedurelog.BaseUnits)) AS fee,
				patient.PatNum,
				procedurelog.ProcNum
				FROM procedurelog
				INNER JOIN patient ON patient.PatNum=procedurelog.PatNum
				INNER JOIN procedurecode ON procedurecode.CodeNum=procedurelog.CodeNum
				INNER JOIN provider ON provider.ProvNum=procedurelog.ProvNum
				WHERE procedurelog.ProcStatus="+SOut.Int((int)ProcStat.C)+" AND "
				+DbHelper.BetweenDates("procedurelog.ProcDate",dateStart,dateEnd)+" "
				+"AND procedurelog.ProcFee>=0 ";
			if(listProvNums!=null && listProvNums.Count > 0) {
				command+="AND procedurelog.ProvNum IN ("+string.Join(",",listProvNums.Select(x => SOut.Long(x)))+") ";
			}
			if(listClinics!=null && listClinics.Count > 0) {
				command+="AND procedurelog.ClinicNum IN ("+string.Join(",",listClinics.Select(x => SOut.Long(x)))+") ";
			}
			if(patNum>0) {
				command+="AND procedurelog.PatNum="+SOut.Long(patNum)+" ";
			}
			command+="ORDER BY procedurelog.ProcDate,patientName,procedurecode.ProcCode,provider.Abbr";
			var rawCompletedProcTable=DataCore.GetTable(command);
			var dictCompletedProcRows=rawCompletedProcTable.Select().ToDictionary(x => SIn.Long(x["ProcNum"].ToString()));
			#endregion
			var table=new DataTable();
			if(dictCompletedProcRows.Count==0) {
				return table;
			}
			#region ClaimProcs
			var listPatNums=rawCompletedProcTable.Select().Select(x => SIn.Long(x["PatNum"].ToString())).Distinct().ToList();
			command=@"SELECT MIN(claimproc.ProcNum) ProcNum,MIN(claimproc.PatNum) PatNum,MIN(claimproc.ProcDate) ProcDate,SUM(claimproc.InsPayAmt) insPayAmt,
				SUM(claimproc.Writeoff) writeoff
				FROM claimproc
				WHERE claimproc.Status NOT IN("+string.Join(",",new List<int>{ (int)ClaimProcStatus.Preauth,
				(int)ClaimProcStatus.CapEstimate,(int)ClaimProcStatus.CapComplete,(int)ClaimProcStatus.Estimate,(int)ClaimProcStatus.InsHist }
					.Select(x => SOut.Int(x)))+") "
				+"AND "+DbHelper.BetweenDates("claimproc.ProcDate",dateStart,dateEnd)+" "
				+"AND claimproc.PatNum IN("+string.Join(",",listPatNums.Select(x => SOut.Long(x)))+") "
				+@"GROUP BY claimproc.ProcNum
				HAVING SUM(claimproc.InsPayAmt+claimproc.Writeoff)>0
				ORDER BY NULL";
			var dictClaimProcRows=DataCore.GetTable(command).Select().ToDictionary(x => SIn.Long(x["ProcNum"].ToString()));
			#endregion
			#region Patient Payments
			command=@"SELECT paysplit.ProcNum,SUM(paysplit.SplitAmt) ptAmt
				FROM paysplit
				WHERE paysplit.ProcNum>0
				AND paysplit.PatNum IN("+string.Join(",",listPatNums.Select(x => SOut.Long(x)))+$@") ";
			if(listHiddenUnearnedDefNums.Count>0) {
				command+=$"AND paysplit.UnearnedType NOT IN ({string.Join(",",listHiddenUnearnedDefNums)}) ";
			}
			command+=@"
				GROUP BY paysplit.ProcNum
				ORDER BY NULL";
			var dictPatPayRows=DataCore.GetTable(command).Select().ToDictionary(x => SIn.Long(x["ProcNum"].ToString()));
			#endregion
			#region Adjustments
			command=@"SELECT adjustment.ProcNum,SUM(adjustment.AdjAmt) AdjAmt
				FROM adjustment
				WHERE adjustment.ProcNum>0
				AND adjustment.PatNum IN("+string.Join(",",listPatNums.Select(x => SOut.Long(x)))+@")
				GROUP BY adjustment.ProcNum
				ORDER BY NULL";
			var dictAdjRows=DataCore.GetTable(command).Select().ToDictionary(x => SIn.Long(x["ProcNum"].ToString()));
			#endregion
			//columns that start with lowercase are altered for display rather than being raw data.
			table.Columns.Add("patientName");
			table.Columns.Add("ProcDate",typeof(DateTime));
			table.Columns.Add("ProcCode");
			table.Columns.Add("ToothNum");
			table.Columns.Add("Abbr");
			table.Columns.Add("fee");
			table.Columns.Add("insPaid");
			table.Columns.Add("wo");
			table.Columns.Add("ptPaid");
			table.Columns.Add("adjAmt");
			table.Columns.Add("overPay");
			table.Columns.Add("PatNum");
			DataRow row;
			foreach(var kvp in dictCompletedProcRows) {
				var procNum=kvp.Key;
				var procFeeAmt=SIn.Decimal(kvp.Value["fee"].ToString());
				decimal insPaidAmt=0;
				decimal woAmt=0;
				decimal ptPaidAmt=0;
				decimal adjAmt=0;
				if(dictClaimProcRows.ContainsKey(procNum)) {
					insPaidAmt=SIn.Decimal(dictClaimProcRows[procNum]["insPayAmt"].ToString());
					woAmt=SIn.Decimal(dictClaimProcRows[procNum]["writeoff"].ToString());
				}
				if(dictPatPayRows.ContainsKey(procNum)) {
					ptPaidAmt=SIn.Decimal(dictPatPayRows[procNum]["ptAmt"].ToString());
				}
				if(dictAdjRows.ContainsKey(procNum)) {
					adjAmt=SIn.Decimal(dictAdjRows[procNum]["AdjAmt"].ToString());
				}
				var overPay=procFeeAmt-insPaidAmt-woAmt-ptPaidAmt+adjAmt;
				if(!CompareDecimal.IsLessThanZero(overPay)) {
					continue;//No overpayment. Not need to continue;
				}
				if(isOnlyShowingInsOrWoOverpaid) {
					if(CompareDecimal.IsZero(insPaidAmt) && CompareDecimal.IsZero(woAmt)) {
						continue;
					}
				}
				row=table.NewRow();
				row["patientName"]=SIn.String(kvp.Value["patientName"].ToString());
				row["ProcDate"]=SIn.Date(kvp.Value["ProcDate"].ToString());
				row["ProcCode"]=SIn.String(kvp.Value["ProcCode"].ToString());
				row["ToothNum"]=SIn.String(kvp.Value["ToothNum"].ToString());
				row["Abbr"]=SIn.String(kvp.Value["Abbr"].ToString()); ;
				row["fee"]=procFeeAmt.ToString();
				row["insPaid"]=insPaidAmt.ToString();
				row["wo"]=woAmt.ToString();
				row["ptPaid"]=ptPaidAmt.ToString();
				row["adjAmt"]=adjAmt.ToString();
				row["overPay"]=overPay.ToString();
				row["PatNum"]=SIn.Long(kvp.Value["PatNum"].ToString());
				table.Rows.Add(row);
			}
			return table;
		}
	}
}
