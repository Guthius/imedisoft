using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CDT;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDental.Properties;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormRegistrationKey : FormODBase
{
    private string _key;

    public FormRegistrationKey()
    {
        InitializeComponent();
    }

    private void FormRegistrationKey_Load(object sender, EventArgs e)
    {
        if (!Security.IsUserLoggedIn)
        {
            checkAgree.Enabled = false;
        }

        _key = PrefC.GetString(PrefName.RegistrationKey);
        if (_key is {Length: 16})
        {
            _key = _key.Substring(0, 4) + "-" + _key.Substring(4, 4) + "-" + _key.Substring(8, 4) + "-" + _key.Substring(12, 4);
        }

        textKey1.Text = _key;
            
        butOK.Enabled = true;
            
        FillListBoxRegistration();
            
        listBoxRegistration.SetSelected(0);
    }
        
    private void FillListBoxRegistration()
    {
        listBoxRegistration.Items.Add("OpenDental", Resources.OpenDentalLicense);
        listBoxRegistration.Items.Add("AForge", Resources.AForge);
        listBoxRegistration.Items.Add("Angular", Resources.Angular);
        listBoxRegistration.Items.Add("Bouncy Castle", Resources.BouncyCastle);
        listBoxRegistration.Items.Add("BSD", Resources.Bsd);
        listBoxRegistration.Items.Add("CDT", Resources.CDT_Content_End_User_License1);
        listBoxRegistration.Items.Add("Dropbox", Resources.Dropbox_Api);
        listBoxRegistration.Items.Add("GPL", Resources.GPL);
        listBoxRegistration.Items.Add("Drifty", Resources.Ionic);
        listBoxRegistration.Items.Add("Mentalis", Resources.Mentalis);
        listBoxRegistration.Items.Add("Microsoft", Resources.Microsoft);
        listBoxRegistration.Items.Add("MigraDoc", Resources.MigraDoc);
        listBoxRegistration.Items.Add("NDde", Resources.NDde);
        listBoxRegistration.Items.Add("Newton Soft", Resources.NewtonSoft_Json);
        listBoxRegistration.Items.Add("Oracle", Resources.Oracle);
        listBoxRegistration.Items.Add("PDFSharp", Resources.PdfSharp);
        listBoxRegistration.Items.Add("SharpDX", Resources.SharpDX);
        listBoxRegistration.Items.Add("Sparks3D", Resources.Sparks3D);
        listBoxRegistration.Items.Add("SSHNet", Resources.SshNet);
        listBoxRegistration.Items.Add("Stdole", Resources.stdole);
        listBoxRegistration.Items.Add("Tamir", Resources.Tamir);
        listBoxRegistration.Items.Add("Tao_Freeglut", Resources.Tao_Freeglut);
        listBoxRegistration.Items.Add("Tao_OpenGL", Resources.Tao_OpenGL);
        listBoxRegistration.Items.Add("Twain Group", Resources.Twain);
        listBoxRegistration.Items.Add("Zxing", Resources.Zxing);
    }

    /// <summary>If using the foreign CDT.dll, it always returns true (valid), regardless of whether the box is blank or malformed.</summary>
    public static bool ValidateKey(string keystr)
    {
        return Class1.ValidateKey(keystr);
    }

    private void textKey1_KeyUp(object sender, KeyEventArgs e)
    {
        var cursorPosition = textKey1.SelectionStart;
        textKey1.Text = textKey1.Text.ToUpper();
        var length = textKey1.Text.Length;
        if (Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{5}$"))
        {
            textKey1.Text = textKey1.Text.Substring(0, 4) + "-" + textKey1.Text.Substring(4);
        }
        else if (Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{4}-[A-Z0-9]{5}$"))
        {
            textKey1.Text = textKey1.Text.Substring(0, 9) + "-" + textKey1.Text.Substring(9);
        }
        else if (Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{5}$"))
        {
            textKey1.Text = textKey1.Text.Substring(0, 14) + "-" + textKey1.Text.Substring(14);
        }

        if (textKey1.Text.Length > length)
        {
            cursorPosition++;
        }

        textKey1.SelectionStart = cursorPosition;
    }

    private void textKey1_TextChanged(object sender, EventArgs e)
    {
        butOK.Enabled = true;
    }

    private void listRegistration_SelectedIndexChanged(object sender, EventArgs e)
    {
        richTextAgreement.Text = listBoxRegistration.GetSelected<string>();
    }

    private void checkAgree_CheckedChanged(object sender, EventArgs e)
    {
        butOK.Enabled = true;
    }

    private void butOK_Click(object sender, EventArgs e)
    {
        if (textKey1.Text != ""
            && !Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$")
            && !Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{16}$"))
        {
            MsgBox.Show(this, "Invalid registration key format.");
            return;
        }

        var regkey = "";
        if (Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$"))
        {
            regkey = textKey1.Text.Substring(0, 4) + textKey1.Text.Substring(5, 4) + textKey1.Text.Substring(10, 4) + textKey1.Text.Substring(15, 4);
        }
        else if (Regex.IsMatch(textKey1.Text, @"^[A-Z0-9]{16}$"))
        {
            regkey = textKey1.Text;
        }

        if (!ValidateKey(regkey))
        {
            MsgBox.Show(this, "Invalid registration key.");
                
            return;
        }

        var regKeyHasChanged = Prefs.UpdateString(PrefName.RegistrationKey, regkey);
        if (regKeyHasChanged)
        {
            Signalods.SetInvalid(InvalidType.Prefs);
        }

        DialogResult = DialogResult.OK;
    }
}