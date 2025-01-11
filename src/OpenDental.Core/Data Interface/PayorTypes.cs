using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PayorTypes
{
    
    public static List<PayorType> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM payortype WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY DateStart";
        return PayorTypeCrud.SelectMany(command);
    }

    ///<summary>This will return "SopCode - Description" or empty string if the patient does not have a payor type entered.</summary>
    public static string GetCurrentDescription(long patNum)
    {
        var payorType = GetCurrentType(patNum);
        if (payorType == null) return "";
        return payorType.SopCode + " - " + Sops.GetDescriptionFromCode(payorType.SopCode);
    }

    ///<summary>Gets most recent PayorType for a patient.</summary>
    public static PayorType GetCurrentType(long patNum)
    {
        var command = DbHelper.LimitOrderBy("SELECT * FROM payortype WHERE PatNum=" + SOut.Long(patNum) + " ORDER BY DateStart DESC", 1);
        return PayorTypeCrud.SelectOne(command);
    }

    ///<summary>Gets one PayorType from the db.</summary>
    public static PayorType GetOne(long payorTypeNum)
    {
        return PayorTypeCrud.SelectOne(payorTypeNum);
    }

    
    public static long Insert(PayorType payorType)
    {
        return PayorTypeCrud.Insert(payorType);
    }

    
    public static void Update(PayorType payorType)
    {
        PayorTypeCrud.Update(payorType);
    }

    
    public static void Delete(long payorTypeNum)
    {
        var command = "DELETE FROM payortype WHERE PayorTypeNum = " + SOut.Long(payorTypeNum);
        Db.NonQ(command);
    }
}