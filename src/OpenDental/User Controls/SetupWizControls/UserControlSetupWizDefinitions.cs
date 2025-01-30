using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.User_Controls.SetupWizard;

public partial class UserControlSetupWizDefinitions : SetupWizControl
{
    private List<Def> _listDefsAll;
    private bool _isDefChanged;

    private DefCatOptions SelectedDefCatOpts => listCategory.GetSelected<DefCatOptions>();

    private List<Def> ListDefsCurs
    {
        get { return _listDefsAll.Where(x => x.Category == SelectedDefCatOpts.DefCat).OrderBy(x => x.ItemOrder).ToList(); }
    }

    public UserControlSetupWizDefinitions()
    {
        InitializeComponent();

        OnControlDone += ControlDone;
    }

    private void UserControlSetupWizDefinitions_Load(object sender, EventArgs e)
    {
        IsDone = true;

        var listDefCats = new List<DefCat>
        {
            DefCat.AccountColors,
            DefCat.AdjTypes,
            DefCat.AppointmentColors,
            DefCat.ApptConfirmed,
            DefCat.ApptProcsQuickAdd,
            DefCat.AutoNoteCats,
            DefCat.BillingTypes,
            DefCat.BlockoutTypes,
            DefCat.ChartGraphicColors,
            DefCat.CommLogTypes,
            DefCat.ImageCats,
            DefCat.PaymentTypes,
            DefCat.ProcCodeCats,
            DefCat.RecallUnschedStatus,
            DefCat.TxPriorities
        };

        var defCatOptionsOrdered = DefL.GetOptionsForDefCats(listDefCats);

        defCatOptionsOrdered = defCatOptionsOrdered.OrderBy(x => x.DefCat.GetDescription()).ToList();

        foreach (var defCOpt in defCatOptionsOrdered)
        {
            listCategory.Items.Add(defCOpt.DefCat.GetDescription(), defCOpt);

            if (defCOpt.DefCat == defCatOptionsOrdered[0].DefCat)
            {
                listCategory.SelectedItem = defCOpt;
            }
        }
    }

    private void FillGridDefs()
    {
        if (_listDefsAll == null || _listDefsAll.Count == 0)
        {
            RefreshDefs();
        }

        DefL.FillGridDefs(gridDefs, SelectedDefCatOpts, ListDefsCurs);

        butHide.Visible = SelectedDefCatOpts.CanHide;

        if (SelectedDefCatOpts.CanEditName)
        {
            groupEdit.Enabled = true;
            groupEdit.Text = "Edit Items";
        }
        else
        {
            groupEdit.Enabled = false;
            groupEdit.Text = "Not allowed";
        }

        textGuide.Text = SelectedDefCatOpts.HelpText;
    }

    private void gridDefs_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var selectedDef = (Def) gridDefs.ListGridRows[e.Row].Tag;

        _isDefChanged = DefL.GridDefsDoubleClick(selectedDef, gridDefs, SelectedDefCatOpts, ListDefsCurs, _listDefsAll, _isDefChanged);
        if (!_isDefChanged)
        {
            return;
        }

        RefreshDefs();
        FillGridDefs();
    }


    private void butAdd_Click(object sender, EventArgs e)
    {
        if (!DefL.AddDef(gridDefs, SelectedDefCatOpts))
        {
            return;
        }

        RefreshDefs();
        FillGridDefs();

        _isDefChanged = true;
    }

    private void listCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        FillGridDefs();
    }

    private void RefreshDefs()
    {
        Defs.RefreshCache();

        _listDefsAll = Defs.GetDeepCopy().SelectMany(x => x.Value).ToList();
    }

    private void butHide_Click(object sender, EventArgs e)
    {
        if (!DefL.TryHideDefSelectedInGrid(gridDefs, SelectedDefCatOpts))
        {
            return;
        }

        RefreshDefs();
        FillGridDefs();

        _isDefChanged = true;
    }

    private void butUp_Click(object sender, EventArgs e)
    {
        if (!DefL.UpClick(gridDefs))
        {
            return;
        }

        _isDefChanged = true;
        FillGridDefs();
    }

    private void butDown_Click(object sender, EventArgs e)
    {
        if (!DefL.DownClick(gridDefs))
        {
            return;
        }

        _isDefChanged = true;
        FillGridDefs();
    }

    private void ControlDone(object sender, EventArgs e)
    {
        var defUpdates = new List<Def>();

        foreach (var kvp in Defs.GetDeepCopy())
        {
            for (var i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].ItemOrder == i)
                {
                    continue;
                }

                kvp.Value[i].ItemOrder = i;

                defUpdates.Add(kvp.Value[i]);
            }
        }

        defUpdates.ForEach(DefL.Update);

        if (_isDefChanged || defUpdates.Count > 0)
        {
            DataValid.SetInvalid(InvalidType.Defs);
        }
    }
}