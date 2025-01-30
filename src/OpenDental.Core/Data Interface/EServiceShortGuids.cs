using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EServiceShortGuids
{
    public static List<EServiceShortGuid> GenerateShortGuid(List<Appointment> listAppointments, eServiceCode eServiceCode, EServiceShortGuidKeyType eServiceShortGuidKeyType)
    {
        var listEServiceShortGuids = new List<EServiceShortGuid>();
        var listClinicNums = listAppointments.Select(x => x.ClinicNum).Distinct().ToList();

        foreach (var clinicNum in listClinicNums)
        {
            var listAppointmentsPerClinic = listAppointments.FindAll(x => x.ClinicNum == clinicNum);
            var listShortGuidResults = WebServiceMainHQProxy.GetShortGUIDs(listAppointmentsPerClinic.Count, listAppointmentsPerClinic.Count, clinicNum, eServiceCode);

            for (var j = 0; j < listAppointmentsPerClinic.Count; j++)
            {
                var appointment = listAppointmentsPerClinic[j];
                var shortGuidResult = listShortGuidResults[j];

                listEServiceShortGuids.Add(new EServiceShortGuid
                {
                    EServiceCode = eServiceCode,
                    ShortGuid = shortGuidResult.ShortGuid,
                    ShortURL = shortGuidResult.ShortURL,
                    DateTimeExpiration = appointment.AptDateTime.Date.AddDays(1),
                    FKey = appointment.AptNum,
                    FKeyType = eServiceShortGuidKeyType
                });
            }
        }

        InsertMany(listEServiceShortGuids);

        return listEServiceShortGuids;
    }

    public static void CreateAndInsertMsgToPayShortGuid(long patNum, string shortGuid)
    {
        Insert(new EServiceShortGuid
        {
            FKey = patNum,
            FKeyType = EServiceShortGuidKeyType.MsgToPayPatient,
            ShortGuid = shortGuid,
            EServiceCode = eServiceCode.IntegratedTexting,
            DateTimeExpiration = DateTime_.Now.AddDays(7)
        });
    }

    public static List<EServiceShortGuid> GetByFKey(EServiceShortGuidKeyType eServiceShortGuidKeyType, List<long> listFKeys, bool doIncludeExpired = false)
    {
        if (listFKeys.IsNullOrEmpty()) return [];

        //KeyType is EnumAsString
        var command = $"SELECT * FROM eserviceshortguid WHERE eserviceshortguid.FKeyType='{SOut.String(eServiceShortGuidKeyType.ToString())}' " +
                      $"AND eserviceshortguid.FKey IN ({string.Join(",", listFKeys)}) ";
        if (!doIncludeExpired) command += "AND eserviceshortguid.DateTimeExpiration > NOW()";
        return EServiceShortGuidCrud.SelectMany(command);
    }

    public static void Insert(EServiceShortGuid eServiceShortGuid)
    {
        EServiceShortGuidCrud.Insert(eServiceShortGuid);
    }

    public static void InsertMany(List<EServiceShortGuid> listEServiceShortGuids)
    {
        EServiceShortGuidCrud.InsertMany(listEServiceShortGuids);
    }
}