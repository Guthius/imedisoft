using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;
using OpenDentBusiness.SheetFramework;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace OpenDentBusiness;


public class PerioExams
{
    ///<summary>Most recent date last.  All exams loaded, even if not displayed.</summary>
    public static List<PerioExam> Refresh(long patNum)
    {
        var command =
            "SELECT * from perioexam"
            + " WHERE PatNum = " + SOut.Long(patNum)
            + " ORDER BY perioexam.ExamDate";
        return PerioExamCrud.SelectMany(command);
    }

    public static bool HasPerio(long patNum)
    {
        var command =
            "SELECT COUNT(perioExamNum) FROM perioexam"
            + " WHERE PatNum = " + SOut.Long(patNum);
        return Db.GetLong(command) > 0;
    }

    /// <summary>
    ///     Gets a list of PerioExams from the DB for the API based on PatNum and/or ExamDate.
    ///     Results ordered by PK.
    /// </summary>
    public static List<PerioExam> GetPerioExamsForApi(long patNum, DateTime examDate)
    {
        var command = "SELECT * FROM perioexam WHERE ExamDate >= " + SOut.Date(examDate) + " ";
        if (patNum != 0) command += " AND PatNum = " + SOut.Long(patNum) + " ";
        command += " ORDER BY perioexam.PerioExamNum";
        return PerioExamCrud.SelectMany(command);
    }

    
    public static void Update(PerioExam Cur)
    {
        PerioExamCrud.Update(Cur);
    }

    
    public static bool Update(PerioExam perioExam, PerioExam perioExamOld)
    {
        return PerioExamCrud.Update(perioExam, perioExamOld);
    }

    
    public static long Insert(PerioExam Cur)
    {
        return PerioExamCrud.Insert(Cur);
    }

    /// <summary>
    ///     Creates a new perio exam for the given patient. Returns that perio exam. Handles setting default skipped
    ///     teeth/implants. Does not create a security log entry.
    /// </summary>
    public static PerioExam CreateNewExam(Patient pat)
    {
        var newExam = new PerioExam
        {
            PatNum = pat.PatNum,
            ExamDate = DateTime.Today,
            ProvNum = pat.PriProv,
            DateTMeasureEdit = MiscData.GetNowDateTime()
        };
        Insert(newExam);
        PerioMeasures.SetSkipped(newExam.PerioExamNum, GetSkippedTeethForExam(pat, newExam));
        return newExam;
    }

    ///<summary>Returns the toothNums from 1-32 to skip for the given patient.</summary>
    private static List<int> GetSkippedTeethForExam(Patient pat, PerioExam examCur)
    {
        var listSkippedTeeth = new List<int>();
        var listOtherExamsForPat = Refresh(pat.PatNum)
            .Where(x => x.PerioExamNum != examCur.PerioExamNum)
            .OrderBy(x => x.ExamDate)
            .ToList();
        //If any other perio exams exist, we'll use the latest exam for the skipped tooth.
        if (!listOtherExamsForPat.IsNullOrEmpty())
        {
            listSkippedTeeth = PerioMeasures.GetSkipped(listOtherExamsForPat.Last().PerioExamNum);
        }
        //For patient's first perio chart, any teeth marked missing are automatically marked skipped.
        else if (PrefC.GetBool(PrefName.PerioSkipMissingTeeth))
        {
            //Procedures will only be queried for as needed.
            List<Procedure> listProcs = null;
            foreach (var missingTooth in ToothInitials.GetMissingOrHiddenTeeth(ToothInitials.GetPatientData(pat.PatNum)))
            {
                if (missingTooth.CompareTo("A") >= 0 && missingTooth.CompareTo("Z") <= 0)
                    //If is a letter (not a number)
                    //Skipped teeth are only recorded by tooth number within the perio exam.
                    continue;
                var toothNum = SIn.Int(missingTooth);
                //Check if this tooth has had an implant done AND the office has the preference to SHOW implants
                if (PrefC.GetBool(PrefName.PerioTreatImplantsAsNotMissing))
                {
                    if (listProcs == null) listProcs = Procedures.Refresh(pat.PatNum);
                    if (IsToothImplant(toothNum, listProcs))
                    {
                        //Remove the tooth from the list of skipped teeth if it exists.
                        listSkippedTeeth.RemoveAll(x => x == toothNum);
                        //We do not want to add it back to the list below.
                        continue;
                    }
                }

                //This tooth is missing and we know it is not an implant OR the office has the preference to ignore implants.
                //Simply add it to our list of skipped teeth.
                listSkippedTeeth.Add(toothNum);
            }
        }

        return listSkippedTeeth;
    }

    ///<summary>Returns true if the toothNum passed in has ever had an implant before. Based on the given patient procedures.</summary>
    private static bool IsToothImplant(int toothNum, List<Procedure> listProcsForPatient)
    {
        return listProcsForPatient
            .FindAll(x => x.ToothNum == toothNum.ToString() && x.ProcStatus.In(ProcStat.C, ProcStat.EC, ProcStat.EO))
            //ProcedureCodes are cached.
            .Any(x => ProcedureCodes.GetProcCode(x.CodeNum).PaintType == ToothPaintingType.Implant);
    }

    
    public static void Delete(PerioExam Cur)
    {
        var command = "DELETE from perioexam WHERE PerioExamNum = '" + Cur.PerioExamNum + "'";
        Db.NonQ(command);
        command = "DELETE from periomeasure WHERE PerioExamNum = '" + Cur.PerioExamNum + "'";
        Db.NonQ(command);
    }

    ///<summary>Used by ContrPerio to get a perio exam.</summary>
    public static PerioExam GetOnePerioExam(long perioExamNum)
    {
        return PerioExamCrud.SelectOne(perioExamNum);
    }

    #region ODXam Methods

    /// <summary>
    ///     Do not use this method in OpenDental.exe. This method is to be used in ODXam only. Creates a PDF of the Perio Chart
    ///     to
    ///     be used in eClipboard.
    /// </summary>
    public static PdfDocument CreatePerioPDF(Patient pat, PerioExam perioExam, List<PerioMeasure> listPerioMeasures = null)
    {
        var marginBounds = new Rectangle();
        //The GraphicsHelper is used to specifically convert pixels to the correct points that PdfSharp uses, so it will be used in several places
        //The numbers used for width, height, etc, were taken from our printing logic
        marginBounds.X = (int) GraphicsHelper.PixelsToPoints(40);
        marginBounds.Y = (int) GraphicsHelper.PixelsToPoints(60);
        marginBounds.Width = (int) GraphicsHelper.PixelsToPoints(750);
        marginBounds.Height = (int) GraphicsHelper.PixelsToPoints(1000);
        var document = new PdfDocument();
        var page = document.AddPage();
        var gx = XGraphics.FromPdfPage(page);
        gx.SmoothingMode = XSmoothingMode.HighQuality;
        var clinicName = "";
        if (pat.ClinicNum != 0)
        {
            var clinic = Clinics.GetClinic(pat.ClinicNum);
            clinicName = clinic.Description;
        }
        else
        {
            clinicName = PrefC.GetString(PrefName.PracticeTitle);
        }

        //We don't get to make use of our printing/sheet drawing logic here, so we have to manually add the margin bounds
        var y = GraphicsHelper.PixelsToPoints(20) + marginBounds.Y;
        //This code mostly mimics code from FormPerioGraphical. The only realy difference is manually adding the margins and we use PdfSharp's
        //X objects instead (XSize==Size, XFont==Font,XGraphics==Graphics, etc). 
        XSize sizeStr;
        var font = new XFont("Arial", 15);
        var titleStr = "Periodontal Examination";
        sizeStr = gx.MeasureString(titleStr, font);
        gx.DrawString(titleStr, font, Brushes.Black, new PointF(marginBounds.X + marginBounds.Width / 2f - (float) sizeStr.Width / 2f, y));
        y += (float) sizeStr.Height;
        //Clinic Name
        font = new XFont("Arial", 11);
        sizeStr = gx.MeasureString(clinicName, font);
        gx.DrawString(clinicName, font, Brushes.Black, new PointF(marginBounds.X + marginBounds.Width / 2f - (float) sizeStr.Width / 2f, y));
        y += (float) sizeStr.Height;
        //PatientName
        var patNameStr = pat.GetNameFLFormal();
        sizeStr = gx.MeasureString(patNameStr, font);
        gx.DrawString(patNameStr, font, Brushes.Black, new PointF(marginBounds.X + marginBounds.Width / 2f - (float) sizeStr.Width / 2f, y));
        y += (float) sizeStr.Height;
        //We put the exam date instead of the current date because the exam date isn't anywhere else on the printout.
        var examDateStr = perioExam.ExamDate.ToShortDateString(); //Locale specific exam date.
        sizeStr = gx.MeasureString(examDateStr, font);
        gx.DrawString(examDateStr, font, Brushes.Black, new PointF(marginBounds.X + marginBounds.Width / 2f - (float) sizeStr.Width / 2f, y));
        y += (float) sizeStr.Height;
        var sizeAvail = new SizeF(marginBounds.Width, marginBounds.Height - y);
        var bitmapPerioChart = (Bitmap) ToothChartHelper.GetImage(pat.PatNum, false, isForPerio: true, perioExam: perioExam);
        var xI = XImage.FromGdiPlusImage(bitmapPerioChart);
        var pdfWidth = GraphicsHelper.PixelsToPoints(xI.PixelWidth);
        var pdfHeight = GraphicsHelper.PixelsToPoints(xI.PixelHeight);
        var ratioBitmapToOutput = pdfHeight / sizeAvail.Height;
        if (pdfWidth / pdfHeight //ratio WtoH of bitmap
            > //if bitmap is proportionally wider than page space
            sizeAvail.Width / sizeAvail.Height) //ratio WtoH of page
            ratioBitmapToOutput = pdfWidth / sizeAvail.Width;
        var sizeBitmapOut = new SizeF(pdfWidth / ratioBitmapToOutput, pdfHeight / ratioBitmapToOutput);
        gx.DrawImage(xI,
            marginBounds.X + (sizeAvail.Width / 2f - sizeBitmapOut.Width / 2f),
            y,
            sizeBitmapOut.Width,
            sizeBitmapOut.Height);
        //Dispose of PdfSharp objects
        gx.Dispose();
        gx = null;
        xI.Dispose();
        xI = null;
        if (bitmapPerioChart != null)
        {
            bitmapPerioChart.Dispose();
            bitmapPerioChart = null;
        }

        GC.Collect(); //We are done creating the pdf so we can forcefully clean up all the objects and controls that were used.
        return document;
    }

    #endregion ODXam Methods
}