using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PerioExam : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PerioExamNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    public DateTime ExamDate;

    ///<summary>FK to provider.ProvNum.</summary>
    public long ProvNum;

    ///<summary>Date and time PerioExam was created or modified, including the associated PerioMeasure rows.</summary>
    public DateTime DateTMeasureEdit;

    public string Note;

    public PerioExam Copy()
    {
        return (PerioExam) MemberwiseClone();
    }
}