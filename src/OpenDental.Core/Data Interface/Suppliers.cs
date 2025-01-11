using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Suppliers
{
    ///<summary>Gets all Suppliers.</summary>
    public static List<Supplier> GetAll()
    {
        var command = "SELECT * FROM supplier ORDER BY Name";
        return SupplierCrud.SelectMany(command);
    }

    ///<summary>Gets one Supplier by num.</summary>
    public static Supplier GetOne(long supplierNum)
    {
        return SupplierCrud.SelectOne(supplierNum);
    }

    
    public static long Insert(Supplier supplier)
    {
        return SupplierCrud.Insert(supplier);
    }

    
    public static void Update(Supplier supplier)
    {
        SupplierCrud.Update(supplier);
    }

    ///<summary>Surround with try-catch.</summary>
    public static void DeleteObject(Supplier supplier)
    {
        //validate that not already in use.
        var command = "SELECT COUNT(*) FROM supplyorder WHERE SupplierNum=" + SOut.Long(supplier.SupplierNum);
        var count = SIn.Int(Db.GetCount(command));
        if (count > 0) throw new ApplicationException(Lans.g("Supplies", "Supplier is already in use on an order. Not allowed to delete."));
        command = "SELECT COUNT(*) FROM supply WHERE SupplierNum=" + SOut.Long(supplier.SupplierNum);
        count = SIn.Int(Db.GetCount(command));
        if (count > 0) throw new ApplicationException(Lans.g("Supplies", "Supplier is already in use on a supply. Not allowed to delete."));
        SupplierCrud.Delete(supplier.SupplierNum);
    }

    public static string GetName(List<Supplier> listSuppliers, long supplierNum)
    {
        for (var i = 0; i < listSuppliers.Count; i++)
            if (listSuppliers[i].SupplierNum == supplierNum)
                return listSuppliers[i].Name;

        return "";
    }
}