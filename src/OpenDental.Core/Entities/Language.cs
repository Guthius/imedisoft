using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Language : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long LanguageNum;

    ///<summary>No longer used.</summary>
    public string EnglishComments;

    ///<summary>A string representing the class where the translation is used.</summary>
    public string ClassType;

    ///<summary>The English version of the phrase, case sensitive.</summary>
    public string English;

    ///<summary>As this gets more sophisticated, we will use this field to mark some phrases obsolete instead of just deleting them outright.  That way, translators will still have access to them.  For now, this is not used at all.</summary>
    public bool IsObsolete;

    public Language Copy()
    {
        return (Language) MemberwiseClone();
    }
}