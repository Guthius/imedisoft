using System;
using System.Windows.Input;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental {
	public partial class FrmEmailMasterTemplate:FrmODBase {

		public FrmEmailMasterTemplate() {
			InitializeComponent();
			Load+=FrmEmailMasterTemplate_Load;
			PreviewKeyDown+=FrmEmailMasterTemplate_PreviewKeyDown;
		}

		private void FrmEmailMasterTemplate_Load(object sender,EventArgs e) {
			Lang.F(this);
			textMaster.Text=PrefC.GetString(PrefName.EmailMasterTemplate);
		}

		private void FrmEmailMasterTemplate_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(Prefs.UpdateString(PrefName.EmailMasterTemplate,textMaster.Text)) {
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			IsDialogOK=true;
		}

	}
}