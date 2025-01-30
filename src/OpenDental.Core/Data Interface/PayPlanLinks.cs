using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PayPlanLinks
{
    public static void Sync(List<PayPlanLink> listPayPlanLinks, long payPlanNum)
    {
        var listDB = GetListForPayplan(payPlanNum);
        PayPlanLinkCrud.Sync(listPayPlanLinks, listDB);
    }

    public static void Insert(PayPlanLink payPlanLink)
    {
        PayPlanLinkCrud.Insert(payPlanLink);
    }

    public static void Delete(long payPlanLinkNum)
    {
        PayPlanLinkCrud.Delete(payPlanLinkNum);
    }

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

    public static List<PayPlanLink> GetForFKeyAndLinkType(long fKey, PayPlanLinkType linkType)
    {
        return GetForFKeysAndLinkType(new List<long> {fKey}, linkType);
    }

    public static List<PayPlanLink> GetForFKeysAndLinkType(List<long> listFKeys, PayPlanLinkType linkType)
    {
        if (listFKeys.IsNullOrEmpty()) return new List<PayPlanLink>();

        var command = $"SELECT * FROM payplanlink WHERE payplanlink.FKey IN ({string.Join(",", listFKeys.Select(x => SOut.Long(x)))}) " +
                      $"AND payplanlink.LinkType={SOut.Int((int) linkType)} ";
        return PayPlanLinkCrud.SelectMany(command);
    }

    public static List<PayPlanLink> GetForPayPlansAndLinkType(List<long> listPayPlanNums, PayPlanLinkType linkType)
    {
        if (listPayPlanNums.Count == 0) return new List<PayPlanLink>();

        var command = $"SELECT * FROM payplanlink WHERE payplanlink.PayPlanNum IN ({string.Join(",", listPayPlanNums.Select(x => SOut.Long(x)))}) " +
                      $"AND payplanlink.LinkType={SOut.Int((int) linkType)}";
        return PayPlanLinkCrud.SelectMany(command);
    }
}