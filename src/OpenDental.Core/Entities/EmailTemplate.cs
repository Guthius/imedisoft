using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EmailTemplate : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EmailTemplateNum;

    public string Subject;
    public string BodyText;
    public string Description;
    public EmailType TemplateType;

    public EmailTemplate Copy()
    {
        return (EmailTemplate) MemberwiseClone();
    }
}