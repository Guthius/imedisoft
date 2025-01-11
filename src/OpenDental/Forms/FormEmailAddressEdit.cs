using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using System.Collections.Generic;
using System.Diagnostics;
using OpenDental.UI;
using System.Linq;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental{

	public partial class FormEmailAddressEdit:FormODBase {
		private EmailAddress _emailAddress;
		private bool _isNew;
		private OAuthType _authenticationType=OAuthType.None;

		
		public FormEmailAddressEdit(EmailAddress emailAddress,bool isOpenedFromEmailSetup=false) {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
			_emailAddress=emailAddress;
			_authenticationType=emailAddress.AuthenticationType;
			List<long> listDefaultAddressNums=new List<long>();
			listDefaultAddressNums.Add(PrefC.GetLong(PrefName.EmailNotifyAddressNum));
			listDefaultAddressNums.Add(PrefC.GetLong(PrefName.EmailDefaultAddressNum));
			if(isOpenedFromEmailSetup && Security.IsAuthorized(EnumPermType.SecurityAdmin,suppressMessage:true) 
				&& (_isNew || !listDefaultAddressNums.Contains(_emailAddress.EmailAddressNum)))
			{
				butPickUserod.Visible=true;
			}
		}

		public FormEmailAddressEdit(long userNum,bool isOpenedFromEmailSetup=false) : this(new EmailAddress() { UserNum=userNum },isOpenedFromEmailSetup) {
			_isNew=true;
		}

		private void FormEmailAddress_Load(object sender,System.EventArgs e) {
			butAuthGoogleImageHelper(butAuthGoogle.Image);
			bool enableEmailFields=true;
			if(_emailAddress!=null) {
				textSMTPserver.Text=_emailAddress.SMTPserver;
				textUsername.Text=_emailAddress.EmailUsername;
				if(!String.IsNullOrEmpty(_emailAddress.EmailPassword)) { //can happen if creating a new user email.
					textPassword.Text=MiscUtils.Decrypt(_emailAddress.EmailPassword);
				}
				textPort.Text=_emailAddress.ServerPort.ToString();
				checkSSL.Checked=_emailAddress.UseSSL;
				textSender.Text=_emailAddress.SenderAddress;
				textSMTPserverIncoming.Text=_emailAddress.Pop3ServerIncoming;
				textPortIncoming.Text=_emailAddress.ServerPortIncoming.ToString();
				//Both EmailNotifyAddressNum and EmailDefaultAddressNum could be 0 (unset), in which case we still may want to display the user.
				List<long> listDefaultAddressNums=new List<long>();
				listDefaultAddressNums.Add(PrefC.GetLong(PrefName.EmailNotifyAddressNum));
				listDefaultAddressNums.Add(PrefC.GetLong(PrefName.EmailDefaultAddressNum));
				if(_isNew || !listDefaultAddressNums.Contains(_emailAddress.EmailAddressNum)) {
					Userod userod=Userods.GetUser(_emailAddress.UserNum);
					textUserod.Tag=userod;
					textUserod.Text=userod?.UserName;
				}
				else {
					panelUserod.Visible=false;
				}
				if(_authenticationType==OAuthType.None) {
					radioPassword.Checked=true;
				}
				else if(_authenticationType==OAuthType.Google) {
					radioGmail.Checked=true;
					enableEmailFields=false;
					textAccessTokenGmail.Text=_emailAddress.AccessToken;
					textRefreshTokenGmail.Text=_emailAddress.RefreshToken;
					checkDownloadGmail.Checked=_emailAddress.DownloadInbox;
					textParams.Text=_emailAddress.QueryString;
					checkUnreadGmail.Checked=_emailAddress.QueryString.ToLower().Contains("is:unread");
				}
			}
			RefreshEmailTextFields(enableEmailFields);
		}

		private void RefreshEmailTextFields(bool isEnablingEmailFields=false) {
			//OAuth uses the API instead of ports so we can disable and autofill fields
			textPassword.Enabled=isEnablingEmailFields;
			textSMTPserver.ReadOnly=!isEnablingEmailFields;
			textPort.Enabled=isEnablingEmailFields;
			textSMTPserverIncoming.ReadOnly=!isEnablingEmailFields;
			textPortIncoming.Enabled=isEnablingEmailFields;
			panelPassword.Visible=radioPassword.Checked;
			groupGmail.Visible=radioGmail.Checked;
			if(radioGmail.Checked) {
				if(textAccessTokenGmail.Text.IsNullOrEmpty()) { //Not signed in
					checkDownloadGmail.Enabled=false;
					butClearTokensGmail.Enabled=false;
					textSMTPserver.Text="";
					textPort.Text="";
				}
				else {
					checkDownloadGmail.Enabled=true;
					butClearTokensGmail.Enabled=true;
					textSMTPserver.Text="smtp.gmail.com";
					textPort.Text="0";
				}
				if(checkDownloadGmail.Checked) {
					textPassword.Text="";
					textSMTPserverIncoming.Text="pop.gmail.com";
					textPortIncoming.Text="0";
					textParams.Enabled=true;
					checkUnreadGmail.Enabled=true;
				}
				else {
					textSMTPserverIncoming.Text="";
					textPortIncoming.Text="";
					textParams.Text="";
					textParams.Enabled=false;
					checkUnreadGmail.Checked=false;
					checkUnreadGmail.Enabled=false;
				}
			}
		}

		private void ClearGmailInfo() {
			textAccessTokenGmail.Text="";
			textRefreshTokenGmail.Text="";
			checkDownloadGmail.Checked=false;
			textParams.Text="";
			checkUnreadGmail.Checked=false;
			textSMTPserver.Text="";
			textPort.Text="";
			groupGmail.Visible=false;
		}
		
		private void checkDownloadGmail_CheckedChanged(object sender,EventArgs e) {
			if(checkDownloadGmail.Checked) {
				textSMTPserverIncoming.Text="pop.gmail.com";
				textPortIncoming.Text="0";
				checkUnreadGmail.Enabled=true;
				textParams.Enabled=true;
				checkUnreadGmail.Checked=textParams.Text.ToLower().Contains("is:unread");
			}
			else {
				textSMTPserverIncoming.Text="";
				textPortIncoming.Text="";
				textParams.Text="";
				textParams.Enabled=false;
				checkUnreadGmail.Checked=false;
				checkUnreadGmail.Enabled=false;
			}
		}

		private void checkUnreadGmail_CheckedChanged(object sender,EventArgs e) {
			string strIsUnread="is:unread";
			string queryString=textParams.Text;
			if(checkUnreadGmail.Checked) {
				//Add is:unread if it is not present within the current query params.
				if(!queryString.ToLower().Contains(strIsUnread)) {
					queryString=queryString.Trim()+" "+strIsUnread;
				}
			}
			else {
				int index=queryString.ToLower().IndexOf(strIsUnread);
				//Remove is:unread if it is present within the current query params.
				if(index>-1) {
					queryString=textParams.Text.Remove(index,strIsUnread.Length);
				}
			}
			textParams.Text=queryString;
		}

		private void butAuthGoogleImageHelper(Image image) {
			butAuthGoogle.SuspendLayout();
			if(LayoutManager.ScaleMy()!=1) {
				Size size=new Size(LayoutManager.Scale(image.Width),LayoutManager.Scale(image.Height));
				Bitmap bitmap=new Bitmap(image,size);
				butAuthGoogle.Image?.Dispose();
				butAuthGoogle.Image=bitmap;//panelEdgeExpress will dispose of this new Bitmap once the control itself is disposed of as normal
			}
			butAuthGoogle.ResumeLayout();
		}

		private void butDelete_Click(object sender,EventArgs e) {
			if(_isNew) {
				DialogResult=DialogResult.Cancel;
				return;
			}
			if(_emailAddress.EmailAddressNum==PrefC.GetLong(PrefName.EmailDefaultAddressNum)) {
				MsgBox.Show(this,"Cannot delete the default email address.");
				return;
			}
			if(_emailAddress.EmailAddressNum==PrefC.GetLong(PrefName.EmailNotifyAddressNum)) {
				MsgBox.Show(this,"Cannot delete the notify email address.");
				return;
			}
			var clinic=Clinics.GetFirstOrDefault(x => x.EmailAddressId==_emailAddress.EmailAddressNum);
			if(clinic!=null) {
				MessageBox.Show(Lan.g(this,"Cannot delete the email address because it is used by clinic")+" "+clinic.Description);
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete this email address?")) {
				return;
			}
			EmailAddresses.Delete(_emailAddress.EmailAddressNum);
			DataValid.SetInvalid(InvalidType.Email);
			DialogResult=DialogResult.OK;//OK triggers a refresh for the grid.
		}

		private void butRegisterCertificate_Click(object sender,EventArgs e) {
			using FormEmailCertRegister formEmailCertRegister=new FormEmailCertRegister(textUsername.Text);
			formEmailCertRegister.ShowDialog();
		}

		private void butPickUserod_Click(object sender,EventArgs e) {
			var listClinics=Clinics.GetDeepCopy();
 			if(listClinics.Any(x=>x.EmailAddressId==_emailAddress.EmailAddressNum)) {
				MsgBox.Show(this,"Cannot associate a user to an email address that is linked to a clinic.");
				return;
			}
			FrmUserPick frmUserPick=new FrmUserPick();
			frmUserPick.UserNumSuggested=((Userod)textUserod.Tag)?.UserNum??0;//Preselect current selection.
			frmUserPick.ShowDialog();
			if(!frmUserPick.IsDialogOK) {
				return;
			}
			Userod userod=Userods.GetUser(frmUserPick.UserNumSelected);
			if(userod.UserNum==(((Userod)textUserod.Tag)?.UserNum??0)) {
				return;//No change.
			}
			//grabs from the database in case the cache hasn't refreshed
			EmailAddress emailAddress=EmailAddresses.GetForUserDb(userod.UserNum);
			if(emailAddress!=null) {
				MsgBox.Show(this,"User email already exists for "+userod.UserName);
				return;
			}
			textUserod.Tag=userod;
			textUserod.Text=userod.UserName;
		}

		private void butClearTokensGmail_Click(object sender,EventArgs e) {
			if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Do you want to Sign Out of Gmail?")) {
				return;
			}
			_authenticationType=OAuthType.None;
			ClearGmailInfo();
			RefreshEmailTextFields(isEnablingEmailFields:true);
		}

		///<summary>Requests authorization for Open Dental to send emails and access the inbox for a gmail address.
		///Google sends us access and refresh tokens that we store in the database.</summary>
		private void butAuthGoogle_Click(object sender,EventArgs e) {
			if(ODEnvironment.IsCloudServer && !CloudClientL.IsCloudClientRunning()) {
				return;
			}
			Google.AuthorizationRequest authorizationRequest=new Google.AuthorizationRequest();
			GoogleToken googleToken=null;
			string emailAddress=textUsername.Text;
			ProgressWin progressOD=new ProgressWin();
			progressOD.StartingMessage=Lan.g(this,"Searching for an available port")+"...";
			progressOD.ActionMain=() => {
				authorizationRequest.StartListener();
				ODEvent.Fire(ODEventType.ProgressBar,Lan.g(this,"Requesting tokens and waiting for a response from Google")+"...");
				googleToken=authorizationRequest.MakeAccessTokenRequest(emailAddress);
			};
			try {
				progressOD.ShowDialog();
			}
			catch(Exception ex) {
				FriendlyException.Show("Failed to get tokens from Google.",ex);
				return;
			}
			finally {
				authorizationRequest.CloseListener();
			}
			if(progressOD.IsCancelled) {
				return;
			}
			if(googleToken==null) {//This should never happen. An error should be thrown or be in the googleToken.
				MsgBox.Show(this,"Failed to get tokens from Google.");
				return;
			}
			//Errors that occur in WebServiceMainHQ are put in the GoogleToken that is returned.
			if(googleToken.ErrorMessage!="") {
				MsgBox.Show("Error: "+googleToken.ErrorMessage);
				return;
			}
			textAccessTokenGmail.Text=googleToken.AccessToken;
			textRefreshTokenGmail.Text=googleToken.RefreshToken;
			textSMTPserver.Text="smtp.gmail.com";
			textPort.Text="0";
			_authenticationType=OAuthType.Google;
			//prompt the user so they can decide if they want to add additional filters.
			if(!checkDownloadGmail.Checked) {
				if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Do you want to download incoming emails for the Gmail account?")) {
					checkDownloadGmail.Checked=true;
				}
			}
			RefreshEmailTextFields();
		}

		private void butAuthGoogle_MouseEnter(object sender,EventArgs e) {
			butAuthGoogle.Image=Properties.Resources.google_signin_focus;
			butAuthGoogleImageHelper(butAuthGoogle.Image);
		}

		private void butAuthGoogle_MouseLeave(object sender,EventArgs e) {
			butAuthGoogle.Image=Properties.Resources.google_signin_normal;
			butAuthGoogleImageHelper(butAuthGoogle.Image);
		}

		private void butAuthGoogle_MouseDown(object sender,MouseEventArgs e) {
			butAuthGoogle.Image=Properties.Resources.google_signin_pressed;
			butAuthGoogleImageHelper(butAuthGoogle.Image);
		}

		private void butAuthGoogle_MouseUp(object sender,MouseEventArgs e) {
			butAuthGoogle.Image=Properties.Resources.google_signin_normal;
			butAuthGoogleImageHelper(butAuthGoogle.Image);
		}

		private void radioPassword_Click(object sender,EventArgs e) {
			if(_authenticationType==OAuthType.Google && !textAccessTokenGmail.Text.IsNullOrEmpty()) {
				if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Do you want to Sign Out of Gmail?")) {
					radioGmail.Checked=true;
					return;
				}
			}
			_authenticationType=OAuthType.None;
			ClearGmailInfo();
			RefreshEmailTextFields(true);
		}

		private void radioGmail_Click(object sender,EventArgs e) {
			if(_authenticationType==OAuthType.Google) {
				return;
			}
			_authenticationType=OAuthType.None;
			RefreshEmailTextFields();
		}

		private void butSave_Click(object sender, System.EventArgs e) {
			try {
				PIn.Int(textPort.Text);
			}
			catch {
				MsgBox.Show(this,"Invalid outgoing port number.");
				return;
			}
			try {
				PIn.Int(textPortIncoming.Text);
			}
			catch {
				MsgBox.Show(this,"Invalid incoming port number.");
				return;
			}
			if(string.IsNullOrWhiteSpace(textUsername.Text)) {
				MsgBox.Show(this,"Please enter a valid email address.");
				return;
			}
			//Only checks against non-user email addresses.
			if(EmailAddresses.AddressExists(textUsername.Text,_emailAddress.EmailAddressNum)) {
				MsgBox.Show(this,"This email address already exists.");
				return;
			}
			_emailAddress.SMTPserver=PIn.String(textSMTPserver.Text);
			_emailAddress.EmailUsername=PIn.String(textUsername.Text);
			_emailAddress.EmailPassword="";
			_emailAddress.ServerPort=PIn.Int(textPort.Text);
			_emailAddress.UseSSL=checkSSL.Checked;
			_emailAddress.SenderAddress=PIn.String(textSender.Text);
			_emailAddress.Pop3ServerIncoming=PIn.String(textSMTPserverIncoming.Text);
			_emailAddress.ServerPortIncoming=PIn.Int(textPortIncoming.Text);
			_emailAddress.UserNum=((Userod)(textUserod.Tag))?.UserNum??0;
			_emailAddress.AuthenticationType=_authenticationType;
			if(_authenticationType==OAuthType.None) {
				_emailAddress.EmailPassword=PIn.String(MiscUtils.Encrypt(textPassword.Text));
				_emailAddress.AccessToken="";
				_emailAddress.RefreshToken="";
				_emailAddress.DownloadInbox=false;
				_emailAddress.QueryString="";
			}
			else if(_authenticationType==OAuthType.Google) {
				_emailAddress.AccessToken=textAccessTokenGmail.Text;
				_emailAddress.RefreshToken=textRefreshTokenGmail.Text;
				_emailAddress.DownloadInbox=checkDownloadGmail.Checked;
				_emailAddress.QueryString=textParams.Text;
			}
			if(_isNew) {
				EmailAddresses.Insert(_emailAddress);
			}
			else {
				EmailAddresses.Update(_emailAddress);
			}
			DataValid.SetInvalid(InvalidType.Email);
			DialogResult=DialogResult.OK;
		}

	}
}