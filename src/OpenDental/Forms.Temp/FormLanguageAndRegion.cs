using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormLanguageAndRegion : FormODBase
{
    private List<CultureInfo> _cultureInfos;

    public FormLanguageAndRegion()
    {
        InitializeComponent();
    }

    private void FormLanguageAndRegion_Load(object sender, EventArgs e)
    {
        var cultureInfo = PrefC.GetLanguageAndRegion();
        
        _cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x => !x.IsNeutralCulture)
            .OrderBy(x => x.DisplayName)
            .ToList();
        
        textLARLocal.Text = CultureInfo.CurrentCulture.DisplayName;
        textLARDB.Text = PrefC.GetString(PrefName.LanguageAndRegion) == "" ? "None" : cultureInfo.DisplayName;

        comboLanguageAndRegion.Items.Clear();
        foreach (var info in _cultureInfos)
        {
            comboLanguageAndRegion.Items.Add(info.DisplayName);
        }

        comboLanguageAndRegion.SelectedIndex = _cultureInfos.FindIndex(x => x.DisplayName == cultureInfo.DisplayName);
        checkNoShow.Checked = ComputerPrefs.LocalComputer.NoShowLanguage;

        if (Security.IsAuthorized(EnumPermType.Setup, true))
        {
            return;
        }
        
        comboLanguageAndRegion.Visible = false;
        labelNewLAR.Visible = false;
        butSave.Enabled = false;
        checkNoShow.Enabled = false;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (comboLanguageAndRegion.SelectedIndex == -1)
        {
            ShowError("Select a language and region.");
            return;
        }

        if (!Security.IsAuthorized(EnumPermType.Setup, true))
        {
            DialogResult = DialogResult.OK;
            return;
        }

        if (Prefs.UpdateString(PrefName.LanguageAndRegion, _cultureInfos[comboLanguageAndRegion.SelectedIndex].Name))
        {
            ShowError("Program must be restarted for changes to take full effect.");
        }

        ComputerPrefs.LocalComputer.NoShowLanguage = checkNoShow.Checked;
        ComputerPrefs.Update(ComputerPrefs.LocalComputer);
        
        DialogResult = DialogResult.OK;
    }
}