using System.Windows.Forms;
using CodeBase;

namespace OpenDental
{
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
            switch (last)
            {
                case "0":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    return str + "th";
                case "1":
                    return str + "st";
                case "2":
                    return str + "nd";
                case "3":
                    return str + "rd";
            }

            return "";
        }
    }
    
    public class ShowErrors : Logger.IWriteLine
    {
        private readonly Control _parent;

        public ShowErrors(Control parent)
        {
            _parent = parent;
        }

        public LogLevel LogLevel { get; set; }

        public void WriteLine(string data, LogLevel logLevel, string subDirectory = "")
        {
            if (logLevel != LogLevel.Error)
            {
                return;
            }

            if (_parent is {InvokeRequired: true})
            {
                _parent.BeginInvoke(() => WriteLine(data, logLevel, subDirectory));
                return;
            }

            ODMessageBox.Show(data);
        }
    }
}