using CodeBase;
using System;
using System.Collections.Generic;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness.WebTypes.WebForms;

public class WebForms_SheetDefs
{
    public static bool TryDownloadSheetDefs(out List<WebForms_SheetDef> listWebFormSheetDefs, string regKey = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        listWebFormSheetDefs = [];
        try
        {
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, new PayloadItem(regKey, "RegKey"));
            listWebFormSheetDefs = WebSerializer.DeserializeTag<List<WebForms_SheetDef>>(
                SheetsSynchProxy.GetWebServiceInstance().DownloadSheetDefs(payload), "Success"
            );
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static void TryUploadSheetDef(SheetDef sheetDef)
    {
        var regKey = PrefC.GetString(PrefName.RegistrationKey);
        var listPayloadItems = new List<PayloadItem>
        {
            new(regKey, "RegKey"),
            new(sheetDef, "SheetDef")
        };
        var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        var result = SheetsSynchProxy.GetWebServiceInstance().UpLoadSheetDef(payload);
        PayloadHelper.CheckForError(result);
    }

    public static void TryUploadSheetDefChunked(SheetDef sheetDef, int chunkSize)
    {
        var regKey = PrefC.GetString(PrefName.RegistrationKey);
        var listPayloadItems = new List<PayloadItem>
        {
            new(regKey, "RegKey"),
            new(sheetDef, "SheetDef")
        };
        var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        var fileNamePayload = UploadSheetChunks(payload, chunkSize);
        var result = SheetsSynchProxy.GetWebServiceInstance().UploadSheetDefFromFile(fileNamePayload);
        PayloadHelper.CheckForError(result);
    }

    public static bool DeleteSheetDef(long webSheetDefId, string regKey = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var listPayloadItems = new List<PayloadItem>
            {
                new(regKey, "RegKey"),
                new(webSheetDefId, "WebSheetDefID")
            };
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
            SheetsSynchProxy.GetWebServiceInstance().DeleteSheetDef(payload);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static void UpdateSheetDef(long webSheetDefId, SheetDef sheetDef, string regKey = null, bool doCatchExceptions = true)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var listPayloadItems = new List<PayloadItem>
            {
                new(regKey, "RegKey"),
                new(webSheetDefId, "WebSheetDefID"),
                new(sheetDef, "SheetDef")
            };
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
            SheetsSynchProxy.GetWebServiceInstance().UpdateSheetDef(payload);
        }
        catch
        {
            if (!doCatchExceptions)
            {
                throw;
            }
        }
    }

    public static void UpdateSheetDefChunked(long webSheetDefId, SheetDef sheetDef, int chunkSize)
    {
        var regKey = PrefC.GetString(PrefName.RegistrationKey);
        var listPayloadItems = new List<PayloadItem>
        {
            new(regKey, "RegKey"),
            new(webSheetDefId, "WebSheetDefID"),
            new(sheetDef, "SheetDef")
        };
        var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
        var fileNamePayload = UploadSheetChunks(payload, chunkSize);
        var result = SheetsSynchProxy.GetWebServiceInstance().UpdateSheetDefFromFile(fileNamePayload);
        PayloadHelper.CheckForError(result);
    }

    private static string UploadSheetChunks(string payload, int chunkSize)
    {
        var listChunks = MiscUtils.CutStringIntoSimilarSizedChunks(payload, chunkSize);
        var fileName = "";
        foreach (var chunk in listChunks)
        {
            var listChunkPayloadItems = new List<PayloadItem>
            {
                new(fileName, "FileName"),
                new(chunk, "ChunkData")
            };
            var chunkPayload = PayloadHelper.CreatePayloadContent(listChunkPayloadItems);
            var result = SheetsSynchProxy.GetWebServiceInstance().UploadSheetDefChunk(chunkPayload);
            PayloadHelper.CheckForError(result);
            fileName = WebSerializer.DeserializeTag<string>(result, "FileName");
        }

        return PayloadHelper.CreatePayloadContent(fileName, "FileName");
    }
}