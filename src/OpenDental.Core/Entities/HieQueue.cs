using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

///<summary>Health Information Exchange queue. This table stores pending patients that need to be considered for an auto-generated CCD.</summary>
[Serializable]
public class HieQueue:TableBase {
	///<summary>Primary key.</summary>
	[CrudColumn(IsPriKey=true)]
	public long HieQueueNum;
	///<summary>FK to patient.PatNum.</summary>
	public long PatNum;

	public HieQueue() {

	}

	public HieQueue(long patNum) {
		PatNum=patNum;
	}
}