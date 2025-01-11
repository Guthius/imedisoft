using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using OpenDental.UI;
using OpenDentBusiness;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using BorderStyle = MigraDoc.DocumentObjectModel.BorderStyle;
using Color = System.Drawing.Color;
using Font = MigraDoc.DocumentObjectModel.Font;

namespace OpenDental
{
    public class MigraDocHelper
    {
        public static TextFrame CreateContainer(Section section)
        {
            var framebig = section.AddTextFrame();
            framebig.RelativeVertical = RelativeVertical.Line;
            framebig.RelativeHorizontal = RelativeHorizontal.Page;
            framebig.MarginLeft = Unit.Zero;
            framebig.MarginTop = Unit.Zero;
            framebig.Top = TopPosition.Parse("0 in");
            framebig.Left = LeftPosition.Parse("0 in");
            framebig.Width = Unit.FromInch(CultureInfo.CurrentCulture.Name == "en-US" ? 8.5 : 8.3);

            return framebig;
        }

        public static TextFrame CreateContainer(Section section, float xPos, float yPos, float width, float height)
        {
            var framebig = section.AddTextFrame();
            framebig.RelativeVertical = RelativeVertical.Page;
            framebig.RelativeHorizontal = RelativeHorizontal.Page;
            framebig.MarginLeft = Unit.Zero;
            framebig.MarginTop = Unit.Zero;
            framebig.Top = TopPosition.Parse(yPos / 100 + " in");
            framebig.Left = LeftPosition.Parse(xPos / 100 + " in");
            framebig.Width = Unit.FromInch(width / 100);
            framebig.Height = Unit.FromInch(height / 100);
            return framebig;
        }

        public static int GetDocWidth()
        {
            return CultureInfo.CurrentCulture.Name == "en-US" ? 850 : 830;
        }

        public static void DrawString(TextFrame frameContainer, string text, Font font, RectangleF rectF, ParagraphAlignment alignment = ParagraphAlignment.Left)
        {
            var frame = new TextFrame();
            var par = frame.AddParagraph();

            par.Format.Font = font.Clone();
            par.Format.Alignment = alignment;
            par.AddText(text);

            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.MarginLeft = Unit.FromInch(rectF.Left / 100);
            frame.MarginTop = Unit.FromInch(rectF.Top / 100);
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = Unit.FromInch(rectF.Right / 100f);

            var bottom = Unit.FromInch(rectF.Bottom / 100f);
            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
        }

        public static void DrawString(TextFrame frameContainer, string text, Font font, float xPos, float yPos)
        {
            var frame = new TextFrame();
            var par = frame.AddParagraph();

            par.Format.Font = font.Clone();
            par.AddText(text);

            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.MarginLeft = Unit.FromInch(xPos / 100);
            frame.MarginTop = Unit.FromInch(yPos / 100);
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = frameContainer.Width;

            var fontstyle = FontStyle.Regular;
            if (font.Bold)
            {
                fontstyle = FontStyle.Bold;
            }

            var fontSystem = new System.Drawing.Font(font.Name, (float) font.Size.Point, fontstyle);
            float fontH = fontSystem.Height;
            var bottom = Unit.FromInch((yPos + fontH) / 100);
            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
        }

        public static void DrawBitmap(TextFrame frameContainer, Bitmap bitmap, float xPos, float yPos)
        {
            var imageFileName = PrefC.GetRandomTempFile(".tmp");

            bitmap.SetResolution(100, 100);
            bitmap.Save(imageFileName);

            var frame = new TextFrame();

            frame.AddImage(imageFileName);
            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.MarginLeft = Unit.FromInch(xPos / 100);
            frame.MarginTop = Unit.FromInch(yPos / 100);
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = frameContainer.Width;

            var bottom = Unit.FromInch((yPos + (double) bitmap.Height) / 100);
            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
        }

        public static Font CreateFont(float fsize)
        {
            return new Font
            {
                Name = "Arial",
                Size = Unit.FromPoint(fsize)
            };
        }

        public static Font CreateFont(float fsize, bool isBold)
        {
            return new Font
            {
                Name = "Arial",
                Size = Unit.FromPoint(fsize),
                Bold = isBold
            };
        }

        public static Font CreateFont(float fsize, bool isBold, Color color)
        {
            return new Font
            {
                Name = "Arial",
                Size = Unit.FromPoint(fsize),
                Bold = isBold,
                Color = ConvertColor(color)
            };
        }

        public static MigraDoc.DocumentObjectModel.Color ConvertColor(Color color)
        {
            var a = color.A;
            var r = color.R;
            var g = color.G;
            var b = color.B;
            return new MigraDoc.DocumentObjectModel.Color(a, r, g, b);
        }

        public static void FillRectangle(TextFrame frameContainer, Color color, float xPos, float yPos, float width, float height)
        {
            var frameRect = new TextFrame
            {
                FillFormat =
                {
                    Visible = true,
                    Color = ConvertColor(color)
                },
                Height = Unit.FromInch(height / 100),
                Width = Unit.FromInch(width / 100)
            };

            var frame = new TextFrame();

            frame.Elements.Add(frameRect);
            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.MarginLeft = Unit.FromInch(xPos / 100);
            frame.MarginTop = Unit.FromInch(yPos / 100);
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = frameContainer.Width;

            var bottom = Unit.FromInch((yPos + height) / 100);
            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
        }

        public static void DrawRectangle(TextFrame frameContainer, Color color, float xPos, float yPos, float width, float height)
        {
            var frameRect = new TextFrame
            {
                LineFormat =
                {
                    Color = ConvertColor(color)
                },
                Height = Unit.FromInch(height / 100),
                Width = Unit.FromInch(width / 100)
            };

            var frame = new TextFrame();
            frame.Elements.Add(frameRect);
            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.MarginLeft = Unit.FromInch(xPos / 100);
            frame.MarginTop = Unit.FromInch(yPos / 100);
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = frameContainer.Width;

            var bottom = Unit.FromInch((yPos + height) / 100);
            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
        }

        public static void DrawLine(TextFrame frameContainer, Color color, float x1, float y1, float x2, float y2)
        {
            var frameRect = new TextFrame();
            var frame = new TextFrame();

            frameRect.LineFormat.Color = ConvertColor(color);
            if (x1 == x2)
            {
                frameRect.Width = Unit.FromPoint(.01);
                frame.MarginLeft = Unit.FromInch(x1 / 100);
                if (y2 > y1)
                {
                    frameRect.Height = Unit.FromInch((y2 - y1) / 100);
                    frame.MarginTop = Unit.FromInch(y1 / 100);
                }
                else
                {
                    frameRect.Height = Unit.FromInch((y1 - y2) / 100);
                    frame.MarginTop = Unit.FromInch(y2 / 100);
                }
            }
            else if (y1 == y2)
            {
                frameRect.Height = Unit.FromPoint(.01);
                frame.MarginTop = Unit.FromInch(y1 / 100);
                if (x2 > x1)
                {
                    frameRect.Width = Unit.FromInch((x2 - x1) / 100);
                    frame.MarginLeft = Unit.FromInch(x1 / 100);
                }
                else
                {
                    frameRect.Width = Unit.FromInch((x1 - x2) / 100);
                    frame.MarginLeft = Unit.FromInch(x2 / 100);
                }
            }
            else
            {
                return;
            }

            frame.Elements.Add(frameRect);
            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = frameContainer.Width;

            var bottom = y1 > y2 ? Unit.FromInch(y1 / 100) : Unit.FromInch(y2 / 100);

            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
        }

        public static Table DrawTable(TextFrame frameContainer, float xPos, float yPos, float height)
        {
            var table = new Table();
            var frame = new TextFrame();

            frame.Elements.Add(table);
            frame.RelativeVertical = RelativeVertical.Page;
            frame.RelativeHorizontal = RelativeHorizontal.Page;
            frame.MarginLeft = Unit.FromInch(xPos / 100);
            frame.MarginTop = Unit.FromInch(yPos / 100);
            frame.Top = TopPosition.Parse("0 in");
            frame.Left = LeftPosition.Parse("0 in");
            frame.Width = frameContainer.Width;

            var bottom = Unit.FromInch((yPos + height) / 100);
            if (frameContainer.Height < bottom)
            {
                frameContainer.Height = bottom;
            }

            frame.Height = frameContainer.Height;
            frameContainer.Elements.Add(frame);
            return table;
        }

        ///<summary>Vertical spacer.</summary>
        public static void InsertSpacer(Section section, int space)
        {
            var frame = section.AddTextFrame();
            frame.Height = Unit.FromInch((double) space / 100);
        }

        public static void DrawGrid(Section section, GridOD grid)
        {
            var table = new Table();
            
            foreach (var t in grid.Columns)
            {
                var col = table.AddColumn(Unit.FromInch((double) t.ColWidth / 96));
                col.LeftPadding = Unit.FromInch(.01);
                col.RightPadding = Unit.FromInch(.01);
            }

            var row = table.AddRow();
            row.HeadingFormat = true;
            row.TopPadding = Unit.FromInch(0);
            row.BottomPadding = Unit.FromInch(-.01);

            Paragraph par;
            Cell cell;

            var fontHead = new Font("Arial", Unit.FromPoint(8.5))
            {
                Bold = true
            };

            var pdfd = new PdfDocument();
            var pg = pdfd.AddPage();
            var gx = XGraphics.FromPdfPage(pg); //A dummy graphics object for measuring the text
            for (var i = 0; i < grid.Columns.Count; i++)
            {
                cell = row.Cells[i + 1];
                par = cell.AddParagraph();
                par.AddFormattedText(grid.Columns[i].Heading, fontHead);
                par.Format.Alignment = ParagraphAlignment.Center;
                cell.Format.Alignment = ParagraphAlignment.Center;
                cell.Borders.Width = Unit.FromPoint(1);
                cell.Borders.Color = Colors.Black;
                cell.Shading.Color = Colors.LightGray;
            }

            Font fontBody = null; //=new MigraDoc.DocumentObjectModel.Font("Arial",Unit.FromPoint(8.5));
            var edgeRows = 1;
            for (var i = 0; i < grid.ListGridRows.Count; i++, edgeRows++)
            {
                row = table.AddRow();
                row.TopPadding = Unit.FromInch(.01);
                row.BottomPadding = Unit.FromInch(0);
                for (var j = 0; j < grid.Columns.Count; j++)
                {
                    cell = row.Cells[j + 1];
                    par = cell.AddParagraph();

                    var isBold = grid.ListGridRows[i].Cells[j].Bold switch
                    {
                        YN.Unknown => grid.ListGridRows[i].Bold,
                        YN.Yes => true,
                        _ => false
                    };

                    var color = grid.ListGridRows[i].Cells[j].ColorText == Color.Empty
                        ? grid.ListGridRows[i].ColorText
                        : grid.ListGridRows[i].Cells[j].ColorText;

                    fontBody = CreateFont(8.5f, isBold, color);
                    var xFont = isBold ? new XFont("Arial", 13.00) : new XFont("Arial", 11.65);
                    var colWidth = grid.Columns[j].ColWidth;
                    var cellText = grid.ListGridRows[i].Cells[j].Text;
                    var listWords = cellText.Split(new[] {" ", "\t", "\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    var isAnyWordWiderThanColumn = listWords.Any(x => gx.MeasureString(x, xFont).Width > colWidth);
                    if (!isAnyWordWiderThanColumn)
                    {
                        par.AddFormattedText(cellText, fontBody);
                    }
                    else
                    {
                        //Do our own line splitting and word splitting
                        DrawTextWithWordSplits(par, fontBody, colWidth, cellText, gx, xFont);
                    }

                    if (grid.Columns[j].TextAlign == HorizontalAlignment.Center)
                    {
                        cell.Format.Alignment = ParagraphAlignment.Center;
                    }

                    if (grid.Columns[j].TextAlign == HorizontalAlignment.Left)
                    {
                        cell.Format.Alignment = ParagraphAlignment.Left;
                    }

                    if (grid.Columns[j].TextAlign == HorizontalAlignment.Right)
                    {
                        cell.Format.Alignment = ParagraphAlignment.Right;
                    }

                    cell.Borders.Color = new MigraDoc.DocumentObjectModel.Color(180, 180, 180);
                    if (grid.ListGridRows[i].ColorLborder != Color.Empty)
                    {
                        cell.Borders.Bottom.Color = ConvertColor(grid.ListGridRows[i].ColorLborder);
                    }
                }

                if (string.IsNullOrEmpty(grid.ListGridRows[i].Note) || grid.NoteSpanStop <= 0 || grid.NoteSpanStart >= grid.Columns.Count)
                {
                    continue;
                }

                row = table.AddRow();
                row.TopPadding = Unit.FromInch(.01);
                row.BottomPadding = Unit.FromInch(0);
                cell = row.Cells[grid.NoteSpanStart + 1];
                par = cell.AddParagraph();
                par.AddFormattedText(grid.ListGridRows[i].Note, fontBody);
                cell.Format.Alignment = ParagraphAlignment.Left;
                cell.Borders.Color = new MigraDoc.DocumentObjectModel.Color(180, 180, 180);
                cell.MergeRight = grid.Columns.Count - 1 - grid.NoteSpanStart;
                edgeRows++;
            }

            table.SetEdge(1, 0, grid.Columns.Count, edgeRows, Edge.Box, BorderStyle.Single, 1, Colors.Black);
            section.Add(table);
        }

        private static void DrawTextWithWordSplits(Paragraph par, Font fontBody, int colWidth, string cellText, XGraphics gx, XFont xFont)
        {
            var line = "";
            var word = "";

            for (var c = 0; c < cellText.Length; c++)
            {
                var letter = cellText[c];
                word += letter;
                if (c == cellText.Length - 1 || (!char.IsWhiteSpace(letter) && char.IsWhiteSpace(cellText[c + 1])))
                {
                    if ((line + word).All(char.IsWhiteSpace))
                    {
                        par.AddFormattedText(line + word, fontBody);
                        line = "";
                        word = "";
                        continue;
                    }

                    if (DoesTextFit(line + word, colWidth, gx, xFont))
                    {
                        line += word;
                        if (IsLastWord(c, cellText))
                        {
                            par.AddFormattedText(line, fontBody);
                        }
                    }
                    else
                    {
                        if (line == "")
                        {
                            DrawWordChunkByChunk(word, colWidth, par, fontBody, gx, xFont);
                        }
                        else
                        {
                            par.AddFormattedText(line, fontBody);
                            if (DoesTextFit(word, colWidth, gx, xFont))
                            {
                                if (IsLastWord(c, cellText))
                                {
                                    par.AddFormattedText(word, fontBody);
                                }

                                line = word;
                            }
                            else
                            {
                                //The word by itself does not fit.
                                DrawWordChunkByChunk(word, colWidth, par, fontBody, gx, xFont);
                                line = "";
                            }
                        }
                    }

                    word = "";
                }
            }
        }

        private static void DrawWordChunkByChunk(string word, int colWidth, Paragraph par, Font fontBody, XGraphics gx, XFont xFont)
        {
            var chunk = "";
            for (var i = 0; i < word.Length; i++)
            {
                var letter = word[i];
                if ((chunk + letter).All(char.IsWhiteSpace))
                {
                    par.AddFormattedText(chunk + letter, fontBody);
                    continue;
                }

                if (DoesTextFit(chunk + letter, colWidth, gx, xFont))
                {
                    if (i == word.Length - 1)
                    {
                        par.AddFormattedText(chunk + letter, fontBody);
                        return;
                    }

                    chunk += letter;
                    continue;
                }

                par.AddFormattedText(chunk, fontBody);
                chunk = "" + letter;
            }
        }

        private static bool IsLastWord(int index, string cellText)
        {
            for (var i = index + 1; i < cellText.Length; i++)
            {
                if (!char.IsWhiteSpace(cellText[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DoesTextFit(string text, int colWidth, XGraphics gx, XFont xFont)
        {
            return gx.MeasureString(text, xFont).Width < colWidth;
        }
    }
}