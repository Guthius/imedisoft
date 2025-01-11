using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

/// <summary>
///     Handles all the business logic for printers.  Used heavily by the UI.  Every single function that makes
///     changes to the database must be completely autonomous and do ALL validation itself.
/// </summary>
public class Printers
{
    ///<summary>Gets the cached list of printers.</summary>
    public static List<Printer> GetListPrinters()
    {
        return _PrinterCache.GetDeepCopy();
    }

    ///<summary>Gets directly from database</summary>
    public static Printer GetOnePrinter(PrintSituation sit, long compNum)
    {
        var command = "SELECT * FROM printer WHERE "
                      + "PrintSit = '" + SOut.Long((int) sit) + "' "
                      + "AND ComputerNum ='" + SOut.Long(compNum) + "'";
        return PrinterCrud.SelectOne(command);
    }

    
    private static long Insert(Printer cur)
    {
        return PrinterCrud.Insert(cur);
    }

    
    private static void Update(Printer cur)
    {
        PrinterCrud.Update(cur);
    }

    
    private static void Delete(Printer cur)
    {
        var command = "DELETE FROM printer "
                      + "WHERE PrinterNum = " + SOut.Long(cur.PrinterNum);
        Db.NonQ(command);
    }

    public static bool PrinterIsInstalled(string name)
    {
        for (var i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            if (PrinterSettings.InstalledPrinters[i] == name)
                return true;

        return false;
    }

    /// <summary>
    ///     Gets the set printer whether or not it is valid.  Returns null if the current computer OR printer cannot be
    ///     found.
    /// </summary>
    public static Printer GetForSit(PrintSituation sit)
    {
        var compCur = Computers.GetCur();
        if (compCur == null) return null;
        return GetFirstOrDefault(x => x.ComputerNum == compCur.ComputerNum && x.PrintSit == sit);
    }

    /// <summary>
    ///     Either does an insert or an update to the database if need to create a Printer object.  Or it also deletes a
    ///     printer object if needed.
    /// </summary>
    public static void PutForSit(PrintSituation sit, string computerName, string printerName, bool displayPrompt, bool isVirtual = false, string fileExtension = "")
    {
        //Computer[] compList=Computers.Refresh();
        //Computer compCur=Computers.GetCur();
        var command = "SELECT ComputerNum FROM computer "
                      + "WHERE CompName = '" + SOut.String(computerName) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count == 0) return; //computer not yet entered in db.
        var compNum = SIn.Long(table.Rows[0][0].ToString());
        //only called from PrinterSetup window. Get info directly from db, then refresh when closing window. 
        var existing = GetOnePrinter(sit, compNum); //GetForSit(sit);
        if (printerName == "" && !displayPrompt && !isVirtual)
        {
            //then should not be an entry in db
            if (existing != null) //need to delete Printer
                Delete(existing);
        }
        else if (existing == null)
        {
            var cur = new Printer();
            cur.ComputerNum = compNum;
            cur.PrintSit = sit;
            cur.PrinterName = printerName;
            cur.DisplayPrompt = displayPrompt;
            cur.IsVirtualPrinter = isVirtual;
            cur.FileExtension = fileExtension;
            Insert(cur);
        }
        else
        {
            existing.PrinterName = printerName;
            existing.DisplayPrompt = displayPrompt;
            existing.IsVirtualPrinter = isVirtual;
            existing.FileExtension = fileExtension;
            Update(existing);
        }
    }

    /// <summary>
    ///     Called from FormPrinterSetup if user selects the easy option.  Since the other options will be hidden, we have
    ///     to clear them.  User should be sternly warned before this happens.
    /// </summary>
    public static void ClearAll()
    {
        //first, delete all entries
        var command = "DELETE FROM printer";
        Db.NonQ(command);
        //then, add one printer for each computer. Default and show prompt
        Computers.RefreshCache();
        Printer cur;
        var listComputers = Computers.GetDeepCopy();
        for (var i = 0; i < listComputers.Count; i++)
        {
            cur = new Printer();
            cur.ComputerNum = listComputers[i].ComputerNum;
            cur.PrintSit = PrintSituation.Default;
            cur.PrinterName = "";
            cur.DisplayPrompt = true;
            Insert(cur);
        }
    }

    #region CachePattern

    private class PrinterCache : CacheListAbs<Printer>
    {
        protected override List<Printer> GetCacheFromDb()
        {
            var command = "SELECT * FROM printer";
            return PrinterCrud.SelectMany(command);
        }

        protected override List<Printer> TableToList(DataTable dataTable)
        {
            return PrinterCrud.TableToList(dataTable);
        }

        protected override Printer Copy(Printer item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<Printer> items)
        {
            return PrinterCrud.ListToTable(items, "Printer");
        }

        protected override void FillCacheIfNeeded()
        {
            Printers.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly PrinterCache _PrinterCache = new();

    public static Printer GetFirstOrDefault(Func<Printer, bool> match, bool isShort = false)
    {
        return _PrinterCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _PrinterCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _PrinterCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _PrinterCache.ClearCache();
    }

    #endregion
}