#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EbillCrud
{
    public static Ebill SelectOne(long ebillNum)
    {
        var command = "SELECT * FROM ebill "
                      + "WHERE EbillNum = " + SOut.Long(ebillNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Ebill SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Ebill> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Ebill> TableToList(DataTable table)
    {
        var retVal = new List<Ebill>();
        Ebill ebill;
        foreach (DataRow row in table.Rows)
        {
            ebill = new Ebill();
            ebill.EbillNum = SIn.Long(row["EbillNum"].ToString());
            ebill.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            ebill.ClientAcctNumber = SIn.String(row["ClientAcctNumber"].ToString());
            ebill.ElectUserName = SIn.String(row["ElectUserName"].ToString());
            ebill.ElectPassword = SIn.String(row["ElectPassword"].ToString());
            ebill.PracticeAddress = (EbillAddress) SIn.Int(row["PracticeAddress"].ToString());
            ebill.RemitAddress = (EbillAddress) SIn.Int(row["RemitAddress"].ToString());
            retVal.Add(ebill);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Ebill> listEbills, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Ebill";
        var table = new DataTable(tableName);
        table.Columns.Add("EbillNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ClientAcctNumber");
        table.Columns.Add("ElectUserName");
        table.Columns.Add("ElectPassword");
        table.Columns.Add("PracticeAddress");
        table.Columns.Add("RemitAddress");
        foreach (var ebill in listEbills)
            table.Rows.Add(SOut.Long(ebill.EbillNum), SOut.Long(ebill.ClinicNum), ebill.ClientAcctNumber, ebill.ElectUserName, ebill.ElectPassword, SOut.Int((int) ebill.PracticeAddress), SOut.Int((int) ebill.RemitAddress));
        return table;
    }

    public static long Insert(Ebill ebill)
    {
        return Insert(ebill, false);
    }

    public static long Insert(Ebill ebill, bool useExistingPK)
    {
        var command = "INSERT INTO ebill (";

        command += "ClinicNum,ClientAcctNumber,ElectUserName,ElectPassword,PracticeAddress,RemitAddress) VALUES(";

        command +=
            SOut.Long(ebill.ClinicNum) + ","
                                       + "'" + SOut.String(ebill.ClientAcctNumber) + "',"
                                       + "'" + SOut.String(ebill.ElectUserName) + "',"
                                       + "'" + SOut.String(ebill.ElectPassword) + "',"
                                       + SOut.Int((int) ebill.PracticeAddress) + ","
                                       + SOut.Int((int) ebill.RemitAddress) + ")";
        {
            ebill.EbillNum = Db.NonQ(command, true, "EbillNum", "ebill");
        }
        return ebill.EbillNum;
    }

    public static long InsertNoCache(Ebill ebill)
    {
        return InsertNoCache(ebill, false);
    }

    public static long InsertNoCache(Ebill ebill, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ebill (";
        if (isRandomKeys || useExistingPK) command += "EbillNum,";
        command += "ClinicNum,ClientAcctNumber,ElectUserName,ElectPassword,PracticeAddress,RemitAddress) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ebill.EbillNum) + ",";
        command +=
            SOut.Long(ebill.ClinicNum) + ","
                                       + "'" + SOut.String(ebill.ClientAcctNumber) + "',"
                                       + "'" + SOut.String(ebill.ElectUserName) + "',"
                                       + "'" + SOut.String(ebill.ElectPassword) + "',"
                                       + SOut.Int((int) ebill.PracticeAddress) + ","
                                       + SOut.Int((int) ebill.RemitAddress) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ebill.EbillNum = Db.NonQ(command, true, "EbillNum", "ebill");
        return ebill.EbillNum;
    }

    public static void Update(Ebill ebill)
    {
        var command = "UPDATE ebill SET "
                      + "ClinicNum       =  " + SOut.Long(ebill.ClinicNum) + ", "
                      + "ClientAcctNumber= '" + SOut.String(ebill.ClientAcctNumber) + "', "
                      + "ElectUserName   = '" + SOut.String(ebill.ElectUserName) + "', "
                      + "ElectPassword   = '" + SOut.String(ebill.ElectPassword) + "', "
                      + "PracticeAddress =  " + SOut.Int((int) ebill.PracticeAddress) + ", "
                      + "RemitAddress    =  " + SOut.Int((int) ebill.RemitAddress) + " "
                      + "WHERE EbillNum = " + SOut.Long(ebill.EbillNum);
        Db.NonQ(command);
    }

    public static bool Update(Ebill ebill, Ebill oldEbill)
    {
        var command = "";
        if (ebill.ClinicNum != oldEbill.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(ebill.ClinicNum) + "";
        }

        if (ebill.ClientAcctNumber != oldEbill.ClientAcctNumber)
        {
            if (command != "") command += ",";
            command += "ClientAcctNumber = '" + SOut.String(ebill.ClientAcctNumber) + "'";
        }

        if (ebill.ElectUserName != oldEbill.ElectUserName)
        {
            if (command != "") command += ",";
            command += "ElectUserName = '" + SOut.String(ebill.ElectUserName) + "'";
        }

        if (ebill.ElectPassword != oldEbill.ElectPassword)
        {
            if (command != "") command += ",";
            command += "ElectPassword = '" + SOut.String(ebill.ElectPassword) + "'";
        }

        if (ebill.PracticeAddress != oldEbill.PracticeAddress)
        {
            if (command != "") command += ",";
            command += "PracticeAddress = " + SOut.Int((int) ebill.PracticeAddress) + "";
        }

        if (ebill.RemitAddress != oldEbill.RemitAddress)
        {
            if (command != "") command += ",";
            command += "RemitAddress = " + SOut.Int((int) ebill.RemitAddress) + "";
        }

        if (command == "") return false;
        command = "UPDATE ebill SET " + command
                                      + " WHERE EbillNum = " + SOut.Long(ebill.EbillNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Ebill ebill, Ebill oldEbill)
    {
        if (ebill.ClinicNum != oldEbill.ClinicNum) return true;
        if (ebill.ClientAcctNumber != oldEbill.ClientAcctNumber) return true;
        if (ebill.ElectUserName != oldEbill.ElectUserName) return true;
        if (ebill.ElectPassword != oldEbill.ElectPassword) return true;
        if (ebill.PracticeAddress != oldEbill.PracticeAddress) return true;
        if (ebill.RemitAddress != oldEbill.RemitAddress) return true;
        return false;
    }

    public static void Delete(long ebillNum)
    {
        var command = "DELETE FROM ebill "
                      + "WHERE EbillNum = " + SOut.Long(ebillNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEbillNums)
    {
        if (listEbillNums == null || listEbillNums.Count == 0) return;
        var command = "DELETE FROM ebill "
                      + "WHERE EbillNum IN(" + string.Join(",", listEbillNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<Ebill> listNew, List<Ebill> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<Ebill>();
        var listUpdNew = new List<Ebill>();
        var listUpdDB = new List<Ebill>();
        var listDel = new List<Ebill>();
        listNew.Sort((x, y) => { return x.EbillNum.CompareTo(y.EbillNum); });
        listDB.Sort((x, y) => { return x.EbillNum.CompareTo(y.EbillNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        Ebill fieldNew;
        Ebill fieldDB;
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

            if (fieldNew.EbillNum < fieldDB.EbillNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.EbillNum > fieldDB.EbillNum)
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

        DeleteMany(listDel.Select(x => x.EbillNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}