using System;
using System.Linq;
using System.Text;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class CreditCardUtils
{
    public static string GetCardType(string ccNum)
    {
        if (string.IsNullOrEmpty(ccNum))
        {
            return "";
        }

        ccNum = StripNonDigits(ccNum);
        if (ccNum.StartsWith("4"))
        {
            return "VISA";
        }

        if (ccNum.StartsWith("5"))
        {
            return "MASTERCARD";
        }

        if (ccNum.StartsWith("34") || ccNum.StartsWith("37"))
        {
            return "AMEX";
        }

        if (ccNum.StartsWith("30") || ccNum.StartsWith("36") || ccNum.StartsWith("38"))
        {
            return "DINERS";
        }

        return ccNum.StartsWith("6011") ? "DISCOVER" : "";
    }

    public static string StripNonDigits(string str)
    {
        return StripNonDigits(str, []);
    }

    public static string StripNonDigits(string str, char[] allowed)
    {
        if (str == null)
        {
            return null;
        }

        var stringBuilder = new StringBuilder(str);

        StripNonDigits(stringBuilder, allowed);

        return stringBuilder.ToString();
    }

    public static void StripNonDigits(StringBuilder stringBuilder, char[] allowed)
    {
        for (var i = 0; i < stringBuilder.Length; i++)
        {
            if (char.IsDigit(stringBuilder[i]) || ContainsCharacter(stringBuilder[i], allowed))
            {
                continue;
            }

            stringBuilder.Remove(i, 1);
            i--;
        }
    }

    public static bool ContainsCharacter(char ch, char[] search)
    {
        return search.Any(x => ch == x);
    }

    public static string AddClinicToReceipt(long clinicNum)
    {
        var result = "";

        var clinic = Clinics.GetClinic(clinicNum);

        if (clinic is not null)
        {
            if (clinic.Description.Length > 0)
            {
                result += clinic.Description + Environment.NewLine;
            }

            if (clinic.AddressLine1.Length > 0)
            {
                result += clinic.AddressLine1 + Environment.NewLine;
            }

            if (clinic.AddressLine2.Length > 0)
            {
                result += clinic.AddressLine2 + Environment.NewLine;
            }

            if (clinic.City.Length > 0 || clinic.State.Length > 0 || clinic.Zip.Length > 0)
            {
                result += clinic.City + ", " + clinic.State + " " + clinic.Zip + Environment.NewLine;
            }

            switch (clinic.PhoneNumber.Length)
            {
                case 10:
                    result += TelephoneNumbers.ReFormat(clinic.PhoneNumber) + Environment.NewLine;
                    break;

                case > 0:
                    result += clinic.PhoneNumber + Environment.NewLine;
                    break;
            }
        }

        result += Environment.NewLine;

        return result;
    }
}