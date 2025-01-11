﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace OpenDentBusiness.EmdeonITS {
    using System.Diagnostics;
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ITSWSSoap", Namespace="https://ITSWebService.changehealthcare.com/")]
    public partial class ITSWS : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback AuthenticateOperationCompleted;
        
        private System.Threading.SendOrPostCallback ChangePasswordOperationCompleted;
        
        private System.Threading.SendOrPostCallback SendRequestOperationCompleted;
        
        private System.Threading.SendOrPostCallback SendRequestAOperationCompleted;
        
        private System.Threading.SendOrPostCallback PutFileExtOperationCompleted;
        
        private System.Threading.SendOrPostCallback PutFileOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetFileOperationCompleted;
        
        private System.Threading.SendOrPostCallback IsAliveOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public ITSWS() {
            this.Url = global::OpenDentBusiness.Properties.Settings.Default.OpenDentBusiness_EmdeonITS_ITSWS;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event AuthenticateCompletedEventHandler AuthenticateCompleted;
        
        /// <remarks/>
        public event ChangePasswordCompletedEventHandler ChangePasswordCompleted;
        
        /// <remarks/>
        public event SendRequestCompletedEventHandler SendRequestCompleted;
        
        /// <remarks/>
        public event SendRequestACompletedEventHandler SendRequestACompleted;
        
        /// <remarks/>
        public event PutFileExtCompletedEventHandler PutFileExtCompleted;
        
        /// <remarks/>
        public event PutFileCompletedEventHandler PutFileCompleted;
        
        /// <remarks/>
        public event GetFileCompletedEventHandler GetFileCompleted;
        
        /// <remarks/>
        public event IsAliveCompletedEventHandler IsAliveCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/Authenticate", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ItsReturn Authenticate(string sUserID, string sPassword) {
            object[] results = this.Invoke("Authenticate", new object[] {
                        sUserID,
                        sPassword});
            return ((ItsReturn)(results[0]));
        }
        
        /// <remarks/>
        public void AuthenticateAsync(string sUserID, string sPassword) {
            this.AuthenticateAsync(sUserID, sPassword, null);
        }
        
        /// <remarks/>
        public void AuthenticateAsync(string sUserID, string sPassword, object userState) {
            if ((this.AuthenticateOperationCompleted == null)) {
                this.AuthenticateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnAuthenticateOperationCompleted);
            }
            this.InvokeAsync("Authenticate", new object[] {
                        sUserID,
                        sPassword}, this.AuthenticateOperationCompleted, userState);
        }
        
        private void OnAuthenticateOperationCompleted(object arg) {
            if ((this.AuthenticateCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.AuthenticateCompleted(this, new AuthenticateCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/ChangePassword", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ItsReturn ChangePassword(string sUserID, string sPassword, string sNewPassword) {
            object[] results = this.Invoke("ChangePassword", new object[] {
                        sUserID,
                        sPassword,
                        sNewPassword});
            return ((ItsReturn)(results[0]));
        }
        
        /// <remarks/>
        public void ChangePasswordAsync(string sUserID, string sPassword, string sNewPassword) {
            this.ChangePasswordAsync(sUserID, sPassword, sNewPassword, null);
        }
        
        /// <remarks/>
        public void ChangePasswordAsync(string sUserID, string sPassword, string sNewPassword, object userState) {
            if ((this.ChangePasswordOperationCompleted == null)) {
                this.ChangePasswordOperationCompleted = new System.Threading.SendOrPostCallback(this.OnChangePasswordOperationCompleted);
            }
            this.InvokeAsync("ChangePassword", new object[] {
                        sUserID,
                        sPassword,
                        sNewPassword}, this.ChangePasswordOperationCompleted, userState);
        }
        
        private void OnChangePasswordOperationCompleted(object arg) {
            if ((this.ChangePasswordCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ChangePasswordCompleted(this, new ChangePasswordCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/SendRequest", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ItsReturn SendRequest(string sUserID, string sPassword, string sMessageType, string sEncodedRequest) {
            object[] results = this.Invoke("SendRequest", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sEncodedRequest});
            return ((ItsReturn)(results[0]));
        }
        
        /// <remarks/>
        public void SendRequestAsync(string sUserID, string sPassword, string sMessageType, string sEncodedRequest) {
            this.SendRequestAsync(sUserID, sPassword, sMessageType, sEncodedRequest, null);
        }
        
        /// <remarks/>
        public void SendRequestAsync(string sUserID, string sPassword, string sMessageType, string sEncodedRequest, object userState) {
            if ((this.SendRequestOperationCompleted == null)) {
                this.SendRequestOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendRequestOperationCompleted);
            }
            this.InvokeAsync("SendRequest", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sEncodedRequest}, this.SendRequestOperationCompleted, userState);
        }
        
        private void OnSendRequestOperationCompleted(object arg) {
            if ((this.SendRequestCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendRequestCompleted(this, new SendRequestCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/SendRequestA", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ITSReturnA SendRequestA(string sUserID, string sPassword, string sMessageType, string sEncodedRequest) {
            object[] results = this.Invoke("SendRequestA", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sEncodedRequest});
            return ((ITSReturnA)(results[0]));
        }
        
        /// <remarks/>
        public void SendRequestAAsync(string sUserID, string sPassword, string sMessageType, string sEncodedRequest) {
            this.SendRequestAAsync(sUserID, sPassword, sMessageType, sEncodedRequest, null);
        }
        
        /// <remarks/>
        public void SendRequestAAsync(string sUserID, string sPassword, string sMessageType, string sEncodedRequest, object userState) {
            if ((this.SendRequestAOperationCompleted == null)) {
                this.SendRequestAOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendRequestAOperationCompleted);
            }
            this.InvokeAsync("SendRequestA", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sEncodedRequest}, this.SendRequestAOperationCompleted, userState);
        }
        
        private void OnSendRequestAOperationCompleted(object arg) {
            if ((this.SendRequestACompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendRequestACompleted(this, new SendRequestACompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/PutFileExt", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ItsReturn PutFileExt(string sUserID, string sPassword, string sMessageType, string sFileName, string sEncodedPutFile) {
            object[] results = this.Invoke("PutFileExt", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sFileName,
                        sEncodedPutFile});
            return ((ItsReturn)(results[0]));
        }
        
        /// <remarks/>
        public void PutFileExtAsync(string sUserID, string sPassword, string sMessageType, string sFileName, string sEncodedPutFile) {
            this.PutFileExtAsync(sUserID, sPassword, sMessageType, sFileName, sEncodedPutFile, null);
        }
        
        /// <remarks/>
        public void PutFileExtAsync(string sUserID, string sPassword, string sMessageType, string sFileName, string sEncodedPutFile, object userState) {
            if ((this.PutFileExtOperationCompleted == null)) {
                this.PutFileExtOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPutFileExtOperationCompleted);
            }
            this.InvokeAsync("PutFileExt", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sFileName,
                        sEncodedPutFile}, this.PutFileExtOperationCompleted, userState);
        }
        
        private void OnPutFileExtOperationCompleted(object arg) {
            if ((this.PutFileExtCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PutFileExtCompleted(this, new PutFileExtCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/PutFile", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ItsReturn PutFile(string sUserID, string sPassword, string sMessageType, string sEncodedPutFile) {
            object[] results = this.Invoke("PutFile", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sEncodedPutFile});
            return ((ItsReturn)(results[0]));
        }
        
        /// <remarks/>
        public void PutFileAsync(string sUserID, string sPassword, string sMessageType, string sEncodedPutFile) {
            this.PutFileAsync(sUserID, sPassword, sMessageType, sEncodedPutFile, null);
        }
        
        /// <remarks/>
        public void PutFileAsync(string sUserID, string sPassword, string sMessageType, string sEncodedPutFile, object userState) {
            if ((this.PutFileOperationCompleted == null)) {
                this.PutFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPutFileOperationCompleted);
            }
            this.InvokeAsync("PutFile", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType,
                        sEncodedPutFile}, this.PutFileOperationCompleted, userState);
        }
        
        private void OnPutFileOperationCompleted(object arg) {
            if ((this.PutFileCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PutFileCompleted(this, new PutFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/GetFile", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ItsReturn GetFile(string sUserID, string sPassword, string sMessageType) {
            object[] results = this.Invoke("GetFile", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType});
            return ((ItsReturn)(results[0]));
        }
        
        /// <remarks/>
        public void GetFileAsync(string sUserID, string sPassword, string sMessageType) {
            this.GetFileAsync(sUserID, sPassword, sMessageType, null);
        }
        
        /// <remarks/>
        public void GetFileAsync(string sUserID, string sPassword, string sMessageType, object userState) {
            if ((this.GetFileOperationCompleted == null)) {
                this.GetFileOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetFileOperationCompleted);
            }
            this.InvokeAsync("GetFile", new object[] {
                        sUserID,
                        sPassword,
                        sMessageType}, this.GetFileOperationCompleted, userState);
        }
        
        private void OnGetFileOperationCompleted(object arg) {
            if ((this.GetFileCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetFileCompleted(this, new GetFileCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://ITSWebService.changehealthcare.com/IsAlive", RequestNamespace="https://ITSWebService.changehealthcare.com/", ResponseNamespace="https://ITSWebService.changehealthcare.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string IsAlive(string s) {
            object[] results = this.Invoke("IsAlive", new object[] {
                        s});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void IsAliveAsync(string s) {
            this.IsAliveAsync(s, null);
        }
        
        /// <remarks/>
        public void IsAliveAsync(string s, object userState) {
            if ((this.IsAliveOperationCompleted == null)) {
                this.IsAliveOperationCompleted = new System.Threading.SendOrPostCallback(this.OnIsAliveOperationCompleted);
            }
            this.InvokeAsync("IsAlive", new object[] {
                        s}, this.IsAliveOperationCompleted, userState);
        }
        
        private void OnIsAliveOperationCompleted(object arg) {
            if ((this.IsAliveCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.IsAliveCompleted(this, new IsAliveCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://ITSWebService.changehealthcare.com/")]
    public partial class ItsReturn {
        
        private int errorCodeField;
        
        private string responseField;
        
        /// <remarks/>
        public int ErrorCode {
            get {
                return this.errorCodeField;
            }
            set {
                this.errorCodeField = value;
            }
        }
        
        /// <remarks/>
        public string Response {
            get {
                return this.responseField;
            }
            set {
                this.responseField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9032.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://ITSWebService.changehealthcare.com/")]
    public partial class ITSReturnA {
        
        private int errorCodeField;
        
        private string responseField;
        
        private string serverCodeField;
        
        /// <remarks/>
        public int ErrorCode {
            get {
                return this.errorCodeField;
            }
            set {
                this.errorCodeField = value;
            }
        }
        
        /// <remarks/>
        public string Response {
            get {
                return this.responseField;
            }
            set {
                this.responseField = value;
            }
        }
        
        /// <remarks/>
        public string ServerCode {
            get {
                return this.serverCodeField;
            }
            set {
                this.serverCodeField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void AuthenticateCompletedEventHandler(object sender, AuthenticateCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AuthenticateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal AuthenticateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ItsReturn Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ItsReturn)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void ChangePasswordCompletedEventHandler(object sender, ChangePasswordCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ChangePasswordCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ChangePasswordCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ItsReturn Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ItsReturn)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void SendRequestCompletedEventHandler(object sender, SendRequestCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SendRequestCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SendRequestCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ItsReturn Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ItsReturn)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void SendRequestACompletedEventHandler(object sender, SendRequestACompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SendRequestACompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SendRequestACompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ITSReturnA Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ITSReturnA)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void PutFileExtCompletedEventHandler(object sender, PutFileExtCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PutFileExtCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PutFileExtCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ItsReturn Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ItsReturn)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void PutFileCompletedEventHandler(object sender, PutFileCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PutFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PutFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ItsReturn Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ItsReturn)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void GetFileCompletedEventHandler(object sender, GetFileCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetFileCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetFileCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ItsReturn Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ItsReturn)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void IsAliveCompletedEventHandler(object sender, IsAliveCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class IsAliveCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal IsAliveCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591