using System;
using System.Windows.Forms;
using CodeBase;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using OpenDentBusiness;
using static OpenDentBusiness.PayConnect2;

namespace OpenDental;

public partial class FormPayConnect2iFrame : FormODBase
{
    private readonly bool _isAddingCard;
    private readonly long _clinicNum;
    private readonly int _amountInCents;
    private PayConnect2Response _response = new();

    public FormPayConnect2iFrame(long clinicNum, int amountInCents = 0, bool isAddingCard = false)
    {
        _isAddingCard = isAddingCard;
        _clinicNum = clinicNum;
        _amountInCents = amountInCents;

        InitializeComponent();
    }

    private async void FormPayConnect2iFrame_Load(object sender, EventArgs e)
    {
        string url;
        try
        {
            url = GetiFrameUrl();
        }
        catch (ODException ex)
        {
            ShowException(ex, "Error loading window.");

            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        webViewMain.Visible = true;
        webBrowserMain.Visible = false;
        try
        {
            await webViewMain.Init();

            webViewMain.CoreWebView2.WebMessageReceived += GetTransactionResult;

            await webViewMain.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.addEventListener(\'message\', e => { window.chrome.webview.postMessage(e.data); })");
        }
        catch (Exception ex)
        {
            ShowException(ex, "Error initializing window.");

            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        webViewMain.CoreWebView2.Navigate(url);
    }

    private string GetiFrameUrl()
    {
        var embedSessionRequest = new EmbedSessionRequest
        {
            Swiper = true
        };

        if (_isAddingCard)
        {
            embedSessionRequest.Type = IframeType.Tokenizer;
        }
        else
        {
            embedSessionRequest.Type = IframeType.Payment;
            if (_amountInCents > 0)
            {
                embedSessionRequest.Amount = _amountInCents;
            }
        }

        var payConnect2Response = PostEmbedSession(embedSessionRequest, _clinicNum);
        if (payConnect2Response.ResponseType == ResponseType.EmbedSession)
        {
            return payConnect2Response.EmbedSessionResponse.Url;
        }

        throw new ODException("Error occurred retrieving payment form URL from PayConnect.");
    }

    private void GetTransactionResult(object sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        iFrameResponse response = null;
        try
        {
            response = JsonConvert.DeserializeObject<iFrameResponse>(args.WebMessageAsJson);
        }
        catch (JsonException)
        {
        }
        catch (Exception ex)
        {
            throw new ODException("Error retrieving response from PayConnect.", ex);
        }

        if (response is null || response.IFrameStatus.ToLower() != "success")
        {
            return;
        }

        if (_isAddingCard)
        {
            // When attempting to add a new card, PayConnect sometimes sends back data fromatted like track 2 of a magstrip.
            // Example: ;1234123412341234=0305101193010877?.
            // If this is the case we need to parse out the card number.
            if (response.Response.CardToken.StartsWith(";") && response.Response.CardToken.EndsWith("?"))
            {
                var magstripCardParser = new MagstripCardParser(response.Response.CardToken, EnumMagstripCardParseTrack.TrackTwo);

                response.Response.CardToken = magstripCardParser.AccountNumber;
            }

            _response.iFrameResponse = response;
            _response.ResponseType = ResponseType.IFrame;
        }
        else
        {
            _response = GetTransactionStatus(_clinicNum, response.Response.ReferenceId);
        }
    }

    public PayConnect2Response GetResponse()
    {
        return _response;
    }
}