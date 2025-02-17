using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptFieldDefs : FormODBase
{
    private List<ApptFieldDef> _apptFieldDefs;

    public FormApptFieldDefs()
    {
        InitializeComponent();
    }

    private void FormApptFieldDefs_Load(object sender, EventArgs e)
    {
        LayoutMenu();

        ApptFieldDefs.RefreshCache();

        _apptFieldDefs = ApptFieldDefs.GetDeepCopy();

        FillListMain();
    }

    private void FormApptFieldDefs_FormClosing(object sender, FormClosingEventArgs e)
    {
        ApptFieldDefs.Sync(_apptFieldDefs);

        DataValid.SetInvalid(InvalidType.PatFields);
        DataValid.SetInvalid(InvalidType.Views);
    }

    private void LayoutMenu()
    {
        menuMain.BeginUpdate();
        menuMain.Add(new MenuItemOD("Setup", MenuItemSetup_Click));
        menuMain.EndUpdate();
    }

    private void FillListMain()
    {
        listMain.Items.Clear();

        _apptFieldDefs.Sort(CompareItemOrder);

        var needsUpdate = false;
        for (var i = 0; i < _apptFieldDefs.Count; i++)
        {
            var apptFieldDefNum = _apptFieldDefs[i].ApptFieldDefNum;
            if (FieldDefLinks.GetExists(x => x.FieldDefType == FieldDefTypes.Appointment && x.FieldDefNum == apptFieldDefNum))
            {
                listMain.Items.Add(_apptFieldDefs[i].FieldName + " (Hidden)");
            }
            else
            {
                listMain.Items.Add(_apptFieldDefs[i].FieldName);
            }

            if (_apptFieldDefs[i].ItemOrder == i)
            {
                continue;
            }

            _apptFieldDefs[i].ItemOrder = i;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            ApptFieldDefs.Sync(_apptFieldDefs.ToList());
        }
    }

    private void MenuItemSetup_Click(object sender, EventArgs e)
    {
        ApptFieldDefs.Sync(_apptFieldDefs);
        ApptFieldDefs.RefreshCache();

        using var formFieldDefLink = new FormFieldDefLink(FieldLocations.AppointmentEdit);

        formFieldDefLink.ShowDialog();

        FillListMain();
    }

    private void ListBoxMain_DoubleClick(object sender, EventArgs e)
    {
        if (listMain.SelectedIndex == -1)
        {
            return;
        }

        var frmApptFieldDefEdit = new FrmApptFieldDefEdit
        {
            ApptFieldDef = _apptFieldDefs[listMain.SelectedIndex]
        };

        frmApptFieldDefEdit.ShowDialog();

        if (!frmApptFieldDefEdit.IsDialogOK)
        {
            return;
        }

        if (frmApptFieldDefEdit.ApptFieldDef == null)
        {
            _apptFieldDefs.Remove(_apptFieldDefs[listMain.SelectedIndex]);
        }
        else
        {
            _apptFieldDefs[listMain.SelectedIndex] = frmApptFieldDefEdit.ApptFieldDef;
        }

        FillListMain();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var frmApptFieldDefEdit = new FrmApptFieldDefEdit
        {
            ApptFieldDef = new ApptFieldDef
            {
                ItemOrder = _apptFieldDefs.Count
            },
            IsNew = true
        };

        frmApptFieldDefEdit.ShowDialog();
        if (!frmApptFieldDefEdit.IsDialogOK)
        {
            return;
        }

        _apptFieldDefs.Add(frmApptFieldDefEdit.ApptFieldDef);

        FillListMain();
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        var index = listMain.SelectedIndex;
        if (index == -1)
        {
            ShowError("Please select an Appointment Field Definition first.");
            return;
        }

        if (index == listMain.Items.Count - 1)
        {
            return;
        }

        var apptFieldDefTemp = _apptFieldDefs[index];

        _apptFieldDefs[index] = _apptFieldDefs[index + 1];
        _apptFieldDefs[index].ItemOrder--;
        _apptFieldDefs[index + 1] = apptFieldDefTemp;
        _apptFieldDefs[index + 1].ItemOrder++;

        FillListMain();

        listMain.SetSelected(index + 1);
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        var index = listMain.SelectedIndex;
        switch (index)
        {
            case -1:
                ShowError("Please select an Appointment Field Definition first.");
                return;

            case 0:
                return;
        }

        var apptFieldDefTemp = _apptFieldDefs[index];

        _apptFieldDefs[index] = _apptFieldDefs[index - 1];
        _apptFieldDefs[index].ItemOrder++;
        _apptFieldDefs[index - 1] = apptFieldDefTemp;
        _apptFieldDefs[index - 1].ItemOrder--;

        FillListMain();

        listMain.SetSelected(index - 1);
    }

    private static int CompareItemOrder(ApptFieldDef apptFieldDef1, ApptFieldDef apptFieldDef2)
    {
        return apptFieldDef1.ItemOrder.CompareTo(apptFieldDef2.ItemOrder);
    }
}