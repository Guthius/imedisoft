using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public class PatientDashboardDataEventArgs : IDisposable
{
    public Family Fam;
    public Patient Pat;
    public PatientNote PatNote;
    public List<InsSub> ListInsSubs;
    public List<InsPlan> ListInsPlans;
    public List<PatPlan> ListPatPlans;
    public List<Benefit> ListBenefits;
    public DiscountPlan DiscountPlan;
    public DiscountPlanSub DiscountPlanSub;
    public List<Claim> ListClaims;
    public List<ClaimProcHist> HistList;
    public List<Procedure> ListProcedures;
    public List<TreatPlan> ListTreatPlans;
    public List<Document> ListDocuments;
    public List<Appointment> ListAppts;
    public List<Appointment> ListPlannedAppts;
    public List<ToothInitial> ListToothInitials;
    public DataTable TableProgNotes;
    public Image ImageToothChart;
    public Bitmap BitmapImagesModule;
    public StaticTextData StaticTextData;

    public void Dispose()
    {
        ImageToothChart?.Dispose();
        ImageToothChart = null;
        BitmapImagesModule?.Dispose();
        BitmapImagesModule = null;
    }

    public List<Appointment> ExtractPlannedAppts(Patient pat, DataTable tablePlannedAppts)
    {
        var listPlannedAppts = new List<Appointment>();
        for (var i = 0; i < tablePlannedAppts.Rows.Count; i++)
        {
            var row = tablePlannedAppts.Rows[i];

            listPlannedAppts.Add(new Appointment
            {
                PatNum = pat.PatNum,
                AptNum = SIn.Long(row["AptNum"].ToString()),
                ItemOrderPlanned = SIn.Int(row["ItemOrder"].ToString())
            });
        }

        return listPlannedAppts.OrderBy(x => x.ItemOrderPlanned).ToList();
    }
}