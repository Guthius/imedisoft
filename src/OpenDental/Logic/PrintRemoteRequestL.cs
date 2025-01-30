using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using OpenDentBusiness;
using Statement = Imedisoft.Core.Entities.Statement;

namespace OpenDental.Logic;

public class PrintRemoteRequestL
{
    public static void ProcessCompPrintSignal(List<Signalod> signals)
    {
        if (signals is not {Count: > 0})
        {
            return;
        }

        foreach (var signal in signals)
        {
            if (signal.IType != InvalidType.Print || signal.FKeyType != KeyType.Computer || signal.FKey != Computers.GetCur().ComputerNum)
            {
                continue;
            }
            
            var printRemoteRequest = JsonConvert.DeserializeObject<PrintRemoteRequest>(signal.MsgValue);
            try
            {
                ProcessRemotePrintRequest(printRemoteRequest);
            }
            catch (Exception ex)
            {
                MobileNotifications.InsertPrintError(printRemoteRequest.FKey, EnumAppTarget.ODTouch, ex.Message);
            }
        }
    }

    public static bool ProcessRemotePrintRequest(PrintRemoteRequest printRemoteRequest)
    {
        var wasUserLoggedInAndDifferent = Security.CurUser != null && Security.CurUser.UserNum != printRemoteRequest.RequestUserNum;

        if (printRemoteRequest.RequestUserNum != 0)
        {
            var userod = Userods.GetUserByUserNumNoCache(printRemoteRequest.RequestUserNum);
            
            Security.SetUserCurT(userod);
        }

        switch (printRemoteRequest.PrintRequestType)
        {
            case EnumPrintRequestType.Rx:
                var patNumRx = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[0]);
                var rxPat = JsonConvert.DeserializeObject<RxPat>(printRemoteRequest.ListParameters[1]);
                var isInstructions = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[2]);
                var clinicNum = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[3]);
                TryPrintRx(patNumRx, rxPat, isInstructions, clinicNum, printerNumOverride: printRemoteRequest.PrinterNumOverride);
                break;
            
            case EnumPrintRequestType.PayPlan:
                var payPlan = JsonConvert.DeserializeObject<PayPlan>(printRemoteRequest.ListParameters[0]);
                TryPrintPayPlan(payPlan);
                break;
            
            case EnumPrintRequestType.TreatPlan:
                var patNumTxPlan = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[0]);
                var clinicNumTxPlan = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[1]);
                var treatPlanNum = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[2]);
                var showDiscountNotAuto = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[3]);
                var showDiscount = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[4]);
                var showMaxDed = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[5]);
                var showSubTotals = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[6]);
                var showTotals = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[7]);
                var showCompleted = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[8]);
                var showFees = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[9]);
                var showIns = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[10]);
                
                var treatPlan = TreatPlans.GetOne(treatPlanNum);
                
                treatPlan = TreatPlans.GetTreatPlanListProcTP(treatPlan);
                
                TryPrintTxPlan(treatPlan, patNumTxPlan, clinicNumTxPlan, showDiscountNotAuto, showDiscount, showMaxDed, showSubTotals, showTotals, showCompleted, showFees, showIns, printerNumOverride: printRemoteRequest.PrinterNumOverride);
                break;
            
            case EnumPrintRequestType.Sheet:
                var sheetNum = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[0]);
                var statementNum = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[1]);
                var medLabNum = JsonConvert.DeserializeObject<long>(printRemoteRequest.ListParameters[2]);
                var isStatement = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[3]);
                var isRxControlled = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[4]);
                var isSuperStatement = JsonConvert.DeserializeObject<bool>(printRemoteRequest.ListParameters[5]);
                
                var medLab = MedLabs.GetOne(medLabNum);
                
                DataSet dataSet;
                
                var statement = Statements.GetStatement(statementNum);
                if (statement is null)
                {
                    TryPrintSheet(sheetNum, isStatement, isRxControlled, null, medLab, null);
                    
                    break;
                }

                if (isSuperStatement || statement.LimitedCustomFamily != EnumLimitedCustomFamily.None)
                {
                    dataSet = AccountModules.GetSuperFamAccount(statement, doShowHiddenPaySplits: statement.IsReceipt, doExcludeTxfrs: true);
                }
                else
                {
                    var patNum = Statements.GetPatNumForGetAccount(statement);
                    
                    dataSet = AccountModules.GetAccount(patNum, statement, doShowHiddenPaySplits: statement.IsReceipt, doExcludeTxfrs: true);
                }
                
                TryPrintSheet(sheetNum, isStatement, isRxControlled, statement, medLab, dataSet, printerNumOverride: printRemoteRequest.PrinterNumOverride);
                break;
            
            default:
                throw new ApplicationException("Unsupported remote print request type.");
        }

        if (wasUserLoggedInAndDifferent)
        {
            Cache.ClearCaches();
        }

        return true;
    }

    public static bool TryPrintRx(long patNum, RxPat rxPat, bool isInstructions, long clinicNum, long printerNumOverride = 0)
    {
        var sheetDef = isInstructions 
            ? SheetDefs.GetInternalOrCustom(SheetInternalType.RxInstruction) 
            : SheetDefs.GetSheetsDefault(SheetTypeEnum.Rx, clinicNum);

        var sheet = SheetUtil.CreateSheet(sheetDef, patNum);
        
        SheetParameter.SetParameter(sheet, "RxNum", rxPat.RxNum);
        SheetFiller.FillFields(sheet);
        SheetUtil.CalculateHeights(sheet);
        
        return SheetPrinting.PrintRx(sheet, rxPat, isRemotePrint: true, printerNumOverride: printerNumOverride);
    }

    public static void TryPrintPayPlan(PayPlan payPlan, long printerNumOverride = 0)
    {
        if (PrefC.GetBool(PrefName.PayPlansUseSheets))
        {
            var dynamicPaymentPlanModuleData = PayPlanEdit.GetDynamicPaymentPlanModuleData(payPlan);
            
            var validationErrors = PayPlanEdit.ValidateDynamicPaymentPlanTerms(
                PayPlanEdit.GetPayPlanTerms(dynamicPaymentPlanModuleData.PayPlan, dynamicPaymentPlanModuleData.ListPayPlanLinks),
                isNew: false,
                dynamicPaymentPlanModuleData.PayPlan.IsLocked,
                dynamicPaymentPlanModuleData.PayPlan.APR != 0,
                dynamicPaymentPlanModuleData.ListPayPlanLinks.Count);
            
            if (!string.IsNullOrWhiteSpace(validationErrors))
            {
                throw new ApplicationException(validationErrors);
            }

            var sheet = PayPlanToSheet(payPlan, dynamicPaymentPlanModuleData);
            
            SheetPrinting.Print(sheet, isPrintRemote: true, printerNumOverride: printerNumOverride);
        }
        else
        {
            throw new ApplicationException("Report complex forms not supported for remote printing.");
        }
    }

    public static Sheet PayPlanToSheet(PayPlan payPlan, DynamicPaymentPlanModuleData dynamicPaymentPlanModuleData)
    {
        var sheet = SheetUtil.CreateSheet(SheetDefs.GetInternalOrCustom(SheetInternalType.PaymentPlan), dynamicPaymentPlanModuleData.Patient.PatNum);
        
        sheet.Parameters.Add(new SheetParameter(true, "payplan") {ParamValue = payPlan});
        sheet.Parameters.Add(new SheetParameter(true, "Principal") {ParamValue = dynamicPaymentPlanModuleData.ListPayPlanProductionEntries.Sum(x => x.AmountOverride == 0 ? x.AmountOriginal : x.AmountOverride).ToString("n")});
        sheet.Parameters.Add(new SheetParameter(true, "totalFinanceCharge") {ParamValue = dynamicPaymentPlanModuleData.TotalInterest});
        sheet.Parameters.Add(new SheetParameter(true, "totalCostOfLoan") {ParamValue = (dynamicPaymentPlanModuleData.ListPayPlanProductionEntries.Sum(x => x.AmountOverride == 0 ? x.AmountOriginal : x.AmountOverride) + (decimal) dynamicPaymentPlanModuleData.TotalInterest).ToString("n")});
        
        SheetFiller.FillFields(sheet);
        
        return sheet;
    }

    public static void TryPrintTxPlan(TreatPlan treatPlan, long patNum, long clinicNum, bool showDiscountNotAuto, bool showDiscount, bool showMaxDed, bool showSubTotals, bool showTotals, bool showCompleted, bool showFees, bool showIns, long printerNumOverride = 0)
    {
        var sheet = TreatPlanToSheet(treatPlan, patNum, clinicNum, showDiscountNotAuto, showDiscount, showMaxDed, showSubTotals, showTotals, showCompleted, showFees, showIns);
        
        SheetPrinting.Print(sheet, isPrintRemote: true, printerNumOverride: printerNumOverride);
    }

    private static Sheet TreatPlanToSheet(TreatPlan treatPlan, long patNum, long clinicNum, bool showDiscountNotAuto, bool showDiscount, bool showMaxDed, bool showSubTotals, bool showTotals, bool showCompleted, bool showFees, bool showIns)
    {
        var sheet = SheetUtil.CreateSheet(SheetDefs.GetSheetsDefault(SheetTypeEnum.TreatmentPlan, clinicNum), patNum);
        
        sheet.Parameters.Add(new SheetParameter(true, "TreatPlan") {ParamValue = treatPlan});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowDiscountNotAutomatic") {ParamValue = showDiscountNotAuto});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowDiscount") {ParamValue = showDiscount});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowMaxDed") {ParamValue = showMaxDed});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowSubTotals") {ParamValue = showSubTotals});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowTotals") {ParamValue = showTotals});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowCompleted") {ParamValue = showCompleted});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowFees") {ParamValue = showFees});
        sheet.Parameters.Add(new SheetParameter(true, "checkShowIns") {ParamValue = showIns});
        sheet.Parameters.Add(new SheetParameter(true, "toothChartImg") {ParamValue = SheetPrinting.GetToothChartHelper(patNum, showCompleted, treatPlan)});
        
        SheetFiller.FillFields(sheet);
        SheetUtil.CalculateHeights(sheet);
        
        return sheet;
    }

    public static void TryPrintSheet(long sheetNum, bool isStatement, bool isRxControlled, Statement statementCur, MedLab medLabCur, DataSet dataSet, long printerNumOverride = 0)
    {
        var sheet = Sheets.GetSheet(sheetNum);
        if (!SheetUtil.ValidateStateField(sheet))
        {
            return;
        }

        var errorString = SheetUtil.FixFontsForPdf(sheet, true);
        if (isStatement)
        {
            SheetPrinting.Print(sheet, dataSet, 1, isRxControlled, statementCur, medLabCur, isPrintRemote: true, printerNumOverride: printerNumOverride);
        }
        else
        {
            SheetPrinting.Print(sheet, 1, isRxControlled, statementCur, medLabCur, isPrintRemote: true, printerNumOverride: printerNumOverride);
        }

        if (!string.IsNullOrWhiteSpace(errorString))
        {
            throw new Exception(errorString);
        }
    }
}