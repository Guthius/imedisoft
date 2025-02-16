using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormFeeEdit : FormODBase
{
    public Fee FeeCur;

    public bool IsNew { get; set; }

    public FormFeeEdit()
    {
        InitializeComponent();
    }

    private void FormFeeEdit_Load(object sender, EventArgs e)
    {
        var feeSched = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == FeeCur.FeeSched);
        if (!FeeL.CanEditFee(feeSched, FeeCur.ProvNum, FeeCur.ClinicNum))
        {
            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        Location = new Point(Location.X - 190, Location.Y - 20);

        textFee.Text = FeeCur.Amount.ToString("F");

        odDatePickerEffectiveDate.SetDateTime(FeeCur.DateEffective);
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textFee.IsValid())
        {
            ShowError("Please fix data entry error first.");
            return;
        }

        if (textFee.Text != "" && Fees.CheckForDuplicate(FeeCur, odDatePickerEffectiveDate.GetDateTime()))
        {
            ShowError("There is already a Fee with that Effective Date. Please enter another date.");
            return;
        }

        var datePrevious = FeeCur.SecDateTEdit;
        if (textFee.Text == "")
        {
            Fees.Delete(FeeCur);
        }
        else if (CompareDouble.IsEqual(FeeCur.Amount, SIn.Double(textFee.Text)) && DateTime.Equals(FeeCur.DateEffective, odDatePickerEffectiveDate.GetDateTime()))
        {
            DialogResult = DialogResult.OK;

            return;
        }
        else
        {
            var feeOld = FeeCur.Copy();

            FeeCur.Amount = SIn.Double(textFee.Text);
            FeeCur.DateEffective = SIn.Date(odDatePickerEffectiveDate.GetDateTime().ToShortDateString());

            Fees.Update(FeeCur, feeOld);
        }

        SecurityLogs.MakeLogEntry(EnumPermType.ProcFeeEdit, 0,
            "Procedure: " + ProcedureCodes.GetStringProcCode(FeeCur.CodeNum) + ", " +
            "Fee: " + FeeCur.Amount.ToString("c") + ", " +
            "Fee Schedule: " + FeeScheds.GetDescription(FeeCur.FeeSched) + ". " +
            "Manual edit in Edit Fee window.", FeeCur.CodeNum, DateTime.MinValue);

        SecurityLogs.MakeLogEntry(EnumPermType.LogFeeEdit, 0, "Fee Updated", FeeCur.FeeNum, datePrevious);

        DialogResult = DialogResult.OK;
    }

    private void FormFeeEdit_Closing(object sender, CancelEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            return;
        }

        if (IsNew)
        {
            Fees.Delete(FeeCur);
        }
    }
}