using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormOrthoRxSelect : FormODBase
{
    private List<OrthoRx> _orthoRxs;
    private List<OrthoRx> _orthoRxsAvailable;

    public List<OrthoRx> SelectedOrthoRxs { get; set; }

    public FormOrthoRxSelect()
    {
        InitializeComponent();
    }

    private void FormOrthoRxSelect_Load(object sender, EventArgs e)
    {
        _orthoRxs = OrthoRxs.GetDeepCopy();

        SelectedOrthoRxs = [];

        FillLists();
    }

    private void FillLists()
    {
        _orthoRxsAvailable = [];

        foreach (var orthoRx in _orthoRxs)
        {
            if (SelectedOrthoRxs.Contains(orthoRx))
            {
                continue;
            }

            _orthoRxsAvailable.Add(orthoRx);
        }

        listBoxAvail.Items.Clear();
        listBoxAvail.Items.AddList(_orthoRxsAvailable, x => x.Description);

        listBoxSelected.Items.Clear();
        listBoxSelected.Items.AddList(SelectedOrthoRxs, x => x.Description);
    }

    private void ButtonRemove_Click(object sender, EventArgs e)
    {
        if (listBoxSelected.SelectedIndices.Count < 1)
        {
            ShowError("Please select items on the right first.");
            return;
        }

        for (var i = listBoxSelected.SelectedIndices.Count - 1; i >= 0; i--)
        {
            SelectedOrthoRxs.Remove(SelectedOrthoRxs[listBoxSelected.SelectedIndices[i]]);
        }

        FillLists();
    }

    private void ButtonSelect_Click(object sender, EventArgs e)
    {
        if (listBoxAvail.SelectedIndices.Count < 1)
        {
            ShowError("Please select items on the left first.");
            return;
        }

        foreach (var index in listBoxAvail.SelectedIndices)
        {
            SelectedOrthoRxs.Add(_orthoRxsAvailable[index]);
        }

        FillLists();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (listBoxSelected.Items.Count < 1)
        {
            ShowError("Please move items to the list on the right first.");
            return;
        }

        DialogResult = DialogResult.OK;
    }
}