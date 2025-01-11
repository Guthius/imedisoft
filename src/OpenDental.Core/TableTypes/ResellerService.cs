using System;
using System.Xml.Serialization;

namespace OpenDentBusiness {

	///<summary>HQ only table. An entry in a list of services for a specific reseller to pick from.  To determine which services a certain customer has access to, check the repeating charges table.</summary>
	[Serializable]
	[CrudTable(IsMissingInGeneral=true)]
	public class ResellerService:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long ResellerServiceNum;
		///<summary>FK to reseller.ResellerNum.</summary>
		public long ResellerNum;
		///<summary>FK to procedurecode.CodeNum.</summary>
		public long CodeNum;
		///<summary>Amount of the service.  Might be a default, or might be the same for all customers of the reseller.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.Double, DecimalPlaces=4)]
		public double Fee;
		///<summary>URL for mass email host</summary>
		public string HostedUrl;
		///<summary>Amount of the service we will display on the signup portal. Default -1. If -1 then the service will not show on signup portal.
		///We will always display the most up-to-date rates.</summary>
		[CrudColumn(SpecialType=CrudSpecialColType.Double,DecimalPlaces=4)]
		public double FeeRetail=-1;
		/// <summary>If > 0 then second Reseller Software only RC will be created after initial fee period has expired.</summary>
		public double Fee2;
		/// <summary>If > 0 then initial RC (linked to Fee) will be given a DateStop on Fee1Duration expiration date. Second Reseller Software only RC will be created with a DateStart same as initial DateStop (follows OD support monthly maint pattern).</summary>
		[XmlIgnore]
		[CrudColumn(SpecialType = CrudSpecialColType.TimeSpanLong)]
		public TimeSpan Fee1Duration;

		///<summary>Used only for serialization purposes. In order to xml serialize, this CANNOT be implemented from an interface.</summary>
		[XmlElement("Fee1Duration",typeof(long))]
		public long Fee1DurationXml {
			get {
				return Fee1Duration.Ticks;
			}
			set {
				Fee1Duration=TimeSpan.FromTicks(value);
			}
		}

		///<summary>Returns a copy of this ResellerService.</summary>
		public ResellerService Copy() {
			return (ResellerService)this.MemberwiseClone();
		}
	}





}













