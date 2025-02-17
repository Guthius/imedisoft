using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenDental;

public partial class FormImageDrawColor : FormODBase
{
    public Color ColorBack { get; set; }
    public Color ColorFore { get; set; }
    public Color ColorTextBack { get; set; }
    public bool IsMount { get; set; }

    public FormImageDrawColor()
    {
        InitializeComponent();
    }

    private void FormImageDrawEdit_Load(object sender, EventArgs e)
    {
        if (!IsMount)
        {
            labelMount.Visible = false;
        }

        butColorFore.BackColor = ColorFore;
        butColorTextBack.BackColor = ColorTextBack;

        if (ColorTextBack.ToArgb() != Color.Transparent.ToArgb())
        {
            return;
        }

        checkTransparent.Checked = true;
        butColorTextBack.BackColor = ColorBack;
    }

    private void ButtonColorFore_Click(object sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog();

        colorDialog.Color = butColorFore.BackColor;
        colorDialog.ShowDialog();

        butColorFore.BackColor = colorDialog.Color;
    }

    private void ButtonColorTextBack_Click(object sender, EventArgs e)
    {
        using var colorDialog = new ColorDialog();

        colorDialog.Color = butColorTextBack.BackColor;

        if (colorDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        checkTransparent.Checked = false;

        butColorTextBack.BackColor = colorDialog.Color;
    }

    private void CheckBoxTransparent_Click(object sender, EventArgs e)
    {
        butColorTextBack.BackColor = ColorBack;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        ColorFore = butColorFore.BackColor;
        ColorTextBack = checkTransparent.Checked ? Color.Transparent : butColorTextBack.BackColor;

        DialogResult = DialogResult.OK;
    }
}