using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness {
	public class RpPPOwriteoff {
		
		public static DataTable GetWriteoffTable(DateTime dateStart,DateTime dateEnd,bool isIndividual,string carrierText, PPOWriteoffDateCalc writeoffType) {
			string queryText="";
			//individual
			if(isIndividual) {
				queryText="SET @DateFrom="+SOut.Date(dateStart)+", @DateTo="+SOut.Date(dateEnd)
				+", @CarrierName='%"+SOut.String(carrierText)+"%';";
				if(writeoffType==PPOWriteoffDateCalc.InsPayDate) {
					queryText+=@"SELECT claimproc.DateCP,
					CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),patient.FName),' '),patient.MiddleI),
					carrier.CarrierName,
					provider.Abbr,
					SUM(claimproc.FeeBilled),
					-SUM(claimproc.WriteOff),
					SUM(claimproc.FeeBilled-claimproc.WriteOff) 'PPO Fee',
					claimproc.ClaimNum
					FROM claimproc
					INNER JOIN insplan ON claimproc.PlanNum = insplan.PlanNum 
						AND insplan.PlanType='p'
					INNER JOIN patient ON claimproc.PatNum = patient.PatNum
					INNER JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum 
						AND carrier.CarrierName LIKE @CarrierName
					INNER JOIN provider ON provider.ProvNum = claimproc.ProvNum
					WHERE claimproc.Status IN ("+(int)ClaimProcStatus.Received+","+(int)ClaimProcStatus.Supplemental+@") 
					AND DATE(claimproc.DateCP)"+@" >= @DateFrom
					AND DATE(claimproc.DateCP)"+@" <= @DateTo
					GROUP BY claimproc.ClaimNum 
					ORDER BY claimproc.DateCP";
				}
				else if(writeoffType==PPOWriteoffDateCalc.ProcDate){//use procedure date
					queryText+=@"SELECT claimproc.ProcDate,
					CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),patient.FName),' '),patient.MiddleI),
					carrier.CarrierName,
					provider.Abbr,
					SUM(claimproc.FeeBilled),
					-SUM(claimproc.WriteOff),
					SUM(claimproc.FeeBilled-claimproc.WriteOff) 'PPO Fee',
					claimproc.ClaimNum
					FROM claimproc
					INNER JOIN insplan ON claimproc.PlanNum = insplan.PlanNum 
						AND insplan.PlanType='p'
					INNER JOIN patient ON claimproc.PatNum = patient.PatNum
					INNER JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum 
						AND carrier.CarrierName LIKE @CarrierName
					INNER JOIN provider ON provider.ProvNum = claimproc.ProvNum
					WHERE claimproc.Status IN ("+(int)ClaimProcStatus.Received+","+(int)ClaimProcStatus.Supplemental+","+(int)ClaimProcStatus.NotReceived+@") 
					AND DATE(claimproc.ProcDate)"+@" >= @DateFrom
					AND DATE(claimproc.ProcDate)"+@" <= @DateTo
					GROUP BY claimproc.ClaimNum 
					ORDER BY claimproc.ProcDate";
				}
				else  {//writeoffType==PPOWriteoffDateCalc.ClaimPayDate
					queryText+=@"SELECT claimsnapshot.DateTEntry,
					CONCAT(CONCAT(CONCAT(CONCAT(patient.LName,', '),patient.FName),' '),patient.MiddleI),
					carrier.CarrierName,
					provider.Abbr,
					SUM(claimproc.FeeBilled),
					-SUM("+DbHelper.IfNull("NULLIF(claimsnapshot.WriteOff, -1)","0",false)+@"),
					SUM(IF(claimproc.Status="+(int)ClaimProcStatus.NotReceived+@",0,"+DbHelper.IfNull("NULLIF(claimsnapshot.WriteOff, -1)","0",false)+@"-claimproc.WriteOff)),
					SUM(claimproc.FeeBilled
						-IF(claimproc.Status="+(int)ClaimProcStatus.NotReceived+@","+DbHelper.IfNull("NULLIF(claimsnapshot.WriteOff, -1)","0",false)+@",claimproc.WriteOff)) 'PPO Fee',
					claimproc.ClaimNum
					FROM claimproc
					INNER JOIN insplan ON insplan.PlanNum=claimProc.PlanNum 
						AND insplan.PlanType='p'
					INNER JOIN patient ON patient.PatNum=claimProc.PatNum
					INNER JOIN carrier ON carrier.CarrierNum=insplan.CarrierNum 
						AND carrier.CarrierName LIKE @CarrierName
					INNER JOIN provider ON provider.ProvNum=claimProc.ProvNum
					INNER JOIN claimsnapshot ON claimsnapshot.ClaimProcNum=claimproc.ClaimProcNum 
						AND DATE(claimsnapshot.DateTEntry)"+@" BETWEEN @DateFrom AND @DateTo
					WHERE claimproc.Status IN ("+(int)ClaimProcStatus.Received+","+(int)ClaimProcStatus.Supplemental+","+(int)ClaimProcStatus.NotReceived+@") 
					GROUP BY claimproc.ClaimNum 
					ORDER BY claimsnapshot.DateTEntry";
				}
			}
			else {
				//group
				if(writeoffType==PPOWriteoffDateCalc.InsPayDate) {
					queryText="SET @DateFrom="+SOut.Date(dateStart)+", @DateTo="+SOut.Date(dateEnd)
						+", @CarrierName='%"+SOut.String(carrierText)+"%';"
						+@"SELECT carrier.CarrierName,
						SUM(claimproc.FeeBilled),
						-SUM(claimproc.WriteOff),
						SUM(claimproc.FeeBilled-claimproc.WriteOff) 'PPO Fee',
						claimproc.ClaimNum
						FROM claimproc
						INNER JOIN insplan ON claimproc.PlanNum = insplan.PlanNum
							AND insplan.PlanType='p'
						INNER JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum
							AND carrier.CarrierName LIKE @CarrierName
						WHERE claimproc.Status IN ("+(int)ClaimProcStatus.Received+","+(int)ClaimProcStatus.Supplemental+@") 
						AND DATE(claimproc.DateCP)"+@" >= @DateFrom
						AND DATE(claimproc.DateCP)"+@" <= @DateTo
						GROUP BY carrier.CarrierNum 
						ORDER BY carrier.CarrierName";
				}
				else if(writeoffType==PPOWriteoffDateCalc.ProcDate) {
					queryText="SET @DateFrom="+SOut.Date(dateStart)+", @DateTo="+SOut.Date(dateEnd)
						+", @CarrierName='%"+SOut.String(carrierText)+"%';"
						+@"SELECT carrier.CarrierName,
						SUM(claimproc.FeeBilled),
						-SUM(claimproc.WriteOff),
						SUM(claimproc.FeeBilled-claimproc.WriteOff) 'PPO Fee',
						claimproc.ClaimNum
						FROM claimproc
						INNER JOIN insplan ON claimproc.PlanNum = insplan.PlanNum
							AND insplan.PlanType='p'
						INNER JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum
							AND carrier.CarrierName LIKE @CarrierName
						WHERE claimproc.Status IN ("+(int)ClaimProcStatus.Received+","+(int)ClaimProcStatus.Supplemental+","+(int)ClaimProcStatus.NotReceived+@") 
						AND DATE(claimproc.ProcDate)"+@" >= @DateFrom
						AND DATE(claimproc.ProcDate)"+@" <= @DateTo
						GROUP BY carrier.CarrierNum 
						ORDER BY carrier.CarrierName";
				}
				else {	// writeoffType==PPOWriteoffDateCalc.ClaimPayDate
					queryText="SET @DateFrom="+SOut.Date(dateStart)+", @DateTo="+SOut.Date(dateEnd)
						+", @CarrierName='%"+SOut.String(carrierText)+"%';"
						+@"SELECT carrier.CarrierName,
						SUM(claimproc.FeeBilled),
						-SUM("+DbHelper.IfNull("NULLIF(claimsnapshot.WriteOff, -1)","0",false)+@"),
						SUM(IF(claimproc.Status="+(int)ClaimProcStatus.NotReceived+@",0,"+DbHelper.IfNull("NULLIF(claimsnapshot.WriteOff, -1)","0",false)+@"-claimproc.WriteOff)),
						SUM(claimproc.FeeBilled
							-IF(claimproc.Status="+(int)ClaimProcStatus.NotReceived+@","+DbHelper.IfNull("NULLIF(claimsnapshot.WriteOff, -1)","0",false)+@",claimproc.WriteOff)) 'PPO Fee',
						claimproc.ClaimNum
						FROM claimproc
						INNER JOIN insplan on claimproc.PlanNum = insplan.PlanNum
							AND insplan.PlanType='p'
						INNER JOIN carrier on carrier.CarrierNum = insplan.CarrierNum
							AND carrier.CarrierName LIKE @CarrierName						
						INNER JOIN claimsnapshot on claimsnapshot.ClaimProcNum=claimproc.ClaimProcNum
							AND DATE(claimsnapshot.DateTEntry)"+@" >= @DateFrom
							AND DATE(claimsnapshot.DateTEntry)"+@" <= @DateTo
						WHERE claimproc.Status IN ("+(int)ClaimProcStatus.Received+","+(int)ClaimProcStatus.Supplemental+","+(int)ClaimProcStatus.NotReceived+@") 
						GROUP BY carrier.CarrierNum 
						ORDER BY carrier.CarrierName";
				}
			}
			return ReportsComplex.GetTable(queryText);
		}	
	}

}
