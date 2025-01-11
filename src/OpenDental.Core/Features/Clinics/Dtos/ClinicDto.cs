using System;

namespace Imedisoft.Core.Features.Clinics.Dtos;

public sealed record ClinicDto
{
    public long Id { get; set; }
    public string Abbr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FaxNumber { get; set; } = string.Empty;
    public string BankNumber { get; set; } = string.Empty;
    public long? DefaultProviderId { get; set; }
    public string DefaultPlaceOfService { get; set; }
    public long? EmailAddressId { get; set; }
    public string EmailAliasOverride { get; set; } = string.Empty;
    public string MedLabAccountNumber { get; set; } = string.Empty;
    public string SchedulingNote { get; set; } = string.Empty;
    public long? RegionId { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public BillingProviderType BillingProviderType { get; set; }
    public long? BillingProviderId { get; set; }
    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string BillingAddressLine2 { get; set; } = string.Empty;
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZip { get; set; } = string.Empty;
    public bool UseBillingAddressOnClaims { get; set; }
    public string PayToAddressLine1 { get; set; } = string.Empty;
    public string PayToAddressLine2 { get; set; } = string.Empty;
    public string PayToCity { get; set; } = string.Empty;
    public string PayToState { get; set; } = string.Empty;
    public string PayToZip { get; set; } = string.Empty;
    public DateTime? SmsContractSignedOn { get; set; }
    public double SmsMonthlyLimit { get; set; }
    public bool HasProceduresOnRx { get; set; }
    public bool ExcludeFromInsuranceVerification { get; set; }
    public bool IsConfirmEnabled { get; set; }
    public bool IsMedicalOnly { get; set; }
    public bool IsHidden { get; set; }
}