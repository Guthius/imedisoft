using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDefEditBlockout : FormODBase
{
    private readonly Def _def;

    public FormDefEditBlockout(Def defCur)
    {
        InitializeComponent();

        _def = defCur.Copy();
    }

    private void FormDefEdit_Load(object sender, EventArgs e)
    {
        textName.Text = _def.ItemName;
        if (_def.ItemValue.Contains(BlockoutType.DontCopy.GetDescription()))
        {
            checkCutCopyPaste.Checked = true;
        }

        if (_def.ItemValue.Contains(BlockoutType.NoSchedule.GetDescription()))
        {
            checkOverlap.Checked = true;
        }

        checkHidden.Checked = _def.IsHidden;
        butColor.BackColor = _def.ItemColor;
    }

    private void ButtonColor_Click(object sender, EventArgs e)
    {
        colorDialog1.Color = butColor.BackColor;
        colorDialog1.ShowDialog();

        butColor.BackColor = colorDialog1.Color;
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (textName.Text == "")
        {
            ShowError("Name required.");
            return;
        }

        _def.ItemName = textName.Text;

        var descriptions = new List<string>();
        if (checkCutCopyPaste.Checked)
        {
            descriptions.Add(BlockoutType.DontCopy.GetDescription());
        }

        if (checkOverlap.Checked)
        {
            descriptions.Add(BlockoutType.NoSchedule.GetDescription());
        }

        _def.ItemValue = string.Join(",", descriptions);
        _def.IsHidden = checkHidden.Checked;
        _def.ItemColor = butColor.BackColor;

        if (_def.DefNum == 0)
        {
            DefL.Insert(_def);
        }
        else
        {
            DefL.Update(_def);
        }

        DialogResult = DialogResult.OK;
    }
}