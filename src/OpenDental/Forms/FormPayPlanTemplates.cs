using System;
using System.Globalization;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormPayPlanTemplates : FormODBase
{
    private readonly bool _isUserClinicRestricted;

    public bool IsSelectionMode { get; set; }
    public PayPlanTemplate SelectedPayPlanTemplate { get; set; }

    public FormPayPlanTemplates()
    {
        InitializeComponent();

        _isUserClinicRestricted = UserClinics.GetForUser(Security.CurUser.UserNum).Count > 0;
    }

    private void FormPayPlanTemplates_Load(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            butOK.Visible = false;
            butAddTemplate.Visible = true;
            checkShowHidden.Visible = true;
        }

        comboBoxClinic.IsAllSelected = true;
        if (_isUserClinicRestricted)
        {
            comboBoxClinic.IncludeAll = false;
        }

        FillTemplates();
    }

    private void FillTemplates()
    {
        gridPayPlanTemplates.BeginUpdate();

        gridPayPlanTemplates.Columns.Clear();
        gridPayPlanTemplates.Columns.Add(new GridColumn("Name", 160, HorizontalAlignment.Left));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Clinic", 80, HorizontalAlignment.Center));
        gridPayPlanTemplates.Columns.Add(new GridColumn("APR", 64, HorizontalAlignment.Center));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Interest\nDelay", 105, HorizontalAlignment.Center));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Payment Amount", 110, HorizontalAlignment.Right));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Number of\nPayments", 70, HorizontalAlignment.Center));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Down Payment", 100, HorizontalAlignment.Right));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Frequency", 100, HorizontalAlignment.Center));
        gridPayPlanTemplates.Columns.Add(new GridColumn("Treatment Plan\nOption", 80, HorizontalAlignment.Center));

        gridPayPlanTemplates.ListGridRows.Clear();

        var payPlanTemplates = !comboBoxClinic.IsAllSelected ? PayPlanTemplates.GetMany(comboBoxClinic.ClinicNumSelected) : PayPlanTemplates.GetAll();

        foreach (var payPlanTemplate in payPlanTemplates)
        {
            if (payPlanTemplate.IsHidden && !checkShowHidden.Checked)
            {
                continue;
            }

            var clinicAbbr = Clinics.GetAbbr(payPlanTemplate.ClinicNum);

            var gridRow = new GridRow();

            gridRow.Cells.Add(payPlanTemplate.PayPlanTemplateName);
            gridRow.Cells.Add(clinicAbbr);
            gridRow.Cells.Add(payPlanTemplate.APR.ToString(CultureInfo.InvariantCulture));
            gridRow.Cells.Add(payPlanTemplate.InterestDelay.ToString());

            if (payPlanTemplate.PayAmt == 0)
            {
                gridRow.Cells.Add("");
                gridRow.Cells.Add(payPlanTemplate.NumberOfPayments.ToString());
            }
            else
            {
                gridRow.Cells.Add(payPlanTemplate.PayAmt.ToString("f"));
                gridRow.Cells.Add("");
            }

            gridRow.Cells.Add(payPlanTemplate.DownPayment.ToString("f"));
            gridRow.Cells.Add(payPlanTemplate.ChargeFrequency.ToString());
            gridRow.Cells.Add(payPlanTemplate.DynamicPayPlanTPOption.ToString());
            gridRow.Tag = payPlanTemplate;

            gridPayPlanTemplates.ListGridRows.Add(gridRow);
        }

        gridPayPlanTemplates.EndUpdate();
    }

    private void ComboBoxClinic_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillTemplates();
    }

    private void GridPayPlanTemplates_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedPayPlanTemplate = gridPayPlanTemplates.SelectedTag<PayPlanTemplate>();

        if (IsSelectionMode)
        {
            DialogResult = DialogResult.OK;

            return;
        }

        OpenEditForm(SelectedPayPlanTemplate);
    }

    private void CheckBoxShowHidden_Click(object sender, EventArgs e)
    {
        FillTemplates();
    }

    private void ButtonAddTemplate_Click(object sender, EventArgs e)
    {
        OpenEditForm();
    }

    private void OpenEditForm(PayPlanTemplate payPlanTempate = null)
    {
        using var formPayPlanTemplateEdit = new FormPayPlanTemplateEdit(payPlanTempate);

        formPayPlanTemplateEdit.ShowDialog();

        FillTemplates();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SelectedPayPlanTemplate = gridPayPlanTemplates.SelectedTag<PayPlanTemplate>();

        DialogResult = DialogResult.OK;
    }
}