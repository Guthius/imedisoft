using OpenDentBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfControls {
	public class SecurityL {
		///<summary>Returns false if user doesn't have permission to edit the document. Suppresses message boxes. User must have ImageEdit permission to edit any document (uses date restriction). User must additionally have SignedImageEdit to edit signed documents.</summary>
		public static bool IsAuthorizedToEditImage(Document document,bool suppressMessage=false) {
			if(!Security.IsAuthorized(EnumPermType.ImageEdit,document.DateCreated,suppressMessage)) {
				return false;//User can't edit due to missing permission or date restriction.
			}
			else if(!string.IsNullOrEmpty(document.Signature) && !Security.IsAuthorized(EnumPermType.SignedImageEdit,suppressMessage)) {
				return false;//Document has signature and user can't edit signed documents.
			}
			return true;//User can edit the document.
		}

		///<summary>Returns the list of permissions the user doesn't have that are needed to delete the document. Suppresses message boxes. ImageDelete is needed to delete any document. SignedImageEdit is also needed to delete signed documents.</summary>
		public static List<EnumPermType> GetMissingPermissionsNeededToDeleteImage(Document document) {
			List<EnumPermType> listPermTypesNeeded=new List<EnumPermType>();
			if(!Security.IsAuthorized(EnumPermType.ImageDelete,document.DateCreated,suppressMessage:true)) {
				listPermTypesNeeded.Add(EnumPermType.ImageDelete);//User can't delete due to missing permission or date restriction.
			}
			if(!string.IsNullOrEmpty(document.Signature) && !Security.IsAuthorized(EnumPermType.SignedImageEdit,suppressMessage:true)) {
				listPermTypesNeeded.Add(EnumPermType.SignedImageEdit);//Document has signature and user can't delete signed documents.
			}
			return listPermTypesNeeded;
		}
	}
}
