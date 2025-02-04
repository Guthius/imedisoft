using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Icd10 : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long Icd10Num;

    public string Icd10Code;
    public string Description;

    ///<summary>0 if the code is a “header” – not valid for submission on a UB04. 1 if the code is valid for submission on a UB04.</summary>
    public string IsCode;
}