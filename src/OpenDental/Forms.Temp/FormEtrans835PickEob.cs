using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental;

public partial class FormEtrans835PickEob : FormODBase
{
    private readonly List<string> _eobTranIds;
    private readonly string _messageText835;
    private readonly Etrans _etrans;
    private readonly bool _openEtrans835;

    public string TransSetIdSelected { get; set; }

    public FormEtrans835PickEob(List<string> eobTranIds, string messageText835, Etrans etrans, bool openEtrans835)
    {
        _eobTranIds = eobTranIds;
        _messageText835 = messageText835;
        _etrans = etrans;
        _openEtrans835 = openEtrans835;

        InitializeComponent();
    }

    private void FormEtrans835PickEob_Load(object sender, EventArgs e)
    {
        FillGridEobs();
    }

    private void FillGridEobs()
    {
        gridEobs.BeginUpdate();
        
        gridEobs.Columns.Clear();
        gridEobs.Columns.Add(new GridColumn("", 20) {IsWidthDynamic = true});
        
        gridEobs.ListGridRows.Clear();
        
        foreach (var transId in _eobTranIds)
        {
            var gridRow = new GridRow();
            
            gridRow.Cells.Add(transId);
            
            gridEobs.ListGridRows.Add(gridRow);
        }

        gridEobs.EndUpdate();
    }

    private void GridEobs_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        TransSetIdSelected = _eobTranIds[gridEobs.SelectedIndices[0]];
        if (!_openEtrans835)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        var formEtrans835Edit = new FormEtrans835Edit();

        formEtrans835Edit.EtransCur = _etrans;
        formEtrans835Edit.MessageText835 = _messageText835;
        formEtrans835Edit.TranSetId835 = TransSetIdSelected;
        formEtrans835Edit.Show();
    }
}