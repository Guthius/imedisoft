using System.Text;

namespace OpenDentBusiness.AutoComm;

public class ArrivalsTagReplacer : ApptTagReplacer
{
    public const string ArrivedTag = "[Arrived]";
    public const string ArrivedCode = "A";

    protected override void ReplaceTagsChild(StringBuilder sbTemplate, AutoCommObj autoCommObj, bool isEmail)
    {
        base.ReplaceTagsChild(sbTemplate, autoCommObj, isEmail);
        
        ReplaceOneTag(sbTemplate, ArrivedTag, ArrivedCode, isEmail);
    }
}