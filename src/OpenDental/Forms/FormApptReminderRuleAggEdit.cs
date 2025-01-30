using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using TabPage = OpenDental.UI.TabPage;

namespace OpenDental.Forms;

public partial class FormApptReminderRuleAggEdit : FormODBase
{
    private readonly ApptReminderRule _apptReminderRule;
    private readonly List<ApptReminderRule> _apptReminderRulesNonDefault;
    private readonly string _selectedLanguageLoading;

    public FormApptReminderRuleAggEdit(ApptReminderRule apptReminderRule, List<ApptReminderRule> apptReminderRules, string selectedLanguageLoading)
    {
        _apptReminderRule = apptReminderRule;
        _apptReminderRulesNonDefault = apptReminderRules;
        _selectedLanguageLoading = selectedLanguageLoading;

        InitializeComponent();
    }

    private void FormApptReminderRuleEdit_Load(object sender, EventArgs e)
    {
        var userControlReminderAgg = new UserControlReminderAgg(_apptReminderRule);

        userControlReminderAgg.Dock = DockStyle.Fill;

        if (_apptReminderRulesNonDefault.Count == 0)
        {
            tabControl1.Visible = false;
            panelMain.Visible = true;

            userControlReminderAgg.Controls.Add(panelMain);
        }
        else
        {
            userControlReminderAgg.Controls.Add(tabPageDefault);
        }

        foreach (var apptReminderRule in _apptReminderRulesNonDefault)
        {
            var tabPageLanguage = new TabPage();

            var cultureInfo = MiscUtils.GetCultureFromThreeLetter(apptReminderRule.Language);

            tabPageLanguage.Text = cultureInfo == null ? apptReminderRule.Language : cultureInfo.DisplayName;

            tabControl1.TabPages.Add(tabPageLanguage);

            var userControlReminderAggLang = new UserControlReminderAgg(apptReminderRule);

            userControlReminderAggLang.Dock = DockStyle.Fill;
            userControlReminderAggLang.Controls.Add(tabPageLanguage);

            if (apptReminderRule.Language == _selectedLanguageLoading)
            {
                tabControl1.SelectedTab = tabPageLanguage;
            }
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (_apptReminderRulesNonDefault.Count == 0)
        {
            var errors = UIHelper.GetAllControls(this).OfType<UserControlReminderAgg>().First().ValidateTemplates();
            if (errors.Count != 0)
            {
                ShowError("You must fix the following errors before continuing.\r\n\r\n-" + string.Join("\r\n-", errors));
                return;
            }

            UIHelper.GetAllControls(this).OfType<UserControlReminderAgg>().First().SaveControlTemplates();
        }
        else
        {
            foreach (var tabPage in tabControl1.TabPages)
            {
                var userControlReminderAgg = (UserControlReminderAgg) tabPage.Controls[0];

                var errors = userControlReminderAgg.ValidateTemplates();
                if (errors.Count != 0)
                {
                    ShowError("You must fix the following errors before continuing.\r\n\r\n-" + string.Join("\r\n-", errors));
                    return;
                }

                userControlReminderAgg.SaveControlTemplates();
            }
        }

        DialogResult = DialogResult.OK;
    }
}