using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HL7ProcAttachCrud
{
    public static void Insert(HL7ProcAttach hL7ProcAttach)
    {
        var command = "INSERT INTO hl7procattach (";

        command += "HL7MsgNum,ProcNum) VALUES(";
        command += SOut.Long(hL7ProcAttach.HL7MsgNum) + "," + SOut.Long(hL7ProcAttach.ProcNum) + ")";
        
        hL7ProcAttach.HL7ProcAttachNum = Db.NonQ(command, true, "HL7ProcAttachNum", "hL7ProcAttach");
    }
}