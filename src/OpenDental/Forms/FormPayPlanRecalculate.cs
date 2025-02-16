using System;
using System.Windows.Forms;

namespace OpenDental.Forms;

public partial class FormPayPlanRecalculate : FormODBase
{
    public bool IsPrepay { get; set; } = true;
    public bool IsRecalculateInterest { get; set; } = true;

    public FormPayPlanRecalculate()
    {
        InitializeComponent();
    }

    private void FormPayPlanRecalculate_Load(object sender, EventArgs e)
    {
        radioPrepay.Checked = IsPrepay;
        checkRecalculateInterest.Checked = IsRecalculateInterest;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        IsPrepay = radioPrepay.Checked;
        IsRecalculateInterest = checkRecalculateInterest.Checked;
        DialogResult = DialogResult.OK;
    }
}