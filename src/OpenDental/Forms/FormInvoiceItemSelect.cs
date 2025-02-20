using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormInvoiceItemSelect : FormODBase
{
    private readonly long _patNum;

    private DataTable _tableSuperFamAcct;
    private GridOD _gridMain;

    public readonly List<DataRow> SelectedDataRows = [];

    public FormInvoiceItemSelect(long patNum)
    {
        _patNum = patNum;

        InitializeComponent();
    }

    private void FormInvoiceItemSelect_Load(object sender, EventArgs e)
    {
        _tableSuperFamAcct = Patients.GetSuperFamProcAdjustsPPCharges(_patNum);

        FillGrid();
    }

    private void FillGrid()
    {
        _gridMain.BeginUpdate();

        _gridMain.Columns.Clear();
        _gridMain.Columns.Add(new GridColumn("Date", 70));
        _gridMain.Columns.Add(new GridColumn("PatName", 100));
        _gridMain.Columns.Add(new GridColumn("Prov", 55));
        _gridMain.Columns.Add(new GridColumn("Code", 55));
        _gridMain.Columns.Add(new GridColumn("Tooth", 50));
        _gridMain.Columns.Add(new GridColumn("Description", 150));
        _gridMain.Columns.Add(new GridColumn("Fee", 60, HorizontalAlignment.Right));

        _gridMain.ListGridRows.Clear();

        var procedureCodes = ProcedureCodes.GetAllCodes();
        
        for (var i = 0; i < _tableSuperFamAcct.Rows.Count; i++)
        {
            if (checkIsFilteringZeroAmount.Checked && SIn.Double(_tableSuperFamAcct.Rows[i]["Amount"].ToString()) == 0)
            {
                continue;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(SIn.DateTime(_tableSuperFamAcct.Rows[i]["Date"].ToString()).ToShortDateString());
            gridRow.Cells.Add(_tableSuperFamAcct.Rows[i]["PatName"].ToString());
            gridRow.Cells.Add(Providers.GetAbbr(SIn.Long(_tableSuperFamAcct.Rows[i]["Prov"].ToString())));
            
            if (!string.IsNullOrWhiteSpace(_tableSuperFamAcct.Rows[i]["AdjType"].ToString()))
            {
                gridRow.Cells.Add("Adjust");
                gridRow.Cells.Add(Tooth.Display(_tableSuperFamAcct.Rows[i]["Tooth"].ToString()));
                gridRow.Cells.Add(Defs.GetName(DefCat.AdjTypes, SIn.Long(_tableSuperFamAcct.Rows[i]["AdjType"].ToString())));
            }
            else if (!string.IsNullOrWhiteSpace(_tableSuperFamAcct.Rows[i]["ChargeType"].ToString()))
            {
                if (PrefC.GetInt(PrefName.PayPlansVersion) != (int) PayPlanVersions.AgeCreditsAndDebits)
                {
                    continue;
                }

                gridRow.Cells.Add("Pay Plan");
                gridRow.Cells.Add(Tooth.Display(_tableSuperFamAcct.Rows[i]["Tooth"].ToString()));
                gridRow.Cells.Add(SIn.Enum<PayPlanChargeType>(SIn.Int(_tableSuperFamAcct.Rows[i]["ChargeType"].ToString())).GetDescription());
            }
            else
            {
                var procedureCode = ProcedureCodes.GetProcCode(SIn.Long(_tableSuperFamAcct.Rows[i]["Code"].ToString()), procedureCodes);

                gridRow.Cells.Add(procedureCode.ProcCode);
                gridRow.Cells.Add(Tooth.Display(_tableSuperFamAcct.Rows[i]["Tooth"].ToString()));
                gridRow.Cells.Add(procedureCode.Descript);
            }

            gridRow.Cells.Add(SIn.Double(_tableSuperFamAcct.Rows[i]["Amount"].ToString()).ToString("F"));
            gridRow.Tag = _tableSuperFamAcct.Rows[i];

            _gridMain.ListGridRows.Add(gridRow);
        }

        _gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var dataRow = (DataRow) _gridMain.ListGridRows[e.Row].Tag;

        SelectedDataRows.Clear();
        SelectedDataRows.Add(dataRow);

        DialogResult = DialogResult.OK;
    }

    private void CheckBoxIsFilteringZeroAmount_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void ButtonAll_Click(object sender, EventArgs e)
    {
        _gridMain.SetAll(true);
    }

    private void ButtonNone_Click(object sender, EventArgs e)
    {
        _gridMain.SetAll(false);
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (_gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select an item first.");
            return;
        }

        SelectedDataRows.Clear();

        foreach (var index in _gridMain.SelectedIndices)
        {
            var dataRow = (DataRow) _gridMain.ListGridRows[index].Tag;

            SelectedDataRows.Add(dataRow);
        }

        DialogResult = DialogResult.OK;
    }
}