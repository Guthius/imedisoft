using System.Collections.Generic;
using System.Windows.Forms;
using Imedisoft.Core.Entities;

namespace OpenDental;

public partial class DashToothChartLegend : PictureBox, IDashWidgetField
{
    public const int DefaultWidth = 600;
    public const int DefaultHeight = 14;
    private SheetField _sheetField;
    private List<Def> _listDefs;

    public DashToothChartLegend()
    {
        InitializeComponent();
    }
}