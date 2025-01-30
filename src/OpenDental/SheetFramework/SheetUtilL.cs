using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;
using ComboBox = OpenDental.UI.ComboBox;

namespace OpenDental;

internal class SheetUtilL
{
    public static void FillComboGrowthBehavior(ComboBox combo, GrowthBehaviorEnum growthBehaviorSelected, bool isDynamicSheetType = false, bool gridCombo = false)
    {
        var growthOptions = new List<GrowthBehaviorEnum>();

        foreach (var growthBehaviorEnum in Enum.GetValues(typeof(GrowthBehaviorEnum)).OfType<GrowthBehaviorEnum>().ToList())
        {
            var sheetGrowthAttribute = EnumTools.GetAttributeOrDefault<SheetGrowthAttribute>(growthBehaviorEnum);
            if (growthBehaviorEnum != GrowthBehaviorEnum.None && isDynamicSheetType != sheetGrowthAttribute.IsDynamic)
            {
                continue;
            }

            if (!gridCombo && sheetGrowthAttribute.IsGridOnly)
            {
                continue;
            }

            growthOptions.Add(growthBehaviorEnum);
        }

        combo.Items.AddList(growthOptions, growthOption => growthOption.ToString());

        for (var i = 0; i < combo.Items.Count; i++)
        {
            if ((GrowthBehaviorEnum) combo.Items.GetObjectAt(i) == growthBehaviorSelected)
            {
                combo.SetSelected(i);
            }
        }
    }

    public static void ShowSheet(Sheet sheet, Patient pat, FormClosingEventHandler onFormClosing)
    {
        if (sheet == null)
        {
            MsgBox.Show("Sheets", "Error opening sheet.");
            return;
        }

        if (sheet.DocNum != 0)
        {
            var sheetDoc = Documents.GetByNum(sheet.DocNum, true);
            if (sheetDoc == null)
            {
                MsgBox.Show("Sheets", "Saved sheet no longer exists.");
                return;
            }

            var patFolder = ImageStore.GetPatientFolder(pat, ImageStore.GetDataFolder());

            FileAtoZ.OpenFile(ImageStore.GetFilePath(sheetDoc, patFolder));
        }
        else
        {
            FormSheetFillEdit.ShowForm(sheet, onFormClosing);
        }
    }

    public static void SetApptProcParamsForSheet(Sheet sheet, SheetDef sheetDef, long patNum)
    {
        if (!SheetDefs.ContainsStaticFields(sheetDef, EnumStaticTextField.apptDateMonthSpelled, EnumStaticTextField.apptProcs, EnumStaticTextField.apptProvNameFormal))
        {
            return;
        }

        var appointmentArray = Appointments.GetForPat(patNum);
        long aptNum;
        
        switch (appointmentArray.Length)
        {
            case 0:
                aptNum = 0;
                break;
            
            case 1:
                aptNum = appointmentArray[0].AptNum;
                break;
            
            default:
            {
                using var formApptsOther = new FormApptsOther(patNum, null);

                formApptsOther.AllowSelectOnly = true;

                if (formApptsOther.ShowDialog() == DialogResult.OK)
                {
                    aptNum = formApptsOther.ListAptNumsSelected[0];
                }
                else
                {
                    return;
                }

                break;
            }
        }

        if (SheetDefs.ContainsStaticFields(sheetDef, EnumStaticTextField.apptDateMonthSpelled, EnumStaticTextField.apptProcs, EnumStaticTextField.apptProvNameFormal))
        {
            SheetParameter.SetParameter(sheet, "AptNum", aptNum);
        }
    }
}