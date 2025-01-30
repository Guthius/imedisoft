using CodeBase;
using Newtonsoft.Json;
using OpenDentBusiness;
using OpenDentBusiness.AutoComm;
using OpenDentBusiness.WebTypes.WebApps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental;

public partial class FormEServicesPaymentPortal:FormODBase {
	private ApptReminderRule _apptReminderRule;
	private UserControlReminderMessage _userControlReminderMessage;
	private MsgToPayEmailTemplate _msgToPayEmailTemplate;
	private MsgToPayEmailTemplate _msgToPayEmailTemplateAppt;

	public FormEServicesPaymentPortal() {
		InitializeComponent();

		textPatientFacingPaymentUrl.Text=WebAppUtil.GetWebAppUrl(eServiceCode.PaymentPortalUI,Clinics.ClinicNum);
	}

	private void FormEServicesPaymentPortal_Load(object sender,EventArgs e) {
		//Disable all controls if user does not have EServicesSetup permission.
		if(!Security.IsAuthorized(EnumPermType.EServicesSetup,suppressMessage:true)) {
			DisableAllExcept();
		}
		labelTags.Text=Lan.g(this,"Use the following replacement tags to customize messages : ")+string.Join(", ",ApptReminderRules.GetAvailableTags(ApptReminderType.PayPortalMsgToPay));
		#region Regular Statement
		_apptReminderRule=new ApptReminderRule();
		_apptReminderRule.TemplateSMS=PrefC.GetString(PrefName.PaymentPortalMsgToPayTextMessageTemplate);
		_apptReminderRule.TemplateEmailSubject=PrefC.GetString(PrefName.PaymentPortalMsgToPaySubjectTemplate);
		ODException.SwallowAnyException(() =>
			_msgToPayEmailTemplate=JsonConvert.DeserializeObject<MsgToPayEmailTemplate>(PrefC.GetString(PrefName.PaymentPortalMsgToPayEmailMessageTemplate))
		);
		if(_msgToPayEmailTemplate==null) {
			_msgToPayEmailTemplate=new MsgToPayEmailTemplate();
			_msgToPayEmailTemplate.Template="";
		}
		_apptReminderRule.TemplateEmail=_msgToPayEmailTemplate.Template;
		_apptReminderRule.EmailTemplateType=_msgToPayEmailTemplate.EmailType;
		_userControlReminderMessage=new UserControlReminderMessage(_apptReminderRule,doCheckDisclaimer:false);
		_userControlReminderMessage.Anchor=System.Windows.Forms.AnchorStyles.Top|System.Windows.Forms.AnchorStyles.Left|System.Windows.Forms.AnchorStyles.Right|System.Windows.Forms.AnchorStyles.Bottom;
		_userControlReminderMessage.Dock=DockStyle.Fill;
		_userControlReminderMessage.Location=new Point(0,0);
		_userControlReminderMessage.Size=new Size(498,315);
		_userControlReminderMessage.Controls.Add(groupBoxUserControlFill);
		_userControlReminderMessage.Size=_userControlReminderMessage.Size;
		#endregion Regular Statement
		comboBoxPayTypeM2P.Items.AddDefNone("("+Lan.g(this,"default")+")");
		comboBoxPayTypeM2P.Items.AddDefs(Defs.GetDefsForCategory(DefCat.PaymentTypes,true));
		comboBoxPayTypeM2P.SetSelectedDefNum(PrefC.GetLong(PrefName.PayTypeMessageToPay));
	}

	private void comboClinicPicker_SelectedIndexChanged(object sender,EventArgs e) {
		if(comboClinicPicker.ClinicNumSelected==-1) {
			return;
		}
		textPatientFacingPaymentUrl.Text=WebAppUtil.GetWebAppUrl(eServiceCode.PaymentPortalUI,comboClinicPicker.ClinicNumSelected);
	}

	///<summary>Validates sms and email templates if they are set. Returns false if template contains any Redirect URLs or if Template is missing the msg to pay tag.</summary>
	private bool IsTemplateValid() {
		var listErrors=new List<string>();
		//Email Template
		if(!string.IsNullOrWhiteSpace(_apptReminderRule.TemplateEmail)) {
			if(!_apptReminderRule.TemplateEmail.Contains(MsgToPayTagReplacer.MSG_TO_PAY_TAG)) {
				listErrors.Add(Lan.g(this,"Your Message-to-Pay Email Template must contain")+$" {MsgToPayTagReplacer.MSG_TO_PAY_TAG} ");
			}
			var emailErrorText=PrefC.GetFirstShortURL(_apptReminderRule.TemplateEmail);
			if(!string.IsNullOrWhiteSpace(emailErrorText)) {
				listErrors.Add(Lan.g(this,"Email Message Template cannot contain the URL")+" "+emailErrorText+" "+Lan.g(this,"as this is only allowed for eServices."));
			}
		}
		//SMS Template
		if(!string.IsNullOrWhiteSpace(_apptReminderRule.TemplateSMS)) {
			if(!_apptReminderRule.TemplateSMS.Contains(MsgToPayTagReplacer.MSG_TO_PAY_TAG)) {
				listErrors.Add(Lan.g(this,"Your Message-to-Pay Text Template must contain")+$" {MsgToPayTagReplacer.MSG_TO_PAY_TAG} ");
			}
			var smsErrorText=PrefC.GetFirstShortURL(_apptReminderRule.TemplateSMS);
			if(!string.IsNullOrWhiteSpace(smsErrorText)) {
				listErrors.Add(Lan.g(this,"Text Message Template cannot contain the URL")+" "+smsErrorText+" "+Lan.g(this,"as this is only allowed for eServices."));
			}
		}
		if(!listErrors.IsNullOrEmpty()) {
			ODMessageBox.Show(Lan.g(this,"Please fix the following errors before continuing:\r\n")+string.Join("\r\n",listErrors));
		}
		return listErrors.IsNullOrEmpty();
	}

	private void butSave_Click(object sender,EventArgs e) {
		//Saving form information to the rule object and then validating
		_userControlReminderMessage.SaveControlTemplates();
		if(!IsTemplateValid()) {
			return;
		}
		var didUpdate=false;
		//Setting object for json serialization in pref
		_msgToPayEmailTemplate.Template=_apptReminderRule.TemplateEmail;
		_msgToPayEmailTemplate.EmailType=_apptReminderRule.EmailTemplateType;
		var emailMessageTemplate=JsonConvert.SerializeObject(_msgToPayEmailTemplate);
		didUpdate|=Prefs.UpdateString(PrefName.PaymentPortalMsgToPayEmailMessageTemplate,emailMessageTemplate);
		didUpdate|=Prefs.UpdateString(PrefName.PaymentPortalMsgToPaySubjectTemplate,_apptReminderRule.TemplateEmailSubject);
		didUpdate|=Prefs.UpdateString(PrefName.PaymentPortalMsgToPayTextMessageTemplate,_apptReminderRule.TemplateSMS);
		didUpdate|=Prefs.UpdateLong(PrefName.PayTypeMessageToPay,comboBoxPayTypeM2P.GetSelectedDefNum());
		if(didUpdate) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		DialogResult=DialogResult.OK;
	}
}