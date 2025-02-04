using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Printers
{
    public static Printer GetOnePrinter(PrintSituation sit, long compNum)
    {
        var command = "SELECT * FROM printer WHERE "
                      + "PrintSit = '" + ((int) sit) + "' "
                      + "AND ComputerNum ='" + (compNum) + "'";
        return PrinterCrud.SelectOne(command);
    }
    
    private static void Insert(Printer cur)
    {
        PrinterCrud.Insert(cur);
    }
    
    private static void Update(Printer cur)
    {
        PrinterCrud.Update(cur);
    }
    
    private static void Delete(Printer cur)
    {
        var command = "DELETE FROM printer "
                      + "WHERE PrinterNum = " + (cur.PrinterNum);
        Db.NonQ(command);
    }

    public static bool PrinterIsInstalled(string name)
    {
        for (var i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
            if (PrinterSettings.InstalledPrinters[i] == name)
                return true;

        return false;
    }

    public static Printer GetForSit(PrintSituation sit)
    {
        var compCur = Computers.GetCur();
        if (compCur == null) return null;
        return GetFirstOrDefault(x => x.ComputerNum == compCur.ComputerNum && x.PrintSit == sit);
    }

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
    
    private static readonly PrinterCache Cache = new();

    public static Printer GetFirstOrDefault(Func<Printer, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}