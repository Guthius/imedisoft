using CodeBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

        if (listClinicNums == null)
        {
            listClinicNums = new List<long>();
        }

        List<PayloadItem> listPayloadItems = new List<PayloadItem>()
        {
            new PayloadItem(regKey, "RegKey"),
            new PayloadItem(listClinicNums, "ListClinicNums"),
        };
        string payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        //Gets all pending SheetIDs for the registration key and clinics.
        string response = SheetsSynchProxy.GetWebServiceInstance().GetWebFormSheetIDs(payload);
        return WebSerializer.DeserializeTag<List<long>>(response, "Success");
    }

    public static List<WebForms_Sheet> GetSheets(string regKey = null, List<long> listSheetIDs = null, List<long> listClinicNums = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        if (listSheetIDs == null)
        {
            listSheetIDs = new List<long>();
        }

        if (listClinicNums == null)
        {
            listClinicNums = new List<long>();
        }

        List<PayloadItem> listPayloadItems = new List<PayloadItem>()
        {
            new PayloadItem(regKey, "RegKey"),
            new PayloadItem(listSheetIDs, "ListSheetIDs"),
            new PayloadItem(listClinicNums, "ListClinicNums"),
        };
        string payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        //Get pending sheets from HQ.
        string resultXml = SheetsSynchProxy.GetWebServiceInstance().GetWebFormSheets(payload);
        List<WebForms_Sheet> listWebForms_Sheets = WebSerializer.DeserializeTag<List<WebForms_Sheet>>(resultXml, "Success");
        for (int i = 0; i < listWebForms_Sheets.Count; i++)
        {
            EServiceLog eServiceLog = EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFDownloadedForm, 0, listWebForms_Sheets[i].ClinicNum, listWebForms_Sheets[i].SheetID);
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
            List<PayloadItem> listPayloadItems = new List<PayloadItem>
            {
                new PayloadItem(regKey, "RegKey"),
                new PayloadItem(listWebForms_Sheets.Select(x => x.SheetID).ToList(), "SheetNumsForDeletion")
            };
            string payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
            string result = SheetsSynchProxy.GetWebServiceInstance().DeleteSheetData(payload);
            PayloadHelper.CheckForError(result);
            for (int i = 0; i < listWebForms_Sheets.Count; i++)
            {
                EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFDeletedForm, FKey: listWebForms_Sheets[i].SheetID, logGuid: listWebForms_Sheets[i].EServiceLogGuid);
            }

            return;
        }
        catch (Exception ex)
        {
            string log = Lans.g("FormWebForms", "There was a problem telling HQ that the web forms were retrieved:") + $" '{ex.Message}'";
            log += "\r\n" + "  ^" + Lans.g("FormWebForms", "The following web forms will be downloaded again the next time forms are retrieved.");
            log += "\r\n" + "  ^" + Lans.g("FormWebForms", "SheetIDs:") + " " + string.Join(", ", listWebForms_Sheets.Select(x => x.SheetID));
            EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFError, note: log);
            return;
        }
    }

    public static void ParseWebFormSheet(WebForms_Sheet sheet, string webFormPrefCulture, out string lName, out string fName, out DateTime birthdate, out List<string> listPhoneNumbers, out string email)
    {
        lName = "";
        fName = "";
        birthdate = new DateTime();
        listPhoneNumbers = new List<string>();
        email = "";
        foreach (WebForms_SheetField field in sheet.SheetFields)
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
        string dateTimeFormat = "M/d/yyyy"; //Default to en-US format just in case we don't currently support the culture passed in.
        if (webFormPrefCulture.IsNullOrEmpty())
        {
            if (WebForms_Preferences.TryGetPreference(out WebForms_Preference webFormPref))
            {
                webFormPrefCulture = webFormPref.CultureName;
            }
            else
            {
                webFormPrefCulture = PrefC.GetString(PrefName.LanguageAndRegion);
            }
        }

        string delimiterSupported = "/";
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

        DateTime retVal;
        //Ensure any characters in between digits are the correct delimiter.
        string dateScrubbed = string.Join(delimiterSupported, Regex.Split(date, "[^\\d]+").Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!DateTime.TryParseExact(dateScrubbed, dateTimeFormat, new CultureInfo(webFormPrefCulture), DateTimeStyles.None, out retVal))
        {
            retVal = DateTime.MinValue;
        }

        return retVal;
    }

    public static List<long> FindSheetsForPat(WebForms_Sheet sheetToMatch, List<WebForms_Sheet> listSheets, string webFormPrefCulture)
    {
        string lName;
        string fName;
        DateTime birthdate;
        List<string> listPhoneNumbers;
        string email;
        ParseWebFormSheet(sheetToMatch, webFormPrefCulture, out lName, out fName, out birthdate, out listPhoneNumbers, out email);
        List<long> listSheetIdMatch = new List<long>();
        foreach (WebForms_Sheet sheet in listSheets)
        {
            string lNameSheet = "";
            string fNameSheet = "";
            DateTime birthdateSheet = new DateTime();
            List<string> listPhoneNumbersSheet = new List<string>();
            string emailSheet = "";
            ParseWebFormSheet(sheet, webFormPrefCulture, out lNameSheet, out fNameSheet, out birthdateSheet, out listPhoneNumbersSheet, out emailSheet);
            if (lName == lNameSheet && fName == fNameSheet && birthdate == birthdateSheet && email == emailSheet //All phone numbers must match in both.
                && listPhoneNumbers.Except(listPhoneNumbersSheet).Count() == 0 && listPhoneNumbersSheet.Except(listPhoneNumbers).Count() == 0)
            {
                listSheetIdMatch.Add(sheet.SheetID);
            }
        }

        return listSheetIdMatch;
    }
}