using System;
using System.Text;
using System.Windows.Forms;
using ImagingDeviceManager;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Forms;

public partial class FormImagingDeviceEdit : FormODBase
{
    public ImagingDevice ImagingDeviceCur;

    public FormImagingDeviceEdit()
    {
        InitializeComponent();
    }

    private void FormImagingDeviceEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = ImagingDeviceCur.Description;
        textComputerName.Text = ImagingDeviceCur.ComputerName;

        switch (ImagingDeviceCur.DeviceType)
        {
            case EnumImgDeviceType.TwainRadiograph or EnumImgDeviceType.XDR:
                radioTwain.Checked = true;
                break;

            case EnumImgDeviceType.TwainMulti:
                radioTwainMulti.Checked = true;
                break;
        }

        comboTwainName.Text = ImagingDeviceCur.TwainName;
        checkShowTwainUI.Checked = ImagingDeviceCur.ShowTwainUI;
    }

    private void ButtonThis_Click(object sender, EventArgs e)
    {
        textComputerName.Text = Environment.MachineName;
    }

    private void ComboBoxTwainName_DropDown(object sender, EventArgs e)
    {
        try
        {
            Twain.ActivateEZTwain();
        }
        catch
        {
            Cursor = Cursors.Default;

            ShowError("EzTwain4.dll not found. Please run the setup file in your images folder.");

            return;
        }

        comboTwainName.Items.Clear();
        if (!EZTwain.GetSourceList())
        {
            return;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.EnsureCapacity(64);
        while (EZTwain.GetNextSourceName(stringBuilder))
        {
            comboTwainName.Items.Add(stringBuilder.ToString());
            stringBuilder.EnsureCapacity(64);
        }
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (ImagingDeviceCur.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        if (!ConfirmOk("Delete?"))
        {
            return;
        }

        ImagingDevices.Delete(ImagingDeviceCur.ImagingDeviceNum);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text == "")
        {
            ShowError("Please enter a description.");
            return;
        }

        ImagingDeviceCur.Description = textDescription.Text;
        ImagingDeviceCur.ComputerName = textComputerName.Text;
        ImagingDeviceCur.DeviceType = EnumImgDeviceType.TwainRadiograph;

        if (radioTwainMulti.Checked)
        {
            ImagingDeviceCur.DeviceType = EnumImgDeviceType.TwainMulti;
        }

        ImagingDeviceCur.TwainName = comboTwainName.Text;
        ImagingDeviceCur.ShowTwainUI = checkShowTwainUI.Checked;

        if (ImagingDeviceCur.IsNew)
        {
            ImagingDevices.Insert(ImagingDeviceCur);
        }
        else
        {
            ImagingDevices.Update(ImagingDeviceCur);
        }

        DialogResult = DialogResult.OK;
    }
}