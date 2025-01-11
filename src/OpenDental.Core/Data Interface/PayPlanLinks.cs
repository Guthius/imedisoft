using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PayPlanLinks
{
    #region Modification Methods

    /// <summary>
    ///     Inserts, updates, or deletes database rows to match supplied list.  Passed in list contains current list and
    ///     payPlanNum gets current
    ///     list from DB.
    /// </summary>
    public static void Sync(List<PayPlanLink> listPayPlanLinks, long payPlanNum)
    {
        var listDB = GetListForPayplan(payPlanNum);
        PayPlanLinkCrud.Sync(listPayPlanLinks, listDB);
    }

    #endregion Modification Methods

    #region Insert

    
    public static long Insert(PayPlanLink payPlanLink)
    {
        return PayPlanLinkCrud.Insert(payPlanLink);
    }

    #endregion Insert

    #region Update

    
    public static void Update(PayPlanLink payPlanLink)
    {
        PayPlanLinkCrud.Update(payPlanLink);
    }

    #endregion Update

    #region Delete

    
    public static void Delete(long payPlanLinkNum)
    {
        PayPlanLinkCrud.Delete(payPlanLinkNum);
    }

    #endregion Delete

    #region Get Methods

    /*
    ///<summary>Not used. There is no PatNum column in the PayPlanLink table.</summary>
    public static List<PayPlanLink> Refresh(long patNum){

        string command="SELECT * FROM payplanlink WHERE PatNum = "+POut.Long(patNum);
        return Crud.PayPlanLinkCrud.SelectMany(command);
    }*/

    public static List<PayPlanLink> GetListForPayplan(long payplanNum)
    {
        var command = $"SELECT * FROM payplanlink WHERE PayPlanNum={SOut.Long(payplanNum)}";
        return PayPlanLinkCrud.SelectMany(command);
    }

    public static List<long> GetListForLinkTypeAndFKeys(PayPlanLinkType linkType, List<long> listFKeys)
    {
        var command = $"SELECT FKey FROM payplanlink WHERE LinkType={SOut.Int((int) linkType)}";
        if (!listFKeys.IsNullOrEmpty()) command += $" AND FKey IN ({string.Join(",", listFKeys.Select(x => SOut.Long(x)))})";
        return Db.GetListLong(command);
    }

    public static List<PayPlanLink> GetForPayPlans(List<long> listPayPlans)
    {
        if (listPayPlans.IsNullOrEmpty()) return new List<PayPlanLink>();

        var command = $"SELECT * FROM payplanlink WHERE PayPlanNum IN ({string.Join(",", listPayPlans.Select(x => SOut.Long(x)))}) ";
        return PayPlanLinkCrud.SelectMany(command);
    }

    ///<summary>Gets one PayPlanLink from the db.</summary>
    public static PayPlanLink GetOne(long payPlanLinkNum)
    {
        return PayPlanLinkCrud.SelectOne(payPlanLinkNum);
    }

    ///<summary>Gets all of the payplanlink entries for the given fKey and linkType.</summary>
    public static List<PayPlanLink> GetForFKeyAndLinkType(long fKey, PayPlanLinkType linkType)
    {
        return GetForFKeysAndLinkType(new List<long> {fKey}, linkType);
    }

    ///<summary>Gets all of the payplanlink entries for the given fKey and linkType.</summary>
    public static List<PayPlanLink> GetForFKeysAndLinkType(List<long> listFKeys, PayPlanLinkType linkType)
    {
        if (listFKeys.IsNullOrEmpty()) return new List<PayPlanLink>();

        var command = $"SELECT * FROM payplanlink WHERE payplanlink.FKey IN ({string.Join(",", listFKeys.Select(x => SOut.Long(x)))}) " +
                      $"AND payplanlink.LinkType={SOut.Int((int) linkType)} ";
        return PayPlanLinkCrud.SelectMany(command);
    }

    ///<summary>Returns all procedure links for the list of PayPlanNums.</summary>
    public static List<PayPlanLink> GetForPayPlansAndLinkType(List<long> listPayPlanNums, PayPlanLinkType linkType)
    {
        if (listPayPlanNums.Count == 0) return new List<PayPlanLink>();

        var command = $"SELECT * FROM payplanlink WHERE payplanlink.PayPlanNum IN ({string.Join(",", listPayPlanNums.Select(x => SOut.Long(x)))}) " +
                      $"AND payplanlink.LinkType={SOut.Int((int) linkType)}";
        return PayPlanLinkCrud.SelectMany(command);
    }

    #endregion Get Methods
}