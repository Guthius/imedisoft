using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;

namespace OpenDental.Forms;

public partial class FormPerioEdit : FormODBase
{
    private readonly PerioExam _perioExam;
    private List<ProviderDto> _providerDtos;

    public FormPerioEdit(PerioExam perioExam)
    {
        _perioExam = perioExam;

        InitializeComponent();
    }

    private void FormPerioEdit_Load(object sender, EventArgs e)
    {
        _providerDtos = Providers.GetDeepCopy(true);
        
        textDate.Text = _perioExam.ExamDate.ToShortDateString();
        textBoxNotes.Text = _perioExam.Note;

        listProv.Items.Clear();
        
        for (var i = 0; i < _providerDtos.Count; i++)
        {
            listProv.Items.Add(_providerDtos[i].Abbr);
            if (_providerDtos[i].Id == _perioExam.ProvNum)
            {
                listProv.SelectedIndex = i;
            }
        }

        if (listProv.SelectedIndex == -1)
        {
            listProv.SelectedIndex = 0;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textDate.Text) || !textDate.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        _perioExam.ExamDate = SIn.Date(textDate.Text);
        _perioExam.Note = SIn.String(textBoxNotes.Text);
        _perioExam.ProvNum = _providerDtos[listProv.SelectedIndex].Id;

        PerioExams.Update(_perioExam);

        DialogResult = DialogResult.OK;
    }
}