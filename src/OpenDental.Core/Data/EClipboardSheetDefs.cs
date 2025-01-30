using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EClipboardSheetDefs
{
    public static void Update(EClipboardSheetDef eClipboardSheetDef)
    {
        EClipboardSheetDefCrud.Update(eClipboardSheetDef);
    }

    public static void Sync(List<EClipboardSheetDef> listEClipboardSheetDefsNew, List<EClipboardSheetDef> listEClipboardSheetDefsOld)
    {
        EClipboardSheetDefCrud.Sync(listEClipboardSheetDefsNew, listEClipboardSheetDefsOld);
    }

    public static List<EClipboardSheetDef> Refresh()
    {
        return EClipboardSheetDefCrud.SelectMany("SELECT * FROM eclipboardsheetdef");
    }

    public static List<EClipboardSheetDef> GetForClinic(long clinicNum)
    {
        return EClipboardSheetDefCrud.SelectMany("SELECT * FROM eclipboardsheetdef WHERE ClinicNum=" + clinicNum);
    }

    public static bool IsSheetDefInUse(long sheetDefNum)
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM eclipboardsheetdef WHERE SheetDefNum=" + sheetDefNum)) > 0;
    }

    public static bool IsEFormDefInUse(long eFormDefNum)
    {
        return SIn.Int(Db.GetCount("SELECT COUNT(*) FROM eclipboardsheetdef WHERE EFormDefNum=" + eFormDefNum)) > 0;
    }

    public static List<EClipboardSheetDef> GetAllForSheetDefForOnceRule(long sheetDefNum)
    {
        return EClipboardSheetDefCrud.SelectMany("SELECT * FROM eclipboardsheetdef WHERE SheetDefNum=" + sheetDefNum + " AND Frequency = " + (int) EnumEClipFreq.Once);
    }

    public static List<long> GetListIgnoreSheetDefNums(EClipboardSheetDef eClipboardSheetDef)
    {
        return eClipboardSheetDef?.IgnoreSheetDefNums == null ? [] : eClipboardSheetDef.IgnoreSheetDefNums.Split(',').Select(x => SIn.Long(x)).ToList();
    }

    public static List<EClipboardSheetDef> GetManyEClipboardSheetDefsForOnceRuleAtClinic(List<long> listSheetDefNums, long clinicNum)
    {
        if (listSheetDefNums == null || listSheetDefNums.Count == 0)
        {
            return [];
        }

        return EClipboardSheetDefCrud.SelectMany(
            "SELECT * FROM eclipboardsheetdef " +
            "WHERE SheetDefNum IN (" + string.Join(",", listSheetDefNums) + ") " +
            "AND ClinicNum=" + clinicNum + " " +
            "AND Frequency = " + (int) EnumEClipFreq.Once);
    }

    public static List<EClipboardSheetDef> FilterPrefillStatuses(List<EClipboardSheetDef> listEClipboardSheetDefs, List<Sheet> listSheetsCompleted, long clinicNum, List<Sheet> listSheetsInTerminal = null)
    {
        var results = new List<EClipboardSheetDef>(listEClipboardSheetDefs);
        var sheetDefNumsToRemove = new List<long>();
        
        // Remove any sheet defs that are set to EnumEClipFreq.Once and have been filled out or have a sheetdef filled out with a revision greathan or equal to the prefillstatusoverride revision id. 
        results.RemoveAll(x => x.Frequency == EnumEClipFreq.Once && listSheetsCompleted.Any(y => y.SheetDefNum == x.SheetDefNum && y.RevID >= x.PrefillStatusOverride));
       
        // Remove any sheets in a EclipboardSheetDefs ignore list that are set to be filled out once.
        foreach (var clipboardSheetDef in results)
        {
            // If the prefill status is not set to once, then ignore lists are not used.
            if (clipboardSheetDef.Frequency != EnumEClipFreq.Once)
            {
                continue;
            }
            
            var listSheetDefNumsIgnore = GetListIgnoreSheetDefNums(clipboardSheetDef);
            
            sheetDefNumsToRemove.AddRange(listSheetDefNumsIgnore);
        }

        if (listSheetsInTerminal is {Count: > 0})
        {
            if (clinicNum > 0)
            {
                clinicNum = ClinicPrefs.GetBool(PrefName.EClipboardUseDefaults, clinicNum) ? 0 : clinicNum;
            }
            
            var listEClipboardSheetDefsForTerminal = GetManyEClipboardSheetDefsForOnceRuleAtClinic(listSheetsInTerminal.Select(x => x.SheetDefNum).ToList(), clinicNum);

            foreach (var clipboardSheetDef in listEClipboardSheetDefsForTerminal)
            {
                sheetDefNumsToRemove.AddRange(GetListIgnoreSheetDefNums(clipboardSheetDef));
            }
        }
        
        results.RemoveAll(x => sheetDefNumsToRemove.Distinct().Contains(x.SheetDefNum));
        
        return results;
    }
}