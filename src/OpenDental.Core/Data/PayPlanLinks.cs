using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PayPlanLinks
{
    public static void Sync(List<PayPlanLink> listPayPlanLinks, long payPlanNum)
    {
        var payPlanLinks = GetListForPayplan(payPlanNum);

        PayPlanLinkCrud.Sync(listPayPlanLinks, payPlanLinks);
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
        return PayPlanLinkCrud.SelectMany($"SELECT * FROM payplanlink WHERE PayPlanNum = {payplanNum}");
    }

    public static List<long> GetListForLinkTypeAndFKeys(PayPlanLinkType linkType, List<long> listFKeys)
    {
        var commandText = $"SELECT FKey FROM payplanlink WHERE LinkType={SOut.Int((int) linkType)}";

        if (!listFKeys.IsNullOrEmpty())
        {
            commandText += $" AND FKey IN ({string.Join(",", listFKeys.Select(x => x))})";
        }

        return Db.GetListLong(commandText);
    }

    public static List<PayPlanLink> GetForPayPlans(List<long> payPlanNums)
    {
        return payPlanNums.IsNullOrEmpty() ? [] : PayPlanLinkCrud.SelectMany($"SELECT * FROM payplanlink WHERE PayPlanNum IN ({string.Join(", ", payPlanNums)})");
    }

    public static List<PayPlanLink> GetForFKeyAndLinkType(long fKey, PayPlanLinkType linkType)
    {
        return GetForFKeysAndLinkType([fKey], linkType);
    }

    public static List<PayPlanLink> GetForFKeysAndLinkType(List<long> fkeys, PayPlanLinkType linkType)
    {
        if (fkeys.IsNullOrEmpty())
        {
            return [];
        }

        return PayPlanLinkCrud.SelectMany(
            $"""
             SELECT * FROM payplanlink 
             WHERE FKey IN ({string.Join(", ", fkeys)}) 
             AND LinkType = {(int) linkType} 
             """);
    }

    public static List<PayPlanLink> GetForPayPlansAndLinkType(List<long> payPlanNums, PayPlanLinkType linkType)
    {
        if (payPlanNums.Count == 0)
        {
            return [];
        }

        return PayPlanLinkCrud.SelectMany(
            $"""
             SELECT * FROM payplanlink 
             WHERE PayPlanNum IN ({string.Join(", ", payPlanNums)}) 
             AND LinkType = {(int) linkType}
             """);
    }
}