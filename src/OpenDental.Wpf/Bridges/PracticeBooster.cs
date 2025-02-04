using System.Diagnostics;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Bridges{
	public class PracticeBooster {

		
		public PracticeBooster(){
			
		}

		
		public static void SendData(Program ProgramCur,Patient pat) {
			string path=Programs.GetProgramPath(ProgramCur);
			try {
				Process.Start(path);//should start PracticeBooster without bringing up a pt.
			}
			catch {
				MessageBox.Show(path+" is not available.");
			}
			return;
		}


	}
}







