using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Forms;

public partial class FormContacts : FormODBase
{
    private List<Contact> _contacts;
    private List<Def> _contactCategoryDefs;

    public FormContacts()
    {
        InitializeComponent();
    }

    private void FormContacts_Load(object sender, EventArgs e)
    {
        _contactCategoryDefs = Defs.GetDefsForCategory(DefCat.ContactCategories, true);

        foreach (var def in _contactCategoryDefs)
        {
            listCategory.Items.Add(def.ItemName);
        }

        if (listCategory.Items.Count > 0)
        {
            listCategory.SelectedIndex = 0;
        }
    }

    private void listCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        FillGrid();
    }

    private void FillGrid()
    {
        if (listCategory.SelectedIndex == -1)
        {
            return;
        }

        _contacts = Contacts.Refresh(_contactCategoryDefs[listCategory.SelectedIndex].DefNum);

        gridMain.BeginUpdate();

        gridMain.Columns.Clear();
        gridMain.Columns.Add(new GridColumn("Last Name", 100));
        gridMain.Columns.Add(new GridColumn("First Name", 100));
        gridMain.Columns.Add(new GridColumn("Wk Phone", 90));
        gridMain.Columns.Add(new GridColumn("Fax", 90));
        gridMain.Columns.Add(new GridColumn("Note", 250));

        gridMain.ListGridRows.Clear();

        foreach (var contact in _contacts)
        {
            var gridRow = new GridRow();

            gridRow.Cells.Add(contact.LName);
            gridRow.Cells.Add(contact.FName);
            gridRow.Cells.Add(contact.WkPhone);
            gridRow.Cells.Add(contact.Fax);
            gridRow.Cells.Add(contact.Notes);

            gridMain.ListGridRows.Add(gridRow);
        }

        gridMain.EndUpdate();
    }

    private void GridMain_CellDoubleClick(object sender, ODGridClickEventArgs e)
    {
        using var formContactEdit = new FormContactEdit();

        formContactEdit.ContactCur = _contacts[e.Row];

        if (formContactEdit.ShowDialog() == DialogResult.OK)
        {
            FillGrid();
        }
    }

    private void ButtonAdd_Click(object sender, EventArgs e)
    {
        var contact = new Contact
        {
            Category = _contactCategoryDefs[listCategory.SelectedIndex].DefNum
        };

        using var formContactEdit = new FormContactEdit();

        formContactEdit.ContactCur = contact;
        formContactEdit.IsNew = true;

        if (formContactEdit.ShowDialog() == DialogResult.OK)
        {
            FillGrid();
        }
    }
}