using System;
using System.Windows.Forms;
using OpenDental.UI;

namespace OpenDental.Forms;

public partial class FormNotePick : FormODBase
{
    public string[] Notes { get; set; }
    public string SelectedNote { get; set; } = string.Empty;
    public bool UseTrojanImportDescription { get; set; }

    public FormNotePick()
    {
        InitializeComponent();

        if (!UseTrojanImportDescription)
        {
            return;
        }

        label1.Text = "Multiple versions of the note exist.  Please pick or edit one version to retain. You can also pick multiple rows to combine notes.";
        label2.Text = "This is the final note that will be used.";
    }

    private void FormNotePick_Load(object sender, EventArgs e)
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("", 630));

        gridMain.ListGridRows.Clear();

        foreach (var note in Notes)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(note);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
        if (Notes.Length == 0)
        {
            return;
        }

        gridMain.SetSelected(0);

        textNote.Text = Notes[0];
    }

    private void GridMain_CellClick(object sender, ODGridClickEventArgs e)
    {
        textNote.Text = "";

        foreach (var index in gridMain.SelectedIndices)
        {
            textNote.Text += Notes[index];
        }
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        SelectedNote = Notes[e.Row];

        DialogResult = DialogResult.OK;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        SelectedNote = textNote.Text;

        DialogResult = DialogResult.OK;
    }
}