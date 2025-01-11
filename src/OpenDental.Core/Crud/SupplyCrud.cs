#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SupplyCrud
{
    public static Supply SelectOne(long supplyNum)
    {
        var command = "SELECT * FROM supply "
                      + "WHERE SupplyNum = " + SOut.Long(supplyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Supply SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Supply> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Supply> TableToList(DataTable table)
    {
        var retVal = new List<Supply>();
        Supply supply;
        foreach (DataRow row in table.Rows)
        {
            supply = new Supply();
            supply.SupplyNum = SIn.Long(row["SupplyNum"].ToString());
            supply.SupplierNum = SIn.Long(row["SupplierNum"].ToString());
            supply.CatalogNumber = SIn.String(row["CatalogNumber"].ToString());
            supply.Descript = SIn.String(row["Descript"].ToString());
            supply.Category = SIn.Long(row["Category"].ToString());
            supply.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            supply.LevelDesired = SIn.Float(row["LevelDesired"].ToString());
            supply.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            supply.Price = SIn.Double(row["Price"].ToString());
            supply.BarCodeOrID = SIn.String(row["BarCodeOrID"].ToString());
            supply.DispDefaultQuant = SIn.Float(row["DispDefaultQuant"].ToString());
            supply.DispUnitsCount = SIn.Int(row["DispUnitsCount"].ToString());
            supply.DispUnitDesc = SIn.String(row["DispUnitDesc"].ToString());
            supply.LevelOnHand = SIn.Float(row["LevelOnHand"].ToString());
            supply.OrderQty = SIn.Int(row["OrderQty"].ToString());
            retVal.Add(supply);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Supply> listSupplys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Supply";
        var table = new DataTable(tableName);
        table.Columns.Add("SupplyNum");
        table.Columns.Add("SupplierNum");
        table.Columns.Add("CatalogNumber");
        table.Columns.Add("Descript");
        table.Columns.Add("Category");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("LevelDesired");
        table.Columns.Add("IsHidden");
        table.Columns.Add("Price");
        table.Columns.Add("BarCodeOrID");
        table.Columns.Add("DispDefaultQuant");
        table.Columns.Add("DispUnitsCount");
        table.Columns.Add("DispUnitDesc");
        table.Columns.Add("LevelOnHand");
        table.Columns.Add("OrderQty");
        foreach (var supply in listSupplys)
            table.Rows.Add(SOut.Long(supply.SupplyNum), SOut.Long(supply.SupplierNum), supply.CatalogNumber, supply.Descript, SOut.Long(supply.Category), SOut.Int(supply.ItemOrder), SOut.Float(supply.LevelDesired), SOut.Bool(supply.IsHidden), SOut.Double(supply.Price), supply.BarCodeOrID, SOut.Float(supply.DispDefaultQuant), SOut.Int(supply.DispUnitsCount), supply.DispUnitDesc, SOut.Float(supply.LevelOnHand), SOut.Int(supply.OrderQty));
        return table;
    }

    public static long Insert(Supply supply)
    {
        return Insert(supply, false);
    }

    public static long Insert(Supply supply, bool useExistingPK)
    {
        var command = "INSERT INTO supply (";

        command += "SupplierNum,CatalogNumber,Descript,Category,ItemOrder,LevelDesired,IsHidden,Price,BarCodeOrID,DispDefaultQuant,DispUnitsCount,DispUnitDesc,LevelOnHand,OrderQty) VALUES(";

        command +=
            SOut.Long(supply.SupplierNum) + ","
                                          + "'" + SOut.String(supply.CatalogNumber) + "',"
                                          + "'" + SOut.String(supply.Descript) + "',"
                                          + SOut.Long(supply.Category) + ","
                                          + SOut.Int(supply.ItemOrder) + ","
                                          + SOut.Float(supply.LevelDesired) + ","
                                          + SOut.Bool(supply.IsHidden) + ","
                                          + SOut.Double(supply.Price) + ","
                                          + "'" + SOut.String(supply.BarCodeOrID) + "',"
                                          + SOut.Float(supply.DispDefaultQuant) + ","
                                          + SOut.Int(supply.DispUnitsCount) + ","
                                          + "'" + SOut.String(supply.DispUnitDesc) + "',"
                                          + SOut.Float(supply.LevelOnHand) + ","
                                          + SOut.Int(supply.OrderQty) + ")";
        {
            supply.SupplyNum = Db.NonQ(command, true, "SupplyNum", "supply");
        }
        return supply.SupplyNum;
    }

    public static long InsertNoCache(Supply supply)
    {
        return InsertNoCache(supply, false);
    }

    public static long InsertNoCache(Supply supply, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO supply (";
        if (isRandomKeys || useExistingPK) command += "SupplyNum,";
        command += "SupplierNum,CatalogNumber,Descript,Category,ItemOrder,LevelDesired,IsHidden,Price,BarCodeOrID,DispDefaultQuant,DispUnitsCount,DispUnitDesc,LevelOnHand,OrderQty) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(supply.SupplyNum) + ",";
        command +=
            SOut.Long(supply.SupplierNum) + ","
                                          + "'" + SOut.String(supply.CatalogNumber) + "',"
                                          + "'" + SOut.String(supply.Descript) + "',"
                                          + SOut.Long(supply.Category) + ","
                                          + SOut.Int(supply.ItemOrder) + ","
                                          + SOut.Float(supply.LevelDesired) + ","
                                          + SOut.Bool(supply.IsHidden) + ","
                                          + SOut.Double(supply.Price) + ","
                                          + "'" + SOut.String(supply.BarCodeOrID) + "',"
                                          + SOut.Float(supply.DispDefaultQuant) + ","
                                          + SOut.Int(supply.DispUnitsCount) + ","
                                          + "'" + SOut.String(supply.DispUnitDesc) + "',"
                                          + SOut.Float(supply.LevelOnHand) + ","
                                          + SOut.Int(supply.OrderQty) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            supply.SupplyNum = Db.NonQ(command, true, "SupplyNum", "supply");
        return supply.SupplyNum;
    }

    public static void Update(Supply supply)
    {
        var command = "UPDATE supply SET "
                      + "SupplierNum     =  " + SOut.Long(supply.SupplierNum) + ", "
                      + "CatalogNumber   = '" + SOut.String(supply.CatalogNumber) + "', "
                      + "Descript        = '" + SOut.String(supply.Descript) + "', "
                      + "Category        =  " + SOut.Long(supply.Category) + ", "
                      + "ItemOrder       =  " + SOut.Int(supply.ItemOrder) + ", "
                      + "LevelDesired    =  " + SOut.Float(supply.LevelDesired) + ", "
                      + "IsHidden        =  " + SOut.Bool(supply.IsHidden) + ", "
                      + "Price           =  " + SOut.Double(supply.Price) + ", "
                      + "BarCodeOrID     = '" + SOut.String(supply.BarCodeOrID) + "', "
                      + "DispDefaultQuant=  " + SOut.Float(supply.DispDefaultQuant) + ", "
                      + "DispUnitsCount  =  " + SOut.Int(supply.DispUnitsCount) + ", "
                      + "DispUnitDesc    = '" + SOut.String(supply.DispUnitDesc) + "', "
                      + "LevelOnHand     =  " + SOut.Float(supply.LevelOnHand) + ", "
                      + "OrderQty        =  " + SOut.Int(supply.OrderQty) + " "
                      + "WHERE SupplyNum = " + SOut.Long(supply.SupplyNum);
        Db.NonQ(command);
    }

    public static bool Update(Supply supply, Supply oldSupply)
    {
        var command = "";
        if (supply.SupplierNum != oldSupply.SupplierNum)
        {
            if (command != "") command += ",";
            command += "SupplierNum = " + SOut.Long(supply.SupplierNum) + "";
        }

        if (supply.CatalogNumber != oldSupply.CatalogNumber)
        {
            if (command != "") command += ",";
            command += "CatalogNumber = '" + SOut.String(supply.CatalogNumber) + "'";
        }

        if (supply.Descript != oldSupply.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(supply.Descript) + "'";
        }

        if (supply.Category != oldSupply.Category)
        {
            if (command != "") command += ",";
            command += "Category = " + SOut.Long(supply.Category) + "";
        }

        if (supply.ItemOrder != oldSupply.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(supply.ItemOrder) + "";
        }

        if (supply.LevelDesired != oldSupply.LevelDesired)
        {
            if (command != "") command += ",";
            command += "LevelDesired = " + SOut.Float(supply.LevelDesired) + "";
        }

        if (supply.IsHidden != oldSupply.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(supply.IsHidden) + "";
        }

        if (supply.Price != oldSupply.Price)
        {
            if (command != "") command += ",";
            command += "Price = " + SOut.Double(supply.Price) + "";
        }

        if (supply.BarCodeOrID != oldSupply.BarCodeOrID)
        {
            if (command != "") command += ",";
            command += "BarCodeOrID = '" + SOut.String(supply.BarCodeOrID) + "'";
        }

        if (supply.DispDefaultQuant != oldSupply.DispDefaultQuant)
        {
            if (command != "") command += ",";
            command += "DispDefaultQuant = " + SOut.Float(supply.DispDefaultQuant) + "";
        }

        if (supply.DispUnitsCount != oldSupply.DispUnitsCount)
        {
            if (command != "") command += ",";
            command += "DispUnitsCount = " + SOut.Int(supply.DispUnitsCount) + "";
        }

        if (supply.DispUnitDesc != oldSupply.DispUnitDesc)
        {
            if (command != "") command += ",";
            command += "DispUnitDesc = '" + SOut.String(supply.DispUnitDesc) + "'";
        }

        if (supply.LevelOnHand != oldSupply.LevelOnHand)
        {
            if (command != "") command += ",";
            command += "LevelOnHand = " + SOut.Float(supply.LevelOnHand) + "";
        }

        if (supply.OrderQty != oldSupply.OrderQty)
        {
            if (command != "") command += ",";
            command += "OrderQty = " + SOut.Int(supply.OrderQty) + "";
        }

        if (command == "") return false;
        command = "UPDATE supply SET " + command
                                       + " WHERE SupplyNum = " + SOut.Long(supply.SupplyNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Supply supply, Supply oldSupply)
    {
        if (supply.SupplierNum != oldSupply.SupplierNum) return true;
        if (supply.CatalogNumber != oldSupply.CatalogNumber) return true;
        if (supply.Descript != oldSupply.Descript) return true;
        if (supply.Category != oldSupply.Category) return true;
        if (supply.ItemOrder != oldSupply.ItemOrder) return true;
        if (supply.LevelDesired != oldSupply.LevelDesired) return true;
        if (supply.IsHidden != oldSupply.IsHidden) return true;
        if (supply.Price != oldSupply.Price) return true;
        if (supply.BarCodeOrID != oldSupply.BarCodeOrID) return true;
        if (supply.DispDefaultQuant != oldSupply.DispDefaultQuant) return true;
        if (supply.DispUnitsCount != oldSupply.DispUnitsCount) return true;
        if (supply.DispUnitDesc != oldSupply.DispUnitDesc) return true;
        if (supply.LevelOnHand != oldSupply.LevelOnHand) return true;
        if (supply.OrderQty != oldSupply.OrderQty) return true;
        return false;
    }

    public static void Delete(long supplyNum)
    {
        var command = "DELETE FROM supply "
                      + "WHERE SupplyNum = " + SOut.Long(supplyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSupplyNums)
    {
        if (listSupplyNums == null || listSupplyNums.Count == 0) return;
        var command = "DELETE FROM supply "
                      + "WHERE SupplyNum IN(" + string.Join(",", listSupplyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<Supply> listNew, List<Supply> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Supply>();
        var listUpdNew = new List<Supply>();
        var listUpdDB = new List<Supply>();
        var listDel = new List<Supply>();
        listNew.Sort((x, y) => { return x.SupplyNum.CompareTo(y.SupplyNum); });
        listDB.Sort((x, y) => { return x.SupplyNum.CompareTo(y.SupplyNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        Supply fieldNew;
        Supply fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.SupplyNum < fieldDB.SupplyNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SupplyNum > fieldDB.SupplyNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.SupplyNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}