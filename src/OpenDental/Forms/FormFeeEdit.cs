using System;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;

namespace OpenDental {
	
	public partial class FormFeeEdit : FormODBase {
		
		public Fee FeeCur;
		
		public bool IsNew;

		
		public FormFeeEdit(){
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormFeeEdit_Load(object sender, System.EventArgs e) {
			FeeSched feeSched=FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum==FeeCur.FeeSched);
			if(!FeeL.CanEditFee(feeSched,FeeCur.ProvNum,FeeCur.ClinicNum)) {
				DialogResult=DialogResult.Cancel;
				Close();
				return;
			}
			Location=new Point(Location.X-190,Location.Y-20);
			textFee.Text=FeeCur.Amount.ToString("F");
			odDatePickerEffectiveDate.SetDateTime(FeeCur.DateEffective);
		}

		private void butSave_Click(object sender, System.EventArgs e) {
			if(!textFee.IsValid()) {
				MessageBox.Show(Lan.g(this,"Please fix data entry error first."));
				return;
			}
			if(textFee.Text!="" && Fees.CheckForDuplicate(FeeCur,odDatePickerEffectiveDate.GetDateTime())) {
				MessageBox.Show(Lan.g(this,"There is already a Fee with that Effective Date. Please enter another date."));
				return;
			}
			DateTime datePrevious=FeeCur.SecDateTEdit;
			if(textFee.Text==""){
				Fees.Delete(FeeCur);
			}
			else if(CompareDouble.IsEqual(FeeCur.Amount,PIn.Double(textFee.Text)) && DateTime.Equals(FeeCur.DateEffective,odDatePickerEffectiveDate.GetDateTime())) {
				DialogResult=DialogResult.OK;
				return;
			}
			else{
				Fee feeOld=FeeCur.Copy();
				FeeCur.Amount=PIn.Double(textFee.Text);
				FeeCur.DateEffective=PIn.Date(odDatePickerEffectiveDate.GetDateTime().ToShortDateString());
				Fees.Update(FeeCur,feeOld);//Fee object always created and inserted externally first
			}
			SecurityLogs.MakeLogEntry(EnumPermType.ProcFeeEdit,0,Lan.g(this,"Procedure")+": "+ProcedureCodes.GetStringProcCode(FeeCur.CodeNum)
				+", "+Lan.g(this,"Fee: ")+""+FeeCur.Amount.ToString("c")+", "+Lan.g(this,"Fee Schedule")+": "+FeeScheds.GetDescription(FeeCur.FeeSched)
				+". "+Lan.g(this,"Manual edit in Edit Fee window."),FeeCur.CodeNum,DateTime.MinValue);
			SecurityLogs.MakeLogEntry(EnumPermType.LogFeeEdit,0,Lan.g(this,"Fee Updated"),FeeCur.FeeNum,datePrevious);
			DialogResult=DialogResult.OK;
		}

		private void FormFeeEdit_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if(DialogResult==DialogResult.OK) {
				return;
			}
			if(IsNew){
				Fees.Delete(FeeCur);
			}
		}

	}
}
