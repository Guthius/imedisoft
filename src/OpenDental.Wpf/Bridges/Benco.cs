using System;
using System.Diagnostics;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Bridges{
	public class Benco {

		
		public Benco(){
			
		}

		
		public static void SendData(Program ProgramCur) {
			string path=ProgramCur.Path;
			try {
				Process.Start(path);
			}
			catch(Exception ex) {
				FriendlyException.Show(Lans.g("Benco","Unable to launch")+" "+ProgramCur.ProgDesc+".",ex);
			}
		}

	}
}