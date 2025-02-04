using System;
using System.Data;

namespace OpenDental {
	
	public partial class FrmTestSplitters:FrmODBase {
		private int _countLoop=60;
		private DataTable _table;

		
		public FrmTestSplitters(){
			InitializeComponent();
			Load+=FrmFrmTestSplitters_Load;
		}

		private void FrmFrmTestSplitters_Load(object sender,EventArgs e) {
			webBrowser.Source=new Uri("https://www.opendental.com/");
		}

	}
}
