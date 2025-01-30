using System;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormCertificationEdit : FormODBase
{
    private readonly Cert _cert;

    public FormCertificationEdit(Cert cert)
    {
        _cert = cert;
        
        InitializeComponent();
    }

    private void FormCertificationEdit_Load(object sender, EventArgs e)
    {
        textDescription.Text = _cert.Description;
        textWikiPage.Text = _cert.WikiPageLink;

        var defs = Defs.GetDefsForCategory(DefCat.CertificationCategories);

        listBoxCategories.Items.AddList(defs, x => x.ItemName);
        listBoxCategories.SetSelected(defs.FindIndex(x => x.DefNum == _cert.CertCategoryNum));

        checkIsHidden.Checked = _cert.IsHidden;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (_cert.IsNew)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        var certEmployees = CertEmployees.GetAll();

        var isCertInUse = false;
        foreach (var certEmployee in certEmployees)
        {
            if (certEmployee.CertNum != _cert.CertNum)
            {
                continue;
            }

            isCertInUse = true;
            break;
        }

        if (isCertInUse)
        {
            ShowError("Certificiation is still in use, remove Certification from all applicable employees first.");
            return;
        }

        if (!ConfirmOk("Are you sure you want to delete this certification?"))
        {
            return;
        }

        Certs.Delete(_cert.CertNum);

        DialogResult = DialogResult.OK;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textDescription.Text == "")
        {
            ShowError("Description cannot be blank.");
            return;
        }

        _cert.Description = SIn.String(textDescription.Text);
        _cert.WikiPageLink = SIn.String(textWikiPage.Text);
        _cert.IsHidden = checkIsHidden.Checked;

        var def = (Def) listBoxCategories.Items.GetObjectAt(listBoxCategories.SelectedIndex);

        var categoryNumOld = _cert.CertCategoryNum;

        _cert.CertCategoryNum = def.DefNum;

        if (_cert.CertCategoryNum != categoryNumOld)
        {
            _cert.ItemOrder = Certs.GetAll(true).FindAll(x => x.CertCategoryNum == listBoxCategories.GetSelected<Def>().DefNum).Count;
        }

        if (_cert.IsNew)
        {
            Certs.Insert(_cert);
        }
        else
        {
            Certs.Update(_cert);
        }

        DialogResult = DialogResult.OK;
    }
}