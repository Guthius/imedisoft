using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Features.Providers.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace OpenDentBusiness.Eclaims
{
    public class EDS
    {
        public static string ErrorMessage = "";
        private const string VALIDATE_ATTACHMENT_URL = "https://web2.edsedi.com/api/validateattachment";
        private const string ATTACHMENTS_URL = "https://web2.edsedi.com/api/attachments";
        
        public static string Benefits270(Clearinghouse clearinghouseClin, string x12message, out Etrans etransHtml)
        {
            //called from x270Controller. Clinic-level clearinghouse passed in.
            var retVal = "";
            etransHtml = null;
            try
            {
                HttpWebRequest webReq;
                WebResponse webResponseXml;
                //Production URL.  For testing, set username to 'test' and password to 'test'.
                //When the username and password are both set to 'test', the X12 270 request will be ignored and just the transmission will be verified.
                webReq = (HttpWebRequest) WebRequest.Create("https://web2.edsedi.com/eds/Transmit_Request");
                webReq.KeepAlive = false;
                webReq.Method = "POST";
                webReq.ContentType = "text/xml";
                var postDataXml = "<?xml version=\"1.0\" encoding=\"us-ascii\"?>"
                                  + "<content>"
                                  + "<header>"
                                  + "<userId>" + clearinghouseClin.LoginID + "</userId>"
                                  + "<pass>" + clearinghouseClin.Password + "</pass>"
                                  + "<process>transmitEligibility</process>"
                                  + "<version>1</version>"
                                  + "</header>"
                                  + "<body>"
                                  + "<type>EDI</type>" //Can only be EDI
                                  + "<data><![CDATA[" + x12message.Replace("\r\n", "").Replace("\n", "") + "]]></data>"
                                  + "<returnType>EDI</returnType>" //Can be EDI, HTML, or EDI.HTML, but should mimic the above type
                                  + "</body>"
                                  + "</content>";
                var encoding = new ASCIIEncoding();
                var arrayXmlBytes = encoding.GetBytes(postDataXml);
                var streamOut = webReq.GetRequestStream();
                streamOut.Write(arrayXmlBytes, 0, arrayXmlBytes.Length);
                streamOut.Close();
                webResponseXml = webReq.GetResponse();
                //Process the response
                var readStream = new StreamReader(webResponseXml.GetResponseStream(), Encoding.ASCII);
                var responseXml = readStream.ReadToEnd();
                readStream.Close();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseXml);
                var nodeErrorCode = xmlDoc.SelectSingleNode(@"content/body/ERROR_CODE");
                if (nodeErrorCode != null && nodeErrorCode.InnerText != "0")
                {
                    throw new Exception("Error Code: " + nodeErrorCode.InnerText + " - " + xmlDoc.SelectSingleNode(@"content/body/ERROR_MSG").InnerText);
                }

                nodeErrorCode = xmlDoc.SelectSingleNode(@"content/error/code");
                if (nodeErrorCode != null && nodeErrorCode.InnerText != "0")
                {
                    throw new Exception("Error Code: " + nodeErrorCode.InnerText + " - " + xmlDoc.SelectSingleNode(@"content/error/description").InnerText);
                }

                var htmlMessage = xmlDoc.SelectSingleNode(@"content/body/htmlData")?.InnerText; //can be null
                if (!string.IsNullOrEmpty(htmlMessage))
                {
                    etransHtml = Etranss.CreateEtrans(DateTime.Now, clearinghouseClin.HqClearinghouseNum, htmlMessage, Security.CurUser.UserNum);
                    etransHtml.Etype = EtransType.HTML;
                    Etranss.Insert(etransHtml);
                }

                retVal = xmlDoc.SelectSingleNode(@"content/body/ediData").InnerText;
            }
            catch (Exception e)
            {
                retVal = e.Message;
            }

            return retVal;
        }

        public static bool Launch(Clearinghouse clearinghouseClin, string x837message)
        {
            try
            {
                HttpWebRequest webReq;
                WebResponse webResponseXml;
                webReq = (HttpWebRequest) WebRequest.Create("https://web2.edsedi.com/eds/Transmit_Request");
                webReq.KeepAlive = false;
                webReq.Method = "POST";
                webReq.ContentType = "text/xml";
                var postDataXml = "<?xml version=\"1.0\" encoding=\"us-ascii\"?>"
                                  + "<content>"
                                  + "<header>"
                                  + "<userId>" + clearinghouseClin.LoginID + "</userId>"
                                  + "<pass>" + clearinghouseClin.Password + "</pass>"
                                  + "<process>transmitClaim</process>"
                                  + "<version>2</version>"
                                  + "</header>"
                                  + "<body>"
                                  + "<type>EDI</type>"
                                  + "<data><![CDATA[" + x837message.Replace("\r\n", "").Replace("\n", "") + "]]></data>"
                                  + "<returnType>XML</returnType>"
                                  + "</body>"
                                  + "</content>";
                var encoding = new ASCIIEncoding();
                var arrayXmlBytes = encoding.GetBytes(postDataXml);
                var streamOut = webReq.GetRequestStream();
                streamOut.Write(arrayXmlBytes, 0, arrayXmlBytes.Length);
                streamOut.Close();
                webResponseXml = webReq.GetResponse();
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

        ///<summary>Attempts to retrieve an End of Day report from EDS.
        ///No need to pass in a date as this web call will retrieve a 277 containing all data since last called.</summary>
        public static bool Retrieve277s(Clearinghouse clearinghouseClin, IODProgressExtended progress)
        {
            progress = progress ?? new ODProgressExtendedNull();
            progress.UpdateProgress(Lans.g(progress.LanThis, "Contacting web server and downloading reports"), "reports", "17%", 17);
            var retVal = false;
            if (progress.IsPauseOrCancel())
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Canceled by user."));
                return false;
            }

            progress.UpdateProgress(Lans.g(progress.LanThis, "Downloading 277s"), "reports", "33%", 33);
            retVal = Retrieve277s(clearinghouseClin);
            if (retVal)
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Retrieved 277s successfully."));
            }
            else
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Retrieving 277s was unsuccessful."));
            }

            return retVal;
        }

        ///<summary>Attempts to retrieve an End of Day report from EDS.
        ///No need to pass in a date as this web call, when clearinghouse.IsEraDownloadAllowed is enabled,
        ///will retrieve an 835 containing all data since last called.</summary>
        public static bool Retrieve835s(Clearinghouse clearinghouseClin, IODProgressExtended progress)
        {
            if (clearinghouseClin.IsEraDownloadAllowed == EraBehaviors.None)
            {
                return true;
            }

            progress = progress ?? new ODProgressExtendedNull();
            progress.UpdateProgress(Lans.g(progress.LanThis, "Contacting web server and downloading reports"), "reports", "40%", 40);
            if (progress.IsPauseOrCancel())
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Canceled by user."));
                return false;
            }

            progress.UpdateProgress(Lans.g(progress.LanThis, "Downloading ERAs"), "reports", "50%", 50);
            if (progress.IsPauseOrCancel())
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Canceled by user."));
                return false;
            }

            var retVal = Retrieve835s(clearinghouseClin);
            if (retVal)
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Retrieved 835s successfully."));
            }
            else
            {
                progress.UpdateProgress(Lans.g(progress.LanThis, "Retrieving 835s was unsuccessful."));
            }

            return retVal;
        }

        public static bool Retrieve277s(Clearinghouse clearinghouseClin)
        {
            try
            {
                HttpWebRequest webReq;
                WebResponse webResponseXml;
                webReq = (HttpWebRequest) WebRequest.Create("https://web2.edsedi.com/eds/Transmit_Request");
                webReq.KeepAlive = false;
                webReq.Method = "POST";
                webReq.ContentType = "text/xml";
                var postDataXml = "<?xml version=\"1.0\" encoding=\"us-ascii\"?>"
                                  + "<content>"
                                  + "<header>"
                                  + "<userId>" + clearinghouseClin.LoginID + "</userId>"
                                  + "<pass>" + clearinghouseClin.Password + "</pass>"
                                  + "<process>requestEndofDay</process>"
                                  + "<version>3</version>"
                                  + "</header>"
                                  + "<body>"
                                  + "<responseType>277</responseType>"
                                  + "</body>"
                                  + "</content>";
                var encoding = new ASCIIEncoding();
                var arrayXmlBytes = encoding.GetBytes(postDataXml);
                var streamOut = webReq.GetRequestStream();
                streamOut.Write(arrayXmlBytes, 0, arrayXmlBytes.Length);
                streamOut.Close();
                webResponseXml = webReq.GetResponse();
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

                var nodeResponseFile = xmlDoc.SelectSingleNode(@"content/body/responseData");
                var exportFilePath = ODFileUtils.CombinePaths(clearinghouseClin.ResponsePath, DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt");
                var reportFileDataBytes = Encoding.UTF8.GetBytes(nodeResponseFile.InnerText);
                File.WriteAllBytes(exportFilePath, reportFileDataBytes);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return false;
            }

            return true;
        }

        public static bool Retrieve835s(Clearinghouse clearinghouseClin)
        {
            try
            {
                HttpWebRequest webReq;
                WebResponse webResponseXml;
                webReq = (HttpWebRequest) WebRequest.Create("https://web2.edsedi.com/eds/Transmit_Request");
                webReq.KeepAlive = false;
                webReq.Method = "POST";
                webReq.ContentType = "text/xml";
                var postDataXml = "<?xml version=\"1.0\" encoding=\"us-ascii\"?>"
                                  + "<content>"
                                  + "<header>"
                                  + "<userId>" + clearinghouseClin.LoginID + "</userId>"
                                  + "<pass>" + clearinghouseClin.Password + "</pass>"
                                  + "<process>listRemits</process>"
                                  + "<version>2</version>"
                                  + "</header>"
                                  + "<body>"
                                  + "<eraBatchId></eraBatchId>" //"Leave blank for open or the eraBatchId to repull".
                                  + "</body>"
                                  + "</content>";
                var encoding = new ASCIIEncoding();
                var arrayXmlBytes = encoding.GetBytes(postDataXml);
                var streamOut = webReq.GetRequestStream();
                streamOut.Write(arrayXmlBytes, 0, arrayXmlBytes.Length);
                streamOut.Close();
                webResponseXml = webReq.GetResponse();
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

                var eraBatchId = xmlDoc.SelectSingleNode(@"content/body/eraBatchId").InnerText;
                var data835 = xmlDoc.SelectSingleNode(@"content/body/eraData").InnerText;
                var exportFilePath = ODFileUtils.CombinePaths(clearinghouseClin.ResponsePath, DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + eraBatchId + ".txt");
                var reportFileDataBytes = Encoding.UTF8.GetBytes(data835);
                File.WriteAllBytes(exportFilePath, reportFileDataBytes);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return false;
            }

            return true;
        }

        ///<summary>Upserts electids returned by calling EDS' payer list web service. Returns an empty string on success. Otherwise, returns an error string.</summary>
        public static string GetPayerList()
        {
            var strResponse = "";
            var xmlDoc = new XmlDocument();
            XmlNodeList xmlNodeList = null;
            try
            {
                xmlDoc.Load("https://web2.edsedi.com/eds/List_Payers");
                var nodeErrorCode = xmlDoc.SelectSingleNode(@"content/error");
                if (nodeErrorCode != null)
                {
                    strResponse = "Error Code: " + nodeErrorCode.SelectSingleNode("code").InnerText + " - " + nodeErrorCode.SelectSingleNode("description").InnerText;
                    return strResponse;
                }

                xmlNodeList = xmlDoc.SelectNodes("//payers/payerRecord");
            }
            catch (Exception e)
            {
                strResponse = e.Message;
                return strResponse;
            }

            var listIdNameAttributes = new List<IdNameAttributes>();
            for (var i = 0; i < xmlNodeList.Count; i++)
            {
                var xmlNodePayer = xmlNodeList.Item(i);
                var idNameAttribute = new IdNameAttributes();
                idNameAttribute.ID = xmlNodePayer.SelectSingleNode("./id").InnerText;
                idNameAttribute.Name = xmlNodePayer.SelectSingleNode("./name").InnerText;
                idNameAttribute.Attributes = string.Join(",", GetAttributes(xmlNodePayer).Select(x => (int) x));
                listIdNameAttributes.Add(idNameAttribute);
            }

            ElectIDs.UpsertFromEds(listIdNameAttributes);
            return strResponse;
        }

        ///<summary>Takes a payer returned from EDS' payer list API method.
        ///Determines the values of each Attribute attached to the payer, returning a list of EnumEDSPayerAttributes which are flagged as supported for the payer.</summary>
        public static List<EnumEDSPayerAttributes> GetAttributes(XmlNode xmlNodePayer)
        {
            var listEDSPayerAttributes = new List<EnumEDSPayerAttributes>();
            if (xmlNodePayer is null)
            {
                return listEDSPayerAttributes;
            }

            var defaultClaimTP = xmlNodePayer.SelectSingleNode("./defaultClaimTP").InnerText;
            if (defaultClaimTP == "ELEC")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.DefaultClaimTP);
            }

            var realtimeClaimTP = xmlNodePayer.SelectSingleNode("./realtimeClaimTP").InnerText;
            if (realtimeClaimTP == "Y")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.RealtimeClaimTP);
            }

            var eligibilityTP = xmlNodePayer.SelectSingleNode("./eligibilityTP").InnerText;
            if (eligibilityTP == "Y")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.EligibilityTP);
            }

            var ERATP = xmlNodePayer.SelectSingleNode("./ERATP").InnerText;
            if (ERATP == "Y")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.ERATP);
            }

            var claimEnrollment = xmlNodePayer.SelectSingleNode("./claimEnrollment").InnerText;
            if (claimEnrollment == "Y")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.ClaimEnrollment);
            }

            var eraEnrollment = xmlNodePayer.SelectSingleNode("./eraEnrollment").InnerText;
            if (eraEnrollment == "Y")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.ERAEnrollment);
            }

            var payerType = xmlNodePayer.SelectSingleNode("./payerType").InnerText;
            if (payerType == "D")
            {
                listEDSPayerAttributes.Add(EnumEDSPayerAttributes.PayerType);
            }

            return listEDSPayerAttributes;
        }

        ///<summary>Throws exceptions. Returns a list of responses indicating whether or not EDS requires allows attachments for the carrier(s)/proccode(s) associated with this claim. This method is the first step of a three step process to add attachments to an EDS claim. Only if attachments are required for this claim are we allowed to proceed to step 2.</summary>
        public static ListPayerResponses ValidateClaim(Claim claim)
        {
            var strJson = CreateValidateAttachmentJSON(claim);
            var jsonReturn = MakeWebRequest(VALIDATE_ATTACHMENT_URL, strJson, claim.ClinicNum);
            return JsonConvert.DeserializeObject<ListPayerResponses>(jsonReturn);
        }

        /// <summary>Throws exceptions. Helper method that will make a call to EDS' API using the passed in endpoint and the JSON content. Returns the JSON response from the API on success.</summary>
        private static string MakeWebRequest(string url, string strJson, long clinicNum)
        {
            var clearinghouse = GetClearinghouse(clinicNum);
            var authenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clearinghouse.LoginID}:{clearinghouse.Password}"));
            using var webClient = new WebClient();
            webClient.Headers.Add("Authorization", $"Basic {authenticationString}");
            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            var response = webClient.UploadString(url, "POST", strJson);
            return response;
        }

        /// <summary>Throws exceptions. Returns a JSON blob containing all the necessary claim info to ask EDS if this claim requires attachments. </summary>
        private static string CreateValidateAttachmentJSON(Claim claim)
        {
            var listProcedureCodes = ProcedureCodes.GetForClaim(claim.ClaimNum);
            var listCarriers = Carriers.GetForClaim(claim);
            var pmsLocation = GetPMSLocation(claim);
            var listOfPayers = new ListPayers();
            listOfPayers.Payers = new List<Payer>();
            Payer payer;
            var clearingHouseEDS = GetClearinghouse(claim.ClinicNum);
            for (var i = 0; i < listCarriers.Count; i++)
            {
                payer = new Payer();
                payer.ID = SOut.Long(listCarriers[i].CarrierNum);
                payer.Date = DateTime.Now;
                payer.PayerId = listCarriers[i].ElectID;
                payer.LocationId = clearingHouseEDS.LocationID;
                payer.PMSLocation = pmsLocation;
                payer.ProcedureCodes = listProcedureCodes.Select(x => new Procedure {ProcId = x.ProcCode}).ToList();
                listOfPayers.Payers.Add(payer);
            }

            return JsonConvert.SerializeObject(listOfPayers, new JsonSerializerSettings
            {
                DateFormatString = "MM/dd/yyyy"
            });
        }

        /// <summary>Returns a PMS location populated with information about the clinic that is associated with this claim. If the office is not using clinics, uses practice information.</summary>
        private static PMSLocation GetPMSLocation(Claim claim)
        {
            var pmsLocation = new PMSLocation();
            var clinic = Clinics.GetClinic(claim.ClinicNum);
            if (clinic != null)
            {
                pmsLocation = CreatePMSLocation(clinic.Id, clinic.Abbr, clinic.AddressLine1, clinic.AddressLine2, clinic.City, clinic.State, clinic.Zip, clinic.PhoneNumber);
            }

            return pmsLocation;
        }

        /// <summary>Creates a new PMSLocation based on the given parameters.</summary>
        private static PMSLocation CreatePMSLocation(long id, string name, string address, string address2, string city, string state, string zipcode, string phoneNumber)
        {
            var pmsLocation = new PMSLocation();
            pmsLocation.ID = id;
            pmsLocation.Name = name;
            pmsLocation.Address1 = address;
            pmsLocation.Address2 = address2;
            pmsLocation.City = city;
            pmsLocation.State = state;
            pmsLocation.Zipcode = zipcode;
            pmsLocation.PhoneNumber = TelephoneNumbers.ReFormat(phoneNumber);
            return pmsLocation;
        }

        /// <summary>Throws exceptions. The second part of EDS' three part system for sending attachments. Returns the unique AttachmentID on EDS' side that will be used as a 'folder' to store attachments for the given claim. AttachmentIDs are only available if EDS requires attachments for this claim (see ValidateClaim).</summary>
        public static AttachmentIDResponse GetAttachmentID(Claim claim)
        {
            var strJson = CreateGetAttachmentIdJSON(claim);
            var jsonReturn = MakeWebRequest(ATTACHMENTS_URL, strJson, claim.ClinicNum);
            return JsonConvert.DeserializeObject<AttachmentIDResponse>(jsonReturn);
        }

        /// <summary>Throws an exception if unable to locate the clearinghouse associated with EDS. Otherwise returns the clearinghouse associated with EDS.</summary>
        private static Clearinghouse GetClearinghouse(long clinicNum)
        {
            var clearinghousehq = Clearinghouses.GetFirstOrDefault(x => x.CommBridge == EclaimsCommBridge.EDS && x.ClinicNum == 0);
            if (clearinghousehq == null)
            {
                throw new ODException("Unable to locate EDS clearinghouse.");
            }

            var clearinghoustClin = Clearinghouses.OverrideFields(clearinghousehq, clinicNum);
            return clearinghoustClin;
        }

        /// <summary>Throws exceptions. Returns a JSON blob with all the info needed to request an attachmentid from EDS. </summary>
        private static string CreateGetAttachmentIdJSON(Claim claim)
        {
            var clearinghouse = GetClearinghouse(claim.ClinicNum);
            var pmsLocation = GetPMSLocation(claim);
            var patPlan = PatPlans.GetPatPlansForPat(claim.PatNum).FirstOrDefault();
            InsSub insSub = null;
            if (patPlan != null)
            {
                insSub = InsSubs.GetOne(patPlan.InsSubNum);
            }

            var providerBill = Providers.GetFirstOrDefault(x => x.Id == claim.ProvBill);
            var providerTreat = Providers.GetFirstOrDefault(x => x.Id == claim.ProvTreat);
            var patient = Patients.GetPat(claim.PatNum);
            var patientInsured = Patients.GetPat(insSub.Subscriber);
            var carrier = Carriers.GetForClaim(claim).FirstOrDefault();
            var error = ValidateAttatchmentResources(patPlan, insSub, providerBill, providerTreat, patient, patientInsured, carrier);
            if (!string.IsNullOrEmpty(error))
            {
                throw new ODException("Unable to retrieve attachment ID for claim:\r\n" + error);
            }

            var attachmentIDRequest = new AttachmentIDRequest();
            attachmentIDRequest.ProviderEntityType = EnumProviderEntityType.Person; //Default the entity type to person
            attachmentIDRequest.VendorClaimId = claim.ClaimIdentifier;
            attachmentIDRequest.PayerId = carrier.ElectID;
            attachmentIDRequest.PayerName = carrier.CarrierName;
            attachmentIDRequest.BillingProviderTaxId = providerBill.Ssn;
            attachmentIDRequest.BillingProviderNpi = providerBill.NationalProviderId;
            attachmentIDRequest.BillingProviderLastName = providerBill.LastName;
            attachmentIDRequest.BillingProviderFirstName = providerBill.FirstName;
            attachmentIDRequest.BillingProviderTaxonomyCode = X12Generator.GetTaxonomy(providerBill);
            attachmentIDRequest = SetBillingProviderAddress(attachmentIDRequest, claim);
            attachmentIDRequest.RenderingProviderNpi = providerTreat.NationalProviderId;
            attachmentIDRequest.RenderingProviderLastName = providerTreat.LastName;
            attachmentIDRequest.RenderingProviderFirstName = providerTreat.FirstName;
            attachmentIDRequest.PatientControlNumber = patient.PatNum.ToString();
            attachmentIDRequest.PatientLastName = patient.LName;
            attachmentIDRequest.PatientFirstName = patient.FName;
            attachmentIDRequest.PatientDob = patient.Birthdate;
            attachmentIDRequest.InsuredId = insSub.SubscriberID;
            attachmentIDRequest.InsuredFirstName = patientInsured.FName;
            attachmentIDRequest.InsuredLastName = patientInsured.LName;
            attachmentIDRequest.InsuredDob = patientInsured.Birthdate;
            attachmentIDRequest.ServiceDate = claim.DateService;
            attachmentIDRequest.TotalChargedAmount = claim.ClaimFee;
            attachmentIDRequest.LocationId = clearinghouse.LocationID;
            attachmentIDRequest.PMSLocation = pmsLocation;
            return JsonConvert.SerializeObject(attachmentIDRequest, new JsonSerializerSettings
            {
                DateFormatString = "MM/dd/yyyy",
            });
        }

        /// <summary>Helper method to validate all the objects used to create an AttachmentID request. Returns error message(s) if any of the passed in arguments are invalid, otherwise returns an empty string.</summary>
        private static string ValidateAttatchmentResources(PatPlan patPlan, InsSub insSub, ProviderDto providerBill, ProviderDto providerTreat, Patient patient, Patient patientInsured, Carrier carrier)
        {
            var stringBuilder = new StringBuilder();
            if (insSub == null || patientInsured == null || patPlan == null)
            {
                stringBuilder.AppendLine("Insurance subscriber not found");
            }

            if (providerBill == null)
            {
                stringBuilder.AppendLine("Billing provider not found");
            }
            else
            {
                //Valid provider so check EDS required fields
                if (!providerBill.IsTin)
                {
                    stringBuilder.AppendLine("Billing provider Tax ID Number is required");
                }

                if (providerBill.Ssn.Length != 9)
                {
                    stringBuilder.AppendLine("Billing provider Tax ID Number must be 9 digits");
                }

                if (providerBill.NationalProviderId.Length != 10)
                {
                    stringBuilder.AppendLine("Billing provider National Provider ID must be 10 digits");
                }
            }

            if (providerTreat == null)
            {
                stringBuilder.AppendLine("Treating provider not found");
            }
            else
            {
                //Valid provider so check EDS required fields
                if (providerTreat.NationalProviderId.Length != 10)
                {
                    stringBuilder.AppendLine("Treating provider National Provider ID must be 10 digits");
                }
            }

            if (patient == null)
            {
                stringBuilder.AppendLine("Patient not found");
            }

            if (carrier == null)
            {
                stringBuilder.AppendLine("Insurance carrier not found");
            }

            return stringBuilder.ToString();
        }

        /// <summary>Sets the provider billing address. Throws exception if ZipCode is not 9 digits since EDS requires that format.</summary>
        private static AttachmentIDRequest SetBillingProviderAddress(AttachmentIDRequest attachmentIDRequest, Claim claim)
        {
            if (attachmentIDRequest == null)
            {
                return attachmentIDRequest;
            }

            string billingAddress;
            string billingCity;
            string billingState;
            string billingZip;
            var clinic = Clinics.GetClinic(claim.ClinicNum);
            if (clinic.UseBillingAddressOnClaims)
            {
                billingAddress = clinic.BillingAddressLine1 + " " + clinic.BillingAddressLine2;
                billingCity = clinic.BillingCity;
                billingState = clinic.BillingState;
                billingZip = clinic.BillingZip;
            }
            else
            {
                billingAddress = clinic.AddressLine1 + " " + clinic.AddressLine2;
                billingCity = clinic.City;
                billingState = clinic.State;
                billingZip = clinic.Zip;
            }

            if (billingZip.Length != 9)
            {
                throw new ODException("Error: EDS requires the billing zipcode to be 9 digits.");
            }

            attachmentIDRequest.BillingProviderStreetAddress = billingAddress;
            attachmentIDRequest.BillingProviderCity = billingCity;
            attachmentIDRequest.BillingProviderState = billingState;
            attachmentIDRequest.BillingProviderZipCode = billingZip;
            return attachmentIDRequest;
        }

        ///<summary>Throws exceptions. Saves the provided list of attachments to EDS and returns the response from EDS. This is the third and final step for sending attachments to EDS and can only be done when steps 1 & 2 are complete (ValidateClaim and GetAttachmentID).</summary>
        public static SaveAttachmentsResponse SaveAttachments(string attachmentId, List<ImageAttachment> listImageAttachments, long clinicNum)
        {
            var strJson = CreateSaveAttachmentsJSON(listImageAttachments);
            var jsonResponse = MakeWebRequest(ATTACHMENTS_URL + @$"/{attachmentId}/images", strJson, clinicNum);
            return JsonConvert.DeserializeObject<SaveAttachmentsResponse>(jsonResponse);
        }

        ///<summary>Returns a JSON blob with all the needed info to save the given list of attachments to EDS.</summary>
        private static string CreateSaveAttachmentsJSON(List<ImageAttachment> listImageAttachments)
        {
            var saveAttachmentsRequest = new SaveAttachmentsRequest();
            saveAttachmentsRequest.RequestType = "JSON";
            saveAttachmentsRequest.EdsClaimId = null;
            saveAttachmentsRequest.ImageCount = listImageAttachments.Count;
            saveAttachmentsRequest.Images = listImageAttachments;
            return JsonConvert.SerializeObject(saveAttachmentsRequest, new JsonSerializerSettings
            {
                DateFormatString = "MM/dd/yyyy",
            });
        }

        /// <summary>Represents the numeric value (1-16) that EDS correlates to different types of documents that can be attached to a claim.</summary>
        public enum EnumDocumentTypeCode
        {
            /// <summary>0</summary>
            [Description("Unknown")]
            Unknown = 0,

            /// <summary>1</summary>
            [Description("EOB or COB")]
            EOB = 1,

            /// <summary>2</summary>
            [Description("Narrative")]
            Narrative,

            /// <summary>3</summary>
            [Description("Student Verification")]
            StudentVerification,

            /// <summary>4</summary>
            [Description("Referral Form")]
            ReferralForm,

            /// <summary>5</summary>
            [Description("Diagnosis")]
            Diagnosis,

            /// <summary>6</summary>
            [Description("Reports")]
            Reports,

            /// <summary>7</summary>
            [Description("Periodontal Charts")]
            PeriodontalCharts,

            /// <summary>8</summary>
            [Description("Progress Notes")]
            ProgressNotes,

            /// <summary>9</summary>
            [Description("Intraoral Image")]
            IntraoralImage,

            /// <summary>10</summary>
            [Description("Pre/Post-op FMX")]
            FMX,

            /// <summary>11</summary>
            [Description("Bitewings")]
            Bitewings,

            /// <summary>12</summary>
            [Description("Pre/Post-op Periapical")]
            Periapical,

            /// <summary>13</summary>
            [Description("Pre/Post-op Panoramic Film")]
            PanoramicFilm,

            /// <summary>14</summary>
            [Description("Partial Mount")]
            PartialMount,

            /// <summary>15</summary>
            [Description("Cephalometric")]
            Cephalometric,

            /// <summary>16</summary>
            [Description("Radiographic Images")]
            XRay
        }

        /// <summary>The enitiy of a provider. Used when requesting an atachment id to save attachments to.</summary>
        public enum EnumProviderEntityType
        {
            ///<summary>0</summary>
            [Description("Person")]
            Person,

            ///<summary>1</summary>
            [Description("Group")]
            Group
        }


        /// <summary>Helpful container class used to store information about EDS attachments.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ImageAttachment
        {
            [JsonIgnore]
            public string FileDisplayName;

            [JsonIgnore]
            public string FileNameActual;

            public EnumDocumentTypeCode DocumentTypeCode;
            public string OrientationCode;
            public int PageCount;
            public long FileSize;
            public DateTime FileDate;
            public byte[] FileData;
            public string FileExt;
            public string Narrative;

            ///<summary>Creates a new ImageAttachment using the passed in parameters.</summary>
            public static ImageAttachment Create(string fileName, DateTime dateTimeCreated, EnumDocumentTypeCode documentTypeCode, Image imageClaim, string narrative, bool isRightOriented = false)
            {
                if (fileName.IsNullOrEmpty() ||
                    dateTimeCreated == DateTime.MinValue ||
                    imageClaim == null)
                {
                    return new ImageAttachment();
                }

                var imageAttachment = new ImageAttachment();
                imageAttachment.FileDate = dateTimeCreated;
                imageAttachment.DocumentTypeCode = documentTypeCode;
                imageAttachment.FileData = ConvertImageToBytes(imageClaim);
                imageAttachment.PageCount = imageClaim.GetFrameCount(FrameDimension.Page);
                imageAttachment.FileSize = imageAttachment.FileData.LongLength;
                imageAttachment.FileExt = GetImageExtension(imageClaim);
                imageAttachment.Narrative = narrative;
                imageAttachment.FileDisplayName = fileName;
                if (isRightOriented)
                {
                    imageAttachment.OrientationCode = "Right";
                }
                else
                {
                    imageAttachment.OrientationCode = "Left";
                }

                return imageAttachment;
            }

            /// <summary>Helper method to parse out the image attachment extension (e.g. .JPEG). If the extension can't be determined from the image.RawFormat, then the default is JPEG.</summary>
            private static string GetImageExtension(Image image)
            {
                var imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == image.RawFormat.Guid);
                if (imageCodecInfo == null)
                {
                    //Typically only happens when dealing with in-memory bitmaps (e.g. snipped image) that haven't been saved to disk yet. Default to JPEG.
                    imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                }

                //FileNameExtension refers to all the file extensions that this image could have in the format *.{extension};*.{extension}; etc. We'll just use the first extension.
                var fileExtension = imageCodecInfo.FilenameExtension.Split(";", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (fileExtension == null)
                {
                    return ".jpeg"; //The FileExtension list did not have any entries. Not likely, but just incase default to .jpeg.
                }

                return fileExtension.TrimStart('*').ToLower(); //Trim off the extra characters so we're left with just the extension itself.
            }

            ///<summary>Takes an image and converts it to a base64 byte representation. EDS requires the image to be in this format when sending attachments.</summary>
            private static byte[] ConvertImageToBytes(Image image)
            {
                using var memoryStream = new MemoryStream();
                var imageFormat = image.RawFormat;
                var imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == image.RawFormat.Guid);
                if (imageCodecInfo == null)
                {
                    //Typically only happens when dealing with in-memory bitmaps (e.g. snipped image) that haven't been saved to disk yet. Default to JPEG.
                    imageFormat = ImageFormat.Jpeg;
                }

                using var bitmap = new Bitmap(image);
                bitmap.Save(memoryStream, imageFormat);
                return memoryStream.ToArray();
            }
        }

        ///<summary>A container object that models the response EDS returns when we ask if a claim requires attachments.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class PayerResponse
        {
            public string ID;
            public bool ClaimLevelResponse;
            public List<ProcedureResponse> ProcedureResponses;
        }

        /// <summary>A procedure level response that will indicate if attachemnts are required or not for this procedure, and if so what type of attachemnts are needed. </summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ProcedureResponse
        {
            public string ProcID;
            public List<ProcedureDocument> Documents;
            public bool Response;
            public string Comments;
        }

        /// <summary>A response from EDS that indicates the type of document required for this claim and the reason it is required. </summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ProcedureDocument
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public EnumDocumentTypeCode DocumentTypeCode;

            public string AttachmentReason;
        }

        /// <summary>An individual payer along with the practice information for the office and the procedurecodes being sent. Used when asking EDS if a claim requires attachments </summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class Payer
        {
            public string ID;
            public DateTime Date;
            public string PayerId;
            public string LocationId;
            public PMSLocation PMSLocation;
            public List<Procedure> ProcedureCodes;
        }

        /// <summary>The practice information for the location sending attachments. If clinics are turned on should reflect the clinic sending the claim, otherwise will reflect information for the practice.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class PMSLocation
        {
            public long ID;
            public string Name;
            public string Address1;
            public string Address2;
            public string City;
            public string State;
            public string Zipcode;
            public string PhoneNumber;
        }

        /// <summary>Container class to hold procedurecodes when asking EDS if a claim requires attachments.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class Procedure
        {
            public string ProcId;
        }

        /// <summary>Wraper class to ensure that the list of payers sent to EDS to ask if a claim requires attachments is named correctly in hte JSON body of the request.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ListPayers
        {
            public List<Payer> Payers;
        }

        /// <summary>Wraper class to ensure that the response EDS returns can be deserialized correctly when we ask if a claim requires attachments.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class ListPayerResponses
        {
            public List<PayerResponse> Payers;
        }

        /// <summary>Container class to hold all the fields necessary to request an attachmentid from EDS </summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AttachmentIDRequest
        {
            public string VendorClaimId;
            public string PayerId;
            public string PayerName;

            [JsonConverter(typeof(StringEnumConverter))]
            public EnumProviderEntityType ProviderEntityType;

            /// <summary>9 digits</summary>
            public string BillingProviderTaxId;

            /// <summary>10 digits</summary>
            public string BillingProviderNpi;

            public string BillingProviderLastName;
            public string BillingProviderFirstName;
            public string BillingProviderTaxonomyCode;
            public string BillingProviderStreetAddress;
            public string BillingProviderCity;
            public string BillingProviderState;

            /// <summary>9 digits</summary>
            public string BillingProviderZipCode;

            /// <summary>10 digits</summary>
            public string RenderingProviderNpi;

            public string RenderingProviderLastName;
            public string RenderingProviderFirstName;
            public string PatientControlNumber;
            public string PatientLastName;
            public string PatientFirstName;
            public DateTime PatientDob;
            public string InsuredId;
            public string InsuredLastName;
            public string InsuredFirstName;
            public DateTime InsuredDob;
            public DateTime ServiceDate;
            public double TotalChargedAmount;
            public string LocationId;
            public PMSLocation PMSLocation;
        }

        /// <summary>Container class to help with the deserialization of EDS' response when we request an attachmentid.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class AttachmentIDResponse
        {
            public string AttachmentID;
            public string Response;
        }

        /// <summary>Container class to hold all the information needed to save attachments to an attachmentid through EDS' attachment service.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SaveAttachmentsRequest
        {
            public string RequestType;
            public string EdsClaimId;
            public int ImageCount;
            public List<ImageAttachment> Images;
        }

        /// <summary>Container class that represents EDS' response after image attachments are saved.</summary>
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        public class SaveAttachmentsResponse
        {
            public string AttachmentID;
            public int ImageCount;
            public string Response;
        }
    }
}