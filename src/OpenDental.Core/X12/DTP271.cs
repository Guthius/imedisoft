using System.Collections.Generic;

namespace OpenDentBusiness;

public class DTP271(X12Segment segment)
{
    public readonly X12Segment Segment = segment;

    private static readonly Dictionary<string, string> Descriptions = new()
    {
        {"102", "Issue"},
        {"152", "Effective Date of Change"},
        {"193", "Period Start"},
        {"194", "Period End"},
        {"198", "Completion"},
        {"290", "Coordination of Benefits"},
        {"291", "Plan"},
        {"292", "Benefit"},
        {"295", "Primary Care Provider"},
        {"304", "Latest Visit or Consultation"},
        {"307", "Eligibility"},
        {"318", "Added"},
        {"340", "Consolidated Omnibus Budget Reconciliation Act (COBRA) Begin"},
        {"341", "Consolidated Omnibus Budget Reconciliation Act (COBRA) End"},
        {"342", "Premium Paid to Date Begin"},
        {"343", "Premium Paid to Date End"},
        {"346", "Plan Begin"},
        {"347", "Plan End"},
        {"348", "Benefit Begin"},
        {"349", "Benefit End"},
        {"356", "Eligibility Begin"},
        {"357", "Eligibility End"},
        {"382", "Enrollment"},
        {"435", "Admission"},
        {"442", "Date of Death"},
        {"458", "Certification"},
        {"472", "Service"},
        {"539", "Policy Effective"},
        {"540", "Policy Expiration"},
        {"636", "Date of Last Update"},
        {"771", "Status"}
    };

    public static string GetDate(string qualifier, string date)
    {
        if (qualifier == "D8")
        {
            return X12Parse.ToDate(date).ToShortDateString();
        }

        var dates = date.Split('-');

        var date1 = X12Parse.ToDate(dates[0]);
        var date2 = X12Parse.ToDate(dates[1]);

        return date1.ToShortDateString() + "-" + date2.ToShortDateString();
    }

    public static string GetQualifierDescription(string code)
    {
        return !Descriptions.TryGetValue(code, value: out var descript) ? "" : descript;
    }
}