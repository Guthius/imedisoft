using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EServiceShortGuids
{
	/// <summary>
	///     Generates a ShortGuid via WebServiceHQ, inserts a corresponding entry into EServiceShortGuid, and returns the
	///     result.
	///     DateTimeExpiration is set to midnight the day after each appointment.
	/// </summary>
	public static List<EServiceShortGuid> GenerateShortGuid(List<Appointment> listAppointments, eServiceCode eServiceCode, EServiceShortGuidKeyType eServiceShortGuidKeyType)
    {
        var listEServiceShortGuids = new List<EServiceShortGuid>();
        //Group by Appointment.ClinicNum if clinics enabled, otherwise one big group using 0 as the key, which will be used as 
        //ShortGuidLookup.ClinicNum at HQ.
        var listClinicNums = listAppointments.Select(x => x.ClinicNum).Distinct().ToList();
        for (var i = 0; i < listClinicNums.Count(); i++)
        {
            var listAppointmentsPerClinic = listAppointments.FindAll(x => x.ClinicNum == listClinicNums[i]);
            var listShortGuidResults = WebServiceMainHQProxy.GetShortGUIDs(
                listAppointmentsPerClinic.Count, listAppointmentsPerClinic.Count, listClinicNums[i], eServiceCode);
            for (var j = 0; j < listAppointmentsPerClinic.Count; j++)
            {
                var appointment = listAppointmentsPerClinic[j];
                var shortGuidResult = listShortGuidResults[j];
                var eServiceShortGuid = new EServiceShortGuid();
                eServiceShortGuid.EServiceCode = eServiceCode;
                eServiceShortGuid.ShortGuid = shortGuidResult.ShortGuid;
                eServiceShortGuid.ShortURL = shortGuidResult.ShortURL;
                eServiceShortGuid.DateTimeExpiration = appointment.AptDateTime.Date.AddDays(1);
                eServiceShortGuid.FKey = appointment.AptNum;
                eServiceShortGuid.FKeyType = eServiceShortGuidKeyType;
                listEServiceShortGuids.Add(eServiceShortGuid);
            }
        }

        InsertMany(listEServiceShortGuids);
        return listEServiceShortGuids;
    }

	/// <summary>
	///     Creates and Inserts an EServiceShortGuid using the passed in PatNum and ShortGuid.
	///     FKeyType=EServiceShortGuidKeyType.MsgToPayPatient, EServiceCode=eServiceCode.IntegratedTexting,
	///     DateTimeExpiration=Now + 7 days.
	/// </summary>
	public static void CreateAndInsertMsgToPayShortGuid(long patNum, string shortGuid)
    {
        var eServiceShortGuid = new EServiceShortGuid();
        eServiceShortGuid.FKey = patNum;
        eServiceShortGuid.FKeyType = EServiceShortGuidKeyType.MsgToPayPatient;
        eServiceShortGuid.ShortGuid = shortGuid;
        eServiceShortGuid.EServiceCode = eServiceCode.IntegratedTexting;
        eServiceShortGuid.DateTimeExpiration = DateTime_.Now.AddDays(7);
        Insert(eServiceShortGuid);
    }

    #region Get Methods

    ///<summary>Gets many EServiceShortGuid from the db.</summary>
    public static List<EServiceShortGuid> GetByShortGuid(string shortGuid, bool doIncludeExpired = false)
    {
        var command = $"SELECT * FROM eserviceshortguid WHERE eserviceshortguid.ShortGuid='{SOut.String(shortGuid)}' ";
        if (!doIncludeExpired) command += "AND eserviceshortguid.DateTimeExpiration > " + DbHelper.Now();
        return EServiceShortGuidCrud.SelectMany(command);
    }

    ///<summary>Gets many EServiceShortGuid from the db.</summary>
    public static List<EServiceShortGuid> GetByFKey(EServiceShortGuidKeyType eServiceShortGuidKeyType, List<long> listFKeys, bool doIncludeExpired = false)
    {
        if (listFKeys.IsNullOrEmpty()) return new List<EServiceShortGuid>();

        //KeyType is EnumAsString
        var command = $"SELECT * FROM eserviceshortguid WHERE eserviceshortguid.FKeyType='{SOut.String(eServiceShortGuidKeyType.ToString())}' " +
                      $"AND eserviceshortguid.FKey IN ({string.Join(",", listFKeys.Select(x => SOut.Long(x)))}) ";
        if (!doIncludeExpired) command += "AND eserviceshortguid.DateTimeExpiration > " + DbHelper.Now();
        return EServiceShortGuidCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Modification Methods

    ///<summary>Inserts one EServiceShortGuid into the db.</summary>
    public static long Insert(EServiceShortGuid eServiceShortGuid)
    {
        return EServiceShortGuidCrud.Insert(eServiceShortGuid);
    }

    ///<summary>Inserts many EServiceShortGuid into the db.</summary>
    public static void InsertMany(List<EServiceShortGuid> listEServiceShortGuids)
    {
        EServiceShortGuidCrud.InsertMany(listEServiceShortGuids);
    }

    #endregion Modification Methods
}