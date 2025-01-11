#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProviderErxCrud
{
    public static ProviderErx SelectOne(long providerErxNum)
    {
        var command = "SELECT * FROM providererx "
                      + "WHERE ProviderErxNum = " + SOut.Long(providerErxNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProviderErx SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProviderErx> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProviderErx> TableToList(DataTable table)
    {
        var retVal = new List<ProviderErx>();
        ProviderErx providerErx;
        foreach (DataRow row in table.Rows)
        {
            providerErx = new ProviderErx();
            providerErx.ProviderErxNum = SIn.Long(row["ProviderErxNum"].ToString());
            providerErx.PatNum = SIn.Long(row["PatNum"].ToString());
            providerErx.NationalProviderID = SIn.String(row["NationalProviderID"].ToString());
            providerErx.IsEnabled = (ErxStatus) SIn.Int(row["IsEnabled"].ToString());
            providerErx.IsIdentifyProofed = SIn.Bool(row["IsIdentifyProofed"].ToString());
            providerErx.IsSentToHq = SIn.Bool(row["IsSentToHq"].ToString());
            providerErx.IsEpcs = SIn.Bool(row["IsEpcs"].ToString());
            providerErx.ErxType = (ErxOption) SIn.Int(row["ErxType"].ToString());
            providerErx.UserId = SIn.String(row["UserId"].ToString());
            providerErx.AccountId = SIn.String(row["AccountId"].ToString());
            providerErx.RegistrationKeyNum = SIn.Long(row["RegistrationKeyNum"].ToString());
            retVal.Add(providerErx);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProviderErx> listProviderErxs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProviderErx";
        var table = new DataTable(tableName);
        table.Columns.Add("ProviderErxNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("NationalProviderID");
        table.Columns.Add("IsEnabled");
        table.Columns.Add("IsIdentifyProofed");
        table.Columns.Add("IsSentToHq");
        table.Columns.Add("IsEpcs");
        table.Columns.Add("ErxType");
        table.Columns.Add("UserId");
        table.Columns.Add("AccountId");
        table.Columns.Add("RegistrationKeyNum");
        foreach (var providerErx in listProviderErxs)
            table.Rows.Add(SOut.Long(providerErx.ProviderErxNum), SOut.Long(providerErx.PatNum), providerErx.NationalProviderID, SOut.Int((int) providerErx.IsEnabled), SOut.Bool(providerErx.IsIdentifyProofed), SOut.Bool(providerErx.IsSentToHq), SOut.Bool(providerErx.IsEpcs), SOut.Int((int) providerErx.ErxType), providerErx.UserId, providerErx.AccountId, SOut.Long(providerErx.RegistrationKeyNum));
        return table;
    }

    public static long Insert(ProviderErx providerErx)
    {
        return Insert(providerErx, false);
    }

    public static long Insert(ProviderErx providerErx, bool useExistingPK)
    {
        var command = "INSERT INTO providererx (";

        command += "PatNum,NationalProviderID,IsEnabled,IsIdentifyProofed,IsSentToHq,IsEpcs,ErxType,UserId,AccountId,RegistrationKeyNum) VALUES(";

        command +=
            SOut.Long(providerErx.PatNum) + ","
                                          + "'" + SOut.String(providerErx.NationalProviderID) + "',"
                                          + SOut.Int((int) providerErx.IsEnabled) + ","
                                          + SOut.Bool(providerErx.IsIdentifyProofed) + ","
                                          + SOut.Bool(providerErx.IsSentToHq) + ","
                                          + SOut.Bool(providerErx.IsEpcs) + ","
                                          + SOut.Int((int) providerErx.ErxType) + ","
                                          + "'" + SOut.String(providerErx.UserId) + "',"
                                          + "'" + SOut.String(providerErx.AccountId) + "',"
                                          + SOut.Long(providerErx.RegistrationKeyNum) + ")";
        {
            providerErx.ProviderErxNum = Db.NonQ(command, true, "ProviderErxNum", "providerErx");
        }
        return providerErx.ProviderErxNum;
    }

    public static long InsertNoCache(ProviderErx providerErx)
    {
        return InsertNoCache(providerErx, false);
    }

    public static long InsertNoCache(ProviderErx providerErx, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO providererx (";
        if (isRandomKeys || useExistingPK) command += "ProviderErxNum,";
        command += "PatNum,NationalProviderID,IsEnabled,IsIdentifyProofed,IsSentToHq,IsEpcs,ErxType,UserId,AccountId,RegistrationKeyNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(providerErx.ProviderErxNum) + ",";
        command +=
            SOut.Long(providerErx.PatNum) + ","
                                          + "'" + SOut.String(providerErx.NationalProviderID) + "',"
                                          + SOut.Int((int) providerErx.IsEnabled) + ","
                                          + SOut.Bool(providerErx.IsIdentifyProofed) + ","
                                          + SOut.Bool(providerErx.IsSentToHq) + ","
                                          + SOut.Bool(providerErx.IsEpcs) + ","
                                          + SOut.Int((int) providerErx.ErxType) + ","
                                          + "'" + SOut.String(providerErx.UserId) + "',"
                                          + "'" + SOut.String(providerErx.AccountId) + "',"
                                          + SOut.Long(providerErx.RegistrationKeyNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            providerErx.ProviderErxNum = Db.NonQ(command, true, "ProviderErxNum", "providerErx");
        return providerErx.ProviderErxNum;
    }

    public static void Update(ProviderErx providerErx)
    {
        var command = "UPDATE providererx SET "
                      + "PatNum            =  " + SOut.Long(providerErx.PatNum) + ", "
                      + "NationalProviderID= '" + SOut.String(providerErx.NationalProviderID) + "', "
                      + "IsEnabled         =  " + SOut.Int((int) providerErx.IsEnabled) + ", "
                      + "IsIdentifyProofed =  " + SOut.Bool(providerErx.IsIdentifyProofed) + ", "
                      + "IsSentToHq        =  " + SOut.Bool(providerErx.IsSentToHq) + ", "
                      + "IsEpcs            =  " + SOut.Bool(providerErx.IsEpcs) + ", "
                      + "ErxType           =  " + SOut.Int((int) providerErx.ErxType) + ", "
                      + "UserId            = '" + SOut.String(providerErx.UserId) + "', "
                      + "AccountId         = '" + SOut.String(providerErx.AccountId) + "', "
                      + "RegistrationKeyNum=  " + SOut.Long(providerErx.RegistrationKeyNum) + " "
                      + "WHERE ProviderErxNum = " + SOut.Long(providerErx.ProviderErxNum);
        Db.NonQ(command);
    }

    public static bool Update(ProviderErx providerErx, ProviderErx oldProviderErx)
    {
        var command = "";
        if (providerErx.PatNum != oldProviderErx.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(providerErx.PatNum) + "";
        }

        if (providerErx.NationalProviderID != oldProviderErx.NationalProviderID)
        {
            if (command != "") command += ",";
            command += "NationalProviderID = '" + SOut.String(providerErx.NationalProviderID) + "'";
        }

        if (providerErx.IsEnabled != oldProviderErx.IsEnabled)
        {
            if (command != "") command += ",";
            command += "IsEnabled = " + SOut.Int((int) providerErx.IsEnabled) + "";
        }

        if (providerErx.IsIdentifyProofed != oldProviderErx.IsIdentifyProofed)
        {
            if (command != "") command += ",";
            command += "IsIdentifyProofed = " + SOut.Bool(providerErx.IsIdentifyProofed) + "";
        }

        if (providerErx.IsSentToHq != oldProviderErx.IsSentToHq)
        {
            if (command != "") command += ",";
            command += "IsSentToHq = " + SOut.Bool(providerErx.IsSentToHq) + "";
        }

        if (providerErx.IsEpcs != oldProviderErx.IsEpcs)
        {
            if (command != "") command += ",";
            command += "IsEpcs = " + SOut.Bool(providerErx.IsEpcs) + "";
        }

        if (providerErx.ErxType != oldProviderErx.ErxType)
        {
            if (command != "") command += ",";
            command += "ErxType = " + SOut.Int((int) providerErx.ErxType) + "";
        }

        if (providerErx.UserId != oldProviderErx.UserId)
        {
            if (command != "") command += ",";
            command += "UserId = '" + SOut.String(providerErx.UserId) + "'";
        }

        if (providerErx.AccountId != oldProviderErx.AccountId)
        {
            if (command != "") command += ",";
            command += "AccountId = '" + SOut.String(providerErx.AccountId) + "'";
        }

        if (providerErx.RegistrationKeyNum != oldProviderErx.RegistrationKeyNum)
        {
            if (command != "") command += ",";
            command += "RegistrationKeyNum = " + SOut.Long(providerErx.RegistrationKeyNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE providererx SET " + command
                                            + " WHERE ProviderErxNum = " + SOut.Long(providerErx.ProviderErxNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProviderErx providerErx, ProviderErx oldProviderErx)
    {
        if (providerErx.PatNum != oldProviderErx.PatNum) return true;
        if (providerErx.NationalProviderID != oldProviderErx.NationalProviderID) return true;
        if (providerErx.IsEnabled != oldProviderErx.IsEnabled) return true;
        if (providerErx.IsIdentifyProofed != oldProviderErx.IsIdentifyProofed) return true;
        if (providerErx.IsSentToHq != oldProviderErx.IsSentToHq) return true;
        if (providerErx.IsEpcs != oldProviderErx.IsEpcs) return true;
        if (providerErx.ErxType != oldProviderErx.ErxType) return true;
        if (providerErx.UserId != oldProviderErx.UserId) return true;
        if (providerErx.AccountId != oldProviderErx.AccountId) return true;
        if (providerErx.RegistrationKeyNum != oldProviderErx.RegistrationKeyNum) return true;
        return false;
    }

    public static void Delete(long providerErxNum)
    {
        var command = "DELETE FROM providererx "
                      + "WHERE ProviderErxNum = " + SOut.Long(providerErxNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProviderErxNums)
    {
        if (listProviderErxNums == null || listProviderErxNums.Count == 0) return;
        var command = "DELETE FROM providererx "
                      + "WHERE ProviderErxNum IN(" + string.Join(",", listProviderErxNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<ProviderErx> listNew, List<ProviderErx> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<ProviderErx>();
        var listUpdNew = new List<ProviderErx>();
        var listUpdDB = new List<ProviderErx>();
        var listDel = new List<ProviderErx>();
        listNew.Sort((x, y) => { return x.ProviderErxNum.CompareTo(y.ProviderErxNum); });
        listDB.Sort((x, y) => { return x.ProviderErxNum.CompareTo(y.ProviderErxNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        ProviderErx fieldNew;
        ProviderErx fieldDB;
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

            if (fieldNew.ProviderErxNum < fieldDB.ProviderErxNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.ProviderErxNum > fieldDB.ProviderErxNum)
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

        DeleteMany(listDel.Select(x => x.ProviderErxNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}