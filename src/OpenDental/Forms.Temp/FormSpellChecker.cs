using System;
using OpenDental.UI;

namespace OpenDental;

public partial class FormSpellChecker:FormODBase {

	public void SetText(string textIn) {
		try {
			textMain.Rtf=textIn;
		}
		catch {
			MsgBox.Show(this,"Invalid RTF. Clicking OK in this window will result in loss of formatting.");
			textMain.Text=textIn;
		}
	}

	public FormSpellChecker() {
		InitializeComponent();
	}

	private void FormSpellChecker_Load(object sender,EventArgs e) {
		textMain.timerSpellCheck.Start();//so the spell check will run when form opens.
		LayoutMenu();
	}

	private void LayoutMenu() {
		menuMain.BeginUpdate();
		menuMain.Add(new MenuItemOD("Setup",setupToolStripMenuItem_Click));
		menuMain.EndUpdate();
	}

	private void setupToolStripMenuItem_Click(object sender,EventArgs e) {
		using var formSpellCheck = new FormSpellCheck();
		formSpellCheck.ShowDialog();
	}

}