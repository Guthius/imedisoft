using System;
using System.Globalization;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormUpdateSetup : FormODBase
{
    private DateTime _dateTimeUpdate;

    public FormUpdateSetup()
    {
        InitializeComponent();
    }

    private void FormUpdateSetup_Load(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.SecurityAdmin, true))
        {
            buttonSave.Enabled = false;
        }

        textUpdateServerAddress.Text = PrefC.GetString(PrefName.UpdateServerAddress);
        textWebsitePath.Text = PrefC.GetString(PrefName.UpdateWebsitePath);

        _dateTimeUpdate = PrefC.GetDateT(PrefName.UpdateDateTime);

        textUpdateTime.Text = _dateTimeUpdate.ToString(CultureInfo.InvariantCulture);
    }

    private void ButtonChangeTime_Click(object sender, EventArgs e)
    {
        using var formTimePick = new FormTimePick(true);

        if (_dateTimeUpdate != DateTime.MinValue)
        {
            formTimePick.DateTimeSelected = _dateTimeUpdate;
        }

        if (formTimePick.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _dateTimeUpdate = formTimePick.DateTimeSelected;

        textUpdateTime.Text = _dateTimeUpdate.ToString(CultureInfo.InvariantCulture);

        if (!Prefs.UpdateDateT(PrefName.UpdateDateTime, _dateTimeUpdate))
        {
            return;
        }

        Cursor = Cursors.WaitCursor;

        DataValid.SetInvalid(InvalidType.Prefs);

        Cursor = Cursors.Default;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var changed = false;

        changed |= Prefs.UpdateString(PrefName.UpdateServerAddress, textUpdateServerAddress.Text);
        changed |= Prefs.UpdateString(PrefName.UpdateWebsitePath, textWebsitePath.Text);

        if (changed)
        {
            Cursor = Cursors.WaitCursor;

            DataValid.SetInvalid(InvalidType.Prefs);

            Cursor = Cursors.Default;
        }

        DialogResult = DialogResult.OK;
    }

    private void FormUpdateSetup_FormClosing(object sender, FormClosingEventArgs e)
    {
        var perm = DialogResult == DialogResult.OK
            ? EnumPermType.SecurityAdmin
            : EnumPermType.Setup;

        SecurityLogs.MakeLogEntry(perm, 0, "Update Setup window accesssed.");
    }
}