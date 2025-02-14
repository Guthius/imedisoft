using System.Collections.Generic;
using DataConnectionBase;

namespace OpenDentBusiness;

public class X999(string messageText) : X12object(messageText)
{
    public int GetBatchNumber()
    {
        if (FunctGroups[0].Transactions.Count != 1)
        {
            return 0;
        }

        var seg = FunctGroups[0].Transactions[0].GetSegmentByID("AK1");
        if (seg == null)
        {
            return 0;
        }

        var num = seg.Get(2);
        try
        {
            return SIn.Int(num);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Do this first to get a list of all trans nums that are contained within this 999.
    /// Then, for each trans num, we can later retrieve the AckCode for that single trans num.
    /// </summary>
    public List<int> GetTransNums()
    {
        var transNums = new List<int>();
        
        foreach (var x12Segment in FunctGroups[0].Transactions[0].Segments)
        {
            if (x12Segment.SegmentID != "AK2")
            {
                continue;
            }
            
            int transNum;
            try
            {
                transNum = SIn.Int(x12Segment.Get(2));
            }
            catch
            {
                transNum = 0;
            }

            if (transNum != 0)
            {
                transNums.Add(transNum);
            }
        }

        return transNums;
    }

    /// <summary>
    /// Use after GetTransNums.
    /// Will return A=Accepted, R=Rejected, or "" if can't determine.
    /// </summary>
    public string GetAckForTrans(int transNum)
    {
        var foundTransNum = false;
        foreach (var x12Segment in FunctGroups[0].Transactions[0].Segments)
        {
            if (foundTransNum)
            {
                if (x12Segment.SegmentID != "IK5")
                {
                    continue;
                }

                var code = x12Segment.Get(1);
                var ack = code is "A" or "E" ? "A" : "R";

                return ack;
            }

            if (x12Segment.SegmentID != "AK2")
            {
                continue;
            }
            
            int thisTransNum;
            try
            {
                thisTransNum = SIn.Int(x12Segment.Get(2));
            }
            catch
            {
                thisTransNum = 0;
            }

            if (thisTransNum == transNum)
            {
                foundTransNum = true;
            }
        }

        return "";
    }

    /// <summary>
    /// Will return "" if unable to determine.
    /// But would normally return A=Accepted or R=Rejected or P=Partially accepted if only some of the transactions were accepted.
    /// </summary>
    public string GetBatchAckCode()
    {
        if (FunctGroups[0].Transactions.Count != 1)
        {
            return "";
        }

        var seg = FunctGroups[0].Transactions[0].GetSegmentByID("AK9");
        if (seg == null)
        {
            return "";
        }

        var code = seg.Get(1);
        var ack = code switch
        {
            "A" or "E" => "A",
            "P" => "P",
            _ => "R"
        };

        return ack;
    }


    public string GetHumanReadable()
    {
        var result = "";
        
        foreach (var x12Segment in Segments)
        {
            if (x12Segment.SegmentID != "IK3" && x12Segment.SegmentID != "IK4")
            {
                continue;
            }

            if (result != "")
            {
                result += "\r\n";
            }

            if (x12Segment.SegmentID == "IK3")
            {
                result += "Segment " + x12Segment.Get(1) + ": " + GetSegmentSyntaxError(x12Segment.Get(4));
            }

            if (x12Segment.SegmentID == "IK4")
            {
                result += "Element " + x12Segment.Get(1) + ": " + GetElementSyntaxError(x12Segment.Get(3));
            }
        }

        return result;
    }

    private static string GetSegmentSyntaxError(string code)
    {
        return code switch
        {
            "1" => "Unrecognized segment ID",
            "2" => "Unexpected segment",
            "3" => "Required segment missing",
            "4" => "Loop occurs over maximum times",
            "5" => "Segment exceeds maximum use",
            "6" => "Segment not in defined transaction set",
            "7" => "Segment not in proper sequence",
            "8" => "Segment has data element errors",
            "I4" => "Implementation \"not used\" segment present",
            "I6" => "Implementation dependent segment missing",
            "I7" => "Implementation loop occurs under minimum times",
            "I8" => "Implementation segment below minimum use",
            "I9" => "Implementation dependent \"not used\" segment present",
            _ => code
        };
    }

    private static string GetElementSyntaxError(string code)
    {
        return code switch
        {
            "1" => "Required data element missing",
            "2" => "Conditional required data element missing",
            "3" => "Too many data elements",
            "4" => "Data element too short",
            "5" => "Data element too long",
            "6" => "Invalid character in data element",
            "7" => "Invalid code value",
            "8" => "Invalid date",
            "9" => "Invalid time",
            "10" => "Exclusion condition violated",
            "12" => "Too many repetitions",
            "13" => "Too many components",
            "I10" => "Implementation \"not used\" data element present",
            "I11" => "Implementation too few repetitions",
            "I12" => "Implementation pattern match failure",
            "I13" => "Implementation dependent \"not used\" data element present",
            "I6" => "Code value not used in implementation",
            "I9" => "Implementation dependent data element missing",
            _ => code
        };
    }
}