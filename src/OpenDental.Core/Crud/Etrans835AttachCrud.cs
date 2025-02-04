using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class Etrans835AttachCrud
{
    public static List<Etrans835Attach> TableToList(DataTable table)
    {
        var retVal = new List<Etrans835Attach>();
        foreach (DataRow row in table.Rows)
        {
            var etrans835Attach = new Etrans835Attach
            {
                Etrans835AttachNum = SIn.Long(row["Etrans835AttachNum"].ToString()),
                EtransNum = SIn.Long(row["EtransNum"].ToString()),
                ClaimNum = SIn.Long(row["ClaimNum"].ToString()),
                ClpSegmentIndex = SIn.Int(row["ClpSegmentIndex"].ToString())
            };
            retVal.Add(etrans835Attach);
        }

        return retVal;
    }

    public static void Insert(Etrans835Attach etrans835Attach)
    {
        var command = "INSERT INTO etrans835attach (";

        command += "EtransNum,ClaimNum,ClpSegmentIndex,DateTimeEntry) VALUES(";

        command +=
            SOut.Long(etrans835Attach.EtransNum) + ","
                                                 + SOut.Long(etrans835Attach.ClaimNum) + ","
                                                 + SOut.Int(etrans835Attach.ClpSegmentIndex) + ","
                                                 + "NOW()" + ")";
        {
            etrans835Attach.Etrans835AttachNum = Db.NonQ(command, true, "Etrans835AttachNum", "etrans835Attach");
        }
    }
}