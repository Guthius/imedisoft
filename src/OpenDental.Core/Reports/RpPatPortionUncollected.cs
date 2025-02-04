using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Diagnostics;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness {
	public class RpPatPortionUncollected {
		
		public static DataTable GetPatUncollected(DateTime dateFrom,DateTime dateTo,List<long> listClinicNums) {
			var s=new Stopwatch();;
			const bool hasClinicsEnabled = true;
			var listHiddenUnearnedDefNums=Defs.GetDefsNoCache(DefCat.PaySplitUnearnedType).FindAll(x => !string.IsNullOrEmpty(x.ItemValue)).Select(x => x.DefNum).ToList();
			var query=$@"SELECT proc.PatNum, proc.ProcDate,CONCAT(patient.LName,', ',patient.FName) Patient,procedurecode.AbbrDesc,proc.Fee,
				proc.Fee-proc.InsEst PatPortion,
				COALESCE(adj.adjAmt,0) Adjustment,
				COALESCE(pay.splitAmt,0) Payment,
				proc.Fee-proc.InsEst+COALESCE(adj.adjAmt,0)-COALESCE(pay.splitAmt,0) Uncollected
				FROM (
					SELECT procedurelog.ProcNum,procedurelog.CodeNum,procedurelog.ProcDate,procedurelog.PatNum,
					procedurelog.ProcFee*(procedurelog.BaseUnits+procedurelog.UnitQty)
						-SUM(IF(claimproc.Status={SOut.Int((int)ClaimProcStatus.CapComplete)},claimproc.WriteOff,0)) Fee,
					SUM(IF(claimproc.Status={SOut.Int((int)ClaimProcStatus.NotReceived)},claimproc.InsPayEst,
						IF(claimproc.Status IN({SOut.Int((int)ClaimProcStatus.Received)},{SOut.Int((int)ClaimProcStatus.Supplemental)}),claimproc.InsPayAmt,0)))
						+SUM(IF(claimproc.Status NOT IN({SOut.Int((int)ClaimProcStatus.CapComplete)},{SOut.Int((int)ClaimProcStatus.InsHist)}),claimproc.WriteOff,0)) InsEst
					FROM procedurelog
					LEFT JOIN claimproc ON claimproc.ProcNum=procedurelog.ProcNum
						AND claimproc.Status IN ({
							string.Join(",",new[] { ClaimProcStatus.NotReceived,ClaimProcStatus.Received,ClaimProcStatus.Supplemental,ClaimProcStatus.CapComplete }
								.Select(x => SOut.Int((int)x)))})
					WHERE procedurelog.ProcStatus={SOut.Int((int)ProcStat.C)}
					AND procedurelog.ProcDate BETWEEN {SOut.Date(dateFrom)} AND {SOut.Date(dateTo)}";
			if(hasClinicsEnabled && listClinicNums.Count>0) {
				query+=$@"
					AND procedurelog.ClinicNum IN({string.Join(",",listClinicNums.Select(x => SOut.Long(x)))})";
			}
			query+=$@"
					GROUP BY procedurelog.ProcNum
					ORDER BY NULL
				) proc
				INNER JOIN patient ON patient.PatNum=proc.PatNum
				INNER JOIN procedurecode ON procedurecode.CodeNum=proc.CodeNum
				LEFT JOIN (
					SELECT adjustment.ProcNum,SUM(adjustment.AdjAmt) - COALESCE(SUM(paysplit.SplitAmt),0) adjAmt
					FROM adjustment
					LEFT JOIN paysplit
						ON paysplit.AdjNum = adjustment.AdjNum
						AND paysplit.AdjNum != 0 ";
			if(listHiddenUnearnedDefNums.Count>0) {
				query+=$"AND paysplit.UnearnedType NOT IN ({string.Join(",",listHiddenUnearnedDefNums)}) ";
			}
			query+=@$"WHERE adjustment.ProcNum!=0
					AND adjustment.ProcDate BETWEEN {SOut.Date(dateFrom)} AND {SOut.Date(dateTo)}
					GROUP BY adjustment.ProcNum
					ORDER BY NULL
				) adj ON adj.ProcNum=proc.ProcNum
				LEFT JOIN (
					SELECT paysplit.ProcNum,SUM(paysplit.SplitAmt) splitAmt
					FROM paysplit
					WHERE paysplit.ProcNum!=0 ";
			if(listHiddenUnearnedDefNums.Count>0) {
				query+=$"AND paysplit.UnearnedType NOT IN ({string.Join(",",listHiddenUnearnedDefNums)}) ";
			}
			query+=@"GROUP BY paysplit.ProcNum
					ORDER BY NULL
				) pay ON pay.ProcNum=proc.ProcNum
				WHERE proc.Fee-proc.InsEst+COALESCE(adj.adjAmt,0)-COALESCE(pay.splitAmt,0)>0.005
				ORDER BY proc.ProcDate,patient.LName,patient.FName,procedurecode.ProcCode";
			var table=DataCore.GetTable(query);
			return table;
		}
	}
}
