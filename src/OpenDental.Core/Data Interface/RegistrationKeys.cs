using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CDT;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

/// <summary>
///     Used to keep track of which product keys have been assigned to which customers.  This class is designed for
///     distributor installations.
/// </summary>
public class RegistrationKeys
{
	/// <summary>
	///     Retrieves all registration keys for a particular customer's family. There can be multiple keys assigned to a single
	///     customer, or keys
	///     assigned to individual family members, since the customer may have multiple physical locations of business.
	/// </summary>
	public static RegistrationKey[] GetForPatient(long patNum)
    {
        var fam = Patients.GetFamily(patNum);
        var command = $"SELECT * FROM registrationkey WHERE PatNum IN ({string.Join(",", fam.GetPatNums())})";
        return RegistrationKeyCrud.SelectMany(command).ToArray();
    }

    ///<summary>Updates the given key data to the database.</summary>
    public static void Update(RegistrationKey registrationKey)
    {
        RegistrationKeyCrud.Update(registrationKey);
    }

    /// <summary>
    ///     Updates one RegistrationKey in the database.  Uses an old object to compare to, and only alters changed
    ///     fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update
    ///     occurred.
    /// </summary>
    public static bool Update(RegistrationKey registrationKey, RegistrationKey registrationKeyOld)
    {
        return RegistrationKeyCrud.Update(registrationKey, registrationKeyOld);
    }

    ///<summary>Inserts a new and unique registration key into the database.</summary>
    public static long Insert(RegistrationKey registrationKey)
    {
        do
        {
            if (registrationKey.IsForeign)
            {
                var rand = new Random();
                var strBuild = new StringBuilder();
                for (var i = 0; i < 16; i++)
                {
                    var r = rand.Next(0, 35);
                    if (r < 10)
                        strBuild.Append((char) ('0' + r));
                    else
                        strBuild.Append((char) ('A' + r - 10));
                }

                registrationKey.RegKey = strBuild.ToString();
            }
            else
            {
                registrationKey.RegKey = Class1.GenerateRandKey();
            }

            if (registrationKey.RegKey == "")
                //Don't loop forever when software is unverified.
                return 0; //not sure what consequence this would have.
        } while (KeyIsInUse(registrationKey.RegKey));

        return RegistrationKeyCrud.Insert(registrationKey);
    }

    public static void Delete(long registrationKeyNum)
    {
        var command = "DELETE FROM registrationkey WHERE RegistrationKeyNum='"
                      + SOut.Long(registrationKeyNum) + "'";
        Db.NonQ(command);
    }

    ///<summary>Returns true if the given registration key is currently in use by a customer, false otherwise.</summary>
    public static bool KeyIsInUse(string regKey)
    {
        var command = "SELECT RegKey FROM registrationkey WHERE RegKey='" + SOut.String(regKey) + "'";
        var table = DataCore.GetTable(command);
        return table.Rows.Count > 0;
    }

    
    public static bool KeyIsEnabled(RegistrationKey registrationKey)
    {
        if (registrationKey.DateDisabled.Year > 1880
            || registrationKey.DateStarted > DateTime.Today
            || (registrationKey.DateEnded.Year > 1880 && registrationKey.DateEnded < DateTime.Today))
            return false;
        return true;
    }

    /// <Summary>
    ///     Returns any active registration keys that have no repeating charges on any corresponding family members.
    ///     Columns include PatNum, LName, FName, and RegKey.
    /// </Summary>
    public static DataTable GetAllWithoutCharges()
    {
        #region Old Code

        /*
        DataTable table=new DataTable();
        table.Columns.Add("dateStop");
        table.Columns.Add("family");
        table.Columns.Add("PatNum");
        table.Columns.Add("RegKey");
        string command=@"
            DROP TABLE IF EXISTS tempRegKeys;
            CREATE TABLE tempRegKeys(
                tempRegKeyId int auto_increment NOT NULL,
                PatNum bigint NOT NULL,
                RegKey VARCHAR(255) NOT NULL,
                IsMissing tinyint NOT NULL,
                Date_ DATE NOT NULL DEFAULT '0001-01-01',
                PRIMARY KEY(tempRegKeyId),
                KEY(PatNum));
            -- Fill table with patnums for all guarantors of regkeys that are still active.
            INSERT INTO tempRegKeys (PatNum,RegKey,Date_)
            SELECT patient.Guarantor,RegKey,'0001-01-01'
            FROM registrationkey
            LEFT JOIN patient ON registrationkey.PatNum=patient.PatNum
            WHERE DateDisabled='0001-01-01'
            AND DateEnded='0001-01-01'
            AND IsFreeVersion=0
            AND IsOnlyForTesting=0;
            -- Set indicators on keys with missing repeatcharges
            UPDATE tempRegKeys
            SET IsMissing=1
            WHERE NOT EXISTS(SELECT * FROM repeatcharge WHERE repeatcharge.PatNum=tempRegKeys.PatNum);

            -- Now, look for expired repeating charges.  This is done in two steps.
            -- Step 1: Mark all keys that have expired repeating charges.
            -- Step 2: Then, remove those markings for all keys that also have unexpired repeating charges.
            UPDATE tempRegKeys
            SET Date_=(
            SELECT IFNULL(MAX(DateStop),'0001-01-01')
            FROM repeatcharge
            WHERE repeatcharge.PatNum=tempRegKeys.PatNum
            AND DateStop < "+DbHelper.Now()+@" AND DateStop > '0001-01-01');
            -- Step 2:
            UPDATE tempRegKeys
            SET Date_='0001-01-01'
            WHERE EXISTS(
            SELECT * FROM repeatcharge
            WHERE repeatcharge.PatNum=tempRegKeys.PatNum
            AND DateStop = '0001-01-01');

            SELECT LName,FName,tempRegKeys.PatNum,tempRegKeys.RegKey,IsMissing,Date_
            FROM tempRegKeys
            LEFT JOIN patient ON patient.PatNum=tempRegKeys.PatNum
            WHERE IsMissing=1
            OR Date_ > '0001-01-01'
            ORDER BY tempRegKeys.PatNum;
            DROP TABLE IF EXISTS tempRegKeys;";
        DataTable raw=DataCore.GetTable(command);
        DataRow row;
        DateTime dateRepeatStop;
        for(int i=0;i<raw.Rows.Count;i++) {
            row=table.NewRow();
            if(raw.Rows[i]["IsMissing"].ToString()=="1") {
                row["dateStop"]="Missing Repeat Charge";
            }
            else {
                row["dateStop"]="";
            }
            dateRepeatStop=PIn.Date(raw.Rows[i]["Date_"].ToString());
            if(dateRepeatStop.Year>1880) {
                if(row["dateStop"].ToString()!="") {
                    row["dateStop"]+="\r\n";
                }
                row["dateStop"]+="Expired Repeat Charge:"+dateRepeatStop.ToShortDateString();
            }
            row["family"]=raw.Rows[i]["LName"].ToString()+", "+raw.Rows[i]["FName"].ToString();
            row["PatNum"]=raw.Rows[i]["PatNum"].ToString();
            row["RegKey"]=raw.Rows[i]["RegKey"].ToString();
            table.Rows.Add(row);
        }
        return table;
        */

        #endregion

        //The detailed queries above were taking far too long and were too complicated.
        //Instead, we will look for any active registration keys that have no repeating charges on any corresponding family members.
        var command = @"SELECT registrationkey.PatNum,registrationkey.RegKey,patient.LName,patient.FName
				FROM registrationkey
				LEFT JOIN patient ON registrationkey.PatNum=patient.PatNum
				LEFT JOIN (
					SELECT family.PatNum
					FROM patient family
					WHERE family.PatNum IN (SELECT repeatcharge.PatNum FROM repeatcharge 
						WHERE repeatcharge.DateStop >= NOW() OR repeatcharge.DateStop='0001-01-01')
					OR family.Guarantor IN (SELECT repeatcharge.PatNum FROM repeatcharge
						WHERE repeatcharge.DateStop >= NOW() OR repeatcharge.DateStop='0001-01-01')
					) AS activecharges ON registrationkey.PatNum=activecharges.PatNum 
				WHERE registrationkey.DateDisabled='0001-01-01'
				AND registrationkey.DateEnded='0001-01-01'
				AND registrationkey.IsFreeVersion=0 
				AND registrationkey.IsOnlyForTesting=0
				AND ISNULL(activecharges.PatNum)";
        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     Does not validate regkey like GetByKey().
    ///     Returns a dictionary such that the key is a registration key and the value is the patient.
    /// </summary>
    public static Dictionary<string, Patient> GetPatientsByKeys(List<string> listRegKeyStrs)
    {
        if (listRegKeyStrs == null || listRegKeyStrs.Count == 0) return new Dictionary<string, Patient>();
        var command = "SELECT * FROM  registrationkey WHERE RegKey IN(" + string.Join(",", listRegKeyStrs.Distinct().Select(x => "'" + SOut.String(x) + "'").ToList()) + ")";
        var listRegKeys = RegistrationKeyCrud.TableToList(DataCore.GetTable(command));
        var pats = Patients.GetMultPats(listRegKeys.Select(x => x.PatNum).ToList());
        return listRegKeys.Select(x => new {RegKeyStr = x.RegKey, PatientCur = pats.FirstOrDefault(y => y.PatNum == x.PatNum) ?? new Patient()})
            .ToDictionary(x => x.RegKeyStr, x => x.PatientCur);
    }

    ///<summary>Throws exceptions.</summary>
    public static RegistrationKey GetByKey(string regKey)
    {
        if (!Regex.IsMatch(regKey, @"^[A-Z0-9]{16}$")) throw new ApplicationException("Invalid registration key format.");
        var command = "SELECT * FROM  registrationkey WHERE RegKey='" + SOut.String(regKey) + "'";
        var key = RegistrationKeyCrud.SelectOne(command);
        if (key == null) throw new ApplicationException("Invalid registration key.");
        return key;
    }

    ///<summary>Get the list of all RegistrationKey rows. DO NOT REMOVE! Used by OD WebApps solution.</summary>
    public static List<RegistrationKey> GetAll()
    {
        var command = "SELECT * FROM registrationkey";
        return RegistrationKeyCrud.SelectMany(command);
    }

    /// <summary>
    ///     Get the list of all RegistrationKey rows that have DateTBackupScheduled set in the future and a BackupPassCode
    ///     set as well.
    /// </summary>
    public static List<RegistrationKey> GetScheduledSupplementalBackups()
    {
        var command = $"SELECT * FROM registrationkey WHERE DateTBackupScheduled > {DbHelper.Now()}";
        //Use C# to filter by BackupPassCode because we don't currently have an index for BackupPassCode in the database.
        return RegistrationKeyCrud.SelectMany(command).FindAll(x => !string.IsNullOrWhiteSpace(x.BackupPassCode));
    }

    ///<summary>Get the list of all RegistrationKey rows that have the passed in RegistrationKeyNums.</summary>
    public static List<RegistrationKey> GetByRegKeyNums(List<long> listRegKeyNums)
    {
        if (listRegKeyNums.IsNullOrEmpty()) return new List<RegistrationKey>();

        var command = $"SELECT * FROM registrationkey WHERE RegistrationKeyNum IN({string.Join(",", listRegKeyNums.Select(x => SOut.Long(x)))})";
        return RegistrationKeyCrud.SelectMany(command);
    }
}