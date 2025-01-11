using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;
using Newtonsoft.Json;
using Microsoft.Win32;

namespace OpenDental {
	
	public partial class FrmQueryMonitorFilterEdit : FrmODBase {
		public QueryFilter QueryFilterCur;

		public FrmQueryMonitorFilterEdit() {
			InitializeComponent();
			Load+=Frm_Load;
			PreviewKeyDown+=FrmQueryMonitorFilterEdit_PreviewKeyDown;
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			textFilterText.Text=QueryFilterCur.FilterText;
			List<string> listGroupNames=QueryFilters.GetAll().Select(x=>x.GroupName).Distinct().ToList();
			for(int i=0;i<listGroupNames.Count;i++){
				comboGroupName.Items.Add(listGroupNames[i]);
				if(listGroupNames[i]==QueryFilterCur.GroupName){
					comboGroupName.SelectedIndex=i;
				}
			}
		}

		private void FrmQueryMonitorFilterEdit_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)){
				butSave_Click(this,new EventArgs());
			}
			if(butDelete.IsAltKey(Key.D,e)){
				butDelete_Click(this,new EventArgs());
			}
		}

		private void butDelete_Click(object sender,EventArgs e) {
			if(QueryFilterCur.IsNew){
				IsDialogCancel=true;
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Delete this filter?")){
				return;
			}
			QueryFilters.Delete(QueryFilterCur.QueryFilterNum);
			IsDialogOK=true;
		}

		private void butSave_Click(object sender, EventArgs e) {
			QueryFilterCur.FilterText=textFilterText.Text;
			QueryFilterCur.GroupName=comboGroupName.GetSelected<string>();
			if(QueryFilterCur.IsNew){
				QueryFilters.Insert(QueryFilterCur);
			}
			else{
				QueryFilters.Update(QueryFilterCur);
			}
			IsDialogOK=true;
		}
	}
}