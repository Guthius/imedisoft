using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormApptViewEdit : FormODBase
{
    private readonly ApptView _apptView;
    private readonly long _clinicNum;
    private List<EnumApptViewElement> _apptViewElementsAvailable;
    private List<long> _apptFieldDefNumsAvailable;
    private List<long> _patFieldDefNums;
    private List<ApptViewItem> _apptViewItemsDisplayedAll;
    private List<ApptViewItem> _apptViewItemsDisplayedMain;
    private List<ApptViewItem> _apptViewItemsDisplayedUr;
    private List<ApptViewItem> _apptViewItemsDisplayedLr;
    private List<ApptViewItem> _apptViewItems;
    private List<ApptViewItem> _apptViewItemsDef;
    private List<long> _opNums;
    private List<ProviderDto> _providers;

    public FormApptViewEdit(ApptView apptView, long clinicNum)
    {
        _apptView = apptView;
        _clinicNum = clinicNum;

        InitializeComponent();
    }

    private void FormApptViewEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _apptView.Description;
        textRowsPerIncr.Text = _apptView.RowsPerIncr == 0 ? "1" : _apptView.RowsPerIncr.ToString();
        textWidthOpMinimum.Text = SOut.Int(_apptView.WidthOpMinimum);
        textScrollTime.Text = _apptView.ApptTimeScrollStart.ToStringHmm();

        checkDynamicScroll.Checked = _apptView.IsScrollStartDynamic;
        checkApptBubblesDisabled.Checked = _apptView.IsApptBubblesDisabled;

        if (_apptView.ApptViewNum == 0)
        {
            checkApptBubblesDisabled.Checked = PrefC.GetBool(PrefName.AppointmentBubblesDisabled);
        }

        checkOnlyScheduledProvs.Checked = _apptView.OnlyScheduledProvs;
        checkOnlyScheduledProvDays.Checked = _apptView.OnlyScheduledProvDays;

        if (_apptView.OnlySchedBeforeTime > new TimeSpan(0, 0, 0))
        {
            textBeforeTime.Text = (DateTime.Today + _apptView.OnlySchedBeforeTime).ToShortTimeString();
        }

        if (_apptView.OnlySchedAfterTime > new TimeSpan(0, 0, 0))
        {
            textAfterTime.Text = (DateTime.Today + _apptView.OnlySchedAfterTime).ToShortTimeString();
        }

        comboClinic.ClinicNumSelected = _clinicNum;

        UpdateDisplayFilterGroup();

        _apptViewItems = ApptViewItems.GetWhere(x => x.ApptViewNum == _apptView.ApptViewNum && !x.IsMobile);
        ApptViewItems.GetWhere(x => x.ApptViewNum == _apptView.ApptViewNum && x.IsMobile);
        _apptViewItemsDef = _apptViewItems.FindAll(x => x.OpNum == 0 && x.ProvNum == 0);

        FillOperatories();

        _providers = Providers.GetDeepCopy(true);

        for (var i = 0; i < _providers.Count; i++)
        {
            listProv.Items.Add(_providers[i].Description);
            if (_apptViewItems.Select(x => x.ProvNum).Contains(_providers[i].Id))
            {
                listProv.SetSelected(i);
            }
        }

        listWaitingRmNameFormat.Items.AddEnums<EnumWaitingRmName>();
        listWaitingRmNameFormat.SetSelected((int) _apptView.WaitingRmName);

        for (var i = 0; i < Enum.GetNames(typeof(ApptViewStackBehavior)).Length; i++)
        {
            listStackUR.Items.Add(Enum.GetNames(typeof(ApptViewStackBehavior))[i]);
            listStackLR.Items.Add(Enum.GetNames(typeof(ApptViewStackBehavior))[i]);
        }

        listStackUR.SelectedIndex = (int) _apptView.StackBehavUR;
        listStackLR.SelectedIndex = (int) _apptView.StackBehavLR;

        _apptViewItemsDisplayedAll = new List<ApptViewItem>(_apptViewItemsDef);

        FillElements();
    }

    private void FormApptViewEdit_Closing(object sender, CancelEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            return;
        }

        if (_apptView.ApptViewNum > 0)
        {
            return;
        }

        ApptViewItems.DeleteAllForView(_apptView);
        ApptViewItems.DeleteAllForView(_apptView, isMobile: true);

        ApptViews.Delete(_apptView);
    }

    private void FillElements()
    {
        _apptViewItemsDisplayedMain = [];
        _apptViewItemsDisplayedUr = [];
        _apptViewItemsDisplayedLr = [];

        foreach (var apptViewItem in _apptViewItemsDisplayedAll)
        {
            switch (apptViewItem.ElementAlignment)
            {
                case ApptViewAlignment.Main:
                    _apptViewItemsDisplayedMain.Add(apptViewItem);
                    break;

                case ApptViewAlignment.UR:
                    _apptViewItemsDisplayedUr.Add(apptViewItem);
                    break;

                case ApptViewAlignment.LR:
                    _apptViewItemsDisplayedLr.Add(apptViewItem);
                    break;
            }
        }

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("", 100));

        gridMain.ListGridRows.Clear();

        GridRow gridRow;
        foreach (var apptViewItem in _apptViewItemsDisplayedMain)
        {
            gridRow = new GridRow();
            if (apptViewItem.ApptFieldDefNum > 0)
            {
                gridRow.Cells.Add(MarkFieldNameIfHidden(apptViewItem.ApptFieldDefNum));
            }
            else if (apptViewItem.PatFieldDefNum > 0)
            {
                gridRow.Cells.Add(PatFieldDefs.GetFieldName(apptViewItem.PatFieldDefNum));
            }
            else
            {
                gridRow.Cells.Add(apptViewItem.ElementDesc);
            }

            if (DoSetBackgroundColor(apptViewItem.ElementDesc))
            {
                gridRow.ColorBackG = apptViewItem.ElementColor;
            }
            else
            {
                gridRow.ColorText = apptViewItem.ElementColor;
            }

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();

        gridUR.BeginUpdate();

        gridUR.Columns.Clear();
        gridUR.Columns.Add(new GridColumn("", 100));

        gridUR.ListGridRows.Clear();

        foreach (var apptViewItem in _apptViewItemsDisplayedUr)
        {
            gridRow = new GridRow();
            if (apptViewItem.ApptFieldDefNum > 0)
            {
                gridRow.Cells.Add(MarkFieldNameIfHidden(apptViewItem.ApptFieldDefNum));
            }
            else if (apptViewItem.PatFieldDefNum > 0)
            {
                gridRow.Cells.Add(PatFieldDefs.GetFieldName(apptViewItem.PatFieldDefNum));
            }
            else
            {
                gridRow.Cells.Add(apptViewItem.ElementDesc);
            }

            if (DoSetBackgroundColor(apptViewItem.ElementDesc))
            {
                gridRow.ColorBackG = apptViewItem.ElementColor;
            }
            else
            {
                gridRow.ColorText = apptViewItem.ElementColor;
            }

            gridUR.ListGridRows.Add(gridRow);
        }

        gridUR.EndUpdate();

        gridLR.BeginUpdate();

        gridLR.Columns.Clear();
        gridLR.Columns.Add(new GridColumn("", 100));

        gridLR.ListGridRows.Clear();

        foreach (var apptViewItem in _apptViewItemsDisplayedLr)
        {
            gridRow = new GridRow();
            if (apptViewItem.ApptFieldDefNum > 0)
            {
                gridRow.Cells.Add(ApptFieldDefs.GetFieldName(apptViewItem.ApptFieldDefNum));
            }
            else if (apptViewItem.PatFieldDefNum > 0)
            {
                gridRow.Cells.Add(PatFieldDefs.GetFieldName(apptViewItem.PatFieldDefNum));
            }
            else
            {
                gridRow.Cells.Add(apptViewItem.ElementDesc);
            }

            if (DoSetBackgroundColor(apptViewItem.ElementDesc))
            {
                gridRow.ColorBackG = apptViewItem.ElementColor;
            }
            else
            {
                gridRow.ColorText = apptViewItem.ElementColor;
            }

            gridLR.ListGridRows.Add(gridRow);
        }

        gridLR.EndUpdate();

        gridAvailable.BeginUpdate();

        gridAvailable.Columns.Clear();
        gridAvailable.Columns.Add(new GridColumn("", 100));

        gridAvailable.ListGridRows.Clear();

        _apptViewElementsAvailable = [];
        for (var i = 0; i < Enum.GetValues(typeof(EnumApptViewElement)).Length; i++)
        {
            if ((EnumApptViewElement) i == EnumApptViewElement.None)
            {
                continue;
            }

            if (ElementIsDisplayed((EnumApptViewElement) i))
            {
                continue;
            }

            _apptViewElementsAvailable.Add((EnumApptViewElement) i);

            gridRow = new GridRow();
            gridRow.Cells.Add(((EnumApptViewElement) i).GetDescription());

            gridAvailable.ListGridRows.Add(gridRow);
        }

        gridAvailable.EndUpdate();

        gridApptFieldDefs.BeginUpdate();

        gridApptFieldDefs.Columns.Clear();
        gridApptFieldDefs.Columns.Add(new GridColumn("", 100));

        gridApptFieldDefs.ListGridRows.Clear();

        _apptFieldDefNumsAvailable = [];

        var apptFieldDefs = ApptFieldDefs.GetDeepCopy();
        foreach (var apptFieldDef in apptFieldDefs)
        {
            if (ApptFieldIsDisplayed(apptFieldDef.ApptFieldDefNum))
            {
                continue;
            }

            _apptFieldDefNumsAvailable.Add(apptFieldDef.ApptFieldDefNum);

            gridRow = new GridRow();
            gridRow.Cells.Add(MarkFieldNameIfHidden(apptFieldDef.ApptFieldDefNum));

            gridApptFieldDefs.ListGridRows.Add(gridRow);
        }

        gridApptFieldDefs.EndUpdate();

        gridPatFieldDefs.BeginUpdate();

        gridPatFieldDefs.Columns.Clear();
        gridPatFieldDefs.Columns.Add(new GridColumn("", 100));

        gridPatFieldDefs.ListGridRows.Clear();

        _patFieldDefNums = [];

        var patFieldDefs = PatFieldDefs.GetDeepCopy(true);
        foreach (var patFieldDef in patFieldDefs)
        {
            if (PatFieldIsDisplayed(patFieldDef.PatFieldDefNum))
            {
                continue;
            }

            _patFieldDefNums.Add(patFieldDef.PatFieldDefNum);

            gridRow = new GridRow();
            gridRow.Cells.Add(patFieldDef.FieldName);

            gridPatFieldDefs.ListGridRows.Add(gridRow);
        }

        gridPatFieldDefs.EndUpdate();
    }

    private void FillOperatories()
    {
        listOps.ClearSelected();
        listOps.Items.Clear();

        _opNums = [];

        var operatories = Operatories.GetDeepCopy(true);

        foreach (var operatory in operatories)
        {
            if (comboClinic.ClinicNumSelected != 0 && operatory.ClinicNum != comboClinic.ClinicNumSelected)
            {
                continue;
            }

            listOps.Items.Add(operatory.OpName);

            _opNums.Add(operatory.OperatoryNum);

            if (_apptViewItems.Select(x => x.OpNum).Contains(operatory.OperatoryNum))
            {
                listOps.SetSelected(listOps.Items.Count - 1);
            }
        }
    }

    private bool ElementIsDisplayed(EnumApptViewElement apptViewElement)
    {
        foreach (var apptViewItem in _apptViewItemsDisplayedAll)
        {
            if (apptViewItem.ApptFieldDefNum != 0 || apptViewItem.PatFieldDefNum != 0)
            {
                continue;
            }

            if (apptViewItem.ElementDesc == apptViewElement.GetDescription())
            {
                return true;
            }
        }

        return false;
    }

    private bool ApptFieldIsDisplayed(long apptFieldDefNum)
    {
        foreach (var apptViewItem in _apptViewItemsDisplayedAll)
        {
            if (apptViewItem.ApptFieldDefNum == apptFieldDefNum)
            {
                return true;
            }
        }

        return false;
    }

    private bool PatFieldIsDisplayed(long patFieldDefNum)
    {
        foreach (var apptViewItem in _apptViewItemsDisplayedAll)
        {
            if (apptViewItem.PatFieldDefNum == patFieldDefNum)
            {
                return true;
            }
        }

        return false;
    }

    private void CheckBoxOnlyScheduledProvs_Click(object sender, EventArgs e)
    {
        UpdateDisplayFilterGroup();
    }

    private void UpdateDisplayFilterGroup()
    {
        if (checkOnlyScheduledProvs.Checked)
        {
            labelBeforeTime.Visible = true;
            labelAfterTime.Visible = true;
            textBeforeTime.Visible = true;
            textAfterTime.Visible = true;
        }
        else
        {
            labelBeforeTime.Visible = false;
            labelAfterTime.Visible = false;
            textBeforeTime.Visible = false;
            textAfterTime.Visible = false;
        }
    }

    public void UpdateMobileViewList(List<ApptViewItem> apptViewItems)
    {
    }

    private void ButtonLeft_Click(object sender, EventArgs e)
    {
        if (gridMain.SelectedIndices.Length > 0)
        {
            _apptViewItemsDisplayedAll.Remove(_apptViewItemsDisplayedMain[gridMain.SelectedIndices[0]]);
        }
        else if (gridUR.SelectedIndices.Length > 0)
        {
            _apptViewItemsDisplayedAll.Remove(_apptViewItemsDisplayedUr[gridUR.SelectedIndices[0]]);
        }
        else if (gridLR.SelectedIndices.Length > 0)
        {
            _apptViewItemsDisplayedAll.Remove(_apptViewItemsDisplayedLr[gridLR.SelectedIndices[0]]);
        }

        FillElements();
    }

    private void ButtonRight_Click(object sender, EventArgs e)
    {
        if (gridAvailable.GetSelectedIndex() != -1)
        {
            var strDescript = _apptViewElementsAvailable[gridAvailable.GetSelectedIndex()].GetDescription();
            var color = Color.Black;
            if (DoSetBackgroundColor(strDescript))
            {
                color = Color.White;
            }

            var item = new ApptViewItem(strDescript, 0, color);
            if (gridMain.SelectedIndices.Length == 1)
            {
                var newIdx = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedMain[gridMain.GetSelectedIndex()]);

                _apptViewItemsDisplayedAll.Insert(newIdx, item);
            }
            else
            {
                _apptViewItemsDisplayedAll.Add(item);
            }

            FillElements();
            for (var i = 0; i < _apptViewItemsDisplayedMain.Count; i++)
            {
                if (_apptViewItemsDisplayedMain[i] != item)
                {
                    continue;
                }

                gridMain.SetSelected(i);
                break;
            }
        }
        else if (gridApptFieldDefs.GetSelectedIndex() != -1)
        {
            var apptViewItem = new ApptViewItem
            {
                ElementColor = Color.Black,
                ApptFieldDefNum = _apptFieldDefNumsAvailable[gridApptFieldDefs.GetSelectedIndex()]
            };

            if (gridMain.SelectedIndices.Length == 1)
            {
                var newIdx = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedMain[gridMain.GetSelectedIndex()]);

                _apptViewItemsDisplayedAll.Insert(newIdx, apptViewItem);
            }
            else
            {
                _apptViewItemsDisplayedAll.Add(apptViewItem);
            }

            FillElements();
            for (var i = 0; i < _apptViewItemsDisplayedMain.Count; i++)
            {
                if (_apptViewItemsDisplayedMain[i] != apptViewItem)
                {
                    continue;
                }

                gridMain.SetSelected(i);
                break;
            }
        }
        else if (gridPatFieldDefs.GetSelectedIndex() != -1)
        {
            var apptViewItem = new ApptViewItem
            {
                ElementColor = Color.Black,
                PatFieldDefNum = _patFieldDefNums[gridPatFieldDefs.GetSelectedIndex()]
            };

            if (gridMain.SelectedIndices.Length == 1)
            {
                var newIdx = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedMain[gridMain.GetSelectedIndex()]);

                _apptViewItemsDisplayedAll.Insert(newIdx, apptViewItem);
            }
            else
            {
                _apptViewItemsDisplayedAll.Add(apptViewItem);
            }

            FillElements();
            for (var i = 0; i < _apptViewItemsDisplayedMain.Count; i++)
            {
                if (_apptViewItemsDisplayedMain[i] != apptViewItem)
                {
                    continue;
                }

                gridMain.SetSelected(i);
                break;
            }
        }
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        int oldIdx;
        int newIdx;
        int newIdxAll;

        ApptViewItem apptViewItem;

        if (gridMain.GetSelectedIndex() != -1)
        {
            oldIdx = gridMain.GetSelectedIndex();
            if (oldIdx == 0)
            {
                return;
            }

            apptViewItem = _apptViewItemsDisplayedMain[oldIdx];

            newIdx = oldIdx - 1;
            newIdxAll = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedMain[newIdx]);

            _apptViewItemsDisplayedAll.Remove(apptViewItem);
            _apptViewItemsDisplayedAll.Insert(newIdxAll, apptViewItem);

            FillElements();

            gridMain.SetSelected(newIdx);
        }
        else if (gridUR.GetSelectedIndex() != -1)
        {
            oldIdx = gridUR.GetSelectedIndex();
            if (oldIdx == 0)
            {
                return;
            }

            apptViewItem = _apptViewItemsDisplayedUr[oldIdx];

            newIdx = oldIdx - 1;
            newIdxAll = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedUr[newIdx]);

            _apptViewItemsDisplayedAll.Remove(apptViewItem);
            _apptViewItemsDisplayedAll.Insert(newIdxAll, apptViewItem);

            FillElements();

            gridUR.SetSelected(newIdx);
        }
        else if (gridLR.GetSelectedIndex() != -1)
        {
            oldIdx = gridLR.GetSelectedIndex();
            if (oldIdx == 0)
            {
                return;
            }

            apptViewItem = _apptViewItemsDisplayedLr[oldIdx];

            newIdx = oldIdx - 1;
            newIdxAll = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedLr[newIdx]);

            _apptViewItemsDisplayedAll.Remove(apptViewItem);
            _apptViewItemsDisplayedAll.Insert(newIdxAll, apptViewItem);

            FillElements();

            gridLR.SetSelected(newIdx);
        }
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        int oldIdx;
        int newIdx;
        int newIdxAll;

        ApptViewItem apptViewItem;

        if (gridMain.GetSelectedIndex() != -1)
        {
            oldIdx = gridMain.GetSelectedIndex();
            if (oldIdx == _apptViewItemsDisplayedMain.Count - 1)
            {
                return;
            }

            apptViewItem = _apptViewItemsDisplayedMain[oldIdx];

            newIdx = oldIdx + 1;
            newIdxAll = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedMain[newIdx]);

            _apptViewItemsDisplayedAll.Remove(apptViewItem);
            _apptViewItemsDisplayedAll.Insert(newIdxAll, apptViewItem);

            FillElements();

            gridMain.SetSelected(newIdx);
        }

        if (gridUR.GetSelectedIndex() != -1)
        {
            oldIdx = gridUR.GetSelectedIndex();
            if (oldIdx == _apptViewItemsDisplayedUr.Count - 1)
            {
                return;
            }

            apptViewItem = _apptViewItemsDisplayedUr[oldIdx];

            newIdx = oldIdx + 1;
            newIdxAll = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedUr[newIdx]);

            _apptViewItemsDisplayedAll.Remove(apptViewItem);
            _apptViewItemsDisplayedAll.Insert(newIdxAll, apptViewItem);

            FillElements();

            gridUR.SetSelected(newIdx);
        }

        if (gridLR.GetSelectedIndex() == -1)
        {
            return;
        }

        oldIdx = gridLR.GetSelectedIndex();
        if (oldIdx == _apptViewItemsDisplayedLr.Count - 1)
        {
            return;
        }

        apptViewItem = _apptViewItemsDisplayedLr[oldIdx];

        newIdx = oldIdx + 1;
        newIdxAll = _apptViewItemsDisplayedAll.IndexOf(_apptViewItemsDisplayedLr[newIdx]);

        _apptViewItemsDisplayedAll.Remove(apptViewItem);
        _apptViewItemsDisplayedAll.Insert(newIdxAll, apptViewItem);

        FillElements();

        gridLR.SetSelected(newIdx);
    }

    private void GridAvailable_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridAvailable.SelectedIndices.Length == 0)
        {
            return;
        }

        gridApptFieldDefs.SetAll(false);
        gridPatFieldDefs.SetAll(false);
    }

    private void GridApptFieldDefs_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridApptFieldDefs.SelectedIndices.Length == 0)
        {
            return;
        }

        gridAvailable.SetAll(false);
        gridPatFieldDefs.SetAll(false);
    }

    private void GridPatFieldDefs_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridPatFieldDefs.SelectedIndices.Length == 0)
        {
            return;
        }

        gridAvailable.SetAll(false);
        gridApptFieldDefs.SetAll(false);
    }

    private void GridMain_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridMain.SelectedIndices.Length == 0)
        {
            return;
        }

        gridUR.SetAll(false);
        gridLR.SetAll(false);
    }

    private void GridUR_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridUR.SelectedIndices.Length == 0)
        {
            return;
        }

        gridMain.SetAll(false);
        gridLR.SetAll(false);
    }

    private void GridLR_CellClick(object sender, ODGridClickEventArgs e)
    {
        if (gridLR.SelectedIndices.Length == 0)
        {
            return;
        }

        gridUR.SetAll(false);
        gridMain.SetAll(false);
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var frmApptViewItemEdit = new FrmApptViewItemEdit
        {
            ApptViewItemCur = _apptViewItemsDisplayedMain[e.Row]
        };

        frmApptViewItemEdit.ShowDialog();

        FillElements();

        ReselectItem(frmApptViewItemEdit.ApptViewItemCur);
    }

    private void GridUR_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var frmApptViewItemEdit = new FrmApptViewItemEdit
        {
            ApptViewItemCur = _apptViewItemsDisplayedUr[e.Row]
        };

        frmApptViewItemEdit.ShowDialog();

        FillElements();

        ReselectItem(frmApptViewItemEdit.ApptViewItemCur);
    }

    private void GridLR_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var frmApptViewItemEdit = new FrmApptViewItemEdit
        {
            ApptViewItemCur = _apptViewItemsDisplayedLr[e.Row]
        };

        frmApptViewItemEdit.ShowDialog();

        FillElements();

        ReselectItem(frmApptViewItemEdit.ApptViewItemCur);
    }

    private static bool DoSetBackgroundColor(string apptItemDescription)
    {
        return apptItemDescription.In(
            EnumApptViewElement.MedOrPremed_plus.GetDescription(),
            EnumApptViewElement.HasIns_I.GetDescription(),
            EnumApptViewElement.InsToSend_excl.GetDescription(),
            EnumApptViewElement.RecallPastDue_R.GetDescription(),
            EnumApptViewElement.ProphyPerioPastDue_P.GetDescription(),
            EnumApptViewElement.LateColor.GetDescription());
    }

    private static string MarkFieldNameIfHidden(long apptFieldDefNum)
    {
        if (FieldDefLinks.GetExists(x => x.FieldDefNum == apptFieldDefNum && x.FieldDefType == FieldDefTypes.Appointment))
        {
            return ApptFieldDefs.GetFieldName(apptFieldDefNum) + " (Hidden)";
        }

        return ApptFieldDefs.GetFieldName(apptFieldDefNum);
    }

    private void ReselectItem(ApptViewItem apptViewItem)
    {
        for (var i = 0; i < _apptViewItemsDisplayedMain.Count; i++)
        {
            if (_apptViewItemsDisplayedMain[i] != apptViewItem)
            {
                continue;
            }

            gridMain.SetSelected(i);
            break;
        }

        for (var i = 0; i < _apptViewItemsDisplayedUr.Count; i++)
        {
            if (_apptViewItemsDisplayedUr[i] != apptViewItem)
            {
                continue;
            }

            gridUR.SetSelected(i);
            break;
        }

        for (var i = 0; i < _apptViewItemsDisplayedLr.Count; i++)
        {
            if (_apptViewItemsDisplayedLr[i] != apptViewItem)
            {
                continue;
            }

            gridLR.SetSelected(i);
            break;
        }
    }

    private void TextBoxRowsPerIncr_Validating(object sender, CancelEventArgs e)
    {
        if (!int.TryParse(textRowsPerIncr.Text, out var rowsPerIncr))
        {
            ShowError("Must be a number between 1 and 3.");

            e.Cancel = true;
            return;
        }

        if (rowsPerIncr is >= 1 and <= 3)
        {
            return;
        }

        ShowError("Must be a number between 1 and 3.");

        e.Cancel = true;
    }

    private void ComboBoxClinic_SelectionChangeCommitted(object sender, EventArgs e)
    {
        FillOperatories();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (!ConfirmOk("Delete this view?"))
        {
            return;
        }

        ApptViewItems.DeleteAllForView(_apptView);
        ApptViewItems.DeleteAllForView(_apptView, isMobile: true);

        ApptViews.Delete(_apptView);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (listProv.SelectedIndices.Count == 0)
        {
            ShowError("At least one provider must be selected.");
            return;
        }

        if (listOps.SelectedIndices.Count == 0)
        {
            ShowError("At least one operatory must be selected.");
            return;
        }

        if (textDescription.Text == "")
        {
            ShowError("A description must be entered.");
            return;
        }

        int widthOpMinimum;
        try
        {
            widthOpMinimum = Convert.ToInt32(textWidthOpMinimum.Text);
            if (widthOpMinimum is < 0 or > 2000)
            {
                throw new Exception();
            }
        }
        catch
        {
            ShowError("Invalid Minimum Op width.");
            return;
        }

        if (_apptViewItemsDisplayedMain.Count == 0)
        {
            ShowError("At least one row type must be displayed.");
            return;
        }

        var timeBefore = new DateTime();
        if (checkOnlyScheduledProvs.Checked && textBeforeTime.Text != "")
        {
            try
            {
                timeBefore = DateTime.Parse(textBeforeTime.Text);
            }
            catch
            {
                ShowError("Time before invalid.");
                return;
            }
        }

        var timeAfter = new DateTime();
        if (checkOnlyScheduledProvs.Checked && textAfterTime.Text != "")
        {
            try
            {
                timeAfter = DateTime.Parse(textAfterTime.Text);
            }
            catch
            {
                ShowError("Time after invalid.");
                return;
            }
        }

        DateTime timeScroll;
        if (textScrollTime.Text == "")
        {
            timeScroll = DateTime.Parse("08:00:00");
        }
        else
        {
            try
            {
                timeScroll = DateTime.Parse(textScrollTime.Text);
            }
            catch
            {
                ShowError("Scroll start time invalid.");
                return;
            }
        }

        ApptViewItems.DeleteAllForView(_apptView);
        ApptViewItems.DeleteAllForView(_apptView, isMobile: true);
        ApptViewItem apptViewItem;

        for (var i = 0; i < _opNums.Count; i++)
        {
            if (!listOps.SelectedIndices.Contains(i))
            {
                continue;
            }

            apptViewItem = new ApptViewItem
            {
                ApptViewNum = _apptView.ApptViewNum,
                OpNum = _opNums[i],
                IsMobile = false
            };
            ApptViewItems.Insert(apptViewItem);
        }

        for (var i = 0; i < _providers.Count; i++)
        {
            if (!listProv.SelectedIndices.Contains(i))
            {
                continue;
            }

            apptViewItem = new ApptViewItem
            {
                ApptViewNum = _apptView.ApptViewNum,
                ProvNum = _providers[i].Id,
                IsMobile = false
            };

            ApptViewItems.Insert(apptViewItem);
        }

        _apptView.StackBehavUR = (ApptViewStackBehavior) listStackUR.SelectedIndex;
        _apptView.StackBehavLR = (ApptViewStackBehavior) listStackLR.SelectedIndex;

        for (var i = 0; i < _apptViewItemsDisplayedMain.Count; i++)
        {
            apptViewItem = _apptViewItemsDisplayedMain[i];
            apptViewItem.ApptViewNum = _apptView.ApptViewNum;
            apptViewItem.ElementOrder = (byte) i;
            apptViewItem.IsMobile = false;
            ApptViewItems.Insert(apptViewItem);
        }

        for (var i = 0; i < _apptViewItemsDisplayedUr.Count; i++)
        {
            apptViewItem = _apptViewItemsDisplayedUr[i];
            apptViewItem.ApptViewNum = _apptView.ApptViewNum;
            apptViewItem.ElementOrder = (byte) i;
            apptViewItem.IsMobile = false;
            ApptViewItems.Insert(apptViewItem);
        }

        for (var i = 0; i < _apptViewItemsDisplayedLr.Count; i++)
        {
            apptViewItem = _apptViewItemsDisplayedLr[i];
            apptViewItem.ApptViewNum = _apptView.ApptViewNum;
            apptViewItem.ElementOrder = (byte) i;
            apptViewItem.IsMobile = false;
            ApptViewItems.Insert(apptViewItem);
        }

        _apptView.WaitingRmName = listWaitingRmNameFormat.GetSelected<EnumWaitingRmName>();
        _apptView.Description = textDescription.Text;
        _apptView.RowsPerIncr = SIn.Byte(textRowsPerIncr.Text);
        _apptView.WidthOpMinimum = widthOpMinimum;
        _apptView.ApptTimeScrollStart = timeScroll.TimeOfDay;
        _apptView.IsScrollStartDynamic = checkDynamicScroll.Checked;
        _apptView.IsApptBubblesDisabled = checkApptBubblesDisabled.Checked;
        _apptView.OnlyScheduledProvs = checkOnlyScheduledProvs.Checked;
        _apptView.OnlyScheduledProvDays = checkOnlyScheduledProvDays.Checked;
        _apptView.OnlySchedBeforeTime = timeBefore.TimeOfDay;
        _apptView.OnlySchedAfterTime = timeAfter.TimeOfDay;

        var clinicOld = _apptView.ClinicNum;

        _apptView.ClinicNum = 0;
        _apptView.ClinicNum = comboClinic.ClinicNumSelected;

        if (_apptView.ClinicNum != clinicOld && ComputerPrefs.LocalComputer.ApptViewNum == _apptView.ApptViewNum)
        {
            ComputerPrefs.LocalComputer.ApptViewNum = 0;
            ComputerPrefs.Update(ComputerPrefs.LocalComputer);
            UserodApptViews.InsertOrUpdate(Security.CurUser.UserNum, clinicOld, 0);
        }

        ApptViews.Update(_apptView);

        DialogResult = DialogResult.OK;
    }
}