using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class Certs
{
    #region Methods - Get

    public static List<Cert> GetAll(bool showHidden)
    {
        var command = "SELECT * FROM cert";
        if (!showHidden) command += " WHERE IsHidden=0";

        command += " ORDER BY ItemOrder";
        return CertCrud.SelectMany(command);
    }

    public static List<Cert> GetAllForCategory(long categoryNum)
    {
        var command = "SELECT * FROM cert WHERE CertCategoryNum=" + SOut.Long(categoryNum);
        command += " ORDER BY ItemOrder";
        return CertCrud.SelectMany(command);
    }

    public static Cert GetOne(long certNum)
    {
        return CertCrud.SelectOne(certNum);
    }

    #endregion Methods - Get

    #region Methods - Modify

    public static long Insert(Cert cert)
    {
        return CertCrud.Insert(cert);
    }

    public static void Update(Cert cert)
    {
        CertCrud.Update(cert);
    }

    public static void Delete(long certNum)
    {
        CertCrud.Delete(certNum);
    }

    #endregion Methods - Modify
}