using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

/// <summary></summary>
public partial class FormLanguagesUsed : FormODBase
{
    private List<CultureInfo> _cultureInfos;
    private List<string> _langsUsed;
    
    public FormLanguagesUsed()
    {
        InitializeComponent();
    }

    private void FormLanguagesUsed_Load(object sender, EventArgs e)
    {
        _cultureInfos = CultureInfo.GetCultures(CultureTypes.NeutralCultures).OrderBy(x => x.DisplayName).ToList();
        
        listAvailable.Items.AddStrings(_cultureInfos.Select(x => x.DisplayName));
        if (PrefC.GetString(PrefName.LanguagesUsedByPatients) == "")
        {
            _langsUsed = [];
            
            FillListUsed();
            
            return;
        }

        _langsUsed = new List<string>(PrefC.GetString(PrefName.LanguagesUsedByPatients).Split(','));
        
        FillListUsed();
    }

    private void FillListUsed()
    {
        listUsed.Items.Clear();
        
        foreach (var language in _langsUsed)
        {
            if (language == "")
            {
                continue;
            }

            var cultureInfo = MiscUtils.GetCultureFromThreeLetter(language);
            if (cultureInfo is null)
            {
                listUsed.Items.Add(language);
                continue;
            }

            listUsed.Items.Add(cultureInfo.DisplayName);
        }

        FillComboLanguagesIndicateNone();
    }

    private void FillComboLanguagesIndicateNone()
    {
        comboLanguagesIndicateNone.Items.Clear();
        foreach (var language in _langsUsed)
        {
            if (language == "")
            {
                continue;
            }

            var cultureInfo = MiscUtils.GetCultureFromThreeLetter(language);
            if (cultureInfo is not null)
            {
                continue;
            }
            
            comboLanguagesIndicateNone.Items.Add(language);
            if (language == PrefC.GetString(PrefName.LanguagesIndicateNone))
            {
                comboLanguagesIndicateNone.SelectedIndex = comboLanguagesIndicateNone.Items.Count - 1;
            }
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (listAvailable.SelectedIndex == -1)
        {
            ShowError("Please select a language first");
            return;
        }

        var lang = _cultureInfos[listAvailable.SelectedIndex].ThreeLetterISOLanguageName;
        if (_langsUsed.Contains(lang))
        {
            ShowError("Language already added.");
            return;
        }

        _langsUsed.Add(lang);
        
        FillListUsed();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (listUsed.SelectedIndex == -1)
        {
            ShowError("Please select a language first");
            return;
        }

        var rules = ApptReminderRules.GetAll().FindAll(x => x.Language != string.Empty).Select(x => x.Language).ToList();
        if (rules.Contains(_langsUsed[listUsed.SelectedIndex]))
        {
            ShowError("Language is in use by:\r\n - eService reminders or confirmations");
            return;
        }

        _langsUsed.RemoveAt(listUsed.SelectedIndex);
        
        FillListUsed();
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        switch (listUsed.SelectedIndex)
        {
            case -1:
                ShowError("Please select a language first");
                return;
            
            case 0:
                return;
        }

        var newIndex = listUsed.SelectedIndex - 1;
        
        _langsUsed.Reverse(listUsed.SelectedIndex - 1, 2);
        
        FillListUsed();
        
        listUsed.SetSelected(newIndex);
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        if (listUsed.SelectedIndex == -1)
        {
            ShowError("Please select a language first");
            return;
        }

        if (listUsed.SelectedIndex == listUsed.Items.Count - 1)
        {
            return;
        }

        var newIndex = listUsed.SelectedIndex + 1;
        
        _langsUsed.Reverse(listUsed.SelectedIndex, 2);
        
        FillListUsed();
        
        listUsed.SetSelected(newIndex);
    }

    private void ButtonAddCustom_Click(object sender, EventArgs e)
    {
        if (textCustom.Text == "")
        {
            ShowError("Please enter a custom language first");
            return;
        }

        var lang = textCustom.Text;
        if (_langsUsed.Contains(lang))
        {
            ShowError("Language already added.");
            return;
        }

        _langsUsed.Add(lang);
        
        textCustom.Clear();
        
        FillListUsed();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var str = "";
        for (var i = 0; i < _langsUsed.Count; i++)
        {
            if (i > 0)
            {
                str += ",";
            }

            str += _langsUsed[i];
        }

        Prefs.UpdateString(PrefName.LanguagesUsedByPatients, str);
        Prefs.UpdateString(PrefName.LanguagesIndicateNone, comboLanguagesIndicateNone.SelectedIndex == -1 ? "" : comboLanguagesIndicateNone.SelectedItem.ToString());

        DialogResult = DialogResult.OK;
    }

    private void FormLanguagesUsed_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!PrefC.GetString(PrefName.LanguagesUsedByPatients).Contains(PrefC.GetString(PrefName.LanguagesIndicateNone)))
        {
            Prefs.UpdateString(PrefName.LanguagesIndicateNone, "");
        }
    }
}