using System.Collections.Generic;
using DataConnectionBase;

namespace OpenDentBusiness;

public class X997(string messageText) : X12object(messageText)
{
    /// <summary>
    /// In X12 lingo, the batchNumber is known as the functional group.
    /// </summary>
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
    /// Do this first to get a list of all trans nums that are contained within this 997.
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
                if (x12Segment.SegmentID != "AK5")
                {
                    continue;
                }

                var code = x12Segment.Get(1);

                return code is "A" or "E" ? "A" : "R";
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

        var x12Segment = FunctGroups[0].Transactions[0].GetSegmentByID("AK9");
        if (x12Segment == null)
        {
            return "";
        }

        var code = x12Segment.Get(1);

        return code switch
        {
            "A" or "E" => "A",
            "P" => "P",
            _ => "R"
        };
    }

    /*Example 997
    ISA*00*          *00*          *ZZ*113504607      *ZZ*               *070813*0930*U*00401*705547511*0*P*:~
    GS*FA*113504607**20070813*0930*705547511*X*004010X097A1~
    ST*997*0001~
    AK1*HC*0001~
    AK2*837*0001~
    AK5*A~
    AK9*A*1*1*1~
    SE*6*0001~
    GE*1*705547511~
    IEA*1*705547511~
    */
    //the only rows that we evaluate are AK2, which has transaction# (batchNumber), and AK5 which has ack code.

    public string GetHumanReadable()
    {
        var result = "";

        foreach (var x12Segment in Segments)
        {
            if (x12Segment.SegmentID != "AK3" && x12Segment.SegmentID != "AK4")
            {
                continue;
            }

            if (result != "")
            {
                result += "\r\n";
            }

            if (x12Segment.SegmentID == "AK3")
            {
                result += "Segment " + x12Segment.Get(1) + ": " + GetSegmentSyntaxError(x12Segment.Get(4));
            }

            if (x12Segment.SegmentID == "AK4")
            {
                result += "Element " + x12Segment.Get(1) + ": " + GetElementSyntaxError(x12Segment.Get(3));
            }
        }

        return result;
    }

    /*Example of 997 from failed 270 request.
    ISA*00*          *00*          *30*330989922      *ZZ*810624427      *090819*1501*U*00401*000000000*0*T*:~
    GS*FA*330989922*330989922*20090819*1501*0*X*004010~
    ST*997*0001~
    AK1*HS*26~
    AK2*270*0001~
    AK3*NM1*4**8~
    AK4*9*725*4*1~
    AK5*R*5~
    AK9*R*1*1*0~
    SE*8*0001~
    GE*1*0~
    IEA*1*000000000~
     */

    private static string GetSegmentSyntaxError(string code)
    {
        return code switch
        {
            "1" => "Unrecognized segment ID",
            "2" => "Unexpected segment",
            "3" => "Mandatory segment missing",
            "4" => "Loop Occurs Over Maximum Times",
            "5" => "Segment Exceeds Maximum Use",
            "6" => "Segment Not in Defined Transaction Set",
            "7" => "Segment Not in Proper Sequence",
            "8" => "Segment Has Data Element Errors",
            _ => code
        };
    }

    private static string GetElementSyntaxError(string code)
    {
        return code switch
        {
            "1" => "Mandatory data element missing",
            "2" => "Conditional required data element missing",
            "3" => "Too many data elements",
            "4" => "Data element too short",
            "5" => "Data element too long",
            "6" => "Invalid character in data element",
            "7" => "Invalid code value",
            "8" => "Invalid Date",
            "9" => "Invalid Time",
            "10" => "Exclusion Condition Violated",
            _ => code
        };
    }
}