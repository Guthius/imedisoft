using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDunningSetup : FormODBase
{
    private List<Dunning> _dunnings;
    private List<Def> _billingTypeDefs;

    public FormDunningSetup()
    {
        InitializeComponent();
    }

    private void FormDunningSetup_Load(object sender, EventArgs e)
    {
        _billingTypeDefs = Defs.GetDefsForCategory(DefCat.BillingTypes, true);

        listBill.Items.Add("(all)");
        listBill.SetSelected(0);
        listBill.Items.AddStrings(_billingTypeDefs.Select(x => x.ItemName));

        comboClinics.ClinicNumSelected = Clinics.ClinicNum;

        FillGrids(true);
    }

    private void FillGrids(bool doRefreshList = false)
    {
        if (doRefreshList)
        {
            var listClinicNums = new List<long>(); //Empty list to allow query to run for all clinics.
            if (Security.CurUser.ClinicIsRestricted)
            {
                listClinicNums = Clinics.GetForUserod(Security.CurUser, true).Select(x => x.Id).ToList();
                listClinicNums.Add(-2);
            }

            _dunnings = Dunnings.Refresh(listClinicNums);
        }

        var dunnings = _dunnings.FindAll(ValidateDunningFilters);
        if (!PrefC.GetBool(PrefName.ShowFeatureSuperfamilies))
        {
            dunnings.RemoveAll(x => x.IsSuperFamily);
        }

        gridDunning.BeginUpdate();
        gridDunning.Columns.Clear();
        gridDunning.Columns.Add(new GridColumn("Billing Type", 80));
        gridDunning.Columns.Add(new GridColumn("Aging", 70));
        gridDunning.Columns.Add(new GridColumn("Ins", 40));
        gridDunning.Columns.Add(new GridColumn("Message", 150));
        gridDunning.Columns.Add(new GridColumn("Bold Message", 150));
        gridDunning.Columns.Add(new GridColumn("Email", 35, HorizontalAlignment.Center));

        if (PrefC.GetBool(PrefName.ShowFeatureSuperfamilies))
        {
            gridDunning.Columns.Add(new GridColumn("SF", 30, HorizontalAlignment.Center));
        }

        gridDunning.Columns.Add(new GridColumn("Clinic", 50));

        gridDunning.ListGridRows.Clear();

        foreach (var dunning in dunnings)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(dunning.BillingType == 0 ? "all" : Defs.GetName(DefCat.BillingTypes, dunning.BillingType));

            if (dunning.AgeAccount == 0)
            {
                gridRow.Cells.Add("any");
            }
            else
            {
                gridRow.Cells.Add("Over " + dunning.AgeAccount);
            }

            switch (dunning.InsIsPending)
            {
                case YN.Yes:
                    gridRow.Cells.Add("Y");
                    break;

                case YN.No:
                    gridRow.Cells.Add("N");
                    break;

                default:
                    gridRow.Cells.Add("any");
                    break;
            }

            gridRow.Cells.Add(dunning.DunMessage);
            gridRow.Cells.Add(new GridCell(dunning.MessageBold) {Bold = YN.Yes, ColorText = Color.DarkRed});
            gridRow.Cells.Add(!string.IsNullOrEmpty(dunning.EmailBody) || !string.IsNullOrEmpty(dunning.EmailSubject) ? "X" : "");

            if (PrefC.GetBool(PrefName.ShowFeatureSuperfamilies))
            {
                gridRow.Cells.Add(dunning.IsSuperFamily ? "X" : "");
            }

            gridRow.Cells.Add(dunning.ClinicNum == -2 ? "All Clinics" : Clinics.GetAbbr(dunning.ClinicNum));
            gridRow.Tag = dunning;

            gridDunning.ListGridRows.Add(gridRow);
        }

        gridDunning.EndUpdate();
    }

    private bool ValidateDunningFilters(Dunning dunning)
    {
        return (comboClinics.IsAllSelected || comboClinics.ClinicNumSelected == dunning.ClinicNum) && 
               (listBill.SelectedIndices.Contains(0) || listBill.SelectedIndices.Select(x => _billingTypeDefs[x - 1].DefNum).Contains(dunning.BillingType)) && 
               (radioAny.Checked || dunning.AgeAccount == (byte) (30 * new List<RadioButton> {radioAny, radio30, radio60, radio90}.FindIndex(x => x.Checked))) && 
               (string.IsNullOrWhiteSpace(textAdv.Text) || dunning.DaysInAdvance == SIn.Int(textAdv.Text, false)) && 
               (radioU.Checked || dunning.InsIsPending == (YN) new List<RadioButton> {radioU, radioY, radioN}.FindIndex(x => x.Checked));
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        if (_dunnings is null)
        {
            return;
        }

        FillGrids();
    }

    private void GridDunning_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formDunningEdit = new FormDunningEdit((Dunning) gridDunning.ListGridRows[e.Row].Tag);

        if (formDunningEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FillGrids(true);
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var dunning = new Dunning();

        long clinicNum = 0;
        if (!comboClinics.IsAllSelected)
        {
            clinicNum = comboClinics.ClinicNumSelected;
        }

        if (Security.CurUser.ClinicIsRestricted)
        {
            var userClinicNum = Clinics.GetForUserod(Security.CurUser, true).Select(x => x.Id).FirstOrDefault();
            
            clinicNum = userClinicNum;
        }

        dunning.ClinicNum = clinicNum;

        using var formDunningEdit = new FormDunningEdit(dunning);

        if (formDunningEdit.ShowDialog() == DialogResult.OK)
        {
            FillGrids(true);
        }
    }

    private void ButtonDuplicate_Click(object sender, EventArgs e)
    {
        if (!gridDunning.SelectedIndices.Any())
        {
            ShowError("Please select a message to duplicate first.");
            return;
        }

        var dunning = ((Dunning) gridDunning.ListGridRows[gridDunning.GetSelectedIndex()].Tag).Copy();

        Dunnings.Insert(dunning);

        FillGrids(true);

        gridDunning.SetSelected(gridDunning.ListGridRows.ToList().FindIndex(x => ((Dunning) x.Tag).DunningNum == dunning.DunningNum));
    }
}