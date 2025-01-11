using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CodeBase;

namespace OpenDentBusiness.Remoting;

public static class WebSerializer
{
    private const string CellDelimiterPlaceHolder = "zzzzzzzzzz";
    private const string CellDelimiter = "~";
    private const string DotNetDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static XmlWriterSettings CreateXmlWriterSettings(bool omitXmlDeclaration)
    {
        return new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            OmitXmlDeclaration = omitXmlDeclaration
        };
    }

    private static string EscapeForXml(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }

        var stringBuilder = new StringBuilder();

        var length = str.Length;
        for (var i = 0; i < length; i++)
        {
            var character = str.Substring(i, 1);
            switch (character)
            {
                case "<":
                    stringBuilder.Append("&lt;");
                    continue;
                case ">":
                    stringBuilder.Append("&gt;");
                    continue;
                case "\"":
                    stringBuilder.Append("&quot;");
                    continue;
                case "\'":
                    stringBuilder.Append("&#039;");
                    continue;
                case "&":
                    stringBuilder.Append("&amp;");
                    continue;
                default:
                    stringBuilder.Append(character);
                    break;
            }
        }

        return stringBuilder.ToString();
    }

    private static string ReplaceEscapes(string myString)
    {
        if (string.IsNullOrEmpty(myString))
        {
            return "";
        }

        var processedXml = new StringBuilder();
        for (var i = 0; i < myString.Length; i++)
        {
            var startChar = myString.Substring(i, 1);
            if (startChar != "[")
            {
                processedXml.Append(startChar);
                continue;
            }

            var nextChar = myString.Substring(i + 1, 1);
            if (nextChar != "[")
            {
                processedXml.Append(startChar);
                continue;
            }

            //search for the consecutive ]] to close the special char indicator
            var remaining = myString.Substring(i, myString.Length - i);
            var endsAt = remaining.IndexOf("]]", StringComparison.Ordinal);
            if (endsAt < 0)
            {
                //make sure the special char is closed before the end of this xml tag
                processedXml.Append(startChar);
                continue;
            }

            //we have a good special char to translate it, append it, and set the new index location
            //get the guts of the special char
            var specialChar = remaining.Substring(2, remaining.IndexOf("]]", StringComparison.Ordinal) - 2);
            //convert to asci
            if (!int.TryParse(specialChar, out var asciiAsInt))
            {
                //not a valid ascii value
                processedXml.Append(startChar);
                continue;
            }

            //append the ascii char as a string
            processedXml.Append(char.ConvertFromUtf32(asciiAsInt));
            //set the new index location, we have skipped a good chunk... [[123]]
            i += endsAt + 1;
        }

        return processedXml.ToString();
    }

    public static string SerializePrimitive<T>(T value)
    {
        return SerializeForCSharp(typeof(T).ToString(), value);
    }

    public static string SerializeForCSharp(string typeName, object value)
    {
        if (value == null)
        {
            return "<" + typeName + "/>";
        }

        if (value.GetType().IsEnum)
        {
            return "<" + typeName + ">" + POut.PInt((int) value) + "</" + typeName + ">";
        }
        
        switch (typeName)
        {
            case "System.Int32" or "int":
                return "<int>" + POut.PInt((int) value) + "</int>";
            case "System.Int64" or "long":
                return "<long>" + Convert.ToInt64((long) value) + "</long>";
            case "System.Boolean" or "bool":
                return "<bool>" + POut.PBool((bool) value) + "</bool>";
            case "System.String" or "string":
                return "<string>" + EscapeForXml((string) value) + "</string>";
            case "System.Char" or "char":
                return "<char>" + Convert.ToChar((char) value) + "</char>";
            case "System.Single" or "Single":
                return "<float>" + POut.PFloat((float) value) + "</float>";
            case "System.Byte" or "byte":
                return "<byte>" + POut.PByte((byte) value) + "</byte>";
            case "System.Double" or "double":
                return "<double>" + POut.PDouble((double) value) + "</double>";
        }

        if (typeName.StartsWith("List"))
        {
            return SerializeList(typeName, value);
        }

        if (typeName.Contains("["))
        {
            return SerializeArray();
        }

        return typeName switch
        {
            "DateTime" => "<DateTime>" + ((DateTime) value).ToString(DotNetDateTimeFormat) + "</DateTime>",
            "DataTable" => SerializeDataTable((DataTable) value),
            "DataSet" => SerializeDataSet((DataSet) value),
            _ => throw new NotSupportedException("SerializeForCSharp, unsupported class type: " + typeName)
        };
    }

    public static object Deserialize(string typeName, string xml)
    {
        Type type = null;
        try
        {
            type = Type.GetType(typeName);
        }
        catch
        {
            // ignored
        }

        if (type is not null)
        {
            if (type.IsEnum)
            {
                using var xmlReader = XmlReader.Create(new StringReader(xml));

                if (xmlReader.Read())
                {
                    return Enum.ToObject(type, PIn.PInt(xmlReader.ReadString()));
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return DeserializeList(xml);
            }
        }

        {
            using var xmlReader = XmlReader.Create(new StringReader(xml));

            while (xmlReader.Read())
            {
                switch (xmlReader.Name.ToLower())
                {
                    case "int":
                    case "int32":
                        return PIn.PInt(xmlReader.ReadString());
                    case "long":
                    case "int64":
                        return PIn.PLong(xmlReader.ReadString());
                    case "bool":
                    case "boolean":
                        return PIn.PBool(xmlReader.ReadString());
                    case "string":
                        return PIn.PString(xmlReader.ReadString());
                    case "char":
                        return Convert.ToChar(xmlReader.ReadString());
                    case "float":
                        return PIn.PFloat(xmlReader.ReadString());
                    case "byte":
                        return PIn.PByte(xmlReader.ReadString());
                    case "double":
                        return PIn.PDouble(xmlReader.ReadString());
                    case "datetime":
                        return DateTime.ParseExact(xmlReader.ReadString(), DotNetDateTimeFormat, null);
                    case "datatable":
                        return DeserializeDataTable(xmlReader.ReadOuterXml());
                    case "dataset":
                        return DeserializeDataSet(xmlReader.ReadOuterXml());
                }
                
                if (typeName.Contains("["))
                {
                    throw new Exception("Multidimensional arrays not supported");
                }
            }
        }

        throw new Exception("Deserialize, unsupported primitive or general type: " + typeName);
    }

    public static T DeserializePrimitive<T>(string xml)
    {
        return (T) Deserialize(typeof(T).ToString(), xml);
    }

    public static T DeserializeTag<T>(string xml, string nodeName, bool throwOdExceptionOnErrorNode = false)
    {
        var doc = new XmlDocument();

        doc.LoadXml(xml);

        var xmlNode = doc.SelectSingleNode("//Error");
        if (xmlNode != null)
        {
            if (throwOdExceptionOnErrorNode)
            {
                throw new ODException(xmlNode.InnerText);
            }

            throw new Exception(xmlNode.InnerText);
        }

        xmlNode = doc.SelectSingleNode("//" + nodeName);
        if (xmlNode is null)
        {
            throw new Exception("tagName node not found: " + nodeName);
        }

        T result;

        using (var xmlReader = XmlReader.Create(new StringReader(xmlNode.InnerXml)))
        {
            var serializer = new XmlSerializer(typeof(T));

            result = (T) serializer.Deserialize(xmlReader);
        }

        if (result == null)
        {
            throw new Exception("tagName node invalid: " + nodeName);
        }

        return result;
    }

    public static string DeserializeNode(string xml, string nodeName, bool throwIfNotFound = true)
    {
        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(xml);

        var xmlNode = xmlDocument.SelectSingleNode("//" + nodeName);
        if (xmlNode is not null)
        {
            return xmlNode.InnerText;
        }

        if (!throwIfNotFound)
        {
            return "";
        }

        throw new ODException("Node not found: " + nodeName);
    }

    private static void ParseErrorAndThrow(string xml)
    {
        using var xmlReader = XmlReader.Create(new StringReader(xml));

        xmlReader.MoveToContent();

        while (xmlReader.Read())
        {
            if (!xmlReader.IsStartElement())
            {
                continue;
            }

            var fieldName = xmlReader.Name;

            xmlReader.Read();

            switch (fieldName)
            {
                case "Error":
                    throw new Exception(ReplaceEscapes(xmlReader.ReadContentAsString()));
            }
        }
    }

    public static T DeserializePrimitiveOrThrow<T>(string xml)
    {
        ParseErrorAndThrow(xml);
        
        return (T) Deserialize(typeof(T).ToString(), xml);
    }

    private static DataTable DeserializeDataTable(string xml)
    {
        var dataTable = new DataTable();

        using var xmlReader = XmlReader.Create(new StringReader(xml));

        if (!xmlReader.ReadToFollowing("Name"))
        {
            throw new Exception("Name tag not found");
        }

        dataTable.TableName = ReplaceEscapes(xmlReader.ReadString());
        while (xmlReader.Read())
        {
            if (!xmlReader.IsStartElement())
            {
                continue;
            }

            switch (xmlReader.Name)
            {
                case "":
                case "Cols":
                    continue;

                case "Col":
                    dataTable.Columns.Add(ReplaceEscapes(xmlReader.ReadString()));
                    continue;

                case "Cells":
                    continue;

                case "y":
                {
                    var dataRow = dataTable.NewRow();
                    var pipedRow = xmlReader.ReadString();
                    
                    var cells = pipedRow.Split([CellDelimiter], StringSplitOptions.None);
                    if (cells.Length == dataTable.Columns.Count)
                    {
                        for (var i = 0; i < cells.Length; i++)
                        {
                            cells[i] = ReplaceEscapes(cells[i].Replace(CellDelimiterPlaceHolder, CellDelimiter));
                        }

                        dataRow.ItemArray = cells;
                        dataTable.Rows.Add(dataRow);
                    }

                    continue;
                }
            }
        }

        return dataTable;
    }

    private static string SerializeDataTable(DataTable dataTable)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<DataTable>");
        stringBuilder.Append("<Name>").Append(dataTable.TableName).Append("</Name>");
        stringBuilder.Append("<Cols>");

        for (var i = 0; i < dataTable.Columns.Count; i++)
        {
            stringBuilder.Append("<Col>").Append(dataTable.Columns[i].ColumnName).Append("</Col>");
        }

        stringBuilder.Append("</Cols>");
        stringBuilder.Append("<Cells>");

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            stringBuilder.Append("<y>");
            for (var j = 0; j < dataTable.Columns.Count; j++)
            {
                var cellValue = dataTable.Rows[i][j].ToString();
                if (dataTable.Columns[j].DataType.Name == "DateTime")
                {
                    if (!DateTime.TryParse(dataTable.Rows[i][j].ToString(), out var dt))
                    {
                        dt = new DateTime(1, 1, 1);
                    }

                    cellValue = dt.ToString(DotNetDateTimeFormat);
                }

                stringBuilder.Append(EscapeForXml(cellValue).Replace(CellDelimiter, CellDelimiterPlaceHolder));
                if (j < dataTable.Columns.Count - 1)
                {
                    stringBuilder.Append(CellDelimiter);
                }
            }

            stringBuilder.Append("</y>");
        }

        stringBuilder.Append("</Cells>");
        stringBuilder.Append("</DataTable>");

        return stringBuilder.ToString();
    }

    private static DataSet DeserializeDataSet(string xml)
    {
        var dataSet = new DataSet();

        using var xmlReader = XmlReader.Create(new StringReader(xml));

        xmlReader.MoveToContent();
        xmlReader.Read();

        var typeName = xmlReader.Name; // Should be "DataTable"
        if (typeName == "DataSet")
        {
            return dataSet;
        }

        do
        {
            dataSet.Tables.Add(DeserializeDataTable(xmlReader.ReadOuterXml()));
        } while (xmlReader.Name == typeName);

        return dataSet;
    }

    private static string SerializeDataSet(DataSet ds)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("<DataSet>");
        stringBuilder.Append("<DataTables>");

        for (var i = 0; i < ds.Tables.Count; i++)
        {
            stringBuilder.Append(SerializeDataTable(ds.Tables[i]));
        }

        stringBuilder.Append("</DataTables>");
        stringBuilder.Append("</DataSet>");

        return stringBuilder.ToString();
    }

    public static string SerializeList(string objectType, object obj)
    {
        var m = Regex.Match(objectType, @"^List\[\[60\]\]([a-zA-Z0-9._%+-]*)\[\[62\]\]$");
        if (!m.Success)
        {
            throw new Exception("SerializeList, unknown object list: " + objectType);
        }

        var listType = m.Result("$1");

        var result = new StringBuilder();
        result.Append("<List>");
        if (obj is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                result.Append(SerializeForCSharp(listType, item));
            }
        }

        result.Append("</List>");
        return result.ToString();
    }

    public static List<object> DeserializeList(string xml)
    {
        var results = new List<object>();

        using var xmlReader = XmlReader.Create(new StringReader(xml));

        xmlReader.MoveToContent();
        xmlReader.Read();

        var typeName = xmlReader.Name;
        if (typeName == "List")
        {
            return results;
        }

        do
        {
            results.Add(Deserialize(typeName, xmlReader.ReadOuterXml()));
        } while (xmlReader.Name == typeName);

        return results;
    }

    public static string SerializeArray()
    {
        throw new Exception("SerializeArray, arrays not supported yet.");
    }

    private static string GetNodeNameFromType(Type t)
    {
        return t.Name + "Xml";
    }

    public static T ReadXml<T>(string xml) where T : new()
    {
        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(xml);

        var node = xmlDocument.SelectSingleNode("//" + GetNodeNameFromType(typeof(T)));
        if (node == null)
        {
            throw new ApplicationException(GetNodeNameFromType(typeof(T)) + " node not present.");
        }

        T result;

        var xmlSerializer = new XmlSerializer(typeof(T));
        using (var xmlReader = XmlReader.Create(new StringReader(node.InnerXml)))
        {
            result = (T) xmlSerializer.Deserialize(xmlReader);
        }

        if (result == null)
        {
            result = new T();
        }

        return result;
    }

    public static string WriteXml<T>(T input)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        var stringBuilder = new StringBuilder();

        using (var xmlWriter = XmlWriter.Create(stringBuilder, CreateXmlWriterSettings(true)))
        {
            xmlWriter.WriteStartElement(GetNodeNameFromType(typeof(T)));
            xmlSerializer.Serialize(xmlWriter, input);
            xmlWriter.WriteEndElement();
        }

        return stringBuilder.ToString();
    }
}

internal class POut
{
    public static string PBool(bool value)
    {
        return value ? "1" : "0";
    }

    public static string PByte(byte value)
    {
        return value.ToString();
    }

    public static string PDouble(double value)
    {
        try
        {
            return value.ToString("f", new NumberFormatInfo());
        }
        catch
        {
            return "0";
        }
    }

    public static string PInt(int value)
    {
        return value.ToString();
    }

    public static string PFloat(float value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}

internal class PIn
{
    public static bool PBool(string str)
    {
        return str == "1";
    }

    public static byte PByte(string str)
    {
        return str == "" ? (byte) 0 : Convert.ToByte(str);
    }

    public static double PDouble(string str)
    {
        if (str == "")
        {
            return 0;
        }

        try
        {
            return Convert.ToDouble(str);
        }
        catch
        {
            return 0;
        }
    }

    public static int PInt(string str)
    {
        return str == "" ? 0 : Convert.ToInt32(str);
    }

    public static long PLong(string str)
    {
        return str == "" ? 0 : Convert.ToInt64(str);
    }

    public static float PFloat(string str)
    {
        if (str == "")
        {
            return 0;
        }

        return Convert.ToSingle(str);
    }

    public static string PString(string str)
    {
        return str;
    }
}