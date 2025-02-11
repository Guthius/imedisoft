using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Linq;
using OpenDental.UI;
using System.IO;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.Logic;

namespace OpenDental;

public partial class FormRpInsPayPlansPastDue:FormODBase {
	private bool _headingPrinted;
	private int _pagesPrinted;
	private int _headingPrintH;
	private List<PayPlanExtended> _listPayPlanExtended;
	private List<ProviderDto> _listProviders;
	//private List<Clinic> _listClinics;

	public FormRpInsPayPlansPastDue() {
		InitializeComponent();
	}

	private void FormRpInsPayPlansPastDue_Load(object sender,EventArgs e) {
		SetFilterControlsAndAction(() => FillGrid(),
			(int)TimeSpan.FromSeconds(0.3).TotalMilliseconds,
			textDaysPastDue);
		FillProvs();
		if(!LoadData()) {
			DialogResult=DialogResult.Cancel;
			return;
		}
		FillGrid();
	}

	private void FillProvs() {
		_listProviders=Providers.GetListReports();
		comboProvs.IncludeAll=true;
		comboProvs.Items.AddProvsFull(_listProviders);
		comboProvs.IsAllSelected=true;
	}

	///<summary>Retrieves data and uses them to create new PayPlanExtended objects.
	///Heavy lifting (db calls and double loops) done here once upon load.  This also gets called if the user clicks "Refresh Data".</summary>
	private bool LoadData() {
		List<PayPlan> listPayPlans;
		List<PayPlanCharge> listPayPlanCharges;
		List<ClaimProc> listPayPlanClaimProcs;
		List<Patient> listPatients;
		List<InsPlan> listInsPlans;
		listPayPlans=PayPlans.GetAllOpenInsPayPlans();
		if(listPayPlans.Count==0) {
			MsgBox.Show(this,"There are no insurance payment plans past due.");
			return false;
		}
		listPayPlanCharges=PayPlanCharges.GetForPayPlans(listPayPlans.Select(x => x.PayPlanNum).ToList()).Where(x => x.ChargeType == PayPlanChargeType.Debit).ToList();
		listPayPlanClaimProcs=ClaimProcs.GetForPayPlans(listPayPlans.Select(x => x.PayPlanNum).ToList()
			, [ClaimProcStatus.Received, ClaimProcStatus.Supplemental]);
		listPatients=Patients.GetLimForPats(listPayPlans.Select(x => x.PatNum).ToList());
		listInsPlans=InsPlans.GetPlans(listPayPlans.Select(x => x.PlanNum).ToList());
		_listPayPlanExtended= [];
		foreach(var plan in listPayPlans) {
			//for each payplan, create a PayPlanExtended object which contains all of the payment plan's information and it's charges.
			//pass in the plan, the list of associated charges, and the list of associated claimprocs (payments).
			_listPayPlanExtended.Add(new PayPlanExtended(plan,
				listPatients.FirstOrDefault(x => x.PatNum == plan.PatNum),
				listPayPlanCharges.Where(x => x.PayPlanNum == plan.PayPlanNum).ToList(),
				listPayPlanClaimProcs.Where(x => x.PayPlanNum == plan.PayPlanNum).ToList(),
				listInsPlans.FirstOrDefault(x => x.PlanNum == plan.PlanNum)));
		}
		return true;
	}

	///<summary>Actually fill the grid with the data. Filtering based on the user-defined criteria gets done here.</summary>
	private void FillGrid() {
		//get the user-entered filter values.
		var daysPassedFilter=SIn.Int(textDaysPastDue.Text,false); //returns 0 if exceptions are thrown.
		var listProvNums=comboProvs.GetSelectedProvNums();
		//fill the grid
		gridMain.BeginUpdate();
		//columns
		gridMain.Columns.Clear();
		var col = new GridColumn(Lan.g("TableInsPayPlanPastDue","Patient"),180);
		gridMain.Columns.Add(col);
		col = new GridColumn(Lan.g("TableInsPayPlanPastDue","DateLastPmt"),90);
		gridMain.Columns.Add(col);
		col = new GridColumn(Lan.g("TableInsPayPlanPastDue","#Overdue"),75);
		gridMain.Columns.Add(col);
		col = new GridColumn(Lan.g("TableInsPayPlanPastDue","AmtOverdue"),90);
		gridMain.Columns.Add(col);
		col = new GridColumn(Lan.g("TableInsPayPlanPastDue","DaysOverdue"),90);
		gridMain.Columns.Add(col);
		col = new GridColumn(Lan.g("TableInsPayPlanPastDue","CarrierName/Phone"),180){ IsWidthDynamic=true };
		gridMain.Columns.Add(col);
		//rows
		gridMain.ListGridRows.Clear();
		GridRow row;
		foreach(var payPlanCur in _listPayPlanExtended) {
			if(daysPassedFilter > payPlanCur.DaysOverdue || payPlanCur.DaysOverdue < 1) {
				continue;
			}
			if(!listProvNums.Contains(payPlanCur.ListPayPlanCharges[0].ProvNum)) {
				continue;
			}
			if(true 
			   && !comboClinics.IsAllSelected
			   && !comboClinics.ListClinicNumsSelected.Contains(payPlanCur.ListPayPlanCharges[0].ClinicNum))
			{
				continue;
			}
			row = new GridRow();
			var patName =payPlanCur.PatientCur.LName + ", " + payPlanCur.PatientCur.FName;
			var carrierNamePhone = payPlanCur.CarrierCur.CarrierName+"\r\n"+Lan.g("TableInsPayPlanPastDue","Ph:")+" "+payPlanCur.CarrierCur.Phone;
			row.Cells.Add(patName);
			row.Cells.Add(payPlanCur.DateLastPayment.ToShortDateString());
			row.Cells.Add(payPlanCur.NumChargesOverdue.ToString());
			row.Cells.Add(payPlanCur.AmtOverdue.ToString("f"));
			row.Cells.Add(payPlanCur.DaysOverdue.ToString());
			row.Cells.Add(carrierNamePhone);
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

	private void ComboProvs_SelectionChangeCommitted(object sender, EventArgs e){
		FillGrid();
	}

	private void ComboClinics_SelectionChangeCommitted(object sender, EventArgs e){
		FillGrid();
	}

	//Copied from FormRpOutstandingIns.cs
	private void butPrint_Click(object sender,EventArgs e) {
		_pagesPrinted=0;
		_headingPrinted=false;
		PrinterL.TryPrintOrDebugRpPreview(pd_PrintPage,Lan.g(this,"Outstanding insurance report printed"));
	}

	//Copied from FormRpOutstandingIns.cs
	private void pd_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
		var bounds=e.MarginBounds;
		//new Rectangle(50,40,800,1035);//Some printers can handle up to 1042
		var g=e.Graphics;
		string text;
		var headingFont=new Font("Arial",13,FontStyle.Bold);
		var subHeadingFont=new Font("Arial",10,FontStyle.Bold);
		var yPos=bounds.Top;
		var center=bounds.X+bounds.Width/2;
		#region printHeading
		if(!_headingPrinted) {
			text=Lan.g(this,"Outstanding Insurance Payment Plans");
			g.DrawString(text,headingFont,Brushes.Black,center-g.MeasureString(text,headingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,headingFont).Height;
			if(comboProvs.IsAllSelected) {
				text="For All Providers";
			}
			else {
				text="For Providers: ";
				for(var i = 0;i<comboProvs.SelectedIndices.Count;i++) {
					if(i!=0) {
						text+=", ";
					}
					var selectedIdx=comboProvs.SelectedIndices[i];
					text+=comboProvs.Items.GetAbbrShowingAt(selectedIdx);
				}
			}
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=(int)g.MeasureString(text,headingFont).Height;
			text="For Clinics: "+comboClinics.GetStringSelectedClinics();
			g.DrawString(text,subHeadingFont,Brushes.Black,center-g.MeasureString(text,subHeadingFont).Width/2,yPos);
			yPos+=20;
			_headingPrinted=true;
			_headingPrintH=yPos;
		}
		#endregion
		yPos=gridMain.PrintPage(g,_pagesPrinted,bounds,_headingPrintH);
		_pagesPrinted++;
		if(yPos==-1) {
			e.HasMorePages=true;
		}
		else {
			e.HasMorePages=false;
			//text="Total: $"+total.ToString("F");
			//g.DrawString(text,subHeadingFont,Brushes.Black,center+gridMain.Width/2-g.MeasureString(text,subHeadingFont).Width-10,yPos);
		}
		g.Dispose();
	}

	//Copied from FormRpOutstandingIns.cs
	private void butExport_Click(object sender,System.EventArgs e) {
		var fileName=Lan.g(this,"Outstanding Insurance Payment Plans");
		var filePath=ODFileUtils.CombinePaths(Path.GetTempPath(),fileName);
		var saveFileDialog=new SaveFileDialog();
		saveFileDialog.AddExtension=true;
		saveFileDialog.FileName=fileName;
		if(!Directory.Exists(PrefC.GetString(PrefName.ExportPath))) {
			try {
				Directory.CreateDirectory(PrefC.GetString(PrefName.ExportPath));
				saveFileDialog.InitialDirectory=PrefC.GetString(PrefName.ExportPath);
			}
			catch {
				//initialDirectory will be blank
			}
		}
		else {
			saveFileDialog.InitialDirectory=PrefC.GetString(PrefName.ExportPath);
		}
		saveFileDialog.Filter="Text files(*.txt)|*.txt|Excel Files(*.xls)|*.xls|All files(*.*)|*.*";
		saveFileDialog.FilterIndex=0;
		if(saveFileDialog.ShowDialog()!=DialogResult.OK) {
			return;
		}
		filePath=saveFileDialog.FileName;
		try {
			using(var sw=new StreamWriter(filePath,false))
				//new FileStream(,FileMode.Create,FileAccess.Write,FileShare.Read)))
			{
				var line="";
				for(var i = 0;i<gridMain.Columns.Count;i++) {
					line+=gridMain.Columns[i].Heading+"\t";
				}
				sw.WriteLine(line);
				for(var i = 0;i<gridMain.ListGridRows.Count;i++) {
					line="";
					for(var j = 0;j<gridMain.Columns.Count;j++) {
						line+=gridMain.ListGridRows[i].Cells[j].Text;
						if(j<gridMain.Columns.Count-1) {
							line+="\t";
						}
					}
					sw.WriteLine(line);
				}
			}
		}
		catch {
			ODMessageBox.Show(Lan.g(this,"File in use by another program.  Close and try again."));
			return;
		}

		ODMessageBox.Show(Lan.g(this,"File created successfully"));
	}

	///<summary>Class that contains a singular payment plan and all relevant information to be displayed in the grid.
	///Pass in the payplan, patient, list of payplancharges for that payplan, list of claimprocs attached to that payplan, and insplan when callingt he constructor.</summary>
	public class PayPlanExtended {
		//passed in
		public PayPlan PayPlan;
		public List<PayPlanCharge> ListPayPlanCharges;
		public List<ClaimProc> ListClaimProcs;
		public Patient PatientCur;
		public InsPlan InsPlanCur;
		//retrieved
		public Carrier CarrierCur;
		//calculated
		public DateTime DateOldestUnpaid;
		public double AmtOverdue;
		public int NumChargesOverdue;
		public int DaysOverdue;
		public DateTime DateLastPayment;

		public PayPlanExtended(PayPlan payPlan,Patient patCur,List<PayPlanCharge> listPayPlanCharges,List<ClaimProc> listClaimProcs,InsPlan insPlan) {
			//assign passed-in values
			PayPlan=payPlan;
			if(patCur == null) {
				PatientCur=new Patient();
			}
			else {
				PatientCur=patCur;
			}
			ListPayPlanCharges=listPayPlanCharges;
			ListClaimProcs=listClaimProcs;
			if(insPlan == null) {
				InsPlanCur = new InsPlan();
			}
			else {
				InsPlanCur=insPlan;
			}
			//find carrierCur. GetCarrier uses the H List if possible.
			CarrierCur = Carriers.GetCarrier(InsPlanCur.CarrierNum);
			CalculateOverdues();
		}

		private void CalculateOverdues() {
			//calculate AmtOverdue, NumChargesOverdue, and DaysOverdue
			DateLastPayment=DateTime.MinValue;
			foreach(var claimProcCur in ListClaimProcs) {
				if(claimProcCur.DateCP > DateLastPayment) {
					DateLastPayment = claimProcCur.DateCP;
				}
				var payAmt = claimProcCur.InsPayAmt;
				foreach(var payPlanChargeCur in ListPayPlanCharges) {
					if(payAmt<=0) {
						break;
					}
					if(payAmt>=payPlanChargeCur.Interest) {
						payAmt-=payPlanChargeCur.Interest;
						payPlanChargeCur.Interest = 0;
					}
					else {
						payPlanChargeCur.Interest -= payAmt;
						payAmt=0;
					}
					if(payAmt>=payPlanChargeCur.Principal) {
						payAmt-=payPlanChargeCur.Principal;
						payPlanChargeCur.Principal = 0;
					}
					else {
						payPlanChargeCur.Principal -= payAmt;
						payAmt=0;
					}
				}
			}
			var listChargesOverdue=ListPayPlanCharges.Where(x => (x.ChargeDate < DateTime.Today) && (x.Principal + x.Interest > 0)).ToList();
			if(listChargesOverdue.Count>0) {
				AmtOverdue=listChargesOverdue.Sum(x => x.Principal + x.Interest);
				NumChargesOverdue=listChargesOverdue.Count;
				DateOldestUnpaid=listChargesOverdue.Min(x => x.ChargeDate);
				DaysOverdue=(DateTime.Today - DateOldestUnpaid).Days;
			}
			else {
				AmtOverdue=0;
				NumChargesOverdue=0;
				DaysOverdue=0;
			}
		}
	}

}