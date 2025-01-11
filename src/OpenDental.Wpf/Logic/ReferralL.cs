using System;
using OpenDentBusiness;
using OpenDental;

namespace WpfControls {
	public class ReferralL {

		///<summary>Attemmpts to get a referral.
		///Returns null and shows a MsgBox if there was an error.</summary>
		public static Referral GetReferral(long referralNum,bool isMsgShown=true) {
			Referral referral=null;
			try{
				referral=Referrals.GetReferral(referralNum);
			}
			catch(ApplicationException appEx){
				if(isMsgShown){
					MsgBox.Show("Referrals","Could not retrieve referral. Please run Database Maintenance or call support.");
				}
			}
			return referral;
		}
	}
}
