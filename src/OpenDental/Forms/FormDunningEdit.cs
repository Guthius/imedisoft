using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormDunningEdit : FormODBase
{
    private readonly Dunning _dunning;
    private List<Def> _billingTypeDefs;

    public FormDunningEdit(Dunning dunning)
    {
        _dunning = dunning.Copy();

        InitializeComponent();
    }

    private void FormDunningEdit_Load(object sender, EventArgs e)
    {
        if (_dunning.ClinicNum == -2)
        {
            comboClinics.IsAllSelected = true;
        }
        else
        {
            comboClinics.ClinicNumSelected = _dunning.ClinicNum;
        }

        if (PrefC.GetBool(PrefName.ShowFeatureSuperfamilies))
        {
            checkSuperFamily.Visible = true;
            checkSuperFamily.Checked = _dunning.IsSuperFamily;
        }

        listBillType.Items.Add("all");
        listBillType.SetSelected(0);

        _billingTypeDefs = Defs.GetDefsForCategory(DefCat.BillingTypes, true);

        for (var i = 0; i < _billingTypeDefs.Count; i++)
        {
            listBillType.Items.Add(_billingTypeDefs[i].ItemName);
            if (_dunning.BillingType == _billingTypeDefs[i].DefNum)
            {
                listBillType.SetSelected(i + 1);
            }
        }

        switch (_dunning.AgeAccount)
        {
            case 0:
                radioAny.Checked = true;
                break;
            case 30:
                radio30.Checked = true;
                break;
            case 60:
                radio60.Checked = true;
                break;
            case 90:
                radio90.Checked = true;
                break;
        }

        switch (_dunning.InsIsPending)
        {
            case YN.Unknown:
                radioU.Checked = true;
                break;
            case YN.Yes:
                radioY.Checked = true;
                break;
            case YN.No:
                radioN.Checked = true;
                break;
        }

        textDaysInAdvance.Text = _dunning.DaysInAdvance.ToString();
        textDunMessage.Text = _dunning.DunMessage;
        textMessageBold.Text = _dunning.MessageBold;
        textEmailBody.Text = _dunning.EmailBody;
        textEmailSubject.Text = _dunning.EmailSubject;
    }

    private void RadioButtonAny_CheckedChanged(object sender, EventArgs e)
    {
        labelDaysInAdvance.Visible = !radioAny.Checked;
        textDaysInAdvance.Visible = !radioAny.Checked;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_dunning.DunningNum == 0)
        {
            DialogResult = DialogResult.Cancel;
        }
        else
        {
            Dunnings.Delete(_dunning);

            DialogResult = DialogResult.OK;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textDaysInAdvance.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        if (textDunMessage.Text == "" && textMessageBold.Text == "" && textEmailSubject.Text == "" && textEmailBody.Text == "")
        {
            ShowError("All messages cannot be blank.");
            return;
        }

        _dunning.BillingType = 0;

        if (listBillType.SelectedIndex > 0)
        {
            _dunning.BillingType = _billingTypeDefs[listBillType.SelectedIndex - 1].DefNum;
        }

        _dunning.AgeAccount = (byte) (30 * new List<RadioButton> {radioAny, radio30, radio60, radio90}.FindIndex(x => x.Checked));
        _dunning.InsIsPending = (YN) new List<RadioButton> {radioU, radioY, radioN}.FindIndex(x => x.Checked);
        _dunning.DaysInAdvance = 0;

        if (!radioAny.Checked)
        {
            _dunning.DaysInAdvance = SIn.Int(textDaysInAdvance.Text);
        }

        _dunning.DunMessage = textDunMessage.Text;
        _dunning.MessageBold = textMessageBold.Text;
        _dunning.EmailBody = textEmailBody.Text;
        _dunning.EmailSubject = textEmailSubject.Text;
        _dunning.IsSuperFamily = checkSuperFamily.Checked;

        if (true)
        {
            _dunning.ClinicNum = comboClinics.ClinicNumSelected;
        }

        if (_dunning.DunningNum == 0)
        {
            Dunnings.Insert(_dunning);
        }
        else
        {
            Dunnings.Update(_dunning);
        }

        DialogResult = DialogResult.OK;
    }
}