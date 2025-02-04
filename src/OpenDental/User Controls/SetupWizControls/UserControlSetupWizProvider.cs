using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizProvider : SetupWizControl
{
    private int _blink;

    public UserControlSetupWizProvider()
    {
        InitializeComponent();
    }

    private void UserControlSetupWizProvider_Load(object sender, EventArgs e)
    {
        FillGrid();

        if (Providers.GetWhere(x => x.FirstName.ToLower() != "default", true).ToList().Count != 0)
        {
            return;
        }

        MsgBox.Show("FormSetupWizard", "You have no valid providers. Please click the add button to Add a provider or Add information to the default providers.");

        timer1.Start();
    }

    private void FillGrid()
    {
        var listProvs = Providers.GetDeepCopy(true);
        var needsAttnCol = OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("First Name", 90));
        gridMain.Columns.Add(new GridColumn("Last Name", 90));
        gridMain.Columns.Add(new GridColumn("Abbrev", 70));
        gridMain.Columns.Add(new GridColumn("Suffix", 60));
        gridMain.Columns.Add(new GridColumn("SSN/TIN", 130));
        gridMain.Columns.Add(new GridColumn("NPI", 130));
        gridMain.Columns.Add(new GridColumn("AptColor", 60));
        gridMain.Columns.Add(new GridColumn("LineColor", 60));
        gridMain.Columns.Add(new GridColumn("IsHyg", 60, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();

        var complete = listProvs.Where(x => x.FirstName.ToLower() != "default").ToList().Count != 0;

        foreach (var prov in listProvs)
        {
            var row = new GridRow();
            var isDentist = OpenDental.SetupWizard.ProvSetup.IsPrimary(prov);
            var isHyg = prov.IsSecondary;

            row.Cells.Add(prov.FirstName);

            if ((isDentist || isHyg) && (string.IsNullOrEmpty(prov.FirstName) || prov.FirstName.ToLower() == "default"))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                complete = false;
            }

            row.Cells.Add(prov.LastName);
            if ((isDentist || isHyg) && string.IsNullOrEmpty(prov.LastName))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                complete = false;
            }

            row.Cells.Add(prov.Abbr);
            if ((isDentist || isHyg) && string.IsNullOrEmpty(prov.Abbr))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                complete = false;
            }

            row.Cells.Add(prov.Suffix);
            if (isDentist && string.IsNullOrEmpty(prov.Suffix))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                complete = false;
            }

            row.Cells.Add(prov.Ssn);
            if (isDentist && string.IsNullOrEmpty(prov.Ssn))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                complete = false;
            }

            row.Cells.Add(prov.NationalProviderId);
            if (isDentist && string.IsNullOrEmpty(prov.NationalProviderId))
            {
                row.Cells[row.Cells.Count - 1].ColorBackG = needsAttnCol;
                complete = false;
            }

            row.Cells.Add("");
            row.Cells[row.Cells.Count - 1].ColorBackG = ColorTranslator.FromHtml(prov.Color);
            //not required
            row.Cells.Add("");
            row.Cells[row.Cells.Count - 1].ColorBackG = ColorTranslator.FromHtml(prov.OutlineColor);
            //not required
            row.Cells.Add(prov.IsSecondary ? "X" : "");
            //not required
            row.Tag = prov;
            gridMain.ListGridRows.Add(row);
        }

        gridMain.EndUpdate();
        IsDone = complete;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (_blink > 5)
        {
            pictureAdd.Visible = true;

            foreach (var rowCur in gridMain.ListGridRows)
            {
                rowCur.ColorBackG = OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention);
            }

            gridMain.Invalidate();
            timer1.Stop();
            return;
        }

        pictureAdd.Visible = !pictureAdd.Visible;

        foreach (var rowCur in gridMain.ListGridRows)
        {
            rowCur.ColorBackG = rowCur.ColorBackG == Color.White ? OpenDental.SetupWizard.GetColor(ODSetupStatus.NeedsAttention) : Color.White;
        }

        gridMain.Invalidate();

        _blink++;
    }

    private void gridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ProviderEdit))
        {
            return;
        }

        var providerDto = (ProviderDto) gridMain.ListGridRows[e.Row].Tag;

        using var formProvEdit = new FormProvEdit(providerDto);

        if (formProvEdit.ShowDialog() == DialogResult.OK)
        {
            FillGrid();
        }
    }

    private void butAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.ProviderAdd))
        {
            return;
        }

        var providerDto = new ProviderDto();

        using var formProvEdit = new FormProvEdit(providerDto);

        formProvEdit.IsNew = true;

        if (formProvEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        Cache.Refresh(InvalidType.Providers);

        FillGrid();
    }

    private void butAdvanced_Click(object sender, EventArgs e)
    {
        using var formProviderSetup = new FormProviderSetup();

        formProviderSetup.ShowDialog();

        FillGrid();
    }
}