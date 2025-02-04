using System.Collections.Generic;

namespace OpenDental.Chart;

public class Face
{
    public List<int> IndexList = [];

    public override string ToString()
    {
        var result = "";
        
        for (var i = 0; i < IndexList.Count; i++)
        {
            if (i > 0)
            {
                result += ",";
            }

            result += IndexList[i].ToString();
        }

        return result;
    }

    public Face Copy()
    {
        return new Face
        {
            IndexList = [..IndexList]
        };
    }
}