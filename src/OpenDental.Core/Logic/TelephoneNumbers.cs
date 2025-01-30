using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenDentBusiness;

public class TelephoneNumbers
{
    public static bool IsFormattingAllowed()
    {
        return CultureInfo.CurrentCulture.Name == "en-US" || CultureInfo.CurrentCulture.Name.EndsWith("CA");
    }

    public static bool IsNumberValidTenDigit(string phoneNumber)
    {
        if (!IsFormattingAllowed())
        {
            return true;
        }

        phoneNumber = phoneNumber.Replace("(", "");
        phoneNumber = phoneNumber.Replace(")", "");
        phoneNumber = phoneNumber.Replace(" ", "");
        phoneNumber = phoneNumber.Replace("-", "");
        
        return phoneNumber.Length is 0 or 10;
    }

    public static string ReFormat(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return "";
        }

        if (!IsFormattingAllowed())
        {
            return phoneNumber;
        }

        var regex = new Regex(@"^\d{10}$"); //eg. 5033635432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(3, 3) + "-" + phoneNumber.Substring(6);
        }

        regex = new Regex(@"^\d{11}$"); //eg. 15033635432
        if (regex.IsMatch(phoneNumber))
        {
            return phoneNumber.Substring(0, 1) + "(" + phoneNumber.Substring(1, 3) + ")" + phoneNumber.Substring(4, 3) + "-" + phoneNumber.Substring(7);
        }

        regex = new Regex(@"^\d{3}-\d{3}-\d{4}"); //eg. 503-363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(4);
        }

        regex = new Regex(@"^\d-\d{3}-\d{3}-\d{4}"); //eg. 1-503-363-5432 to 1(503)363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return phoneNumber.Substring(0, 1) + "(" + phoneNumber.Substring(2, 3) + ")" + phoneNumber.Substring(6);
        }

        regex = new Regex(@"^\d{3} \d{3}-\d{4}"); //eg 503 363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(4);
        }

        regex = new Regex(@"^\d{3} \d{3} \d{4}"); //eg 916 363 5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(4, 3) + "-" + phoneNumber.Substring(8);
        }

        regex = new Regex(@"^\(\d{3}\) \d{3} \d{4}"); //eg (916) 363 5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(1, 3) + ")" + phoneNumber.Substring(6, 3) + "-" + phoneNumber.Substring(10);
        }

        regex = new Regex(@"^\(\d{3}\) \d{3}-\d{4}"); //eg (916) 363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(1, 3) + ")" + phoneNumber.Substring(6, 3) + "-" + phoneNumber.Substring(10);
        }

        regex = new Regex(@"^\d{7}"); //eg 3635432
        if (regex.IsMatch(phoneNumber))
        {
            //this must be run after the d{10} match up above.
            return phoneNumber.Substring(0, 3) + "-" + phoneNumber.Substring(3);
        }

        regex = new Regex(@"^\(\d{3}-\d{3}-\d{4}"); //eg (916-363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(1, 3) + ")" + phoneNumber.Substring(5, 3) + "-" + phoneNumber.Substring(9);
        }

        regex = new Regex(@"^\d{3}\)\d{3}-\d{4}"); //eg 916)363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(4, 3) + "-" + phoneNumber.Substring(8);
        }

        regex = new Regex(@"^\d{6}-\d{4}"); //eg 916363-5432
        if (regex.IsMatch(phoneNumber))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(3, 3) + "-" + phoneNumber.Substring(7);
        }

        return phoneNumber;
    }

    public static string AutoFormat(string phoneNumber)
    {
        if (!IsFormattingAllowed())
        {
            return phoneNumber;
        }

        if (Regex.IsMatch(phoneNumber, @"^[2-9]$"))
        {
            return "(" + phoneNumber;
        }

        if (Regex.IsMatch(phoneNumber, @"^1\d$"))
        {
            return "1(" + phoneNumber.Substring(1);
        }

        if (Regex.IsMatch(phoneNumber, @"^\(\d\d\d\d$"))
        {
            return phoneNumber.Substring(0, 4) + ")" + phoneNumber.Substring(4);
        }

        if (Regex.IsMatch(phoneNumber, @"^1\(\d\d\d\d$"))
        {
            return phoneNumber.Substring(0, 5) + ")" + phoneNumber.Substring(5);
        }

        if (Regex.IsMatch(phoneNumber, @"^\(\d\d\d\)\d\d\d\d$"))
        {
            return phoneNumber.Substring(0, 8) + "-" + phoneNumber.Substring(8);
        }

        if (Regex.IsMatch(phoneNumber, @"^1\(\d\d\d\)\d\d\d\d$"))
        {
            return phoneNumber.Substring(0, 9) + "-" + phoneNumber.Substring(9);
        }

        if (Regex.IsMatch(phoneNumber, @"^1\d\d\d\d\d\d\d\d\d\d$"))
        {
            return phoneNumber.Substring(0, 1) + "(" + phoneNumber.Substring(1, 3) + ")" + phoneNumber.Substring(4, 3) + "-" + phoneNumber.Substring(7);
        }

        if (Regex.IsMatch(phoneNumber, @"^\d\d\d\d\d\d\d\d\d\d$"))
        {
            return "(" + phoneNumber.Substring(0, 3) + ")" + phoneNumber.Substring(3, 3) + "-" + phoneNumber.Substring(6);
        }

        if (!Regex.IsMatch(phoneNumber, @"^\(\d\d\d\)\d\d\d-\d\d\d\d$") || !Regex.IsMatch(phoneNumber, @"^1\(\d\d\d\)\d\d\d-\d\d\d\d$"))
        {
            return ReFormat(phoneNumber);
        }

        return phoneNumber;
    }

    public static string FormatNumbersOnly(string phoneNumber)
    {
        var result = "";
        var count = 0;
        
        for (var i = 0; i < phoneNumber.Length; i++)
        {
            if (count == 2)
            {
                return result;
            }

            if (char.IsNumber(phoneNumber, i))
            {
                result += phoneNumber.Substring(i, 1);
                count = 0;
            }
            else
            {
                count++;
            }
        }

        return result;
    }

    public static string FormatNumbersExactTen(string phoneNumber)
    {
        var result = "";
        
        for (var i = 0; i < phoneNumber.Length; i++)
        {
            if (char.IsNumber(phoneNumber, i))
            {
                if (result == "" && phoneNumber.Substring(i, 1) == "1")
                {
                    continue;
                }

                result += phoneNumber.Substring(i, 1);
            }

            if (result.Length == 10)
            {
                return result;
            }
        }
        
        return "";
    }
}