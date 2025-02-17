using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormSheetProcSelect : FormODBase
{
    public List<long> SelectedProcNums { get; set; }
    public long PatNum;

    public FormSheetProcSelect()
    {
        InitializeComponent();
    }

    private void FormSheetProcSelect_Load(object sender, EventArgs e)
    {
        FillGridProcs();

        gridProcs.ScrollToEnd();
    }

    private void FillGridProcs()
    {
        var procedures = Procedures.GetPatientData(PatNum);

        gridProcs.BeginUpdate();

        gridProcs.Columns.Clear();
        gridProcs.Columns.Add(new GridColumn("Date", 67, HorizontalAlignment.Left));
        gridProcs.Columns.Add(new GridColumn("Th", 27, HorizontalAlignment.Left));
        gridProcs.Columns.Add(new GridColumn("Surf", 40, HorizontalAlignment.Left));
        gridProcs.Columns.Add(new GridColumn("Description", 318, HorizontalAlignment.Left));
        gridProcs.Columns.Add(new GridColumn("Stat", 30, HorizontalAlignment.Left));
        gridProcs.Columns.Add(new GridColumn("Amount", 63, HorizontalAlignment.Right));
        gridProcs.Columns.Add(new GridColumn("Code", 0, HorizontalAlignment.Center));

        gridProcs.ListGridRows.Clear();
        
        foreach (var procedure in procedures)
        {
            var procedureCode = ProcedureCodes.GetProcCode(procedure.CodeNum);

            var displaySurf = ProcedureCodes.GetProcCode(procedure.CodeNum).TreatArea == TreatmentArea.Sextant
                ? Tooth.GetSextant(procedure.Surf, (ToothNumberingNomenclature) PrefC.GetInt(PrefName.UseInternationalToothNumbers))
                : Tooth.SurfTidyFromDbToDisplay(procedure.Surf, procedure.ToothNum);

            var gridRow = new GridRow();

            gridRow.Cells.Add(procedure.ProcDate.ToShortDateString());
            gridRow.Cells.Add(Tooth.Display(procedure.ToothNum));
            gridRow.Cells.Add(displaySurf);
            gridRow.Cells.Add(procedureCode.LaymanTerm == "" ? procedureCode.Descript : procedureCode.LaymanTerm);
            gridRow.Cells.Add(ProcMultiVisits.IsProcInProcess(procedure.ProcNum) ? ProcStatExt.InProcess : procedure.ProcStatus.ToString());
            gridRow.Cells.Add(procedure.ProcFee.ToString("C"));
            gridRow.Cells.Add(procedureCode.ProcCode);
            gridRow.Tag = procedure;

            gridProcs.ListGridRows.Add(gridRow);
        }

        gridProcs.EndUpdate();
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        if (gridProcs.SelectedIndices.Length == 0)
        {
            ShowError("Please select at least 1 procedure.");
            return;
        }

        SelectedProcNums = gridProcs.SelectedTags<Procedure>().Select(x => x.ProcNum).ToList();

        DialogResult = DialogResult.OK;
    }
}