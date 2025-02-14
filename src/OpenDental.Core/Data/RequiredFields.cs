using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class RequiredFields
{
    public static List<RequiredFieldName> GetFieldNamesForType(RequiredFieldType requiredFieldType)
    {
        var requiredFieldNames = new List<RequiredFieldName>();

        switch (requiredFieldType)
        {
            case RequiredFieldType.PatientInfo:
                requiredFieldNames =
                [
                    RequiredFieldName.Address,
                    RequiredFieldName.Address2,
                    RequiredFieldName.AddressPhoneNotes,
                    RequiredFieldName.AdmitDate,
                    RequiredFieldName.AskArriveEarly,
                    RequiredFieldName.BillingType,
                    RequiredFieldName.Birthdate,
                    RequiredFieldName.Carrier,
                    RequiredFieldName.ChartNumber,
                    RequiredFieldName.City,
                    RequiredFieldName.Clinic,
                    RequiredFieldName.CollegeName,
                    RequiredFieldName.County,
                    RequiredFieldName.CreditType,
                    RequiredFieldName.DateFirstVisit,
                    RequiredFieldName.DateTimeDeceased,
                    RequiredFieldName.DischargeDate,
                    RequiredFieldName.EligibilityExceptCode,
                    RequiredFieldName.EmailAddress,
                    RequiredFieldName.EmergencyName,
                    RequiredFieldName.EmergencyPhone,
                    RequiredFieldName.Employer,
                    RequiredFieldName.Ethnicity,
                    RequiredFieldName.FeeSchedule,
                    RequiredFieldName.FirstName,
                    RequiredFieldName.Gender,
                    RequiredFieldName.GenderIdentity,
                    RequiredFieldName.GradeLevel,
                    RequiredFieldName.GroupName,
                    RequiredFieldName.GroupNum,
                    RequiredFieldName.HomePhone,
                    RequiredFieldName.InsurancePhone,
                    RequiredFieldName.InsuranceSubscriber,
                    RequiredFieldName.InsuranceSubscriberID,
                    RequiredFieldName.Language,
                    RequiredFieldName.LastName,
                    RequiredFieldName.Position,
                    RequiredFieldName.MedicaidID,
                    RequiredFieldName.MedicaidState,
                    RequiredFieldName.MiddleInitial,
                    RequiredFieldName.MothersMaidenFirstName,
                    RequiredFieldName.MothersMaidenLastName,
                    RequiredFieldName.PatientStatus,
                    RequiredFieldName.PreferConfirmMethod,
                    RequiredFieldName.PreferContactMethod,
                    RequiredFieldName.PreferRecallMethod,
                    RequiredFieldName.PreferredName,
                    RequiredFieldName.PrimaryProvider,
                    RequiredFieldName.Race,
                    RequiredFieldName.ReferredFrom,
                    RequiredFieldName.ResponsibleParty,
                    RequiredFieldName.Salutation,
                    RequiredFieldName.SecondaryProvider,
                    RequiredFieldName.SexualOrientation,
                    RequiredFieldName.Site,
                    RequiredFieldName.SocialSecurityNumber,
                    RequiredFieldName.State,
                    RequiredFieldName.StudentStatus,
                    RequiredFieldName.TextOK,
                    RequiredFieldName.Title,
                    RequiredFieldName.TreatmentUrgency,
                    RequiredFieldName.TrophyFolder,
                    RequiredFieldName.Ward,
                    RequiredFieldName.WirelessPhone,
                    RequiredFieldName.WorkPhone,
                    RequiredFieldName.Zip
                ];
                break;

            case RequiredFieldType.InsPayEdit:
                requiredFieldNames =
                [
                    RequiredFieldName.BatchNumber,
                    RequiredFieldName.CheckDate,
                    RequiredFieldName.CheckNumber,
                    RequiredFieldName.DepositAccountNumber,
                    RequiredFieldName.DepositDate,
                    RequiredFieldName.InsPayEditClinic,
                    RequiredFieldName.PaymentAmount,
                    RequiredFieldName.PaymentType
                ];
                break;
        }

        return requiredFieldNames;
    }

    public static void Insert(RequiredField requiredField)
    {
        RequiredFieldCrud.Insert(requiredField);
    }

    public static void Delete(long requiredFieldNum)
    {
        Db.NonQ("DELETE FROM requiredfieldcondition WHERE RequiredFieldNum = " + requiredFieldNum);

        RequiredFieldCrud.Delete(requiredFieldNum);
    }

    public static List<RequiredField> GetRequiredFields()
    {
        var requiredFields = GetWhere(x => x.FieldType == RequiredFieldType.PatientInfo);

        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.InsuranceSubscriber);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.InsuranceSubscriberID);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.Carrier);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.InsurancePhone);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.GroupName);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.GroupNum);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.MothersMaidenFirstName);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.MothersMaidenLastName);
        requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.DateTimeDeceased);

        if (!Programs.IsEnabled(Programs.GetProgramNum(ProgramName.TrophyEnhanced)))
        {
            requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.TrophyFolder);
        }

        if (PrefC.GetBool(PrefName.EasyHideHospitals))
        {
            requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.Ward);
            requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.AdmitDate);
            requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.DischargeDate);
        }

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.StudentStatus);
        }
        else
        {
            requiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.EligibilityExceptCode);
        }

        if (PrefC.GetBool(PrefName.EasyHidePublicHealth))
        {
            requiredFields.RemoveAll(x => x.FieldName is
                RequiredFieldName.Race or
                RequiredFieldName.Ethnicity or
                RequiredFieldName.County or
                RequiredFieldName.Site or
                RequiredFieldName.GradeLevel or
                RequiredFieldName.TreatmentUrgency or
                RequiredFieldName.ResponsibleParty or
                RequiredFieldName.SexualOrientation or
                RequiredFieldName.GenderIdentity);
        }

        return requiredFields;
    }

    private class RequiredFieldCache : CacheListAbs<RequiredField>
    {
        protected override List<RequiredField> GetCacheFromDb()
        {
            return RequiredFieldCrud.SelectMany("SELECT * FROM requiredfield ORDER BY FieldType, FieldName");
        }

        protected override List<RequiredField> TableToList(DataTable dataTable)
        {
            return RequiredFieldCrud.TableToList(dataTable);
        }

        protected override RequiredField Copy(RequiredField item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<RequiredField> items)
        {
            return RequiredFieldCrud.ListToTable(items, "RequiredField");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly RequiredFieldCache Cache = new();

    public static List<RequiredField> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<RequiredField> GetWhere(Predicate<RequiredField> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}