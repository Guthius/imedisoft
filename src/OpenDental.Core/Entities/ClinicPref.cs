using System;
using DataConnectionBase;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

///<summary>Used to store preferences specific to clinics.</summary>
[Serializable]
[CrudTable(IsSynchable=true)]
public class ClinicPref:TableBase{
	///<summary>Primary key.</summary>
	[CrudColumn(IsPriKey=true)]
	public long ClinicPrefNum;
	///<summary>FK to clinic.ClinicNum.</summary>
	public long ClinicNum;
	///<summary>Enum: </summary>
	[CrudColumn(SpecialType=CrudSpecialColType.EnumAsString)]
	public PrefName PrefName;
	///<summary>The stored value.</summary>
	[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
	public string ValueString;

	public ClinicPref() {
			
	}

	public ClinicPref(long clinicNum, PrefName prefName, bool valueBool) {
		ClinicNum=clinicNum;
		PrefName=prefName;
		ValueString=SOut.Bool(valueBool);
	}

	public ClinicPref(long clinicNum, PrefName prefName, string valueString) {
		ClinicNum=clinicNum;
		PrefName=prefName;
		ValueString=valueString;
	}

		
	public ClinicPref Clone() {
		return (ClinicPref)MemberwiseClone();
	}

}