using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfControls.UI{
/*
Jordan is the only one allowed to edit this file.

How to use the WebBrowser control:


*/
	
	public partial class WebBrowser : UserControl{

		#region Constructor
		public WebBrowser(){
			InitializeComponent();
			webBrowser.Navigated+=WebBrowser_Navigated;
			webBrowser.Navigating+=WebBrowser_Navigating;
		}

		private void WebBrowser_Navigating(object sender,NavigatingCancelEventArgs e) {
			if(!webBrowser.IsLoaded){
				return;
			}
			if(IsEnabled){
				return;
			}
			//This is the only way to disable the WebBrowser. I tried lots of different things.
			//It's an ActiveX control, so you can't put anything on top of it or intercept any mouse events.
			//The downside to this strategy is that it also blocks programmatic navigation, so I added a bit of logic over there to allow.
			e.Cancel=true;
		}


		#endregion Constructor

		#region Events
		[Category("OD")]
		public event NavigatedEventHandler Navigated;
		#endregion Events

		#region Properties

		#endregion Properties

		#region Methods - public
		public bool CanGoBack(){
			return webBrowser.CanGoBack;
		}

		public bool CanGoForward(){
			return webBrowser.CanGoForward;
		}

		public Uri GetUri(){
			return webBrowser.Source;
		}

		public void GoBack(){
			webBrowser.GoBack();
		}

		public void GoForward(){
			webBrowser.GoForward();
		}

		public void Navigate(string url){
			bool isEnabled=IsEnabled;
			if(!isEnabled){
				IsEnabled=true;//so that the programmatic navigate will work
			}
			webBrowser.Navigate(url);
			if(!isEnabled){
				IsEnabled=false;
			}
		}

		public void NavigateToString(string text){
			webBrowser.NavigateToString(text);
		}
		#endregion Methods - public

		#region Methods - private event handlers
		private void WebBrowser_Navigated(object sender,NavigationEventArgs e) {
			System.Windows.Controls.WebBrowser webBrowser2=sender as System.Windows.Controls.WebBrowser;
			if(webBrowser2!=null){
				dynamic activeX = webBrowser2.GetType().InvokeMember("ActiveXInstance",
						BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
						binder:null,webBrowser2,new object[] { });
				activeX.Silent = true;
			}
			Navigated?.Invoke(sender,e);
		}
		#endregion Methods - private event handlers

		#region Methods - private
		
		#endregion Methods - private
	}
}
