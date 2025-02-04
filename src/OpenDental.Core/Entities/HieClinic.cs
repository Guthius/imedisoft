using System;
using CodeBase;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class HieClinic : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long HieClinicNum;

    ///<summary>FK to clinic.ClincNum.</summary>
    public long ClinicNum;

    ///<summary>Enum:HieCarrierFlags AllPatient=0,Medicaid=1.  Indicates the supported carrier, bitwise.</summary>
    public HieCarrierFlags SupportedCarrierFlags;

    ///<summary>The path to export CCD. This field will not be blank when enabled.</summary>
    public string PathExportCCD;

    ///<summary>The time to export CCD.</summary>
    public TimeSpan TimeOfDayExportCCD;

    public bool IsEnabled;
    
    public bool IsTimeToProcess()
    {
        var timeStart = TimeOfDayExportCCD;
        //Time end will be time start plus 1 Hour
        var timeEnd = TimeOfDayExportCCDEnd;
        var timeNow = DateTime_.Now.TimeOfDay;
        if (timeStart <= timeEnd)
        {
            //start and end time are in the same day.
            return timeNow >= timeStart && timeNow <= timeEnd;
        }

        //Start and end times are on different days.
        return timeNow >= timeStart || timeNow <= timeEnd;
    }

    public TimeSpan TimeOfDayExportCCDEnd => TimeOfDayExportCCD + TimeSpan.FromHours(1);

    public HieClinic()
    {
    }

    public HieClinic(long clinicNum, TimeSpan timeOfDateExportCCD, bool isEnabled = true, HieCarrierFlags carrierFlags = HieCarrierFlags.AllCarriers, string pathExportCCD = "")
    {
        ClinicNum = clinicNum;
        TimeOfDayExportCCD = timeOfDateExportCCD;
        IsEnabled = isEnabled;
        SupportedCarrierFlags = carrierFlags;
        PathExportCCD = pathExportCCD;
    }
}

[Flags]
public enum HieCarrierFlags
{
    AllCarriers = 0,
    Medicaid = 1
}