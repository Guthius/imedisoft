using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using Microsoft.Web.WebView2.Core;

namespace OpenDental;

public partial class FormWebBrowserPrefs : FormODBase
{
    public string HtmlContent { get; set; }
    
    public Point PointStart = new(0, 0);
    public Size SizeWindow = new(0, 0);

    public FormWebBrowserPrefs()
    {
        InitializeComponent();
    }

    private async void FormWebBrowserPrefs_Load(object sender, EventArgs e)
    {
        if (SizeWindow != new Size(0, 0))
        {
            Size = SizeWindow;
        }

        if (PointStart == new Point(0, 0))
        {
            CenterToScreen();
        }
        else
        {
            var rectangleWorkingArea = Screen.FromHandle(Handle).WorkingArea;

            var x = PointStart.X;
            if (x + Width > rectangleWorkingArea.Right - 10)
            {
                x = rectangleWorkingArea.Right - Width - 10;
            }

            var y = PointStart.Y;
            if (y + Height > rectangleWorkingArea.Bottom - 10)
            {
                y = rectangleWorkingArea.Bottom - Height - 10;
            }

            Location = new Point(x, y);
        }

        try
        {
            await webView.Init();

            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }
        catch
        {
            DialogResult = DialogResult.Cancel;

            Close();

            return;
        }

        if (string.IsNullOrEmpty(HtmlContent))
        {
            return;
        }

        webView.Visible = true;

        ODException.SwallowAnyException(() => webView.CoreWebView2.NavigateToString(HtmlContent));
    }

    private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;

        try
        {
            Process.Start(e.Uri);
        }
        catch
        {
            ShowError("Could not find " + e.Uri + "\r\nPlease set up a default web browser.");
        }
    }
}