using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDentBusiness.HL7;

public class FieldHL7
{
    private readonly char[] _delimiters;
    private string _fullText;

    public List<FieldHL7> ListRepeatFields = [];
    public List<ComponentHL7> Components;


    internal FieldHL7(char[] delimiters)
    {
        _delimiters = delimiters;

        _fullText = "";

        Components =
        [
            new ComponentHL7("")
        ];
    }

    public FieldHL7(string fieldText, char[] delimiters)
    {
        _delimiters = delimiters;

        FullText = fieldText;
    }

    public override string ToString()
    {
        return FullText;
    }

    public string FullText
    {
        get
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_fullText);

            foreach (var fieldHL7 in ListRepeatFields)
            {
                stringBuilder.Append(_delimiters[1]);
                stringBuilder.Append(fieldHL7.FullText);
            }

            return stringBuilder.ToString();
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                _fullText = "";
                ListRepeatFields = [];
                Components = [];
                return;
            }

            var repeatSepEscaped = "\\~";
            if (_delimiters is {Length: > 2})
            {
                repeatSepEscaped = _delimiters[2] + _delimiters[1].ToString();
            }

            var repeats = value
                .Replace(repeatSepEscaped, "&126;")
                .Split([_delimiters[1]], StringSplitOptions.None)
                .Select(x => x.Replace("&126;", repeatSepEscaped))
                .ToArray();

            FieldHL7 repeatField = null;

            var componentSepEscaped = "\\^";
            if (_delimiters is {Length: > 2})
            {
                componentSepEscaped = _delimiters[2] + _delimiters[0].ToString();
            }

            for (var r = 0; r < repeats.Length; r++)
            {
                var components = repeats[r]
                    .Replace(componentSepEscaped, "&94;")
                    .Split([_delimiters[0]], StringSplitOptions.None)
                    .Select(x => x.Replace("&94;", componentSepEscaped))
                    .ToArray();

                if (r == 0)
                {
                    _fullText = repeats[r];
                    Components = [];
                }
                else
                {
                    repeatField = new FieldHL7(_delimiters)
                    {
                        _fullText = repeats[r],
                        Components = []
                    };
                }

                foreach (var t in components)
                {
                    var component = new ComponentHL7(t, _delimiters[2]);
                    if (r == 0)
                    {
                        Components.Add(component);
                    }
                    else
                    {
                        repeatField.Components.Add(component);
                    }
                }

                if (r > 0 && repeatField != null)
                {
                    ListRepeatFields.Add(repeatField);
                }
            }
        }
    }

    public string GetComponentVal(int indexPos)
    {
        return indexPos > Components.Count - 1 ? "" : Components[indexPos].ComponentVal;
    }

    public void SetVals(params string[] values)
    {
        if (values.Length == 1)
        {
            FullText = values[0];
            return;
        }

        _fullText = "";
        Components = [];

        for (var i = 0; i < values.Length; i++)
        {
            Components.Add(new ComponentHL7(values[i], _delimiters[2]));

            _fullText += values[i];
            if (i < values.Length - 1)
            {
                _fullText += _delimiters[0];
            }
        }
    }

    public void RepeatVals(params string[] values)
    {
        var field = new FieldHL7(_delimiters);

        field.SetVals(values);

        ListRepeatFields.Add(field);
    }
}