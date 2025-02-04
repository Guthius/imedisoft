using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormQueryEdit : FormODBase {
	public UserQuery UserQueryCur;

		
	public FormQueryEdit(){
		InitializeComponent();// Required for Windows Form Designer support
	}

	private void FormQueryEdit_Load(object sender, System.EventArgs e) {
		textTitle.Text=UserQueryCur.Description;
		textQuery.Text=UserQueryCur.QueryText;
		textFileName.Text=UserQueryCur.FileName;
		checkReleased.Checked=UserQueryCur.IsReleased;
		if(UserQueryCur.DefaultFormatRaw){
			radioRaw.Checked=true;
		}
		else{
			radioHuman.Checked=true;
		}
		checkIsPromptSetup.Checked=UserQueryCur.IsPromptSetup;
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(textTitle.Text==""){
			ODMessageBox.Show(Lan.g(this,"Please enter a title first."));
			return;
		}
		UserQueryCur.Description=textTitle.Text;
		UserQueryCur.QueryText=textQuery.Text;
		UserQueryCur.FileName=textFileName.Text;
		UserQueryCur.IsReleased=checkReleased.Checked;
		UserQueryCur.IsPromptSetup=checkIsPromptSetup.Checked;
		UserQueryCur.DefaultFormatRaw=radioRaw.Checked;
		if(UserQueryCur.IsNew){
			UserQueries.Insert(UserQueryCur);
		}
		else{
			UserQueries.Update(UserQueryCur);
		}
		DataValid.SetInvalid(InvalidType.UserQueries);
		DialogResult=DialogResult.OK;
	}

}