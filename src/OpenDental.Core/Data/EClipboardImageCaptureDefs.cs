using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EClipboardImageCaptureDefs
{
    public static List<EClipboardImageCaptureDef> Refresh()
    {
        return EClipboardImageCaptureDefCrud.SelectMany("SELECT * FROM eclipboardimagecapturedef");
    }

    public static void Sync(List<EClipboardImageCaptureDef> listEClipboardImageCaptureDefsNew, List<EClipboardImageCaptureDef> listEClipboardImageCaptureDefsOld)
    {
        EClipboardImageCaptureDefCrud.Sync(listEClipboardImageCaptureDefsNew, listEClipboardImageCaptureDefsOld);
    }

    public static bool IsEClipboardImageDefInUse(long defNum)
    {
        return Db.GetCount("SELECT COUNT(*) FROM eclipboardimagecapturedef WHERE DefNum=" + defNum) != "0";
    }
}