using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CodeBase;
using DataConnectionBase;
using OpenDental.Thinfinity;
using OpenDentBusiness;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Margins = System.Drawing.Printing.Margins;

namespace OpenDental.Graph.Dashboard
{
    public partial class FormPrintImage : Form
    {
        private const int CHART_SPACING = 30;
        private readonly DashboardPanelCtrl _dashPanel;
        private Dictionary<Point, Chart> _dictCharts;
        private bool _isLoading = true;
        private int _marginWidth;
        private int _marginHeight;
        private int _imageWidth;
        private int _imageHeight;
        private int _xPos;
        private int _yPos;
        private Bitmap _bmpSheet;
        private Bitmap _bmpImage;
        private XGraphics _xg;
        private Graphics _g;

        private Rectangle PageDimensions => new Rectangle(0, 0, printDocument1.DefaultPageSettings.Bounds.Width, printDocument1.DefaultPageSettings.Bounds.Height);

        public FormPrintImage(DashboardPanelCtrl dashPanel)
        {
            InitializeComponent();
            _dashPanel = dashPanel;
        }

        private void FormPrintSettings_Load(object sender, EventArgs e)
        {
            printPreviewControl.Document = new PrintDocument();
            _dictCharts = _dashPanel.GetGraphsAsDictionary();
            //figure out whether the printed page should default to landscape or portait
            checkLandscape.Checked = (_dashPanel.Columns * 8 > _dashPanel.Rows * 5);
            printDocument1.DefaultPageSettings.Landscape = checkLandscape.Checked;
            FillDimensions();
            _bmpImage = ConvertToBmp(true);
            //we want the chart to maintain its aspect ratio.
            textWidth.Text = _bmpImage.Width.ToString();
            textHeight.Text = _bmpImage.Height.ToString();
            MakePage(false);
            _isLoading = false;
        }

        private Bitmap ConvertToBmp(bool useDefaultSize)
        {
            //the preliminary size of the entire image. This is the total space we have available to use.
            Rectangle imgBounds;
            int chartWidth;
            int chartHeight;
            //base the chart heights and width on the total space we have to fill the entire image. Charts often look stretched.
            //We want to default the charts to be wider than they are tall. To prevent stretched looking charts.
            //However, in order to render the charts at the highest quality regardless of what height the user inputs, we'll only enforce this while loading.
            if (useDefaultSize)
            {
                imgBounds = new Rectangle(0, 0, PageDimensions.Width - (_marginWidth * 2), PageDimensions.Height - (_marginHeight * 2));
                chartWidth = ((imgBounds.Width - (CHART_SPACING * (_dashPanel.Columns - 1))) / _dashPanel.Columns);
                chartHeight = ((imgBounds.Height - (CHART_SPACING * (_dashPanel.Rows - 1))) / _dashPanel.Rows);
                if (chartHeight > (2 * chartWidth / 3))
                {
                    chartHeight = (2 * chartWidth / 3); //make the height shorter.
                    imgBounds.Height = (chartHeight * _dashPanel.Rows) + (CHART_SPACING * (_dashPanel.Rows - 1)); //now change the size of the total space to match.
                }
            }
            else
            {
                imgBounds = new Rectangle(0, 0, _imageWidth, _imageHeight);
                chartWidth = ((imgBounds.Width - (CHART_SPACING * (_dashPanel.Columns - 1))) / _dashPanel.Columns);
                chartHeight = ((imgBounds.Height - (CHART_SPACING * (_dashPanel.Rows - 1))) / _dashPanel.Rows);
            }

            chartWidth = Math.Max(chartWidth, 1);
            chartHeight = Math.Max(chartHeight, 1);
            //below we actually generate the bitmap and draw each chart to it's corresponding location.
            //If they have a chart area without a chart, it won't draw anything.
            var bmp = new Bitmap(imgBounds.Width, imgBounds.Height);
            for (var row = 0; row < _dashPanel.Rows; row++)
            {
                for (var col = 0; col < _dashPanel.Columns; col++)
                {
                    if (!_dictCharts.TryGetValue(new Point(row, col), out var chartCur))
                    {
                        continue;
                    }

                    var chartResult = new Chart();
                    using (var ms = new MemoryStream())
                    {
                        chartCur.Serializer.Save(ms);
                        chartResult.Serializer.Load(ms); //now we have a copy of the chart that we can manipulate without affecting the parent chart.
                    }

                    chartResult.Width = chartWidth;
                    chartResult.Height = chartHeight;
                    chartResult.DrawToBitmap(bmp, new Rectangle(((col * chartWidth) + ((col) * CHART_SPACING)), ((row * chartHeight) + ((row) * CHART_SPACING)), chartWidth, chartHeight));
                }
            }

            return bmp;
        }

        private void MakePage(bool createBitmap)
        {
            //check to see if landscape mode was toggled.
            printDocument1.DefaultPageSettings.Landscape = checkLandscape.Checked;
            //update private variables
            FillDimensions();
            if (createBitmap)
            {
                _bmpImage?.Dispose();

                _bmpImage = ConvertToBmp(false);
            }

            //reset the document margins
            printDocument1.DefaultPageSettings.Margins = new Margins(_marginWidth, _marginWidth, _marginHeight, _marginHeight);

            //Reset the graphics objects by disposing of them and reinitializing them.
            _bmpSheet?.Dispose();
            _xg?.Dispose();
            _g?.Dispose();

            _bmpSheet = new Bitmap(PageDimensions.Width, PageDimensions.Height);
            _g = Graphics.FromImage(_bmpSheet);
            _xg = XGraphics.FromGraphics(_g, new XSize(_bmpSheet.Width, _bmpSheet.Height));
            //margins are taken care of because we set them up in DefaultPageSettings.
            _xg.DrawImage(_bmpImage, _xPos, _yPos, _imageWidth, _imageHeight);
            printPreviewControl.Document = printDocument1;
        }

        private void FillDimensions()
        {
            _imageWidth = FuncGetIntFromText(textWidth.Text, 1, 10000);
            _imageHeight = FuncGetIntFromText(textHeight.Text, 1, 10000);
            _marginWidth = FuncGetIntFromText(textMarginWidth.Text, 0, PageDimensions.Width);
            _marginHeight = FuncGetIntFromText(textMarginHeight.Text, 0, PageDimensions.Height);
            _xPos = SIn.Int(textXPos.Text, false);
            _yPos = SIn.Int(textYPos.Text, false);
            return;

            int FuncGetIntFromText(string text, int minVal, int maxVal)
            {
                var ret = Math.Max(minVal, SIn.Int(text, false));
                return Math.Min(ret, maxVal);
            }
        }

        private void ImageGenericFormat_PrintPage(object sender, PrintPageEventArgs ev)
        {
            ev.Graphics.DrawImage(_bmpSheet, 0, 0, PageDimensions.Width, PageDimensions.Height);
        }

        private void butExport_Click(object sender, EventArgs e)
        {
            var sd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FilterIndex = 1,
                RestoreDirectory = true,
            };
            if (ODEnvironment.IsCloudInstance)
            {
                sd.FileName = ODFileUtils.CombinePaths(Path.GetTempPath(), "image_export.pdf");
            }
            else
            {
                if (sd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            using var pdfDoc = new PdfDocument();

            //save the chart into the memoryStream as a BitMap
            try
            {
                var page = new PdfPage();
                pdfDoc.Pages.Add(page);
                page.Orientation = checkLandscape.Checked ? PageOrientation.Landscape : PageOrientation.Portrait;
                using (var xGraphics = XGraphics.FromPdfPage(page))
                {
                    xGraphics.DrawImage(_bmpSheet, _xPos + _marginWidth, _yPos + _marginHeight, page.Width, page.Height);
                }

                pdfDoc.Save(sd.FileName);
                if (false)
                {
                    ThinfinityUtils.ExportForDownload(sd.FileName);
                }

                if (false)
                {
                    ODCloudClient.ExportForAppStream(sd.FileName);
                }

                MessageBox.Show(Lans.g(this, "File saved."));
            }
            catch (Exception ex)
            {
                MessageBox.Show("File not saved." + "\r\n" + ex.Source + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void butPrint_Click(object sender, EventArgs e)
        {
            printPreviewControl.Document.Print();
        }

        private void butClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void refresh_Event(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            timer1.Stop();
            timer1.Start();
        }

        private void checkLandscape_CheckedChanged(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            MakePage(true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            timer1.Stop();
            MakePage(true);
        }
    }
}