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
            replaceWith = (replaceWith ?? "").Replace(">", "&>").Replace("<", "&<");
        }

        StringTools.RegReplace(stringBuilder, "\\" + tagToReplace, replaceWith ?? "");
    }

    public static string ReplaceUser(string message, Userod userod)
    {
        var retVal = message;
        var userNameF = "";
        var userNameL = "";
        if (userod.ProvNum != 0)
        {
            var prov = Providers.GetById(userod.ProvNum);
            userNameF = prov.FirstName;
            userNameL = prov.LastName;
        }
        else if (userod.EmployeeNum != 0)
        {
            var emp = Employees.GetEmp(userod.EmployeeNum);
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
        var retVal = message;
        retVal = retVal.Replace("[CurrentMonth]", DateTime.Today.ToString("MMMM"));
        return retVal;
    }
}