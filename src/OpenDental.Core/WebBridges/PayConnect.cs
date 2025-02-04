using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using CDT;
using CodeBase;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using OpenDentBusiness.PayConnectService;

namespace OpenDentBusiness;

public class PayConnect
{
    private static Credentials GetCredentials(Program prog, long clinicNum)
    {
        var cred = new Credentials
        {
            Username = OpenDentBusiness.ProgramProperties.GetPropVal(prog.ProgramNum, "Username", clinicNum),
            Password = Class1.TryDecrypt(OpenDentBusiness.ProgramProperties.GetPropVal(prog.ProgramNum, "Password", clinicNum)),
            Client = "OpenDental2"
        };

        cred.ServiceID = "DCI Web Service ID: 006328";
        cred.version = "0310";
        return cred;
    }

    public static string GetMerchantServiceUrl()
    {
        var url = "https://webservices.dentalxchange.com/merchant/MerchantService?wsdl";

        return url;
    }

    public static creditCardRequest BuildSaleRequest(decimal amount, string cardNumber, int expYear, int expMonth, string nameOnCard, string securityCode, string zip, string magData, transType transtype, string refNumber, bool tokenRequested, string authCode = "", bool isForced = false)
    {
        var request = new creditCardRequest
        {
            Amount = amount,
            AmountSpecified = true,
            CardNumber = cardNumber,
            Expiration = new expiration
            {
                year = expYear,
                month = expMonth
            }
        };

        if (magData is not null)
        {
            request.MagData = magData;
        }

        request.NameOnCard = nameOnCard;
        request.RefNumber = refNumber;
        request.SecurityCode = securityCode;
        request.TransType = transtype;
        request.Zip = zip;
        request.PaymentTokenRequestedSpecified = true;
        request.PaymentTokenRequested = tokenRequested;
        request.ForceDuplicateSpecified = true;
        request.ForceDuplicate = isForced;

        return request;
    }

    public static transResponse ProcessCreditCard(creditCardRequest request, long clinicNum, Action<string> showError)
    {
        try
        {
            var prog = Programs.GetCur(ProgramName.PayConnect);
            var cred = GetCredentials(prog, clinicNum);
            var ms = new MerchantService();
            ms.Url = GetMerchantServiceUrl();
            var response = ms.processCreditCard(cred, request);
            ms.Dispose();
            if (response.Status.code != 0 && response.Status.description.ToLower().Contains("duplicate"))
            {
                showError(Lans.g("PayConnect", "Payment failed") + ". \r\n" + Lans.g("PayConnect", "Error message from") + " Pay Connect: \""
                          + response.Status.description + "\"\r\n"
                          + Lans.g("PayConnect", "Try using the Force Duplicate checkbox if a duplicate is intended."));
            }

            if (response.Status.code != 0 && response.Status.description.ToLower().Contains("invalid user"))
            {
                showError(Lans.g("PayConnect", "Payment failed") + ".\r\n"
                                                                 + Lans.g("PayConnect", "PayConnect username and password combination invalid.") + "\r\n"
                                                                 + Lans.g("PayConnect", "Verify account settings by going to") + "\r\n"
                                                                 + Lans.g("PayConnect", "Setup | Program Links | PayConnect. The PayConnect username and password are probably the same as the DentalXChange login ID and password."));
            }

            if (response.Status.code == 170)
            {
                showError(Lans.g("PayConnect", "Invalid token. Generate new token using the 'Generate' button on the PayConnect Setup window."));
            }
            else if (response.Status.code != 0)
            {
                //Error
                showError(Lans.g("PayConnect", "Payment failed") + ". \r\n" + Lans.g("PayConnect", "Error message from") + " Pay Connect: \""
                          + response.Status.description + "\"");
            }

            return response;
        }
        catch (Exception ex)
        {
            showError(Lans.g("PayConnect", "Payment failed") + ". \r\n" + Lans.g("PayConnect", "Error message") + ": \"" + ex.Message + "\"");
        }

        return null;
    }

    public static signatureResponse ProcessSignature(signatureRequest sigRequest, long clinicNum, Action<string> showError)
    {
        try
        {
            var prog = Programs.GetCur(ProgramName.PayConnect);
            var cred = GetCredentials(prog, clinicNum);
            var ms = new MerchantService();
            ms.Url = GetMerchantServiceUrl();
            var response = ms.processSignature(cred, sigRequest);
            ms.Dispose();
            if (response.Status.code != 0)
            {
                //Error
                showError(Lans.g("PayConnect", "Signature capture failed") + ". \r\n" + Lans.g("PayConnect", "Error message from") + " Pay Connect: \"" + response.Status.description + "\"");
            }

            return response;
        }
        catch (Exception ex)
        {
            showError(Lans.g("PayConnect", "Signature capture failed") + ". \r\n" + Lans.g("PayConnect", "Error message from") + " Open Dental: \"" + ex.Message + "\"");
        }

        return null;
    }

    public static bool IsValidCardAndExp(string cardNumber, int expYear, int expMonth, Action<string> showError)
    {
        var isValid = false;
        try
        {
            var pcExp = new expiration
            {
                year = expYear,
                month = expMonth
            };
            var ms = new MerchantService();
            ms.Url = GetMerchantServiceUrl();
            isValid = (ms.isValidCard(cardNumber) && ms.isValidExpiration(pcExp));
            ms.Dispose();
        }
        catch (Exception ex)
        {
            showError(Lans.g("PayConnect", "Credit Card validation failed") + ". \r\n" + Lans.g("PayConnect", "Error message from")
                      + " Open Dental: \"" + ex.Message + "\"");
        }

        return isValid;
    }

    public static string BuildReceiptString(creditCardRequest request, transResponse response, signatureResponse sigResponse, long clinicNum, string cardHolder = "")
    {
        if (response == null)
        {
            return "";
        }

        var doShowSignatureLine = DoShowSignatureLine(sigResponse);
        return BuildReceiptString(request.TransType, response.RefNumber, request.NameOnCard, request.CardNumber, request.MagData, response.AuthCode, response.Status.description, response.Messages == null ? null : response.Messages.ToList(), request.Amount, doShowSignatureLine, clinicNum, cardHolder: cardHolder);
    }

    public static string BuildReceiptString(transType transType, string refNum, string patName, string cardNumber, string magData, string authCode, string statusDescription, List<string> messages, decimal amount, bool doShowSignatureLine, long clinicNum, string cardType = "", decimal surchargeAmount = 0, string cardHolder = "")
    {
        var result = "";
        cardNumber = cardNumber ?? ""; //Prevents null reference exceptions when PayConnectPortal transactions don't have an associated card number
        var xmin = 0;
        var xleft = xmin;
        var xright = 15;
        var xmax = 37;
        result += Environment.NewLine;
        result += CreditCardUtils.AddClinicToReceipt(clinicNum);
        //Print body
        result += "Date".PadRight(xright - xleft, '.') + DateTime.Now.ToString() + Environment.NewLine;
        result += Environment.NewLine;
        result += "Trans Type".PadRight(xright - xleft, '.') + transType + Environment.NewLine;
        result += Environment.NewLine;
        result += "Transaction #".PadRight(xright - xleft, '.') + refNum + Environment.NewLine;
        result += "Patient".PadRight(xright - xleft, '.') + patName + Environment.NewLine;
        if (!string.IsNullOrWhiteSpace(cardHolder))
        {
            result += "Cardholder".PadRight(xright - xleft, '.') + cardHolder + Environment.NewLine;
        }

        result += "Account".PadRight(xright - xleft, '.');
        if (cardNumber.Length > 4)
        {
            for (var i = 0; i < cardNumber.Length - 4; i++)
            {
                result += "*";
            }

            result += cardNumber.Substring(cardNumber.Length - 4) + Environment.NewLine; //last 4 digits of card number only.
        }
        else
        {
            //Cardnumber is the last 4 digits
            result += cardNumber.PadLeft(16, '*') + Environment.NewLine;
        }

        if (cardType.IsNullOrEmpty())
        {
            cardType = CreditCardUtils.GetCardType(cardNumber);
        }

        result += "Card Type".PadRight(xright - xleft, '.') + cardType + Environment.NewLine;
        result += "Entry".PadRight(xright - xleft, '.') + (string.IsNullOrEmpty(magData) ? "Manual" : "Swiped") + Environment.NewLine;
        result += "Auth Code".PadRight(xright - xleft, '.') + authCode + Environment.NewLine;
        result += "Result".PadRight(xright - xleft, '.') + statusDescription + Environment.NewLine;
        if (messages != null)
        {
            var label = "Message";
            foreach (var m in messages)
            {
                result += label.PadRight(xright - xleft, '.') + m + Environment.NewLine;
                label = "";
            }
        }

        result += Environment.NewLine + Environment.NewLine + Environment.NewLine;
        if (transType == transType.VOID)
        {
            result += GetAmountStringForReceipt((amount * -1), (surchargeAmount * -1), xleft, xright);
        }
        else
        {
            result += GetAmountStringForReceipt(amount, surchargeAmount, xleft, xright);
        }

        result += Environment.NewLine + Environment.NewLine + Environment.NewLine;
        result += "I agree to pay the above total amount according to my card issuer/bank agreement." + Environment.NewLine;
        result += Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        if (doShowSignatureLine)
        {
            result += "Signature X".PadRight(xmax - xleft, '_');
        }
        else
        {
            result += "Electronically signed";
        }

        return result;
    }

    private static string GetAmountStringForReceipt(decimal amount, decimal surchargeAmount, int xleft, int xright)
    {
        var receiptString = "";
        if (surchargeAmount != 0)
        {
            receiptString += "Amt".PadRight(xright - xleft, '.') + amount.ToString("c") + Environment.NewLine;
            receiptString += "Surcharge".PadRight(xright - xleft, '.') + surchargeAmount.ToString("c") + Environment.NewLine;
            receiptString += "Total Amt".PadRight(xright - xleft, '.') + (amount + surchargeAmount).ToString("c") + Environment.NewLine;
        }
        else
        {
            receiptString += "Total Amt".PadRight(xright - xleft, '.') + amount.ToString("c") + Environment.NewLine;
        }

        return receiptString;
    }

    private static bool DoShowSignatureLine(signatureResponse sigResponse)
    {
        if (sigResponse == null || sigResponse.Status == null)
        {
            return true; //no signature was provided, show line for user to sign manually
        }

        return sigResponse.Status.code != 0; //0 is success, anything else would be a failure to process signature.
    }

    public class WebPaymentProperties
    {
        public bool IsPaymentsAllowed;
        public string Token;
        public int ProgramVersion;
    }

    public static class ProgramProperties
    {
        public const string PayConnectForceRecurringCharge = "PayConnectForceRecurringCharge";
        public const string DefaultProcessingMethod = "DefaultProcessingMethod";
        public const string PayConnectPreventSavingNewCC = "PayConnectPreventSavingNewCC";
        public const string PatientPortalPaymentsEnabled = "IsOnlinePaymentsEnabled";
        public const string PatientPortalPaymentsToken = "Patient Portal Payments Token";
        public const string ProgramVersion = "Program Version";
    }
}

public class PayConnectREST
{
    public static string GetAccountToken(string username, string password)
    {
        #region Response Object

        var resObj = new
        {
            AccountToken = "",
            Status = new
            {
                code = -1,
                description = "",
            },
            Messages = new
            {
                Message = new string[0]
            }
        };

        #endregion

        //var res=GetAccountTokenResponseMock(resObj);
        var listHeaders = GetClientRequestHeaders();
        listHeaders.Add("Username: " + username);
        listHeaders.Add("Password: " + password);
        var res = Request(ApiRoute.AccountToken, HttpMethod.Get, listHeaders, "", resObj);
        if (res == null)
        {
            throw new ODException("Invalid response from PayConnect.");
        }

        var code = -1;
        var codeMsg = "";
        if (res.Status != null)
        {
            code = res.Status.code;
            codeMsg = "Response code: " + res.Status.code + "\r\n";
        }

        if (code == 1000)
        {
            throw new ODException("Request to PayConnect resulted in an internal server error.\r\n"
                                  + "This may be due to invalid credentials.  Please check your username and password.");
        }

        if (code > 0)
        {
            var err = "Invalid response from PayConnect.\r\nResponse code: " + code;
            if (res.Messages != null && res.Messages.Message != null && res.Messages.Message.Length > 0)
            {
                err += "\r\nError retrieving account token.\r\nResponse message(s):\r\n" + string.Join("\r\n", res.Messages.Message);
            }

            throw new ODException(err);
        }

        if (string.IsNullOrWhiteSpace(res.AccountToken))
        {
            throw new ODException("Invalid account token was retrieved from PayConnect." + (string.IsNullOrEmpty(codeMsg) ? "" : "\r\n" + codeMsg));
        }

        return res.AccountToken;
    }

    private static List<string> GetClientRequestHeaders()
    {
        return new List<string>()
        {
            "Client: OpenDental2",
            "ServiceID: DCI Web Service ID: 006328",
            "Version: 0310",
        };
    }

    private static T Request<T>(ApiRoute route, HttpMethod method, List<string> listHeaders, string body, T responseType, string queryStr = "")
    {
        using (var client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            listHeaders.ForEach(x => client.Headers.Add(x));
            client.Encoding = Encoding.UTF8;
            try
            {
                var res = "";
                if (method == HttpMethod.Get)
                {
                    res = client.DownloadString(GetApiUrl(route) + queryStr);
                }
                else if (method == HttpMethod.Post)
                {
                    res = client.UploadString(GetApiUrl(route) + queryStr, HttpMethod.Post.Method, body);
                }
                else if (method == HttpMethod.Put)
                {
                    res = client.UploadString(GetApiUrl(route) + queryStr, HttpMethod.Put.Method, body);
                }
                else
                {
                    throw new Exception("Unsupported HttpMethod type: " + method.Method);
                }

                if ( /* ODBuild.IsDebug() */ false)
                {
                    if ((typeof(T) == typeof(string)))
                    {
                        //If user wants the entire json response as a string
                        return (T) Convert.ChangeType(res, typeof(T));
                    }
                }

                return JsonConvert.DeserializeAnonymousType(res, responseType);
            }
            catch (WebException wex)
            {
                var res = "";
                if (!(wex.Response is HttpWebResponse))
                {
                    throw new Exception("Could not connect to the PayConnect server:\r\n" + wex.Message, wex);
                }

                using (var sr = new StreamReader(((HttpWebResponse) wex.Response).GetResponseStream()))
                {
                    res = sr.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(res))
                {
                    //The response didn't contain a body.  Through my limited testing, it only happens for 401 (Unauthorized) requests.
                    if (wex.Response.GetType() == typeof(HttpWebResponse))
                    {
                        var statusCode = ((HttpWebResponse) wex.Response).StatusCode;
                        if (statusCode == HttpStatusCode.Unauthorized)
                        {
                            throw new ODException(Lans.g("PayConnect", "Invalid PayConnect credentials."));
                        }
                    }
                }

                var errorMsg = wex.Message + (string.IsNullOrWhiteSpace(res) ? "" : "\r\nRaw response:\r\n" + res);
                throw new Exception(errorMsg, wex); //If we got this far and haven't rethrown, simply throw the entire exception.
            }
            catch (Exception ex)
            {
                //WebClient returned an http status code >= 300
                //For now, rethrow error and let whoever is expecting errors to handle them.
                //We may enhance this to care about codes at some point.
                throw;
            }
        }
    }

    private static string GetApiUrl(ApiRoute route)
    {
        var apiUrl = "https://payconnect.dentalxchange.com/pay/rest/PayService";

        switch (route)
        {
            case ApiRoute.Root:
                //Do nothing.  This is to allow someone to quickly grab the URL without having to make a copy+paste reference.
                break;
            case ApiRoute.AccountToken:
                apiUrl += "/accountToken";
                break;
            case ApiRoute.PaymentRequest:
                apiUrl += "/paymentRequest";
                break;
            case ApiRoute.PaymentStatus:
                apiUrl += "/paymentStatus";
                break;
            default:
                break;
        }

        return apiUrl;
    }

    private enum ApiRoute
    {
        Root,
        AccountToken,
        PaymentRequest,
        PaymentStatus,
    }

    public static PayConnectResponse ToPayConnectResponse(transResponse response, creditCardRequest request)
    {
        var pcResponse = new PayConnectResponse();
        if (response != null)
        {
            pcResponse.AuthCode = response.AuthCode;
            pcResponse.RefNumber = response.RefNumber;
            if (response.Status != null)
            {
                pcResponse.Description = response.Status.description;
                pcResponse.StatusCode = response.Status.code.ToString();
            }

            if (response.PaymentToken != null)
            {
                pcResponse.PaymentToken = response.PaymentToken.TokenId;
                if (response.PaymentToken.Expiration != null)
                {
                    pcResponse.TokenExpiration = new DateTime(response.PaymentToken.Expiration.year, response.PaymentToken.Expiration.month, 1);
                }
            }

            pcResponse.CardType = CreditCardUtils.GetCardType(request.CardNumber);
            //PayConnectService.transResponse does not have an amount field, so use the amount we sent them.
            pcResponse.Amount = request.Amount;
        }

        return pcResponse;
    }
}

public enum PayConnectProcessingMethod
{
    [Description("Web Service")]
    WebService,

    [Description("Terminal")]
    Terminal,
}