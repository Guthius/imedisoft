using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormAsapSetup : FormODBase
{
    public FormAsapSetup()
    {
        InitializeComponent();
    }

    private void FormAsapSetup_Load(object sender, EventArgs e)
    {
        comboClinic.ClinicNumSelected = Clinics.ClinicNum;
        
        FillPrefs();
    }

    private void FillPrefs()
    {
        gridMain.BeginUpdate();
        
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Type", 100));
        gridMain.Columns.Add(new GridColumn("", 250));
        gridMain.Columns.Add(new GridColumn("Template", 500));
        
        gridMain.ListGridRows.Clear();
        
        checkUseDefaults.Checked = true;
        
        const string baseVars = "Available variables: [NameF], [Date], [Time], [OfficeName], [OfficePhone]";

        gridMain.ListGridRows.Add(BuildRowForTemplate(PrefName.ASAPTextTemplate, "Text manual", baseVars));
        gridMain.ListGridRows.Add(BuildRowForTemplate(PrefName.WebSchedAsapTextTemplate, "Web Sched Text", baseVars + ", [AsapURL]"));
        gridMain.ListGridRows.Add(BuildRowForTemplate(PrefName.WebSchedAsapEmailTemplate, "Web Sched Email Body", baseVars + ", [AsapURL]"));
        gridMain.ListGridRows.Add(BuildRowForTemplate(PrefName.WebSchedAsapEmailSubj, "Web Sched Email Subject", baseVars));
        gridMain.EndUpdate();
        
        if (comboClinic.ClinicNumSelected == 0)
        {
            textWebSchedPerDay.Text = PrefC.GetString(PrefName.WebSchedAsapTextLimit);
            checkAsapPromptEnabled.Checked = PrefC.GetBool(PrefName.AsapPromptEnabled);
            checkUseDefaults.Checked = false;
            return;
        }

        var clinicPref = ClinicPrefs.GetPref(PrefName.WebSchedAsapTextLimit, comboClinic.ClinicNumSelected);
        if (clinicPref?.ValueString == null)
        {
            textWebSchedPerDay.Text = PrefC.GetString(PrefName.WebSchedAsapTextLimit);
        }
        else
        {
            textWebSchedPerDay.Text = clinicPref.ValueString;
            checkUseDefaults.Checked = false;
        }

        clinicPref = ClinicPrefs.GetPref(PrefName.AsapPromptEnabled, comboClinic.ClinicNumSelected);
        if (clinicPref?.ValueString == null)
        {
            checkAsapPromptEnabled.Checked = PrefC.GetBool(PrefName.AsapPromptEnabled);
        }
        else
        {
            checkAsapPromptEnabled.Checked = SIn.Bool(clinicPref.ValueString);
            checkUseDefaults.Checked = false;
        }
    }
    
    private GridRow BuildRowForTemplate(PrefName prefName, string templateName, string availableVars)
    {
        string templateText;
        
        var showDefault = false;
        if (comboClinic.ClinicNumSelected == 0)
        {
            templateText = PrefC.GetString(prefName);
            checkUseDefaults.Checked = false;
        }
        else
        {
            var clinicPref = ClinicPrefs.GetPref(prefName, comboClinic.ClinicNumSelected);
            if (clinicPref?.ValueString == null)
            {
                templateText = PrefC.GetString(prefName);
                showDefault = true;
            }
            else
            {
                templateText = clinicPref.ValueString;
                checkUseDefaults.Checked = false;
            }
        }

        var gridRow = new GridRow();
        
        gridRow.Cells.Add(templateName + (showDefault ? " (Default)" : ""));
        gridRow.Cells.Add(availableVars);
        gridRow.Cells.Add(templateText);
        gridRow.Tag = prefName;
        
        return gridRow;
    }
    
    private void ComboBoxClinic_SelectedIndexChanged(object sender, EventArgs e)
    {
        checkUseDefaults.Visible = comboClinic.ClinicNumSelected != 0;

        FillPrefs();
    }

    private void CheckBoxUseDefaults_Click(object sender, EventArgs e)
    {
        var prefNames = new List<PrefName>
        {
            PrefName.ASAPTextTemplate,
            PrefName.WebSchedAsapTextTemplate,
            PrefName.WebSchedAsapEmailTemplate,
            PrefName.WebSchedAsapEmailSubj,
            PrefName.WebSchedAsapTextLimit,
            PrefName.AsapPromptEnabled
        };
        
        if (checkUseDefaults.Checked)
        {
            if (Confirm("Delete custom templates for this clinic and switch to using defaults? This cannot be undone."))
            {
                ClinicPrefs.DeletePrefs(comboClinic.ClinicNumSelected, prefNames);
                
                DataValid.SetInvalid(InvalidType.ClinicPrefs);
            }
            else
            {
                checkUseDefaults.Checked = false;
            }
        }
        else
        {
            var changed = false;
            
            foreach (var prefName in prefNames)
            {
                if (ClinicPrefs.Upsert(prefName, comboClinic.ClinicNumSelected, PrefC.GetString(prefName)))
                {
                    changed = true;
                }
            }

            if (changed)
            {
                DataValid.SetInvalid(InvalidType.ClinicPrefs);
            }
        }

        FillPrefs();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var prefName = (PrefName) gridMain.ListGridRows[e.Row].Tag;
        var curPrefValue = GetClinicPrefValue(prefName);
        var emailType = SIn.Enum<EmailType>(GetClinicPrefValue(PrefName.WebSchedAsapEmailTemplateType));
        string newPrefValue;
        
        var isHtmlTemplate = prefName == PrefName.WebSchedAsapEmailTemplate;
        if (isHtmlTemplate)
        {
            using var formEmailEdit = new FormEmailEdit
            {
                MarkupText = curPrefValue,
                DoCheckForDisclaimer = true,
                IsRawAllowed = true,
                IsRaw = emailType == EmailType.RawHtml,
            };

            if (formEmailEdit.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            emailType = EmailType.Html;
            if (formEmailEdit.IsRaw)
            {
                emailType = EmailType.RawHtml;
            }

            newPrefValue = formEmailEdit.MarkupText;
        }
        else
        {
            var frmRecallMessageEdit = new FrmRecallMessageEdit(prefName)
            {
                MessageVal = curPrefValue
            };
            
            frmRecallMessageEdit.ShowDialog();
            
            if (!frmRecallMessageEdit.IsDialogOK)
            {
                return;
            }

            newPrefValue = frmRecallMessageEdit.MessageVal;
        }

        if (comboClinic.ClinicNumSelected == 0)
        {
            if (Prefs.UpdateString(prefName, newPrefValue) | Prefs.UpdateInt(PrefName.WebSchedAsapEmailTemplateType, (int) emailType))
            {
                DataValid.SetInvalid(InvalidType.Prefs);
            }
        }
        else
        {
            if (ClinicPrefs.Upsert(prefName, comboClinic.ClinicNumSelected, newPrefValue) | 
                ClinicPrefs.Upsert(PrefName.WebSchedAsapEmailTemplate, comboClinic.ClinicNumSelected, ((int) emailType).ToString()))
            {
                DataValid.SetInvalid(InvalidType.ClinicPrefs);
            }
        }

        FillPrefs();
    }

    private string GetClinicPrefValue(PrefName prefName)
    {
        if (comboClinic.ClinicNumSelected == 0)
        {
            return PrefC.GetString(prefName);
        }

        var clinicPref = ClinicPrefs.GetPref(prefName, comboClinic.ClinicNumSelected);
        if (clinicPref is null || string.IsNullOrEmpty(clinicPref.ValueString))
        {
            return PrefC.GetString(prefName);
        }

        return clinicPref.ValueString;
    }

    private void TextBoxWebSchedPerDaySave()
    {
        if (!textWebSchedPerDay.IsValid())
        {
            return;
        }

        if (comboClinic.ClinicNumSelected == 0)
        {
            if (Prefs.UpdateString(PrefName.WebSchedAsapTextLimit, textWebSchedPerDay.Text))
            {
                DataValid.SetInvalid(InvalidType.Prefs);
            }
        }
        else
        {
            if (ClinicPrefs.Upsert(PrefName.WebSchedAsapTextLimit, comboClinic.ClinicNumSelected, textWebSchedPerDay.Text))
            {
                DataValid.SetInvalid(InvalidType.ClinicPrefs);
            }
        }
    }

    private void CheckBoxAsapPromptEnabled_Click(object sender, EventArgs e)
    {
        if (comboClinic.ClinicNumSelected == 0)
        {
            if (Prefs.UpdateBool(PrefName.AsapPromptEnabled, checkAsapPromptEnabled.Checked))
            {
                DataValid.SetInvalid(InvalidType.Prefs);
            }
        }
        else
        {
            if (ClinicPrefs.Upsert(PrefName.AsapPromptEnabled, comboClinic.ClinicNumSelected, SOut.Bool(checkAsapPromptEnabled.Checked)))
            {
                DataValid.SetInvalid(InvalidType.ClinicPrefs);
            }
        }
    }

    private void TextBoxWebSchedPerDay_Validating(object sender, CancelEventArgs e)
    {
        TextBoxWebSchedPerDaySave();
    }

    private void FormAsapSetup_FormClosing(object sender, FormClosingEventArgs e)
    {
        TextBoxWebSchedPerDaySave();
    }
}