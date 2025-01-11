﻿using System;
using System.Collections;
using System.Drawing;

namespace OpenDentBusiness{
	///<summary>Enables viewing a variety of views in chart module.</summary>
	[Serializable]
	public class ChartView:TableBase{
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long ChartViewNum;
		///<summary>Description of this view.  Gets displayed at top of Progress Notes grid.</summary>
		public string Description;
		///<summary>0-based order to display in lists.</summary>
		public int ItemOrder;
		///<summary>Enum:ChartViewProcStat None=0,TP=1,Complete=2,Existing Cur Prov=4,Existing Other Prov=8,Referred=16,Deleted=32,Condition=64,All=127.</summary>
		public ChartViewProcStat ProcStatuses;
		///<summary>Enum:ChartViewObjs None=0,Appointments=1,Comm Log=2,Comm Log Family=4,Tasks=8,Email=16,LabCases=32,Rx=64,Sheets=128,Comm Log Super Family=256,All=511.</summary>
		public ChartViewObjs ObjectTypes;
		///<summary>Set true to show procedure notes.</summary>
		public bool ShowProcNotes;
		///<summary>Set true to enable audit mode.</summary>
		public bool IsAudit;
		///<summary>Set true to only show information regarding the selected teeth.</summary>
		public bool SelectedTeethOnly;
		///<summary>Enum:OrionStatus Which orion statuses to show. Will be zero if not orion.</summary>
		public OrionStatus OrionStatusFlags;
		///<summary>Enum:ChartViewDates All,Today,Yesterday,ThisYear,LastYear</summary>
		public ChartViewDates DatesShowing;
		///<summary>set true to show treatment plan controls in chart module.</summary>
		public bool IsTpCharting;

		public ChartView Copy() {
			return (ChartView)this.MemberwiseClone();
		}	
	}

	
	[Flags]
	public enum ChartViewObjs {
		///<summary>0- None</summary>
		None=0,
		///<summary>1- Appointments</summary>
		Appointments=1,
		///<summary>2- Comm Log</summary>
		CommLog=2,
		///<summary>4- Comm Log Family</summary>
		CommLogFamily=4,
		///<summary>8- Tasks</summary>
		Tasks=8,
		///<summary>16- Email</summary>
		Email=16,
		///<summary>32- Lab Cases</summary>
		LabCases=32,
		///<summary>64- Rx</summary>
		Rx=64,
		///<summary>128- Sheets</summary>
		Sheets=128,
		///<summary>256- Comm Log Super Family</summary>
		CommLogSuperFamily=256,
		///<summary>511- All</summary>
		All=511
	}

	
	[Flags]
	public enum ChartViewProcStat {
		///<summary>0- None.</summary>
		None=0,
		///<summary>1- Treatment Plan.</summary>
		TP=1,
		///<summary>2- Complete.</summary>
		C=2,
		///<summary>4- Existing Current Provider.</summary>
		EC=4,
		///<summary>8- Existing Other Provider.</summary>
		EO=8,
		///<summary>16- Referred Out.</summary>
		R=16,
		///<summary>32- Deleted.</summary>
		D=32,
		///<summary>64- Condition.</summary>
		Cn=64,
		///<summary>127- All.</summary>
		All=127
	}

	
	public enum ChartViewDates{
		/// <summary>0- All</summary>
		All=0,
		/// <summary>1- Today</summary>
		Today=1,
		/// <summary>2- Yesterday</summary>
		Yesterday=2,
		/// <summary>3- This Year</summary>
		ThisYear=3,
		/// <summary>4- Last Year</summary>
		LastYear=4
	}

	
	[Flags]
	public enum OrionStatus {
		///<summary>0- None.  While a normal orion proc would never have this status2, it is still needed for flags in ChartViews.  And it's also possible that a status2 slipped through the cracks and was not assigned, leaving it with this value.</summary>
		None=0,
		///<summary>1– Treatment planned</summary>
		TP=1,
		///<summary>2– Completed</summary>
		C=2,
		///<summary>4– Existing prior to incarceration</summary>
		E=4,
		///<summary>8– Refused treatment</summary>
		R=8,
		///<summary>16– Referred out to specialist</summary>
		RO=16,
		///<summary>32– Completed by specialist</summary>
		CS=32,
		///<summary>64– Completed by registry</summary>
		CR=64,
		///<summary>128- Cancelled, tx plan changed</summary>
		CA_Tx=128,
		///<summary>256- Cancelled, eligible parole</summary>
		CA_EPRD=256,
		///<summary>512- Cancelled, parole/discharge</summary>
		CA_PD=512,
		///<summary>1024– Suspended, unacceptable plaque</summary>
		S=1024,
		///<summary>2048- Stop clock, multi visit</summary>
		ST=2048,
		///<summary>4096– Watch</summary>
		W=4096,
		///<summary>8192– Alternative</summary>
		A=8192
	}
}
