using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PayConnectResponseWebs
{
    #region Insert

    
    public static long Insert(PayConnectResponseWeb payConnectResponseWeb)
    {
        return PayConnectResponseWebCrud.Insert(payConnectResponseWeb);
    }

    #endregion Insert

    #region Update

    
    public static void Update(PayConnectResponseWeb payConnectResponseWeb)
    {
        PayConnectResponseWebCrud.Update(payConnectResponseWeb);
    }

    #endregion Update

    #region Misc Methods

    public static void HandleResponseError(PayConnectResponseWeb responseWeb, string resStr)
    {
        responseWeb.LastResponseStr = resStr;
        if (responseWeb.ProcessingStatus == PayConnectWebStatus.Created)
            responseWeb.ProcessingStatus = PayConnectWebStatus.CreatedError;
        else if (responseWeb.ProcessingStatus == PayConnectWebStatus.Pending)
            responseWeb.ProcessingStatus = PayConnectWebStatus.PendingError;
        else
            responseWeb.ProcessingStatus = PayConnectWebStatus.UnknownError;
        responseWeb.DateTimeLastError = MiscData.GetNowDateTime();
    }

    #endregion Misc Methods

    #region Get Methods

    ///<summary>Gets one PayConnectResponseWeb from the db.</summary>
    public static PayConnectResponseWeb GetOne(long payConnectResponseWebNum)
    {
        return PayConnectResponseWebCrud.SelectOne(payConnectResponseWebNum);
    }

    public static PayConnectResponseWeb GetOneByPayNum(long payNum)
    {
        var command = $"SELECT * FROM payconnectresponseweb WHERE PayNum={SOut.Long(payNum)}";
        return PayConnectResponseWebCrud.SelectOne(command);
    }

    public static PayConnectResponseWeb GetOneByRefNumber(string refNumber)
    {
        var command = $"SELECT * FROM payconnectresponseweb WHERE RefNumber='{SOut.String(refNumber)}'";
        return PayConnectResponseWebCrud.SelectOne(command);
    }

    ///<summary>Gets all PayConnectResponseWebs that have a status of Pending from the db.</summary>
    public static List<PayConnectResponseWeb> GetAllPending()
    {
        return PayConnectResponseWebCrud.SelectMany("SELECT * FROM payconnectresponseweb WHERE ProcessingStatus='" + PayConnectWebStatus.Pending + "'");
    }

    #endregion Get Methods
}