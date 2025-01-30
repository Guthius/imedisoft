using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PearlRequests
{
    public static bool IsRequestHandled(PearlRequest pearlRequest)
    {
        return pearlRequest?.RequestStatus is EnumPearlStatus.Received or EnumPearlStatus.Error;
    }

    public static PearlRequest GetOneByDocNum(long docNum)
    {
        return PearlRequestCrud.SelectOne("SELECT * FROM pearlrequest WHERE DocNum=" + docNum + " ORDER BY DateTSent DESC");
    }

    public static PearlRequest GetOneByRequestId(string requestId)
    {
        return PearlRequestCrud.SelectOne("SELECT * FROM pearlrequest WHERE RequestId=\'" + SOut.String(requestId) + "\' ORDER BY DateTSent DESC");
    }

    public static void Insert(PearlRequest pearlRequest)
    {
        PearlRequestCrud.Insert(pearlRequest);
    }

    public static void Update(PearlRequest pearlRequest)
    {
        PearlRequestCrud.Update(pearlRequest);
    }

    public static void UpdateStatusForRequests(List<PearlRequest> listPearlRequests, EnumPearlStatus pearlStatus)
    {
        var pearlRequestNums = listPearlRequests.Select(x => x.PearlRequestNum).ToList();

        Db.NonQ("UPDATE pearlrequest SET RequestStatus=" + (int) pearlStatus + " WHERE PearlRequestNum IN (" + string.Join(",", pearlRequestNums) + ")");
    }

    public static void Delete(long pearlRequestNum)
    {
        PearlRequestCrud.Delete(pearlRequestNum);
    }

    public static void DeleteByDocNum(long docNum)
    {
        Db.NonQ("DELETE FROM pearlrequest WHERE DocNum=" + docNum);
    }
}