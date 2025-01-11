using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoPlanLinks
{
    #region Insert

    ///<summary>Inserts a OrthoPlanLink into the database. Returns the OrthoPlanLinkNum</summary>
    public static long Insert(OrthoPlanLink orthoPlanLink)
    {
        return OrthoPlanLinkCrud.Insert(orthoPlanLink);
    }

    #endregion Insert

    #region Update

    ///<summary>Update only data that is different in newOrthoPlanLink.</summary>
    public static void Update(OrthoPlanLink orthoPlanLinkNew, OrthoPlanLink orthoPlanLinkOld)
    {
        OrthoPlanLinkCrud.Update(orthoPlanLinkNew, orthoPlanLinkOld);
    }

    #endregion Update

    #region Get Methods

    ///<summary>Gets one OrthoPlanLink from the db.</summary>
    public static OrthoPlanLink GetOne(long orthoPlanLinkNum)
    {
        return OrthoPlanLinkCrud.SelectOne(orthoPlanLinkNum);
    }

    /// <summary>
    ///     Gets one OrthoPlanLink by OrthoCaseNum and OrthoPlanLinkType. Each OrthoCase should have no more than one of each
    ///     OrthoPlanLinkType associated to it.
    /// </summary>
    public static OrthoPlanLink GetOneForOrthoCaseByType(long orthoCaseNum, OrthoPlanLinkType orthoPlanLinkType)
    {
        var command = $@"SELECT * FROM orthoplanlink WHERE orthoplanlink.OrthoCaseNum={SOut.Long(orthoCaseNum)}
				AND orthoplanlink.LinkType={SOut.Int((int) orthoPlanLinkType)}";
        return OrthoPlanLinkCrud.SelectOne(command);
    }

    ///<summary>Gets a list of all OrthoPlanLinks from db by OrthoPlanLinkType for a list of OrthoCaseNums.</summary>
    public static List<OrthoPlanLink> GetAllForOrthoCasesByType(List<long> listOrthoCaseNums, OrthoPlanLinkType orthoPlanLinkType)
    {
        if (listOrthoCaseNums.Count <= 0) return new List<OrthoPlanLink>();

        var command = $@"SELECT * FROM orthoplanlink WHERE orthoplanlink.LinkType={SOut.Int((int) orthoPlanLinkType)}
				AND orthoplanlink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoPlanLinkCrud.SelectMany(command);
    }

    ///<summary>Gets all OrthoPlanLinks for a list of OrthoCaseNums.</summary>
    public static List<OrthoPlanLink> GetManyForOrthoCases(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums.Count <= 0) return new List<OrthoPlanLink>();

        var command = $"SELECT * FROM orthoplanlink WHERE orthoplanlink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoPlanLinkCrud.SelectMany(command);
    }

    ///<summary>Gets one OrthoPlanLink of the requested type if one is found on the supplied PayPlan(num).</summary>
    public static OrthoPlanLink GetOrthoPlanLinkOfType(OrthoPlanLinkType orthoPlanLinkType, long payPlanNum)
    {
        var command = $"SELECT * FROM orthoplanlink WHERE LinkType={SOut.Enum(orthoPlanLinkType)} AND FKey={SOut.Long(payPlanNum)}";
        return OrthoPlanLinkCrud.SelectOne(command);
    }

    #endregion Get Methods

    #region Delete

    /////<summary>Delete an OrthoPlanLink from the database.</summary>
    //public static void Delete(long orthoPlanLinkNum) {
    //	
    //	Crud.OrthoPlanLinkCrud.Delete(orthoPlanLinkNum);
    //}

    #endregion Delete

    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    
    public static List<OrthoPlanLink> Refresh(long patNum){

        string command="SELECT * FROM orthoplanlink WHERE PatNum = "+POut.Long(patNum);
        return Crud.OrthoPlanLinkCrud.SelectMany(command);
    }
    
    public static void Update(OrthoPlanLink orthoPlanLink){

        Crud.OrthoPlanLinkCrud.Update(orthoPlanLink);
    }
    */
}