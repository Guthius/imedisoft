using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SupplyOrders
{
    /*///<summary>Gets all SupplyOrders for one supplier, ordered by date.</summary>
    public static List<SupplyOrder> CreateObjects(long supplierNum) {

        string command="SELECT * FROM supplyorder "
            +"WHERE SupplierNum="+POut.Long(supplierNum)
            +" ORDER BY DatePlaced";
        return Crud.SupplyOrderCrud.SelectMany(command);
    }


    ///<summary>Gets all SupplyOrders, ordered by date.</summary>
    public static List<SupplyOrder> GetAll() {

        string command="SELECT * FROM supplyorder ORDER BY DatePlaced";
        return Crud.SupplyOrderCrud.SelectMany(command);
    }*/

    ///<summary>Use supplierNum=0 for all suppliers.</summary>
    public static List<SupplyOrder> GetList(long supplierNum)
    {
        var command = "SELECT * FROM supplyorder ";
        if (supplierNum > 0) command += "WHERE SupplierNum=" + SOut.Long(supplierNum) + " ";
        command += "ORDER BY DatePlaced";
        return SupplyOrderCrud.SelectMany(command);
    }

    
    public static long Insert(SupplyOrder supplyOrder)
    {
        return SupplyOrderCrud.Insert(supplyOrder);
    }

    
    public static void Update(SupplyOrder supplyOrder)
    {
        SupplyOrderCrud.Update(supplyOrder);
    }

    ///<summary>No need to surround with try-catch.  Also deletes supplyOrderItems.</summary>
    public static void DeleteObject(SupplyOrder supplyOrder)
    {
        //validate that not already in use-no
        //delete associated orderItems
        var command = "DELETE FROM supplyorderitem WHERE SupplyOrderNum=" + SOut.Long(supplyOrder.SupplyOrderNum);
        Db.NonQ(command);
        SupplyOrderCrud.Delete(supplyOrder.SupplyOrderNum);
    }

    //Retotals all items attached to order and updates AmountTotal.
    public static SupplyOrder UpdateOrderPrice(long orderNum)
    {
        var command = "SELECT SUM(Qty*Price) FROM supplyorderitem WHERE SupplyOrderNum=" + orderNum;
        var amountTotal = SIn.Double(DataCore.GetScalar(command));
        command = "SELECT * FROM supplyorder WHERE SupplyOrderNum=" + orderNum;
        var supplyOrder = SupplyOrderCrud.SelectOne(command);
        supplyOrder.AmountTotal = amountTotal;
        Update(supplyOrder);
        return supplyOrder;
    }
}