using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CertCrud
{
    public static Cert SelectOne(long certNum)
    {
        var command = "SELECT * FROM cert "
                      + "WHERE CertNum = " + SOut.Long(certNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Cert> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Cert> TableToList(DataTable table)
    {
        var retVal = new List<Cert>();
        Cert cert;
        foreach (DataRow row in table.Rows)
        {
            cert = new Cert();
            cert.CertNum = SIn.Long(row["CertNum"].ToString());
            cert.Description = SIn.String(row["Description"].ToString());
            cert.WikiPageLink = SIn.String(row["WikiPageLink"].ToString());
            cert.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            cert.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            cert.CertCategoryNum = SIn.Long(row["CertCategoryNum"].ToString());
            retVal.Add(cert);
        }

        return retVal;
    }

    public static long Insert(Cert cert)
    {
        var command = "INSERT INTO cert (";

        command += "Description,WikiPageLink,ItemOrder,IsHidden,CertCategoryNum) VALUES(";

        command +=
            "'" + SOut.String(cert.Description) + "',"
            + "'" + SOut.String(cert.WikiPageLink) + "',"
            + SOut.Int(cert.ItemOrder) + ","
            + SOut.Bool(cert.IsHidden) + ","
            + SOut.Long(cert.CertCategoryNum) + ")";
        {
            cert.CertNum = Db.NonQ(command, true, "CertNum", "cert");
        }
        return cert.CertNum;
    }

    public static void Update(Cert cert)
    {
        var command = "UPDATE cert SET "
                      + "Description    = '" + SOut.String(cert.Description) + "', "
                      + "WikiPageLink   = '" + SOut.String(cert.WikiPageLink) + "', "
                      + "ItemOrder      =  " + SOut.Int(cert.ItemOrder) + ", "
                      + "IsHidden       =  " + SOut.Bool(cert.IsHidden) + ", "
                      + "CertCategoryNum=  " + SOut.Long(cert.CertCategoryNum) + " "
                      + "WHERE CertNum = " + SOut.Long(cert.CertNum);
        Db.NonQ(command);
    }

    public static void Delete(long certNum)
    {
        var command = "DELETE FROM cert "
                      + "WHERE CertNum = " + SOut.Long(certNum);
        Db.NonQ(command);
    }
}