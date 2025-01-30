using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormConfirmationSetup : FormODBase
{
    private List<Def> _apptConfirmedDefs;

    public FormConfirmationSetup()
    {
        InitializeComponent();
    }

    public void FormConfirmationSetup_Load(object sender, EventArgs e)
    {
        FillTabManualConfirmation();
    }

    private void FillTabManualConfirmation()
    {
        _apptConfirmedDefs = Defs.GetDefsForCategory(DefCat.ApptConfirmed, isShort: false);

        comboStatusEmailedConfirm.Items.AddDefs(_apptConfirmedDefs);
        comboStatusEmailedConfirm.SetSelectedDefNum(PrefC.GetLong(PrefName.ConfirmStatusEmailed));

        comboStatusTextMessagedConfirm.Items.AddDefs(_apptConfirmedDefs);
        comboStatusTextMessagedConfirm.SetSelectedDefNum(PrefC.GetLong(PrefName.ConfirmStatusTextMessaged));

        checkGroupFamilies.Checked = PrefC.GetBool(PrefName.ConfirmGroupByFamily);

        FillGrid();
    }

    private static GridRow MakeRow(PrefName prefName, string mode, string description)
    {
        var gridRow = new GridRow();

        gridRow.Cells.Add(mode);
        gridRow.Cells.Add(description);
        gridRow.Cells.Add(PrefC.GetString(prefName));
        gridRow.Tag = prefName;

        return gridRow;
    }

    private void FillGrid()
    {
        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Mode", 61));
        gridMain.Columns.Add(new GridColumn("", 300));
        gridMain.Columns.Add(new GridColumn("Message", 500));

        gridMain.ListGridRows.Clear();
        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmPostcardMessage,
            "Postcard", "Confirmation message. Available variables: [NameF], [date], [time]."));

        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmPostcardFamMessage,
            "Postcard", "For multiple patients in one family. Available variables: [FamilyApptList]."));

        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmEmailSubject,
            "E-mail", "Confirmation subject line."));

        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmEmailMessage,
            "E-mail", "Confirmation message. Available variables: [NameF], [date], [time]."));

        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmEmailFamMessage,
            "E-mail", "For multiple patients in one family. Available variables: [FamilyApptList]."));

        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmTextMessage,
            "Text", "Confirmation message. Available variables: [NameF], [date], [time]."));

        gridMain.ListGridRows.Add(MakeRow(PrefName.ConfirmTextFamMessage,
            "Text", "For multiple patients in one family. Available variables: [FamilyApptList]."));

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var prefName = (PrefName) gridMain.ListGridRows[e.Row].Tag;

        var frmRecallMessageEdit = new FrmRecallMessageEdit(prefName)
        {
            MessageVal = PrefC.GetString(prefName)
        };

        frmRecallMessageEdit.ShowDialog();

        if (!frmRecallMessageEdit.IsDialogOK)
        {
            return;
        }

        Prefs.UpdateString(prefName, frmRecallMessageEdit.MessageVal);

        FillGrid();
    }

    private void ButtonSetup_Click(object sender, EventArgs e)
    {
        using var formEServicesAutoMsging = new FormEServicesAutoMsging();

        formEServicesAutoMsging.ShowDialog();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        Prefs.UpdateLong(PrefName.ConfirmStatusEmailed, comboStatusEmailedConfirm.GetSelectedDefNum());
        Prefs.UpdateLong(PrefName.ConfirmStatusTextMessaged, comboStatusTextMessagedConfirm.GetSelectedDefNum());
        Prefs.UpdateBool(PrefName.ConfirmGroupByFamily, checkGroupFamilies.Checked);

        DataValid.SetInvalid(InvalidType.Prefs);

        DialogResult = DialogResult.OK;
    }
}