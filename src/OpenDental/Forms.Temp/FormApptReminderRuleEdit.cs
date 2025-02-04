using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.Forms;

namespace OpenDental;

///<summary>In-memory form. Changes are not saved to the DB from this form.</summary>
public partial class FormApptReminderRuleEdit:FormODBase {
	///<summary>The default appointment reminder rule</summary>
	public ApptReminderRule ApptReminderRuleCur;
	///<summary>Do not edit this. A copy of the passed in default reminder rule.</summary>
	public readonly ApptReminderRule ApptReminderRuleOld;
	///<summary>True if any preferences were updated.</summary>
	public bool IsPrefsChanged;
	private List<CommType> _listCommTypesSendOrder;
	private List<ApptReminderRule> _listApptReminderRulesClinic;
	/// <summary>Public so it can be passed back to the parent form. The list of new language rules that were added from this window. </summary>
	public List<ApptReminderRule> ListApptReminderRulesNonDefaultAdded= [];
	///<summary>A list to keep track of the loosely associated rules that get removed in this form so they can get sync'd properly.</summary>
	public List<ApptReminderRule> ListApptReminderRulesNonDefaultRemoving= [];
	///<summary>Do not edit this. A copied list of the passed in list rules for clinic.</summary>
	public readonly List<ApptReminderRule> ListApptReminderRulesClinicOld= [];
	///<summary>The control that handles the message templates for the default language.</summary>
	private UserControlReminderMessage _userControlReminderMessageDefault;

	public FormApptReminderRuleEdit(ApptReminderRule apptReminderRuleCur,List<ApptReminderRule> listApptReminderRulesClinic=null) {
		InitializeComponent();

		ApptReminderRuleCur=apptReminderRuleCur;
		ApptReminderRuleOld=GenericTools.DeepCopy<ApptReminderRule,ApptReminderRule>(apptReminderRuleCur);
		if(listApptReminderRulesClinic is null) {
			_listApptReminderRulesClinic= [];
		}
		else {
			_listApptReminderRulesClinic=listApptReminderRulesClinic;
		}
		ListApptReminderRulesClinicOld=ListTools.DeepCopy<ApptReminderRule,ApptReminderRule>(listApptReminderRulesClinic);
	}

	///<summary>A list that holds associated language rules for this rule.</summary>
	public List<ApptReminderRule> GetListLanguageRules() {
		return _listApptReminderRulesClinic.FindAll(x => x.TSPrior==ApptReminderRuleCur.TSPrior
		                                                 && x.ClinicNum==ApptReminderRuleCur.ClinicNum
		                                                 && x.TypeCur==ApptReminderRuleCur.TypeCur
		                                                 && x.Language!="");
	}

	private void FormApptReminderRuleEdit_Load(object sender,EventArgs e) {
		Text=Lan.g(this,"Edit")+" "+ApptReminderRuleCur.TypeCur.GetDescription(true)+" "+Lan.g(this,"Rule");
		checkEnabled.Checked=ApptReminderRuleCur.IsEnabled;
		checkEConfirmationAutoReplies.Checked=ApptReminderRuleCur.IsAutoReplyEnabled;
		labelTags.Text=Lan.g(this,"Use the following replacement tags to customize messages : ")
		               +string.Join(", ",ApptReminderRules.GetAvailableTags(ApptReminderRuleCur.TypeCur));
		textPatientPortalLastVisit.Text=ApptReminderRuleCur.TimeSpanMultipleInvites.Days.ToString();
		switch(ApptReminderRuleCur.SendMultipleInvites) {
			case SendMultipleInvites.UntilPatientVisitsPortal:
				textPatientPortalLastVisit.Enabled=false;
				radioSendPatientPortalInviteOnce.Checked=true;
				break;
			case SendMultipleInvites.EveryAppointment:
				textPatientPortalLastVisit.Enabled=false;
				radioSendPatientPortalInviteMultiple.Checked=true;
				break;
			case SendMultipleInvites.NoVisitInTimespan:
				textPatientPortalLastVisit.Enabled=true;
				radioSendPatientPortalInviteNoVisit.Checked=true;
				break;
		}
		if(ApptReminderRuleCur.TypeCur==ApptReminderType.PatientPortalInvite) {
			groupPatientPortalInvites.Visible=true;
		}
		if(ApptReminderRuleCur.TypeCur.In(ApptReminderType.PatientPortalInvite,ApptReminderType.Arrival)) {
			checkSendAll.Visible=false;
		}
		if(!ApptReminderRuleCur.TypeCur.In(ApptReminderType.ConfirmationFutureDay,ApptReminderType.Arrival)) {
			checkEConfirmationAutoReplies.Visible=false;
		}
		//case where ArrLanguages is empty or is unspecified, hide the add language button
		var languagesUsedByPatients=PrefC.GetString(PrefName.LanguagesUsedByPatients).Split(',');
		if(languagesUsedByPatients.Length==0) {
			butLanguage.Visible=false;
		}
		if(languagesUsedByPatients.Length==1) {
			if(languagesUsedByPatients[0].ToLower().Trim()==PrefC.GetString(PrefName.LanguagesIndicateNone).ToLower().Trim()) {
				butLanguage.Visible=false;
			}
		}
		if(GetListLanguageRules().Count==0) {
			butRemove.Visible=false;
		}
		_listCommTypesSendOrder=ApptReminderRuleCur.SendOrder.Split(',').Select(x => (CommType)SIn.Int(x)).ToList();
		if(Clinics.IsSecureEmailEnabled(ApptReminderRuleCur.ClinicNum)) { 
			checkSendSecureEmail.Enabled=true;
			checkSendSecureEmail.Checked=_listCommTypesSendOrder.Contains(CommType.SecureEmail);
		}
		else {//Secure email not enabled
			checkSendSecureEmail.Enabled=false;
			checkSendSecureEmail.Checked=false;
		}
		FillGridPriority();
		FillTimeSpan();
		FillTabs();
		checkSendAll.Checked=ApptReminderRuleCur.IsSendAll;
	}
		
	private void FillTabs() {
		_userControlReminderMessageDefault=new UserControlReminderMessage(ApptReminderRuleCur);
		_userControlReminderMessageDefault.Anchor=((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
		                                           | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom);
		var listApptReminderRulesLanguage=GetListLanguageRules();
		//using languages
		_userControlReminderMessageDefault.Dock=DockStyle.Fill;
		tabPageDefault.Tag=ApptReminderRuleCur;
		_userControlReminderMessageDefault.Controls.Add(tabPageDefault);
		for(var i = 0; i < listApptReminderRulesLanguage.Count; i++) {
			var tabPagelanguage=new UI.TabPage();
			var cultureInfo=MiscUtils.GetCultureFromThreeLetter(listApptReminderRulesLanguage[i].Language);
			if(cultureInfo==null) {
				tabPagelanguage.Text=listApptReminderRulesLanguage[i].Language;
			}
			else {
				tabPagelanguage.Text=cultureInfo.DisplayName;
			}
			tabPagelanguage.Tag=listApptReminderRulesLanguage[i];
			tabControl.Controls.Add(tabPagelanguage);
			var userControlReminderMessageLanguage=new UserControlReminderMessage(listApptReminderRulesLanguage[i]);
			userControlReminderMessageLanguage.Anchor=_userControlReminderMessageDefault.Anchor;
			userControlReminderMessageLanguage.Dock=DockStyle.Fill;
			userControlReminderMessageLanguage.Controls.Add(tabPagelanguage);
		}
	}

	private void FillTimeSpan() {
		textHours.Text=Math.Abs(ApptReminderRuleCur.TSPrior.Hours).ToString();//Hours, not total hours.
		textDays.Text=Math.Abs(ApptReminderRuleCur.TSPrior.Days).ToString();//Days, not total Days.
		if(ApptReminderRuleCur.TypeCur!=ApptReminderType.PatientPortalInvite) {
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.GeneralMessage) {
				//Controls are enabled by default, disable these controls for after appointment rules (GeneralMessages, PatientPortalInvites)
				//Disregarding PatientPortalInvites here since radioBeforeAfterAppt_CheckedChanged() takes care of it.
				EnableWithinDaysAndHoursControls(enable: false);
			}
			radioBeforeAppt.Visible=false;
			radioAfterAppt.Visible=false;
		}
		else if(ApptReminderRuleCur.TSPrior>=TimeSpan.Zero) {
			radioBeforeAppt.Checked=true;
		}
		else if(ApptReminderRuleCur.TSPrior<TimeSpan.Zero) {
			radioAfterAppt.Checked=true;
		}
		switch(ApptReminderRuleCur.TypeCur) {
			case ApptReminderType.Arrival:
			case ApptReminderType.Reminder:
			case ApptReminderType.ConfirmationFutureDay:
				groupSendTime.Text=Lan.g(this,"Send Time - before appointment.");
				break;
			case ApptReminderType.ScheduleThankYou:
			case ApptReminderType.NewPatientThankYou:
				groupSendTime.Text=Lan.g(this,"Send Time - after appointment scheduled.");
				break;
			case ApptReminderType.GeneralMessage:
				groupSendTime.Text=Lan.g(this,"Send Time - after appointment.");
				break;
			//Patient Portal Invite will just say "Send Time" since customers can select before / after appointment.
		}
		if(ApptReminderRuleCur.DoNotSendWithin.Days > 0) {
			textDaysWithin.Text=ApptReminderRuleCur.DoNotSendWithin.Days.ToString();
		}
		if(ApptReminderRuleCur.DoNotSendWithin.Hours > 0) {
			textHoursWithin.Text=ApptReminderRuleCur.DoNotSendWithin.Hours.ToString();
		}
		UpdateDoNotSendWithinLabel();
	}

	private void FillGridPriority() {
		gridPriorities.BeginUpdate();
		gridPriorities.Columns.Clear();
		gridPriorities.Columns.Add(new GridColumn("",50){ IsWidthDynamic=true });
		gridPriorities.ListGridRows.Clear();
		for(var i=0;i<_listCommTypesSendOrder.Count;i++) {
			var commType=_listCommTypesSendOrder[i];
			GridRow gridRow;
			if(commType==CommType.Preferred) {
				if(checkSendAll.Checked) {
					//"Preferred" is irrelevant when SendAll is checked.
					continue;
				}
				gridRow=new GridRow();
				gridRow.Cells.Add(Lan.g(this,"Preferred Confirm Method"));
				gridPriorities.ListGridRows.Add(gridRow);
				continue;
			}
			if(commType==CommType.Text && !SmsPhones.IsIntegratedTextingEnabled()) {
				gridRow=new GridRow();
				gridRow.Cells.Add(Lan.g(this,commType.ToString())+" ("+Lan.g(this,"Not Configured")+")");
				gridRow.ColorBackG=Color.LightGray;
				gridPriorities.ListGridRows.Add(gridRow);
			}
			else {
				gridRow=new GridRow();
				gridRow.Cells.Add(Lan.g(this,commType.GetDescription()));
				gridPriorities.ListGridRows.Add(gridRow);
			}
		}
		gridPriorities.EndUpdate();
	}

	/// <summary>Enables or disables the fields associated with the 'Do Not Send Within' label based on the boolean passed in.</summary>
	private void EnableWithinDaysAndHoursControls(bool enable) {
		labelDoNotSendWithin.Enabled=enable;
		labelDaysWithin.Enabled=enable;
		labelHoursWithin.Enabled=enable;
		textDaysWithin.Enabled=enable;
		textHoursWithin.Enabled=enable;
	}

	private void radioBeforeAfterAppt_CheckedChanged(object sender,EventArgs e) {
		var enabled=radioBeforeAppt.Checked || ApptReminderRuleCur.TypeCur.In(ApptReminderType.ScheduleThankYou, ApptReminderType.NewPatientThankYou);
		EnableWithinDaysAndHoursControls(enabled);
	}

	private void radioSendPatientPortalInviteOnce_Click(object sender,EventArgs e) {
		textPatientPortalLastVisit.Enabled=false;
	}

	private void radioSendPatientPortalInviteMultiple_Click(object sender,EventArgs e) {
		textPatientPortalLastVisit.Enabled=false;
	}

	private void radioSendPatientPortalInviteNoVisit_Click(object sender,EventArgs e) {
		textPatientPortalLastVisit.Enabled=true;
	}

	private void textDoNotSendWithin_TextChanged(object sender,EventArgs e) {
		UpdateDoNotSendWithinLabel();
	}

	private void UpdateDoNotSendWithinLabel() {
		var daysHoursTxt="";
		var daysWithin=SIn.Int(textDaysWithin.Text,false);
		var hoursWithin=SIn.Int(textHoursWithin.Text,false);
		if(!textDaysWithin.IsValid() || !textHoursWithin.IsValid()
		                             || (daysWithin==0 && hoursWithin==0)) 
		{
			daysHoursTxt="_____________";
		}
		else {
			if(daysWithin==1) {
				daysHoursTxt+=daysWithin+" "+Lans.g("day");
			}
			else if(daysWithin > 1) {
				daysHoursTxt+=daysWithin+" "+Lans.g("days");
			}
			if(daysWithin > 0 && hoursWithin > 0) {
				daysHoursTxt+=" ";
			}
			if(hoursWithin==1) {
				daysHoursTxt+=hoursWithin+" "+Lans.g("hour");
			}
			else if(hoursWithin > 1) {
				daysHoursTxt+=hoursWithin+" "+Lans.g("hours");
			}
		}
		labelDoNotSendWithin.Text=Lans.g("Do not send within")+" "+daysHoursTxt+" "+Lans.g("of appointment");
	}

	private void butUp_Click(object sender,EventArgs e) {
		var idx = gridPriorities.GetSelectedIndex();
		if(idx<1) {
			//-1 if nothing selected. 0 if top item selected.
			return;
		}
		_listCommTypesSendOrder.Reverse(idx-1,2);
		FillGridPriority();
		gridPriorities.SetSelected(idx-1,true);
	}

	private void butDown_Click(object sender,EventArgs e) {
		var idx = gridPriorities.GetSelectedIndex();
		if(idx==-1 || idx==_listCommTypesSendOrder.Count-1) {
			//-1 nothing selected. Count-1 if last item selected.
			return;
		}
		_listCommTypesSendOrder.Reverse(idx,2);
		FillGridPriority();
		gridPriorities.SetSelected(idx+1,true);
	}

	private void butAdvanced_Click(object sender,EventArgs e) {
		var listApptReminderRulesOldAndNew=new List<ApptReminderRule>();
		listApptReminderRulesOldAndNew.AddRange(GetListLanguageRules());
		listApptReminderRulesOldAndNew.AddRange(ListApptReminderRulesNonDefaultAdded);
		var selectedLanguage="";
		if(tabControl.TabPages.Count > 1) {
			selectedLanguage=((ApptReminderRule)tabControl.SelectedTab.Tag).Language;
		}
		using var formApptReminderRuleAggEdit=new FormApptReminderRuleAggEdit(ApptReminderRuleCur,listApptReminderRulesOldAndNew,selectedLanguage);
		formApptReminderRuleAggEdit.ShowDialog();
	}

	///<summary>Removes 'Do not send eConfirmations' from the confirmed status for 'eConfirm Sent' if multiple eConfirmations are set up.</summary>
	private void CheckMultipleEConfirms() {
		var countEConfirm=_listApptReminderRulesClinic?.Count(x => x.TypeCur==ApptReminderType.ConfirmationFutureDay && x.Language=="")??0;
		var confStatusEConfirmSent=Defs.GetDef(DefCat.ApptConfirmed,PrefC.GetLong(PrefName.ApptEConfirmStatusSent)).ItemName;
		var listExclude=PrefC.GetString(PrefName.ApptConfirmExcludeESend)
			.Split([','],StringSplitOptions.RemoveEmptyEntries).ToList();
		if(ApptReminderRuleCur.TypeCur==ApptReminderType.ConfirmationFutureDay
		   //And there is more than 1 eConfirmation rule.
		   && (countEConfirm > 1 || (countEConfirm==1 && ApptReminderRuleCur.ApptReminderRuleNum==0))
		   //And the confirmed status for 'eConfirm Sent' is marked 'Do not send eConfirmations'
		   && listExclude.Contains(PrefC.GetString(PrefName.ApptEConfirmStatusSent))
		   //Ask them to fix their exclude send statuses
		   && ODMessageBox.Show(Lans.g("Appointments will not receive multiple eConfirmations if the '")+confStatusEConfirmSent+"' "+
		                        Lans.g("status is set as 'Don't Send'. Would you like to remove 'Don't Send' from that status?"),
			   "",MessageBoxButtons.YesNo)==DialogResult.Yes) 
		{
			listExclude.RemoveAll(x => x==PrefC.GetString(PrefName.ApptEConfirmStatusSent));
			IsPrefsChanged|=Prefs.UpdateString(PrefName.ApptConfirmExcludeESend,string.Join(",",listExclude));
		}
	}

	private void butLanguage_Click(object sender,EventArgs e) {
		var listLanguagesDisplay=new List<string>();//contains the user friendly name ex: "Spanish"
		var listLanguagesData=new List<string>();//contains the db friendly string ex: "SPA"
		var listApptReminderRulesCurrentControl=new List<ApptReminderRule>();
		for( var i=0; i<tabControl.TabPages.Count;i++) {
			listApptReminderRulesCurrentControl.Add((ApptReminderRule)tabControl.TabPages[i].Tag);
		}
		var languagesUsedbyPatients=PrefC.GetString(PrefName.LanguagesUsedByPatients).Split(',');
		for(var i = 0; i < languagesUsedbyPatients.Length; i++) {
			if(languagesUsedbyPatients[i].IsNullOrEmpty() || languagesUsedbyPatients[i].ToLower().Trim()==PrefC.GetString(PrefName.LanguagesIndicateNone).ToLower().Trim()) {
				continue;
			}
			if(listApptReminderRulesCurrentControl.Select(x => x.Language).ToList().Contains(languagesUsedbyPatients[i]))	{
				continue;//we already have a tab for this langauge. 
			}
			var cultureInfo=MiscUtils.GetCultureFromThreeLetter(languagesUsedbyPatients[i]);
			if(cultureInfo==null) {
				listLanguagesDisplay.Add(languagesUsedbyPatients[i]);//custom language - display what the user entered
			}
			else {
				listLanguagesDisplay.Add(cultureInfo.DisplayName);//display full name of the abbreviation
			}
			listLanguagesData.Add(languagesUsedbyPatients[i]);
		}
		if(listLanguagesDisplay.Count==0) {
			MsgBox.Show(this,"No additional languages available.");
			return;
		}
		var inputBoxlanguageSelect=new InputBox(Lan.g(this,"Select language for template: "),listLanguagesDisplay,0);
		inputBoxlanguageSelect.ShowDialog();
		if(inputBoxlanguageSelect.IsDialogCancel) {
			return;
		}
		var apptReminderRule=GenericTools.DeepCopy<ApptReminderRule,ApptReminderRule>(ApptReminderRuleCur);
		apptReminderRule.ApptReminderRuleNum=0;
		apptReminderRule.Language=listLanguagesData[inputBoxlanguageSelect.SelectedIndex];
		var tabPageLanguage=new OpenDental.UI.TabPage();
		tabPageLanguage.Tag=apptReminderRule;
		tabPageLanguage.Text=listLanguagesDisplay[inputBoxlanguageSelect.SelectedIndex];
		tabControl.Controls.Add(tabPageLanguage);
		var userControlReminderMessageRule=new UserControlReminderMessage(apptReminderRule);
		userControlReminderMessageRule.Anchor=tabPageDefault.Controls[0].Anchor;
		userControlReminderMessageRule.Dock=DockStyle.Fill;
		userControlReminderMessageRule.Controls.Add(tabPageLanguage);
		tabControl.SelectedTab=tabPageLanguage;
		ListApptReminderRulesNonDefaultAdded.Add(apptReminderRule);
		butRemove.Visible=true;
	}

	private void butRemove_Click(object sender,EventArgs e) {
		//Don't delete the default.
		if(((ApptReminderRule)tabControl.SelectedTab.Tag).Language=="") {
			MsgBox.Show(this,"Cannot remove the default template.");
			return;
		}
		if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Delete the currently selected language?")) {
			return;
		}
		var apptReminderRuleRemoving=(ApptReminderRule)tabControl.SelectedTab.Tag;
		if(!ListApptReminderRulesNonDefaultAdded.Contains(apptReminderRuleRemoving)) {
			ListApptReminderRulesNonDefaultRemoving.Add(apptReminderRuleRemoving);
		}
		ListApptReminderRulesNonDefaultAdded.Remove(apptReminderRuleRemoving);
		tabControl.TabPages.Remove(tabControl.SelectedTab);
	}

	private void CheckSendSecureEmail_Click(object sender,EventArgs e) {
		EmailCommTypeSwap();
		FillGridPriority();
	}

	private void EmailCommTypeSwap() {
		for(var i=0;i<_listCommTypesSendOrder.Count;i++) {
			if(checkSendSecureEmail.Checked) { 
				if(_listCommTypesSendOrder[i]==CommType.Email) { 
					_listCommTypesSendOrder[i]=CommType.SecureEmail;
				}
			}
			else {//not checked
				if(_listCommTypesSendOrder[i]==CommType.SecureEmail) { 
					_listCommTypesSendOrder[i]=CommType.Email;
				}
			}
		}
	}

	private void butSave_Click(object sender,EventArgs e) {
		var hasValidNums=UIHelper.GetAllControls(this).OfType<ValidNum>().All(x => x.IsValid());
		if(!hasValidNums) {
			MsgBox.Show(this,"Fix data entry errors first.");
			return;
		}
		if(!ValidateRule()) {
			return;
		}
		if(ContainsShortURLs()) {
			return;
		}
		var timeSpanPrior=new TimeSpan(SIn.Int(textDays.Text,false),SIn.Int(textHours.Text,false),0,0);
		if(ApptReminderRuleCur.TypeCur==ApptReminderType.PatientPortalInvite && !radioBeforeAppt.Checked) {
			timeSpanPrior=timeSpanPrior.Negate();
		}
		else if(ApptReminderRuleCur.TypeCur.In(ApptReminderType.ScheduleThankYou,ApptReminderType.GeneralMessage,ApptReminderType.NewPatientThankYou)) {
			timeSpanPrior=timeSpanPrior.Negate();
		}
		if(_listApptReminderRulesClinic.Any(x => x.TypeCur!=ApptReminderRuleCur.TypeCur && x.TSPrior==timeSpanPrior && x.IsEnabled
		                                         && x.IsAfterApptScheduled==ApptReminderRuleCur.IsAfterApptScheduled))
		{
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"There are multiple rules for sending at this send time. Are you sure you want to send multiple "
			                                            +"messages at the same time?")) 
			{
				return;
			}
		}
		if(_listApptReminderRulesClinic.FindAll(
			   x => x.TSPrior==timeSpanPrior 
			        && x.ClinicNum==ApptReminderRuleCur.ClinicNum
			        && x.TypeCur==ApptReminderRuleCur.TypeCur
			        && (x.ApptReminderRuleNum!=ApptReminderRuleCur.ApptReminderRuleNum || 
			            (x.ApptReminderRuleNum==0 && ApptReminderRuleCur.ApptReminderRuleNum==0 && x!=ApptReminderRuleCur)
			        )
			        && x.Language=="").Count>0) 
		{
			MsgBox.Show(this,"Not allowed to create a duplicate rule for the same send time.");
			return;
		}
		CheckMultipleEConfirms();
		ApptReminderRuleCur.SendOrder=string.Join(",",_listCommTypesSendOrder.Select(x => ((int)x).ToString()).ToArray());
		ApptReminderRuleCur.IsSendAll=checkSendAll.Checked;
		ApptReminderRuleCur.TSPrior=timeSpanPrior;
		if(radioBeforeAppt.Checked || ApptReminderRules.IsReminderTypeAlwaysSendBefore(ApptReminderRuleCur.TypeCur)) {
			ApptReminderRuleCur.DoNotSendWithin=new TimeSpan(SIn.Int(textDaysWithin.Text,false),SIn.Int(textHoursWithin.Text,false),0,0);
		}
		ApptReminderRuleCur.IsEnabled=checkEnabled.Checked;
		ApptReminderRuleCur.IsAutoReplyEnabled=checkEConfirmationAutoReplies.Checked;
		ApptReminderRuleCur.TimeSpanMultipleInvites=TimeSpan.FromDays(SIn.Int(textPatientPortalLastVisit.Text,false));
		if(radioSendPatientPortalInviteOnce.Checked) {
			ApptReminderRuleCur.SendMultipleInvites=SendMultipleInvites.UntilPatientVisitsPortal;
		}
		else if(radioSendPatientPortalInviteMultiple.Checked) {
			ApptReminderRuleCur.SendMultipleInvites=SendMultipleInvites.EveryAppointment;
		}
		else if(radioSendPatientPortalInviteNoVisit.Checked) {
			ApptReminderRuleCur.SendMultipleInvites=SendMultipleInvites.NoVisitInTimespan;
		}
		_userControlReminderMessageDefault.SaveControlTemplates();
		if(tabControl.TabPages.Count<=1) {
			DialogResult=DialogResult.OK;
			return;
		}
		//handle additional language rules
		for(var i = 0; i < tabControl.TabPages.Count; i++) {
			var userControlReminderMessage=(UserControlReminderMessage)tabControl.TabPages[i].Controls[0];//update the additional languages to match
			if(userControlReminderMessage.Rule.Language=="") {
				continue;//this is the default, which we already took care of.
			}
			userControlReminderMessage.Rule.TSPrior=ApptReminderRuleCur.TSPrior;
			userControlReminderMessage.Rule.IsAutoReplyEnabled=ApptReminderRuleCur.IsAutoReplyEnabled;
			userControlReminderMessage.Rule.IsEnabled=ApptReminderRuleCur.IsEnabled;
			userControlReminderMessage.Rule.SendOrder=ApptReminderRuleCur.SendOrder;
			userControlReminderMessage.Rule.IsSendAll=ApptReminderRuleCur.IsSendAll;
			userControlReminderMessage.Rule.DoNotSendWithin=ApptReminderRuleCur.DoNotSendWithin;
			userControlReminderMessage.Rule.SendMultipleInvites=ApptReminderRuleCur.SendMultipleInvites;
			userControlReminderMessage.Rule.TimeSpanMultipleInvites=ApptReminderRuleCur.TimeSpanMultipleInvites;
			userControlReminderMessage.SaveControlTemplates();
		}
		DialogResult=DialogResult.OK;
	}

	private bool ValidateRule() {
		if(!checkEnabled.Checked) {
			return true;
		}
		var errors = new List<string>();
		if(butLanguage.Visible==false) {
			errors.AddRange(_userControlReminderMessageDefault.ValidateTemplates());
		}
		else{ 
			for(var i=0;i<tabControl.TabPages.Count;i++) {
				errors.AddRange(((UserControlReminderMessage)tabControl.TabPages[i].Controls[0]).ValidateTemplates());
			}
		}
		if(SIn.Int(textDays.Text,false)>=366) {
			errors.Add(Lan.g(this,"Lead time must 365 days or less."));
		}
		//ScheduleThankYou, NewPatientThankYou, and GeneralMessage can be 0, meaning send immediately. ConfirmationFutureDay has a separate check so exclude it here.
		if(checkEnabled.Checked && SIn.Int(textHours.Text,false)==0 
		                        && SIn.Int(textDays.Text,false)==0 
		                        && !ApptReminderRuleCur.TypeCur.In(ApptReminderType.ScheduleThankYou,ApptReminderType.PatientPortalInvite,ApptReminderType.NewPatientThankYou,ApptReminderType.GeneralMessage,ApptReminderType.ConfirmationFutureDay))
		{
			errors.Add(Lan.g(this,"Lead time must be greater than 0 hours."));
		}
		if(ApptReminderRuleCur.TypeCur==ApptReminderType.ConfirmationFutureDay) {
			if(SIn.Int(textDays.Text,false)==0) {
				errors.Add(Lan.g(this,"Lead time must be greater than or equal to 1 day for confirmations."));
			}
		}
		if(radioBeforeAppt.Checked || ApptReminderRules.IsReminderTypeAlwaysSendBefore(ApptReminderRuleCur.TypeCur)) {
			var timeSpanPrior=new TimeSpan(SIn.Int(textDays.Text,false),SIn.Int(textHours.Text,false),0,0);
			var timeSpanDoNotSendWithin=new TimeSpan(SIn.Int(textDaysWithin.Text,false),SIn.Int(textHoursWithin.Text,false),0,0);
			//If we set the autocomm to be sent 1 hour prior to the appointment but DoNotSend to 2 hours prior, that wouldn't make a whole lot of sense, now would it?
			//That being said, this doesn't apply for Thank Yous because you send depending on when the patient scheduled not the scheduled time of the appointment
			if(timeSpanDoNotSendWithin >= timeSpanPrior && !ApptReminderRuleCur.TypeCur.In(ApptReminderType.ScheduleThankYou,ApptReminderType.NewPatientThankYou)) {
				errors.Add(Lan.g(this,"'Send Time' must be greater than 'Do Not Send Within' time."));
			}
		}
		if(errors.Count>0) {
			ODMessageBox.Show(Lan.g(this,"You must fix the following errors before continuing.")+"\r\n\r\n-"+string.Join("\r\n-",errors));
			return false;
		}
		return true;
	}

	private bool ContainsShortURLs() {
		var listUserControlReminderMessages=tabControl.TabPages.Select(x=>x.Controls).OfType<UserControlReminderMessage>().ToList();
		var listMessages=listUserControlReminderMessages.Select(x => x.TemplateSms).ToList();
		var aggMessages=string.Join(" ",listMessages);
		var firstShortURL=PrefC.GetFirstShortUrl(aggMessages);
		if(string.IsNullOrWhiteSpace(firstShortURL)) {
			return false;
		}
		var errorMessage=Lan.g(this,"Message cannot contain the URL")+$" {firstShortURL} "+Lan.g(this,"as these are only allowed for eServices.");
		ODMessageBox.Show(errorMessage);
		return true;
	}

	private void checkSendAll_CheckedChanged(object sender,EventArgs e) {
		butUp.Enabled=!checkSendAll.Checked;
		butDown.Enabled=!checkSendAll.Checked;
		gridPriorities.Enabled=!checkSendAll.Checked;
		gridPriorities.SetAll(false);
		FillGridPriority();
	}

	private void butCancel_Click(object sender,EventArgs e) {
		DialogResult=DialogResult.Cancel;
	}

	private void butDelete_Click(object sender,EventArgs e) {
		var listApptReminderRulesLanguage=GetListLanguageRules();
		for(var i = 0; i < listApptReminderRulesLanguage.Count; i++) {
			if(!ListApptReminderRulesNonDefaultRemoving.Contains(listApptReminderRulesLanguage[i]) && !ListApptReminderRulesNonDefaultAdded.Contains(listApptReminderRulesLanguage[i])) {
				ListApptReminderRulesNonDefaultRemoving.Add(listApptReminderRulesLanguage[i]);
			}
		}
		ListApptReminderRulesNonDefaultAdded.Clear();
		ApptReminderRuleCur=null;
		DialogResult=DialogResult.OK;
	}
}