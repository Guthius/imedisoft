using System.Collections;
using System.Linq;

namespace OpenDental.ReportingComplex
{
    public class ReportObjectCollection : CollectionBase
    {
        public ReportObject this[int index] => (ReportObject) List[index];

        public ReportObject this[string name]
        {
            get
            {
                return List.Cast<ReportObject>().FirstOrDefault(ro => ro.Name == name);
            }
        }
        
        public int Add(ReportObject value)
        {
            return List.Add(value);
        }

        public int IndexOf(ReportObject value)
        {
            return List.IndexOf(value);
        }
        
        public void Insert(int index, ReportObject value)
        {
            List.Insert(index, value);
        }
    }
}