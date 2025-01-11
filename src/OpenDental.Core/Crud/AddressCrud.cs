using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AddressCrud
{
    public static Address SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Address> TableToList(DataTable table)
    {
        var retVal = new List<Address>();
        Address address;
        foreach (DataRow row in table.Rows)
        {
            address = new Address();
            address.AddressNum = SIn.Long(row["AddressNum"].ToString());
            address.Address1 = SIn.String(row["Address1"].ToString());
            address.Address2 = SIn.String(row["Address2"].ToString());
            address.City = SIn.String(row["City"].ToString());
            address.State = SIn.String(row["State"].ToString());
            address.Zip = SIn.String(row["Zip"].ToString());
            address.PatNumTaxPhysical = SIn.Long(row["PatNumTaxPhysical"].ToString());
            retVal.Add(address);
        }

        return retVal;
    }

    public static long Insert(Address address)
    {
        var command = "INSERT INTO address (";

        command += "Address1,Address2,City,State,Zip,PatNumTaxPhysical) VALUES(";

        command +=
            "'" + SOut.String(address.Address1) + "',"
            + "'" + SOut.String(address.Address2) + "',"
            + "'" + SOut.String(address.City) + "',"
            + "'" + SOut.String(address.State) + "',"
            + "'" + SOut.String(address.Zip) + "',"
            + SOut.Long(address.PatNumTaxPhysical) + ")";
        {
            address.AddressNum = Db.NonQ(command, true, "AddressNum", "address");
        }
        return address.AddressNum;
    }

    public static void Update(Address address)
    {
        var command = "UPDATE address SET "
                      + "Address1         = '" + SOut.String(address.Address1) + "', "
                      + "Address2         = '" + SOut.String(address.Address2) + "', "
                      + "City             = '" + SOut.String(address.City) + "', "
                      + "State            = '" + SOut.String(address.State) + "', "
                      + "Zip              = '" + SOut.String(address.Zip) + "', "
                      + "PatNumTaxPhysical=  " + SOut.Long(address.PatNumTaxPhysical) + " "
                      + "WHERE AddressNum = " + SOut.Long(address.AddressNum);
        Db.NonQ(command);
    }

    public static void Delete(long addressNum)
    {
        var command = "DELETE FROM address "
                      + "WHERE AddressNum = " + SOut.Long(addressNum);
        Db.NonQ(command);
    }
}