using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutoNoteExport : FormODBase
{
    private UserOdPref _userOdPrefDefNumsExpanded;

    public FormAutoNoteExport()
    {
        InitializeComponent();
    }

    private void FormAutoNoteExport_Load(object sender, EventArgs e)
    {
        _userOdPrefDefNumsExpanded = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.AutoNoteExpandedCats).FirstOrDefault();

        AutoNoteL.FillListTree(treeNotes, _userOdPrefDefNumsExpanded);
    }

    private static SaveFileDialog ExportDialogSetup()
    {
        var exportPath = PrefC.GetString(PrefName.ExportPath);

        var saveFileDialog = new SaveFileDialog();

        saveFileDialog.AddExtension = true;
        saveFileDialog.FileName = "autonotes.json";
        saveFileDialog.Filter = "JSON files(*.json)|*.json";

        if (Directory.Exists(exportPath))
        {
            saveFileDialog.InitialDirectory = exportPath;

            return saveFileDialog;
        }

        try
        {
            Directory.CreateDirectory(exportPath);

            saveFileDialog.InitialDirectory = exportPath;
        }
        catch
        {
            saveFileDialog.InitialDirectory = Path.GetTempPath();
        }

        return saveFileDialog;
    }

    private static void ToggleCheckboxes(TreeNodeCollection treeNodeCollection, bool value)
    {
        for (var i = 0; i < treeNodeCollection.Count; i++)
        {
            ToggleCheckboxes(treeNodeCollection[i].Nodes, value);

            treeNodeCollection[i].Checked = value;
        }
    }

    private static void ToggleCheckboxes(TreeNode treeNode, bool value)
    {
        ToggleCheckboxes(treeNode.Nodes, value);

        treeNode.Checked = value;
    }

    private static List<AutoNote> GetCheckedAutoNotes(TreeNodeCollection treeNodeCollection)
    {
        var autoNotesChecked = new List<AutoNote>();
        for (var i = 0; i < treeNodeCollection.Count; i++)
        {
            autoNotesChecked.AddRange(GetCheckedAutoNotes(treeNodeCollection[i].Nodes));

            if (treeNodeCollection[i].Checked && treeNodeCollection[i].Tag is AutoNote)
            {
                autoNotesChecked.Add((AutoNote) treeNodeCollection[i].Tag);
            }
        }

        return autoNotesChecked;
    }

    private void ButtonClear_Click(object sender, EventArgs e)
    {
        ToggleCheckboxes(treeNotes.Nodes, false);
    }

    private void ButtonExport_Click(object sender, EventArgs e)
    {
        var autoNotesChecked = GetCheckedAutoNotes(treeNotes.Nodes);
        if (autoNotesChecked.Count == 0)
        {
            ShowError("You must select at least one Auto Note to export.");
            return;
        }

        string fileName;

        using (var saveDialog = ExportDialogSetup())
        {
            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            fileName = saveDialog.FileName;
        }

        var serializableAutoNotes = AutoNotes.GetSerializableAutoNotes(autoNotesChecked);
        var autoNoteControls = AutoNoteControls.GetListByParsingAutoNoteText(serializableAutoNotes);
        var serializableAutoNoteControls = AutoNoteControls.GetSerializableAutoNoteControls(autoNoteControls);

        try
        {
            AutoNotes.WriteAutoNotesToJson(serializableAutoNotes, serializableAutoNoteControls, fileName);
        }
        catch (Exception ex)
        {
            ShowException(ex, "AutoNote(s) failed to export.");
            return;
        }

        SecurityLogs.MakeLogEntry(EnumPermType.AutoNoteQuickNoteEdit, 0, "Auto Note Export");

        ShowInfo("Auto Note(s) successfully exported.");
    }

    private void CheckBoxCollapse_CheckedChanged(object sender, EventArgs e)
    {
        AutoNoteL.SetCollapsed(treeNotes, checkCollapse.Checked);
    }

    private void ButtonSelectAll_Click(object sender, EventArgs e)
    {
        ToggleCheckboxes(treeNotes.Nodes, true);
    }

    private void Node_AfterCheck(object sender, TreeViewEventArgs e)
    {
        if (e.Action != TreeViewAction.Unknown)
        {
            ToggleCheckboxes(e.Node, e.Node.Checked);
        }
    }
}