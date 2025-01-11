using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.localhost;

namespace OpenDentBusiness;


public class OIDInternals
{
    public static string OpenDentalOID = "2.16.840.1.113883.3.4337";
    private static long _customerPatNum;

    /// <summary>
    ///     The PatNum at Open Dental HQ associated to this database's registration key.
    ///     Makes a web call to WebServiceCustomerUpdates in order to get the PatNum from HQ.
    ///     Throws exceptions to show to the user if anything goes wrong in communicating with the web service.  Exceptions are
    ///     already translated.
    /// </summary>
    public static long GetCustomerPatNum()
    {
        if (_customerPatNum != 0) return _customerPatNum;
        //prepare the xml document to send--------------------------------------------------------------------------------------
        var xmlWriterSettings = new XmlWriterSettings();
        xmlWriterSettings.Indent = true;
        xmlWriterSettings.IndentChars = "    ";
        var stringBuilder = new StringBuilder();
        using var xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings);
        xmlWriter.WriteStartElement("CustomerIdRequest");
        xmlWriter.WriteStartElement("RegistrationKey");
        xmlWriter.WriteString(PrefC.GetString(PrefName.RegistrationKey));
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("RegKeyDisabledOverride");
        xmlWriter.WriteString("true");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndElement();
        xmlWriter?.Dispose();
#if DEBUG
        var service1 = new Service1();
#else
			OpenDentBusiness.customerUpdates.Service1 service1 = new OpenDentBusiness.customerUpdates.Service1();
			service1.Url = PrefC.GetString(PrefName.UpdateServerAddress);
#endif
        //Send the message and get the result---------------------------------------------------------------------------------------
        var result = "";
        try
        {
            result = service1.RequestCustomerID(stringBuilder.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception(Lans.g("OIDInternals", "Error obtaining CustomerID:") + " " + ex.Message);
        }

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(result);
        //Process errors------------------------------------------------------------------------------------------------------------
        var xmlNode = xmlDocument.SelectSingleNode("//Error");
        if (xmlNode != null) throw new Exception(Lans.g("OIDInternals", "Error:") + " " + xmlNode.InnerText);
        //Process a valid return value----------------------------------------------------------------------------------------------
        xmlNode = xmlDocument.SelectSingleNode("//CustomerIdResponse");
        if (xmlNode == null) throw new ODException(Lans.g("OIDInternals", "There was an error requesting your OID or processing the result of the request.  Please try again."));
        if (xmlNode.InnerText == "") throw new ODException(Lans.g("OIDInternals", "Invalid registration key.  Your OIDs will have to be set manually."));
        //CustomerIdResponse has been returned and is not blank
        _customerPatNum = SIn.Long(xmlNode.InnerText);
        return _customerPatNum;
    }

    ///<summary>Returns the currently defined OID for a given IndentifierType.  If not defined, IDroot will be empty string.</summary>
    public static OIDInternal GetForType(IdentifierType identifierType)
    {
        InsertMissingValues(); //
        var command = "SELECT * FROM oidinternal WHERE IDType='" + identifierType + "'"; //should only return one row.
        return OIDInternalCrud.SelectOne(command);
    }

    ///<summary>There should always be one entry in the DB per IdentifierType enumeration.</summary>
    public static void InsertMissingValues()
    {
        //string command= "SELECT COUNT(*) FROM oidinternal";
        //if(PIn.Long(Db.GetCount(command))==Enum.GetValues(typeof(IdentifierType)).Length) {
        //	return;//The DB table has the right count. Which means there is probably nothing wrong with the values in it. This may need to be enhanced if customers have any issues.
        //}
        var command = "SELECT * FROM oidinternal";
        var listOIDInternals = OIDInternalCrud.SelectMany(command);
        var listIDTypes = new List<IdentifierType>();
        for (var i = 0; i < listOIDInternals.Count; i++) listIDTypes.Add(listOIDInternals[i].IDType);
        for (var i = 0; i < Enum.GetValues(typeof(IdentifierType)).Length; i++)
        {
            if (listIDTypes.Contains((IdentifierType) i)) continue; //DB contains a row for this enum value.
            //Insert missing row with blank OID.
            command = "INSERT INTO oidinternal (IDType,IDRoot) "
                      + "VALUES('" + ((IdentifierType) i) + "','')";
            DataCore.NonQ(command, false);
        }
    }

    
    public static List<OIDInternal> GetAll()
    {
        InsertMissingValues(); //there should always be one entry in the DB for each IdentifierType enumeration, insert any missing
        var command = "SELECT * FROM oidinternal";
        return OIDInternalCrud.SelectMany(command);
    }

    
    public static void Update(OIDInternal oIDInternal)
    {
        OIDInternalCrud.Update(oIDInternal);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<OIDInternal> Refresh(long patNum){

        string command="SELECT * FROM oidinternal WHERE PatNum = "+POut.Long(patNum);
        return Crud.OIDInternalCrud.SelectMany(command);
    }

    ///<summary>Gets one OIDInternal from the db.</summary>
    public static OIDInternal GetOne(long ehrOIDNum){

        return Crud.OIDInternalCrud.SelectOne(ehrOIDNum);
    }

    
    public static long Insert(OIDInternal oIDInternal){

        return Crud.OIDInternalCrud.Insert(oIDInternal);
    }

    
    public static void Delete(long ehrOIDNum) {

        string command= "DELETE FROM oidinternal WHERE EhrOIDNum = "+POut.Long(ehrOIDNum);
        Db.NonQ(command);
    }
    */
}