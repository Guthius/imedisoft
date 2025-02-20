using System;
using System.Globalization;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormOrthoPat : FormODBase
{
    private readonly PatPlan _patPlan;
    private readonly InsPlan _insPlan;

    public FormOrthoPat(PatPlan patPlan, InsPlan insPlan, string carrierName, string subId, double defaultFee)
    {
        InitializeComponent();

        _patPlan = patPlan;
        _insPlan = insPlan;

        var patient = Patients.GetLim(patPlan.PatNum);

        textPatient.Text = patient.GetNameLF();
        textCarrier.Text = carrierName;
        textSubID.Text = subId;

        if (patPlan.OrthoAutoFeeBilledOverride == -1)
        {
            checkUseDefaultFee.Checked = true;
            textFee.ReadOnly = true;
            textFee.Text = defaultFee.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            checkUseDefaultFee.Checked = false;
            textFee.ReadOnly = false;
            textFee.Text = patPlan.OrthoAutoFeeBilledOverride.ToString(CultureInfo.InvariantCulture);
        }

        textDateNextClaim.Text = patPlan.OrthoAutoNextClaimDate.Date != DateTime.MinValue.Date ? patPlan.OrthoAutoNextClaimDate.ToShortDateString() : "";
    }

    private void CheckBoxUseDefaultFee_CheckedChanged(object sender, EventArgs e)
    {
        textFee.ReadOnly = checkUseDefaultFee.Checked;
        if (checkUseDefaultFee.Checked)
        {
            textFee.Text = _insPlan.OrthoAutoFeeBilled.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void TextBoxDateNextClaim_Validated(object sender, EventArgs e)
    {
        if (!DateTime.TryParse(textDateNextClaim.Text, out var nextClaim))
        {
            return;
        }
        
        if (nextClaim.Day == 1)
        {
            return;
        }

        var firstOfMonth = new DateTime(nextClaim.Year, nextClaim.Month, 1);

        textDateNextClaim.Text = firstOfMonth.ToShortDateString();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!decimal.TryParse(textFee.Text, out var fee))
        {
            ShowError("Please enter a valid fee.");
            return;
        }

        var nextClaim = DateTime.MinValue;
        if (textDateNextClaim.Text != "" && !DateTime.TryParse(textDateNextClaim.Text, out nextClaim))
        {
            ShowError("Please enter a valid date.");
            return;
        }

        if (textDateNextClaim.Text != "")
        {
            var orthoCodeNums = ProcedureCodes.GetOrthoBandingCodeNums();
            var orthoProcedures = Procedures.GetProcsByStatusForPat(_patPlan.PatNum, ProcStat.C).FindAll(x => orthoCodeNums.Contains(x.CodeNum));

            if (orthoProcedures.Count == 0)
            {
                ShowError("Cannot enter Next Claim Date until at least one Ortho Proc in Ortho Placement Procedures is complete. See Ortho Setup.");
                return;
            }
        }

        if (checkUseDefaultFee.Checked)
        {
            _patPlan.OrthoAutoFeeBilledOverride = -1;
        }
        else
        {
            _patPlan.OrthoAutoFeeBilledOverride = (double) fee;
        }

        if (textDateNextClaim.Visible)
        {
            _patPlan.OrthoAutoNextClaimDate = nextClaim;
        }

        DialogResult = DialogResult.OK;
    }
}