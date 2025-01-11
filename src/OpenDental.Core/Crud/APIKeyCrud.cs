using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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