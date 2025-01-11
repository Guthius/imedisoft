using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenDentBusiness.BetterDiag {
	public class BetterDiagResponse {
		public int Status { get; set; }
		public Findings findings { get; set; }
	}

	public class Findings {
		///<summary>Must be Dictionary to deserialize dynamic JSON property name like "SmithJohn25.jpg".</summary>
		[JsonProperty(PropertyName="Analysis")]
		public Dictionary<string,List<ImageResult>> dictImageResults { get; set; }
	}

	public class ImageResult {
		public List<List<double>> points { get; set; }
		public string tag_name { get; set; }
		public double score { get; set; }
	}
}
