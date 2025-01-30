using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptViews : FormODBase
{
    private bool _changed;
    private List<ApptView> _apptViews;

    public FormApptViews()
    {
        InitializeComponent();
    }

    private void FormApptViews_Load(object sender, EventArgs e)
    {
        comboClinic.ClinicNumSelected = Clinics.ClinicNum;

        FillViewList();

        if (PrefC.GetInt(PrefName.AppointmentTimeIncrement) == 5)
        {
            radioFive.Checked = true;
        }
        else if (PrefC.GetInt(PrefName.AppointmentTimeIncrement) == 10)
        {
            radioTen.Checked = true;
        }
        else
        {
            radioFifteen.Checked = true;
        }
    }

    private void FormApptViews_FormClosing(object sender, FormClosingEventArgs e)
    {
        var newIncrement = 15;
        if (radioFive.Checked)
        {
            newIncrement = 5;
        }

        if (radioTen.Checked)
        {
            newIncrement = 10;
        }

        if (Prefs.UpdateInt(PrefName.AppointmentTimeIncrement, newIncrement))
        {
            DataValid.SetInvalid(InvalidType.Prefs);
        }

        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Views);
        }
    }

    private void FillViewList()
    {
        ApptViews.RefreshCache();
        ApptViewItems.RefreshCache();

        listViews.Items.Clear();

        _apptViews = [];

        var apptViews = ApptViews.GetDeepCopy();

        apptViews = apptViews.FindAll(x => x.ClinicNum == comboClinic.ClinicNumSelected);
        for (var i = 0; i < apptViews.Count; i++)
        {
            if (apptViews[i].ItemOrder != i)
            {
                apptViews[i].ItemOrder = i;

                ApptViews.Update(apptViews[i]);

                _changed = true;
            }

            string prefix;
            if (listViews.Items.Count < 12)
            {
                prefix = "F" + (listViews.Items.Count + 1) + "-";
            }
            else
            {
                prefix = "";
            }

            listViews.Items.Add(prefix + apptViews[i].Description);

            _apptViews.Add(apptViews[i]);
        }
    }

    private void ComboBoxClinic_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillViewList();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var apptView = new ApptView();
        if (_apptViews.Count == 0)
        {
            apptView.ItemOrder = 0;
        }
        else
        {
            apptView.ItemOrder = _apptViews[_apptViews.Count - 1].ItemOrder + 1;
        }

        apptView.ApptTimeScrollStart = DateTime.Parse("08:00:00").TimeOfDay;
        apptView.RowsPerIncr = 1;

        ApptViews.Insert(apptView);

        using var formApptViewEdit = new FormApptViewEdit(apptView, comboClinic.ClinicNumSelected);

        if (formApptViewEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillViewList();

        listViews.SelectedIndex = listViews.Items.Count - 1;
    }

    private void ButtonCopyView_Click(object sender, EventArgs e)
    {
        var index = listViews.SelectedIndex;
        if (index == -1)
        {
            ShowError("Select a view to copy first.");
            return;
        }

        if (!ConfirmOk("This will make a copy of the selected view. Click OK to continue, otherwise click Cancel."))
        {
            return;
        }

        var apptViewOriginal = _apptViews[index];
        var apptViewCopy = apptViewOriginal.Copy();

        apptViewCopy.ItemOrder = _apptViews.Count;

        var uniqueId = 1;

        string viewDescription;
        while (true)
        {
            viewDescription = apptViewCopy.Description + "_" + uniqueId;
            if (_apptViews.All(x => x.Description != viewDescription))
            {
                break;
            }

            uniqueId++;
        }

        apptViewCopy.Description = viewDescription;

        var apptViewNum = ApptViews.Insert(apptViewCopy);

        var apptViewItems = ApptViewItems.GetWhere(x => x.ApptViewNum == apptViewOriginal.ApptViewNum);
        var newApptViewItems = new List<ApptViewItem>();

        foreach (var viewItem in apptViewItems)
        {
            var apptViewItem = viewItem.Copy();

            apptViewItem.ApptViewNum = apptViewNum;

            newApptViewItems.Add(apptViewItem);
        }

        ApptViewItems.InsertMany(newApptViewItems);

        _changed = true;

        FillViewList();

        listViews.SetSelected(listViews.Items.Count - 1);
    }

    private void ListViews_DoubleClick(object sender, EventArgs e)
    {
        if (listViews.SelectedIndex == -1)
        {
            return;
        }

        var selected = listViews.SelectedIndex;
        var apptView = _apptViews[listViews.SelectedIndex];

        using var formApptViewEdit = new FormApptViewEdit(apptView, comboClinic.ClinicNumSelected);

        if (formApptViewEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _changed = true;

        FillViewList();

        if (selected < listViews.Items.Count)
        {
            listViews.SelectedIndex = selected;
        }
        else
        {
            listViews.SelectedIndex = -1;
        }
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        switch (listViews.SelectedIndex)
        {
            case -1:
                ShowError("Please select a category first.");
                return;

            case 0:
                return;
        }

        var apptView = _apptViews[listViews.SelectedIndex - 1];

        apptView.ItemOrder = listViews.SelectedIndex;

        ApptViews.Update(apptView);

        apptView = _apptViews[listViews.SelectedIndex];
        apptView.ItemOrder = listViews.SelectedIndex - 1;

        ApptViews.Update(apptView);

        _changed = true;

        FillViewList();

        listViews.SelectedIndex = _apptViews.FindIndex(x => x.ApptViewNum == apptView.ApptViewNum);
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        if (listViews.SelectedIndex == -1)
        {
            ShowError("Please select a category first.");
            return;
        }

        if (listViews.SelectedIndex == listViews.Items.Count - 1)
        {
            return;
        }

        var apptView = _apptViews[listViews.SelectedIndex + 1];
        apptView.ItemOrder = listViews.SelectedIndex;

        ApptViews.Update(apptView);

        apptView = _apptViews[listViews.SelectedIndex];
        apptView.ItemOrder = listViews.SelectedIndex + 1;

        ApptViews.Update(apptView);

        _changed = true;

        FillViewList();

        listViews.SelectedIndex = _apptViews.FindIndex(x => x.ApptViewNum == apptView.ApptViewNum);
    }

    private void ButtonProcColors_Click(object sender, EventArgs e)
    {
        using var formProcApptColors = new FormProcApptColors();

        formProcApptColors.ShowDialog();

        DialogResult = DialogResult.None;
    }
}