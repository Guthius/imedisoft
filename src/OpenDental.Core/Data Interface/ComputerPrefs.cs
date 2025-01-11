using System;
using System.Data;
using System.Net;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ComputerPrefs
{
    ///<summary>Null prior to conversion script, then it always has a value.</summary>
    private static ComputerPref _computerPrefLocal;

    ///<summary>Used when updating so that we only update columns that changed.</summary>
    private static ComputerPref _computerPrefLocalOld;

    /// <summary>
    ///     Used when initially loading GetForLocalComputer(). Lets us display the warning about duplicate computer names
    ///     only once.
    /// </summary>
    private static bool _didShowError;

    public static ComputerPref LocalComputer
    {
        get
        {
            if (_computerPrefLocal == null || (ODEnvironment.IsCloudInstance && _computerPrefLocal.ComputerName.ToLower() != ODEnvironment.MachineName.ToLower()))
            {
                _computerPrefLocal = GetForLocalComputer();
                _computerPrefLocalOld = _computerPrefLocal.Copy();
            }

            return _computerPrefLocal;
        }
        //set {
        //I don't think this gets used.
        //}
    }

    public static bool IsLocalComputerNull()
    {
        return _computerPrefLocal is null;
    }

    ///<summary>Returns the computer preferences for the computer which this instance of Open Dental is running on.</summary>
    private static ComputerPref GetForLocalComputer()
    {
        return GetForComputer(ODEnvironment.MachineName);
    }

    ///<summary>Returns the computer preferences for the computer with the passed in computer name.</summary>
    public static ComputerPref GetForComputer(string computerName)
    {
        ComputerPref computerPref;
        var table = GetPrefsForComputer(computerName);
        if (table == null)
            //In case of database error, just use default graphics settings so that it is possible for the program to start.
            return GetDefaultComputerPref(computerName);

        if (table.Rows.Count > 1)
            //Should never happen (would only happen if the primary key showed up more than once).
            if (!_didShowError)
            {
                //This shows a dialog box, which causes paint to call this again. Recursive bad stuff happens, so we essentially only want to show this once.
                _didShowError = true;
                Logger.openlog.LogMB("Error in the database computerpref table. The computer name '"
                                     + SOut.String(computerName) + "' is a ComputerName in multiple records. Please run the "
                                     + $"database maintenance method {SOut.String(nameof(DatabaseMaintenances.ComputerPrefDuplicates))}, then call us for help if you still get this message.",
                    Logger.Severity.WARNING);
            }

        if (table.Rows.Count == 1)
        {
            computerPref = ComputerPrefCrud.TableToList(table)[0];
            return computerPref;
        }

        //Computer pref row does not yet exist for this computer.
        //In version 21.3, we transitioned from using Dns.GetHostName() to ODEnvironment.MachineName for the ComputerPref.ComputerName.
        //If the user is coming from the older version, we want to copy their preferences so they do not get reset when we use ODEnvironment instead of Dns.
        //For example, in the old version, there would only be one entry for an app server. This will copy the computerpref of the server and use it for each client.
        //If there exists a database entry for computerName=Dns.GetHostName(), use those preferences as a template for the new ComputerPref.
        //Otherwise, use the default preferences from GetDefaultComputerPref();
        table = GetPrefsForComputer(Dns.GetHostName());
        if (table == null)
            //In case of database error, just use default graphics settings so that it is possible for the program to start.
            return GetDefaultComputerPref(computerName);

        if (table.Rows.Count >= 1)
        {
            //There should not be more than 1 row in the table, but if there are then we will just use the first row as the template.
            computerPref = ComputerPrefCrud.TableToList(table)[0];
            computerPref.ComputerName = computerName;
        }
        else
        {
            //There is not a ComputerPref entry for this DNS host name. Use the default preferences.
            computerPref = GetDefaultComputerPref(computerName);
        }

        Insert(computerPref); //Create default prefs for the specified computer. Also sets primary key in our computerPref object.
        return computerPref;
    }

    ///<summary>Returns a ComputerPref object with the default preferences and the given computer name.</summary>
    private static ComputerPref GetDefaultComputerPref(string computerName)
    {
        var computerPref = new ComputerPref();
        computerPref.SensorType = "D";
        computerPref.SensorPort = 0;
        computerPref.SensorExposure = 1;
        computerPref.SensorBinned = false;
        computerPref.AtoZpath = "";
        computerPref.TaskKeepListHidden = false; //show docked task list on this computer 
        computerPref.TaskDock = 0; //bottom
        computerPref.TaskX = 900;
        computerPref.TaskY = 625;
        computerPref.ComputerName = computerName;
        computerPref.DirectXFormat = "";
        computerPref.ScanDocSelectSource = false;
        computerPref.ScanDocShowOptions = false;
        computerPref.ScanDocDuplex = false;
        computerPref.ScanDocGrayscale = false;
        computerPref.ScanDocResolution = 150; //default suggested in FormImagingSetup
        computerPref.ScanDocQuality = 40; //default suggested in FormImagingSetup
        computerPref.GraphicsSimple = DrawingMode.DirectX;
        computerPref.NoShowLanguage = false;
        return computerPref;
    }

    ///<summary>Gets from database. Should not be called by external classes.</summary>
    public static DataTable GetPrefsForComputer(string computerName)
    {
        var command = "SELECT * FROM computerpref WHERE ComputerName='" + SOut.String(computerName) + "'"; //ignores case
        try
        {
            return DataCore.GetTable(command);
        }
        catch
        {
            return null; //Maybe for conversion from older version of OD?
        }
    }

    ///<summary>Should not be called by external classes.</summary>
    public static long Insert(ComputerPref computerPref)
    {
        return ComputerPrefCrud.Insert(computerPref);
    }

    /// <summary>
    ///     Any time this is called, ComputerPrefs.LocalComputer MUST be passed in.
    ///     It will have already been changed for local use, and this saves it for next time.
    /// </summary>
    public static void Update(ComputerPref computerPref)
    {
        Update(computerPref, _computerPrefLocalOld);
    }

    /// <summary>
    ///     Updates the database with computerPrefNew.  Returns true if changes were needed or if computerPrefOld is null.
    ///     Automatically clears out class-wide variables if any changes were needed.
    /// </summary>
    public static bool Update(ComputerPref computerPrefNew, ComputerPref computerPrefOld)
    {
        var hadChanges = false;
        if (computerPrefOld == null)
        {
            ComputerPrefCrud.Update(computerPrefNew);
            hadChanges = true; //Assume that there were database changes.
        }
        else
        {
            hadChanges = ComputerPrefCrud.Update(computerPrefNew, computerPrefOld);
        }

        if (!hadChanges) return hadChanges;

        //DB was updated, may also contain other changes from other WS.
        //Set to null so that we can go to DB again when needed, ensures _localComputer is never stale.
        //_localComputer=null;
        //_localComputerOld=null;
        _computerPrefLocal = GetForLocalComputer();
        _computerPrefLocalOld = _computerPrefLocal.Copy();
        return hadChanges;
    }

    /// <summary>
    ///     Sets the GraphicsSimple column to 1.  Added to fix machines (lately tablets) that are having graphics problems
    ///     and cannot start OpenDental.
    /// </summary>
    public static void SetToSimpleGraphics(string computerName)
    {
        var command = "UPDATE computerpref SET GraphicsSimple=1 WHERE ComputerName='" + SOut.String(computerName) + "'";
        Db.NonQ(command);
    }

    public static void ResetZoom(string computerName)
    {
        var command = "UPDATE computerpref SET Zoom=0 WHERE ComputerName='" + SOut.String(computerName) + "'";
        Db.NonQ(command);
    }

    ///<summary>Updates the local computerpref's ComputerOS if it is different than what is stored.</summary>
    public static void UpdateLocalComputerOS()
    {
        var stringPlatformId = SOut.String(Environment.OSVersion.Platform.ToString());
        if (LocalComputer.ComputerOS.ToString() == stringPlatformId)
            //We only want to update this column if there is indeed a difference between values.
            return;

        UpdateComputerOS(stringPlatformId, LocalComputer.ComputerPrefNum);
        //_localComputer=null;//Setting to null will trigger LocalComputer to update from DB next time it is accessed.
        _computerPrefLocal = GetForLocalComputer();
        _computerPrefLocalOld = _computerPrefLocal.Copy();
    }

    ///<summary>Updates the ComputerOS for the computerPrefNum passed in.</summary>
    public static void UpdateComputerOS(string platformId, long computerPrefNum)
    {
        //We have to use a query and not the normal Update(computerPref) method because the 
        //Environment.OSVersion.Platform enum is different than the computerpref.ComputerOS enum.
        var command = "UPDATE computerpref SET ComputerOS = '" + SOut.String(platformId) + "' "
                      + "WHERE ComputerPrefNum = " + SOut.Long(computerPrefNum);
        Db.NonQ(command);
    }
}