using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsVerifyHists
{
    #region Get Methods

    public static List<InsVerifyHist> GetForFKeyByType(long fKey, VerifyTypes verifyType)
    {
        var command = $"SELECT * FROM insverifyhist WHERE FKey={SOut.Long(fKey)} AND VerifyType={SOut.Int((int) verifyType)}";
        return InsVerifyHistCrud.SelectMany(command);
    }

    #endregion

    ///<summary>Gets one InsVerifyHist from the db.</summary>
    public static InsVerifyHist GetOne(long insVerifyHistNum)
    {
        return InsVerifyHistCrud.SelectOne(insVerifyHistNum);
    }

    
    public static long Insert(InsVerifyHist insVerifyHist)
    {
        return InsVerifyHistCrud.Insert(insVerifyHist);
    }

    
    public static void Delete(long insVerifyHistNum)
    {
        InsVerifyHistCrud.Delete(insVerifyHistNum);
    }

    /// <summary>
    ///     If the passed in InsVerify is null, do nothing.  Otherwise, insert the passed in InsVerify into InsVerifyHist
    ///     and blank out InsVerify's UserNum, Status, and Note. We blank out certain InsVerify data once the user has verified
    ///     because we reuse for the same InsPlan/PatPlan.
    /// </summary>
    public static void InsertFromInsVerify(InsVerify insVerify)
    {
        if (insVerify == null) return;
        var isInsVerifyFutureDateBenefit = PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear) && insVerify.VerifyType == VerifyTypes.InsuranceBenefit;
        var isInsVerifyFutureDatePatEnrollment = PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear) && insVerify.VerifyType == VerifyTypes.PatientEnrollment;
        if (insVerify.AppointmentDateTime > DateTime.MinValue)
            if (isInsVerifyFutureDateBenefit || isInsVerifyFutureDatePatEnrollment)
                insVerify.DateLastVerified = insVerify.AppointmentDateTime;

        Insert(new InsVerifyHist(insVerify));
        insVerify.UserNum = 0;
        insVerify.DefNum = 0;
        insVerify.Note = "";
        insVerify.DateLastAssigned = DateTime.MinValue;
        InsVerifies.Update(insVerify);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<InsVerifyHist> Refresh(long patNum){

        string command="SELECT * FROM insverifyhist WHERE PatNum = "+POut.Long(patNum);
        return Crud.InsVerifyHistCrud.SelectMany(command);
    }

    */
}