using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class StatementProds
{
    public static List<StatementProd> GetManyForStatements(List<long> statementNums)
    {
        return statementNums is not {Count: > 0} ? [] : StatementProdCrud.SelectMany($"SELECT * FROM statementprod WHERE StatementNum IN ({string.Join(", ", statementNums)})");
    }

    public static void Sync(List<StatementProd> statementProdsNew, List<StatementProd> statementProdsInDb)
    {
        StatementProdCrud.Sync(statementProdsNew, statementProdsInDb);
    }

    public static void SyncForStatement(DataSet dataSet, long statementNum, long docNum)
    {
        var statementProds = GetManyForStatements([statementNum]);

        Sync(CreateManyForStatement(dataSet, statementNum, docNum, statementProds), statementProds);
    }

    public static void SyncForMultipleStatements(List<StatementData> statementDatas)
    {
        var statementProdsNew = new List<StatementProd>();
        var statementProdsFromDb = new List<StatementProd>();
        var statementNums = new List<long>();

        foreach (var statementData in statementDatas)
        {
            if (statementData.ListStatementProds.Count == 0)
            {
                continue;
            }

            statementNums.Add(statementData.ListStatementProds[0].StatementNum);
        }

        var statementProdsSyncData = GetManyForStatements(statementNums);
        for (var i = 0; i < statementDatas.Count; i++)
        {
            var statementProds = statementProdsSyncData.Where(x => x.FKey == statementDatas[i].ListStatementProds[i].FKey).ToList();

            statementProdsFromDb.AddRange(statementProds);

            if (statementDatas[i].ListStatementProds.Count == 0)
            {
                continue;
            }

            var statementProds2 = CreateManyForStatement(
                statementDatas[i].DataSetStmtNew,
                statementDatas[i].ListStatementProds[0].StatementNum,
                statementDatas[i].DocNum,
                statementProds);

            statementProdsNew.AddRange(statementProds2);
        }

        Sync(statementProdsNew, statementProdsFromDb);
    }

    private static List<StatementProd> CreateManyForStatement(DataSet dataSet, long statementNum, long docNum, List<StatementProd> listStatementProdsAll)
    {
        var statementProds = new List<StatementProd>();
        for (var i = 0; i < dataSet.Tables.Count; i++)
        {
            var table = dataSet.Tables[i];

            if (!table.TableName.StartsWith("account"))
            {
                continue;
            }

            for (var j = 0; j < table.Rows.Count; j++)
            {
                var dataRow = table.Rows[j];
                var procNum = SIn.Long(dataRow["ProcNum"].ToString());
                var adjNum = SIn.Long(dataRow["AdjNum"].ToString());
                var payPlanChargeNum = SIn.Long(dataRow["PayPlanChargeNum"].ToString());
                var creditsDouble = SIn.Double(dataRow["CreditsDouble"].ToString());
                
                long fKey;
                ProductionType productionType;
                StatementProd statementProd;
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

                statementProd ??= new StatementProd
                {
                    StatementNum = statementNum,
                    FKey = fKey,
                    ProdType = productionType,
                    LateChargeAdjNum = 0
                };

                statementProd.DocNum = docNum;
                statementProds.Add(statementProd);
                listStatementProdsAll.RemoveAll(x => x.FKey == fKey);
            }
        }
        
        statementProds.AddRange(listStatementProdsAll);
        
        return statementProds;
    }

    public static void UpdateLateChargeAdjNumForMany(long adjNum, List<long> procNums, List<long> adjNums, List<long> payPlanChargeNums, DateTime dateMaxUpdateStmtProd)
    {
        var isOrStatementNeeded = false;
        if (procNums.IsNullOrEmpty() && adjNums.IsNullOrEmpty() && payPlanChargeNums.IsNullOrEmpty())
        {
            return;
        }

        var command =
            $"""
             UPDATE statementprod
             INNER JOIN statement ON statementprod.StatementNum = statement.StatementNum
             AND statement.DateSent <= {SOut.Date(dateMaxUpdateStmtProd)}
             SET statementprod.LateChargeAdjNum = {adjNum}
             WHERE statementprod.LateChargeAdjNum = 0 
             AND 
             """;

        if (procNums is {Count: > 0})
        {
            command += @$"(statementprod.ProdType = {(int) ProductionType.Procedure} AND FKey IN ({string.Join(",", procNums)})) ";
            isOrStatementNeeded = true;
        }

        if (adjNums is {Count: > 0})
        {
            if (isOrStatementNeeded)
            {
                command += "OR ";
            }

            command += $"(statementprod.ProdType = {(int) ProductionType.Adjustment} AND FKey IN ({string.Join(",", adjNums)})) ";
            isOrStatementNeeded = true;
        }

        if (payPlanChargeNums is {Count: > 0})
        {
            if (isOrStatementNeeded)
            {
                command += "OR ";
            }

            command += $"(statementprod.ProdType = {(int) ProductionType.PayPlanCharge} AND FKey IN ({string.Join(",", payPlanChargeNums)}))";
        }

        Db.NonQ(command);
    }

    public static void UpdateLateChargeAdjNumForMany(long adjNumNew, params long[] adjNumsOld)
    {
        if (adjNumsOld.Length == 0)
        {
            return;
        }

        Db.NonQ($"UPDATE statementprod SET statementprod.LateChargeAdjNum = {adjNumNew} WHERE statementprod.LateChargeAdjNum IN ({string.Join(",", adjNumsOld)})");
    }
}