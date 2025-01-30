using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness.localhost;

namespace OpenDentBusiness;

public class OIDInternals
{
    public const string OpenDentalOid = "2.16.840.1.113883.3.4337";

    private static long _customerPatNum;

    public static long GetCustomerPatNum()
    {
        if (_customerPatNum != 0)
        {
            return _customerPatNum;
        }

        var stringBuilder = new StringBuilder();

        using var xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    "
        });

        xmlWriter.WriteStartElement("CustomerIdRequest");
        xmlWriter.WriteStartElement("RegistrationKey");
        xmlWriter.WriteString(PrefC.GetString(PrefName.RegistrationKey));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("RegKeyDisabledOverride");
        xmlWriter.WriteString("true");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndElement();

#if DEBUG
        var service1 = new Service1();
#else
			OpenDentBusiness.customerUpdates.Service1 service1 = new OpenDentBusiness.customerUpdates.Service1();
			service1.Url = PrefC.GetString(PrefName.UpdateServerAddress);
#endif

        string result;
        try
        {
            result = service1.RequestCustomerID(stringBuilder.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception("Error obtaining CustomerID: " + ex.Message);
        }

        var xmlDocument = new XmlDocument();

        xmlDocument.LoadXml(result);

        var xmlNode = xmlDocument.SelectSingleNode("//Error");
        if (xmlNode != null)
        {
            throw new Exception("Error: " + xmlNode.InnerText);
        }

        xmlNode = xmlDocument.SelectSingleNode("//CustomerIdResponse");
        if (xmlNode == null)
        {
            throw new ODException("There was an error requesting your OID or processing the result of the request.  Please try again.");
        }

        if (xmlNode.InnerText == "")
        {
            throw new ODException("Invalid registration key.  Your OIDs will have to be set manually.");
        }

        return _customerPatNum = SIn.Long(xmlNode.InnerText);
    }

    public static OIDInternal GetForType(IdentifierType identifierType)
    {
        InsertMissingValues();

        return OIDInternalCrud.SelectOne("SELECT * FROM oidinternal WHERE IDType='" + identifierType + "'");
    }

    public static void InsertMissingValues()
    {
        var identifierTypes = new List<IdentifierType>();

        var oidInternals = OIDInternalCrud.SelectMany("SELECT * FROM oidinternal");
        foreach (var oidInternal in oidInternals)
        {
            identifierTypes.Add(oidInternal.IDType);
        }

        for (var i = 0; i < Enum.GetValues(typeof(IdentifierType)).Length; i++)
        {
            if (identifierTypes.Contains((IdentifierType) i))
            {
                continue;
            }

            DataCore.NonQ("INSERT INTO oidinternal (IDType,IDRoot) VALUES('" + (IdentifierType) i + "','')", false);
        }
    }

    public static List<OIDInternal> GetAll()
    {
        InsertMissingValues();

        return OIDInternalCrud.SelectMany("SELECT * FROM oidinternal");
    }

    public static void Update(OIDInternal oidInternal)
    {
        OIDInternalCrud.Update(oidInternal);
    }
}