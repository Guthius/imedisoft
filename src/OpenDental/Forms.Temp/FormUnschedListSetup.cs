using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormUnschedListSetup:FormODBase {

	public FormUnschedListSetup() {
		InitializeComponent();
	}
		
	private void FormUnschedListSetup_Load(object sender,EventArgs e) {
		var daysPast=PrefC.GetInt(PrefName.UnschedDaysPast);
		if(daysPast!=-1) {
			textDaysPast.Text=daysPast.ToString();
		}
		var daysFuture=PrefC.GetInt(PrefName.UnschedDaysFuture);
		if(daysFuture!=-1) {
			textDaysFuture.Text=daysFuture.ToString();
		}
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(!textDaysPast.IsValid() || !textDaysFuture.IsValid()) {
			MsgBox.Show(this,"Please fix data entry errors first.");
			return;
		}
		var isPrefsInvalid=false;
		var unschedDaysPastValue=-1;
		var unschedDaysFutureValue=-1;
		if(!string.IsNullOrWhiteSpace(textDaysPast.Text)) {
			unschedDaysPastValue=SIn.Int(textDaysPast.Text,false);
		}
		if(!string.IsNullOrWhiteSpace(textDaysFuture.Text)) {
			unschedDaysFutureValue=SIn.Int(textDaysFuture.Text,false);
		}
		isPrefsInvalid=Prefs.UpdateInt(PrefName.UnschedDaysPast,unschedDaysPastValue) 
		               | Prefs.UpdateInt(PrefName.UnschedDaysFuture,unschedDaysFutureValue);
		if(isPrefsInvalid) {
			DataValid.SetInvalid(InvalidType.Prefs);
		}
		DialogResult=DialogResult.OK;
	}

}