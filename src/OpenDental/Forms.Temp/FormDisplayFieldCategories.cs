using System;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental;

/// <summary></summary>
public partial class FormDisplayFieldCategories:FormODBase {
		
	public FormDisplayFieldCategories()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormDisplayFields_Load(object sender,EventArgs e) {
		//Alphabetical order.  When new display fields are added this will need to be changed.
		var listDisplayFieldCategories=
			Enum.GetValues(typeof(DisplayFieldCategory)).OfType<DisplayFieldCategory>().OrderBy(x=>x.GetDescription()).ToList();
		for(var i=0;i<listDisplayFieldCategories.Count;i++) {
			if(listDisplayFieldCategories[i]==DisplayFieldCategory.None) {//skip None because user not allowed to select that
				continue;
			}
			if(listDisplayFieldCategories[i]==DisplayFieldCategory.OrthoChart) { //orthochart tabs can have their own name.
				listCategory.Items.Add(OrthoChartTabs.GetFirst(shortList:true).TabName,listDisplayFieldCategories[i]);
				continue;
			}
			listCategory.Items.Add(Lan.g("enumDisplayFieldCategory",listDisplayFieldCategories[i].GetDescription()),listDisplayFieldCategories[i]);
		}
		listCategory.SelectedIndex=0;
	}

	private void listCategory_DoubleClick(object sender,EventArgs e) {
		ShowCategoryEdit();
		Close();
	}

	private void ShowCategoryEdit() {
		var displayFieldCategorySelected=listCategory.GetSelected<DisplayFieldCategory>();
		if(displayFieldCategorySelected==DisplayFieldCategory.None) {//should never happen.
			return;
		}
		//The ortho chart is a more complicated display field so it has its own window.
		if(displayFieldCategorySelected==DisplayFieldCategory.OrthoChart) {
			using var formDisplayFieldsOrthoChart=new FormDisplayFieldsOrthoChart();
			formDisplayFieldsOrthoChart.ShowDialog();
		}
		else {//All other display fields use the base display fields window.
			using var formDisplayFields=new FormDisplayFields();
			formDisplayFields.DisplayFieldCategoryCur=displayFieldCategorySelected;
			formDisplayFields.ShowDialog();
		}
	}

	private void butOK_Click(object sender,EventArgs e) {
		ShowCategoryEdit();
		Close();
	}

}