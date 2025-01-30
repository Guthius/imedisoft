using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormAutoNoteControlEdit : FormODBase
{
    private readonly AutoNoteControl _autoNoteControl;

    public FormAutoNoteControlEdit(AutoNoteControl autoNoteControl)
    {
        _autoNoteControl = autoNoteControl;

        InitializeComponent();
    }

    private void FormAutoNoteControlEdit_Load(object sender, EventArgs e)
    {
        textBoxControlDescript.Text = _autoNoteControl.Descript;
        textBoxControlLabel.Text = _autoNoteControl.ControlLabel;

        comboType.Items.Clear();
        comboType.Items.Add("Text");
        comboType.Items.Add("OneResponse");
        comboType.Items.Add("MultiResponse");
        comboType.SelectedItem = _autoNoteControl.ControlType;

        textOptions.Text = _autoNoteControl.ControlOptions;
    }

    private void ComboBoxType_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (comboType.GetSelected<string>())
        {
            case "Text":
                labelResponses.Text = "Default text";
                butAutoNoteResp.Visible = false;
                break;

            case "OneResponse":
                labelResponses.Text = "Possible responses (one line per item)";
                butAutoNoteResp.Visible = true;
                break;

            case "MultiResponse":
                labelResponses.Text = "Possible responses (one line per item)";
                butAutoNoteResp.Visible = false;
                break;
        }
    }

    private void ButtonUp_Click(object sender, EventArgs e)
    {
        if (textOptions.Text == "")
        {
            return;
        }

        var selStartNum = textOptions.SelectionStart;
        var selectedRowNum = 0;
        var sumPreviousLines = 0;
        var strArrayLinesOrig = new string[textOptions.Lines.Length];

        textOptions.Lines.CopyTo(strArrayLinesOrig, 0);

        for (var l = 0; l < textOptions.Lines.Length; l++)
        {
            if (l > 0)
            {
                sumPreviousLines += textOptions.Lines[l - 1].Length + 2;
            }

            if (selStartNum >= sumPreviousLines + textOptions.Lines[l].Length)
            {
                continue;
            }

            selectedRowNum = l;
            break;
        }

        int newSelectedRowNum;
        if (selectedRowNum == 0)
        {
            newSelectedRowNum = 0;
        }
        else
        {
            var newText = "";
            for (var l = 0; l < textOptions.Lines.Length; l++)
            {
                if (l > 0)
                {
                    newText += "\r\n";
                }

                if (l == selectedRowNum)
                {
                    newText += strArrayLinesOrig[selectedRowNum - 1];
                }
                else if (l == selectedRowNum - 1)
                {
                    newText += strArrayLinesOrig[selectedRowNum];
                }
                else
                {
                    newText += strArrayLinesOrig[l];
                }
            }

            textOptions.Text = newText;
            newSelectedRowNum = selectedRowNum - 1;
        }

        sumPreviousLines = 0;
        for (var l = 0; l < textOptions.Lines.Length; l++)
        {
            if (l > 0)
            {
                sumPreviousLines += textOptions.Lines[l - 1].Length + 2;
            }

            if (newSelectedRowNum != l)
            {
                continue;
            }

            textOptions.Select(sumPreviousLines, textOptions.Lines[l].Length);
            break;
        }
    }

    private void ButtonDown_Click(object sender, EventArgs e)
    {
        if (textOptions.Text == "")
        {
            return;
        }

        var selectionStart = textOptions.SelectionStart;

        var selectedRow = 0;
        var sumPreviousLines = 0;
        var stringArrayLinesOrig = new string[textOptions.Lines.Length];

        textOptions.Lines.CopyTo(stringArrayLinesOrig, 0);
        for (var l = 0; l < textOptions.Lines.Length; l++)
        {
            if (l > 0)
            {
                sumPreviousLines += textOptions.Lines[l - 1].Length + 2;
            }

            if (selectionStart >= sumPreviousLines + textOptions.Lines[l].Length)
            {
                continue;
            }

            selectedRow = l;
            break;
        }

        int newSelectedRowNum;
        if (selectedRow == textOptions.Lines.Length - 1)
        {
            newSelectedRowNum = textOptions.Lines.Length - 1;
        }
        else
        {
            var newText = "";
            for (var i = 0; i < textOptions.Lines.Length; i++)
            {
                if (i > 0)
                {
                    newText += "\r\n";
                }

                if (i == selectedRow)
                {
                    newText += stringArrayLinesOrig[selectedRow + 1];
                }
                else if (i == selectedRow + 1)
                {
                    newText += stringArrayLinesOrig[selectedRow];
                }
                else
                {
                    newText += stringArrayLinesOrig[i];
                }
            }

            textOptions.Text = newText;
            newSelectedRowNum = selectedRow + 1;
        }

        sumPreviousLines = 0;
        for (var l = 0; l < textOptions.Lines.Length; l++)
        {
            if (l > 0)
            {
                sumPreviousLines += textOptions.Lines[l - 1].Length + 2;
            }

            if (newSelectedRowNum != l)
            {
                continue;
            }

            textOptions.Select(sumPreviousLines, textOptions.Lines[l].Length);
            break;
        }
    }

    private void ButtonAutoNoteResp_Click(object sender, EventArgs e)
    {
        if (comboType.GetSelected<string>() != "OneResponse")
        {
            ShowError("Can only add AutoNotes to single response types.");
            return;
        }

        using var formAutoNoteResponsePicker = new FormAutoNoteResponsePicker();
        if (formAutoNoteResponsePicker.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var selectedResponse = formAutoNoteResponsePicker.AutoNoteResponseText;
        if (textOptions.SelectionStart < textOptions.Text.Length - 1)
        {
            textOptions.Text = textOptions.Text.Insert(textOptions.SelectionStart, selectedResponse);
        }
        else
        {
            textOptions.Text += selectedResponse + "\r\n";
        }
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_autoNoteControl.AutoNoteControlNum == 0)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!ConfirmOk("Completely delete this prompt? It will not be available from any AutoNote."))
        {
            return;
        }

        AutoNoteControls.Delete(_autoNoteControl.AutoNoteControlNum);

        DialogResult = DialogResult.OK;
    }

    private void TextBoxControlLabel_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textBoxControlDescript.Text == "" || comboType.SelectedIndex == -1)
        {
            ShowError("Please make sure that the Description and Type are not blank");
            return;
        }

        if (!Regex.IsMatch(textBoxControlDescript.Text, "^[a-zA-Z_0-9 ]*$"))
        {
            ShowError("The description can only contain letters, numbers, underscore, and space.");
            return;
        }

        _autoNoteControl.Descript = textBoxControlDescript.Text;
        _autoNoteControl.ControlLabel = textBoxControlLabel.Text;
        _autoNoteControl.ControlType = comboType.GetSelected<string>();
        _autoNoteControl.ControlOptions = textOptions.Text;

        if (_autoNoteControl.AutoNoteControlNum == 0)
        {
            AutoNoteControls.Insert(_autoNoteControl);
        }
        else
        {
            AutoNoteControls.Update(_autoNoteControl);
        }

        DialogResult = DialogResult.OK;
    }
}