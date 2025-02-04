using System;
using System.Collections.Generic;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class FeeSchedNote : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long FeeSchedNoteNum;

    ///<summary>FK to feesched.FeeSchedNum.</summary>
    public long FeeSchedNum;

    ///<summary>A comma delimited list of clinic nums that the fee schedule note is for.</summary>
    public string ClinicNums;

    public string Note;

    ///<summary>The date of this note. User is allowed to change this date.</summary>
    public DateTime DateEntry;

    ///<summary>FK to userod.UserNum. Set to the user logged in when the row was inserted at SecDateEntry date and time.</summary>
    public long SecUserNumEntry;

    ///<summary>Timestamp automatically generated and user not allowed to change. The actual date of entry.</summary>
    public DateTime SecDateEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program updates. Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    ///<summary>Not a db column. Holds a list of clinic nums that this fee schedule note is assigned to.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public List<long> ListClinicNums = [];
}