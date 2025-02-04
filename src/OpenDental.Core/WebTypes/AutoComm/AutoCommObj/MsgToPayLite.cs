using Imedisoft.Core.Entities;
using OpenDentBusiness.AutoComm;

namespace OpenDentBusiness;

public class MsgToPayLite : AutoCommObj
{
    public MsgToPayLite(Patient patient)
    {
        NameF = patient.FName;
        NamePreferredOrFirst = patient.GetNameFirstOrPreferred();
        ProvNum = patient.PriProv;
        PatNum = patient.PatNum;
    }
}