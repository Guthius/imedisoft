using System;
using System.Xml;

namespace OpenDentBusiness.Remoting;

public static class WebSerializer
{
    public static XmlWriterSettings CreateXmlWriterSettings(bool omitXmlDeclaration)
    {
        return new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            OmitXmlDeclaration = omitXmlDeclaration
        };
    }

    public static string DeserializeNode(string xml, string nodeName, bool throwIfNotFound = true)
    {
        throw new NotImplementedException();
    }

    public static T DeserializePrimitiveOrThrow<T>(string xml)
    {
        throw new NotImplementedException();
    }
}