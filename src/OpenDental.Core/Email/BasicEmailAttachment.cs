namespace OpenDentBusiness.Email;

public class BasicEmailAttachment(string fullPath, string displayedFilename)
{
    public readonly string FullPath = fullPath;
    public readonly string DisplayedFilename = displayedFilename;
}