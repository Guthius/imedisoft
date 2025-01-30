using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness.Eclaims;

internal class EdsMedical
{
    public static string ErrorMessage = "";
    
    public static bool Launch(Clearinghouse clearinghouseClin, string x837message)
    {
        try
        {
            var webReq = (HttpWebRequest) WebRequest.Create("https://web2.edsedi.com/eds/Transmit_Request");
            webReq.KeepAlive = false;
            webReq.Method = "POST";
            webReq.ContentType = "text/xml";
            var postDataXml = "<?xml version=\"1.0\" encoding=\"us-ascii\"?>"
                              + "<content>"
                              + "<header>"
                              + "<userId>" + clearinghouseClin.LoginID + "</userId>"
                              + "<pass>" + clearinghouseClin.Password + "</pass>"
                              + "<process>transmitMedicalClaim</process>"
                              + "<version>1</version>"
                              + "</header>"
                              + "<body>"
                              + "<type>EDI</type>"
                              + "<data><![CDATA[" + x837message.Replace("\r\n", "").Replace("\n", "") + "]]></data>"
                              + "</body>"
                              + "</content>";
            var encoding = new ASCIIEncoding();
            var arrayXmlBytes = encoding.GetBytes(postDataXml);
            var streamOut = webReq.GetRequestStream();
            streamOut.Write(arrayXmlBytes, 0, arrayXmlBytes.Length);
            streamOut.Close();
            var webResponseXml = webReq.GetResponse();
            //Process the response
            var readStream = new StreamReader(webResponseXml.GetResponseStream(), Encoding.ASCII);
            var responseXml = readStream.ReadToEnd();
            readStream.Close();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(responseXml);
            var nodeErrorCode = xmlDoc.SelectSingleNode(@"content/error");
            if (nodeErrorCode != null)
            {
                ErrorMessage = "Error Code: " + nodeErrorCode.SelectSingleNode("code").InnerText + " - " + nodeErrorCode.SelectSingleNode("description").InnerText;
                return false;
            }
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
            
            return false;
        }

        return true;
    }
}