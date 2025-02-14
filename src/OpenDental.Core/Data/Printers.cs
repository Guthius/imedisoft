using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Printers
{
    public static Printer GetOnePrinter(PrintSituation printSituation, long compNum)
    {
        return PrinterCrud.SelectOne("SELECT * FROM printer WHERE PrintSit = '" + (int) printSituation + "' AND ComputerNum ='" + compNum + "'");
    }

    private static void Insert(Printer printer)
    {
        PrinterCrud.Insert(printer);
    }

    private static void Update(Printer printer)
    {
        PrinterCrud.Update(printer);
    }

    private static void Delete(Printer printer)
    {
        Db.NonQ("DELETE FROM printer WHERE PrinterNum = " + printer.PrinterNum);
    }

    public static bool PrinterIsInstalled(string name)
    {
        for (var i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
        {
            if (PrinterSettings.InstalledPrinters[i] == name)
            {
                return true;
            }
        }

        return false;
    }

    public static Printer GetForSit(PrintSituation sit)
    {
        var computer = Computers.GetCur();

        return computer == null ? null : GetFirstOrDefault(x => x.ComputerNum == computer.ComputerNum && x.PrintSit == sit);
    }

    public static void PutForSit(PrintSituation sit, string computerName, string printerName, bool displayPrompt, bool isVirtual = false, string fileExtension = "")
    {
        var dataTable = DataCore.GetTable("SELECT ComputerNum FROM computer WHERE CompName = '" + SOut.String(computerName) + "'");
        if (dataTable.Rows.Count == 0)
        {
            return;
        }

        var compNum = SIn.Long(dataTable.Rows[0][0].ToString());

        var existing = GetOnePrinter(sit, compNum);
        if (printerName == "" && !displayPrompt && !isVirtual)
        {
            if (existing is not null)
            {
                Delete(existing);
            }
        }
        else if (existing is null)
        {
            Insert(new Printer
            {
                ComputerNum = compNum,
                PrintSit = sit,
                PrinterName = printerName,
                DisplayPrompt = displayPrompt,
                IsVirtualPrinter = isVirtual,
                FileExtension = fileExtension
            });
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
        Db.NonQ("DELETE FROM printer");

        Computers.RefreshCache();

        var computers = Computers.GetDeepCopy();

        foreach (var computer in computers)
        {
            Insert(new Printer
            {
                ComputerNum = computer.ComputerNum,
                PrintSit = PrintSituation.Default,
                PrinterName = "",
                DisplayPrompt = true
            });
        }
    }

    private class PrinterCache : CacheListAbs<Printer>
    {
        protected override List<Printer> GetCacheFromDb()
        {
            return PrinterCrud.SelectMany("SELECT * FROM printer");
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
            GetTableFromCache(false);
        }
    }

    private static readonly PrinterCache Cache = new();

    public static Printer GetFirstOrDefault(Func<Printer, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}