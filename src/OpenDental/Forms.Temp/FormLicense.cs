using System;
using OpenDental.Properties;

namespace OpenDental.Forms;

public partial class FormLicense : FormODBase
{
    private readonly string _selectedLicense;
        
    public FormLicense(string selectedLicense = null)
    {
        InitializeComponent();

        _selectedLicense = Resources.OpenDentalLicense;
            
        if (!string.IsNullOrWhiteSpace(selectedLicense))
        {
            _selectedLicense = selectedLicense;
        }
    }

    private void FormLicense_Load(object sender, EventArgs e)
    {
        FillListBoxLicense();
            
        for (var i = 0; i < listBoxLicense.Items.Count; i++)
        {
            if ((string) listBoxLicense.Items.GetObjectAt(i) == _selectedLicense)
            {
                listBoxLicense.SetSelected(i);
            }
        }
    }

    private void FillListBoxLicense()
    {
        listBoxLicense.Items.Add("OpenDental", Resources.OpenDentalLicense);
        listBoxLicense.Items.Add("OpenDental API End User", Resources.OpenDentalApiEndUserLicense);
        listBoxLicense.Items.Add("AForge", Resources.AForge);
        listBoxLicense.Items.Add("Angular", Resources.Angular);
        listBoxLicense.Items.Add("Bouncy Castle", Resources.BouncyCastle);
        listBoxLicense.Items.Add("BSD", Resources.Bsd);
        listBoxLicense.Items.Add("CDT", Resources.CDT_Content_End_User_License1);
        listBoxLicense.Items.Add("Dropbox", Resources.Dropbox_Api);
        listBoxLicense.Items.Add("GPL", Resources.GPL);
        listBoxLicense.Items.Add("Drifty", Resources.Ionic);
        listBoxLicense.Items.Add("Mentalis", Resources.Mentalis);
        listBoxLicense.Items.Add("Microsoft", Resources.Microsoft);
        listBoxLicense.Items.Add("MigraDoc", Resources.MigraDoc);
        listBoxLicense.Items.Add("NDde", Resources.NDde);
        listBoxLicense.Items.Add("Newton Soft", Resources.NewtonSoft_Json);
        listBoxLicense.Items.Add("Oracle", Resources.Oracle);
        listBoxLicense.Items.Add("PDFSharp", Resources.PdfSharp);
        listBoxLicense.Items.Add("SharpDX", Resources.SharpDX);
        listBoxLicense.Items.Add("Sparks3D", Resources.Sparks3D);
        listBoxLicense.Items.Add("SSHNet", Resources.SshNet);
        listBoxLicense.Items.Add("Stdole", Resources.stdole);
        listBoxLicense.Items.Add("Tamir", Resources.Tamir);
        listBoxLicense.Items.Add("Tao_Freeglut", Resources.Tao_Freeglut);
        listBoxLicense.Items.Add("Tao_OpenGL", Resources.Tao_OpenGL);
        listBoxLicense.Items.Add("Twain Group", Resources.Twain);
        listBoxLicense.Items.Add("Zxing", Resources.Zxing);
    }
        
    private void listLicense_SelectedIndexChanged(object sender, EventArgs e)
    {
        textLicense.Text = listBoxLicense.GetSelected<string>();
    }
}