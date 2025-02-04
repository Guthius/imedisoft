using System.Collections.Generic;

namespace OpenDentBusiness.AutoComm;

public class AutoCommObj
{
    public long PatNum;
    public long ProvNum;
    public string NameF;
    public string NamePreferredOrFirst;
    public string NamePreferred;
    public long StatementNum;

    protected virtual void SetPatientContact(PatComm patComm, Dictionary<long, PatComm> dictPatComms)
    {
        if (patComm is null)
        {
            return;
        }

        NameF = patComm.FName;
        NamePreferredOrFirst = patComm.GetFirstOrPreferred();
        
        if (patComm.PatNum == PatNum)
        {
            return;
        }
        
        if (!dictPatComms.TryGetValue(PatNum, out var patientPatComm))
        {
            return;
        }
        
        NameF = patientPatComm.FName;
        NamePreferredOrFirst = patientPatComm.GetFirstOrPreferred();
    }
}