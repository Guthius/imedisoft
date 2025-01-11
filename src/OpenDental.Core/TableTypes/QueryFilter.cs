using System;
using System.Collections;
using System.Drawing;

namespace OpenDentBusiness{
	///<summary>Each row is a query filter for the Query Monitor window. That window will exclude queries from showing and logging when they contain FilterText. This can significantly reduce the noise when looking through the queries.</summary>
	[Serializable]
	public class QueryFilter:TableBase{
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long QueryFilterNum;
		///<summary>This is a simple string instead of a FK to another small table.</summary>
		public string GroupName;
		///<summary>The text that we look for in the query monitor. Any query that contains this text will be filtered out.</summary>
		public string FilterText;
	}
}