namespace OpenDental.ReportingComplex
{
    public class Section
    {
        public Section(AreaSectionType type, int height)
        {
            SectionType = type;
            Height = height;
        }

        public int Height { get; set; }
        public AreaSectionType SectionType { get; }
    }

    public enum AreaSectionType
    {
        None,
        ReportHeader,
        PageHeader,
        GroupTitle,
        GroupHeader,
        Detail,
        GroupFooter,
        PageFooter,
        ReportFooter,
        Query
    }
}