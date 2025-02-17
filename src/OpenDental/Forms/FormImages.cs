using System;
using System.Windows.Forms;

namespace OpenDental.Forms;

public partial class FormImages : FormODBase
{
    public long ClaimPaymentNum { get; set; }

    public FormImages()
    {
        InitializeComponent();
    }

    private void FormImages_Shown(object sender, EventArgs e)
    {
        if (ClaimPaymentNum != 0)
        {
            contrImagesMain.ModuleSelectedClaimPayment(ClaimPaymentNum);
        }

        contrImagesMain.CloseClick += (_, _) => DialogResult = DialogResult.OK;
    }
}