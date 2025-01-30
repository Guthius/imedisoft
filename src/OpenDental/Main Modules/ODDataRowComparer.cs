using System.Collections.Generic;
using System.Data;
using OpenDentBusiness;

namespace OpenDental;

public class ODDataRowComparer : IEqualityComparer<DataRow>
{
    public bool Equals(DataRow dataRowX, DataRow dataRowY)
    {
        var typeX = ChartModules.GetRowType(dataRowX, out var x);
        var typeY = ChartModules.GetRowType(dataRowY, out var y);
        
        return typeX == typeY && x == y;
    }

    public int GetHashCode(DataRow dataRowObj)
    {
        return 0;
    }
}