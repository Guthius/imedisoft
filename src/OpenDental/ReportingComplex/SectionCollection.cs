using System.Collections;
using System.Linq;

namespace OpenDental.ReportingComplex
{
    public class SectionCollection : CollectionBase
    {
        public Section this[AreaSectionType kind]
        {
            get
            {
                return List.Cast<Section>().FirstOrDefault(section => section.SectionType == kind);
            }
        }
        
        public void Add(Section value)
        {
            if (List.Count == 0)
            {
                List.Add(value);
                return;
            }

            for (var i = 0; i < List.Count; i++)
            {
                if (i == List.Count - 1)
                {
                    List.Insert(i, value);
                    return;
                }

                if ((int) value.SectionType >= (int) ((Section) List[i]).SectionType)
                {
                    continue;
                }
                
                List.Insert(i, value);
                return;
            }
        }

        public bool Contains(AreaSectionType kind)
        {
            return List.Cast<Section>().Any(section => section.SectionType == kind);
        }
    }
}