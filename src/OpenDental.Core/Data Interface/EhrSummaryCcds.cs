using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrSummaryCcds
{
	/// <summary>
	///     This will be null if EHR didn't load up.  EHRTEST conditional compilation constant is used because the EHR
	///     project is only part of the solution here at HQ.  We need to use late binding in a few places so that it will still
	///     compile for people who download our sourcecode.  But late binding prevents us from stepping through for debugging,
	///     so the EHRTEST lets us switch to early binding.
	/// </summary>
	public static object ObjFormEhrMeasures;

    ///<summary>This will be null if EHR didn't load up.</summary>
    public static Assembly AssemblyEHR;

    
    public static List<EhrSummaryCcd> Refresh(long patNum)
    {
        var command = "SELECT * FROM ehrsummaryccd WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY DateSummary";
        return EhrSummaryCcdCrud.SelectMany(command);
    }

    
    public static long Insert(EhrSummaryCcd ehrSummaryCcd)
    {
        return EhrSummaryCcdCrud.Insert(ehrSummaryCcd);
    }

    ///<summary>Returns null if no record is found.</summary>
    public static EhrSummaryCcd GetOneForEmailAttach(long emailAttachNum)
    {
        var command = "SELECT * FROM ehrsummaryccd WHERE EmailAttachNum=" + SOut.Long(emailAttachNum) + " LIMIT 1";
        return EhrSummaryCcdCrud.SelectOne(command);
    }

    
    public static void Update(EhrSummaryCcd ehrSummaryCcd)
    {
        EhrSummaryCcdCrud.Update(ehrSummaryCcd);
    }

    ///<summary>Constructs the ObjFormEhrMeasures fro use with late binding.</summary>
    private static void constructObjFormEhrMeasuresHelper()
    {
        var dllPathEHR = ODFileUtils.CombinePaths(Application.StartupPath, "EHR.dll");
        ObjFormEhrMeasures = null;
        AssemblyEHR = null;
        if (File.Exists(dllPathEHR))
        {
            //EHR.dll is available, so load it up
            AssemblyEHR = Assembly.LoadFile(dllPathEHR);
            var type = AssemblyEHR.GetType("EHR.FormEhrMeasures"); //namespace.class
            ObjFormEhrMeasures = Activator.CreateInstance(type);
        }
#if EHRTEST
				ObjFormEhrMeasures = new FormEhrMeasures();
#endif
    }

    /// <summary>
    ///     Loads a resource file from the EHR assembly and returns the file text as a string.
    ///     Returns "" is the EHR assembly did not load. strResourceName can be either "CCD" or "CCR".
    ///     This function performs a late binding to the EHR.dll, because resellers do not have EHR.dll necessarily.
    /// </summary>
    public static string GetEhrResource(string strResourceName)
    {
        if (AssemblyEHR == null)
        {
            constructObjFormEhrMeasuresHelper();
            if (AssemblyEHR == null) return "";
        }

        var stream = AssemblyEHR.GetManifestResourceStream("EHR.Properties.Resources.resources");
        var resourceReader = new ResourceReader(stream);
        var strResourceType = "";
        byte[] byteArrayResource = null;
        resourceReader.GetResourceData(strResourceName, out strResourceType, out byteArrayResource);
        resourceReader.Dispose();
        stream.Dispose();
        var memoryStream = new MemoryStream(byteArrayResource);
        var binaryReader = new BinaryReader(memoryStream);
        var retVal = binaryReader.ReadString(); //Removes the leading binary characters from the string.
        memoryStream.Dispose();
        binaryReader.Dispose();
        return retVal;
    }

    #region CachePattern

    private class EhrSummaryCcdCache : CacheListAbs<EhrSummaryCcd>
    {
        protected override List<EhrSummaryCcd> GetCacheFromDb()
        {
            var command = "SELECT * FROM ehrsummaryccd";
            return EhrSummaryCcdCrud.SelectMany(command);
        }

        protected override List<EhrSummaryCcd> TableToList(DataTable dataTable)
        {
            return EhrSummaryCcdCrud.TableToList(dataTable);
        }

        protected override EhrSummaryCcd Copy(EhrSummaryCcd item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EhrSummaryCcd> items)
        {
            return EhrSummaryCcdCrud.ListToTable(items, "EhrSummaryCcd");
        }

        protected override void FillCacheIfNeeded()
        {
            EhrSummaryCcds.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EhrSummaryCcdCache _ehrSummaryCcdCache = new();

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
        _ehrSummaryCcdCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _ehrSummaryCcdCache.GetTableFromCache(doRefreshCache);
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.


    ///<summary>Gets one EhrSummaryCcd from the db.</summary>
    public static EhrSummaryCcd GetOne(long ehrSummaryCcdNum){

        return Crud.EhrSummaryCcdCrud.SelectOne(ehrSummaryCcdNum);
    }

    
    public static void Delete(long ehrSummaryCcdNum) {

        string command= "DELETE FROM ehrsummaryccd WHERE EhrSummaryCcdNum = "+POut.Long(ehrSummaryCcdNum);
        Db.NonQ(command);
    }
    */
}