using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;

namespace OpenDental;

public partial class FormHL7DefSegmentEdit : FormODBase
{
    public HL7DefSegment HL7DefSegmentCur;
    public bool IsHL7DefInternal;
    public HL7InternalType HL7InternalType_;
    
    public FormHL7DefSegmentEdit()
    {
        InitializeComponent();
    }

    private void FormHL7DefSegmentEdit_Load(object sender, EventArgs e)
    {
        FillGrid();
        
        for (var i = 0; i < Enum.GetNames(typeof(SegmentNameHL7)).Length; i++)
        {
            comboSegmentName.Items.Add(Enum.GetName(typeof(SegmentNameHL7), i));
        }

        if (HL7DefSegmentCur is not null)
        {
            comboSegmentName.SelectedIndex = (int) HL7DefSegmentCur.SegmentName;
            textItemOrder.Text = HL7DefSegmentCur.ItemOrder.ToString();
            checkCanRepeat.Checked = HL7DefSegmentCur.CanRepeat;
            checkIsOptional.Checked = HL7DefSegmentCur.IsOptional;
            textNote.Text = HL7DefSegmentCur.Note;
        }

        if (!IsHL7DefInternal && HL7InternalType_ != HL7InternalType.MedLabv2_3)
        {
            return;
        }
        
        butSave.Enabled = false;
        butDelete.Enabled = false;
        labelDelete.Visible = true;
        butAdd.Enabled = false;
            
        if (HL7InternalType_ == HL7InternalType.MedLabv2_3)
        {
            labelDelete.Text = "The segments and their item orders cannot be modified in a MedLabv2_3 definition.";
        }
    }

    private void FillGrid()
    {
        if (!IsHL7DefInternal && !HL7DefSegmentCur.IsNew)
        {
            HL7DefSegmentCur.hl7DefFields = HL7DefFields.GetFromDb(HL7DefSegmentCur.HL7DefSegmentNum);
        }

        gridMain.BeginUpdate();
        
        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Field Name", 180));
        gridMain.Columns.Add(new GridColumn("Fixed Text", 240));
        gridMain.Columns.Add(new GridColumn("Type", 40));
        gridMain.Columns.Add(new GridColumn("Order", 40, HorizontalAlignment.Center));
        gridMain.Columns.Add(new GridColumn("Table ID", 75));
        
        gridMain.ListGridRows.Clear();
        
        if (HL7DefSegmentCur is {hl7DefFields: not null})
        {
            foreach (var hl7DefField in HL7DefSegmentCur.hl7DefFields)
            {
                var gridRow = new GridRow();
                
                gridRow.Cells.Add(hl7DefField.FieldName);
                gridRow.Cells.Add(hl7DefField.FixedText);
                gridRow.Cells.Add(hl7DefField.DataType.ToString());
                gridRow.Cells.Add(hl7DefField.OrdinalPos.ToString());
                gridRow.Cells.Add(hl7DefField.TableId);
                
                gridMain.ListGridRows.Add(gridRow);
            }
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formHL7DefFieldEdit = new FormHL7DefFieldEdit();
        
        formHL7DefFieldEdit.HL7DefFieldCur = HL7DefSegmentCur.hl7DefFields[e.Row];
        formHL7DefFieldEdit.IsHL7DefInternal = IsHL7DefInternal;
        formHL7DefFieldEdit.ShowDialog();
        
        FillGrid();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (!ConfirmOk("Delete Segment?"))
        {
            return;
        }

        foreach (var hl7DefField in HL7DefSegmentCur.hl7DefFields)
        {
            HL7DefFields.Delete(hl7DefField.HL7DefFieldNum);
        }
        
        HL7DefSegments.Delete(HL7DefSegmentCur.HL7DefSegmentNum);
        
        DialogResult = DialogResult.OK;
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (HL7DefSegmentCur.IsNew)
        {
            HL7DefSegments.Insert(HL7DefSegmentCur);
            HL7DefSegmentCur.IsNew = false;
        }

        var hl7DefField = new HL7DefField
        {
            HL7DefSegmentNum = HL7DefSegmentCur.HL7DefSegmentNum,
            IsNew = true,
            FixedText = ""
        };
        
        using var formHL7DefFieldEdit = new FormHL7DefFieldEdit();

        formHL7DefFieldEdit.HL7DefFieldCur = hl7DefField;
        formHL7DefFieldEdit.IsHL7DefInternal = false;
        formHL7DefFieldEdit.ShowDialog();
        
        FillGrid();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textItemOrder.IsValid())
        {
            ShowError("Please fix data entry error first.");
            return;
        }

        HL7DefSegmentCur.SegmentName = (SegmentNameHL7) comboSegmentName.SelectedIndex;
        HL7DefSegmentCur.ItemOrder = SIn.Int(textItemOrder.Text);
        HL7DefSegmentCur.CanRepeat = checkCanRepeat.Checked;
        HL7DefSegmentCur.IsOptional = checkIsOptional.Checked;
        HL7DefSegmentCur.Note = textNote.Text;
        
        if (HL7DefSegmentCur.ItemOrder == 0 && HL7DefSegmentCur.SegmentName == SegmentNameHL7.MSH)
        {
            foreach (var hl7DefField in HL7DefSegmentCur.hl7DefFields)
            {
                switch (hl7DefField.FieldName)
                {
                    case "separators^~\\&" when hl7DefField.OrdinalPos != 1:
                        ShowError("The separators^~\\& field must be in position 1 of the message header segment.");
                        return;
                    
                    case "messageType" when hl7DefField.OrdinalPos != 8:
                        ShowError("The messageType field must be in position 8 of the message header segment.");
                        return;
                }
            }
        }

        if (HL7DefSegmentCur.IsNew)
        {
            HL7DefSegments.Insert(HL7DefSegmentCur);
        }
        else
        {
            HL7DefSegments.Update(HL7DefSegmentCur);
        }

        DialogResult = DialogResult.OK;
    }
}