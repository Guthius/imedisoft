using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness;

namespace OpenDental.ReportingComplex;

public class QueryObject : ReportObject
{
    private readonly string _stringQuery;

    public bool IsPrinted;

    public QueryObject()
    {
    }

    public QueryObject(string title, DataTable tableQuery = null, string stringQuery = "", Font font = null, bool isCentered = true, int queryGroupValue = 0, string columnNameToSplitOn = "", SplitByKind splitByKind = SplitByKind.None, List<string> listEnumNames = null, Dictionary<long, string> dictDefNames = null)
    {
        ODEvent.Fire(ODEventType.ReportComplex, tableQuery == null ? "Adding Query To Report..." : "Adding Table To Report...");

        var grfx = Graphics.FromImage(new Bitmap(1, 1));
        font ??= new Font("Tahoma", 9);

        ColumnNameToSplitOn = columnNameToSplitOn;
        ReportTable = tableQuery;
        _stringQuery = stringQuery;
        SectionType = AreaSectionType.Query;
        Name = "Query";
        SplitByKind = splitByKind;
        ListEnumNames = listEnumNames;
        DictDefNames = dictDefNames;
        QueryGroupValue = queryGroupValue;
        IsCentered = isCentered;
        ObjectType = ReportObjectType.QueryObject;
        Sections.Add(new Section(AreaSectionType.GroupTitle, 0));
        if (tableQuery == null)
        {
            ReportObjects.Add(new ReportObject("Group Title", AreaSectionType.GroupTitle, new Point(0, 0), new Size((int) (grfx.MeasureString(title, font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(title, font).Height / grfx.DpiY * 100 + 2)), title, font, ContentAlignment.MiddleLeft, 0, 0));
        }
        else
        {
            ReportObjects.Add(new ReportObject("Group Title", AreaSectionType.GroupTitle, new Point(0, 0), new Size((int) (grfx.MeasureString(title, font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(title, font).Height / grfx.DpiY * 100 + 2)), title, font, ContentAlignment.MiddleLeft));
        }

        ReportObjects["Group Title"].IsUnderlined = true;
        Sections.Add(new Section(AreaSectionType.GroupHeader, 0));
        Sections.Add(new Section(AreaSectionType.Detail, 0));
        Sections.Add(new Section(AreaSectionType.GroupFooter, 0));
        QueryWidth = 0;
        SuppressHeaders = true;
        IsLastSplit = true;
        ExportTable = new DataTable();
        grfx.Dispose();
    }

    public SectionCollection Sections { get; private set; } = new();

    public ArrayList ArrDataFields { get; private set; } = new();

    public ReportObjectCollection ReportObjects { get; private set; } = new();

    public string ColumnNameToSplitOn { get; private set; }

    public DataTable ReportTable { get; set; }

    public DataTable ExportTable { get; set; }

    public List<string> ListEnumNames { get; private set; }

    public Dictionary<long, string> DictDefNames { get; private set; }

    public SplitByKind SplitByKind { get; private set; }

    public List<int> RowHeightValues { get; set; }

    public int QueryGroupValue { get; set; }

    public bool IsCentered { get; set; }

    public int QueryWidth { get; set; }

    public bool SuppressHeaders { get; set; }

    public bool IsLastSplit { get; set; }

    public bool IsNegativeSummary { get; set; }

    public bool IsQueryStringNullOrEmpty => string.IsNullOrEmpty(_stringQuery);

    public void AddColumn(string dataField, int width, FieldValueType fieldValueType = FieldValueType.String, Font font = null, string formatString = "")
    {
        ODEvent.Fire(ODEventType.ReportComplex, "Adding Column To Table...");

        var grfx = Graphics.FromImage(new Bitmap(1, 1));
        
        ArrDataFields.Add(dataField);
        
        font ??= new Font("Tahoma", 9);

        var fontHeader = new Font(font.FontFamily, font.Size - 1, FontStyle.Bold);
        var fontFooter = new Font(font.FontFamily, font.Size, FontStyle.Bold);
        var textAlign = fieldValueType == FieldValueType.Number ? ContentAlignment.TopRight : ContentAlignment.TopLeft;

        if (formatString == "")
        {
            if (fieldValueType == FieldValueType.Number)
            {
                formatString = "n";
            }

            if (fieldValueType == FieldValueType.Date)
            {
                formatString = "d";
            }
        }

        QueryWidth += width;
        
        var sizeHeader = new Size((int) grfx.MeasureString(dataField, fontHeader, (int) (width / grfx.DpiX * 100 + 2)).Width, (int) grfx.MeasureString(dataField, fontHeader, (int) (width / grfx.DpiY * 100 + 2)).Height);
        var sizeDetail = new Size((int) grfx.MeasureString(dataField, font, (int) (width / grfx.DpiX * 100 + 2)).Width, (int) grfx.MeasureString(dataField, font, (int) (width / grfx.DpiY * 100 + 2)).Height);
        var sizeFooter = new Size((int) grfx.MeasureString(dataField, fontFooter, (int) (width / grfx.DpiX * 100 + 2)).Width, (int) grfx.MeasureString(dataField, fontFooter, (int) (width / grfx.DpiY * 100 + 2)).Height);
        var xPos = 0;
        //find next available xPos
        foreach (ReportObject reportObject in ReportObjects)
        {
            if (reportObject.SectionType != AreaSectionType.GroupHeader)
            {
                continue;
            }

            if (reportObject.Location.X + reportObject.Size.Width > xPos)
            {
                xPos = reportObject.Location.X + reportObject.Size.Width;
            }
        }

        ReportObjects.Add(new ReportObject(dataField + "Header", AreaSectionType.GroupHeader, new Point(xPos, 0), sizeHeader with {Width = width}, dataField, fontHeader, textAlign));
        ReportObjects.Add(new ReportObject(dataField + "Detail", AreaSectionType.Detail, new Point(xPos, 0), sizeDetail with {Width = width}, dataField, fieldValueType, font, textAlign, formatString));

        if (fieldValueType == FieldValueType.Number)
        {
            ReportObjects.Add(new ReportObject(dataField + "Footer", AreaSectionType.GroupFooter, new Point(xPos, 0), sizeFooter with {Width = width}, SummaryOperation.Sum, dataField, fontFooter, textAlign, formatString));
        }

        ExportTable.Columns.Add(dataField);
        grfx.Dispose();
    }

    public void AddGroupSummaryField(string staticText, string columnName, string dataFieldName, SummaryOperation summaryOperation, List<int> queryGroupValues = null, Color color = default(Color), Font font = null, int offSetX = 0, int offSetY = 0, string formatString = "")
    {
        ODEvent.Fire(ODEventType.ReportComplex, "Adding Group Summary To Tables...");

        var grfx = Graphics.FromImage(new Bitmap(1, 1));

        queryGroupValues ??= [0];

        if (color == default)
        {
            color = Color.Black;
        }

        font ??= new Font("Tahoma", 8, FontStyle.Bold);

        var location = GetObjectByName(columnName + "Header").Location;
        var labelSize = new Size((int) (grfx.MeasureString(staticText, font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(staticText, font).Height / grfx.DpiY * 100 + 2));
        var i = ReportObjects.Add(new ReportObject(columnName + "GroupSummaryLabel", AreaSectionType.GroupFooter, new Point(location.X - labelSize.Width, 0), labelSize, staticText, font, ContentAlignment.MiddleRight, offSetX, offSetY));
        ReportObjects[i].DataField = dataFieldName;
        ReportObjects[i].SummaryGroups = queryGroupValues;
        Sections[AreaSectionType.GroupFooter].Height += (int) (grfx.MeasureString(staticText, font).Height / grfx.DpiY * 100 + 2) + offSetY;
        i = ReportObjects.Add(new ReportObject(columnName + "GroupSummaryText", AreaSectionType.GroupFooter, location, new Size(0, 0), color, summaryOperation, columnName, font, ContentAlignment.MiddleLeft, dataFieldName, offSetX, offSetY, formatString));
        ReportObjects[i].SummaryGroups = queryGroupValues;
        grfx.Dispose();
    }

    public void AddInitialHeader(string title, Font font)
    {
        ODEvent.Fire(ODEventType.ReportComplex, "Adding Initial Header To Table...");
        var grfx = Graphics.FromImage(new Bitmap(1, 1));
        var newFont = new Font(font.FontFamily, font.Size + 2, font.Style);
        ReportObjects.Insert(0, new ReportObject("Initial Group Title", AreaSectionType.GroupTitle, new Point(0, 0), new Size((int) (grfx.MeasureString(title, newFont).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(title, newFont).Height / grfx.DpiY * 100 + 2)), title, newFont, ContentAlignment.MiddleLeft));
        ReportObjects["Initial Group Title"].IsUnderlined = true;
        grfx.Dispose();
    }

    public void AddLine(string name, AreaSectionType sectionType, LineOrientation lineOrientation, LinePosition linePosition, Color color, float floatLineThickness, int linePercentValue, int offSetX, int offSetY)
    {
        ODEvent.Fire(ODEventType.ReportComplex, "Adding Line To Table...");
        ReportObjects.Add(new ReportObject(name, sectionType, color, floatLineThickness, lineOrientation, linePosition, linePercentValue, offSetX, offSetY));
    }

    public void AddSummaryLabel(string dataFieldName, string summaryText, SummaryOrientation summaryOrientation, bool hasWordWrap, Font font)
    {
        ODEvent.Fire(ODEventType.ReportComplex, "Adding Summary To Table...");
        var grfx = Graphics.FromImage(new Bitmap(1, 1));
        var summaryField = GetObjectByName(dataFieldName + "Footer");
        Size size;
        if (hasWordWrap)
        {
            size = summaryField.Size with {Height = (int) (grfx.MeasureString(summaryText, font, summaryField.Size.Width).Height / grfx.DpiY * 100 + 2)};
        }
        else
        {
            size = new Size((int) (grfx.MeasureString(summaryText, font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(summaryText, font).Height / grfx.DpiY * 100 + 2));
        }

        if (summaryOrientation == SummaryOrientation.North)
        {
            var summaryLabel = new ReportObject(dataFieldName + "Label", AreaSectionType.GroupFooter, summaryField.Location, size, summaryText, font, summaryField.ContentAlignment);
            summaryLabel.DataField = dataFieldName;
            summaryLabel.SummaryOrientation = summaryOrientation;
            ReportObjects.Insert(ReportObjects.IndexOf(summaryField), summaryLabel);
        }
        else if (summaryOrientation == SummaryOrientation.South)
        {
            var summaryLabel = new ReportObject(dataFieldName + "Label", AreaSectionType.GroupFooter, summaryField.Location, size, summaryText, font, summaryField.ContentAlignment);
            summaryLabel.DataField = dataFieldName;
            summaryLabel.SummaryOrientation = summaryOrientation;
            ReportObjects.Add(summaryLabel);
        }
        else if (summaryOrientation == SummaryOrientation.West)
        {
            var summaryLabel = new ReportObject(dataFieldName + "Label", AreaSectionType.GroupFooter
                , new Point(summaryField.Location.X - size.Width)
                , size
                , summaryText
                , font
                , summaryField.ContentAlignment);
            summaryLabel.DataField = dataFieldName;
            summaryLabel.SummaryOrientation = summaryOrientation;
            ReportObjects.Insert(ReportObjects.IndexOf(summaryField), summaryLabel);
        }
        else
        {
            var summaryLabel = new ReportObject(dataFieldName + "Label", AreaSectionType.GroupFooter
                , new Point(summaryField.Location.X + size.Width + summaryField.Size.Width)
                , size
                , summaryText
                , font
                , summaryField.ContentAlignment);
            summaryLabel.DataField = dataFieldName;
            summaryLabel.SummaryOrientation = summaryOrientation;
            ReportObjects.Insert(ReportObjects.IndexOf(summaryField) + 1, summaryLabel);
        }

        grfx.Dispose();
    }

    public void CalculateRowHeights(bool isWrapping)
    {
        ODEvent.Fire(ODEventType.ReportComplex, "Creating Query In Report...");
            
        var g = Graphics.FromImage(new Bitmap(1, 1));
            
        RowHeightValues = [];
            
        for (var i = 0; i < ReportTable.Rows.Count; i++)
        {
            var prevDisplayText = "";
            var rowHeight = 0;
                
            foreach (ReportObject reportObject in ReportObjects)
            {
                if (reportObject.SectionType != AreaSectionType.Detail)
                {
                    continue;
                }

                if (reportObject.ObjectType != ReportObjectType.FieldObject)
                {
                    continue;
                }
                    
                var rawText = ReportTable.Rows[i][ArrDataFields.IndexOf(reportObject.DataField)].ToString();
                if (string.IsNullOrWhiteSpace(rawText))
                {
                    continue;
                }

                var listString = GetDisplayString(rawText, prevDisplayText, reportObject, i);
                var displayText = listString[0];
                prevDisplayText = listString[1];
                var curCellHeight = 0;
                if (isWrapping)
                {
                    curCellHeight = (int) (g.MeasureString(displayText, reportObject.Font, (int) reportObject.Size.Width,
                        GetStringFormatAlignment(reportObject.ContentAlignment)).Height * (100f / 96f)); //due to pixel factor
                }
                else
                {
                    curCellHeight = (int) (g.MeasureString(displayText, reportObject.Font, 0,
                        GetStringFormatAlignment(reportObject.ContentAlignment)).Height * (100f / 96f)); //due to pixel factor
                }

                if (curCellHeight > rowHeight)
                {
                    rowHeight = curCellHeight;
                }
            }

            RowHeightValues.Add(rowHeight);
        }

        g.Dispose();
    }

    public QueryObject DeepCopyQueryObject()
    {
        var queryObj = new QueryObject
        {
            Name = Name,
            SectionType = SectionType,
            ObjectType = ObjectType,
            Sections = Sections,
            ArrDataFields = ArrDataFields,
            QueryGroupValue = QueryGroupValue,
            IsCentered = IsCentered,
            QueryWidth = QueryWidth,
            SuppressHeaders = SuppressHeaders,
            ColumnNameToSplitOn = ColumnNameToSplitOn,
            SplitByKind = SplitByKind,
            IsPrinted = IsPrinted,
            SummaryOrientation = SummaryOrientation,
            SummaryGroups = SummaryGroups,
            IsLastSplit = IsLastSplit,
            RowHeightValues = [],
            IsNegativeSummary = IsNegativeSummary
        };
            
        for (var i = 0; i < RowHeightValues.Count; i++)
        {
            queryObj.RowHeightValues.Add(RowHeightValues[i]);
        }

        var reportObjectsNew = new ReportObjectCollection();
        for (var i = 0; i < ReportObjects.Count; i++)
        {
            reportObjectsNew.Add(ReportObjects[i].DeepCopyReportObject());
        }

        queryObj.ReportObjects = reportObjectsNew;
        queryObj.ReportTable = new DataTable();

        for (var i = 0; i < ReportTable.Columns.Count; i++)
        {
            queryObj.ReportTable.Columns.Add(new DataColumn(ReportTable.Columns[i].ColumnName));
        }

        queryObj.ExportTable = new DataTable();

        for (var i = 0; i < ExportTable.Columns.Count; i++)
        {
            queryObj.ExportTable.Columns.Add(new DataColumn(ExportTable.Columns[i].ColumnName));
        }

        var enumNamesNew = new List<string>();
        if (ListEnumNames != null)
        {
            foreach (var item in ListEnumNames)
            {
                enumNamesNew.Add(item);
            }
        }

        queryObj.ListEnumNames = enumNamesNew;
        var defNamesNew = new Dictionary<long, string>();
        if (DictDefNames != null)
        {
            foreach (var defNum in DictDefNames.Keys)
            {
                defNamesNew.Add(defNum, DictDefNames[defNum]);
            }
        }

        queryObj.DictDefNames = defNamesNew;
        return queryObj;
    }

    public ReportObject GetColumnDetail(string columnName)
    {
        return ReportObjects[columnName + "Detail"];
    }

    public ReportObject GetColumnHeader(string columnName)
    {
        return ReportObjects[columnName + "Header"];
    }

    private List<string> GetDisplayString(string rawText, string prevDisplayText, ReportObject reportObject, int i)
    {
        var displayText = "";
        var retVals = new List<string>();

        switch (reportObject.FieldValueType)
        {
            case FieldValueType.Age:
                displayText = Patients.AgeToString(Patients.DateToAge(SIn.Date(rawText))); //(fieldObject.FormatString);
                break;

            case FieldValueType.Boolean:
            {
                displayText = SIn.Bool(ReportTable.Rows[i][ArrDataFields.IndexOf(reportObject.DataField)].ToString()).ToString(); //(fieldObject.FormatString);
                if (i > 0 && reportObject.SuppressIfDuplicate)
                {
                    prevDisplayText = SIn.Bool(ReportTable.Rows[i - 1][ArrDataFields.IndexOf(reportObject.DataField)].ToString()).ToString();
                }

                break;
            }

            case FieldValueType.Date:
            {
                var rowDateTime = SIn.DateTime(ReportTable.Rows[i][ArrDataFields.IndexOf(reportObject.DataField)].ToString());
                if (rowDateTime.Year > 1880)
                {
                    displayText = rowDateTime.ToString(reportObject.StringFormat);
                }

                if (i > 0 && reportObject.SuppressIfDuplicate)
                {
                    rowDateTime = SIn.DateTime(ReportTable.Rows[i - 1][ArrDataFields.IndexOf(reportObject.DataField)].ToString());
                    prevDisplayText = "";
                    if (rowDateTime.Year > 1880)
                    {
                        prevDisplayText = rowDateTime.ToString(reportObject.StringFormat);
                    }
                }

                break;
            }

            case FieldValueType.Integer:
            {
                displayText = SIn.Long(ReportTable.Rows[i][ArrDataFields.IndexOf(reportObject.DataField)].ToString()).ToString(reportObject.StringFormat);
                if (i > 0 && reportObject.SuppressIfDuplicate)
                {
                    prevDisplayText = SIn.Long(ReportTable.Rows[i - 1][ArrDataFields.IndexOf(reportObject.DataField)].ToString()).ToString(reportObject.StringFormat);
                }

                break;
            }

            case FieldValueType.Number:
            {
                displayText = SIn.Double(ReportTable.Rows[i][ArrDataFields.IndexOf(reportObject.DataField)].ToString()).ToString(reportObject.StringFormat);
                if (i > 0 && reportObject.SuppressIfDuplicate)
                {
                    prevDisplayText = SIn.Double(ReportTable.Rows[i - 1][ArrDataFields.IndexOf(reportObject.DataField)].ToString()).ToString(reportObject.StringFormat);
                }

                break;
            }

            case FieldValueType.String:
            {
                displayText = rawText;
                if (i > 0 && reportObject.SuppressIfDuplicate)
                {
                    prevDisplayText = ReportTable.Rows[i - 1][ArrDataFields.IndexOf(reportObject.DataField)].ToString();
                }

                break;
            }
        }

        retVals.Add(displayText);
        retVals.Add(prevDisplayText);
        return retVals;
    }

    public ReportObject GetGroupTitle()
    {
        return ReportObjects["Group Title"];
    }

    public ReportObject GetObjectByName(string name)
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

    public int GetSectionHeight(AreaSectionType sectionType)
    {
        return Sections[sectionType].Height;
    }

    public bool SubmitQuery()
    {
        if (!string.IsNullOrWhiteSpace(_stringQuery))
        {
            try
            {
                ReportTable = DataCore.GetTable(_stringQuery);
            }
            catch (Exception)
            {
                return false;
            }
        }

        RowHeightValues = [];
        for (var i = 0; i < ReportTable.Rows.Count; i++)
        {
            var row = ExportTable.NewRow();
            for (var j = 0; j < ExportTable.Columns.Count; j++)
            {
                row[j] = ReportTable.Rows[i][j];
            }

            ExportTable.Rows.Add(row);
        }

        return true;
    }
}