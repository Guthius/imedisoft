using CodeBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Reflection;
using Microsoft.VisualBasic.Devices;
using System.Management;
using System.Linq;

namespace OpenDentBusiness {

	///<summary>Miscellaneous database functions.</summary>
	public class MiscData {

		///<summary>Gets the current date/Time direcly from the server.  Mostly used to prevent uesr from altering the workstation date to bypass security.</summary>
		public static DateTime GetNowDateTime() {
			if(ODBuild.IsDebug()) {
				return DateTime_.Now;
			}
			string command="SELECT NOW()";
			DataTable table=DataCore.GetTable(command);
			return PIn.DateTime(table.Rows[0][0].ToString());
		}

		///<summary>Gets the current date/Time with milliseconds directly from server.  In Mysql we must query the server until the second rolls over, which may take up to one second.  Used to confirm synchronization in time for EHR.</summary>
		public static DateTime GetNowDateTimeWithMilli() {
			string command;
			string dbtime;
			command="SELECT NOW()"; //Only up to 1 second precision pre-Mysql 5.6.4.  Does not round milliseconds.
			dbtime=DataCore.GetScalar(command);
			int secondInit=PIn.DateTime(dbtime).Second;
			int secondCur;
			//Continue querying server for current time until second changes (milliseconds will be close to 0)
			do {
				dbtime=DataCore.GetScalar(command);
				secondCur=PIn.DateTime(dbtime).Second;
			}
			while(secondInit==secondCur);
			return PIn.DateTime(dbtime);
		}

		///<summary>Returns specific information regarding the current version of Windows that is running.</summary>
		public static string GetOSVersionInfo() {
			string versionInfo="";
			//Utilize the visual basic ComputerInfo class in order to get the most accurate OS version.
			//This is because Environment.OSVersion was always returning a version that represented Windows 8.
			ComputerInfo computerInfo=new ComputerInfo();
			versionInfo=computerInfo.OSFullName+(Environment.Is64BitOperatingSystem ? " 64-bit":" 32-bit");	//OSFullName will be blank on wine
			//This chunk is to get the correct version number.  If we use Environment.OSVersion, it will return an incorrect number.
			//This is because the application isn't manifested to windows 8.1 or 10.  As a result is will always return a version num of 6.2.
			//https://msdn.microsoft.com/en-us/library/windows/desktop/ms724832(v=vs.85).aspx
			try {
				ManagementObjectSearcher mangementQuery=new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
				ManagementObject systemInfo=mangementQuery.Get().Cast<ManagementObject>().FirstOrDefault();
				versionInfo+=" Build "+systemInfo.Properties["Version"].Value;
			}
			catch(Exception e) {
			}
			return versionInfo;
		}
		
		///<summary>Gets the assembly version from the OpenDentBusiness assembly (technically the assembly of the MiscData Type).</summary>
		public static string GetAssemblyVersion() {
			return typeof(MiscData).Assembly.GetName().Version.ToString();
		}

		public static string GetCurrentDatabase() {
			string command="SELECT database()";
			DataTable table=DataCore.GetTable(command);
			return PIn.String(table.Rows[0][0].ToString());
		}

		/// <summary>Method retained for backwards compatibility across MiddleTier.</summary>
		public static string GetMySqlVersion() {
			return GetMySqlVersion(getRawVersion:false);
		}

		///<summary>Returns the major and minor version of MySQL for the current connection.  Returns a version of 0.0 if the MySQL version cannot be determined.</summary>
		public static string GetMySqlVersion(bool getRawVersion=false) {
			string command="SELECT @@version";
			DataTable table=DataCore.GetTable(command);
			string version=PIn.String(table.Rows[0][0].ToString());
			string[] arrayVersion=version.Split('.');
			try {
				if(getRawVersion) {
					return version;
				}
				return int.Parse(arrayVersion[0])+"."+int.Parse(arrayVersion[1]);
			}
			catch {
			}
			return "0.0";
		}

		///<summary>Returns the current value in the GLOBAL max_allowed_packet variable.
		///max_allowed_packet is stored as an integer in multiples of 1,024 with a min value of 1,024 and a max value of 1,073,741,824.</summary>
		public static int GetMaxAllowedPacket() {
			int maxAllowedPacket=0;
			//The SHOW command is used because it was able to run with a user that had no permissions whatsoever.
			string command="SHOW GLOBAL VARIABLES WHERE Variable_name='max_allowed_packet'";
			DataTable table=DataCore.GetTable(command);
			if(table.Rows.Count > 0) {
				maxAllowedPacket=PIn.Int(table.Rows[0]["Value"].ToString());
			}
			return maxAllowedPacket;
		}

		public static void SetSqlMode() {
			try {
				if(PrefC.GetBool(PrefName.DatabaseGlobalVariablesDontSet)) {
					return;
				}
			}
			catch(Exception ex) {
			}
			//The SHOW command is used because it was able to run with a user that had no permissions whatsoever.
			string command="SHOW GLOBAL VARIABLES WHERE Variable_name='sql_mode'";
			DataTable table=DataCore.GetTable(command);
			//We want to run the SET GLOBAL command when no rows were returned (above query failed) or if the sql_mode is not blank or NO_AUTO_CREATE_USER
			//(set to something that could cause errors).
			if(table.Rows.Count<1 || (table.Rows[0]["Value"].ToString()!="" && table.Rows[0]["Value"].ToString().ToUpper()!="NO_AUTO_CREATE_USER")) {
				command="SET GLOBAL sql_mode=''";//in case user did not use our my.ini file.  http://www.opendental.com/manual/mysqlservervariables.html
				Db.NonQ(command);
			}
		}

		public static string GetStorageEngine() {
			string command="SELECT ENGINE FROM information_schema.ENGINES WHERE Support='DEFAULT'";
			DataTable table=DataCore.GetTable(command);
			return PIn.String(table.Rows[0][0].ToString());
		}

		///<summary>Runs the 'FLUSH TABLES;' command.</summary>
		public static void FlushTables() {
			Db.NonQ("FLUSH TABLES");
		}

		/// <summary>Runs the 'ENABLE KEYS' command for any tables that currently have their indexes disabled.</summary>
		public static void EnableIndexesIfNeeded(List<string> listTableNames) {
			if(listTableNames.IsNullOrEmpty()) {
				return;//There are no tables with disabled keys to re-enable.
			}
			string command="";
			for(int i=0;i<listTableNames.Count;i++) {
				ODEvent.Fire(ODEventType.ProgressBar,"Enabling indexes for table: "+POut.String(listTableNames[i]));
				command="ALTER TABLE "+POut.String(listTableNames[i])+" ENABLE KEYS";
				Db.NonQ(command);
			}
		}

		/// <summary>Returns a list of all tables that have their indexes disabled.</summary>
		public static List<string> GetTablesDisabledIndexes() {
			string command="SELECT * " +//If we only select table_name this query will return empty set even if there are 'disabled' rows.
				"FROM INFORMATION_SCHEMA.STATISTICS " +
				"WHERE TABLE_SCHEMA=DATABASE() AND COMMENT LIKE '%disabled%' " +
				"GROUP BY TABLE_NAME";
			DataTable dataTable=DataCore.GetTable(command);
			List<string> listTableNames=new List<string>();
			for(int i=0;i<dataTable.Rows.Count;i++) {
				listTableNames.Add(PIn.String(dataTable.Rows[i]["TABLE_NAME"].ToString()));
			}
			return listTableNames;
		}
	}

	
}































