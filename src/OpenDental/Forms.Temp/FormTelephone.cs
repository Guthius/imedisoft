using System.Globalization;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

/// <summary></summary>
public partial class FormTelephone : FormODBase {

		
	public FormTelephone()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormTelephone_Load(object sender, System.EventArgs e) {
		
	}

	private void butReformat_Click(object sender, System.EventArgs e) {
		if(CultureInfo.CurrentCulture.Name!="en-US"){
			if(ODMessageBox.Show(Lan.g(this,"Are you sure?  The phone number formatting is only meant for the United States?"),"",MessageBoxButtons.OKCancel)!=DialogResult.OK){
				return;
			}
		}
		Patients.ReformatAllPhoneNumbers();
		//refresh carriers:
		DataValid.SetInvalid(InvalidType.Carriers);
		ODMessageBox.Show(Lan.g(this,"Telephone numbers reformatted."));
		SecurityLogs.MakeLogEntry(EnumPermType.Setup,0,"Telephone");
	}

}