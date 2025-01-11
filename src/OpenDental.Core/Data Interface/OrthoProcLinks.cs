using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoProcLinks
{
    #region Insert

    ///<summary>Inserts an OrthoProcLink into the database. Returns the OrthoProcLinkNum.</summary>
    public static long Insert(OrthoProcLink orthoProcLink)
    {
        return OrthoProcLinkCrud.Insert(orthoProcLink);
    }

    #endregion Insert

    #region Update

    /// <summary>Update only data that is different in newOrthoProcLink</summary>
    public static void Update(OrthoProcLink orthoProcLinkNew, OrthoProcLink orthoProcLinkOld)
    {
        OrthoProcLinkCrud.Update(orthoProcLinkNew, orthoProcLinkOld);
    }

    #endregion Update

    #region Get Methods

    ///<summary>Gets all orthoproclinks from DB.</summary>
    public static List<OrthoProcLink> GetAll()
    {
        var command = "SELECT orthoproclink.* FROM orthoproclink";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    ///<summary>Get a list of all OrthoProcLinks for an OrthoCase.</summary>
    public static List<OrthoProcLink> GetManyByOrthoCase(long orthoCaseNum)
    {
        var command = "SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum = " + SOut.Long(orthoCaseNum);
        return OrthoProcLinkCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets one OrthoProcLink of the specified OrthoProcType for an OrthoCase. This should only be used to get procedures
    ///     of the
    ///     Banding or Debond types as only one of each can be linked to an Orthocase.
    /// </summary>
    public static OrthoProcLink GetByType(long orthoCaseNum, OrthoProcType orthoProcType)
    {
        var command = $@"SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum={SOut.Long(orthoCaseNum)}
				AND orthoproclink.ProcLinkType={SOut.Int((int) orthoProcType)}";
        return OrthoProcLinkCrud.SelectOne(command);
    }

    public static List<OrthoProcLink> GetPatientData(List<OrthoCase> listOrthoCases)
    {
        return GetManyByOrthoCases(listOrthoCases.Select(x => x.OrthoCaseNum).ToList());
    }

    /// <summary>
    ///     Returns a list of OrthoProcLinks from db of the specified type that are associated to any OrthoCaseNum from
    ///     the list passed in.
    /// </summary>
    public static List<OrthoProcLink> GetManyByOrthoCases(List<long> listOrthoCaseNums)
    {
        if (listOrthoCaseNums.Count <= 0) return new List<OrthoProcLink>();

        var command = $"SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum IN({string.Join(",", listOrthoCaseNums)})";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    ///<summary>Gets all OrthoProcLinks associated to any procedures in the list passed in.</summary>
    public static List<OrthoProcLink> GetManyForProcs(List<long> listProcNums)
    {
        if (listProcNums.Count <= 0) return new List<OrthoProcLink>();

        var command = $@"SELECT * FROM orthoproclink
				WHERE orthoproclink.ProcNum IN({string.Join(",", listProcNums)})";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    ///<summary>Returns a single OrthoProcLink for the procNum. There should only be one in db per procedure.</summary>
    public static OrthoProcLink GetByProcNum(long procNum)
    {
        var command = "SELECT * FROM orthoproclink WHERE ProcNum=" + SOut.Long(procNum);
        return OrthoProcLinkCrud.SelectOne(command);
    }

    ///<summary>Returns a list of all ProcLinks of the visit type associated to an OrthoCase.</summary>
    public static List<OrthoProcLink> GetVisitLinksForOrthoCase(long orthoCaseNum)
    {
        var command = $@"SELECT * FROM orthoproclink WHERE orthoproclink.OrthoCaseNum={SOut.Long(orthoCaseNum)}
			AND orthoproclink.ProcLinkType={SOut.Int((int) OrthoProcType.Visit)}";
        return OrthoProcLinkCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Delete

    ///<summary>Delete an OrthoProcLink from the database.</summary>
    public static void Delete(long orthoProcLinkNum)
    {
        OrthoProcLinkCrud.Delete(orthoProcLinkNum);
    }

    ///<summary>Deletes all ProcLinks in the provided list of OrthoProcLinkNums.</summary>
    public static void DeleteMany(List<long> listOrthoProcLinkNums)
    {
        if (listOrthoProcLinkNums.Count <= 0) return;

        var command = $"DELETE FROM orthoproclink WHERE OrthoProcLinkNum IN({string.Join(",", listOrthoProcLinkNums)})";
        Db.NonQ(command);
    }

    #endregion Delete

    #region Misc Methods

    /// <summary>
    ///     Does not insert it in the DB. Returns an OrthoProcLink of the specified type for the OrthoCaseNum and procNum
    ///     passed in.
    /// </summary>
    public static OrthoProcLink CreateHelper(long orthoCaseNum, long procNum, OrthoProcType orthoProcType)
    {
        var orthoProcLink = new OrthoProcLink();
        orthoProcLink.OrthoCaseNum = orthoCaseNum;
        orthoProcLink.ProcNum = procNum;
        orthoProcLink.ProcLinkType = orthoProcType;
        orthoProcLink.SecUserNumEntry = Security.CurUser.UserNum;
        return orthoProcLink;
    }

    ///<summary>Returns true if the procNum is contained in at least one OrthoProcLink.</summary>
    public static bool IsProcLinked(long procNum)
    {
        var command = "SELECT * FROM orthoproclink WHERE orthoproclink.ProcNum=" + SOut.Long(procNum);
        return OrthoProcLinkCrud.SelectMany(command).Count > 0;
    }

    #endregion Misc Methods

    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    ///<summary>Gets one OrthoProcLink from the db.</summary>
    public static OrthoProcLink GetOne(long orthoProcLinkNum){

        return Crud.OrthoProcLinkCrud.SelectOne(orthoProcLinkNum);
    }
    
    public static void Update(OrthoProcLink orthoProcLink){

        Crud.OrthoProcLinkCrud.Update(orthoProcLink);
    }
    */
}