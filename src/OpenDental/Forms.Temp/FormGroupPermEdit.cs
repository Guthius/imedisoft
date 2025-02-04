using System;
using System.Globalization;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormGroupPermEdit : FormODBase
{
    private readonly GroupPermission _groupPermission;

    public FormGroupPermEdit(GroupPermission groupPermission)
    {
        InitializeComponent();

        _groupPermission = groupPermission.Copy();
    }

    private void FormGroupPermEdit_Load(object sender, EventArgs e)
    {
        textName.Text = GroupPermissions.GetDesc(_groupPermission.PermType);
        textDate.Text = _groupPermission.NewerDate.Year < 1880 ? "" : _groupPermission.NewerDate.ToShortDateString();
        textDays.Text = _groupPermission.NewerDays == 0 ? "" : _groupPermission.NewerDays.ToString();
    }

    private void TextBoxDate_KeyDown(object sender, KeyEventArgs e)
    {
        textDays.Text = "";
    }

    private void TextBoxDays_KeyDown(object sender, KeyEventArgs e)
    {
        textDate.Text = "";
        textDate.Validate();
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (!textDate.IsValid() || !textDays.IsValid())
        {
            ShowError("Please fix data entry errors first.");
            return;
        }

        var newerDays = SIn.Int(textDays.Text);
        if (newerDays > GroupPermissions.NewerDaysMax)
        {
            ShowError($"Days must be less than {GroupPermissions.NewerDaysMax.ToString(CultureInfo.InvariantCulture)}.");
            return;
        }

        _groupPermission.NewerDays = newerDays;
        _groupPermission.NewerDate = SIn.Date(textDate.Text);

        try
        {
            if (_groupPermission.IsNew)
            {
                GroupPermissions.Insert(_groupPermission);
            }
            else
            {
                GroupPermissions.Update(_groupPermission);
            }

            SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0,
                $"Permission '{_groupPermission.PermType}' granted to '{UserGroups.GetGroup(_groupPermission.UserGroupNum).Description}'");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);

            return;
        }

        DialogResult = DialogResult.OK;
    }
}