#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ImagingDeviceCrud
{
    public static ImagingDevice SelectOne(long imagingDeviceNum)
    {
        var command = "SELECT * FROM imagingdevice "
                      + "WHERE ImagingDeviceNum = " + SOut.Long(imagingDeviceNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ImagingDevice SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ImagingDevice> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ImagingDevice> TableToList(DataTable table)
    {
        var retVal = new List<ImagingDevice>();
        ImagingDevice imagingDevice;
        foreach (DataRow row in table.Rows)
        {
            imagingDevice = new ImagingDevice();
            imagingDevice.ImagingDeviceNum = SIn.Long(row["ImagingDeviceNum"].ToString());
            imagingDevice.Description = SIn.String(row["Description"].ToString());
            imagingDevice.ComputerName = SIn.String(row["ComputerName"].ToString());
            imagingDevice.DeviceType = (EnumImgDeviceType) SIn.Int(row["DeviceType"].ToString());
            imagingDevice.TwainName = SIn.String(row["TwainName"].ToString());
            imagingDevice.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            imagingDevice.ShowTwainUI = SIn.Bool(row["ShowTwainUI"].ToString());
            retVal.Add(imagingDevice);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ImagingDevice> listImagingDevices, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ImagingDevice";
        var table = new DataTable(tableName);
        table.Columns.Add("ImagingDeviceNum");
        table.Columns.Add("Description");
        table.Columns.Add("ComputerName");
        table.Columns.Add("DeviceType");
        table.Columns.Add("TwainName");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("ShowTwainUI");
        foreach (var imagingDevice in listImagingDevices)
            table.Rows.Add(SOut.Long(imagingDevice.ImagingDeviceNum), imagingDevice.Description, imagingDevice.ComputerName, SOut.Int((int) imagingDevice.DeviceType), imagingDevice.TwainName, SOut.Int(imagingDevice.ItemOrder), SOut.Bool(imagingDevice.ShowTwainUI));
        return table;
    }

    public static long Insert(ImagingDevice imagingDevice)
    {
        return Insert(imagingDevice, false);
    }

    public static long Insert(ImagingDevice imagingDevice, bool useExistingPK)
    {
        var command = "INSERT INTO imagingdevice (";

        command += "Description,ComputerName,DeviceType,TwainName,ItemOrder,ShowTwainUI) VALUES(";

        command +=
            "'" + SOut.String(imagingDevice.Description) + "',"
            + "'" + SOut.String(imagingDevice.ComputerName) + "',"
            + SOut.Int((int) imagingDevice.DeviceType) + ","
            + "'" + SOut.String(imagingDevice.TwainName) + "',"
            + SOut.Int(imagingDevice.ItemOrder) + ","
            + SOut.Bool(imagingDevice.ShowTwainUI) + ")";
        {
            imagingDevice.ImagingDeviceNum = Db.NonQ(command, true, "ImagingDeviceNum", "imagingDevice");
        }
        return imagingDevice.ImagingDeviceNum;
    }

    public static long InsertNoCache(ImagingDevice imagingDevice)
    {
        return InsertNoCache(imagingDevice, false);
    }

    public static long InsertNoCache(ImagingDevice imagingDevice, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO imagingdevice (";
        if (isRandomKeys || useExistingPK) command += "ImagingDeviceNum,";
        command += "Description,ComputerName,DeviceType,TwainName,ItemOrder,ShowTwainUI) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(imagingDevice.ImagingDeviceNum) + ",";
        command +=
            "'" + SOut.String(imagingDevice.Description) + "',"
            + "'" + SOut.String(imagingDevice.ComputerName) + "',"
            + SOut.Int((int) imagingDevice.DeviceType) + ","
            + "'" + SOut.String(imagingDevice.TwainName) + "',"
            + SOut.Int(imagingDevice.ItemOrder) + ","
            + SOut.Bool(imagingDevice.ShowTwainUI) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            imagingDevice.ImagingDeviceNum = Db.NonQ(command, true, "ImagingDeviceNum", "imagingDevice");
        return imagingDevice.ImagingDeviceNum;
    }

    public static void Update(ImagingDevice imagingDevice)
    {
        var command = "UPDATE imagingdevice SET "
                      + "Description     = '" + SOut.String(imagingDevice.Description) + "', "
                      + "ComputerName    = '" + SOut.String(imagingDevice.ComputerName) + "', "
                      + "DeviceType      =  " + SOut.Int((int) imagingDevice.DeviceType) + ", "
                      + "TwainName       = '" + SOut.String(imagingDevice.TwainName) + "', "
                      + "ItemOrder       =  " + SOut.Int(imagingDevice.ItemOrder) + ", "
                      + "ShowTwainUI     =  " + SOut.Bool(imagingDevice.ShowTwainUI) + " "
                      + "WHERE ImagingDeviceNum = " + SOut.Long(imagingDevice.ImagingDeviceNum);
        Db.NonQ(command);
    }

    public static bool Update(ImagingDevice imagingDevice, ImagingDevice oldImagingDevice)
    {
        var command = "";
        if (imagingDevice.Description != oldImagingDevice.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(imagingDevice.Description) + "'";
        }

        if (imagingDevice.ComputerName != oldImagingDevice.ComputerName)
        {
            if (command != "") command += ",";
            command += "ComputerName = '" + SOut.String(imagingDevice.ComputerName) + "'";
        }

        if (imagingDevice.DeviceType != oldImagingDevice.DeviceType)
        {
            if (command != "") command += ",";
            command += "DeviceType = " + SOut.Int((int) imagingDevice.DeviceType) + "";
        }

        if (imagingDevice.TwainName != oldImagingDevice.TwainName)
        {
            if (command != "") command += ",";
            command += "TwainName = '" + SOut.String(imagingDevice.TwainName) + "'";
        }

        if (imagingDevice.ItemOrder != oldImagingDevice.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(imagingDevice.ItemOrder) + "";
        }

        if (imagingDevice.ShowTwainUI != oldImagingDevice.ShowTwainUI)
        {
            if (command != "") command += ",";
            command += "ShowTwainUI = " + SOut.Bool(imagingDevice.ShowTwainUI) + "";
        }

        if (command == "") return false;
        command = "UPDATE imagingdevice SET " + command
                                              + " WHERE ImagingDeviceNum = " + SOut.Long(imagingDevice.ImagingDeviceNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ImagingDevice imagingDevice, ImagingDevice oldImagingDevice)
    {
        if (imagingDevice.Description != oldImagingDevice.Description) return true;
        if (imagingDevice.ComputerName != oldImagingDevice.ComputerName) return true;
        if (imagingDevice.DeviceType != oldImagingDevice.DeviceType) return true;
        if (imagingDevice.TwainName != oldImagingDevice.TwainName) return true;
        if (imagingDevice.ItemOrder != oldImagingDevice.ItemOrder) return true;
        if (imagingDevice.ShowTwainUI != oldImagingDevice.ShowTwainUI) return true;
        return false;
    }

    public static void Delete(long imagingDeviceNum)
    {
        var command = "DELETE FROM imagingdevice "
                      + "WHERE ImagingDeviceNum = " + SOut.Long(imagingDeviceNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listImagingDeviceNums)
    {
        if (listImagingDeviceNums == null || listImagingDeviceNums.Count == 0) return;
        var command = "DELETE FROM imagingdevice "
                      + "WHERE ImagingDeviceNum IN(" + string.Join(",", listImagingDeviceNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}