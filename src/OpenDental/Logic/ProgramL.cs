using System.Drawing;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDental.Bridges;
using OpenDental.Properties;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental.Logic;

public class ProgramL
{
    public static void LoadToolBar(ToolBarOD toolBar, EnumToolBar toolBarsAvail)
    {
        var toolButItems = ToolButItems.GetForToolBar(toolBarsAvail);
        foreach (var toolButItem in toolButItems)
        {
            var program = Programs.GetProgram(toolButItem.ProgramNum);
            
            var programProperty = ProgramProperties.GetPropForProgByDesc(program.ProgramNum, ProgramProperties.PropertyDescs.ClinicHideButton, Clinics.ClinicNum);
            if (programProperty is not null)
            {
                continue;
            }

            if (ProgramProperties.IsAdvertisingDisabled(program))
            {
                continue;
            }

            var programNumStr = program.ProgramNum + program.ProgName;
            
            if (toolBar.ImageList.Images.ContainsKey(programNumStr))
            {
                toolBar.ImageList.Images[toolBar.ImageList.Images.IndexOfKey(programNumStr)].Dispose();
                toolBar.ImageList.Images.RemoveByKey(programNumStr);
            }

            if (program.ButtonImage != "")
            {
                Image image = SIn.Bitmap(program.ButtonImage);
                toolBar.ImageList.Images.Add(programNumStr, image);
            }
            else if (program.ProgName == ProgramName.PracticeBooster.ToString())
            {
                Image image = Resources.Practice_Booster_Icon_22x22;
                toolBar.ImageList.Images.Add(programNumStr, image);
            }

            if (toolBarsAvail != EnumToolBar.MainToolbar)
            {
                toolBar.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
            }
            
            var toolBarButton = new ODToolBarButton(toolButItem.ButtonText, -1, "", program);
            
            AddDropDown(toolBarButton, program);
            
            toolBar.Buttons.Add(toolBarButton);
        }

        for (var i = 0; i < toolBar.Buttons.Count; i++)
        {
            if (toolBar.Buttons[i].Tag.GetType() != typeof(Program))
            {
                continue;
            }

            var program = (Program) toolBar.Buttons[i].Tag;
            var programNumStr = program.ProgramNum + program.ProgName;
            
            if (toolBar.ImageList.Images.ContainsKey(programNumStr))
            {
                toolBar.Buttons[i].ImageIndex = toolBar.ImageList.Images.IndexOfKey(programNumStr);
            }
        }
    }

    private static void AddDropDown(ODToolBarButton toolBarButton, Program program)
    {
        if (program.ProgName != ProgramName.Oryx.ToString())
        {
            return;
        }
        
        var contextMenuOryx = new ContextMenu();
        var menuItemUserSettings = new MenuItem();
            
        menuItemUserSettings.Index = 0;
        menuItemUserSettings.Text = "User Settings";
        menuItemUserSettings.Click += Oryx.menuItemUserSettingsClick;
            
        contextMenuOryx.MenuItems.AddRange([menuItemUserSettings]);
            
        toolBarButton.Style = ODToolBarButtonStyle.DropDownButton;
        toolBarButton.DropDownMenu = contextMenuOryx;
    }
}