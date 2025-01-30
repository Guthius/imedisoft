using System;
using System.Collections.Generic;
using System.IO;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EobAttaches
{
    public static List<EobAttach> Refresh(long claimPaymentNum)
    {
        return EobAttachCrud.SelectMany("SELECT * FROM eobattach WHERE ClaimPaymentNum=" + claimPaymentNum + " ORDER BY DateTCreated");
    }

    public static EobAttach GetOne(long eobAttachNum)
    {
        return EobAttachCrud.SelectOne(eobAttachNum);
    }

    public static bool Exists(long claimPaymentNum)
    {
        return DataCore.GetScalar("SELECT COUNT(*) FROM eobattach WHERE ClaimPaymentNum=" + claimPaymentNum) != "0";
    }

    public static void Insert(EobAttach eobAttach)
    {
        eobAttach.EobAttachNum = EobAttachCrud.Insert(eobAttach);

        if (eobAttach.FileName != Path.GetExtension(eobAttach.FileName))
        {
            return;
        }

        var extension = eobAttach.FileName;

        eobAttach.FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_") + eobAttach.EobAttachNum + extension;

        Update(eobAttach);
    }


    public static void Update(EobAttach eobAttach)
    {
        EobAttachCrud.Update(eobAttach);
    }

    public static void Delete(long eobAttachNum)
    {
        Db.NonQ("DELETE FROM eobattach WHERE EobAttachNum = " + eobAttachNum);
    }
}