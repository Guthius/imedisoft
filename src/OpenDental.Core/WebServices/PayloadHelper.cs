using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness;

public class PayloadHelper
{
    public static string CreatePayload(string payloadContentxAsXml, eServiceCode serviceCode, string registrationKey = null, string practiceTitle = null, string practicePhone = null, string programVersion = null, string computerName = null, string programName = null)
    {
        var stringBuilder = new StringBuilder();
        
        using (var xmlWriter = XmlWriter.Create(stringBuilder, WebSerializer.CreateXmlWriterSettings(false)))
        {
            xmlWriter.WriteStartElement("Request");
            xmlWriter.WriteStartElement("Credentials");
            xmlWriter.WriteStartElement("RegistrationKey");
            xmlWriter.WriteString(registrationKey ?? PrefC.GetString(PrefName.RegistrationKey));
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("ProgramVersion");
            xmlWriter.WriteString(programVersion ?? PrefC.GetString(PrefName.ProgramVersion));
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("ServiceCode");
            xmlWriter.WriteString(serviceCode.ToString());
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("ComputerName");
            xmlWriter.WriteString(computerName ?? Environment.MachineName);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("ProgramName");
            xmlWriter.WriteString(programName ?? Assembly.GetEntryAssembly()?.GetName()?.Name ?? "");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement(); //Credentials
            xmlWriter.WriteRaw(payloadContentxAsXml);
            xmlWriter.WriteEndElement(); //Request
        }

        return stringBuilder.ToString();
    }

    public static string CreatePayload(List<PayloadItem> listPayloadItems, eServiceCode serviceCode, string registrationKey = null, string practiceTitle = null, string practicePhone = null, string programVersion = null)
    {
        return CreatePayload(CreatePayloadContent(listPayloadItems), serviceCode, registrationKey, practiceTitle, practicePhone, programVersion);
    }

    public static string CreatePayloadContent(object content, string tagName)
    {
        return CreatePayloadContent([new PayloadItem(content, tagName)]);
    }
        
    public static string CreatePayloadContent(List<PayloadItem> listPayloadItems)
    {
        var stringBuilder = new StringBuilder();
            
        using (var xmlWriter = XmlWriter.Create(stringBuilder, WebSerializer.CreateXmlWriterSettings(true)))
        {
            xmlWriter.WriteStartElement("Payload");
                
            foreach (var payLoadItem in listPayloadItems)
            {
                var xmlListConfirmationRequestSerializer = new XmlSerializer(payLoadItem.Content.GetType());
                    
                xmlWriter.WriteStartElement(payLoadItem.TagName);
                xmlListConfirmationRequestSerializer.Serialize(xmlWriter, payLoadItem.Content);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        return stringBuilder.ToString();
    }

    public static void CheckForError(string xmlResult)
    {
        var xmlDocument = new XmlDocument();
        
        xmlDocument.LoadXml(xmlResult);
        
        var xmlNode = xmlDocument.SelectSingleNode("//Error");
        if (xmlNode is not null)
        {
            throw new Exception(xmlNode.InnerText);
        }
    }
}

public class PayloadItem(object content, string tagName) : Tuple<object, string>(content, tagName)
{
    public object Content => Item1;
    public string TagName => Item2;
}