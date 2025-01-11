using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class StmtLinks
{
    #region Get Methods

    /// <summary>
    ///     Gets all FKeys for the statement and StmtLinkType passed in.  Returns an empty list if statementNum is invalid
    ///     or none found.
    /// </summary>
    public static List<long> GetForStatementAndType(long statementNum, StmtLinkTypes stmtLinkTypes)
    {
        var command = "SELECT FKey FROM stmtlink "
                      + "WHERE StatementNum=" + SOut.Long(statementNum) + " "
                      + "AND StmtLinkType=" + SOut.Int((int) stmtLinkTypes);
        return Db.GetListLong(command);
    }

    #endregion

    ///<summary>Gets the a list of StmtLinks with an FKey in the provided list and a matching StmtLinkType</summary>
    public static List<StmtLink> GetForFKeyAndType(List<long> listFKeys, StmtLinkTypes stmtLinkType)
    {
        if (listFKeys.IsNullOrEmpty()) return new List<StmtLink>();

        var command = "SELECT * FROM stmtlink WHERE StmtLinkType=" + SOut.Int((int) stmtLinkType) + " AND FKey IN (" + string.Join(",", listFKeys.Select(x => SOut.Long(x))) + ")";
        return StmtLinkCrud.SelectMany(command);
    }

    #region Insert

    
    public static long Insert(StmtLink stmtLink)
    {
        return StmtLinkCrud.Insert(stmtLink);
    }

    ///<summary>Creates stmtlink entries for the statement and FKs passed in.</summary>
    public static void AttachFKeysToStatement(long stmtNum, List<long> listFKeys, StmtLinkTypes stmtLinkTypes)
    {
        //Remoting role check due to looping.  Without this there would be a potential for lots of network traffic.

        for (var i = 0; i < listFKeys.Count; i++)
        {
            var stmtLink = new StmtLink();
            stmtLink.StatementNum = stmtNum;
            stmtLink.FKey = listFKeys[i];
            stmtLink.StmtLinkType = stmtLinkTypes;
            Insert(stmtLink);
        }
    }

    public static void AttachMsgToPayToStatement(long stmtNum, List<long> listMsgToPayNums)
    {
        AttachFKeysToStatement(stmtNum, listMsgToPayNums, StmtLinkTypes.MsgToPaySent);
    }

    public static void AttachPaySplitsToStatement(long stmtNum, List<long> listPaySplitNums)
    {
        AttachFKeysToStatement(stmtNum, listPaySplitNums, StmtLinkTypes.PaySplit);
    }

    public static void AttachAdjsToStatement(long stmtNum, List<long> listAdjNums)
    {
        AttachFKeysToStatement(stmtNum, listAdjNums, StmtLinkTypes.Adj);
    }

    public static void AttachProcsToStatement(long stmtNum, List<long> listProcNums)
    {
        AttachFKeysToStatement(stmtNum, listProcNums, StmtLinkTypes.Proc);
    }

    public static void AttachClaimsToStatement(long stmtNum, List<long> listClaimNums)
    {
        AttachFKeysToStatement(stmtNum, listClaimNums, StmtLinkTypes.ClaimPay);
    }

    public static void AttachPayPlanChargesToStatement(long stmtNum, List<long> listPayPlanChargeNums)
    {
        AttachFKeysToStatement(stmtNum, listPayPlanChargeNums, StmtLinkTypes.PayPlanCharge);
    }

    #endregion

    #region Update

    /*
    
    public static void Update(StmtLink stmtLink){

        Crud.StmtLinkCrud.Update(stmtLink);
    }
    */

    #endregion

    #region Delete

    public static void Delete(long stmtLinkNum)
    {
        StmtLinkCrud.Delete(stmtLinkNum);
    }

    public static void DetachAllFromStatement(long statementNum)
    {
        var command = "DELETE FROM stmtlink WHERE StatementNum=" + SOut.Long(statementNum);
        Db.NonQ(command);
    }

    
    public static void DetachAllFromStatements(List<long> listStatementNums)
    {
        if (listStatementNums == null || listStatementNums.Count == 0) return;
        var command = DbHelper.WhereIn("DELETE FROM stmtlink WHERE StatementNum IN ({0})", false, listStatementNums.Select(x => SOut.Long(x)).ToList());
        Db.NonQ(command);
    }

    #endregion
}