using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FormPats
{
    
    public static long Insert(FormPat formPat)
    {
        return FormPatCrud.Insert(formPat);
    }

    public static FormPat GetOne(long formPatNum)
    {
        var command = "SELECT * FROM formpat WHERE FormPatNum=" + SOut.Long(formPatNum);
        var formpat = FormPatCrud.SelectOne(formPatNum);
        if (formpat == null) return null; //should never happen.
        command = "SELECT * FROM question WHERE FormPatNum=" + SOut.Long(formPatNum);
        formpat.QuestionList = QuestionCrud.SelectMany(command);
        return formpat;
    }

    
    public static void Delete(long formPatNum)
    {
        var command = "DELETE FROM formpat WHERE FormPatNum=" + SOut.Long(formPatNum);
        Db.NonQ(command);
        command = "DELETE FROM question WHERE FormPatNum=" + SOut.Long(formPatNum);
        Db.NonQ(command);
    }
}