using System.Collections.Generic;
using System.Globalization;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PayPlanTemplates
{
    #region Methods - Misc

    /// <summary>Gets all of the templates from the db. Convertes them into Terms objects and then returns a list of terms. </summary>
    public static PayPlanTerms ConvertTemplateToTerms(PayPlanTemplate payPlanTemplate)
    {
        var payPlanTerms = new PayPlanTerms();
        payPlanTerms.APR = payPlanTemplate.APR;
        payPlanTerms.PeriodPayment = (decimal) payPlanTemplate.PayAmt;
        payPlanTerms.PayCount = payPlanTemplate.NumberOfPayments; //This may need to be removed.
        payPlanTerms.RoundDec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalDigits;
        payPlanTerms.Frequency = payPlanTemplate.ChargeFrequency;
        payPlanTerms.DownPayment = payPlanTemplate.DownPayment;
        payPlanTerms.DynamicPayPlanTPOption = payPlanTemplate.DynamicPayPlanTPOption;
        payPlanTerms.SheetDefNum = payPlanTemplate.SheetDefNum;
        return payPlanTerms;
    }

    #endregion Methods - Misc

    #region Methods - Get

    ///<summary>Gets all PayPlanTemplates from the db.</summary>
    public static List<PayPlanTemplate> GetAll()
    {
        var command = "SELECT * FROM payplantemplate";
        return PayPlanTemplateCrud.SelectMany(command);
    }

    public static List<PayPlanTemplate> GetMany(long clinicNum = 0)
    {
        var command = "SELECT * FROM payplantemplate WHERE ClinicNum = " + SOut.Long(clinicNum);
        return PayPlanTemplateCrud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(PayPlanTemplate payPlanTemplate)
    {
        return PayPlanTemplateCrud.Insert(payPlanTemplate);
    }

    
    public static void Update(PayPlanTemplate payPlanTemplate)
    {
        PayPlanTemplateCrud.Update(payPlanTemplate);
    }

    #endregion Methods - Modify

    //Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    //#region Methods - Get
    //
    //public static List<PayPlanTemplate> Refresh(long patNum){
    //	
    //	string command="SELECT * FROM payplantemplate WHERE PatNum = "+POut.Long(patNum);
    //	return Crud.PayPlanTemplateCrud.SelectMany(command);
    //}

    /////<summary>Gets one PayPlanTemplate from the db.</summary>
    //public static PayPlanTemplate GetOne(long payPlanTemplateNum){
    //	
    //	return Crud.PayPlanTemplateCrud.SelectOne(payPlanTemplateNum);
    //}
    //#endregion Methods - Get

    //#region Methods - Modify
    //
    //public static void Delete(long payPlanTemplateNum) {
    //	
    //	Crud.PayPlanTemplateCrud.Delete(payPlanTemplateNum);
    //}
    //#endregion Methods - Modify
}