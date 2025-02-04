using System;
using System.Collections.Generic;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SheetParameter
{
    public bool IsRequired;
    public string ParamName;
    public object ParamValue;

    public SheetParameter Copy()
    {
        return (SheetParameter) MemberwiseClone();
    }

    public SheetParameter()
    {
        IsRequired = false;
        ParamName = "";
    }

    public SheetParameter(bool isRequired, string paramName)
    {
        IsRequired = isRequired;
        ParamName = paramName;
    }

    public SheetParameter(bool isRequired, string paramName, string paramValue)
    {
        IsRequired = isRequired;
        ParamName = paramName;
        ParamValue = paramValue;
    }

    public static List<SheetParameter> GetForType(SheetTypeEnum sheetType)
    {
        var parameters = new List<SheetParameter>();
        switch (sheetType)
        {
            case SheetTypeEnum.LabelPatient:
                parameters.Add(new SheetParameter(true, "PatNum"));
                break;
            
            case SheetTypeEnum.LabelCarrier:
                parameters.Add(new SheetParameter(true, "CarrierNum"));
                break;
            
            case SheetTypeEnum.LabelReferral:
                parameters.Add(new SheetParameter(true, "ReferralNum"));
                break;
            
            case SheetTypeEnum.ReferralSlip:
                parameters.Add(new SheetParameter(true, "PatNum"));
                parameters.Add(new SheetParameter(true, "ReferralNum"));
                break;
            
            case SheetTypeEnum.LabelAppointment:
                parameters.Add(new SheetParameter(true, "AptNum"));
                break;
            
            case SheetTypeEnum.Consent:
                parameters.Add(new SheetParameter(true, "PatNum"));
                parameters.Add(new SheetParameter(false, "ListProcNums"));
                break;
            
            case SheetTypeEnum.PatientLetter:
                parameters.Add(new SheetParameter(true, "PatNum"));
                parameters.Add(new SheetParameter(false, "AptNum"));
                parameters.Add(new SheetParameter(false, "ListProcNums"));
                break;
            
            case SheetTypeEnum.ReferralLetter:
                parameters.Add(new SheetParameter(true, "PatNum"));
                parameters.Add(new SheetParameter(true, "ReferralNum"));
                parameters.Add(new SheetParameter(false, "CompletedProcs"));
                parameters.Add(new SheetParameter(false, "toothChartImg"));
                parameters.Add(new SheetParameter(false, "AptNum"));
                parameters.Add(new SheetParameter(false, "ListProcNums"));
                break;
            
            case SheetTypeEnum.PatientForm:
                parameters.Add(new SheetParameter(true, "PatNum"));
                parameters.Add(new SheetParameter(false, "ListProcNums"));
                break;
            
            case SheetTypeEnum.RoutingSlip:
                parameters.Add(new SheetParameter(true, "AptNum"));
                break;
            
            case SheetTypeEnum.MedicalHistory:
                parameters.Add(new SheetParameter(true, "PatNum"));
                break;
            
            case SheetTypeEnum.LabSlip:
                parameters.Add(new SheetParameter(true, "PatNum"));
                parameters.Add(new SheetParameter(true, "LabCaseNum"));
                break;
            
            case SheetTypeEnum.ExamSheet:
                parameters.Add(new SheetParameter(true, "PatNum"));
                break;
            
            case SheetTypeEnum.DepositSlip:
                parameters.Add(new SheetParameter(true, "DepositNum"));
                break;
            
            case SheetTypeEnum.PaymentPlan:
                parameters.Add(new SheetParameter(false, "keyData"));
                break;
            
            case SheetTypeEnum.ERA:
                parameters.Add(new SheetParameter(true, "ERA"));
                parameters.Add(new SheetParameter(false, "IsSingleClaimPaid"));
                break;
            
            case SheetTypeEnum.ERAGridHeader:
                parameters.Add(new SheetParameter(true, "EraClaimPaid"));
                parameters.Add(new SheetParameter(true, "ClaimIndexNum"));
                break;
            
            case SheetTypeEnum.TreatmentPlan:
            case SheetTypeEnum.Statement:
            case SheetTypeEnum.MedLabResults:
            default:
                break;
        }

        return parameters;
    }

    public static void SetParameter(Sheet sheet, string paramName, object paramValue)
    {
        var param = GetParamByName(sheet.Parameters, paramName);
        if (param == null)
        {
            throw new ApplicationException(Lans.g("Sheet", "Parameter not found: ") + paramName);
        }

        param.ParamValue = paramValue;
    }

    public static SheetParameter GetParamByName(List<SheetParameter> parameters, string paramName)
    {
        foreach (var param in parameters)
        {
            if (param.ParamName == paramName)
            {
                return param;
            }
        }

        return null;
    }
}