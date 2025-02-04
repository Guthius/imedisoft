using System.Windows.Media;

namespace WpfControls.UI {
	public class GridCellOld {
		///<summary>If null, then the row state is used for bold.  Otherwise, this overrides the row.</summary>
		public bool? Bold { get; set; } = null;
		///<summary>Do not set this manually.  Only used for binding.</summary>
		public string BoldStr { get; set; }
		///<summary>Default is Color.Transparent.  If any color is set, it will override the background color.</summary>
		public Color ColorBackG {get;set;}= Colors.Transparent;
		///<summary>Do not set this manually.  Only used for binding. This is a combination of row colors and cell overrides.  So all cells get an individual color set for binding.  Triggers are used to change color of a separate transparent overlay for selection and hover so they are unrelated.</summary>
		public string ColorBackGStr{get;set;}
		///<summary>Default is Color.Transparent.  If any color is set, it will override the row color.</summary>
		public Color ColorText {get;set;}= Colors.Transparent;
		///<summary>Do not set this manually.  Only used for binding.</summary>
		public string ColorTextStr{get;set; }
		
		public string Text {get;set; } = "";

		
		public GridCellOld(){

		}

		public GridCellOld(string text){
			Text=text;
		}
	}
}
