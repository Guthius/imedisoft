using System;
using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormPatientListDiscount : FormODBase
{
    public DiscountPlan DiscountPlanCur;
    public List<string> ListPatNames { get; set; }

    public FormPatientListDiscount()
    {
        InitializeComponent();
    }

    private void FormPatientListDiscount_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        ListPatNames ??= DiscountPlans
            .GetPatNamesForPlan(DiscountPlanCur.DiscountPlanNum)
            .Distinct().OrderBy(patName => patName)
            .ToList();

        gridMain.BeginUpdate();
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Name", 100));

        gridMain.ListGridRows.Clear();

        foreach (var patName in ListPatNames)
        {
            var gridRow = new GridRow(patName);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }
}