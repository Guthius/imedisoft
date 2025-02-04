using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

///<summary>This is used in both the dashboard and in ControlTreat.  We paint instead of using controls because it's snappier, but mostly because we couldn't do it any other way. Dynamic addition of controls does not play well with LayoutManager and threads.</summary>
public partial class DashFamilyInsurance : Control, IDashWidgetField
{
    private List<InsPlan> _listInsPlans;
    private List<InsSub> _listInsSubs;
    private List<PatPlan> _listPatPlans;
    private List<Benefit> _listBenefits;
    private Patient _pat;
    private string _strFamPriMax = "";
    private string _strFamPriDed = "";
    private string _strFamSecMax = "";
    private string _strFamSecDed = "";

    public DashFamilyInsurance()
    {
        InitializeComponent();
        DoubleBuffered = true;
    }

    protected override Size DefaultSize
    {
        get { return new Size(193, 80); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics; //don't dispose
        using var brushBack = new SolidBrush(ColorOD.Background);
        g.FillRectangle(brushBack, ClientRectangle);
        using var penOutline = new Pen(ColorOD.Outline);
        var rectangle = new Rectangle(0, 0, Width - 1, Height - 1);
        GraphicsHelper.DrawRoundedRectangle(g, Pens.Silver, rectangle, 4);
        //We must ignore the Font and do our own.
        var scaledFontSize = 8.25f;
        //scaledFontSize*=0.97f;//layout manager uses .92
        //using Font fontTitle=new Font(FontFamily.GenericSansSerif,scaledFontSize);
        using var font = new Font(FontFamily.GenericSansSerif, scaledFontSize);
        g.DrawString(Lan.g(this, "Family Insurance"), font, Brushes.Black, 4, 1);
        StringFormat stringFormat; //disposed as used and at bottom
        stringFormat = new StringFormat();
        //top labels-------------------------------------------------------------------------
        stringFormat.Alignment = StringAlignment.Center; //horiz
        stringFormat.LineAlignment = StringAlignment.Far;
        rectangle = new Rectangle(74, 16, 60, 15);
        g.DrawString(Lan.g(this, "Primary"), font, Brushes.Black, rectangle, stringFormat);
        rectangle = new Rectangle(131, 16, 60, 15);
        g.DrawString(Lan.g(this, "Secondary"), font, Brushes.Black, rectangle, stringFormat);
        //row 1-----------------------------------------------------------------------
        stringFormat?.Dispose();
        stringFormat = new StringFormat();
        stringFormat.Alignment = StringAlignment.Far;
        stringFormat.LineAlignment = StringAlignment.Center;
        rectangle = new Rectangle(4, 37, 66, 15);
        g.DrawString(Lan.g(this, "AnnualMax"), font, Brushes.Black, rectangle, stringFormat);
        rectangle = new Rectangle(73, 36, 58, 18);
        g.FillRectangle(Brushes.White, rectangle);
        g.DrawRectangle(penOutline, rectangle);
        g.DrawString(_strFamPriMax, font, Brushes.Black, rectangle, stringFormat);
        rectangle = new Rectangle(131, 36, 58, 18);
        g.FillRectangle(Brushes.White, rectangle);
        g.DrawRectangle(penOutline, rectangle);
        g.DrawString(_strFamSecMax, font, Brushes.Black, rectangle, stringFormat);
        //row 2-------------------------------------------------------------------------
        rectangle = new Rectangle(4, 57, 66, 15);
        g.DrawString(Lan.g(this, "Fam Ded"), font, Brushes.Black, rectangle, stringFormat);
        rectangle = new Rectangle(73, 56, 58, 18);
        g.FillRectangle(Brushes.White, rectangle);
        g.DrawRectangle(penOutline, rectangle);
        g.DrawString(_strFamPriDed, font, Brushes.Black, rectangle, stringFormat);
        rectangle = new Rectangle(131, 56, 58, 18);
        g.FillRectangle(Brushes.White, rectangle);
        g.DrawRectangle(penOutline, rectangle);
        g.DrawString(_strFamSecDed, font, Brushes.Black, rectangle, stringFormat);
        stringFormat?.Dispose();
    }

    public string GetFamPriMax()
    {
        return _strFamPriMax;
    }

    public string GetFamPriDed()
    {
        return _strFamPriDed;
    }

    public string GetFamSecMax()
    {
        return _strFamSecMax;
    }

    public string GetFamSecDed()
    {
        return _strFamSecDed;
    }

    public void RefreshInsurance(Patient pat, List<InsPlan> listInsPlans, List<InsSub> listInsSubs, List<PatPlan> listPatPlans, List<Benefit> listBenefits)
    {
        _strFamPriMax = "";
        _strFamPriDed = "";
        _strFamSecMax = "";
        _strFamSecDed = "";
        if (pat == null)
        {
            return;
        }

        double maxFam = 0;
        double maxInd = 0;
        double dedFam = 0;
        InsPlan PlanCur; //=new InsPlan();
        InsSub SubCur;
        if (listPatPlans.Count > 0)
        {
            SubCur = InsSubs.GetSub(listPatPlans[0].InsSubNum, listInsSubs);
            PlanCur = InsPlans.GetPlan(SubCur.PlanNum, listInsPlans);
            maxFam = Benefits.GetAnnualMaxDisplay(listBenefits, PlanCur.PlanNum, listPatPlans[0].PatPlanNum, true);
            maxInd = Benefits.GetAnnualMaxDisplay(listBenefits, PlanCur.PlanNum, listPatPlans[0].PatPlanNum, false);
            if (maxFam == -1)
            {
                _strFamPriMax = "";
            }
            else
            {
                _strFamPriMax = maxFam.ToString("F");
            }

            //deductible:
            dedFam = Benefits.GetDeductGeneralDisplay(listBenefits, PlanCur.PlanNum, listPatPlans[0].PatPlanNum, BenefitCoverageLevel.Family);
            if (dedFam != -1)
            {
                _strFamPriDed = dedFam.ToString("F");
            }
        }

        if (listPatPlans.Count > 1)
        {
            SubCur = InsSubs.GetSub(listPatPlans[1].InsSubNum, listInsSubs);
            PlanCur = InsPlans.GetPlan(SubCur.PlanNum, listInsPlans);
            //max=Benefits.GetAnnualMaxDisplay(listBenefits,PlanCur.PlanNum,listPatPlans[1].PatPlanNum);
            maxFam = Benefits.GetAnnualMaxDisplay(listBenefits, PlanCur.PlanNum, listPatPlans[1].PatPlanNum, true);
            maxInd = Benefits.GetAnnualMaxDisplay(listBenefits, PlanCur.PlanNum, listPatPlans[1].PatPlanNum, false);
            if (maxFam == -1)
            {
                _strFamSecMax = "";
            }
            else
            {
                _strFamSecMax = maxFam.ToString("F");
            }

            //deductible:
            dedFam = Benefits.GetDeductGeneralDisplay(listBenefits, PlanCur.PlanNum, listPatPlans[1].PatPlanNum, BenefitCoverageLevel.Family);
            if (dedFam != -1)
            {
                _strFamSecDed = dedFam.ToString("F");
            }
        }

        Invalidate();
    }
}