using System;
using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness;


public class UpdateHistories
{
    
    public static long Insert(UpdateHistory updateHistory)
    {
        return UpdateHistoryCrud.Insert(updateHistory);
    }

    
    public static void Update(UpdateHistory updateHistory)
    {
        UpdateHistoryCrud.Update(updateHistory);
    }

    ///<summary>All updatehistory entries ordered by DateTimeUpdated.</summary>
    public static List<UpdateHistory> GetAll()
    {
        var command = "SELECT * FROM updatehistory ORDER BY DateTimeUpdated";
        return UpdateHistoryCrud.SelectMany(command);
    }

    ///<summary>Get the most recently inserted updatehistory entry. Ordered by DateTimeUpdated.</summary>
    public static UpdateHistory GetLastUpdateHistory()
    {
        var command = @"SELECT * 
				FROM updatehistory
				ORDER BY DateTimeUpdated DESC
				LIMIT 1";
        return UpdateHistoryCrud.SelectOne(command);
    }

    ///<summary>Gets the most recently inserted updatehistory entries. Ordered by DateTimeUpdated.</summary>
    public static List<UpdateHistory> GetPreviousUpdateHistories(int count)
    {
        var command = @"SELECT * 
				FROM updatehistory
				ORDER BY DateTimeUpdated DESC
				LIMIT " + SOut.Int(count);
        return UpdateHistoryCrud.SelectMany(command);
    }

    ///<summary>Returns the latest version information.</summary>
    public static UpdateHistory GetForVersion(string strVersion)
    {
        var command = "SELECT * FROM updatehistory WHERE ProgramVersion='" + SOut.String(strVersion) + "'";
        return UpdateHistoryCrud.SelectOne(command);
    }

    /// <summary>
    ///     Returns the earliest datetime that a version was reached. If that version has not been reached, returns the
    ///     MinDate.
    /// </summary>
    public static DateTime GetDateForVersion(Version version)
    {
        var listUpdateHistories = GetAll();
        for (var i = 0; i < listUpdateHistories.Count; i++)
        {
            var versionCompare = new Version();
            ODException.SwallowAnyException(() => { versionCompare = new Version(listUpdateHistories[i].ProgramVersion); }); //Just in case.
            if (versionCompare >= version) return listUpdateHistories[i].DateTimeUpdated;
        }

        //The earliest version was later than the version passed in.
        return new DateTime(1, 1, 1);
    }

    ///<summary>Get the most recently signed updatehistory entry. Ordered by DateTimeUpdated.</summary>
    public static UpdateHistory GetLastSignedUpdateHistory()
    {
        var command = @"SELECT * 
				FROM updatehistory
				WHERE Signature!=''
				ORDER BY DateTimeUpdated DESC
				LIMIT 1";
        return UpdateHistoryCrud.SelectOne(command);
    }

    /// <summary>
    ///     Determines if the License Agreement needs to be "signed". Returns true if no entries contain a signature
    ///     string, otherwise false.
    /// </summary>
    public static bool IsLicenseAgreementNeeded()
    {
        var command = @"SELECT COUNT(*) 
				FROM updatehistory
				WHERE Signature!=''";
        return Db.GetCount(command) == "0";
    }

    /// <summary>
    ///     Attempts to send the customer's License Agreement signature to BugsHQ database. Returns true if web call
    ///     successfully inserts a signature, otherwise false.
    /// </summary>
    public static bool SendSignatureToHQ(string regKey, string obfuscatedSignature)
    {
        var listPayloadItems = new List<PayloadItem>();
        listPayloadItems.Add(new PayloadItem(regKey, "RegistrationKey"));
        listPayloadItems.Add(new PayloadItem(obfuscatedSignature, "Signature"));
        var officeData = PayloadHelper.CreatePayload(listPayloadItems, eServiceCode.LicenseAgreementSig);
        string result;
        try
        {
            result = WebServiceMainHQProxy.GetWebServiceMainHQInstance().LicenseAgreementAccepted(officeData);
        }
        catch (Exception ex)
        {
            return false;
        }

        return WebSerializer.DeserializePrimitive<bool>(result);
    }
}