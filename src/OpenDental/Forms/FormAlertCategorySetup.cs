using System;
using System.Collections.Generic;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAlertCategorySetup : FormODBase
{
    private readonly List<AlertCategory> _alertCategoriesInternal = [];
    private readonly List<AlertCategory> _alertCategoriesCustom = [];

    public FormAlertCategorySetup()
    {
        InitializeComponent();
    }

    private void FormAlertCategorySetup_Load(object sender, EventArgs e)
    {
        FillGrids();
    }

    private void FillGrids(long selectedInternalKey = 0, long selectedCustomKey = 0)
    {
        _alertCategoriesCustom.Clear();
        _alertCategoriesInternal.Clear();

        var alertCategories = AlertCategories.GetDeepCopy();

        foreach (var alertCategory in alertCategories)
        {
            if (alertCategory.IsHQCategory)
            {
                _alertCategoriesInternal.Add(alertCategory);
            }
            else
            {
                _alertCategoriesCustom.Add(alertCategory);
            }
        }

        FillInternalGrid(selectedInternalKey);
        FillCustomGrid(selectedCustomKey);
    }

    private void FillInternalGrid(long selectedInternalKey)
    {
        gridInternal.BeginUpdate();

        gridInternal.Columns.Clear();
        gridInternal.Columns.Add(new GridColumn("Description", 100));

        gridInternal.ListGridRows.Clear();

        foreach (var alertCategory in _alertCategoriesInternal)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(alertCategory.Description);
            gridRow.Tag = alertCategory.AlertCategoryNum;

            gridInternal.ListGridRows.Add(gridRow);

            var index = gridInternal.ListGridRows.Count - 1;
            if (selectedInternalKey == alertCategory.AlertCategoryNum)
            {
                gridCustom.SetSelected(index);
            }
        }

        gridInternal.EndUpdate();
    }

    private void FillCustomGrid(long selectedCustomKey)
    {
        gridCustom.BeginUpdate();

        gridCustom.Columns.Clear();
        gridCustom.Columns.Add(new GridColumn("Description", 100));

        gridCustom.ListGridRows.Clear();

        var index = 0;
        foreach (var alertCategory in _alertCategoriesCustom)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(alertCategory.Description);
            gridRow.Tag = alertCategory.AlertCategoryNum;

            gridCustom.ListGridRows.Add(gridRow);

            index = gridCustom.ListGridRows.Count - 1;
            if (selectedCustomKey != alertCategory.AlertCategoryNum)
            {
                index = 0;
            }
        }

        if (index != 0)
        {
            gridCustom.SetSelected(index);
        }

        gridCustom.EndUpdate();
    }

    private void gridInternal_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var frmAlertCategoryEdit = new FrmAlertCategoryEdit(_alertCategoriesInternal[e.Row]);

        frmAlertCategoryEdit.ShowDialog();

        if (frmAlertCategoryEdit.IsDialogOK)
        {
            FillGrids();
        }
    }

    private void gridCustom_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var frmAlertCategoryEdit = new FrmAlertCategoryEdit(_alertCategoriesCustom[e.Row]);

        frmAlertCategoryEdit.ShowDialog();

        if (frmAlertCategoryEdit.IsDialogOK)
        {
            FillGrids();
        }
    }

    private void butCopy_Click(object sender, EventArgs e)
    {
        if (gridInternal.GetSelectedIndex() == -1)
        {
            ShowError("Please select an internal alert category from the list first.");
            return;
        }

        InsertCopyAlertCategory(_alertCategoriesInternal[gridInternal.GetSelectedIndex()].Copy());
    }

    private void butDuplicate_Click(object sender, EventArgs e)
    {
        if (gridCustom.GetSelectedIndex() == -1)
        {
            ShowError("Please select a custom alert category from the list first.");
            return;
        }

        InsertCopyAlertCategory(_alertCategoriesCustom[gridCustom.GetSelectedIndex()].Copy());
    }

    private void InsertCopyAlertCategory(AlertCategory alertCategory)
    {
        alertCategory.IsHQCategory = false;
        alertCategory.Description += "(Copy)";

        var alertCategoryLinks = AlertCategoryLinks.GetForCategory(alertCategory.AlertCategoryNum);

        alertCategory.AlertCategoryNum = AlertCategories.Insert(alertCategory);
        alertCategoryLinks.ForEach(x =>
        {
            x.AlertCategoryNum = alertCategory.AlertCategoryNum;

            AlertCategoryLinks.Insert(x);
        });

        DataValid.SetInvalid(InvalidType.AlertCategories, InvalidType.AlertCategoryLinks);

        FillGrids();
    }
}