using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PerioExams
{
    public static List<PerioExam> Refresh(long patNum)
    {
        return PerioExamCrud.SelectMany("SELECT * from perioexam WHERE PatNum = " + patNum + " ORDER BY perioexam.ExamDate");
    }

    public static bool HasPerio(long patNum)
    {
        return Db.GetLong("SELECT COUNT(perioExamNum) FROM perioexam WHERE PatNum = " + patNum) > 0;
    }

    public static void Update(PerioExam perioExam)
    {
        PerioExamCrud.Update(perioExam);
    }
    
    public static bool Update(PerioExam perioExam, PerioExam perioExamOld)
    {
        return PerioExamCrud.Update(perioExam, perioExamOld);
    }
    
    public static void Insert(PerioExam perioExam)
    {
        PerioExamCrud.Insert(perioExam);
    }
    
    public static void Delete(PerioExam perioExam)
    {
        Db.NonQ("DELETE from perioexam WHERE PerioExamNum = " + perioExam.PerioExamNum);
        Db.NonQ("DELETE from periomeasure WHERE PerioExamNum = " + perioExam.PerioExamNum);
    }
    
    public static PerioExam GetOnePerioExam(long perioExamNum)
    {
        return PerioExamCrud.SelectOne(perioExamNum);
    }
}