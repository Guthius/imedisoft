using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

/// <summary>
///     In ProcGroupItems the ProcNum is a procedure in a group and GroupNum is the group the procedure is in.
///     GroupNum is a FK to the Procedure table. There is a special type of procedure with the procedure code "~GRP~" that
///     is used to indicate this is a group Procedure.
/// </summary>
public class ProcGroupItems
{
    #region Insert

    public static void InsertMany(List<ProcGroupItem> listGroupItems)
    {
        if (listGroupItems.IsNullOrEmpty()) return;

        ProcGroupItemCrud.InsertMany(listGroupItems);
    }

    #endregion

    
    public static List<ProcGroupItem> Refresh(long patNum)
    {
        var command = "SELECT procgroupitem.* FROM procgroupitem "
                      + "INNER JOIN procedurelog ON procedurelog.ProcNum=procgroupitem.GroupNum AND procedurelog.PatNum=" + SOut.Long(patNum);
        return ProcGroupItemCrud.SelectMany(command);
    }

    ///<summary>Gets all the ProcGroupItems linked to a patient.</summary>
    public static List<ProcGroupItem> GetPatientData(long patNum)
    {
        var command = "SELECT procgroupitem.* FROM procgroupitem "
                      + "INNER JOIN procedurelog ON procedurelog.ProcNum=procgroupitem.GroupNum AND procedurelog.PatNum=" + SOut.Long(patNum);
        return ProcGroupItemCrud.SelectMany(command);
    }

    ///<summary>Gets all the ProcGroupItems for a Procedure Group.</summary>
    public static List<ProcGroupItem> GetForGroup(long groupNum)
    {
        var command = "SELECT * FROM procgroupitem WHERE GroupNum = " + SOut.Long(groupNum) + " ORDER BY ProcNum ASC"; //Order is important for creating signature key in FormProcGroup.cs.
        return ProcGroupItemCrud.SelectMany(command);
    }

    ///<summary>Adds a procedure to a group.</summary>
    public static long Insert(ProcGroupItem procGroupItem)
    {
        return ProcGroupItemCrud.Insert(procGroupItem);
    }

    ///<summary>Deletes a ProcGroupItem based on its procGroupItemNum.</summary>
    public static void Delete(long procGroupItemNum)
    {
        DeleteMany(new List<long> {procGroupItemNum});
    }

    ///<summary>Deletes many ProcGroupItems based on the given list of ProcGroupItemNums.</summary>
    public static void DeleteMany(List<long> listProcGroupItemNums)
    {
        if (listProcGroupItemNums.IsNullOrEmpty()) return;

        var command = $@"
					DELETE
					FROM procgroupitem
					WHERE ProcGroupItemNum IN({string.Join(",", listProcGroupItemNums.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Returns a count of the number of procedures attached to a group note.  Takes the ProcNum of a group note.
    ///     Used when deleting group notes to determine which permission to check. If a list of complete statuses is not
    ///     included, will default to
    ///     C, EO, and EC.
    /// </summary>
    public static int GetCountCompletedProcsForGroup(long groupNum, List<ProcStat> listStatusComplete = null)
    {
        if (listStatusComplete == null) listStatusComplete = new List<ProcStat> {ProcStat.C, ProcStat.EO, ProcStat.EC};
        var command = $@"
				SELECT COUNT(*) 
				FROM procgroupitem 
				INNER JOIN procedurelog 
					ON procedurelog.ProcNum=procgroupitem.ProcNum
					AND procedurelog.ProcStatus IN ({string.Join(",", listStatusComplete.Select(x => SOut.Int((int) x)))}) 
				WHERE GroupNum = {SOut.Long(groupNum)}";
        return SIn.Int(Db.GetCount(command));
    }

    /// <summary>
    ///     For the procnums passed in, returns a dictionary containing all grouped Procedures with the given statuses. The key
    ///     is the groupNum.
    ///     If no attached group nums, the group num will not be in the dictionary.
    /// </summary>
    public static Dictionary<long, List<Procedure>> GetCompletedProcsForGroups(List<long> listProcNums, List<ProcStat> listStatusComplete)
    {
        if (listProcNums.IsNullOrEmpty() || listStatusComplete.IsNullOrEmpty()) return new Dictionary<long, List<Procedure>>();

        var command = $@"
				SELECT procedurelog.*,procgroupitem.GroupNum ProcGroupItemGroupNum
				FROM procgroupitem 
				INNER JOIN procedurelog 
					ON procedurelog.ProcNum=procgroupitem.ProcNum
					AND procedurelog.ProcStatus IN ({string.Join("", listStatusComplete.Select(x => SOut.Int((int) x)))}) 
				WHERE procgroupitem.GroupNum IN ({string.Join(",", listProcNums)})";
        var table = DataCore.GetTable(command);
        var listProcs = ProcedureCrud.TableToList(table);
        var retVal = new Dictionary<long, List<Procedure>>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var groupNum = SIn.Long(table.Rows[i]["ProcGroupItemGroupNum"].ToString());
            if (!retVal.ContainsKey(groupNum)) retVal[groupNum] = new List<Procedure>();
            retVal[groupNum].Add(listProcs[i]);
        }

        return retVal;
    }

    /*
    #region CachePattern

    private class ProcGroupItemCache : CacheListAbs<ProcGroupItem> {
        protected override List<ProcGroupItem> GetCacheFromDb() {
            string command="SELECT * FROM ProcGroupItem ORDER BY ItemOrder";
            return Crud.ProcGroupItemCrud.SelectMany(command);
        }
        protected override List<ProcGroupItem> TableToList(DataTable table) {
            return Crud.ProcGroupItemCrud.TableToList(table);
        }
        protected override ProcGroupItem Copy(ProcGroupItem ProcGroupItem) {
            return ProcGroupItem.Clone();
        }
        protected override DataTable ListToTable(List<ProcGroupItem> listProcGroupItems) {
            return Crud.ProcGroupItemCrud.ListToTable(listProcGroupItems,"ProcGroupItem");
        }
        protected override void FillCacheIfNeeded() {
            ProcGroupItems.GetTableFromCache(false);
        }
        protected override bool IsInListShort(ProcGroupItem ProcGroupItem) {
            return !ProcGroupItem.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static ProcGroupItemCache _ProcGroupItemCache=new ProcGroupItemCache();

    ///<summary>A list of all ProcGroupItems. Returns a deep copy.</summary>
    public static List<ProcGroupItem> ListDeep {
        get {
            return _ProcGroupItemCache.ListDeep;
        }
    }

    ///<summary>A list of all visible ProcGroupItems. Returns a deep copy.</summary>
    public static List<ProcGroupItem> ListShortDeep {
        get {
            return _ProcGroupItemCache.ListShortDeep;
        }
    }

    ///<summary>A list of all ProcGroupItems. Returns a shallow copy.</summary>
    public static List<ProcGroupItem> ListShallow {
        get {
            return _ProcGroupItemCache.ListShallow;
        }
    }

    ///<summary>A list of all visible ProcGroupItems. Returns a shallow copy.</summary>
    public static List<ProcGroupItem> ListShort {
        get {
            return _ProcGroupItemCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _appointmentTypeCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _appointmentTypeCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.



    ///<summary>Gets one ProcGroupItem from the db.</summary>
    public static ProcGroupItem GetOne(long procGroupItemNum){

        return Crud.ProcGroupItemCrud.SelectOne(procGroupItemNum);
    }

    
    public static void Update(ProcGroupItem procGroupItem){

        Crud.ProcGroupItemCrud.Update(procGroupItem);
    }

    
    public static void Delete(long procGroupItemNum) {

        string command= "DELETE FROM procgroupitem WHERE ProcGroupItemNum = "+POut.Long(procGroupItemNum);
        Db.NonQ(command);
    }
    */
}