#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MobileNotificationCrud
{
    public static MobileNotification SelectOne(long mobileNotificationNum)
    {
        var command = "SELECT * FROM mobilenotification "
                      + "WHERE MobileNotificationNum = " + SOut.Long(mobileNotificationNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MobileNotification SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MobileNotification> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MobileNotification> TableToList(DataTable table)
    {
        var retVal = new List<MobileNotification>();
        MobileNotification mobileNotification;
        foreach (DataRow row in table.Rows)
        {
            mobileNotification = new MobileNotification();
            mobileNotification.MobileNotificationNum = SIn.Long(row["MobileNotificationNum"].ToString());
            mobileNotification.NotificationType = (MobileNotificationType) SIn.Int(row["NotificationType"].ToString());
            mobileNotification.DeviceId = SIn.String(row["DeviceId"].ToString());
            mobileNotification.PrimaryKeys = SIn.String(row["PrimaryKeys"].ToString());
            mobileNotification.Tags = SIn.String(row["Tags"].ToString());
            mobileNotification.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            mobileNotification.DateTimeExpires = SIn.DateTime(row["DateTimeExpires"].ToString());
            mobileNotification.AppTarget = (EnumAppTarget) SIn.Int(row["AppTarget"].ToString());
            retVal.Add(mobileNotification);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MobileNotification> listMobileNotifications, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MobileNotification";
        var table = new DataTable(tableName);
        table.Columns.Add("MobileNotificationNum");
        table.Columns.Add("NotificationType");
        table.Columns.Add("DeviceId");
        table.Columns.Add("PrimaryKeys");
        table.Columns.Add("Tags");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("DateTimeExpires");
        table.Columns.Add("AppTarget");
        foreach (var mobileNotification in listMobileNotifications)
            table.Rows.Add(SOut.Long(mobileNotification.MobileNotificationNum), SOut.Int((int) mobileNotification.NotificationType), mobileNotification.DeviceId, mobileNotification.PrimaryKeys, mobileNotification.Tags, SOut.DateT(mobileNotification.DateTimeEntry, false), SOut.DateT(mobileNotification.DateTimeExpires, false), SOut.Int((int) mobileNotification.AppTarget));
        return table;
    }

    public static long Insert(MobileNotification mobileNotification)
    {
        return Insert(mobileNotification, false);
    }

    public static long Insert(MobileNotification mobileNotification, bool useExistingPK)
    {
        var command = "INSERT INTO mobilenotification (";

        command += "NotificationType,DeviceId,PrimaryKeys,Tags,DateTimeEntry,DateTimeExpires,AppTarget) VALUES(";

        command +=
            SOut.Int((int) mobileNotification.NotificationType) + ","
                                                                + "'" + SOut.String(mobileNotification.DeviceId) + "',"
                                                                + DbHelper.ParamChar + "paramPrimaryKeys,"
                                                                + DbHelper.ParamChar + "paramTags,"
                                                                + SOut.DateT(mobileNotification.DateTimeEntry) + ","
                                                                + SOut.DateT(mobileNotification.DateTimeExpires) + ","
                                                                + SOut.Int((int) mobileNotification.AppTarget) + ")";
        if (mobileNotification.PrimaryKeys == null) mobileNotification.PrimaryKeys = "";
        var paramPrimaryKeys = new OdSqlParameter("paramPrimaryKeys", OdDbType.Text, SOut.StringParam(mobileNotification.PrimaryKeys));
        if (mobileNotification.Tags == null) mobileNotification.Tags = "";
        var paramTags = new OdSqlParameter("paramTags", OdDbType.Text, SOut.StringParam(mobileNotification.Tags));
        {
            mobileNotification.MobileNotificationNum = Db.NonQ(command, true, "MobileNotificationNum", "mobileNotification", paramPrimaryKeys, paramTags);
        }
        return mobileNotification.MobileNotificationNum;
    }

    public static long InsertNoCache(MobileNotification mobileNotification)
    {
        return InsertNoCache(mobileNotification, false);
    }

    public static long InsertNoCache(MobileNotification mobileNotification, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO mobilenotification (";
        if (isRandomKeys || useExistingPK) command += "MobileNotificationNum,";
        command += "NotificationType,DeviceId,PrimaryKeys,Tags,DateTimeEntry,DateTimeExpires,AppTarget) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(mobileNotification.MobileNotificationNum) + ",";
        command +=
            SOut.Int((int) mobileNotification.NotificationType) + ","
                                                                + "'" + SOut.String(mobileNotification.DeviceId) + "',"
                                                                + DbHelper.ParamChar + "paramPrimaryKeys,"
                                                                + DbHelper.ParamChar + "paramTags,"
                                                                + SOut.DateT(mobileNotification.DateTimeEntry) + ","
                                                                + SOut.DateT(mobileNotification.DateTimeExpires) + ","
                                                                + SOut.Int((int) mobileNotification.AppTarget) + ")";
        if (mobileNotification.PrimaryKeys == null) mobileNotification.PrimaryKeys = "";
        var paramPrimaryKeys = new OdSqlParameter("paramPrimaryKeys", OdDbType.Text, SOut.StringParam(mobileNotification.PrimaryKeys));
        if (mobileNotification.Tags == null) mobileNotification.Tags = "";
        var paramTags = new OdSqlParameter("paramTags", OdDbType.Text, SOut.StringParam(mobileNotification.Tags));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPrimaryKeys, paramTags);
        else
            mobileNotification.MobileNotificationNum = Db.NonQ(command, true, "MobileNotificationNum", "mobileNotification", paramPrimaryKeys, paramTags);
        return mobileNotification.MobileNotificationNum;
    }

    public static void Update(MobileNotification mobileNotification)
    {
        var command = "UPDATE mobilenotification SET "
                      + "NotificationType     =  " + SOut.Int((int) mobileNotification.NotificationType) + ", "
                      + "DeviceId             = '" + SOut.String(mobileNotification.DeviceId) + "', "
                      + "PrimaryKeys          =  " + DbHelper.ParamChar + "paramPrimaryKeys, "
                      + "Tags                 =  " + DbHelper.ParamChar + "paramTags, "
                      + "DateTimeEntry        =  " + SOut.DateT(mobileNotification.DateTimeEntry) + ", "
                      + "DateTimeExpires      =  " + SOut.DateT(mobileNotification.DateTimeExpires) + ", "
                      + "AppTarget            =  " + SOut.Int((int) mobileNotification.AppTarget) + " "
                      + "WHERE MobileNotificationNum = " + SOut.Long(mobileNotification.MobileNotificationNum);
        if (mobileNotification.PrimaryKeys == null) mobileNotification.PrimaryKeys = "";
        var paramPrimaryKeys = new OdSqlParameter("paramPrimaryKeys", OdDbType.Text, SOut.StringParam(mobileNotification.PrimaryKeys));
        if (mobileNotification.Tags == null) mobileNotification.Tags = "";
        var paramTags = new OdSqlParameter("paramTags", OdDbType.Text, SOut.StringParam(mobileNotification.Tags));
        Db.NonQ(command, paramPrimaryKeys, paramTags);
    }

    public static bool Update(MobileNotification mobileNotification, MobileNotification oldMobileNotification)
    {
        var command = "";
        if (mobileNotification.NotificationType != oldMobileNotification.NotificationType)
        {
            if (command != "") command += ",";
            command += "NotificationType = " + SOut.Int((int) mobileNotification.NotificationType) + "";
        }

        if (mobileNotification.DeviceId != oldMobileNotification.DeviceId)
        {
            if (command != "") command += ",";
            command += "DeviceId = '" + SOut.String(mobileNotification.DeviceId) + "'";
        }

        if (mobileNotification.PrimaryKeys != oldMobileNotification.PrimaryKeys)
        {
            if (command != "") command += ",";
            command += "PrimaryKeys = " + DbHelper.ParamChar + "paramPrimaryKeys";
        }

        if (mobileNotification.Tags != oldMobileNotification.Tags)
        {
            if (command != "") command += ",";
            command += "Tags = " + DbHelper.ParamChar + "paramTags";
        }

        if (mobileNotification.DateTimeEntry != oldMobileNotification.DateTimeEntry)
        {
            if (command != "") command += ",";
            command += "DateTimeEntry = " + SOut.DateT(mobileNotification.DateTimeEntry) + "";
        }

        if (mobileNotification.DateTimeExpires != oldMobileNotification.DateTimeExpires)
        {
            if (command != "") command += ",";
            command += "DateTimeExpires = " + SOut.DateT(mobileNotification.DateTimeExpires) + "";
        }

        if (mobileNotification.AppTarget != oldMobileNotification.AppTarget)
        {
            if (command != "") command += ",";
            command += "AppTarget = " + SOut.Int((int) mobileNotification.AppTarget) + "";
        }

        if (command == "") return false;
        if (mobileNotification.PrimaryKeys == null) mobileNotification.PrimaryKeys = "";
        var paramPrimaryKeys = new OdSqlParameter("paramPrimaryKeys", OdDbType.Text, SOut.StringParam(mobileNotification.PrimaryKeys));
        if (mobileNotification.Tags == null) mobileNotification.Tags = "";
        var paramTags = new OdSqlParameter("paramTags", OdDbType.Text, SOut.StringParam(mobileNotification.Tags));
        command = "UPDATE mobilenotification SET " + command
                                                   + " WHERE MobileNotificationNum = " + SOut.Long(mobileNotification.MobileNotificationNum);
        Db.NonQ(command, paramPrimaryKeys, paramTags);
        return true;
    }

    public static bool UpdateComparison(MobileNotification mobileNotification, MobileNotification oldMobileNotification)
    {
        if (mobileNotification.NotificationType != oldMobileNotification.NotificationType) return true;
        if (mobileNotification.DeviceId != oldMobileNotification.DeviceId) return true;
        if (mobileNotification.PrimaryKeys != oldMobileNotification.PrimaryKeys) return true;
        if (mobileNotification.Tags != oldMobileNotification.Tags) return true;
        if (mobileNotification.DateTimeEntry != oldMobileNotification.DateTimeEntry) return true;
        if (mobileNotification.DateTimeExpires != oldMobileNotification.DateTimeExpires) return true;
        if (mobileNotification.AppTarget != oldMobileNotification.AppTarget) return true;
        return false;
    }

    public static void Delete(long mobileNotificationNum)
    {
        var command = "DELETE FROM mobilenotification "
                      + "WHERE MobileNotificationNum = " + SOut.Long(mobileNotificationNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMobileNotificationNums)
    {
        if (listMobileNotificationNums == null || listMobileNotificationNums.Count == 0) return;
        var command = "DELETE FROM mobilenotification "
                      + "WHERE MobileNotificationNum IN(" + string.Join(",", listMobileNotificationNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}