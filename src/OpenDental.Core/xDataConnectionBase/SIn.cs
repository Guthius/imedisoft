using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace DataConnectionBase;

public class SIn
{
    public static Bitmap Bitmap(string str)
    {
        if (str == null || str.Length < 0x32)
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(str);
            var memoryStream = new MemoryStream(bytes);
            
            return new Bitmap(memoryStream);
        }
        catch
        {
            return null;
        }
    }

    public static bool Bool(string str)
    {
        return str != "" && str != "0" && str.ToLower() != "false";
    }

    public static byte Byte(string str, bool throwExceptions = true)
    {
        if (str == "")
        {
            return 0;
        }

        try
        {
            return Convert.ToByte(str);
        }
        catch
        {
            if (throwExceptions)
            {
                throw;
            }

            return 0;
        }
    }

    public static string ByteArray(object obj)
    {
        if (obj is not byte[] bytes)
        {
            return obj.ToString();
        }

        var stringBuilder = new StringBuilder();

        foreach (var b in bytes)
        {
            stringBuilder.Append((char) b);
        }

        return stringBuilder.ToString();
    }

    public static DateTime Date(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return System.DateTime.MinValue;
        }

        try
        {
            return System.DateTime.Parse(str);
        }
        catch
        {
            return System.DateTime.MinValue;
        }
    }

    public static DateTime DateTime(string str)
    {
        if (str == "")
        {
            return System.DateTime.MinValue;
        }

        try
        {
            return System.DateTime.Parse(str);
        }
        catch
        {
            return System.DateTime.MinValue;
        }
    }

    public static decimal Decimal(string str)
    {
        return decimal.TryParse(str, out var result) ? result : 0;
    }

    public static double Double(string str, bool useUsFormat = false)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }

        try
        {
            if (!useUsFormat)
            {
                return Convert.ToDouble(str);
            }

            var numberFormatInfo = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberGroupSeparator = ","
            };

            return Convert.ToDouble(str, numberFormatInfo);
        }
        catch
        {
            return 0;
        }
    }

    public static int Int(string str, bool throwExceptions = true)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }

        try
        {
            return Convert.ToInt32(str);
        }
        catch
        {
            if (throwExceptions)
            {
                throw;
            }

            return 0;
        }
    }
        
    public static float Float(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }

        try
        {
            return Convert.ToSingle(str);
        }
        catch
        {
            return Convert.ToSingle(str, CultureInfo.InvariantCulture);
        }
    }

    public static long Long(string str, bool throwExceptions = true)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }

        try
        {
            return Convert.ToInt64(str);
        }
        catch
        {
            if (throwExceptions)
            {
                throw;
            }

            return 0;
        }
    }
        
    public static T Enum<T>(string str, bool enumString = false, T defaultValue = default) where T : struct, Enum
    {
        if (!enumString)
        {
            return int.TryParse(str, out var valueAsInt) ? Enum<T>(valueAsInt) : defaultValue;
        }
            
        return System.Enum.TryParse<T>(str, out var result) ? result : defaultValue;
    }
        
    public static T Enum<T>(int value) where T : Enum
    {
        return (T) (object) value;
    }
        
    public static void Sound(string sound, string filename)
    {
        if (!filename.ToLower().EndsWith(".wav"))
        {
            throw new ApplicationException("Filename must end with .wav");
        }

        var bytes = Convert.FromBase64String(sound);
            
        var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            
        fileStream.Write(bytes, 0, bytes.Length);
        fileStream.Close();
    }
        
    public static string String(string str)
    {
        return str;
    }
        
    public static TimeSpan TimeSpan(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return System.TimeSpan.Zero;
        }

        try
        {
            return System.TimeSpan.Parse(str);
        }
        catch
        {
            return System.TimeSpan.Zero;
        }
    }
}