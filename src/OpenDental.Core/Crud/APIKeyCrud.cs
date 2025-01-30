using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class APIKeyCrud
{
    public static void Insert(APIKey apiKey)
    {
        var command = "INSERT INTO apikey (";

        command += "CustApiKey,DevName) VALUES(";

        command +=
            "'" + SOut.String(apiKey.CustApiKey) + "',"
            + "'" + SOut.String(apiKey.DevName) + "')";
        {
            apiKey.APIKeyNum = Db.NonQ(command, true, "APIKeyNum", "aPIKey");
        }
    }
}