using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using DataConnectionBase;

namespace OpenDental.ReportingComplex
{
    public class ReportObject
    {
        private Point _location;
        private Size _size;
        private Font _font;
        private string _staticText;
        
        public AreaSectionType SectionType { get; protected set; }

        public Point Location
        {
            get => _location;
            set => _location = value;
        }

        public Size Size
        {
            get => _size;
            set => _size = value;
        }

        public string Name { get; protected set; }
        public ReportObjectType ObjectType { get; protected set; }

        public Font Font
        {
            get => _font;
            set
            {
                _font = value;
                _size = CalculateNewSize(_staticText, _font);
            }
        }
        
        public ContentAlignment ContentAlignment { get; set; }
        public Color ForeColor { get; set; }
        
        public string StaticText
        {
            get => _staticText;
            set
            {
                _staticText = value;
                _size = CalculateNewSize(_staticText, _font);
            }
        }
        
        public string StringFormat { get; set; }
        public bool SuppressIfDuplicate { get; set; }
        public float FloatLineThickness { get; set; }
        public LineOrientation LineOrientation { get; set; }
        public LinePosition LinePosition { get; set; }
        public int IntLinePercent { get; set; }
        public int OffSetX { get; set; }
        public int OffSetY { get; set; }
        public bool IsUnderlined { get; set; }
        public FieldDefKind FieldDefKind { get; set; }
        public FieldValueType FieldValueType { get; set; }
        public SpecialFieldType SpecialFieldType { get; set; }
        public SummaryOperation SummaryOperation { get; set; }
        public string SummarizedField { get; set; }
        public string DataField { get; set; }
        public SummaryOrientation SummaryOrientation { get; set; }
        public List<int> SummaryGroups { get; set; }

        protected ReportObject()
        {
        }

        public ReportObject(string name, AreaSectionType sectionType, Point location, Size size, string staticText, Font font, ContentAlignment contentAlignment, int offSetX = 0, int offSetY = 0)
        {
            Name = name;
            SectionType = sectionType;
            _location = location;
            _size = size;
            _staticText = staticText;
            _font = font;
            ContentAlignment = contentAlignment;
            OffSetX = offSetX;
            OffSetY = offSetY;
            ForeColor = Color.Black;
            ObjectType = ReportObjectType.TextObject;
        }

        public ReportObject(string name, AreaSectionType sectionType, Color color, float lineThickness, LineOrientation lineOrientation, LinePosition linePosition, int linePercent, int offSetX, int offSetY)
        {
            Name = name;
            SectionType = sectionType;
            ForeColor = color;
            FloatLineThickness = lineThickness;
            LineOrientation = lineOrientation;
            LinePosition = linePosition;
            IntLinePercent = linePercent;
            OffSetX = offSetX;
            OffSetY = offSetY;
            ObjectType = ReportObjectType.LineObject;
        }

        public ReportObject(string name, AreaSectionType sectionType, Point location, Size size, string dataFieldName, FieldValueType fieldValueType, Font font, ContentAlignment contentAlignment, string stringFormat)
        {
            Name = name;
            SectionType = sectionType;
            _location = location;
            _size = size;
            _font = font;
            ContentAlignment = contentAlignment;
            StringFormat = stringFormat;
            FieldDefKind = FieldDefKind.DataTableField;
            DataField = dataFieldName;
            FieldValueType = fieldValueType;
            ForeColor = Color.Black;
            ObjectType = ReportObjectType.FieldObject;
        }

        public ReportObject(string name, AreaSectionType sectionType, Point location, Size size, SummaryOperation summaryOperation, string summarizedFieldName, Font font, ContentAlignment contentAlignment, string stringFormat)
        {
            Name = name;
            SectionType = sectionType;
            _location = location;
            _size = size;
            _font = font;
            ContentAlignment = contentAlignment;
            StringFormat = stringFormat;
            FieldDefKind = FieldDefKind.SummaryField;
            FieldValueType = FieldValueType.Number;
            SummaryOperation = summaryOperation;
            SummarizedField = summarizedFieldName;
            ForeColor = Color.Black;
            ObjectType = ReportObjectType.FieldObject;
        }

        public ReportObject(string name, AreaSectionType sectionType, Point location, Size size, Color color, SummaryOperation summaryOperation, string summarizedFieldName, Font font, ContentAlignment contentAlignment, string datafield, int offSetX, int offSetY, string stringFormat = "")
        {
            Name = name;
            SectionType = sectionType;
            _location = location;
            _size = size;
            DataField = datafield;
            _font = font;
            FieldDefKind = FieldDefKind.SummaryField;
            FieldValueType = FieldValueType.Number;
            StringFormat = stringFormat;
            SummaryOperation = summaryOperation;
            SummarizedField = summarizedFieldName;
            OffSetX = offSetX;
            OffSetY = offSetY;
            ForeColor = color;
            ContentAlignment = contentAlignment;
            ObjectType = ReportObjectType.TextObject;
        }

        public ReportObject(string name, AreaSectionType sectionType, Point location, Size size, FieldValueType fieldValueType, SpecialFieldType specialType, Font font, ContentAlignment contentAlignment, string stringFormat)
        {
            Name = name;
            SectionType = sectionType;
            _location = location;
            _size = size;
            _font = font;
            ContentAlignment = contentAlignment;
            StringFormat = stringFormat;
            FieldDefKind = FieldDefKind.SpecialField;
            FieldValueType = fieldValueType;
            SpecialFieldType = specialType;
            ForeColor = Color.Black;
            ObjectType = ReportObjectType.FieldObject;
        }
        
        public static StringFormat GetStringFormatAlignment(ContentAlignment contentAlignment)
        {
            if (!Enum.IsDefined(typeof(ContentAlignment), (int) contentAlignment))
            {
                throw new InvalidEnumArgumentException(nameof(contentAlignment), (int) contentAlignment, typeof(ContentAlignment));
            }
            
            var stringFormat = new StringFormat();
            switch (contentAlignment)
            {
                case ContentAlignment.MiddleCenter:
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleLeft:
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleRight:
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.TopCenter:
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopLeft:
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopRight:
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomCenter:
                    stringFormat.LineAlignment = StringAlignment.Far;
                    stringFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                    stringFormat.LineAlignment = StringAlignment.Far;
                    stringFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.BottomRight:
                    stringFormat.LineAlignment = StringAlignment.Far;
                    stringFormat.Alignment = StringAlignment.Far;
                    break;
            }

            return stringFormat;
        }
        
        public ReportObject DeepCopyReportObject()
        {
            var reportObj = new ReportObject
            {
                SectionType = SectionType,
                _location = new Point(_location.X, _location.Y),
                _size = new Size(_size.Width, _size.Height),
                Name = Name,
                ObjectType = ObjectType,
                _font = (Font) _font.Clone(),
                ContentAlignment = ContentAlignment,
                ForeColor = ForeColor,
                _staticText = _staticText,
                StringFormat = StringFormat,
                SuppressIfDuplicate = SuppressIfDuplicate,
                FloatLineThickness = FloatLineThickness,
                FieldDefKind = FieldDefKind,
                FieldValueType = FieldValueType,
                SpecialFieldType = SpecialFieldType,
                SummaryOperation = SummaryOperation,
                LineOrientation = LineOrientation,
                LinePosition = LinePosition,
                IntLinePercent = IntLinePercent,
                OffSetX = OffSetX,
                OffSetY = OffSetY,
                IsUnderlined = IsUnderlined,
                SummarizedField = SummarizedField,
                DataField = DataField,
                SummaryOrientation = SummaryOrientation
            };

            var summaryGroupsNew = new List<int>();
            if (SummaryGroups != null)
            {
                foreach (var item in SummaryGroups)
                {
                    summaryGroupsNew.Add(item);
                }
            }

            reportObj.SummaryGroups = summaryGroupsNew;
            return reportObj;
        }
        
        public double GetSummaryValue(DataTable dataTable, int col)
        {
            double retVal = 0;
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                switch (SummaryOperation)
                {
                    case SummaryOperation.Sum:
                        retVal += SIn.Double(dataTable.Rows[i][col].ToString());
                        break;
                    
                    case SummaryOperation.Count:
                        retVal++;
                        break;
                    
                    case SummaryOperation.Average:
                        retVal += SIn.Double(dataTable.Rows[i][col].ToString()) / dataTable.Rows.Count;
                        break;
                }
            }

            return retVal;
        }
        
        private Size CalculateNewSize(string text, Font font)
        {
            var grfx = Graphics.FromImage(new Bitmap(1, 1));
            
            Size size;
            
            if (SectionType == AreaSectionType.GroupHeader || SectionType == AreaSectionType.GroupFooter || SectionType == AreaSectionType.Detail)
            {
                size = new Size(_size.Width, (int) (grfx.MeasureString(text, font).Height / grfx.DpiY * 100 + 2));
            }
            else
            {
                size = new Size((int) (grfx.MeasureString(text, font).Width / grfx.DpiX * 100 + 2), (int) (grfx.MeasureString(text, font).Height / grfx.DpiY * 100 + 2));
            }

            if (SectionType != AreaSectionType.ReportHeader)
            {
                return size;
            }
            
            _location.X += _size.Width / 2;
            _location.X -= size.Width / 2;

            return size;
        }
    }

    public enum FieldDefKind
    {
        DataTableField,
        FormulaField,
        SpecialField,
        SummaryField
    }

    public enum ReportObjectType
    {
        BoxObject,
        FieldObject,
        LineObject,
        QueryObject,
        TextObject
    }

    public enum SpecialFieldType
    {
        PageNofM,
        PageNumber,
        PrintDate
    }
    
    public enum SummaryOperation
    {
        Count,
        Sum,
        Average
    }

    public enum LineOrientation
    {
        Horizontal,
        Vertical
    }
    
    public enum LinePosition
    {
        Center,
        East,
        North,
        South,
        West
    }

    public enum SplitByKind
    {
        None,
        Date,
        Enum,
        Definition,
        Value
    }

    public enum SummaryOrientation
    {
        None,
        North,
        South,
        East,
        West
    }
}