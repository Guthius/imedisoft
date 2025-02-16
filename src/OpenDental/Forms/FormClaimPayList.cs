using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormClaimPayList : FormODBase
{
    private List<ClaimPayment> _claimPayments;
    private List<Def> _claimPaymentGroupDefs;

    public FormClaimPayList()
    {
        InitializeComponent();
    }

    private void FormClaimPayList_Load(object sender, EventArgs e)
    {
        textDateFrom.Text = DateTime.Now.AddDays(-10).ToShortDateString();
        textDateTo.Text = DateTime.Now.ToShortDateString();
        
        comboClinic.IsAllSelected = true;
        
        _claimPaymentGroupDefs = Defs.GetDefsForCategory(DefCat.ClaimPaymentGroups, true);
        
        FillComboPaymentGroup();
        FillGrid();
    }

    private void FillGrid()
    {
        var dateFrom = SIn.Date(textDateFrom.Text);
        var dateTo = SIn.Date(textDateTo.Text);
        
        long clinicNum = 0;
        
        if (!comboClinic.IsAllSelected)
        {
            clinicNum = comboClinic.ClinicNumSelected;
        }

        long defNum = 0;
        if (comboPayGroup.SelectedIndex != 0)
        {
            defNum = _claimPaymentGroupDefs[comboPayGroup.SelectedIndex - 1].DefNum;
        }

        var dataTable = ClaimPayments.GetForDateRange(dateFrom, dateTo, clinicNum, defNum);
        
        _claimPayments = ClaimPaymentCrud.TableToList(dataTable);
        
        gridMain.BeginUpdate();
        
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 65));
        gridMain.Columns.Add(new GridColumn("Type", 70));
        gridMain.Columns.Add(new GridColumn("Amount", 75, HorizontalAlignment.Right));
        gridMain.Columns.Add(new GridColumn("Partial", 40, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Carrier", 180));
        gridMain.Columns.Add(new GridColumn("PayGroup", 80));
        gridMain.Columns.Add(new GridColumn("Clinic", 80));
        gridMain.Columns.Add(new GridColumn("Note", 180));
        gridMain.Columns.Add(new GridColumn("Scanned", 40, HorizontalAlignment.Center));
        
        gridMain.ListGridRows.Clear();
        
        for (var i = 0; i < _claimPayments.Count; i++)
        {
            var gridRow = new GridRow();
            
            gridRow.Cells.Add(_claimPayments[i].CheckDate.Year < 1800 ? "" : _claimPayments[i].CheckDate.ToShortDateString());
            gridRow.Cells.Add(Defs.GetName(DefCat.InsurancePaymentType, _claimPayments[i].PayType));
            gridRow.Cells.Add(_claimPayments[i].CheckAmt.ToString("c"));
            gridRow.Cells.Add(_claimPayments[i].IsPartial ? "X" : "");
            gridRow.Cells.Add(_claimPayments[i].CarrierName);
            gridRow.Cells.Add(Defs.GetName(DefCat.ClaimPaymentGroups, _claimPayments[i].PayGroup));
            gridRow.Cells.Add(Clinics.GetAbbr(_claimPayments[i].ClinicNum));
            gridRow.Cells.Add(_claimPayments[i].Note);
            gridRow.Cells.Add(dataTable.Rows[i]["hasEobAttach"].ToString() == "1" ? "X" : "");
            
            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        gridMain.ScrollToEnd();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPayCreate))
        {
            return;
        }

        var claimPayment = new ClaimPayment
        {
            CheckDate = DateTime.Now,
            IsPartial = true,
            ClinicNum = Clinics.ClinicNum
        };

        using var formClaimPayEdit = new FormClaimPayEdit(claimPayment);
        
        formClaimPayEdit.IsNew = true;
        
        if (formClaimPayEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var formClaimPayBatch = new FormClaimPayBatch(claimPayment, isRefreshNeeded: true);
        
        formClaimPayBatch.Show();
        formClaimPayBatch.FormClosed += FormClaimPayBatchAdd_FormClosed;
    }

    private void FormClaimPayBatchAdd_FormClosed(object sender, FormClosedEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        FillGrid();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.InsPayCreate))
        {
            return;
        }

        var formClaimPayBatch = new FormClaimPayBatch(_claimPayments[gridMain.GetSelectedIndex()]);
        
        formClaimPayBatch.Show();
        formClaimPayBatch.FormClosed += FormClaimPayBatchEdit_FormClosed;
    }

    private void FormClaimPayBatchEdit_FormClosed(object sender, FormClosedEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        var formClaimPayBatch = (FormClaimPayBatch) sender;
        if (formClaimPayBatch.GotoClaimNum != 0)
        {
            Close();
        }
        else
        {
            FillGrid();
        }
    }

    private void ButtonRefresh_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillComboPaymentGroup(long selectedDefNum = 0)
    {
        comboPayGroup.Items.Clear();
        comboPayGroup.Items.Add("All");
        comboPayGroup.SelectedIndex = 0;

        for (var i = 0; i < _claimPaymentGroupDefs.Count; i++)
        {
            var def = _claimPaymentGroupDefs[i];

            comboPayGroup.Items.Add(def.ItemName);

            if (selectedDefNum != 0 && selectedDefNum == def.DefNum)
            {
                comboPayGroup.SelectedIndex = i + 1;
            }
        }
    }

    private void ButtonPickPaymentGroup_Click(object sender, EventArgs e)
    {
        using var formDefinitionPicker = new FormDefinitionPicker(DefCat.ClaimPaymentGroups);

        if (formDefinitionPicker.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (formDefinitionPicker.SelectedDefs.Count < 1)
        {
            FillComboPaymentGroup();
        }
        else
        {
            FillComboPaymentGroup(formDefinitionPicker.SelectedDefs[0].DefNum);
        }
    }
}