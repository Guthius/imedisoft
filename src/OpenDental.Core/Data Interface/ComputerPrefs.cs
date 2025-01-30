using System;
using System.Data;
using System.Net;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ComputerPrefs
{
    private static ComputerPref _computerPrefLocal;
    private static ComputerPref _computerPrefLocalOld;
    private static bool _didShowError;

    public static ComputerPref LocalComputer
    {
        get
        {
            if (_computerPrefLocal != null)
            {
                return _computerPrefLocal;
            }

            _computerPrefLocal = GetForLocalComputer();
            _computerPrefLocalOld = _computerPrefLocal.Copy();

            return _computerPrefLocal;
        }
    }

    public static bool IsLocalComputerNull()
    {
        return _computerPrefLocal is null;
    }

    private static ComputerPref GetForLocalComputer()
    {
        return GetForComputer(ODEnvironment.MachineName);
    }

    public static ComputerPref GetForComputer(string computerName)
    {
        ComputerPref computerPref;

        var dataTable = GetPrefsForComputer(computerName);
        if (dataTable == null)
        {
            return GetDefaultComputerPref(computerName);
        }

        switch (dataTable.Rows.Count)
        {
            case > 1:
            {
                if (!_didShowError)
                {
                    _didShowError = true;

                    ODMessageBox.Show(
                        "Error in the database computerpref table. " +
                        "The computer name '" + SOut.String(computerName) + "' is a ComputerName in multiple records. " +
                        $"Please run the database maintenance method {SOut.String(nameof(DatabaseMaintenances.ComputerPrefDuplicates))}, " +
                        "then call us for help if you still get this message.");
                }

                break;
            }

            case 1:
                computerPref = ComputerPrefCrud.TableToList(dataTable)[0];
                return computerPref;
        }

        dataTable = GetPrefsForComputer(Dns.GetHostName());
        if (dataTable == null)
        {
            return GetDefaultComputerPref(computerName);
        }

        if (dataTable.Rows.Count >= 1)
        {
            computerPref = ComputerPrefCrud.TableToList(dataTable)[0];
            computerPref.ComputerName = computerName;
        }
        else
        {
            computerPref = GetDefaultComputerPref(computerName);
        }

        Insert(computerPref);

        return computerPref;
    }

    private static ComputerPref GetDefaultComputerPref(string computerName)
    {
        return new ComputerPref
        {
            SensorType = "D",
            SensorPort = 0,
            SensorExposure = 1,
            SensorBinned = false,
            AtoZpath = "",
            TaskKeepListHidden = false,
            TaskDock = 0,
            TaskX = 900,
            TaskY = 625,
            ComputerName = computerName,
            DirectXFormat = "",
            ScanDocSelectSource = false,
            ScanDocShowOptions = false,
            ScanDocDuplex = false,
            ScanDocGrayscale = false,
            ScanDocResolution = 150,
            ScanDocQuality = 40,
            GraphicsSimple = DrawingMode.DirectX,
            NoShowLanguage = false
        };
    }

    public static DataTable GetPrefsForComputer(string computerName)
    {
        try
        {
            return DataCore.GetTable("SELECT * FROM computerpref WHERE ComputerName='" + SOut.String(computerName) + "'");
        }
        catch
        {
            return null;
        }
    }

    public static void Insert(ComputerPref computerPref)
    {
        ComputerPrefCrud.Insert(computerPref);
    }

    public static void Update(ComputerPref computerPref)
    {
        Update(computerPref, _computerPrefLocalOld);
    }

    public static void Update(ComputerPref computerPrefNew, ComputerPref computerPrefOld)
    {
        bool changed;

        if (computerPrefOld == null)
        {
            ComputerPrefCrud.Update(computerPrefNew);
            changed = true;
        }
        else
        {
            changed = ComputerPrefCrud.Update(computerPrefNew, computerPrefOld);
        }

        if (!changed)
        {
            return;
        }

        _computerPrefLocal = GetForLocalComputer();
        _computerPrefLocalOld = _computerPrefLocal.Copy();
    }

    public static void SetToSimpleGraphics(string computerName)
    {
        Db.NonQ("UPDATE computerpref SET GraphicsSimple=1 WHERE ComputerName='" + SOut.String(computerName) + "'");
    }

    public static void ResetZoom(string computerName)
    {
        Db.NonQ("UPDATE computerpref SET Zoom=0 WHERE ComputerName='" + SOut.String(computerName) + "'");
    }

    public static void UpdateLocalComputerOs()
    {
        var platformId = SOut.String(Environment.OSVersion.Platform.ToString());

        if (LocalComputer.ComputerOS.ToString() == platformId)
        {
            return;
        }

        UpdateComputerOs(platformId, LocalComputer.ComputerPrefNum);

        _computerPrefLocal = GetForLocalComputer();
        _computerPrefLocalOld = _computerPrefLocal.Copy();
    }

    public static void UpdateComputerOs(string platformId, long computerPrefNum)
    {
        Db.NonQ("UPDATE computerpref SET ComputerOS = '" + SOut.String(platformId) + "' WHERE ComputerPrefNum = " + computerPrefNum);
    }
}