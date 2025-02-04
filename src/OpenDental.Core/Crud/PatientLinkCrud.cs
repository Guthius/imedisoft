using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatientLinkCrud
{
    public static List<PatientLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatientLink> TableToList(DataTable table)
    {
        var retVal = new List<PatientLink>();
        foreach (DataRow row in table.Rows)
        {
            var patientLink = new PatientLink
            {
                PatientLinkNum = SIn.Long(row["PatientLinkNum"].ToString()),
                PatNumFrom = SIn.Long(row["PatNumFrom"].ToString()),
                PatNumTo = SIn.Long(row["PatNumTo"].ToString()),
                LinkType = (PatientLinkType) SIn.Int(row["LinkType"].ToString()),
                DateTimeLink = SIn.DateTime(row["DateTimeLink"].ToString())
            };
            retVal.Add(patientLink);
        }

        return retVal;
    }

    public static void Insert(PatientLink patientLink)
    {
        var command = "INSERT INTO patientlink (";

        command += "PatNumFrom,PatNumTo,LinkType,DateTimeLink) VALUES(";

        command +=
            SOut.Long(patientLink.PatNumFrom) + ","
                                              + SOut.Long(patientLink.PatNumTo) + ","
                                              + SOut.Int((int) patientLink.LinkType) + ","
                                              + "NOW()" + ")";
        {
            patientLink.PatientLinkNum = Db.NonQ(command, true, "PatientLinkNum", "patientLink");
        }
    }
}