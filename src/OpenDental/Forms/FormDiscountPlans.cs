using System;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDiscountPlans : FormODBase
{
    public bool IsSelectionMode { get; set; }
    public DiscountPlan SelectedDiscountPlan { get; set; }

    public FormDiscountPlans()
    {
        InitializeComponent();
    }

    private void FormDiscountPlans_Load(object sender, EventArgs e)
    {
        FillGrid();

        if (!IsSelectionMode)
        {
            return;
        }
        
        butMerge.Visible = false;
        checkShowHidden.Visible = false;
    }

    private void FillGrid()
    {
        var discountPlans = DiscountPlans.GetAll(checkShowHidden.Checked);
        var discountPlanNums = discountPlans.Select(x => x.DiscountPlanNum).ToList();
        var countsForPlans = DiscountPlans.GetPatCountsForPlans(discountPlanNums);
        
        discountPlans.Sort(DiscountPlanComparer);
        
        gridMain.BeginUpdate();
        
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Description", 200));
        gridMain.Columns.Add(new GridColumn("Fee Schedule", 170));
        gridMain.Columns.Add(new GridColumn("Adjustment Type", checkShowHidden.Checked ? 150 : 170));
        gridMain.Columns.Add(new GridColumn("Pats", 40));
        
        if (checkShowHidden.Checked)
        {
            gridMain.Columns.Add(new GridColumn("Hidden", 20, HorizontalAlignment.Center));
        }

        gridMain.ListGridRows.Clear();
        
        var selectedIdx = -1;
        for (var i = 0; i < discountPlans.Count; i++)
        {
            var adjTypeDef = Defs.GetDef(DefCat.AdjTypes, discountPlans[i].DefNum);
            var countPerPlan = countsForPlans.FirstOrDefault(x => x.DiscountPlanNum == discountPlans[i].DiscountPlanNum);
            
            var gridRow = new GridRow();
            
            gridRow.Cells.Add(discountPlans[i].Description);
            gridRow.Cells.Add(FeeScheds.GetDescription(discountPlans[i].FeeSchedNum));
            gridRow.Cells.Add(adjTypeDef is null ? "" : adjTypeDef.ItemName);
            gridRow.Cells.Add(countPerPlan is null ? "0" : countPerPlan.Count.ToString());

            if (checkShowHidden.Checked)
            {
                gridRow.Cells.Add(discountPlans[i].IsHidden ? "X" : "");
            }

            gridRow.Tag = discountPlans[i];
            
            gridMain.ListGridRows.Add(gridRow);
            
            if (SelectedDiscountPlan != null && discountPlans[i].DiscountPlanNum == SelectedDiscountPlan.DiscountPlanNum)
            {
                selectedIdx = i;
            }
        }

        gridMain.EndUpdate();
        gridMain.SetSelected(selectedIdx);
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedDiscountPlan = (DiscountPlan) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
        if (IsSelectionMode)
        {
            DialogResult = DialogResult.OK;
            return;
        }

        using var formDiscountPlanEdit = new FormDiscountPlanEdit();
        
        formDiscountPlanEdit.DiscountPlanCur = SelectedDiscountPlan.Copy();
        
        if (formDiscountPlanEdit.ShowDialog() == DialogResult.OK)
        {
            FillGrid();
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPlanEdit))
        {
            return;
        }

        var discountPlan = new DiscountPlan
        {
            IsNew = true
        };

        using var formDiscountPlanEdit = new FormDiscountPlanEdit();
        
        formDiscountPlanEdit.DiscountPlanCur = discountPlan;

        if (formDiscountPlanEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }
        
        SelectedDiscountPlan = discountPlan;
            
        FillGrid();
    }

    private void ButtonMerge_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPlanEdit))
        {
            return;
        }

        using var formDiscountPlanMerge = new FormDiscountPlanMerge();
        
        formDiscountPlanMerge.ShowDialog();
        
        FillGrid();
    }

    private void CheckBoxShowHidden_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            DialogResult = DialogResult.OK;
            return;
        }
        
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an entry.");
            return;
        }

        SelectedDiscountPlan = (DiscountPlan) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
        
        DialogResult = DialogResult.OK;
    }

    private static int DiscountPlanComparer(DiscountPlan discountPlan1, DiscountPlan discountPlan2)
    {
        return discountPlan1.IsHidden switch
        {
            false when discountPlan2.IsHidden => -1,
            true when !discountPlan2.IsHidden => 1,
            _ => string.Compare(discountPlan1.Description, discountPlan2.Description, StringComparison.Ordinal)
        };
    }
}