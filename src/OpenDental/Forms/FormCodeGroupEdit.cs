using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormCodeGroupEdit : FormODBase
{
    private readonly CodeGroup _codeGroup;
    private readonly List<EnumCodeGroupFixed> _codeGroupFixedsInUse;

    public FormCodeGroupEdit(CodeGroup codeGroup, List<EnumCodeGroupFixed> codeGroupFixedsInUse)
    {
        _codeGroup = codeGroup;
        _codeGroupFixedsInUse = codeGroupFixedsInUse;

        InitializeComponent();
    }

    private void FormCodeGroupEdit_Load(object sender, EventArgs e)
    {
        if (!CultureInfo.CurrentCulture.Name.EndsWith("US"))
        {
            labelCodeListDesc.Text = "List of procedure codes with commas and dashes. No spaces.";
        }

        checkShowInFreq.Checked = !_codeGroup.IsHidden;
        checkShowInAgeLim.Checked = _codeGroup.ShowInAgeLimit;

        textGroupName.Text = _codeGroup.GroupName;
        textProcCodes.Text = _codeGroup.ProcCodes;

        var codeGroupFixeds = Enum.GetValues(typeof(EnumCodeGroupFixed)).Cast<EnumCodeGroupFixed>().ToList();
        if (!_codeGroupFixedsInUse.IsNullOrEmpty())
        {
            _codeGroupFixedsInUse.RemoveAll(x => x.In(EnumCodeGroupFixed.None, _codeGroup.CodeGroupFixed));

            codeGroupFixeds = codeGroupFixeds.Except(_codeGroupFixedsInUse).ToList();
        }

        comboCodeGroupFixed.Items.AddListEnum(codeGroupFixeds);
        comboCodeGroupFixed.SetSelectedEnum(_codeGroup.CodeGroupFixed);
    }

    private void ButtonProcedureCodesAdd_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes();

        formProcCodes.IsSelectionMode = true;
        formProcCodes.CanAllowMultipleSelections = true;

        if (formProcCodes.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var procedureCodes = GetProcedureCodesFromTextBox();

        procedureCodes.AddRange(formProcCodes.ListProcedureCodesSelected.Select(x => x.ProcCode));

        textProcCodes.Text = string.Join(",", procedureCodes);
    }

    private List<string> GetProcedureCodesFromTextBox()
    {
        return textProcCodes.Text
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct()
            .ToList();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textGroupName.Text))
        {
            ShowError("Group Name is required.");
            return;
        }

        var procedureCodes = GetProcedureCodesFromTextBox();
        foreach (var procedureCode in procedureCodes)
        {
            var codes = procedureCode.Split('-').ToList();
            if (codes.Count <= 2)
            {
                continue;
            }

            ShowError("Ranges of codegroups must be formatted with a dash between two codes.");
            return;
        }

        _codeGroup.CodeGroupFixed = comboCodeGroupFixed.GetSelected<EnumCodeGroupFixed>();
        _codeGroup.GroupName = textGroupName.Text;
        _codeGroup.IsHidden = !checkShowInFreq.Checked;
        _codeGroup.ShowInAgeLimit = checkShowInAgeLim.Checked;
        _codeGroup.ProcCodes = string.Join(",", procedureCodes);

        DialogResult = DialogResult.OK;
    }
}