using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EFormFields
{
    public static List<EFormField> GetForForm(long eFormNum)
    {
        return EFormFieldCrud.SelectMany("SELECT * FROM eformfield WHERE EFormNum = " + eFormNum + " ORDER BY ItemOrder");
    }
    
    public static void Insert(EFormField eFormField)
    {
        EFormFieldCrud.Insert(eFormField);
    }

    public static void DeleteForForm(long eFormNum)
    {
        Db.NonQ("DELETE FROM eformfield WHERE EFormNum = " + eFormNum);
    }

    public static EFormField FromDef(EFormFieldDef eFormFieldDef, long patNum = 0)
    {
        return new EFormField
        {
            PatNum = patNum,
            FieldType = eFormFieldDef.FieldType,
            DbLink = eFormFieldDef.DbLink,
            ValueLabel = eFormFieldDef.ValueLabel,
            ItemOrder = eFormFieldDef.ItemOrder,
            PickListVis = eFormFieldDef.PickListVis,
            PickListDb = eFormFieldDef.PickListDb,
            IsHorizStacking = eFormFieldDef.IsHorizStacking,
            IsTextWrap = eFormFieldDef.IsTextWrap,
            Width = eFormFieldDef.Width,
            FontScale = eFormFieldDef.FontScale,
            IsRequired = eFormFieldDef.IsRequired,
            ConditionalParent = eFormFieldDef.ConditionalParent,
            ConditionalValue = eFormFieldDef.ConditionalValue,
            LabelAlign = eFormFieldDef.LabelAlign,
            SpaceBelow = eFormFieldDef.SpaceBelow,
            ReportableName = eFormFieldDef.ReportableName,
            IsLocked = eFormFieldDef.IsLocked,
            Border = eFormFieldDef.Border,
            IsWidthPercentage = eFormFieldDef.IsWidthPercentage,
            MinWidth = eFormFieldDef.MinWidth,
            WidthLabel = eFormFieldDef.WidthLabel,
            SpaceToRight = eFormFieldDef.SpaceToRight,
            EFormFieldDefNum = eFormFieldDef.EFormFieldDefNum
        };
    }

    public static List<EFormField> FromListDefs(List<EFormFieldDef> listEFormFieldDefs, long patNum = 0)
    {
        var eFormFields = new List<EFormField>();
        
        foreach (var eFormFieldDef in listEFormFieldDefs)
        {
            var eFormField = FromDef(eFormFieldDef, patNum);
            
            eFormFields.Add(eFormField);
        }

        return eFormFields;
    }

    public static FlowDocument DeserializeFlowDocument(string xmlString)
    {
        if (xmlString == "") return new FlowDocument();
        var xamlString = xmlString;
        xamlString = xamlString.Replace("<FlowDocument>", "<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
        using var stringReader = new StringReader(xamlString);
        using var xmlReader = XmlReader.Create(stringReader);
        var flowDocument = (FlowDocument) XamlReader.Load(xmlReader);
        return flowDocument;
    }

    public static string SerializeFlowDocument(FlowDocument flowDocument)
    {
        var thicknessOriginal = flowDocument.PagePadding;
        
        flowDocument.PagePadding = new Thickness(0);
        flowDocument.AllowDrop = true;
        
        var memoryStream = new MemoryStream();
        
        var xmlWriterSettings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            CloseOutput = false,
            OmitXmlDeclaration = true,
            NewLineHandling = NewLineHandling.None
        };
        
        using var xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
        
        XamlWriter.Save(flowDocument, xmlWriter);
        xmlWriter.Close();
        
        var xamlString = Encoding.UTF8.GetString(memoryStream.ToArray());
        memoryStream.Dispose();
        var pattern = @"<FlowDocument"
                      + "[^>]*" //any number of characters that are not >
                      + ">";
        xamlString = Regex.Replace(xamlString, pattern, "<FlowDocument>"); //get rid of all the attributes like xmlns. We will add that back for viewing.
        var byteArray = Encoding.UTF8.GetBytes(xamlString);
        memoryStream = new MemoryStream(byteArray);
        //memoryStream.Position=0;
        using var streamReader = new StreamReader(memoryStream);
        var retVal = streamReader.ReadToEnd();
        memoryStream.Dispose();
        flowDocument.PagePadding = thicknessOriginal;
        
        return retVal;
    }

    public static string GetValParent(EFormField eFormField)
    {
        if (eFormField.FieldType != EnumEFormFieldType.RadioButtons) //This only works with radiobutton fields.
            return "";
        var listPickListDb = eFormField.PickListDb.Split('|').ToList();
        var listPickListVis = eFormField.PickListVis.Split('|').ToList();
        var idxDb = listPickListDb.IndexOf(eFormField.ValueString);
        if (eFormField.DbLink == "")
            //This fixes problem with conditional children being displayed when no radio button is selected.
            //Only happens when there's no dblink. 
            //idxDb could be falsely assigned if eFormField.ValueString="".
            idxDb = -1; //Set to -1 and use idxVis instead.
        if (idxDb != -1)
        {
            //The value string was found in the PickListDb.
            //We must test to see if the this radiobutton has an empty string in PickListVis
            var retVal = listPickListVis[idxDb];
            if (retVal == "") return listPickListDb[idxDb];
            return retVal;
        }

        //value not found in PickListDb
        if (eFormField.DbLink != "")
            //They typed it wrong
            return "";
        //This radioButton group does not have a DbLink
        //Our UI requires values on labels when no DbLink, although I suppose that could change some day.
        //Anyway, we don't have to worry about empty strings here.
        var idxVis = listPickListVis.IndexOf(eFormField.ValueString);
        if (idxVis != -1) return listPickListVis[idxVis];
        //value not found in PickListVis
        return "";
    }

    public static List<EFormField> GetSiblingsInStack(EFormField eFormField, List<EFormField> listEFormFields, bool isThisFieldHStacking, bool includeSelf = false)
    {
        var listEFormFieldsRet = new List<EFormField>();
        if (includeSelf) listEFormFieldsRet.Add(eFormField);
        //work backward
        var idx = listEFormFields.IndexOf(eFormField);
        var isPreviousSibling = false;
        if (idx > 0 && EFormFieldDefs.IsHorizStackableType(listEFormFields[idx - 1].FieldType) //previous must be stackable (probably already enforced)
                    && isThisFieldHStacking) //and we are stacking
            isPreviousSibling = true;
        if (isPreviousSibling)
        {
            listEFormFieldsRet.Insert(0, listEFormFields[idx - 1]);
            //now we can work backward and check to see if previous ones are actually stacked
            for (var i = idx - 1; i >= 0; i--)
            {
                if (!listEFormFields[i].IsHorizStacking) break; //we've reached the end of stacking
                //this one was already added.
                //If it's stacked, then that means we add the previous field (idx-2) and keep going
                listEFormFieldsRet.Insert(0, listEFormFields[i - 1]);
                //this won't crash at zero because the 0 field won't be stacked.
            }
        }

        //now work forward
        for (var i = idx + 1; i < listEFormFields.Count; i++)
        {
            if (!listEFormFields[i].IsHorizStacking) break;
            //this one is stacked, so add it to our list and keep going
            listEFormFieldsRet.Add(listEFormFields[i]);
        }

        return listEFormFieldsRet;
    }

    public static double CalcFieldWidth(EFormField eFormField, List<EFormField> listEFormFields, double widthAvail, double spaceToRightEachField)
    {
        double marginLeftOfPage = 5;
        //double marginRightOfField=10;
        var paddingLeft = 4; //The amount of padding on the left side of each field within its border box.
        var paddingRight = 4; //The amount of padding on the right side of each field within its border box.
        var thicknessLRBorders = 2; //This is the sum of the thickness of the left and right of the border box. It's 1+1=2
        var listEFormFieldsInStack = GetSiblingsInStack(eFormField, listEFormFields, eFormField.IsHorizStacking, true);
        if (!eFormField.IsWidthPercentage)
        {
            //fixed width
            double spaceRightOfField = PrefC.GetInt(PrefName.EformsSpaceToRightEachField);
            if (spaceToRightEachField != -1) spaceRightOfField = spaceToRightEachField;
            if (eFormField.SpaceToRight != -1) spaceRightOfField = eFormField.SpaceToRight;
            widthAvail -= marginLeftOfPage + spaceRightOfField;
            if (eFormField.Border == EnumEFormBorder.None)
            {
                widthAvail -= paddingLeft;
                widthAvail -= 1; //left border box thickness
            }
            else
            {
                //3D
                widthAvail -= paddingLeft + paddingRight;
                widthAvail -= thicknessLRBorders;
            }

            if (widthAvail < 0) widthAvail = 0;
            if (eFormField.Width == 0)
            {
                //no width specified
                if (listEFormFieldsInStack.Count > 1) //stacking
                    return 100; //This gracefully handles missing widths.
                //all alone
                return widthAvail; //so full width
            }

            if (eFormField.Width < widthAvail) //will fit
                return eFormField.Width;
            return widthAvail; //full width
        }

        //Width is percentage from here down----------------------------------------------------------------------------------------
        //Only allowed for the stackable fields: text, label, date, and checkbox.
        //We must always calculate the width of all fields within the same stack group
        //Order does matter now.
        var listWs = new List<W>();
        W wOurs = null;
        for (var i = 0; i < listEFormFieldsInStack.Count; i++)
        {
            var w = new W();
            w.EFormField_ = listEFormFieldsInStack[i];
            if (listEFormFieldsInStack[i].Width == 0)
            {
                //not specified, so we must gracefully handle
                if (listEFormFieldsInStack.Count > 1) //stacking
                    w.Percentage = 100; //an arbitrary default to gracefully handle
                else //all alone
                    w.Percentage = 100; //fill entire width
            }
            else
            {
                w.Percentage = listEFormFieldsInStack[i].Width;
            }

            if (eFormField == listEFormFieldsInStack[i]) wOurs = w;
            listWs.Add(w);
        }

        var totalPercent = listWs.Sum(x => x.Percentage);
        //totalPercent is allowed to be less than 100, but not greater:
        if (totalPercent > 100)
        {
            //Normalize to 100%
            var ratioNormalize = 100d / totalPercent; //this ratio will be <1
            for (var i = 0; i < listWs.Count; i++) listWs[i].Percentage = listWs[i].Percentage * ratioNormalize;
        }

        //So now they add up to 100 or less
        //We have to do percentage math in such a way that vertical columns can be created, even with varying numbers of fields.
        //Example: 40-40-20 should line up with 40-40
        //This means available width should not include the left, but should include the right margin.
        //And then the calculated width will be for the field plus all padding and border, and also right margin.
        widthAvail -= marginLeftOfPage;
        if (widthAvail < 0) //make sure it isn't negative.
            widthAvail = 0;
        //set MinWidthPlusMargin for each field
        for (var i = 0; i < listWs.Count; i++)
        {
            double spaceRightOfField = PrefC.GetInt(PrefName.EformsSpaceToRightEachField);
            if (spaceToRightEachField != -1) spaceRightOfField = spaceToRightEachField;
            if (listWs[i].EFormField_.SpaceToRight != -1) spaceRightOfField = listWs[i].EFormField_.SpaceToRight;
            listWs[i].MinWidthPlusMargin = listWs[i].EFormField_.MinWidth + spaceRightOfField;
            if (listEFormFieldsInStack[i].Border == EnumEFormBorder.None)
            {
                listWs[i].MinWidthPlusMargin += paddingLeft;
                listWs[i].MinWidthPlusMargin += 1; //left border box thickness
            }
            else
            {
                //3D
                listWs[i].MinWidthPlusMargin += paddingLeft + paddingRight;
                listWs[i].MinWidthPlusMargin += thicknessLRBorders;
            }
        }

        for (var i = 0; i < listWs.Count; i++)
        {
            //shrink each field to percentage
            listWs[i].WidthPlusMargin = listWs[i].Percentage * widthAvail / 100d;
            if (listWs[i].EFormField_.MinWidth == 0)
            {
                listWs[i].AboveMin = listWs[i].WidthPlusMargin;
            }
            else if (listWs[i].WidthPlusMargin < listWs[i].MinWidthPlusMargin)
            {
                //would be under min
                //example field WidthPlusMargin=20, but MinWidthPlusMargin=40, so bump width back up to 40. That's 0 aboveMin.
                listWs[i].WidthPlusMargin = listWs[i].MinWidthPlusMargin;
                listWs[i].AboveMin = 0;
            }
            else
            {
                //example field widthPlusMargin=50 and minWidthPlusMargin=40, so widthPlusMargin stays at 50. That's 10 aboveMin (width-minWidth)
                listWs[i].AboveMin = listWs[i].WidthPlusMargin - listWs[i].MinWidthPlusMargin;
            }
        }

        //Because we limited to minWidths, we can be over our avail width
        var totalAboveMin = listWs.Sum(x => x.AboveMin);
        var totalAboveAvail = listWs.Sum(x => x.WidthPlusMargin) - widthAvail;
        //totalAboveAvail could be a very tiny number, so we treat anything under .1 as zero
        if (totalAboveMin == 0 //they've all hit their min
            || totalAboveAvail <= 0.1) //or: fields are already calculated and fit inside available space.
        {
            //This fixes 'Bug: Set a text field to 60%. Add min width of 100. It shouldn't change, but it does shift to fill 100%.'
        }
        else
        {
            for (var i = 0; i < listWs.Count; i++)
            {
                if (listWs[i].AboveMin == 0) continue;
                //shrink it proportionally
                listWs[i].WidthPlusMargin -= totalAboveAvail * listWs[i].AboveMin / totalAboveMin;
                //but that might overshoot min again, so
                if (listWs[i].WidthPlusMargin < listWs[i].MinWidthPlusMargin) listWs[i].WidthPlusMargin = listWs[i].MinWidthPlusMargin;
            }
        }

        //All fields are now as small as they are going to get except for when one field remains in a row.
        //Because of min widths on each field, we might now have some wrapping going on.
        //We want add some extra behavior when percentage fields wrap.
        //We want to recalculate them again to fill available space on their new row.
        //This makes them look nice by lining up nicely along the right.
        //The original percentages make them look good on a big screen, 
        //and this recalculation below will make them look good on medium and smaller screens, all the way down to phones with a single column of fields.
        //Examples:
        //40-20-20 This is not initially full, so once it hits min width, fields will start wrapping without any expansion.
        //Also, because of this scenario, we don't expand if only one row.
        //The scenarios below will be more typical:
        //50-25-25 should convert to 67-33,33 and 100,50-50 and 100,100,100
        //25-25-25-25 should convert to 33-33-33,33 and 50-50,50-50 and 100,100,100,100
        //40-20-20-20 should convert to 50-25-25,25 and 67-33,33-33 and 100,50-50,50
        //20-20-20-20-20 should convert to 25-25-25-25,25 and 33-33-33,33-33 and 50-50,50-50,50 and 100,100,100,100,100
        //Now this will only work properly if the minWidths are also proportional.
        //Because the math must actually start with the minWidths, and then it must scale them up proportional to their original percentages.
        //First, split them up into rows
        double widthThisRow = 0;
        var rowNum = 0;
        for (var i = 0; i < listWs.Count; i++)
        {
            var areOthersInRow = listWs.Any(x => x.RowNum == rowNum); //false positive on i=1
            if (i == 0 || !areOthersInRow)
            {
                //This is needed for when available space gets very narrow.
                //Of course we want at least one field on each row.
                widthThisRow += listWs[i].WidthPlusMargin;
                listWs[i].RowNum = rowNum;
                continue;
            }

            widthThisRow += listWs[i].WidthPlusMargin;
            if (widthThisRow <= widthAvail + 0.1)
            {
                //for rounding error
                listWs[i].RowNum = rowNum;
                continue;
            }

            //this field won't fit
            rowNum++;
            listWs[i].RowNum = rowNum; //it goes on next row
            widthThisRow = listWs[i].WidthPlusMargin;
        }

        //We want to measure the width remaining on each row in order to know how much to increase.
        //We convert that width into a percentage that the fields would need to increase
        //If it's not the last row, we increase to fill.
        //If it is the last row and at least one other row has more than one field, then we increase the last row by the greatest growth of previous rows.
        var rowCount = listWs.Max(x => x.RowNum) + 1; //plus one because it's a count
        double percentGrowthMost = 0;
        for (var r = 0; r < rowCount; r++)
        {
            var listWsThisRow = listWs.FindAll(x => x.RowNum == r);
            if (totalPercent < 100)
            {
                //original percent did not fill entire row
                if (listWsThisRow.Count == 1) //if only one field left
                    if (listWsThisRow[0].WidthPlusMargin > widthAvail)
                        //and it's too big to fit
                        listWsThisRow[0].WidthPlusMargin = widthAvail; //shrink to keep from spilling over

                continue; //don't do any of the growth below
            }

            if (rowCount == 1) break; //if there's just one row, we don't do any of this
            var widthRemainThisRow = widthAvail - listWsThisRow.Sum(x => x.WidthPlusMargin);
            if (widthRemainThisRow < 0)
            {
                //must be a single field on this row that is bigger than the avail space because of min width
                if (listWsThisRow.Count > 1) throw new Exception(); //just proving it to myself
                //make it full width
                listWsThisRow[0].WidthPlusMargin = widthAvail;
                continue;
            }

            //what percentage would we use to expand to fill that remining width?
            //Example widthRemain=56, 2 fields are 40+80=120. 56/120=47% growth. 40x.47=19, 80x.47=38. 19+38=57
            var percentGrowthThisRow = widthRemainThisRow / listWsThisRow.Sum(x => x.WidthPlusMargin);
            if (r == rowCount - 1)
            {
                //last row
                if (percentGrowthMost > 0)
                    for (var i = 0; i < listWsThisRow.Count; i++)
                        listWsThisRow[i].WidthPlusMargin += listWsThisRow[i].WidthPlusMargin * percentGrowthMost;
                //example 80x.47=38
                else
                    //no previous row has multiple fields, so fill this row completely.
                    //This works even if this row has two fields.
                    for (var i = 0; i < listWsThisRow.Count; i++)
                        listWsThisRow[i].WidthPlusMargin += listWsThisRow[i].WidthPlusMargin * percentGrowthThisRow;
                //example 80x.47=38
                break;
            }

            //not last row
            for (var i = 0; i < listWsThisRow.Count; i++) listWsThisRow[i].WidthPlusMargin += listWsThisRow[i].WidthPlusMargin * percentGrowthThisRow;
            //example 80x.47=38
            if (percentGrowthThisRow > percentGrowthMost && listWsThisRow.Count > 1) percentGrowthMost = percentGrowthThisRow;
            //Since we're growing from the the starting point of minWidth, we don't need to check minWidth again.
        }

        //Remove one pixel from the last field in each row to keep it from prematurely wrapping
        for (var r = 0; r < rowCount; r++)
        {
            var w = listWs.FindAll(x => x.RowNum == r).Last();
            w.WidthPlusMargin--;
        }

        var retVal = wOurs.WidthPlusMargin;
        //this is the only one we care about even though we just calculated the entire h-stack.
        //Now, we need to strip off the white space to get our actual field width.
        double spaceRightOfField2 = PrefC.GetInt(PrefName.EformsSpaceToRightEachField);
        if (spaceToRightEachField != -1) spaceRightOfField2 = spaceToRightEachField;
        if (wOurs.EFormField_.SpaceToRight != -1) spaceRightOfField2 = wOurs.EFormField_.SpaceToRight;
        retVal -= spaceRightOfField2;
        if (eFormField.Border == EnumEFormBorder.None)
        {
            retVal -= paddingLeft;
            retVal -= 1; //left border box thickness
        }
        else
        {
            //3D
            retVal -= paddingLeft + paddingRight;
            retVal -= thicknessLRBorders;
        }

        if (retVal < 0) retVal = 0;
        return retVal;
    }

    public static bool IsAnyChanged(List<EFormField> listEFormFieldsNew, List<EFormField> listEFormFieldsOld)
    {
        //This doesn't need to test whether any have been added or removed because fields on existing eForms can't be added or removed.
        //This also doesn't need to worry about order because fields onexisting eForms can't be reordered.
        if (listEFormFieldsNew.Count != listEFormFieldsOld.Count)
            //this should never happen
            return true;
        var isChanged = false;
        for (var i = 0; i < listEFormFieldsNew.Count; i++) isChanged |= EFormFieldCrud.UpdateComparison(listEFormFieldsNew[i], listEFormFieldsOld[i]);
        return isChanged;
    }
    
    public static List<EFormField> GetDeepCopy(List<EFormField> listEFormFields)
    {
        var listEFormFieldsRet = new List<EFormField>();
        for (var i = 0; i < listEFormFields.Count; i++) listEFormFieldsRet.Add(listEFormFields[i].Copy());
        return listEFormFieldsRet;
    }

    private class W
    {
        ///<summary>The excess above each min width or above 0 if there is no minWidth</summary>
        public double AboveMin;

        public EFormField EFormField_;

        ///<summary>named to make it clear that we will still need to remove margins, etc.</summary>
        public double MinWidthPlusMargin;

        ///<summary>This contains normalized percentages. Example 40.</summary>
        public double Percentage;

        ///<summary>This percentage is renormalized within the wrapped row that the field ends up in.</summary>
        public double PercentThisRow;

        ///<summary>0-based. When the controls need to wrap, this keeps track of which row it's in. 0 if no wrapping.</summary>
        public int RowNum;

        ///<summary>named to make it clear that we will still need to remove margins, etc.</summary>
        public double WidthPlusMargin;

        public override string ToString()
        {
            return EFormField_.ValueLabel;
        }
    }
}