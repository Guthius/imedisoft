using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental{
	/// <summary></summary>
	public partial class FormLetterEdit : FormODBase {
		
		public bool IsNew;
		public Letter LetterCur;

		
		public FormLetterEdit()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormLetterEdit_Load(object sender, System.EventArgs e) {
			textDescription.Text=LetterCur.Description;
			textBody.Text=LetterCur.BodyText;
		}

		private void butSave_Click(object sender, System.EventArgs e) {
			LetterCur.Description=textDescription.Text;
			LetterCur.BodyText=textBody.Text;
			if(IsNew){
				Letters.Insert(LetterCur);
				DialogResult=DialogResult.OK;
				return;
			}
			Letters.Update(LetterCur);
			DialogResult=DialogResult.OK;
		}

	}
}