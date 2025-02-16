using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormFeeSchedGroups : FormODBase
{
    private List<ClinicDto> _clinicDtos;
    private List<ClinicDto> _clinicDtosForGroup = [];
    private List<FeeSchedGroup> _feeSchedGroups;

    public FormFeeSchedGroups()
    {
        InitializeComponent();
    }

    private void FormFeeSchedGroups_Load(object sender, EventArgs e)
    {
        SetFilterControlsAndAction(FilterFeeSchedGroups, textFeeSched);

        _clinicDtos = Clinics
            .GetWhere(x => x.IsHidden == false)
            .OrderBy(x => x.Abbr)
            .ToList();

        _feeSchedGroups = FeeSchedGroups.GetAll().OrderBy(x => x.Description).ToList();

        ListTools.DeepCopy<FeeSchedGroup, FeeSchedGroup>(_feeSchedGroups);

        FillClinicCombo();
        FilterFeeSchedGroups();
    }

    private void FillClinicCombo()
    {
        comboClinic.Items.Clear();
        comboClinic.Items.Add("All");

        foreach (var clinicDto in _clinicDtos)
        {
            comboClinic.Items.Add(clinicDto.Abbr);
        }

        comboClinic.SelectedIndex = 0;
    }

    private void ComboBoxClinic_SelectionChanged(object sender, EventArgs e)
    {
        FilterFeeSchedGroups();
    }

    private void FilterFeeSchedGroups()
    {
        var filteredFeeScheds = FeeScheds.GetWhere(x => x.Description.ToLower().Contains(textFeeSched.Text.ToLower()));
        var filteredClinics = comboClinic.SelectedIndex == 0 ? _clinicDtos : ListTools.FromSingle(_clinicDtos[comboClinic.SelectedIndex - 1]);

        _feeSchedGroups
            .Where(x => filteredFeeScheds.Select(y => y.FeeSchedNum).Contains(x.FeeSchedNum))
            .Where(x => x.ListClinicNumsAll.Any(y => filteredClinics.Select(z => z.Id).Contains(y)))
            .ToList();

        FillGridGroups();
        FillGridClinics();
    }

    private void FillGridGroups()
    {
        gridGroups.BeginUpdate();

        gridGroups.Columns.Clear();
        gridGroups.Columns.Add(new GridColumn("Group Name", 200));
        gridGroups.Columns.Add(new GridColumn("Fee Schedule", 75));

        gridGroups.ListGridRows.Clear();

        foreach (var feeSchedGroup in _feeSchedGroups)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(feeSchedGroup.Description);
            gridRow.Cells.Add(FeeScheds.GetDescription(feeSchedGroup.FeeSchedNum));
            gridRow.Tag = feeSchedGroup;

            gridGroups.ListGridRows.Add(gridRow);
        }

        gridGroups.EndUpdate();
    }

    private void FillGridClinics()
    {
        _clinicDtosForGroup.Clear();
        if (gridGroups.GetSelectedIndex() >= 0)
        {
            _clinicDtosForGroup = Clinics.GetClinics(gridGroups.SelectedTag<FeeSchedGroup>().ListClinicNumsAll).OrderBy(x => x.Abbr).ToList();
        }

        gridClinics.BeginUpdate();

        gridClinics.Columns.Clear();
        gridClinics.Columns.Add(new GridColumn("Abbr", 100) {IsWidthDynamic = true});
        gridClinics.Columns.Add(new GridColumn("Description", 100) {IsWidthDynamic = true, DynamicWeight = 2});

        gridClinics.ListGridRows.Clear();

        foreach (var clinicDto in _clinicDtosForGroup)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(clinicDto.Abbr);
            gridRow.Cells.Add(clinicDto.Description + (clinicDto.IsHidden ? " (Hidden)" : ""));
            gridRow.Tag = clinicDto;

            gridClinics.ListGridRows.Add(gridRow);
        }

        gridClinics.EndUpdate();
    }

    private void GridGroups_CellClick(object sender, ODGridClickEventArgs e)
    {
        FillGridClinics();
    }

    private void GridGroups_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var feeSchedGroup = (FeeSchedGroup) gridGroups.ListGridRows[e.Row].Tag;

        using var formFeeSchedGroupEdit = new FormFeeSchedGroupEdit(feeSchedGroup);

        if (formFeeSchedGroupEdit.ShowDialog() == DialogResult.OK)
        {
            FeeSchedGroups.Update(feeSchedGroup);
        }

        FilterFeeSchedGroups();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var feeSchedGroup = new FeeSchedGroup
        {
            ListClinicNumsAll = [],
            IsNew = true
        };

        using var formFeeSchedGroupEdit = new FormFeeSchedGroupEdit(feeSchedGroup);

        if (formFeeSchedGroupEdit.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        FeeSchedGroups.Insert(feeSchedGroup);

        _feeSchedGroups.Add(feeSchedGroup);
        _feeSchedGroups = _feeSchedGroups.OrderBy(x => x.Description).ToList();

        FilterFeeSchedGroups();
    }
}