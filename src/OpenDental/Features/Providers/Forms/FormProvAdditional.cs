using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Features.Providers.Forms;

public partial class FormProvAdditional : FormODBase
{
    private readonly ProviderDto _providerDto;
    private readonly List<ProviderClinicDto> _providerClinicDtos;
    private ProviderClinicDto _defaultProviderClinicDto;

    public List<ProviderClinicDto> ModifiedProviderClinicDtos { get; set; } = [];

    public FormProvAdditional(List<ProviderClinicDto> providerClinicDtos, ProviderDto providerDto)
    {
        InitializeComponent();

        _providerClinicDtos = providerClinicDtos;
        _providerDto = providerDto;
    }

    private void FormProvAdditional_Load(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        Cursor = Cursors.WaitCursor;

        gridProvProperties.BeginUpdate();

        gridProvProperties.Columns.Clear();
        gridProvProperties.Columns.Add(new GridColumn("Clinic", 120));
        gridProvProperties.Columns.Add(new GridColumn("DEA Num", 120, true));
        gridProvProperties.Columns.Add(new GridColumn("State License Num", 120, true));
        gridProvProperties.Columns.Add(new GridColumn("State Rx ID", 120, true));
        gridProvProperties.Columns.Add(new GridColumn("State Where Licensed", 120, true));

        gridProvProperties.ListGridRows.Clear();

        _defaultProviderClinicDto = _providerClinicDtos.FirstOrDefault(x => x.ClinicId is null);
        if (_defaultProviderClinicDto is null)
        {
            _defaultProviderClinicDto = new ProviderClinicDto();

            _providerClinicDtos.Add(_defaultProviderClinicDto);
        }

        var gridRow = new GridRow();

        gridRow.Cells.Add("Default");
        gridRow.Cells.Add(_defaultProviderClinicDto.DeaNumber);
        gridRow.Cells.Add(_defaultProviderClinicDto.StateLicense);
        gridRow.Cells.Add(_defaultProviderClinicDto.StateRxId);
        gridRow.Cells.Add(_defaultProviderClinicDto.StateWhereLicensed);
        gridRow.Tag = _defaultProviderClinicDto;

        gridProvProperties.ListGridRows.Add(gridRow);

        var clinicDtos = Clinics.GetForUserod(Security.CurUser);

        foreach (var clinicDto in clinicDtos)
        {
            gridRow = new GridRow();

            var providerClinicDto = _providerDto.Clinics.FirstOrDefault(x => x.ClinicId == clinicDto.Id);
            if (providerClinicDto is null)
            {
                providerClinicDto = new ProviderClinicDto
                {
                    ClinicId = clinicDto.Id
                };

                _providerDto.Clinics.Add(providerClinicDto);
            }

            gridRow.Cells.Add(clinicDto.Abbr);
            gridRow.Cells.Add(providerClinicDto.DeaNumber);
            gridRow.Cells.Add(providerClinicDto.StateLicense);
            gridRow.Cells.Add(providerClinicDto.StateRxId);
            gridRow.Cells.Add(providerClinicDto.StateWhereLicensed);
            gridRow.Tag = providerClinicDto;

            gridProvProperties.ListGridRows.Add(gridRow);
        }

        gridProvProperties.EndUpdate();

        Cursor = Cursors.Default;
    }

    private void GridProvProperties_CellLeave(object sender, ODGridClickEventArgs e)
    {
        var selectedGridRow = gridProvProperties.SelectedGridRows.First();
        if (selectedGridRow is null)
        {
            return;
        }

        var providerClinic = (ProviderClinicDto) selectedGridRow.Tag;
        var value = SIn.String(selectedGridRow.Cells[e.Col].Text);

        switch (e.Col)
        {
            case 1:
                providerClinic.DeaNumber = value;
                break;

            case 2:
                providerClinic.StateLicense = value;
                break;

            case 3:
                providerClinic.StateRxId = value;
                break;

            case 4:
                providerClinic.StateWhereLicensed = value;
                break;
        }
    }

    private static bool IsEmpty(ProviderClinicDto providerClinicDto)
    {
        return providerClinicDto is not null &&
               string.IsNullOrEmpty(providerClinicDto.DeaNumber) &&
               string.IsNullOrEmpty(providerClinicDto.StateLicense) &&
               string.IsNullOrEmpty(providerClinicDto.StateRxId) &&
               string.IsNullOrEmpty(providerClinicDto.StateWhereLicensed);
    }

    private bool IsProviderClinicModified(ProviderClinicDto providerClinic)
    {
        if (providerClinic is null)
        {
            return false;
        }

        return providerClinic == _defaultProviderClinicDto || !IsEmpty(providerClinic);
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        var providerClinicDtos = gridProvProperties.ListGridRows.Select(x => (ProviderClinicDto) x.Tag).ToList();

        ModifiedProviderClinicDtos = [];

        foreach (var providerClinicDto in providerClinicDtos)
        {
            if (!IsProviderClinicModified(providerClinicDto))
            {
                continue;
            }

            ModifiedProviderClinicDtos.Add(providerClinicDto);
        }

        DialogResult = DialogResult.OK;
    }
}