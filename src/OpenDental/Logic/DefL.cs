using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Providers;
using OpenDental.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using Def = Imedisoft.Core.Entities.Def;

namespace OpenDental;

public class DefL
{
    private const string LanThis = "FormDefinitions";

    public static List<DefCatOptions> GetOptionsForDefCats(List<DefCat> defCats)
    {
        var defCatOptions = new List<DefCatOptions>();

        foreach (var defCat in defCats)
        {
            if (defCat.GetDescription() == "NotUsed")
            {
                continue;
            }

            if (defCat.GetDescription().Contains("HqOnly"))
            {
                continue;
            }

            var options = new DefCatOptions(defCat);
            switch (defCat)
            {
                case DefCat.AccountColors:
                    options.CanEditName = false;
                    options.EnableColor = true;
                    options.HelpText = "Changes the color of text for different types of entries in Account Module";
                    break;

                case DefCat.AccountQuickCharge:
                    options.CanDelete = true;
                    options.EnableValue = true;
                    options.ValueText = "Procedure Codes";
                    options.HelpText = "Account Proc Quick Add items.  Each entry can be a series of procedure codes separated by commas (e.g. D0180,D1101,D8220).  Used in the account module to quickly charge patients for items.";
                    break;

                case DefCat.AdjTypes:
                    options.EnableValue = true;
                    options.ValueText = "+, -, or dp";
                    options.HelpText = "Plus increases the patient balance.  Minus decreases it.  Dp means discount plan.  Not allowed to change value after creating new type since changes affect all patient accounts.";
                    break;

                case DefCat.AppointmentColors:
                    options.CanEditName = false;
                    options.EnableColor = true;
                    options.HelpText = "Changes colors of background in Appointments Module, and colors for completed appointments.";
                    break;

                case DefCat.ApptConfirmed:
                    options.EnableColor = true;
                    options.EnableValue = true;
                    options.ValueText = "Abbrev";
                    options.HelpText = "Color shows on each appointment if Appointment View is set to show ConfirmedColor.";
                    break;

                case DefCat.ApptProcsQuickAdd:
                    options.EnableValue = true;
                    options.ValueText = CultureInfo.CurrentCulture.Name.EndsWith("CA") ? "CDA Code(s)" : "ADA Code(s)";
                    options.HelpText = Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)
                        ? "These are the procedures that you can quickly add to the treatment plan from within the appointment editing window.  Multiple procedures may be separated by commas with no spaces. These definitions may be freely edited without affecting any patient records."
                        : "These are the procedures that you can quickly add to the treatment plan from within the appointment editing window. Multiple procedures may be separated by commas with no spaces. They generally will not require a tooth number, but a single tooth number is allowed. Example: D1111#8. These definitions may be freely edited without affecting any patient records.";
                    break;

                case DefCat.AutoDeposit:
                    options.CanDelete = true;
                    options.CanHide = true;
                    options.EnableValue = true;
                    options.ValueText = "Account Number";
                    break;

                case DefCat.AutoNoteCats:
                    options.CanDelete = true;
                    options.CanHide = false;
                    options.EnableValue = true;
                    options.IsValueDefNum = true;
                    options.ValueText = "Parent Category";
                    options.HelpText = "Each category can have a parent so that categories can be nested. Leave the Parent Category blank for categories at the root level. The order set here will only affect the order within the assigned Parent Category.";
                    break;

                case DefCat.BillingTypes:
                    options.EnableValue = true;
                    options.ValueText = "E, C, or CE";
                    options.HelpText = "E=Email bill, C=Collection, CE=Collection Excluded.  It is recommended to use as few billing types as possible.  They can be useful when running reports to separate delinquent accounts, but can cause 'forgotten accounts' if used without good office procedures. Changes affect all patients.";
                    break;

                case DefCat.BlockoutTypes:
                    options.EnableColor = true;
                    options.HelpText = "Blockout types are used in the appointments module.";
                    options.EnableValue = true;
                    options.ValueText = "Flags";
                    break;

                case DefCat.CertificationCategories:
                    options.HelpText = "Categories for employee certifications.";
                    break;

                case DefCat.ChartGraphicColors:
                    options.CanEditName = false;
                    options.EnableColor = true;
                    options.HelpText = Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)
                        ? "These colors will be used to graphically display treatments."
                        : "These colors will be used on the graphical tooth chart to draw restorations.";
                    break;

                case DefCat.ClaimCustomTracking:
                    options.CanDelete = true;
                    options.EnableValue = true;
                    options.ValueText = "Days Suppressed";
                    options.HelpText =
                        "Some offices may set up claim tracking statuses such as 'review', 'hold', 'riskmanage', etc.\r\n" +
                        "Set the value of 'Days Suppressed' to the number of days the claim will be suppressed from the Outstanding Claims Report when the status is changed to the selected status.";
                    break;

                case DefCat.ClaimErrorCode:
                    options.CanDelete = true;
                    options.CanHide = false;
                    options.EnableValue = true;
                    options.ValueText = "Description";
                    options.HelpText = "Used to track error codes when entering claim custom statuses.";
                    break;

                case DefCat.ClaimPaymentTracking:
                    options.ValueText = "Value";
                    options.HelpText = "EOB adjudication method codes to be used for insurance payments.  Last entry cannot be hidden.";
                    break;

                case DefCat.ClaimPaymentGroups:
                    options.ValueText = "Value";
                    options.HelpText = "Used to group claim payments in the daily payments report.";
                    break;

                case DefCat.ClinicSpecialty:
                    options.CanHide = true;
                    options.CanDelete = false;
                    options.HelpText = "You can add as many specialties as you want.  Changes affect all current records.";
                    break;

                case DefCat.CommLogTypes:
                    options.EnableValue = true;
                    options.EnableColor = true;
                    options.DoShowNoColor = true;
                    var commItemTypes = string.Join(", ", Commlogs.GetCommItemTypes().Select(x => x.GetDescription(useShortVersionIfAvailable: true)));
                    options.ValueText = "Usage";
                    options.HelpText = "Changes affect all current commlog entries.  Optionally set Usage to one of the following: " + commItemTypes + ". Only one of each. This helps automate new entries.";
                    break;

                case DefCat.ContactCategories:
                    options.HelpText = "You can add as many categories as you want.  Changes affect all current contact records.";
                    break;

                case DefCat.Diagnosis:
                    options.EnableValue = true;
                    options.ValueText = "1 or 2 letter abbreviation";
                    options.HelpText = "The diagnosis list is shown when entering a procedure.  Ones that are less used should go lower on the list.  The abbreviation is shown in the progress notes.  BE VERY CAREFUL.  Changes affect all patients.";
                    break;

                case DefCat.FeeColors:
                    options.CanEditName = false;
                    options.CanHide = false;
                    options.EnableColor = true;
                    options.HelpText = "These are the colors associated to fee types.";
                    break;

                case DefCat.ImageCats:
                    options.ValueText = "Usage";
                    options.HelpText = "These are the categories that will be available in the image and chart modules.  If you hide a category, images in that category will be hidden, so only hide a category if you are certain it has never been used.  Multiple categories can be set to show in the Chart module, but only one category should be set for patient pictures, statements, and tooth charts. Selecting multiple categories for treatment plans will save the treatment plan in each category. Affects all patient records.";
                    break;

                case DefCat.InsurancePaymentType:
                    options.CanDelete = true;
                    options.CanHide = true;
                    options.EnableValue = true;
                    options.ValueText = "N=Not selected for deposit";
                    options.HelpText = "These are claim payment types for insurance payments attached to claims.";
                    break;

                case DefCat.InsuranceVerificationStatus:
                    options.ValueText = "Usage";
                    options.HelpText = "These are statuses for the insurance verification list.";
                    break;

                case DefCat.LetterMergeCats:
                    options.HelpText = "Categories for Letter Merge.  You can safely make any changes you want.";
                    break;

                case DefCat.MiscColors:
                    options.CanEditName = false;
                    options.EnableColor = true;
                    options.DoShowNoColor = true;
                    options.HelpText = "";
                    break;

                case DefCat.OperatoryTypes:
                    options.CanDelete = true;
                    options.CanHide = true;
                    options.CanEditName = true;
                    options.HelpText = "Types for the Operatory. This value is not normally used.";
                    break;

                case DefCat.PaymentTypes:
                    options.EnableValue = true;
                    options.ValueText = "N=Not selected for deposit";
                    options.HelpText = "Types of payments that patients might make. Any changes will affect all patients.";
                    break;

                case DefCat.PayPlanCategories:
                    options.HelpText = "Assign payment plans to different categories";
                    break;

                case DefCat.PaySplitUnearnedType:
                    options.ValueText = "Do Not Show on Account";
                    options.HelpText = "Typically used when a payment is posted to an account with a credit or no balance. Any changes will affect all patients.";
                    options.EnableValue = true;
                    break;

                case DefCat.ProcButtonCats:
                    options.HelpText = "These are similar to the procedure code categories, but are only used for organizing and grouping the procedure buttons in the Chart module.";
                    break;

                case DefCat.ProcCodeCats:
                    options.HelpText = "These are the categories for organizing procedure codes. They do not have to follow ADA categories.  There is no relationship to insurance categories which are setup in the Ins Categories section.  Does not affect any patient records.";
                    break;

                case DefCat.ProgNoteColors:
                    options.CanEditName = false;
                    options.EnableColor = true;
                    options.HelpText = "Changes color of text for different types of entries in the Chart Module Progress Notes.";
                    break;

                case DefCat.Prognosis:
                    break;

                case DefCat.ProviderSpecialties:
                    options.HelpText = "Provider specialties cannot be deleted.  Changes to provider specialties could affect e-claims.";
                    break;

                case DefCat.RecallUnschedStatus:
                    options.EnableValue = true;
                    options.ValueText = "Abbreviation";
                    options.HelpText = "Recall/Unsched Status.  Abbreviation must be 7 characters or less.  Changes affect all patients.";
                    break;

                case DefCat.Regions:
                    options.CanHide = false;
                    options.HelpText = "The region identifying the clinic it is assigned to.";
                    break;

                case DefCat.SupplyCats:
                    options.CanDelete = true;
                    options.CanHide = false;
                    options.HelpText = "The categories for inventory supplies.";
                    break;

                case DefCat.TaskCategories:
                    options.CanDelete = true;
                    options.DoShowNoColor = true;
                    options.EnableColor = true;
                    options.HelpText = "The categories for tasks. HQ Only as of now.";
                    break;

                case DefCat.TaskPriorities:
                    options.EnableColor = true;
                    options.EnableValue = true;
                    options.ValueText = "D = Default, R = Reminder";
                    options.HelpText = "Priorities available for selection within the task edit window.  Task lists are sorted using the order of these priorities.  They can have any description and color.  At least one priority should be Default (D).  If more than one priority is flagged as the default, the last default in the list will be used.  If no default is set, the last priority will be used.  Use (R) to indicate the initial reminder task priority to use when creating reminder tasks.  Changes affect all tasks where the definition is used.";
                    break;

                case DefCat.TxPriorities:
                    options.EnableColor = true;
                    options.EnableValue = true;
                    options.DoShowItemOrderInValue = true;
                    options.ValueText = "Internal Priority";
                    options.HelpText =
                        "Displayed order should match order of priority of treatment.  They are used in Treatment Plan and Chart " +
                        "modules. They can be simple numbers or descriptive abbreviations 7 letters or less.  Changes affect all procedures where the " +
                        "definition is used.  'Internal Priority' does not show, but is used for list order and for automated selection of which procedures " +
                        "are next in a planned appointment.";
                    break;

                case DefCat.CarrierGroupNames:
                    options.CanHide = true;
                    options.HelpText = "These are group names for Carriers.";
                    break;

                case DefCat.TimeCardAdjTypes:
                    options.CanEditName = true;
                    options.CanHide = true;
                    options.HelpText = "These are PTO Adjustments Types used for tracking on employee time cards and ADP export.";
                    break;
            }

            defCatOptions.Add(options);
        }

        return defCatOptions;
    }

    private static string GetItemDescForImages(string itemValue)
    {
        var listDescriptions = new List<string>();
        if (itemValue.Contains("X"))
        {
            listDescriptions.Add(Lan.g(LanThis, "ChartModule"));
        }

        if (itemValue.Contains("M"))
        {
            listDescriptions.Add(Lan.g(LanThis, "Thumbnails"));
        }

        if (itemValue.Contains("F"))
        {
            listDescriptions.Add(Lan.g(LanThis, "PatientForm"));
        }

        if (itemValue.Contains("P"))
        {
            listDescriptions.Add(Lan.g(LanThis, "PatientPic"));
        }

        if (itemValue.Contains("S"))
        {
            listDescriptions.Add(Lan.g(LanThis, "Statement"));
        }

        if (itemValue.Contains("T"))
        {
            listDescriptions.Add(Lan.g(LanThis, "ToothChart"));
        }

        if (itemValue.Contains("R"))
        {
            listDescriptions.Add(Lan.g(LanThis, "TreatPlans"));
        }

        if (itemValue.Contains("L"))
        {
            listDescriptions.Add(Lan.g(LanThis, "PatientPortal"));
        }

        if (itemValue.Contains("A"))
        {
            listDescriptions.Add(Lan.g(LanThis, "PayPlans"));
        }

        if (itemValue.Contains("C"))
        {
            listDescriptions.Add(Lan.g(LanThis, "ClaimAttachments"));
        }

        if (itemValue.Contains("B"))
        {
            listDescriptions.Add(Lan.g(LanThis, "LabCases"));
        }

        if (itemValue.Contains("U"))
        {
            listDescriptions.Add(Lan.g(LanThis, "AutoSaveForms"));
        }

        if (itemValue.Contains("Y"))
        {
            listDescriptions.Add(Lan.g(LanThis, "TaskAttachments"));
        }

        if (itemValue.Contains("N"))
        {
            listDescriptions.Add(Lan.g(LanThis, "ClaimResponses"));
        }

        return string.Join(", ", listDescriptions);
    }

    public static void FillGridDefs(GridOD gridDefs, DefCatOptions defCatOptionsSelected, List<Def> defs)
    {
        Def selectedDef = null;
        if (gridDefs.GetSelectedIndex() > -1)
        {
            selectedDef = (Def) gridDefs.ListGridRows[gridDefs.GetSelectedIndex()].Tag;
        }

        var scroll = gridDefs.ScrollValue;
        gridDefs.BeginUpdate();

        gridDefs.Columns.Clear();
        gridDefs.Columns.Add(new GridColumn("Name", 190));
        gridDefs.Columns.Add(new GridColumn(defCatOptionsSelected.ValueText, 190));
        gridDefs.Columns.Add(new GridColumn(defCatOptionsSelected.EnableColor ? "Color" : "", 40));
        gridDefs.Columns.Add(new GridColumn(defCatOptionsSelected.CanHide ? "Hide" : "", 30, HorizontalAlignment.Center));

        gridDefs.ListGridRows.Clear();

        foreach (var def in defs)
        {
            if (Defs.IsDefDeprecated(def))
            {
                def.IsHidden = true;
            }

            var gridRow = new GridRow();

            gridRow.Cells.Add(def.ItemName);

            switch (defCatOptionsSelected.DefCat)
            {
                case DefCat.ImageCats:
                    gridRow.Cells.Add(GetItemDescForImages(def.ItemValue));
                    break;

                case DefCat.AutoNoteCats:
                {
                    var autoNoteDefs = defs.ToDictionary(x => x.DefNum.ToString(), x => x.ItemName);

                    gridRow.Cells.Add(autoNoteDefs.TryGetValue(def.ItemValue, out var nameCur) ? nameCur : def.ItemValue);
                    break;
                }

                default:
                {
                    gridRow.Cells.Add(defCatOptionsSelected.DoShowItemOrderInValue ? def.ItemOrder.ToString() : def.ItemValue);
                    break;
                }
            }

            gridRow.Cells.Add("");
            if (defCatOptionsSelected.EnableColor)
            {
                gridRow.Cells[gridRow.Cells.Count - 1].ColorBackG = def.ItemColor;
            }

            gridRow.Cells.Add(def.IsHidden ? "X" : "");
            gridRow.Tag = def;

            gridDefs.ListGridRows.Add(gridRow);
        }

        gridDefs.EndUpdate();

        if (selectedDef is not null)
        {
            for (var i = 0; i < gridDefs.ListGridRows.Count; i++)
            {
                if (((Def) gridDefs.ListGridRows[i].Tag).DefNum != selectedDef.DefNum)
                {
                    continue;
                }

                gridDefs.SetSelected(i);
                break;
            }
        }

        gridDefs.ScrollValue = scroll;
    }

    public static bool GridDefsDoubleClick(Def defSelected, DefCatOptions defCatOptionsSelected, List<Def> listDefs, List<Def> listDefsAll, bool isDefChanged)
    {
        switch (defCatOptionsSelected.DefCat)
        {
            case DefCat.BlockoutTypes:
                using (var formDefEditBlockout = new FormDefEditBlockout(defSelected))
                {
                    if (formDefEditBlockout.ShowDialog() == DialogResult.OK)
                    {
                        isDefChanged = true;
                    }
                }

                break;

            case DefCat.ImageCats:
                using (var formDefEditImages = new FormDefEditImages(defSelected))
                {
                    formDefEditImages.IsNew = false;

                    if (formDefEditImages.ShowDialog() == DialogResult.OK)
                    {
                        isDefChanged = true;
                    }
                }

                break;

            default:
                using (var formDefEdit = new FormDefEdit(defSelected, listDefs, defCatOptionsSelected))
                {
                    formDefEdit.IsNew = false;

                    if (formDefEdit.ShowDialog() == DialogResult.OK)
                    {
                        if (formDefEdit.IsDeleted)
                        {
                            listDefsAll.Remove(defSelected);
                        }

                        isDefChanged = true;
                    }
                }

                break;
        }

        return isDefChanged;
    }

    public static bool AddDef(GridOD grid, DefCatOptions selectedDefCatOptions)
    {
        var def = new Def
        {
            IsNew = true
        };

        var itemOrder = 0;
        if (Defs.GetDefsForCategory(selectedDefCatOptions.DefCat).Count > 0)
        {
            itemOrder = Defs.GetDefsForCategory(selectedDefCatOptions.DefCat).Max(x => x.ItemOrder) + 1;
        }

        def.ItemOrder = itemOrder;
        def.Category = selectedDefCatOptions.DefCat;
        def.ItemName = "";
        def.ItemValue = "";

        if (selectedDefCatOptions.DefCat == DefCat.InsurancePaymentType)
        {
            def.ItemValue = "N";
        }

        switch (selectedDefCatOptions.DefCat)
        {
            case DefCat.BlockoutTypes:
                using (var formDefEditBlockout = new FormDefEditBlockout(def))
                {
                    if (formDefEditBlockout.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }
                }

                break;

            case DefCat.ImageCats:
                using (var formDefEditImages = new FormDefEditImages(def))
                {
                    formDefEditImages.IsNew = true;

                    if (formDefEditImages.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }
                }

                break;

            default:
                var currentDefs = new List<Def>();

                foreach (var gridRow in grid.ListGridRows)
                {
                    currentDefs.Add((Def) gridRow.Tag);
                }

                using (var formDefEdit = new FormDefEdit(def, currentDefs, selectedDefCatOptions))
                {
                    formDefEdit.IsNew = true;

                    if (formDefEdit.ShowDialog() != DialogResult.OK)
                    {
                        return false;
                    }
                }

                break;
        }

        return true;
    }

    public static bool TryHideDefSelectedInGrid(GridOD gridDefs, DefCatOptions selectedDefCatOpt)
    {
        if (gridDefs.GetSelectedIndex() == -1)
        {
            MsgBox.Show(LanThis, "Please select item first,");
            return false;
        }

        var defSelected = (Def) gridDefs.ListGridRows[gridDefs.GetSelectedIndex()].Tag;

        if (!CanHideDef(defSelected, selectedDefCatOpt))
        {
            return false;
        }

        HideDef(defSelected);
        return true;
    }

    public static bool CanHideDef(Def def, DefCatOptions defCatOptions)
    {
        if (def.IsHidden)
        {
            return true;
        }

        if (!defCatOptions.CanHide || !defCatOptions.CanEditName)
        {
            MsgBox.Show(LanThis, "Definitions of this category cannot be hidden.");

            return false;
        }

        var visibleDefs = Defs.GetDefsForCategory(defCatOptions.DefCat, true);
        if (Defs.NeedOneUnhidden(def.Category) && visibleDefs.Count == 1)
        {
            MsgBox.Show(LanThis, "You cannot hide the last definition in this category.");

            return false;
        }

        if (def.Category == DefCat.ProviderSpecialties)
        {
            if (Providers.IsSpecialtyInUse(def.DefNum))
            {
                MsgBox.Show(LanThis, "You cannot hide a specialty if it is in use by a provider.");
                return false;
            }

            if (Referrals.IsSpecialtyInUse(def.DefNum))
            {
                MsgBox.Show(LanThis, "You cannot hide a specialty if it is in use by a referral source.");
                return false;
            }
        }

        if (Defs.IsDefinitionInUse(def))
        {
            var isClinicDefaultBillingType = ClinicPrefs.GetPrefAllClinics(PrefName.PracticeDefaultBillType).Any(x => x.ValueString == def.DefNum.ToString());

            if (def.DefNum.In(
                    PrefC.GetLong(PrefName.BrokenAppointmentAdjustmentType),
                    PrefC.GetLong(PrefName.AppointmentTimeArrivedTrigger),
                    PrefC.GetLong(PrefName.AppointmentTimeSeatedTrigger),
                    PrefC.GetLong(PrefName.AppointmentTimeDismissedTrigger),
                    PrefC.GetLong(PrefName.TreatPlanDiscountAdjustmentType),
                    PrefC.GetLong(PrefName.BillingChargeAdjustmentType),
                    PrefC.GetLong(PrefName.FinanceChargeAdjustmentType),
                    PrefC.GetLong(PrefName.LateChargeAdjustmentType),
                    PrefC.GetLong(PrefName.PrepaymentUnearnedType),
                    PrefC.GetLong(PrefName.SalesTaxAdjustmentType),
                    PrefC.GetLong(PrefName.RecurringChargesPayTypeCC),
                    PrefC.GetLong(PrefName.EraChkPaymentType),
                    PrefC.GetLong(PrefName.EraAchPaymentType),
                    PrefC.GetLong(PrefName.EraFwtPaymentType),
                    PrefC.GetLong(PrefName.EraDefaultPaymentType)))
            {
                MsgBox.Show(LanThis, "You cannot hide a definition if it is in use within Preferences.");
                return false;
            }

            if (def.DefNum.In(
                    PrefC.GetLong(PrefName.RecallStatusMailed),
                    PrefC.GetLong(PrefName.RecallStatusTexted),
                    PrefC.GetLong(PrefName.RecallStatusEmailed),
                    PrefC.GetLong(PrefName.RecallStatusEmailedTexted)))
            {
                MsgBox.Show(LanThis, "You cannot hide a definition that is used as a status in the Setup Recall window.");
                return false;
            }

            if (def.DefNum == PrefC.GetLong(PrefName.WebSchedNewPatConfirmStatus))
            {
                MsgBox.Show(LanThis, "You cannot hide a definition that is used as an appointment confirmation status in Web Sched New Pat Appt.");
                return false;
            }

            if (def.DefNum == PrefC.GetLong(PrefName.WebSchedRecallConfirmStatus))
            {
                MsgBox.Show(LanThis, "You cannot hide a definition that is used as an appointment confirmation status in Web Sched Recall Appt.");
                return false;
            }

            if (def.DefNum == PrefC.GetLong(PrefName.PracticeDefaultBillType))
            {
                MsgBox.Show(LanThis, "You cannot hide a billing type when it is selected as the practice default billing type.");
                return false;
            }

            if (isClinicDefaultBillingType)
            {
                MsgBox.Show(LanThis, "You cannot hide a billing type when it is selected as a clinic's default billing type.");
                return false;
            }

            if (Defs.IsPaymentTypeInUse(def))
            {
                MsgBox.Show(LanThis, "You cannot hide a payment type when it is the default payment type for PayConnect, PaySimple, EdgeExpress, or XCharge.");
                return false;
            }

            if (!MsgBox.Show(LanThis, MsgBoxButtons.OKCancel, "Warning: This definition is currently in use within the program."))
            {
                return false;
            }
        }

        if (def.Category == DefCat.PaySplitUnearnedType)
        {
            if (visibleDefs.FindAll(x => string.IsNullOrEmpty(x.ItemValue)).Count == 1 && def.ItemValue == "")
            {
                MsgBox.Show(LanThis, "Must have at least one definition that shows in Account");
                return false;
            }
        }

        if (defCatOptions.DefCat != DefCat.BillingTypes || !Patients.IsBillingTypeInUse(def.DefNum))
        {
            return true;
        }
        
        return MsgBox.Show(LanThis, MsgBoxButtons.OKCancel, "Warning: Billing type is currently in use by patients, insurance plans, or preferences.");
    }

    public static bool UpClick(GridOD grid)
    {
        if (grid.GetSelectedIndex() == -1)
        {
            ODMessageBox.Show("Please select an item first.");
            return false;
        }

        if (grid.GetSelectedIndex() == 0)
        {
            return false;
        }

        var defSelected = (Def) grid.ListGridRows[grid.GetSelectedIndex()].Tag;
        var defAbove = (Def) grid.ListGridRows[grid.GetSelectedIndex() - 1].Tag;

        (defSelected.ItemOrder, defAbove.ItemOrder) = (defAbove.ItemOrder, defSelected.ItemOrder);

        Update(defSelected);
        Update(defAbove);

        return true;
    }

    public static bool DownClick(GridOD grid)
    {
        if (grid.GetSelectedIndex() == -1)
        {
            ODMessageBox.Show("Please select an item first.");
            return false;
        }

        if (grid.GetSelectedIndex() == grid.ListGridRows.Count - 1)
        {
            return false;
        }

        var defSelected = (Def) grid.ListGridRows[grid.GetSelectedIndex()].Tag;
        var defBelow = (Def) grid.ListGridRows[grid.GetSelectedIndex() + 1].Tag;

        (defSelected.ItemOrder, defBelow.ItemOrder) = (defBelow.ItemOrder, defSelected.ItemOrder);

        Update(defSelected);
        Update(defBelow);

        return true;
    }

    public static void Insert(Def def)
    {
        var logMessage = "Definition created: " + def.ItemName + " with category: " + def.Category.GetDescription();

        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logMessage);

        Defs.Insert(def);
    }

    public static void Update(Def def)
    {
        var logMessage = "Definition edited: " + def.ItemName + " with category: " + def.Category.GetDescription();

        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logMessage);

        Defs.Update(def);
    }

    public static void HideDef(Def def)
    {
        var logMessage = "Definition hidden: " + def.ItemName + " with category: " + def.Category.GetDescription();

        SecurityLogs.MakeLogEntry(EnumPermType.DefEdit, 0, logMessage);

        Defs.HideDef(def);
    }
}