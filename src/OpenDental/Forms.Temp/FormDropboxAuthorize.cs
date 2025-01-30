using System;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDentBusiness.Remoting;

namespace OpenDental;

public partial class FormDropboxAuthorize:FormODBase {
		
	public ProgramProperty ProgramPropertyAccessToken;

	public FormDropboxAuthorize() {
		InitializeComponent();
	}

	private void FormDropboxAuthorize_Load(object sender,EventArgs e) {
		try {
			var regKey=PrefC.GetString(PrefName.RegistrationKey);
			var iWebServiceMainHQ=WebServiceMainHQProxy.GetWebServiceMainHQInstance();
			var urlPrimitive=iWebServiceMainHQ.BuildOAuthUrl(regKey,OAuthApplicationNames.Dropbox.ToString());
			//In OpenDentalWebApps, see WebServiceMainHQ.asmx.cs, BuildOAuthUrl().
			var url=WebSerializer.DeserializePrimitiveOrThrow<string>(urlPrimitive);
			System.Diagnostics.Process.Start(url);
		}
		catch(Exception ex) {
			ODMessageBox.Show(Lan.g(this,"Error:")+"  "+ex.Message);
		}
	}

	private void butSave_Click(object sender,EventArgs e) {
		try {
			var accessTokenFinal=WebSerializer.DeserializePrimitiveOrThrow<string>(
				WebServiceMainHQProxy.GetWebServiceMainHQInstance().GetDropboxAccessToken(WebSerializer.SerializePrimitive<string>(textAccessToken.Text)));
			ProgramPropertyAccessToken.PropertyValue=accessTokenFinal;
		}
		catch(Exception ex) {
			ODMessageBox.Show(Lan.g(this,"Error:")+"  "+ex.Message);
			return;
		}
		ProgramProperties.Update(ProgramPropertyAccessToken);
		DataValid.SetInvalid(InvalidType.Programs);
		DialogResult=DialogResult.OK;
	}

}