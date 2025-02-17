using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormEmailDigitalSignature : FormODBase
{
    private readonly X509Certificate2 _x509Certificate2;
    private bool _isTrusted;

    public FormEmailDigitalSignature(X509Certificate2 x509Certificate2)
    {
        InitializeComponent();

        _x509Certificate2 = x509Certificate2;
    }

    private void FormEmailDigitalSignature_Load(object sender, EventArgs e)
    {
        var signedByAddress = EmailNameResolver.GetCertSubjectName(_x509Certificate2);

        textSignedBy.Text = signedByAddress;
        textCertificateAuthority.Text = _x509Certificate2.IssuerName.Name;
        textValidFrom.Text = _x509Certificate2.NotBefore.ToShortDateString() + " to " + _x509Certificate2.NotAfter.ToShortDateString();
        textThumbprint.Text = _x509Certificate2.Thumbprint;
        textVersion.Text = _x509Certificate2.Version.ToString();

        _isTrusted = EmailMessages.GetReceiverUntrustedCount(signedByAddress) == -1;

        if (_isTrusted)
        {
            butTrust.Visible = false;

            textTrustStatus.Text = "Trusted";
            textTrustExplanation.Text = "Encrypted email and EHR Direct messaging are currently enabled for the signer.";
        }
        else
        {
            butTrust.Visible = true;

            textTrustStatus.Text = "Untrusted or invalid";
            textTrustExplanation.Text =
                "Encrypted email and EHR Direct messaging will not work until this digital signature is trusted by you. " +
                "Click the Trust button to add trust for this digital signature.";
        }
    }

    private void ButtonTrust_Click(object sender, EventArgs e)
    {
        try
        {
            EmailMessages.TryAddTrustForSignature(_x509Certificate2);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);

            return;
        }

        ShowInfo("Trust added for digital signature.");

        DialogResult = DialogResult.OK;
    }
}