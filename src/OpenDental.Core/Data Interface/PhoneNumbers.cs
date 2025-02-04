using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;

namespace OpenDentBusiness;

public class PhoneNumbers
{
    public const int SyncBatchSize = 5000;

    public static void SyncAllPats()
    {
        //Get all PhoneNumbers we will delete later, anything except 'Other' that has a PhoneNumberVal.
        ODEvent.Fire(ODEventType.ProgressBar, "Initializing...");
        var command = $"SELECT PhoneNumberNum FROM phonenumber WHERE PhoneType!={(int) PhoneType.Other} OR PhoneNumberVal=''";
        var listPhoneNumberNumsToDelete = DataCore.GetList(command, x => SIn.Long(x["PhoneNumberNum"].ToString()));
        //Per clinic, including 0 clinic.
        var listClinics = Clinics.GetWhere(x => true).Concat(new List<ClinicDto> {Clinics.GetPracticeAsClinicZero()}).ToList();
        for (var i = 0; i < listClinics.Count; i++)
        {
            var clinic = listClinics[i];
            var clinicNum = clinic.Id;
            //Per Patient table phone number field.
            foreach (var phoneType in new List<PhoneType> {PhoneType.HmPhone, PhoneType.WkPhone, PhoneType.WirelessPhone}) AddPhoneNumbers(clinic, i, listClinics.Count, phoneType);
        }

        //Remove old PhoneNumbers in batches of 5000
        ODEvent.Fire(ODEventType.ProgressBar, Lans.g("PhoneNumber", "Cleaning up..."));
        while (listPhoneNumberNumsToDelete.Count > 0)
        {
            command = $"DELETE FROM phonenumber WHERE PhoneNumberNum IN ({string.Join(",", listPhoneNumberNumsToDelete.Take(SyncBatchSize).Select(x => (x)))})";
            Db.NonQ(command);
            listPhoneNumberNumsToDelete.RemoveRange(0, Math.Min(listPhoneNumberNumsToDelete.Count, SyncBatchSize));
        }
    }

    private static void AddPhoneNumbers(ClinicDto clinic, int clinicIndex, int countClinics, PhoneType phoneType)
    {
        //Map PhoneType to Patient phone number field.
        var field = phoneType switch
        {
            PhoneType.HmPhone => nameof(Patient.HmPhone),
            PhoneType.WkPhone => nameof(Patient.WkPhone),
            PhoneType.WirelessPhone => nameof(Patient.WirelessPhone),
            _ => ""
        };
        if (string.IsNullOrWhiteSpace(field)) return; //Skip on unknown field.
        //Get PatNums for all the patients with any value in this phone number field.
        var command = $"SELECT PatNum FROM patient WHERE {SOut.String(field)}!='' AND ClinicNum={(clinic.Id)}";
        var listPatNums = Db.GetListLong(command);
        var countPatNums = listPatNums.Count;
        var countPatsProcessed = 0;
        while (listPatNums.Count > 0)
        {
            //Process in batches.
            var listPatNumsBatch = listPatNums.Take(SyncBatchSize).ToList();
            countPatsProcessed += listPatNumsBatch.Count;
            ODEvent.Fire(ODEventType.ProgressBar, Lans.g("PhoneNumber", "Processing")
                                                  + $" ({clinicIndex + 1}/{countClinics}): {clinic.Abbr} {phoneType.GetDescription()} {countPatsProcessed}/{countPatNums}");
            //PhoneNumberNum,PatNum,PhoneNumberVal,PhoneNumberDigits,PhoneType
            command = $@"SELECT 
					0 PhoneNumberNum,
					PatNum,
					{SOut.String(field)} PhoneNumberVal,
					'' PhoneNumberDigits,
					{(int) phoneType} PhoneType
				FROM patient
				WHERE PatNum IN ({string.Join(",", listPatNumsBatch.Select(x => (x)))})";
            var listPhoneNumbers = PhoneNumberCrud.SelectMany(command);
            //Normalize PhoneNumberDigits field.
            listPhoneNumbers.ForEach(x => x.PhoneNumberDigits = RemoveNonDigitsAndTrimStart(x.PhoneNumberVal));
            //Ignore empty phone numbers.
            listPhoneNumbers.RemoveAll(x => x.PhoneType != PhoneType.Other && string.IsNullOrEmpty(x.PhoneNumberDigits));
            PhoneNumberCrud.InsertMany(listPhoneNumbers);
            listPatNums.RemoveRange(0, Math.Min(listPatNums.Count, SyncBatchSize));
        }
    }

    public static void SyncPat(Patient pat)
    {
        SyncPats([pat]);
    }

    public static void SyncPats(List<Patient> listPats)
    {
        if (listPats.Count == 0) return;
        var command = $@"DELETE FROM phonenumber
				WHERE PatNum IN ({string.Join(",", listPats.Select(x => (x.PatNum)))}) AND PhoneType!={(int) PhoneType.Other}";
        Db.NonQ(command);
        var listForInsert = listPats
            .SelectMany(x => Enumerable.Range(1, 3)
                .Select(y =>
                {
                    var phNumCur = y == 1 ? x.HmPhone : y == 2 ? x.WkPhone : y == 3 ? x.WirelessPhone : "";
                    return new PhoneNumber
                    {
                        PatNum = x.PatNum,
                        PhoneNumberVal = phNumCur,
                        PhoneNumberDigits = RemoveNonDigitsAndTrimStart(phNumCur),
                        PhoneType = (PhoneType) y
                    };
                }))
            .Where(x => !string.IsNullOrEmpty(x.PhoneNumberVal) && !string.IsNullOrEmpty(x.PhoneNumberDigits)).ToList();
        if (listForInsert.Count > 0) PhoneNumberCrud.InsertMany(listForInsert);
    }

    public static string RemoveNonDigitsAndTrimStart(string phoneNumber)
    {
        return string.IsNullOrEmpty(phoneNumber) ? string.Empty : new string(phoneNumber.Where(x => x is >= '0' and <= '9').ToArray()).TrimStart('0', '1');
    }
}