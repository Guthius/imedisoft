using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormIcd10s : FormODBase
{
    private List<Icd10> _icd10s;
    
    public bool IsSelectionMode { get; set; }
    public Icd10 SelectedIcd10 { get; set; }
    
    public FormIcd10s()
    {
        InitializeComponent();
    }

    private void FormIcd10s_Load(object sender, EventArgs e)
    {
        if (!IsSelectionMode)
        {
            butOK.Visible = false;
        }

        ActiveControl = textCode;
    }

    private void ButtonSearch_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();
        
        gridMain.Columns.Clear();
        gridMain.Columns.Add( new GridColumn("Icd10 Code", 100));
        gridMain.Columns.Add(new GridColumn("Description", 500));
        
        gridMain.ListGridRows.Clear();
        
        _icd10s = Icd10s.GetBySearchText(textCode.Text);
        
        foreach (var icd10 in _icd10s)
        {
            var gridRow = new GridRow();
            
            gridRow.Cells.Add(icd10.Icd10Code);
            gridRow.Cells.Add(icd10.Description);
            gridRow.Tag = icd10;
            
            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }
    
    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!IsSelectionMode)
        {
            return;
        }
        
        SelectedIcd10 = (Icd10) gridMain.ListGridRows[e.Row].Tag;
        
        DialogResult = DialogResult.OK;
    }
    
    private void ButtonCodeImport_Click(object sender, EventArgs e)
    {
        using var formCodeSystemsImport = new FormCodeSystemsImport();
        
        formCodeSystemsImport.ShowDialog();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        SelectedIcd10 = (Icd10) gridMain.ListGridRows[gridMain.GetSelectedIndex()].Tag;
        
        DialogResult = DialogResult.OK;
    }
}