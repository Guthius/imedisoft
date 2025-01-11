#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RegistrationKeyCrud
{
    public static RegistrationKey SelectOne(long registrationKeyNum)
    {
        var command = "SELECT * FROM registrationkey "
                      + "WHERE RegistrationKeyNum = " + SOut.Long(registrationKeyNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RegistrationKey SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RegistrationKey> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RegistrationKey> TableToList(DataTable table)
    {
        var retVal = new List<RegistrationKey>();
        RegistrationKey registrationKey;
        foreach (DataRow row in table.Rows)
        {
            registrationKey = new RegistrationKey();
            registrationKey.RegistrationKeyNum = SIn.Long(row["RegistrationKeyNum"].ToString());
            registrationKey.PatNum = SIn.Long(row["PatNum"].ToString());
            registrationKey.RegKey = SIn.String(row["RegKey"].ToString());
            registrationKey.Note = SIn.String(row["Note"].ToString());
            registrationKey.DateStarted = SIn.Date(row["DateStarted"].ToString());
            registrationKey.DateDisabled = SIn.Date(row["DateDisabled"].ToString());
            registrationKey.DateEnded = SIn.Date(row["DateEnded"].ToString());
            registrationKey.IsForeign = SIn.Bool(row["IsForeign"].ToString());
            registrationKey.UsesServerVersion = SIn.Bool(row["UsesServerVersion"].ToString());
            registrationKey.IsFreeVersion = SIn.Bool(row["IsFreeVersion"].ToString());
            registrationKey.IsOnlyForTesting = SIn.Bool(row["IsOnlyForTesting"].ToString());
            registrationKey.VotesAllotted = SIn.Int(row["VotesAllotted"].ToString());
            registrationKey.IsResellerCustomer = SIn.Bool(row["IsResellerCustomer"].ToString());
            registrationKey.HasEarlyAccess = SIn.Bool(row["HasEarlyAccess"].ToString());
            registrationKey.DateTBackupScheduled = SIn.DateTime(row["DateTBackupScheduled"].ToString());
            registrationKey.BackupPassCode = SIn.String(row["BackupPassCode"].ToString());
            retVal.Add(registrationKey);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RegistrationKey> listRegistrationKeys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RegistrationKey";
        var table = new DataTable(tableName);
        table.Columns.Add("RegistrationKeyNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("RegKey");
        table.Columns.Add("Note");
        table.Columns.Add("DateStarted");
        table.Columns.Add("DateDisabled");
        table.Columns.Add("DateEnded");
        table.Columns.Add("IsForeign");
        table.Columns.Add("UsesServerVersion");
        table.Columns.Add("IsFreeVersion");
        table.Columns.Add("IsOnlyForTesting");
        table.Columns.Add("VotesAllotted");
        table.Columns.Add("IsResellerCustomer");
        table.Columns.Add("HasEarlyAccess");
        table.Columns.Add("DateTBackupScheduled");
        table.Columns.Add("BackupPassCode");
        foreach (var registrationKey in listRegistrationKeys)
            table.Rows.Add(SOut.Long(registrationKey.RegistrationKeyNum), SOut.Long(registrationKey.PatNum), registrationKey.RegKey, registrationKey.Note, SOut.DateT(registrationKey.DateStarted, false), SOut.DateT(registrationKey.DateDisabled, false), SOut.DateT(registrationKey.DateEnded, false), SOut.Bool(registrationKey.IsForeign), SOut.Bool(registrationKey.UsesServerVersion), SOut.Bool(registrationKey.IsFreeVersion), SOut.Bool(registrationKey.IsOnlyForTesting), SOut.Int(registrationKey.VotesAllotted), SOut.Bool(registrationKey.IsResellerCustomer), SOut.Bool(registrationKey.HasEarlyAccess), SOut.DateT(registrationKey.DateTBackupScheduled, false), registrationKey.BackupPassCode);
        return table;
    }

    public static long Insert(RegistrationKey registrationKey)
    {
        return Insert(registrationKey, false);
    }

    public static long Insert(RegistrationKey registrationKey, bool useExistingPK)
    {
        var command = "INSERT INTO registrationkey (";

        command += "PatNum,RegKey,Note,DateStarted,DateDisabled,DateEnded,IsForeign,UsesServerVersion,IsFreeVersion,IsOnlyForTesting,VotesAllotted,IsResellerCustomer,HasEarlyAccess,DateTBackupScheduled,BackupPassCode) VALUES(";

        command +=
            SOut.Long(registrationKey.PatNum) + ","
                                              + "'" + SOut.String(registrationKey.RegKey) + "',"
                                              + "'" + SOut.String(registrationKey.Note) + "',"
                                              + SOut.Date(registrationKey.DateStarted) + ","
                                              + SOut.Date(registrationKey.DateDisabled) + ","
                                              + SOut.Date(registrationKey.DateEnded) + ","
                                              + SOut.Bool(registrationKey.IsForeign) + ","
                                              + SOut.Bool(registrationKey.UsesServerVersion) + ","
                                              + SOut.Bool(registrationKey.IsFreeVersion) + ","
                                              + SOut.Bool(registrationKey.IsOnlyForTesting) + ","
                                              + SOut.Int(registrationKey.VotesAllotted) + ","
                                              + SOut.Bool(registrationKey.IsResellerCustomer) + ","
                                              + SOut.Bool(registrationKey.HasEarlyAccess) + ","
                                              + SOut.DateT(registrationKey.DateTBackupScheduled) + ","
                                              + "'" + SOut.String(registrationKey.BackupPassCode) + "')";
        {
            registrationKey.RegistrationKeyNum = Db.NonQ(command, true, "RegistrationKeyNum", "registrationKey");
        }
        return registrationKey.RegistrationKeyNum;
    }

    public static long InsertNoCache(RegistrationKey registrationKey)
    {
        return InsertNoCache(registrationKey, false);
    }

    public static long InsertNoCache(RegistrationKey registrationKey, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO registrationkey (";
        if (isRandomKeys || useExistingPK) command += "RegistrationKeyNum,";
        command += "PatNum,RegKey,Note,DateStarted,DateDisabled,DateEnded,IsForeign,UsesServerVersion,IsFreeVersion,IsOnlyForTesting,VotesAllotted,IsResellerCustomer,HasEarlyAccess,DateTBackupScheduled,BackupPassCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(registrationKey.RegistrationKeyNum) + ",";
        command +=
            SOut.Long(registrationKey.PatNum) + ","
                                              + "'" + SOut.String(registrationKey.RegKey) + "',"
                                              + "'" + SOut.String(registrationKey.Note) + "',"
                                              + SOut.Date(registrationKey.DateStarted) + ","
                                              + SOut.Date(registrationKey.DateDisabled) + ","
                                              + SOut.Date(registrationKey.DateEnded) + ","
                                              + SOut.Bool(registrationKey.IsForeign) + ","
                                              + SOut.Bool(registrationKey.UsesServerVersion) + ","
                                              + SOut.Bool(registrationKey.IsFreeVersion) + ","
                                              + SOut.Bool(registrationKey.IsOnlyForTesting) + ","
                                              + SOut.Int(registrationKey.VotesAllotted) + ","
                                              + SOut.Bool(registrationKey.IsResellerCustomer) + ","
                                              + SOut.Bool(registrationKey.HasEarlyAccess) + ","
                                              + SOut.DateT(registrationKey.DateTBackupScheduled) + ","
                                              + "'" + SOut.String(registrationKey.BackupPassCode) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            registrationKey.RegistrationKeyNum = Db.NonQ(command, true, "RegistrationKeyNum", "registrationKey");
        return registrationKey.RegistrationKeyNum;
    }

    public static void Update(RegistrationKey registrationKey)
    {
        var command = "UPDATE registrationkey SET "
                      + "PatNum              =  " + SOut.Long(registrationKey.PatNum) + ", "
                      + "RegKey              = '" + SOut.String(registrationKey.RegKey) + "', "
                      + "Note                = '" + SOut.String(registrationKey.Note) + "', "
                      + "DateStarted         =  " + SOut.Date(registrationKey.DateStarted) + ", "
                      + "DateDisabled        =  " + SOut.Date(registrationKey.DateDisabled) + ", "
                      + "DateEnded           =  " + SOut.Date(registrationKey.DateEnded) + ", "
                      + "IsForeign           =  " + SOut.Bool(registrationKey.IsForeign) + ", "
                      + "UsesServerVersion   =  " + SOut.Bool(registrationKey.UsesServerVersion) + ", "
                      + "IsFreeVersion       =  " + SOut.Bool(registrationKey.IsFreeVersion) + ", "
                      + "IsOnlyForTesting    =  " + SOut.Bool(registrationKey.IsOnlyForTesting) + ", "
                      + "VotesAllotted       =  " + SOut.Int(registrationKey.VotesAllotted) + ", "
                      + "IsResellerCustomer  =  " + SOut.Bool(registrationKey.IsResellerCustomer) + ", "
                      + "HasEarlyAccess      =  " + SOut.Bool(registrationKey.HasEarlyAccess) + ", "
                      + "DateTBackupScheduled=  " + SOut.DateT(registrationKey.DateTBackupScheduled) + ", "
                      + "BackupPassCode      = '" + SOut.String(registrationKey.BackupPassCode) + "' "
                      + "WHERE RegistrationKeyNum = " + SOut.Long(registrationKey.RegistrationKeyNum);
        Db.NonQ(command);
    }

    public static bool Update(RegistrationKey registrationKey, RegistrationKey oldRegistrationKey)
    {
        var command = "";
        if (registrationKey.PatNum != oldRegistrationKey.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(registrationKey.PatNum) + "";
        }

        if (registrationKey.RegKey != oldRegistrationKey.RegKey)
        {
            if (command != "") command += ",";
            command += "RegKey = '" + SOut.String(registrationKey.RegKey) + "'";
        }

        if (registrationKey.Note != oldRegistrationKey.Note)
        {
            if (command != "") command += ",";
            command += "Note = '" + SOut.String(registrationKey.Note) + "'";
        }

        if (registrationKey.DateStarted.Date != oldRegistrationKey.DateStarted.Date)
        {
            if (command != "") command += ",";
            command += "DateStarted = " + SOut.Date(registrationKey.DateStarted) + "";
        }

        if (registrationKey.DateDisabled.Date != oldRegistrationKey.DateDisabled.Date)
        {
            if (command != "") command += ",";
            command += "DateDisabled = " + SOut.Date(registrationKey.DateDisabled) + "";
        }

        if (registrationKey.DateEnded.Date != oldRegistrationKey.DateEnded.Date)
        {
            if (command != "") command += ",";
            command += "DateEnded = " + SOut.Date(registrationKey.DateEnded) + "";
        }

        if (registrationKey.IsForeign != oldRegistrationKey.IsForeign)
        {
            if (command != "") command += ",";
            command += "IsForeign = " + SOut.Bool(registrationKey.IsForeign) + "";
        }

        if (registrationKey.UsesServerVersion != oldRegistrationKey.UsesServerVersion)
        {
            if (command != "") command += ",";
            command += "UsesServerVersion = " + SOut.Bool(registrationKey.UsesServerVersion) + "";
        }

        if (registrationKey.IsFreeVersion != oldRegistrationKey.IsFreeVersion)
        {
            if (command != "") command += ",";
            command += "IsFreeVersion = " + SOut.Bool(registrationKey.IsFreeVersion) + "";
        }

        if (registrationKey.IsOnlyForTesting != oldRegistrationKey.IsOnlyForTesting)
        {
            if (command != "") command += ",";
            command += "IsOnlyForTesting = " + SOut.Bool(registrationKey.IsOnlyForTesting) + "";
        }

        if (registrationKey.VotesAllotted != oldRegistrationKey.VotesAllotted)
        {
            if (command != "") command += ",";
            command += "VotesAllotted = " + SOut.Int(registrationKey.VotesAllotted) + "";
        }

        if (registrationKey.IsResellerCustomer != oldRegistrationKey.IsResellerCustomer)
        {
            if (command != "") command += ",";
            command += "IsResellerCustomer = " + SOut.Bool(registrationKey.IsResellerCustomer) + "";
        }

        if (registrationKey.HasEarlyAccess != oldRegistrationKey.HasEarlyAccess)
        {
            if (command != "") command += ",";
            command += "HasEarlyAccess = " + SOut.Bool(registrationKey.HasEarlyAccess) + "";
        }

        if (registrationKey.DateTBackupScheduled != oldRegistrationKey.DateTBackupScheduled)
        {
            if (command != "") command += ",";
            command += "DateTBackupScheduled = " + SOut.DateT(registrationKey.DateTBackupScheduled) + "";
        }

        if (registrationKey.BackupPassCode != oldRegistrationKey.BackupPassCode)
        {
            if (command != "") command += ",";
            command += "BackupPassCode = '" + SOut.String(registrationKey.BackupPassCode) + "'";
        }

        if (command == "") return false;
        command = "UPDATE registrationkey SET " + command
                                                + " WHERE RegistrationKeyNum = " + SOut.Long(registrationKey.RegistrationKeyNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(RegistrationKey registrationKey, RegistrationKey oldRegistrationKey)
    {
        if (registrationKey.PatNum != oldRegistrationKey.PatNum) return true;
        if (registrationKey.RegKey != oldRegistrationKey.RegKey) return true;
        if (registrationKey.Note != oldRegistrationKey.Note) return true;
        if (registrationKey.DateStarted.Date != oldRegistrationKey.DateStarted.Date) return true;
        if (registrationKey.DateDisabled.Date != oldRegistrationKey.DateDisabled.Date) return true;
        if (registrationKey.DateEnded.Date != oldRegistrationKey.DateEnded.Date) return true;
        if (registrationKey.IsForeign != oldRegistrationKey.IsForeign) return true;
        if (registrationKey.UsesServerVersion != oldRegistrationKey.UsesServerVersion) return true;
        if (registrationKey.IsFreeVersion != oldRegistrationKey.IsFreeVersion) return true;
        if (registrationKey.IsOnlyForTesting != oldRegistrationKey.IsOnlyForTesting) return true;
        if (registrationKey.VotesAllotted != oldRegistrationKey.VotesAllotted) return true;
        if (registrationKey.IsResellerCustomer != oldRegistrationKey.IsResellerCustomer) return true;
        if (registrationKey.HasEarlyAccess != oldRegistrationKey.HasEarlyAccess) return true;
        if (registrationKey.DateTBackupScheduled != oldRegistrationKey.DateTBackupScheduled) return true;
        if (registrationKey.BackupPassCode != oldRegistrationKey.BackupPassCode) return true;
        return false;
    }

    public static void Delete(long registrationKeyNum)
    {
        var command = "DELETE FROM registrationkey "
                      + "WHERE RegistrationKeyNum = " + SOut.Long(registrationKeyNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRegistrationKeyNums)
    {
        if (listRegistrationKeyNums == null || listRegistrationKeyNums.Count == 0) return;
        var command = "DELETE FROM registrationkey "
                      + "WHERE RegistrationKeyNum IN(" + string.Join(",", listRegistrationKeyNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}