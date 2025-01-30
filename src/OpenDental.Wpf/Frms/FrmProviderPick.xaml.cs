using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {
///<summary>Pick a provider from the list.</summary>
	public partial class FrmProviderPick:FrmODBase {
		//private bool changed;
		//private User user;
		//private DataTable table;
		///<summary>This can be set ahead of time to preselect a provider.  After closing with OK, this will have the selected provider number.</summary>
		public long ProvNumSelected;
		///<summary>Setting to true will show a none button and will allow 0 to be returned in the SelectedProvNum variable.  It will be -1 if the user cancels out of the window.</summary>
		public bool IsNoneAvailable=false;
		///<summary>Will be set to a specific list of providers passed in.  Will be null if no defined list of providers is desired.</summary>
		private List<Provider> _listProviders;
		///<summary>Will enable the checkbox that shows all non-hidden providers regardless of schedule, clinic, or what _listProviders was set to initially</summary>
		public bool IsShowAllAvailable=false;
		private FilterControlsAndAction _filterControlsAndAction;
		
		
		public FrmProviderPick(List<Provider> listProviders=null) {
			InitializeComponent();
			_listProviders=listProviders;
			_filterControlsAndAction=new FilterControlsAndAction();
			_filterControlsAndAction.AddControl(textFilter);
			_filterControlsAndAction.FuncDb=RefreshDBForGrid;
			_filterControlsAndAction.ActionComplete=FillGrid;
			Load+=FrmProviderSelect_Load;
			gridMain.CellDoubleClick+=gridMain_CellDoubleClick;
			PreviewKeyDown+=FrmProviderPick_PreviewKeyDown;
		}

		private void FrmProviderSelect_Load(object sender, System.EventArgs e) {
			Lang.F(this);
			checkShowAll.Visible=IsShowAllAvailable;
			List<Provider> listProviders=RefreshDBForGrid();
			FillGrid(listProviders);
			if(_listProviders!=null) {
				for(int i=0;i<_listProviders.Count;i++) {
					if(_listProviders[i].ProvNum==ProvNumSelected) {
						gridMain.SetSelected(i,true);
						break;
					}
				}
			}
			else if(ProvNumSelected!=0) {
				gridMain.SetSelected(Providers.GetIndex(ProvNumSelected),true);
			}
			butSelectNone.Visible=IsNoneAvailable;
			if(IsNoneAvailable) {
				//Default value for the selected provider when none is an option is always -1
				ProvNumSelected=-1;
			}
			textFilter.Focus();
		}

		private List<Provider> RefreshDBForGrid(){
			ComboBox comboBoxClass=null;
			List<Provider> listProviders;
			CheckBox checkBoxShowAll=null;
			Dispatcher.Invoke(()=>checkBoxShowAll=checkShowAll);
			if(_listProviders!=null && checkBoxShowAll.Checked==false) {//User wants to use a specific list of providers.
				listProviders=GetFilteredProviderList(_listProviders);
			}
			else {
				listProviders=Providers.GetDeepCopy(true);
				listProviders=GetFilteredProviderList(listProviders);//Filters the list of all providers. 
			}
			return listProviders;
		}

		private void FillGrid(object data){
			List<Provider> listProviders=(List<Provider>)data;
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn col;
			col=new GridColumn("Abbrev",80);
			gridMain.Columns.Add(col);
			col=new GridColumn("LName",100);
			gridMain.Columns.Add(col);
			col=new GridColumn("FName",100);
			gridMain.Columns.Add(col);
			gridMain.ListGridRows.Clear();
			GridRow row;
			for(int i=0;i<listProviders.Count;i++) {
				row=new GridRow();
				row.Cells.Add(listProviders[i].Abbr);
				row.Cells.Add(listProviders[i].LName);
				row.Cells.Add(listProviders[i].FName);
				row.Tag=listProviders[i].ProvNum;
				gridMain.ListGridRows.Add(row);
			}
			gridMain.EndUpdate();
		}

		/// <summary>Filters the list of providers by search terms and returns the filtered list. If used outside of FormProviderPick, make sure your list of providers isn't null before calling this method.</summary>
		private List<Provider> GetFilteredProviderList(List<Provider> listProviders) {
			string txtFilter="";
			Dispatcher.Invoke(() => txtFilter=textFilter.Text);
			if(string.IsNullOrWhiteSpace(txtFilter)) { 
				return listProviders;	
			}
			List<Provider> listProvidersFiltered=new List<Provider>();
			for(int i=0;i<listProviders.Count;i++) {
				if(listProviders[i].FName==null || listProviders[i].LName==null || listProviders[i].Abbr==null) {
					continue;
				}
				if(listProviders[i].FName.ToUpper().Trim().Contains(txtFilter.ToUpper().Trim()) ||
					listProviders[i].LName.ToUpper().Trim().Contains(txtFilter.ToUpper().Trim()) ||
					listProviders[i].Abbr.ToUpper().Trim().Contains(txtFilter.ToUpper().Trim())) 
				{
					listProvidersFiltered.Add(listProviders[i]);
				}
			}
			listProvidersFiltered=listProvidersFiltered
				.OrderByDescending(x=>x.FName.ToUpper().Trim().StartsWith(txtFilter.ToUpper().Trim()))
				.ThenByDescending(x=>x.LName.ToUpper().Trim().StartsWith(txtFilter.ToUpper().Trim()))
				.ThenByDescending(x=>x.Abbr.ToUpper().Trim().StartsWith(txtFilter.ToUpper().Trim())).ToList();
			return listProvidersFiltered;
		}

		private void gridMain_CellDoubleClick(object sender,GridClickEventArgs e) {
			if(gridMain.GetSelectedIndex()<0 || gridMain.GetSelectedIndex()>=gridMain.ListGridRows.Count) {//adding this check to fix an Index out of Range UE per tasknum:6313708
				return;
			}
			ProvNumSelected=SIn.Long(gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag.ToString());
			IsDialogOK=true;
		}

		private void checkShowAll_Click(object sender,EventArgs e) {
			List<Provider> listProviders=RefreshDBForGrid();
			FillGrid(listProviders);
		}

		private void FrmProviderPick_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(gridMain.GetSelectedIndex()==-1) {
				MsgBox.Show(this,"Please select a provider first.");
				return;
			}
			ProvNumSelected=SIn.Long(gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag.ToString());
			IsDialogOK=true;
		}

		private void butSelectNone_Click(object sender,EventArgs e) {
			ProvNumSelected=0;
			IsDialogOK=true;
		}

	}
}