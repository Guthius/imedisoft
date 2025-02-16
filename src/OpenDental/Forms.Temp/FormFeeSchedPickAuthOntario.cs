using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormFeeSchedPickAuthOntario : FormODBase
{
    private const string OntarioDentalAssociation = "ODA";
    private const string BritishColumbiaDentalAssociation = "BCDA";

    private readonly string _dentalAssociation;

    public string GetOdaMemberNumber()
    {
        return textODAMemberNumber.Text;
    }

    public string GetOdaMemberPassword()
    {
        return textODAMemberPassword.Text;
    }

    public FormFeeSchedPickAuthOntario(string dentalAssociation)
    {
        InitializeComponent();

        _dentalAssociation = dentalAssociation;
        if (string.IsNullOrWhiteSpace(_dentalAssociation))
        {
            _dentalAssociation = OntarioDentalAssociation;
        }

        Text = $"Fee Schedule Authorization for {(_dentalAssociation == BritishColumbiaDentalAssociation ? "British Columbia" : "Ontario")}";
    }

    private void FormFeeSchedPickAuthOntario_Load(object sender, EventArgs e)
    {
        if (_dentalAssociation != OntarioDentalAssociation)
        {
            return;
        }
        
        textODAMemberNumber.Text = PrefC.GetString(PrefName.CanadaODAMemberNumber);
        textODAMemberPassword.Text = PrefC.GetString(PrefName.CanadaODAMemberPass);
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textODAMemberNumber.Text == "")
        {
            ShowError($"{_dentalAssociation} Member Number cannot be blank.");
            return;
        }

        if (textODAMemberPassword.Text == "")
        {
            ShowError($"{_dentalAssociation} Member Password cannot be blank.");
            return;
        }

        if (_dentalAssociation == OntarioDentalAssociation)
        {
            Prefs.UpdateString(PrefName.CanadaODAMemberNumber, textODAMemberNumber.Text);
            Prefs.UpdateString(PrefName.CanadaODAMemberPass, textODAMemberPassword.Text);
        }

        DialogResult = DialogResult.OK;
    }
}