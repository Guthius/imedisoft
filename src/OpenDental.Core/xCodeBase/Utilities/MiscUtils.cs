using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CodeBase;

public static class MiscUtils
{
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

    public static string CreateRandomAlphaNumericString(int length)
    {
        string result = "";
        string randChrs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        for (int i = 0; i < length; i++)
        {
            result += randChrs[ODRandom.Next(0, randChrs.Length - 1)];
        }

        return result;
    }

    public static string CreateRandomAlphaString(int length)
    {
        string result = "";
        string randChrs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < length; i++)
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

        if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult))
        {
            return false;
        }

        return uriResult.Scheme.In(Uri.UriSchemeHttp, Uri.UriSchemeHttps);
    }

    public static string CreateRandomNumericString(int length)
    {
        string result = "";
        string randChrs = "0123456789";
        for (int i = 0; i < length; i++)
        {
            result += randChrs[ODRandom.Next(0, randChrs.Length - 1)];
        }

        return result;
    }

    public static DateTime GetRandomDate(DateTime lowerBound, DateTime upperBound)
    {
        int daysInRange = (int) (upperBound - lowerBound).TotalDays;
        return lowerBound.AddDays(ODRandom.Next(daysInRange));
    }

    public static List<DateTime> GetDatesInRange(DateTime dateTimeStart, DateTime dateTimeEnd)
    {
        List<DateTime> listDateTimes = new List<DateTime>();
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

        for (DateTime dateTime = dateTimeStart; dateTime <= dateTimeEnd; dateTime = dateTime.AddDays(1))
        {
            listDateTimes.Add(dateTime.Date);
        }

        return listDateTimes;
    }

    public static List<string> CutStringIntoSimilarSizedChunks(string inputString, int chunkSize)
    {
        List<string> listChunks = new List<string>();
        int to = 0;
        int from = 0;
        int end = inputString.Length;
        string splitString;
        while (to < end)
        {
            to = Math.Min(to + chunkSize, end);
            int length = to - from;
            splitString = inputString.Substring(from, length);
            while (Encoding.UTF8.GetByteCount(splitString) > chunkSize)
            {
                length--;
                to--;
                splitString = inputString.Substring(from, length);
            }

            listChunks.Add(splitString);
            from += length;
        }

        return listChunks;
    }

    public static CultureInfo GetCultureFromThreeLetter(string strThreeLetterISOname)
    {
        if (strThreeLetterISOname == null || strThreeLetterISOname.Length != 3)
        {
            //Length check helps quickly identify custom languages.
            return null;
        }

        CultureInfo[] arrayCulturesNeutral = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
        for (int i = 0; i < arrayCulturesNeutral.Length; i++)
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
        HashSet<TKey> hashSet = new HashSet<TKey>();
        foreach (T source in listSource)
        {
            if (hashSet.Add(keySelector(source)))
            {
                yield return source; //Manipulates the current sourceList instead of having to return an entire list.
            }
        }
    }

    public static void ForEach<T>(this IEnumerable<T> listSource, Action<T> action)
    {
        foreach (T source in listSource)
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
        string text = "";
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
            foreach (Exception innerEx in ((AggregateException) e).InnerExceptions)
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
        ExceptionDispatchInfo exInfo = ExceptionDispatchInfo.Capture(ex);
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

        for (int i = 0; i < 7; i++)
        {
            DateTime newDate = date.AddDays(-i);
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

        for (int i = 0; i < 7; i++)
        {
            DateTime newDate = date.AddDays(i);
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
        UTF8Encoding enc = new UTF8Encoding();
        byte[] arrayEncryptBytes = Encoding.UTF8.GetBytes(encrypt);
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = null;
        Aes aes = new AesCryptoServiceProvider();
        aes.Key = enc.GetBytes("AKQjlLUjlcABVbqp");
        aes.IV = new byte[16];
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(arrayEncryptBytes, 0, arrayEncryptBytes.Length);
        cs.FlushFinalBlock();
        byte[] retval = new byte[ms.Length];
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
            byte[] encrypted = Convert.FromBase64String(encString);
            MemoryStream ms = null;
            CryptoStream cs = null;
            StreamReader sr = null;
            Aes aes = new AesCryptoServiceProvider();
            UTF8Encoding enc = new UTF8Encoding();
            aes.Key = enc.GetBytes("AKQjlLUjlcABVbqp");
            aes.IV = new byte[16];
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            ms = new MemoryStream(encrypted);
            cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            sr = new StreamReader(cs);
            string decrypted = sr.ReadToEnd();
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

    public static bool TryUpdateIeEmulation()
    {
        bool ret = false;
        try
        {
            int browserVersion;
            //Get the installed IE version.
            using (WebBrowser wb = new WebBrowser())
            {
                browserVersion = wb.Version.Major;
            }

            int regVal;
            //Set the appropriate IE version
            if (browserVersion >= 11)
            {
                regVal = 11001;
            }
            else if (browserVersion == 10)
            {
                regVal = 10001;
            }
            else if (browserVersion == 9)
            {
                regVal = 9999;
            }
            else if (browserVersion == 8)
            {
                regVal = 8888;
            }
            else if (browserVersion == 7)
            {
                regVal = 7000;
            }
            else
            {
                //Unknown version.  This will happen when version 12 and beyond are released.
                regVal = browserVersion * 1000 + 1; //Guess the regVal code needed based on the historic pattern.
            }

            //Set the actual key.  This key can be set without admin rights, because it is within the current user's registry store.
            string applicationName = Process.GetCurrentProcess().ProcessName + ".exe"; //This is OpenDental.vhost.exe when debugging, different for distributors.
            string keyPath = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(keyPath);
            }

            object keyValueCur = key.GetValue(applicationName);
            if (keyValueCur == null || keyValueCur.ToString() != regVal.ToString())
            {
                key.SetValue(applicationName, regVal, RegistryValueKind.DWord);
                ret = true;
            }

            key.Close();
        }
        catch (Exception e)
        {
        }

        return ret;
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