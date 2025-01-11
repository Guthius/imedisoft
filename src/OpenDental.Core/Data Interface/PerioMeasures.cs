using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PerioMeasures
{
    public static List<PerioMeasure> GetForPatient(long patNum)
    {
        var command =
            "SELECT periomeasure.*"
            + " FROM periomeasure,perioexam"
            + " WHERE periomeasure.PerioExamNum = perioexam.PerioExamNum"
            + " AND perioexam.PatNum = " + SOut.Long(patNum);
        return PerioMeasureCrud.SelectMany(command);
    }

    
    public static void Update(PerioMeasure perioMeasure)
    {
        PerioMeasureCrud.Update(perioMeasure);
        //3-10-10 A bug that only lasted for a few weeks has resulted in a number of duplicate entries for each tooth.
        //So we need to clean up duplicates as we go.  Might put in db maint later.
        var command = "DELETE FROM periomeasure WHERE "
                      + "PerioExamNum = " + SOut.Long(perioMeasure.PerioExamNum)
                      + " AND SequenceType = " + SOut.Long((int) perioMeasure.SequenceType)
                      + " AND IntTooth = " + SOut.Long(perioMeasure.IntTooth)
                      + " AND PerioMeasureNum != " + SOut.Long(perioMeasure.PerioMeasureNum);
        Db.NonQ(command);
    }

    
    public static long Insert(PerioMeasure perioMeasure)
    {
        return PerioMeasureCrud.Insert(perioMeasure);
    }

    
    public static void InsertMany(List<PerioMeasure> listPerioMeasures)
    {
        PerioMeasureCrud.InsertMany(listPerioMeasures);
    }

    
    public static void Delete(PerioMeasure perioMeasure)
    {
        var command = "DELETE from periomeasure WHERE PerioMeasureNum = '"
                      + perioMeasure.PerioMeasureNum + "'";
        Db.NonQ(command);
    }

    public static bool Sync(List<PerioMeasure> listPerioMeasuresNew, List<PerioMeasure> listPerioMeasuresOld)
    {
        return PerioMeasureCrud.Sync(listPerioMeasuresNew, listPerioMeasuresOld);
    }

    /// <summary>
    ///     For the current exam, clears existing skipped teeth and resets them to the specified skipped teeth. The
    ///     ArrayList valid values are 1-32 int.
    /// </summary>
    public static void SetSkipped(long perioExamNum, List<int> listSkippedTeeth)
    {
        //for(int i=0;i<skippedTeeth.Count;i++){
        //MessageBox.Show(skippedTeeth[i].ToString());
        //}
        //first, delete all skipped teeth for this exam
        var command = "DELETE from periomeasure WHERE "
                      + "PerioExamNum = " + perioExamNum + " "
                      + "AND SequenceType = '" + SOut.Long((int) PerioSequenceType.SkipTooth) + "'";
        Db.NonQ(command);
        //then add the new ones in one at a time.
        PerioMeasure perioMeasure;
        //There should only be one periomeasure entry per skipped tooth.
        var listDistinctTeeth = listSkippedTeeth.Distinct().ToList();
        for (var i = 0; i < listDistinctTeeth.Count; i++)
        {
            perioMeasure = new PerioMeasure();
            perioMeasure.PerioExamNum = perioExamNum;
            perioMeasure.SequenceType = PerioSequenceType.SkipTooth;
            perioMeasure.IntTooth = listDistinctTeeth[i];
            perioMeasure.ToothValue = 1;
            perioMeasure.MBvalue = -1;
            perioMeasure.Bvalue = -1;
            perioMeasure.DBvalue = -1;
            perioMeasure.MLvalue = -1;
            perioMeasure.Lvalue = -1;
            perioMeasure.DLvalue = -1;
            Insert(perioMeasure);
        }
    }

    /// <summary>
    ///     Used in FormPerio.Add_Click. For the specified exam, gets a list of all skipped teeth. The ArrayList valid
    ///     values are 1-32 int.
    /// </summary>
    public static List<int> GetSkipped(long perioExamNum)
    {
        var command = "SELECT IntTooth FROM periomeasure WHERE "
                      + "SequenceType = '" + SOut.Int((int) PerioSequenceType.SkipTooth) + "' "
                      + "AND PerioExamNum = '" + perioExamNum + "' "
                      + "AND ToothValue = '1'";
        var tableSkippedTeeth = DataCore.GetTable(command);
        var listSkippedTeeth = new List<int>();
        for (var i = 0; i < tableSkippedTeeth.Rows.Count; i++) listSkippedTeeth.Add(SIn.Int(tableSkippedTeeth.Rows[i][0].ToString()));
        return listSkippedTeeth;
    }

    ///<summary>Get a list of periomeasures from the db.</summary>
    public static List<PerioMeasure> GetPerioMeasuresForApi(int limit, int offset, long perioExamNum)
    {
        var command = "SELECT * FROM periomeasure ";
        if (perioExamNum > 0) command += "WHERE PerioExamNum=" + SOut.Long(perioExamNum) + " ";
        command += "ORDER BY PerioMeasureNum "
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit) + " ";
        return PerioMeasureCrud.SelectMany(command);
    }

    ///<summary>Gets a PerioMeasure by PerioMeasureNum from the database. Returns null if not found.</summary>
    public static PerioMeasure GetOne(long perioMeasureNum)
    {
        var command = "SELECT * FROM periomeasure "
                      + "WHERE PerioMeasureNum = " + SOut.Long(perioMeasureNum);
        return PerioMeasureCrud.SelectOne(command);
    }

    public static List<PerioMeasure> GetAllForExam(long perioExamNum)
    {
        var command = "SELECT * FROM periomeasure "
                      + "WHERE PerioExamNum = " + SOut.Long(perioExamNum);
        return PerioMeasureCrud.SelectMany(command);
    }

    /// <summary>
    ///     A -1 will be changed to a 0. Measures over 100 are changed to 100-measure. i.e. 100-104=-4 for hyperplastic
    ///     GM.
    /// </summary>
    public static int AdjustGMVal(int measure)
    {
        if (measure == -1) //-1 means no measurement, null.  In the places where this method is used, we have designed it to expect a 0 in those cases.
            return 0;

        if (measure > 100) return 100 - measure;
        return measure; //no adjustments needed.
    }
}