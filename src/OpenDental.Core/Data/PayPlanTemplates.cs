using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class PayPlanTemplates
{
    public static List<PayPlanTemplate> GetAll()
    {
        return PayPlanTemplateCrud.SelectMany("SELECT * FROM payplantemplate");
    }

    public static List<PayPlanTemplate> GetMany(long clinicNum = 0)
    {
        return PayPlanTemplateCrud.SelectMany("SELECT * FROM payplantemplate WHERE ClinicNum = " + clinicNum);
    }
    
    public static void Insert(PayPlanTemplate payPlanTemplate)
    {
        PayPlanTemplateCrud.Insert(payPlanTemplate);
    }
    
    public static void Update(PayPlanTemplate payPlanTemplate)
    {
        PayPlanTemplateCrud.Update(payPlanTemplate);
    }
}