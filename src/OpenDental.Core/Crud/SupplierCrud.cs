#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SupplierCrud
{
    public static Supplier SelectOne(long supplierNum)
    {
        var command = "SELECT * FROM supplier "
                      + "WHERE SupplierNum = " + SOut.Long(supplierNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Supplier SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Supplier> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Supplier> TableToList(DataTable table)
    {
        var retVal = new List<Supplier>();
        Supplier supplier;
        foreach (DataRow row in table.Rows)
        {
            supplier = new Supplier();
            supplier.SupplierNum = SIn.Long(row["SupplierNum"].ToString());
            supplier.Name = SIn.String(row["Name"].ToString());
            supplier.Phone = SIn.String(row["Phone"].ToString());
            supplier.CustomerId = SIn.String(row["CustomerId"].ToString());
            supplier.Website = SIn.String(row["Website"].ToString());
            supplier.UserName = SIn.String(row["UserName"].ToString());
            supplier.Password = SIn.String(row["Password"].ToString());
            supplier.Note = SIn.String(row["Note"].ToString());
            retVal.Add(supplier);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Supplier> listSuppliers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Supplier";
        var table = new DataTable(tableName);
        table.Columns.Add("SupplierNum");
        table.Columns.Add("Name");
        table.Columns.Add("Phone");
        table.Columns.Add("CustomerId");
        table.Columns.Add("Website");
        table.Columns.Add("UserName");
        table.Columns.Add("Password");
        table.Columns.Add("Note");
        foreach (var supplier in listSuppliers)
            table.Rows.Add(SOut.Long(supplier.SupplierNum), supplier.Name, supplier.Phone, supplier.CustomerId, supplier.Website, supplier.UserName, supplier.Password, supplier.Note);
        return table;
    }

    public static long Insert(Supplier supplier)
    {
        return Insert(supplier, false);
    }

    public static long Insert(Supplier supplier, bool useExistingPK)
    {
        var command = "INSERT INTO supplier (";

        command += "Name,Phone,CustomerId,Website,UserName,Password,Note) VALUES(";

        command +=
            "'" + SOut.String(supplier.Name) + "',"
            + "'" + SOut.String(supplier.Phone) + "',"
            + "'" + SOut.String(supplier.CustomerId) + "',"
            + DbHelper.ParamChar + "paramWebsite,"
            + "'" + SOut.String(supplier.UserName) + "',"
            + "'" + SOut.String(supplier.Password) + "',"
            + DbHelper.ParamChar + "paramNote)";
        if (supplier.Website == null) supplier.Website = "";
        var paramWebsite = new OdSqlParameter("paramWebsite", OdDbType.Text, SOut.StringParam(supplier.Website));
        if (supplier.Note == null) supplier.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplier.Note));
        {
            supplier.SupplierNum = Db.NonQ(command, true, "SupplierNum", "supplier", paramWebsite, paramNote);
        }
        return supplier.SupplierNum;
    }

    public static long InsertNoCache(Supplier supplier)
    {
        return InsertNoCache(supplier, false);
    }

    public static long InsertNoCache(Supplier supplier, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO supplier (";
        if (isRandomKeys || useExistingPK) command += "SupplierNum,";
        command += "Name,Phone,CustomerId,Website,UserName,Password,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(supplier.SupplierNum) + ",";
        command +=
            "'" + SOut.String(supplier.Name) + "',"
            + "'" + SOut.String(supplier.Phone) + "',"
            + "'" + SOut.String(supplier.CustomerId) + "',"
            + DbHelper.ParamChar + "paramWebsite,"
            + "'" + SOut.String(supplier.UserName) + "',"
            + "'" + SOut.String(supplier.Password) + "',"
            + DbHelper.ParamChar + "paramNote)";
        if (supplier.Website == null) supplier.Website = "";
        var paramWebsite = new OdSqlParameter("paramWebsite", OdDbType.Text, SOut.StringParam(supplier.Website));
        if (supplier.Note == null) supplier.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplier.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramWebsite, paramNote);
        else
            supplier.SupplierNum = Db.NonQ(command, true, "SupplierNum", "supplier", paramWebsite, paramNote);
        return supplier.SupplierNum;
    }

    public static void Update(Supplier supplier)
    {
        var command = "UPDATE supplier SET "
                      + "Name       = '" + SOut.String(supplier.Name) + "', "
                      + "Phone      = '" + SOut.String(supplier.Phone) + "', "
                      + "CustomerId = '" + SOut.String(supplier.CustomerId) + "', "
                      + "Website    =  " + DbHelper.ParamChar + "paramWebsite, "
                      + "UserName   = '" + SOut.String(supplier.UserName) + "', "
                      + "Password   = '" + SOut.String(supplier.Password) + "', "
                      + "Note       =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE SupplierNum = " + SOut.Long(supplier.SupplierNum);
        if (supplier.Website == null) supplier.Website = "";
        var paramWebsite = new OdSqlParameter("paramWebsite", OdDbType.Text, SOut.StringParam(supplier.Website));
        if (supplier.Note == null) supplier.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplier.Note));
        Db.NonQ(command, paramWebsite, paramNote);
    }

    public static bool Update(Supplier supplier, Supplier oldSupplier)
    {
        var command = "";
        if (supplier.Name != oldSupplier.Name)
        {
            if (command != "") command += ",";
            command += "Name = '" + SOut.String(supplier.Name) + "'";
        }

        if (supplier.Phone != oldSupplier.Phone)
        {
            if (command != "") command += ",";
            command += "Phone = '" + SOut.String(supplier.Phone) + "'";
        }

        if (supplier.CustomerId != oldSupplier.CustomerId)
        {
            if (command != "") command += ",";
            command += "CustomerId = '" + SOut.String(supplier.CustomerId) + "'";
        }

        if (supplier.Website != oldSupplier.Website)
        {
            if (command != "") command += ",";
            command += "Website = " + DbHelper.ParamChar + "paramWebsite";
        }

        if (supplier.UserName != oldSupplier.UserName)
        {
            if (command != "") command += ",";
            command += "UserName = '" + SOut.String(supplier.UserName) + "'";
        }

        if (supplier.Password != oldSupplier.Password)
        {
            if (command != "") command += ",";
            command += "Password = '" + SOut.String(supplier.Password) + "'";
        }

        if (supplier.Note != oldSupplier.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (supplier.Website == null) supplier.Website = "";
        var paramWebsite = new OdSqlParameter("paramWebsite", OdDbType.Text, SOut.StringParam(supplier.Website));
        if (supplier.Note == null) supplier.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(supplier.Note));
        command = "UPDATE supplier SET " + command
                                         + " WHERE SupplierNum = " + SOut.Long(supplier.SupplierNum);
        Db.NonQ(command, paramWebsite, paramNote);
        return true;
    }

    public static bool UpdateComparison(Supplier supplier, Supplier oldSupplier)
    {
        if (supplier.Name != oldSupplier.Name) return true;
        if (supplier.Phone != oldSupplier.Phone) return true;
        if (supplier.CustomerId != oldSupplier.CustomerId) return true;
        if (supplier.Website != oldSupplier.Website) return true;
        if (supplier.UserName != oldSupplier.UserName) return true;
        if (supplier.Password != oldSupplier.Password) return true;
        if (supplier.Note != oldSupplier.Note) return true;
        return false;
    }

    public static void Delete(long supplierNum)
    {
        var command = "DELETE FROM supplier "
                      + "WHERE SupplierNum = " + SOut.Long(supplierNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSupplierNums)
    {
        if (listSupplierNums == null || listSupplierNums.Count == 0) return;
        var command = "DELETE FROM supplier "
                      + "WHERE SupplierNum IN(" + string.Join(",", listSupplierNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}