using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormOrthoHardwareEdit : FormODBase
{
    public OrthoHardware OrthoHardwareCur;

    public FormOrthoHardwareEdit()
    {
        InitializeComponent();
    }

    private void FormOrthoHardwareEdit_Load(object sender, EventArgs e)
    {
        textDateExam.Text = OrthoHardwareCur.DateExam.ToShortDateString();
        textType.Text = OrthoHardwareCur.OrthoHardwareType.ToString();

        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        switch (OrthoHardwareCur.OrthoHardwareType)
        {
            case EnumOrthoHardwareType.Bracket:
                labelTeeth.Text = "Tooth";
                labelComments.Text = "Example: " + Tooth.Display("7", toothNumberingNomenclature);
                textToothRange.Text = Tooth.Display(OrthoHardwareCur.ToothRange, toothNumberingNomenclature);
                break;

            case EnumOrthoHardwareType.Elastic:
                labelTeeth.Text = "Teeth";
                labelComments.Text = "Example: " + Tooth.Display("6", toothNumberingNomenclature) + "," + Tooth.Display("27", toothNumberingNomenclature) + "," + Tooth.Display("28", toothNumberingNomenclature);
                textToothRange.Text = Tooth.DisplayOrthoCommas(OrthoHardwareCur.ToothRange, toothNumberingNomenclature);
                break;

            case EnumOrthoHardwareType.Wire:
                labelTeeth.Text = "Tooth Range";
                labelComments.Text = "Example: " + Tooth.Display("3", toothNumberingNomenclature) + "-" + Tooth.Display("14", toothNumberingNomenclature);
                textToothRange.Text = Tooth.DisplayOrthoDash(OrthoHardwareCur.ToothRange, toothNumberingNomenclature);
                break;
        }

        textNote.Text = OrthoHardwareCur.Note;
        checkIsHidden.Checked = OrthoHardwareCur.IsHidden;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (OrthoHardwareCur.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        OrthoHardwares.Delete(OrthoHardwareCur.OrthoHardwareNum);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textToothRange.Text == "")
        {
            ShowError(OrthoHardwareCur.OrthoHardwareType == EnumOrthoHardwareType.Bracket
                ? "Please enter a tooth."
                : "Please enter teeth.");

            return;
        }

        if (!textDateExam.IsValid())
        {
            return;
        }

        OrthoHardwareCur.DateExam = textDateExam.Value;

        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        switch (OrthoHardwareCur.OrthoHardwareType)
        {
            case EnumOrthoHardwareType.Bracket when !Tooth.IsValidEntry(textToothRange.Text, toothNumberingNomenclature):
                ShowError("Invalid tooth number.");
                return;

            case EnumOrthoHardwareType.Bracket:
                OrthoHardwareCur.ToothRange = Tooth.Parse(textToothRange.Text, toothNumberingNomenclature);
                break;

            case EnumOrthoHardwareType.Elastic:
                try
                {
                    OrthoHardwareCur.ToothRange = Tooth.ParseOrthoCommas(textToothRange.Text, toothNumberingNomenclature);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);

                    return;
                }

                break;

            case EnumOrthoHardwareType.Wire:
                try
                {
                    OrthoHardwareCur.ToothRange = Tooth.ParseOrthoDash(textToothRange.Text, toothNumberingNomenclature);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);

                    return;
                }

                break;
        }

        OrthoHardwareCur.Note = textNote.Text;
        OrthoHardwareCur.IsHidden = checkIsHidden.Checked;

        if (OrthoHardwareCur.IsNew)
        {
            OrthoHardwares.Insert(OrthoHardwareCur);
        }
        else
        {
            OrthoHardwares.Update(OrthoHardwareCur);
        }

        DialogResult = DialogResult.OK;
    }
}