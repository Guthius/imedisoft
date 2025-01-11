using CodeBase;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OpenDentBusiness.BetterDiag {
	public class BetterDiagApiClient {
		///<summary>HttpClient used for communicating with BetterDiagnostics API.</summary>
		private static HttpClient _httpClient=new HttpClient(){ 
			BaseAddress=new Uri("https://api.toothinsights.com")
		};

		///<summary>Sends image to BetterDiagnostics and returns their results synchronously.</summary>
		public static BetterDiagResponse SendOneImageToBetterDiag(Document document,Bitmap bitmap,Patient patient) {
			throw new NotImplementedException();//Throw exception here until API code is finalized.
			if(bitmap==null) {
				return null;
			}
			string apiKeyEncrypted="";
			if(!CDT.Class1.Decrypt(apiKeyEncrypted,out string apiKeyDecrypted)) {
				return null;
			}
			if(!_httpClient.DefaultRequestHeaders.Contains("accept")) {
				_httpClient.DefaultRequestHeaders.Add("accept","application/json");
			}
			if(!_httpClient.DefaultRequestHeaders.Contains("x-api-key")) {
				_httpClient.DefaultRequestHeaders.Add("x-api-key",apiKeyDecrypted);
			}
			//Create the form data
			long programNum=Programs.GetProgramNum(ProgramName.BetterDiagnostics);
			if(programNum==0) {
				return null;
			}
			string practiceId=ProgramProperties.GetPropForProgByDesc(programNum,"Practice ID").PropertyValue;
			if(practiceId.IsNullOrEmpty()) {
				//Practice ID not set, create alert here?
				return null;
			}
			string tempPath=Path.Combine(Path.GetTempPath(),"opendental",document.FileName);
			ImageStore.SaveBitmap(bitmap,tempPath,quality:100);//sets mime type
			string mimeType=System.Web.MimeMapping.GetMimeMapping(tempPath);
			MultipartFormDataContent formData=new MultipartFormDataContent();
			formData.Add(new StringContent(patient.PatNum.ToString()),"patient_id");
			formData.Add(new StringContent(practiceId),"practice_id");
			formData.Add(new StringContent("1"),"business_id");//have not been given a business_id for Open Dental.
			formData.Add(new StringContent("https://www.opendental.com"),"domain_address");
			formData.Add(new StringContent("True"),"co_or");
			BetterDiagResponse response;
			//Create temp image file for API call to read from
			using(FileStream tempFileStream=new FileStream(tempPath,FileMode.Open)) {
				StreamContent streamContent=new StreamContent(tempFileStream);
				streamContent.Headers.Add("type",mimeType);
				formData.Add(streamContent,"images",document.FileName);
				response=APIRequest.Inst.SendRequest<BetterDiagResponse,MultipartFormDataContent>(
					urlEndpoint:"/upload_direct_images",
					method:HttpMethod.Post,
					authHeaderVal:null,
					body:formData,
					contentType:HttpContentType.Multipart,
					clientOverride:_httpClient
				);
				//Delete temp file
				if(File.Exists(tempPath)) {
					File.Delete(tempPath);
				}
			}
			return response;
		}
	}
}
