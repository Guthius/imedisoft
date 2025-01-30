using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MedLabFacAttachCrud
{
    public static List<MedLabFacAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedLabFacAttach> TableToList(DataTable table)
    {
        var retVal = new List<MedLabFacAttach>();
        MedLabFacAttach medLabFacAttach;
        foreach (DataRow row in table.Rows)
        {
            medLabFacAttach = new MedLabFacAttach();
            medLabFacAttach.MedLabFacAttachNum = SIn.Long(row["MedLabFacAttachNum"].ToString());
            medLabFacAttach.MedLabNum = SIn.Long(row["MedLabNum"].ToString());
            medLabFacAttach.MedLabResultNum = SIn.Long(row["MedLabResultNum"].ToString());
            medLabFacAttach.MedLabFacilityNum = SIn.Long(row["MedLabFacilityNum"].ToString());
            retVal.Add(medLabFacAttach);
        }

        return retVal;
    }

    public static void Insert(MedLabFacAttach medLabFacAttach)
    {
        var command = "INSERT INTO medlabfacattach (";

        command += "MedLabNum,MedLabResultNum,MedLabFacilityNum) VALUES(";

        command +=
            SOut.Long(medLabFacAttach.MedLabNum) + ","
                                                 + SOut.Long(medLabFacAttach.MedLabResultNum) + ","
                                                 + SOut.Long(medLabFacAttach.MedLabFacilityNum) + ")";
        {
            medLabFacAttach.MedLabFacAttachNum = Db.NonQ(command, true, "MedLabFacAttachNum", "medLabFacAttach");
        }
    }
}