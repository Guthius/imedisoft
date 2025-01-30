using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PayorTypes
{
    public static List<PayorType> GetPatientData(long patNum)
    {
        return PayorTypeCrud.SelectMany("SELECT * FROM payortype WHERE PatNum = " + patNum + " ORDER BY DateStart");
    }

    public static string GetCurrentDescription(long patNum)
    {
        var payorType = GetCurrentType(patNum);
        if (payorType == null)
        {
            return "";
        }

        return payorType.SopCode + " - " + Sops.GetDescriptionFromCode(payorType.SopCode);
    }

    public static PayorType GetCurrentType(long patNum)
    {
        return PayorTypeCrud.SelectOne("SELECT * FROM payortype WHERE PatNum=" + patNum + " ORDER BY DateStart DESC LIMIT 1");
    }

    public static void Insert(PayorType payorType)
    {
        PayorTypeCrud.Insert(payorType);
    }

    public static void Update(PayorType payorType)
    {
        PayorTypeCrud.Update(payorType);
    }

    public static void Delete(long payorTypeNum)
    {
        Db.NonQ("DELETE FROM payortype WHERE PayorTypeNum = " + payorTypeNum);
    }
}