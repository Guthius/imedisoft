using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using CDT;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.WebTypes.Shared.XWeb;
using PasswordVaultWrapper;

namespace OpenDentBusiness;

public class CentralConnections
{
    public static List<string> GetComputerNames()
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix) return new List<string>();
        try
        {
            Logger.LogToPath("Delete tempCompNames.txt", LogPath.Startup, LogPhase.Start);
            File.Delete(ODFileUtils.CombinePaths(Application.StartupPath, "tempCompNames.txt"));
            Logger.LogToPath("Delete tempCompNames.txt", LogPath.Startup, LogPhase.End);
            var listStrings = new List<string>();
            //string myAdd=Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();//obsolete
            Logger.LogToPath("Dns.GetHostEntry", LogPath.Startup, LogPhase.Start);
            var myAdd = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            Logger.LogToPath("Dns.GetHostEntry", LogPath.Startup, LogPhase.End);
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"C:\WINDOWS\system32\cmd.exe"; //Path for the cmd prompt
            processStartInfo.Arguments = "/c net view > tempCompNames.txt"; //Arguments for the command prompt
            //"/c" tells it to run the following command which is "net view > tempCompNames.txt"
            //"net view" lists all the computers on the network
            //" > tempCompNames.txt" tells dos to put the results in a file called tempCompNames.txt
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden; //Hide the window
            Logger.LogToPath("CMD net view", LogPath.Startup, LogPhase.Start);
            Process.Start(processStartInfo);
            StreamReader streamReader = null; //Gets disposed in close() call at end of method
            var filename = ODFileUtils.CombinePaths(Application.StartupPath, "tempCompNames.txt");
            Thread.Sleep(200); //sleep for 1/5 second
            if (!File.Exists(filename))
            {
                Logger.LogToPath("CMD net view - FileNotExist", LogPath.Startup, LogPhase.End);
                return new List<string>();
            }

            try
            {
                streamReader = new StreamReader(filename);
            }
            catch (Exception)
            {
                Logger.LogToPath("CMD net view - StreamReader Exception", LogPath.Startup, LogPhase.End);
                return new List<string>();
            }

            if (streamReader.Peek() == -1)
            {
                //If the file is empty, return.
                Logger.LogToPath("CMD net view - Empty File", LogPath.Startup, LogPhase.End);
                return new List<string>();
            }

            while (!streamReader.ReadLine().StartsWith("--"))
            {
                //The line just before the data looks like: --------------------------
            }

            var line = "";
            listStrings.Add("localhost");
            while (true)
            {
                line = streamReader.ReadLine();
                if (line.StartsWith("The")) //cycle until we reach,"The command completed successfully."
                    break;
                line = line.Split(char.Parse(" "))[0]; // Split the line after the first space
                // Normally, in the file it lists it like this
                // \\MyComputer                 My Computer's Description
                // Take off the slashes, "\\MyComputer" to "MyComputer"
                listStrings.Add(line.Substring(2, line.Length - 2));
            }

            streamReader.Close();
            Logger.LogToPath("CMD net view", LogPath.Startup, LogPhase.End);
            Logger.LogToPath("Delete tempCompNames.txt cleanup", LogPath.Startup, LogPhase.Start);
            File.Delete(ODFileUtils.CombinePaths(Application.StartupPath, "tempCompNames.txt"));
            Logger.LogToPath("Delete tempCompNames.txt cleanup", LogPath.Startup, LogPhase.End);
            return listStrings;
        }
        catch (Exception)
        {
            //it will always fail if not WinXP
            Logger.LogToPath("CMD net view - Generic Exception", LogPath.Startup, LogPhase.End);
            return new List<string>();
        }
    }
    
    public static List<string> GetDatabases(CentralConnection centralConnection, DatabaseType databaseType)
    {
        Logger.LogToPath("GetDatabases", LogPath.Startup, LogPhase.Start);
        if (centralConnection.ServerName == "")
        {
            Logger.LogToPath("GetDatabases - Blank ServerName", LogPath.Startup, LogPhase.End);
            return new List<string>();
        }

        if (databaseType != DatabaseType.MySql)
        {
            Logger.LogToPath("GetDatabases - Not MySQL", LogPath.Startup, LogPhase.End);
            return new List<string>(); //because SHOW DATABASES won't work
        }

        try
        {
            Logger.LogToPath("DataConnection", LogPath.Startup, LogPhase.Start);
            DataConnection dataConnection;
            //use the one table that we know exists
            if (centralConnection.MySqlUser == "")
                dataConnection = new DataConnection(centralConnection.ServerName, "information_schema", "root", centralConnection.MySqlPassword);
            else
                dataConnection = new DataConnection(centralConnection.ServerName, "information_schema", centralConnection.MySqlUser, centralConnection.MySqlPassword);
            Logger.LogToPath("DataConnection", LogPath.Startup, LogPhase.End);
            Logger.LogToPath("SHOW DATABASES", LogPath.Startup, LogPhase.Start);
            var command = "SHOW DATABASES";
            //if this next step fails, table will simply have 0 rows
            var table = dataConnection.GetTable(command);
            Logger.LogToPath("SHOW DATABASES", LogPath.Startup, LogPhase.End);
            var listNames = new List<string>();
            for (var i = 0; i < table.Rows.Count; i++) listNames.Add(table.Rows[i][0].ToString());
            Logger.LogToPath("GetDatabases", LogPath.Startup, LogPhase.End);
            return listNames;
        }
        catch (Exception)
        {
            Logger.LogToPath("GetDatabases - Generic Exception", LogPath.Startup, LogPhase.End);
            return new List<string>();
        }
    }

    public static void TryToConnect(CentralConnection centralConnection, DatabaseType databaseType, string connectionString = "", bool noShowOnStartup = false, List<string> listAdminCompNames = null, bool isCommandLineArgs = false, bool useDynamicMode = false, bool allowAutoLogin = true)
    {
        Logger.LogToPath("DataConnection.SetDb", LogPath.Startup, LogPhase.Start);
        var dataConnection = new DataConnection();
        if (connectionString.Length > 0)
            DataConnection.SetDb(connectionString);
        else
            //Password could be plain text password from the Password field of the config file, the decrypted password from the MySQLPassHash field
            //of the config file, or password entered by the user and can be blank (empty string) in all cases
            DataConnection.SetDb(centralConnection.ServerName, centralConnection.DatabaseName, centralConnection.MySqlUser
                , centralConnection.MySqlPassword, false, centralConnection.SslCA);
        Logger.LogToPath("DataConnection.SetDb", LogPath.Startup, LogPhase.End);
        //Only gets this far if we have successfully connected, thus, saving connection settings is appropriate.
        TrySaveConnectionSettings(centralConnection, databaseType, connectionString, noShowOnStartup, listAdminCompNames,
            isCommandLineArgs, useDynamicMode, allowAutoLogin);
    }
    
    public static bool TrySaveConnectionSettings(CentralConnection centralConnection, DatabaseType databaseType, string connectionString = "", bool noShowOnStartup = false, List<string> listAdminCompNames = null, bool isCommandLineArgs = false, bool useDynamicMode = false, bool allowAutoLogin = true)
    {
        try
        {
            Logger.LogToPath("TrySaveConnectionSettings", LogPath.Startup, LogPhase.Start);
            //The parameters passed in might have misleading information (like noShowOnStartup) if they were comprised from command line arguments.
            //Non-command line settings within the FreeDentalConfig.xml need to be preserved when command line arguments are used.
            if (isCommandLineArgs)
                //Updating the freedentalconfig.xml file when connecting via command line arguments causes issues for users
                //who prefer to have a desktop icon pointing to their main database and additional icons for other databases (popular amongst CEMT users).
                return false;
            /**** The following code will be reintroduced in the near future.  Leaving as a comment on purpose. ****
                //Override all variables that are NOT valid command line arguments with their current values within the FreeDentalConfig.xml
                //This will preserve the values that should stay the same (e.g. noShowOnStartup is NOT a command line arg and will always be true).
                CentralConnection centralConnectionFile;
                string connectionStringFile;
                YN noShowFile;
                DatabaseType dbTypeFile;
                List<string> listAdminCompNamesFile;
                GetChooseDatabaseConnectionSettings(out centralConnectionFile,out connectionStringFile,out noShowFile,out dbTypeFile
                    ,out listAdminCompNamesFile,out useDynamicMode);
                //Since command line args are being used, override any variables that are NOT allowed to be passed in via the command line.
                #region FreeDentalConfig Nodes That Do Not Have a Corresponding Command Line Arg
                switch(noShowFile) {//config node: <NoShowOnStartup>
                    case YN.Yes:
                        noShowOnStartup=true;
                        break;
                    case YN.No:
                    case YN.Unknown:
                        //Either NoShowOnStartup was not found or was explicitly set to no.
                        noShowOnStartup=false;
                        break;
                }
                listAdminCompNames=listAdminCompNamesFile;//config node: <AdminCompNames>
                connectionString=connectionStringFile;//config node: <ConnectionString>
                centralConnection.IsAutomaticLogin=centralConnectionFile.IsAutomaticLogin;//config node: <UsingAutoLogin>
                #endregion
                ********************************************************************************************************/
            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "    ";
            using var xmlWriter = XmlWriter.Create(ODFileUtils.CombinePaths(Application.StartupPath, "FreeDentalConfig.xml"), xmlWriterSettings);
            xmlWriter.WriteStartElement("ConnectionSettings");
            if (!allowAutoLogin)
                //Only add if it was added before.
                xmlWriter.WriteElementString("AllowAutoLogin", "False");
            if (connectionString != "")
            {
                xmlWriter.WriteStartElement("ConnectionString");
                xmlWriter.WriteString(connectionString);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteStartElement("DatabaseConnection");
            xmlWriter.WriteStartElement("ComputerName");
            xmlWriter.WriteString(centralConnection.ServerName);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("Database");
            xmlWriter.WriteString(centralConnection.DatabaseName);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("User");
            xmlWriter.WriteString(centralConnection.MySqlUser);
            xmlWriter.WriteEndElement();
            string encryptedPwd;
            Class1.Encrypt(centralConnection.MySqlPassword, out encryptedPwd); //sets encryptedPwd ot value or null
            xmlWriter.WriteStartElement("Password");
            //If encryption fails, write plain text password to xml file; maintains old behavior.
            xmlWriter.WriteString(string.IsNullOrEmpty(encryptedPwd) ? centralConnection.MySqlPassword : "");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("MySQLPassHash");
            xmlWriter.WriteString(encryptedPwd ?? "");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("NoShowOnStartup");
            if (noShowOnStartup)
                xmlWriter.WriteString("True");
            else
                xmlWriter.WriteString("False");
            xmlWriter.WriteEndElement();
            if (!string.IsNullOrEmpty(centralConnection.SslCA))
            {
                xmlWriter.WriteStartElement("SslCa");
                xmlWriter.WriteString(centralConnection.SslCA);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteStartElement("DatabaseType");
            if (databaseType == DatabaseType.MySql)
                xmlWriter.WriteString("MySql");
            else
                xmlWriter.WriteString("Oracle");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("UseDynamicMode");
            xmlWriter.WriteString(useDynamicMode.ToString());
            xmlWriter.WriteEndElement();
            if (XWebs.UseXWebTestGateway)
            {
                xmlWriter.WriteStartElement("UseXWebTestGateway");
                xmlWriter.WriteString("True");
                xmlWriter.WriteEndElement();
            }

            if (listAdminCompNames != null && listAdminCompNames.Count > 0)
            {
                xmlWriter.WriteStartElement("AdminCompNames");
                for (var i = 0; i < listAdminCompNames.Count; i++)
                {
                    xmlWriter.WriteStartElement("CompName");
                    xmlWriter.WriteString(listAdminCompNames[i]);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.Close();
        }
        catch (Exception ex)
        {
            Logger.LogToPath("TrySaveConnectionSettings failed: " + ex.Message, LogPath.Startup, LogPhase.Unspecified);
            return false;
        }
        finally
        {
            Logger.LogToPath("TrySaveConnectionSettings", LogPath.Startup, LogPhase.End);
        }

        return true;
    }
    
    public static ChooseDatabaseInfo GetChooseDatabaseConnectionSettings()
    {
        var chooseDatabaseInfo = new ChooseDatabaseInfo();
        var centralConnection = new CentralConnection();
        var connectionString = "";
        var yNNoShow = YN.Unknown;
        var databaseType = DatabaseType.MySql;
        var listAdminCompNames = new List<string>();
        var useDynamicMode = false;
        var allowAutoLogin = true;
        var xmlPath = ODFileUtils.CombinePaths(Application.StartupPath, "FreeDentalConfig.xml");

        #region Permission Check

        //Improvement should be made here to avoid requiring admin priv.
        //Search path should be something like this:
        //1. /home/username/.opendental/config.xml (or corresponding user path in Windows)
        //2. /etc/opendental/config.xml (or corresponding machine path in Windows) (should be default for new installs) 
        //3. Application Directory/FreeDentalConfig.xml (read-only if user is not admin)
        if (!File.Exists(xmlPath))
        {
            FileStream fileStream;
            try
            {
                fileStream = File.Create(xmlPath);
            }
            catch (Exception)
            {
                //No translation right here because we typically do not have a database connection yet.
                throw new ODException("The very first time that the program is run, it must be run as an Admin.  "
                                      + "If using Vista, right click, run as Admin.");
            }

            fileStream.Close();
        }

        #endregion

        var xmlDocument = new XmlDocument();
        try
        {
            xmlDocument.Load(xmlPath);
            var xPathNavigator = xmlDocument.CreateNavigator();
            XPathNavigator xPathNavigator2;

            #region Nodes with No UI

            //Always look for these settings first in order to always preserve them correctly.
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//AdminCompNames");
            if (xPathNavigator2 != null)
            {
                listAdminCompNames.Clear(); //this method gets called more than once
                var xPathNavigatorIterator = xPathNavigator2.SelectChildren(XPathNodeType.All);
                for (var i = 0; i < xPathNavigatorIterator.Count; i++)
                {
                    xPathNavigatorIterator.MoveNext();
                    listAdminCompNames.Add(xPathNavigatorIterator.Current.Value); //Add this computer name to the list.
                }
            }

            //See if there's a UseXWebTestGateway
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//UseXWebTestGateway");
            if (xPathNavigator2 != null) XWebs.UseXWebTestGateway = xPathNavigator2.Value.ToLower() == "true";
            //See if there's a AllowAutoLogin node
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//AllowAutoLogin");
            if (xPathNavigator2 != null && xPathNavigator2.Value.ToLower() == "false")
                //Node must be specifically set to false to change the allowAutoLogin bool.
                allowAutoLogin = false;

            #endregion

            #region Nodes from Choose Database Window

            #region Nodes with No Group Box

            //Database Type
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//DatabaseType");
            databaseType = DatabaseType.MySql;
            //ConnectionString
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//ConnectionString");
            if (xPathNavigator2 != null)
                //If there is a ConnectionString, then use it.
                connectionString = xPathNavigator2.Value;
            //UseDynamicMode
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//UseDynamicMode");
            if (xPathNavigator2 != null)
                //If there is a node, take in its value
                useDynamicMode = SIn.Bool(xPathNavigator2.Value);

            #endregion

            #region Connection Settings Group Box

            //See if there's a DatabaseConnection
            xPathNavigator2 = xPathNavigator.SelectSingleNode("//DatabaseConnection");
            if (xPathNavigator2 != null)
            {
                //If there is a DatabaseConnection, then use it.
                centralConnection.ServerName = xPathNavigator2.SelectSingleNode("ComputerName").Value;
                centralConnection.DatabaseName = xPathNavigator2.SelectSingleNode("Database").Value;
                centralConnection.MySqlUser = xPathNavigator2.SelectSingleNode("User").Value;
                centralConnection.MySqlPassword = xPathNavigator2.SelectSingleNode("Password").Value;
                centralConnection.SslCA = xPathNavigator2.SelectSingleNode("SslCa")?.Value ?? "";
                var xPathNavigatorEncryptedPwdNode = xPathNavigator2.SelectSingleNode("MySQLPassHash");
                //If the Password node is empty, but there is a value in the MySQLPassHash node, decrypt the node value and use that instead
                string _decryptedPwd;
                if (centralConnection.MySqlPassword == ""
                    && xPathNavigatorEncryptedPwdNode != null
                    && xPathNavigatorEncryptedPwdNode.Value != ""
                    && Class1.Decrypt(xPathNavigatorEncryptedPwdNode.Value, out _decryptedPwd))
                    //decrypted value could be an empty string, which means they don't have a password set, so textPassword will be an empty string
                    centralConnection.MySqlPassword = _decryptedPwd;
                var xPathNavigatorNoShow = xPathNavigator2.SelectSingleNode("NoShowOnStartup");
                if (xPathNavigatorNoShow != null)
                {
                    if (xPathNavigatorNoShow.Value == "True")
                        yNNoShow = YN.Yes;
                    else
                        yNNoShow = YN.No;
                }
            }

            #endregion

            #region Connect to Middle Tier Group Box

            xPathNavigator2 = xPathNavigator.SelectSingleNode("//ServerConnection");
            /* example:
            <ServerConnection>
                <URI>http://server/OpenDentalServer/ServiceMain.asmx</URI>
                <UsingEcw>True</UsingEcw>
            </ServerConnection>
            */
            if (xPathNavigator2 != null)
            {
                //If there is a ServerConnection, then use it.
                centralConnection.ServiceURI = xPathNavigator2.SelectSingleNode("URI").Value;
                var xPathNavigatorEcw = xPathNavigator2.SelectSingleNode("UsingEcw");
                if (xPathNavigatorEcw != null && xPathNavigatorEcw.Value == "True")
                {
                    yNNoShow = YN.Yes;
                    centralConnection.WebServiceIsEcw = true;
                }

                var xPathNavigatorAutoLogin = xPathNavigator2.SelectSingleNode("UsingAutoLogin");
                //Retrieve the user from the Windows password vault for the current ServiceURI that was last to successfully single sign on.
                //If credentials are found then log the user in.  This is safe to do because the password vault is unique per Windows user and workstation.
                //There is code elsewhere that will handle password vault management (only storing the last valid single sign on per ServiceURI).
                //allowAutoLogin defaults to true unless the office specifically set it to false.
                if (xPathNavigatorAutoLogin != null && xPathNavigatorAutoLogin.Value == "True" && allowAutoLogin)
                {
                    if (!WindowsPasswordVaultWrapper.TryRetrieveUserName(centralConnection.ServiceURI, out centralConnection.OdUser)) centralConnection.OdUser = xPathNavigator2.SelectSingleNode("User").Value; //No username found.  Use the User in FreeDentalConfig (preserve old behavior).
                    //Get the user's password from Windows Credential Manager
                    try
                    {
                        centralConnection.OdPassword =
                            WindowsPasswordVaultWrapper.RetrievePassword(centralConnection.ServiceURI, centralConnection.OdUser);
                        //Must set this so FormChooseDatabase() does not launch
                        yNNoShow = YN.Yes;
                        centralConnection.IsAutomaticLogin = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            #endregion

            #endregion
        }
        catch (Exception)
        {
            //Common error: root element is missing
            centralConnection.ServerName = "localhost";
            if (ODBuild.IsTrial())
                centralConnection.DatabaseName = "demo";
            else
                centralConnection.DatabaseName = "opendental";
            centralConnection.MySqlUser = "root";
        }

        chooseDatabaseInfo.AllowAutoLogin = allowAutoLogin;
        chooseDatabaseInfo.CentralConnectionCur = centralConnection;
        chooseDatabaseInfo.ConnectionString = connectionString;
        chooseDatabaseInfo.DatabaseType = databaseType;
        //IsAccessibleFromMainMenu is set in FormOpenDental
        chooseDatabaseInfo.ListAdminCompNames = listAdminCompNames;
        chooseDatabaseInfo.NoShow = yNNoShow;
        chooseDatabaseInfo.UseDynamicMode = useDynamicMode;
        return chooseDatabaseInfo;
    }
}

#region ChooseDatabaseInfo

/// <summary>
///     A storage class that contains database connection information and other information that can show within the
///     Choose Database window and even some information that is only stored within the FreeDentalConfig.xml and has no UI
///     but needs to be preserved.
/// </summary>
public class ChooseDatabaseInfo
{
    ///<summary>Defaults to true. Allows the user to choose whether or not they can select 'Log me in automatically.'</summary>
    public bool AllowAutoLogin = true;

    ///<summmary></summmary>
    public CentralConnection CentralConnectionCur = new();

    
    public string ConnectionString = "";

    
    public DatabaseType DatabaseType;

    ///<summary>This is used when selecting File>Choose Database.  It will behave slightly differently.</summary>
    public bool IsAccessedFromMainMenu;

    ///<summary>Stored so that they don't get deleted when re-writing the FreeDentalConfig file.</summary>
    public List<string> ListAdminCompNames = new();

    /// <summary>
    ///     When silently running GetConfig() without showing UI, this gets set to true if either NoShowOnStartup or
    ///     UsingEcw is found in config file.
    /// </summary>
    public YN NoShow;

    /// <summary>
    ///     Indicates whether the user is using dynamic mode. That is, when connecting to a database of a lower version, they
    ///     will
    ///     download the version from the server and run that instead of upgrading/downgrading their own client.
    /// </summary>
    public bool UseDynamicMode;

    /// <summary>
    ///     Every optional parameter provided should coincide with a command line argument. The values passed in will
    ///     typically override any settings loaded in from the config file. Passing in a value for webServiceUri or
    ///     databaseName will cause the config file to not even be considered.
    /// </summary>
    public static ChooseDatabaseInfo GetChooseDatabaseInfoFromConfig(string webServiceUri = "", YN webServiceIsEcw = YN.Unknown, string odUser = ""
        , string serverName = "", string databaseName = "", string mySqlUser = "", string mySqlPassword = "", string mySqlPassHash = "", YN yNNoShow = YN.Unknown
        , string odPassword = "", bool useDynamicMode = false, string odPassHash = "")
    {
        var chooseDatabaseInfo = new ChooseDatabaseInfo();
        //Even if we are passed a URI as a command line argument we still need to check the FreeDentalConfig file for middle tier automatic log in.
        //The only time we do not need to do that is if a direct DB has been passed in.
        if (string.IsNullOrEmpty(databaseName))
        {
            Logger.LogToPath("GetChooseDatabaseConnectionSettings", LogPath.Startup, LogPhase.Start);
            chooseDatabaseInfo = CentralConnections.GetChooseDatabaseConnectionSettings();
            Logger.LogToPath("GetChooseDatabaseConnectionSettings", LogPath.Startup, LogPhase.End);
        }

        //Command line args should always trump settings from the config file.

        #region Command Line Arguements

        if (webServiceUri != "") //if a URI was passed in
            chooseDatabaseInfo.CentralConnectionCur.ServiceURI = webServiceUri;
        if (webServiceIsEcw != YN.Unknown) chooseDatabaseInfo.CentralConnectionCur.WebServiceIsEcw = webServiceIsEcw == YN.Yes ? true : false;
        if (odUser != "") chooseDatabaseInfo.CentralConnectionCur.OdUser = odUser;
        if (odPassword != "") chooseDatabaseInfo.CentralConnectionCur.OdPassword = odPassword;
        if (!string.IsNullOrEmpty(odPassHash)) chooseDatabaseInfo.CentralConnectionCur.OdPassHash = odPassHash;
        if (serverName != "") chooseDatabaseInfo.CentralConnectionCur.ServerName = serverName;
        if (databaseName != "") chooseDatabaseInfo.CentralConnectionCur.DatabaseName = databaseName;
        if (mySqlUser != "") chooseDatabaseInfo.CentralConnectionCur.MySqlUser = mySqlUser;
        if (mySqlPassword != "") chooseDatabaseInfo.CentralConnectionCur.MySqlPassword = mySqlPassword;
        if (mySqlPassHash != "")
        {
            string decryptedPwd;
            Class1.Decrypt(mySqlPassHash, out decryptedPwd);
            chooseDatabaseInfo.CentralConnectionCur.MySqlPassword = decryptedPwd;
        }

        if (yNNoShow != YN.Unknown) chooseDatabaseInfo.NoShow = yNNoShow;
        if (odUser != "" && odPassword != "") chooseDatabaseInfo.NoShow = YN.Yes;
        //If they are overridding to say to use dynamic mode.
        if (useDynamicMode) chooseDatabaseInfo.UseDynamicMode = useDynamicMode;

        #endregion

        return chooseDatabaseInfo;
    }
}

#endregion ChooseDatabaseInfo