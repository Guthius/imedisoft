using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDentBusiness {
	///<summary>Can be used when using clinics, and a single fee schedule has fees that are region specific and need to be different for different clinics. A FeeSchedGroup stores a list of clinics that one fee schedule applies to, overriding the normal fee sched. This is designed so that you can have a few "groups" per fee sched instead of dozens or hundreds of clinics. Fees are still created on a per-clinic basis, and we attempt to manage them when we do things like change fees or edit groups.</summary>
	[Serializable]
	public class FeeSchedGroup:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long FeeSchedGroupNum;
		
		public string Description;
		///<summary>FK to FeeSched.FeeSchedNum.</summary>
		public long FeeSchedNum;
		///<summary>Comma delimited list of Clinic.ClinicNums.</summary>
		public string ClinicNums;

		
		public FeeSchedGroup Copy() {
			return (FeeSchedGroup)this.MemberwiseClone();
		}

		///<summary>The list of Clinic.ClinicNums filled from the ClinicNums comma delimited string.  Does not filter out restricted clinics.</summary>
		[XmlIgnore, JsonIgnore]
		public List<long> ListClinicNumsAll {
			get {
				if(this.ClinicNums=="") {
					return new List<long>();
				}
				return new List<long>(this.ClinicNums.Split(',').Select(long.Parse).Distinct().ToList());
			}
			set {
				ClinicNums=string.Join(",",value);
			}
		}

		///<summary>The list of Clinic.ClinicNums filled from the ClinicNums comma delimited string that exist in the given list of clinics.</summary>
		public List<long> GetListClinicNumsFiltered(List<ClinicDto> listClinicsFiltered) {
			return new List<long>(this.ClinicNums.Split(',').Select(long.Parse).Distinct().ToList())
				.FindAll(x => listClinicsFiltered.Select(y => y.Id).Contains(x));
		}
	}
}