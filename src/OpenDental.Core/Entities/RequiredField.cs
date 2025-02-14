using System.Collections.Generic;
using Imedisoft.Core.Data;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class RequiredField : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long RequiredFieldNum;

    public RequiredFieldType FieldType;
    public RequiredFieldName FieldName;

    [CrudColumn(IsNotDbColumn = true)]
    private List<RequiredFieldCondition> _listRequiredFieldConditions;

    public List<RequiredFieldCondition> ListRequiredFieldConditions
    {
        get
        {
            if (_listRequiredFieldConditions != null) return _listRequiredFieldConditions;

            _listRequiredFieldConditions = RequiredFieldNum == 0 ? [] : RequiredFieldConditions.GetForRequiredField(RequiredFieldNum);

            return _listRequiredFieldConditions;
        }
    }

    public void RefreshConditions()
    {
        _listRequiredFieldConditions = null;

        RequiredFieldConditions.RefreshCache();
    }

    public RequiredField Clone()
    {
        return (RequiredField) MemberwiseClone();
    }
}

public enum RequiredFieldType
{
    ///<summary>0 - Edit Patient Information window and Add Family (FormPatientAddAll) window.</summary>
    PatientInfo,

    ///<summary>1 - Edit Claim Payment window.</summary>
    InsPayEdit
}

public enum RequiredFieldName
{
    Address,
    Address2,
    AddressPhoneNotes,
    AdmitDate,
    AskArriveEarly,
    BatchNumber,
    BillingType,
    Birthdate,
    Carrier,
    ChartNumber,
    CheckDate,
    CheckNumber,
    City,
    Clinic,
    CollegeName,
    County,
    CreditType,
    DateFirstVisit,
    DateTimeDeceased,
    DepositAccountNumber,
    DepositDate,
    DischargeDate,
    EligibilityExceptCode,
    EmailAddress,
    EmergencyName,
    EmergencyPhone,
    Employer,
    Ethnicity,
    FeeSchedule,
    FirstName,
    Gender,
    GenderIdentity,
    GradeLevel,
    GroupName,
    GroupNum,
    HomePhone,
    InsPayEditClinic,
    InsurancePhone,
    InsuranceSubscriber,
    InsuranceSubscriberID,
    Language,
    LastName,
    PaymentAmount,
    PaymentType,
    Position,
    MedicaidID,
    MedicaidState,
    MiddleInitial,
    MothersMaidenFirstName,
    MothersMaidenLastName,
    PatientStatus,
    PreferConfirmMethod,
    PreferContactMethod,
    PreferRecallMethod,
    PreferredName,
    PrimaryProvider,
    Race,
    ReferredFrom,
    ResponsibleParty,
    Salutation,
    SecondaryProvider,
    SexualOrientation,
    Site,
    SocialSecurityNumber,
    State,
    StudentStatus,
    TextOK,
    Title,
    TreatmentUrgency,
    TrophyFolder,
    Ward,
    WirelessPhone,
    WorkPhone,
    Zip
}