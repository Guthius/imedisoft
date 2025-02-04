using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using Topaz;

namespace OpenDental.UI;

public class TopazWrapper
{
    public static Control GetTopaz()
    {
        var sigPlusNet = new SigPlusNET();
        
        sigPlusNet.HandleDestroyed += topazWrapper_HandleDestroyed;
        
        return sigPlusNet;
    }

    private static void topazWrapper_HandleDestroyed(object sender, EventArgs e)
    {
    }

    public static int GetTopazState(Control topaz)
    {
        return ((SigPlusNET) topaz).GetTabletState();
    }

    public static int GetTopazNumberOfTabletPoints(Control topaz)
    {
        return ((SigPlusNET) topaz).NumberOfTabletPoints();
    }

    public static string GetTopazString(Control topaz)
    {
        return ((SigPlusNET) topaz).GetSigString();
    }
    
    public static void ClearTopaz(Control topaz)
    {
        ((SigPlusNET) topaz).ClearTablet();
        ((SigPlusNET) topaz).SetTabletLogicalXSize(2000);
        ((SigPlusNET) topaz).SetTabletLogicalYSize(600);
    }

    public static void SetTopazCompressionMode(Control topaz, int compressionMode)
    {
        ((SigPlusNET) topaz).SetSigCompressionMode(compressionMode);
    }

    public static void SetTopazEncryptionMode(Control topaz, int encryptionMode)
    {
        ((SigPlusNET) topaz).SetEncryptionMode(encryptionMode);
    }

    public static void SetTopazKeyString(Control topaz, string str)
    {
        ((SigPlusNET) topaz).SetKeyString(str);
    }

    public static void SetTopazAutoKeyANSIData(Control topaz, string data)
    {
        ((SigPlusNET) topaz).AutoKeyStart();
        ((SigPlusNET) topaz).SetAutoKeyANSIData(data);
        ((SigPlusNET) topaz).AutoKeyFinish();
    }

    public static void SetTopazAutoKeyData(Control topaz, string data)
    {
        ((SigPlusNET) topaz).AutoKeyStart();
        ((SigPlusNET) topaz).SetAutoKeyData(data);
        ((SigPlusNET) topaz).AutoKeyFinish();
    }

    public static void SetTopazSigString(Control topaz, string signature)
    {
        try
        {
            ((SigPlusNET) topaz).SetSigString(signature);
        }
        catch
        {
            // ignored
        }
    }

    public static void SetTopazState(Control topaz, int state)
    {
        ((SigPlusNET) topaz).SetTabletState(state);
    }
    
    public static void FillSignatureANSI(Control topaz, string keyData, string signature, SignatureBoxWrapper.SigMode sigMode)
    {
        ClearTopaz(topaz);
        SetTopazCompressionMode(topaz, 0);
        SetTopazEncryptionMode(topaz, 0);
        SetTopazKeyString(topaz, "0000000000000000");
        SetTopazEncryptionMode(topaz, 2);
        SetTopazCompressionMode(topaz, 2);
        
        string hashedKeyData;
        switch (sigMode)
        {
            case SignatureBoxWrapper.SigMode.TreatPlan:
                hashedKeyData = TreatPlans.GetHashStringForSignature(keyData); 
                SetTopazAutoKeyANSIData(topaz, hashedKeyData);
                break;
            
            case SignatureBoxWrapper.SigMode.OrthoChart:
                hashedKeyData = OrthoCharts.GetHashStringForSignature(keyData);
                SetTopazAutoKeyANSIData(topaz, hashedKeyData);
                break;
            
            case SignatureBoxWrapper.SigMode.Document:
            case SignatureBoxWrapper.SigMode.Default:
            default:
                SetTopazAutoKeyANSIData(topaz, keyData);
                break;
        }

        SetTopazSigString(topaz, signature);
    }

    public static void FillSignatureEncodings(Control topaz, string keyData, string signature, SignatureBoxWrapper.SigMode sigMode)
    {
        string keyDataHashed;
        switch (sigMode)
        {
            case SignatureBoxWrapper.SigMode.TreatPlan:
                keyDataHashed = TreatPlans.GetHashStringForSignature(keyData);
                break;
            
            case SignatureBoxWrapper.SigMode.OrthoChart:
                keyDataHashed = OrthoCharts.GetHashStringForSignature(keyData);
                break;
            
            case SignatureBoxWrapper.SigMode.Document:
            case SignatureBoxWrapper.SigMode.Default:
            default:
                keyDataHashed = keyData;
                break;
        }

        Signature topazSignature;
        MethodInfo methodInfoSetAutoKeyData;
        try
        {
            var sigFieldInfo = typeof(SigPlusNET).GetField("Sig", BindingFlags.NonPublic | BindingFlags.Instance);
            
            topazSignature = (Signature) sigFieldInfo!.GetValue(topaz);
            
            methodInfoSetAutoKeyData = sigFieldInfo.FieldType.GetMethod("SetAutoKeyData", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(byte[])], null);
        }
        catch 
        {
            return;
        }

        var encodings = Encoding.GetEncodings().Select(x => x.GetEncoding()).ToList();
        foreach (var encoding in encodings)
        {
            ClearTopaz(topaz);
            SetTopazCompressionMode(topaz, 0);
            SetTopazEncryptionMode(topaz, 0);
            SetTopazKeyString(topaz, "0000000000000000"); 
            SetTopazEncryptionMode(topaz, 2); 
            SetTopazCompressionMode(topaz, 2);
            
            try
            {
                ((SigPlusNET) topaz).AutoKeyStart();
                
                methodInfoSetAutoKeyData.Invoke(topazSignature, [encoding.GetBytes(keyDataHashed)]);
                
                ((SigPlusNET) topaz).AutoKeyFinish();
            }
            catch
            {
                continue;
            }

            SetTopazSigString(topaz, signature);
            
            if (GetTopazNumberOfTabletPoints(topaz) > 0)
            {
                break;
            }
        }
    }
}