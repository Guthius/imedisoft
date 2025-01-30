using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness {
	public class WebServiceMainHQReal : OpenDentBusiness.WebServiceMainHQ.WebServiceMainHQ, IWebServiceMainHQ {
		public List<long> GetEServiceClinicsAllowed(List<long> listClinicNums,eServiceCode eService) {
			return WebServiceMainHQProxy.GetEServiceClinicsAllowed(listClinicNums,eService);
		}
	}
}
