namespace OpenDentBusiness
{
    public class FHIRUtils
    {
        public static FHIRKeyStatus ToFHIRKeyStatus(APIKeyStatus statusOld)
        {
            return statusOld switch
            {
                APIKeyStatus.ReadEnabled or APIKeyStatus.WritePending => FHIRKeyStatus.EnabledReadOnly,
                APIKeyStatus.WriteEnabled => FHIRKeyStatus.Enabled,
                _ => FHIRKeyStatus.DisabledByHQ
            };
        }
    }
}