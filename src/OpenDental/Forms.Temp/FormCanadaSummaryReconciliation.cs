using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental;

public partial class FormCanadaSummaryReconciliation:FormODBase {

	private List<Carrier> _listCarriers= [];
	private List<Provider> _listProviders;
	private List<CanadianNetwork> _listCanadianNetworks;

	public FormCanadaSummaryReconciliation() {
		InitializeComponent();
	}

	private void FormCanadaPaymentReconciliation_Load(object sender,EventArgs e) {
		_listCanadianNetworks=CanadianNetworks.GetDeepCopy();
		for(var i=0;i<_listCanadianNetworks.Count;i++) {
			listNetworks.Items.Add(_listCanadianNetworks[i].Abbrev+" - "+_listCanadianNetworks[i].Descript);
		}
		_listCarriers=Carriers.GetWhere(x => x.CDAnetVersion!="02" &&//This transaction does not exist in version 02.
		                                     (x.CanadianSupportedTypes & CanSupTransTypes.RequestForSummaryReconciliation_05)==CanSupTransTypes.RequestForSummaryReconciliation_05);
		for(var i = 0;i<_listCarriers.Count;++i) {
			listCarriers.Items.Add(_listCarriers[i].CarrierName);
		}
		var defaultProvNum=PrefC.GetLong(PrefName.PracticeDefaultProv);
		_listProviders=Providers.GetDeepCopy(true);
		for(var i=0;i<_listProviders.Count;i++) {
			listTreatingProvider.Items.Add(_listProviders[i].Abbr);
			if(_listProviders[i].ProvNum==defaultProvNum) {
				listTreatingProvider.SelectedIndex=i;
			}
		}
		textDateReconciliation.Text=DateTime.Today.ToShortDateString();
	}

	private void checkGetForAllCarriers_Click(object sender,EventArgs e) {
		groupCarrierOrNetwork.Enabled=!checkGetForAllCarriers.Checked;
	}

	private void listCarriers_Click(object sender,EventArgs e) {
		listNetworks.SelectedIndex=-1;
	}

	private void listNetwork_Click(object sender,EventArgs e) {
		listCarriers.SelectedIndex=-1;
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(!checkGetForAllCarriers.Checked) {
			if(listCarriers.SelectedIndex<0 && listNetworks.SelectedIndex<0) {
				MsgBox.Show(this,"You must first choose one carrier or one network.");
				return;
			}
		}
		if(listTreatingProvider.SelectedIndex<0) {
			MsgBox.Show(this,"You must first choose a treating provider.");
			return;
		}
		DateTime reconciliationDate;
		try {
			reconciliationDate=DateTime.Parse(textDateReconciliation.Text).Date;
		}
		catch {
			MsgBox.Show(this,"Reconciliation date invalid.");
			return;
		}
		Cursor=Cursors.WaitCursor;
		try {
			if(checkGetForAllCarriers.Checked) {
				var carrier=new Carrier();
				carrier.CDAnetVersion="04";
				carrier.ElectID="999999";//The whole ITRANS network.
				carrier.CanadianEncryptionMethod=1;//No encryption.
				var clearinghouseHq=Canadian.GetCanadianClearinghouseHq(carrier);
				var clearinghouseClin=Clearinghouses.OverrideFields(clearinghouseHq,Clinics.ClinicNum);
				CanadianOutput.GetSummaryReconciliation(clearinghouseClin,carrier,null,
					_listProviders[listTreatingProvider.SelectedIndex],reconciliationDate,false,FormCCDPrint.PrintCCD);
			}
			else {
				if(listCarriers.SelectedIndex>=0) {
					var carrier=_listCarriers[listCarriers.SelectedIndex];
					var clearinghouseHq=Canadian.GetCanadianClearinghouseHq(carrier);
					var clearinghouseClin=Clearinghouses.OverrideFields(clearinghouseHq,Clinics.ClinicNum);
					CanadianOutput.GetSummaryReconciliation(clearinghouseClin,carrier,null,
						_listProviders[listTreatingProvider.SelectedIndex],reconciliationDate,false,FormCCDPrint.PrintCCD);
				}
				else {
					var clearinghouseHq=Canadian.GetCanadianClearinghouseHq(null);
					var clearinghouseClin=Clearinghouses.OverrideFields(clearinghouseHq,Clinics.ClinicNum);
					CanadianOutput.GetSummaryReconciliation(clearinghouseClin,null,_listCanadianNetworks[listNetworks.SelectedIndex],
						_listProviders[listTreatingProvider.SelectedIndex],reconciliationDate,false,FormCCDPrint.PrintCCD);
				}
			}
			Cursor=Cursors.Default;
			MsgBox.Show(this,"Done.");
		}
		catch(Exception ex) {
			Cursor=Cursors.Default;
			ODMessageBox.Show(Lan.g(this,"Request failed: ")+ex.Message);
		}			
		DialogResult=DialogResult.OK;
	}

}