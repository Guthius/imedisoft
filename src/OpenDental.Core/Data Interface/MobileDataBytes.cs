using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using PdfSharp.Pdf;

namespace OpenDentBusiness;

public class MobileDataBytes
{
    private static readonly Random Random = new();

    public static bool IsValidUnlockCode(string rawUnlockCode)
    {
        return TryGetForUnlockCode(rawUnlockCode) is not null;
    }

    public static MobileDataByte TryGetForUnlockCode(string rawUnlockCode)
    {
        return string.IsNullOrEmpty(rawUnlockCode) ? null : MobileDataByteCrud.SelectOne($"SELECT * FROM mobiledatabyte WHERE RawBase64Code='{Convert.ToBase64String(Encoding.UTF8.GetBytes(rawUnlockCode))}'");
    }

    public static MobileDataByte GetOne(long mobileDataByteNum)
    {
        return MobileDataByteCrud.SelectOne(mobileDataByteNum);
    }

    public static long Insert(MobileDataByte mobileDataByte)
    {
        return MobileDataByteCrud.Insert(mobileDataByte);
    }

    public static void Delete(long mobileDataByteNum)
    {
        MobileDataByteCrud.Delete(mobileDataByteNum);
    }

    public static long InsertPDF(PdfDocument pdfDocument, long patNum, string unlockCode, eActionType eActionType, List<string> listTagVals = null)
    {
        long mobileDataByteNum = -1;
        using var memoryStream = new MemoryStream();
        pdfDocument.Save(memoryStream);
        var byteBase64 = Convert.ToBase64String(memoryStream.ToArray());
        var tagData = "";
        if (!listTagVals.IsNullOrEmpty()) tagData = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join("###", listTagVals)));
        var mobileDataByte = new MobileDataByte();
        mobileDataByte.PatNum = patNum;
        mobileDataByte.RawBase64Data = byteBase64;
        mobileDataByte.RawBase64Code = unlockCode.IsNullOrEmpty() ? "" : Convert.ToBase64String(Encoding.UTF8.GetBytes(unlockCode));
        mobileDataByte.RawBase64Tag = tagData;
        mobileDataByte.ActionType = eActionType;
        mobileDataByteNum = Insert(mobileDataByte);
        return mobileDataByteNum;
    }

    public static MobileDataByte InsertTreatPlanPDF(PdfDocument pdfDocument, TreatPlan treatPlan, bool hasPracticeSig, string unlockCode)
    {
        MobileDataByte mobileDataByte = null;
        if (!MobileAppDevices.IsClinicSignedUpForEClipboard(Clinics.ClinicNum)) throw new Exception("This practice or clinic is not signed up for eClipboard.\r\nGo to eServices | Signup Portal to sign up.");
        //If there is no heading present, than UTF8.GetBytes ignores empty strings, which means it doesn't put an empty header into the list of
        //tags that gets sent over to eClipboard.
        var treatPlanHeading = treatPlan.Heading.IsNullOrEmpty() ? "(No heading)" : treatPlan.Heading;
        var listTagValues = new List<string>
        {
            treatPlanHeading, treatPlan.TreatPlanNum.ToString(), hasPracticeSig.ToString(),
            treatPlan.DateTP.Ticks.ToString()
        };
        long mobileDataByteNum = -1;
        mobileDataByteNum = InsertPDF(pdfDocument, treatPlan.PatNum, unlockCode, eActionType.TreatmentPlan, listTagValues);
        if (mobileDataByteNum > -1) mobileDataByte = GetOne(mobileDataByteNum);
        return mobileDataByte;
    }

    public static MobileDataByte InsertPatientCheckin(Patient patient, string unlockCode)
    {
        var mobileDataByte = new MobileDataByte
        {
            PatNum = patient.PatNum,
            RawBase64Data = patient.PatNum.ToString(),
            RawBase64Code = unlockCode.IsNullOrEmpty() ? "" : Convert.ToBase64String(Encoding.UTF8.GetBytes(unlockCode)),
            RawBase64Tag = Convert.ToBase64String(Encoding.UTF8.GetBytes(Security.CurUser.UserNum.ToString())),
            ActionType = eActionType.Checkin
        };

        mobileDataByte.MobileDataByteNum = Insert(mobileDataByte);

        return mobileDataByte;
    }

    public static string GenerateUnlockCode()
    {
        const string input = "0123456789";

        var unlockCode = Generate();
        var tries = 0;

        while (true)
        {
            if (!IsValidUnlockCode(unlockCode))
            {
                break;
            }

            if (++tries > 100)
            {
                return null;
            }

            unlockCode = Generate();
        }

        return unlockCode;

        string Generate()
        {
            return new string(Enumerable.Range(0, 6).Select(_ => input[Random.Next(0, input.Length)]).ToArray());
        }
    }
}