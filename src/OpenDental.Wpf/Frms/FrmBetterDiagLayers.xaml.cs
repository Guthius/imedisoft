using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using OpenDentBusiness.Bridges;
using WpfControls.UI;

namespace OpenDental {
	/// <summary></summary>
	public partial class FrmBetterDiagLayers : FrmODBase {
		///<summary>List of EnumCategoryBetterDiags that are currently toggled on to be visible. Used in ControlImages.</summary>
		public List<EnumCategoryBetterDiag> ListEnumCategoryBetterDiagsShown=new List<EnumCategoryBetterDiag>();
		///<summary>Used to refresh shown categories on ControlImages. Applied to click event of every checkbox.</summary>
		public EventHandler EventRefreshControlImages=null;
		///<summary>When set to false via the All Annotations checkbox, all Better Diagnostics annotations are hidden from view.</summary>
		public bool ShowBetterDiagLayers=true;
		///<summary>Used to set the starting location of the form.</summary>
		public System.Drawing.Point Location;

		
		public FrmBetterDiagLayers() {
			InitializeComponent();
			Load+=FrmBetterDiagLayers_Load;
		}

		///<summary>Refreshes UI to reflect the state of ListEnumCategoryBetterDiagsShown and ShowBetterDiagLayers. Used to synchronize with docked window checkboxes.</summary>
		public void RefreshUI() {
			radioShow.Checked=ShowBetterDiagLayers;
			radioHide.Checked=!ShowBetterDiagLayers;
			DisableGroupsIfHidingAll();
			SetAllCheckBoxCheckeds();
		}

		#region Event Handlers
		private void FrmBetterDiagLayers_Load(object sender, EventArgs e) {
			Lang.F(this);
			SetAllCheckBoxClickEventHandlers();
			SetAllCheckBoxTags();
			SetAllCheckBoxCheckeds();
			SetAllCheckBoxColors();
			SetAllToothPartLegendColors();
			radioShow.Checked=ShowBetterDiagLayers;
			radioHide.Checked=!ShowBetterDiagLayers;
			DisableGroupsIfHidingAll();
			_formFrame.Location=Location;
		}

		private void radioShow_Click(object sender,EventArgs e) {
			ShowBetterDiagLayers=true;
			DisableGroupsIfHidingAll();
			EventRefreshControlImages?.Invoke(sender,e);
		}

		private void radioHide_Click(object sender,EventArgs e) {
			ShowBetterDiagLayers=false;
			DisableGroupsIfHidingAll();
			EventRefreshControlImages?.Invoke(sender,e);
		}
		#endregion Event Handlers

		///<summary>Adds Checked event handler for every checkbox that represents a category. </summary>
		private void SetAllCheckBoxClickEventHandlers() {
			//Set checked status of each checkbox depending on whether all tag categories are included in the list of shown categories.
			for(int i=0;i<groupFindings.Items.Count;i++) {
				if(groupFindings.Items[i] is CheckBox) {
					CheckBox checkbox=(CheckBox)groupFindings.Items[i];
					checkbox.Click+=(s,e) => UpdateListForCheckBox(checkbox);
					checkbox.Click+=EventRefreshControlImages;
				}
			}
			for(int i=0;i<groupRestorations.Items.Count;i++) {
				if(groupRestorations.Items[i] is CheckBox) {
					CheckBox checkbox=(CheckBox)groupRestorations.Items[i];
					checkbox.Click+=(s,e) => UpdateListForCheckBox(checkbox);
					checkbox.Click+=EventRefreshControlImages;
				}
			}
			checkToothParts.Click+=(s,e) => UpdateListForCheckBox(checkToothParts);
			checkToothParts.Click+=EventRefreshControlImages;
		}
		
		///<summary>Sets all checkbox tags to indicate what ImageDraw categories should be hidden/shown by each checkbox.</summary>
		private void SetAllCheckBoxTags() {
			checkCavity.Tag=     new List<EnumCategoryBetterDiag>() { EnumCategoryBetterDiag.Cavity };
			checkBoneLoss.Tag=   new List<EnumCategoryBetterDiag>() { EnumCategoryBetterDiag.BoneLoss };
			checkInfection.Tag=  new List<EnumCategoryBetterDiag>() { EnumCategoryBetterDiag.Infection };
			checkBoneLevel.Tag=  new List<EnumCategoryBetterDiag>() { EnumCategoryBetterDiag.BoneLevel };
			checkRestoration.Tag=new List<EnumCategoryBetterDiag>() { EnumCategoryBetterDiag.Restoration };
			checkCrown.Tag=      new List<EnumCategoryBetterDiag>() { EnumCategoryBetterDiag.Crown };
			checkToothParts.Tag=BetterDiag.GetBetterDiagToothPartsCategories();
		}

		///<summary>Set checked status of each checkbox depending on whether all of its tag categories are included in the list of shown categories.</summary>
		private void SetAllCheckBoxCheckeds() {
			for(int i=0;i<groupFindings.Items.Count;i++) {
				if(groupFindings.Items[i] is CheckBox) {
					CheckBox checkbox=(CheckBox)groupFindings.Items[i];
					SetCheckBoxFromList(checkbox);
				}
			}
			for(int i=0;i<groupRestorations.Items.Count;i++) {
				if(groupRestorations.Items[i] is CheckBox) {
					CheckBox checkbox=(CheckBox)groupRestorations.Items[i];
					SetCheckBoxFromList(checkbox);
				}
			}
			SetCheckBoxFromList(checkToothParts);
		}

		///<summary>Set checkbox background to legend color.</summary>
		private void SetAllCheckBoxColors() {
			checkCavity.ColorBack=     ColorOD.ToWpf(BetterDiag.GetTextColorForCategory(EnumCategoryBetterDiag.Cavity));
			checkBoneLoss.ColorBack=   ColorOD.ToWpf(BetterDiag.GetTextColorForCategory(EnumCategoryBetterDiag.BoneLoss));
			checkInfection.ColorBack=  ColorOD.ToWpf(BetterDiag.GetTextColorForCategory(EnumCategoryBetterDiag.Infection));
			checkBoneLevel.ColorBack=  ColorOD.ToWpf(BetterDiag.GetTextColorForCategory(EnumCategoryBetterDiag.BoneLevel));
			checkRestoration.ColorBack=ColorOD.ToWpf(BetterDiag.GetTextColorForCategory(EnumCategoryBetterDiag.Restoration));
			checkCrown.ColorBack=      ColorOD.ToWpf(BetterDiag.GetTextColorForCategory(EnumCategoryBetterDiag.Crown));
		}

		///<summary>Sets colors in tooth parts legend.</summary>
		private void SetAllToothPartLegendColors() {
			panelColorEnamel.ColorBack=ColorOD.ToWpf(BetterDiag.GetColorForCategory(EnumCategoryBetterDiag.Enamel));
			panelColorDentin.ColorBack=ColorOD.ToWpf(BetterDiag.GetColorForCategory(EnumCategoryBetterDiag.Dentin));
			panelColorPulp.ColorBack=  ColorOD.ToWpf(BetterDiag.GetColorForCategory(EnumCategoryBetterDiag.Pulp));
		}

		///<summary>Updates ListEnumCategoryBetterDiagsShown to match the state of a checkbox in this form. If the checkbox is checked, it will
		///add its tag categories to the list. If unchecked, it will remove its tag categories from the list.</summary>
		private void UpdateListForCheckBox(CheckBox checkBox) {
			if(checkBox.Checked==true) {
				//Add tag categories to list. Union ensures we only add if they don't already exist in the list, avoiding duplicates.
				ListEnumCategoryBetterDiagsShown=ListEnumCategoryBetterDiagsShown.Union((List<EnumCategoryBetterDiag>)checkBox.Tag).ToList();
			}
			else {
				//Remove categories from list. Except ensures we only remove if they exist in the list.
				ListEnumCategoryBetterDiagsShown=ListEnumCategoryBetterDiagsShown.Except((List<EnumCategoryBetterDiag>)checkBox.Tag).ToList();
			}
		}

		///<summary>Updates checkbox state. Sets unchecked if any of its tag categories are not present in 
		///ListEnumCategoryBetterDiagsShown, otherwise sets checked.</summary>
		private void SetCheckBoxFromList(CheckBox checkBox) {
			List<EnumCategoryBetterDiag> listEnumCategoryBetterDiags=(List<EnumCategoryBetterDiag>)checkBox.Tag;
			//If any categories for this checkbox are not toggled on, then load checkbox as unchecked.
			for(int i=0;i<listEnumCategoryBetterDiags.Count;i++) {
				if(!ListEnumCategoryBetterDiagsShown.Contains(listEnumCategoryBetterDiags[i])) {
					checkBox.Checked=false;
					return;
				}
			}
			checkBox.Checked=true;
		}

		///<summary>Sets IsEnabled for each of the checkbox groups.</summary>
		private void DisableGroupsIfHidingAll() {
			//Disable layer filter checkboxes when hiding all annotations.
			groupRestorations.IsEnabled=ShowBetterDiagLayers;
			groupFindings.IsEnabled=ShowBetterDiagLayers;
			groupToothPartsLegend.IsEnabled=ShowBetterDiagLayers;
		}
	}
}