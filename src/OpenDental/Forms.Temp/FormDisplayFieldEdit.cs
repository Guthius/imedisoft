using System;
using System.Drawing;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class FormDisplayFieldEdit : FormODBase
{
    private readonly Font _font = new(FontFamily.GenericSansSerif, 8.5f, FontStyle.Bold);

    public DisplayField DisplayFieldCur { get; set; }
    public bool AllowZeroWidth { get; set; }

    public FormDisplayFieldEdit()
    {
        InitializeComponent();
    }

    private void FormDisplayFieldEdit_Load(object sender, EventArgs e)
    {
        textInternalName.Text = DisplayFieldCur.InternalName;
        textDescription.Text = DisplayFieldCur.Description;
        textDescriptionOverride.Text = DisplayFieldCur.DescriptionOverride;
        textWidth.Text = DisplayFieldCur.ColumnWidth.ToString();

        if (DisplayFieldCur.Category == DisplayFieldCategory.SuperFamilyGridCols && DisplayFieldCur.InternalName == "")
        {
            labelInternalName.Visible = false;
            textInternalName.Visible = false;
            labelDescriptionOption.Visible = false;
            textDescription.ReadOnly = true;
        }
        else
        {
            labelDescriptionOverride.Visible = false;
            textDescriptionOverride.Visible = false;
            labelDescriptionOverrideOption.Visible = false;
        }

        FillWidth();
    }

    private void FillWidth()
    {
        using var graphics = CreateGraphics();

        var description = textDescriptionOverride.Text;
        if (description == "")
        {
            description = textDescription.Text;
        }

        if (description == "")
        {
            description = textInternalName.Text;
        }

        var width = (int) graphics.MeasureString(description, _font).Width + 5;

        textWidthMin.Text = width.ToString();
    }

    private void TextBoxDescription_TextChanged(object sender, EventArgs e)
    {
        FillWidth();
    }

    private void TextBoxDescriptionOverride_TextChanged(object sender, EventArgs e)
    {
        FillWidth();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textWidth.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        DisplayFieldCur.Description = textDescription.Text;
        DisplayFieldCur.DescriptionOverride = textDescriptionOverride.Text;
        DisplayFieldCur.ColumnWidth = SIn.Int(textWidth.Text);
        DialogResult = DialogResult.OK;
    }
}