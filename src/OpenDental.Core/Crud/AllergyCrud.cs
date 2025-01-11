using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AllergyCrud
{
    public static List<Allergy> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Allergy> TableToList(DataTable table)
    {
        var retVal = new List<Allergy>();
        Allergy allergy;
        foreach (DataRow row in table.Rows)
        {
            allergy = new Allergy();
            allergy.AllergyNum = SIn.Long(row["AllergyNum"].ToString());
            allergy.AllergyDefNum = SIn.Long(row["AllergyDefNum"].ToString());
            allergy.PatNum = SIn.Long(row["PatNum"].ToString());
            allergy.Reaction = SIn.String(row["Reaction"].ToString());
            allergy.StatusIsActive = SIn.Bool(row["StatusIsActive"].ToString());
            allergy.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            allergy.DateAdverseReaction = SIn.Date(row["DateAdverseReaction"].ToString());
            allergy.SnomedReaction = SIn.String(row["SnomedReaction"].ToString());
            retVal.Add(allergy);
        }

        return retVal;
    }

    public static long Insert(Allergy allergy)
    {
        var command = "INSERT INTO allergy (";

        command += "AllergyDefNum,PatNum,Reaction,StatusIsActive,DateAdverseReaction,SnomedReaction) VALUES(";

        command +=
            SOut.Long(allergy.AllergyDefNum) + ","
                                             + SOut.Long(allergy.PatNum) + ","
                                             + "'" + SOut.String(allergy.Reaction) + "',"
                                             + SOut.Bool(allergy.StatusIsActive) + ","
                                             //DateTStamp can only be set by MySQL
                                             + SOut.Date(allergy.DateAdverseReaction) + ","
                                             + "'" + SOut.String(allergy.SnomedReaction) + "')";
        {
            allergy.AllergyNum = Db.NonQ(command, true, "AllergyNum", "allergy");
        }
        return allergy.AllergyNum;
    }

    public static void Update(Allergy allergy)
    {
        var command = "UPDATE allergy SET "
                      + "AllergyDefNum      =  " + SOut.Long(allergy.AllergyDefNum) + ", "
                      + "PatNum             =  " + SOut.Long(allergy.PatNum) + ", "
                      + "Reaction           = '" + SOut.String(allergy.Reaction) + "', "
                      + "StatusIsActive     =  " + SOut.Bool(allergy.StatusIsActive) + ", "
                      //DateTStamp can only be set by MySQL
                      + "DateAdverseReaction=  " + SOut.Date(allergy.DateAdverseReaction) + ", "
                      + "SnomedReaction     = '" + SOut.String(allergy.SnomedReaction) + "' "
                      + "WHERE AllergyNum = " + SOut.Long(allergy.AllergyNum);
        Db.NonQ(command);
    }
}