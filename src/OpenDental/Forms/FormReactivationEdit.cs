using System;
using System.Globalization;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormReactivationEdit : FormODBase
{
    public Reactivation ReactivationCur;

    public FormReactivationEdit(long patNum)
    {
        var reactivation = new Reactivation
        {
            PatNum = patNum,
            IsNew = true
        };

        InitializeComponent();

        ReactivationCur = reactivation;
    }

    public FormReactivationEdit(Reactivation reactivation)
    {
        InitializeComponent();

        ReactivationCur = reactivation;
    }

    private void FormReactivationEdit_Load(object sender, EventArgs e)
    {
        textPatName.Text = Patients.GetNameFL(ReactivationCur.PatNum);

        var lastContactedAt = Reactivations.GetDateLastContacted(ReactivationCur.PatNum);

        textDateLastContacted.Text = lastContactedAt == DateTime.MinValue ? "" : lastContactedAt.ToString(CultureInfo.InvariantCulture);

        comboStatus.Items.AddDefNone();
        comboStatus.Items.AddDefs(Defs.GetDefsForCategory(DefCat.RecallUnschedStatus, isShort: true));
        comboStatus.SetSelectedDefNum(ReactivationCur.ReactivationStatus);

        checkBoxDNC.Checked = ReactivationCur.DoNotContact;

        textNote.Text = ReactivationCur.ReactivationNote;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (!Confirm("Delete this Reactivation?"))
        {
            return;
        }

        if (ReactivationCur.ReactivationNum != 0)
        {
            Reactivations.Delete(ReactivationCur.ReactivationNum);
        }

        ReactivationCur = null;

        DialogResult = DialogResult.Abort;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var selectedStatusDef = comboStatus.GetSelected<Def>();
        var statusChanged = ReactivationCur.ReactivationStatus != (selectedStatusDef?.DefNum ?? 0);

        ReactivationCur.ReactivationStatus = selectedStatusDef?.DefNum ?? 0;
        ReactivationCur.ReactivationNote = textNote.Text;
        ReactivationCur.DoNotContact = checkBoxDNC.Checked;

        if (ReactivationCur.ReactivationNum == 0)
        {
            Reactivations.Insert(ReactivationCur);
        }
        else
        {
            Reactivations.Update(ReactivationCur);
        }

        if (statusChanged)
        {
            DialogResult = DialogResult.Yes;

            return;
        }

        DialogResult = DialogResult.OK;
    }
}