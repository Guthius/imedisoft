using System.Globalization;
using System.Windows.Forms;
using Imedisoft.Core.Entities;
using OpenDental.Forms;
using OpenDental.Logic;
using OpenDental.UI;
using OpenDentalImaging;

namespace OpenDental;

partial class FormOpenDental
{
    private MenuItemOD _menuItemAccount;
    private MenuItemOD _menuItemAlerts;
    private MenuItemOD _menuItemClinicsMain;
    private MenuItemOD _menuItemCounties;
    private MenuItemOD _menuItemCreateAtoZ;
    private MenuItemOD _menuItemFeeSchedGroups;
    private MenuItemOD _menuItemFinanceCharges;
    private MenuItemOD _menuItemHL7;
    private MenuItemOD _menuItemLateCharges;
    private MenuItemOD _menuItemLocalHelpWindows;
    private MenuItemOD _menuItemPatDashboards;
    private MenuItemOD _menuItemPatPortalTransactions;
    private MenuItemOD _menuItemOnlinePayments;
    private MenuItemOD _menuItemProcLockTool;
    private MenuItemOD _menuItemQueryFavorites;
    private MenuItemOD _menuItemReports;
    private MenuItemOD _menuItemReactivation;
    private MenuItemOD _menuItemRepeatingCharges;
    private MenuItemOD _menuItemSites;
    private MenuItemOD _menuItemStandard;
    private MenuItemOD _menuItemStandardFiltered;
    private MenuItemOD _menuItemPrinter;
    private MenuItemOD _menuItemUnfinalizedPay;
    private MenuItemOD _menuItemUserQuery;

    private void LayoutMenu()
    {
        menuMain.BeginUpdate();
        //Log Off--------------------------------------------------------------------------------------------------------
        menuMain.Add(new MenuItemOD("Log &Off", menuItemLogOff_Click));
        //File-----------------------------------------------------------------------------------------------------------
        var menuItemFile = new MenuItemOD("&File");
        menuMain.Add(menuItemFile);
        LayoutMenuFile(menuItemFile);
        //Setup----------------------------------------------------------------------------------------------------------
        var menuItemSetup = new MenuItemOD("&Setup");
        menuMain.Add(menuItemSetup);
        LayoutMenuSetup(menuItemSetup);
        //Lists----------------------------------------------------------------------------------------------------------
        var menuItemLists = new MenuItemOD("&Lists");
        menuMain.Add(menuItemLists);
        LayoutMenuLists(menuItemLists);
        //Reports--------------------------------------------------------------------------------------------------------
        _menuItemReports = new MenuItemOD("&Reports");
        menuMain.Add(_menuItemReports);
        LayoutMenuReports(_menuItemReports);
        //Tools----------------------------------------------------------------------------------------------------------
        var menuItemTools = new MenuItemOD("&Tools");
        menuMain.Add(menuItemTools);
        LayoutMenuTools(menuItemTools);
        //Clinics--------------------------------------------------------------------------------------------------------
        _menuItemClinicsMain = new MenuItemOD("&Clinics");
        menuMain.Add(_menuItemClinicsMain);
        //Alerts---------------------------------------------------------------------------------------------------------
        _menuItemAlerts = new MenuItemOD("Alerts (0)", menuItemAlerts_Click);
        menuMain.Add(_menuItemAlerts);
        //Help-----------------------------------------------------------------------------------------------------------
        var menuItemHelp = new MenuItemOD("&Help");
        menuMain.Add(menuItemHelp);
        LayoutMenuHelp(menuItemHelp);
        menuMain.EndUpdate();
    }

    private void LayoutMenuFile(MenuItemOD menuItemFile)
    {
        menuItemFile.Add("User Password", (_, _) => SecurityL.ChangePassword(false));
        menuItemFile.Add("User Email Address", menuItemUserEmailAddress_Click);
        menuItemFile.Add("User Settings", menuItemUserSettings_Click);
        menuItemFile.AddSeparator();
        _menuItemPrinter = new MenuItemOD("&Printer", menuItemPrinter_Click);
        menuItemFile.Add(_menuItemPrinter);
        menuItemFile.Add("Graphics", menuItemGraphics_Click);
        menuItemFile.AddSeparator();
        menuItemFile.Add("&Choose Database", menuItemConfig_Click);
        menuItemFile.AddSeparator();
        menuItemFile.Add("E&xit", (_, _) => Application.Exit());
    }

    private void LayoutMenuSetup(MenuItemOD menuItemSetup)
    {
        //Preferences-----------------------------------------------------------------------------------------------------
        var menuItemPrefences = new MenuItemOD("Preferences", menuItemPreferences_Click);
        menuItemSetup.Add(menuItemPrefences);
        menuItemSetup.AddSeparator();
        //Appointments----------------------------------------------------------------------------------------------------
        var menuItemAppts = new MenuItemOD("Appointments");
        menuItemSetup.Add(menuItemAppts);
        LayoutSubMenuAppts(menuItemAppts);
        //Family/Insurance------------------------------------------------------------------------------------------------
        var menuItemFamIns = new MenuItemOD("Family / Insurance");
        menuItemSetup.Add(menuItemFamIns);
        LayoutSubMenuFamIns(menuItemFamIns);
        //Account---------------------------------------------------------------------------------------------------------
        _menuItemAccount = new MenuItemOD("Account");
        menuItemSetup.Add(_menuItemAccount);
        LayoutSubMenuAccount(_menuItemAccount);
        //Chart-----------------------------------------------------------------------------------------------------------
        var menuItemChart = new MenuItemOD("Chart");
        menuItemSetup.Add(menuItemChart);
        LayoutSubMenuChart(menuItemChart);
        //Imaging---------------------------------------------------------------------------------------------------------
        var menuItemImaging = new MenuItemOD("Imaging");
        menuItemSetup.Add(menuItemImaging);
        LayoutSubMenuImaging(menuItemImaging);
        //Manage----------------------------------------------------------------------------------------------------------
        var menuItemManage = new MenuItemOD("Manage");
        menuItemSetup.Add(menuItemManage);
        LayoutSubMenuManage(menuItemManage);
        menuItemSetup.AddSeparator();
        //Advanced Setup--------------------------------------------------------------------------------------------------
        var menuItemAdvSetup = new MenuItemOD("Advanced Setup");
        menuItemSetup.Add(menuItemAdvSetup);
        LayoutSubMenuAdvSetup(menuItemAdvSetup);
        //Menus below have no submenus (name as shown)--------------------------------------------------------------------
        menuItemSetup.Add("Alert Categories", (_, _) => Open<FormAlertCategorySetup>(EnumPermType.SecurityAdmin, "Alert Categories"));
        menuItemSetup.Add("Auto Codes", (_, _) => Open<FormAutoCode>(EnumPermType.Setup, "Auto Codes"));
        menuItemSetup.Add("Automation", (_, _) => Open<FormAutomation>(EnumPermType.Setup, "Automation"));
        menuItemSetup.Add("Auto Notes", (_, _) => Open<FormAutoNotes>(EnumPermType.AutoNoteQuickNoteEdit, "Auto Notes Setup"));
        menuItemSetup.Add("Code Groups", (_, _) => Open<FormCodeGroups>(EnumPermType.Setup, "Code Groups"));
        menuItemSetup.Add("Data Paths", menuItemDataPath_Click);
        menuItemSetup.Add("Definitions", menuItemDefinitions_Click);
        menuItemSetup.Add("Display Fields", menuItemDisplayFields_Click);
        menuItemSetup.Add("Fee Schedules", menuItemFeeScheds_Click);
        _menuItemFeeSchedGroups = new MenuItemOD("Fee Schedule Groups", menuFeeSchedGroups_Click);
        menuItemSetup.Add(_menuItemFeeSchedGroups);
        menuItemSetup.Add("Laboratories", (_, _) => Open<FormLaboratories>(EnumPermType.Setup, "Laboratories"));
        menuItemSetup.Add("Practice", menuItemPractice_Click);
        menuItemSetup.Add("Program Links", menuItemLinks_Click);
        menuItemSetup.Add("Quick Paste Notes", menuItemQuickPasteNotes_Click);
        menuItemSetup.Add("Reports", menuItemReports_Click);
        menuItemSetup.Add("Required Fields", (_, _) => Open<FormRequiredFields>(EnumPermType.Setup, "Required Fields"));
        menuItemSetup.Add("Schedules", menuItemSched_Click);
        menuItemSetup.Add("Security", menuItemSecurity_Click);
        menuItemSetup.Add("Security Add User", menuItemSecurityAddUser_Click);
        menuItemSetup.Add("Sheets", (_, _) => Open<FormSheetDefs>(EnumPermType.Setup, "Sheets"));
        menuItemSetup.Add("Spell Check", (_, _) => Open<FormSpellCheck>());
        menuItemSetup.Add("Tasks", MenuItemTask_Click);
        menuItemSetup.AddSeparator();
    }

    private void LayoutSubMenuAppts(MenuItemOD menuItemAppts)
    {
        menuItemAppts.Add("Appointment Field Defs", (_, _) => Open<FormApptFieldDefs>(EnumPermType.Setup, "Appointment Field Defs"));
        menuItemAppts.Add("Appointment Rules", (_, _) => Open<FormApptRules>(EnumPermType.Setup, "Appointment Rules"));
        menuItemAppts.Add("Appointment Types", (_, _) => Open<FormApptTypes>(EnumPermType.Setup, "Appointment Types"));
        menuItemAppts.Add("Appointment Views", menuItemApptViews_Click);
        menuItemAppts.Add("ASAP List", (_, _) => Open<FormAsapSetup>(EnumPermType.Setup, "ASAP List Setup"));
        menuItemAppts.Add("Confirmations", (_, _) => Open<FormConfirmationSetup>(EnumPermType.Setup, "Confirmation Setup"));
        menuItemAppts.Add("Insurance Verification", (_, _) => Open<FormInsVerificationSetup>(EnumPermType.Setup, "Insurance Verification"));
        menuItemAppts.Add("Operatories", menuItemOperatories_Click);
        menuItemAppts.Add("Recall", (_, _) => Open<FormRecallSetup>(EnumPermType.Setup, "Recall"));
        menuItemAppts.Add("Recall Types", (_, _) => Open<FormRecallTypes>(EnumPermType.Setup, "Recall Types"));
        _menuItemReactivation = new MenuItemOD("Reactivation", (_, _) => Open<FormReactivationSetup>(EnumPermType.Setup, "Reactivation"));
        menuItemAppts.Add(_menuItemReactivation);
    }

    private void LayoutSubMenuFamIns(MenuItemOD menuItemFamIns)
    {
        menuItemFamIns.Add("Claim Forms", (_, _) => Open<FormClaimForms>(EnumPermType.Setup, "Claim Forms"));
        menuItemFamIns.Add("Clearinghouses", (_, _) => Open<FormClearinghouses>(EnumPermType.Setup, "Clearinghouses"));
        menuItemFamIns.Add("Insurance Blue Book", (_, _) => Open<FormInsBlueBookRules>(EnumPermType.Setup, "Insurance Blue Book"));
        menuItemFamIns.Add("Insurance Categories", (_, _) => Open<FormInsCatsSetup>(EnumPermType.Setup, "Insurance Categories"));
        menuItemFamIns.Add("Insurance Filing Codes", (_, _) => Open<FormInsFilingCodes>(EnumPermType.Setup, "Insurance Filing Codes"));
        menuItemFamIns.Add("Patient Field Defs", menuItemPatFieldDefs_Click);
        menuItemFamIns.Add("Payer IDs", (_, _) => Open<FormElectIDs>(EnumPermType.Setup, "Payer IDs"));
    }

    private void LayoutSubMenuAccount(MenuItemOD menuItemAccount)
    {
        menuItemAccount.Add("Allocations", (_, _) => Open<FormAllocationsSetup>());
        menuItemAccount.Add("Pay Plan Templates", (_, _) => Open<FormPayPlanTemplates>(EnumPermType.Setup, "Pay Plan Templates"));
    }

    private void LayoutSubMenuChart(MenuItemOD menuItemChart)
    {
        menuItemChart.Add("Procedure Buttons", menuItemProcedureButtons_Click);
    }

    private void LayoutSubMenuImaging(MenuItemOD menuItemImaging)
    {
        menuItemImaging.Add("Devices", (_, _) => Open<FormImagingDevices>(EnumPermType.Setup, "Imaging Devices"));
        menuItemImaging.Add("Mounts", (_, _) => Open<FormMountDefs>(EnumPermType.Setup, "Mounts"));
        menuItemImaging.Add("Scanning", (_, _) => Open<FormImagingSetup>(EnumPermType.Setup, "Imaging"));
    }

    private void LayoutSubMenuManage(MenuItemOD menuItemManage)
    {
        menuItemManage.Add("E-mail", (_, _) => Open<FormEmailAddresses>(EnumPermType.Setup, "Email"));
    }

    private void LayoutSubMenuAdvSetup(MenuItemOD menuItemAdvSetup)
    {
        menuItemAdvSetup.Add("Computers", (_, _) => Open<FormComputers>(EnumPermType.Setup, "Computers"));
        menuItemAdvSetup.Add("HIE", (_, _) => Open<FormHieSetup>(EnumPermType.Setup, "HIE"));
        _menuItemHL7 = new MenuItemOD("HL7", menuItemHL7_Click);
        menuItemAdvSetup.Add(_menuItemHL7);
        menuItemAdvSetup.Add("Show Features", MenuItemEasy_Click);
        menuItemAdvSetup.Add("Scheduled Processes", (_, _) => Open<FormScheduledProcesses>());
    }

    private void LayoutMenuLists(MenuItemOD menuItemLists)
    {
        var menuItemProcedureCodes = new MenuItemOD("&Procedure Codes", menuItemProcCodes_Click);
        menuItemProcedureCodes.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
        menuItemLists.Add(menuItemProcedureCodes);
        menuItemLists.AddSeparator();
        menuItemLists.Add("Allergies", (_, _) => Open<FormAllergySetup>());
        menuItemLists.Add("Clinics", menuItemClinics_Click);
        var menuItemContacts = new MenuItemOD("&Contacts", (_, _) => Open<FormContacts>());
        menuItemContacts.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
        menuItemLists.Add(menuItemContacts);
        _menuItemCounties = new MenuItemOD("Counties", (_, _) => Open<FormCounties>(EnumPermType.Setup, "Counties"));
        menuItemLists.Add(_menuItemCounties);
        menuItemLists.Add("Discount Plans", (_, _) => Open<FormDiscountPlans>(EnumPermType.Setup, "Discount Plans"));
        menuItemLists.Add("&Employees", (_, _) => Open<FormEmployeeSelect>(EnumPermType.Setup, "Employees"));
        menuItemLists.Add("Employers", (_, _) => Open<FormEmployers>());
        menuItemLists.Add("Insurance Carriers", menuItemCarriers_Click);
        menuItemLists.Add("&Insurance Plans", menuItemInsPlans_Click);
        menuItemLists.Add("Lab Cases", menuItemLabCases_Click);
        menuItemLists.Add("&Medications", (_, _) => Open<FormMedications>());
        menuItemLists.Add("Pharmacies", (_, _) => Open<FormPharmacies>());
        menuItemLists.Add("Problems", (_, _) => Open<FormDiseaseDefs>());
        menuItemLists.Add("Providers", menuItemProviders_Click);
        menuItemLists.Add("&Referrals", menuItemReferrals_Click);
        _menuItemSites = new MenuItemOD("Sites", menuItemSites_Click);
        menuItemLists.Add(_menuItemSites);
        menuItemLists.Add("State Abbreviations", menuItemStateAbbrs_Click);
        menuItemLists.Add(CultureInfo.CurrentCulture.Name.EndsWith("CA") ? "Postal Codes" : "&Zip Codes", menuItemZipCodes_Click);
    }

    private void LayoutMenuReports(MenuItemOD menuItemReports)
    {
        _menuItemStandard = new MenuItemOD("&Standard", menuItemReportsStandard_Click);
        menuItemReports.Add(_menuItemStandard);
        _menuItemStandardFiltered = new MenuItemOD("Standard Favorites", menuItemReportsFilteredClick_Click);
        menuItemReports.Add(_menuItemStandardFiltered);
        _menuItemUserQuery = new MenuItemOD("&User Query", menuItemReportsUserQuery_Click);
        menuItemReports.Add(_menuItemUserQuery);
        _menuItemQueryFavorites = new MenuItemOD("User Query Favorites", menuItemReportsQueryFavorites_Click);
        _menuItemReports.Add(_menuItemQueryFavorites);
        menuItemReports.AddSeparator();
        _menuItemUnfinalizedPay = new MenuItemOD("Unfinalized Payments", menuItemReportsUnfinalizedPay_Click);
        menuItemReports.Add(_menuItemUnfinalizedPay);
    }

    private void LayoutMenuTools(MenuItemOD menuItemTools)
    {
        //Snipping Tool-----------------------------------------------------------------------------------------------
        menuItemTools.Add("&Screen Snipping Tool", menuItemScreenSnip_Click);
        //Print Screen Tool-----------------------------------------------------------------------------------------------
        menuItemTools.Add("&Print Screen Tool", MenuItemPrintScreen_Click);
        //Misc Tools------------------------------------------------------------------------------------------------------
        var menuItemMiscTools = new MenuItemOD("Misc Tools");
        menuItemTools.Add(menuItemMiscTools);
        LayoutSubMenuMiscTools(menuItemMiscTools);
        menuItemTools.AddSeparator();
        //Menus below have no submenus (name as shown)--------------------------------------------------------------------
        menuItemTools.Add("Audit Trail", menuItemAuditTrail_Click);
        _menuItemFinanceCharges = new MenuItemOD("Billing/&Finance Charges", menuItemFinanceCharge_Click);
        menuItemTools.Add(_menuItemFinanceCharges);
        menuItemTools.Add("CC Recurring Charges", menuItemCCRecurring_Click);
        menuItemTools.Add("Certifications", menuItemCertifications_Click);
        //menuItemTools.Add("Dispensary",menuItemDispensary_Click);//FormDispensary is not fully functional and should not be an available option at this time
        menuItemTools.Add("Kiosk", menuItemTerminal_Click);
        menuItemTools.Add("Kiosk Manager", menuItemTerminalManager_Click);
        _menuItemLateCharges = new MenuItemOD("Late Charges", menuItemLateCharges_Click);
        menuItemTools.Add(_menuItemLateCharges);
        menuItemTools.Add("Ortho Auto Claims", menuItemOrthoAuto_Click);
        _menuItemPatDashboards = new MenuItemOD("Patient Dashboards");
        menuItemTools.Add(_menuItemPatDashboards);
        _menuItemPatPortalTransactions = new MenuItemOD("Patient Portal Transactions", menuItemXWebTrans_Click);
        menuItemTools.Add(_menuItemPatPortalTransactions);
        _menuItemOnlinePayments = new MenuItemOD("&Online Payments", menuItemOnlinePayments_Click);
        menuItemTools.Add(_menuItemOnlinePayments);
        _menuItemRepeatingCharges = new MenuItemOD("Repeating Charges", menuItemRepeatingCharges_Click);
        menuItemTools.Add(_menuItemRepeatingCharges);
        menuItemTools.Add("Setup Wizard", menuItemSetupWizard_Click);
        menuItemTools.Add("Zoom", menuItemZoom_Click);
    }

    private void LayoutSubMenuMiscTools(MenuItemOD menuItemMiscTools)
    {
        menuItemMiscTools.Add("Close Payment Plans", menuItemAutoClosePayPlans_Click);
        menuItemMiscTools.Add("Clear Duplicate Blockouts", menuItemDuplicateBlockouts_Click);
        _menuItemCreateAtoZ = new MenuItemOD("Create A to Z Folders", menuItemCreateAtoZFolders_Click);
        menuItemMiscTools.Add(_menuItemCreateAtoZ);
        menuItemMiscTools.Add("Merge Billing Types", menuItemMergeBillingType_Click);
        menuItemMiscTools.Add("Merge Discount Plans", menuItemMergeDPs_Click);
        menuItemMiscTools.Add("Merge Image Categories", menuItemMergeImageCat_Click);
        menuItemMiscTools.Add("Merge Patients", menuItemMergePatients_Click);
        menuItemMiscTools.Add("Merge Providers", menuItemMergeProviders_Click);
        menuItemMiscTools.Add("Merge Referrals", menuItemMergeReferrals_Click);
        menuItemMiscTools.Add("Move Subscribers", menuItemMoveSubscribers_Click);
        menuItemMiscTools.Add("Patient Status Setter", menuPatientStatusSetter_Click);
        _menuItemProcLockTool = new MenuItemOD("Procedure Lock Tool", menuItemProcLockTool_Click);
        menuItemMiscTools.Add(_menuItemProcLockTool);
        menuItemMiscTools.Add("Shutdown All Workstations", menuItemShutdown_Click);
        menuItemMiscTools.Add("Telephone Numbers", menuTelephone_Click);
    }

    private void LayoutMenuHelp(MenuItemOD menuItemHelp)
    {
        menuItemHelp.Add("Online Support", MenuItemRemote_Click);
        _menuItemLocalHelpWindows = new MenuItemOD("Local Help-Windows", MenuItemHelpWindows_Click);
        menuItemHelp.Add(_menuItemLocalHelpWindows);
        menuItemHelp.Add("Online Help - Contents", MenuItemHelpContents_Click);
        var menuItemOnlineHelpIndex = new MenuItemOD("Online Help - Index", MenuItemHelpIndex_Click);
        menuItemOnlineHelpIndex.ShortcutKeys = Keys.Shift | Keys.F1;
        menuItemHelp.Add(menuItemOnlineHelpIndex);
        menuItemHelp.Add("Training Videos", MenuItemWebinar_Click);
        menuItemHelp.Add("Query Monitor", MenuItemQueryMonitor_Click);
        menuItemHelp.AddSeparator();
        menuItemHelp.Add("About", (_, _) => Open<FormAbout>());
    }
}