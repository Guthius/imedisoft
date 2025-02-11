using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using CommunityToolkit.Mvvm.DependencyInjection;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using LanguageExt;
using LanguageExt.Common;
using OpenDental.Core.Services;
using OpenDental.Extensions;
using OpenDental.Features.Providers.ViewModels;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental.Features.Providers.Forms;

public partial class FormProvEdit : FormODBase
{
    private readonly ProviderDto _providerDto;

    public FormProvEdit(ProviderDto providerDto)
    {
        _providerDto = providerDto;

        InitializeComponent();

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            labelNPI.Text = "CDA Number";
        }
        else
        {
            labelCanadianOfficeNum.Visible = false;
            textCanadianOfficeNum.Visible = false;
        }
    }

    private void FormProvEdit_Load(object sender, EventArgs e)
    {
        if (_providerDto.Id != 0)
        {
            textProviderID.Text = _providerDto.Id.ToString();
        }

        textAbbr.Text = _providerDto.Abbr;
        textLName.Text = _providerDto.LastName;
        textFName.Text = _providerDto.FirstName;
        textMI.Text = _providerDto.MiddleName;
        textSuffix.Text = _providerDto.Suffix;
        textPreferredName.Text = _providerDto.PreferredName;
        textSSN.Text = _providerDto.Ssn;

        dateTerm.SetDateTime(_providerDto.TerminatedOn ?? DateTime.MinValue);

        if (_providerDto.IsTin)
        {
            radioTIN.Checked = true;
        }
        else
        {
            radioSSN.Checked = true;
        }

        _providerDto.Clinics = _providerDto.Clinics;

        if (_providerDto.Clinics.Any(x => x.ClinicId is null))
        {
            _providerDto.Clinics.Add(new ProviderClinicDto());
        }

        textMedicaidID.Text = _providerDto.MedicaidId;
        textNationalProvID.Text = _providerDto.NationalProviderId;
        textCanadianOfficeNum.Text = _providerDto.CanadianOfficeNumber;
        textSchedRules.Text = _providerDto.SchedulerNote;
        textBirthdate.Text = "";
        textProdGoalHr.Text = _providerDto.HourlyProductionGoal.ToString("f");

        checkIsSecondary.Checked = _providerDto.IsSecondary;
        checkSigOnFile.Checked = _providerDto.IsSignatureOnFile;
        checkIsHidden.Checked = _providerDto.IsHidden;
        checkIsHiddenOnReports.Checked = _providerDto.IsHiddenFromReports;

        odColorPickerAppt.AllowTransparentColor = true;
        odColorPickerOutline.AllowTransparentColor = true;
        odColorPickerAppt.BackgroundColor = ColorTranslator.FromHtml(_providerDto.Color);
        odColorPickerOutline.BackgroundColor = ColorTranslator.FromHtml(_providerDto.OutlineColor);

        if (_providerDto.DateOfBirth is not null)
        {
            textBirthdate.Text = _providerDto.DateOfBirth.Value.ToShortDateString();
        }

        var feeScheds = FeeScheds.GetDeepCopy(true);
        foreach (var feeSched in feeScheds)
        {
            listFeeSched.Items.Add(feeSched);
            if (feeSched.FeeSchedNum == _providerDto.FeeScheduleId)
            {
                listFeeSched.SelectedItem = feeSched;
            }
        }

        if (listFeeSched.SelectedIndex == -1)
        {
            listFeeSched.SelectedIndex = 0;
        }

        listSpecialty.Items.Clear();

        var providerSpecialtyDtos = ProviderService.GetSpecialties();
        foreach (var yy in providerSpecialtyDtos)
        {
            listSpecialty.Items.Add(yy);
            if (yy.Id == _providerDto.Specialty.Id)
            {
                listSpecialty.SelectedItem = yy;
            }
        }

        textTaxonomyOverride.Text = _providerDto.TaxonomyCode;

        FillGridProvIdent();

        checkIsCDAnet.Checked = _providerDto.IsCdaNet;

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            checkIsCDAnet.Visible = true;
        }

        checkIsNotPerson.Checked = _providerDto.IsNotPerson;

        comboProv.Items.AddProvNone();
        comboProv.Items.AddProvsFull(Imedisoft.Core.Features.Providers.Providers.GetDeepCopy(true));
        comboProv.SetSelectedProvNum(_providerDto.BillingProvider?.Id ?? 0);

        if (_providerDto.IsDeleted)
        {
            DisableAllExcept();
        }
    }

    private void FormProvEdit_Closing(object sender, CancelEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            DataValid.SetInvalid(InvalidType.Providers);
        }
    }

    private void RadioButtonSsn_Click(object sender, EventArgs e)
    {
        _providerDto.IsTin = false;
    }

    private void RadioButtonTin_Click(object sender, EventArgs e)
    {
        _providerDto.IsTin = true;
    }

    private void FillGridProvIdent()
    {
        gridProvIdent.BeginUpdate();

        gridProvIdent.Columns.Clear();
        gridProvIdent.Columns.Add(new GridColumn("Payor ID", 90, HorizontalAlignment.Center));
        gridProvIdent.Columns.Add(new GridColumn("Type", 110, HorizontalAlignment.Center));
        gridProvIdent.Columns.Add(new GridColumn("ID Number", 100, HorizontalAlignment.Center));
        gridProvIdent.ListGridRows.Clear();

        foreach (var providerIdentityDto in _providerDto.Identities)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(providerIdentityDto.PayorId);
            gridRow.Cells.Add(providerIdentityDto.Type);
            gridRow.Cells.Add(providerIdentityDto.Value);
            gridRow.Tag = providerIdentityDto;

            gridProvIdent.ListGridRows.Add(gridRow);
        }

        gridProvIdent.EndUpdate();
    }

    private void GridProvIdent_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        var providerIdentityDto = gridProvIdent.SelectedTag<ProviderIdentityDto>();
        if (providerIdentityDto is null)
        {
            return;
        }

        var dialogService = Ioc.Default.GetRequiredService<IDialogService>();

        var providerIdentityViewModel = new ProviderIdentityViewModel(providerIdentityDto);

        dialogService.Show(providerIdentityViewModel);

        FillGridProvIdent();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var providerIdentityDto = new ProviderIdentityDto();

        var dialogService = Ioc.Default.GetRequiredService<IDialogService>();

        var providerIdentityViewModel = new ProviderIdentityViewModel(providerIdentityDto);

        var result = dialogService.Show(providerIdentityViewModel);
        if (result is not true)
        {
            return;
        }

        _providerDto.Identities.Add(providerIdentityDto);

        FillGridProvIdent();
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        var providerIdentityDto = gridProvIdent.SelectedTag<ProviderIdentityDto>();
        if (providerIdentityDto is null)
        {
            ShowError("Please select an item first.");
            return;
        }

        if (!ConfirmOk("Delete the selected Provider Identifier?"))
        {
            return;
        }

        _providerDto.Identities.Remove(providerIdentityDto);

        FillGridProvIdent();
    }

    private void ButtonClinicOverrides_Click(object sender, EventArgs e)
    {
        using var formProvAdditional = new FormProvAdditional(_providerDto.Clinics, _providerDto);

        if (formProvAdditional.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _providerDto.Clinics = formProvAdditional.ModifiedProviderClinicDtos;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!dateTerm.IsValid())
        {
            ShowError("Term Date invalid.");
            return;
        }

        if (textAbbr.Text == "")
        {
            ShowError("Abbreviation not allowed to be blank.");
            return;
        }

        if (textSSN.Text.Contains("-"))
        {
            ShowError("SSN/TIN not allowed to have dash.");
            return;
        }

        if (checkIsHidden.Checked)
        {
            if (PrefC.GetLong(PrefName.PracticeDefaultProv) == _providerDto.Id)
            {
                ShowError("Not allowed to hide practice default provider.");
                return;
            }

            if (Clinics.IsDefaultClinicProvider(_providerDto.Id))
            {
                ShowError("Not allowed to hide a clinic default provider.");
                return;
            }

            if (PrefC.GetLong(PrefName.InsBillingProv) == _providerDto.Id)
            {
                if (!Confirm("You are about to hide the default ins billing provider. Continue?"))
                {
                    return;
                }
            }

            if (Clinics.IsInsBillingProvider(_providerDto.Id))
            {
                if (!Confirm("You are about to hide a clinic ins billing provider. Continue?"))
                {
                    return;
                }
            }

            var listApptViewItems = ApptViewItems.GetForProvider(_providerDto.Id);
            if (!listApptViewItems.IsNullOrEmpty())
            {
                #region Provider Attached to View Check

                //Create a list of Provider associated Appointment Views
                var listApptView = listApptViewItems.Select(x => ApptViews.GetApptView(x.ApptViewNum)).Where(x => x != null).ToList();
                //This list must be distincted before being shown to the user. A single Provider can be associated to the same "view" multiple times.
                if (!listApptView.IsNullOrEmpty())
                {
                    var listProviderAssociatedViews = string.Join("\r\n", listApptView.Select(x => x.Description).Distinct().OrderBy(x => x));
                    var msg = ("Not allowed to hide a Provider associated to an Appointment View.");
                    msg += "\r\n" + ("To continue remove them from the following Appointment View(s):");
                    msg += "\r\n" + listProviderAssociatedViews;
                    MsgBox.Show(msg);
                    return;
                }

                #endregion
            }
        }

        if (Imedisoft.Core.Features.Providers.Providers.GetExists(x => x.Id != _providerDto.Id && x.Abbr == textAbbr.Text))
        {
            if (!ConfirmOk("This abbreviation is already in use by another provider.  Continue anyway?"))
            {
                return;
            }
        }

        if (CultureInfo.CurrentCulture.Name.EndsWith("CA") && checkIsCDAnet.Checked)
        {
            if (textNationalProvID.Text != Canadian.TidyAN(textNationalProvID.Text, 9, true))
            {
                ShowError("CDA number must be 9 characters long and composed of numbers and letters only.");
                return;
            }

            if (textCanadianOfficeNum.Text != Canadian.TidyAN(textCanadianOfficeNum.Text, 4, true))
            {
                ShowError("Office number must be 4 characters long and composed of numbers and letters only.");
                return;
            }
        }

        if (checkIsNotPerson.Checked)
        {
            if (textFName.Text != "" || textMI.Text != "")
            {
                ShowError("When the 'Not a Person' box is checked, the provider may not have a First Name or Middle Initial entered.");
                return;
            }
        }

        if (checkIsHidden.Checked && !_providerDto.IsHidden)
        {
            if (!ConfirmOk(
                    "If there are any future hours on this provider's schedule, they will be removed.  " +
                    "This does not affect scheduled appointments or any other appointments in any way."))
            {
                return;
            }

            Imedisoft.Core.Features.Providers.Providers.RemoveProvFromFutureSchedule(_providerDto.Id);
        }

        var billingProvider = comboProv.GetSelected<ProviderDto>();
        if (billingProvider.Id == 0)
        {
            billingProvider = null;
        }
        
        if (billingProvider is {Id: > 0, IsNotPerson: false})
        {
            ShowError("E-claim Billing Prov Override cannot be a person.");
            return;
        }

        if (_providerDto.Id > 0 && comboProv.GetSelectedProvNum() == _providerDto.Id)
        {
            ShowError("E-claim Billing Prov Override cannot be the same provider.");
            return;
        }

        DateTime? dateOfBirth = null;
        if (textBirthdate.Text != "")
        {
            if (DateTime.TryParse(textBirthdate.Text, out var dateTime))
            {
                ShowError("Birthdate invalid.");
                return;
            }

            dateOfBirth = dateTime;
        }

        if (!decimal.TryParse(textProdGoalHr.Text, out var hourlyProductionGoal))
        {
            ShowError("Hourly production goal invalid.");
            return;
        }

        if (listSpecialty.SelectedItem is not ProviderSpecialtyDto providerSpecialtyDto)
        {
            ShowError("Select an specialty.");
            return;
        }

        Either<Error, ProviderDto> result;
        if (_providerDto.Id == 0)
        {
            result = ProviderService.Create(new CreateProviderRequest
            {
                SpecialtyId = providerSpecialtyDto.Id,
                Abbr = textAbbr.Text,
                LastName = textLName.Text,
                MiddleName = textMI.Text,
                FirstName = textFName.Text,
                Suffix = textSuffix.Text,
                PreferredName = textPreferredName.Text,
                Ssn = textSSN.Text,
                UsingTin = radioTIN.Checked,
                NationalProviderId = textNationalProvID.Text,
                MedicaidId = textMedicaidID.Text,
                DateOfBirth = dateOfBirth,
                SchedulerNote = textSchedRules.Text,
                FeeScheduleId = listFeeSched.GetSelected<FeeSched>()?.FeeSchedNum,
                HourlyProductionGoal = hourlyProductionGoal,
                BillingProviderId = billingProvider?.Id,
                TaxonomyCode = textTaxonomyOverride.Text,
                Color = ColorTranslator.ToHtml(odColorPickerAppt.BackgroundColor),
                OutlineColor = ColorTranslator.ToHtml(odColorPickerOutline.BackgroundColor),
                IsCdaNet = checkIsCDAnet.Checked,
                CanadianOfficeNumber = textCanadianOfficeNum.Text,
                IsSecondary = checkIsSecondary.Checked,
                IsNotPerson = checkIsNotPerson.Checked,
                IsSignatureOnFile = checkSigOnFile.Checked,
                IsHiddenFromReports = checkIsHiddenOnReports.Checked,
                IsHidden = checkIsHidden.Checked,
                TerminatedOn = dateTerm.GetDateTimeNullable(),
                Clinics = _providerDto.Clinics,
                Identities = _providerDto.Identities
            });
        }
        else
        {
            result = ProviderService.Update(_providerDto.Id, new UpdateProviderRequest
            {
                SpecialtyId = providerSpecialtyDto.Id,
                Abbr = textAbbr.Text,
                LastName = textLName.Text,
                MiddleName = textMI.Text,
                FirstName = textFName.Text,
                Suffix = textSuffix.Text,
                PreferredName = textPreferredName.Text,
                Ssn = textSSN.Text,
                UsingTin = radioTIN.Checked,
                NationalProviderId = textNationalProvID.Text,
                MedicaidId = textMedicaidID.Text,
                DateOfBirth = dateOfBirth,
                SchedulerNote = textSchedRules.Text,
                FeeScheduleId = listFeeSched.GetSelected<FeeSched>()?.FeeSchedNum,
                HourlyProductionGoal = hourlyProductionGoal,
                BillingProviderId = billingProvider?.Id,
                TaxonomyCode = textTaxonomyOverride.Text,
                Color = ColorTranslator.ToHtml(odColorPickerAppt.BackgroundColor),
                OutlineColor = ColorTranslator.ToHtml(odColorPickerOutline.BackgroundColor),
                IsCdaNet = checkIsCDAnet.Checked,
                CanadianOfficeNumber = textCanadianOfficeNum.Text,
                IsSecondary = checkIsSecondary.Checked,
                IsNotPerson = checkIsNotPerson.Checked,
                IsSignatureOnFile = checkSigOnFile.Checked,
                IsHiddenFromReports = checkIsHiddenOnReports.Checked,
                IsHidden = checkIsHidden.Checked,
                TerminatedOn = dateTerm.GetDateTimeNullable(),
                Clinics = _providerDto.Clinics,
                Identities = _providerDto.Identities
            });

            if (_providerDto.TerminatedOn is not null && _providerDto.TerminatedOn.Value < DateTime.Now)
            {
                var claimPaySplits = Claims.GetOutstandingClaimsByProvider(_providerDto.Id, _providerDto.TerminatedOn.Value);

                var stringBuilder = new StringBuilder("Clinic\tPatNum\tPatient Name\tDate of Service\tClaim Status\tFee\tCarrier\r\n");
                foreach (var claimPaySplit in claimPaySplits)
                {
                    stringBuilder.Append(
                        claimPaySplit.ClinicDesc + "\t" +
                        claimPaySplit.PatNum + "\t" +
                        claimPaySplit.PatName + "\t" +
                        claimPaySplit.DateClaim.ToShortDateString() + "\t");

                    switch (claimPaySplit.ClaimStatus)
                    {
                        case "W":
                            stringBuilder.Append("Waiting in Queue\t");
                            break;

                        case "H":
                            stringBuilder.Append("Hold\t");
                            break;

                        case "U":
                            stringBuilder.Append("Unsent\t");
                            break;

                        case "S":
                            stringBuilder.Append("Sent\t");
                            break;
                    }

                    stringBuilder.AppendLine(claimPaySplit.FeeBilled + "\t" + claimPaySplit.Carrier);
                }

                using var msgBoxCopyPaste = new MsgBoxCopyPaste(stringBuilder.ToString());

                msgBoxCopyPaste.Text = "Outstanding Claims for the Provider Whose Term Has Expired";

                if (claimPaySplits.Count > 0)
                {
                    msgBoxCopyPaste.ShowDialog();
                }
            }
        }

        result.Match(_ => DialogResult = DialogResult.OK, error => ShowError(error.Message));
    }
}