using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDefinitionPicker : FormODBase
{
    private readonly List<Def> _initialSelectedDefs = [];
    private List<Def> _showingDefs;

    public bool HasShowHiddenOption { get; set; }
    public bool IsMultiSelectionMode { get; set; }
    public List<Def> SelectedDefs { get; set; }

    public FormDefinitionPicker(DefCat defCat, List<Def> listDefs = null, long defNumExclude = 0)
    {
        InitializeComponent();

        if (listDefs != null)
        {
            _initialSelectedDefs = new List<Def>(listDefs);
        }

        gridMain.Title = defCat.ToString();

        FillListDefs(defCat, defNumExclude);
    }

    private void FormDefinitionPicker_Load(object sender, EventArgs e)
    {
        if (!HasShowHiddenOption)
        {
            checkShowHidden.Visible = false;
        }

        if (!IsMultiSelectionMode)
        {
            gridMain.SelectionMode = GridSelectionMode.OneRow;
        }

        if (_initialSelectedDefs.Any(x => x.IsHidden))
        {
            checkShowHidden.Checked = true;
        }

        FillGrid();

        var selectedDefNums = _initialSelectedDefs.Select(x => x.DefNum).ToList();

        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            if (selectedDefNums.Contains(((Def) gridMain.ListGridRows[i].Tag).DefNum))
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void FillListDefs(DefCat defCat, long excludeDefNum)
    {
        _showingDefs = Defs.GetDefsForCategory(defCat);

        var excludeDef = _showingDefs.FirstOrDefault(x => x.DefNum == excludeDefNum);
        if (excludeDef is null)
        {
            return;
        }

        var invalidDefNums = new List<long> {excludeDef.DefNum};
        for (var i = 0; i < invalidDefNums.Count; i++)
        {
            var defNumStr = invalidDefNums[i].ToString();
            var defNums = _showingDefs.Where(x => x.ItemValue == defNumStr).Select(x => x.DefNum);

            invalidDefNums.AddRange(defNums);
        }

        _showingDefs.RemoveAll(x => invalidDefNums.Contains(x.DefNum));
    }

    private void FillGrid()
    {
        var selectedDefNums = gridMain.SelectedTags<Def>().Select(x => x.DefNum).ToList();

        gridMain.BeginUpdate();
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Definition", 200));
        gridMain.Columns.Add(new GridColumn("ItemValue", 70));
        
        if (HasShowHiddenOption)
        {
            gridMain.Columns.Add(new GridColumn("Hidden", 20) {IsWidthDynamic = true});
        }

        gridMain.ListGridRows.Clear();
        foreach (var def in _showingDefs)
        {
            if (def.IsHidden && !checkShowHidden.Checked)
            {
                continue;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(def.ItemName);

            if (def.Category == DefCat.AutoNoteCats && !string.IsNullOrWhiteSpace(def.ItemValue))
            {
                var parentDef = _showingDefs.FirstOrDefault(x => x.DefNum.ToString() == def.ItemValue);

                gridRow.Cells.Add(parentDef is null ? def.ItemValue : parentDef.ItemName);
            }
            else
            {
                gridRow.Cells.Add(def.ItemValue);
            }

            if (HasShowHiddenOption)
            {
                gridRow.Cells.Add(def.IsHidden ? "X" : "");
            }

            gridRow.Tag = def;
            
            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        for (var i = 0; i < gridMain.ListGridRows.Count; i++)
        {
            if (selectedDefNums.Contains(((Def) gridMain.ListGridRows[i].Tag).DefNum))
            {
                gridMain.SetSelected(i);
            }
        }
    }

    private void CheckBoxShowHidden_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedDefs = ListTools.FromSingle((Def) gridMain.ListGridRows[e.Row].Tag);

        DialogResult = DialogResult.OK;
    }

    private void ButtonNone_Click(object sender, EventArgs e)
    {
        gridMain.SetAll(false);
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SelectedDefs = gridMain.SelectedTags<Def>();

        DialogResult = DialogResult.OK;
    }
}