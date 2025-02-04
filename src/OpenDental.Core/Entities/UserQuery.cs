using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class UserQuery : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long QueryNum;

    public string Description;

    ///<summary>The name of the file to export to.</summary>
    public string FileName;

    ///<summary>The text of the query.</summary>
    public string QueryText;

    ///<summary>Determines whether the query is safe for users with lower permissions.  Also causes this user query to be available in the Main Menu, Reports, Query Favorites Filtered.</summary>
    public bool IsReleased;

    ///<summary>Determines whether the Query Favorites window should prompt for query values via FormQueryParser/'SET Fields' popup when running query.</summary>
    public bool IsPromptSetup;

    ///<summary>Determines whether the UserQuery window loads with the 'Raw' format radio button pre-selected. For a new userquery, this is set based on pref.UserQueryDefaultRaw.</summary>
    public bool DefaultFormatRaw;

    public UserQuery Copy()
    {
        return (UserQuery) MemberwiseClone();
    }
}