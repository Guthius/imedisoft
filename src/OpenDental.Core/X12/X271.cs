using System;
using System.Collections.Generic;
using Imedisoft.Core.Data;

namespace OpenDentBusiness;

public class X271(string messageText) : X12object(messageText)
{
    public List<EB271> GetListEB(bool isInNetwork, bool isCoinsuranceInverted)
    {
        var results = new List<EB271>();
        
        EB271 eb = null;
        
        for (var i = 0; i < Segments.Count; i++)
        {
            if (Segments[i].SegmentID != "EB" && eb is null)
            {
                continue;
            }

            if (Segments[i].SegmentID == "EB")
            {
                if (eb is not null)
                {
                    results.Add(eb);
                }

                X12Segment hsdSegment = null;
                if (Segments[i + 1].SegmentID == "HSD")
                {
                    hsdSegment = Segments[i + 1];
                }
                
                eb = new EB271(Segments[i], isInNetwork, isCoinsuranceInverted, hsdSegment);
                continue;
            }

            if (Segments[i].SegmentID == "SE")
            {
                results.Add(eb);
                break;
            }

            eb?.SupplementalSegments.Add(Segments[i]);
        }

        return results;
    }

    public string GetGroupNum()
    {
        foreach (var segment in Segments)
        {
            if (segment.SegmentID == "REF" && segment.Elements[1] == "6P")
            {
                return segment.Elements[2];
            }
        }

        return "";
    }

    public List<DTP271> GetListDtpSubscriber()
    {
        var result = new List<DTP271>();
        
        foreach (var segment in Segments)
        {
            if (segment.SegmentID == "EB")
            {
                break;
            }

            if (segment.SegmentID != "DTP")
            {
                continue;
            }

            result.Add(new DTP271(segment));
        }

        return result;
    }

    public string GetProcessingError()
    {
        var result = "";
        
        foreach (var segment in Segments)
        {
            if (segment.SegmentID != "AAA")
            {
                continue;
            }

            if (result != "")
            {
                result += ", ";
            }

            result += GetRejectReason(segment.Get(3)) + ", " + GetFollowupAction(segment.Get(4));
        }

        return result;
    }

    private static string GetRejectReason(string code)
    {
        return code switch
        {
            "04" => "Authorized Quantity Exceeded (too many patients in request)",
            "15" => "Required application data missing",
            "41" => "Authorization Access Restriction (not allowed to submit requests)",
            "42" => "Unable to Respond at Current Time",
            "43" => "Invalid/Missing Provider Identification",
            "44" => "Invalid/Missing Provider Name",
            "45" => "Invalid/Missing Provider Specialty",
            "46" => "Invalid/Missing Provider Phone Number",
            "47" => "Invalid/Missing Provider State",
            "48" => "Invalid/Missing Referring Provider Identification Number",
            "49" => "Provider is Not Primary Care Physician",
            "50" => "Provider Ineligible for Inquiries",
            "51" => "Provider Not on File",
            "52" => "Service Dates Not Within Provider Plan Enrollment",
            "53" => "Inquired Benefit Inconsistent with Provider Type",
            "54" => "Inappropriate Product/Service ID Qualifier",
            "55" => "Inappropriate Product/Service ID",
            "56" => "Inappropriate Date",
            "57" => "Invalid/Missing Date(s) of Service",
            "58" => "Invalid/Missing Date-of-Birth",
            "60" => "Date of Birth Follows Date(s) of Service",
            "61" => "Date of Death Precedes Date(s) of Service",
            "62" => "Date of Service Not Within Allowable Inquiry Period",
            "63" => "Date of Service in Future",
            "64" => "Invalid/Missing Patient ID",
            "65" => "Invalid/Missing Patient Name",
            "66" => "Invalid/Missing Patient Gender Code",
            "67" => "Patient Not Found",
            "68" => "Duplicate Patient ID Number",
            "69" => "Inconsistent with Patient�s Age",
            "70" => "Inconsistent with Patient�s Gender",
            "71" => "Patient Birth Date Does Not Match That for the Patient on the Database",
            "72" => "Invalid/Missing Subscriber/Insured ID",
            "73" => "Invalid/Missing Subscriber/Insured Name",
            "74" => "Invalid/Missing Subscriber/Insured Gender Code",
            "75" => "Subscriber/Insured Not Found",
            "76" => "Duplicate Subscriber/Insured ID Number",
            "77" => "Subscriber Found, Patient Not Found",
            "78" => "Subscriber/Insured Not in Group/Plan Identified",
            "79" => "Invalid Participant Identification (this payer does not provide e-benefits)",
            "80" => "No Response received - Transaction Terminated",
            "97" => "Invalid or Missing Provider Address",
            "T4" => "Payer Name or Identifier Missing",
            _ => "Error code '" + code + "' not valid."
        };
    }

    private static string GetFollowupAction(string code)
    {
        return code switch
        {
            "C" => "Please Correct and Resubmit",
            "N" => "Resubmission Not Allowed",
            "P" => "Please Resubmit Original Transaction",
            "R" => "Resubmission Allowed",
            "S" => "Do Not Resubmit; Inquiry Initiated to a Third Party",
            "W" => "Please Wait 30 Days and Resubmit",
            "X" => "Please Wait 10 Days and Resubmit",
            "Y" => "Do Not Resubmit; We Will Hold Your Request and Respond Again Shortly",
            _ => "Error code '" + code + "' not valid."
        };
    }

    public static string ValidateSettings()
    {
        var validationErrors = "";
        var ebenetitCats = Enum.GetValues(typeof(EbenefitCategory));
        
        for (var i = 0; i < ebenetitCats.Length; i++)
        {
            var ebenCat = (EbenefitCategory) ebenetitCats.GetValue(i);
            if (ebenCat == EbenefitCategory.None)
            {
                continue;
            }

            var covCat = CovCats.GetForEbenCat(ebenCat);
            if (covCat is not null)
            {
                continue;
            }
            
            if (validationErrors != "")
            {
                validationErrors += ", ";
            }

            validationErrors += ebenCat.ToString();
        }

        if (validationErrors != "")
        {
            validationErrors = 
                "Missing or hidden insurance category for each of the following E-benefits:\r\n" + validationErrors + "\r\n" + 
                "Go to Setup then Insurance Categories to add or edit.";
        }

        return validationErrors;
    }
}