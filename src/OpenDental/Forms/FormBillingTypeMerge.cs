using System;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormBillingTypeMerge : FormODBase
{
    private long _defNumTo;
    private long _defNumFrom;

    public FormBillingTypeMerge()
    {
        InitializeComponent();
    }

    private void ButtonChangeInto_Click(object sender, EventArgs e)
    {
        using var formDefinitionPicker = new FormDefinitionPicker(DefCat.BillingTypes);

        formDefinitionPicker.IsMultiSelectionMode = false;

        if (formDefinitionPicker.ShowDialog() != DialogResult.OK || formDefinitionPicker.SelectedDefs.Count == 0)
        {
            return;
        }

        var def = formDefinitionPicker.SelectedDefs.First();

        textDefNumInto.Text = def.DefNum.ToString();
        textNameInto.Text = def.ItemName;
        textItemValueInto.Text = def.ItemValue;

        _defNumTo = def.DefNum;

        UpdateEnabledState();
    }

    private void ButtonChangeFrom_Click(object sender, EventArgs e)
    {
        using var formDefinitionPicker = new FormDefinitionPicker(DefCat.BillingTypes);

        formDefinitionPicker.IsMultiSelectionMode = false;

        if (formDefinitionPicker.ShowDialog() != DialogResult.OK || formDefinitionPicker.SelectedDefs.Count == 0)
        {
            return;
        }

        var def = formDefinitionPicker.SelectedDefs.First();

        textDefNumFrom.Text = def.DefNum.ToString();
        textNameFrom.Text = def.ItemName;
        textItemValueFrom.Text = def.ItemValue;

        _defNumFrom = def.DefNum;

        UpdateEnabledState();
    }

    private void UpdateEnabledState()
    {
        butMerge.Enabled = _defNumFrom > 0 && _defNumTo > 0;
    }

    private void ButtonMerge_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        if (_defNumTo == _defNumFrom)
        {
            ShowError("Cannot merge the same Billing Type. Please update either the merge into field or the merge From field.");
            return;
        }

        if (!Confirm("Are you sure? The results are permanent and cannot be undone."))
        {
            return;
        }

        try
        {
            Defs.MergeBillingTypeDefNums(_defNumFrom, _defNumTo);
        }
        catch (Exception ex)
        {
            ShowException(ex, "Billing Types failed to merge.");
            return;
        }

        DefL.HideDef(Defs.GetDef(DefCat.BillingTypes, _defNumFrom));
        DataValid.SetInvalid(InvalidType.Defs, InvalidType.ClinicPrefs, InvalidType.Prefs, InvalidType.Programs);

        var logMessage = "Billing Type Merge from " + Defs.GetName(DefCat.BillingTypes, _defNumFrom) + " to " + Defs.GetName(DefCat.BillingTypes, _defNumTo);

        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logMessage);

        ShowInfo("Billing Types merged successfully.");

        textDefNumFrom.Clear();
        textNameFrom.Clear();
        textItemValueFrom.Clear();

        _defNumFrom = 0;

        UpdateEnabledState();
    }
}