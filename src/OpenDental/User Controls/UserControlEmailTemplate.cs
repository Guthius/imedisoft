using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class UserControlEmailTemplate:UserControl {

	public UserControlEmailTemplate() {
		InitializeComponent();
		Font=new("Microsoft Sans Serif", 8.25f);
	}

	public void RefreshView(string plainText,string htmlText,EmailType emailType) {
		textboxPlainText.Text=plainText;
		if(string.IsNullOrEmpty(htmlText)) {
			labelNoHtml.Visible=true;
			labelHtml.Visible=false;
			webBrowserEmail.Visible=false;
			tableLayoutPanel1.ColumnStyles[1].Width=0;
				
		}
		else {
			labelNoHtml.Visible=false;
			labelHtml.Visible=true;
			tableLayoutPanel1.ColumnStyles[1].Width=50;//50%
			webBrowserEmail.Visible=true;
			var xhtml=htmlText;
			if(emailType==EmailType.Html) {
				//This might not work for images, we should consider blocking them or warning them about sending if we detect images
				try {
					xhtml=MarkupEdit.TranslateToXhtml(htmlText,true);
				}
				catch {
				}
			}
			webBrowserEmail.DocumentText=xhtml;
		}
	}


}