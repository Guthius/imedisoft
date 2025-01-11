using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>Handles database commands related to the statementprod table in the db.</summary>
public class StatementProds
{
    #region Get Methods

    ///<summary>Gets all of the StatementProds for a list of StatementNums.</summary>
    public static List<StatementProd> GetManyForStatements(List<long> listStatementNums)
    {
        if (listStatementNums.IsNullOrEmpty()) return new List<StatementProd>();

        var command = $"SELECT * FROM statementprod WHERE StatementNum IN({string.Join(",", listStatementNums.Select(x => SOut.Long(x)))})";
        return StatementProdCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Modification Methods

    /// <summary>
    ///     Takes a list of statement prods for statements and a list of statements prods for the same statements that
    ///     exist in DB, then performs inserts, updates, and deletes to sync the new list to the DB.
    /// </summary>
    public static void Sync(List<StatementProd> listStatementProdsNew, List<StatementProd> listStatementProdsInDb)
    {
        StatementProdCrud.Sync(listStatementProdsNew, listStatementProdsInDb);
    }

    /// <summary>
    ///     Creates statement prods for a statement based off of the dataSet passed in, then syncs this list with the
    ///     existing statementprods for the statement in the DB.
    /// </summary>
    public static void SyncForStatement(DataSet dataSet, long statementNum, long docNum)
    {
        var listStatementProds = GetManyForStatements(new List<long> {statementNum});
        Sync(CreateManyForStatement(dataSet, statementNum, docNum, listStatementProds), listStatementProds);
    }

    /// <summary>
    ///     Pass in a list of statement DataSets. Creates statement prods for the statements based off of their dataSets,
    ///     then syncs these statementprods with the existing statementprods for the statements in the DB.
    /// </summary>
    public static void SyncForMultipleStatements(List<StatementData> listStatementDatas)
    {
        var listStatementProdsNew = new List<StatementProd>();
        var listStatementProdsFromDb = new List<StatementProd>();
        var listStatementNums = new List<long>();
        for (var i = 0; i < listStatementDatas.Count; i++)
        {
            if (listStatementDatas[i].ListStatementProds.Count == 0) continue;
            listStatementNums.Add(listStatementDatas[i].ListStatementProds[0].StatementNum);
        }

        var listStatementProdsSyncData = GetManyForStatements(listStatementNums);
        for (var i = 0; i < listStatementDatas.Count; i++)
        {
            var listStatementProds = listStatementProdsSyncData.FindAll(x => x.FKey == listStatementDatas[i].ListStatementProds[i].FKey);
            listStatementProdsFromDb.AddRange(listStatementProds);
            if (listStatementDatas[i].ListStatementProds.Count == 0) continue;
            var listStatementProds2 = CreateManyForStatement(listStatementDatas[i].DataSetStmtNew, listStatementDatas[i].ListStatementProds[0].StatementNum, listStatementDatas[i].DocNum, listStatementProds);
            listStatementProdsNew.AddRange(listStatementProds2);
        }

        Sync(listStatementProdsNew, listStatementProdsFromDb);
    }

    /// <summary>
    ///     Creates StatementProds for all of the production items on a statement. Any StatementProds that already exist
    ///     in the DB for the statement that would be created again, get copied from the DB so that the sync method maintains
    ///     them. All new table and column requirements from the DataSet passed in need to be added to
    ///     StatementData.StmtDataSet.
    /// </summary>
    private static List<StatementProd> CreateManyForStatement(DataSet dataSet, long statementNum, long docNum,
        List<StatementProd> listStatementProdsAll)
    {
        var listStatementProds = new List<StatementProd>();
        for (var i = 0; i < dataSet.Tables.Count; i++)
        {
            var table = dataSet.Tables[i];
            //Each family member will have their own account table, so only consider tables that start with 'account'.
            if (!table.TableName.StartsWith("account")) continue;
            for (var j = 0; j < table.Rows.Count; j++)
            {
                var dataRow = table.Rows[j];
                var procNum = SIn.Long(dataRow["ProcNum"].ToString());
                var adjNum = SIn.Long(dataRow["AdjNum"].ToString());
                var payPlanChargeNum = SIn.Long(dataRow["PayPlanChargeNum"].ToString());
                var creditsDouble = SIn.Double(dataRow["CreditsDouble"].ToString());
                long fKey;
                ProductionType productionType;
                var statementProd = new StatementProd();
                if (procNum != 0)
                {
                    fKey = procNum;
                    productionType = ProductionType.Procedure;
                    statementProd = listStatementProdsAll.Find(x => x.ProdType == ProductionType.Procedure && x.FKey == fKey);
                }
                else if (adjNum != 0 && SIn.Long(dataRow["ProcsOnObj"].ToString()) == 0)
                {
                    fKey = adjNum;
                    productionType = ProductionType.Adjustment;
                    statementProd = listStatementProdsAll.Find(x => x.ProdType == ProductionType.Adjustment && x.FKey == fKey);
                }
                else if (payPlanChargeNum != 0 && CompareDouble.IsZero(creditsDouble))
                {
                    fKey = payPlanChargeNum;
                    productionType = ProductionType.PayPlanCharge;
                    statementProd = listStatementProdsAll.Find(x => x.ProdType == ProductionType.PayPlanCharge && x.FKey == fKey);
                }
                else
                {
                    continue;
                }

                if (statementProd == null)
                {
                    statementProd = new StatementProd();
                    statementProd.StatementNum = statementNum;
                    statementProd.FKey = fKey;
                    statementProd.ProdType = productionType;
                    statementProd.LateChargeAdjNum = 0;
                }

                statementProd.DocNum = docNum; //DocNum is the only thing that should change for existing statementprods that are being updated.
                listStatementProds.Add(statementProd);
                listStatementProdsAll.RemoveAll(x => x.FKey == fKey);
            }
        }

        //We have removed any StatementProds from our Db collection that are for production items that are still on the statement.
        //We are only left with StatementProds for production items that aren't on the statement anymore.
        //These may be associated to a late charge, so we should keep them.
        listStatementProds.AddRange(listStatementProdsAll);
        return listStatementProds;
    }

    /// <summary>
    ///     Used when creating late charge adjustments. All of the StatementProds for statements sent on or before the
    ///     date passed in that have an FKey in the lists passed in get assigned the AdjNum, even those that aren't on the
    ///     statement that the late charge is being made for.
    /// </summary>
    public static void UpdateLateChargeAdjNumForMany(long adjNum, List<long> listProcNums, List<long> listAdjNums
        , List<long> listPayPlanChargeNums, DateTime dateMaxUpdateStmtProd)
    {
        var isOrStatementNeeded = false;
        if (listProcNums.IsNullOrEmpty() && listAdjNums.IsNullOrEmpty() && listPayPlanChargeNums.IsNullOrEmpty()) return;

        var command = $@"UPDATE statementprod
				INNER JOIN statement
				ON statementprod.StatementNum=statement.StatementNum
				AND statement.DateSent<={SOut.Date(dateMaxUpdateStmtProd)}
				SET statementprod.LateChargeAdjNum={SOut.Long(adjNum)}
				WHERE statementprod.LateChargeAdjNum=0 
				AND "; //One of the lists below must be non-null and have an FKey in it.
        if (!listProcNums.IsNullOrEmpty())
        {
            command += @$"(statementprod.ProdType={SOut.Int((int) ProductionType.Procedure)}
					AND FKey IN({string.Join(",", listProcNums.Select(x => SOut.Long(x)).ToList())})) ";
            isOrStatementNeeded = true;
        }

        if (!listAdjNums.IsNullOrEmpty())
        {
            if (isOrStatementNeeded) command += "OR ";
            command += @$"(statementprod.ProdType={SOut.Int((int) ProductionType.Adjustment)}
					AND FKey IN({string.Join(",", listAdjNums.Select(x => SOut.Long(x)).ToList())})) ";
            isOrStatementNeeded = true;
        }

        if (!listPayPlanChargeNums.IsNullOrEmpty())
        {
            if (isOrStatementNeeded) command += "OR ";
            command += @$"(statementprod.ProdType={SOut.Int((int) ProductionType.PayPlanCharge)}
					AND FKey IN({string.Join(",", listPayPlanChargeNums.Select(x => SOut.Long(x)).ToList())}))";
        }

        Db.NonQ(command);
    }

    /// <summary>
    ///     Typically only used when setting StatementProd.LateChargeAdjNum back to 0 when the late charge Adjustment is
    ///     deleted.
    /// </summary>
    public static void UpdateLateChargeAdjNumForMany(long adjNumNew, params long[] adjNumArrayOld)
    {
        if (adjNumArrayOld.IsNullOrEmpty()) return;

        var command = $@"UPDATE statementprod SET statementprod.LateChargeAdjNum={SOut.Long(adjNumNew)}
				WHERE statementprod.LateChargeAdjNum IN ({string.Join(",", adjNumArrayOld.Select(x => SOut.Long(x)).ToList())})";
        Db.NonQ(command);
    }

    #endregion Modification Methods

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Methods - Get
    
    ///<summary>Gets one StatementProd from the db.</summary>
    public static StatementProd GetOne(long statementProdNum){

        return Crud.StatementProdCrud.SelectOne(statementProdNum);
    }
    #endregion Methods - Get
    #region Methods - Modify
    
    public static long Insert(StatementProd statementProd){

        return Crud.StatementProdCrud.Insert(statementProd);
    }
    
    public static void Update(StatementProd statementProd){

        Crud.StatementProdCrud.Update(statementProd);
    }
    
    public static void Delete(long statementProdNum) {

        Crud.StatementProdCrud.Delete(statementProdNum);
    }
    #endregion Methods - Modify
    */
}