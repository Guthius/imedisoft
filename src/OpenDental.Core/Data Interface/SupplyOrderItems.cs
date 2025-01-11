using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SupplyOrderItems
{
    ///<summary>Items in the table are not SupplyOrderItems. Includes CatalogNumber and Descript</summary>
    public static DataTable GetItemsInfoForOrder(long orderNum)
    {
        var command = "SELECT CatalogNumber,Descript,Qty,supplyorderitem.Price,SupplyOrderItemNum,supplyorderitem.SupplyNum,supplyorderitem.DateReceived "
                      + "FROM supplyorderitem,definition,supply "
                      + "WHERE definition.DefNum=supply.Category "
                      + "AND supply.SupplyNum=supplyorderitem.SupplyNum "
                      + "AND supplyorderitem.SupplyOrderNum=" + SOut.Long(orderNum) + " "
                      + "ORDER BY definition.ItemOrder,supply.ItemOrder";
        return DataCore.GetTable(command);
    }

    ///<summary>Gets all SupplyOrderItems for the passed in OrderNum</summary>
    public static List<SupplyOrderItem> GetSupplyItemsForOrder(long orderNum)
    {
        var command = "SELECT * FROM supplyorderitem WHERE SupplyOrderNum=" + SOut.Long(orderNum);
        return SupplyOrderItemCrud.SelectMany(command);
    }

    public static SupplyOrderItem SelectOne(long supplyOrderItemNum)
    {
        var command = "SELECT * FROM supplyorderitem WHERE SupplyOrderItemNum=" + SOut.Long(supplyOrderItemNum);
        return SupplyOrderItemCrud.SelectOne(command);
    }

    
    public static long Insert(SupplyOrderItem supplyOrderItem)
    {
        return SupplyOrderItemCrud.Insert(supplyOrderItem);
    }

    
    public static void Update(SupplyOrderItem supplyOrderItem)
    {
        SupplyOrderItemCrud.Update(supplyOrderItem);
    }

    ///<summary>Surround with try-catch.</summary>
    public static void DeleteObject(SupplyOrderItem supplyOrderItem)
    {
        //validate that not already in use.
        SupplyOrderItemCrud.Delete(supplyOrderItem.SupplyOrderItemNum);
    }
}