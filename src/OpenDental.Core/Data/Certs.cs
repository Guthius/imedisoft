using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class Certs
{
    public static List<Cert> GetAll(bool showHidden)
    {
        var command = "SELECT * FROM cert";
        if (!showHidden)
        {
            command += " WHERE IsHidden=0";
        }

        command += " ORDER BY ItemOrder";

        return CertCrud.SelectMany(command);
    }

    public static List<Cert> GetAllForCategory(long categoryNum)
    {
        return CertCrud.SelectMany("SELECT * FROM cert WHERE CertCategoryNum=" + categoryNum + " ORDER BY ItemOrder");
    }

    public static Cert GetOne(long certNum)
    {
        return CertCrud.SelectOne(certNum);
    }

    public static void Insert(Cert cert)
    {
        CertCrud.Insert(cert);
    }

    public static void Update(Cert cert)
    {
        CertCrud.Update(cert);
    }

    public static void Delete(long certNum)
    {
        CertCrud.Delete(certNum);
    }
}