using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using CDT;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.WebTypes.Shared.XWeb;

namespace OpenDentBusiness;

public class CentralConnections
{
    public static List<string> GetDatabases(CentralConnection centralConnection)
    {
        Logger.LogToPath("GetDatabases", LogPath.Startup, LogPhase.Start);
        if (centralConnection.ServerName == "")
        {
            Logger.LogToPath("GetDatabases - Blank ServerName", LogPath.Startup, LogPhase.End);
            return new List<string>();
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

    public static void TryToConnect(CentralConnection centralConnection, string connectionString = "", bool noShowOnStartup = false, List<string> listAdminCompNames = null, bool isCommandLineArgs = false, bool useDynamicMode = false, bool allowAutoLogin = true)
    {
        Logger.LogToPath("DataConnection.SetDb", LogPath.Startup, LogPhase.Start);
        if (connectionString.Length > 0)
            DataConnection.SetDb(connectionString);
        else
            //Password could be plain text password from the Password field of the config file, the decrypted password from the MySQLPassHash field
            //of the config file, or password entered by the user and can be blank (empty string) in all cases
            DataConnection.SetDb(centralConnection.ServerName, centralConnection.DatabaseName, centralConnection.MySqlUser
                , centralConnection.MySqlPassword, false, centralConnection.SslCA);
        Logger.LogToPath("DataConnection.SetDb", LogPath.Startup, LogPhase.End);
        //Only gets this far if we have successfully connected, thus, saving connection settings is appropriate.
        TrySaveConnectionSettings(centralConnection, connectionString, noShowOnStartup, listAdminCompNames,
            isCommandLineArgs, useDynamicMode, allowAutoLogin);
    }
    
    public static bool TrySaveConnectionSettings(CentralConnection centralConnection, string connectionString = "", bool noShowOnStartup = false, List<string> listAdminCompNames = null, bool isCommandLineArgs = false, bool useDynamicMode = false, bool allowAutoLogin = true)
    {
        try
        {
            Logger.LogToPath("TrySaveConnectionSettings", LogPath.Startup, LogPhase.Start);
            //The parameters passed in might have misleading information (like noShowOnStartup) if they were comprised from command line arguments.
            //Non-command line settings within the FreeDentalConfig.xml need to be preserved when command line arguments are used.
            if (isCommandLineArgs)

                return false;

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
            xmlWriter.WriteString("MySql");
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
            
            #endregion
        }
        catch (Exception)
        {
            centralConnection.ServerName = "localhost";
            centralConnection.DatabaseName = "opendental";
            centralConnection.MySqlUser = "root";
        }

        chooseDatabaseInfo.AllowAutoLogin = allowAutoLogin;
        chooseDatabaseInfo.CentralConnectionCur = centralConnection;
        chooseDatabaseInfo.ConnectionString = connectionString;
        chooseDatabaseInfo.ListAdminCompNames = listAdminCompNames;
        chooseDatabaseInfo.NoShow = yNNoShow;
        chooseDatabaseInfo.UseDynamicMode = useDynamicMode;
        return chooseDatabaseInfo;
    }
}

#region ChooseDatabaseInfo

public class ChooseDatabaseInfo
{
    public bool AllowAutoLogin = true;
    public CentralConnection CentralConnectionCur = new();
    public string ConnectionString = "";
    public bool IsAccessedFromMainMenu;
    public List<string> ListAdminCompNames = new();
    public YN NoShow;
    public bool UseDynamicMode;
    
    public static ChooseDatabaseInfo GetChooseDatabaseInfoFromConfig(string webServiceUri = "", YN webServiceIsEcw = YN.Unknown, string odUser = "", string serverName = "", string databaseName = "", string mySqlUser = "", string mySqlPassword = "", string mySqlPassHash = "", YN yNNoShow = YN.Unknown, string odPassword = "", bool useDynamicMode = false, string odPassHash = "")
    {
        var chooseDatabaseInfo = new ChooseDatabaseInfo();

        if (string.IsNullOrEmpty(databaseName))
        {
            Logger.LogToPath("GetChooseDatabaseConnectionSettings", LogPath.Startup, LogPhase.Start);
            chooseDatabaseInfo = CentralConnections.GetChooseDatabaseConnectionSettings();
            Logger.LogToPath("GetChooseDatabaseConnectionSettings", LogPath.Startup, LogPhase.End);
        }
        
        #region Command Line Arguements
        
        if (!string.IsNullOrEmpty(odPassHash)) chooseDatabaseInfo.CentralConnectionCur.OdPassHash = odPassHash;
        if (serverName != "") chooseDatabaseInfo.CentralConnectionCur.ServerName = serverName;
        if (databaseName != "") chooseDatabaseInfo.CentralConnectionCur.DatabaseName = databaseName;
        if (mySqlUser != "") chooseDatabaseInfo.CentralConnectionCur.MySqlUser = mySqlUser;
        if (mySqlPassword != "") chooseDatabaseInfo.CentralConnectionCur.MySqlPassword = mySqlPassword;
        if (mySqlPassHash != "")
        {
            Class1.Decrypt(mySqlPassHash, out var decryptedPwd);
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