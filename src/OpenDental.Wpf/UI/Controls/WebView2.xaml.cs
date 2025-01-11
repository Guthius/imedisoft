using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeBase;
using Microsoft.Web.WebView2.Core;

namespace WpfControls.UI {
/*
	Run in x86 for debug, otherwise an exception will occur
*/

	public partial class WebView2:UserControl {
		private bool _hasInitialized;
		public string Username;
		public string Pw;

		public WebView2() {
			InitializeComponent();
			webView2.NavigationStarting+=WebView2_NavigationStarting;
			webView2.NavigationCompleted+=WebView2_NavigationCompleted;
			Unloaded+=WebView2_Unloaded;
		}

		public event EventHandler<CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;

		private void WebView2_NavigationStarting(object sender,CoreWebView2NavigationStartingEventArgs e) {
			if(!webView2.IsLoaded) {
				return;
			}
			if(webView2.IsEnabled) {
				return;
			}
			e.Cancel=true;
		}

		private void CoreWebView2_BasicAuthenticationRequested(object sender,CoreWebView2BasicAuthenticationRequestedEventArgs e) {
			e.Response.UserName=Username;
			e.Response.Password=Pw;
		}

		public bool CanGoBack() {
			return webView2.CanGoBack;
		}

		public bool CanGoForward() {
			return webView2.CanGoForward;
		}

		public Uri GetUri() {
			return webView2.Source;
		}

		public void GoBack() {
			webView2.GoBack();
		}

		public void GoForward() {
			webView2.GoForward();
		}

		///<summary>Can throw exceptions, especially if not running in x86.</summary>
		public async void Navigate(string url) {
			if(!_hasInitialized) {
				//Was like below by default, which didn't work because of file permissions in that folder.
				//string userDataFolder= "C:\\Program Files (x86)\\OpenDental\\OpenDental.exe.WebView2";
				//New location is like this:
				//C:\\Users\\User\\AppData\\Local\\Temp\\opendental
				string userDataFolder=OpenDentBusiness.PrefC.GetTempFolderPath();
				try {
					//exceptions include no permission for folder, or runtime not installed, like not running in x86
					CoreWebView2Environment coreWebView2Environment=await CoreWebView2Environment.CreateAsync(userDataFolder:userDataFolder);
					await webView2.EnsureCoreWebView2Async(coreWebView2Environment);
					_hasInitialized=true;
				}
				catch(Exception ex) {
					throw new Exception("Error loading window. Run in x86 for debug.",ex);
				}
			}//end of initialization
			bool isEnabled=webView2.IsEnabled;
			if(!isEnabled) {
				//it won't navigate if disabled, so we temporarily enable it to navigate.
				webView2.IsEnabled=true;
			}
			webView2.CoreWebView2.BasicAuthenticationRequested+=CoreWebView2_BasicAuthenticationRequested;
			webView2.CoreWebView2.Navigate(url);//only exceptions here are for bad format urls
			if(!isEnabled) {
				webView2.IsEnabled=false;
			}
		}

		///<summary>For this, pass in the entire text of the html page to show.</summary>
		public void NavigateToString(string text) {
			webView2.NavigateToString(text);
		}

		private void WebView2_NavigationCompleted(object sender,CoreWebView2NavigationCompletedEventArgs e) {
			NavigationCompleted?.Invoke(sender,e);
			Microsoft.Web.WebView2.Wpf.WebView2 webView2=sender as Microsoft.Web.WebView2.Wpf.WebView2;
			if(webView2!=null) {
				dynamic activeX=webView2.GetType().InvokeMember("ActiveXInstance",
						BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
						binder:null,webView2,new object[] { });
				activeX.Silent=true;
			}
		}

		private void WebView2_Unloaded(object sender,RoutedEventArgs e) {
			webView2?.Dispose();
		}
	}
}

