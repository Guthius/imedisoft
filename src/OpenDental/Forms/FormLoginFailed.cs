using System;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;

namespace OpenDental {
	public partial class FormLoginFailed:FormODBase {
		private string _errorMsg;

		
		public FormLoginFailed(string errorMessage) {
			InitializeComponent();
			InitializeLayoutManager();
			_errorMsg=errorMessage;
		}

		private void FormLoginFailed_Load(object sender,EventArgs e) {
			labelErrMsg.Text=_errorMsg;
			textUser.Text=Security.CurUser.UserName;//CurUser verified to not be null in FormOpenDental before loading this form
			textPassword.Focus();
		}

		private void butLogin_Click(object sender,EventArgs e) {
			Userod userodEntered;
			string password;
			bool useEcwAlgorithm=Programs.UsingEcwTightOrFullMode();
			//ecw requires hash, but non-ecw requires actual password
			password=textPassword.Text;
			if(useEcwAlgorithm) {
				//It doesn't matter what Security.CurUser is when it is null because we are technically trying to set it for the first time.
					//It cannot be null before invoking HashPassword because middle needs it to NOT be null when creating the credentials for DtoGetString.
				if(Security.CurUser==null) {
						Security.CurUser=new Userod();
				}
				try{
					password=Authentication.HashPasswordMD5(password,true);
				}
				catch(Exception ex) {
					MessageBox.Show(ex.Message);
					return;
				}
			}
			string username=textUser.Text;
			if(ODBuild.IsDebug()) {
				if(username=="") {
					username="Admin";
					password="pass";
				}
			}
			//Set the PasswordTyped property prior to checking the credentials for Middle Tier.
			Security.PasswordTyped=password;
			try{
				userodEntered=Userods.CheckUserAndPassword(username,password,useEcwAlgorithm);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
				return;
			}
			//successful login.
			Security.CurUser=userodEntered;
			Security.IsUserLoggedIn=true;
			if(PrefC.GetBool(PrefName.PasswordsMustBeStrong)
				&& PrefC.GetBool(PrefName.PasswordsWeakChangeToStrong)
				&& Userods.IsPasswordStrong(textPassword.Text)!="") //Password is not strong
			{
				MsgBox.Show(this,"You must change your password to a strong password due to the current Security settings.");
				if(!SecurityL.ChangePassword(true)) {//Failed password update.
					return;
				}
			}
			SecurityLogs.MakeLogEntry(EnumPermType.UserLogOnOff,0,"User: "+Security.CurUser.UserNum+" has logged on.");
			DialogResult=DialogResult.OK;
		}

	}
}