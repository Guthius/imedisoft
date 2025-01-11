using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Security.Credentials;

namespace PasswordVaultWrapper {
	///This used to reside in CodeBase, but because this file uses Windows.Security.Credentials (a type from the Windows.winmd reference), ASP Web API projects that reference CodeBase couldn't run. Moving this into OpenDentBusiness didn't work either because building the project while generating the serialization assembly consistently produced an SGEN error (even after verifying that the SGEN configs were set correctly). The only way OpenDentBusiness would successfully build was to remove all of the Web References. It didn't appear that a single Web Reference was the cause of this issue and testing the various sets of them was very time consuming. Moving this file and the Windows.winmd reference into its own project for the time being. This will give us time to understand what the root of the issue is without blocking other jobs.

	///<summary>This wrapper class protects Windows 7 users from a runtime error that is caused by the Windows.wnd reference. This reference only works on windows 8 and up. Any calling of the member's methods MUST be try-caught to avoid a runtime error.</summary>
	public class WindowsPasswordVaultWrapper {
		private const string _strMTResourcePrefix="OpenDental Middle Tier:";

		///<summary>Retrieves the first "OpenDental Middle Tier" username listed in the Password Vault for the URI passed in.</summary>
		public static bool TryRetrieveUserName(string uri,out string username) {
			username = string.Empty;
			IReadOnlyList<PasswordCredential> listCreds;
			try {
				listCreds = new PasswordVault().FindAllByResource(_strMTResourcePrefix + uri);
			}
			catch(Exception ex) {
				return false;
			}
			if(listCreds.Count > 0) {
				username = listCreds.FirstOrDefault().UserName;
				return true;//Found at least one PasswordCredential.  Use the first.
			}
			return false;//Couldn't find any PasswordCredentials.
		}

		///<summary>An exception will be thrown if the password cannot be found. Callers of this method should consider this scenario.</summary>
		public static string RetrievePassword(string uri,string username) {
			//This will only return the password if it has been saved under the current Windows user.
			PasswordCredential cred=new PasswordVault().Retrieve(_strMTResourcePrefix+uri,username);
			cred.RetrievePassword();
			return cred.Password;
		}
	}
}
