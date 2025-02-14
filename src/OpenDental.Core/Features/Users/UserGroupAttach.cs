using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class UserGroupAttach : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long UserGroupAttachNum;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    ///<summary>FK to usergroup.UserGroupNum. </summary>
    public long UserGroupNum;

    public UserGroupAttach Copy()
    {
        return (UserGroupAttach) MemberwiseClone();
    }
}