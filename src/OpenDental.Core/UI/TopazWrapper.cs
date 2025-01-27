using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using Topaz;

namespace OpenDental.UI {
	public class TopazWrapper {

		#region GetTopaz... Methods
		//The plugin calls in this class were added in order to use a Topaz pad in a RDP/Citrix remote session environment so that signature pads can be
		//plugged in on the client computer while the signature displays in the SigPlusNET UI control on the terminal server.  The signature pad and the
		//UI control are always kept in sync, i.e. the state, encryption and compression modes, key data and key string, signature string etc.  This means
		//the GetTopaz... methods could retrieve data from the UI control and not the remote signature pad.  The GetTopazNumberOfTabletPoints method returns
		//0 if sent to the remote device, so it must be called on the local UI control.

		public static Control GetTopaz() {
			Control retVal=new SigPlusNET();
			retVal.HandleDestroyed+=new EventHandler(topazWrapper_HandleDestroyed);
			return retVal;
		}

		///<summary>This allows third-party software to unload device/clean-up.</summary>
		private static void topazWrapper_HandleDestroyed(object sender,EventArgs e) {
		}

		///<summary>0=disable signature capture.  1=enable capture.</summary>
		public static int GetTopazState(Control topaz) {
			var stateCur = ((SigPlusNET)topaz).GetTabletState();
			return stateCur;
		}

		///<summary>Returns 0 if the signature is not valid, i.e. the compression or encryption modes are not the same used when the signature was saved,
		///or the key data is different than that used to save the signature, or the signature string is blank, etc.  Valid signatures should consist of
		///at least 200 points, as this represents approximately 1 second of active signature time.  We will consider any value greater than 0 to mean a
		///signature is valid.</summary>
		public static int GetTopazNumberOfTabletPoints(Control topaz) {
			return ((SigPlusNET)topaz).NumberOfTabletPoints();
		}

		public static string GetTopazString(Control topaz) {
			//if in remote session, always get the sig string from the UI control, not the remote sig pad, because a form can have more than one sig box
			//and the sig pad will only have the most recent sig string.
			return ((SigPlusNET)topaz).GetSigString();
		}
		#endregion GetTopaz... Methods

		#region SetTopaz... Methods
		//The SetTopaz... methods have a plugin call in order for HSB signature pads to be used in a RDP/Citrix remote session environment.  It may not be
		//necessary to send all of the SetTopaz... methods to the device connected to the client machine.  The encryption and compression modes and the
		//key data and key string are only necessary when saving the signature, not during signature capture.  Since the captured signature is duplicated
		//on the server in the SigPlusNET control in the OD UI, we can set the compression, encryption, key data, and key string on the control before
		//getting the signature string and validate it using the control as well.  We will leave the plugin calls just in case they are needed in the future.

		public static void ClearTopaz(Control topaz) {
			((SigPlusNET)topaz).ClearTablet();
			((SigPlusNET)topaz).SetTabletLogicalXSize(2000);
			((SigPlusNET)topaz).SetTabletLogicalYSize(600);
		}

		public static void SetTopazCompressionMode(Control topaz,int compressionMode) {
			((SigPlusNET)topaz).SetSigCompressionMode(compressionMode);
		}

		public static void SetTopazEncryptionMode(Control topaz,int encryptionMode) {
			((SigPlusNET)topaz).SetEncryptionMode(encryptionMode);
		}

		public static void SetTopazKeyString(Control topaz,string str) {
			((SigPlusNET)topaz).SetKeyString(str);
		}

		public static void SetTopazAutoKeyANSIData(Control topaz,string data) {
			((SigPlusNET)topaz).AutoKeyStart();
			((SigPlusNET)topaz).SetAutoKeyANSIData(data);
			((SigPlusNET)topaz).AutoKeyFinish();
		}

		///<summary>Topaz Systems Inc. deprecated SetAutoKeyData() and AutoKeyAddData(). Use SetAutoKeyANSIData() instead.</summary>
		public static void SetTopazAutoKeyData(Control topaz,string data) {
			((SigPlusNET)topaz).AutoKeyStart();
			((SigPlusNET)topaz).SetAutoKeyData(data);
			((SigPlusNET)topaz).AutoKeyFinish();
		}

		public static void SetTopazSigString(Control topaz,string signature) {
			try {
				((SigPlusNET)topaz).SetSigString(signature);
			}
			catch(Exception ex) {
				//The signature could have become corrupted within the database (or from wherever it is coming from).
				//The Topaz dll can throw exceptions for invalid signatures even if the signature data is a valid ASCII string.
				//E.g. A customer had a plethora of treatment plan signatures with a value of '1' which is a valid ASCII string but is not a valid signature.
				//This caused Topaz to throw a System.IndexOutOfRangeException: 'Index was outside the bounds of the array.'
				//No 'tablet points' should be present on the signature pad at this time which is how calling methods will know the signature was invalid.
			}
			//this hook shouldn't be implemented or necessary, we only set/get the sig string on the local UI control, so this command shouldn't be sent to
			//the remote connection sig pad
		}

		///<summary>0=disable signature capture.  1=enable.</summary>
		public static void SetTopazState(Control topaz,int state) {
			((SigPlusNET)topaz).SetTabletState(state);
		}
		#endregion SetTopaz... Methods

		#region Helper Methods

		public static void FillSignatureANSI(Control topaz,string keyData,string signature,OpenDental.UI.SignatureBoxWrapper.SigMode sigMode) {
			ClearTopaz(topaz);
			SetTopazCompressionMode(topaz,0);
			SetTopazEncryptionMode(topaz,0);
			SetTopazKeyString(topaz,"0000000000000000");//Clear out the key string
			SetTopazEncryptionMode(topaz,2);//high encryption
			SetTopazCompressionMode(topaz,2);//high compression
			string hashedKeyData;
			switch(sigMode) {
				case OpenDental.UI.SignatureBoxWrapper.SigMode.TreatPlan:
					hashedKeyData=TreatPlans.GetHashStringForSignature(keyData);//Passed in key data has not been hashed yet.
					SetTopazAutoKeyANSIData(topaz,hashedKeyData);
					break;
				case OpenDental.UI.SignatureBoxWrapper.SigMode.OrthoChart:
					hashedKeyData=OrthoCharts.GetHashStringForSignature(keyData);//Passed in key data has not been hashed yet.
					SetTopazAutoKeyANSIData(topaz,hashedKeyData);
					break;
				case OpenDental.UI.SignatureBoxWrapper.SigMode.Document:
				case OpenDental.UI.SignatureBoxWrapper.SigMode.Default:
				default:
					SetTopazAutoKeyANSIData(topaz,keyData);
					break;
			}
			SetTopazSigString(topaz,signature);
		}

		///<summary>Use as a last resort. Enumerates all encodings to find the first encoding that successfully decrypts the signature.</summary>
		public static void FillSignatureEncodings(Control topaz,string keyData,string signature,OpenDental.UI.SignatureBoxWrapper.SigMode sigMode) {
			//Figure out if key data needs to be hashed depending on the SigMode.
			string keyDataHashed;
			switch(sigMode) {
				case OpenDental.UI.SignatureBoxWrapper.SigMode.TreatPlan:
					keyDataHashed=TreatPlans.GetHashStringForSignature(keyData);
					break;
				case OpenDental.UI.SignatureBoxWrapper.SigMode.OrthoChart:
					keyDataHashed=OrthoCharts.GetHashStringForSignature(keyData);
					break;
				case OpenDental.UI.SignatureBoxWrapper.SigMode.Document:
				case OpenDental.UI.SignatureBoxWrapper.SigMode.Default:
				default:
					keyDataHashed=keyData;
					break;
			}
			//Use reflection to directly set encoded bytes as the KeyData that Topaz should use when decrypting the signature.
			FieldInfo fieldInfoSig;
			Topaz.Signature topazSignature;
			MethodInfo methodInfoSetAutoKeyData;
			try {
				//Get the field info for SigPlusNET's private Sig field.
				fieldInfoSig=typeof(SigPlusNET).GetField("Sig",BindingFlags.NonPublic | BindingFlags.Instance);
				//Get the value of the Sig field from the instance of the SigPlusNET object passed in.
				topazSignature=(Topaz.Signature)fieldInfoSig.GetValue(topaz);
				//Get the method info of the private SetAutoKeyData() method that accepts a byte array from the Sig field.
				//This method allows us to bypass the incorrect encodings that are forced on us in the public methods.
				methodInfoSetAutoKeyData=fieldInfoSig.FieldType.GetMethod("SetAutoKeyData",BindingFlags.NonPublic | BindingFlags.Instance,null,new Type[] { typeof(Byte[]) },null);
			}
			catch(Exception ex) {
				return;
			}
			List<Encoding> listEncodings=Encoding.GetEncodings().Select(x => x.GetEncoding()).ToList();
			for(int i=0;i<listEncodings.Count;i++) {
				//Completely clear and reset the signature pad in-between each encoding.
				ClearTopaz(topaz);
				SetTopazCompressionMode(topaz,0);
				SetTopazEncryptionMode(topaz,0);
				SetTopazKeyString(topaz,"0000000000000000");//Clear out the key string
				SetTopazEncryptionMode(topaz,2);//high encryption
				SetTopazCompressionMode(topaz,2);//high compression
				try {
					((SigPlusNET)topaz).AutoKeyStart();
					//Invoke SetAutoKeyData() with the encoded bytes of keyDataHashed.
					methodInfoSetAutoKeyData.Invoke(topazSignature,new object[] { listEncodings[i].GetBytes(keyDataHashed) });
					((SigPlusNET)topaz).AutoKeyFinish();
				}
				catch(Exception ex) {
					continue;
				}
				SetTopazSigString(topaz,signature);
				if(GetTopazNumberOfTabletPoints(topaz) > 0) {
					break;
				}
			}
		}

		#endregion

	}
}
