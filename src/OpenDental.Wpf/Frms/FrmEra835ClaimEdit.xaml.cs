using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {
	
	public partial class FrmEra835ClaimEdit:FrmODBase {

		public Hx835_Claim Hx835Claim;

		
		public FrmEra835ClaimEdit() {
			InitializeComponent();
			Load+=Frm_Load;
		}

		private void Frm_Load(object sender, EventArgs e) {
			Lang.F(this);
			textVDoubleClaimFeeOverride.Value=(double)Hx835Claim.ClaimFee;
			FillGridProcedures();
		}

		private void FillGridProcedures() {
			gridProcedures.BeginUpdate();
			gridProcedures.Columns.Clear();
			//Most column names and widths below mimic those in FormEtrans835ClaimEdit.
			GridColumn gridColumn=new GridColumn(Lang.g(this,"ProcCodeBilled"),130,HorizontalAlignment.Center);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"ProcCodeAdjudicated"),130,HorizontalAlignment.Center);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"FeeBilled"),70,HorizontalAlignment.Right);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"InsPaid"),70,HorizontalAlignment.Right);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"PatPort"),70,HorizontalAlignment.Right);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Deduct"),70,HorizontalAlignment.Right);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Writeoff"),70,HorizontalAlignment.Right);
			gridColumn.IsEditable=true;
			gridProcedures.Columns.Add(gridColumn);
			gridColumn=new GridColumn(Lang.g(this,"Remarks"),0);
			gridProcedures.Columns.Add(gridColumn);
			gridProcedures.ListGridRows.Clear();
			for(int i=0;i<Hx835Claim.ListProcs.Count;i++) {
				GridRow gridRow=new GridRow();
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].ProcCodeBilled);
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].ProcCodeAdjudicated);
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].ProcFee.ToString("f2"));
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].InsPaid.ToString("f2"));
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].PatientPortionAmt.ToString("f2"));
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].DeductibleAmt.ToString("f2"));
				gridRow.Cells.Add(Hx835Claim.ListProcs[i].WriteoffAmt.ToString("f2"));
				gridRow.Tag=Hx835Claim.ListProcs[i];
				gridProcedures.ListGridRows.Add(gridRow);
			}
			gridProcedures.EndUpdate();
			textClaimFee.Text=Hx835Claim.ListProcs.Sum(x => x.ProcFee).ToString();//Simple ToString() to mimic behavior for the claim fee override field.
		}

		private void butSave_Click(object sender,EventArgs e) {
			for(int i=0;i<gridProcedures.ListGridRows.Count;i++) {
				Hx835_Proc hx835_proc=(Hx835_Proc)gridProcedures.ListGridRows[i].Tag;
				hx835_proc.ProcCodeBilled=gridProcedures.ListGridRows[i].Cells[0].Text;
				hx835_proc.ProcCodeAdjudicated=gridProcedures.ListGridRows[i].Cells[1].Text;
				hx835_proc.ProcFee=PIn.Decimal(gridProcedures.ListGridRows[i].Cells[2].Text);
				hx835_proc.InsPaid=PIn.Decimal(gridProcedures.ListGridRows[i].Cells[3].Text);
				hx835_proc.PatientPortionAmt=PIn.Decimal(gridProcedures.ListGridRows[i].Cells[4].Text);
				hx835_proc.DeductibleAmt=PIn.Decimal(gridProcedures.ListGridRows[i].Cells[5].Text);
				hx835_proc.WriteoffAmt=PIn.Decimal(gridProcedures.ListGridRows[i].Cells[6].Text);
			}
			Hx835Claim.ClaimFee=PIn.Decimal(textClaimFee.Text);
			if(textVDoubleClaimFeeOverride.Text!="") {
				Hx835Claim.ClaimFee=(decimal)textVDoubleClaimFeeOverride.Value;
			}
			IsDialogOK=true;
		}

	}
}