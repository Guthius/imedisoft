using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class MedLabResults
{
    public static List<MedLabResult> GetForLab(long medLabNum)
    {
        return MedLabResultCrud.SelectMany("SELECT * FROM medlabresult WHERE MedLabNum = " + medLabNum + " ORDER BY ObsID, ObsIDSub, ResultStatus, DateTimeObs DESC");
    }

    public static List<MedLabResult> GetResultHist(MedLabResult medLabResult, long patNum, string specimenId, string specimenIdFiller)
    {
        var medLabResults = new List<MedLabResult>();
        if (medLabResult is null)
        {
            return medLabResults;
        }

        return MedLabResultCrud.SelectMany(
            "SELECT medlabresult.* FROM medlabresult " +
            "INNER JOIN medlab ON medlab.MedLabNum = medlabresult.MedLabNum " +
            "AND medlab.PatNum = " + patNum + " " +
            "AND medlab.SpecimenID = '" + SOut.String(specimenId) + "' " +
            "AND medlab.SpecimenIDFiller = '" + SOut.String(specimenIdFiller) + "' " +
            "WHERE medlabresult.ObsID = '" + SOut.String(medLabResult.ObsID) + "' " +
            "AND medlabresult.ObsIDSub = '" + SOut.String(medLabResult.ObsIDSub) + "' " +
            "ORDER BY medlabresult.ResultStatus, medlabresult.DateTimeObs DESC, medlabresult.MedLabResultNum DESC");
    }

    public static long Insert(MedLabResult medLabResult)
    {
        return MedLabResultCrud.Insert(medLabResult);
    }

    public static void Update(MedLabResult medLabResult)
    {
        MedLabResultCrud.Update(medLabResult);
    }

    public static void DeleteAllForMedLabs(List<long> medLabNums)
    {
        if (medLabNums == null || medLabNums.Count < 1)
        {
            return;
        }

        Db.NonQ("DELETE FROM medlabresult WHERE MedLabNum IN (" + string.Join(", ", medLabNums) + ")");
    }

    public static string GetAbnormalFlagDescript(AbnormalFlag abnormalFlag)
    {
        return abnormalFlag switch
        {
            AbnormalFlag._gt => "Panic High",
            AbnormalFlag._lt => "Panic Low",
            AbnormalFlag.A => "Abnormal",
            AbnormalFlag.AA => "Critical Abnormal",
            AbnormalFlag.H => "Above High Normal",
            AbnormalFlag.HH => "Alert High",
            AbnormalFlag.I => "Intermediate",
            AbnormalFlag.L => "Below Low Normal",
            AbnormalFlag.LL => "Alert Low",
            AbnormalFlag.NEG => "Negative",
            AbnormalFlag.POS => "Positive",
            AbnormalFlag.R => "Resistant",
            AbnormalFlag.S => "Susceptible",
            AbnormalFlag.None => "",
            _ => ""
        };
    }
}