using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormSupportStatus:FormODBase {
	private string _regKey;

	public FormSupportStatus() {
		InitializeComponent();
	}

	private void FormSupportStatus_Load(object sender,EventArgs e) {
		Cursor=Cursors.WaitCursor;
		_regKey=PrefC.GetString(PrefName.RegistrationKey);
		textRegKey.Text=_regKey;
		var xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent=true;
		xmlWriterSettings.IndentChars=("    ");
		var stringBuilder=new StringBuilder();
		using var xmlWriter = XmlWriter.Create(stringBuilder,xmlWriterSettings);
		xmlWriter.WriteStartElement("RegistrationKey");
		xmlWriter.WriteString(_regKey);
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
		var updateService=CustomerUpdatesProxy.GetWebServiceInstance();
		var result="";
		try
		{
			result=updateService.RequestRegKeyStatus(stringBuilder.ToString());
		}
		catch(Exception ex) {
			Cursor=Cursors.Default;
			ODMessageBox.Show("Error: "+ex.Message);
			this.Close();
			return;
		}
		try{
			var helpKeyDecrypted=OpenDentBusiness.Help.UpdateHelpKey();
			var arrayHelpKeyValues=helpKeyDecrypted.Split(',');
			var onSupport=SIn.Bool(arrayHelpKeyValues[1]);
			if(onSupport){
				labelHelpKey.Text="Yes";
			}
			else{
				labelHelpKey.Text="No";
			}
		}
		catch(Exception ex){
			labelHelpKey.Text="error:"+ex;
		}
		Cursor=Cursors.Default;
		var xmlDocument=new XmlDocument();
		xmlDocument.LoadXml(result);
		var xmlNode=xmlDocument.SelectSingleNode("//Error");
		if(xmlNode!=null) {
			ODMessageBox.Show(xmlNode.InnerText,"Error");
			return;
		}
		xmlNode=xmlDocument.SelectSingleNode("//KeyDisabled");
		if(xmlNode!=null) {
			if(Prefs.UpdateBool(PrefName.RegistrationKeyIsDisabled,true)) {
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			labelStatusValue.Text="DISABLED "+xmlNode.InnerText;
			labelStatusValue.ForeColor=Color.Red;
		}
		//Checking all three statuses in case RequestRegKeyStatus changes in the future
		xmlNode=xmlDocument.SelectSingleNode("//KeyEnabled");
		if(xmlNode!=null) {
			if(Prefs.UpdateBool(PrefName.RegistrationKeyIsDisabled,false)) {
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			labelStatusValue.Text="ENABLED";
			labelStatusValue.ForeColor=Color.Green;
		}
		xmlNode=xmlDocument.SelectSingleNode("//KeyEnded");
		if(xmlNode!=null) {
			labelStatusValue.Text="EXPIRED "+xmlNode.InnerText;
			labelStatusValue.ForeColor=Color.Red;
		}
	}

}