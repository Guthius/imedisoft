using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DataConnectionBase;

namespace OpenDentBusiness.Remoting;

public class XmlConverter
{
    private const char CharTab = (char) 0x9;
    private const char CharSpace = (char) 0x20;
    private const char CharLCurly = (char) 0x7B;
    private const char CharRCurly = (char) 0x7D;
    private const char CharSurr = (char) 0xD7FF;
    private const char CharPrivFirst = (char) 0xE000;
    private const char CharRepl = (char) 0xFFFD;

    private static string[] _xmlEscapeStrings;

    public static string[] XmlEscapeStrings
    {
        get
        {
            if (_xmlEscapeStrings is not null)
            {
                return _xmlEscapeStrings;
            }

            _xmlEscapeStrings = new string[0x10000];

            for (var i = 0; i < 0x10000; i++)
            {
                if (i is CharTab or >= CharSpace and <= CharLCurly or >= CharRCurly and <= CharSurr or >= CharPrivFirst and <= CharRepl)
                {
                    _xmlEscapeStrings[i] = ((char) i).ToString();
                }
                else
                {
                    _xmlEscapeStrings[i] = "§#" + i + ";";
                }
            }

            return _xmlEscapeStrings;
        }
    }

    public static string TableToXml(DataTable dataTable)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<DataTable>");
        stringBuilder.Append("<Name>").Append(XmlEscape(dataTable.TableName)).Append("</Name>");
        stringBuilder.Append("<Cols>");

        foreach (DataColumn dataColumn in dataTable.Columns)
        {
            stringBuilder.Append("<Col");

            if (dataColumn.DataType == typeof(decimal))
            {
                stringBuilder.Append(" DataType=\"decimal\"");
            }
            else if (dataColumn.DataType == typeof(DateTime))
            {
                stringBuilder.Append(" DataType=\"DateTime\"");
            }

            stringBuilder.Append(">");
            stringBuilder.Append(XmlEscape(dataColumn.ColumnName));
            stringBuilder.Append("</Col>");
        }

        stringBuilder.Append("</Cols>");

        stringBuilder.Append("<Cells>");

        foreach (DataRow row in dataTable.Rows)
        {
            var cells = new List<string>();

            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                if (dataColumn.DataType == typeof(DateTime))
                {
                    cells.Add(string.IsNullOrEmpty(row[dataColumn].ToString()) ? row[dataColumn].ToString() : XmlEscape(((DateTime) row[dataColumn]).ToString("o")));
                }
                else if (dataColumn.DataType == typeof(byte[]))
                {
                    cells.Add(XmlEscape(SIn.ByteArray(row[dataColumn])));
                }
                else
                {
                    cells.Add(XmlEscape(row[dataColumn].ToString()));
                }
            }

            stringBuilder.Append("<y>" + string.Join("|", cells) + "</y>");
        }

        stringBuilder.Append("</Cells>");
        stringBuilder.Append("</DataTable>");

        return stringBuilder.ToString();
    }

    public static string DataSetToXml(DataSet dataSet)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<DataSet>");
        stringBuilder.Append("<DataTables>");

        for (var i = 0; i < dataSet.Tables.Count; i++)
        {
            stringBuilder.Append(TableToXml(dataSet.Tables[i]));
        }

        stringBuilder.Append("</DataTables>");
        stringBuilder.Append("</DataSet>");

        return stringBuilder.ToString();
    }

    public static DataTable XmlToDataTable(string xmlData)
    {
        var dataTable = new DataTable();
        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(xmlData);

        var nodeName = xmlDocument.SelectSingleNode("//Name");

        dataTable.TableName = XmlStringUnescape(nodeName.InnerText);

        var colsNode = xmlDocument.SelectSingleNode("//Cols");
        if (colsNode is not null)
        {
            foreach (XmlNode childNode in colsNode.ChildNodes)
            {
                var dataColumn = new DataColumn(XmlStringUnescape(childNode.InnerText));
                if (childNode.Attributes.Count > 0)
                {
                    var dataType = XmlStringUnescape(childNode.Attributes["DataType"].InnerText);

                    dataColumn.DataType = dataType switch
                    {
                        "decimal" => typeof(decimal),
                        "DateTime" => typeof(DateTime),
                        _ => dataColumn.DataType
                    };
                }

                dataTable.Columns.Add(dataColumn);
            }
        }

        var cellsNode = xmlDocument.SelectSingleNode("//Cells");
        if (cellsNode is null)
        {
            return dataTable;
        }
        
        foreach (XmlNode xmlNode in cellsNode.ChildNodes)
        {
            var dataRow = dataTable.NewRow();
            var columnNames = new List<string>();

            if (xmlNode.HasChildNodes)
            {
                columnNames = xmlNode.ChildNodes[0].InnerText.Split('|').ToList();
            }

            for (var i = 0; i < columnNames.Count; i++)
            {
                var columnName = XmlStringUnescape(columnNames[i]);

                if (dataTable.Columns[i].DataType == typeof(DateTime))
                {
                    if (string.IsNullOrEmpty(columnName))
                    {
                        columnName = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var dateTime = DateTime.ParseExact(columnName, "o", null);

                        columnName = dateTime.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else if (dataTable.Columns[i].DataType == typeof(decimal) && string.IsNullOrEmpty(columnName))
                {
                    columnName = "0";
                }

                dataRow[i] = columnName;
            }

            dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    public static DataSet XmlToDataSet(string xml)
    {
        var dataSet = new DataSet();
        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(xml);

        var dataTablesNode = xmlDocument.SelectSingleNode("//DataTables");
        if (dataTablesNode is null)
        {
            return dataSet;
        }

        for (var i = 0; i < dataTablesNode.ChildNodes.Count; i++)
        {
            dataSet.Tables.Add(XmlToDataTable(dataTablesNode.ChildNodes[i].OuterXml));
        }

        return dataSet;
    }

    public static string XmlEscape(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        var stringBuilder = new StringBuilder();

        foreach (var ch in str)
        {
            stringBuilder.Append(XmlEscapeStrings[ch]);
        }

        var xmlDocument = new XmlDocument();

        XmlNode xmlNode = xmlDocument.CreateElement("root");
        xmlNode.InnerText = stringBuilder.ToString();

        return xmlNode.InnerXml;
    }

    public static string XmlStringUnescape(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        for (var i = 0; i < XmlEscapeStrings.Length; i++)
        {
            if (i % 255 == 0 && !Regex.IsMatch(str, @"§#[0-9]+;"))
            {
                break;
            }

            if (i is CharTab or >= CharSpace and <= CharLCurly or >= CharRCurly and <= CharSurr or >= CharPrivFirst and <= CharRepl)
            {
                continue;
            }

            str = str.Replace(XmlEscapeStrings[i], ((char) i).ToString());
        }

        return str;
    }

    public static List<string> XmlFindAllInvalidChars(string str)
    {
        const string regexHexChars = @"&#x[0-9abcdefABCDEF]+;";

        if (string.IsNullOrEmpty(str))
        {
            return [];
        }

        var listStrsInvalidChars = new List<string>();
        var matchCollection = Regex.Matches(str, regexHexChars);
        for (var i = 0; i < matchCollection.Count; i++)
        {
            listStrsInvalidChars.Add(matchCollection[i].Value);
        }

        var listCharsInvalid = str.Where(x => !XmlConvert.IsXmlChar(x)).ToList();
        for (var i = 0; i < listCharsInvalid.Count; i++)
        {
            string strUnicodeChar;
            if (listCharsInvalid.Count >= 2 && char.IsSurrogate(listCharsInvalid[i]))
            {
                if (i > listCharsInvalid.Count - 2)
                {
                    continue;
                }

                strUnicodeChar = string.Concat(listCharsInvalid[i], listCharsInvalid[i + 1]);
                i++;
            }
            else
            {
                strUnicodeChar = listCharsInvalid[i].ToString();
            }

            if (!listStrsInvalidChars.Contains(strUnicodeChar))
            {
                listStrsInvalidChars.Add(strUnicodeChar);
            }
        }

        return listStrsInvalidChars;
    }
}