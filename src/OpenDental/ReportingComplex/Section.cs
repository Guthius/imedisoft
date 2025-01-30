namespace OpenDental.ReportingComplex;

public class Section(AreaSectionType type, int height)
{
    public int Height { get; set; } = height;
    public AreaSectionType SectionType { get; } = type;
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