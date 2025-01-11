using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace CodeBase
{
    [Serializable]
    public class ODException : ApplicationException
    {
        private string _query = "";

        public int ErrorCode { get; }
        public string Query => _query ?? "";

        public static ErrorCodes GetErrorCodeAsEnum(int errorCode)
        {
            if (!Enum.IsDefined(typeof(ErrorCodes), errorCode))
            {
                return ErrorCodes.NotDefined;
            }

            return (ErrorCodes) errorCode;
        }

        public ErrorCodes ErrorCodeAsEnum => GetErrorCodeAsEnum(ErrorCode);

        public ODException()
        {
        }

        public ODException(int errorCode) : this("", errorCode)
        {
        }

        public ODException(string message) : this(message, 0)
        {
        }

        public ODException(string message, ErrorCodes errorCodeAsEnum) : this(message, (int) errorCodeAsEnum)
        {
        }

        public ODException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public ODException(string message, string query, Exception ex) : base(message, ex)
        {
            _query = query;

            ErrorCode = (int) ErrorCodes.DbQueryError;
        }

        public ODException(string message, Exception ex) : base(message, ex)
        {
        }

        protected ODException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorCode = info.GetInt32(nameof(ErrorCode));

            _query = info.GetString(nameof(Query));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(Query), Query);

            base.GetObjectData(info, context);
        }

        public static void SwallowAnyException(Action a)
        {
            try
            {
                a();
            }
            catch
            {
                // ignored
            }
        }

        public static void SwallowAndLogAnyException(string subDirectory, Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                Logger.WriteLine(MiscUtils.GetExceptionText(ex), subDirectory);
            }
        }

        public static void TryThrowPreservedCallstack(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            throw new Exception(ex.Message, ex);
        }

        public enum ErrorCodes
        {
            NotDefined = 0,
            OtkArgsInvalid = 200,
            MaxRequestDataExceeded = 202,
            XWebProgramProperties = 203,
            PayConnectProgramProperties = 204,
            DoseSpotNotAuthorized = 206,
            XWebDTGFailed = 207,
            PaySimpleProgramProperties = 208,
            NoPatientFound = 400,
            NoOperatoriesSetup = 406,
            SessionExpired = 409,
            ReceiptEmailAddressInvalid = 410,
            ReceiptEmailFailedToSend = 411,
            FormClosed = 500,
            CheckUserAndPasswordFailed = 600,
            DbQueryError = 700,
            BugSubmissionMessage = 800,
            FileExists = 4000,
            ODCloudClientTimeout = 4001,
            ClaimArchiveFailed = 4002,
            BrowserTimeout = 4003
        }
    }
}