using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class RecallTypes
{
    public static long ProphyType => PrefC.GetLong(PrefName.RecallTypeSpecialProphy);
    public static long PerioType => PrefC.GetLong(PrefName.RecallTypeSpecialPerio);
    public static long ChildProphyType => PrefC.GetLong(PrefName.RecallTypeSpecialChildProphy);

    public static void Insert(RecallType recallType)
    {
        RecallTypeCrud.Insert(recallType);
    }

    public static void Update(RecallType recallType)
    {
        RecallTypeCrud.Update(recallType);
    }

    public static string GetDescription(long recallTypeNum)
    {
        var recallType = GetFirstOrDefault(x => x.RecallTypeNum == recallTypeNum);
        return recallType == null ? "" : recallType.Description;
    }

    public static Interval GetInterval(long recallTypeNum)
    {
        var recallType = GetFirstOrDefault(x => x.RecallTypeNum == recallTypeNum);
        return recallType == null ? new Interval(0, 0, 0, 0) : recallType.DefaultInterval;
    }

    public static List<string> GetProcs(long recallTypeNum)
    {
        var recallType = GetFirstOrDefault(x => x.RecallTypeNum == recallTypeNum);
        return recallType == null || string.IsNullOrEmpty(recallType.Procedures) ? new List<string>() : recallType.Procedures.Split(',').ToList();
    }

    public static bool PerioAndProphyBothHaveTriggers()
    {
        if (PerioType == 0 || ProphyType == 0) return false;
        if (RecallTriggers.GetForType(PerioType).Count == 0) return false;
        if (RecallTriggers.GetForType(ProphyType).Count == 0) return false;
        return true;
    }

    public static string GetTimePattern(long recallTypeNum)
    {
        var recallType = GetFirstOrDefault(x => x.RecallTypeNum == recallTypeNum);
        return recallType == null ? "" : recallType.TimePattern;
    }

    public static string ConvertTimePattern(string timePattern)
    {
        //convert time pattern to 5 minute increment
        var patternConverted = new StringBuilder();
        for (var i = 0; i < timePattern.Length; i++)
        {
            patternConverted.Append(timePattern.Substring(i, 1));
            if (PrefC.GetLong(PrefName.AppointmentTimeIncrement) == 10) patternConverted.Append(timePattern.Substring(i, 1));
            if (PrefC.GetLong(PrefName.AppointmentTimeIncrement) == 15)
            {
                patternConverted.Append(timePattern.Substring(i, 1));
                patternConverted.Append(timePattern.Substring(i, 1));
            }
        }

        if (patternConverted.ToString() == "")
        {
            if (PrefC.GetLong(PrefName.AppointmentTimeIncrement) == 15)
                patternConverted.Append("///XXX///");
            else
                patternConverted.Append("//XX//");
        }

        return patternConverted.ToString();
    }

    public static string GetSpecialTypeStr(long recallTypeNum)
    {
        if (recallTypeNum == PrefC.GetLong(PrefName.RecallTypeSpecialProphy)) return Lans.g("FormRecallTypeEdit", "Prophy");
        if (recallTypeNum == PrefC.GetLong(PrefName.RecallTypeSpecialChildProphy)) return Lans.g("FormRecallTypeEdit", "ChildProphy");
        if (recallTypeNum == PrefC.GetLong(PrefName.RecallTypeSpecialPerio)) return Lans.g("FormRecallTypeEdit", "Perio");
        return "";
    }

    public static bool IsSpecialRecallType(long recallTypeNum)
    {
        if (recallTypeNum == PrefC.GetLong(PrefName.RecallTypeSpecialProphy)) return true;
        if (recallTypeNum == PrefC.GetLong(PrefName.RecallTypeSpecialChildProphy)) return true;
        if (recallTypeNum == PrefC.GetLong(PrefName.RecallTypeSpecialPerio)) return true;
        return false;
    }

    public static List<RecallType> GetActive()
    {
        var retVal = new List<RecallType>();
        List<RecallTrigger> triggers;
        var listRecallTypes = GetDeepCopy();
        for (var i = 0; i < listRecallTypes.Count; i++)
        {
            triggers = RecallTriggers.GetForType(listRecallTypes[i].RecallTypeNum);
            if (triggers.Count > 0) retVal.Add(listRecallTypes[i].Copy());
        }

        return retVal;
    }

    public static void SetToDefault()
    {
        var command = "DELETE FROM recalltype WHERE RecallTypeNum >= 1 AND RecallTypeNum <= 7"; //Don't delete manually added recall types
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,TimePattern,Procedures) VALUES (1,'Prophy',393217,'/XXXX/','D1110')";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,TimePattern,Procedures) VALUES (2,'Child Prophy',0,'XXX','D1120,D1208')";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,TimePattern,Procedures) VALUES (3,'Perio',262144,'/XXXX/','D4910')";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,Procedures,AppendToSpecial) VALUES (4,'4BW',16777216,'D0274',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,Procedures,AppendToSpecial) VALUES (5,'Pano',83886080,'D0330',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,Procedures,AppendToSpecial) VALUES (6,'FMX',83886080,'D0210',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,Procedures,AppendToSpecial) VALUES (7,'Exam',393217,'D0120',1)";
        Db.NonQ(command);
        command = "DELETE FROM recalltrigger"; //OK to delete triggers for manually added recalls, because deleting the triggers disables the recall type.
        Db.NonQ(command);
        //command="INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (1,1,"+ProcedureCodes.GetCodeNum("D0415")+")";//collection of microorg for culture
        //Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (1,7," + ProcedureCodes.GetCodeNum("D0150") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (2,4," + ProcedureCodes.GetCodeNum("D0274") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (3,5," + ProcedureCodes.GetCodeNum("D0330") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (4,6," + ProcedureCodes.GetCodeNum("D0210") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (5,1," + ProcedureCodes.GetCodeNum("D1110") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (6,1," + ProcedureCodes.GetCodeNum("D1120") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (7,3," + ProcedureCodes.GetCodeNum("D4910") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (8,3," + ProcedureCodes.GetCodeNum("D4341") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (9,7," + ProcedureCodes.GetCodeNum("D0120") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (10,7," + ProcedureCodes.GetCodeNum("D0180") + ")";
        Db.NonQ(command);
        //Update the special types in preference table.
        command = "UPDATE preference SET ValueString='1' WHERE PrefName='RecallTypeSpecialProphy'";
        Db.NonQ(command);
        command = "UPDATE preference SET ValueString='2' WHERE PrefName='RecallTypeSpecialChildProphy'";
        Db.NonQ(command);
        command = "UPDATE preference SET ValueString='3' WHERE PrefName='RecallTypeSpecialPerio'";
        Db.NonQ(command);
        command = "UPDATE preference SET ValueString='1,2,3' WHERE PrefName='RecallTypesShowingInList'";
        Db.NonQ(command);
        //Delete recalls for manually added recall types.  This is the same strategy we use in FormRecallTypeEdit
        //Types 1 through 6 were reinserted above, and thus the foreign keys will still be correct.
        command = "DELETE FROM recall WHERE RecallTypeNum < 1 OR RecallTypeNum > 7";
        Db.NonQ(command);
    }

    public static void SetToDefaultCA()
    {
        var command = "DELETE FROM recalltype WHERE RecallTypeNum >= 1 AND RecallTypeNum <= 5"; //Don't delete manually added recall types
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,TimePattern,Procedures,AppendToSpecial) VALUES (1,'Scaling',196609,'XXXX','11113',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,TimePattern,Procedures,AppendToSpecial) VALUES (2,'Scaling (Child)',0,'XX','11111,11117',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,TimePattern,Procedures,AppendToSpecial) VALUES (3,'Root Planing',196609,'XXXX','49101',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,Procedures,AppendToSpecial) VALUES (4,'Pan',50331648,'02601',1)";
        Db.NonQ(command);
        command = "INSERT INTO recalltype (RecallTypeNum,Description,DefaultInterval,Procedures,AppendToSpecial) VALUES (5,'Recall',393216,'11101,01202',1)";
        Db.NonQ(command);
        command = "TRUNCATE recalltrigger"; //OK to delete triggers for manually added recalls, because deleting the triggers disables the recall type.
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (1,3," + ProcedureCodes.GetCodeNum("43421") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (2,3," + ProcedureCodes.GetCodeNum("43422") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (3,3," + ProcedureCodes.GetCodeNum("43423") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (4,3," + ProcedureCodes.GetCodeNum("43424") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (5,3," + ProcedureCodes.GetCodeNum("49101") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (6,1," + ProcedureCodes.GetCodeNum("11114") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (7,1," + ProcedureCodes.GetCodeNum("11113") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (8,1," + ProcedureCodes.GetCodeNum("11112") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (9,1," + ProcedureCodes.GetCodeNum("11111") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (10,1," + ProcedureCodes.GetCodeNum("11117") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (15,4," + ProcedureCodes.GetCodeNum("02601") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (11,5," + ProcedureCodes.GetCodeNum("01202") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (12,5," + ProcedureCodes.GetCodeNum("01101") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (13,5," + ProcedureCodes.GetCodeNum("01102") + ")";
        Db.NonQ(command);
        command = "INSERT INTO recalltrigger (RecallTriggerNum,RecallTypeNum,CodeNum) VALUES (14,5," + ProcedureCodes.GetCodeNum("01103") + ")";
        Db.NonQ(command);
        //Update the special types in preference table.
        command = "UPDATE preference SET ValueString='1' WHERE PrefName='RecallTypeSpecialProphy'";
        Db.NonQ(command);
        command = "UPDATE preference SET ValueString='2' WHERE PrefName='RecallTypeSpecialChildProphy'";
        Db.NonQ(command);
        command = "UPDATE preference SET ValueString='3' WHERE PrefName='RecallTypeSpecialPerio'";
        Db.NonQ(command);
        command = "UPDATE preference SET ValueString='1,2,3' WHERE PrefName='RecallTypesShowingInList'";
        Db.NonQ(command);
        //Delete recalls for manually added recall types.  This is the same strategy we use in FormRecallTypeEdit
        //Types 1 through 5 were reinserted above, and thus the foreign keys will still be correct.
        command = "DELETE FROM recall WHERE RecallTypeNum < 1 OR RecallTypeNum > 5";
        Db.NonQ(command);
    }
    
    private class RecallTypeCache : CacheListAbs<RecallType>
    {
        protected override List<RecallType> GetCacheFromDb()
        {
            var command = "SELECT * FROM recalltype ORDER BY Description";
            return RecallTypeCrud.SelectMany(command);
        }

        protected override List<RecallType> TableToList(DataTable dataTable)
        {
            return RecallTypeCrud.TableToList(dataTable);
        }

        protected override RecallType Copy(RecallType item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<RecallType> items)
        {
            return RecallTypeCrud.ListToTable(items, "RecallType");
        }

        protected override void FillCacheIfNeeded()
        {
            RecallTypes.GetTableFromCache(false);
        }
    }
    
    private static readonly RecallTypeCache Cache = new();

    public static List<RecallType> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<RecallType> GetWhere(Predicate<RecallType> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static RecallType GetFirstOrDefault(Func<RecallType, bool> match, bool isShort = false)
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