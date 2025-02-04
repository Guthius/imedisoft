using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Cdcrec : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CdcrecNum;

    ///<summary>CDCREC Code.  Example: 1002-5.  Not allowed to edit this column once saved in the database.</summary>
    public string CdcrecCode;

    ///<summary>Heirarchical Code. Example:
    ///<para>R1       =="American Indian or alaska Native"</para>
    ///<para>R1.01    =="American Indian"</para>
    ///<para>R1.01.001=="Abenaki"</para>
    ///<para>Not allowed to edit this column once saved in the database. </para></summary>
    public string HeirarchicalCode;

    public string Description;
}