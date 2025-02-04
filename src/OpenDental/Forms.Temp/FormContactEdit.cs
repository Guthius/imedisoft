using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormContactEdit : FormODBase
{
    public bool IsNew;
    public Contact ContactCur;

    private List<Def> _contactCategoriesDefs;

    public FormContactEdit()
    {
        InitializeComponent();
    }

    private void FormContactEdit_Load(object sender, EventArgs e)
    {
        _contactCategoriesDefs = Defs.GetDefsForCategory(DefCat.ContactCategories, true);

        for (var i = 0; i < _contactCategoriesDefs.Count; i++)
        {
            listCategory.Items.Add(_contactCategoriesDefs[i].ItemName);
            if (ContactCur.Category == _contactCategoriesDefs[i].DefNum)
            {
                listCategory.SelectedIndex = i;
            }
        }

        textLName.Text = ContactCur.LName;
        textFName.Text = ContactCur.FName;
        textWkPhone.Text = ContactCur.WkPhone;
        textFax.Text = ContactCur.Fax;
        textNotes.Text = ContactCur.Notes;
    }

    private void ButtonDelete_Click(object sender, EventArgs e)
    {
        if (!ConfirmOk("Delete contact"))
        {
            return;
        }

        if (IsNew)
        {
            DialogResult = DialogResult.Cancel;
        }
        else
        {
            Contacts.Delete(ContactCur);

            DialogResult = DialogResult.OK;
        }
    }

    private void TextBoxLName_TextChanged(object sender, EventArgs e)
    {
        if (textLName.Text.Length != 1)
        {
            return;
        }

        textLName.Text = textLName.Text.ToUpper();
        textLName.SelectionStart = 1;
    }

    private void TextBoxFName_TextChanged(object sender, EventArgs e)
    {
        if (textFName.Text.Length != 1)
        {
            return;
        }

        textFName.Text = textFName.Text.ToUpper();
        textFName.SelectionStart = 1;
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        if (textLName.Text == "")
        {
            ShowError("Last Name cannot be blank.");
            return;
        }

        ContactCur.Category = _contactCategoriesDefs[listCategory.SelectedIndex].DefNum;
        ContactCur.LName = textLName.Text;
        ContactCur.FName = textFName.Text;
        ContactCur.WkPhone = textWkPhone.Text;
        ContactCur.Fax = textFax.Text;
        ContactCur.Notes = textNotes.Text;

        if (IsNew)
        {
            Contacts.Insert(ContactCur);
        }
        else
        {
            Contacts.Update(ContactCur);
        }

        DialogResult = DialogResult.OK;
    }
}