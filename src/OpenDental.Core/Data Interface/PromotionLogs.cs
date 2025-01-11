using System.Collections.Generic;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class PromotionLogs
{
    public static List<PromotionLog> GetForPromotion(List<long> listPromotionNums)
    {
        if (listPromotionNums.IsNullOrEmpty()) return new List<PromotionLog>();

        return PromotionLogCrud.SelectMany($"SELECT * FROM promotionlog WHERE PromotionNum IN ({string.Join(",", listPromotionNums)})");
    }

    public static void Insert(PromotionLog promotionLog)
    {
        PromotionLogCrud.Insert(promotionLog);
    }

    public static void InsertMany(List<PromotionLog> listLogs)
    {
        if (listLogs.IsNullOrEmpty()) return;

        PromotionLogCrud.InsertMany(listLogs);
    }
}