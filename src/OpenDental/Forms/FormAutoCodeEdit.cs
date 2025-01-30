using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutoCodeEdit : FormODBase
{
    public bool IsNew;

    private readonly AutoCode _autoCode;
    private List<AutoCodeItem> _autoCodeItems;
    private List<AutoCodeCond> _autoCodeConds;

    public FormAutoCodeEdit(AutoCode autoCode)
    {
        _autoCode = autoCode;

        InitializeComponent();
    }

    private void FormAutoCodeEdit_Load(object sender, EventArgs e)
    {
        if (IsNew)
        {
            Text = "Add Auto Code";
        }
        else
        {
            Text = "Edit Auto Code";

            textDescript.Text = _autoCode.Description;

            checkHidden.Checked = _autoCode.IsHidden;
            checkLessIntrusive.Checked = _autoCode.LessIntrusive;
        }

        FillGrid();
    }

    private void FormAutoCodeEdit_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK)
        {
            if (IsNew)
            {
                try
                {
                    AutoCodes.Delete(_autoCode);
                }
                catch (ApplicationException ex)
                {
                    ShowError(ex.Message);
                }

                return;
            }

            if (_autoCodeItems.Count == 0)
            {
                try
                {
                    AutoCodes.Delete(_autoCode);
                }
                catch
                {
                    // ignored
                }

                return;
            }
        }

        AutoCodeItems.RefreshCache();
        AutoCodeConds.RefreshCache();

        foreach (var autoCodeItem in _autoCodeItems)
        {
            autoCodeItem.ListConditions = [];

            foreach (var autoCodeCond in _autoCodeConds)
            {
                if (autoCodeCond.AutoCodeItemNum == autoCodeItem.AutoCodeItemNum)
                {
                    autoCodeItem.ListConditions.Add(autoCodeCond);
                }
            }
        }

        for (var i = 1; i < _autoCodeItems.Count; i++)
        {
            if (_autoCodeItems[i].ListConditions.Count == _autoCodeItems[0].ListConditions.Count)
            {
                continue;
            }

            ShowError("All AutoCode items must have the same number of conditions.");

            e.Cancel = true;

            return;
        }

        if (_autoCodeItems[0].ListConditions.Count == 0)
        {
            return;
        }

        for (var i = 1; i < _autoCodeItems.Count; i++)
        {
            for (var j = 0; j < i; j++)
            {
                var matches = _autoCodeItems[i].ListConditions.Where((t, k) => t.Cond == _autoCodeItems[j].ListConditions[k].Cond).Count();
                if (matches != _autoCodeItems[i].ListConditions.Count)
                {
                    continue;
                }

                ShowError("Cannot have two AutoCode Items with duplicate conditions.");

                e.Cancel = true;

                return;
            }
        }

        var isAnterior = false;
        var isPosterior = false;
        var isPremolarOrMolar = false;
        var isNumberOfSurfaces = false;
        var isFirstOrEachAdditional = false;
        var isMaxillaryOrMandibular = false;
        var isPrimaryOrPermanent = false;
        var isPonticOrRetainer = false;

        foreach (var autoCodeItem in _autoCodeItems)
        {
            foreach (var autoCodeCond in autoCodeItem.ListConditions)
            {
                switch (autoCodeCond.Cond)
                {
                    case AutoCondition.Anterior:
                        isAnterior = true;
                        continue;

                    case AutoCondition.Posterior:
                        isPosterior = true;
                        continue;

                    case AutoCondition.Premolar or AutoCondition.Molar:
                        isPremolarOrMolar = true;
                        continue;

                    case AutoCondition.One_Surf or AutoCondition.Two_Surf or AutoCondition.Three_Surf or AutoCondition.Four_Surf or AutoCondition.Five_Surf:
                        isNumberOfSurfaces = true;
                        continue;

                    case AutoCondition.First or AutoCondition.EachAdditional:
                        isFirstOrEachAdditional = true;
                        continue;

                    case AutoCondition.Maxillary or AutoCondition.Mandibular:
                        isMaxillaryOrMandibular = true;
                        continue;

                    case AutoCondition.Primary or AutoCondition.Permanent:
                        isPrimaryOrPermanent = true;
                        continue;

                    case AutoCondition.Pontic or AutoCondition.Retainer:
                        isPonticOrRetainer = true;
                        break;
                }
            }
        }

        if (isPosterior && isPremolarOrMolar)
        {
            ShowError("Cannot have both Posterior and Premolar/Molar categories.");

            e.Cancel = true;
            return;
        }

        if (isAnterior)
        {
            if (!isPosterior && !isPremolarOrMolar)
            {
                ShowError("Anterior condition is present without any corresponding posterior or premolar/molar condition.");

                e.Cancel = true;
                return;
            }
        }

        var categoryCount = 0;
        if (isPosterior)
        {
            categoryCount++;
        }

        if (isPremolarOrMolar)
        {
            categoryCount++;
        }

        if (isNumberOfSurfaces)
        {
            categoryCount++;
        }

        if (isFirstOrEachAdditional)
        {
            categoryCount++;
        }

        if (isMaxillaryOrMandibular)
        {
            categoryCount++;
        }

        if (isPrimaryOrPermanent)
        {
            categoryCount++;
        }

        if (isPonticOrRetainer)
        {
            categoryCount++;
        }

        if (categoryCount != _autoCodeItems[0].ListConditions.Count)
        {
            ShowError(
                "When using " + _autoCodeItems[0].ListConditions.Count + " condition(s), you must use conditions from " + _autoCodeItems[0].ListConditions.Count + " logical categories. " +
                "You are using conditions from " + categoryCount + " logical categories.");

            e.Cancel = true;
            return;
        }

        var requiredAutoCodeItems = 1;
        if (isPosterior)
        {
            requiredAutoCodeItems *= 2;
        }

        if (isPremolarOrMolar)
        {
            if (isPrimaryOrPermanent)
            {
                requiredAutoCodeItems *= 5;
            }
            else
            {
                requiredAutoCodeItems *= 3;
            }
        }
        else
        {
            if (isPrimaryOrPermanent)
            {
                requiredAutoCodeItems *= 2;
            }
        }

        if (isNumberOfSurfaces)
        {
            requiredAutoCodeItems *= 5;
        }

        if (isFirstOrEachAdditional)
        {
            requiredAutoCodeItems *= 2;
        }

        if (isMaxillaryOrMandibular)
        {
            requiredAutoCodeItems *= 2;
        }

        if (isPonticOrRetainer)
        {
            requiredAutoCodeItems *= 2;
        }

        if (_autoCodeItems.Count == requiredAutoCodeItems)
        {
            return;
        }

        ShowError(
            "For the condition categories you are using, you should have " + requiredAutoCodeItems + " entries in your list. " +
            "You have " + _autoCodeItems.Count + ".");

        e.Cancel = true;
    }

    private void FillGrid()
    {
        AutoCodeItems.RefreshCache();
        AutoCodeConds.RefreshCache();

        _autoCodeConds = AutoCodeConds.GetDeepCopy();
        _autoCodeItems = AutoCodeItems.GetListForCode(_autoCode.AutoCodeNum);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Code", 100));
        gridMain.Columns.Add(new GridColumn("Description", 200));
        gridMain.Columns.Add(new GridColumn("Conditions", 400));

        gridMain.ListGridRows.Clear();

        foreach (var autoCodeItem in _autoCodeItems)
        {
            var conditions = "";

            var count = 0;

            foreach (var autoCodeCond in _autoCodeConds)
            {
                if (autoCodeCond.AutoCodeItemNum != autoCodeItem.AutoCodeItemNum)
                {
                    continue;
                }

                if (count != 0)
                {
                    conditions += ", ";
                }

                conditions += autoCodeCond.Cond.ToString();
                count++;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(ProcedureCodes.GetProcCode(autoCodeItem.CodeNum).ProcCode);
            gridRow.Cells.Add(ProcedureCodes.GetProcCode(autoCodeItem.CodeNum).Descript);
            gridRow.Cells.Add(conditions);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var autoCodeItem = _autoCodeItems[e.Row];

        using var formAutoItemEdit = new FormAutoItemEdit(autoCodeItem);

        formAutoItemEdit.ShowDialog();

        FillGrid();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var autoCodeItem = new AutoCodeItem
        {
            AutoCodeNum = _autoCode.AutoCodeNum
        };

        using var formAutoItemEdit = new FormAutoItemEdit(autoCodeItem);

        formAutoItemEdit.ShowDialog();

        FillGrid();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        var index = gridMain.GetSelectedIndex();
        if (index == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        var autoCodeItem = _autoCodeItems[index];

        AutoCodeConds.DeleteForItemNum(autoCodeItem.AutoCodeItemNum);
        AutoCodeItems.Delete(autoCodeItem);

        FillGrid();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescript.Text == "")
        {
            ShowError("The Description cannot be blank");

            return;
        }

        if (_autoCodeItems.Count == 0)
        {
            ShowError("Must have at least one item in the list.");

            return;
        }

        _autoCode.Description = textDescript.Text;
        _autoCode.IsHidden = checkHidden.Checked;
        _autoCode.LessIntrusive = checkLessIntrusive.Checked;

        AutoCodes.Update(_autoCode);

        DialogResult = DialogResult.OK;
    }
}