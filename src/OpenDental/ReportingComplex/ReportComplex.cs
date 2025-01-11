using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.ReportingComplex
{
    public class ReportComplex
    {
        private bool _hasGridLines;
        private readonly Graphics _grfx;

        public Margins PrintMargins = new Margins(30, 0, 50, 50);

        public SectionCollection Sections { get; } = new SectionCollection();
        public ReportObjectCollection ReportObjects { get; set; } = new ReportObjectCollection();
        public bool IsLandscape { get; set; }
        public string ReportName { get; set; }
        public int TotalRows { get; set; }

        public ReportComplex(bool hasGridLines, bool isLandscape)
        {
            _grfx = Graphics.FromImage(new Bitmap(1, 1));
            IsLandscape = isLandscape;
            if (hasGridLines)
            {
                AddGridLines();
            }

            if (Sections[AreaSectionType.ReportHeader] == null)
            {
                Sections.Add(new Section(AreaSectionType.ReportHeader, 0));
            }

            if (Sections[AreaSectionType.PageHeader] == null)
            {
                Sections.Add(new Section(AreaSectionType.PageHeader, 0));
            }

            if (Sections[AreaSectionType.PageFooter] == null)
            {
                Sections.Add(new Section(AreaSectionType.PageFooter, 0));
            }

            if (Sections[AreaSectionType.ReportFooter] == null)
            {
                Sections.Add(new Section(AreaSectionType.ReportFooter, 0));
            }
        }

        public void AddTitle(string name, string title)
        {
            AddTitle(name, title, new Font("Tahoma", 17, FontStyle.Bold));
        }

        public void AddTitle(string name, string title, Font font)
        {
            ODEvent.Fire(ODEventType.ReportComplex, "Adding Title To Report...");

            var sizeF = _grfx.MeasureString(title, font);
            var size = new Size((int) (sizeF.Width / _grfx.DpiX * 100 + 2), (int) (sizeF.Height / _grfx.DpiY * 100));
            int xPos;
            if (IsLandscape)
            {
                xPos = 1100 / 2;
                xPos -= 50;
            }
            else
            {
                xPos = 850 / 2;
                xPos -= 30;
            }

            xPos -= size.Width / 2;
            if (Sections[AreaSectionType.ReportHeader] == null)
            {
                Sections.Add(new Section(AreaSectionType.ReportHeader, 0));
            }

            ReportObjects.Add(new ReportObject(name, AreaSectionType.ReportHeader, new Point(xPos, 0), size, title, font, ContentAlignment.MiddleCenter));

            Sections[AreaSectionType.ReportHeader].Height = size.Height + 10;
        }

        public void AddSubTitle(string name, string subTitle)
        {
            AddSubTitle(name, subTitle, new Font("Tahoma", 10, FontStyle.Bold));
        }

        public void AddSubTitle(string name, string subTitle, Font font)
        {
            AddSubTitle(name, subTitle, font, 0);
        }

        public void AddSubTitle(string name, string subTitle, Font font, int padding)
        {
            ODEvent.Fire(ODEventType.ReportComplex, "Adding SubTitle To Report...");

            var sizeF = _grfx.MeasureString(subTitle, font);
            var size = new Size((int) (sizeF.Width / _grfx.DpiX * 100 + 2), (int) (sizeF.Height / _grfx.DpiY * 100));
            int xPos;
            if (IsLandscape)
            {
                xPos = 1100 / 2;
                xPos -= 50;
            }
            else
            {
                xPos = 850 / 2;
                xPos -= 30;
            }

            xPos -= size.Width / 2;
            if (Sections[AreaSectionType.ReportHeader] == null)
            {
                Sections.Add(new Section(AreaSectionType.ReportHeader, 0));
            }

            var yPos = ReportObjects
                .OfType<ReportObject>()
                .Where(x => x.SectionType == AreaSectionType.ReportHeader)
                .Select(x => x.Location.Y + x.Size.Height)
                .Where(x => x > 0)
                .DefaultIfEmpty(0)
                .Max();

            ReportObjects.Add(new ReportObject(name, AreaSectionType.ReportHeader, new Point(xPos, yPos + padding), size, subTitle, font, ContentAlignment.MiddleCenter));
            Sections[AreaSectionType.ReportHeader].Height += size.Height + padding;
        }

        public void AddFooterText(string name, string text, Font font, int padding, ContentAlignment contentAlign)
        {
            const int borderPadding = 50;

            var size = IsLandscape
                ? new Size(1100 - borderPadding * 2, (int) (_grfx.MeasureString(text, font).Height / _grfx.DpiY * 100 + 2))
                : new Size(850 - borderPadding * 2, (int) (_grfx.MeasureString(text, font).Height / _grfx.DpiY * 100 + 2));

            if (Sections[AreaSectionType.ReportFooter] == null)
            {
                Sections.Add(new Section(AreaSectionType.ReportFooter, 0));
            }

            var yPos = 0;
            foreach (ReportObject reportObject in ReportObjects)
            {
                if (reportObject.SectionType != AreaSectionType.ReportFooter)
                {
                    continue;
                }

                if (reportObject.Location.Y + reportObject.Size.Height > yPos)
                {
                    yPos = reportObject.Location.Y + reportObject.Size.Height;
                }
            }

            ReportObjects.Add(new ReportObject(name, AreaSectionType.ReportFooter, new Point(borderPadding, yPos + padding), size, text, font, contentAlign));
            Sections[AreaSectionType.ReportFooter].Height += size.Height + padding;
        }

        public void AddPageFooterText(string name, string text, Font font, int padding, ContentAlignment contentAlign)
        {
            const int borderPadding = 50;

            var size = IsLandscape
                ? new Size(1100 - borderPadding * 2, (int) (_grfx.MeasureString(text, font).Height / _grfx.DpiY * 100 + 2))
                : new Size(850 - borderPadding * 2, (int) (_grfx.MeasureString(text, font).Height / _grfx.DpiY * 100 + 2));

            if (Sections[AreaSectionType.PageFooter] == null)
            {
                Sections.Add(new Section(AreaSectionType.PageFooter, 0));
            }

            var yPos = 0;
            foreach (ReportObject reportObject in ReportObjects)
            {
                if (reportObject.SectionType != AreaSectionType.PageFooter)
                {
                    continue;
                }

                if (reportObject.Location.Y + reportObject.Size.Height > yPos)
                {
                    yPos = reportObject.Location.Y + reportObject.Size.Height;
                }
            }

            ReportObjects.Add(new ReportObject(name, AreaSectionType.PageFooter, new Point(borderPadding, yPos + padding), size, text, font, contentAlign));
            Sections[AreaSectionType.PageFooter].Height += size.Height + padding;
        }

        public QueryObject AddQuery(DataTable query, string title)
        {
            var queryObj = new QueryObject(title, query);
            ReportObjects.Add(queryObj);
            return queryObj;
        }

        public QueryObject AddQuery(DataTable query, string title, string columnNameToSplitOn, SplitByKind splitByKind, int queryGroup)
        {
            var queryObj = new QueryObject(title, query, queryGroupValue: queryGroup, columnNameToSplitOn: columnNameToSplitOn, splitByKind: splitByKind);
            ReportObjects.Add(queryObj);
            return queryObj;
        }

        public QueryObject AddQuery(DataTable table, string title, string columnNameToSplitOn, SplitByKind splitByKind, int queryGroup, bool isCentered)
        {
            var queryObj = new QueryObject(title, table, isCentered: isCentered, queryGroupValue: queryGroup, columnNameToSplitOn: columnNameToSplitOn, splitByKind: splitByKind);
            ReportObjects.Add(queryObj);
            return queryObj;
        }

        public QueryObject AddQuery(DataTable query, string title, string columnNameToSplitOn, SplitByKind splitByKind, int queryGroup, bool isCentered, Dictionary<long, string> dictDefNames, Font font)
        {
            var queryObj = new QueryObject(title, query, font: font, isCentered: isCentered, queryGroupValue: queryGroup, columnNameToSplitOn: columnNameToSplitOn, splitByKind: splitByKind, listEnumNames: null, dictDefNames: dictDefNames);
            ReportObjects.Add(queryObj);
            return queryObj;
        }

        public ReportObject GetTitle(string name)
        {
            for (var i = ReportObjects.Count - 1; i >= 0; i--)
            {
                if (ReportObjects[i].Name == name)
                {
                    return ReportObjects[i];
                }
            }

            ODMessageBox.Show("end of loop");
            return null;
        }

        public void AddPageNum()
        {
            AddPageNum(new Font("Tahoma", 9));
        }

        public void AddPageNum(Font font)
        {
            var size = new Size(150, (int) (_grfx.MeasureString("anytext", font).Height / _grfx.DpiY * 100 + 2));
            if (Sections[AreaSectionType.PageFooter] == null)
            {
                Sections.Add(new Section(AreaSectionType.PageFooter, 0));
            }

            if (Sections[AreaSectionType.PageFooter].Height == 0)
            {
                Sections[AreaSectionType.PageFooter].Height = size.Height;
            }

            ReportObjects.Add(new ReportObject("PageNum", AreaSectionType.PageFooter, new Point(0, 0), size, FieldValueType.String, SpecialFieldType.PageNumber, font, ContentAlignment.MiddleLeft, ""));
        }

        public void AddGridLines()
        {
            _hasGridLines = true;
        }

        public bool SubmitQueries(bool isShowMessage = false, bool hasPreferenceTable = true)
        {
            var hasRows = false;
            var hasReportServer = false;
            if (hasPreferenceTable)
            {
                hasReportServer = !string.IsNullOrEmpty(PrefC.ReportingServer.DisplayStr);
            }

            var grfx = Graphics.FromImage(new Bitmap(1, 1));
            var newReportObjects = new ReportObjectCollection();
            Sections.Add(new Section(AreaSectionType.Query, 0));
            for (var i = 0; i < ReportObjects.Count; i++)
            {
                if (ReportObjects[i].ObjectType == ReportObjectType.QueryObject)
                {
                    var query = (QueryObject) ReportObjects[i];
                    var wasSubmitted = false;
                    var progressOD = new ProgressWin();
                    if (query.IsQueryStringNullOrEmpty)
                    {
                        wasSubmitted = query.SubmitQuery();
                    }
                    else
                    {
                        progressOD.ActionMain = () => { wasSubmitted = query.SubmitQuery(); };
                        progressOD.ShowDialog();
                    }

                    if (!wasSubmitted)
                    {
                        MsgBox.Show(this, "There was an error generating this report."
                                          + (hasReportServer ? "\r\nVerify or remove the report server connection settings and try again." : ""));
                        return false;
                    }

                    if (!query.IsQueryStringNullOrEmpty && progressOD.IsCancelled)
                    {
                        return false;
                    }

                    if (query.ReportTable.Rows.Count == 0)
                    {
                        continue;
                    }

                    hasRows = true;
                    TotalRows += query.ReportTable.Rows.Count;
                    if (!string.IsNullOrWhiteSpace(query.ColumnNameToSplitOn))
                    {
                        ODEvent.Fire(ODEventType.ReportComplex, Lan.g("ReportComplex", "Creating Splits Based On") + " " + query.ColumnNameToSplitOn + "...");
                        var lastCellValue = "";
                        query.IsLastSplit = false;
                        QueryObject newQuery = null;
                        string displayText;
                        for (var j = 0; j < query.ReportTable.Rows.Count; j++)
                        {
                            if (query.ReportTable.Rows[j][query.ColumnNameToSplitOn].ToString() == lastCellValue)
                            {
                                if (newQuery == null)
                                {
                                    newQuery = query.DeepCopyQueryObject();
                                    newQuery.AddInitialHeader(newQuery.GetGroupTitle().StaticText, newQuery.GetGroupTitle().Font);
                                }

                                newQuery.ReportTable.ImportRow(query.ReportTable.Rows[j]);
                            }
                            else
                            {
                                if (newQuery != null)
                                {
                                    switch (newQuery.SplitByKind)
                                    {
                                        case SplitByKind.None:
                                            return false;

                                        case SplitByKind.Enum:
                                            if (newQuery.ListEnumNames == null)
                                            {
                                                return false;
                                            }

                                            displayText = newQuery.ListEnumNames[SIn.Int(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString())];
                                            newQuery.GetGroupTitle().Size = new Size((int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Height / grfx.DpiY * 100 + 2));
                                            newQuery.GetGroupTitle().StaticText = displayText;
                                            break;

                                        case SplitByKind.Definition:
                                            if (newQuery.DictDefNames == null)
                                            {
                                                return false;
                                            }

                                            if (newQuery.DictDefNames.ContainsKey(SIn.Long(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString())))
                                            {
                                                displayText = newQuery.DictDefNames[SIn.Long(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString())];
                                                newQuery.GetGroupTitle().Size = new Size((int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Height / grfx.DpiY * 100 + 2));
                                                newQuery.GetGroupTitle().StaticText = displayText;
                                            }
                                            else
                                            {
                                                newQuery.GetGroupTitle().StaticText = "Undefined";
                                            }

                                            break;

                                        case SplitByKind.Date:
                                            displayText = SIn.Date(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString()).ToShortDateString();
                                            newQuery.GetGroupTitle().StaticText = displayText;
                                            newQuery.GetGroupTitle().Size = new Size((int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Height / grfx.DpiY * 100 + 2));
                                            break;

                                        case SplitByKind.Value:
                                            displayText = newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString();
                                            newQuery.GetGroupTitle().StaticText = displayText;
                                            newQuery.GetGroupTitle().Size = new Size((int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Height / grfx.DpiY * 100 + 2));
                                            break;
                                    }

                                    newQuery.SubmitQuery();
                                    newReportObjects.Add(newQuery);
                                }

                                if (newQuery == null && query.GetGroupTitle().StaticText != "")
                                {
                                    newQuery = query.DeepCopyQueryObject();
                                    newQuery.ReportTable.ImportRow(query.ReportTable.Rows[j]);
                                    newQuery.AddInitialHeader(newQuery.GetGroupTitle().StaticText, newQuery.GetGroupTitle().Font);
                                }
                                else
                                {
                                    newQuery = query.DeepCopyQueryObject();
                                    newQuery.ReportTable.ImportRow(query.ReportTable.Rows[j]);
                                }
                            }

                            lastCellValue = query.ReportTable.Rows[j][query.ColumnNameToSplitOn].ToString();
                        }

                        switch (newQuery.SplitByKind)
                        {
                            case SplitByKind.None:
                                return false;

                            case SplitByKind.Enum:
                                if (newQuery.ListEnumNames == null)
                                {
                                    return false;
                                }

                                displayText = newQuery.ListEnumNames[SIn.Int(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString())];
                                newQuery.GetGroupTitle().Size = new Size((int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Height / grfx.DpiY * 100 + 2));
                                newQuery.GetGroupTitle().StaticText = displayText;
                                break;

                            case SplitByKind.Definition:
                                if (newQuery.DictDefNames == null)
                                {
                                    return false;
                                }

                                if (newQuery.DictDefNames.ContainsKey(SIn.Long(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString())))
                                {
                                    displayText = newQuery.DictDefNames[SIn.Long(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString())];
                                    newQuery.GetGroupTitle().Size = new Size((int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(displayText, newQuery.GetGroupTitle().Font).Height / grfx.DpiY * 100 + 2));
                                    newQuery.GetGroupTitle().StaticText = displayText;
                                }
                                else
                                {
                                    newQuery.GetGroupTitle().StaticText = Lans.g(this, "Undefined");
                                }

                                break;

                            case SplitByKind.Date:
                                newQuery.GetGroupTitle().StaticText = SIn.Date(newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString()).ToShortDateString();
                                break;

                            case SplitByKind.Value:
                                newQuery.GetGroupTitle().StaticText = newQuery.ReportTable.Rows[0][query.ColumnNameToSplitOn].ToString();
                                break;
                        }

                        newQuery.SubmitQuery();
                        newQuery.IsLastSplit = true;
                        newReportObjects.Add(newQuery);
                    }
                    else
                    {
                        newReportObjects.Add(ReportObjects[i]);
                    }
                }
                else
                {
                    newReportObjects.Add(ReportObjects[i]);
                }
            }

            if (!hasRows && isShowMessage)
            {
                MsgBox.Show(this, "The report has no results to show.");
                return false;
            }

            ReportObjects = newReportObjects;
            return true;
        }

        public int GetSectionHeight(AreaSectionType sectionType)
        {
            return !Sections.Contains(sectionType) ? 0 : Sections[sectionType].Height;
        }

        public bool HasGridLines()
        {
            return _hasGridLines;
        }
    }
}