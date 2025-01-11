using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CloudAddressCrud
{
    public static CloudAddress SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<CloudAddress> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CloudAddress> TableToList(DataTable table)
    {
        var retVal = new List<CloudAddress>();
        CloudAddress cloudAddress;
        foreach (DataRow row in table.Rows)
        {
            cloudAddress = new CloudAddress();
            cloudAddress.CloudAddressNum = SIn.Long(row["CloudAddressNum"].ToString());
            cloudAddress.IpAddress = SIn.String(row["IpAddress"].ToString());
            cloudAddress.UserNumLastConnect = SIn.Long(row["UserNumLastConnect"].ToString());
            cloudAddress.DateTimeLastConnect = SIn.DateTime(row["DateTimeLastConnect"].ToString());
            retVal.Add(cloudAddress);
        }

        return retVal;
    }

    public static long Insert(CloudAddress cloudAddress)
    {
        var command = "INSERT INTO cloudaddress (";

        command += "IpAddress,UserNumLastConnect,DateTimeLastConnect) VALUES(";

        command +=
            "'" + SOut.String(cloudAddress.IpAddress) + "',"
            + SOut.Long(cloudAddress.UserNumLastConnect) + ","
            + SOut.DateT(cloudAddress.DateTimeLastConnect) + ")";
        {
            cloudAddress.CloudAddressNum = Db.NonQ(command, true, "CloudAddressNum", "cloudAddress");
        }
        return cloudAddress.CloudAddressNum;
    }

    public static void Update(CloudAddress cloudAddress)
    {
        var command = "UPDATE cloudaddress SET "
                      + "IpAddress          = '" + SOut.String(cloudAddress.IpAddress) + "', "
                      + "UserNumLastConnect =  " + SOut.Long(cloudAddress.UserNumLastConnect) + ", "
                      + "DateTimeLastConnect=  " + SOut.DateT(cloudAddress.DateTimeLastConnect) + " "
                      + "WHERE CloudAddressNum = " + SOut.Long(cloudAddress.CloudAddressNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listCloudAddressNums)
    {
        if (listCloudAddressNums == null || listCloudAddressNums.Count == 0) return;
        var command = "DELETE FROM cloudaddress "
                      + "WHERE CloudAddressNum IN(" + string.Join(",", listCloudAddressNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}