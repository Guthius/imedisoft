using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CodeBase;

public static class MiscUtils
{
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

    public static string CreateRandomAlphaNumericString(int length)
    {
        var result = "";
        var randChrs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        for (var i = 0; i < length; i++)
        {
            result += randChrs[ODRandom.Next(0, randChrs.Length - 1)];
        }

        return result;
    }

    public static bool IsValidHttpUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return false;
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var uriResult))
        {
            return false;
        }

        return uriResult.Scheme.In(Uri.UriSchemeHttp, Uri.UriSchemeHttps);
    }

    public static string CreateRandomNumericString(int length)
    {
        var result = "";
        var randChrs = "0123456789";
        for (var i = 0; i < length; i++)
        {
            result += randChrs[ODRandom.Next(0, randChrs.Length - 1)];
        }

        return result;
    }

    public static List<DateTime> GetDatesInRange(DateTime dateTimeStart, DateTime dateTimeEnd)
    {
        var listDateTimes = new List<DateTime>();
        if (dateTimeStart != DateTime.MinValue && dateTimeEnd == DateTime.MinValue)
        {
            return ListTools.FromSingle(dateTimeStart);
        }

        if (dateTimeStart == DateTime.MinValue && dateTimeEnd != DateTime.MinValue)
        {
            return ListTools.FromSingle(dateTimeEnd);
        }

        if (dateTimeStart == DateTime.MinValue && dateTimeEnd == DateTime.MinValue)
        {
            return listDateTimes;
        }

        for (var dateTime = dateTimeStart; dateTime <= dateTimeEnd; dateTime = dateTime.AddDays(1))
        {
            listDateTimes.Add(dateTime.Date);
        }

        return listDateTimes;
    }

    public static CultureInfo GetCultureFromThreeLetter(string strThreeLetterISOname)
    {
        if (strThreeLetterISOname == null || strThreeLetterISOname.Length != 3)
        {
            //Length check helps quickly identify custom languages.
            return null;
        }

        var arrayCulturesNeutral = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
        for (var i = 0; i < arrayCulturesNeutral.Length; i++)
        {
            if (arrayCulturesNeutral[i].ThreeLetterISOLanguageName == strThreeLetterISOname)
            {
                return arrayCulturesNeutral[i];
            }
        }

        return null;
    }

    public static bool Between<T>(this T item, T lowerBound, T upperBound, bool isLowerBoundInclusive = true, bool isUpperBoundInclusive = true) where T : IComparable
    {
        if (isLowerBoundInclusive && isUpperBoundInclusive)
        {
            return (item.CompareTo(lowerBound) >= 0 && item.CompareTo(upperBound) <= 0);
        }

        if (isLowerBoundInclusive && !isUpperBoundInclusive)
        {
            return (item.CompareTo(lowerBound) >= 0 && item.CompareTo(upperBound) < 0);
        }

        if (!isLowerBoundInclusive && isUpperBoundInclusive)
        {
            return (item.CompareTo(lowerBound) > 0 && item.CompareTo(upperBound) <= 0);
        }

        if (!isLowerBoundInclusive && !isUpperBoundInclusive)
        {
            return (item.CompareTo(lowerBound) > 0 && item.CompareTo(upperBound) < 0);
        }

        return false; //This code is unreachable but the compiler doesn't realize it.
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> listSource, Func<T, TKey> keySelector)
    {
        var hashSet = new HashSet<TKey>();
        foreach (var source in listSource)
        {
            if (hashSet.Add(keySelector(source)))
            {
                yield return source; //Manipulates the current sourceList instead of having to return an entire list.
            }
        }
    }

    public static void ForEach<T>(this IEnumerable<T> listSource, Action<T> action)
    {
        foreach (var source in listSource)
        {
            action(source);
        }
    }

    public static bool DoSlotsOverlap(DateTime slot1Start, DateTime slot1End, DateTime slot2Start, DateTime slot2End)
    {
        return (slot1End > slot2Start && slot1Start < slot2End);
    }

    public static string GetExceptionText(Exception e, string threadName = null, bool isUnhandledException = true)
    {
        var text = "";
        if (isUnhandledException)
        {
            text = "Unhandled exception ";
        }

        text += (string.IsNullOrEmpty(threadName) ? "" : "from " + threadName)
                + (isUnhandledException ? ":  " : "")
                + (string.IsNullOrEmpty(e.Message) ? "No Exception Message" : e.Message + "\r\n")
                + (string.IsNullOrEmpty(e.GetType().ToString()) ? "No Exception Type" : e.GetType().ToString()) + "\r\n"
                + (string.IsNullOrEmpty(e.StackTrace) ? "No StackTrace" : e.StackTrace);
        if (e is AggregateException)
        {
            foreach (var innerEx in ((AggregateException) e).InnerExceptions)
            {
                text += InnerExceptionToString(innerEx);
            }
        }
        else
        {
            text += InnerExceptionToString(e.InnerException); //New lines handled in method.
        }

        return text;
    }

    public static string InnerExceptionToString(Exception innerEx, int depth = 0)
    {
        if (innerEx == null
            || depth >= 5) //Limit to 5 inner exceptions to prevent infinite recursion
        {
            return "";
        }

        return "\r\n-------------------------------------------\r\n"
               + "Inner exception:  " + innerEx.Message + "\r\n" + innerEx.GetType().ToString() + "\r\n"
               + innerEx.StackTrace
               + InnerExceptionToString(innerEx.InnerException, ++depth);
    }

    public static void PreserveExceptionInfoAndThrow(Exception ex)
    {
        var exInfo = ExceptionDispatchInfo.Capture(ex);
        exInfo.Throw(); //This line should actually throw.
    }

    public static string GetOrdinalIndicator(string num)
    {
        try
        {
            return GetOrdinalIndicator(Convert.ToInt32(num));
        }
        catch
        {
            return ""; //invalid number
        }
    }

    public static string GetOrdinalIndicator(int num)
    {
        if (num <= 0)
        {
            return "";
        }

        switch (num % 100)
        {
            case 11:
            case 12:
            case 13:
                return "th";
        }

        switch (num % 10)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";
        }
    }

    public static DateTime GetMostRecentDayOfWeek(DateTime date, DayOfWeek dayOfWeek)
    {
        if (DateTime.MinValue.AddDays(7) > date)
        {
            throw new ArgumentException("Date must be at least 7 days greater than MinDate: " + date);
        }

        for (var i = 0; i < 7; i++)
        {
            var newDate = date.AddDays(-i);
            if (newDate.DayOfWeek == dayOfWeek)
            {
                return newDate;
            }
        }

        throw new Exception("Unable to find day of the week: " + dayOfWeek.ToString());
    }

    public static DateTime GetUpcomingDayOfWeek(DateTime date, DayOfWeek dayOfWeek)
    {
        if (DateTime.MaxValue.AddDays(-7) < date)
        {
            throw new ArgumentException("Date must be at least 7 days smaller than MaxValue: " + date);
        }

        for (var i = 0; i < 7; i++)
        {
            var newDate = date.AddDays(i);
            if (newDate.DayOfWeek == dayOfWeek)
            {
                return newDate;
            }
        }

        throw new Exception("Unable to find day of the week: " + dayOfWeek.ToString());
    }

    private static DateTime dateTimeDebugPrevious;

    public static string Encrypt(string encrypt)
    {
        var enc = new UTF8Encoding();
        var arrayEncryptBytes = Encoding.UTF8.GetBytes(encrypt);
        var ms = new MemoryStream();
        CryptoStream cs = null;
        Aes aes = new AesCryptoServiceProvider();
        aes.Key = enc.GetBytes("AKQjlLUjlcABVbqp");
        aes.IV = new byte[16];
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(arrayEncryptBytes, 0, arrayEncryptBytes.Length);
        cs.FlushFinalBlock();
        var retval = new byte[ms.Length];
        ms.Position = 0;
        ms.Read(retval, 0, (int) ms.Length);
        cs.Dispose();
        ms.Dispose();
        if (aes != null)
        {
            aes.Clear();
        }

        return Convert.ToBase64String(retval);
    }

    public static string Decrypt(string encString, bool doThrow = false, bool isSilent = false)
    {
        try
        {
            var encrypted = Convert.FromBase64String(encString);
            MemoryStream ms = null;
            CryptoStream cs = null;
            StreamReader sr = null;
            Aes aes = new AesCryptoServiceProvider();
            var enc = new UTF8Encoding();
            aes.Key = enc.GetBytes("AKQjlLUjlcABVbqp");
            aes.IV = new byte[16];
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            ms = new MemoryStream(encrypted);
            cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            sr = new StreamReader(cs);
            var decrypted = sr.ReadToEnd();
            ms.Dispose();
            cs.Dispose();
            sr.Dispose();
            if (aes != null)
            {
                aes.Clear();
            }

            return decrypted;
        }
        catch (Exception e)
        {
            if (doThrow)
            {
                throw e;
            }

            if (!isSilent)
            {
                MessageBox.Show("Text entered was not valid encrypted text.");
            }

            return "";
        }
    }
}

public static class ODRandom
{
    private static readonly object Lock = new();
    private static readonly Random Rand = new();

    public static int Next(int minValue, int maxValue)
    {
        lock (Lock)
        {
            return Rand.Next(minValue, maxValue);
        }
    }

    public static int Next(int maxValue)
    {
        lock (Lock)
        {
            return Rand.Next(maxValue);
        }
    }
}