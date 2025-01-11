using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;

namespace OpenDental {
	public class SecurityL {
		
		///<summary>Called to change the password for Security.CurUser.
		///Returns true if password was changed successfully.
		///Set isForcedLogOff to force the program to log the user off if they cancel out of the Change Password window.</summary>
		public static bool ChangePassword(bool isForcedLogOff,bool willRefreshSecurityCache=true) {
			//no security blocking because everyone is allowed to change their own password.
			if(Security.CurUser.UserNumCEMT!=0) {
				MsgBox.Show("FormOpenDental","Use the CEMT tool to change your password.");
				return false;
			}
			using FormUserPassword formUserPassword=new FormUserPassword(isCreate:false,Security.CurUser.UserName);
			formUserPassword.ShowDialog();
			if(formUserPassword.DialogResult==DialogResult.Cancel) {
				if(isForcedLogOff) {
					FormOpenDental formOpenDental=Application.OpenForms.OfType<FormOpenDental>().ToList()[0];//There always should be exactly 1.
					formOpenDental.LogOffNow(true);
				}
				return false;
			}
			bool isPasswordStrong=formUserPassword.IsPasswordStrong;
			try {
				Userods.UpdatePassword(Security.CurUser,formUserPassword.PasswordContainer_,isPasswordStrong);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
				return false;
			}
			Security.CurUser.PasswordIsStrong=formUserPassword.IsPasswordStrong;
			Security.CurUser.SetPassword(formUserPassword.PasswordContainer_);
			Security.PasswordTyped=formUserPassword.PasswordTyped;
			if(willRefreshSecurityCache) {
				DataValid.SetInvalid(InvalidType.Security);
			}
			return true;
		}

		///<summary>Shows message box and returns false if user doesn't have permission to edit the document. User must have ImageEdit permission to edit any document (uses date restriction). User must additionally have SignedImageEdit to edit signed documents.</summary>
		public static bool IsAuthorizedToEditImage(Document document) {
			if(!Security.IsAuthorized(EnumPermType.ImageEdit,document.DateCreated)) {
				return false;//User can't edit due to missing permission or date restriction.
			}
			else if(!document.Signature.IsNullOrEmpty() && !Security.IsAuthorized(EnumPermType.SignedImageEdit)) {
				return false;//Document has signature and user can't edit signed documents.
			}
			return true;//User can edit the document.
		}

		///<summary>Shows message box and returns false if user doesn't have permission to delete the document. User must have ImageDelete permission to delete any document (uses date restriction). User must additionally have SignedImageEdit to delete signed documents.</summary>
		public static bool IsAuthorizedToDeleteImage(Document document) {
			if(!Security.IsAuthorized(EnumPermType.ImageDelete,document.DateCreated)) {
				return false;//User can't delete due to missing permission or date restriction.
			}
			else if(!document.Signature.IsNullOrEmpty() && !Security.IsAuthorized(EnumPermType.SignedImageEdit)) {
				return false;//Document has signature and user can't delete signed documents.
			}
			return true;//User can delete the document.
		}
	}
}
