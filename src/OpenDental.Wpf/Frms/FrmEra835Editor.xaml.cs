using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;
using Microsoft.Win32;
using CodeBase;

namespace OpenDental {
	///<summary>HQ Only form for generating an ERA from the database or from an existing ERA.</summary>
	public partial class FrmEra835Editor:FrmODBase {

		private X835 _x835;

		
		public FrmEra835Editor() {
			InitializeComponent();
			Load+=Frm_Load;
			gridClaims.CellDoubleClick+=gridClaims_CellDoubleClick;
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			FillGridClaims();
		}

		private void FillGridClaims() {
			gridClaims.BeginUpdate();
			gridClaims.Columns.Clear();
			//Most column names and widths below mimic those in FormEtrans835Edit.
			GridColumn gridColumn=new GridColumn(Lang.g(this,"Patient"),70);
			gridColumn.IsWidthDynamic=true;
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Subscriber"),70);
			gridColumn.IsWidthDynamic=true;
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"DateService"),80,HorizontalAlignment.Center);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"ClaimId"),50);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"PayerCtrl#"),56,HorizontalAlignment.Center);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Status"),70);
			gridColumn.IsWidthDynamic=true;
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"IsSplit"),50,HorizontalAlignment.Center);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"ClaimFee"),70,HorizontalAlignment.Right);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"InsPaid"),70,HorizontalAlignment.Right);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"PatPort"),70,HorizontalAlignment.Right);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Deduct"),70,HorizontalAlignment.Right);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Writeoff"),70,HorizontalAlignment.Right);
			gridClaims.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Remarks"),0,HorizontalAlignment.Left);
			gridColumn.IsWidthDynamic=true;
			gridClaims.Columns.Add(gridColumn);
			gridClaims.ListGridRows.Clear();
			if(_x835==null) {
				gridClaims.EndUpdate();
				return;
			}
			for(int i=0;i<_x835.ListClaimsPaid.Count;i++){
				GridRow gridRow=new GridRow();
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].PatientName.ToString());
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].SubscriberName.ToString());
				DateTime dateService=_x835.ListClaimsPaid[i].DateServiceStart;
				if(dateService==DateTime.MinValue) {
					dateService=_x835.ListClaimsPaid[i].DateServiceEnd;
				}
				gridRow.Cells.Add(dateService.ToShortDateString());
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].ClaimTrackingNumber);
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].PayerControlNumber);
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].StatusCodeDescript);
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].IsSplitClaim?"X":"");
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].ClaimFee.ToString("f2"));
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].InsPaid.ToString("f2"));
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].PatientPortionAmt.ToString("f2"));
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].PatientDeductAmt.ToString("f2"));
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].WriteoffAmt.ToString("f2"));
				gridRow.Cells.Add(_x835.ListClaimsPaid[i].GetRemarks());
				gridRow.Tag=_x835.ListClaimsPaid[i];
				gridClaims.ListGridRows.Add(gridRow);
			}
			gridClaims.EndUpdate();
		}

		private void gridClaims_CellDoubleClick(object sender,GridClickEventArgs e) {
			FrmEra835ClaimEdit frmEra835ClaimEdit=new FrmEra835ClaimEdit();
			frmEra835ClaimEdit.Hx835Claim=(Hx835_Claim)gridClaims.ListGridRows[e.Row].Tag;
			frmEra835ClaimEdit.ShowDialog();//Edits in this window change the Hx835_Claim direclty. Do not need to check IsDialogOK.
			int selectedIndex=gridClaims.SelectedIndices[0];
			FillGridClaims();
			gridClaims.SetSelected(selectedIndex);
		}

		private void butImportEra_Click(object sender,EventArgs e) {
			OpenFileDialog openFileDialog=new OpenFileDialog();
			openFileDialog.ShowDialog();
			string messageText=File.ReadAllText(openFileDialog.FileName);
			X12object x12object=X12object.ToX12object(messageText);
			List<string> listTranSetIds=x12object.GetTranSetIds();
			if(listTranSetIds.Count > 1) {
				MsgBox.Show(this,MsgBoxButtons.OKCancel,"This 835 contains more than 1 transaction/EOB. Only the first EOB will be loaded. In the future, we will let you pick one.");
			}
			Cursor=Cursors.Wait;
			EtransMessageText etransMessageText=new EtransMessageText();
			etransMessageText.MessageText=messageText;
			EtransMessageTexts.Insert(etransMessageText);
			Etrans etrans=new Etrans();
			etrans.DateTimeTrans=DateTime.Now;
			etrans.Etype=EtransType.ERA_835;
			etrans.EtransMessageTextNum=etransMessageText.EtransMessageTextNum;
			Etranss.Insert(etrans);
			_x835=new X835(etrans,messageText,"");//Blank means to load first transaction.
			FillGridClaims();
			Cursor=Cursors.Arrow;
		}

		private void butBuildEra_Click(object sender,EventArgs e) {
			Cursor=Cursors.Wait;
			_x835=new X835();
			_x835.ListClaimsPaid=new List<Hx835_Claim>();
			List<ClaimPaySplit> listClaimPaySplits=Claims.GetOutstandingClaims("",DateTime.MinValue);
			List<long> listClaimNums=listClaimPaySplits.Select(x => x.ClaimNum).ToList();
			List<Claim> listClaims=Claims.GetClaimsFromClaimNums(listClaimNums);
			List<ClaimProc> listClaimProcs=ClaimProcs.RefreshForClaims(listClaimNums);
			for(int i=0;i<listClaims.Count;i++) {
				Hx835_Claim hx835_claim=new Hx835_Claim();
				hx835_claim.PatientName=new Hx835_Name();
				Patient patient=Patients.GetPat(listClaims[i].PatNum);
				hx835_claim.PatientName.Fname=patient.FName;
				hx835_claim.PatientName.Mname=patient.MiddleI;
				hx835_claim.PatientName.Lname=patient.LName;
				hx835_claim.SubscriberName=new Hx835_Name();
				InsSub insSub=InsSubs.GetOne(listClaims[i].InsSubNum);
				Patient subscriber=Patients.GetPat(insSub.Subscriber);
				hx835_claim.SubscriberName.Fname=subscriber.FName;
				hx835_claim.SubscriberName.Mname=subscriber.MiddleI;
				hx835_claim.SubscriberName.Lname=subscriber.LName;
				hx835_claim.DateServiceStart=listClaims[i].DateService;
				hx835_claim.ClaimTrackingNumber=listClaims[i].ClaimIdentifier;
				hx835_claim.PayerControlNumber=ODRandom.Next(1000,1000000).ToString().PadLeft(7,'0');
				//StatusCodeDescript
				//IsSplitClaim
				List<ClaimProc> listClaimProcsForClaim=ClaimProcs.GetForSendClaim(listClaimProcs,listClaims[i].ClaimNum);
				hx835_claim.ListProcs=new List<Hx835_Proc>();
				hx835_claim.ClaimFee=(decimal)listClaimProcsForClaim.Sum(x => x.FeeBilled);
				for(int j=0;j<listClaimProcsForClaim.Count;j++) {
					Hx835_Proc hx835_proc=new Hx835_Proc();
					hx835_proc.ProcCodeBilled=listClaimProcsForClaim[j].CodeSent;
					hx835_proc.ProcCodeAdjudicated=listClaimProcsForClaim[j].CodeSent;
					hx835_proc.ProcFee=(decimal)listClaimProcsForClaim[j].FeeBilled;
					hx835_proc.InsPaid=(decimal)listClaimProcsForClaim[j].InsPayEst;
					if(listClaims[i].ClaimType=="PreAuth") {
						hx835_proc.InsPaid=(decimal)listClaimProcsForClaim[j].InsPayAmt;
					}
					hx835_proc.DeductibleAmt=(decimal)listClaimProcsForClaim[j].DedApplied;
					if(listClaims[i].ClaimType=="PreAuth") {
						hx835_proc.DeductibleAmt=(decimal)listClaimProcsForClaim[j].DedEst;
						if(listClaimProcsForClaim[j].DedEstOverride!=-1) {
							hx835_proc.DeductibleAmt=(decimal)listClaimProcsForClaim[j].DedEstOverride;
						}
					}
					hx835_proc.WriteoffAmt=(decimal)listClaimProcs[j].WriteOff;
					if(listClaims[i].ClaimType=="PreAuth") {
						hx835_proc.WriteoffAmt=(decimal)listClaimProcsForClaim[j].WriteOffEst;
						if(listClaimProcsForClaim[j].WriteOffEstOverride!=-1) {
							hx835_proc.WriteoffAmt=(decimal)listClaimProcsForClaim[j].WriteOffEstOverride;
						}
					}
					hx835_proc.DateServiceStart=listClaimProcsForClaim[j].ProcDate;
					hx835_claim.ListProcs.Add(hx835_proc);
				}
				hx835_claim.ClaimFee=hx835_claim.ListProcs.Sum(x => x.ProcFee);
				hx835_claim.InsPaid=hx835_claim.ListProcs.Sum(x => x.InsPaid);
				hx835_claim.PatientDeductAmt=hx835_claim.ListProcs.Sum(x => x.DeductibleAmt);
				hx835_claim.WriteoffAmt=hx835_claim.ListProcs.Sum(x => x.WriteoffAmt);
				hx835_claim.ListClaimAdjustments=new List<Hx835_Adj>();
				_x835.ListClaimsPaid.Add(hx835_claim);
			}
			FillGridClaims();
			Cursor=Cursors.Arrow;
		}

		private void butDeleteClaims_Click(object sender,EventArgs e) {
			if(gridClaims.SelectedIndices.Length==0) {
				MsgBox.Show(this,"You must select some claims before you can delete.");
				return;
			}
			for(int i=0;i<gridClaims.SelectedIndices.Length;i++) {
				Hx835_Claim hx835_claim=(Hx835_Claim)gridClaims.ListGridRows[gridClaims.SelectedIndices[i]].Tag;
				_x835.ListClaimsPaid.Remove(hx835_claim);
			}
			FillGridClaims();
		}

		private void butSave_Click(object sender,EventArgs e) {
			SaveFileDialog saveFileDialog=new SaveFileDialog();
			string clearinghouseReportPath=Clearinghouses.GetDefaultDental().ResponsePath;
			if(Directory.Exists(clearinghouseReportPath)) {
				saveFileDialog.InitialDirectory=clearinghouseReportPath;
			}
			saveFileDialog.DefaultExt=".txt";
			saveFileDialog.FileName="era_fake_"+DateTime.Now.ToString("yyyyMMddHHmmss")+".txt";
			if(saveFileDialog.ShowDialog()==false) {
				return;
			}
			File.WriteAllText(saveFileDialog.FileName,_x835.GenerateMessageText());
			IsDialogOK=true;
		}

	}
}