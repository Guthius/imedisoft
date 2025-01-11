namespace OpenDentBusiness;

///<summary>Used to specify where the files are coming from and going when copying.</summary>
public enum FileAtoZSourceDestination
{
    ///<summary>Copying a local file to AtoZ folder. Equivalent to 'upload.'</summary>
    LocalToAtoZ,

    ///<summary>Copying an AtoZ file to a local file. Equivalent to 'download'.</summary>
    AtoZToLocal,

    ///<summary>Copying an AtoZ file to another AtoZ file. Equivalent to 'download' then 'upload'.</summary>
    AtoZToAtoZ
}