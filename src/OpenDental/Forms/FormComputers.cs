using System;
using System.ComponentModel;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormComputers : FormODBase
{
    private bool _changed;

    public FormComputers()
    {
        InitializeComponent();
    }

    private void FormComputers_Load(object sender, EventArgs e)
    {
        FillList();

        if (!Security.IsAuthorized(EnumPermType.GraphicsEdit, suppressMessage: true))
        {
            butSetSimpleGraphics.Enabled = false;
        }
    }

    private void FillList()
    {
        Computers.RefreshCache();

        listComputer.Items.Clear();

        var serviceInfos = Computers.GetServiceInfo();

        textService.Text = serviceInfos[0];
        textVersion.Text = serviceInfos[3];
        textServComment.Text = serviceInfos[1];
        textCurComp.Text = Environment.MachineName.ToUpper();

        listComputer.Items.AddList(Computers.GetDeepCopy(), x => x.CompName);
    }

    private void ListBoxComputer_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (!Security.IsAuthorized(EnumPermType.GraphicsEdit))
        {
            return;
        }

        using var formGraphics = new FormGraphics();

        formGraphics.ComputerPrefCur = ComputerPrefs.GetForComputer(listComputer.GetSelected<Computer>().CompName);
        formGraphics.ShowDialog();
    }

    private void ButtonSetSimpleGraphics_Click(object sender, EventArgs e)
    {
        if (listComputer.SelectedIndex == -1)
        {
            ShowError("You must select a computer name first.");
            return;
        }

        ComputerPrefs.SetToSimpleGraphics(listComputer.GetSelected<Computer>().CompName);

        ShowInfo("Done.");

        SecurityLogs.MakeLogEntry(EnumPermType.GraphicsEdit, 0, "Set the graphics for computer " + listComputer.GetSelected<Computer>().CompName + " to simple");
    }

    private void ButtonResetZoom_Click(object sender, EventArgs e)
    {
        if (listComputer.SelectedIndex == -1)
        {
            ShowError("You must select a computer name first.");
            return;
        }

        ComputerPrefs.ResetZoom(listComputer.GetSelected<Computer>().CompName);

        ShowInfo("Done.");

        SecurityLogs.MakeLogEntry(EnumPermType.GraphicsEdit, 0, "Reset zoom for computer " + listComputer.GetSelected<Computer>().CompName + " to 0.");
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (listComputer.SelectedIndex == -1)
        {
            return;
        }

        Computers.Delete(listComputer.GetSelected<Computer>());

        _changed = true;

        FillList();
    }

    private void FormComputers_Closing(object sender, CancelEventArgs e)
    {
        if (_changed)
        {
            DataValid.SetInvalid(InvalidType.Computers);
        }
    }
}