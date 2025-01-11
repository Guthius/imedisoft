using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CommOptOuts
{
    ///<summary>Returns an entry from the db, or a new instance(not in db yet) if not found.</summary>
    public static CommOptOut Refresh(long patNum)
    {
        var commOptOut = GetForPat(patNum);
        if (commOptOut != null) return commOptOut;

        commOptOut = new CommOptOut();
        commOptOut.PatNum = patNum;
        return commOptOut;
    }

    ///<summary>Returns an entry from the db for the given patNum.</summary>
    public static CommOptOut GetForPat(long patNum)
    {
        var command = "SELECT * FROM commoptout WHERE PatNum = " + SOut.Long(patNum);
        return CommOptOutCrud.SelectOne(command);
    }

    public static List<CommOptOut> GetForPats(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<CommOptOut>();

        var command = "SELECT * FROM commoptout WHERE PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ") ";
        return CommOptOutCrud.SelectMany(command);
    }


    
    public static void InsertMany(List<CommOptOut> listCommOptOuts)
    {
        if (listCommOptOuts.Count == 0) return;

        CommOptOutCrud.InsertMany(listCommOptOuts);
    }

    
    public static void Update(CommOptOut commOptOutNew, CommOptOut commOptOutOld)
    {
        CommOptOutCrud.Update(commOptOutNew, commOptOutOld);
    }

    public static void Upsert(CommOptOut commOptOut)
    {
        //This assures one-to-one with patient table.  Also safe for concurrent editing.
        CommOptOut commOptOutDb = null;
        if (commOptOut.PatNum > 0) commOptOutDb = GetForPat(commOptOut.PatNum);

        if (commOptOutDb is null)
        {
            var listCommOptOuts = new List<CommOptOut>();
            listCommOptOuts.Add(commOptOut);
            InsertMany(listCommOptOuts);
            return;
        }

        Update(commOptOut, commOptOutDb);
    }
}