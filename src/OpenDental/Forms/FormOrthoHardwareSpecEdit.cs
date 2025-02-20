using System;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormOrthoHardwareSpecEdit : FormODBase
{
    public OrthoHardwareSpec OrthoHardwareSpecCur;

    public FormOrthoHardwareSpecEdit()
    {
        InitializeComponent();
    }

    private void FormOrthoHardwareSpecEdit_Load(object sender, EventArgs e)
    {
        textType.Text = OrthoHardwareSpecCur.OrthoHardwareType.ToString();
        textDescription.Text = OrthoHardwareSpecCur.Description;

        butColor.BackColor = OrthoHardwareSpecCur.ItemColor;

        checkHidden.Checked = OrthoHardwareSpecCur.IsHidden;
    }

    private void ButtonColor_Click(object sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog();

        colorDialog.Color = butColor.BackColor;

        if (colorDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        butColor.BackColor = colorDialog.Color;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (OrthoHardwareSpecCur.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        try
        {
            OrthoHardwareSpecs.Delete(OrthoHardwareSpecCur.OrthoHardwareSpecNum);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text == "")
        {
            ShowError("Please enter a description.");
            return;
        }

        OrthoHardwareSpecCur.Description = textDescription.Text;
        OrthoHardwareSpecCur.ItemColor = butColor.BackColor;
        OrthoHardwareSpecCur.IsHidden = checkHidden.Checked;

        if (OrthoHardwareSpecCur.IsNew)
        {
            OrthoHardwareSpecs.Insert(OrthoHardwareSpecCur);
        }
        else
        {
            OrthoHardwareSpecs.Update(OrthoHardwareSpecCur);
        }

        DialogResult = DialogResult.OK;
    }
}