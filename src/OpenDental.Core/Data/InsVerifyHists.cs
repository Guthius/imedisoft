using System;
using System.Collections.Generic;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class InsVerifyHists
{
    public static List<InsVerifyHist> GetForFKeyByType(long fKey, VerifyTypes verifyType)
    {
        return InsVerifyHistCrud.SelectMany($"SELECT * FROM insverifyhist WHERE FKey={fKey} AND VerifyType={(int) verifyType}");
    }

    public static void Insert(InsVerifyHist insVerifyHist)
    {
        InsVerifyHistCrud.Insert(insVerifyHist);
    }

    public static void InsertFromInsVerify(InsVerify insVerify)
    {
        if (insVerify == null)
        {
            return;
        }

        var isInsVerifyFutureDateBenefit = PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear) && insVerify.VerifyType == VerifyTypes.InsuranceBenefit;
        var isInsVerifyFutureDatePatEnrollment = PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear) && insVerify.VerifyType == VerifyTypes.PatientEnrollment;

        if (insVerify.AppointmentDateTime > DateTime.MinValue)
        {
            if (isInsVerifyFutureDateBenefit || isInsVerifyFutureDatePatEnrollment)
            {
                insVerify.DateLastVerified = insVerify.AppointmentDateTime;
            }
        }

        Insert(new InsVerifyHist(insVerify));

        insVerify.UserNum = 0;
        insVerify.DefNum = 0;
        insVerify.Note = "";
        insVerify.DateLastAssigned = DateTime.MinValue;

        InsVerifies.Update(insVerify);
    }
}