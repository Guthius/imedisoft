using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Questions
{
    ///<summary>Gets a list of all Questions for a given patient.  Sorted by ItemOrder.</summary>
    public static Question[] Refresh(long patNum)
    {
        var command = "SELECT * FROM question WHERE PatNum=" + SOut.Long(patNum)
                                                             + " ORDER BY ItemOrder";
        return QuestionCrud.SelectMany(command).ToArray();
    }

    
    public static void Update(Question quest)
    {
        QuestionCrud.Update(quest);
    }

    
    public static long Insert(Question quest)
    {
        return QuestionCrud.Insert(quest);
    }
}