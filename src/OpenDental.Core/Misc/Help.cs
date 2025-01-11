using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CodeBase;
using OpenDentBusiness;

namespace OpenDentBusiness {
	///<summary>Generally need to specify namespace when using this class to disambiguate from System.Windows.Forms.Help. This entirely replaces  the OpenDentalHelp.ODHelp class.</summary>
	public class Help {
		///<summary>Gets a url that looks like this: https://opendental.com/autoLogin.aspx?token=d83JWerd&redirect=help244/family.html, which redirects to the specified page. Or, if user is not on support, then they get https://www.opendental.com/site/helpfeature.html, which is unprotected.</summary>
		public static string GetManualPage(string formName,bool isKeyValid) {
			//Some formName examples: FormApptEdit, FormAutoCode, FormAutoCodeCanada
			//The mapping to help pages on our website used to be done with a server call so that we could "fix" the mapping at any time.
			//But now it's more important to be faster and to hit our servers less,
			//So we are moving mapping to the client.
			if(CanSaveFormName()) {
				ODClipboard.SetClipboard(formName);
			}
			if(!isKeyValid) {
				//This is the default page for when they're not allowed to see the other help pages.
				string helpFeatureUrl="https://www.opendental.com/site/helpfeature.html";
				return helpFeatureUrl; 
			}
			//For now, we will deserialize the entire list each time we need to launch help and find the matching help html page.
			//It should be blazing fast, and I think we only need it once per new window.
			string strXml=Properties.Resources.LinkWinForms;
			MemoryStream memoryStream=new MemoryStream();
			StreamWriter streamWriter=new StreamWriter(memoryStream);
			streamWriter.Write(strXml);
			streamWriter.Flush();
			memoryStream.Position=0;
			using StreamReader streamReader = new StreamReader(memoryStream,Encoding.UTF8,true);
			List<HelpLinkWin> listHelpLinkWins=new List<HelpLinkWin>();
			XmlSerializer xmlSerializer=new XmlSerializer(listHelpLinkWins.GetType());
			listHelpLinkWins=(List<HelpLinkWin>)xmlSerializer.Deserialize(streamReader);
			HelpLinkWin helpLinkWin=listHelpLinkWins.Find(x=>x.FormName==formName);
			if(helpLinkWin is null){
				string helpFeatureUrl="https://www.opendental.com/site/helpfeature.html";
				return helpFeatureUrl; 
			}
			string topicName=helpLinkWin.TopicName;
			List<string> listSplits=PrefC.GetString(PrefName.ProgramVersion).Split('.').ToList();
			string version3dig=listSplits[0]+listSplits[1];//example 243
			//helpPageURL=WebServiceMainHQProxy.GetWebServiceMainHQInstance().GetManualPage(formName,programVersion);
			if(ODBuild.IsDebug()){
				version3dig="244";
				//helpPageURL="https://www.opendental.com/help/test1.html";
			}
			//It's ok to show this token in the clear for now.
			//It's not used for security, but rather to hide the pages from google and other bots.
			string url="https://opendental.com/autoLogin.aspx?token=d83JWerd&redirect=help"+version3dig+"/"+topicName+".html";
			return url;
		}

		//public static string GetStableVersion() {
		//	string retVal="";
		//	try {
		//		retVal=WebServiceMainHQProxy.GetWebServiceMainHQInstance().GetStableManualVersion();
		//	}
		//	catch(Exception) {
		//		retVal="";
		//	}
		//	return retVal;
		//}

		///<summary>A helper method that determines if the requested form name should be copied to the user's clipboard. This 
		///will only happen on Hq's side and is used to help the documentation team identify manual pages for forms. See
		///PrefName.OpenDentalHelpCaptureFormName for more information.</summary>
		private static bool CanSaveFormName() {
			if(ODBuild.IsDebug()) {
				return true;
			}
			try {
				Pref pref=Prefs.GetOne(PrefName.OpenDentalHelpCaptureFormName);
				return true;
			}
			catch {//GetOne() will throw if the preference doesn't exist.
				return false;
			}
		}

		///<summary>This will hit our server once per week, whether user is on support or not.</summary>
		public static bool IsEncryptedKeyValid(){
			string keyPlainText=DecryptKey(PrefC.GetString(PrefName.HelpKey));
			try{
				return IsKeyValid(keyPlainText);
			}
			catch{ 
				return false;
			}
		}

		///<summary>Uses pref.HelpKey to validate office. This reduces the number of calls made to CustUpdates. helpKey format "DateTime,onSupport". This method can throw exceptions.</summary>
		public static bool IsKeyValid(string helpKeyDecrypted) {
			if(ODBuild.IsDebug() && !ODBuild.IsUnitTest) {
				return true;
			}
			if(helpKeyDecrypted=="") {//Will be empty for everyone on their first request
				helpKeyDecrypted=UpdateHelpKey();
			}
			string[] arrayHelpKeyValues=helpKeyDecrypted.Split(',');
			bool onSupport=PIn.Bool(arrayHelpKeyValues[1]);
			//These help keys are provided by HQ and are always in the en-US format and are created using DateTime.UtcNow.ToString()
			//If this starts to fail in the future then it is because our web service is handing out a different format or C# changed something with parsing.
			if(!DateTime.TryParse(arrayHelpKeyValues[0],new CultureInfo("en-US"),DateTimeStyles.None,out DateTime dateTimeKey)) {
				throw new ODException("Could not parse helpKey. Please try again or call support.");
			}
			if(onSupport){//key indicates they are on support
				if(DateTime.UtcNow < dateTimeKey.AddDays(7)) {//key is valid, less than 7 days old
					return true;
				}
			}
			string newHelpKey=UpdateHelpKey(); //If help key indicates they are not on support or help key is older than 7 days, update key with HQ and check if on support again. Per Jordan
			arrayHelpKeyValues=newHelpKey.Split(',');
			return PIn.Bool(arrayHelpKeyValues[1]);
		}

		/// <summary>Gets new encrypted helpKey from HQ.  Saves encrypted to pref.HelpKey.  Returns decrypted plain text helpKey.</summary>
		public static string UpdateHelpKey() {
			if(ODBuild.IsDebug()) {
				return DateTime.UtcNow.ToString()+",1";
			}
			string officeData=PayloadHelper.CreatePayload("",eServiceCode.ODHelp);
			string helpKeyEncrypted=WebServiceMainHQProxy.GetWebServiceMainHQInstance().CreateNewHelpKey(officeData);
			if(!Prefs.UpdateString(PrefName.HelpKey,helpKeyEncrypted)) {
				throw new ODException("Could not update HelpKey, try again. If the problem persists please call support.");
			}
			string helpKeyPlainText=DecryptKey(helpKeyEncrypted);
			return helpKeyPlainText;
		}

		/// <summary>Pass in encrypted PrefName.HelpKey.  Returns decrypted version.  Throws ApplicationException if decryption fails.</summary>
		public static string DecryptKey(string helpKey) {
			string failText="";
			string helpKeyPlainText;
			if(!ODCrypt.Encryption.DecryptString(helpKey,true,out helpKeyPlainText,ref failText)) {
				//Decryption failed
				throw new ApplicationException(failText);
			}
			return helpKeyPlainText;
		}
	}

	///<summary>Used by Help. List of these is serialized and stored as a resource in OD proper. The UI for that is over in Manual Publisher. Multiple FormNames can point to a single TopicName. These are extracted from ManualPage entries stored in the jordans_mp database. Empty entries over there do not get serialized into the resource.</summary>
	[Serializable]
	public class HelpLinkWin{
		///<summary>Example FormApptViewEdit. If it's a Frm, this will still be Form. No extension.</summary>
		public string FormName;
		///<summary>Example: appointmentvieweditwindow. No extension.</summary>
		public string TopicName;
	}
}
