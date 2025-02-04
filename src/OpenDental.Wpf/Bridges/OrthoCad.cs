using System;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Bridges{
	public class OrthoCad {

		
		public OrthoCad(){
			
		}

		
		public static void SendData(Program ProgramCur,Patient pat) {
			if(pat==null) {
				MsgBox.Show("OrthoCAD","Please select a patient first.");
				return;
			}
			string path=Programs.GetProgramPath(ProgramCur);
			string cmd="";
			if(ProgramProperties.GetPropVal(ProgramCur.ProgramNum,"Enter 0 to use PatientNum, or 1 to use ChartNum")=="0") {
				cmd+="-patient_id="+SOut.Long(pat.PatNum);
			}
			else {
				cmd+="-chart_number="+pat.ChartNumber;
			}
			try {
				ODFileUtils.ProcessStart(path,cmd);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

	}
}







