using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CloudAddresses
{
    #region Methods - Get

    public static List<CloudAddress> GetAll()
    {
        var command = "SELECT * FROM cloudaddress;";
        return CloudAddressCrud.SelectMany(command);
    }

    ///<summary>Gets one CloudAddress from the db.</summary>
    public static CloudAddress GetByIpAddress(string ipAddress)
    {
        var command = $"SELECT * FROM cloudaddress WHERE IpAddress='{SOut.String(ipAddress)}'";
        return CloudAddressCrud.SelectOne(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(CloudAddress cloudAddress)
    {
        return CloudAddressCrud.Insert(cloudAddress);
    }

    
    public static void Update(CloudAddress cloudAddress)
    {
        CloudAddressCrud.Update(cloudAddress);
    }

    public static void DeleteMany(List<long> listCloudAddressNums)
    {
        CloudAddressCrud.DeleteMany(listCloudAddressNums);
    }

    #endregion Methods - Modify
}