using System;
using System.Windows.Forms;
using CodeBase;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using OpenDentBusiness;
using static OpenDentBusiness.PayConnect2;

namespace OpenDental;

///<summary>Use this form when there is no token on file for the credit card being used.</summary>
public partial class FormPayConnect2iFrame:FormODBase {
	/// <summary>Controls if we request a tokenizer or payment type iFrame from PayConnect</summary>
	private bool _isAddingCard;
	private PayConnect2Response _response=new PayConnect2Response();
	private long _clinicNum;
	private int _amountInCents;

	public FormPayConnect2iFrame(long clinicNum,int amountInCents=0,bool isAddingCard=false) {
		_isAddingCard=isAddingCard;
		_clinicNum=clinicNum;
		_amountInCents=amountInCents;
		InitializeComponent();
	}

	private async void FormPayConnect2iFrame_Load(object sender,EventArgs e) {
		var url="";
		try {
			url=GetiFrameUrl();
		}
		catch(ODException ex) {
			FriendlyException.Show("Error loading window.",ex);
			DialogResult=DialogResult.Cancel;
			Close();
			return;
		}
		//Cloud requires using the old web browser control due to constraints from thinfinity.
		webViewMain.Visible=true;
		webBrowserMain.Visible=false;
		try {
			await webViewMain.Init();
			webViewMain.CoreWebView2.WebMessageReceived+=GetTransactionResult;
			await webViewMain.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.addEventListener(\'message\', e => { window.chrome.webview.postMessage(e.data); })");
		}
		catch(Exception ex){
			FriendlyException.Show("Error initializing window.",ex);
			DialogResult=DialogResult.Cancel;
			Close();
			return;
		}
		webViewMain.CoreWebView2.Navigate(url);
	}

	///<summary>Throws exceptions.</summary>
	private string GetiFrameUrl() {
		var embedSessionRequest=new EmbedSessionRequest();
		embedSessionRequest.Swiper=true;
		if(_isAddingCard) { 
			embedSessionRequest.Type=PayConnect2.IframeType.Tokenizer;
		}
		else {
			embedSessionRequest.Type=PayConnect2.IframeType.Payment;
			if(_amountInCents>0) {
				//Amount is optional, setting it just prevents it from being changed in the PayConnect iFrame.
				embedSessionRequest.Amount=_amountInCents;
			}
		}
		var payConnect2Response=PayConnect2.PostEmbedSession(embedSessionRequest,_clinicNum);
		if(payConnect2Response.ResponseType==PayConnect2.ResponseType.EmbedSession) {
			return payConnect2Response.EmbedSessionResponse.Url;
		}
		throw new ODException("Error occurred retrieving payment form URL from PayConnect.");
	}

	///<summary>Throws exceptions.</summary>
	private void GetTransactionResult(object sender,CoreWebView2WebMessageReceivedEventArgs args) {
		iFrameResponse response=null;
		try {
			response=JsonConvert.DeserializeObject<iFrameResponse>(args.WebMessageAsJson);
		}
		catch(JsonException) {
			//failed to deserialize, we probably did not recieve a success response from the iFrame.
		}
		catch (Exception ex) {
			throw new ODException("Error retrieving response from PayConnect.",ex);
		}
		//Immediately call the GetStatus endpoint  for easier processing.
		if(response!=null && response.IFrameStatus.ToLower()=="success") {
			//When adding a card the transaction status and reference ID fields will return null from PayConnect, therefore we cannot run GetStatus.
			if(_isAddingCard) {
				//When attempting to add a new card, PayConnect sometimes sends back data fromatted like track 2 of a magstrip. Example: ;1234123412341234=0305101193010877?. If this is the case we need to parse out the card number.
				if(response.Response.CardToken.StartsWith(";") && response.Response.CardToken.EndsWith("?")) {
					var magstripCardParser=new MagstripCardParser(response.Response.CardToken,EnumMagstripCardParseTrack.TrackTwo);
					response.Response.CardToken=magstripCardParser.AccountNumber;
				}
				_response.iFrameResponse=response;
				_response.ResponseType=ResponseType.IFrame;
			}
			else {
				_response=GetTransactionStatus(_clinicNum,response.Response.ReferenceId);
			}
		}
	}

	public PayConnect2Response GetResponse() {
		return _response;
	}

	private void GetTransactionResultCloud(object sender, EventArgs e) {
	}

	private void webBrowserMain_DocumentCompleted(object sender,WebBrowserDocumentCompletedEventArgs e) {
		webBrowserMain.Document.AttachEventHandler("message",GetTransactionResultCloud);
	}

}