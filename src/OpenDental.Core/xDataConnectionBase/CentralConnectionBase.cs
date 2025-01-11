namespace DataConnectionBase;

public class CentralConnectionBase
{
    ///<summary>Primary key.</summary>
    public long CentralConnectionNum;

    ///<summary>If direct db connection.  Can be ip address.</summary>
    public string ServerName;

    ///<summary>If direct db connection.</summary>
    public string DatabaseName;

    ///<summary>If direct db connection.</summary>
    public string MySqlUser;

    ///<summary>If direct db connection.  Symmetrically encrypted.</summary>
    public string MySqlPassword;
    
    ///<summary>When being used by ConnectionStore xml file, must deserialize to a ConnectionNames enum value. Otherwise just used as a generic notes field.</summary>
    public string Note;

    ///<summary>0-based.</summary>
    public int ItemOrder;

    ///<summary>Contains the most recent information about this connection.  OK if no problems, version information if version mismatch, nothing for not checked, and OFFLINE if previously couldn't connect.</summary>
    public string ConnectionStatus;

    ///<summary>If set to True, display clinic breakdown in reports, else only show practice totals.</summary>
    public bool HasClinicBreakdownReports;

    ///<summary>Set when reading from the config file. Not an actual DB column.</summary>
    public bool IsAutomaticLogin;

    ///<summary>This is a helper variable used for Reports. If we want to start supporting connection string for the Reporting Server, we need to add this as a db column. This was needed for the scenario where a customer connected to OD using a connection string.</summary>
    public string ConnectionString;

    ///<summary>Helper variable to keep track of the password hash that was passed in as a command line argument. This is necessary for automatically logging eCW users in when they are utilizing the Middle Tier.</summary>
    public string OdPassHash;
}