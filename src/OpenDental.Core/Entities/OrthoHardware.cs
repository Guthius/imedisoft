using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoHardware : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoHardwareNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>Every hardware entry is tied to a single date. At each exam, a copy can be made of the hardware from the previous exam, and then it can be edited. It normally shows the most recent exam, and the hardware items showing in the ortho grid only include the most recent exam. Not sure yet how we will show hardware for previous exams/dates.</summary>
    public DateTime DateExam;

    public EnumOrthoHardwareType OrthoHardwareType;

    ///<summary>FK to orthohardwarespec.OrthoHardwareSpecNum. This is where the description and color come from.</summary>
    public long OrthoHardwareSpecNum;

    /// <summary>
    /// Tooth numbers stored here are always stored in Universal (1-32) notation.
    /// They are displayed to the user as Palmer notation. For brackets, always use single tooth numbers,
    /// like 8. For wires, must use a range like 2-15. For elastics, typically use 2 teeth separated with commas,
    /// but more are allowed.
    /// </summary>
    public string ToothRange;

    public string Note;
    public bool IsHidden;
}

public enum EnumOrthoHardwareType
{
    Bracket,
    Wire,
    Elastic
}