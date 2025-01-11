using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ReqNeededs
{
    public static DataTable Refresh(long schoolClassNum, long schoolCourseNum)
    {
        var command = "SELECT * FROM reqneeded WHERE SchoolClassNum=" + SOut.Long(schoolClassNum)
                                                                      + " AND SchoolCourseNum=" + SOut.Long(schoolCourseNum)
                                                                      + " ORDER BY Descript";
        return DataCore.GetTable(command);
    }

    public static ReqNeeded GetReq(long reqNeededNum)
    {
        var command = "SELECT * FROM reqneeded WHERE ReqNeededNum=" + SOut.Long(reqNeededNum);
        return ReqNeededCrud.SelectOne(command);
    }

    
    public static void Update(ReqNeeded reqNeeded)
    {
        ReqNeededCrud.Update(reqNeeded);
    }

    
    public static long Insert(ReqNeeded reqNeeded)
    {
        return ReqNeededCrud.Insert(reqNeeded);
    }

    ///<summary>Surround with try/catch.</summary>
    public static void Delete(long reqNeededNum)
    {
        //still need to validate
        var command = "DELETE FROM reqneeded "
                      + "WHERE ReqNeededNum = " + SOut.Long(reqNeededNum);
        Db.NonQ(command);
    }

    ///<summary>Returns a list with all reqneeded entries in the database.</summary>
    public static List<ReqNeeded> GetListFromDb()
    {
        var command = "SELECT * FROM reqneeded ORDER BY Descript";
        return ReqNeededCrud.SelectMany(command);
    }

    ///<summary>Inserts, updates, or deletes rows to reflect changes between listNew and stale listOld.</summary>
    public static void Sync(List<ReqNeeded> listReqNeededsNew, List<ReqNeeded> listReqNeededsOld)
    {
        ReqNeededCrud.Sync(listReqNeededsNew, listReqNeededsOld);
    }
}