using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class PayConnectResponseWebs
{
    public static void Insert(PayConnectResponseWeb payConnectResponseWeb)
    {
        PayConnectResponseWebCrud.Insert(payConnectResponseWeb);
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