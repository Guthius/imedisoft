using System;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutomationConditionEdit : FormODBase
{
    private readonly AutomationCondition _automationCondition;

    public FormAutomationConditionEdit(AutomationCondition automationCondition)
    {
        _automationCondition = automationCondition;

        InitializeComponent();
    }

    private void FormAutomationConditionEdit_Load(object sender, EventArgs e)
    {
        var autoCondFields = Enum.GetValues(typeof(AutoCondField)).OfType<AutoCondField>();
        foreach (var autoCondField in autoCondFields)
        {
            listCompareField.Items.Add(autoCondField.GetDescription());
            listCompareField.SelectedIndex = 0;
        }

        var autoCondComparisons = Enum.GetNames(typeof(AutoCondComparison));
        foreach (var autoCondComparison in autoCondComparisons)
        {
            if (autoCondComparison == "None")
            {
                continue;
            }

            listComparison.Items.Add(autoCondComparison);
            listComparison.SelectedIndex = 0;
        }

        if (_automationCondition.AutomationConditionNum == 0)
        {
            return;
        }

        textCompareString.Text = _automationCondition.CompareString;
        listCompareField.SelectedIndex = (int) _automationCondition.CompareField;

        if (_automationCondition.CompareField is not (AutoCondField.InsuranceNotEffective or AutoCondField.IsControlled or AutoCondField.IsProcRequired or AutoCondField.IsPatientInstructionPresent))
        {
            listComparison.SelectedIndex = (int) _automationCondition.Comparison;
        }

        ShowOrHideFields((AutoCondField) listCompareField.SelectedIndex);
    }

    private void ShowOrHideFields(AutoCondField autoCondField)
    {
        if (autoCondField is AutoCondField.BillingType or AutoCondField.ClaimContainsProcCode)
        {
            butSelect.Visible = true;
            labelCompareString.Text = "Text (Type or pick from list)";
        }
        else
        {
            butSelect.Visible = false;
            labelCompareString.Text = "Text";
        }

        switch (autoCondField)
        {
            case AutoCondField.InsuranceNotEffective or AutoCondField.IsControlled or AutoCondField.IsProcRequired or AutoCondField.IsPatientInstructionPresent:
            {
                labelWarning.Visible = true;

                if (autoCondField == AutoCondField.InsuranceNotEffective)
                {
                    labelWarning.Text =
                        "At any point in time this rule is triggered, the current date and time will be compared to " +
                        "the effective date range of the current patient's primary insurance. " +
                        "If it does not fall within the range then the action for this automation will be triggered.";
                }
                else
                {
                    labelWarning.Text = "This automation field will only work when using the RxCreate trigger.";
                }

                labelComparison.Visible = false;
                labelCompareString.Visible = false;
                listComparison.Visible = false;
                textCompareString.Visible = false;
                break;
            }

            case AutoCondField.ClaimContainsProcCode:
                labelWarning.Visible = true;
                labelCompareString.Visible = true;
                textCompareString.Visible = true;
                labelComparison.Visible = false;
                listComparison.Visible = false;
                labelWarning.Text = "This automation field will only work when using the CreateClaim or OpenClaim trigger.";
                break;

            default:
                labelWarning.Visible = false;
                labelComparison.Visible = true;
                labelCompareString.Visible = true;
                listComparison.Visible = true;
                textCompareString.Visible = true;
                break;
        }
    }

    private bool ReasonableLogic()
    {
        var autoCondComparison = (AutoCondComparison) listComparison.SelectedIndex;
        var autoCondField = (AutoCondField) listCompareField.SelectedIndex;

        if (autoCondField != AutoCondField.Age)
        {
            if (autoCondComparison is AutoCondComparison.GreaterThan or AutoCondComparison.LessThan)
            {
                return false;
            }
        }
        else
        {
            if (!int.TryParse(textCompareString.Text, out _))
            {
                return false;
            }
        }

        return true;
    }

    private void ListBoxCompareField_Click(object sender, EventArgs e)
    {
        ShowOrHideFields((AutoCondField) listCompareField.SelectedIndex);
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_automationCondition.AutomationConditionNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!ConfirmOk("Delete this condition?"))
        {
            return;
        }

        try
        {
            AutomationConditions.Delete(_automationCondition.AutomationConditionNum);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);

            return;
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonSelect_Click(object sender, EventArgs e)
    {
        var autoCondField = (AutoCondField) listCompareField.SelectedIndex;

        switch (autoCondField)
        {
            case AutoCondField.BillingType:
            {
                using var formDefinitionPicker = new FormDefinitionPicker(DefCat.BillingTypes);

                formDefinitionPicker.HasShowHiddenOption = false;
                formDefinitionPicker.IsMultiSelectionMode = false;

                if (formDefinitionPicker.ShowDialog() == DialogResult.OK)
                {
                    if (formDefinitionPicker.ListDefsSelected.Count == 0)
                    {
                        textCompareString.Text = "";
                        return;
                    }

                    textCompareString.Text = formDefinitionPicker.ListDefsSelected?[0]?.ItemName ?? "";
                }

                break;
            }

            case AutoCondField.ClaimContainsProcCode:
            {
                using var formProcCodes = new FormProcCodes();

                formProcCodes.IsSelectionMode = true;

                if (formProcCodes.ShowDialog() == DialogResult.OK)
                {
                    textCompareString.Text = ProcedureCodes.GetStringProcCode(formProcCodes.CodeNumSelected);
                }

                break;
            }
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var autoCondField = (AutoCondField) listCompareField.SelectedIndex;

        switch (autoCondField)
        {
            case AutoCondField.InsuranceNotEffective or AutoCondField.IsProcRequired or AutoCondField.IsControlled or AutoCondField.IsPatientInstructionPresent:
                _automationCondition.CompareString = "";
                _automationCondition.Comparison = AutoCondComparison.None;
                break;

            case AutoCondField.ClaimContainsProcCode when textCompareString.Text.Contains(" "):
                ShowError("Procedure codes cannot contain any spaces.");
                return;

            case AutoCondField.ClaimContainsProcCode when textCompareString.Text.Trim() == "":
                ShowError("Please enter a valid procedure code first.");
                return;

            case AutoCondField.ClaimContainsProcCode when !ProcedureCodes.IsValidCode(textCompareString.Text):
                ShowError("Procedure code is not valid");
                return;

            case AutoCondField.ClaimContainsProcCode:
                _automationCondition.CompareString = textCompareString.Text;
                _automationCondition.Comparison = AutoCondComparison.None;
                break;

            default:
            {
                if (textCompareString.Text.Trim() == "")
                {
                    ShowError("Text not allowed to be blank.");
                    return;
                }

                if (!ReasonableLogic())
                {
                    ShowError("Comparison does not make sense with chosen field.");
                    return;
                }

                if (autoCondField == AutoCondField.Gender && !(textCompareString.Text.ToLower() == "m" || textCompareString.Text.ToLower() == "f"))
                {
                    ShowError("Allowed gender values are M or F.");
                    return;
                }

                _automationCondition.CompareString = textCompareString.Text;
                _automationCondition.Comparison = (AutoCondComparison) listComparison.SelectedIndex;
                break;
            }
        }

        _automationCondition.CompareField = autoCondField;

        if (_automationCondition.AutomationConditionNum == 0)
        {
            AutomationConditions.Insert(_automationCondition);
        }
        else
        {
            AutomationConditions.Update(_automationCondition);
        }

        DialogResult = DialogResult.OK;
    }
}