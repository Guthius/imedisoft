using System;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormOrthoRxEdit : FormODBase
{
    public OrthoRx OrthoRxCur;

    public FormOrthoRxEdit()
    {
        InitializeComponent();
    }

    private void FormOrthoRxEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = OrthoRxCur.Description;

        var orthoHardwareSpecs = OrthoHardwareSpecs.GetDeepCopy();

        comboHardwareSpec.Items.AddNone<OrthoHardwareSpec>();
        comboHardwareSpec.Items.AddList(orthoHardwareSpecs, x => x.Description);
        comboHardwareSpec.SetSelectedKey<OrthoHardwareSpec>(OrthoRxCur.OrthoHardwareSpecNum, x => x.OrthoHardwareSpecNum);

        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        var orthoHardwareSpec = comboHardwareSpec.GetSelected<OrthoHardwareSpec>();
        if (orthoHardwareSpec.OrthoHardwareSpecNum != 0)
        {
            if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
            {
                textToothRange.Text = Tooth.DisplayOrthoCommas(OrthoRxCur.ToothRange, toothNumberingNomenclature);
            }

            if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
            {
                textToothRange.Text = Tooth.DisplayOrthoDash(OrthoRxCur.ToothRange, toothNumberingNomenclature);
            }
        }

        SetTeethInstructions();
    }

    private void SetTeethInstructions()
    {
        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        var orthoHardwareSpec = comboHardwareSpec.GetSelected<OrthoHardwareSpec>();
        if (orthoHardwareSpec.OrthoHardwareSpecNum == 0)
        {
            labelTeeth.Text = "Teeth";
            labelComments.Text = "";
        }
        else if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
        {
            labelTeeth.Text = "Teeth";
            labelComments.Text = "Example: " + Tooth.Display("6", toothNumberingNomenclature) + "," + Tooth.Display("27", toothNumberingNomenclature) + "," + Tooth.Display("28", toothNumberingNomenclature);
        }
        else if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
        {
            labelTeeth.Text = "Tooth Range";
            labelComments.Text = "Example: " + Tooth.Display("3", toothNumberingNomenclature) + "-" + Tooth.Display("14", toothNumberingNomenclature);
        }
    }

    private void ComboBoxHardwareSpec_SelectionChangeCommitted(object sender, EventArgs e)
    {
        SetTeethInstructions();
    }

    private void ButtonUpper_Click(object sender, EventArgs e)
    {
        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        var orthoHardwareSpec = comboHardwareSpec.GetSelected<OrthoHardwareSpec>();
        if (orthoHardwareSpec.OrthoHardwareSpecNum == 0)
        {
            ShowError("Please select a hardware spec first.");
            return;
        }

        if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
        {
            textToothRange.Text = Tooth.DisplayOrthoCommas("2,3,4,5,6,7,8,9,10,11,12,13,14,15", toothNumberingNomenclature);
        }
        else if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
        {
            textToothRange.Text = Tooth.DisplayOrthoDash("2-15", toothNumberingNomenclature);
        }
    }

    private void ButtonLower_Click(object sender, EventArgs e)
    {
        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        var orthoHardwareSpec = comboHardwareSpec.GetSelected<OrthoHardwareSpec>();
        if (orthoHardwareSpec.OrthoHardwareSpecNum == 0)
        {
            ShowError("Please select a hardware spec first.");
            return;
        }

        if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
        {
            textToothRange.Text = Tooth.DisplayOrthoCommas("18,19,20,21,22,23,24,25,26,27,28,29,30,31", toothNumberingNomenclature);
        }
        else if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
        {
            textToothRange.Text = Tooth.DisplayOrthoDash("18-31", toothNumberingNomenclature);
        }
    }

    private void ButtonAll_Click(object sender, EventArgs e)
    {
        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        var orthoHardwareSpec = comboHardwareSpec.GetSelected<OrthoHardwareSpec>();
        if (orthoHardwareSpec.OrthoHardwareSpecNum == 0)
        {
            ShowError("Please select a hardware spec first.");
            return;
        }

        if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
        {
            textToothRange.Text = Tooth.DisplayOrthoCommas("2,3,4,5,6,7,8,9,10,11,12,13,14,15,18,19,20,21,22,23,24,25,26,27,28,29,30,31", toothNumberingNomenclature);
        }
        else if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
        {
            ShowError("All doesn't work with wires.");
        }
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (OrthoRxCur.IsNew)
        {
            DialogResult = DialogResult.Cancel;

            return;
        }

        try
        {
            OrthoRxs.Delete(OrthoRxCur.OrthoRxNum);
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
            ShowError("Please enter a description first.");
            return;
        }

        OrthoRxCur.Description = textDescription.Text;

        var orthoHardwareSpec = comboHardwareSpec.GetSelected<OrthoHardwareSpec>();
        if (orthoHardwareSpec.OrthoHardwareSpecNum == 0)
        {
            ShowError("Please select a hardware spec first.");
            return;
        }

        OrthoRxCur.OrthoHardwareSpecNum = orthoHardwareSpec.OrthoHardwareSpecNum;

        var toothNumberingNomenclature = (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers);
        if (toothNumberingNomenclature == ToothNumberingNomenclature.Universal)
        {
            toothNumberingNomenclature = ToothNumberingNomenclature.Palmer;
        }

        if (orthoHardwareSpec.OrthoHardwareType.In(EnumOrthoHardwareType.Bracket, EnumOrthoHardwareType.Elastic))
        {
            try
            {
                OrthoRxCur.ToothRange = Tooth.ParseOrthoCommas(textToothRange.Text, toothNumberingNomenclature);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }
        }

        if (orthoHardwareSpec.OrthoHardwareType == EnumOrthoHardwareType.Wire)
        {
            try
            {
                OrthoRxCur.ToothRange = Tooth.ParseOrthoDash(textToothRange.Text, toothNumberingNomenclature);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }
        }

        if (OrthoRxCur.IsNew)
        {
            OrthoRxs.Insert(OrthoRxCur);
        }
        else
        {
            OrthoRxs.Update(OrthoRxCur);
        }

        DialogResult = DialogResult.OK;
    }
}