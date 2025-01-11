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
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EFormFields
{
    
    public static List<EFormField> GetForForm(long eFormNum)
    {
        var command = "SELECT * FROM eformfield WHERE EFormNum = " + SOut.Long(eFormNum) + " ORDER BY ItemOrder";
        return EFormFieldCrud.SelectMany(command);
    }

    
    public static long Insert(EFormField eFormField)
    {
        return EFormFieldCrud.Insert(eFormField);
    }

    
    public static void DeleteForForm(long eFormNum)
    {
        var command = "DELETE FROM eformfield WHERE EFormNum = " + SOut.Long(eFormNum);
        Db.NonQ(command);
    }

    public static EFormField FromDef(EFormFieldDef eFormFieldDef, long patNum = 0)
    {
        var eFormField = new EFormField();
        eFormField.PatNum = patNum;
        eFormField.FieldType = eFormFieldDef.FieldType;
        eFormField.DbLink = eFormFieldDef.DbLink;
        eFormField.ValueLabel = eFormFieldDef.ValueLabel;
        //eFormField.ValueString //set as part of fill
        eFormField.ItemOrder = eFormFieldDef.ItemOrder;
        eFormField.PickListVis = eFormFieldDef.PickListVis;
        eFormField.PickListDb = eFormFieldDef.PickListDb;
        eFormField.IsHorizStacking = eFormFieldDef.IsHorizStacking;
        eFormField.IsTextWrap = eFormFieldDef.IsTextWrap;
        eFormField.Width = eFormFieldDef.Width;
        eFormField.FontScale = eFormFieldDef.FontScale;
        eFormField.IsRequired = eFormFieldDef.IsRequired;
        eFormField.ConditionalParent = eFormFieldDef.ConditionalParent;
        eFormField.ConditionalValue = eFormFieldDef.ConditionalValue;
        eFormField.LabelAlign = eFormFieldDef.LabelAlign;
        eFormField.SpaceBelow = eFormFieldDef.SpaceBelow;
        eFormField.ReportableName = eFormFieldDef.ReportableName;
        eFormField.IsLocked = eFormFieldDef.IsLocked;
        eFormField.Border = eFormFieldDef.Border;
        eFormField.IsWidthPercentage = eFormFieldDef.IsWidthPercentage;
        eFormField.MinWidth = eFormFieldDef.MinWidth;
        eFormField.WidthLabel = eFormFieldDef.WidthLabel;
        eFormField.SpaceToRight = eFormFieldDef.SpaceToRight;
        //not a db field, but critical:
        eFormField.EFormFieldDefNum = eFormFieldDef.EFormFieldDefNum;
        return eFormField;
    }

    public static EFormFieldDef ToDef(EFormField eFormField)
    {
        var eFormFieldDef = new EFormFieldDef();
        eFormFieldDef.FieldType = eFormField.FieldType;
        eFormFieldDef.DbLink = eFormField.DbLink;
        eFormFieldDef.ValueLabel = eFormField.ValueLabel;
        //eFormField.ValueString //set as part of fill
        eFormFieldDef.ItemOrder = eFormField.ItemOrder;
        eFormFieldDef.PickListVis = eFormField.PickListVis;
        eFormFieldDef.PickListDb = eFormField.PickListDb;
        eFormFieldDef.IsHorizStacking = eFormField.IsHorizStacking;
        eFormFieldDef.IsTextWrap = eFormField.IsTextWrap;
        eFormFieldDef.Width = eFormField.Width;
        eFormFieldDef.FontScale = eFormField.FontScale;
        eFormFieldDef.IsRequired = eFormField.IsRequired;
        eFormFieldDef.ConditionalParent = eFormField.ConditionalParent;
        eFormFieldDef.ConditionalValue = eFormField.ConditionalValue;
        eFormFieldDef.LabelAlign = eFormField.LabelAlign;
        eFormFieldDef.SpaceBelow = eFormField.SpaceBelow;
        eFormFieldDef.ReportableName = eFormField.ReportableName;
        eFormFieldDef.IsLocked = eFormField.IsLocked;
        eFormFieldDef.Border = eFormField.Border;
        eFormFieldDef.IsWidthPercentage = eFormField.IsWidthPercentage;
        eFormFieldDef.MinWidth = eFormField.MinWidth;
        eFormFieldDef.WidthLabel = eFormField.WidthLabel;
        eFormFieldDef.SpaceToRight = eFormField.SpaceToRight;
        eFormFieldDef.EFormFieldDefNum = eFormField.EFormFieldDefNum; //this is the special non-db field
        return eFormFieldDef;
    }

    public static List<EFormField> FromListDefs(List<EFormFieldDef> listEFormFieldDefs, long patNum = 0)
    {
        var listEFormFields = new List<EFormField>();
        for (var i = 0; i < listEFormFieldDefs.Count; i++)
        {
            var eFormField = FromDef(listEFormFieldDefs[i], patNum);
            listEFormFields.Add(eFormField);
        }

        return listEFormFields;
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
        //a few properties are set because it's coming from a richTextBox.
        //They need to be reset to match out pattern further down when we strip them out.
        var thicknessOriginal = flowDocument.PagePadding;
        flowDocument.PagePadding = new Thickness(0);
        flowDocument.AllowDrop = true; //
        var memoryStream = new MemoryStream();
        var xmlWriterSettings = new XmlWriterSettings();
        xmlWriterSettings.Encoding = Encoding.UTF8;
        xmlWriterSettings.CloseOutput = false; //for xmlWriter.Close(); to not close the stream
        xmlWriterSettings.OmitXmlDeclaration = true;
        xmlWriterSettings.NewLineHandling = NewLineHandling.None; //new lines inside of runs are translated as a space, messing up the text
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
        //This prevents the existing flowDocument in the richTextBox on the screen from shifting slightly.
        //We might also want to take a look at this number when loading up a flowDocument into the UI.
        return retVal;
    }

    /// <summary>
    ///     Returns the current value of the parent from the PickListVis. This works in most cases, including when no
    ///     DbLink. But sometimes a user will have a DbLink and will leave the PickListVis value as an empty string. In those
    ///     cases, we use the value from the PickListDb instead. If no match can be made, it's usually because user mistyped
    ///     and we return an empty string.
    /// </summary>
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

    /// <summary>
    ///     If isLastInHorizStack then this field can have "space below" set. It could be last in h-stack, or it could be
    ///     all by itself.
    /// </summary>
    public static bool IsLastInHorizStack(EFormField eFormField, List<EFormField> listEFormFields)
    {
        var idx = listEFormFields.IndexOf(eFormField);
        if (idx == listEFormFields.Count - 1) //it's the last field
            return true;

        if (listEFormFields[idx + 1].FieldType == EnumEFormFieldType.PageBreak) return true;

        if (!listEFormFields[idx + 1].IsHorizStacking) //the next field is not horiz stacking
            return true;
        return false;
    }

    
    public static bool IsPreviousStackable(EFormField eFormField, List<EFormField> listEFormFields)
    {
        var idx = listEFormFields.IndexOf(eFormField);
        if (idx > 0 && EFormFieldDefs.IsHorizStackableType(listEFormFields[idx - 1].FieldType)) return true;
        return false;
    }

    /// <summary>
    ///     Guaranteed to never be -1. Could be list.Count, indicating that it's supposed to go after the last item in the
    ///     list.
    /// </summary>
    public static int GetLastIdxThisPage(List<EFormField> listEFormFields, int page)
    {
        var pageCur = 1;
        for (var i = 0; i < listEFormFields.Count; i++)
        {
            if (i == listEFormFields.Count - 1) return listEFormFields.Count; //whether or not we have a page number match
            if (listEFormFields[i].FieldType != EnumEFormFieldType.PageBreak) continue;
            //so we have a page break
            if (pageCur == page) return i; //the idx of the page break itself
            pageCur++;
        }

        return listEFormFields.Count; //even though it's impossible to get to this point.
    }

    /// <summary>
    ///     Returns all sibings in a horizontal stack. Only includes the field passed in if includeSelf is true. If not in
    ///     a h-stack, then it returns empty list. Even if the current field is not stacking, it can be part of a stack group
    ///     if the next field is set as stacking. Because this is used from the editor and the user can check or uncheck the
    ///     stacking box, we must ignore eFormField.IsHorizStacking and instead pass in the current stacking state from the
    ///     checkbox as isThisFieldHStacking.
    /// </summary>
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

    ///<summary>spaceToRightEachField refers to the EForm or EFormDef default.</summary>
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

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EFormField> Refresh(long patNum){

        string command="SELECT * FROM eformfield WHERE PatNum = "+POut.Long(patNum);
        return Crud.EFormFieldCrud.SelectMany(command);
    }

    ///<summary>Gets one EFormField from the db.</summary>
    public static EFormField GetOne(long eFormFieldNum){

        return Crud.EFormFieldCrud.SelectOne(eFormFieldNum);
    }



    
    public static void Update(EFormField eFormField){

        Crud.EFormFieldCrud.Update(eFormField);
    }

    
    public static void Delete(long eFormFieldNum) {

        Crud.EFormFieldCrud.Delete(eFormFieldNum);
    }

    */
}