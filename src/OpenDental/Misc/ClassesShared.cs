using System.Windows.Forms;
using CodeBase;

namespace OpenDental;

public class Shared
{
    public static string NumberToOrdinal(int number)
    {
        switch (number)
        {
            case 11:
                return "11th";
            case 12:
                return "12th";
            case 13:
                return "13th";
        }

        var str = number.ToString();
        var last = str.Substring(str.Length - 1);
        return last switch
        {
            "0" or "4" or "5" or "6" or "7" or "8" or "9" => str + "th",
            "1" => str + "st",
            "2" => str + "nd",
            "3" => str + "rd",
            _ => ""
        };
    }
}

public class ShowErrors(Control parent) : Logger.IWriteLine
{
    public void WriteLine(string data, LogLevel logLevel)
    {
        if (logLevel != LogLevel.Error)
        {
            return;
        }

        if (parent is {InvokeRequired: true})
        {
            parent.BeginInvoke(() => WriteLine(data, logLevel));
            return;
        }

        ODMessageBox.Show(data);
    }
}