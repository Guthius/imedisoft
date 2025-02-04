using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.WebTypes.Shared.XWeb;
using PayConnectService=OpenDentBusiness.PayConnectService;

namespace OpenDental;

public partial class FormXWebTransactions:FormODBase {
	///<summary>The XWeb and PayConnect transactions for the selected date range and clinics.</summary>
	private DataTable _tableTrans;
	///<summary>The list of clinics available to the current user.</summary>
	private List<ClinicDto> _listClinics;

	public FormXWebTransactions() {
		InitializeComponent();
	}

	private void FormXWebTransactions_Load(object sender,EventArgs e) {
		FillClinics();
		textDateFrom.Text=DateTime.Today.ToShortDateString();
		textDateTo.Text=DateTime.Today.ToShortDateString();
		FillGrid();
	}

	///<summary>Fills the clinics combo box with the clincs available to this user.</summary>
	private void FillClinics() {
		_listClinics=Clinics.GetForUserod(Security.CurUser);
		comboClinic.Items.Add(Lan.g(this,"All"));
		comboClinic.SelectedIndex=0;
		var offset=1;
		if(!Security.CurUser.ClinicIsRestricted) {
			comboClinic.Items.Add(Lan.g(this,"Unassigned"));
			offset++;
		}
		for(var i=0;i<_listClinics.Count;i++) {
			comboClinic.Items.Add(_listClinics[i].Abbr);
		}
		comboClinic.SelectedIndex=_listClinics.FindIndex(x => x.Id==Clinics.ClinicNum)+offset;
		if(comboClinic.SelectedIndex-offset<0) {
			comboClinic.SelectedIndex=0;
		}
	}

	private void FillGrid() {
		var listClinicNums=new List<long>();
		if(true && comboClinic.SelectedIndex!=0) {//Not 'All' selected
			if(Security.CurUser.ClinicIsRestricted) {
				listClinicNums.Add(_listClinics[comboClinic.SelectedIndex-1].Id);//Minus 1 for 'All'
			}
			else {
				if(comboClinic.SelectedIndex==1) {//'Unassigned' selected
					listClinicNums.Add(0);
				}
				else if(comboClinic.SelectedIndex>1) {
					listClinicNums.Add(_listClinics[comboClinic.SelectedIndex-2].Id);//Minus 2 for 'All' and 'Unassigned'
				}
			}
		}
		else {
			//Send an empty list of clinics to get all transactions
		}
		var dateFrom=SIn.Date(textDateFrom.Text);
		var dateTo=SIn.Date(textDateTo.Text);
		_tableTrans=XWebResponses.GetApprovedTransactions(listClinicNums,dateFrom,dateTo);
		gridMain.BeginUpdate();
		gridMain.Columns.Clear();
		GridColumn col;
		col=new GridColumn(Lan.g(this,"Patient"),120);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Amount"),60,HorizontalAlignment.Right);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Date"),80);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Tran Type"),80);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Card Number"),140);
		gridMain.Columns.Add(col);
		col=new GridColumn(Lan.g(this,"Expiration"),70);
		gridMain.Columns.Add(col);
		if(true) {
			col=new GridColumn(Lan.g(this,"Clinic"),100);
			gridMain.Columns.Add(col);
		}
		col=new GridColumn(Lan.g(this,"Transaction ID"),110);
		gridMain.Columns.Add(col);
		gridMain.ListGridRows.Clear();
		GridRow row;
		for(var i=0;i<_tableTrans.Rows.Count;i++) {
			var isXWeb=IsXWebTransaction(i); //Only other option at the moment is PayConnect. This will need to be refactored if we add more payment options
			row=new GridRow();
			row.Cells.Add(_tableTrans.Rows[i]["Patient"].ToString());
			row.Cells.Add(SIn.Double(_tableTrans.Rows[i]["Amount"].ToString()).ToString("f"));
			row.Cells.Add(SIn.Date(_tableTrans.Rows[i]["DateTUpdate"].ToString()).ToShortDateString());
			if(isXWeb) {
				var tranStatus=(XWebTransactionStatus)SIn.Int(_tableTrans.Rows[i]["TransactionStatus"].ToString());
				row.Cells.Add(GetXWebTranTypeByStatus(tranStatus));
			}
			else {
				//This is actually the PayConnectResponseWeb.TransType
				row.Cells.Add(_tableTrans.Rows[i]["TransactionStatus"].ToString());
			}
			row.Cells.Add(_tableTrans.Rows[i]["MaskedAcctNum"].ToString());
			row.Cells.Add(_tableTrans.Rows[i]["ExpDate"].ToString());
			if(true) {
				row.Cells.Add(_tableTrans.Rows[i]["Clinic"].ToString());
			}
			row.Cells.Add(_tableTrans.Rows[i]["TransactionID"].ToString());
			gridMain.ListGridRows.Add(row);
		}
		gridMain.EndUpdate();
	}

	private string GetXWebTranTypeByStatus(XWebTransactionStatus status) {
		string strTranStatus;
		switch(status) {
			case XWebTransactionStatus.DtgPaymentApproved:
			case XWebTransactionStatus.HpfCompletePaymentApproved:
			case XWebTransactionStatus.HpfCompletePaymentApprovedPartial:
			case XWebTransactionStatus.EdgeExpressCompletePaymentApproved:
			case XWebTransactionStatus.EdgeExpressCompletePaymentApprovedPartial:
				strTranStatus="Sale";
				break;
			case XWebTransactionStatus.DtgPaymentReturned:
				strTranStatus="Return";
				break;
			case XWebTransactionStatus.DtgPaymentVoided:
				strTranStatus="Void";
				break;
			default://These other values should not be returned from the query.
				strTranStatus=status.ToString();
				break;
		}
		return strTranStatus;
	}

	private bool IsXWebTransaction(int selectedIndex) {
		return SIn.Int(_tableTrans.Rows[selectedIndex]["isXWeb"].ToString())==1;
	}

	private void butRefresh_Click(object sender,EventArgs e) {
		if(textDateFrom.Text==""
		   || textDateTo.Text==""
		   || !textDateFrom.IsValid()
		   || !textDateTo.IsValid())
		{
			MsgBox.Show(this,"Please fix data entry errors first.");
			return;
		}
		FillGrid();
	}

	private void gridMain_MouseDown(object sender,MouseEventArgs e) {
		if(e.Button==MouseButtons.Right) {
			gridMain.SetAll(false);
		}
	}

	private void contextMenu_Opening(object sender,System.ComponentModel.CancelEventArgs e) {
		if(gridMain.SelectedIndices.Length!=1) {
			e.Cancel=true;
			return;
		}
		try {
			SetContextMenuItemVisibility();
		}
		catch(Exception ex) {
			MsgBox.Show("An error occurred: "+ex.Message);
			e.Cancel=true;
		}
	}

	private void SetContextMenuItemVisibility() {
		var idxSelected=gridMain.GetSelectedIndex();
		if(idxSelected<0) {
			return;
		}
		openPaymentToolStripMenuItem.Visible=SIn.Bool(_tableTrans.Rows[idxSelected]["doesPaymentExist"].ToString());
		voidPaymentToolStripMenuItem.Visible=false;
		processReturnToolStripMenuItem.Visible=false;
		if(IsXWebTransaction(idxSelected)) {
			switch((XWebTransactionStatus)SIn.Int(_tableTrans.Rows[idxSelected]["TransactionStatus"].ToString())) {
				case XWebTransactionStatus.DtgPaymentApproved:
				case XWebTransactionStatus.HpfCompletePaymentApproved:
				case XWebTransactionStatus.HpfCompletePaymentApprovedPartial:
				case XWebTransactionStatus.DtgPaymentReturned:
				case XWebTransactionStatus.EdgeExpressCompletePaymentApproved:
				case XWebTransactionStatus.EdgeExpressCompletePaymentApprovedPartial:
					voidPaymentToolStripMenuItem.Visible=true;
					processReturnToolStripMenuItem.Visible=true;
					break;
			}
			return;
		}
		switch(SIn.String(_tableTrans.Rows[idxSelected]["TransactionStatus"].ToString())) {
			case "SALE":
				voidPaymentToolStripMenuItem.Visible=true;
				processReturnToolStripMenuItem.Visible=true;
				break;
		}
	}

	private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
		if(e.Row<0 || !Security.IsAuthorized(EnumPermType.AccountModule)) {
			return;
		}
		var patNum=SIn.Long(_tableTrans.Rows[e.Row]["PatNum"].ToString());
		GlobalFormOpenDental.GoToModule(EnumModuleType.Account,patNum:patNum);
	}

	private void menuItemGoTo_Click(object sender,EventArgs e) {
		if(gridMain.SelectedIndices.Length<1 || !Security.IsAuthorized(EnumPermType.AccountModule)) {
			return;
		}
		var patNum=SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["PatNum"].ToString());
		GlobalFormOpenDental.GoToModule(EnumModuleType.Account,patNum:patNum);
	}

	private void openPaymentToolStripMenuItem_Click(object sender,EventArgs e) {
		if(gridMain.SelectedIndices.Length<1) {
			return;
		}
		var payment=Payments.GetPayment(SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["PaymentNum"].ToString()));
		if(payment==null) {//The payment has been deleted
			MsgBox.Show(this,"This payment no longer exists.");
			return;
		}
		var patient=Patients.GetPat(payment.PatNum);
		var family=Patients.GetFamily(patient.PatNum);
		using var formPayment=new FormPayment(patient,family,payment,false);
		formPayment.ShowDialog();
		FillGrid();
	}

	private void voidPaymentToolStripMenuItem_Click(object sender,EventArgs e) {
		//A new payment is being created upon clicking this menu item. The payment date is set to "DateTime.Now" in 
		//PayConnectL.VoidOrRefundPayConnectPortalTransaction(...) and in XWebs.VoidPayment paynote
		if(!Security.IsAuthorized(EnumPermType.PaymentCreate,DateTime.Today)) {
			return;
		}
		if(gridMain.SelectedIndices.Length<1
		   || !MsgBox.Show(this,MsgBoxButtons.YesNo,"Void this payment?"))
		{
			return;
		}
		Cursor=Cursors.WaitCursor;
		if(IsXWebTransaction(gridMain.SelectedIndices[0])) {
			var patNum=SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["PatNum"].ToString());
			var responseNum=SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["ResponseNum"].ToString());
			var payNote=Lan.g(this,"Void XWeb payment made from within Open Dental")+"\r\n"
			                                                                        +Lan.g(this,"Amount:")+" "+SIn.Double(_tableTrans.Rows[gridMain.SelectedIndices[0]]["Amount"].ToString()).ToString("f")+"\r\n"
			                                                                        +Lan.g(this,"Transaction ID:")+" "+_tableTrans.Rows[gridMain.SelectedIndices[0]]["TransactionID"]+"\r\n"
			                                                                        +Lan.g(this,"Card Number:")+" "+_tableTrans.Rows[gridMain.SelectedIndices[0]]["MaskedAcctNum"]+"\r\n"
			                                                                        +Lan.g(this,"Processed:")+" "+DateTime.Now.ToShortDateString()+" "+DateTime.Now.ToShortTimeString();
			try {
				XWebs.VoidPayment(patNum,payNote,responseNum); 
			}
			catch(ODException ex) {
				Cursor=Cursors.Default;
				ODMessageBox.Show(ex.Message);
				return;
			}
		}
		else {
			var payment=Payments.GetPayment(SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["PaymentNum"].ToString()));
			var payConnectResponseWeb=PayConnectResponseWebs.GetOne(SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["ResponseNum"].ToString()));
			var amt=SIn.Decimal(_tableTrans.Rows[gridMain.SelectedIndices[0]]["Amount"].ToString());
			var refNum=_tableTrans.Rows[gridMain.SelectedIndices[0]]["TransactionID"].ToString(); //This is actually PayConnectResponseWeb.RefNumber, it's just stored in the TransactionID column
			if(!PayConnectL.VoidOrRefundPayConnectPortalTransaction(payConnectResponseWeb,payment,PayConnectService.transType.VOID,refNum,amt)) {
				Cursor=Cursors.Default;
				return;
			}
		}
		Cursor=Cursors.Default;
		MsgBox.Show(this,"Void successful");
		FillGrid();
	}

	private void processReturnToolStripMenuItem_Click(object sender,EventArgs e) {
		//using DateTime.Today because this process will create a new payment (refund)
		if(!Security.IsAuthorized(EnumPermType.PaymentCreate,DateTime.Today)) {
			return;
		}
		if(gridMain.SelectedIndices.Length<1) {
			return;
		}
		var payment=Payments.GetPayment(SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["PaymentNum"].ToString()));
		if(IsXWebTransaction(gridMain.SelectedIndices[0])) {
			var patNum=SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["PatNum"].ToString());
			var alias=_tableTrans.Rows[gridMain.SelectedIndices[0]]["Alias"].ToString();
			var listCreditCards=CreditCards.GetCardsByToken(alias,
				[CreditCardSource.XWeb, CreditCardSource.XWebPortalLogin, CreditCardSource.XWebPaymentPortal, CreditCardSource.XWebPaymentPortalGuest]);
			if(listCreditCards.Count==0) {
				MsgBox.Show(this,"This credit card is no longer stored in the database. Return cannot be processed.");
				return;
			}
			if(listCreditCards.Count>1) {
				MsgBox.Show(this,"There is more than one card in the database with this token. Return cannot be processed due to the risk of charging the "+
				                 "incorrect card.");
				return;
			}
			var amt=SIn.Double(_tableTrans.Rows[gridMain.SelectedIndices[0]]["Amount"].ToString());
			using var formXWeb=new FormXWeb(listCreditCards.FirstOrDefault(),XWebTransactionType.CreditReturnTransaction,createPayment:false,amt);
			formXWeb.LockCardInfo=true;
			if(formXWeb.ShowDialog()==DialogResult.OK) {
				var paymentReturn=Payments.InsertReturnXWebPayment(payment,formXWeb.XWebResponse.GetFormattedNote(false),(-formXWeb.XWebResponse.Amount));
				formXWeb.XWebResponse.PaymentNum=paymentReturn.PayNum;
				XWebResponses.Update(formXWeb.XWebResponse);
				SecurityLogs.MakeLogEntry(EnumPermType.PaymentCreate,paymentReturn.PatNum,
					Patients.GetLim(paymentReturn.PatNum).GetNameLF() + ", " + paymentReturn.PayAmt.ToString("c"));
				FillGrid();
			}
			return;
		}
		var payConnectResponseWeb=PayConnectResponseWebs.GetOne(SIn.Long(_tableTrans.Rows[gridMain.SelectedIndices[0]]["ResponseNum"].ToString()));
		var amount=SIn.Decimal(_tableTrans.Rows[gridMain.SelectedIndices[0]]["Amount"].ToString());
		var refNum=_tableTrans.Rows[gridMain.SelectedIndices[0]]["TransactionID"].ToString(); //This is actually PayConnectResponseWeb.RefNumber, it's just stored in the TransactionID column
		if(!PayConnectL.VoidOrRefundPayConnectPortalTransaction(payConnectResponseWeb,payment,PayConnectService.transType.RETURN,refNum,amount)) {
			return;
		}
		MsgBox.Show("Return successful.");
		FillGrid();
	}

}