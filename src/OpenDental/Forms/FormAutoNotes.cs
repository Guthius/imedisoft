using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutoNotes : FormODBase
{
    private UserOdPref _autoNoteExpandedCatsUserPref;
    private TreeNode _treeNodeGray;

    public bool IsSelectionMode { get; set; }
    public AutoNote SelectedAutoNote { get; set; }

    public FormAutoNotes()
    {
        InitializeComponent();
    }

    private void FormAutoNotes_Load(object sender, EventArgs e)
    {
        if (IsSelectionMode)
        {
            butAdd.Visible = false;
            labelSelection.Visible = true;
        }

        _autoNoteExpandedCatsUserPref = UserOdPrefs.GetByUserAndFkeyType(Security.CurUser.UserNum, UserOdFkeyType.AutoNoteExpandedCats).FirstOrDefault();

        AutoNoteL.FillListTree(treeNotes, _autoNoteExpandedCatsUserPref);
    }

    private static OpenFileDialog ImportDialogSetup()
    {
        var openFileDialog = new OpenFileDialog();

        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "JSON files(*.json)|*.json";
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        return openFileDialog;
    }

    private static bool IsValidDestination(TreeNode treeNode, TreeNode treeNodeDestination, bool isSourceDef)
    {
        if (treeNode is null || treeNode.Parent == treeNodeDestination)
        {
            return false;
        }

        if (!isSourceDef)
        {
            return true;
        }

        return treeNodeDestination == null || !treeNodeDestination.FullPath.StartsWith(treeNode.FullPath);
    }

    private void TreeNotes_MouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node is not {Tag: AutoNote note})
        {
            return;
        }

        var autoNote = note.Copy();

        if (IsSelectionMode)
        {
            SelectedAutoNote = autoNote;

            DialogResult = DialogResult.OK;
            return;
        }

        using var formAutoNoteEdit = new FormAutoNoteEdit(autoNote);

        if (formAutoNoteEdit.ShowDialog() == DialogResult.OK)
        {
            AutoNoteL.FillListTree(treeNotes, _autoNoteExpandedCatsUserPref);
        }
    }

    private void TreeNotes_DragOver(object sender, DragEventArgs e)
    {
        var point = treeNotes.PointToClient(new Point(e.X, e.Y));
        var treeNodeSelected = treeNotes.GetNodeAt(point);

        if (_treeNodeGray is not null && _treeNodeGray != treeNodeSelected)
        {
            _treeNodeGray.BackColor = Color.White;
            _treeNodeGray = null;
        }

        if (treeNodeSelected is not null && treeNodeSelected.BackColor != Color.LightGray)
        {
            treeNodeSelected.BackColor = Color.LightGray;

            _treeNodeGray = treeNodeSelected;
        }

        if (point.Y < 25)
        {
            MiscUtils.SendMessage(treeNotes.Handle, 277, 0, 0);
        }
        else if (point.Y > treeNotes.Height - 25)
        {
            MiscUtils.SendMessage(treeNotes.Handle, 277, 1, 0);
        }
    }

    private void TreeNotes_ItemDrag(object sender, ItemDragEventArgs e)
    {
        treeNotes.SelectedNode = (TreeNode) e.Item;

        DoDragDrop(e.Item, DragDropEffects.Move);
    }

    private void TreeNotes_DragEnter(object sender, DragEventArgs e)
    {
        e.Effect = DragDropEffects.Move;
    }

    private void TreeNotes_DragDrop(object sender, DragEventArgs e)
    {
        if (_treeNodeGray is not null)
        {
            _treeNodeGray.BackColor = Color.White;
        }

        if (!e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
        {
            return;
        }

        var sourceTreeNode = (TreeNode) e.Data.GetData("System.Windows.Forms.TreeNode");
        if (sourceTreeNode is not {Tag: Def or AutoNote})
        {
            return;
        }

        if (sourceTreeNode.Tag is Def && !Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        var topTreeNode = treeNotes.TopNode;
        if (treeNotes.TopNode == sourceTreeNode && sourceTreeNode.PrevVisibleNode != null)
        {
            topTreeNode = sourceTreeNode.PrevVisibleNode;
        }

        var point = ((TreeView) sender).PointToClient(new Point(e.X, e.Y));

        var destinationTreeNode = ((TreeView) sender).GetNodeAt(point);
        if (destinationTreeNode is not {Tag: Def or AutoNote})
        {
            if (sourceTreeNode.Parent is null)
            {
                return;
            }

            if (!Confirm("Move the selected " + (sourceTreeNode.Tag is AutoNote ? "Auto Note" : "category") + " to the root level?"))
            {
                return;
            }

            if (sourceTreeNode.Tag is Def def)
            {
                def.ItemValue = "";
            }
            else
            {
                ((AutoNote) sourceTreeNode.Tag).Category = 0;
            }
        }
        else
        {
            if (destinationTreeNode.Tag is AutoNote)
            {
                destinationTreeNode = destinationTreeNode.Parent;
            }

            if (!IsValidDestination(sourceTreeNode, destinationTreeNode, sourceTreeNode.Tag is Def))
            {
                return;
            }

            if (!Confirm("Move the selected " + (sourceTreeNode.Tag is AutoNote ? "Auto Note" : "category") + (destinationTreeNode is null ? " to the root level" : "") + "?"))
            {
                return;
            }

            var destDefNum = ((Def) destinationTreeNode?.Tag)?.DefNum ?? 0;
            if (sourceTreeNode.Tag is Def def)
            {
                def.ItemValue = destDefNum == 0 ? "" : destDefNum.ToString();
            }
            else
            {
                ((AutoNote) sourceTreeNode.Tag).Category = destDefNum;
            }
        }

        if (sourceTreeNode.Tag is Def tag)
        {
            DefL.Update(tag);
            DataValid.SetInvalid(InvalidType.Defs);
        }
        else
        {
            AutoNotes.Update((AutoNote) sourceTreeNode.Tag);
            DataValid.SetInvalid(InvalidType.AutoNotes);
        }

        treeNotes.TopNode = topTreeNode;

        AutoNoteL.FillListTree(treeNotes, _autoNoteExpandedCatsUserPref);
    }

    private void CheckBoxCollapse_CheckedChanged(object sender, EventArgs e)
    {
        AutoNoteL.SetCollapsed(treeNotes, checkCollapse.Checked);
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.AutoNoteQuickNoteEdit))
        {
            return;
        }

        var defNum = treeNotes.SelectedNode?.Tag switch
        {
            Def def => def.DefNum,
            AutoNote note => note.Category,
            _ => 0
        };

        var autoNote = new AutoNote
        {
            Category = defNum
        };

        using var formAutoNoteEdit = new FormAutoNoteEdit(autoNote);

        if (formAutoNoteEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        treeNotes.SelectedNode?.Expand();

        AutoNoteL.FillListTree(treeNotes, _autoNoteExpandedCatsUserPref);

        if (autoNote is not {AutoNoteNum: > 0})
        {
            return;
        }

        treeNotes.SelectedNode = treeNotes.Nodes
            .OfType<TreeNode>()
            .SelectMany(x => AutoNoteL.GetNodeAndChildren(x))
            .Where(x => x.Tag is AutoNote)
            .FirstOrDefault(x => ((AutoNote) x.Tag).AutoNoteNum == autoNote.AutoNoteNum);

        treeNotes.SelectedNode?.EnsureVisible();
        treeNotes.Focus();
    }

    private void ButtonExport_Click(object sender, EventArgs e)
    {
        using var formAutoNoteExport = new FormAutoNoteExport();

        formAutoNoteExport.ShowDialog();
    }

    private void ButtonImport_Click(object sender, EventArgs e)
    {
        using var openFileDialog = ImportDialogSetup();

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var fileName = openFileDialog.FileName;

        string contents;
        try
        {
            contents = File.ReadAllText(fileName);
        }
        catch (Exception ex)
        {
            ShowException(ex, "Auto Note(s) failed to import.");
            return;
        }

        var transferableAutoNotesImport = JsonConvert.DeserializeObject<TransferableAutoNotes>(contents);

        AutoNoteControls.RemoveDuplicatesFromList(transferableAutoNotesImport.AutoNoteControls, transferableAutoNotesImport.AutoNotes);
        AutoNoteControls.InsertBatch(transferableAutoNotesImport.AutoNoteControls);

        AutoNotes.InsertBatch(transferableAutoNotesImport.AutoNotes);

        DataValid.SetInvalid(InvalidType.AutoNotes);

        AutoNoteL.FillListTree(treeNotes, _autoNoteExpandedCatsUserPref);

        SecurityLogs.MakeLogEntry(EnumPermType.AutoNoteQuickNoteEdit, 0, $"Auto Note Import. {transferableAutoNotesImport.AutoNotes.Count} new Auto Notes, {transferableAutoNotesImport.AutoNoteControls.Count} new Prompts");

        ShowError(
            "Auto Notes successfully imported!\r\n" +
            transferableAutoNotesImport.AutoNotes.Count + " new Auto Notes\r\n" +
            transferableAutoNotesImport.AutoNoteControls.Count + " new Prompts");
    }

    private void FormAutoNotes_FormClosing(object sender, FormClosingEventArgs e)
    {
        var expandedDefNums = treeNotes.Nodes
            .OfType<TreeNode>()
            .SelectMany(x => AutoNoteL.GetNodeAndChildren(x, true))
            .Where(x => x.IsExpanded)
            .Select(x => ((Def) x.Tag).DefNum)
            .Where(x => x > 0).ToList();

        if (_autoNoteExpandedCatsUserPref is null)
        {
            UserOdPrefs.Insert(new UserOdPref
            {
                UserNum = Security.CurUser.UserNum,
                FkeyType = UserOdFkeyType.AutoNoteExpandedCats,
                ValueString = string.Join(",", expandedDefNums)
            });

            DataValid.SetInvalid(InvalidType.UserOdPrefs);
            return;
        }

        var userPrefOld = _autoNoteExpandedCatsUserPref.Clone();

        _autoNoteExpandedCatsUserPref.ValueString = string.Join(",", expandedDefNums);

        if (UserOdPrefs.Update(_autoNoteExpandedCatsUserPref, userPrefOld))
        {
            DataValid.SetInvalid(InvalidType.UserOdPrefs);
        }
    }
}