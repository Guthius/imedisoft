using System.Text;

namespace OpenDentBusiness.AutoComm;

public class ApptTagReplacer : TagReplacer
{
    protected override void ReplaceTagsChild(StringBuilder sbTemplate, AutoCommObj autoCommObj, bool isEmail)
    {
        if (autoCommObj is not ApptLite appt)
        {
            return;
        }
            
        ReplaceOneTag(sbTemplate, "[ApptTime]", appt.AptDateTime.ToString(PrefC.PatientCommunicationTimeFormat), isEmail);
        ReplaceOneTag(sbTemplate, "[ApptDate]", appt.AptDateTime.ToString(PrefC.PatientCommunicationDateFormat), isEmail);
        ReplaceOneTag(sbTemplate, "[ApptTimeAskedArrive]", appt.DateTimeAskedToArrive.ToString(PrefC.PatientCommunicationTimeFormat), isEmail);
    }
}