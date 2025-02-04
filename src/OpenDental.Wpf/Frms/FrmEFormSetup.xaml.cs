using System;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental {
	
	public partial class FrmEFormSetup : FrmODBase {
		
		public FrmEFormSetup() {
			InitializeComponent();
			Load+=FrmEformSetup_Load;
		}

		
		private void FrmEformSetup_Load(object sender, EventArgs e) {
			Lang.F(this);
			textVIntSpaceBelowEachField.Value=PrefC.GetInt(PrefName.EformsSpaceBelowEachField);
			textVIntSpaceToRightEachField.Value=PrefC.GetInt(PrefName.EformsSpaceToRightEachField);
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(!textVIntSpaceBelowEachField.IsValid()
				|| !textVIntSpaceToRightEachField.IsValid())
			{
				MsgBox.Show(this,"Please fix data entry errors first.");
				return;
			}
			bool changed=false;
			changed|=Prefs.UpdateInt(PrefName.EformsSpaceBelowEachField,textVIntSpaceBelowEachField.Value);
			changed|=Prefs.UpdateInt(PrefName.EformsSpaceToRightEachField,textVIntSpaceToRightEachField.Value);
			if(changed){
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			IsDialogOK=true;
		}
	}
}