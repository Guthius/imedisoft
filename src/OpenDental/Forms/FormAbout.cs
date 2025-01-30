using System;
using System.Windows.Forms;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormAbout : FormODBase
{
    public FormAbout()
    {
        InitializeComponent();
    }

    private void FormAbout_Load(object sender, EventArgs e)
    {
        var softwareName = PrefC.GetString(PrefName.SoftwareName);

        labelVersion.Text = "Version: " + Application.ProductVersion;

        var updateHistory = UpdateHistories.GetForVersion(Application.ProductVersion);
        if (updateHistory != null)
        {
            labelVersion.Text += "  Since: " + updateHistory.DateTimeUpdated.ToShortDateString();
        }

        labelCopyright.Text = softwareName + " " + "Copyright 2003-" + DateTime.Now.ToString("yyyy") + ", Jordan Sparks, D.M.D.";
        labelMySQLCopyright.Text = "MySQL - Copyright 1995-" + DateTime.Now.ToString("yyyy") + ", www.mysql.com";
        labelMariaDBCopyright.Text = "MariaDB - Copyright 2009-" + DateTime.Now.ToString("yyyy") + ", www.mariadb.com";

        var serviceInfo = Computers.GetServiceInfo();

        labelName.Text += serviceInfo[2];
        labelService.Text += serviceInfo[0];
        labelMySqlVersion.Text += serviceInfo[3];
        labelServComment.Text += serviceInfo[1];
        labelMachineName.Text += Environment.MachineName.ToUpper();
        labelDatabase.Text += serviceInfo[4];
    }

    private void ButtonDiagnostics_Click(object sender, EventArgs e)
    {
        var diagnostics = BugSubmissions.GetDiagnostics(FormOpenDental.PatNumCur);

        using var msgBoxCopyPaste = new MsgBoxCopyPaste(diagnostics);

        msgBoxCopyPaste.Text = "Diagnostics";
        msgBoxCopyPaste.ShowDialog();
    }

    private void ButtonLicense_Click(object sender, EventArgs e)
    {
        using var formLicense = new FormLicense();

        formLicense.ShowDialog();
    }
}