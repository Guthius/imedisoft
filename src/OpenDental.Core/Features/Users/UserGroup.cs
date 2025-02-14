using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class UserGroup : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long UserGroupNum;

    ///<summary>.</summary>
    public string Description;

    ///<summary>FK to usergroup.UserGroupNum.  The user group num within the Central Manager database.  Only editable via CEMT.  Can change when CEMT syncs.</summary>
    public long UserGroupNumCEMT;
    
    public UserGroup Copy()
    {
        return new UserGroup
        {
            UserGroupNum = UserGroupNum,
            Description = Description
        };
    }
}