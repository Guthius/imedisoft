using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Features.Providers.Dtos;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.Eclaims;

namespace OpenDental;

public partial class FormProvEdit : FormODBase
{
    private readonly ProviderDto _providerDto;
    private List<ClinicDto> _clinicsForUser = [];

    public bool IsNew;

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

        _clinicsForUser = Clinics.GetAllForUserod(Security.CurUser);

        _providerDto.Clinics = _providerDto.Clinics;

        var defaultProviderClinicDto = _providerDto.Clinics.Find(x => x.ClinicId is null);
        if (defaultProviderClinicDto is null)
        {
            defaultProviderClinicDto = new ProviderClinicDto();

            _providerDto.Clinics.Add(defaultProviderClinicDto);
        }

        textDEANum.Text = defaultProviderClinicDto.DeaNumber;
        textStateLicense.Text = defaultProviderClinicDto.StateLicense;
        textStateWhereLicensed.Text = defaultProviderClinicDto.StateWhereLicensed;
        textStateRxID.Text = defaultProviderClinicDto.StateRxId;
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

        listFeeSched.Items.AddList(FeeScheds.GetDeepCopy(true), x => x.Description);
        for (var i = 0; i < listFeeSched.Items.Count; i++)
        {
            if (((FeeSched) listFeeSched.Items.GetObjectAt(i)).FeeSchedNum == _providerDto.FeeScheduleId)
            {
                listFeeSched.SelectedIndex = i;
            }
        }

        if (listFeeSched.SelectedIndex == -1)
        {
            listFeeSched.SelectedIndex = 0;
        }

        listSpecialty.Items.Clear();

        var defArray = Defs.GetDefsForCategory(DefCat.ProviderSpecialties, true).ToArray();
        for (var i = 0; i < defArray.Length; i++)
        {
            listSpecialty.Items.Add(defArray[i].ItemName);
            if (i == 0 || _providerDto.Specialty.Id == defArray[i].DefNum)
            {
                listSpecialty.SelectedIndex = i;
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
        comboProv.Items.AddProvsFull(Providers.GetDeepCopy(true));
        comboProv.SetSelectedProvNum(_providerDto.BillingProvider?.Id ?? 0);

        if (_providerDto.IsDeleted)
        {
            DisableAllExcept();
        }

        var selectAll = _providerDto.Clinics.Count == 0 || _clinicsForUser.All(x => _providerDto.Clinics.Any(y => y.ClinicId == x.Id));
        var visibleClinics = _clinicsForUser.FindAll(x => !x.IsHidden);

        listBoxClinics.Items.AddList(visibleClinics, x => x.Abbr);

        var clinicIdsForClinicLinks = _providerDto.Clinics.Select(x => x.ClinicId).ToList();
        for (var i = 0; i < visibleClinics.Count; i++)
        {
            if (!selectAll && clinicIdsForClinicLinks.Contains(visibleClinics[i].Id))
            {
                listBoxClinics.SetSelected(i);
            }
        }

        checkAllClinics.Checked = selectAll;
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

        var frmProviderIdentEdit = new FrmProviderIdentEdit(providerIdentityDto);

        frmProviderIdentEdit.ShowDialog();

        FillGridProvIdent();
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var providerIdentityDto = new ProviderIdentityDto();

        var frmProviderIdentEdit = new FrmProviderIdentEdit(providerIdentityDto);

        frmProviderIdentEdit.ShowDialog();

        if (frmProviderIdentEdit.IsDialogOK)
        {
            _providerDto.Identities.Add(providerIdentityDto);
        }

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

        var defaultProviderClinicDto = _providerDto.Clinics.Find(x => x.ClinicId is null);
        if (defaultProviderClinicDto is null)
        {
            return;
        }

        textDEANum.Text = defaultProviderClinicDto.DeaNumber;
        textStateLicense.Text = defaultProviderClinicDto.StateLicense;
        textStateRxID.Text = defaultProviderClinicDto.StateRxId;
        textStateWhereLicensed.Text = defaultProviderClinicDto.StateWhereLicensed;
    }

    private void ListBoxClinics_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!checkAllClinics.Checked)
        {
            return;
        }

        checkAllClinics.CheckedChanged -= CheckBoxAllClinics_CheckedChanged;
        checkAllClinics.Checked = false;
        checkAllClinics.CheckedChanged += CheckBoxAllClinics_CheckedChanged;
    }

    private void CheckBoxAllClinics_CheckedChanged(object sender, EventArgs e)
    {
        if (!checkAllClinics.Checked)
        {
            return;
        }

        listBoxClinics.SelectedIndexChanged -= ListBoxClinics_SelectedIndexChanged;
        listBoxClinics.SelectedIndices.Clear();
        listBoxClinics.SelectedIndexChanged += ListBoxClinics_SelectedIndexChanged;
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

        if (Providers.GetExists(x => x.Id != _providerDto.Id && x.Abbr == textAbbr.Text))
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

        if (checkIsHidden.Checked && _providerDto.IsHidden == false)
        {
            if (!ConfirmOk(
                    "If there are any future hours on this provider's schedule, they will be removed.  " +
                    "This does not affect scheduled appointments or any other appointments in any way."))
            {
                return;
            }

            Providers.RemoveProvFromFutureSchedule(_providerDto.Id);
        }

        var provNumClaimBillingOverride = comboProv.GetSelectedProvNum();
        if (provNumClaimBillingOverride != 0)
        {
            var providerClaimBillingOverride = comboProv.GetSelected<ProviderDto>() ?? Providers.GetById(provNumClaimBillingOverride);

            if (providerClaimBillingOverride is {IsNotPerson: false})
            {
                ShowError("E-claim Billing Prov Override cannot be a person.");
                return;
            }
        }

        if (_providerDto.Id > 0 && comboProv.GetSelectedProvNum() == _providerDto.Id)
        {
            ShowError("E-claim Billing Prov Override cannot be the same provider.");
            return;
        }

        if (textBirthdate.Text != "" && !textBirthdate.IsValid())
        {
            ShowError("Birthdate invalid.");
            return;
        }

        if (!textProdGoalHr.IsValid())
        {
            ShowError("Hourly production goal invalid.");
            return;
        }

        var defaultProviderClinicDto = _providerDto.Clinics.Find(x => x.ClinicId is null);
        if (defaultProviderClinicDto is null)
        {
            defaultProviderClinicDto = new ProviderClinicDto();

            _providerDto.Clinics.Add(defaultProviderClinicDto);
        }

        defaultProviderClinicDto.StateLicense = textStateLicense.Text;
        defaultProviderClinicDto.StateWhereLicensed = textStateWhereLicensed.Text;
        defaultProviderClinicDto.DeaNumber = textDEANum.Text;
        defaultProviderClinicDto.StateRxId = textStateRxID.Text;

        _providerDto.Abbr = textAbbr.Text;
        _providerDto.LastName = textLName.Text;
        _providerDto.FirstName = textFName.Text;
        _providerDto.MiddleName = textMI.Text;
        _providerDto.Suffix = textSuffix.Text;
        _providerDto.PreferredName = textPreferredName.Text;
        _providerDto.Ssn = textSSN.Text;
        _providerDto.MedicaidId = textMedicaidID.Text;
        _providerDto.NationalProviderId = textNationalProvID.Text;
        _providerDto.CanadianOfficeNumber = textCanadianOfficeNum.Text;
        _providerDto.IsSecondary = checkIsSecondary.Checked;
        _providerDto.IsSignatureOnFile = checkSigOnFile.Checked;
        _providerDto.IsHidden = checkIsHidden.Checked;
        _providerDto.IsCdaNet = checkIsCDAnet.Checked;
        _providerDto.Color = ColorTranslator.ToHtml(odColorPickerAppt.BackgroundColor);
        _providerDto.OutlineColor = ColorTranslator.ToHtml(odColorPickerOutline.BackgroundColor);
        _providerDto.IsHiddenFromReports = checkIsHiddenOnReports.Checked;
        _providerDto.SchedulerNote = textSchedRules.Text;
        _providerDto.DateOfBirth = SIn.Date(textBirthdate.Text);
        _providerDto.HourlyProductionGoal = SIn.Decimal(textProdGoalHr.Text);
        _providerDto.TerminatedOn = dateTerm.GetDateTime();

        if (listFeeSched.SelectedIndex != -1)
        {
            _providerDto.FeeScheduleId = listFeeSched.GetSelected<FeeSched>().FeeSchedNum;
        }

        // TODO: ProviderCur.Specialty = Defs.GetByExactNameNeverZero(DefCat.ProviderSpecialties, listSpecialty.SelectedItem.ToString());
        _providerDto.TaxonomyCode = textTaxonomyOverride.Text;
        _providerDto.IsNotPerson = checkIsNotPerson.Checked;
        // TODO: ProviderCur.ProvNumBillingOverride = comboProv.GetSelectedProvNum();

        if (IsNew)
        {
            var provNum = Providers.Insert(_providerDto);
        }
        else
        {
            Providers.Update(_providerDto);

            #region Date Term Check

            if (_providerDto.TerminatedOn is not null && _providerDto.TerminatedOn.Value < DateTime.Now)
            {
                var listClaimPaySplits = Claims.GetOutstandingClaimsByProvider(_providerDto.Id, _providerDto.TerminatedOn.Value);
                var stringBuilderClaimMessage = new StringBuilder(Lan.g(this, "Clinic\tPatNum\tPatient Name\tDate of Service\tClaim Status\tFee\tCarrier") + "\r\n");
                for (var i = 0; i < listClaimPaySplits.Count; i++)
                {
                    stringBuilderClaimMessage.Append(listClaimPaySplits[i].ClinicDesc + "\t"
                                                                                      + SOut.Long(listClaimPaySplits[i].PatNum) + "\t"
                                                                                      + listClaimPaySplits[i].PatName + "\t"
                                                                                      + listClaimPaySplits[i].DateClaim.ToShortDateString() + "\t");
                    switch (listClaimPaySplits[i].ClaimStatus)
                    {
                        case "W":
                            stringBuilderClaimMessage.Append("Waiting in Queue\t");
                            break;
                        case "H":
                            stringBuilderClaimMessage.Append("Hold\t");
                            break;
                        case "U":
                            stringBuilderClaimMessage.Append("Unsent\t");
                            break;
                        case "S":
                            stringBuilderClaimMessage.Append("Sent\t");
                            break;
                    }

                    stringBuilderClaimMessage.AppendLine(listClaimPaySplits[i].FeeBilled + "\t" + listClaimPaySplits[i].Carrier);
                }

                using var msgBoxCopyPaste = new MsgBoxCopyPaste(stringBuilderClaimMessage.ToString());
                msgBoxCopyPaste.Text = Lan.g(this, "Outstanding Claims for the Provider Whose Term Has Expired");
                if (listClaimPaySplits.Count > 0)
                {
                    msgBoxCopyPaste.ShowDialog();
                }
            }

            #endregion Date Term Check
        }

        DialogResult = DialogResult.OK;
    }
}