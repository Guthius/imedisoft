using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class RequiredFields
{
    public static List<RequiredFieldName> GetFieldNamesForType(RequiredFieldType requiredFieldType)
    {
        var retVal = new List<RequiredFieldName>();
        switch (requiredFieldType)
        {
            case RequiredFieldType.PatientInfo:
                retVal = new List<RequiredFieldName>
                {
                    RequiredFieldName.Address, RequiredFieldName.Address2, RequiredFieldName.AddressPhoneNotes, RequiredFieldName.AdmitDate,
                    RequiredFieldName.AskArriveEarly, RequiredFieldName.BillingType, RequiredFieldName.Birthdate, RequiredFieldName.Carrier,
                    RequiredFieldName.ChartNumber, RequiredFieldName.City, RequiredFieldName.Clinic, RequiredFieldName.CollegeName, RequiredFieldName.County,
                    RequiredFieldName.CreditType, RequiredFieldName.DateFirstVisit, RequiredFieldName.DateTimeDeceased, RequiredFieldName.DischargeDate, RequiredFieldName.EligibilityExceptCode,
                    RequiredFieldName.EmailAddress, RequiredFieldName.EmergencyName, RequiredFieldName.EmergencyPhone, RequiredFieldName.Employer,
                    RequiredFieldName.Ethnicity, RequiredFieldName.FeeSchedule, RequiredFieldName.FirstName, RequiredFieldName.Gender, RequiredFieldName.GenderIdentity,
                    RequiredFieldName.GradeLevel, RequiredFieldName.GroupName, RequiredFieldName.GroupNum, RequiredFieldName.HomePhone, RequiredFieldName.InsurancePhone,
                    RequiredFieldName.InsuranceSubscriber, RequiredFieldName.InsuranceSubscriberID, RequiredFieldName.Language, RequiredFieldName.LastName,
                    RequiredFieldName.Position, RequiredFieldName.MedicaidID, RequiredFieldName.MedicaidState, RequiredFieldName.MiddleInitial,
                    RequiredFieldName.MothersMaidenFirstName, RequiredFieldName.MothersMaidenLastName, RequiredFieldName.PatientStatus,
                    RequiredFieldName.PreferConfirmMethod, RequiredFieldName.PreferContactMethod, RequiredFieldName.PreferRecallMethod, RequiredFieldName.PreferredName,
                    RequiredFieldName.PrimaryProvider, RequiredFieldName.Race, RequiredFieldName.ReferredFrom, RequiredFieldName.ResponsibleParty,
                    RequiredFieldName.Salutation, RequiredFieldName.SecondaryProvider, RequiredFieldName.SexualOrientation, RequiredFieldName.Site,
                    RequiredFieldName.SocialSecurityNumber, RequiredFieldName.State, RequiredFieldName.StudentStatus, RequiredFieldName.TextOK, RequiredFieldName.Title,
                    RequiredFieldName.TreatmentUrgency, RequiredFieldName.TrophyFolder, RequiredFieldName.Ward, RequiredFieldName.WirelessPhone,
                    RequiredFieldName.WorkPhone, RequiredFieldName.Zip
                };
                break;
            case RequiredFieldType.InsPayEdit:
                retVal = new List<RequiredFieldName>
                {
                    RequiredFieldName.BatchNumber, RequiredFieldName.CheckDate, RequiredFieldName.CheckNumber,
                    RequiredFieldName.DepositAccountNumber, RequiredFieldName.DepositDate, RequiredFieldName.InsPayEditClinic,
                    RequiredFieldName.PaymentAmount, RequiredFieldName.PaymentType
                };
                break;
        }

        return retVal;
    }

    public static void Insert(RequiredField requiredField)
    {
        RequiredFieldCrud.Insert(requiredField);
    }

    public static void Delete(long requiredFieldNum)
    {
        var command = "DELETE FROM requiredfieldcondition WHERE RequiredFieldNum=" + SOut.Long(requiredFieldNum);
        Db.NonQ(command);
        RequiredFieldCrud.Delete(requiredFieldNum);
    }

    public static List<RequiredField> GetRequiredFields()
    {
        var listRequiredFields = GetWhere(x => x.FieldType == RequiredFieldType.PatientInfo);
        //Remove the RequiredFields that are only on the Add Family window
        listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.InsuranceSubscriber);
        listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.InsuranceSubscriberID);
        listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.Carrier);
        listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.InsurancePhone);
        listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.GroupName);
        listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.GroupNum);
        //Remove RequiredFields where the text field is invisible.
        if (!PrefC.GetBool(PrefName.ShowFeatureEhr))
        {
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.MothersMaidenFirstName);
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.MothersMaidenLastName);
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.DateTimeDeceased);
        }

        if (!Programs.IsEnabled(Programs.GetProgramNum(ProgramName.TrophyEnhanced))) listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.TrophyFolder);
        if (PrefC.GetBool(PrefName.EasyHideHospitals))
        {
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.Ward);
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.AdmitDate);
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.DischargeDate);
        }

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA")) //Canadian. en-CA or fr-CA
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.StudentStatus);
        else //Not Canadian
            listRequiredFields.RemoveAll(x => x.FieldName == RequiredFieldName.EligibilityExceptCode);
        //Remove Required Fields if the Public Health Tab(tabPublicHealth) is hidden
        if (PrefC.GetBool(PrefName.EasyHidePublicHealth))
            listRequiredFields.RemoveAll(x => x.FieldName.In(
                RequiredFieldName.Race, RequiredFieldName.Ethnicity, RequiredFieldName.County,
                RequiredFieldName.Site, RequiredFieldName.GradeLevel, RequiredFieldName.TreatmentUrgency,
                RequiredFieldName.ResponsibleParty, RequiredFieldName.SexualOrientation, RequiredFieldName.GenderIdentity
            ));
        return listRequiredFields;
    }
    
    private class RequiredFieldCache : CacheListAbs<RequiredField>
    {
        protected override List<RequiredField> GetCacheFromDb()
        {
            var command = "SELECT * FROM requiredfield ORDER BY FieldType,FieldName";
            return RequiredFieldCrud.SelectMany(command);
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
            RequiredFields.GetTableFromCache(false);
        }
    }

    private static readonly RequiredFieldCache Cache = new();

    public static List<RequiredField> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<RequiredField> GetWhere(Predicate<RequiredField> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}