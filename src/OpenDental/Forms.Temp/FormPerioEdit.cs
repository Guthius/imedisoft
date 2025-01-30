using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDental;

/// <summary>
/// Summary description for FormBasicTemplate.
/// </summary>
public partial class FormPerioEdit : FormODBase {
	public PerioExam PerioExamCur;
	private List<Provider> _listProviders;

		
	public FormPerioEdit()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	private void FormPerioEdit_Load(object sender, System.EventArgs e) {
		textDate.Text=PerioExamCur.ExamDate.ToShortDateString();
		textBoxNotes.Text=PerioExamCur.Note;
		listProv.Items.Clear();
		_listProviders=Providers.GetDeepCopy(true);
		for(var i=0;i<_listProviders.Count;i++) {
			listProv.Items.Add(_listProviders[i].Abbr);
			if(_listProviders[i].ProvNum==PerioExamCur.ProvNum){
				listProv.SelectedIndex=i;
			}
		}
		if(listProv.SelectedIndex==-1) {
			listProv.SelectedIndex=0;
		}
	}

	private void butSave_Click(object sender, System.EventArgs e) {
		if(string.IsNullOrEmpty(textDate.Text) || !textDate.IsValid()){
			ODMessageBox.Show(Lan.g(this,"Please fix data entry errors first."));
			return;
		}
		PerioExamCur.ExamDate=SIn.Date(textDate.Text);
		PerioExamCur.Note=SIn.String(textBoxNotes.Text);
		PerioExamCur.ProvNum=_listProviders[listProv.SelectedIndex].ProvNum;
		PerioExams.Update(PerioExamCur);
		DialogResult=DialogResult.OK;
	}

}