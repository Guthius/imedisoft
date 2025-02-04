using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class DefLinks
{
    public static List<DefLink> GetDefLinksByType(DefLinkType defLinkType)
    {
        return DefLinkCrud.SelectMany("SELECT * FROM deflink WHERE LinkType = " + (int) defLinkType);
    }

    public static List<DefLink> GetDefLinksByType(DefLinkType defLinkType, long defNum)
    {
        return DefLinkCrud.SelectMany("SELECT * FROM deflink WHERE LinkType = " + (int) defLinkType + " AND DefNum = " + defNum);
    }

    public static List<DefLink> GetDefLinksByTypeAndDefs(DefLinkType defLinkType, List<long> defNums)
    {
        if (defNums.IsNullOrEmpty())
        {
            return [];
        }

        return DefLinkCrud.SelectMany(
            "SELECT * FROM deflink " +
            "WHERE LinkType = " + (int) defLinkType + " " +
            "AND DefNum IN (" + string.Join(",", defNums) + ")");
    }

    public static List<DefLink> GetDefLinksForClinicSpecialties(long clinicNum)
    {
        return GetDefLinksByType(DefLinkType.ClinicSpecialty).FindAll(x => x.FKey == clinicNum);
    }

    public static List<DefLink> GetOperatoryDefLinksForCategory(DefCat defCat, bool shortList = false)
    {
        var defs = Defs.GetDefsForCategory(defCat, shortList);

        return GetDefLinksByTypeAndDefs(DefLinkType.Operatory, defs.Select(x => x.DefNum).ToList());
    }

    public static List<DefLink> GetForPatsAndType(List<long> patNums)
    {
        return DefLinkCrud.SelectMany("SELECT * FROM deflink WHERE LinkType = " + (int) DefLinkType.Patient + " AND FKey IN (" + string.Join(",", patNums) + ")");
    }

    public static DefLink GetOneByFKey(long fkey, DefLinkType defLinkType)
    {
        return GetListByFKeys([fkey], defLinkType).FirstOrDefault();
    }

    public static List<DefLink> GetListByFKey(long fkey, DefLinkType defLinkType)
    {
        return GetListByFKeys([fkey], defLinkType);
    }

    public static List<DefLink> GetListByFKeys(List<long> fkeys, DefLinkType defLinkType)
    {
        return fkeys.Count == 0
            ? []
            : DefLinkCrud.SelectMany(
                "SELECT * FROM deflink " +
                "WHERE FKey IN (" + string.Join(",", fkeys) + ") " +
                "AND LinkType = " + (int) defLinkType);
    }

    public static void Insert(DefLink defLink)
    {
        DefLinkCrud.Insert(defLink);
    }

    public static void SetFKeyForDef(long defNum, long fkey, DefLinkType defLinkType)
    {
        var defLinks = GetDefLinksByType(defLinkType, defNum);
        if (defLinks.Count > 0)
        {
            UpdateDefWithFKey(defNum, fkey, defLinkType);
        }
        else
        {
            Insert(new DefLink
            {
                DefNum = defNum,
                FKey = fkey,
                LinkType = defLinkType
            });
        }
    }

    public static void InsertDefLinksForDefs(List<long> defNums, long fkey, DefLinkType defLinkType)
    {
        if (defNums == null || defNums.Count < 1)
        {
            return;
        }

        foreach (var defNum in defNums)
        {
            Insert(new DefLink
            {
                DefNum = defNum,
                FKey = fkey,
                LinkType = defLinkType
            });
        }
    }

    public static void InsertDefLinksForFKeys(long defNum, List<long> fkeys, DefLinkType defLinkType)
    {
        if (fkeys is not {Count: > 0})
        {
            return;
        }

        DefLinkCrud.InsertMany(fkeys
            .Select(x => new DefLink
            {
                DefNum = defNum, FKey = x,
                LinkType = defLinkType
            })
            .ToList());
    }

    public static void Update(DefLink defLink)
    {
        DefLinkCrud.Update(defLink);
    }

    public static void UpdateDefWithFKey(long defNum, long fKey, DefLinkType defLinkType)
    {
        Db.NonQ(
            "UPDATE deflink SET FKey = " + fKey + " " +
            "WHERE LinkType = " + (int) defLinkType + " " +
            "AND DefNum = " + defNum);
    }

    public static void Delete(long defLinkNum)
    {
        DefLinkCrud.Delete(defLinkNum);
    }

    public static void DeleteAllForFKey(long fKey, DefLinkType defLinkType)
    {
        Db.NonQ(
            "DELETE FROM deflink " +
            "WHERE LinkType = " + (int) defLinkType + " " +
            "AND FKey = " + fKey);
    }

    public static void DeleteAllForFKeys(List<long> fkeys, DefLinkType defLinkType)
    {
        if (fkeys == null || fkeys.Count < 1)
        {
            return;
        }

        Db.NonQ(
            "DELETE FROM deflink " +
            "WHERE LinkType = " + (int) defLinkType + " " +
            "AND FKey IN (" + string.Join(", ", fkeys) + ")");
    }

    public static void DeleteAllForDef(long defNum)
    {
        Db.NonQ(
            "DELETE FROM deflink " +
            "WHERE DefNum = " + defNum + " " +
            "OR (LinkType = " + (int) DefLinkType.BlockoutType + " AND FKey = " + defNum + ")");
    }

    public static void DeleteAllForDef(long defNum, DefLinkType defLinkType)
    {
        Db.NonQ("DELETE FROM deflink WHERE LinkType = " + (int) defLinkType + " AND DefNum = " + defNum);
    }

    public static void DeleteDefLinks(List<long> defLinkNums)
    {
        if (defLinkNums == null || defLinkNums.Count < 1)
        {
            return;
        }

        Db.NonQ("DELETE FROM deflink WHERE DefLinkNum IN (" + string.Join(", ", defLinkNums) + ")");
    }
}