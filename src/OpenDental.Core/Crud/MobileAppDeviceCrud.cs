#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MobileAppDeviceCrud
{
    public static MobileAppDevice SelectOne(long mobileAppDeviceNum)
    {
        var command = "SELECT * FROM mobileappdevice "
                      + "WHERE MobileAppDeviceNum = " + SOut.Long(mobileAppDeviceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MobileAppDevice SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MobileAppDevice> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MobileAppDevice> TableToList(DataTable table)
    {
        var retVal = new List<MobileAppDevice>();
        MobileAppDevice mobileAppDevice;
        foreach (DataRow row in table.Rows)
        {
            mobileAppDevice = new MobileAppDevice();
            mobileAppDevice.MobileAppDeviceNum = SIn.Long(row["MobileAppDeviceNum"].ToString());
            mobileAppDevice.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            mobileAppDevice.DeviceName = SIn.String(row["DeviceName"].ToString());
            mobileAppDevice.UniqueID = SIn.String(row["UniqueID"].ToString());
            mobileAppDevice.IsEclipboardEnabled = SIn.Bool(row["IsEclipboardEnabled"].ToString());
            mobileAppDevice.PatNum = SIn.Long(row["PatNum"].ToString());
            mobileAppDevice.IsBYODDevice = SIn.Bool(row["IsBYODDevice"].ToString());
            mobileAppDevice.LastCheckInActivity = SIn.DateTime(row["LastCheckInActivity"].ToString());
            mobileAppDevice.EclipboardLastAttempt = SIn.DateTime(row["EclipboardLastAttempt"].ToString());
            mobileAppDevice.EclipboardLastLogin = SIn.DateTime(row["EclipboardLastLogin"].ToString());
            mobileAppDevice.DevicePage = (MADPage) SIn.Int(row["DevicePage"].ToString());
            mobileAppDevice.UserNum = SIn.Long(row["UserNum"].ToString());
            mobileAppDevice.ODTouchLastLogin = SIn.DateTime(row["ODTouchLastLogin"].ToString());
            mobileAppDevice.ODTouchLastAttempt = SIn.DateTime(row["ODTouchLastAttempt"].ToString());
            mobileAppDevice.IsODTouchEnabled = SIn.Bool(row["IsODTouchEnabled"].ToString());
            retVal.Add(mobileAppDevice);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MobileAppDevice> listMobileAppDevices, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MobileAppDevice";
        var table = new DataTable(tableName);
        table.Columns.Add("MobileAppDeviceNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("DeviceName");
        table.Columns.Add("UniqueID");
        table.Columns.Add("IsEclipboardEnabled");
        table.Columns.Add("PatNum");
        table.Columns.Add("IsBYODDevice");
        table.Columns.Add("LastCheckInActivity");
        table.Columns.Add("EclipboardLastAttempt");
        table.Columns.Add("EclipboardLastLogin");
        table.Columns.Add("DevicePage");
        table.Columns.Add("UserNum");
        table.Columns.Add("ODTouchLastLogin");
        table.Columns.Add("ODTouchLastAttempt");
        table.Columns.Add("IsODTouchEnabled");
        foreach (var mobileAppDevice in listMobileAppDevices)
            table.Rows.Add(SOut.Long(mobileAppDevice.MobileAppDeviceNum), SOut.Long(mobileAppDevice.ClinicNum), mobileAppDevice.DeviceName, mobileAppDevice.UniqueID, SOut.Bool(mobileAppDevice.IsEclipboardEnabled), SOut.Long(mobileAppDevice.PatNum), SOut.Bool(mobileAppDevice.IsBYODDevice), SOut.DateT(mobileAppDevice.LastCheckInActivity, false), SOut.DateT(mobileAppDevice.EclipboardLastAttempt, false), SOut.DateT(mobileAppDevice.EclipboardLastLogin, false), SOut.Int((int) mobileAppDevice.DevicePage), SOut.Long(mobileAppDevice.UserNum), SOut.DateT(mobileAppDevice.ODTouchLastLogin, false), SOut.DateT(mobileAppDevice.ODTouchLastAttempt, false), SOut.Bool(mobileAppDevice.IsODTouchEnabled));
        return table;
    }

    public static long Insert(MobileAppDevice mobileAppDevice)
    {
        return Insert(mobileAppDevice, false);
    }

    public static long Insert(MobileAppDevice mobileAppDevice, bool useExistingPK)
    {
        var command = "INSERT INTO mobileappdevice (";

        command += "ClinicNum,DeviceName,UniqueID,IsEclipboardEnabled,PatNum,IsBYODDevice,LastCheckInActivity,EclipboardLastAttempt,EclipboardLastLogin,DevicePage,UserNum,ODTouchLastLogin,ODTouchLastAttempt,IsODTouchEnabled) VALUES(";

        command +=
            SOut.Long(mobileAppDevice.ClinicNum) + ","
                                                 + "'" + SOut.String(mobileAppDevice.DeviceName) + "',"
                                                 + "'" + SOut.String(mobileAppDevice.UniqueID) + "',"
                                                 + SOut.Bool(mobileAppDevice.IsEclipboardEnabled) + ","
                                                 + SOut.Long(mobileAppDevice.PatNum) + ","
                                                 + SOut.Bool(mobileAppDevice.IsBYODDevice) + ","
                                                 + SOut.DateT(mobileAppDevice.LastCheckInActivity) + ","
                                                 + SOut.DateT(mobileAppDevice.EclipboardLastAttempt) + ","
                                                 + SOut.DateT(mobileAppDevice.EclipboardLastLogin) + ","
                                                 + SOut.Int((int) mobileAppDevice.DevicePage) + ","
                                                 + SOut.Long(mobileAppDevice.UserNum) + ","
                                                 + SOut.DateT(mobileAppDevice.ODTouchLastLogin) + ","
                                                 + SOut.DateT(mobileAppDevice.ODTouchLastAttempt) + ","
                                                 + SOut.Bool(mobileAppDevice.IsODTouchEnabled) + ")";
        {
            mobileAppDevice.MobileAppDeviceNum = Db.NonQ(command, true, "MobileAppDeviceNum", "mobileAppDevice");
        }
        return mobileAppDevice.MobileAppDeviceNum;
    }

    public static long InsertNoCache(MobileAppDevice mobileAppDevice)
    {
        return InsertNoCache(mobileAppDevice, false);
    }

    public static long InsertNoCache(MobileAppDevice mobileAppDevice, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mobileappdevice (";
        if (isRandomKeys || useExistingPK) command += "MobileAppDeviceNum,";
        command += "ClinicNum,DeviceName,UniqueID,IsEclipboardEnabled,PatNum,IsBYODDevice,LastCheckInActivity,EclipboardLastAttempt,EclipboardLastLogin,DevicePage,UserNum,ODTouchLastLogin,ODTouchLastAttempt,IsODTouchEnabled) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mobileAppDevice.MobileAppDeviceNum) + ",";
        command +=
            SOut.Long(mobileAppDevice.ClinicNum) + ","
                                                 + "'" + SOut.String(mobileAppDevice.DeviceName) + "',"
                                                 + "'" + SOut.String(mobileAppDevice.UniqueID) + "',"
                                                 + SOut.Bool(mobileAppDevice.IsEclipboardEnabled) + ","
                                                 + SOut.Long(mobileAppDevice.PatNum) + ","
                                                 + SOut.Bool(mobileAppDevice.IsBYODDevice) + ","
                                                 + SOut.DateT(mobileAppDevice.LastCheckInActivity) + ","
                                                 + SOut.DateT(mobileAppDevice.EclipboardLastAttempt) + ","
                                                 + SOut.DateT(mobileAppDevice.EclipboardLastLogin) + ","
                                                 + SOut.Int((int) mobileAppDevice.DevicePage) + ","
                                                 + SOut.Long(mobileAppDevice.UserNum) + ","
                                                 + SOut.DateT(mobileAppDevice.ODTouchLastLogin) + ","
                                                 + SOut.DateT(mobileAppDevice.ODTouchLastAttempt) + ","
                                                 + SOut.Bool(mobileAppDevice.IsODTouchEnabled) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            mobileAppDevice.MobileAppDeviceNum = Db.NonQ(command, true, "MobileAppDeviceNum", "mobileAppDevice");
        return mobileAppDevice.MobileAppDeviceNum;
    }

    public static void Update(MobileAppDevice mobileAppDevice)
    {
        var command = "UPDATE mobileappdevice SET "
                      + "ClinicNum            =  " + SOut.Long(mobileAppDevice.ClinicNum) + ", "
                      + "DeviceName           = '" + SOut.String(mobileAppDevice.DeviceName) + "', "
                      + "UniqueID             = '" + SOut.String(mobileAppDevice.UniqueID) + "', "
                      + "IsEclipboardEnabled  =  " + SOut.Bool(mobileAppDevice.IsEclipboardEnabled) + ", "
                      + "PatNum               =  " + SOut.Long(mobileAppDevice.PatNum) + ", "
                      + "IsBYODDevice         =  " + SOut.Bool(mobileAppDevice.IsBYODDevice) + ", "
                      + "LastCheckInActivity  =  " + SOut.DateT(mobileAppDevice.LastCheckInActivity) + ", "
                      + "EclipboardLastAttempt=  " + SOut.DateT(mobileAppDevice.EclipboardLastAttempt) + ", "
                      + "EclipboardLastLogin  =  " + SOut.DateT(mobileAppDevice.EclipboardLastLogin) + ", "
                      + "DevicePage           =  " + SOut.Int((int) mobileAppDevice.DevicePage) + ", "
                      + "UserNum              =  " + SOut.Long(mobileAppDevice.UserNum) + ", "
                      + "ODTouchLastLogin     =  " + SOut.DateT(mobileAppDevice.ODTouchLastLogin) + ", "
                      + "ODTouchLastAttempt   =  " + SOut.DateT(mobileAppDevice.ODTouchLastAttempt) + ", "
                      + "IsODTouchEnabled     =  " + SOut.Bool(mobileAppDevice.IsODTouchEnabled) + " "
                      + "WHERE MobileAppDeviceNum = " + SOut.Long(mobileAppDevice.MobileAppDeviceNum);
        Db.NonQ(command);
    }

    public static bool Update(MobileAppDevice mobileAppDevice, MobileAppDevice oldMobileAppDevice)
    {
        var command = "";
        if (mobileAppDevice.ClinicNum != oldMobileAppDevice.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(mobileAppDevice.ClinicNum) + "";
        }

        if (mobileAppDevice.DeviceName != oldMobileAppDevice.DeviceName)
        {
            if (command != "") command += ",";
            command += "DeviceName = '" + SOut.String(mobileAppDevice.DeviceName) + "'";
        }

        if (mobileAppDevice.UniqueID != oldMobileAppDevice.UniqueID)
        {
            if (command != "") command += ",";
            command += "UniqueID = '" + SOut.String(mobileAppDevice.UniqueID) + "'";
        }

        if (mobileAppDevice.IsEclipboardEnabled != oldMobileAppDevice.IsEclipboardEnabled)
        {
            if (command != "") command += ",";
            command += "IsEclipboardEnabled = " + SOut.Bool(mobileAppDevice.IsEclipboardEnabled) + "";
        }

        if (mobileAppDevice.PatNum != oldMobileAppDevice.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(mobileAppDevice.PatNum) + "";
        }

        if (mobileAppDevice.IsBYODDevice != oldMobileAppDevice.IsBYODDevice)
        {
            if (command != "") command += ",";
            command += "IsBYODDevice = " + SOut.Bool(mobileAppDevice.IsBYODDevice) + "";
        }

        if (mobileAppDevice.LastCheckInActivity != oldMobileAppDevice.LastCheckInActivity)
        {
            if (command != "") command += ",";
            command += "LastCheckInActivity = " + SOut.DateT(mobileAppDevice.LastCheckInActivity) + "";
        }

        if (mobileAppDevice.EclipboardLastAttempt != oldMobileAppDevice.EclipboardLastAttempt)
        {
            if (command != "") command += ",";
            command += "EclipboardLastAttempt = " + SOut.DateT(mobileAppDevice.EclipboardLastAttempt) + "";
        }

        if (mobileAppDevice.EclipboardLastLogin != oldMobileAppDevice.EclipboardLastLogin)
        {
            if (command != "") command += ",";
            command += "EclipboardLastLogin = " + SOut.DateT(mobileAppDevice.EclipboardLastLogin) + "";
        }

        if (mobileAppDevice.DevicePage != oldMobileAppDevice.DevicePage)
        {
            if (command != "") command += ",";
            command += "DevicePage = " + SOut.Int((int) mobileAppDevice.DevicePage) + "";
        }

        if (mobileAppDevice.UserNum != oldMobileAppDevice.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(mobileAppDevice.UserNum) + "";
        }

        if (mobileAppDevice.ODTouchLastLogin != oldMobileAppDevice.ODTouchLastLogin)
        {
            if (command != "") command += ",";
            command += "ODTouchLastLogin = " + SOut.DateT(mobileAppDevice.ODTouchLastLogin) + "";
        }

        if (mobileAppDevice.ODTouchLastAttempt != oldMobileAppDevice.ODTouchLastAttempt)
        {
            if (command != "") command += ",";
            command += "ODTouchLastAttempt = " + SOut.DateT(mobileAppDevice.ODTouchLastAttempt) + "";
        }

        if (mobileAppDevice.IsODTouchEnabled != oldMobileAppDevice.IsODTouchEnabled)
        {
            if (command != "") command += ",";
            command += "IsODTouchEnabled = " + SOut.Bool(mobileAppDevice.IsODTouchEnabled) + "";
        }

        if (command == "") return false;
        command = "UPDATE mobileappdevice SET " + command
                                                + " WHERE MobileAppDeviceNum = " + SOut.Long(mobileAppDevice.MobileAppDeviceNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(MobileAppDevice mobileAppDevice, MobileAppDevice oldMobileAppDevice)
    {
        if (mobileAppDevice.ClinicNum != oldMobileAppDevice.ClinicNum) return true;
        if (mobileAppDevice.DeviceName != oldMobileAppDevice.DeviceName) return true;
        if (mobileAppDevice.UniqueID != oldMobileAppDevice.UniqueID) return true;
        if (mobileAppDevice.IsEclipboardEnabled != oldMobileAppDevice.IsEclipboardEnabled) return true;
        if (mobileAppDevice.PatNum != oldMobileAppDevice.PatNum) return true;
        if (mobileAppDevice.IsBYODDevice != oldMobileAppDevice.IsBYODDevice) return true;
        if (mobileAppDevice.LastCheckInActivity != oldMobileAppDevice.LastCheckInActivity) return true;
        if (mobileAppDevice.EclipboardLastAttempt != oldMobileAppDevice.EclipboardLastAttempt) return true;
        if (mobileAppDevice.EclipboardLastLogin != oldMobileAppDevice.EclipboardLastLogin) return true;
        if (mobileAppDevice.DevicePage != oldMobileAppDevice.DevicePage) return true;
        if (mobileAppDevice.UserNum != oldMobileAppDevice.UserNum) return true;
        if (mobileAppDevice.ODTouchLastLogin != oldMobileAppDevice.ODTouchLastLogin) return true;
        if (mobileAppDevice.ODTouchLastAttempt != oldMobileAppDevice.ODTouchLastAttempt) return true;
        if (mobileAppDevice.IsODTouchEnabled != oldMobileAppDevice.IsODTouchEnabled) return true;
        return false;
    }

    public static void Delete(long mobileAppDeviceNum)
    {
        var command = "DELETE FROM mobileappdevice "
                      + "WHERE MobileAppDeviceNum = " + SOut.Long(mobileAppDeviceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMobileAppDeviceNums)
    {
        if (listMobileAppDeviceNums == null || listMobileAppDeviceNums.Count == 0) return;
        var command = "DELETE FROM mobileappdevice "
                      + "WHERE MobileAppDeviceNum IN(" + string.Join(",", listMobileAppDeviceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<MobileAppDevice> listNew, List<MobileAppDevice> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<MobileAppDevice>();
        var listUpdNew = new List<MobileAppDevice>();
        var listUpdDB = new List<MobileAppDevice>();
        var listDel = new List<MobileAppDevice>();
        listNew.Sort((x, y) => { return x.MobileAppDeviceNum.CompareTo(y.MobileAppDeviceNum); });
        listDB.Sort((x, y) => { return x.MobileAppDeviceNum.CompareTo(y.MobileAppDeviceNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        MobileAppDevice fieldNew;
        MobileAppDevice fieldDB;
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

            if (fieldNew.MobileAppDeviceNum < fieldDB.MobileAppDeviceNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.MobileAppDeviceNum > fieldDB.MobileAppDeviceNum)
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

        DeleteMany(listDel.Select(x => x.MobileAppDeviceNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}