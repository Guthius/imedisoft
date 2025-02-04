using System;
using System.Collections.Generic;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness.AutoComm;

public sealed class ApptLite : AutoCommObj
{
    public DateTime AptDateTime;
    public DateTime DateTimeAskedToArrive;

    public ApptLite(Appointment appt, PatComm patComm)
    {
        AptDateTime = appt.AptDateTime;
        DateTimeAskedToArrive = appt.DateTimeAskedToArrive;
        
        if (DateTimeAskedToArrive.Year < 1880)
        {
            DateTimeAskedToArrive = AptDateTime;
        }

        PatNum = appt.PatNum;
        ProvNum = appt.ProvNum;
        
        var clinic = appt.ClinicNum == 0 ? Clinics.GetPracticeAsClinicZero() : Clinics.GetClinic(appt.ClinicNum);
        
        Clinics.GetOfficeAddress(clinic);
        
        SetPatientContact(patComm, new Dictionary<long, PatComm>());
    }
}