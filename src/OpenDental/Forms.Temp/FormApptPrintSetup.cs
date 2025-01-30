using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormApptPrintSetup : FormODBase
{
    private readonly List<long> _aptNums;
    private readonly DateTime _selectedDate;
    private readonly bool _weeklyView;

    public DateTime DateTimeApptPrintStart;
    public DateTime DateTimeApptPrintStop;
    public int ApptPrintFontSize;
    public int ApptPrintColsPerPage;
    public bool IsPrintPreview;
    public ApptPrintColorBehavior PrintColorBehavior;
    public bool IsLandscape;

    public FormApptPrintSetup(List<long> aptNums, DateTime selectedDate, bool weeklyView)
    {
        _aptNums = aptNums;
        _selectedDate = selectedDate;
        _weeklyView = weeklyView;

        InitializeComponent();
    }

    private void FormApptPrintSetup_Load(object sender, EventArgs e)
    {
        var timeStart = PrefC.GetDateT(PrefName.ApptPrintTimeStart).ToShortTimeString();
        var timeStop = PrefC.GetDateT(PrefName.ApptPrintTimeStop).ToShortTimeString();
        for (var i = 0; i <= 24; i++)
        {
            var timeSpan = new TimeSpan(i, 0, 0);

            comboStart.Items.Add(timeSpan.ToShortTimeString());
            comboStop.Items.Add(timeSpan.ToShortTimeString());

            if (timeSpan.ToShortTimeString() == timeStart)
            {
                comboStart.SelectedIndex = i;
            }

            if (timeSpan.ToShortTimeString() == timeStop)
            {
                comboStop.SelectedIndex = i;
            }
        }

        IsLandscape = PrefC.GetBool(PrefName.ApptPrintIsLandscape);

        radioLandscape.Checked = IsLandscape;
        radioPortrait.Checked = !IsLandscape;

        textFontSize.Text = PrefC.GetString(PrefName.ApptPrintFontSize);
        textColumnsPerPage.Text = PrefC.GetInt(PrefName.ApptPrintColumnsPerPage).ToString();
        PrintColorBehavior = PrefC.GetEnum<ApptPrintColorBehavior>(PrefName.ApptPrintColorBehavior);
        radioFullColor.Checked = true;

        switch (PrintColorBehavior)
        {
            case ApptPrintColorBehavior.LessColor:
                radioLessColor.Checked = true;
                break;

            case ApptPrintColorBehavior.Grayscale:
                radioGrayscale.Checked = true;
                break;
        }

        if (Clinics.ClinicNum == 0)
        {
            groupBoxPrintRouting.Enabled = false;
        }

        if (_weeklyView)
        {
            groupBoxPrintRouting.Enabled = false;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!ValidEntries())
        {
            return;
        }

        SaveChanges(false);
    }

    private bool ValidEntries()
    {
        var from = SIn.DateTime(comboStart.GetSelected<string>());
        var to = SIn.DateTime(comboStop.GetSelected<string>());

        if (from.Minute > 0 || to.Minute > 0)
        {
            ShowError("Please use hours only, no minutes.");
            return false;
        }

        if (to.Hour == from.Hour && (to.Hour != 0 && from.Hour != 0))
        {
            ShowError("Start time must be different than stop time.");
            return false;
        }

        if (to.Hour != 0 && to.Hour < from.Hour)
        {
            ShowError("Start time cannot exceed stop time.");
            return false;
        }

        if (from == DateTime.MinValue)
        {
            ShowError("Please enter a valid start time.");
            return false;
        }

        if (to == DateTime.MinValue)
        {
            ShowError("Please enter a valid stop time.");
            return false;
        }

        if (!textColumnsPerPage.IsValid() || !textFontSize.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return false;
        }

        if (SIn.Int(textColumnsPerPage.Text) >= 1)
        {
            return true;
        }

        ShowError("Columns per page cannot be 0 or less.");
        return false;
    }

    private void SaveChanges(bool suppressMessage)
    {
        if (!ValidEntries())
        {
            return;
        }

        Prefs.UpdateDateT(PrefName.ApptPrintTimeStart, SIn.DateTime(comboStart.GetSelected<string>()));
        Prefs.UpdateDateT(PrefName.ApptPrintTimeStop, SIn.DateTime(comboStop.GetSelected<string>()));
        Prefs.UpdateString(PrefName.ApptPrintFontSize, textFontSize.Text);
        Prefs.UpdateInt(PrefName.ApptPrintColumnsPerPage, SIn.Int(textColumnsPerPage.Text));

        IsLandscape = radioLandscape.Checked;

        Prefs.UpdateBool(PrefName.ApptPrintIsLandscape, IsLandscape);

        if (radioFullColor.Checked)
        {
            Prefs.UpdateInt(PrefName.ApptPrintColorBehavior, (int) ApptPrintColorBehavior.FullColor);
            PrintColorBehavior = ApptPrintColorBehavior.FullColor;
        }
        else if (radioLessColor.Checked)
        {
            Prefs.UpdateInt(PrefName.ApptPrintColorBehavior, (int) ApptPrintColorBehavior.LessColor);
            PrintColorBehavior = ApptPrintColorBehavior.LessColor;
        }
        else
        {
            Prefs.UpdateInt(PrefName.ApptPrintColorBehavior, (int) ApptPrintColorBehavior.Grayscale);
            PrintColorBehavior = ApptPrintColorBehavior.Grayscale;
        }

        if (!suppressMessage)
        {
            ShowInfo("Settings saved.");
        }
    }

    private bool PrintViewSetup()
    {
        var changed = false;

        if (!ValidEntries())
        {
            return false;
        }

        if (SIn.DateTime(comboStart.GetSelected<string>()).Hour != PrefC.GetDateT(PrefName.ApptPrintTimeStart).Hour ||
            SIn.DateTime(comboStop.GetSelected<string>()).Hour != PrefC.GetDateT(PrefName.ApptPrintTimeStop).Hour ||
            textFontSize.Text != PrefC.GetString(PrefName.ApptPrintFontSize) ||
            textColumnsPerPage.Text != PrefC.GetInt(PrefName.ApptPrintColumnsPerPage).ToString())
        {
            changed = true;
        }

        switch (PrintColorBehavior)
        {
            case ApptPrintColorBehavior.FullColor when !radioFullColor.Checked:
            case ApptPrintColorBehavior.LessColor when !radioLessColor.Checked:
            case ApptPrintColorBehavior.Grayscale when !radioGrayscale.Checked:
                changed = true;
                break;
        }

        if (IsLandscape != radioLandscape.Checked)
        {
            changed = true;
        }

        if (changed)
        {
            if (Confirm("Save the changes that were made?"))
            {
                SaveChanges(true);
            }
        }

        DateTimeApptPrintStart = SIn.DateTime(comboStart.GetSelected<string>());
        DateTimeApptPrintStop = SIn.DateTime(comboStop.GetSelected<string>());
        ApptPrintFontSize = SIn.Int(textFontSize.Text);
        ApptPrintColsPerPage = SIn.Int(textColumnsPerPage.Text);
        IsLandscape = radioLandscape.Checked;
        PrintColorBehavior = ApptPrintColorBehavior.FullColor;

        if (radioLessColor.Checked)
        {
            PrintColorBehavior = ApptPrintColorBehavior.LessColor;
        }
        else if (radioGrayscale.Checked)
        {
            PrintColorBehavior = ApptPrintColorBehavior.Grayscale;
        }

        return true;
    }

    private void ButtonPreview_Click(object sender, EventArgs e)
    {
        if (!PrintViewSetup())
        {
            return;
        }

        IsPrintPreview = true;
        DialogResult = DialogResult.OK;
    }

    private void ButtonAllForDay_Click(object sender, EventArgs e)
    {
        using var formRpRouting = new FormRpRouting();

        formRpRouting.DateSelected = _selectedDate;
        formRpRouting.IsAutoRunForDateSelected = true;
        formRpRouting.ShowDialog();
    }

    private void ButtonCurrentView_Click(object sender, EventArgs e)
    {
        using var formRpRouting = new FormRpRouting();

        formRpRouting.ListAptNums = _aptNums;
        formRpRouting.IsAutoRunForListAptNums = true;
        formRpRouting.ShowDialog();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (!PrintViewSetup())
        {
            return;
        }

        IsPrintPreview = false;
        DialogResult = DialogResult.OK;
    }
}