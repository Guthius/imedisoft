using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SmsBlockPhoneCrud
{
    public static void Insert(SmsBlockPhone smsBlockPhone)
    {
        var command = "INSERT INTO smsblockphone (";

        command += "BlockWirelessNumber) VALUES(";
        command += "'" + SOut.String(smsBlockPhone.BlockWirelessNumber) + "')";

        smsBlockPhone.SmsBlockPhoneNum = Db.NonQ(command, true, "SmsBlockPhoneNum", "smsBlockPhone");
    }
}