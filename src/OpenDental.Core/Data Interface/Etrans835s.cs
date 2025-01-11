using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Etrans835s
{
    #region Methods - Get

    
    public static List<Etrans835> GetByEtransNums(params long[] longArrayEtransNums)
    {
        if (longArrayEtransNums.Length == 0) return new List<Etrans835>();
        var command = "SELECT * FROM etrans835 WHERE EtransNum IN(" + string.Join(",", longArrayEtransNums) + ")";
        return Etrans835Crud.SelectMany(command);
    }

    /// <summary>
    ///     All parameters are optional and will be excluded from the query if not set.
    ///     Strings are considered not set if blank, dates are considered not set if equal to DateTime.MinVal, decimals are not
    ///     set if negative.
    /// </summary>
    public static List<Etrans835> GetFiltered(DateTime dateFrom, DateTime dateTo, string carrierName, string checkTraceNum, decimal insPaidMin, decimal insPaidMax,
        string controlId, List<X835AutoProcessed> listX835AutoProcesseds = null, bool doIncludeAcknowledged = true, params X835Status[] x835StatusArray)
    {
        var command = "SELECT * FROM etrans835 "
                      + "INNER JOIN etrans on etrans.EtransNum=etrans835.EtransNum ";
        var listJoinClauses = new List<string>();
        if (carrierName != "") listJoinClauses.Add("LOWER(TRIM(etrans835.PayerName)) LIKE '%" + SOut.String(carrierName.ToLower().Trim()) + "%'");
        if (checkTraceNum != "") listJoinClauses.Add("LOWER(TRIM(etrans835.TransRefNum)) LIKE '%" + SOut.String(checkTraceNum.ToLower().Trim()) + "%'");
        if (insPaidMin >= 0) listJoinClauses.Add("etrans835.InsPaid >= " + SOut.Decimal(insPaidMin));
        if (insPaidMax >= 0) listJoinClauses.Add("etrans835.InsPaid <= " + SOut.Decimal(insPaidMax));
        if (controlId != "") listJoinClauses.Add("LOWER(TRIM(etrans835.ControlId)) LIKE '%" + SOut.String(controlId.ToLower().Trim()) + "%'");
        if (x835StatusArray.Length > 0) listJoinClauses.Add("etrans835.Status IN (" + string.Join(",", x835StatusArray.Select(x => (int) x)) + ")");
        if (!doIncludeAcknowledged) listJoinClauses.Add("etrans835.IsApproved=" + SOut.Bool(false));
        if (!listX835AutoProcesseds.IsNullOrEmpty()) listJoinClauses.Add("etrans835.AutoProcessed IN (" + string.Join(",", listX835AutoProcesseds.Select(x => (int) x)) + ")");
        if (listJoinClauses.Count > 0) command += " AND " + string.Join(" AND ", listJoinClauses);
        var listWhereClauses = new List<string>();
        if (dateFrom != DateTime.MinValue) listWhereClauses.Add(DbHelper.DtimeToDate("etrans.DateTimeTrans") + " >= " + SOut.Date(dateFrom));
        if (dateTo != DateTime.MinValue) listWhereClauses.Add(DbHelper.DtimeToDate("etrans.DateTimeTrans") + " <= " + SOut.Date(dateTo));
        if (listWhereClauses.Count > 0) command += " WHERE " + string.Join(" AND ", listWhereClauses);
        return Etrans835Crud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(Etrans835 etrans835)
    {
        return Etrans835Crud.Insert(etrans835);
    }

    
    public static void Update(Etrans835 etrans835, Etrans835 etrans835Old)
    {
        Etrans835Crud.Update(etrans835, etrans835Old);
    }

    public static void Upsert(Etrans835 etrans835, X835 x835, X835AutoProcessed x835AutoProcessed = X835AutoProcessed.None)
    {
        var etrans835Old = etrans835.Copy();
        if (x835AutoProcessed != X835AutoProcessed.None) etrans835.AutoProcessed = x835AutoProcessed;
        etrans835.PayerName = x835.PayerName;
        etrans835.TransRefNum = x835.TransRefNum;
        etrans835.InsPaid = (double) x835.InsPaid;
        etrans835.ControlId = x835.ControlId;
        etrans835.PaymentMethodCode = x835.PaymentMethodCode;
        var listPatNames = x835.ListClaimsPaid.Select(x => x.PatientName.ToString()).Distinct().ToList();
        etrans835.PatientName = "";
        if (listPatNames.Count > 0) etrans835.PatientName = listPatNames[0];
        if (listPatNames.Count > 1) etrans835.PatientName = "(" + SOut.Long(listPatNames.Count) + ")";
        etrans835.Status = x835.GetStatus();
        if (etrans835.Etrans835Num == 0)
            Insert(etrans835);
        else
            Update(etrans835, etrans835Old);
    }

    #endregion Methods - Modify
}