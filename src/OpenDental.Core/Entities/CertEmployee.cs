using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

///<summary>A certification completed by an employee on a specific date.</summary>
[Serializable]
public class CertEmployee:TableBase{
	///<summary>Primary key.</summary>
	[CrudColumn(IsPriKey=true)]
	public long CertEmployeeNum;
	///<summary>FK to cert.CertNum.</summary>
	public long CertNum;
	///<summary>FK to employee.EmployeeNum.</summary>
	public long EmployeeNum;
		
	public DateTime DateCompleted;
	///<summary>Rarely, a very short note is required.</summary>
	public string Note;
	///<summary>FK to userod.UserNum. The user who made this entry.  Usually some sort of supervisor.</summary>
	public long UserNum;

		
	public CertEmployee Copy() {
		return (CertEmployee)MemberwiseClone();
	}


}