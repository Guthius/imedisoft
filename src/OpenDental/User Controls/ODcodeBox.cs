using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;

namespace OpenDental;

public class ODcodeBox : RichTextBox
{
    private readonly ContextMenu _contextMenu = new();
    private int _lastLineWidth;
    private int _lineNumberWidth = -1;
    private SizeF _lineNumberSize;
    private List<int> _listIndicesOfNewLines;
    private readonly StringFormat _stringFormatForDrawingNumbers;
    private readonly Font _lineNumberFont = new("Courier New", 9F, FontStyle.Regular);
    private readonly Brush _brushOlive = new SolidBrush(Color.Olive);

    public ODcodeBox() : this(false)
    {
    }

    public ODcodeBox(bool isQueryText)
    {
        _stringFormatForDrawingNumbers = new StringFormat();
        _stringFormatForDrawingNumbers.Alignment = StringAlignment.Center;
        _stringFormatForDrawingNumbers.LineAlignment = StringAlignment.Near;
        _stringFormatForDrawingNumbers.Trimming = StringTrimming.None;

        EventHandler onClickCut = menuItemCut_Click;
        EventHandler onClickCopy = menuItemCopy_Click;
        EventHandler onClickPaste = menuItemPaste_Click;
        EventHandler onClickSelectAll = menuItemSelectAll_Click;

        _contextMenu.MenuItems.Add(new MenuItem("Cut", onClickCut, Shortcut.CtrlX));
        _contextMenu.MenuItems.Add(new MenuItem("Copy", onClickCopy, Shortcut.CtrlC));
        _contextMenu.MenuItems.Add(new MenuItem("Paste", onClickPaste, Shortcut.CtrlV));
        _contextMenu.MenuItems.Add(new MenuItem("Select All", onClickSelectAll, Shortcut.CtrlA));

        if (isQueryText)
        {
            EventHandler onClickCommentLine = menuItemAddComment_Click;
            EventHandler onClickUnCommentLine = menuItemRemoveComment_Click;

            _contextMenu.MenuItems.Add(new MenuItem("Comment Selection", onClickCommentLine, Shortcut.CtrlShiftC));
            _contextMenu.MenuItems.Add(new MenuItem("Remove Comment From Selection", onClickUnCommentLine, Shortcut.CtrlShiftR));
        }

        ContextMenu = _contextMenu;
    }

    private List<int> GetListIndicesOfNewLines()
    {
        if (_listIndicesOfNewLines != null)
        {
            return _listIndicesOfNewLines;
        }

        _listIndicesOfNewLines = [];

        var index = 0;
        foreach (var character in Text)
        {
            if (character == '\n')
            {
                _listIndicesOfNewLines.Add(index);
            }

            index++;
        }

        _listIndicesOfNewLines.Add(Text.Length - 1);

        return _listIndicesOfNewLines;
    }

    private int GetLineNumberWidth()
    {
        if (_lineNumberWidth > 0)
        {
            return _lineNumberWidth;
        }

        var numberOfDigits = 1;
        if (GetListIndicesOfNewLines().Count > 0)
        {
            numberOfDigits = (int) (1 + Math.Log(GetListIndicesOfNewLines().Count, 10));
        }

        var widthForDigits = new string('Z', numberOfDigits);

        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);

        _lineNumberSize = graphics.MeasureString(widthForDigits, _lineNumberFont);
        _lineNumberWidth = (int) Math.Ceiling(_lineNumberSize.Width + 10);

        return _lineNumberWidth;
    }

    protected override void OnTextChanged(EventArgs e)
    {
        LineNumberRecalculate();

        base.OnTextChanged(e);
    }

    private void LineNumberRecalculate()
    {
        _listIndicesOfNewLines = null;
        _lineNumberWidth = -1;

        WindowsApiWrapper.SendMessage(Handle, (int) WindowsApiWrapper.WinMessagesOther.WM_PAINT, 0, 0);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == (int) WindowsApiWrapper.WinMessagesOther.WM_CHAR)
        {
            LineNumberRecalculate();
            base.WndProc(ref m);
            return;
        }

        if (m.Msg == (int) WindowsApiWrapper.WinMessagesOther.WM_PAINT)
        {
            base.WndProc(ref m);
            using (var g = Graphics.FromHwnd(Handle))
            {
                PaintLineNumbers(g);
            }

            return;
        }

        base.WndProc(ref m);
    }

    private void PaintLineNumbers(Graphics g)
    {
        var lineNumberWidth = GetLineNumberWidth();

        if (lineNumberWidth != _lastLineWidth)
        {
            SetLeftMargin(lineNumberWidth + Margin.Left);

            _lastLineWidth = lineNumberWidth;

            g.FillRectangle(Brushes.White, ClientRectangle);

            WindowsApiWrapper.SendMessage(Handle, (int) WindowsApiWrapper.WinMessagesOther.WM_PAINT, 0, 0);

            return;
        }

        var firstLineNumber = GetListIndicesOfNewLines().Count;
        var firstCharIndex = GetCharIndexFromPosition(new Point(1, 1));

        var idx = 0;
        foreach (var index in GetListIndicesOfNewLines())
        {
            idx++;
            if (firstCharIndex > index)
            {
                continue;
            }

            firstLineNumber = idx;
            break;
        }

        if (Height == 0)
        {
            Height++;
        }

        using var bitmap = new Bitmap(lineNumberWidth, Height);
        using var graphics = Graphics.FromImage(bitmap);

        var rect = new Rectangle(0, 0, lineNumberWidth, Height);

        graphics.FillRectangle(SystemBrushes.ControlLight, rect);

        var listIndicesNewLines = GetListIndicesOfNewLines();
        for (var i = firstLineNumber; i <= listIndicesNewLines.Count; i++)
        {
            var curPointY = 0;
            if (firstLineNumber != 1)
            {
                curPointY = GetPosFromCharIndex(listIndicesNewLines[i - 2] + 1).Y;
            }

            if (Height <= curPointY)
            {
                break;
            }

            var rectDraw = new Rectangle(0, curPointY, lineNumberWidth, _lineNumberSize.ToSize().Height);

            graphics.DrawString(firstLineNumber.ToString(), _lineNumberFont, _brushOlive, rectDraw, _stringFormatForDrawingNumbers);

            firstLineNumber++;
        }

        g.DrawImage(bitmap, new Point(0, 0));
    }

    private Point GetPosFromCharIndex(int index)
    {
        var rawPointSize = Marshal.SizeOf(typeof(Point));
        var wParam = Marshal.AllocHGlobal(rawPointSize);
        WindowsApiWrapper.SendMessage(Handle, (int) WindowsApiWrapper.EM_Rich.EM_POSFROMCHAR, wParam, index);
        var point = (Point) Marshal.PtrToStructure(wParam, typeof(Point));
        Marshal.FreeHGlobal(wParam);
        return point;
    }

    private void SetLeftMargin(int widthInPixels)
    {
        WindowsApiWrapper.SendMessage(Handle, (int) WindowsApiWrapper.EM_Rich.EM_SETMARGINS, WindowsApiWrapper.EC_LEFTMARGIN, widthInPixels);
    }

    private void menuItemCopy_Click(object sender, EventArgs e)
    {
        Copy();
    }

    private void menuItemPaste_Click(object sender, EventArgs e)
    {
        var format = DataFormats.GetFormat("Text");

        Paste(format);
    }

    private void menuItemCut_Click(object sender, EventArgs e)
    {
        Cut();
    }

    private void menuItemSelectAll_Click(object sender, EventArgs e)
    {
        SelectAll();
    }

    private void menuItemRemoveComment_Click(object sender, EventArgs e)
    {
        if (SelectedText.Contains("/*") && SelectedText.Contains("*/"))
        {
            var temp = SelectedText;

            temp = temp.Replace("/*", "");
            temp = temp.Replace("*/", "");

            SelectedText = temp;

            return;
        }

        var newLineIndex = Text.Substring(0, SelectionStart).LastIndexOf("\n");
        if (newLineIndex == -1)
        {
            if (Text.Substring(0, 3) != "-- ")
            {
                return;
            }

            var saveIndex = SelectionStart;

            Select(0, 3);
            SelectedText = SelectedText.Replace("-- ", "");
            Select(saveIndex - 3, 0);

            return;
        }

        if (Text.Substring(newLineIndex + 1, 3) != "-- ")
        {
            return;
        }

        {
            var saveIndex = SelectionStart;

            Select(newLineIndex + 1, 3);
            SelectedText = SelectedText.Replace("-- ", "");
            Select(saveIndex - 3, 0);
        }
    }

    private void menuItemAddComment_Click(object sender, EventArgs e)
    {
        var selectedComment = SelectedText;

        if (!string.IsNullOrEmpty(selectedComment))
        {
            SelectedText = $"/*{selectedComment}*/";
            return;
        }

        var newLineIndex = Text.Substring(0, SelectionStart).LastIndexOf("\n");
        if (newLineIndex == -1)
        {
            Select(0, 0);
        }
        else
        {
            Select(newLineIndex + 1, 0);
        }

        SelectedText = $"-- {SelectedText}";
    }
}