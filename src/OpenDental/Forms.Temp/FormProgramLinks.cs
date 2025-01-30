using System.Collections.Generic;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Drawing;
using System;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormProgramLinks : FormODBase {
	private bool _didChange;
	private List<Program> _listPrograms;

		
	public FormProgramLinks(){
		InitializeComponent();// Required for Windows Form Designer support
	}

	private void FormProgramLinks_Load(object sender, System.EventArgs e) {
		FillList();
	}

	private void FillList(){
		Programs.RefreshCache();
		_listPrograms=Programs.GetListDeep();
		if(!false) {
			_listPrograms.RemoveAll(x => x.ProgName==ProgramName.AvaTax.ToString());
		}
		_listPrograms.RemoveAll(x => !Programs.IsEnabledByHq(x,out var _));//Remove all programs that are disabled by HQ from the list to fill.
		gridProgram.BeginUpdate();
		gridProgram.Columns.Clear();
		gridProgram.Columns.Add(new GridColumn("Enabled",55,HorizontalAlignment.Center));
		gridProgram.Columns.Add(new GridColumn("Description",100){ IsWidthDynamic=true });
		gridProgram.ListGridRows.Clear();
		for(var i=0;i<_listPrograms.Count;i++){
			var row=new GridRow();
			row.Tag=_listPrograms[i];
			var color = Color.FromArgb(230, 255, 238);
			row.ColorBackG=row.ColorBackG;
			if(_listPrograms[i].Enabled){
				row.ColorBackG=color;
			}
			var cell=new GridCell(_listPrograms[i].Enabled ? "X" : "");
			row.Cells.Add(cell);
			row.Cells.Add(_listPrograms[i].ProgDesc);
			gridProgram.ListGridRows.Add(row);
		}
		gridProgram.EndUpdate();
	}

	private void butAdd_Click(object sender, System.EventArgs e) {
		using var formProgramLinkEdit=new FormProgramLinkEdit();
		formProgramLinkEdit.IsNew=true;
		formProgramLinkEdit.ProgramCur=new Program();
		formProgramLinkEdit.ShowDialog();
		_didChange=true;//because we don't really know what they did, so assume changed.
		FillList();
	}

	private void gridProgram_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		var dialogResult=DialogResult.None;
		var program=_listPrograms[gridProgram.GetSelectedIndex()].Copy();
		switch(program.ProgName) {
			case "Mountainside":
				using(var formMountainside=new FormMountainside()) {
					formMountainside.ProgramCur=program;
					dialogResult=formMountainside.ShowDialog();
				}
				break;
			case "PayConnect":
				using(var formPayConnectSetup=new FormPayConnectSetup()) {
					dialogResult=formPayConnectSetup.ShowDialog();
				}
				break;
			case "Podium":
				using(var formPodiumSetup=new FormPodiumSetup()) {
					dialogResult=formPodiumSetup.ShowDialog();
				}
				break;
			case "Xcharge":
				using(var fromXChargeSetup=new FormXchargeSetup()) {
					dialogResult=fromXChargeSetup.ShowDialog();
				}
				break;
			case "FHIR":
				using(var formFHIRSetup=new FormFHIRSetup()) {
					dialogResult=formFHIRSetup.ShowDialog();
				}
				break;
			case "Transworld":
				using(var formTransworldSetup=new FormTransworldSetup()) {
					dialogResult=formTransworldSetup.ShowDialog();
				}
				break;
			case "PaySimple":
				using(var formPaySimpleSetup=new FormPaySimpleSetup()) {
					dialogResult=formPaySimpleSetup.ShowDialog();
				}
				break;
			case "XDR":
				using(var formXDRSetup=new FormXDRSetup()) {
					dialogResult=formXDRSetup.ShowDialog();
				}
				break;
			case "TrojanExpressCollect":
				using(var formTrojanCollectSetup=new FormTrojanCollectSetup()) {
					dialogResult=formTrojanCollectSetup.ShowDialog();
				}
				break;
			case "BencoPracticeManagement":
				var frmBencoSetup=new FrmBencoSetup();
				frmBencoSetup.ShowDialog();
				if(frmBencoSetup.IsDialogOK){
					dialogResult=DialogResult.OK;
				}
				break;
			case nameof(ProgramName.EdgeExpress):
				using(var formEdgeExpressSetup=new FormEdgeExpressSetup()) {
					dialogResult=formEdgeExpressSetup.ShowDialog();
				}
				break;
			default:
				using(var formProgramLinkEdit=new FormProgramLinkEdit()) {
					if(Programs.IsStatic(program)) {
						formProgramLinkEdit.AllowToolbarChanges=false;
					}
					formProgramLinkEdit.ProgramCur=program;
					dialogResult=formProgramLinkEdit.ShowDialog();
				}
				break;
		}
		if(dialogResult==DialogResult.OK) {
			_didChange=true;
			FillList();
		}
	}

	private void FormProgramLinks_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
		if(!_didChange){
			return;
		}
		Cursor=Cursors.WaitCursor;
		try {
			//Let HQ know the program link change.
			Programs.SendEnabledProgramsToHQ();
		}
		catch(Exception ex) {
		}
		Cursor=Cursors.Default;
		DataValid.SetInvalid(InvalidType.Programs, InvalidType.ToolButsAndMounts);
	}

}