using CodeBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Imedisoft.Core.Caching;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness.WebTypes.WebForms;

public class WebForms_Sheets
{
    public static List<long> GetSheetIDs(string regKey = null, List<long> listClinicNums = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        listClinicNums ??= [];

        var listPayloadItems = new List<PayloadItem>
        {
            new(regKey, "RegKey"),
            new(listClinicNums, "ListClinicNums"),
        };
        var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        
        var response = SheetsSynchProxy.GetWebServiceInstance().GetWebFormSheetIDs(payload);
        return WebSerializer.DeserializeTag<List<long>>(response, "Success");
    }

    public static List<WebForms_Sheet> GetSheets(string regKey = null, List<long> listSheetIDs = null, List<long> listClinicNums = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        listSheetIDs ??= [];
        listClinicNums ??= [];

        var listPayloadItems = new List<PayloadItem>
        {
            new(regKey, "RegKey"),
            new(listSheetIDs, "ListSheetIDs"),
            new(listClinicNums, "ListClinicNums"),
        };
        
        var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        //Get pending sheets from HQ.
        var resultXml = SheetsSynchProxy.GetWebServiceInstance().GetWebFormSheets(payload);
        var listWebForms_Sheets = WebSerializer.DeserializeTag<List<WebForms_Sheet>>(resultXml, "Success");
        for (var i = 0; i < listWebForms_Sheets.Count; i++)
        {
            var eServiceLog = EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFDownloadedForm, 0, listWebForms_Sheets[i].ClinicNum, listWebForms_Sheets[i].SheetID);
            listWebForms_Sheets[i].EServiceLogGuid = eServiceLog.LogGuid;
        }

        return listWebForms_Sheets;
    }

    public static void DeleteSheetData(List<WebForms_Sheet> listWebForms_Sheets, string regKey = null)
    {
        if (listWebForms_Sheets.IsNullOrEmpty())
        {
            return;
        }

        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var listPayloadItems = new List<PayloadItem>
            {
                new(regKey, "RegKey"),
                new(listWebForms_Sheets.Select(x => x.SheetID).ToList(), "SheetNumsForDeletion")
            };
            
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
            var result = SheetsSynchProxy.GetWebServiceInstance().DeleteSheetData(payload);
            
            PayloadHelper.CheckForError(result);
            
            for (var i = 0; i < listWebForms_Sheets.Count; i++)
            {
                EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFDeletedForm, FKey: listWebForms_Sheets[i].SheetID, logGuid: listWebForms_Sheets[i].EServiceLogGuid);
            }
        }
        catch (Exception ex)
        {
            var log = Lans.g("FormWebForms", "There was a problem telling HQ that the web forms were retrieved:") + $" '{ex.Message}'";
            
            log += "\r\n" + "  ^" + Lans.g("FormWebForms", "The following web forms will be downloaded again the next time forms are retrieved.");
            log += "\r\n" + "  ^" + Lans.g("FormWebForms", "SheetIDs:") + " " + string.Join(", ", listWebForms_Sheets.Select(x => x.SheetID));
            
            EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFError, note: log);
        }
    }

    public static void ParseWebFormSheet(WebForms_Sheet sheet, string webFormPrefCulture, out string lName, out string fName, out DateTime birthdate, out List<string> listPhoneNumbers, out string email)
    {
        lName = "";
        fName = "";
        birthdate = new DateTime();
        listPhoneNumbers = [];
        email = "";
        foreach (var field in sheet.SheetFields)
        {
            //Loop through each field.
            switch (field.FieldName.ToLower())
            {
                case "lname":
                case "lastname":
                    lName = field.FieldValue;
                    break;
                case "fname":
                case "firstname":
                    fName = field.FieldValue;
                    break;
                case "bdate":
                case "birthdate":
                    birthdate = ParseDateWebForms(field.FieldValue, webFormPrefCulture);
                    break;
                case "hmphone":
                case "wkphone":
                case "wirelessphone":
                    if (field.FieldValue != "")
                    {
                        listPhoneNumbers.Add(field.FieldValue);
                    }

                    break;
                case "email":
                    email = field.FieldValue;
                    break;
            }
        }
    }

    public static DateTime ParseDateWebForms(string date, string webFormPrefCulture = null)
    {
        var dateTimeFormat = "M/d/yyyy"; //Default to en-US format just in case we don't currently support the culture passed in.
        if (webFormPrefCulture.IsNullOrEmpty())
        {
            webFormPrefCulture = WebForms_Preferences.TryGetPreference(out var webFormPref) ? webFormPref.CultureName : PrefC.GetString(PrefName.LanguageAndRegion);
        }

        var delimiterSupported = "/";
        switch (webFormPrefCulture)
        {
            case "ar-JO":
            case "en-CA":
            case "en-GB":
            case "en-ES":
            case "en-MX":
            case "en-PR":
            case "nl-NL":
                dateTimeFormat = "dd/MM/yyyy";
                break;
            case "da-DK":
            case "en-IN":
                dateTimeFormat = "dd-MM-yyyy";
                delimiterSupported = "-";
                break;
            case "en-AU":
            case "en-NZ":
                dateTimeFormat = "d/MM/yyyy";
                break;
            case "mn-MN":
                dateTimeFormat = "yy.MM.dd";
                delimiterSupported = ".";
                break;
            case "zh-CN":
                dateTimeFormat = "yyyy/M/d";
                break;
        }

        //Ensure any characters in between digits are the correct delimiter.
        var dateScrubbed = string.Join(delimiterSupported, Regex.Split(date, "[^\\d]+").Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!DateTime.TryParseExact(dateScrubbed, dateTimeFormat, new CultureInfo(webFormPrefCulture), DateTimeStyles.None, out var retVal))
        {
            retVal = DateTime.MinValue;
        }

        return retVal;
    }

    public static List<long> FindSheetsForPat(WebForms_Sheet sheetToMatch, List<WebForms_Sheet> listSheets, string webFormPrefCulture)
    {
        ParseWebFormSheet(sheetToMatch, webFormPrefCulture, out var lName, out var fName, out var birthdate, out var listPhoneNumbers, out var email);
        var listSheetIdMatch = new List<long>();
        foreach (var sheet in listSheets)
        {
            ParseWebFormSheet(sheet, webFormPrefCulture, out var lNameSheet, out var fNameSheet, out var birthdateSheet, out var listPhoneNumbersSheet, out var emailSheet);
            if (lName == lNameSheet && fName == fNameSheet && birthdate == birthdateSheet && email == emailSheet //All phone numbers must match in both.
                && !listPhoneNumbers.Except(listPhoneNumbersSheet).Any() && !listPhoneNumbersSheet.Except(listPhoneNumbers).Any())
            {
                listSheetIdMatch.Add(sheet.SheetID);
            }
        }

        return listSheetIdMatch;
    }
}