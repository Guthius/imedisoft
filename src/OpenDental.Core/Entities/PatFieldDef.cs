using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

/// <summary>These are the definitions for the custom patient fields added and managed by the user.</summary>
[Serializable]
[CrudTable(IsSynchable=true)]
public class PatFieldDef:TableBase {
	///<summary>Primary key.</summary>
	[CrudColumn(IsPriKey=true)]
	public long PatFieldDefNum;
	///<summary>This is treated as the key. The name of the field that the user will be allowed to fill in the patient info window.</summary>
	public string FieldName;
	///<summary>Enum:PatFieldType Text=0,PickList=1,Date=2,Checkbox=3,Currency=4</summary>
	public PatFieldType FieldType;
	///<summary>Deprecated. Use patfieldpickitem.</summary>
	[CrudColumn(SpecialType=CrudSpecialColType.IsText)]
	public string PickList;
	//<summary>Enum:PatFieldMapping Certain reports such as Medicaid make use of patient fields that are explicitly mapped.</summary>
	//public PatFieldMapping FieldMapping;
		
	public int ItemOrder;
	///<summary>Hides this PatField for any patient where it's currently blank. If already in use by a patient, then it still shows.</summary>
	public bool IsHidden;

		
	public PatFieldDef Copy() {
		return (PatFieldDef)MemberwiseClone();
	}
}

	
public enum PatFieldType {
	///<summary>0</summary>
	Text = 0,
	///<summary>1</summary>
	PickList = 1,
	///<summary>2-Stored in db as entered, already localized.  For example, it could be 2/04/11, 2/4/11, 2/4/2011, or any other variant.  This makes it harder to create queries that filter by date, but easier to display dates as part of results.</summary>
	Date = 2,
	///<summary>3-If checked, value stored as "1".  If unchecked, row deleted.</summary>
	Checkbox = 3,
	///<summary>4-Numbers only.</summary>
	Currency = 4,
	///<summary>5 - DEPRECATED. (Only used 16.3.1, deprecated by 16.3.4)</summary>
	InCaseOfEmergency = 5
}

//public enum PatFieldMapping{
//<summary>0</summary>
//None,
//<summary>1</summary>
//IncomeForPoverty	
//}