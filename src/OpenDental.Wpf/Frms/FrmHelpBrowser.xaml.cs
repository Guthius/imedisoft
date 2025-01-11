using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using OpenDentBusiness;
using WpfControls.UI;

namespace OpenDental {


	public partial class FrmHelpBrowser:FrmODBase {
		private static string _stableVersion;
		private bool _hasInitialized=false;

		///<summary>If enableUI is set to false, then it just shows them the Help Feature page and doesn't let them click.</summary>
		public void EnableUI(bool enableUI) {
			//only intended to run once.
			if(!enableUI) {
				//Locks web browser UI on the form.
				webView2Main.IsEnabled=false;
				webView2Faq.IsEnabled=false;
				toolBarMain.IsEnabled=false;
			}
		}

		//<summary>Gets the latest stable version in the format of "XXX" (205,194,etc).</summary>
		//private static string GetStableVersion() {
		//	if(_stableVersion==null) {
		//		_stableVersion=OpenDentBusiness.Help.GetStableVersion();
		//	}
		//	return _stableVersion;
		//}

		public FrmHelpBrowser() {
			InitializeComponent();
			webView2Main.NavigationCompleted+=webView2Main_NavigationCompleted;
			webView2Faq.NavigationCompleted+=WebView2Faq_NavigationCompleted;
			HasHelpButton=false;
			Load+=FrmHelpBrowser_Load;
		}

		private void FrmHelpBrowser_Load(object sender,EventArgs e) {
			LayoutToolBar();
			StartMaximized=true;
		}

		public void LayoutToolBar() {
			toolBarMain.Clear();
			//this is always true now
			//if(false && Security.IsAuthorized(EnumPermType.FAQEdit,suppressMessage:true)) {
			ToolBarButton toolBarButtonManageFaqs=new ToolBarButton();
			toolBarButtonManageFaqs.Text=Lans.g(this,"Manage FAQ's");
			toolBarButtonManageFaqs.Click+=ManageFAQs_Click;
			toolBarMain.Add(toolBarButtonManageFaqs);
			ToolBarButton toolBarButtonAddFaqs=new ToolBarButton();
			toolBarButtonAddFaqs.Text=Lans.g(this,"Add FAQ for Current Page");
			toolBarButtonAddFaqs.Click+=AddFAQ_Click;
			toolBarMain.Add(toolBarButtonAddFaqs);
			//}
			ToolBarButton toolBarButtonBrowser=new ToolBarButton();
			toolBarButtonBrowser.Text=Lans.g(this,"Browser");
			toolBarButtonBrowser.Click+=Browser_Click;
			toolBarMain.Add(toolBarButtonBrowser);
			//Probably don't even need the forward and back buttons anymore
			ToolBarButton toolBarButtonBack=new ToolBarButton();
			toolBarButtonBack.Text=Lans.g(this,"Back");
			toolBarButtonBack.Icon=EnumIcons.ArrowLeft;
			toolBarButtonBack.Click+=Back_Click;
			toolBarMain.Add(toolBarButtonBack);
			ToolBarButton toolBarButtonForward=new ToolBarButton();
			toolBarButtonForward.Text=Lans.g(this,"Forward");
			toolBarButtonForward.Icon=EnumIcons.ArrowRight;
			toolBarButtonForward.Click+=Forward_Click;
			toolBarMain.Add(toolBarButtonForward);
		}

		public void GoToPage(string url) {
			//if(CodeBase.ODBuild.IsDebug() && Environment.MachineName.ToLower()=="jordanhome"){
			//webView2Main.Username="iis_readonly_user";
			//webView2Main.Pw="vb8932mvdfh";
			//This actually goes to a page that looks something like this: https://opendental.com/autoLogin.aspx?token=d83JWerd&redirect=help244/family.html
			//which redirects to something like this: https://opendental.com/help244/family.html
			webView2Main.Navigate(url);
		}

		///<summary>When the web browser navigates we attempt to determine if it has navigated to a help page. If it has, we parse it and send a new request for the associated faqs. If it navigates to a page not recognized, we hide the faq browser pannel.</summary>
		private void webView2Main_NavigationCompleted(object sender,CoreWebView2NavigationCompletedEventArgs e) {
			ShowAndLoadFaq(webView2Main.GetUri().ToString());
		}

		///<summary>Either shows or hides the FAQ panel depending on the URL passed in. If the URL is a help page, then the panel will navigate (load) the corresponding FAQ page.</summary>
		private void ShowAndLoadFaq(string url) {
			if(!IsHelpPageUrl(url)) {
				splitContainer.SetCollapsed(splitterPanel2,doCollapse:true);
				return;
			}
			webView2Faq.Navigate(HelpUrlToFaqUrl(url));
		}

		private void WebView2Faq_NavigationCompleted(object sender,CoreWebView2NavigationCompletedEventArgs e) {
			//This event gets fired for every iframe in a page to load.  
			//We only care about the webBrowserFaq.Url because e.Url will be iframe urls in addition to the original one.
			if(webView2Faq.GetUri().Query.Contains("results=empty")) {
				splitContainer.SetCollapsed(splitterPanel2,doCollapse:true);
			}
			else {
				splitContainer.SetCollapsed(splitterPanel2,doCollapse:false);
			}
		}

		///<summary>Parses the help page url into the associated faq page url.</summary>
		private string HelpUrlToFaqUrl(string helpPageUrl) {
			string version = "";
			string page = "";
			//urlparams example: /help###/pagename.html



			string[] urlParams = helpPageUrl.Replace("https://www.opendental.com/","").Replace("https://opendental.com/","").Split('/');
			//for example '/help244/'
			version=urlParams[0].Replace("help","");
			page=urlParams[1].Replace(".html","");
			return $"https://opendentalsoft.com:1943/ODFaq/{page}/{version}";
		}

		///<summary>Tries to determine if the navigated url is a help page. To be honest, this method is just hacking apart the url and returning false as soon as it finds something that doesn't fit the help page url pattern. This will definitely have to be added upon in the future.</summary>
		private bool IsHelpPageUrl(string url) {
			if(url.StartsWith("https://www.opendental.com/help")) {
				return true;
			}
			if(url.StartsWith("https://opendental.com/help")) {
				return true;
			}
			return false;
		}

		private void ManageFAQs_Click(object sender,EventArgs e) {
		}

		///<summary>Returns an empty string if the url passed in is empty or is not a help page or if the url is formatted in a way we don't expect. (See IsHelpPageUrl) Otherwise the help page subject will be returned.</summary>
		private string GetPageTopicFromUrl(string url) {
			//The url is expected to in this format: https://opendental.com/help244/claimedit.html. We would just want the "claimedit" piece.
			if(string.IsNullOrWhiteSpace(url) || !IsHelpPageUrl(url)) {
				return "";
			}
			int startIndex = url.LastIndexOf('/')+1;//we want to exclude the '/' so we go one position past the value of startIndex
			int length = (url.LastIndexOf('.')-startIndex);
			if(length<0) {
				length=0;
			}
			string retVal = url.Substring(startIndex,length);
			return retVal;
		}

		///<summary>Parses the version from the help page url. Checks to make sure we're on a legitimate help topic page first and returns an empty string if not.</summary>
		private string GetVersionFromUrl(string url) {
			if(string.IsNullOrWhiteSpace(url) || !IsHelpPageUrl(url)) {
				return "";
			}
			//url example: "https://www.opendental.com/help244/mytopic3.html";
			Match match = Regex.Match(url, @"/help(\d+)/");
			//if(match.Success) {//always works?
			string retVal=match.Groups[1].Value; // Extracts '244'
			return retVal;
		}

		///<summary>Allows the user to go back in navigation of web pages if possible.</summary>
		private void Back_Click(object sender,EventArgs e) {
			if(!webView2Main.CanGoBack()) {//If nothing to navigate back to in history, do nothing
				return;
			}
			Cursor=Cursors.Wait;//Is set back to default cursor after the document loads inside the browser.
													//Application.DoEvents();//To show cursor change.
			webView2Main.GoBack();
			ShowAndLoadFaq(webView2Main.GetUri().ToString());
			Cursor=Cursors.Arrow;//Default didn't exist
		}

		///<summary>Allows the user to go forward in navigation of web pages if possible.</summary>
		private void Forward_Click(object sender,EventArgs e) {
			if(!webView2Main.CanGoForward()) {//If nothing to navigate forward to in history, do nothing
				return;
			}
			Cursor=Cursors.Wait;//Is set back to default cursor after the document loads inside the browser.
													//Application.DoEvents();//To show cursor change.
			webView2Main.GoForward();
			ShowAndLoadFaq(webView2Main.GetUri().ToString());
			Cursor=Cursors.Arrow;//Default didn't exist
		}

		private void AddFAQ_Click(object sender,EventArgs e) {
		}

		private void Browser_Click(object sender,EventArgs e) {
			string topic=GetPageTopicFromUrl(webView2Main.GetUri().ToString());
			string version=GetVersionFromUrl(webView2Main.GetUri().ToString());
			string url="https://opendental.com/help"+version+"/"+topic+".html";
			Process.Start(url);
		}
	}
}
