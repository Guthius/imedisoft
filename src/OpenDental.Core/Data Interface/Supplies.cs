using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Supplies
{
	/// <summary>
	///     Gets all Supplies, ordered by category and itemOrder.  Use listCategories=null to indicate all not hidden.
	///     Use listSupplierNums=null to indicate all suppliers.
	/// </summary>
	public static List<Supply> GetList(List<long> listSupplierNums, bool showHidden, string textFind, List<long> listCategories)
    {
        var command = "SELECT supply.* "
                      + "FROM supply,definition "
                      + "WHERE definition.DefNum=supply.Category ";
        if (listSupplierNums != null) command += "AND SupplierNum IN (" + string.Join(",", listSupplierNums.ConvertAll(x => x.ToString())) + ") ";
        if (!showHidden) command += "AND supply.IsHidden=0 ";
        if (textFind != "")
            command += "AND (supply.Descript LIKE '%" + SOut.String(textFind) + "%' "
                       + "OR supply.CatalogNumber LIKE '%" + SOut.String(textFind) + "%' "
                       + ")";
        if (listCategories != null && listCategories.Count > 0) command += "AND supply.Category IN (" + string.Join(",", listCategories.ConvertAll(x => x.ToString())) + ") ";
        command += "ORDER BY definition.ItemOrder,supply.ItemOrder";
        return SupplyCrud.SelectMany(command);
    }

    ///<Summary>Gets one supply from the database.  Used for display in SupplyOrderItemEdit window.</Summary>
    public static Supply GetSupply(long supplyNum)
    {
        return SupplyCrud.SelectOne(supplyNum);
    }

    
    public static long Insert(Supply supply)
    {
        return SupplyCrud.Insert(supply);
    }

    
    public static void Update(Supply supply)
    {
        SupplyCrud.Update(supply);
    }

    ///<summary>Surround with try-catch.  Handles ItemOrders below this supply.</summary>
    public static void DeleteObject(Supply supply)
    {
        //validate that not already in use.
        var command = "SELECT COUNT(*) FROM supplyorderitem WHERE SupplyNum=" + SOut.Long(supply.SupplyNum);
        var count = SIn.Int(Db.GetCount(command));
        if (count > 0) throw new ApplicationException(Lans.g("Supplies", "Supply is already in use on an order. Not allowed to delete."));
        SupplyCrud.Delete(supply.SupplyNum);
        command = "UPDATE supply SET ItemOrder=(ItemOrder-1) WHERE Category=" + SOut.Long(supply.Category) + " AND ItemOrder>" + SOut.Int(supply.ItemOrder);
        Db.NonQ(command);
    }

    ///<summary>Uses single query to subtract a count, typically 1, from each supply in the list.</summary>
    public static void OrderSubtract(List<long> listSupplyNums, int countMove)
    {
        if (listSupplyNums.Count == 0) return;
        var command = "UPDATE supply SET ItemOrder=(ItemOrder-" + countMove + ") WHERE SupplyNum IN("
                      + string.Join(",", listSupplyNums.ConvertAll(x => x.ToString())) + ")";
        Db.NonQ(command);
    }

    ///<summary>Uses single query to add a count, typically 1, to each supply in the list.</summary>
    public static void OrderAdd(List<long> listSupplyNums, int countMove)
    {
        if (listSupplyNums.Count == 0) return;
        var command = "UPDATE supply SET ItemOrder=(ItemOrder+" + countMove + ") WHERE SupplyNum IN("
                      + string.Join(",", listSupplyNums.ConvertAll(x => x.ToString())) + ")";
        Db.NonQ(command);
    }

    ///<summary>Uses single query to add one to each supply.ItemOrder that has an itemOrder >= than specified.</summary>
    public static void OrderAddOneGreater(int itemOrder, long category, long supplyNumExclude)
    {
        var command = "UPDATE supply SET ItemOrder=(ItemOrder+1) "
                      + "WHERE ItemOrder >= " + SOut.Int(itemOrder)
                      + " AND Category=" + SOut.Long(category)
                      + " AND SupplyNum !=" + SOut.Long(supplyNumExclude);
        Db.NonQ(command);
    }

    ///<summary>Gets the last ItemOrder for a category.  -1 if none in that category yet.</summary>
    public static int GetLastItemOrder(long category)
    {
        var command = "SELECT MAX(ItemOrder) FROM supply "
                      + "WHERE Category=" + SOut.Long(category);
        var result = DataCore.GetScalar(command);
        if (result == "") return -1;
        return SIn.Int(result);
    }
}