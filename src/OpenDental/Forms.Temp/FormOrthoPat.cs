using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using OpenDental.UI;

namespace OpenDental;

public partial class FormOrthoPat:FormODBase {
	private PatPlan _patPlan;
	private InsPlan _insPlan;

	///<summary>This form does NOT take care of updating the patplan table for you. 
	///It passes around the patPlan you passed in by reference, so you can call update on the same patplan you passed in to update.</summary>
	public FormOrthoPat(PatPlan patPlan,InsPlan insPlan, string carrierName, string subID, double defaultFee) {
		InitializeComponent();

		_patPlan=patPlan;
		_insPlan=insPlan;
		var patient=Patients.GetLim(patPlan.PatNum);
		textPatient.Text=patient.GetNameLF();
		textCarrier.Text = carrierName;
		textSubID.Text = subID;
		if(patPlan.OrthoAutoFeeBilledOverride==-1) {
			checkUseDefaultFee.Checked=true;
			textFee.ReadOnly=true;
			textFee.Text=defaultFee.ToString();
		}
		else {
			checkUseDefaultFee.Checked=false;
			textFee.ReadOnly=false;
			textFee.Text=patPlan.OrthoAutoFeeBilledOverride.ToString();
		}
		if(patPlan.OrthoAutoNextClaimDate.Date != DateTime.MinValue.Date) {
			textDateNextClaim.Text=patPlan.OrthoAutoNextClaimDate.ToShortDateString();
		}
		else { //there is no initial procedure or claim, so showing when the "next" claim will be would be misleading.
			textDateNextClaim.Text="";
		}
	}

	private void checkUseDefaultFee_CheckedChanged(object sender,EventArgs e) {
		textFee.ReadOnly=checkUseDefaultFee.Checked;
		if(checkUseDefaultFee.Checked) {
			textFee.Text=_insPlan.OrthoAutoFeeBilled.ToString();
		}
	}

	private void textDateNextClaim_Validated(object sender,EventArgs e) {
		DateTime dateTimeEntered;
		try { 
			dateTimeEntered=SIn.Date(textDateNextClaim.Text);
		}
		catch {
			return;//do nothing
		}
		if(dateTimeEntered.Day == 1) {
			return;
		}
		var firstOfMonth=new DateTime(dateTimeEntered.Year, dateTimeEntered.Month,1);
		textDateNextClaim.Text = firstOfMonth.ToShortDateString();
	}

	private void butSave_Click(object sender,EventArgs e) {
		if(!textFee.IsValid()) {
			MsgBox.Show(this,"Please enter a valid fee.");
			return;
		}
		if(!textDateNextClaim.IsValid()) {
			MsgBox.Show(this,"Please enter a valid date.");
			return;
		}
		if(textDateNextClaim.Text!="") {
			var listOrthoCodeNums=ProcedureCodes.GetOrthoBandingCodeNums();
			var listOrthoProcedures=Procedures.GetProcsByStatusForPat(_patPlan.PatNum,ProcStat.C).FindAll(x => listOrthoCodeNums.Contains(x.CodeNum));
			if(listOrthoProcedures.Count==0) {
				MsgBox.Show(this,"Cannot enter Next Claim Date until at least one Ortho Proc in Ortho Placement Procedures is complete. See Ortho Setup.");
				return;
			}
		}
		if(checkUseDefaultFee.Checked) {
			_patPlan.OrthoAutoFeeBilledOverride = -1;
		}
		else {
			_patPlan.OrthoAutoFeeBilledOverride = SIn.Double(textFee.Text);
		}
		if(textDateNextClaim.Visible) {
			_patPlan.OrthoAutoNextClaimDate = SIn.Date(textDateNextClaim.Text); //MinValue if blank.
		}
		DialogResult=DialogResult.OK;
	}

}