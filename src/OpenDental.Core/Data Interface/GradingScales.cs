using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class GradingScales
{
    
    public static List<GradingScale> RefreshList()
    {
        var command = "SELECT * FROM gradingscale ";
        return GradingScaleCrud.SelectMany(command);
    }

    ///<summary>Gets one GradingScale from the db.</summary>
    public static GradingScale GetOne(long gradingScaleNum)
    {
        return GradingScaleCrud.SelectOne(gradingScaleNum);
    }

    public static bool IsDupicateDescription(GradingScale gradingScale)
    {
        var command = "SELECT COUNT(*) FROM gradingscale WHERE Description = '" + SOut.String(gradingScale.Description) + "' "
                      + "AND GradingScaleNum != " + SOut.Long(gradingScale.GradingScaleNum);
        var count = SIn.Int(Db.GetCount(command));
        if (count > 0) return true;
        return false;
    }

    public static bool IsInUseByEvaluation(GradingScale gradingScale)
    {
        var command = "SELECT COUNT(*) FROM evaluation,evaluationcriterion "
                      + "WHERE evaluation.GradingScaleNum = " + SOut.Long(gradingScale.GradingScaleNum) + " "
                      + "OR evaluationcriterion.GradingScaleNum = " + SOut.Long(gradingScale.GradingScaleNum);
        var count = SIn.Int(Db.GetCount(command));
        if (count > 0) return true;
        return false;
    }

    
    public static long Insert(GradingScale gradingScale)
    {
        return GradingScaleCrud.Insert(gradingScale);
    }

    
    public static void Update(GradingScale gradingScale)
    {
        GradingScaleCrud.Update(gradingScale);
    }

    /// <summary>
    ///     Also deletes attached GradeScaleItems.  Will throw an error if GradeScale is in use.  Be sure to surround with
    ///     try-catch.
    /// </summary>
    public static void Delete(long gradingScaleNum)
    {
        var error = "";
        var command = "SELECT COUNT(*) FROM evaluationdef WHERE GradingScaleNum=" + SOut.Long(gradingScaleNum);
        if (Db.GetCount(command) != "0") error += " EvaluationDef,";
        command = "SELECT COUNT(*) FROM evaluationcriteriondef WHERE GradingScaleNum=" + SOut.Long(gradingScaleNum);
        if (Db.GetCount(command) != "0") error += " EvaluationCriterionDef,";
        command = "SELECT COUNT(*) FROM evaluation WHERE GradingScaleNum=" + SOut.Long(gradingScaleNum);
        if (Db.GetCount(command) != "0") error += " Evaluation,";
        command = "SELECT COUNT(*) FROM evaluationcriterion WHERE GradingScaleNum=" + SOut.Long(gradingScaleNum);
        if (Db.GetCount(command) != "0") error += " EvaluationCriterion,";
        if (error != "") throw new ApplicationException(Lans.g("GradingScaleEdit", "Grading scale is in use by") + ":" + error.TrimEnd(','));
        GradingScaleItems.DeleteAllByGradingScale(gradingScaleNum);
        command = "DELETE FROM gradingscale WHERE GradingScaleNum = " + SOut.Long(gradingScaleNum);
        Db.NonQ(command);
    }
}