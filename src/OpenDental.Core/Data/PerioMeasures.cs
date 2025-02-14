using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class PerioMeasures
{
    public static List<PerioMeasure> GetForPatient(long patNum)
    {
        return PerioMeasureCrud.SelectMany(
            "SELECT periomeasure.* " +
            "FROM periomeasure, perioexam " +
            "WHERE periomeasure.PerioExamNum = perioexam.PerioExamNum " +
            "AND perioexam.PatNum = " + patNum);
    }

    public static void Update(PerioMeasure perioMeasure)
    {
        PerioMeasureCrud.Update(perioMeasure);

        Db.NonQ(
            "DELETE FROM periomeasure WHERE " +
            "PerioExamNum = " + perioMeasure.PerioExamNum + " AND " +
            "SequenceType = " + (int) perioMeasure.SequenceType + " AND " +
            "IntTooth = " + perioMeasure.IntTooth + " AND " +
            "PerioMeasureNum != " + perioMeasure.PerioMeasureNum);
    }

    public static void Insert(PerioMeasure perioMeasure)
    {
        PerioMeasureCrud.Insert(perioMeasure);
    }

    public static void InsertMany(List<PerioMeasure> listPerioMeasures)
    {
        PerioMeasureCrud.InsertMany(listPerioMeasures);
    }

    public static void Delete(PerioMeasure perioMeasure)
    {
        Db.NonQ("DELETE from periomeasure WHERE PerioMeasureNum = " + perioMeasure.PerioMeasureNum);
    }

    public static void SetSkipped(long perioExamNum, List<int> listSkippedTeeth)
    {
        var command = "DELETE from periomeasure WHERE PerioExamNum = " + perioExamNum + " AND SequenceType = " + (int) PerioSequenceType.SkipTooth;
        Db.NonQ(command);

        var listDistinctTeeth = listSkippedTeeth.Distinct().ToList();

        foreach (var t in listDistinctTeeth)
        {
            Insert(new PerioMeasure
            {
                PerioExamNum = perioExamNum,
                SequenceType = PerioSequenceType.SkipTooth,
                IntTooth = t,
                ToothValue = 1,
                MBvalue = -1,
                Bvalue = -1,
                DBvalue = -1,
                MLvalue = -1,
                Lvalue = -1,
                DLvalue = -1
            });
        }
    }

    public static List<int> GetSkipped(long perioExamNum)
    {
        var tableSkippedTeeth = DataCore.GetTable(
            "SELECT IntTooth FROM periomeasure WHERE " +
            "SequenceType = " + (int) PerioSequenceType.SkipTooth + " AND " +
            "PerioExamNum = " + perioExamNum + " AND " +
            "ToothValue = '1'");

        var skippedTeeth = new List<int>();
        for (var i = 0; i < tableSkippedTeeth.Rows.Count; i++)
        {
            skippedTeeth.Add(SIn.Int(tableSkippedTeeth.Rows[i][0].ToString()));
        }

        return skippedTeeth;
    }

    public static List<PerioMeasure> GetAllForExam(long perioExamNum)
    {
        return PerioMeasureCrud.SelectMany("SELECT * FROM periomeasure WHERE PerioExamNum = " + perioExamNum);
    }

    public static int AdjustGingivalMarginValue(int measure)
    {
        return measure switch
        {
            -1 => 0,
            > 100 => 100 - measure,
            _ => measure
        };
    }
}