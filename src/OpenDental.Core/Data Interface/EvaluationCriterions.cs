using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EvaluationCriterions
{
    ///<summary>Get all Criterion attached to an Evaluation.</summary>
    public static List<EvaluationCriterion> Refresh(long evaluationNum)
    {
        var command = "SELECT * FROM evaluationcriterion WHERE EvaluationNum = " + SOut.Long(evaluationNum);
        return EvaluationCriterionCrud.SelectMany(command);
    }

    ///<summary>Gets one EvaluationCriterion from the db.</summary>
    public static EvaluationCriterion GetOne(long evaluationCriterionNum)
    {
        return EvaluationCriterionCrud.SelectOne(evaluationCriterionNum);
    }

    
    public static long Insert(EvaluationCriterion evaluationCriterion)
    {
        return EvaluationCriterionCrud.Insert(evaluationCriterion);
    }

    
    public static void Update(EvaluationCriterion evaluationCriterion)
    {
        EvaluationCriterionCrud.Update(evaluationCriterion);
    }

    
    public static void Delete(long evaluationCriterionNum)
    {
        var command = "DELETE FROM evaluationcriterion WHERE EvaluationCriterionNum = " + SOut.Long(evaluationCriterionNum);
        Db.NonQ(command);
    }
}