using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

///<summary>If a number is entered in this table, then any incoming text message will not be entered into the database.</summary>
[Serializable]
public class SmsBlockPhone:TableBase {
	///<summary>Primary key.</summary>
	[CrudColumn(IsPriKey=true)]
	public long SmsBlockPhoneNum;
	///<summary>The phone number to be blocked.</summary>
	public string BlockWirelessNumber;

		
	public SmsBlockPhone Copy() {
		return (SmsBlockPhone)MemberwiseClone();
	}
}