using System;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormBenefitFrequencyEdit : FormODBase
{
    public Benefit BenefitCur;
    public long PatPlanNum;

    public FormBenefitFrequencyEdit()
    {
        InitializeComponent();
    }

    private void FormBenefitFrequencyEdit_Load(object sender, EventArgs e)
    {
        var codeGroups = CodeGroups.GetDeepCopy(shortList: true);
        
        listBoxCodeGroup.Items.AddList(codeGroups, x => x.GroupName);
        
        if (BenefitCur.CodeGroupNum > 0)
        {
            listBoxCodeGroup.SetSelectedKey<CodeGroup>(BenefitCur.CodeGroupNum, x => x.CodeGroupNum);
        }

        textNumber.Text = BenefitCur.Quantity.ToString();
        
        listBoxTimePeriod.Items.AddEnums<FrequencyOptions>();
        listBoxTimePeriod.SetSelectedEnum(BenefitCur.GetFrequencyOption());
        listBoxTreatArea.Items.Add("Default", TreatmentArea.None);
            
        var treatmentAreas = typeof(TreatmentArea)
            .GetEnumValues()
            .Cast<TreatmentArea>()
            .Where(x => x != TreatmentArea.None)
            .ToList();
        
        listBoxTreatArea.Items.AddList(treatmentAreas, x => x.ToString());
        listBoxTreatArea.SetSelected((int) BenefitCur.TreatArea);
        
        checkPat.Checked = BenefitCur.PatPlanNum != 0;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textNumber.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        if (listBoxCodeGroup.GetSelected<CodeGroup>() == null)
        {
            ShowError("Please select a Code Group.");
            return;
        }

        var codeGroup = listBoxCodeGroup.GetSelected<CodeGroup>();
        
        BenefitCur.CodeGroupNum = codeGroup.CodeGroupNum;
        
        var frequencyOptions = listBoxTimePeriod.GetSelected<FrequencyOptions>();
        var isCalendarYear = InsPlans.GetPlan(BenefitCur.PlanNum, null).MonthRenew == 0;
        
        BenefitCur.SetFrequencyOption(frequencyOptions, isCalendarYear);
        BenefitCur.Quantity = SIn.Byte(textNumber.Text);
        BenefitCur.TreatArea = listBoxTreatArea.GetSelected<TreatmentArea>();
        BenefitCur.IsNew = false;
        BenefitCur.PatPlanNum = 0;
        
        if (checkPat.Checked)
        {
            BenefitCur.PatPlanNum = PatPlanNum;
            BenefitCur.PlanNum = 0;
        }

        DialogResult = DialogResult.OK;
    }
}