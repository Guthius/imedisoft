using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MedLabFacilityCrud
{
    public static MedLabFacility SelectOne(long medLabFacilityNum)
    {
        var command = "SELECT * FROM medlabfacility "
                      + "WHERE MedLabFacilityNum = " + SOut.Long(medLabFacilityNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedLabFacility SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedLabFacility> TableToList(DataTable table)
    {
        var retVal = new List<MedLabFacility>();
        MedLabFacility medLabFacility;
        foreach (DataRow row in table.Rows)
        {
            medLabFacility = new MedLabFacility();
            medLabFacility.MedLabFacilityNum = SIn.Long(row["MedLabFacilityNum"].ToString());
            medLabFacility.FacilityName = SIn.String(row["FacilityName"].ToString());
            medLabFacility.Address = SIn.String(row["Address"].ToString());
            medLabFacility.City = SIn.String(row["City"].ToString());
            medLabFacility.State = SIn.String(row["State"].ToString());
            medLabFacility.Zip = SIn.String(row["Zip"].ToString());
            medLabFacility.Phone = SIn.String(row["Phone"].ToString());
            medLabFacility.DirectorTitle = SIn.String(row["DirectorTitle"].ToString());
            medLabFacility.DirectorLName = SIn.String(row["DirectorLName"].ToString());
            medLabFacility.DirectorFName = SIn.String(row["DirectorFName"].ToString());
            retVal.Add(medLabFacility);
        }

        return retVal;
    }

    public static long Insert(MedLabFacility medLabFacility)
    {
        var command = "INSERT INTO medlabfacility (";

        command += "FacilityName,Address,City,State,Zip,Phone,DirectorTitle,DirectorLName,DirectorFName) VALUES(";

        command +=
            "'" + SOut.String(medLabFacility.FacilityName) + "',"
            + "'" + SOut.String(medLabFacility.Address) + "',"
            + "'" + SOut.String(medLabFacility.City) + "',"
            + "'" + SOut.String(medLabFacility.State) + "',"
            + "'" + SOut.String(medLabFacility.Zip) + "',"
            + "'" + SOut.String(medLabFacility.Phone) + "',"
            + "'" + SOut.String(medLabFacility.DirectorTitle) + "',"
            + "'" + SOut.String(medLabFacility.DirectorLName) + "',"
            + "'" + SOut.String(medLabFacility.DirectorFName) + "')";
        {
            medLabFacility.MedLabFacilityNum = Db.NonQ(command, true, "MedLabFacilityNum", "medLabFacility");
        }
        return medLabFacility.MedLabFacilityNum;
    }
}