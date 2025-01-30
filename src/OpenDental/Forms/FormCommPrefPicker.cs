using System;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCommPrefPicker : FormODBase
{
    public ContactMethod ContactMethodCur;

    public FormCommPrefPicker()
    {
        InitializeComponent();
    }

    private void FormCommPrefPicker_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Contact Preference", 120, HorizontalAlignment.Center));

        gridMain.ListGridRows.Clear();
        for (var i = 0; i < Enum.GetNames(typeof(ContactMethod)).Length; i++)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(Enum.GetNames(typeof(ContactMethod))[i]);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        ContactMethodCur = (ContactMethod) e.Row;
        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridMain.GetSelectedIndex() == -1)
        {
            ShowError("Please select a communication preference first.");
            return;
        }

        ContactMethodCur = (ContactMethod) gridMain.GetSelectedIndex();
        DialogResult = DialogResult.OK;
    }
}