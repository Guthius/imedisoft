using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PearlRequests
{
    #region Methods - Misc

    ///<summary>Returns whether the request has been processed by Pearl, either with success or error.</summary>
    public static bool IsRequestHandled(PearlRequest pearlRequest)
    {
        if (pearlRequest == null) return false;
        return pearlRequest.RequestStatus.In(EnumPearlStatus.Received, EnumPearlStatus.Error);
    }

    #endregion Methods - Misc

    #region Methods - Get

    /*
    
    public static List<PearlRequest> Refresh(long patNum){

        string command="SELECT * FROM pearlrequest WHERE PatNum = "+POut.Long(patNum);
        return Crud.PearlRequestCrud.SelectMany(command);
    }

    ///<summary>Gets one PearlRequest from the db.</summary>
    public static PearlRequest GetOne(long pearlRequestNum){

        return Crud.PearlRequestCrud.SelectOne(pearlRequestNum);
    }
    */

    ///<summary>Gets the most recently sent PearlRequest from the db that matches the given DocNum.</summary>
    public static PearlRequest GetOneByDocNum(long docNum)
    {
        var command = "SELECT * FROM pearlrequest WHERE DocNum=" + SOut.Long(docNum) + " ORDER BY DateTSent DESC";
        return PearlRequestCrud.SelectOne(command);
    }

    ///<summary>Gets the most recently sent PearlRequest from the db that matches the given RequestId.</summary>
    public static PearlRequest GetOneByRequestId(string requestId)
    {
        var command = "SELECT * FROM pearlrequest WHERE RequestId=\'" + SOut.String(requestId) + "\' ORDER BY DateTSent DESC";
        return PearlRequestCrud.SelectOne(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(PearlRequest pearlRequest)
    {
        return PearlRequestCrud.Insert(pearlRequest);
    }

    
    public static void Update(PearlRequest pearlRequest)
    {
        PearlRequestCrud.Update(pearlRequest);
    }

    
    public static void UpdateStatusForRequests(List<PearlRequest> listPearlRequests, EnumPearlStatus pearlStatus)
    {
        var listPearlRequestNums = listPearlRequests.Select(x => x.PearlRequestNum).ToList();
        var command = "UPDATE pearlrequest SET RequestStatus=" + SOut.Enum(pearlStatus)
                                                               + " WHERE PearlRequestNum IN (" + string.Join(",", listPearlRequestNums) + ")";
        Db.NonQ(command);
    }

    ///<summary>DEPRECATED. Sets RequestStatus to ServicePolling for each request matching a given DocNum.</summary>
    //public static void SwitchDocsToServerPolling(List<long> listDocNums){
    //	
    //	string command="UPDATE pearlrequest SET RequestStatus="+POut.Enum<EnumPearlStatus>(EnumPearlStatus.ServicePolling)
    //		+" WHERE DocNum IN ("+string.Join(",",listDocNums.Select(x => POut.Long(x)))+")";
    //	Db.NonQ(command);
    //}

    
    public static void Delete(long pearlRequestNum)
    {
        PearlRequestCrud.Delete(pearlRequestNum);
    }

    
    public static void DeleteByDocNum(long docNum)
    {
        var command = "DELETE FROM pearlrequest WHERE DocNum=" + SOut.Long(docNum);
        Db.NonQ(command);
    }

    #endregion Methods - Modify
}