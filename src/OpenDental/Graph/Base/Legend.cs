using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenDental.Graph.Base
{
    public partial class Legend : UserControl
    {
        private LegendDockType _legendDock = LegendDockType.Left;
        private List<ODGraphLegendItem> _legendItems = new List<ODGraphLegendItem>();
        private float _paddingPx = 3f;
        private int _scrollOffsetX;
        private int _contentWidth;
        private DateTime _stepDownStart;
        private DateTime _stepUpStart;
        private bool _mouseIsDown;
        private int _lastMouseX;

        public LegendDockType LegendDock
        {
            get => _legendDock;
            set
            {
                _legendDock = value;
                _scrollOffsetX = 0;
                try
                {
                    switch (_legendDock)
                    {
                        case LegendDockType.Bottom:
                            panelDraw.AutoScroll = false;
                            panelDraw.AutoScrollMinSize = new Size(0, 0);
                            tableLayoutPanel1.ColumnStyles[0].Width = 68;
                            tableLayoutPanel1.ColumnStyles[2].Width = 68;
                            break;
                        
                        case LegendDockType.Left:
                            panelDraw.AutoScroll = true;
                            tableLayoutPanel1.ColumnStyles[0].Width = 0;
                            tableLayoutPanel1.ColumnStyles[2].Width = 0;
                            break;
                        
                        case LegendDockType.None:
                        default:
                            break;
                    }
                }
                catch
                {
                    // ignored
                }

                panelDraw.Invalidate();
            }
        }

        public float PaddingPx
        {
            get => _paddingPx;
            set
            {
                _paddingPx = value;
                panelDraw.Invalidate();
            }
        }

        public Legend()
        {
            InitializeComponent();
        }

        public void SetLegendItems(List<ODGraphLegendItem> items)
        {
            _legendItems = items;
            float leftBoxPx = 0;
            using (var g = panelDraw.CreateGraphics())
            {
                var boxEdgePx = g.MeasureString("A", Font).Height;
                foreach (var legendItem in _legendItems)
                {
                    leftBoxPx += PaddingPx + boxEdgePx + PaddingPx + g.MeasureString(legendItem.ItemName, Font).Width + PaddingPx;
                }
            }

            _contentWidth = (int) Math.Ceiling(leftBoxPx);

            Invalidate();

            panelDraw.Invalidate();
        }

        private void DrawLegendBottom(PaintEventArgs e)
        {
            const float topPx = 5;
            float leftBoxPx = 0;
            var boxEdgePx = e.Graphics.MeasureString("A", Font).Height;
            e.Graphics.TranslateTransform(_scrollOffsetX, 0);
            foreach (var legendItem in _legendItems)
            {
                using var brushText = new SolidBrush(legendItem.IsEnabled ? ForeColor : Color.FromArgb(100, ForeColor));
                leftBoxPx += PaddingPx;
                var sizeText = e.Graphics.MeasureString(legendItem.ItemName, Font);
                using (Brush hoverBox = new SolidBrush(legendItem.Hovered ? Color.LightCoral : panelDraw.BackColor))
                {
                    e.Graphics.FillRectangle(hoverBox, leftBoxPx - 2, topPx - 2, boxEdgePx + 4, boxEdgePx + 4);
                }

                using (Brush brushBox = new SolidBrush(legendItem.IsEnabled ? legendItem.ItemColor : Color.FromArgb(50, legendItem.ItemColor)))
                {
                    e.Graphics.FillRectangle(brushBox, leftBoxPx, topPx, boxEdgePx, boxEdgePx);
                    legendItem.LocationBox = new Rectangle((int) leftBoxPx, (int) topPx, (int) boxEdgePx, (int) boxEdgePx);
                }

                var textLeftPx = leftBoxPx + boxEdgePx + PaddingPx;
                e.Graphics.DrawString(legendItem.ItemName, Font, brushText, textLeftPx, topPx);
                leftBoxPx = textLeftPx + sizeText.Width + PaddingPx;
            }
        }

        private void DrawLegendLeft(PaintEventArgs e)
        {
            float topPx = 0;
            var leftBoxPx = PaddingPx;
            var boxEdgePx = e.Graphics.MeasureString("A", Font).Height;
            var maxRightEdge = 0f;
            var maxBottom = 0f;
            e.Graphics.TranslateTransform(panelDraw.AutoScrollPosition.X, panelDraw.AutoScrollPosition.Y);
            foreach (var legendItem in _legendItems)
            {
                using var brushText = new SolidBrush(legendItem.IsEnabled ? ForeColor : Color.FromArgb(100, ForeColor));

                topPx += PaddingPx;
                var size = e.Graphics.MeasureString(legendItem.ItemName, Font);
                using (Brush hoverBox = new SolidBrush(legendItem.Hovered ? Color.LightCoral : panelDraw.BackColor))
                {
                    e.Graphics.FillRectangle(hoverBox, leftBoxPx - 2, topPx - 2, boxEdgePx + 4, boxEdgePx + 4);
                }

                using (Brush brushBox = new SolidBrush(legendItem.IsEnabled ? legendItem.ItemColor : Color.FromArgb(50, legendItem.ItemColor)))
                {
                    e.Graphics.FillRectangle(brushBox, leftBoxPx, topPx, boxEdgePx, boxEdgePx);
                    legendItem.LocationBox = new Rectangle((int) leftBoxPx, (int) topPx, (int) boxEdgePx, (int) boxEdgePx);
                }

                var textLeftPx = leftBoxPx + boxEdgePx + PaddingPx;
                e.Graphics.DrawString(legendItem.ItemName, Font, brushText, textLeftPx, topPx);
                topPx += boxEdgePx;
                maxRightEdge = Math.Max(maxRightEdge, textLeftPx + size.Width);
                maxBottom += PaddingPx + size.Height;
            }

            var sizeContents = new Size((int) Math.Ceiling(maxRightEdge), (int) Math.Ceiling(maxBottom));
            if (panelDraw.AutoScrollMinSize != sizeContents)
            {
                panelDraw.AutoScrollMinSize = sizeContents;
            }
        }

        private void Legend_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            switch (LegendDock)
            {
                case LegendDockType.Bottom:
                    DrawLegendBottom(e);
                    break;
                case LegendDockType.Left:
                    DrawLegendLeft(e);
                    break;
                case LegendDockType.None:
                default:
                    break;
            }
        }

        private void butScrollEnd_Click(object sender, EventArgs e)
        {
            var maxScroll = _contentWidth - panelDraw.Width;

            _scrollOffsetX = -maxScroll;

            panelDraw.Invalidate();
        }

        private void butScrollDownStep_Click(object sender, EventArgs e)
        {
            _scrollOffsetX -= 50;

            var maxScroll = _contentWidth - panelDraw.Width;
            if (Math.Abs(_scrollOffsetX) > maxScroll)
            {
                _scrollOffsetX = -maxScroll;
            }

            if (_scrollOffsetX > 0)
            {
                _scrollOffsetX = 0;
            }

            panelDraw.Invalidate();
        }

        private void butScrollStart_Click(object sender, EventArgs e)
        {
            _scrollOffsetX = 0;
            panelDraw.Invalidate();
        }


        private void butScrollUpStep_Click(object sender, EventArgs e)
        {
            _scrollOffsetX += 50;

            if (_scrollOffsetX > 0)
            {
                _scrollOffsetX = 0;
            }

            panelDraw.Invalidate();
        }

        private void timerStepUp_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Subtract(_stepUpStart).TotalMilliseconds > 400)
            {
                timerStepUp.Interval = 20;
            }

            butScrollUpStep_Click(sender, e);
        }

        private void timerStepDown_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Subtract(_stepDownStart).TotalMilliseconds > 400)
            {
                timerStepDown.Interval = 20;
            }

            butScrollDownStep_Click(sender, e);
        }

        private void butScrollDownStep_MouseDown(object sender, MouseEventArgs e)
        {
            _stepDownStart = DateTime.Now;

            timerStepDown.Interval = 200;
            timerStepDown.Start();
        }

        private void butScrollDownStep_MouseUp(object sender, MouseEventArgs e)
        {
            timerStepDown.Stop();
        }

        private void butScrollUpStep_MouseDown(object sender, MouseEventArgs e)
        {
            _stepUpStart = DateTime.Now;

            timerStepUp.Interval = 200;
            timerStepUp.Start();
        }

        private void butScrollUpStep_MouseUp(object sender, MouseEventArgs e)
        {
            timerStepUp.Stop();
        }

        private void panelDraw_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseIsDown = true;
            _lastMouseX = e.X;
            
            var clicked = LegendDock == LegendDockType.Bottom
                ? _legendItems.FirstOrDefault(x =>
                    new Rectangle(x.LocationBox.X + _scrollOffsetX, x.LocationBox.Y, x.LocationBox.Width, x.LocationBox.Height)
                        .Contains(e.X, e.Y))
                : _legendItems.FirstOrDefault(x =>
                    new Rectangle(x.LocationBox.X, x.LocationBox.Y + panelDraw.AutoScrollPosition.Y, x.LocationBox.Width, x.LocationBox.Height)
                        .Contains(e.X, e.Y));

            if (clicked == null)
            {
                return;
            }

            clicked.IsEnabled = !clicked.IsEnabled;
            clicked.Filter();
        }

        private void panelDraw_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (var legendItem in _legendItems)
            {
                legendItem.Hovered = false;
            }

            if (!_mouseIsDown)
            {
                ODGraphLegendItem hovered;
                if (LegendDock == LegendDockType.Bottom)
                {
                    hovered = _legendItems //find the hovered item.
                        .FirstOrDefault(x => new Rectangle(x.LocationBox.X + _scrollOffsetX, x.LocationBox.Y, x.LocationBox.Width, x.LocationBox.Height)
                            .Contains(e.X, e.Y));
                }
                else
                {
                    hovered = _legendItems //find the hovered item.
                        .FirstOrDefault(x => new Rectangle(x.LocationBox.X, x.LocationBox.Y + panelDraw.AutoScrollPosition.Y, x.LocationBox.Width, x.LocationBox.Height)
                            .Contains(e.X, e.Y));
                }

                if (hovered == null)
                {
                    //not hovering over a box, don't mark any hovered and redraw.
                    panelDraw.Invalidate();
                    return;
                }

                hovered.Hovered = true; //hovering over a box, mark that box as hovered and redraw.
                panelDraw.Invalidate();
            }
            else
            {
                //dragging
                if (LegendDock != LegendDockType.Bottom)
                {
                    //no dragging for Left-Docked legends; they get a scrollbar.
                    return;
                }

                var move = e.X - _lastMouseX;
                _lastMouseX = e.X;
                _scrollOffsetX += move;
                var maxScroll = _contentWidth - panelDraw.Width;
                if (Math.Abs(_scrollOffsetX) > maxScroll)
                {
                    _scrollOffsetX = -maxScroll;
                }

                if (_scrollOffsetX > 0)
                {
                    _scrollOffsetX = 0;
                }

                panelDraw.Invalidate();
            }
        }

        private void panelDraw_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseIsDown = false;
            _lastMouseX = 0;
        }
        
        public Legend PrintCopy()
        {
            var retVal = new Legend
            {
                BackColor = Color.White,
                LegendDock = LegendDockType.Bottom,
                BorderStyle = BorderStyle.Fixed3D,
                Dock = DockStyle.None,
            };
            retVal.SetLegendItems(_legendItems);
            retVal.tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Absolute;
            retVal.tableLayoutPanel1.ColumnStyles[0].Width = 0;
            retVal.tableLayoutPanel1.ColumnStyles[2].SizeType = SizeType.Absolute;
            retVal.tableLayoutPanel1.ColumnStyles[2].Width = 0;
            return retVal;
        }
        
        private class PanelNoFlicker : Panel
        {
            public PanelNoFlicker()
            {
                DoubleBuffered = true;
            }
        }
    }
}