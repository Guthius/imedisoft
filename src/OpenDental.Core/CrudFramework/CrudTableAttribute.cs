using System;
using System.Linq;

namespace OpenDentBusiness;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CrudTableAttribute : Attribute
{
    public string TableName { get; set; } = "";
    public bool IsDeleteForbidden { get; set; }
    public bool IsMissingInGeneral { get; set; }
    public bool IsSynchable { get; set; }
    public bool IsSynchableBatchWriteMethods { get; set; }
    public CrudAuditPerm AuditPerms { get; set; } = CrudAuditPerm.None;
    public bool IsSecurityStamped { get; set; }
    public bool HasBatchWriteMethods { get; set; }
    public string CrudLocationOverride { get; set; }
    public string NamespaceOverride { get; set; }
    public bool CrudExcludePrefC { get; set; }
    public bool IsLargeTable { get; set; }
    public bool UsesDataReader { get; set; }

    public static CrudAuditPerm GetCrudAuditPermForClass(Type typeClass)
    {
        var attributes = typeClass.GetCustomAttributes(typeof(CrudTableAttribute), true);
        if (attributes.Length == 0)
        {
            return CrudAuditPerm.None;
        }

        return attributes
            .OfType<CrudTableAttribute>()
            .Where(attribute => attribute.AuditPerms != CrudAuditPerm.None)
            .Select(attribute => attribute.AuditPerms)
            .FirstOrDefault();
    }
}

[Flags]
public enum CrudAuditPerm
{
    None = 0,
    AppointmentCompleteEdit = 1,
    AppointmentCreate = 2,
    AppointmentEdit = 4,
    AppointmentMove = 8,
    ClaimHistoryEdit = 16,
    ImageDelete = 32,
    ImageEdit = 64,
    InsPlanChangeCarrierName = 128,
    RxCreate = 256,
    RxEdit = 512,
    TaskNoteEdit = 1024,
    PatientPortal = 2048,
    ProcFeeEdit = 4096,
    LogFeeEdit = 8192,
    LogSubscriberEdit = 16384,
    AppointmentDelete = 32768,
    AppointmentCompleteDelete = 65536
}