#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProviderClinicCrud
{
    public static ProviderClinic SelectOne(long providerClinicNum)
    {
        var command = "SELECT * FROM providerclinic "
                      + "WHERE ProviderClinicNum = " + SOut.Long(providerClinicNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProviderClinic SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProviderClinic> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProviderClinic> TableToList(DataTable table)
    {
        var retVal = new List<ProviderClinic>();
        ProviderClinic providerClinic;
        foreach (DataRow row in table.Rows)
        {
            providerClinic = new ProviderClinic();
            providerClinic.ProviderClinicNum = SIn.Long(row["ProviderClinicNum"].ToString());
            providerClinic.ProvNum = SIn.Long(row["ProvNum"].ToString());
            providerClinic.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            providerClinic.DEANum = SIn.String(row["DEANum"].ToString());
            providerClinic.StateLicense = SIn.String(row["StateLicense"].ToString());
            providerClinic.StateRxID = SIn.String(row["StateRxID"].ToString());
            providerClinic.StateWhereLicensed = SIn.String(row["StateWhereLicensed"].ToString());
            providerClinic.CareCreditMerchantId = SIn.String(row["CareCreditMerchantId"].ToString());
            retVal.Add(providerClinic);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProviderClinic> listProviderClinics, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProviderClinic";
        var table = new DataTable(tableName);
        table.Columns.Add("ProviderClinicNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("DEANum");
        table.Columns.Add("StateLicense");
        table.Columns.Add("StateRxID");
        table.Columns.Add("StateWhereLicensed");
        table.Columns.Add("CareCreditMerchantId");
        foreach (var providerClinic in listProviderClinics)
            table.Rows.Add(SOut.Long(providerClinic.ProviderClinicNum), SOut.Long(providerClinic.ProvNum), SOut.Long(providerClinic.ClinicNum), providerClinic.DEANum, providerClinic.StateLicense, providerClinic.StateRxID, providerClinic.StateWhereLicensed, providerClinic.CareCreditMerchantId);
        return table;
    }

    public static long Insert(ProviderClinic providerClinic)
    {
        return Insert(providerClinic, false);
    }

    public static long Insert(ProviderClinic providerClinic, bool useExistingPK)
    {
        var command = "INSERT INTO providerclinic (";

        command += "ProvNum,ClinicNum,DEANum,StateLicense,StateRxID,StateWhereLicensed,CareCreditMerchantId) VALUES(";

        command +=
            SOut.Long(providerClinic.ProvNum) + ","
                                              + SOut.Long(providerClinic.ClinicNum) + ","
                                              + "'" + SOut.String(providerClinic.DEANum) + "',"
                                              + "'" + SOut.String(providerClinic.StateLicense) + "',"
                                              + "'" + SOut.String(providerClinic.StateRxID) + "',"
                                              + "'" + SOut.String(providerClinic.StateWhereLicensed) + "',"
                                              + "'" + SOut.String(providerClinic.CareCreditMerchantId) + "')";
        {
            providerClinic.ProviderClinicNum = Db.NonQ(command, true, "ProviderClinicNum", "providerClinic");
        }
        return providerClinic.ProviderClinicNum;
    }

    public static long InsertNoCache(ProviderClinic providerClinic)
    {
        return InsertNoCache(providerClinic, false);
    }

    public static long InsertNoCache(ProviderClinic providerClinic, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO providerclinic (";
        if (isRandomKeys || useExistingPK) command += "ProviderClinicNum,";
        command += "ProvNum,ClinicNum,DEANum,StateLicense,StateRxID,StateWhereLicensed,CareCreditMerchantId) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(providerClinic.ProviderClinicNum) + ",";
        command +=
            SOut.Long(providerClinic.ProvNum) + ","
                                              + SOut.Long(providerClinic.ClinicNum) + ","
                                              + "'" + SOut.String(providerClinic.DEANum) + "',"
                                              + "'" + SOut.String(providerClinic.StateLicense) + "',"
                                              + "'" + SOut.String(providerClinic.StateRxID) + "',"
                                              + "'" + SOut.String(providerClinic.StateWhereLicensed) + "',"
                                              + "'" + SOut.String(providerClinic.CareCreditMerchantId) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            providerClinic.ProviderClinicNum = Db.NonQ(command, true, "ProviderClinicNum", "providerClinic");
        return providerClinic.ProviderClinicNum;
    }

    public static void Update(ProviderClinic providerClinic)
    {
        var command = "UPDATE providerclinic SET "
                      + "ProvNum             =  " + SOut.Long(providerClinic.ProvNum) + ", "
                      + "ClinicNum           =  " + SOut.Long(providerClinic.ClinicNum) + ", "
                      + "DEANum              = '" + SOut.String(providerClinic.DEANum) + "', "
                      + "StateLicense        = '" + SOut.String(providerClinic.StateLicense) + "', "
                      + "StateRxID           = '" + SOut.String(providerClinic.StateRxID) + "', "
                      + "StateWhereLicensed  = '" + SOut.String(providerClinic.StateWhereLicensed) + "', "
                      + "CareCreditMerchantId= '" + SOut.String(providerClinic.CareCreditMerchantId) + "' "
                      + "WHERE ProviderClinicNum = " + SOut.Long(providerClinic.ProviderClinicNum);
        Db.NonQ(command);
    }

    public static bool Update(ProviderClinic providerClinic, ProviderClinic oldProviderClinic)
    {
        var command = "";
        if (providerClinic.ProvNum != oldProviderClinic.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(providerClinic.ProvNum) + "";
        }

        if (providerClinic.ClinicNum != oldProviderClinic.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(providerClinic.ClinicNum) + "";
        }

        if (providerClinic.DEANum != oldProviderClinic.DEANum)
        {
            if (command != "") command += ",";
            command += "DEANum = '" + SOut.String(providerClinic.DEANum) + "'";
        }

        if (providerClinic.StateLicense != oldProviderClinic.StateLicense)
        {
            if (command != "") command += ",";
            command += "StateLicense = '" + SOut.String(providerClinic.StateLicense) + "'";
        }

        if (providerClinic.StateRxID != oldProviderClinic.StateRxID)
        {
            if (command != "") command += ",";
            command += "StateRxID = '" + SOut.String(providerClinic.StateRxID) + "'";
        }

        if (providerClinic.StateWhereLicensed != oldProviderClinic.StateWhereLicensed)
        {
            if (command != "") command += ",";
            command += "StateWhereLicensed = '" + SOut.String(providerClinic.StateWhereLicensed) + "'";
        }

        if (providerClinic.CareCreditMerchantId != oldProviderClinic.CareCreditMerchantId)
        {
            if (command != "") command += ",";
            command += "CareCreditMerchantId = '" + SOut.String(providerClinic.CareCreditMerchantId) + "'";
        }

        if (command == "") return false;
        command = "UPDATE providerclinic SET " + command
                                               + " WHERE ProviderClinicNum = " + SOut.Long(providerClinic.ProviderClinicNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProviderClinic providerClinic, ProviderClinic oldProviderClinic)
    {
        if (providerClinic.ProvNum != oldProviderClinic.ProvNum) return true;
        if (providerClinic.ClinicNum != oldProviderClinic.ClinicNum) return true;
        if (providerClinic.DEANum != oldProviderClinic.DEANum) return true;
        if (providerClinic.StateLicense != oldProviderClinic.StateLicense) return true;
        if (providerClinic.StateRxID != oldProviderClinic.StateRxID) return true;
        if (providerClinic.StateWhereLicensed != oldProviderClinic.StateWhereLicensed) return true;
        if (providerClinic.CareCreditMerchantId != oldProviderClinic.CareCreditMerchantId) return true;
        return false;
    }

    public static void Delete(long providerClinicNum)
    {
        var command = "DELETE FROM providerclinic "
                      + "WHERE ProviderClinicNum = " + SOut.Long(providerClinicNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProviderClinicNums)
    {
        if (listProviderClinicNums == null || listProviderClinicNums.Count == 0) return;
        var command = "DELETE FROM providerclinic "
                      + "WHERE ProviderClinicNum IN(" + string.Join(",", listProviderClinicNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ProviderClinic> listNew, List<ProviderClinic> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ProviderClinic>();
        var listUpdNew = new List<ProviderClinic>();
        var listUpdDB = new List<ProviderClinic>();
        var listDel = new List<ProviderClinic>();
        listNew.Sort((x, y) => { return x.ProviderClinicNum.CompareTo(y.ProviderClinicNum); });
        listDB.Sort((x, y) => { return x.ProviderClinicNum.CompareTo(y.ProviderClinicNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ProviderClinic fieldNew;
        ProviderClinic fieldDB;
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

            if (fieldNew.ProviderClinicNum < fieldDB.ProviderClinicNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ProviderClinicNum > fieldDB.ProviderClinicNum)
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

        DeleteMany(listDel.Select(x => x.ProviderClinicNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}