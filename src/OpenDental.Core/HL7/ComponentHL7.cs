using System.Text.RegularExpressions;

namespace OpenDentBusiness.HL7;

public class ComponentHL7
{
    public readonly string ComponentVal;

    public ComponentHL7(string componentVal, char escCh = '\\')
    {
        if (string.IsNullOrWhiteSpace(componentVal))
        {
            ComponentVal = "";
            return;
        }

        var e = escCh.ToString();
        var e2 = e;
        if (escCh == '\\')
        {
            componentVal = componentVal.Replace(e + ".br" + e, "&92;.br&92;");
            e2 += e;
        }

        ComponentVal = Regex.Replace(componentVal, e2 + "(.)?", "$1", RegexOptions.Singleline);
        if (escCh == '\\')
        {
            ComponentVal = ComponentVal.Replace("&92;.br&92;", e + ".br" + e);
        }
    }

    public override string ToString()
    {
        return ComponentVal;
    }
}