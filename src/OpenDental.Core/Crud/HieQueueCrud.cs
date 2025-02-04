using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class HieQueueCrud
{
    public static void Insert(HieQueue hieQueue)
    {
        var command = "INSERT INTO hiequeue (";

        command += "PatNum) VALUES(";
        command += SOut.Long(hieQueue.PatNum) + ")";

        hieQueue.HieQueueNum = Db.NonQ(command, true, "HieQueueNum", "hieQueue");
    }
}