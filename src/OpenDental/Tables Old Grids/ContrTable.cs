using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OpenDental;

public class ContrTable : UserControl
{
    private readonly Container _components = null;

    public int MaxRows = 10;

    public string[,] Cell = new string[10, 10];
    public float[,] FontSize = new float[10, 10];
    public bool[,] FontBold = new bool[10, 10];
    public Color[,] FontColor = new Color[10, 10];
    public Color[,] BackGColor = new Color[10, 10];
    public Color[,] LeftBorder = new Color[10, 10];
    public Color[,] TopBorder = new Color[10, 10];
    public int[] RowHeight = new int[10];
    public bool[,] IsOverflow;
    public int SelectedRow = -1;

    protected int MaxCols = 10;
    protected int[] ColWidth = new int[10];
    protected HorizontalAlignment[] ColAlign = new HorizontalAlignment[10];
    protected string Heading = "";
    protected bool HeadingIsPresent = true;
    protected bool FieldsArePresent = true;
    protected bool ShowScroll = false;

    private string[] _fields = new string[10];
    private int[] _colPos = new int[10];
    private int[] _rowPos = new int[10];
    private int _scrollWidth;
    private Panel _panelHead;
    private Panel _panelScroll;
    private VScrollBar _vScrollBar1;
    private ContrPanelTable _panelTable;
    private Point _mouseDownPosition;
    private Font _myFont = new("Microsoft Sans Serif", 12);
    private float _myFontSize;
    private FontStyle _myFontStyle = FontStyle.Regular;
    private Color _myFontColor = Color.Black;
    private FontFamily _myFontFamily = FontFamily.GenericSansSerif;
    private int _offset;
    private bool _controlIsDown;
    private int[] _selectedIndices;

    public ContrTable()
    {
        InitializeComponent();

        _panelTable.MouseWheel += panelTable_MouseWheel;

        _selectedIndices = [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_components != null)
            {
                _components.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _panelScroll = new Panel();
        _vScrollBar1 = new VScrollBar();
        _panelTable = new ContrPanelTable();
        _panelHead = new Panel();
        _panelScroll.SuspendLayout();
        SuspendLayout();
        // 
        // panelScroll
        // 
        _panelScroll.BackColor = SystemColors.Control;
        _panelScroll.BorderStyle = BorderStyle.FixedSingle;
        _panelScroll.Controls.Add(_vScrollBar1);
        _panelScroll.Controls.Add(_panelTable);
        _panelScroll.Location = new Point(0, 100);
        _panelScroll.Name = "_panelScroll";
        _panelScroll.Size = new Size(464, 160);
        _panelScroll.TabIndex = 1;
        // 
        // vScrollBar1
        // 
        _vScrollBar1.Dock = DockStyle.Right;
        _vScrollBar1.LargeChange = 50;
        _vScrollBar1.Location = new Point(445, 0);
        _vScrollBar1.Maximum = 200;
        _vScrollBar1.Minimum = 1;
        _vScrollBar1.Name = "_vScrollBar1";
        _vScrollBar1.Size = new Size(17, 158);
        _vScrollBar1.SmallChange = 50;
        _vScrollBar1.TabIndex = 1;
        _vScrollBar1.Value = 150;
        _vScrollBar1.KeyPress += vScrollBar1_KeyPress;
        _vScrollBar1.Scroll += vScrollBar1_Scroll;
        // 
        // panelTable
        // 
        _panelTable.BackColor = SystemColors.Window;
        _panelTable.Location = new Point(0, 0);
        _panelTable.Name = "_panelTable";
        _panelTable.Size = new Size(420, 168);
        _panelTable.TabIndex = 2;
        _panelTable.KeyPress += panelTable_KeyPress;
        _panelTable.MouseUp += panelTable_MouseUp;
        _panelTable.Paint += panelTable_Paint;
        _panelTable.KeyUp += panelTable_KeyUp;
        _panelTable.KeyDown += panelTable_KeyDown;
        _panelTable.DoubleClick += panelTable_DoubleClick;
        _panelTable.MouseWheel += panelTable_MouseWheel;
        _panelTable.MouseDown += panelTable_MouseDown;
        // 
        // panelHead
        // 
        _panelHead.AutoScroll = true;
        _panelHead.BorderStyle = BorderStyle.FixedSingle;
        _panelHead.Location = new Point(1, 1);
        _panelHead.Name = "_panelHead";
        _panelHead.Size = new Size(436, 84);
        _panelHead.TabIndex = 2;
        _panelHead.Click += panelHead_Click;
        _panelHead.Paint += panelHead_Paint;
        // 
        // ContrTable
        // 
        BackColor = SystemColors.Window;
        Controls.Add(_panelScroll);
        Controls.Add(_panelHead);
        Name = "ContrTable";
        Size = new Size(484, 356);
        Paint += ContrTable_Paint;
        _panelScroll.ResumeLayout(false);
        ResumeLayout(false);
    }

    [Category("Behavior"), Description("Exactly like the listBox.SelectionMode, except no MultiSimple.")]
    public SelectionMode SelectionMode { get; set; }

    public int ScrollValue
    {
        get => _vScrollBar1.Value;
        set
        {
            if (value > _vScrollBar1.Maximum)
            {
                value = _vScrollBar1.Maximum;
            }

            if (value < _vScrollBar1.Minimum)
            {
                value = _vScrollBar1.Minimum;
            }

            _vScrollBar1.Value = value;

            _panelTable.Location = new Point(0, -value);
        }
    }

    protected void InstantClassesPar()
    {
        Cell = new string[MaxCols, MaxRows];
        FontSize = new float[MaxCols, MaxRows];
        FontBold = new bool[MaxCols, MaxRows];
        FontColor = new Color[MaxCols, MaxRows];
        BackGColor = new Color[MaxCols, MaxRows];
        LeftBorder = new Color[MaxCols, MaxRows];
        TopBorder = new Color[MaxCols, MaxRows];
        RowHeight = new int[MaxRows];
        ColWidth = new int[MaxCols];
        ColAlign = new HorizontalAlignment[MaxCols];
        _fields = new string[MaxCols];
        IsOverflow = new bool[MaxCols, MaxRows];
        _colPos = new int[MaxCols];
        _rowPos = new int[MaxRows];
        _selectedIndices = [];
    }

    public void LayoutTables(bool preserveScroll = false)
    {
        var scroll = ScrollValue;

        _scrollWidth = ShowScroll ? 17 : 0;
        if (MaxRows != 0)
        {
            _rowPos[0] = 0;
        }

        for (var i = 1; i < MaxRows; i++)
        {
            _rowPos[i] = _rowPos[i - 1] + RowHeight[i - 1];
        }

        if (!ShowScroll && MaxRows > 0)
        {
            Height = FieldsArePresent switch
            {
                true when HeadingIsPresent => _rowPos[MaxRows - 1] + RowHeight[MaxRows - 1] + 1 + 17 + 15 + 1,
                true => _rowPos[MaxRows - 1] + RowHeight[MaxRows - 1] + 1 + 15 + 2,
                _ => _rowPos[MaxRows - 1] + RowHeight[MaxRows - 1] + 1
            };
        }

        _colPos[0] = 0;
        for (var i = 1; i < MaxCols; i++)
        {
            _colPos[i] = _colPos[i - 1] + ColWidth[i - 1];
        }

        if (ColWidth[MaxCols - 1] != 0)
        {
            _panelHead.Width = _colPos[MaxCols - 1] + ColWidth[MaxCols - 1] + _scrollWidth;
            if (!DesignMode)
            {
                Width = _panelHead.Width + 2;
            }

            switch (FieldsArePresent)
            {
                case true when HeadingIsPresent:
                    _panelHead.Height = 17 + 15 + 1;
                    break;

                case true:
                    _panelHead.Height = 15 + 2;
                    break;

                default:
                    _panelHead.Visible = false;
                    _panelHead.Height = 1;
                    break;
            }

            _panelScroll.Width = Width - 2;
            _panelTable.Width = Width - _scrollWidth - 2;
            if (MaxRows == 0)
            {
                _panelTable.Height = 0;
            }
            else
            {
                _panelTable.Height = _rowPos[MaxRows - 1] + RowHeight[MaxRows - 1];
            }

            _panelScroll.Location = new Point(1, _panelHead.Height - 1);
            _panelScroll.Height = Height - _panelHead.Height;

            if (ShowScroll)
            {
                if (_panelTable.Height < _panelScroll.Height)
                {
                    _vScrollBar1.Enabled = false;
                    _vScrollBar1.Maximum = 1;
                    _vScrollBar1.Value = 1;
                    _panelTable.Location = new Point(0, -1);
                }
                else
                {
                    _vScrollBar1.Enabled = true;
                    _vScrollBar1.Minimum = 1;
                    _vScrollBar1.Maximum = _panelTable.Height + 2;
                    _vScrollBar1.LargeChange = _panelScroll.Height;
                    _vScrollBar1.SmallChange = 3 * 14;
                    if (_panelTable.Height == 0)
                    {
                        _vScrollBar1.Value = 1;
                    }
                    else
                    {
                        _vScrollBar1.Value = _panelTable.Height - _panelScroll.Height + 2;
                    }

                    _panelTable.Location = new Point(0, -_vScrollBar1.Value);
                }
            }
            else
            {
                _vScrollBar1.Visible = false;
                _panelTable.Location = new Point(0, -1);
            }
        }

        if (preserveScroll)
        {
            ScrollValue = scroll;
        }

        Refresh();
    }

    protected void SetRowHeight(int rowStart, int rowStop, int rowHeight)
    {
        for (var i = rowStart; i <= rowStop; i++)
        {
            RowHeight[i] = rowHeight;
        }
    }

    public void ColorRow(int row, Color myColor)
    {
        if (row > MaxRows - 1)
        {
            return;
        }

        var grfx = _panelTable.CreateGraphics();

        for (var j = 0; j < MaxCols; j++)
        {
            BackGColor[j, row] = myColor;
            if (!IsOverflow[j, row])
            {
                continue;
            }

            grfx.FillRectangle(new SolidBrush(BackGColor[j, row]), _colPos[j] + 1, _rowPos[row] + 1, ColWidth[j] - 1, RowHeight[row] - 1);
            grfx.DrawLine(new Pen(LeftBorder[j, row]), _colPos[j], _rowPos[row] + 1, _colPos[j], _rowPos[row] + RowHeight[row] - 1);
        }

        for (var j = 0; j < MaxCols; j++)
        {
            if (IsOverflow[j, row] == false)
            {
                grfx.FillRectangle(new SolidBrush(BackGColor[j, row]), _colPos[j] + 1, _rowPos[row] + 1, ColWidth[j] - 1, RowHeight[row] - 1);
                grfx.DrawLine(new Pen(LeftBorder[j, row]), _colPos[j], _rowPos[row] + 1, _colPos[j], _rowPos[row] + RowHeight[row]);
            }

            _myFontStyle = FontBold[j, row] ? FontStyle.Bold : FontStyle.Regular;
            _myFontFamily = FontFamily.GenericSansSerif;
            _myFontSize = FontSize[j, row] != 0 ? FontSize[j, row] : 8.5f;
            _myFont = new Font(FontFamily.GenericSansSerif, _myFontSize, _myFontStyle);
            _myFontColor = FontColor[j, row].IsEmpty ? Color.Black : FontColor[j, row];

            _offset = ColAlign[j] switch
            {
                HorizontalAlignment.Center => (int) ((ColWidth[j] - grfx.MeasureString(Cell[j, row], _myFont).Width) / 2),
                HorizontalAlignment.Right when _myFontStyle == FontStyle.Bold => ColWidth[j] - (int) Math.Round(Convert.ToDouble(grfx.MeasureString(Cell[j, row], _myFont).Width)) - 1,
                HorizontalAlignment.Right => ColWidth[j] - (int) Math.Round(Convert.ToDouble(grfx.MeasureString(Cell[j, row], _myFont).Width * .92)) - 1,
                _ => 1
            };

            grfx.DrawString(Cell[j, row], _myFont, new SolidBrush(_myFontColor), (float) _colPos[j] + _offset, _rowPos[row] + 1);
        }

        if (row == MaxRows - 1)
        {
            grfx.DrawLine(new Pen(Color.Black), 0, _panelTable.Height - 1, _panelTable.Width - 1, _panelTable.Height - 1);
        }

        grfx.Dispose();
    }

    protected void SetGridColor(Color myColor)
    {
        for (var i = 0; i < MaxRows; i++)
        {
            for (var j = 0; j < MaxCols; j++)
            {
                LeftBorder[j, i] = myColor;
                TopBorder[j, i] = myColor;
            }
        }
    }

    protected void panelHead_Paint(object sender, PaintEventArgs pea)
    {
        if (FieldsArePresent == false)
        {
            return;
        }

        var grfx = pea.Graphics;
        var penB = new Pen(Color.Black);
        var penW = new Pen(Color.White);

        grfx.FillRectangle(new SolidBrush(Color.LightGray), 0, 0, _panelHead.Width, _panelHead.Height);

        var fieldsLoc = HeadingIsPresent ? 17 : 0;

        if (HeadingIsPresent)
        {
            grfx.FillRectangle(new SolidBrush(Color.White), 0, 0, _panelHead.Width, fieldsLoc);
            grfx.DrawLine(penW, 0, 0, _panelHead.Width, 0);
            grfx.DrawLine(penB, 0, fieldsLoc - 1, _panelHead.Width, fieldsLoc - 1);
            grfx.DrawLine(penW, 0, 0, 0, fieldsLoc - 2);
        }

        grfx.DrawLine(penW, 1, fieldsLoc, _panelHead.Width, fieldsLoc);
        grfx.DrawLine(penW, 0, fieldsLoc + 1, 0, fieldsLoc + 15 + 1);

        for (var j = 1; j < MaxCols; j++)
        {
            grfx.DrawLine(penB, _colPos[j], fieldsLoc - 1, _colPos[j], fieldsLoc + 14 + 1);
            grfx.DrawLine(penW, _colPos[j] + 1, fieldsLoc, _colPos[j] + 1, fieldsLoc + 14 + 1);
        }

        var myFont = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);
        var offset = (int) (((float) _panelHead.Width - _scrollWidth - grfx.MeasureString(Heading, myFont).Width) / 2);

        if (HeadingIsPresent)
        {
            grfx.DrawString(Heading, myFont, new SolidBrush(Color.Black), offset, 0);
        }

        myFont = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Bold);

        for (var j = 0; j < MaxCols; j++)
        {
            offset = (int) ((ColWidth[j] - grfx.MeasureString(_fields[j], myFont).Width) / 2);
            grfx.DrawString(_fields[j], myFont, new SolidBrush(Color.Black), _colPos[j] + offset, fieldsLoc);
        }
    }

    private void panelTable_Paint(object sender, PaintEventArgs pea)
    {
        var grfx = pea.Graphics;
        var penB = new Pen(Color.Black);

        var minRow = WorldToLine(pea.ClipRectangle.Top) - 1;
        if (minRow < 0)
        {
            minRow = 0;
        }

        var maxRow = WorldToLine(pea.ClipRectangle.Bottom) + 1;
        if (maxRow > MaxRows)
        {
            maxRow = MaxRows;
        }

        try
        {
            for (var i = minRow; i < maxRow; i++)
            {
                for (var j = 0; j < MaxCols; j++)
                {
                    if (!IsOverflow[j, i])
                    {
                        continue;
                    }

                    grfx.FillRectangle(new SolidBrush(BackGColor[j, i]), _colPos[j] + 1, _rowPos[i] + 1, ColWidth[j] - 1, RowHeight[i] - 1);
                    grfx.DrawLine(new Pen(LeftBorder[j, i]), _colPos[j], _rowPos[i] + 1, _colPos[j], _rowPos[i] + RowHeight[i]);
                }

                for (var j = 0; j < MaxCols; j++)
                {
                    if (IsOverflow[j, i] == false)
                    {
                        grfx.FillRectangle(new SolidBrush(BackGColor[j, i]), _colPos[j] + 1, _rowPos[i] + 1, ColWidth[j] - 1, RowHeight[i] - 1);
                        grfx.DrawLine(new Pen(LeftBorder[j, i]), _colPos[j], _rowPos[i] + 1, _colPos[j], _rowPos[i] + RowHeight[i]);
                    }

                    grfx.DrawLine(new Pen(TopBorder[j, i]), _colPos[j] + 1, _rowPos[i], _colPos[j] + ColWidth[j], _rowPos[i]);

                    _myFontStyle = FontBold[j, i] ? FontStyle.Bold : FontStyle.Regular;
                    _myFontFamily = FontFamily.GenericSansSerif;

                    if (FontSize[j, i] != 0) _myFontSize = FontSize[j, i];
                    else if (FontBold[j, i]) _myFontSize = 8.5f;
                    else _myFontSize = 8.5f;

                    _myFont = new Font(_myFontFamily, _myFontSize, _myFontStyle);
                    _myFontColor = FontColor[j, i].IsEmpty ? Color.Black : FontColor[j, i];

                    _offset = ColAlign[j] switch
                    {
                        HorizontalAlignment.Center => (int) ((ColWidth[j] - grfx.MeasureString(Cell[j, i], _myFont).Width) / 2),
                        HorizontalAlignment.Right when _myFontStyle == FontStyle.Bold => ColWidth[j] - (int) Math.Round(Convert.ToDouble(grfx.MeasureString(Cell[j, i], _myFont).Width)) - 1,
                        HorizontalAlignment.Right => ColWidth[j] - (int) Math.Round(Convert.ToDouble(grfx.MeasureString(Cell[j, i], _myFont).Width * .92)) - 1,
                        _ => 1
                    };

                    grfx.DrawString(Cell[j, i], _myFont, new SolidBrush(_myFontColor), _colPos[j] + _offset, _rowPos[i] + 1);
                }
            }
        }
        catch
        {
            // ignored
        }

        grfx.DrawLine(penB, 0, _panelTable.Height - 1, _panelTable.Width - 1, _panelTable.Height - 1);
    }

    private void ContrTable_Paint(object sender, PaintEventArgs e)
    {
        var grfx = e.Graphics;
        var penBlue = new Pen(Color.FromArgb(127, 157, 185));

        grfx.DrawLine(penBlue, 0, 0, 0, Height - 1);
        grfx.DrawLine(penBlue, 0, Height - 1, Width - 1, Height - 1);
        grfx.DrawLine(penBlue, 0, 0, Width, 0);
        grfx.DrawLine(penBlue, Width - 1, 0, Width - 1, Height - 1);
    }

    private void panelTable_KeyPress(object sender, KeyPressEventArgs e)
    {
        OnKeyPress(e);
    }

    private void panelTable_DoubleClick(object sender, EventArgs e)
    {
        var rowIndex = 0;

        for (var i = 0; i < MaxRows; i++)
        {
            if (_mouseDownPosition.Y > _rowPos[i])
            {
                rowIndex = i;
            }
        }

        OnCellDoubleClicked(new CellEventArgs(rowIndex));
    }

    private void panelTable_MouseDown(object sender, MouseEventArgs e)
    {
        _mouseDownPosition = new Point(e.X, e.Y);
    }

    private void panelTable_MouseUp(object sender, MouseEventArgs e)
    {
        var ea = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y + _panelScroll.Top + _panelTable.Top, e.Delta);

        OnMouseUp(ea);

        if (e.Button == MouseButtons.Right)
        {
            return;
        }

        var rowIndex = 0;

        for (var i = 0; i < MaxRows; i++)
        {
            if (_mouseDownPosition.Y > _rowPos[i])
            {
                rowIndex = i;
            }
        }

        switch (SelectionMode)
        {
            case SelectionMode.None:
                break;

            case SelectionMode.One:
                if (SelectedRow != -1)
                {
                    ColorRow(SelectedRow, Color.White);
                }

                ColorRow(rowIndex, Color.Silver);
                SelectedRow = rowIndex;
                break;

            case SelectionMode.MultiExtended:
                var arrayList = new ArrayList();
                foreach (var inde in _selectedIndices)
                {
                    arrayList.Add(inde);
                }

                if (!_controlIsDown)
                {
                    foreach (var t in arrayList)
                    {
                        ColorRow((int) t, Color.White);
                    }

                    arrayList.Clear();
                    arrayList.Add(rowIndex);

                    ColorRow(rowIndex, Color.Silver);
                }
                else
                {
                    if (arrayList.Contains(rowIndex))
                    {
                        arrayList.Remove(rowIndex);

                        ColorRow(rowIndex, Color.White);
                    }
                    else
                    {
                        arrayList.Add(rowIndex);

                        ColorRow(rowIndex, Color.Silver);
                    }
                }

                _selectedIndices = new int[arrayList.Count];
                for (var i = 0; i < arrayList.Count; i++)
                {
                    _selectedIndices[i] = (int) arrayList[i];
                }

                if (_selectedIndices.Length == 1)
                {
                    SelectedRow = _selectedIndices[0];
                }

                break;
        }

        OnCellClicked(new CellEventArgs(rowIndex));
    }

    private void panelTable_MouseWheel(object sender, MouseEventArgs e)
    {
        if (!ShowScroll)
        {
            return;
        }

        if (_panelTable.Height < _panelScroll.Height)
        {
            return;
        }

        var max = _panelTable.Height - _panelScroll.Height + 3;

        var newScrollVal = _vScrollBar1.Value - e.Delta / 3;
        if (newScrollVal > max)
        {
            _vScrollBar1.Value = max;
        }
        else if (newScrollVal < _vScrollBar1.Minimum)
        {
            _vScrollBar1.Value = _vScrollBar1.Minimum;
        }
        else
        {
            _vScrollBar1.Value = newScrollVal;
        }

        _panelTable.Location = new Point(0, -_vScrollBar1.Value);
    }

    private void panelTable_KeyDown(object sender, KeyEventArgs e)
    {
        OnKeyDown(e);
        if (e.KeyCode == Keys.ControlKey)
        {
            _controlIsDown = true;
        }
    }

    private void panelTable_KeyUp(object sender, KeyEventArgs e)
    {
        OnKeyUp(e);
        if (e.KeyCode == Keys.ControlKey)
        {
            _controlIsDown = false;
        }
    }

    private void vScrollBar1_KeyPress(object sender, KeyPressEventArgs e)
    {
        OnKeyPress(e);
    }

    private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
    {
        _panelTable.Location = new Point(0, -e.NewValue);
        _panelTable.Select();
    }

    public delegate void CellEventHandler(object sender, CellEventArgs e);

    public event CellEventHandler CellClicked;

    protected virtual void OnCellClicked(CellEventArgs e)
    {
        CellClicked?.Invoke(this, e);
    }

    protected virtual void OnCellDoubleClicked(CellEventArgs e)
    {
    }

    public int WorldToLine(int y)
    {
        var retVal = 0;
        for (var i = 0; i < MaxRows; i++)
        {
            if (y > _rowPos[i])
            {
                retVal = i;
            }
        }

        return retVal;
    }

    private void panelHead_Click(object sender, EventArgs e)
    {
        _panelTable.Select();
    }
}

public class CellEventArgs(int row) : EventArgs
{
    public int Row { get; } = row;
}