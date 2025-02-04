using System;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental {
	
	public partial class FrmBadgeEdit:FrmODBase {
		
		public Userod UserodCur;

		
		public FrmBadgeEdit() {
			InitializeComponent();
			Load+=FrmBadgeEdit_Load;
		}

		private void FrmBadgeEdit_Load(object sender,EventArgs e) {
			Lang.F(this);
			textUserName.Text=UserodCur.UserName;
			textBadgeID.Text=UserodCur.BadgeId;
		}

		private void butSave_Click(object sender, System.EventArgs e) {
			UserodCur.BadgeId=textBadgeID.Text;
			Userods.Update(UserodCur);
			SecurityLogs.MakeLogEntry(EnumPermType.BadgeIdEdit,0,"The BadgeId for "+UserodCur.UserName+" was edited.");
			IsDialogOK=true;
			DataValid.SetInvalid(InvalidType.Security);
		}

	}
}