using System;
using System.Text;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ReplaceTags
{
    public static void ReplaceOneTag(StringBuilder stringBuilder, string tagToReplace, string replaceWith, bool isHtmlEmail)
    {
        if (isHtmlEmail)
        {
            //Because we are sending emails as HTML, we need to escape characters that can cause our email text to become invalid HTML.
            replaceWith = (replaceWith ?? "").Replace(">", "&>").Replace("<", "&<");
        }

        //Note: RegReplace is case insensitive by default.
        StringTools.RegReplace(stringBuilder, "\\" + tagToReplace, replaceWith ?? "");
    }

    public static string ReplaceUser(string message, Userod userod)
    {
        string retVal = message;
        string userNameF = "";
        string userNameL = "";
        if (userod.ProvNum != 0)
        {
            Provider prov = Providers.GetProv(userod.ProvNum);
            userNameF = prov.FName;
            userNameL = prov.LName;
        }
        else if (userod.EmployeeNum != 0)
        {
            Employee emp = Employees.GetEmp(userod.EmployeeNum);
            userNameF = emp.FName;
            userNameL = emp.LName;
        }

        retVal = retVal.Replace("[UserNameF]", userNameF);
        retVal = retVal.Replace("[UserNameL]", userNameL);
        retVal = retVal.Replace("[UserNameFL]", Patients.GetNameFL(userNameL, userNameF, "", ""));
        return retVal;
    }

    public static string ReplaceMisc(string message)
    {
        string retVal = message;
        retVal = retVal.Replace("[CurrentMonth]", DateTime.Today.ToString("MMMM"));
        return retVal;
    }
}