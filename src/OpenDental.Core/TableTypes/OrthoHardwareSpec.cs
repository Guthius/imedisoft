using System;
using System.Collections;
using System.Drawing;
using System.Xml.Serialization;

namespace OpenDentBusiness {
	///<summary>Specification for ortho hardware. Linked to one type such as bracket, wire, or elastic. This is a pick list of description and color for the user. These remain linked to patient data, so changes here will affect historical chart entries.</summary>
	[Serializable]
	public class OrthoHardwareSpec:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long OrthoHardwareSpecNum;
		///<summary>Enum:EnumOrthoHardwareType Bracket, Wire, or Elastic.</summary>
		public EnumOrthoHardwareType OrthoHardwareType;
		///<summary>Example NITI 16x25</summary>
		public string Description;
		
		[XmlIgnore]
		public Color ItemColor;
		
		public bool IsHidden;
		///<summary>0 indexed. User controls it with arrows.</summary>
		public int ItemOrder;

		///<summary>Used only for serialization purposes</summary>
		[XmlElement("ItemColor",typeof(int))]
		public int ItemColorXml {
			get {
				return ItemColor.ToArgb();
			}
			set {
				ItemColor = Color.FromArgb(value);
			}
		}

		
		public OrthoHardwareSpec Copy() {
			return (OrthoHardwareSpec)this.MemberwiseClone();
		}

	}
}

