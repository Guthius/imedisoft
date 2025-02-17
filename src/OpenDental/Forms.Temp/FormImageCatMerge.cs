using System;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormImageCatMerge : FormODBase
{
    private long _defNumInto;
    private long _defNumFrom;

    public FormImageCatMerge()
    {
        InitializeComponent();
    }

    private void ButtonChangeInto_Click(object sender, EventArgs e)
    {
        using var formDefinitionPicker = new FormDefinitionPicker(DefCat.ImageCats);

        if (formDefinitionPicker.ShowDialog() != DialogResult.OK || formDefinitionPicker.SelectedDefs.Count == 0)
        {
            return;
        }

        var def = formDefinitionPicker.SelectedDefs.First();

        textBoxInto.Text = def.ItemName;

        _defNumInto = def.DefNum;

        UpdateMergeButtonState();
    }

    private void ButtonChangeFrom_Click(object sender, EventArgs e)
    {
        using var formDefinitionPicker = new FormDefinitionPicker(DefCat.ImageCats);

        if (formDefinitionPicker.ShowDialog() != DialogResult.OK || formDefinitionPicker.SelectedDefs.Count == 0)
        {
            return;
        }

        var def = formDefinitionPicker.SelectedDefs.First();

        textBoxFrom.Text = def.ItemName;

        _defNumFrom = def.DefNum;

        UpdateMergeButtonState();
    }

    private void UpdateMergeButtonState()
    {
        butMerge.Enabled = textBoxFrom.Text.Trim() != "" && textBoxInto.Text.Trim() != "";
    }

    private bool IsMergeAllowed()
    {
        if (PrefC.GetLong(PrefName.TaskAttachmentCategory) != _defNumFrom)
        {
            return true;
        }

        return Confirm(
            "The 'From' image category is currently being used to store task attachments. " +
            "Images will continue to be stored in this image category but the category will be marked hidden by this tool. " +
            "Continue Anyway?");
    }

    private void butMerge_Click(object sender, EventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.DefEdit))
        {
            return;
        }

        if (_defNumInto == _defNumFrom)
        {
            ShowError("Cannot merge the same Image Category. Please update either the merge Into field or the merge From field.");
            return;
        }

        if (!IsMergeAllowed())
        {
            return;
        }

        if (!Confirm("Are you sure? The results are permanent and cannot be undone."))
        {
            return;
        }

        try
        {
            Defs.MergeImageCatDefNums(_defNumFrom, _defNumInto);
        }
        catch (Exception ex)
        {
            ShowException(ex, "Image Categories failed to merge.");
            return;
        }

        DefL.HideDef(Defs.GetDef(DefCat.ImageCats, _defNumFrom));

        DataValid.SetInvalid(InvalidType.Defs);

        ShowInfo("Image Categories merged successfully.");

        var logMessage = "Image Category Merge from " + Defs.GetName(DefCat.ImageCats, _defNumFrom) + " to " + Defs.GetName(DefCat.ImageCats, _defNumInto);

        SecurityLogs.MakeLogEntry(EnumPermType.Setup, 0, logMessage);

        textBoxFrom.Clear();
        textBoxInto.Clear();

        UpdateMergeButtonState();
    }
}