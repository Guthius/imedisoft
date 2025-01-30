using System.Windows;

namespace OpenDental;

public class Lang
{
    public static string g(string classType, string text)
    {
        return text;
    }

    public static string g(object sender, string text)
    {
        return text;
    }
        
    public static void F(FrmODBase frmODBase, params FrameworkElement[] frameworkElementArrayExclusions)
    {
    }
}