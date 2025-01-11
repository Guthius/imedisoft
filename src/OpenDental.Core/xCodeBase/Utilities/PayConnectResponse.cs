using System;

namespace CodeBase;

public class PayConnectResponse
{
    public string Description;
    public string StatusCode;
    public string AuthCode;
    public string RefNumber;
    public string PaymentToken;
    public DateTime TokenExpiration;
    public string CardType;
    public decimal Amount;
    public decimal OriginalAmount;
    public decimal AmountSurcharged;
    public decimal SurchargePercent;
    public string EntryMode;
    public string CardNumber;
    public string MerchantId;
    public string TerminalId;
    public string Mode;
    public string CardVerificationMethod;
    public TransactionType TransType;
    public EmvData EMV;
    public string CardHolder;

    public class EmvData
    {
        public string AppId;
        public string TermVerifResults;
        public string IssuerAppData;
        public string TransStatusInfo;
        public string AuthResponseCode;
    }

    public enum TransactionType
    {
        Sale = 0,
        Authorize = 1,
        Refund = 2,
        Capture = 3,
        Void = 4,
        Unknown = 5
    }
}