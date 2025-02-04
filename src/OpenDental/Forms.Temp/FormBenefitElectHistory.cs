using System;
using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormBenefitElectHistory : FormODBase
{
    private List<Etrans> _etranses;
    private readonly long _planNum;
    private readonly long _patPlanNum;
    private readonly long _subNum;
    private readonly long _subPatNum;
    private readonly long _carrierNum;
    private Patient[] _patients;
    public List<Benefit> ListBenefits;

    public FormBenefitElectHistory(long planNum, long patPlanNum, long subNum, long subPatNum, long carrierNum)
    {
        InitializeComponent();

        _planNum = planNum;
        _patPlanNum = patPlanNum;
        _subNum = subNum;
        _subPatNum = subPatNum;
        _carrierNum = carrierNum;
    }

    private void FormBenefitElectHistory_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        _etranses = Etranss.GetList270ForPlan(_planNum, _subNum);

        var patNums = _etranses.Select(x => x.PatNum).ToList();

        patNums.Add(_subPatNum);

        _patients = Patients.GetMultPats(patNums);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Date", 100));
        gridMain.Columns.Add(new GridColumn("Patient", 100));
        gridMain.Columns.Add(new GridColumn("Response", 100));

        gridMain.ListGridRows.Clear();
        foreach (var etrans in _etranses)
        {
            var patNum = etrans.PatNum == 0 ? _subPatNum : etrans.PatNum;
            var patName = Patients.GetOnePat(_patients, patNum).GetNameLFnoPref();

            var gridRow = new GridRow();

            gridRow.Cells.Add(etrans.DateTimeTrans.ToShortDateString());
            gridRow.Cells.Add(patName);
            gridRow.Cells.Add(etrans.Note);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var etrans = _etranses[e.Row];
        if (etrans.Etype == EtransType.Eligibility_CA)
        {
            using var formEtransEdit = new FormEtransEdit();

            formEtransEdit.EtransCur = etrans;
            formEtransEdit.ShowDialog();
        }
        else
        {
            var errorMessage = X271.ValidateSettings();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ShowError(errorMessage);
                return;
            }

            var isDependent = etrans.PatNum != 0 && _subPatNum != etrans.PatNum;
            var carrier = Carriers.GetCarrier(_carrierNum);

            using var formEtrans270Edit = new FormEtrans270Edit(_patPlanNum, _planNum, _subNum, isDependent, _subPatNum, carrier.IsCoinsuranceInverted);

            formEtrans270Edit.EtransCur = etrans;
            formEtrans270Edit.ListBenefits = ListBenefits;
            formEtrans270Edit.ShowDialog();
        }

        FillGrid();
    }
}