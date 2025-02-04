using System;
using System.Windows.Forms;

namespace OpenDental;

///<summary>An empty window that is entirely filled by a copy of the old Images module.  Used from FormClaimEdit to view EOB.  Also, from FormClaimPayBatch to view EOB.  Finally, from FormEhrAmendmentEdit to scan.</summary>
public partial class FormImages:FormODBase {
	///<summary>Right now, this form only supports claimpayment and amendment mode.</summary>
	public long ClaimPaymentNum;

	public FormImages() {
		InitializeComponent();
	}

	private void FormImages_Shown(object sender,EventArgs e) {
		if(ClaimPaymentNum!=0) {
			//this line has to be in Shown instead of Load for the cloudiFrame used by ODCloud
			contrImagesMain.ModuleSelectedClaimPayment(ClaimPaymentNum);
		}
		contrImagesMain.CloseClick+=ContrImagesMain_CloseClick;
	}

	private void ContrImagesMain_CloseClick(object sender,EventArgs e) {
		DialogResult=DialogResult.OK;
	}
}