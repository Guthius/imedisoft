using System;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class QuestionDefs
{
    ///<summary>Gets a list of all QuestionDefs.</summary>
    public static QuestionDef[] Refresh()
    {
        var command = "SELECT * FROM questiondef ORDER BY ItemOrder";
        return QuestionDefCrud.SelectMany(command).ToArray();
    }

    
    public static void Update(QuestionDef def)
    {
        QuestionDefCrud.Update(def);
    }

    
    public static long Insert(QuestionDef def)
    {
        return QuestionDefCrud.Insert(def);
    }

    ///<summary>Ok to delete whenever, because no patients are tied to this table by any dependencies.</summary>
    public static void Delete(QuestionDef def)
    {
        var command = "DELETE FROM questiondef WHERE QuestionDefNum =" + SOut.Long(def.QuestionDefNum);
        Db.NonQ(command);
    }


    ///<summary>Moves the selected item up in the list.</summary>
    public static void MoveUp(int selected, QuestionDef[] List)
    {
        if (selected < 0) throw new ApplicationException(Lans.g("QuestionDefs", "Please select an item first."));
        if (selected == 0) //already at top
            return;
        if (selected > List.Length - 1) throw new ApplicationException(Lans.g("QuestionDefs", "Invalid selection."));
        SetOrder(selected - 1, List[selected].ItemOrder, List);
        SetOrder(selected, List[selected].ItemOrder - 1, List);
    }

    
    public static void MoveDown(int selected, QuestionDef[] List)
    {
        if (selected < 0) throw new ApplicationException(Lans.g("QuestionDefs", "Please select an item first."));
        if (selected == List.Length - 1) //already at bottom
            return;
        if (selected > List.Length - 1) throw new ApplicationException(Lans.g("QuestionDefs", "Invalid selection."));
        SetOrder(selected + 1, List[selected].ItemOrder, List);
        SetOrder(selected, List[selected].ItemOrder + 1, List);
        //selected+=1;
    }

    ///<summary>Used by MoveUp and MoveDown.</summary>
    private static void SetOrder(int mySelNum, int myItemOrder, QuestionDef[] List)
    {
        var temp = List[mySelNum];
        temp.ItemOrder = myItemOrder;
        Update(temp);
    }
}