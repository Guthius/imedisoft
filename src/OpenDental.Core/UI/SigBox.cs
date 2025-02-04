using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CodeBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness.UI;

public class SigBox
{
    public static string GetSignatureKeySheets(List<SheetField> sheetFields)
    {
        var stringBuilder = new StringBuilder();
        
        foreach (var sheetField in sheetFields)
        {
            if (sheetField.FieldValue == "")
            {
                continue;
            }

            if (sheetField.FieldType is SheetFieldType.SigBox or SheetFieldType.SigBoxPractice)
            {
                continue;
            }

            stringBuilder.Append(sheetField.FieldValue);
        }

        return stringBuilder.ToString();
    }
    
    public static string EncryptSigString(byte[] key, string sigAsPoints)
    {
        if (string.IsNullOrWhiteSpace(sigAsPoints))
        {
            return "";
        }

        var bytes = Encoding.UTF8.GetBytes(sigAsPoints);
        
        using var memoryStream = new MemoryStream();
        using var rijndael = Rijndael.Create();
        
        rijndael.KeySize = 128; 
        rijndael.Key = key;
        rijndael.IV = new byte[16];
        
        using var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(bytes, 0, bytes.Length);
        cryptoStream.FlushFinalBlock();
        
        var encryptedBytes = new byte[memoryStream.Length];
        
        memoryStream.Position = 0;
        
        _ = memoryStream.Read(encryptedBytes, 0, (int) memoryStream.Length);
        
        return Convert.ToBase64String(encryptedBytes);
    }
}