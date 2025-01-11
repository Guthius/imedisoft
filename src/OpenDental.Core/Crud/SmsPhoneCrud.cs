#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SmsPhoneCrud
{
    public static SmsPhone SelectOne(long smsPhoneNum)
    {
        var command = "SELECT * FROM smsphone "
                      + "WHERE SmsPhoneNum = " + SOut.Long(smsPhoneNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SmsPhone SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SmsPhone> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SmsPhone> TableToList(DataTable table)
    {
        var retVal = new List<SmsPhone>();
        SmsPhone smsPhone;
        foreach (DataRow row in table.Rows)
        {
            smsPhone = new SmsPhone();
            smsPhone.SmsPhoneNum = SIn.Long(row["SmsPhoneNum"].ToString());
            smsPhone.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            smsPhone.PhoneNumber = SIn.String(row["PhoneNumber"].ToString());
            smsPhone.DateTimeActive = SIn.DateTime(row["DateTimeActive"].ToString());
            smsPhone.DateTimeInactive = SIn.DateTime(row["DateTimeInactive"].ToString());
            smsPhone.InactiveCode = SIn.String(row["InactiveCode"].ToString());
            smsPhone.CountryCode = SIn.String(row["CountryCode"].ToString());
            retVal.Add(smsPhone);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SmsPhone> listSmsPhones, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SmsPhone";
        var table = new DataTable(tableName);
        table.Columns.Add("SmsPhoneNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("PhoneNumber");
        table.Columns.Add("DateTimeActive");
        table.Columns.Add("DateTimeInactive");
        table.Columns.Add("InactiveCode");
        table.Columns.Add("CountryCode");
        foreach (var smsPhone in listSmsPhones)
            table.Rows.Add(SOut.Long(smsPhone.SmsPhoneNum), SOut.Long(smsPhone.ClinicNum), smsPhone.PhoneNumber, SOut.DateT(smsPhone.DateTimeActive, false), SOut.DateT(smsPhone.DateTimeInactive, false), smsPhone.InactiveCode, smsPhone.CountryCode);
        return table;
    }

    public static long Insert(SmsPhone smsPhone)
    {
        return Insert(smsPhone, false);
    }

    public static long Insert(SmsPhone smsPhone, bool useExistingPK)
    {
        var command = "INSERT INTO smsphone (";

        command += "ClinicNum,PhoneNumber,DateTimeActive,DateTimeInactive,InactiveCode,CountryCode) VALUES(";

        command +=
            SOut.Long(smsPhone.ClinicNum) + ","
                                          + "'" + SOut.String(smsPhone.PhoneNumber) + "',"
                                          + SOut.DateT(smsPhone.DateTimeActive) + ","
                                          + SOut.DateT(smsPhone.DateTimeInactive) + ","
                                          + "'" + SOut.String(smsPhone.InactiveCode) + "',"
                                          + "'" + SOut.String(smsPhone.CountryCode) + "')";
        {
            smsPhone.SmsPhoneNum = Db.NonQ(command, true, "SmsPhoneNum", "smsPhone");
        }
        return smsPhone.SmsPhoneNum;
    }

    public static long InsertNoCache(SmsPhone smsPhone)
    {
        return InsertNoCache(smsPhone, false);
    }

    public static long InsertNoCache(SmsPhone smsPhone, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO smsphone (";
        if (isRandomKeys || useExistingPK) command += "SmsPhoneNum,";
        command += "ClinicNum,PhoneNumber,DateTimeActive,DateTimeInactive,InactiveCode,CountryCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(smsPhone.SmsPhoneNum) + ",";
        command +=
            SOut.Long(smsPhone.ClinicNum) + ","
                                          + "'" + SOut.String(smsPhone.PhoneNumber) + "',"
                                          + SOut.DateT(smsPhone.DateTimeActive) + ","
                                          + SOut.DateT(smsPhone.DateTimeInactive) + ","
                                          + "'" + SOut.String(smsPhone.InactiveCode) + "',"
                                          + "'" + SOut.String(smsPhone.CountryCode) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            smsPhone.SmsPhoneNum = Db.NonQ(command, true, "SmsPhoneNum", "smsPhone");
        return smsPhone.SmsPhoneNum;
    }

    public static void Update(SmsPhone smsPhone)
    {
        var command = "UPDATE smsphone SET "
                      + "ClinicNum       =  " + SOut.Long(smsPhone.ClinicNum) + ", "
                      + "PhoneNumber     = '" + SOut.String(smsPhone.PhoneNumber) + "', "
                      + "DateTimeActive  =  " + SOut.DateT(smsPhone.DateTimeActive) + ", "
                      + "DateTimeInactive=  " + SOut.DateT(smsPhone.DateTimeInactive) + ", "
                      + "InactiveCode    = '" + SOut.String(smsPhone.InactiveCode) + "', "
                      + "CountryCode     = '" + SOut.String(smsPhone.CountryCode) + "' "
                      + "WHERE SmsPhoneNum = " + SOut.Long(smsPhone.SmsPhoneNum);
        Db.NonQ(command);
    }

    public static bool Update(SmsPhone smsPhone, SmsPhone oldSmsPhone)
    {
        var command = "";
        if (smsPhone.ClinicNum != oldSmsPhone.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(smsPhone.ClinicNum) + "";
        }

        if (smsPhone.PhoneNumber != oldSmsPhone.PhoneNumber)
        {
            if (command != "") command += ",";
            command += "PhoneNumber = '" + SOut.String(smsPhone.PhoneNumber) + "'";
        }

        if (smsPhone.DateTimeActive != oldSmsPhone.DateTimeActive)
        {
            if (command != "") command += ",";
            command += "DateTimeActive = " + SOut.DateT(smsPhone.DateTimeActive) + "";
        }

        if (smsPhone.DateTimeInactive != oldSmsPhone.DateTimeInactive)
        {
            if (command != "") command += ",";
            command += "DateTimeInactive = " + SOut.DateT(smsPhone.DateTimeInactive) + "";
        }

        if (smsPhone.InactiveCode != oldSmsPhone.InactiveCode)
        {
            if (command != "") command += ",";
            command += "InactiveCode = '" + SOut.String(smsPhone.InactiveCode) + "'";
        }

        if (smsPhone.CountryCode != oldSmsPhone.CountryCode)
        {
            if (command != "") command += ",";
            command += "CountryCode = '" + SOut.String(smsPhone.CountryCode) + "'";
        }

        if (command == "") return false;
        command = "UPDATE smsphone SET " + command
                                         + " WHERE SmsPhoneNum = " + SOut.Long(smsPhone.SmsPhoneNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SmsPhone smsPhone, SmsPhone oldSmsPhone)
    {
        if (smsPhone.ClinicNum != oldSmsPhone.ClinicNum) return true;
        if (smsPhone.PhoneNumber != oldSmsPhone.PhoneNumber) return true;
        if (smsPhone.DateTimeActive != oldSmsPhone.DateTimeActive) return true;
        if (smsPhone.DateTimeInactive != oldSmsPhone.DateTimeInactive) return true;
        if (smsPhone.InactiveCode != oldSmsPhone.InactiveCode) return true;
        if (smsPhone.CountryCode != oldSmsPhone.CountryCode) return true;
        return false;
    }

    public static void Delete(long smsPhoneNum)
    {
        var command = "DELETE FROM smsphone "
                      + "WHERE SmsPhoneNum = " + SOut.Long(smsPhoneNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSmsPhoneNums)
    {
        if (listSmsPhoneNums == null || listSmsPhoneNums.Count == 0) return;
        var command = "DELETE FROM smsphone "
                      + "WHERE SmsPhoneNum IN(" + string.Join(",", listSmsPhoneNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<SmsPhone> listNew, List<SmsPhone> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<SmsPhone>();
        var listUpdNew = new List<SmsPhone>();
        var listUpdDB = new List<SmsPhone>();
        var listDel = new List<SmsPhone>();
        listNew.Sort((x, y) => { return x.SmsPhoneNum.CompareTo(y.SmsPhoneNum); });
        listDB.Sort((x, y) => { return x.SmsPhoneNum.CompareTo(y.SmsPhoneNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        SmsPhone fieldNew;
        SmsPhone fieldDB;
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

            if (fieldNew.SmsPhoneNum < fieldDB.SmsPhoneNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.SmsPhoneNum > fieldDB.SmsPhoneNum)
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

        DeleteMany(listDel.Select(x => x.SmsPhoneNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}