using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {
	
	public partial class FrmApptFieldEdit:FrmODBase {
		private ApptField _field;

		
		public FrmApptFieldEdit(ApptField field) {
			InitializeComponent();
			_field=field;
			KeyDown+=Frm_KeyDown;
			Load+=FrmApptFieldEdit_Load;
			PreviewKeyDown+=FrmApptFieldEdit_PreviewKeyDown;
		}

		private void FrmApptFieldEdit_Load(object sender, EventArgs e) {
			Lang.F(this);
			labelName.Text=_field.FieldName;
			textValue.Text=_field.FieldValue;
		}

		private void Frm_KeyDown(object sender,KeyEventArgs e) {
			if(e.Key==Key.Enter) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void FrmApptFieldEdit_PreviewKeyDown(object sender,KeyEventArgs e) {
			if(butSave.IsAltKey(Key.S,e)) {
				butSave_Click(this,new EventArgs());
			}
		}

		private void butSave_Click(object sender, System.EventArgs e) {
			_field.FieldValue=textValue.Text;
			if(_field.FieldValue=="") {//if blank, then delete
				if(_field.IsNew) {
					IsDialogOK=false;
					return;
				}
				ApptFields.DeleteFieldForAppt(_field.FieldName,_field.AptNum);
			}
			else {
				ApptFields.Upsert(_field);
			}
			IsDialogOK=true;
		}

	}
}