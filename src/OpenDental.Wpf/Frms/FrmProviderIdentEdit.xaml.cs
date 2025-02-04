using System;
using System.Windows.Input;
using Imedisoft.Features.Providers.Dtos;
using OpenDentBusiness;

namespace OpenDental;

public partial class FrmProviderIdentEdit : FrmODBase
{
    private readonly ProviderIdentityDto _providerIdentityDto;
    
    public FrmProviderIdentEdit(ProviderIdentityDto providerIdentityDto)
    {
        _providerIdentityDto = providerIdentityDto;

        InitializeComponent();

        Load += FrmProviderIdentEdit_Load;
        PreviewKeyDown += FrmProviderIdentEdit_PreviewKeyDown;
    }

    private void FrmProviderIdentEdit_Load(object sender, EventArgs e)
    {
        textPayorID.Text = _providerIdentityDto.PayorId;

        listType.Items.AddEnums<ProviderSupplementalID>();
        listType.SetSelectedEnum(_providerIdentityDto.Type);

        textIDNumber.Text = _providerIdentityDto.Value;
    }

    private void FrmProviderIdentEdit_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (butSave.IsAltKey(Key.S, e))
        {
            butSave_Click(this, EventArgs.Empty);
        }
    }

    private void butSave_Click(object sender, EventArgs e)
    {
        _providerIdentityDto.PayorId = textPayorID.Text;
        _providerIdentityDto.Type = listType.GetSelected<ProviderSupplementalID>().ToString();
        _providerIdentityDto.Value = textIDNumber.Text;

        IsDialogOK = true;
    }
}