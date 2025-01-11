using System;

namespace OpenDentBusiness;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CrudTableAttribute : Attribute
{
    public string TableName { get; set; } = "";
    public bool IsDeleteForbidden { get; set; } = false;
    public bool IsMissingInGeneral { get; set; } = false;
    public bool IsSynchable { get; set; } = false;
    public bool IsSynchableBatchWriteMethods { get; set; } = false;
    public CrudAuditPerm AuditPerms { get; set; } = CrudAuditPerm.None;
    public bool IsSecurityStamped { get; set; } = false;
    public bool HasBatchWriteMethods { get; set; } = false;
    public string CrudLocationOverride { get; set; }
    public string NamespaceOverride { get; set; }
    public bool CrudExcludePrefC { get; set; }
    public bool IsTableHist { get; set; } = false;
    public bool IsLargeTable { get; set; }
    public bool UsesDataReader { get; set; } = false;

    public static CrudAuditPerm GetCrudAuditPermForClass(Type typeClass)
    {
        var attributes = typeClass.GetCustomAttributes(typeof(CrudTableAttribute), true);
        if (attributes.Length == 0)
        {
            return CrudAuditPerm.None;
        }

        foreach (var t in attributes)
        {
            if (t.GetType() != typeof(CrudTableAttribute))
            {
                continue;
            }

            if (((CrudTableAttribute) t).AuditPerms != CrudAuditPerm.None)
            {
                return ((CrudTableAttribute) t).AuditPerms;
            }
        }

        return CrudAuditPerm.None;
    }

    public static string GetTableName(Type typeClass)
    {
        var attributes = typeClass.GetCustomAttributes(typeof(CrudTableAttribute), true);
        if (attributes.Length == 0)
        {
            return typeClass.Name.ToLower();
        }

        for (var i = 0; i < attributes.Length; i++)
        {
            if (attributes[i].GetType() != typeof(CrudTableAttribute))
            {
                continue;
            }

            if (((CrudTableAttribute) attributes[i]).TableName != "")
            {
                return ((CrudTableAttribute) attributes[i]).TableName;
            }
        }

        //couldn't find any override.
        return typeClass.Name.ToLower();
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