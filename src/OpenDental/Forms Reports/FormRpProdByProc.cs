using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;
using CodeBase;
using System.Linq;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDental{

	public partial class FormRpProdByProc : FormODBase {
		private List<ClinicDto> _listClinics;
		private List<long> _listClinicNums;
		private List<long> _listProvNums;
		private List<Provider> _listProviders;

		
		public FormRpProdByProc(){
			InitializeComponent();
			InitializeLayoutManager();
 			Lan.F(this);
		}

		private void FormDailySummary_Load(object sender, System.EventArgs e) {
			_listProviders=Providers.GetListReports();
			date1.SelectionStart=DateTime.Today;
			date2.SelectionStart=DateTime.Today;
			if(!Security.IsAuthorized(EnumPermType.ReportDailyAllProviders,true)) {
				//They either have permission or have a provider at this point.  If they don't have permission they must have a provider.
				_listProviders=_listProviders.FindAll(x => x.ProvNum==Security.CurUser.ProvNum);
				checkAllProv.Checked=false;
				checkAllProv.Enabled=false;
			}
			listProv.Items.AddList(_listProviders,x => x.GetLongDesc());
			if(checkAllProv.Enabled==false && _listProviders.Count>0) {
				listProv.SetSelected(0);
			}
			if(!true) {
				listClin.Visible=false;
				labelClin.Visible=false;
				checkAllClin.Visible=false;
			}
			else {
				_listClinics=Clinics.GetForUserod(Security.CurUser);
				if(!Security.CurUser.ClinicIsRestricted) {
					listClin.Items.Add(Lan.g(this,"Unassigned"));
					listClin.SetSelected(0);
				}
				for(int i=0;i<_listClinics.Count;i++) {
					listClin.Items.Add(_listClinics[i].Abbr);
					if(Clinics.ClinicNum==0) {
						listClin.SetSelected(listClin.Items.Count-1);
						checkAllClin.Checked=true;
					}
					if(_listClinics[i].Id==Clinics.ClinicNum) {
						listClin.SelectedIndices.Clear();
						listClin.SetSelected(listClin.Items.Count-1);
					}
				}
			}
		}

		private void checkAllProv_Click(object sender,EventArgs e) {
			if(checkAllProv.Checked) {
				listProv.ClearSelected();
			}
		}

		private void listProv_Click(object sender,EventArgs e) {
			if(listProv.SelectedIndices.Count>0) {
				checkAllProv.Checked=false;
			}
		}

		private void checkAllClin_Click(object sender,EventArgs e) {
			if(checkAllClin.Checked) {
				listClin.SetAll(true);
			}
			else {
				listClin.ClearSelected();
			}
		}

		private void listClin_Click(object sender,EventArgs e) {
			if(listClin.SelectedIndices.Count>0) {
				checkAllClin.Checked=false;
			}
		}

		private void butOK_Click(object sender,System.EventArgs e) {
			if(date2.SelectionStart<date1.SelectionStart) {
				MsgBox.Show(this,"End date cannot be before start date.");
				return;
			}
			if(!checkAllProv.Checked && listProv.SelectedIndices.Count==0) {
				MsgBox.Show(this,"At least one provider must be selected.");
				return;
			}
			if(true) {
				if(!checkAllClin.Checked && listClin.SelectedIndices.Count==0) {
					MsgBox.Show(this,"At least one clinic must be selected.");
					return;
				}
			}
			_listProvNums=new List<long>();
			_listClinicNums=new List<long>();
			for(int i=0;i<listProv.SelectedIndices.Count;i++) {
				_listProvNums.Add(_listProviders[listProv.SelectedIndices[i]].ProvNum);
			}
			if(true) {
				for(int i=0;i<listClin.SelectedIndices.Count;i++) {
					if(Security.CurUser.ClinicIsRestricted) {
						_listClinicNums.Add(_listClinics[listClin.SelectedIndices[i]].Id);//we know that the list is a 1:1 to _listClinics
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							_listClinicNums.Add(0);
						}
						else {
							_listClinicNums.Add(_listClinics[listClin.SelectedIndices[i]-1].Id);//Minus 1 from the selected index
						}
					}
				}
				if(checkAllClin.Checked) {//All Clinics selected; add all visible or hidden unrestricted clinics to the list
					_listClinicNums=_listClinicNums.Union(Clinics.GetAllForUserod(Security.CurUser).Select(x => x.Id)).ToList();
				}
			}
			CreateGrouped();
		}

		private void CreateGrouped() {
			ReportComplex report=new ReportComplex(true,false);
			DataTable table=RpProcSheet.GetGroupedTable(date1.SelectionStart,date2.SelectionStart,_listProvNums,_listClinicNums,textCode.Text,checkAllProv.Checked);
			string subtitleProvs=ConstructProviderSubtitle();
			string subtitleClinics=ConstructClinicSubtitle();
			Font font=new Font("Tahoma",9);
			Font fontBold=new Font("Tahoma",9,FontStyle.Bold);
			Font fontTitle=new Font("Tahoma",17,FontStyle.Bold);
			Font fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
			report.ReportName=Lan.g(this,"Procedures By Procedure Code");
			report.AddTitle("Title",Lan.g(this,"Procedures By Procedure Code"),fontTitle);
			report.AddSubTitle("Practice Title",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
			report.AddSubTitle("Dates of Report",date1.SelectionStart.ToString("d")+" - "+date2.SelectionStart.ToString("d"),fontSubTitle);
			report.AddSubTitle("Providers",subtitleProvs,fontSubTitle);
			if(true) {
				report.AddSubTitle("Clinics",subtitleClinics,fontSubTitle);
			}
			QueryObject query=report.AddQuery(table,Lan.g(this,"Date")+": "+DateTime.Today.ToString("d"));
			query.AddColumn(Lan.g(this,"Category"),150,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Code"),130,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Description"),140,FieldValueType.String,font);
			query.AddColumn(Lan.g(this,"Quantity"),60,FieldValueType.Integer,font);
			query.GetColumnDetail(Lan.g(this,"Quantity")).ContentAlignment=ContentAlignment.MiddleRight;
			query.AddColumn(Lan.g(this,"Average Fee"),110,FieldValueType.String,font);
			query.GetColumnDetail(Lan.g(this,"Average Fee")).ContentAlignment=ContentAlignment.MiddleRight;
			query.AddColumn(Lan.g(this,"Total Fees"),110,FieldValueType.Number,font);
			report.AddPageNum(font);
			if(!report.SubmitQueries()) {
				return;
			}
			using FormReportComplex FormR=new FormReportComplex(report);
			FormR.ShowDialog();
			DialogResult=DialogResult.OK;
		}

		///<summary>Returns 'All Providers' or comma separated string of clinics providers selected.</summary>
		private string ConstructProviderSubtitle() {
			string subtitleProvs="";
			if(checkAllProv.Checked) {
				return Lan.g(this,"All Providers");
			}
			for(int i=0;i<listProv.SelectedIndices.Count;i++) {
				if(i>0) {
					subtitleProvs+=", ";
				}
				subtitleProvs+=_listProviders[listProv.SelectedIndices[i]].Abbr;
			}
			return subtitleProvs;
		}

		///<summary>Returns 'All Clinics' or comma separated string of clinics selected.</summary>
		private string ConstructClinicSubtitle() {
			string subtitleClinics="";
			if(!true) {
				return subtitleClinics;
			}
			if(checkAllClin.Checked) {
				return Lan.g(this,"All Clinics (includes hidden)");
			}
			for(int i=0;i<listClin.SelectedIndices.Count;i++) {
				if(i>0) {
					subtitleClinics+=", ";
				}
				if(Security.CurUser.ClinicIsRestricted) {
					subtitleClinics+=_listClinics[listClin.SelectedIndices[i]].Abbr;
				}
				else {
					if(listClin.SelectedIndices[i]==0) {
						subtitleClinics+=Lan.g(this,"Unassigned");
					}
					else {
						subtitleClinics+=_listClinics[listClin.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index to account for 'Unassigned' 
					}
				}
			}
			return subtitleClinics;
		}
		
	}
}