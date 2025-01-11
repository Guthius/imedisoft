using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimCrud
{
    public static Claim SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Claim> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Claim> TableToList(DataTable table)
    {
        var retVal = new List<Claim>();
        Claim claim;
        foreach (DataRow row in table.Rows)
        {
            claim = new Claim();
            claim.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            claim.PatNum = SIn.Long(row["PatNum"].ToString());
            claim.DateService = SIn.Date(row["DateService"].ToString());
            claim.DateSent = SIn.Date(row["DateSent"].ToString());
            claim.ClaimStatus = SIn.String(row["ClaimStatus"].ToString());
            claim.DateReceived = SIn.Date(row["DateReceived"].ToString());
            claim.PlanNum = SIn.Long(row["PlanNum"].ToString());
            claim.ProvTreat = SIn.Long(row["ProvTreat"].ToString());
            claim.ClaimFee = SIn.Double(row["ClaimFee"].ToString());
            claim.InsPayEst = SIn.Double(row["InsPayEst"].ToString());
            claim.InsPayAmt = SIn.Double(row["InsPayAmt"].ToString());
            claim.DedApplied = SIn.Double(row["DedApplied"].ToString());
            claim.PreAuthString = SIn.String(row["PreAuthString"].ToString());
            claim.IsProsthesis = SIn.String(row["IsProsthesis"].ToString());
            claim.PriorDate = SIn.Date(row["PriorDate"].ToString());
            claim.ReasonUnderPaid = SIn.String(row["ReasonUnderPaid"].ToString());
            claim.ClaimNote = SIn.String(row["ClaimNote"].ToString());
            claim.ClaimType = SIn.String(row["ClaimType"].ToString());
            claim.ProvBill = SIn.Long(row["ProvBill"].ToString());
            claim.ReferringProv = SIn.Long(row["ReferringProv"].ToString());
            claim.RefNumString = SIn.String(row["RefNumString"].ToString());
            claim.PlaceService = (PlaceOfService) SIn.Int(row["PlaceService"].ToString());
            claim.AccidentRelated = SIn.String(row["AccidentRelated"].ToString());
            claim.AccidentDate = SIn.Date(row["AccidentDate"].ToString());
            claim.AccidentST = SIn.String(row["AccidentST"].ToString());
            claim.EmployRelated = (YN) SIn.Int(row["EmployRelated"].ToString());
            claim.IsOrtho = SIn.Bool(row["IsOrtho"].ToString());
            claim.OrthoRemainM = SIn.Byte(row["OrthoRemainM"].ToString());
            claim.OrthoDate = SIn.Date(row["OrthoDate"].ToString());
            claim.PatRelat = (Relat) SIn.Int(row["PatRelat"].ToString());
            claim.PlanNum2 = SIn.Long(row["PlanNum2"].ToString());
            claim.PatRelat2 = (Relat) SIn.Int(row["PatRelat2"].ToString());
            claim.WriteOff = SIn.Double(row["WriteOff"].ToString());
            claim.Radiographs = SIn.Byte(row["Radiographs"].ToString());
            claim.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            claim.ClaimForm = SIn.Long(row["ClaimForm"].ToString());
            claim.AttachedImages = SIn.Int(row["AttachedImages"].ToString());
            claim.AttachedModels = SIn.Int(row["AttachedModels"].ToString());
            claim.AttachedFlags = SIn.String(row["AttachedFlags"].ToString());
            claim.AttachmentID = SIn.String(row["AttachmentID"].ToString());
            claim.CanadianMaterialsForwarded = SIn.String(row["CanadianMaterialsForwarded"].ToString());
            claim.CanadianReferralProviderNum = SIn.String(row["CanadianReferralProviderNum"].ToString());
            claim.CanadianReferralReason = SIn.Byte(row["CanadianReferralReason"].ToString());
            claim.CanadianIsInitialLower = SIn.String(row["CanadianIsInitialLower"].ToString());
            claim.CanadianDateInitialLower = SIn.Date(row["CanadianDateInitialLower"].ToString());
            claim.CanadianMandProsthMaterial = SIn.Byte(row["CanadianMandProsthMaterial"].ToString());
            claim.CanadianIsInitialUpper = SIn.String(row["CanadianIsInitialUpper"].ToString());
            claim.CanadianDateInitialUpper = SIn.Date(row["CanadianDateInitialUpper"].ToString());
            claim.CanadianMaxProsthMaterial = SIn.Byte(row["CanadianMaxProsthMaterial"].ToString());
            claim.InsSubNum = SIn.Long(row["InsSubNum"].ToString());
            claim.InsSubNum2 = SIn.Long(row["InsSubNum2"].ToString());
            claim.CanadaTransRefNum = SIn.String(row["CanadaTransRefNum"].ToString());
            claim.CanadaEstTreatStartDate = SIn.Date(row["CanadaEstTreatStartDate"].ToString());
            claim.CanadaInitialPayment = SIn.Double(row["CanadaInitialPayment"].ToString());
            claim.CanadaPaymentMode = SIn.Byte(row["CanadaPaymentMode"].ToString());
            claim.CanadaTreatDuration = SIn.Byte(row["CanadaTreatDuration"].ToString());
            claim.CanadaNumAnticipatedPayments = SIn.Byte(row["CanadaNumAnticipatedPayments"].ToString());
            claim.CanadaAnticipatedPayAmount = SIn.Double(row["CanadaAnticipatedPayAmount"].ToString());
            claim.PriorAuthorizationNumber = SIn.String(row["PriorAuthorizationNumber"].ToString());
            claim.SpecialProgramCode = (EnumClaimSpecialProgram) SIn.Int(row["SpecialProgramCode"].ToString());
            claim.UniformBillType = SIn.String(row["UniformBillType"].ToString());
            claim.MedType = (EnumClaimMedType) SIn.Int(row["MedType"].ToString());
            claim.AdmissionTypeCode = SIn.String(row["AdmissionTypeCode"].ToString());
            claim.AdmissionSourceCode = SIn.String(row["AdmissionSourceCode"].ToString());
            claim.PatientStatusCode = SIn.String(row["PatientStatusCode"].ToString());
            claim.CustomTracking = SIn.Long(row["CustomTracking"].ToString());
            claim.DateResent = SIn.Date(row["DateResent"].ToString());
            claim.CorrectionType = (ClaimCorrectionType) SIn.Int(row["CorrectionType"].ToString());
            claim.ClaimIdentifier = SIn.String(row["ClaimIdentifier"].ToString());
            claim.OrigRefNum = SIn.String(row["OrigRefNum"].ToString());
            claim.ProvOrderOverride = SIn.Long(row["ProvOrderOverride"].ToString());
            claim.OrthoTotalM = SIn.Byte(row["OrthoTotalM"].ToString());
            claim.ShareOfCost = SIn.Double(row["ShareOfCost"].ToString());
            claim.SecUserNumEntry = SIn.Long(row["SecUserNumEntry"].ToString());
            claim.SecDateEntry = SIn.Date(row["SecDateEntry"].ToString());
            claim.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            claim.OrderingReferralNum = SIn.Long(row["OrderingReferralNum"].ToString());
            claim.DateSentOrig = SIn.Date(row["DateSentOrig"].ToString());
            claim.DateIllnessInjuryPreg = SIn.Date(row["DateIllnessInjuryPreg"].ToString());
            claim.DateIllnessInjuryPregQualifier = (DateIllnessInjuryPregQualifier) SIn.Int(row["DateIllnessInjuryPregQualifier"].ToString());
            claim.DateOther = SIn.Date(row["DateOther"].ToString());
            claim.DateOtherQualifier = (DateOtherQualifier) SIn.Int(row["DateOtherQualifier"].ToString());
            claim.IsOutsideLab = SIn.Bool(row["IsOutsideLab"].ToString());
            claim.SecurityHash = SIn.String(row["SecurityHash"].ToString());
            claim.Narrative = SIn.String(row["Narrative"].ToString());
            retVal.Add(claim);
        }

        return retVal;
    }

    public static long Insert(Claim claim)
    {
        var command = "INSERT INTO claim (";

        command += "PatNum,DateService,DateSent,ClaimStatus,DateReceived,PlanNum,ProvTreat,ClaimFee,InsPayEst,InsPayAmt,DedApplied,PreAuthString,IsProsthesis,PriorDate,ReasonUnderPaid,ClaimNote,ClaimType,ProvBill,ReferringProv,RefNumString,PlaceService,AccidentRelated,AccidentDate,AccidentST,EmployRelated,IsOrtho,OrthoRemainM,OrthoDate,PatRelat,PlanNum2,PatRelat2,WriteOff,Radiographs,ClinicNum,ClaimForm,AttachedImages,AttachedModels,AttachedFlags,AttachmentID,CanadianMaterialsForwarded,CanadianReferralProviderNum,CanadianReferralReason,CanadianIsInitialLower,CanadianDateInitialLower,CanadianMandProsthMaterial,CanadianIsInitialUpper,CanadianDateInitialUpper,CanadianMaxProsthMaterial,InsSubNum,InsSubNum2,CanadaTransRefNum,CanadaEstTreatStartDate,CanadaInitialPayment,CanadaPaymentMode,CanadaTreatDuration,CanadaNumAnticipatedPayments,CanadaAnticipatedPayAmount,PriorAuthorizationNumber,SpecialProgramCode,UniformBillType,MedType,AdmissionTypeCode,AdmissionSourceCode,PatientStatusCode,CustomTracking,DateResent,CorrectionType,ClaimIdentifier,OrigRefNum,ProvOrderOverride,OrthoTotalM,ShareOfCost,SecUserNumEntry,SecDateEntry,OrderingReferralNum,DateSentOrig,DateIllnessInjuryPreg,DateIllnessInjuryPregQualifier,DateOther,DateOtherQualifier,IsOutsideLab,SecurityHash,Narrative) VALUES(";

        command +=
            SOut.Long(claim.PatNum) + ","
                                    + SOut.Date(claim.DateService) + ","
                                    + SOut.Date(claim.DateSent) + ","
                                    + "'" + SOut.String(claim.ClaimStatus) + "',"
                                    + SOut.Date(claim.DateReceived) + ","
                                    + SOut.Long(claim.PlanNum) + ","
                                    + SOut.Long(claim.ProvTreat) + ","
                                    + SOut.Double(claim.ClaimFee) + ","
                                    + SOut.Double(claim.InsPayEst) + ","
                                    + SOut.Double(claim.InsPayAmt) + ","
                                    + SOut.Double(claim.DedApplied) + ","
                                    + "'" + SOut.String(claim.PreAuthString) + "',"
                                    + "'" + SOut.String(claim.IsProsthesis) + "',"
                                    + SOut.Date(claim.PriorDate) + ","
                                    + "'" + SOut.String(claim.ReasonUnderPaid) + "',"
                                    + "'" + SOut.String(claim.ClaimNote) + "',"
                                    + "'" + SOut.String(claim.ClaimType) + "',"
                                    + SOut.Long(claim.ProvBill) + ","
                                    + SOut.Long(claim.ReferringProv) + ","
                                    + "'" + SOut.String(claim.RefNumString) + "',"
                                    + SOut.Int((int) claim.PlaceService) + ","
                                    + "'" + SOut.String(claim.AccidentRelated) + "',"
                                    + SOut.Date(claim.AccidentDate) + ","
                                    + "'" + SOut.String(claim.AccidentST) + "',"
                                    + SOut.Int((int) claim.EmployRelated) + ","
                                    + SOut.Bool(claim.IsOrtho) + ","
                                    + SOut.Byte(claim.OrthoRemainM) + ","
                                    + SOut.Date(claim.OrthoDate) + ","
                                    + SOut.Int((int) claim.PatRelat) + ","
                                    + SOut.Long(claim.PlanNum2) + ","
                                    + SOut.Int((int) claim.PatRelat2) + ","
                                    + SOut.Double(claim.WriteOff) + ","
                                    + SOut.Byte(claim.Radiographs) + ","
                                    + SOut.Long(claim.ClinicNum) + ","
                                    + SOut.Long(claim.ClaimForm) + ","
                                    + SOut.Int(claim.AttachedImages) + ","
                                    + SOut.Int(claim.AttachedModels) + ","
                                    + "'" + SOut.String(claim.AttachedFlags) + "',"
                                    + "'" + SOut.String(claim.AttachmentID) + "',"
                                    + "'" + SOut.String(claim.CanadianMaterialsForwarded) + "',"
                                    + "'" + SOut.String(claim.CanadianReferralProviderNum) + "',"
                                    + SOut.Byte(claim.CanadianReferralReason) + ","
                                    + "'" + SOut.String(claim.CanadianIsInitialLower) + "',"
                                    + SOut.Date(claim.CanadianDateInitialLower) + ","
                                    + SOut.Byte(claim.CanadianMandProsthMaterial) + ","
                                    + "'" + SOut.String(claim.CanadianIsInitialUpper) + "',"
                                    + SOut.Date(claim.CanadianDateInitialUpper) + ","
                                    + SOut.Byte(claim.CanadianMaxProsthMaterial) + ","
                                    + SOut.Long(claim.InsSubNum) + ","
                                    + SOut.Long(claim.InsSubNum2) + ","
                                    + "'" + SOut.String(claim.CanadaTransRefNum) + "',"
                                    + SOut.Date(claim.CanadaEstTreatStartDate) + ","
                                    + SOut.Double(claim.CanadaInitialPayment) + ","
                                    + SOut.Byte(claim.CanadaPaymentMode) + ","
                                    + SOut.Byte(claim.CanadaTreatDuration) + ","
                                    + SOut.Byte(claim.CanadaNumAnticipatedPayments) + ","
                                    + SOut.Double(claim.CanadaAnticipatedPayAmount) + ","
                                    + "'" + SOut.String(claim.PriorAuthorizationNumber) + "',"
                                    + SOut.Int((int) claim.SpecialProgramCode) + ","
                                    + "'" + SOut.String(claim.UniformBillType) + "',"
                                    + SOut.Int((int) claim.MedType) + ","
                                    + "'" + SOut.String(claim.AdmissionTypeCode) + "',"
                                    + "'" + SOut.String(claim.AdmissionSourceCode) + "',"
                                    + "'" + SOut.String(claim.PatientStatusCode) + "',"
                                    + SOut.Long(claim.CustomTracking) + ","
                                    + SOut.Date(claim.DateResent) + ","
                                    + SOut.Int((int) claim.CorrectionType) + ","
                                    + "'" + SOut.String(claim.ClaimIdentifier) + "',"
                                    + "'" + SOut.String(claim.OrigRefNum) + "',"
                                    + SOut.Long(claim.ProvOrderOverride) + ","
                                    + SOut.Byte(claim.OrthoTotalM) + ","
                                    + SOut.Double(claim.ShareOfCost) + ","
                                    + SOut.Long(claim.SecUserNumEntry) + ","
                                    + DbHelper.Now() + ","
                                    //SecDateTEdit can only be set by MySQL
                                    + SOut.Long(claim.OrderingReferralNum) + ","
                                    + SOut.Date(claim.DateSentOrig) + ","
                                    + SOut.Date(claim.DateIllnessInjuryPreg) + ","
                                    + SOut.Int((int) claim.DateIllnessInjuryPregQualifier) + ","
                                    + SOut.Date(claim.DateOther) + ","
                                    + SOut.Int((int) claim.DateOtherQualifier) + ","
                                    + SOut.Bool(claim.IsOutsideLab) + ","
                                    + "'" + SOut.String(claim.SecurityHash) + "',"
                                    + DbHelper.ParamChar + "paramNarrative)";
        if (claim.Narrative == null) claim.Narrative = "";
        var paramNarrative = new OdSqlParameter("paramNarrative", OdDbType.Text, SOut.StringParam(claim.Narrative));
        {
            claim.ClaimNum = Db.NonQ(command, true, "ClaimNum", "claim", paramNarrative);
        }
        return claim.ClaimNum;
    }

    public static void Update(Claim claim)
    {
        var command = "UPDATE claim SET "
                      + "PatNum                        =  " + SOut.Long(claim.PatNum) + ", "
                      + "DateService                   =  " + SOut.Date(claim.DateService) + ", "
                      + "DateSent                      =  " + SOut.Date(claim.DateSent) + ", "
                      + "ClaimStatus                   = '" + SOut.String(claim.ClaimStatus) + "', "
                      + "DateReceived                  =  " + SOut.Date(claim.DateReceived) + ", "
                      + "PlanNum                       =  " + SOut.Long(claim.PlanNum) + ", "
                      + "ProvTreat                     =  " + SOut.Long(claim.ProvTreat) + ", "
                      + "ClaimFee                      =  " + SOut.Double(claim.ClaimFee) + ", "
                      + "InsPayEst                     =  " + SOut.Double(claim.InsPayEst) + ", "
                      + "InsPayAmt                     =  " + SOut.Double(claim.InsPayAmt) + ", "
                      + "DedApplied                    =  " + SOut.Double(claim.DedApplied) + ", "
                      + "PreAuthString                 = '" + SOut.String(claim.PreAuthString) + "', "
                      + "IsProsthesis                  = '" + SOut.String(claim.IsProsthesis) + "', "
                      + "PriorDate                     =  " + SOut.Date(claim.PriorDate) + ", "
                      + "ReasonUnderPaid               = '" + SOut.String(claim.ReasonUnderPaid) + "', "
                      + "ClaimNote                     = '" + SOut.String(claim.ClaimNote) + "', "
                      + "ClaimType                     = '" + SOut.String(claim.ClaimType) + "', "
                      + "ProvBill                      =  " + SOut.Long(claim.ProvBill) + ", "
                      + "ReferringProv                 =  " + SOut.Long(claim.ReferringProv) + ", "
                      + "RefNumString                  = '" + SOut.String(claim.RefNumString) + "', "
                      + "PlaceService                  =  " + SOut.Int((int) claim.PlaceService) + ", "
                      + "AccidentRelated               = '" + SOut.String(claim.AccidentRelated) + "', "
                      + "AccidentDate                  =  " + SOut.Date(claim.AccidentDate) + ", "
                      + "AccidentST                    = '" + SOut.String(claim.AccidentST) + "', "
                      + "EmployRelated                 =  " + SOut.Int((int) claim.EmployRelated) + ", "
                      + "IsOrtho                       =  " + SOut.Bool(claim.IsOrtho) + ", "
                      + "OrthoRemainM                  =  " + SOut.Byte(claim.OrthoRemainM) + ", "
                      + "OrthoDate                     =  " + SOut.Date(claim.OrthoDate) + ", "
                      + "PatRelat                      =  " + SOut.Int((int) claim.PatRelat) + ", "
                      + "PlanNum2                      =  " + SOut.Long(claim.PlanNum2) + ", "
                      + "PatRelat2                     =  " + SOut.Int((int) claim.PatRelat2) + ", "
                      + "WriteOff                      =  " + SOut.Double(claim.WriteOff) + ", "
                      + "Radiographs                   =  " + SOut.Byte(claim.Radiographs) + ", "
                      + "ClinicNum                     =  " + SOut.Long(claim.ClinicNum) + ", "
                      + "ClaimForm                     =  " + SOut.Long(claim.ClaimForm) + ", "
                      + "AttachedImages                =  " + SOut.Int(claim.AttachedImages) + ", "
                      + "AttachedModels                =  " + SOut.Int(claim.AttachedModels) + ", "
                      + "AttachedFlags                 = '" + SOut.String(claim.AttachedFlags) + "', "
                      + "AttachmentID                  = '" + SOut.String(claim.AttachmentID) + "', "
                      + "CanadianMaterialsForwarded    = '" + SOut.String(claim.CanadianMaterialsForwarded) + "', "
                      + "CanadianReferralProviderNum   = '" + SOut.String(claim.CanadianReferralProviderNum) + "', "
                      + "CanadianReferralReason        =  " + SOut.Byte(claim.CanadianReferralReason) + ", "
                      + "CanadianIsInitialLower        = '" + SOut.String(claim.CanadianIsInitialLower) + "', "
                      + "CanadianDateInitialLower      =  " + SOut.Date(claim.CanadianDateInitialLower) + ", "
                      + "CanadianMandProsthMaterial    =  " + SOut.Byte(claim.CanadianMandProsthMaterial) + ", "
                      + "CanadianIsInitialUpper        = '" + SOut.String(claim.CanadianIsInitialUpper) + "', "
                      + "CanadianDateInitialUpper      =  " + SOut.Date(claim.CanadianDateInitialUpper) + ", "
                      + "CanadianMaxProsthMaterial     =  " + SOut.Byte(claim.CanadianMaxProsthMaterial) + ", "
                      + "InsSubNum                     =  " + SOut.Long(claim.InsSubNum) + ", "
                      + "InsSubNum2                    =  " + SOut.Long(claim.InsSubNum2) + ", "
                      + "CanadaTransRefNum             = '" + SOut.String(claim.CanadaTransRefNum) + "', "
                      + "CanadaEstTreatStartDate       =  " + SOut.Date(claim.CanadaEstTreatStartDate) + ", "
                      + "CanadaInitialPayment          =  " + SOut.Double(claim.CanadaInitialPayment) + ", "
                      + "CanadaPaymentMode             =  " + SOut.Byte(claim.CanadaPaymentMode) + ", "
                      + "CanadaTreatDuration           =  " + SOut.Byte(claim.CanadaTreatDuration) + ", "
                      + "CanadaNumAnticipatedPayments  =  " + SOut.Byte(claim.CanadaNumAnticipatedPayments) + ", "
                      + "CanadaAnticipatedPayAmount    =  " + SOut.Double(claim.CanadaAnticipatedPayAmount) + ", "
                      + "PriorAuthorizationNumber      = '" + SOut.String(claim.PriorAuthorizationNumber) + "', "
                      + "SpecialProgramCode            =  " + SOut.Int((int) claim.SpecialProgramCode) + ", "
                      + "UniformBillType               = '" + SOut.String(claim.UniformBillType) + "', "
                      + "MedType                       =  " + SOut.Int((int) claim.MedType) + ", "
                      + "AdmissionTypeCode             = '" + SOut.String(claim.AdmissionTypeCode) + "', "
                      + "AdmissionSourceCode           = '" + SOut.String(claim.AdmissionSourceCode) + "', "
                      + "PatientStatusCode             = '" + SOut.String(claim.PatientStatusCode) + "', "
                      + "CustomTracking                =  " + SOut.Long(claim.CustomTracking) + ", "
                      + "DateResent                    =  " + SOut.Date(claim.DateResent) + ", "
                      + "CorrectionType                =  " + SOut.Int((int) claim.CorrectionType) + ", "
                      + "ClaimIdentifier               = '" + SOut.String(claim.ClaimIdentifier) + "', "
                      + "OrigRefNum                    = '" + SOut.String(claim.OrigRefNum) + "', "
                      + "ProvOrderOverride             =  " + SOut.Long(claim.ProvOrderOverride) + ", "
                      + "OrthoTotalM                   =  " + SOut.Byte(claim.OrthoTotalM) + ", "
                      + "ShareOfCost                   =  " + SOut.Double(claim.ShareOfCost) + ", "
                      //SecUserNumEntry excluded from update
                      //SecDateEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "OrderingReferralNum           =  " + SOut.Long(claim.OrderingReferralNum) + ", "
                      + "DateSentOrig                  =  " + SOut.Date(claim.DateSentOrig) + ", "
                      + "DateIllnessInjuryPreg         =  " + SOut.Date(claim.DateIllnessInjuryPreg) + ", "
                      + "DateIllnessInjuryPregQualifier=  " + SOut.Int((int) claim.DateIllnessInjuryPregQualifier) + ", "
                      + "DateOther                     =  " + SOut.Date(claim.DateOther) + ", "
                      + "DateOtherQualifier            =  " + SOut.Int((int) claim.DateOtherQualifier) + ", "
                      + "IsOutsideLab                  =  " + SOut.Bool(claim.IsOutsideLab) + ", "
                      + "SecurityHash                  = '" + SOut.String(claim.SecurityHash) + "', "
                      + "Narrative                     =  " + DbHelper.ParamChar + "paramNarrative "
                      + "WHERE ClaimNum = " + SOut.Long(claim.ClaimNum);
        if (claim.Narrative == null) claim.Narrative = "";
        var paramNarrative = new OdSqlParameter("paramNarrative", OdDbType.Text, SOut.StringParam(claim.Narrative));
        Db.NonQ(command, paramNarrative);
    }

    public static void Update(Claim claim, Claim oldClaim)
    {
        var command = "";
        if (claim.PatNum != oldClaim.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(claim.PatNum) + "";
        }

        if (claim.DateService.Date != oldClaim.DateService.Date)
        {
            if (command != "") command += ",";
            command += "DateService = " + SOut.Date(claim.DateService) + "";
        }

        if (claim.DateSent.Date != oldClaim.DateSent.Date)
        {
            if (command != "") command += ",";
            command += "DateSent = " + SOut.Date(claim.DateSent) + "";
        }

        if (claim.ClaimStatus != oldClaim.ClaimStatus)
        {
            if (command != "") command += ",";
            command += "ClaimStatus = '" + SOut.String(claim.ClaimStatus) + "'";
        }

        if (claim.DateReceived.Date != oldClaim.DateReceived.Date)
        {
            if (command != "") command += ",";
            command += "DateReceived = " + SOut.Date(claim.DateReceived) + "";
        }

        if (claim.PlanNum != oldClaim.PlanNum)
        {
            if (command != "") command += ",";
            command += "PlanNum = " + SOut.Long(claim.PlanNum) + "";
        }

        if (claim.ProvTreat != oldClaim.ProvTreat)
        {
            if (command != "") command += ",";
            command += "ProvTreat = " + SOut.Long(claim.ProvTreat) + "";
        }

        if (claim.ClaimFee != oldClaim.ClaimFee)
        {
            if (command != "") command += ",";
            command += "ClaimFee = " + SOut.Double(claim.ClaimFee) + "";
        }

        if (claim.InsPayEst != oldClaim.InsPayEst)
        {
            if (command != "") command += ",";
            command += "InsPayEst = " + SOut.Double(claim.InsPayEst) + "";
        }

        if (claim.InsPayAmt != oldClaim.InsPayAmt)
        {
            if (command != "") command += ",";
            command += "InsPayAmt = " + SOut.Double(claim.InsPayAmt) + "";
        }

        if (claim.DedApplied != oldClaim.DedApplied)
        {
            if (command != "") command += ",";
            command += "DedApplied = " + SOut.Double(claim.DedApplied) + "";
        }

        if (claim.PreAuthString != oldClaim.PreAuthString)
        {
            if (command != "") command += ",";
            command += "PreAuthString = '" + SOut.String(claim.PreAuthString) + "'";
        }

        if (claim.IsProsthesis != oldClaim.IsProsthesis)
        {
            if (command != "") command += ",";
            command += "IsProsthesis = '" + SOut.String(claim.IsProsthesis) + "'";
        }

        if (claim.PriorDate.Date != oldClaim.PriorDate.Date)
        {
            if (command != "") command += ",";
            command += "PriorDate = " + SOut.Date(claim.PriorDate) + "";
        }

        if (claim.ReasonUnderPaid != oldClaim.ReasonUnderPaid)
        {
            if (command != "") command += ",";
            command += "ReasonUnderPaid = '" + SOut.String(claim.ReasonUnderPaid) + "'";
        }

        if (claim.ClaimNote != oldClaim.ClaimNote)
        {
            if (command != "") command += ",";
            command += "ClaimNote = '" + SOut.String(claim.ClaimNote) + "'";
        }

        if (claim.ClaimType != oldClaim.ClaimType)
        {
            if (command != "") command += ",";
            command += "ClaimType = '" + SOut.String(claim.ClaimType) + "'";
        }

        if (claim.ProvBill != oldClaim.ProvBill)
        {
            if (command != "") command += ",";
            command += "ProvBill = " + SOut.Long(claim.ProvBill) + "";
        }

        if (claim.ReferringProv != oldClaim.ReferringProv)
        {
            if (command != "") command += ",";
            command += "ReferringProv = " + SOut.Long(claim.ReferringProv) + "";
        }

        if (claim.RefNumString != oldClaim.RefNumString)
        {
            if (command != "") command += ",";
            command += "RefNumString = '" + SOut.String(claim.RefNumString) + "'";
        }

        if (claim.PlaceService != oldClaim.PlaceService)
        {
            if (command != "") command += ",";
            command += "PlaceService = " + SOut.Int((int) claim.PlaceService) + "";
        }

        if (claim.AccidentRelated != oldClaim.AccidentRelated)
        {
            if (command != "") command += ",";
            command += "AccidentRelated = '" + SOut.String(claim.AccidentRelated) + "'";
        }

        if (claim.AccidentDate.Date != oldClaim.AccidentDate.Date)
        {
            if (command != "") command += ",";
            command += "AccidentDate = " + SOut.Date(claim.AccidentDate) + "";
        }

        if (claim.AccidentST != oldClaim.AccidentST)
        {
            if (command != "") command += ",";
            command += "AccidentST = '" + SOut.String(claim.AccidentST) + "'";
        }

        if (claim.EmployRelated != oldClaim.EmployRelated)
        {
            if (command != "") command += ",";
            command += "EmployRelated = " + SOut.Int((int) claim.EmployRelated) + "";
        }

        if (claim.IsOrtho != oldClaim.IsOrtho)
        {
            if (command != "") command += ",";
            command += "IsOrtho = " + SOut.Bool(claim.IsOrtho) + "";
        }

        if (claim.OrthoRemainM != oldClaim.OrthoRemainM)
        {
            if (command != "") command += ",";
            command += "OrthoRemainM = " + SOut.Byte(claim.OrthoRemainM) + "";
        }

        if (claim.OrthoDate.Date != oldClaim.OrthoDate.Date)
        {
            if (command != "") command += ",";
            command += "OrthoDate = " + SOut.Date(claim.OrthoDate) + "";
        }

        if (claim.PatRelat != oldClaim.PatRelat)
        {
            if (command != "") command += ",";
            command += "PatRelat = " + SOut.Int((int) claim.PatRelat) + "";
        }

        if (claim.PlanNum2 != oldClaim.PlanNum2)
        {
            if (command != "") command += ",";
            command += "PlanNum2 = " + SOut.Long(claim.PlanNum2) + "";
        }

        if (claim.PatRelat2 != oldClaim.PatRelat2)
        {
            if (command != "") command += ",";
            command += "PatRelat2 = " + SOut.Int((int) claim.PatRelat2) + "";
        }

        if (claim.WriteOff != oldClaim.WriteOff)
        {
            if (command != "") command += ",";
            command += "WriteOff = " + SOut.Double(claim.WriteOff) + "";
        }

        if (claim.Radiographs != oldClaim.Radiographs)
        {
            if (command != "") command += ",";
            command += "Radiographs = " + SOut.Byte(claim.Radiographs) + "";
        }

        if (claim.ClinicNum != oldClaim.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(claim.ClinicNum) + "";
        }

        if (claim.ClaimForm != oldClaim.ClaimForm)
        {
            if (command != "") command += ",";
            command += "ClaimForm = " + SOut.Long(claim.ClaimForm) + "";
        }

        if (claim.AttachedImages != oldClaim.AttachedImages)
        {
            if (command != "") command += ",";
            command += "AttachedImages = " + SOut.Int(claim.AttachedImages) + "";
        }

        if (claim.AttachedModels != oldClaim.AttachedModels)
        {
            if (command != "") command += ",";
            command += "AttachedModels = " + SOut.Int(claim.AttachedModels) + "";
        }

        if (claim.AttachedFlags != oldClaim.AttachedFlags)
        {
            if (command != "") command += ",";
            command += "AttachedFlags = '" + SOut.String(claim.AttachedFlags) + "'";
        }

        if (claim.AttachmentID != oldClaim.AttachmentID)
        {
            if (command != "") command += ",";
            command += "AttachmentID = '" + SOut.String(claim.AttachmentID) + "'";
        }

        if (claim.CanadianMaterialsForwarded != oldClaim.CanadianMaterialsForwarded)
        {
            if (command != "") command += ",";
            command += "CanadianMaterialsForwarded = '" + SOut.String(claim.CanadianMaterialsForwarded) + "'";
        }

        if (claim.CanadianReferralProviderNum != oldClaim.CanadianReferralProviderNum)
        {
            if (command != "") command += ",";
            command += "CanadianReferralProviderNum = '" + SOut.String(claim.CanadianReferralProviderNum) + "'";
        }

        if (claim.CanadianReferralReason != oldClaim.CanadianReferralReason)
        {
            if (command != "") command += ",";
            command += "CanadianReferralReason = " + SOut.Byte(claim.CanadianReferralReason) + "";
        }

        if (claim.CanadianIsInitialLower != oldClaim.CanadianIsInitialLower)
        {
            if (command != "") command += ",";
            command += "CanadianIsInitialLower = '" + SOut.String(claim.CanadianIsInitialLower) + "'";
        }

        if (claim.CanadianDateInitialLower.Date != oldClaim.CanadianDateInitialLower.Date)
        {
            if (command != "") command += ",";
            command += "CanadianDateInitialLower = " + SOut.Date(claim.CanadianDateInitialLower) + "";
        }

        if (claim.CanadianMandProsthMaterial != oldClaim.CanadianMandProsthMaterial)
        {
            if (command != "") command += ",";
            command += "CanadianMandProsthMaterial = " + SOut.Byte(claim.CanadianMandProsthMaterial) + "";
        }

        if (claim.CanadianIsInitialUpper != oldClaim.CanadianIsInitialUpper)
        {
            if (command != "") command += ",";
            command += "CanadianIsInitialUpper = '" + SOut.String(claim.CanadianIsInitialUpper) + "'";
        }

        if (claim.CanadianDateInitialUpper.Date != oldClaim.CanadianDateInitialUpper.Date)
        {
            if (command != "") command += ",";
            command += "CanadianDateInitialUpper = " + SOut.Date(claim.CanadianDateInitialUpper) + "";
        }

        if (claim.CanadianMaxProsthMaterial != oldClaim.CanadianMaxProsthMaterial)
        {
            if (command != "") command += ",";
            command += "CanadianMaxProsthMaterial = " + SOut.Byte(claim.CanadianMaxProsthMaterial) + "";
        }

        if (claim.InsSubNum != oldClaim.InsSubNum)
        {
            if (command != "") command += ",";
            command += "InsSubNum = " + SOut.Long(claim.InsSubNum) + "";
        }

        if (claim.InsSubNum2 != oldClaim.InsSubNum2)
        {
            if (command != "") command += ",";
            command += "InsSubNum2 = " + SOut.Long(claim.InsSubNum2) + "";
        }

        if (claim.CanadaTransRefNum != oldClaim.CanadaTransRefNum)
        {
            if (command != "") command += ",";
            command += "CanadaTransRefNum = '" + SOut.String(claim.CanadaTransRefNum) + "'";
        }

        if (claim.CanadaEstTreatStartDate.Date != oldClaim.CanadaEstTreatStartDate.Date)
        {
            if (command != "") command += ",";
            command += "CanadaEstTreatStartDate = " + SOut.Date(claim.CanadaEstTreatStartDate) + "";
        }

        if (claim.CanadaInitialPayment != oldClaim.CanadaInitialPayment)
        {
            if (command != "") command += ",";
            command += "CanadaInitialPayment = " + SOut.Double(claim.CanadaInitialPayment) + "";
        }

        if (claim.CanadaPaymentMode != oldClaim.CanadaPaymentMode)
        {
            if (command != "") command += ",";
            command += "CanadaPaymentMode = " + SOut.Byte(claim.CanadaPaymentMode) + "";
        }

        if (claim.CanadaTreatDuration != oldClaim.CanadaTreatDuration)
        {
            if (command != "") command += ",";
            command += "CanadaTreatDuration = " + SOut.Byte(claim.CanadaTreatDuration) + "";
        }

        if (claim.CanadaNumAnticipatedPayments != oldClaim.CanadaNumAnticipatedPayments)
        {
            if (command != "") command += ",";
            command += "CanadaNumAnticipatedPayments = " + SOut.Byte(claim.CanadaNumAnticipatedPayments) + "";
        }

        if (claim.CanadaAnticipatedPayAmount != oldClaim.CanadaAnticipatedPayAmount)
        {
            if (command != "") command += ",";
            command += "CanadaAnticipatedPayAmount = " + SOut.Double(claim.CanadaAnticipatedPayAmount) + "";
        }

        if (claim.PriorAuthorizationNumber != oldClaim.PriorAuthorizationNumber)
        {
            if (command != "") command += ",";
            command += "PriorAuthorizationNumber = '" + SOut.String(claim.PriorAuthorizationNumber) + "'";
        }

        if (claim.SpecialProgramCode != oldClaim.SpecialProgramCode)
        {
            if (command != "") command += ",";
            command += "SpecialProgramCode = " + SOut.Int((int) claim.SpecialProgramCode) + "";
        }

        if (claim.UniformBillType != oldClaim.UniformBillType)
        {
            if (command != "") command += ",";
            command += "UniformBillType = '" + SOut.String(claim.UniformBillType) + "'";
        }

        if (claim.MedType != oldClaim.MedType)
        {
            if (command != "") command += ",";
            command += "MedType = " + SOut.Int((int) claim.MedType) + "";
        }

        if (claim.AdmissionTypeCode != oldClaim.AdmissionTypeCode)
        {
            if (command != "") command += ",";
            command += "AdmissionTypeCode = '" + SOut.String(claim.AdmissionTypeCode) + "'";
        }

        if (claim.AdmissionSourceCode != oldClaim.AdmissionSourceCode)
        {
            if (command != "") command += ",";
            command += "AdmissionSourceCode = '" + SOut.String(claim.AdmissionSourceCode) + "'";
        }

        if (claim.PatientStatusCode != oldClaim.PatientStatusCode)
        {
            if (command != "") command += ",";
            command += "PatientStatusCode = '" + SOut.String(claim.PatientStatusCode) + "'";
        }

        if (claim.CustomTracking != oldClaim.CustomTracking)
        {
            if (command != "") command += ",";
            command += "CustomTracking = " + SOut.Long(claim.CustomTracking) + "";
        }

        if (claim.DateResent.Date != oldClaim.DateResent.Date)
        {
            if (command != "") command += ",";
            command += "DateResent = " + SOut.Date(claim.DateResent) + "";
        }

        if (claim.CorrectionType != oldClaim.CorrectionType)
        {
            if (command != "") command += ",";
            command += "CorrectionType = " + SOut.Int((int) claim.CorrectionType) + "";
        }

        if (claim.ClaimIdentifier != oldClaim.ClaimIdentifier)
        {
            if (command != "") command += ",";
            command += "ClaimIdentifier = '" + SOut.String(claim.ClaimIdentifier) + "'";
        }

        if (claim.OrigRefNum != oldClaim.OrigRefNum)
        {
            if (command != "") command += ",";
            command += "OrigRefNum = '" + SOut.String(claim.OrigRefNum) + "'";
        }

        if (claim.ProvOrderOverride != oldClaim.ProvOrderOverride)
        {
            if (command != "") command += ",";
            command += "ProvOrderOverride = " + SOut.Long(claim.ProvOrderOverride) + "";
        }

        if (claim.OrthoTotalM != oldClaim.OrthoTotalM)
        {
            if (command != "") command += ",";
            command += "OrthoTotalM = " + SOut.Byte(claim.OrthoTotalM) + "";
        }

        if (claim.ShareOfCost != oldClaim.ShareOfCost)
        {
            if (command != "") command += ",";
            command += "ShareOfCost = " + SOut.Double(claim.ShareOfCost) + "";
        }

        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (claim.OrderingReferralNum != oldClaim.OrderingReferralNum)
        {
            if (command != "") command += ",";
            command += "OrderingReferralNum = " + SOut.Long(claim.OrderingReferralNum) + "";
        }

        if (claim.DateSentOrig.Date != oldClaim.DateSentOrig.Date)
        {
            if (command != "") command += ",";
            command += "DateSentOrig = " + SOut.Date(claim.DateSentOrig) + "";
        }

        if (claim.DateIllnessInjuryPreg.Date != oldClaim.DateIllnessInjuryPreg.Date)
        {
            if (command != "") command += ",";
            command += "DateIllnessInjuryPreg = " + SOut.Date(claim.DateIllnessInjuryPreg) + "";
        }

        if (claim.DateIllnessInjuryPregQualifier != oldClaim.DateIllnessInjuryPregQualifier)
        {
            if (command != "") command += ",";
            command += "DateIllnessInjuryPregQualifier = " + SOut.Int((int) claim.DateIllnessInjuryPregQualifier) + "";
        }

        if (claim.DateOther.Date != oldClaim.DateOther.Date)
        {
            if (command != "") command += ",";
            command += "DateOther = " + SOut.Date(claim.DateOther) + "";
        }

        if (claim.DateOtherQualifier != oldClaim.DateOtherQualifier)
        {
            if (command != "") command += ",";
            command += "DateOtherQualifier = " + SOut.Int((int) claim.DateOtherQualifier) + "";
        }

        if (claim.IsOutsideLab != oldClaim.IsOutsideLab)
        {
            if (command != "") command += ",";
            command += "IsOutsideLab = " + SOut.Bool(claim.IsOutsideLab) + "";
        }

        if (claim.SecurityHash != oldClaim.SecurityHash)
        {
            if (command != "") command += ",";
            command += "SecurityHash = '" + SOut.String(claim.SecurityHash) + "'";
        }

        if (claim.Narrative != oldClaim.Narrative)
        {
            if (command != "") command += ",";
            command += "Narrative = " + DbHelper.ParamChar + "paramNarrative";
        }

        if (command == "") return;
        if (claim.Narrative == null) claim.Narrative = "";
        var paramNarrative = new OdSqlParameter("paramNarrative", OdDbType.Text, SOut.StringParam(claim.Narrative));
        command = "UPDATE claim SET " + command
                                      + " WHERE ClaimNum = " + SOut.Long(claim.ClaimNum);
        Db.NonQ(command, paramNarrative);
    }
    
    public static bool UpdateComparison(Claim claim, Claim oldClaim)
    {
        if (claim.PatNum != oldClaim.PatNum) return true;
        if (claim.DateService.Date != oldClaim.DateService.Date) return true;
        if (claim.DateSent.Date != oldClaim.DateSent.Date) return true;
        if (claim.ClaimStatus != oldClaim.ClaimStatus) return true;
        if (claim.DateReceived.Date != oldClaim.DateReceived.Date) return true;
        if (claim.PlanNum != oldClaim.PlanNum) return true;
        if (claim.ProvTreat != oldClaim.ProvTreat) return true;
        if (claim.ClaimFee != oldClaim.ClaimFee) return true;
        if (claim.InsPayEst != oldClaim.InsPayEst) return true;
        if (claim.InsPayAmt != oldClaim.InsPayAmt) return true;
        if (claim.DedApplied != oldClaim.DedApplied) return true;
        if (claim.PreAuthString != oldClaim.PreAuthString) return true;
        if (claim.IsProsthesis != oldClaim.IsProsthesis) return true;
        if (claim.PriorDate.Date != oldClaim.PriorDate.Date) return true;
        if (claim.ReasonUnderPaid != oldClaim.ReasonUnderPaid) return true;
        if (claim.ClaimNote != oldClaim.ClaimNote) return true;
        if (claim.ClaimType != oldClaim.ClaimType) return true;
        if (claim.ProvBill != oldClaim.ProvBill) return true;
        if (claim.ReferringProv != oldClaim.ReferringProv) return true;
        if (claim.RefNumString != oldClaim.RefNumString) return true;
        if (claim.PlaceService != oldClaim.PlaceService) return true;
        if (claim.AccidentRelated != oldClaim.AccidentRelated) return true;
        if (claim.AccidentDate.Date != oldClaim.AccidentDate.Date) return true;
        if (claim.AccidentST != oldClaim.AccidentST) return true;
        if (claim.EmployRelated != oldClaim.EmployRelated) return true;
        if (claim.IsOrtho != oldClaim.IsOrtho) return true;
        if (claim.OrthoRemainM != oldClaim.OrthoRemainM) return true;
        if (claim.OrthoDate.Date != oldClaim.OrthoDate.Date) return true;
        if (claim.PatRelat != oldClaim.PatRelat) return true;
        if (claim.PlanNum2 != oldClaim.PlanNum2) return true;
        if (claim.PatRelat2 != oldClaim.PatRelat2) return true;
        if (claim.WriteOff != oldClaim.WriteOff) return true;
        if (claim.Radiographs != oldClaim.Radiographs) return true;
        if (claim.ClinicNum != oldClaim.ClinicNum) return true;
        if (claim.ClaimForm != oldClaim.ClaimForm) return true;
        if (claim.AttachedImages != oldClaim.AttachedImages) return true;
        if (claim.AttachedModels != oldClaim.AttachedModels) return true;
        if (claim.AttachedFlags != oldClaim.AttachedFlags) return true;
        if (claim.AttachmentID != oldClaim.AttachmentID) return true;
        if (claim.CanadianMaterialsForwarded != oldClaim.CanadianMaterialsForwarded) return true;
        if (claim.CanadianReferralProviderNum != oldClaim.CanadianReferralProviderNum) return true;
        if (claim.CanadianReferralReason != oldClaim.CanadianReferralReason) return true;
        if (claim.CanadianIsInitialLower != oldClaim.CanadianIsInitialLower) return true;
        if (claim.CanadianDateInitialLower.Date != oldClaim.CanadianDateInitialLower.Date) return true;
        if (claim.CanadianMandProsthMaterial != oldClaim.CanadianMandProsthMaterial) return true;
        if (claim.CanadianIsInitialUpper != oldClaim.CanadianIsInitialUpper) return true;
        if (claim.CanadianDateInitialUpper.Date != oldClaim.CanadianDateInitialUpper.Date) return true;
        if (claim.CanadianMaxProsthMaterial != oldClaim.CanadianMaxProsthMaterial) return true;
        if (claim.InsSubNum != oldClaim.InsSubNum) return true;
        if (claim.InsSubNum2 != oldClaim.InsSubNum2) return true;
        if (claim.CanadaTransRefNum != oldClaim.CanadaTransRefNum) return true;
        if (claim.CanadaEstTreatStartDate.Date != oldClaim.CanadaEstTreatStartDate.Date) return true;
        if (claim.CanadaInitialPayment != oldClaim.CanadaInitialPayment) return true;
        if (claim.CanadaPaymentMode != oldClaim.CanadaPaymentMode) return true;
        if (claim.CanadaTreatDuration != oldClaim.CanadaTreatDuration) return true;
        if (claim.CanadaNumAnticipatedPayments != oldClaim.CanadaNumAnticipatedPayments) return true;
        if (claim.CanadaAnticipatedPayAmount != oldClaim.CanadaAnticipatedPayAmount) return true;
        if (claim.PriorAuthorizationNumber != oldClaim.PriorAuthorizationNumber) return true;
        if (claim.SpecialProgramCode != oldClaim.SpecialProgramCode) return true;
        if (claim.UniformBillType != oldClaim.UniformBillType) return true;
        if (claim.MedType != oldClaim.MedType) return true;
        if (claim.AdmissionTypeCode != oldClaim.AdmissionTypeCode) return true;
        if (claim.AdmissionSourceCode != oldClaim.AdmissionSourceCode) return true;
        if (claim.PatientStatusCode != oldClaim.PatientStatusCode) return true;
        if (claim.CustomTracking != oldClaim.CustomTracking) return true;
        if (claim.DateResent.Date != oldClaim.DateResent.Date) return true;
        if (claim.CorrectionType != oldClaim.CorrectionType) return true;
        if (claim.ClaimIdentifier != oldClaim.ClaimIdentifier) return true;
        if (claim.OrigRefNum != oldClaim.OrigRefNum) return true;
        if (claim.ProvOrderOverride != oldClaim.ProvOrderOverride) return true;
        if (claim.OrthoTotalM != oldClaim.OrthoTotalM) return true;
        if (claim.ShareOfCost != oldClaim.ShareOfCost) return true;
        //SecUserNumEntry excluded from update
        //SecDateEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (claim.OrderingReferralNum != oldClaim.OrderingReferralNum) return true;
        if (claim.DateSentOrig.Date != oldClaim.DateSentOrig.Date) return true;
        if (claim.DateIllnessInjuryPreg.Date != oldClaim.DateIllnessInjuryPreg.Date) return true;
        if (claim.DateIllnessInjuryPregQualifier != oldClaim.DateIllnessInjuryPregQualifier) return true;
        if (claim.DateOther.Date != oldClaim.DateOther.Date) return true;
        if (claim.DateOtherQualifier != oldClaim.DateOtherQualifier) return true;
        if (claim.IsOutsideLab != oldClaim.IsOutsideLab) return true;
        if (claim.SecurityHash != oldClaim.SecurityHash) return true;
        if (claim.Narrative != oldClaim.Narrative) return true;
        return false;
    }

    public static void Delete(long claimNum)
    {
        ClearFkey(claimNum);
        var command = "DELETE FROM claim "
                      + "WHERE ClaimNum = " + SOut.Long(claimNum);
        Db.NonQ(command);
    }
    
    public static void ClearFkey(long claimNum)
    {
        if (claimNum == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey=" + SOut.Long(claimNum) + " AND PermType IN (95)";
        Db.NonQ(command);
    }
    
    public static void ClearFkey(List<long> listClaimNums)
    {
        if (listClaimNums == null || listClaimNums.FindAll(x => x != 0).Count == 0) return;
        var command = "UPDATE securitylog SET FKey=0 WHERE FKey IN(" + string.Join(",", listClaimNums.FindAll(x => x != 0)) + ") AND PermType IN (95)";
        Db.NonQ(command);
    }
}