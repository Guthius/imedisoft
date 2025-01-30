using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormProvAdditional : FormODBase
{
    public List<ProviderClinic> ListProviderClinicsOut = [];

    private readonly Provider _provider;
    private readonly List<ProviderClinic> _providerClinics;
    private ProviderClinic _providerClinic;

    public FormProvAdditional(List<ProviderClinic> providerClinics, Provider provider)
    {
        InitializeComponent();

        _providerClinics = providerClinics.Select(x => x.Copy()).ToList();
        _provider = provider;
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
        _providerClinic = _providerClinics.Find(x => x.ClinicNum == 0);

        if (_providerClinic == null)
        {
            _providerClinic = ProviderClinics.GetOne(_provider.ProvNum, 0) ?? new ProviderClinic
            {
                ProvNum = _provider.ProvNum,
                ClinicNum = 0,
                DEANum = _provider.DEANum,
                StateLicense = _provider.StateLicense,
                StateRxID = _provider.StateRxID,
                StateWhereLicensed = _provider.StateWhereLicensed
            };

            _providerClinics.Add(_providerClinic);
        }

        var gridRow = new GridRow();
        
        gridRow.Cells.Add("Default");
        gridRow.Cells.Add(_providerClinic.DEANum);
        gridRow.Cells.Add(_providerClinic.StateLicense);
        gridRow.Cells.Add(_providerClinic.StateRxID);
        gridRow.Cells.Add(_providerClinic.StateWhereLicensed);
        gridRow.Tag = _providerClinic;
        
        gridProvProperties.ListGridRows.Add(gridRow);
        
        var clinicDtos = Clinics.GetForUserod(Security.CurUser);
        foreach (var clinicDto in clinicDtos)
        {
            gridRow = new GridRow();
            
            var providerClinic = _providerClinics.Find(x => x.ClinicNum == clinicDto.Id);
            if (providerClinic == null)
            {
                providerClinic = new ProviderClinic
                {
                    ProvNum = _provider.ProvNum,
                    ClinicNum = clinicDto.Id
                };
                
                _providerClinics.Add(providerClinic);
            }

            gridRow.Cells.Add(clinicDto.Abbr);
            gridRow.Cells.Add(providerClinic.DEANum);
            gridRow.Cells.Add(providerClinic.StateLicense);
            gridRow.Cells.Add(providerClinic.StateRxID);
            gridRow.Cells.Add(providerClinic.StateWhereLicensed);
            gridRow.Tag = providerClinic;
            
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

        var providerClinic = (ProviderClinic) selectedGridRow.Tag;
        var value = SIn.String(selectedGridRow.Cells[e.Col].Text);
        
        switch (e.Col)
        {
            case 1:
                providerClinic.DEANum = value;
                break;
            
            case 2:
                providerClinic.StateLicense = value;
                break;
            
            case 3:
                providerClinic.StateRxID = value;
                break;
            
            case 4:
                providerClinic.StateWhereLicensed = value;
                break;
        }
    }

    private static bool IsEmpty(ProviderClinic providerClinic)
    {
        return providerClinic is not null && 
               string.IsNullOrEmpty(providerClinic.DEANum) && 
               string.IsNullOrEmpty(providerClinic.StateLicense) && 
               string.IsNullOrEmpty(providerClinic.StateRxID) && 
               string.IsNullOrEmpty(providerClinic.StateWhereLicensed) && 
               string.IsNullOrEmpty(providerClinic.CareCreditMerchantId);
    }

    private bool IsProviderClinicModified(ProviderClinic providerClinic)
    {
        if (providerClinic is null)
        {
            return false;
        }
        
        return providerClinic == _providerClinic || !IsEmpty(providerClinic);
    }

    private void ButtonAccept_Click(object sender, EventArgs e)
    {
        var providerClinics = gridProvProperties.ListGridRows.Select(x => (ProviderClinic) x.Tag).ToList();
        
        ListProviderClinicsOut = [];
        
        foreach (var providerClinic in providerClinics)
        {
            if (!IsProviderClinicModified(providerClinic))
            {
                continue;
            }

            ListProviderClinicsOut.Add(providerClinic);
        }

        DialogResult = DialogResult.OK;
    }
}