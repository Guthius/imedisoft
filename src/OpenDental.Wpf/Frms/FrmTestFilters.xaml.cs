using System;
using System.Collections.Generic;
using System.Threading;
using Imedisoft.Core.Entities;

namespace OpenDental {
	
	public partial class FrmTestFilters:FrmODBase {
		FilterControlsAndAction _filterControlsAndAction;

		
		public FrmTestFilters() {
			InitializeComponent();
			_filterControlsAndAction=new FilterControlsAndAction();
			_filterControlsAndAction.AddControl(textBox);
			_filterControlsAndAction.AddControl(checkBox);
			_filterControlsAndAction.FuncDb=RefreshFromDb;
			//_filterControlsAndAction.SetInterval(700);
			_filterControlsAndAction.ActionComplete=FillGrid;
			textBox.Click+=TextBox_Click;
		}

		private void TextBox_Click(object sender,EventArgs e) {
			throw new NotImplementedException();
		}

		private List<Patient> RefreshFromDb(){
			Thread.Sleep(500);
			Dispatcher.Invoke(()=>textResults.Text+="Refreshing db1\r\n");
			DoEvents();
			Thread.Sleep(500);
			Dispatcher.Invoke(()=>textResults.Text+="Refreshing db2\r\n");
			DoEvents();
			Thread.Sleep(500);
			Dispatcher.Invoke(()=>textResults.Text+="Refreshing db3\r\n");
			DoEvents();
			Thread.Sleep(500);
			Dispatcher.Invoke(()=>textResults.Text+="Refreshing db4\r\n");
			DoEvents();
			Thread.Sleep(500);
			Dispatcher.Invoke(()=>textResults.Text+="Refreshing db5\r\n");
			DoEvents();
			Thread.Sleep(500);
			List<Patient> listPatients=new List<Patient>();
			Patient patient=new Patient();
			patient.LName="Smith";
			listPatients.Add(patient);
			return listPatients;
		}

		private void FillGrid(object data){
			List<Patient> listPatients=(List<Patient>)data;
			textResults.Text+="Fill Grid, patient name="+listPatients[0].LName+"\r\n";
		}

		
	}
}



