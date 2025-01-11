using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDentBusiness {

/*
PrefInf rows are managed by our writers, using the Manual Publisher (MP).
They are a shorter version of PrefInfo, which the class used in MP.
These represent the hover info windows that the user can see in the Prefs setup UI.
The MP serializes this and places it in our project as the resource PrefInfos.xml.
This xml is then baked into OD and consumed by our UI.
We frequently need to update the hover text, so here are instructions how.
Jordan will always be involved and must approve every change made by engineers.
You will create a small spreadsheet that will eventually be submitted to the writers.
It must have these exact columns in this order and with this spelling:
Category, ControlName, ControlText, PrefName, Details Old, Details New
Category example: AccountAdjustments.
ControlName example: labelSalesTaxAdjType.
	These must be named well. Don't leave it like label27.
	Common to also see checkboxes, like checkAutomateSales.
	If you change an existing control name, bring it to the attention of the writers by using red text.
ControlText: This is the text of the lable or checkbox, as seen in the UI.
	Either Control Name or Control Text is required. If both are specified, Control Name takes priority.
PrefName example: PayPlanAdjType. The exact name as in our enum.
Details Old: It's too hard to do this if it's long, so Jordan can paste it in while connected.
Details New: Ordinary html <a href> link tags can be included, but no bold, etc. 
	You must include target='_blank' on every link because they will need to open in a browser.
	Include <br/> just before every carriage return.
	Capitalization and puctuation are unique.
	For small clauses that are incomplete sentences, to not capitalize or use a period.
	Complete sentences or multiple sentences get capital and period.
	Two short clauses are frequently chained together. Example: for sales tax, no 0
The spreadsheet will typicall only have one or a few rows in it.
Once the spreadsheet is ready, it will be reviewed as part of the normal review process with Jordan.
After review, it will be submitted to the writers at techwriting@opendental.com. Cc Jordan.
*/

	///<summary>See notes above.</summary>
	[Serializable]
	public class PrefInf{
		///<summary>Can be blank.</summary>
		public string PrefName;
		///<summary>String version of EnumPrefInfoCategory. Example: ApptGeneral. Can be blank if ControlText is being used.</summary>
		public string Category;
		///<summary>The name of the control in the UI. Can be blank if ControlText is being used.</summary>
		public string ControlName;
		///<summary>The text that shows in the UI. Can be blank if ControlName is being used.</summary>
		public string ControlText;
		///<summary>TEXT is 65k char capacity. This could be empty, one sentence, or many paragraphs.  It will be loaded as a chunk of html, so it can contain ordinary html link tags.  We won't allow inline styles, bold, etc for now, but it's possible later.</summary>
		public string Details;
		public int WidthWindow;
		public int HeightWindow;
	}
	
	public enum EnumPrefInfoCategory{
		///<summary>0- This value is never stored in db</summary>
		None,
		MainWindowGeneral,
		MainWindowMisc,
		ApptGeneral,
		ApptAppearance,
		FamilyGeneral,
		FamilyInsurance,
		AccountGeneral,
		AccountAdjustments,
		AccountClaimReceive,
		AccountClaimSend,
		AccountPayments,
		AccountRecAndRepCharges,
		TreatPlanGeneral,
		TreatPlanFreqLimit,
		ChartGeneral,
		ChartProcedures,
		ImagingGeneral,
		ManageGeneral,
		ManageBilling,
		Ortho,
		EnterpriseGeneral,
		EnterpriseAccount,
		EnterpriseAppts,
		EnterpriseFamily,
		EnterpriseManage,
		EnterpriseReports,
		ServerConnections,
		ExperimentalPrefs
	}
}
