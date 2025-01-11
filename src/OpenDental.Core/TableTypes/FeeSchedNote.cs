using System;
using System.Collections.Generic;

namespace OpenDentBusiness{

	///<summary>A note that is attached to a fee schedule, and can additionally be associated with a clinic too.</summary>
	[Serializable]
	[CrudTable(IsSecurityStamped=true)]
	public class FeeSchedNote:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long FeeSchedNoteNum;
		///<summary>FK to feesched.FeeSchedNum.</summary>
		public long FeeSchedNum;
		///<summary>A comma delimited list of clinic nums that the fee schedule note is for.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
		public string ClinicNums;
		///<summary>A note for a particular fee schedule. No character lim.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
		public string Note;
		///<summary>The date of this note. User is allowed to change this date.</summary>
		public DateTime DateEntry;
		///<summary>FK to userod.UserNum. Set to the user logged in when the row was inserted at SecDateEntry date and time.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.ExcludeFromUpdate)]
		public long SecUserNumEntry;
		///<summary>Timestamp automatically generated and user not allowed to change. The actual date of entry.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.DateEntry)]
		public DateTime SecDateEntry;
		///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program updates. Not user editable with the UI.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.TimeStamp)]
		public DateTime SecDateTEdit;
		///<summary>Not a db column. Holds a list of clinic nums that this fee schedule note is assigned to.</summary>
		[CrudColumn(IsNotDbColumn=true)]
		public List<long> ListClinicNums;


		
		public FeeSchedNote Copy(){
			return (FeeSchedNote)MemberwiseClone();
		}

		public FeeSchedNote(){
			ListClinicNums=new List<long>();
		}

	}
}