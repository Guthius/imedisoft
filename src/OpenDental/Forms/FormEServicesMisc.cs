using CodeBase;
using Microsoft.Win32;
using OpenDental.UI;
using OpenDentBusiness;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using System.Data;
using System.Linq;
using System.IO;
using OpenDentBusiness.WebServiceMainHQ;
using OpenDentBusiness.WebTypes.WebSched.TimeSlot;

namespace OpenDental {

	public partial class FormEServicesMisc:FormODBase {
		private string _formatShortDate="d";
		private string _formatLongDate="D";
		private string _formatMMMMdyyyy="MMMM d, yyyy";
		private string _formatM="m";
		private string _formatShortTime="t";
		private string _formatLongTime="T";
		private string _format24HourShort="HH:mm";
		private string _format24HourLong="HH:mm:ss";
		private WebServiceMainHQProxy.EServiceSetup.SignupOut _signupOut;
		///<summary>Keeps track if the user selected the Short Date or Long Date format.</summary>
		private bool _wasShortOrLongDateClicked=false;
		///<summary>Keeps track of if the user selected the Short Time or Long Time format.</summary>
		private bool _wasShortOrLongTimeClicked=false;
		
		public FormEServicesMisc(WebServiceMainHQProxy.EServiceSetup.SignupOut signupOut=null) {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
			_signupOut=signupOut;
		}

		private void FormEServicesMisc_Load(object sender,EventArgs e) {
			//Disable all controls if user does not have EServicesSetup permission.
			if(!Security.IsAuthorized(EnumPermType.EServicesSetup,suppressMessage:true)) {
				DisableAllExcept();
			}
			if(_signupOut==null){
				_signupOut=FormEServicesSetup.GetSignupOut();
			}
			//.NET has a bug in the DateTimePicker control where the text will not get updated and will instead default to showing DateTime.Now.
			//In order to get the control into a mode where it will display the correct value that we set, we need to set the property Checked to true.
			//Today's date will show even when the property is defaulted to true (via the designer), so we need to do it programmatically right here.
			//E.g. set your computer region to Assamese (India) and the DateTimePickers on the Automation Setting tab will both be set to todays date
			// if the tab is NOT set to be the first tab to display (don't ask me why it works then).
			//This is bad for our customers because setting both of the date pickers to the same date and time will cause automation to stop.
			dateRunStart.Checked=true;
			dateRunEnd.Checked=true;
			//Now that the DateTimePicker controls are ready to display the DateTime we set, go ahead and set them.
			//If loading the picker controls with the DateTime fields from the database failed, the date picker controls default to 7 AM and 10 PM.
			ODException.SwallowAnyException(() => {
				dateRunStart.Value=PrefC.GetDateT(PrefName.AutomaticCommunicationTimeStart);
				dateRunEnd.Value=PrefC.GetDateT(PrefName.AutomaticCommunicationTimeEnd);
			});
			labelDateCustom.Text="";
			radioDateShortDate.Text=DateTime.Today.ToString(_formatShortDate);//Formats as '3/15/2018'
			radioDateLongDate.Text=DateTime.Today.ToString(_formatLongDate);//Formats as 'Thursday, March 15, 2018'
			radioDateMMMMdyyyy.Text=DateTime.Today.ToString(_formatMMMMdyyyy);//Formats as 'March 15, 2018'
			radioDatem.Text=DateTime.Today.ToString(_formatM);//Formats as 'March 15'
			labelTimeCustom.Text="";
			radioTimeShortTime.Text=DateTime.Now.ToString(_formatShortTime);//Formats as '9:45 PM'
			radioTimeLongTime.Text=DateTime.Now.ToString(_formatLongTime);//Formats as '9:45:17 PM'
			radioTime24HourShort.Text=DateTime.Now.ToString(_format24HourShort);//Formats as '21:45'
			radioTime24HourLong.Text=DateTime.Now.ToString(_format24HourLong);//Formats as '21:45:17'
			string formatDate=PrefC.GetString(PrefName.PatientCommunicationDateFormat);
			if(formatDate==CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern) {
				formatDate=_formatShortDate;
			}
			if(formatDate==CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern) {
				formatDate=_formatLongDate;
			}
			if(formatDate==_formatShortDate) {
				radioDateShortDate.Checked=true;
			}
			else if(formatDate==_formatLongDate) {
				radioDateLongDate.Checked=true;
			}
			else if(formatDate==_formatMMMMdyyyy) {
				radioDateMMMMdyyyy.Checked=true;
			}
			else if(formatDate==_formatM) {
				radioDatem.Checked=true;
			}
			else {
				radioDateCustom.Checked=true;
				textDateCustom.Text=PrefC.GetString(PrefName.PatientCommunicationDateFormat);
			}
			string formatTime=PrefC.GetString(PrefName.PatientCommunicationTimeFormat);
			if(formatTime==CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern) {
				formatTime=_formatShortTime;
			}
			if(formatTime==CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern) {
				formatTime=_formatLongTime;
			}
			if(formatTime==_formatShortTime) {
				radioTimeShortTime.Checked=true;
			}
			else if(formatTime==_formatLongTime) {
				radioTimeLongTime.Checked=true;
			}
			else if(formatTime==_format24HourShort) {
				radioTime24HourShort.Checked=true;
			}
			else if(formatTime==_format24HourLong) {
				radioTime24HourLong.Checked=true;
			}
			else {
				radioTimeCustom.Checked=true;
				textTimeCustom.Text=PrefC.GetString(PrefName.PatientCommunicationTimeFormat);
			}
		}

		private void SaveTabMisc() {
			Prefs.UpdateDateT(PrefName.AutomaticCommunicationTimeStart,dateRunStart.Value);
			Prefs.UpdateDateT(PrefName.AutomaticCommunicationTimeEnd,dateRunEnd.Value);
			string formatDateOld=PrefC.GetString(PrefName.PatientCommunicationDateFormat);
			string formatDateNew;
			if(radioDateShortDate.Checked) {
				if(_wasShortOrLongDateClicked) {
					formatDateNew=CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
				}
				else {
					formatDateNew=formatDateOld;//If the user didn't actually select this, we'll keep the pattern what it was before.
				}
			}
			else if(radioDateLongDate.Checked) {
				if(_wasShortOrLongDateClicked) {
					formatDateNew=CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
				}
				else {
					formatDateNew=formatDateOld;//If the user didn't actually select this, we'll keep the pattern what it was before.
				}
			}
			else if(radioDateMMMMdyyyy.Checked) {
				formatDateNew=_formatMMMMdyyyy;
			}
			else if(radioDatem.Checked) {
				formatDateNew=_formatM;
			}
			else {
				formatDateNew=textDateCustom.Text;
			}
			Prefs.UpdateString(PrefName.PatientCommunicationDateFormat,formatDateNew);
			string formatTimeOld=PrefC.GetString(PrefName.PatientCommunicationTimeFormat);
			string formatTimeNew;
			if(radioTimeShortTime.Checked) {
				if(_wasShortOrLongTimeClicked) {
					formatTimeNew=CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
				}
				else {
					formatTimeNew=formatTimeOld;//If the user didn't actually select this, we'll keep the pattern what it was before.
				}
			}
			else if(radioTimeLongTime.Checked) {
				if(_wasShortOrLongTimeClicked) {
					formatTimeNew=CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
				}
				else {
					formatTimeNew=formatTimeOld;//If the user didn't actually select this, we'll keep the pattern what it was before.
				}
			}
			else if(radioTime24HourShort.Checked) {
				formatTimeNew=_format24HourShort;
			}
			else if(radioTime24HourLong.Checked) {
				formatTimeNew=_format24HourLong;
			}
			else {
				formatTimeNew=textTimeCustom.Text;
			}
			Prefs.UpdateString(PrefName.PatientCommunicationTimeFormat,formatTimeNew);
		}

		private void textDateCustom_TextChanged(object sender,EventArgs e) {
			if(textDateCustom.Text.Trim()=="") {
				labelDateCustom.Text="";
				return;
			}
			try {
				labelDateCustom.Text=DateTime.Now.ToString(textDateCustom.Text);
			}
			catch(Exception ex) {
				labelDateCustom.Text="";
			}
		}

		private void textTimeCustom_TextChanged(object sender,EventArgs e) {
			if(textTimeCustom.Text.Trim()=="") {
				labelTimeCustom.Text="";
				return;
			}
			try {
				labelTimeCustom.Text=DateTime.Now.ToString(textTimeCustom.Text);
			}
			catch(Exception ex) {
				labelTimeCustom.Text="";
			}
		}

		private void radioDateFormat_Click(object sender,EventArgs e) {
			_wasShortOrLongDateClicked=true;
		}

		private void radioTimeFormat_Click(object sender,EventArgs e) {
			_wasShortOrLongTimeClicked=true;
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(radioDateCustom.Checked) {
				bool isValidDateFormat=true;
				if(textDateCustom.Text.Trim()=="") {
					isValidDateFormat=false;
				}
				try {
					DateTime.Today.ToString(textDateCustom.Text);
				}
				catch(Exception ex) {
					isValidDateFormat=false;
				}
				if(!isValidDateFormat) {
					MsgBox.Show(this,"Please enter a valid format in the Custom date format text box.");
					return;
				}
			}
			if(radioTimeCustom.Checked) {
				bool isValidTimeFormat=true;
				if(textTimeCustom.Text.Trim()=="") {
					isValidTimeFormat=false;
				}
				try {
					DateTime.Today.ToString(textTimeCustom.Text);
				}
				catch(Exception ex) {
					isValidTimeFormat=false;
				}
				if(!isValidTimeFormat) {
					MsgBox.Show(this,"Please enter a valid format in the Custom time format text box.");
					return;
				}
			}
			SaveTabMisc();
			DialogResult=DialogResult.OK;	
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void textDateCustom_KeyPress(object sender,KeyPressEventArgs e) {
			Regex regex=new Regex(@"[0-9]");
			//Block numerical characters to prevent users from accidentally inputting dates rather than a date format.
			if(regex.IsMatch(e.KeyChar.ToString())) {
				e.Handled=true;
			}
		}

		private void textTimeCustom_KeyPress(object sender,KeyPressEventArgs e) {
			Regex regex=new Regex(@"[0-9]");
			//Block numerical characters to prevent users from accidentally inputting times rather than a time format.
			if(regex.IsMatch(e.KeyChar.ToString())) {
				e.Handled=true;
			}
		}
	}
}