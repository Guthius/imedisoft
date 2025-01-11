using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental {
	public partial class FormProvAdditional:FormODBase {
		/// <summary>This is a list of providerclinic rows that were given to this form, containing any modifications that were made while in FormProvAdditional.</summary>
		public List<ProviderClinic> ListProviderClinicsOut=new List<ProviderClinic>();
		private Provider _provider;
		private List<ProviderClinic> _listProviderClinics;
		private ProviderClinic ProviderClinicDefault;

		public FormProvAdditional(List<ProviderClinic> listProviderClinics,Provider provider) {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
			_listProviderClinics=listProviderClinics.Select(x => x.Copy()).ToList();
			_provider=provider;
		}

		private void FormProvAdditional_Load(object sender,EventArgs e) {
			FillGrid();
		}

		private void FillGrid() {
			Cursor=Cursors.WaitCursor;
			gridProvProperties.BeginUpdate();
			gridProvProperties.Columns.Clear();
			GridColumn col=new GridColumn(Lan.g("TableProviderProperties","Clinic"),120);
			gridProvProperties.Columns.Add(col);
			col=new GridColumn(Lan.g("TableProviderProperties","DEA Num"),120,true);
			gridProvProperties.Columns.Add(col);
			col=new GridColumn(Lan.g("TableProviderProperties","State License Num"),120,true);
			gridProvProperties.Columns.Add(col);
			col=new GridColumn(Lan.g("TableProviderProperties","State Rx ID"),120,true);
			gridProvProperties.Columns.Add(col);
			col=new GridColumn(Lan.g("TableProviderProperties","State Where Licensed"),120,true);
			gridProvProperties.Columns.Add(col);
			gridProvProperties.ListGridRows.Clear();
			GridRow row;
			ProviderClinicDefault=_listProviderClinics.Find(x => x.ClinicNum==0);
			//Didn't have an HQ row
			if(ProviderClinicDefault==null) {//Doesn't exist in list
				ProviderClinicDefault=ProviderClinics.GetOne(_provider.ProvNum,0);
				if(ProviderClinicDefault==null) {//Doesn't exist in database
					ProviderClinicDefault=new ProviderClinic {
						ProvNum=_provider.ProvNum,
						ClinicNum=0,
						DEANum=_provider.DEANum,
						StateLicense=_provider.StateLicense,
						StateRxID=_provider.StateRxID,
						StateWhereLicensed=_provider.StateWhereLicensed,
						CareCreditMerchantId="",
					};
				}
				_listProviderClinics.Add(ProviderClinicDefault);//If not in list, add to list.
			}
			row=new GridRow();
			row.Cells.Add("Default");
			row.Cells.Add(ProviderClinicDefault.DEANum);
			row.Cells.Add(ProviderClinicDefault.StateLicense);
			row.Cells.Add(ProviderClinicDefault.StateRxID);
			row.Cells.Add(ProviderClinicDefault.StateWhereLicensed);
			row.Tag=ProviderClinicDefault;
			gridProvProperties.ListGridRows.Add(row);
			if(true) {
				var listClinics=Clinics.GetForUserod(Security.CurUser);
				for(int i=0;i<listClinics.Count;i++) {
					row=new GridRow();
					ProviderClinic providerClinic=_listProviderClinics.Find(x => x.ClinicNum == listClinics[i].Id);
					//Doesn't exist in Db, create a new one
					if(providerClinic==null) {
						providerClinic=new ProviderClinic();
						providerClinic.ProvNum=_provider.ProvNum;
						providerClinic.ClinicNum=listClinics[i].Id;
						providerClinic.CareCreditMerchantId="";
						_listProviderClinics.Add(providerClinic);
					}
					row.Cells.Add(listClinics[i].Abbr);
					row.Cells.Add(providerClinic.DEANum);
					row.Cells.Add(providerClinic.StateLicense);
					row.Cells.Add(providerClinic.StateRxID);
					row.Cells.Add(providerClinic.StateWhereLicensed);
					row.Tag=providerClinic;
					gridProvProperties.ListGridRows.Add(row);
				}
			}
			gridProvProperties.EndUpdate();
			Cursor=Cursors.Default;
		}

		private void gridProvProperties_CellLeave(object sender,ODGridClickEventArgs e) {
			GridRow rowSelected=gridProvProperties.SelectedGridRows.First();
			if(rowSelected==null) {
				return;
			}
			ProviderClinic providerClinic=(ProviderClinic)rowSelected.Tag;
			string strNewValue=PIn.String(rowSelected.Cells[e.Col].Text);
			if(e.Col==1) {
				providerClinic.DEANum=strNewValue;
			}
			else if(e.Col==2) {
				providerClinic.StateLicense=strNewValue;
			}
			else if(e.Col==3) {
				providerClinic.StateRxID=strNewValue;
			}
			else if(e.Col==4) {
				providerClinic.StateWhereLicensed=strNewValue;
			}
		}

		///<summary>Returns true if provClinic is empty. ProvClinic is considered empty if the DEANum,StateLicense,StateRxID, StateWereLicensed,
		///CareCreditMerchantId are empty.</summary>
		private bool IsEmpty(ProviderClinic providerClinic) {
			return (providerClinic!=null && string.IsNullOrEmpty(providerClinic.DEANum)
				&& string.IsNullOrEmpty(providerClinic.StateLicense)
				&& string.IsNullOrEmpty(providerClinic.StateRxID)
				&& string.IsNullOrEmpty(providerClinic.StateWhereLicensed)
				&& string.IsNullOrEmpty(providerClinic.CareCreditMerchantId));
		}

		///<summary>Returns true if the provClinic was modified. Otherwise, returns false.</summary>
		private bool IsProvClinicModified(ProviderClinic providerClinic) {
			if(providerClinic==null) {
				return false;
			}
			//No override added/edited or override was removed
			if(providerClinic!=ProviderClinicDefault && IsEmpty(providerClinic)){
				return false;
			}
			return true;
		}
		
		private void butOK_Click(object sender,EventArgs e) {
			List<ProviderClinic> listProviderClinics=gridProvProperties.ListGridRows.Select(x => (ProviderClinic)x.Tag).ToList();
			ListProviderClinicsOut=new List<ProviderClinic>();
			for(int i=0; i<listProviderClinics.Count;i++) {
				//always add the default _provClinicDefault
				if(!IsProvClinicModified(listProviderClinics[i])) {
					continue;//No overrided added/edited or override were removed
				}
				ListProviderClinicsOut.Add(listProviderClinics[i]);
			}
			DialogResult=DialogResult.OK;
		}

	}
}