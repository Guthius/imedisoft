using System.Collections.Generic;

namespace OpenDentBusiness;

internal class ToothComparer : IComparer<string>
{
    public int Compare(string toothA, string toothB)
    {
        return Tooth.ToOrdinal(toothA).CompareTo(Tooth.ToOrdinal(toothB));
    }
}