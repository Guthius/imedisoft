using System.Windows.Forms;

namespace OpenDental {
	
	public partial class ValidDate {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ){
			if(disposing){
				components?.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		private void InitializeComponent(){
			this.SuspendLayout();
			// 
			// ValidDate
			// 
			this.Validating += new System.ComponentModel.CancelEventHandler(this.ValidDate_Validating);
			this.ResumeLayout(false);

		}
		#endregion

	}
}