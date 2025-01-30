using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using OpenDental.UI;
using System.Data;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormPatFieldDefEdit : FormODBase {
		
	public bool IsNew;
	public bool HasChanged;
	public PatFieldDef PatFieldDefCur;
	private string _fieldNameOld;
	private List<PatFieldPickItem> _listPatFieldPickItems= [];

		
	public FormPatFieldDefEdit() {
		// Required for Windows Form Designer support
		InitializeComponent();
	}

	private void FormPatFieldDefEdit_Load(object sender, EventArgs e) {
		textName.Text=PatFieldDefCur.FieldName;
		TogglePickList(makeVisible:false);
		textName.ReadOnly=false;
		comboFieldType.Items.Clear();
		//InCaseOfEmergency DEPRECATED. (Only used 16.3.1,deprecated by 16.3.4)
		var listPatFieldTypes=Enum.GetValues(typeof(PatFieldType)).Cast<PatFieldType>().Where(x => x!=PatFieldType.InCaseOfEmergency).ToList();
		comboFieldType.Items.Clear();
		comboFieldType.Items.AddList(listPatFieldTypes,x => x.GetDescription());
		comboFieldType.SetSelectedEnum(PatFieldDefCur.FieldType);
		if(!IsNew) {
			_fieldNameOld=PatFieldDefCur.FieldName;
			checkHidden.Checked=PatFieldDefCur.IsHidden;
		}
		if(comboFieldType.GetSelected<PatFieldType>()==PatFieldType.PickList) {
			TogglePickList(makeVisible:true);
			FillGrid();
		}
	}

	private void comboFieldType_SelectedIndexChanged(object sender,EventArgs e) {
		if(!IsNew) {
			//todo: check existing values to make sure that it makes sense to change the type.  Especially when moving to currency or date.
		}
		TogglePickList(makeVisible:false);
		textName.ReadOnly=false;
		if(comboFieldType.GetSelected<PatFieldType>()==PatFieldType.PickList) {
			TogglePickList(makeVisible:true);
		}
	}

	private void buttonDelete_Click(object sender,EventArgs e) {
		if(IsNew) {
			DialogResult=DialogResult.Cancel;
			return;
		}
		// If picklist, check all picklist items associated with that PatFieldDefNum
		if(comboFieldType.GetSelected<PatFieldType>()==PatFieldType.PickList) {
			var message="";
			var listPatFieldPickItems=PatFieldPickItems.GetWhere(x=>x.PatFieldDefNum==PatFieldDefCur.PatFieldDefNum);
			for(var i=0;i<listPatFieldPickItems.Count;i++) {
				var listPatNumsUsingPickItem=PatFields.GetPatNumsUsingPickItem(listPatFieldPickItems[i].Name,PatFieldDefCur.FieldName);
				if(listPatNumsUsingPickItem.Count>0) {
					message+="\r\n"+listPatFieldPickItems[i].Name;
				}
			}
			if(message!="") {
				var str=Lang.g(this,"Not allowed to delete because the following pick list items are still in use:");
				MsgBox.Show(str+message);
				return;
			}
		}
		if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete this item?")) {
			return;
		}
		try {
			PatFieldDefs.Delete(PatFieldDefCur);//Throws exception if in use
			HasChanged=true;
		}
		catch(ApplicationException ex) {
			ODMessageBox.Show(ex.Message);
			return;
		}
		FieldDefLinks.DeleteForFieldDefNum(PatFieldDefCur.PatFieldDefNum,FieldDefTypes.Patient);//Delete any FieldDefLinks to this PatFieldDef
		PatFieldDefCur=null;
		DialogResult=DialogResult.OK;
	}

	private void butSave_Click(object sender, EventArgs e) {
		if(_fieldNameOld!=textName.Text && !IsNew) {
			if(PatFieldDefs.GetExists(x => x.FieldName==textName.Text)) {
				MsgBox.Show(this,"Field name currently being used.");
				return;
			}
		}
		PatFieldDefCur.FieldName=textName.Text;
		PatFieldDefCur.IsHidden=checkHidden.Checked;
		PatFieldDefCur.FieldType=comboFieldType.GetSelected<PatFieldType>();
		if(PatFieldDefCur.FieldType==PatFieldType.PickList && _listPatFieldPickItems.Count==0) {
			MsgBox.Show(this,"List cannot be blank.");
			return;
		}
		PatFieldDefs.Update(PatFieldDefCur);
		HasChanged=true;
		if(!IsNew && PatFieldDefCur.FieldName!=_fieldNameOld) {
			PatFields.UpdateFieldName(PatFieldDefCur.FieldName,_fieldNameOld);
		}
		//sync display fields.
		var listDisplayFields=DisplayFields.GetForCategory(DisplayFieldCategory.SuperFamilyGridCols).Where(x=>x.InternalName=="" && x.Description==_fieldNameOld).ToList();
		if(listDisplayFields.Count>0) {
			for(var i=0;i<listDisplayFields.Count;i++) {
				listDisplayFields[i].Description=PatFieldDefCur.FieldName;
			}
			DataValid.SetInvalid(InvalidType.DisplayFields);
		}
		DialogResult=DialogResult.OK;
	}

	private void FillGrid() {
		_listPatFieldPickItems=PatFieldPickItems.GetWhere(x=>x.PatFieldDefNum==PatFieldDefCur.PatFieldDefNum).OrderBy(x=>x.ItemOrder).ToList();
		gridPickListItems.BeginUpdate();
		gridPickListItems.Columns.Clear();
		GridColumn gridColumn;
		gridColumn=new GridColumn(Lan.g(this,"Hidden"),50,HorizontalAlignment.Center);
		gridPickListItems.Columns.Add(gridColumn);
		gridColumn=new GridColumn(Lan.g(this,"Item Name"),200) { IsWidthDynamic=true };
		gridPickListItems.Columns.Add(gridColumn);
		gridColumn=new GridColumn(Lan.g(this,"Abbr"),50,HorizontalAlignment.Center);
		gridPickListItems.Columns.Add(gridColumn);
		gridPickListItems.ListGridRows.Clear();
		for(var i=0;i<_listPatFieldPickItems.Count;i++) {
			var gridRow=new GridRow();
			gridRow.Cells.Add(_listPatFieldPickItems[i].IsHidden?"X":"");
			gridRow.Cells.Add(_listPatFieldPickItems[i].Name);
			gridRow.Cells.Add(_listPatFieldPickItems[i].Abbreviation);
			gridRow.Tag=_listPatFieldPickItems[i];
			gridPickListItems.ListGridRows.Add(gridRow);
		}
		gridPickListItems.EndUpdate();
		comboFieldType.Enabled=true;
		labelFieldType.Text="Field Type";
		if(comboFieldType.GetSelected<PatFieldType>()==PatFieldType.PickList && _listPatFieldPickItems.Count>0) {
			comboFieldType.Enabled=false;
			labelFieldType.Text="Field Type  (Picklist must be empty to change type)";
		}
	}

	private void gridPickListItems_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var patFieldPickItem=(PatFieldPickItem)gridPickListItems.ListGridRows[e.Row].Tag;
		var frmPatFieldPickItem=new FrmPatFieldPickItem();
		frmPatFieldPickItem.PatFieldPickItemCur=patFieldPickItem;
		frmPatFieldPickItem.ShowDialog();
		if(frmPatFieldPickItem.IsDialogCancel) {
			return;
		}
		HasChanged=true;
		PatFieldPickItems.RefreshCache();
		FillGrid();
	}

	private void butAdd_Click(object sender,EventArgs e) {
		var patFieldPickItem=new PatFieldPickItem();
		patFieldPickItem.IsNew=true;
		patFieldPickItem.ItemOrder=_listPatFieldPickItems.Count;
		patFieldPickItem.PatFieldDefNum=PatFieldDefCur.PatFieldDefNum;
		var frmPatFieldPickItem=new FrmPatFieldPickItem();
		frmPatFieldPickItem.PatFieldPickItemCur=patFieldPickItem;
		frmPatFieldPickItem.ShowDialog();
		if(frmPatFieldPickItem.IsDialogCancel) {
			return;
		}
		HasChanged=true;
		PatFieldPickItems.RefreshCache();
		FillGrid();
	}

	private void butUp_Click(object sender,EventArgs e) {
		if(gridPickListItems.SelectedIndices.Length==0) {
			MsgBox.Show(this,"Please select an item in the grid first.");
			return;
		}
		var listSelectedIncides=gridPickListItems.SelectedIndices.ToList();
		if(listSelectedIncides.Min()==0) { //Can't go up anymore
			return;
		}
		for(var i=0;i<listSelectedIncides.Count;i++) {
			_listPatFieldPickItems.Reverse(listSelectedIncides[i]-1,2);
		}
		HasChanged=true;
		UpdateItemOrder(_listPatFieldPickItems); //Does PatFieldPickItems.RefreshCache(); in method
		FillGrid();
		for(var i=0;i<listSelectedIncides.Count;i++) {
			gridPickListItems.SetSelected(listSelectedIncides[i]-1,true);
		}
	}

	private void butDown_Click(object sender,EventArgs e) {
		if(gridPickListItems.SelectedIndices.Length==0) {
			MsgBox.Show(this,"Please select an item in the grid first.");
			return;
		}
		var listSelectedIncides=gridPickListItems.SelectedIndices.ToList();
		if(listSelectedIncides.Max()==_listPatFieldPickItems.Count-1) { //Can't go down anymore
			return;
		}
		for(var i= listSelectedIncides.Count-1;i>=0;i--) {
			_listPatFieldPickItems.Reverse(listSelectedIncides[i],2);
		}
		HasChanged=true;
		UpdateItemOrder(_listPatFieldPickItems); //Does PatFieldPickItems.RefreshCache(); in method
		FillGrid();
		for(var i=0;i<listSelectedIncides.Count;i++) {
			gridPickListItems.SetSelected(listSelectedIncides[i]+1,true);
		}
	}

	///<summary>Updates ItemOrder for all PatFieldPickListItems in the list and updates db entry for every item in the list.
	///Uses the list's order to set ItemOrder. Refreshes cache when done.</summary>
	private void UpdateItemOrder(List<PatFieldPickItem> listPatFieldPickItems) {
		var hasChanged=false;
		for(var i=0;i<listPatFieldPickItems.Count;i++) {
			if(listPatFieldPickItems[i].ItemOrder==i) {
				continue;
			}
			listPatFieldPickItems[i].ItemOrder=i;
			PatFieldPickItems.Update(listPatFieldPickItems[i]);
			hasChanged=true;
		}
		if(hasChanged) {
			PatFieldPickItems.RefreshCache();
		}
	}

	private void TogglePickList(bool? makeVisible=null) {
		if(makeVisible==true) {
			gridPickListItems.Visible=true;
			butAdd.Visible=true;
			butUp.Visible=true;
			butDown.Visible=true;
			butMerge.Visible=true;
		}
		else if(makeVisible==false) {
			gridPickListItems.Visible=false;
			butAdd.Visible=false;
			butUp.Visible=false;
			butDown.Visible=false;
			butMerge.Visible=false;
		}
		else {//to the opposite of whatever it was
			gridPickListItems.Visible=!gridPickListItems.Visible;
			butAdd.Visible=!butAdd.Visible;
			butUp.Visible=!butUp.Visible;
			butDown.Visible=!butDown.Visible;
			butMerge.Visible=!butMerge.Visible;
		}
	}

	private void butMerge_Click(object sender,EventArgs e) {
		if(gridPickListItems.SelectedIndices.Count()!=2) {
			MsgBox.Show(this,"Please select exactly 2 items to merge together.");
			return;
		}
		if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Once items have been merged, it is not possible to unmerge them.  Continue?")) {
			return;
		}
		var patFieldPickItemA=_listPatFieldPickItems[gridPickListItems.SelectedIndices[0]];
		var patFieldPickItemB=_listPatFieldPickItems[gridPickListItems.SelectedIndices[1]];
		var nameNew=patFieldPickItemA.Name+"/"+patFieldPickItemB.Name;
		var patFieldName=PatFieldDefs.GetFieldName(PatFieldDefCur.PatFieldDefNum);
		PatFields.UpdatePatFieldValues(patFieldName,nameNew,patFieldPickItemA.Name);
		PatFields.UpdatePatFieldValues(patFieldName,nameNew,patFieldPickItemB.Name);
		patFieldPickItemA.Name=nameNew;
		PatFieldPickItems.Update(patFieldPickItemA);
		PatFieldPickItems.Delete(patFieldPickItemB.PatFieldPickItemNum);
		PatFieldPickItems.RefreshCache();
		FillGrid();
	}
}