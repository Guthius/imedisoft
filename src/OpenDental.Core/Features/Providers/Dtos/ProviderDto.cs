using System;
using System.Collections.Generic;

namespace Imedisoft.Features.Providers.Dtos;

public sealed record ProviderDto
{
    public long Id { get; set; }
    public ProviderSpecialtyDto Specialty { get; set; } = new();
    public string Abbr { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public string PreferredName { get; set; } = string.Empty;
    public string Ssn { get; set; } = string.Empty;
    public bool IsTin { get; set; }
    public string NationalProviderId { get; set; } = string.Empty;
    public string MedicaidId { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string SchedulerNote { get; set; } = string.Empty;
    public long? FeeScheduleId { get; set; }
    public decimal HourlyProductionGoal { get; set; }
    public ProviderNameDto? BillingProvider { get; set; }
    public string? TaxonomyCode { get; set; }
    public string Color { get; set; } = string.Empty;
    public string OutlineColor { get; set; } = string.Empty;
    public bool IsCdaNet { get; set; }
    public string CanadianOfficeNumber { get; set; } = string.Empty;
    public bool IsSecondary { get; set; }
    public bool IsNotPerson { get; set; }
    public bool IsSignatureOnFile { get; set; }
    public bool IsHiddenFromReports { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? TerminatedOn { get; set; }
    public List<ProviderClinicDto> Clinics { get; set; } = [];
    public List<ProviderIdentityDto> Identities { get; set; } = [];

    public string Description
    {
        get
        {
            var description = Abbr + "- " + LastName + ", " + FirstName;

            if (IsHidden)
            {
                description += " (hidden)";
            }

            return description;
        }
    }

    public string FormalName
    {
        get
        {
            var formalName = FirstName + " " + MiddleName;
            if (MiddleName.Length == 1)
            {
                formalName += ".";
            }

            if (MiddleName != "")
            {
                formalName += " ";
            }

            formalName += LastName;
            if (Suffix != "")
            {
                formalName += ", " + Suffix;
            }

            return formalName;
        }
    }
}