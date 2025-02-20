using System;
using System.Drawing;

namespace OpenDental;

public partial class FormTrojanHelp : FormODBase
{
    public FormTrojanHelp()
    {
        InitializeComponent();
    }

    private void FormTrojanHelp_Load(object sender, EventArgs e)
    {
        textMain.Select(0, 31);
        textMain.SelectionFont = new Font(Font, FontStyle.Bold);
        textMain.Select(323, 20);
        textMain.SelectionFont = new Font(Font, FontStyle.Bold);
        textMain.Select(571, 5);
        textMain.SelectionFont = new Font(Font, FontStyle.Bold);
        textMain.Select(933, 18);
        textMain.SelectionFont = new Font(Font, FontStyle.Bold);
        textMain.Select(1302, 31);
        textMain.SelectionFont = new Font(Font, FontStyle.Bold);
        textMain.Select(1473, 10);
        textMain.SelectionFont = new Font(Font, FontStyle.Bold);
    }
}