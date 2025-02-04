using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EmailAutograph : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EmailAutographNum;

    public string Description;
    public string EmailAddress;
    public string AutographText;

    public EmailAutograph Copy()
    {
        return (EmailAutograph) MemberwiseClone();
    }
}