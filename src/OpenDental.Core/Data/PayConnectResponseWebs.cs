using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PayConnectResponseWebs
{
    public static void Insert(PayConnectResponseWeb payConnectResponseWeb)
    {
        PayConnectResponseWebCrud.Insert(payConnectResponseWeb);
    }

    public static void HandleResponseError(PayConnectResponseWeb responseWeb, string resStr)
    {
        responseWeb.LastResponseStr = resStr;
        responseWeb.ProcessingStatus = responseWeb.ProcessingStatus switch
        {
            PayConnectWebStatus.Created => PayConnectWebStatus.CreatedError,
            PayConnectWebStatus.Pending => PayConnectWebStatus.PendingError,
            _ => PayConnectWebStatus.UnknownError
        };
        responseWeb.DateTimeLastError = MiscData.GetNowDateTime();
    }

    public static PayConnectResponseWeb GetOne(long payConnectResponseWebNum)
    {
        return PayConnectResponseWebCrud.SelectOne(payConnectResponseWebNum);
    }

    public static PayConnectResponseWeb GetOneByPayNum(long payNum)
    {
        return PayConnectResponseWebCrud.SelectOne($"SELECT * FROM payconnectresponseweb WHERE PayNum={payNum}");
    }
}