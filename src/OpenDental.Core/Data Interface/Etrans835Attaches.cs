using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Etrans835Attaches
{
	/// <summary>
	///     Get all claim attachments for every 835 in the list.  Ran as a batch for efficiency purposes.
	///     Returned list is ordered by Etrans835Attach.DateTimeEntry, this is very important when identifying claims split
	///     from an ERA.
	/// </summary>
	public static List<Etrans835Attach> GetForClaimNums(params long[] listClaimNums)
    {
        return GetForEtransNumOrClaimNums(true, 0, listClaimNums);
    }

	/// <summary>
	///     Returns a list of Etrans835Attach for the given etransNum and/or listClaimNums.
	///     Set isSimple to false to run a simpiler query and if attach.DateTimeTrans is not needed.
	///     Returned list is ordered by Etrans835Attach.DateTimeEntry, this is very important when identifying claims split
	///     from an ERA.
	/// </summary>
	public static List<Etrans835Attach> GetForEtransNumOrClaimNums(bool isSimple, long etransNum = 0, params long[] listClaimNums)
    {
        return GetForEtransNumOrClaimNums(isSimple, new List<long> {etransNum}, listClaimNums);
    }

	/// <summary>
	///     Returns a list of Etrans835Attach for the given list of etransNums and/or listClaimNums.
	///     Set isSimple to false to run a simpiler query and if attach.DateTimeTrans is not needed.
	///     Returned list is ordered by Etrans835Attach.DateTimeEntry, this is very important when identifying claims split
	///     from an ERA.
	/// </summary>
	public static List<Etrans835Attach> GetForEtransNumOrClaimNums(bool isSimple, List<long> listEtransNums = null, params long[] listClaimNums)
    {
        if ((listEtransNums == null || listEtransNums.Count == 0) && (listClaimNums == null || listClaimNums.Length == 0)) return new List<Etrans835Attach>(); //Both are either not defined or contain no information, there would be no WHERE clause.
        var listWhereClauses = new List<string>();
        if (listClaimNums.Length != 0) listWhereClauses.Add("etrans835attach.ClaimNum IN (" + string.Join(",", listClaimNums.Select(x => SOut.Long(x))) + ")");
        if (!isSimple && listEtransNums != null && listEtransNums.Count > 0) //Includes manually detached and split attaches created when spliting procs from ERA.
            listWhereClauses.Add("etrans.EtransNum IN (" + string.Join(",", listEtransNums.Select(x => SOut.Long(x))) + ")");
        if (listWhereClauses.Count == 0) return new List<Etrans835Attach>();
        var command = "SELECT etrans835attach.* ";
        if (!isSimple) command += ",etrans.DateTimeTrans ";
        command += "FROM etrans835attach ";
        if (!isSimple) command += "INNER JOIN etrans ON etrans.EtransNum=etrans835attach.EtransNum ";
        command += "WHERE " + string.Join(" OR ", listWhereClauses) + " "
                   + "ORDER BY etrans835attach.DateTimeEntry"; //Attaches created from splitting an ERA need to be after the original claim attach.
        var table = DataCore.GetTable(command);
        if (isSimple) return Etrans835AttachCrud.TableToList(table);
        var listEtrans835Attaches = Etrans835AttachCrud.TableToList(table);
        for (var i = 0; i < listEtrans835Attaches.Count; i++)
        {
            var etrans835Attach = listEtrans835Attaches[i];
            var dataRow = table.Rows[i];
            etrans835Attach.DateTimeTrans = SIn.DateTime(dataRow["DateTimeTrans"].ToString());
        }

        return listEtrans835Attaches;
    }

	/// <summary>
	///     Get all claim attachments for every 835 in the list.  Ran as a batch for efficiency purposes.
	///     Returned list is ordered by Etrans835Attach.DateTimeEntry, this is very important when identifying claims split
	///     from an ERA.
	/// </summary>
	/// </summary>
	public static List<Etrans835Attach> GetForEtrans(params long[] listEtrans835Nums)
    {
        return GetForEtrans(true, listEtrans835Nums);
    }

	/// <summary>
	///     Returns a list of Etrans835Attachs for given etransNums.
	///     Set isSimple to false to run a simpiler query and if attach.DateTimeTrans is not needed.
	///     Returned list is ordered by Etrans835Attach.DateTimeEntry, this is very important when identifying claims split
	///     from an ERA.
	/// </summary>
	public static List<Etrans835Attach> GetForEtrans(bool isSimple, params long[] listEtrans835Nums)
    {
        if (listEtrans835Nums.Length == 0) return new List<Etrans835Attach>();
        var command = "SELECT etrans835attach.* ";
        if (!isSimple) command += ",etrans.DateTimeTrans,claim.ClinicNum,insplan.CarrierNum ";
        command += "FROM etrans835attach ";
        if (!isSimple)
            command += "INNER JOIN etrans ON etrans.EtransNum=etrans835attach.EtransNum "
                       + "INNER JOIN claim ON claim.ClaimNum=etrans835attach.ClaimNum "
                       + "INNER JOIN insplan ON insplan.PlanNum=claim.PlanNum ";
        command += "WHERE etrans835attach.EtransNum IN (" + string.Join(",", listEtrans835Nums.Select(x => SOut.Long(x))) + ") "
                   + "ORDER BY etrans835attach.DateTimeEntry"; //Attaches created from splitting an ERA need to be after the original claim attach.
        var table = DataCore.GetTable(command);
        if (isSimple) return Etrans835AttachCrud.TableToList(table);
        var listEtrans835Attaches = Etrans835AttachCrud.TableToList(table);
        for (var i = 0; i < listEtrans835Attaches.Count; i++)
        {
            var etrans835Attach = listEtrans835Attaches[i];
            var dataRow = table.Rows[i];
            etrans835Attach.DateTimeTrans = SIn.DateTime(dataRow["DateTimeTrans"].ToString());
            etrans835Attach.ClinicNum = SIn.Long(dataRow["ClinicNum"].ToString());
            etrans835Attach.CarrierNum = SIn.Long(dataRow["CarrierNum"].ToString());
        }

        return listEtrans835Attaches;
    }

    ///<summary>Create a single attachment for a claim to an 835.</summary>
    public static long Insert(Etrans835Attach etrans835Attach)
    {
        return Etrans835AttachCrud.Insert(etrans835Attach);
    }

    /// <summary>
    ///     Delete the attachment for the claim currently attached to the 835 with the specified segment index.
    ///     Safe to run even if no claim is currently attached at the specified index.
    ///     Set clpSegmentIndex equal to a negative number if not
    /// </summary>
    public static void DeleteMany(int clpSegmentIndex, params long[] arrayEtranNums)
    {
        if (arrayEtranNums.Length == 0) return;

        var command = "DELETE FROM etrans835attach "
                      + "WHERE EtransNum IN (" + string.Join(",", arrayEtranNums.Select(x => SOut.Long(x))) + ")";
        if (clpSegmentIndex >= 0) command += " AND ClpSegmentIndex=" + SOut.Int(clpSegmentIndex);
        Db.NonQ(command);
    }

    ///<summary>Deletes all attachments associated to the given listEtrans835AttachNums.  Can handle null.</summary>
    public static void DeleteMany(List<long> listEtrans835AttachNums)
    {
        if (listEtrans835AttachNums == null || listEtrans835AttachNums.Count == 0) return;

        Db.NonQ("DELETE FROM etrans835attach WHERE Etrans835AttachNum IN (" + string.Join(",", listEtrans835AttachNums.Select(x => SOut.Long(x))) + ")");
    }

    public static void DetachEraClaim(Hx835_Claim hx835_Claim)
    {
        DeleteMany(hx835_Claim.ClpSegmentIndex, hx835_Claim.Era.EtransSource.EtransNum);
        var etrans835Attach = new Etrans835Attach();
        etrans835Attach.EtransNum = hx835_Claim.Era.EtransSource.EtransNum;
        etrans835Attach.ClaimNum = 0;
        etrans835Attach.ClpSegmentIndex = hx835_Claim.ClpSegmentIndex;
        Insert(etrans835Attach);
        hx835_Claim.IsAttachedToClaim = true;
        hx835_Claim.ClaimNum = 0;
    }

    /// <summary>
    ///     Inserts new Etrans835Attach for given claimPaid and claim.
    ///     Deletes any existing Etrans835Attach prior to inserting new one.
    ///     Sets claimPaid.ClaimNum and claimPaid.IsAttachedToClaim.
    ///     Removes deleted attaches from list and adds a new one if it is created when canModifyList is true.
    /// </summary>
    public static void CreateForClaim(X835 x835, Hx835_Claim hx835_Claim,
        long claimNum, bool isNewAttachNeeded, List<Etrans835Attach> listEtrans835Attaches, bool canModifyList = false)
    {
        if (!isNewAttachNeeded
            && listEtrans835Attaches.Exists(
                x => x.ClaimNum == claimNum
                     && x.EtransNum == x835.EtransSource.EtransNum
                     && x.ClpSegmentIndex == hx835_Claim.ClpSegmentIndex))
            //Not forcing a new attach and one already exists.
            return;
        //Create a hard link between the selected claim and the claim info on the 835.
        DeleteMany(hx835_Claim.ClpSegmentIndex, x835.EtransSource.EtransNum); //Detach existing if any.
        //Remove deleted attaches from list.
        if (canModifyList) listEtrans835Attaches.RemoveAll(x => x.EtransNum == x835.EtransSource.EtransNum && x.ClpSegmentIndex == hx835_Claim.ClpSegmentIndex);
        var etrans835Attach = new Etrans835Attach();
        etrans835Attach.EtransNum = x835.EtransSource.EtransNum;
        etrans835Attach.ClaimNum = claimNum;
        etrans835Attach.ClpSegmentIndex = hx835_Claim.ClpSegmentIndex;
        Insert(etrans835Attach);
        hx835_Claim.ClaimNum = claimNum;
        hx835_Claim.IsAttachedToClaim = true;
        if (canModifyList) listEtrans835Attaches.Add(etrans835Attach);
    }

    /// <summary>
    ///     This should only be called with an X835 for a newly imported Etrans that was just inserted into the DB.
    ///     For the X835 passed in, the X835.EtransSource.EtransNum must be set with the Etrans.EtransNum. It should not be
    ///     zero.
    /// </summary>
    public static void CreateManyForNewEra(X835 x835)
    {
        for (var i = 0; i < x835.ListClaimsPaid.Count; i++)
        {
            if (x835.ListClaimsPaid[i].ClaimNum == 0) continue; //No matched claim to attach
            CreateForClaim(x835, x835.ListClaimsPaid[i], x835.ListClaimsPaid[i].ClaimNum, true, new List<Etrans835Attach>());
        }
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    
    public static List<Etrans835Attach> Refresh(long patNum){

        string command="SELECT * FROM etrans835attach WHERE PatNum = "+POut.Long(patNum);
        return Crud.Etrans835AttachCrud.SelectMany(command);
    }

    ///<summary>Gets one Etrans835Attach from the db.</summary>
    public static Etrans835Attach GetOne(long etrans835AttachNum){

        return Crud.Etrans835AttachCrud.SelectOne(etrans835AttachNum);
    }
    
    public static void Update(Etrans835Attach etrans835Attach){

        Crud.Etrans835AttachCrud.Update(etrans835Attach);
    }
    
    public static void Delete(long etrans835AttachNum) {

        Crud.Etrans835AttachCrud.Delete(etrans835AttachNum);
    }
    */
}