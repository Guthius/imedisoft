using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Contact : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ContactNum;

    public string LName;
    public string FName;
    public string WkPhone;
    public string Fax;

    ///<summary>FK to definition.DefNum</summary>
    public long Category;
    
    public string Notes;
}