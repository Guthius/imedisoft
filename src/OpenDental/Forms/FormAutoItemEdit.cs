using System;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAutoItemEdit : FormODBase
{
    private readonly AutoCodeItem _autoCodeItem;

    public FormAutoItemEdit(AutoCodeItem autoCodeItem)
    {
        _autoCodeItem = autoCodeItem;

        InitializeComponent();
    }

    private void FormAutoItemEdit_Load(object sender, EventArgs e)
    {
        AutoCodeConds.RefreshCache();

        if (_autoCodeItem.AutoCodeItemNum == 0)
        {
            Text = "Add Auto Code Item";
        }
        else
        {
            Text = "Edit Auto Code Item";

            textADA.Text = ProcedureCodes.GetStringProcCode(_autoCodeItem.CodeNum);
        }

        FillList();
    }

    private void FillList()
    {
        var autoConditions = Enum.GetNames(typeof(AutoCondition));
        
        listConditions.Items.Clear();
        foreach (var autoCondition in autoConditions)
        {
            listConditions.Items.Add(autoCondition);
        }

        var autoCodeConds = AutoCodeConds.GetWhere(x => x.AutoCodeItemNum == _autoCodeItem.AutoCodeItemNum);
        foreach (var autoCodeCond in autoCodeConds)
        {
            listConditions.SetSelected((int) autoCodeCond.Cond);
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textADA.Text == "")
        {
            ShowError("Code cannot be left blank.");

            listConditions.SelectedIndex = -1;

            FillList();

            return;
        }

        _autoCodeItem.CodeNum = ProcedureCodes.GetCodeNum(textADA.Text);
        if (_autoCodeItem.AutoCodeItemNum == 0)
        {
            AutoCodeItems.Insert(_autoCodeItem);
        }
        else
        {
            AutoCodeItems.Update(_autoCodeItem);
        }

        AutoCodeConds.DeleteForItemNum(_autoCodeItem.AutoCodeItemNum);

        foreach (var index in listConditions.SelectedIndices)
        {
            AutoCodeConds.Insert(new AutoCodeCond
            {
                AutoCodeItemNum = _autoCodeItem.AutoCodeItemNum,
                Cond = (AutoCondition) index
            });
        }

        DialogResult = DialogResult.OK;
    }

    private void ButtonChange_Click(object sender, EventArgs e)
    {
        using var formProcCodes = new FormProcCodes();

        formProcCodes.IsSelectionMode = true;

        if (formProcCodes.ShowDialog() == DialogResult.Cancel)
        {
            textADA.Text = ProcedureCodes.GetStringProcCode(_autoCodeItem.CodeNum);
            return;
        }

        if (AutoCodeItems.GetContainsKey(formProcCodes.CodeNumSelected) && AutoCodeItems.GetOne(formProcCodes.CodeNumSelected).AutoCodeNum != _autoCodeItem.AutoCodeNum)
        {
            if (AutoCodes.GetContainsKey(AutoCodeItems.GetOne(formProcCodes.CodeNumSelected).AutoCodeNum))
            {
                ShowError("That procedure code is already in use in a different Auto Code. Not allowed to use it here.");

                textADA.Text = ProcedureCodes.GetStringProcCode(_autoCodeItem.CodeNum);
            }
            else
            {
                AutoCodeItems.Delete(AutoCodeItems.GetOne(formProcCodes.CodeNumSelected));

                textADA.Text = ProcedureCodes.GetStringProcCode(formProcCodes.CodeNumSelected);
            }
        }
        else
        {
            textADA.Text = ProcedureCodes.GetStringProcCode(formProcCodes.CodeNumSelected);
        }
    }
}