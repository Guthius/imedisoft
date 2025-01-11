using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrMeasureEvents
{
    #region Insert

    
    public static void InsertMany(List<EhrMeasureEvent> listEhrMeasureEvents)
    {
        EhrMeasureEventCrud.InsertMany(listEhrMeasureEvents);
    }

    #endregion

    /// <summary>
    ///     Gets a list of MeasureEvents.  Primarily used in FormEhrMeasureEvents.  Pass in true to get all
    ///     EhrMeasureEvents for the date range.  Passing in true will ignore the specified measure event type.
    /// </summary>
    public static List<EhrMeasureEvent> GetAllByTypeFromDB(DateTime dateStart, DateTime dateEnd, EhrMeasureEventType measureEventType, bool isAll)
    {
        var command = "SELECT * FROM ehrmeasureevent "
                      + "WHERE DateTEvent >= " + SOut.DateT(dateStart) + " "
                      + "AND DateTEvent <= " + SOut.DateT(dateEnd) + " ";
        if (!isAll) command += "AND EventType = " + SOut.Int((int) measureEventType) + " ";
        command += "ORDER BY EventType,DateTEvent,PatNum";
        return EhrMeasureEventCrud.SelectMany(command);
    }

    ///<summary>Gets the MoreInfo column from the most recent event of the specified type. Returns blank if none exists.</summary>
    public static string GetLatestInfoByType(EhrMeasureEventType measureEventType)
    {
        var command = "SELECT * FROM ehrmeasureevent WHERE EventType=" + SOut.Int((int) measureEventType)
                                                                       + " ORDER BY DateTEvent DESC LIMIT 1";
        var measureEvent = EhrMeasureEventCrud.SelectOne(command);
        if (measureEvent == null) return "";

        return measureEvent.MoreInfo;
    }

    ///<summary>Ordered by dateT</summary>
    public static List<EhrMeasureEvent> Refresh(long patNum)
    {
        var command = "SELECT * FROM ehrmeasureevent WHERE PatNum = " + SOut.Long(patNum) + " "
                      + "ORDER BY DateTEvent";
        return EhrMeasureEventCrud.SelectMany(command);
    }

    ///<summary>Ordered by dateT</summary>
    public static List<EhrMeasureEvent> RefreshByType(long patNum, params EhrMeasureEventType[] ehrMeasureEventTypes)
    {
        var command = "SELECT * FROM ehrmeasureevent WHERE (";
        for (var i = 0; i < ehrMeasureEventTypes.Length; i++)
        {
            if (i > 0) command += "OR ";
            command += "EventType = " + SOut.Int((int) ehrMeasureEventTypes[i]) + " ";
        }

        command += ") AND PatNum = " + SOut.Long(patNum) + " "
                   + "ORDER BY DateTEvent";
        return EhrMeasureEventCrud.SelectMany(command);
    }

    public static List<EhrMeasureEvent> GetPatientData(long patNum)
    {
        return RefreshByType(patNum, EhrMeasureEventType.TobaccoUseAssessed);
    }

    ///<summary>Creates a measure event for the patient and event type passed in.  Used by eServices.</summary>
    public static long CreateEventForPat(long patNum, EhrMeasureEventType measureEventType)
    {
        var measureEvent = new EhrMeasureEvent();
        measureEvent.DateTEvent = DateTime.Now;
        measureEvent.EventType = measureEventType;
        measureEvent.PatNum = patNum;
        measureEvent.MoreInfo = "";
        return Insert(measureEvent);
    }

    
    public static long Insert(EhrMeasureEvent ehrMeasureEvent)
    {
        return EhrMeasureEventCrud.Insert(ehrMeasureEvent);
    }

    
    public static void Update(EhrMeasureEvent ehrMeasureEvent)
    {
        EhrMeasureEventCrud.Update(ehrMeasureEvent);
    }

    
    public static void Delete(long ehrMeasureEventNum)
    {
        var command = "DELETE FROM ehrmeasureevent WHERE EhrMeasureEventNum = " + SOut.Long(ehrMeasureEventNum);
        Db.NonQ(command);
    }

    
    public static List<EhrMeasureEvent> GetByType(List<EhrMeasureEvent> listMeasures, EhrMeasureEventType eventType)
    {
        var retVal = new List<EhrMeasureEvent>();
        for (var i = 0; i < listMeasures.Count; i++)
            if (listMeasures[i].EventType == eventType)
                retVal.Add(listMeasures[i]);

        return retVal;
    }

    /// <summary>
    ///     Gets codes (SNOMEDCT) from CodeValueResult for EhrMeasureEvents with DateTEvent within the last year for the given
    ///     EhrMeasureEventType.
    ///     Result list is grouped by code.
    /// </summary>
    public static List<string> GetListCodesUsedForType(EhrMeasureEventType eventType)
    {
        var command = "SELECT CodeValueResult FROM ehrmeasureevent "
                      + "WHERE EventType=" + SOut.Int((int) eventType) + " "
                      + "AND CodeValueResult!='' "
                      + "AND " + DbHelper.DtimeToDate("DateTEvent") + ">=" + SOut.Date(MiscData.GetNowDateTime().AddYears(-1)) + " "
                      + "GROUP BY CodeValueResult";
        return Db.GetListString(command);
    }

    /*

    ///<summary>Gets one EhrMeasureEvent from the db.</summary>
    public static EhrMeasureEvent GetOne(long ehrMeasureEventNum){

        return Crud.EhrMeasureEventCrud.SelectOne(ehrMeasureEventNum);
    }



    */
}