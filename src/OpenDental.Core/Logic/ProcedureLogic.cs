using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcedureLogic
{
    public static int CompareProcedures(DataRow dataRowX, DataRow dataRowY)
    {
        //first, by status
        if (dataRowX.Table.Columns.Contains("ProcStatus") && dataRowY.Table.Columns.Contains("ProcStatus"))
        {
            if (dataRowX["ProcStatus"].ToString() != dataRowY["ProcStatus"].ToString())
            {
                var idxX = dataRowX["ProcStatus"].ToString() switch
                {
                    "8" => //TPi
                        0,
                    "7" => //Cn
                        1,
                    "1" => //TP
                        2,
                    "5" => //R
                        3,
                    "4" => //EO
                        4,
                    "2" => //C
                        5,
                    "3" => //EC
                        6,
                    "6" => //D
                        7,
                    _ => 0
                };

                var idxY = dataRowY["ProcStatus"].ToString() switch
                {
                    "8" => //TPi
                        0,
                    "7" => //Cn
                        1,
                    "1" => //TP
                        2,
                    "5" => //R
                        3,
                    "4" => //EO
                        4,
                    "2" => //C
                        5,
                    "3" => //EC
                        6,
                    "6" => //D
                        7,
                    _ => 0
                };

                return idxX.CompareTo(idxY);
            }
        }

        //by priority
        if (dataRowX.Table.Columns.Contains("Priority") && dataRowY.Table.Columns.Contains("Priority"))
        {
            if (dataRowX["Priority"].ToString() != dataRowY["Priority"].ToString())
            {
                //if priorities are different
                if (dataRowX["Priority"].ToString() == "0")
                {
                    return 1; //x is greater than y. Priorities always come first.
                }

                if (dataRowY["Priority"].ToString() == "0")
                {
                    return -1; //x is less than y. Priorities always come first.
                }

                var defOrderX = Defs.GetOrder(DefCat.TxPriorities, SIn.Long(dataRowX["Priority"].ToString()));
                var defOrderY = Defs.GetOrder(DefCat.TxPriorities, SIn.Long(dataRowY["Priority"].ToString()));
                return defOrderX.CompareTo(defOrderY);
            }
        }

        //priorities are the same, so sort by toothrange
        if (dataRowX["ToothRange"].ToString() != dataRowY["ToothRange"].ToString())
        {
            //empty toothranges come before filled toothrange values
            return dataRowX["ToothRange"].ToString().CompareTo(dataRowY["ToothRange"].ToString());
        }

        //toothranges are the same (usually empty), so compare toothnumbers
        if (dataRowX["ToothNum"].ToString() != dataRowY["ToothNum"].ToString())
        {
            //this also puts invalid or empty toothnumbers before the others.
            return Tooth.ToInt(dataRowX["ToothNum"].ToString()).CompareTo(Tooth.ToInt(dataRowY["ToothNum"].ToString()));
        }

        if (dataRowX["ProcCode"].ToString() != dataRowY["ProcCode"].ToString())
        {
            //priority and toothnums are the same, so sort by proccode if different.
            return dataRowX["ProcCode"].ToString().CompareTo(dataRowY["ProcCode"].ToString());
        }

        //priority, tooth number, and proccode are all the same.  Sort by ProcNum so we have a determinate order if everything else is the same.
        return dataRowX["ProcNum"].ToString().CompareTo(dataRowY["ProcNum"].ToString());
    }

    public static int CompareProcedures(Procedure procedureX, Procedure procedureY)
    {
        //We cannot sort Canadian labs within this comparer because there can be multiple labs associated to one procedure.
        //This comparer doesn't have enough information in order to sort a procedure and correctly move the corresponding lab(s) with it.
        //Therefore, Canadian labs need to be sorted as an additional step after this comparer has been invoked.
        //=========================================================================================================================
        //if(CultureInfo.CurrentCulture.Name.EndsWith("CA") && x.ProcNumLab!=y.ProcNumLab) {//This code should not impact USA users
        //	int retVal=CanadianLabSortHelper(x,y);
        //	if(retVal!=0) {
        //		return retVal;
        //	}
        //}
        //=========================================================================================================================
        //first by status
        if (procedureX.ProcStatus != procedureY.ProcStatus)
        {
            //Cn,TP,R,EO,C,EC,D
            //EC procs will draw on top of C procs of same date in the 3D tooth chart, 
            //but this is not a problem since C procs should always have a later date than EC procs.
            //EC must come after C so that group notes will come after their procedures in Progress Notes.
            var sortOrder = new List<ProcStat>
            {
                //The order of statuses in this list is very important and determines the sort order for procedures.
                ProcStat.TPi,
                ProcStat.Cn,
                ProcStat.TP,
                ProcStat.R,
                ProcStat.EO,
                ProcStat.C,
                ProcStat.EC,
                ProcStat.D
            };
            var idxX = sortOrder.IndexOf(procedureX.ProcStatus);
            var idxY = sortOrder.IndexOf(procedureY.ProcStatus);
            return idxX.CompareTo(idxY);
        }

        //by priority
        if (procedureX.Priority != procedureY.Priority)
        {
            //if priorities are different
            if (procedureX.Priority == 0)
            {
                return 1; //x is greater than y. Priorities always come first.
            }

            if (procedureY.Priority == 0)
            {
                return -1; //x is less than y. Priorities always come first.
            }

            return Defs.GetOrder(DefCat.TxPriorities, procedureX.Priority).CompareTo(Defs.GetOrder(DefCat.TxPriorities, procedureY.Priority));
        }

        //priorities are the same, so sort by toothrange
        if (procedureX.ToothRange != procedureY.ToothRange)
        {
            //empty toothranges come before filled toothrange values
            return procedureX.ToothRange.CompareTo(procedureY.ToothRange);
        }

        //toothranges are the same (usually empty), so compare toothnumbers
        if (procedureX.ToothNum != procedureY.ToothNum)
        {
            //this also puts invalid or empty toothnumbers before the others.
            return Tooth.ToInt(procedureX.ToothNum).CompareTo(Tooth.ToInt(procedureY.ToothNum));
        }

        //priority and toothnums are the same, so sort by proccode.
        if (procedureX.CodeNum != procedureY.CodeNum)
        {
            //GetProcCode(...).ProcCode can be null.
            //We do not protect the second call because comparing any string to null doesn't cause an error.
            var procCode = ProcedureCodes.GetProcCode(procedureX.CodeNum).ProcCode ?? "";
            return procCode.CompareTo(ProcedureCodes.GetProcCode(procedureY.CodeNum).ProcCode);
        }

        //if everything else is the same, sort by ProcNum so sort is deterministic
        return procedureX.ProcNum.CompareTo(procedureY.ProcNum);
    }

    public static void SortProcedures(List<Procedure> listProcedures)
    {
        //Keep track of all Canadian labs which will be manually re-inserted underneath their corresponding parent procedure.
        var listProceduresLab = listProcedures.FindAll(x => x.ProcNumLab != 0);
        //Remove all labs from the list prior to sorting.  Labs are always below their parent proc.
        listProcedures.RemoveAll(x => x.ProcNumLab != 0);
        //Sort the list of procedures that are missing Canadian labs.
        listProcedures.Sort(CompareProcedures);
        //Loop backward so we can insert as we go without affecting the index.
        for (var i = listProcedures.Count - 1; i >= 0; i--)
        {
            var listProceduresChildren = listProceduresLab.FindAll(x => x.ProcNumLab == listProcedures[i].ProcNum);
            listProcedures.InsertRange(i + 1, listProceduresChildren); //Insert labs below parent proc.
        }
    }
}