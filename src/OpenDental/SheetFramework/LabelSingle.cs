using System;
using System.Collections.Generic;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public class LabelSingle
{
    public static void PrintPat(long patNum)
    {
        var sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelPatientMail);
        if (PrefC.GetLong(PrefName.LabelPatientDefaultSheetDefNum) != 0)
        {
            try
            {
                sheetDef = SheetDefs.GetSheetDef(PrefC.GetLong(PrefName.LabelPatientDefaultSheetDefNum));
            }
            catch
            {
                // The default label could not be retrieved so just use the internal sheet.
            }
        }

        var sheet = SheetUtil.CreateSheet(sheetDef);
        
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintCustomPatient(long patNum, SheetDef sheetDef)
    {
        SheetDefs.GetFieldsAndParameters(sheetDef);
        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintPatientLFAddress(long patNum)
    {
        var sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelPatientLFAddress);
        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintPatientLFChartNumber(long patNum)
    {
        var sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelPatientLFChartNumber);
        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintPatientLFPatNum(long patNum)
    {
        var sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelPatientLFPatNum);
        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintPatRadiograph(long patNum)
    {
        var sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelPatientRadiograph);
        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintText(long patNum, string text)
    {
        var sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelText);
        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        sheet.Parameters.Add(new SheetParameter(false, "text"));
        SheetParameter.SetParameter(sheet, "text", text);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }
    
    public static void PrintCarriers(List<long> carrierNums)
    {
        SheetDef sheetDef;
        var customSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.LabelCarrier);
        if (customSheetDefs.Count == 0)
        {
            sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelCarrier);
        }
        else
        {
            sheetDef = customSheetDefs[0];
            SheetDefs.GetFieldsAndParameters(sheetDef);
        }

        var sheetBatch = SheetUtil.CreateBatch(sheetDef, carrierNums);
        try
        {
            SheetPrinting.PrintBatch(sheetBatch);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintCarrier(long carrierNum)
    {
        SheetDef sheetDef;
        var customSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.LabelCarrier);
        if (customSheetDefs.Count == 0)
        {
            sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelCarrier);
        }
        else
        {
            sheetDef = customSheetDefs[0];
            SheetDefs.GetFieldsAndParameters(sheetDef);
        }

        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "CarrierNum", carrierNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }
    
    public static void PrintReferral(long referralNum)
    {
        SheetDef sheetDef;
        var customSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.LabelReferral);
        if (customSheetDefs.Count == 0)
        {
            sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelReferral);
        }
        else
        {
            sheetDef = customSheetDefs[0];
            SheetDefs.GetFieldsAndParameters(sheetDef);
        }

        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "ReferralNum", referralNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }

    public static void PrintAppointment(long aptNum)
    {
        SheetDef sheetDef;
        var customSheetDefs = SheetDefs.GetCustomForType(SheetTypeEnum.LabelAppointment);
        if (customSheetDefs.Count == 0)
        {
            sheetDef = SheetsInternal.GetSheetDef(SheetInternalType.LabelAppointment);
        }
        else
        {
            sheetDef = customSheetDefs[0];
            SheetDefs.GetFieldsAndParameters(sheetDef);
        }

        var sheet = SheetUtil.CreateSheet(sheetDef);
        SheetParameter.SetParameter(sheet, "AptNum", aptNum);
        SheetFiller.FillFields(sheet);
        try
        {
            SheetPrinting.Print(sheet);
        }
        catch (Exception ex)
        {
            ODMessageBox.Show(ex.Message);
        }
    }
}