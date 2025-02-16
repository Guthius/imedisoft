using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDiscountPlanSubEdit : FormODBase
{
    public DiscountPlan DiscountPlanCur;
    public long PatNum;
    public DiscountPlanSub DiscountPlanSubCur;

    public FormDiscountPlanSubEdit()
    {
        InitializeComponent();
    }

    private void FormDiscountPlanSubEdit_Load(object sender, EventArgs e)
    {
        if (DiscountPlanSubCur is not null)
        {
            DiscountPlanCur = DiscountPlans.GetPlan(DiscountPlanSubCur.DiscountPlanNum);

            PatNum = DiscountPlanSubCur.PatNum;

            FillForm();

            return;
        }

        DiscountPlanSubCur = new DiscountPlanSub
        {
            IsNew = true,
            DiscountPlanNum = DiscountPlanCur.DiscountPlanNum,
            PatNum = PatNum
        };
        
        FillForm();
    }

    private void FillForm()
    {
        if (DiscountPlanCur != null)
        {
            textDescript.Text = DiscountPlanCur.Description;
            textFeeSched.Text = FeeScheds.GetFirstOrDefault(x => x.FeeSchedNum == DiscountPlanCur.FeeSchedNum, true)?.Description ?? "";
            textAdjustmentType.Text = Defs.GetDef(DefCat.AdjTypes, DiscountPlanCur.DefNum).ItemName;
            textPlanNote.Text = DiscountPlanCur.PlanNote;

            if (!Security.IsAuthorized(EnumPermType.InsPlanEdit, true))
            {
                textPlanNote.Enabled = false;
            }

            textPlanNum.Text = DiscountPlanCur.DiscountPlanNum.ToString();
        }

        if (DiscountPlanSubCur is null)
        {
            return;
        }


        textName.Text = Patients.GetLim(DiscountPlanSubCur.PatNum).GetNameLF();
        textDateEffective.Text = DiscountPlanSubCur.DateEffective.Year < 1880 ? "" : DiscountPlanSubCur.DateEffective.ToShortDateString();
        textDateTerm.Text = DiscountPlanSubCur.DateTerm.Year < 1880 ? "" : DiscountPlanSubCur.DateTerm.ToShortDateString();
        textSubNote.Text = DiscountPlanSubCur.SubNote;
    }

    private void ButtonDiscountPlans_Click(object sender, EventArgs e)
    {
        using var formDiscountPlans = new FormDiscountPlans();

        formDiscountPlans.SelectedDiscountPlan = DiscountPlanCur;
        formDiscountPlans.IsSelectionMode = true;

        if (formDiscountPlans.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        DiscountPlanCur = formDiscountPlans.SelectedDiscountPlan;

        FillForm();
    }

    private void ButtonDrop_Click(object sender, EventArgs e)
    {
        if (DiscountPlanSubCur.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!ConfirmOk("Drop Discount Plan?"))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(textSubNote.Text) && Confirm("Save Subscriber Note to Commlog?"))
        {
            Commlogs.Insert(new Commlog
            {
                PatNum = DiscountPlanSubCur.PatNum,
                CommDateTime = DateTime.Now,
                CommType = Commlogs.GetTypeAuto(CommItemTypeAuto.MISC),
                Note = "Subscriber note from dropped discount plan, saved copy: " + textSubNote.Text,
                UserNum = Security.CurUser.UserNum
            });
        }

        DiscountPlanSubs.UpdateAssociatedDiscountPlanAmts([DiscountPlanSubCur], true);
        DiscountPlanSubs.Delete(DiscountPlanSubCur.DiscountSubNum);

        var logText = "The discount plan " + DiscountPlanCur.Description + " was dropped.";

        SecurityLogs.MakeLogEntry(EnumPermType.DiscountPlanAddDrop, DiscountPlanSubCur.PatNum, logText);

        DiscountPlanSubCur = null;

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textDateEffective.IsValid() || !textDateTerm.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        DiscountPlanSubCur.DateEffective = SIn.Date(textDateEffective.Text);
        DiscountPlanSubCur.DateTerm = SIn.Date(textDateTerm.Text);
        DiscountPlanSubCur.DiscountPlanNum = DiscountPlanCur.DiscountPlanNum;

        if (DiscountPlanSubCur.DiscountPlanNum == 0)
        {
            ShowError("Invald plan. Please select another plan.");
            return;
        }

        DiscountPlanSubCur.SubNote = textSubNote.Text;
        
        var discountPlanSub = DiscountPlanSubs.GetSubForPat(PatNum);
        if (discountPlanSub is not null)
        {
            DiscountPlanSubs.Update(DiscountPlanSubCur);
            if (discountPlanSub.DiscountPlanNum != DiscountPlanSubCur.DiscountPlanNum)
            {
                var logText = "The discount plan changed to " + DiscountPlanCur.Description + ".";
                
                SecurityLogs.MakeLogEntry(EnumPermType.DiscountPlanAddDrop, DiscountPlanSubCur.PatNum, logText);
            }
        }
        else
        {
            DiscountPlanSubs.Insert(DiscountPlanSubCur);

            DiscountPlanSubCur.IsNew = false;

            var logText = "The discount plan " + DiscountPlanCur.Description + " was added.";

            SecurityLogs.MakeLogEntry(EnumPermType.DiscountPlanAddDrop, DiscountPlanSubCur.PatNum, logText);
        }

        DiscountPlanSubs.UpdateAssociatedDiscountPlanAmts([DiscountPlanSubCur]);
        if (DiscountPlanCur is not null && DiscountPlanCur.PlanNote != textPlanNote.Text)
        {
            var logText = "Discount plan: " + DiscountPlanCur.Description + " plan note changed from \"" + DiscountPlanCur.PlanNote + "\" to \"" + textPlanNote.Text + "\"";
            
            DiscountPlanCur.PlanNote = textPlanNote.Text;
            
            DiscountPlans.Update(DiscountPlanCur);
            
            SecurityLogs.MakeLogEntry(EnumPermType.DiscountPlanEdit, DiscountPlanSubCur.PatNum, logText);
        }

        DialogResult = DialogResult.OK;
    }
}