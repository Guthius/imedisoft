using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormBackup : FormODBase
{
    public FormBackup()
    {
        InitializeComponent();
    }

    private void FormBackup_Load(object sender, EventArgs e)
    {
        checkExcludeImages.Checked = PrefC.GetBool(PrefName.BackupExcludeImageFolder);

        textBackupFromPath.Text = PrefC.GetString(PrefName.BackupFromPath);
        textBackupToPath.Text = PrefC.GetString(PrefName.BackupToPath);
        textBackupRestoreFromPath.Text = PrefC.GetString(PrefName.BackupRestoreFromPath);
        textBackupRestoreToPath.Text = PrefC.GetString(PrefName.BackupRestoreToPath);
        textBackupRestoreAtoZToPath.Text = PrefC.GetString(PrefName.BackupRestoreAtoZToPath);
        textBackupRestoreAtoZToPath.Enabled = ShouldBackupFiles();

        butBrowseRestoreAtoZTo.Enabled = ShouldBackupFiles();
    }

    private bool SavePreferences()
    {
        var changed = false;

        changed |= Prefs.UpdateBool(PrefName.BackupExcludeImageFolder, checkExcludeImages.Checked);
        changed |= Prefs.UpdateString(PrefName.BackupFromPath, textBackupFromPath.Text);
        changed |= Prefs.UpdateString(PrefName.BackupToPath, textBackupToPath.Text);
        changed |= Prefs.UpdateString(PrefName.BackupRestoreFromPath, textBackupRestoreFromPath.Text);
        changed |= Prefs.UpdateString(PrefName.BackupRestoreToPath, textBackupRestoreToPath.Text);
        changed |= Prefs.UpdateString(PrefName.BackupRestoreAtoZToPath, textBackupRestoreAtoZToPath.Text);

        return changed;
    }

    private bool ShouldBackupFiles()
    {
        return !checkExcludeImages.Checked;
    }

    private static void SelectFolder(TextBox textBox)
    {
        using var folderBrowserDialog = new FolderBrowserDialog();

        folderBrowserDialog.SelectedPath = textBox.Text;

        if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
        {
            return;
        }

        textBox.Text = folderBrowserDialog.SelectedPath;
    }

    private void ButtonBrowseFrom_Click(object sender, EventArgs e)
    {
        SelectFolder(textBackupFromPath);
    }

    private void ButtonBrowseTo_Click(object sender, EventArgs e)
    {
        SelectFolder(textBackupToPath);
    }

    private void ButtonBrowseRestoreFrom_Click(object sender, EventArgs e)
    {
        SelectFolder(textBackupRestoreFromPath);
    }

    private void ButtonBrowseRestoreTo_Click(object sender, EventArgs e)
    {
        SelectFolder(textBackupRestoreToPath);
    }

    private void ButtonBrowseRestoreAtoZTo_Click(object sender, EventArgs e)
    {
        SelectFolder(textBackupRestoreAtoZToPath);
    }

    private void ButtonBackup_Click(object sender, EventArgs e)
    {
        // TODO: Implement me...
    }

    private void ButtonRestore_Click(object sender, EventArgs e)
    {
        // TODO: Implement me...
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (SavePreferences())
        {
            DataValid.SetInvalid(InvalidType.Prefs);
        }
    }

    private void CheckBoxExcludeImages_Click(object sender, EventArgs e)
    {
        textBackupRestoreAtoZToPath.Enabled = ShouldBackupFiles();
        butBrowseRestoreAtoZTo.Enabled = ShouldBackupFiles();
    }
}