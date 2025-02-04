using DataConnectionBase;

namespace OpenDentBusiness.HL7;

public class MessageParser
{
    public static int M11CheckDigit(string objectId)
    {
        if (objectId == "")
        {
            return -1;
        }

        var arrayIds = new int[objectId.Length];
        for (var i = 0; i < objectId.Length; i++)
        {
            try
            {
                arrayIds[objectId.Length - (1 + i)] = SIn.Int(objectId[i].ToString());
            }
            catch
            {
                return -1;
            }
        }

        var checkDigitCalc = 0;

        int[] arrayWeights = [2, 3, 4, 5, 6, 7];

        //Step 1
        for (var i = 0; i < arrayIds.Length; i++)
        {
            checkDigitCalc += arrayIds[i] * arrayWeights[i % 6];
        }

        //Step 2
        checkDigitCalc %= 11;

        //Step 3
        if (checkDigitCalc == 0)
        {
            checkDigitCalc = 1;
        }

        //Step 4
        checkDigitCalc = (11 - checkDigitCalc) % 10;
        return checkDigitCalc;
    }
}