using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutomationEdit : FormODBase
{
    private readonly Automation _automation;
    private List<AutomationCondition> _automationConditions;
    private List<AutomationAction> _automationActions;
    private List<AppointmentType> _appointmentTypes;
    private List<Def> _commLogTypesDefs;

    public FormAutomationEdit(Automation automation)
    {
        _automation = automation.Copy();

        InitializeComponent();
    }

    private void FormAutomationEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _automation.Description;

        _commLogTypesDefs = Defs.GetDefsForCategory(DefCat.CommLogTypes, true);

        _appointmentTypes = [new AppointmentType {AppointmentTypeName = "none"}];
        _appointmentTypes.AddRange(AppointmentTypes.GetWhere(x => !x.IsHidden || x.AppointmentTypeNum == _automation.AppointmentTypeNum));
        _appointmentTypes = _appointmentTypes
            .OrderBy(x => x.AppointmentTypeNum > 0)
            .ThenBy(x => x.ItemOrder)
            .ToList();

        Enum.GetNames(typeof(EnumAutomationTrigger)).ToList().ForEach(x => comboTrigger.Items.Add(x));

        comboTrigger.SelectedIndex = (int) _automation.Autotrigger;

        textProcCodes.Text = _automation.ProcCodes;
        textMessage.Text = _automation.MessageContent;

        FillGrid();
    }

    private void FormAutomationEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            return;
        }

        if (_automation.AutomationNum > 0)
        {
            return;
        }

        AutomationConditions.DeleteByAutomationNum(_automation.AutomationNum);
        Automations.Delete(_automation);
    }

    private void FillGrid()
    {
        AutomationConditions.RefreshCache();

        _automationConditions = AutomationConditions.GetListByAutomationNum(_automation.AutomationNum);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Field", 200));
        gridMain.Columns.Add(new GridColumn("Comparison", 75));
        gridMain.Columns.Add(new GridColumn("Text", 100));

        gridMain.ListGridRows.Clear();

        _automationConditions.ForEach(x => gridMain.ListGridRows.Add(new GridRow(x.CompareField.ToString(), x.Comparison.ToString(), x.CompareString)));

        gridMain.EndUpdate();
    }

    private void ComboBoxTrigger_SelectedIndexChanged(object sender, EventArgs e)
    {
        comboAction.Items.Clear();

        _automationActions = Enum.GetValues(typeof(AutomationAction)).OfType<AutomationAction>().ToList();

        var automationTrigger = (EnumAutomationTrigger) comboTrigger.SelectedIndex;
        if (automationTrigger is not (EnumAutomationTrigger.ApptCreate or EnumAutomationTrigger.ApptNewPatCreate))
        {
            _automationActions.Remove(AutomationAction.SetApptASAP);
            _automationActions.Remove(AutomationAction.SetApptType);
        }

        if (automationTrigger is not EnumAutomationTrigger.RxCreate)
        {
            _automationActions.Remove(AutomationAction.PrintRxInstruction);
        }

        _automationActions.ForEach(x => comboAction.Items.Add(x.GetDescription()));

        comboAction.SelectedIndex = _automation.Autotrigger == automationTrigger ? _automationActions.IndexOf(_automation.AutoAction) : 0;

        if (automationTrigger is EnumAutomationTrigger.ProcedureComplete or EnumAutomationTrigger.ProcSchedule)
        {
            labelProcCodes.Visible = true;
            textProcCodes.Visible = true;
            butProcCode.Visible = true;
        }
        else
        {
            labelProcCodes.Visible = false;
            textProcCodes.Visible = false;
            butProcCode.Visible = false;
        }
    }

    private void ComboBoxAction_SelectedIndexChanged(object sender, EventArgs e)
    {
        labelActionObject.Text = "Action Object";
        labelActionObject.Visible = false;
        comboActionObject.Visible = false;
        labelMessage.Visible = false;
        textMessage.Visible = false;

        if (comboAction.SelectedIndex < 0 || comboAction.SelectedIndex >= _automationActions.Count)
        {
            return;
        }

        comboActionObject.Items.Clear();
        switch (_automationActions[comboAction.SelectedIndex])
        {
            case AutomationAction.CreateCommlog:
                labelActionObject.Visible = true;
                labelActionObject.Text = "Commlog Type";
                comboActionObject.Visible = true;
                _commLogTypesDefs.ForEach(x => comboActionObject.Items.Add(x.ItemName));
                comboActionObject.SelectedIndex = _commLogTypesDefs.FindIndex(x => x.DefNum == _automation.CommType);
                labelMessage.Visible = true;
                textMessage.Visible = true;
                return;

            case AutomationAction.PopUp:
            case AutomationAction.PopUpThenDisable10Min:
                labelMessage.Visible = true;
                textMessage.Visible = true;
                return;

            case AutomationAction.SetApptASAP:
                return;

            case AutomationAction.SetApptType:
                labelActionObject.Visible = true;
                labelActionObject.Text = "Appointment Type";
                comboActionObject.Visible = true;
                _appointmentTypes.ForEach(x => comboActionObject.Items.Add(x.AppointmentTypeName));
                comboActionObject.SelectedIndex = _appointmentTypes.FindIndex(x => _automation.AppointmentTypeNum == x.AppointmentTypeNum);
                return;

            case AutomationAction.PrintPatientLetter:
            case AutomationAction.PrintReferralLetter:
            case AutomationAction.ShowConsentForm:
            case AutomationAction.ShowExamSheet:
            case AutomationAction.PrintRxInstruction:
                labelActionObject.Visible = true;
                labelActionObject.Text = "Sheet Definition";
                comboActionObject.Visible = true;

                var sheetDefs = SheetDefs.GetDeepCopy().FindAll(x => !SheetDefs.IsDashboardType(x));
                foreach (var sheetDef in sheetDefs)
                {
                    switch (sheetDef.SheetType)
                    {
                        case SheetTypeEnum.PatientLetter when _automationActions[comboAction.SelectedIndex] == AutomationAction.PrintPatientLetter:
                        case SheetTypeEnum.ReferralLetter when _automationActions[comboAction.SelectedIndex] == AutomationAction.PrintReferralLetter:
                        case SheetTypeEnum.Consent when _automationActions[comboAction.SelectedIndex] == AutomationAction.ShowConsentForm:
                        case SheetTypeEnum.ExamSheet when _automationActions[comboAction.SelectedIndex] == AutomationAction.ShowExamSheet:
                        case SheetTypeEnum.RxInstruction when _automationActions[comboAction.SelectedIndex] == AutomationAction.PrintRxInstruction:
                            comboActionObject.Items.Add(sheetDef.Description, sheetDef.SheetDefNum);
                            break;
                    }
                }

                comboActionObject.SetSelectedKey<long>(_automation.SheetDefNum, x => x);
                return;

            case AutomationAction.ChangePatStatus:
                labelActionObject.Visible = true;
                labelActionObject.Text = "Patient Status";
                comboActionObject.Visible = true;

                var patientStatuses = Enum.GetValues(typeof(PatientStatus)).Cast<PatientStatus>().ToList();

                foreach (var patientStatus in patientStatuses)
                {
                    if (patientStatus == PatientStatus.Deleted)
                    {
                        continue;
                    }

                    comboActionObject.Items.Add(patientStatus.GetDescription(), patientStatus);
                }

                comboActionObject.SetSelectedEnum(_automation.PatStatus);
                return;
        }
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formAutmationConditionEdit = new FormAutomationConditionEdit(_automationConditions[e.Row]);

        formAutmationConditionEdit.ShowDialog();

        FillGrid();
    }

    private void ButtonProcCode_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes();

        formProcCodes.IsSelectionMode = true;

        if (formProcCodes.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        textProcCodes.Text = string.Join(",", new[] {textProcCodes.Text, ProcedureCodes.GetStringProcCode(formProcCodes.CodeNumSelected)}.Where(x => !string.IsNullOrEmpty(x)));
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var automationCondition = new AutomationCondition
        {
            AutomationNum = _automation.AutomationNum
        };

        using var formAutomationConditionEdit = new FormAutomationConditionEdit(automationCondition);

        if (formAutomationConditionEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrid();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_automation.AutomationNum == 0)
        {
            DialogResult = DialogResult.Cancel;
        }
        else
        {
            AutomationConditions.DeleteByAutomationNum(_automation.AutomationNum);
            Automations.Delete(_automation);

            DialogResult = DialogResult.OK;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text == "")
        {
            ShowError("Description not allowed to be blank.");
            return;
        }

        if (comboAction.SelectedIndex == -1)
        {
            ShowError("Action not allowed to be blank.");
            return;
        }

        _automation.Description = textDescription.Text;
        _automation.Autotrigger = (EnumAutomationTrigger) comboTrigger.SelectedIndex;
        _automation.ProcCodes = "";

        if (_automation.Autotrigger is EnumAutomationTrigger.ProcedureComplete or EnumAutomationTrigger.ProcSchedule)
        {
            if (textProcCodes.Text.Contains(" "))
            {
                ShowError("Procedure codes cannot contain any spaces.");
                return;
            }

            if (textProcCodes.Text == "")
            {
                ShowError("Please enter valid procedure code(s) first.");
                return;
            }

            var invalidCodes = string.Join(", ", textProcCodes.Text.Split(',').Where(x => !ProcedureCodes.IsValidCode(x)));
            if (!string.IsNullOrEmpty(invalidCodes))
            {
                ShowError("The following procedure code(s) are not valid: " + invalidCodes);
                return;
            }

            _automation.ProcCodes = textProcCodes.Text;
        }

        _automation.AutoAction = _automationActions[comboAction.SelectedIndex];
        _automation.SheetDefNum = 0;
        _automation.CommType = 0;
        _automation.MessageContent = "";
        _automation.AptStatus = ApptStatus.None;
        _automation.AppointmentTypeNum = 0;

        switch (_automation.AutoAction)
        {
            case AutomationAction.CreateCommlog:
                if (comboActionObject.SelectedIndex == -1)
                {
                    ShowError("A commlog type must be selected.");
                    return;
                }

                _automation.CommType = _commLogTypesDefs[comboActionObject.SelectedIndex].DefNum;
                _automation.MessageContent = textMessage.Text;
                break;

            case AutomationAction.PopUp:
            case AutomationAction.PopUpThenDisable10Min:
                if (string.IsNullOrEmpty(textMessage.Text.Trim()))
                {
                    ShowError("The message cannot be blank.");
                    return;
                }

                _automation.MessageContent = textMessage.Text;
                break;

            case AutomationAction.PrintPatientLetter:
            case AutomationAction.PrintReferralLetter:
            case AutomationAction.ShowExamSheet:
            case AutomationAction.ShowConsentForm:
            case AutomationAction.PrintRxInstruction:
                if (comboActionObject.SelectedIndex == -1)
                {
                    ShowError("A sheet definition must be selected.");
                    return;
                }

                _automation.SheetDefNum = comboActionObject.GetSelected<long>();
                break;

            case AutomationAction.SetApptASAP:
                break;

            case AutomationAction.SetApptType:
                if (comboActionObject.SelectedIndex == -1)
                {
                    ShowError("An appointment type must be selected.");
                    return;
                }

                _automation.AppointmentTypeNum = _appointmentTypes[comboActionObject.SelectedIndex].AppointmentTypeNum;
                break;

            case AutomationAction.ChangePatStatus:
                if (comboAction.SelectedIndex == -1)
                {
                    ShowError("A patient status must be selected.");
                    return;
                }

                _automation.PatStatus = comboActionObject.GetSelected<PatientStatus>();
                break;
        }

        Automations.Update(_automation);

        DialogResult = DialogResult.OK;
    }
}