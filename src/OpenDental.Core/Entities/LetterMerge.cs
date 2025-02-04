using System.Collections.Generic;
using Imedisoft.Core.Data;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class LetterMerge : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long LetterMergeNum;

    ///<summary>Description of this letter.</summary>
    public string Description;

    ///<summary>The filename of the Word template. eg MyTemplate.doc.</summary>
    public string TemplateName;

    ///<summary>The name of the data file. eg MyTemplate.txt.</summary>
    public string DataFileName;

    ///<summary>FK to definition.DefNum.</summary>
    public long Category;

    ///<summary>FK to definition.DefNum. This determines the default Image Category that will be selected when printing or previewing the letter.
    ///Can be 0 which means 'None' will be selected.</summary>
    public long ImageFolder;

    ///<summary>Not a database column.  Filled using fk from the lettermergefields table.  A collection of strings representing field names.</summary>
    public List<string> Fields => LetterMergeFields.GetForLetter(LetterMergeNum);

    public LetterMerge Copy()
    {
        return (LetterMerge) MemberwiseClone();
    }
}