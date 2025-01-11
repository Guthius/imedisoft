using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DataConnectionBase;

public class SOut
{
    public static string Bool(bool value)
    {
        return value ? "1" : "0";
    }

    public static string Byte(byte value)
    {
        return value.ToString();
    }

    public static string DateTime(DateTime value, bool encapsulate = true)
    {
        if (value.Year < 1880)
        {
            value = System.DateTime.MinValue;
        }

        string result;
        try
        {
            result = value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
        catch
        {
            return "";
        }

        if (encapsulate)
        {
            result = "'" + result + "'";
        }

        return result;
    }

    public static string Date(DateTime value, bool encapsulate = true)
    {
        try
        {
            var result = value.ToString("yyyy-MM-dd", new DateTimeFormatInfo());

            if (encapsulate)
            {
                result = "'" + result + "'";
            }

            return result;
        }
        catch
        {
            return "";
        }
    }

    public static string TimeSpan(TimeSpan value)
    {
        if (value == System.TimeSpan.Zero)
        {
            return "00:00:00";
        }

        try
        {
            var result = "";
            if (value < System.TimeSpan.Zero)
            {
                result += "-";
                value = value.Duration();
            }

            var hours = value.Days * 24 + value.Hours;

            result += hours.ToString().PadLeft(2, '0') + ":" + value.Minutes.ToString().PadLeft(2, '0') + ":" + value.Seconds.ToString().PadLeft(2, '0');

            return result;
        }
        catch
        {
            return "00:00:00";
        }
    }

    public static string Time(TimeSpan value, bool encapsulate = true)
    {
        var result =
            value.Hours.ToString().PadLeft(2, '0') + ":" +
            value.Minutes.ToString().PadLeft(2, '0') + ":" +
            value.Seconds.ToString().PadLeft(2, '0');

        if (encapsulate)
        {
            return "'" + result + "'";
        }

        return result;
    }

    public static string Double(double value, bool doRounding = true, bool useEnUsFormat = false)
    {
        try
        {
            if (doRounding)
            {
                return value.ToString("f", CultureInfo.InvariantCulture);
            }

            if (!useEnUsFormat)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            var numberFormatInfo = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = ","
            };

            return value.ToString(numberFormatInfo);
        }
        catch
        {
            return "0";
        }
    }

    public static string Double(double value, int decimalPlaces)
    {
        if (decimalPlaces < 0)
        {
            throw new Exception("Cannot round to a negative number of decimal places.");
        }

        try
        {
            var format = "0.";
            for (var i = 0; i < decimalPlaces; i++)
            {
                if (i < 2)
                {
                    format += "0";
                }
                else
                {
                    format += "#";
                }
            }

            return value.ToString(format);
        }
        catch
        {
            return "0";
        }
    }

    public static string Decimal(decimal value)
    {
        try
        {
            return value.ToString("f", CultureInfo.InvariantCulture);
        }
        catch
        {
            return "0";
        }
    }

    public static string Long(long value)
    {
        return value.ToString();
    }

    public static string Int(int value)
    {
        return value.ToString();
    }

    public static string Enum<T>(T value) where T : Enum
    {
        return Int((int) (object) value);
    }

    public static string Float(float value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string String(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }

        var strBuild = new StringBuilder();

        str = StringScrub(str);

        for (var i = 0; i < str.Length; i++)
        {
            switch (str.Substring(i, 1))
            {
                case "'": strBuild.Append(@"\'"); break;
                case "\"": strBuild.Append("\\\""); break;
                case @"\": strBuild.Append(@"\\"); break;
                case "\r": strBuild.Append(@"\r"); break;
                case "\n": strBuild.Append(@"\n"); break;
                case "\t": strBuild.Append(@"\t"); break;
                default: strBuild.Append(str.Substring(i, 1)); break;
            }
        }

        return strBuild.ToString();
    }

    public static string StringNote(string str, bool escapeCharacters = false)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }

        str = StringScrub(str);
        str = str.Replace("\r\n", "\n");
        str = str.Replace("\r", "\n");
        str = Regex.Replace(str, @"[\s]{100,}", " ");
        str = Regex.Replace(str, @"[\0]", "");
        str = str.Replace("\n", "\r\n");

        return escapeCharacters ? String(str) : str;
    }

    public static string StringParam(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }

        str = StringScrub(str);

        return str;
    }

    public static string Bitmap(Bitmap bitmap, ImageFormat imageFormat)
    {
        if (bitmap is null)
        {
            return "";
        }

        using var memoryStream = new MemoryStream();

        bitmap.Save(memoryStream, imageFormat);

        var bytes = memoryStream.ToArray();

        return Convert.ToBase64String(bytes);
    }

    public static string Sound(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new ApplicationException("File does not exist.");
        }

        if (!filename.ToLower().EndsWith(".wav"))
        {
            throw new ApplicationException("Filename must end with .wav");
        }

        var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytes = new byte[fileStream.Length];

        fileStream.Read(bytes, 0, (int) fileStream.Length);

        return Convert.ToBase64String(bytes);
    }

    private static string StringScrub(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }

        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        str = Regex.Replace(str, @"[\u2600-\u27FF]", "\uFFFD");
        str = Regex.Replace(str, @"\uD83C[\uDC00-\uDFFF]", "\uFFFD");
        str = Regex.Replace(str, @"\uD83D[\uDC00-\uDFFF]", "\uFFFD");
        str = Regex.Replace(str, @"\uD83E[\uDD10-\uDDC0]", "\uFFFD");

        return str;
    }
}