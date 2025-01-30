using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ImagingDeviceCrud
{
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

    public static void Insert(ImagingDevice imagingDevice)
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
}