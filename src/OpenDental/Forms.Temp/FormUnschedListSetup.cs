using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormUnschedListSetup : FormODBase
{
    public FormUnschedListSetup()
    {
        InitializeComponent();
    }

    private void FormUnschedListSetup_Load(object sender, EventArgs e)
    {
        var daysPast = PrefC.GetInt(PrefName.UnschedDaysPast);
        if (daysPast != -1)
        {
            textDaysPast.Text = daysPast.ToString();
        }

        var daysFuture = PrefC.GetInt(PrefName.UnschedDaysFuture);
        if (daysFuture != -1)
        {
            textDaysFuture.Text = daysFuture.ToString();
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var unschedDaysPastValue = -1;
        var unschedDaysFutureValue = -1;

        if ((textDaysPast.Text != "" && !int.TryParse(textDaysPast.Text, out unschedDaysPastValue)) ||
            (textDaysFuture.Text != "" && !int.TryParse(textDaysFuture.Text, out unschedDaysFutureValue)))
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        var changed =
            Prefs.UpdateInt(PrefName.UnschedDaysPast, unschedDaysPastValue) |
            Prefs.UpdateInt(PrefName.UnschedDaysFuture, unschedDaysFutureValue);

        if (changed)
        {
            DataValid.SetInvalid(InvalidType.Prefs);
        }

        DialogResult = DialogResult.OK;
    }
}