using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenDentBusiness;
using System.ComponentModel;
using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDental;

public partial class UserControlSecurityTree : UserControl
{
    private TreeNode _clickedPermNode;
    private List<long> _listUserGroupNums = [];
    
    public delegate DialogResult SecurityTreeEventHandler(object sender, SecurityEventArgs e);

    [Category("OD")]
    [Description("Occurs when the Report Permission window would be shown. Open the Report Permission form and return its DialogResult.")]
    public event SecurityTreeEventHandler ReportPermissionChecked;

    [Category("OD")]
    [Description("Occurs when the Group Permission Edit window would be shown. Open the Group Permission form and return its DialogResult.")]
    public event SecurityTreeEventHandler GroupPermissionChecked;

    [Category("OD")]
    [Description("Occurs when the Definition Picker window would be shown. Open the Definition Picker Form and return its DialogResult.")]
    public event SecurityTreeEventHandler AdjustmentTypeDenyPermissionChecked;

    [Category("OD")]
    [Description("Set to true to disallow users from interacting with the control. The tree will still display correctly.")]
    public bool ReadOnly { get; set; }

    public UserControlSecurityTree()
    {
        InitializeComponent();
    }

    private void UserControlSecurityTree_Load(object sender, EventArgs e)
    {
        if (ReadOnly)
        {
            treePermissions.BackColor = SystemColors.Control;
        }
    }

    public void FillTreePermissionsInitial()
    {
        TreeNode node;
        TreeNode node2; //second level
        TreeNode node3; //third level
        TreeNode node4; //fourth level

        #region Main Menu

        node = SetNode("Main Menu");

        #region File

        node2 = SetNode("File");
        node3 = SetNode(EnumPermType.GraphicsEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ChooseDatabase);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.PrinterSetup);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);

        #endregion

        #region Setup

        node2 = SetNode(EnumPermType.Setup);

        #region EHR

        node3 = SetNode("Chart - EHR");
        node4 = SetNode(EnumPermType.EhrEmergencyAccess);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.EhrMeasureEventEdit);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);

        #endregion

        node3 = SetNode("Advanced Setup");
        node4 = SetNode(EnumPermType.ReplicationSetup);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ShowFeatures);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AutoNoteQuickNoteEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode("Definitions");
        node4 = SetNode(EnumPermType.DefEdit);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);

        #region Dental School

        node3 = SetNode("Dental School");
        node4 = SetNode(EnumPermType.AdminDentalInstructors);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.AdminDentalStudents);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.AdminDentalEvaluations);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);

        #endregion

        node3 = SetNode(EnumPermType.Schedules);
        node2.Nodes.Add(node3);
        node3 = SetNode("Security");
        node4 = SetNode(EnumPermType.SecurityAdmin);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.AddNewUser);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ManageHighSecurityProgProperties);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.UpdateInstall);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.BadgeIdEdit);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);

        #endregion

        #region Lists

        node2 = SetNode("Lists");
        node3 = SetNode(EnumPermType.ProcCodeEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.FeeSchedEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AllowFeeEditWhileReceivingClaim);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProviderFeeEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.MedicationDefEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AllergyDefEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AllergyMerge);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProblemDefEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode("Providers");
        node4 = SetNode(EnumPermType.ProviderAdd);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ProviderEdit);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ProviderAlphabetize);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);

        #region Clinics

        node3 = SetNode("Clinics");
        node4 = SetNode(EnumPermType.ClinicEdit);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.UnrestrictedSearch);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);

        #endregion

        #region Referrals

        node3 = SetNode("Referrals");
        node4 = SetNode(EnumPermType.ReferralAdd);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ReferralEdit);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.RefAttachAdd);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.RefAttachDelete);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);

        #endregion

        node.Nodes.Add(node2);

        #endregion

        #region Reports

        node2 = SetNode(EnumPermType.Reports);
        node3 = SetNode(EnumPermType.ReportProdIncAllProviders);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ReportDailyAllProviders);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.GraphicalReportSetup);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.GraphicalReports);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.UserQuery);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.UserQueryAdmin);
        node2.Nodes.Add(node3);
        if (! /* ODEnvironment.IsCloudServer */ false)
        {
            node3 = SetNode(EnumPermType.CommandQuery);
            node2.Nodes.Add(node3);
        }

        node3 = SetNode(EnumPermType.NewClaimsProcNotBilled);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);

        #endregion

        #region Tools

        node2 = SetNode("Tools");
        node3 = SetNode("Misc Tools");
        node4 = SetNode(EnumPermType.MedicationMerge);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.PatientMerge);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ProviderMerge);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.ReferralMerge);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.Advertising);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AuditTrail);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.CertificationEmployee);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.CertificationSetup);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.RepeatChargeTool);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.SetupWizard);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.WikiAdmin);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.WikiListSetup);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.WebFormAccess);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.Zoom);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);

        #endregion

        #region eServices

        node2 = SetNode("eServices");
        node3 = SetNode(EnumPermType.EServicesSetup);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);

        #endregion

        #region Help

        node2 = SetNode("Help");
        node3 = SetNode(EnumPermType.QueryMonitor);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);

        #endregion

        treePermissions.Nodes.Add(node);

        #endregion

        #region Main Toolbar

        node = SetNode("Main Toolbar");
        node2 = SetNode(EnumPermType.CommlogCreate);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.CommlogEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.EmailSend);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TextMessageView);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TextMessageSend);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.WebMailSend);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.SheetEdit);
        node.Nodes.Add(node2);
        node3 = SetNode(EnumPermType.SheetDelete);
        node2.Nodes.Add(node3);
        node2 = SetNode(EnumPermType.EFormEdit);
        node.Nodes.Add(node2);
        node3 = SetNode(EnumPermType.EFormDelete);
        node2.Nodes.Add(node3);
        node2 = SetNode(EnumPermType.TaskEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TaskNoteEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TaskDelete);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TaskListCreate);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PopupEdit);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Appts Module

        node = SetNode(EnumPermType.AppointmentsModule);
        node2 = SetNode(EnumPermType.AppointmentCreate);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.AppointmentMove);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.AppointmentResize);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.AppointmentEdit);
        node.Nodes.Add(node2);
        node3 = SetNode(EnumPermType.AppointmentDelete);
        node2.Nodes.Add(node3);
        node2 = SetNode(EnumPermType.AppointmentCompleteEdit);
        node.Nodes.Add(node2);
        node3 = SetNode(EnumPermType.AppointmentCompleteDelete);
        node2.Nodes.Add(node3);
        node2 = SetNode(EnumPermType.ViewAppointmentAuditTrail);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.EcwAppointmentRevise);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.InsPlanVerifyList);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ApptConfirmStatusEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.Blockouts);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Family Module

        node = SetNode(EnumPermType.FamilyModule);
        node2 = SetNode(EnumPermType.InsPlanEdit);
        node.Nodes.Add(node2);
        node3 = SetNode(EnumPermType.InsPlanPickListExisting);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.InsuranceVerification);
        node2.Nodes.Add(node3);
        node2 = SetNode(EnumPermType.InsPlanChangeAssign);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.InsPlanChangeSubsc);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.InsPlanOrthoEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.CarrierCreate);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.CarrierEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatientBillingEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatPriProvEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatientApptRestrict);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ArchivedPatientSelect);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ArchivedPatientEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatientSSNView);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatientDOBView);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatientEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.SuperFamilyDisband);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Account Module

        node = SetNode(EnumPermType.AccountModule);
        node2 = SetNode("Claim");
        node3 = SetNode(EnumPermType.ClaimSend);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimSentEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimDelete);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimHistoryEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimView);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimProcClaimAttachedProvEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimProcFeeBilledToInsEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ClaimProcReceivedEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.UpdateCustomTracking);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.PreAuthSentEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.AccountProcsQuickAdd);
        node.Nodes.Add(node2);
        node2 = SetNode("Insurance Payment");
        node3 = SetNode(EnumPermType.InsPayCreate);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.InsPayEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.InsWriteOffEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Payment");
        node3 = SetNode(EnumPermType.PaymentCreate);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.PaymentEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.SplitCreatePastLockDate);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Payment Plan");
        node3 = SetNode(EnumPermType.PayPlanEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.PayPlanChargeDateEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Adjustment");
        node3 = SetNode(EnumPermType.AdjustmentCreate);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AdjustmentEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AdjustmentEditZero);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AdjustmentTypeDeny);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Statement");
        node3 = SetNode(EnumPermType.StatementCSV);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Treat Plan Module

        node = SetNode(EnumPermType.TPModule);
        node2 = SetNode(EnumPermType.TreatPlanEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TreatPlanPresenterEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.TreatPlanSign);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Chart Module

        node = SetNode(EnumPermType.ChartModule);
        node2 = SetNode("Procedure");
        node3 = SetNode(EnumPermType.ProcExistingEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcEditShowFee);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcDelete);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcedureNoteFull);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcedureNoteUser);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.GroupNoteEditSigned);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Completed Procedure");
        node3 = SetNode(EnumPermType.ProcComplCreate);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcCompleteEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcCompleteStatusEdit);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcCompleteNote);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcCompleteAddAdj);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProcCompleteEditMisc);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Rx");
        node3 = SetNode(EnumPermType.RxCreate);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.RxEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.OrthoChartEditFull);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.OrthoChartEditUser);
        node.Nodes.Add(node2);
        if (!Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum))
        {
            node2 = SetNode(EnumPermType.PerioEdit);
            node.Nodes.Add(node2);
        }

        node2 = SetNode(EnumPermType.PerioEditCopy);
        node.Nodes.Add(node2);
        node2 = SetNode("Anesthesia");
        node3 = SetNode(EnumPermType.AnesthesiaIntakeMeds);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AnesthesiaControlMeds);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatMedicationListEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatAllergyListEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.PatProblemListEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ChartViewsEdit);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Imaging Module

        node = SetNode(EnumPermType.ImagingModule);
        node2 = SetNode(EnumPermType.ImageCreate);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ImageDelete);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ImageEdit);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ImageExport);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.ImageSignatureCreate);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.SignedImageEdit);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        #region Manage Module

        node = SetNode(EnumPermType.ManageModule);
        node2 = SetNode(EnumPermType.Accounting);
        node3 = SetNode(EnumPermType.AccountingCreate);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.AccountingEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.Billing);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.DepositSlips);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.Backup);
        node.Nodes.Add(node2);
        node2 = SetNode("Time Card");
        node3 = SetNode(EnumPermType.TimecardsEditAll);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.TimecardDeleteEntry);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.ProtectedLeaveAdjustmentEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        node2 = SetNode("Supply Inventory");
        node3 = SetNode("Equipment");
        node4 = SetNode(EnumPermType.EquipmentSetup);
        node3.Nodes.Add(node4);
        node4 = SetNode(EnumPermType.EquipmentDelete);
        node3.Nodes.Add(node4);
        node2.Nodes.Add(node3);
        node3 = SetNode(EnumPermType.SupplierEdit);
        node2.Nodes.Add(node3);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);

        #endregion

        node = SetNode("Merge Tools");
        node2 = SetNode(EnumPermType.InsCarrierCombine);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.InsPlanMerge);
        node.Nodes.Add(node2);
        node2 = SetNode(EnumPermType.RxMerge);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);
        node = SetNode("Web Applications");
        node2 = SetNode(EnumPermType.MobileWeb);
        node.Nodes.Add(node2);
        treePermissions.Nodes.Add(node);
        treePermissions.ExpandAll();
    }

    public void FillTreePerm()
    {
        GroupPermissions.RefreshCache();
        
        treePermissions.Enabled = _listUserGroupNums.Count != 0;
        treePermissions.BeginUpdate();
        
        for (var i = 0; i < treePermissions.Nodes.Count; i++)
        {
            FillNodes(treePermissions.Nodes[i], _listUserGroupNums);
        }

        treePermissions.EndUpdate();
    }
    
    private static TreeNode SetNode(string text)
    {
        return new TreeNode
        {
            Text = text,
            Tag = EnumPermType.None,
            ImageIndex = 0,
            SelectedImageIndex = 0
        };
    }
    
    private static TreeNode SetNode(EnumPermType perm)
    {
        return new TreeNode
        {
            Text = GroupPermissions.GetDesc(perm),
            Tag = perm,
            ImageIndex = 1,
            SelectedImageIndex = 1
        };
    }

    ///<summary>Returns an integer associated to the TreeNode.ImageIndex for the passed in PermType. Determined by the amount of FKeys.</summary>
    private static int GetNodeStatusForPerm(EnumPermType permission, List<GroupPermission> listGroupPerms, List<long> listUserGroupNums)
    {
        var listAllPermsForGroup = listGroupPerms.FindAll(x => x.PermType == permission && listUserGroupNums.Contains(x.UserGroupNum) && x.FKey == 0);
        var listIndividualPermsForGroup = listGroupPerms.FindAll(x => x.PermType == permission && listUserGroupNums.Contains(x.UserGroupNum) && x.FKey != 0);
        
        if (listIndividualPermsForGroup.Count == 0 && listAllPermsForGroup.Count == listUserGroupNums.Count)
        {
            return 2; //Check
        }

        if (listIndividualPermsForGroup.Count > 0 || listAllPermsForGroup.Count > 0)
        {
            return 3; //Square
        }

        return 1; //Empty
    }

    private void FillNodes(TreeNode node, List<long> listUserGroupNums)
    {
        //first, any child nodes
        for (var i = 0; i < node.Nodes.Count; i++)
        {
            FillNodes(node.Nodes[i], listUserGroupNums);
        }

        //then this node
        if (node.ImageIndex == 0)
        {
            return;
        }

        node.ImageIndex = 1;
        node.Text = GroupPermissions.GetDesc((EnumPermType) node.Tag);
        
        //get all grouppermissions for the passed-in usergroups
        var listGroupPerms = GroupPermissions.GetForUserGroups(listUserGroupNums);
        
        //group by permtype, preferring newerdays/newerdate that are further back in the past.
        listGroupPerms = listGroupPerms.GroupBy(x => x.PermType)
            .Select(x => x.OrderBy(y =>
                {
                    return y.NewerDays switch
                    {
                        0 when y.NewerDate == DateTime.MinValue => DateTime.MinValue,
                        0 => y.NewerDate,
                        _ => DateTime.Today.AddDays(-y.NewerDays)
                    };
                })
                .FirstOrDefault())
            .ToList();
        
        foreach (var t in listGroupPerms)
        {
            if (!listUserGroupNums.Contains(t.UserGroupNum) || t.PermType != (EnumPermType) node.Tag)
            {
                continue;
            }
            
            node.ImageIndex = 2;
            if (t.NewerDate.Year > 1880)
            {
                node.Text += " (if date newer than " + t.NewerDate.ToShortDateString() + ")";
            }
            else if (t.NewerDays > 0)
            {
                node.Text += " (if days newer than " + t.NewerDays + ")";
            }

            if ((EnumPermType) node.Tag != EnumPermType.AdjustmentTypeDeny)
            {
                continue;
            }
            
            var listGroupPermAdjustmentTypesDenied = GroupPermissions.GetAdjustmentTypeDenyPermsForUserGroup(t.UserGroupNum);
            var countAdjustmentTypesDeniedStr = listGroupPermAdjustmentTypesDenied.Count.ToString();
            if (listGroupPermAdjustmentTypesDenied.Any(x => x.FKey == 0))
            {
                countAdjustmentTypesDeniedStr = "All";
            }

            node.Text += " (" + countAdjustmentTypesDeniedStr + " adjustment types denied)";
        }

        var listPermissionsSpecial = new List<EnumPermType>
        {
            EnumPermType.AdjustmentTypeDeny,
            EnumPermType.Reports
        };
        
        if (listPermissionsSpecial.Contains((EnumPermType) node.Tag))
        {
            node.ImageIndex = GetNodeStatusForPerm((EnumPermType) node.Tag, listGroupPerms, listUserGroupNums);
        }
    }

    public void CollapseAll()
    {
        treePermissions.CollapseAll();
    }

    public void ExpandAll()
    {
        treePermissions.ExpandAll();
    }

    public void SetAll()
    {
        if (_listUserGroupNums.Count != 1)
        {
            throw new Exception("SetAll may not be called when multiple usergroups are selected.");
        }

        var userGroupNum = _listUserGroupNums.First();
        for (var i = 0; i < Enum.GetNames(typeof(EnumPermType)).Length; i++)
        {
            var permType = (EnumPermType) i;
            if (permType is EnumPermType.SecurityAdmin or EnumPermType.StartupMultiUserOld or EnumPermType.StartupSingleUserOld)
            {
                continue;
            }

            if (GroupPermissions.DoesPermissionTreatZeroFKeyAsAll(permType))
            {
                GroupPermissions.GiveUserGroupPermissionAll(userGroupNum, permType);
                
                continue;
            }

            var perm = GroupPermissions.GetPerm(userGroupNum, permType);
            if (perm is not null)
            {
                continue;
            }
            
            perm = new GroupPermission
            {
                PermType = permType,
                UserGroupNum = userGroupNum
            };
            try
            {
                GroupPermissions.Insert(perm);
            }
            catch (Exception ex)
            {
                ODMessageBox.Show(ex.Message);
            }
        }

        FillTreePerm();
    }

    public void SetNone()
    {
        if (_listUserGroupNums.Count != 1)
        {
            throw new Exception("SetNone may not be called when multiple usergroups are selected.");
        }

        var userGroupNum = _listUserGroupNums.First();
        for (var i = 0; i < Enum.GetNames(typeof(EnumPermType)).Length; i++)
        {
            var permType = (EnumPermType) i;
            if (permType is EnumPermType.SecurityAdmin or EnumPermType.StartupMultiUserOld or EnumPermType.StartupSingleUserOld)
            {
                continue;
            }

            GroupPermissions.DeleteForPermTypeAndUserGroup(permType, userGroupNum);

            if (permType == EnumPermType.AdjustmentTypeDeny)
            {
                GroupPermissions.Insert(new GroupPermission
                {
                    NewerDate = DateTime.MinValue,
                    NewerDays = 0,
                    PermType = permType,
                    UserGroupNum = userGroupNum,
                    FKey = 0
                });
            }
        }

        FillTreePerm();
    }
    
    public void FillForUserGroup(long userGroupNum)
    {
        _listUserGroupNums = [userGroupNum];
        FillTreePerm();
    }

    public void FillForUserGroup(List<long> listUserGroupNums)
    {
        _listUserGroupNums = listUserGroupNums;
        FillTreePerm();
    }
    
    private void CheckFKeyPermissions(GroupPermission perm, SecurityTreeEventHandler securityTreeEventHandler, object sender, SecurityEventArgs securityEventArgs)
    {
        var result = securityTreeEventHandler?.Invoke(sender, securityEventArgs) ?? DialogResult.Cancel;
        if (result == DialogResult.OK)
        {
            SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                "Permission '" + _clickedPermNode.Tag + "' changes made for '" + UserGroups.GetGroup(perm.UserGroupNum).Description + "'");
        }

        FillTreePerm();
    }
    
    private void treePermissions_MouseDown(object sender, MouseEventArgs e)
    {
        if (ReadOnly || _listUserGroupNums.Count != 1)
        {
            return;
        }

        _clickedPermNode = treePermissions.GetNodeAt(e.X, e.Y);
        if (_clickedPermNode == null)
        {
            return;
        }
        
        if (_clickedPermNode.Parent == null)
        {
            if (e.X is < 24 or > 35)
            {
                return;
            }
        }
        else if (_clickedPermNode.Parent.Parent == null)
        {
            if (e.X is < 43 or > 54)
            {
                return;
            }
        }
        else if (_clickedPermNode.Parent.Parent.Parent == null)
        {
            if (e.X is < 62 or > 73)
            {
                return;
            }
        }

        var listLimitedPermissions = new List<EnumPermType>
        {
            EnumPermType.ProcCompleteNote,
            EnumPermType.ProcCompleteAddAdj,
            EnumPermType.ProcCompleteEditMisc
        };
        
        var perm = new GroupPermission
        {
            PermType = (EnumPermType) _clickedPermNode.Tag,
            UserGroupNum = _listUserGroupNums.First()
        };
        const EnumPermType permEcEo = EnumPermType.ProcExistingEdit;
        var securityEventArgs = new SecurityEventArgs(perm);
        
        switch (perm.PermType)
        {
            case EnumPermType.Reports:
                CheckFKeyPermissions(perm, ReportPermissionChecked, sender, securityEventArgs);
                return;
            
            case EnumPermType.AdjustmentTypeDeny:
                CheckFKeyPermissions(perm, AdjustmentTypeDenyPermissionChecked, sender, securityEventArgs);
                return;
        }

        if (_clickedPermNode.ImageIndex == 1)
        {
            if (GroupPermissions.PermTakesDates(perm.PermType))
            {
                perm.IsNew = true;
                
                var result = GroupPermissionChecked?.Invoke(sender, new SecurityEventArgs(perm)) ?? DialogResult.Cancel;
                if (result == DialogResult.Cancel)
                {
                    treePermissions.EndUpdate();
                    return;
                }
            }
            else
            {
                try
                {
                    GroupPermissions.Insert(perm);
                    SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                        "Permission '" + perm.PermType + "' granted to '" + UserGroups.GetGroup(perm.UserGroupNum).Description + "'");
                }
                catch (Exception ex)
                {
                    ODMessageBox.Show(ex.Message);
                    return;
                }
            }

            if (perm.PermType == permEcEo)
            {
                foreach (var permission in listLimitedPermissions)
                {
                    var permLimited = GroupPermissions.GetPerm(_listUserGroupNums.First(), permission);
                    if (permLimited != null)
                    {
                        continue;
                    }

                    GroupPermissions.RefreshCache();
                    
                    perm = GroupPermissions.GetPerm(_listUserGroupNums.First(), perm.PermType);
                    
                    permLimited = new GroupPermission
                    {
                        NewerDate = perm.NewerDate,
                        NewerDays = perm.NewerDays,
                        UserGroupNum = perm.UserGroupNum,
                        PermType = permission
                    };

                    try
                    {
                        GroupPermissions.Insert(permLimited);
                        
                        SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                            "Permission '" + permLimited.PermType + "' granted to '" + UserGroups.GetGroup(perm.UserGroupNum).Description + "'");
                    }
                    catch (Exception ex)
                    {
                        ODMessageBox.Show(ex.Message);
                        
                        return;
                    }
                }
            }
            else if (perm.PermType == EnumPermType.FeeSchedEdit)
            {
                var permFeeSchedLimited = new GroupPermission
                {
                    PermType = EnumPermType.AllowFeeEditWhileReceivingClaim,
                    UserGroupNum = _listUserGroupNums.First()
                };
                
                try
                {
                    GroupPermissions.Insert(permFeeSchedLimited);
                    SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                        "Permission '" + permFeeSchedLimited.PermType + "' granted to '" + UserGroups.GetGroup(permFeeSchedLimited.UserGroupNum).Description + "'");
                }
                catch (Exception ex)
                {
                    ODMessageBox.Show(ex.Message);
                    
                    return;
                }
            }
        }
        else if (_clickedPermNode.ImageIndex == 2)
        {
            try
            {
                if ((EnumPermType) _clickedPermNode.Tag == EnumPermType.AllowFeeEditWhileReceivingClaim && GroupPermissions.HasPermission(_listUserGroupNums.First(), EnumPermType.FeeSchedEdit, 0))
                {
                    MsgBox.Show(this, $"{GroupPermissions.GetDesc(EnumPermType.AllowFeeEditWhileReceivingClaim)} " +
                                      $"cannot be removed from a user who has {GroupPermissions.GetDesc(EnumPermType.FeeSchedEdit)}. Please remove this permission first.");
                    return;
                }

                GroupPermissions.RemovePermission(_listUserGroupNums.First(), (EnumPermType) _clickedPermNode.Tag);
                
                SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                    "Permission '" + _clickedPermNode.Tag + "' revoked from '" + UserGroups.GetGroup(_listUserGroupNums.First()).Description + "'");
            }
            catch (Exception ex)
            {
                ODMessageBox.Show(ex.Message);
                return;
            }

            if (listLimitedPermissions.Contains((EnumPermType) _clickedPermNode.Tag))
            {
                if (GroupPermissions.HasPermission(_listUserGroupNums.First(), permEcEo, 0))
                {
                    try
                    {
                        GroupPermissions.RemovePermission(_listUserGroupNums.First(), permEcEo);
                        
                        SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                            "Permission '" + permEcEo + "' revoked from '" + UserGroups.GetGroup(_listUserGroupNums.First()).Description + "'");
                    }
                    catch (Exception ex)
                    {
                        ODMessageBox.Show(ex.Message);
                        
                        return;
                    }
                }
            }
        }
        else if (_clickedPermNode.ImageIndex == 3)
        {
            try
            {
                GroupPermissions.RemovePermission(_listUserGroupNums.First(), (EnumPermType) _clickedPermNode.Tag);
                
                SecurityLogs.MakeLogEntry(EnumPermType.SecurityAdmin, 0, 
                    "Permission '" + _clickedPermNode.Tag + "' revoked from '" + UserGroups.GetGroup(_listUserGroupNums.First()).Description + "'");
            }
            catch (Exception ex)
            {
                ODMessageBox.Show(ex.Message);
                
                return;
            }
        }

        FillTreePerm();
    }

    private void treePermissions_AfterSelect(object sender, TreeViewEventArgs e)
    {
        treePermissions.SelectedNode = null;
    }

    private void treePermissions_DoubleClick(object sender, EventArgs e)
    {
        if (ReadOnly || _listUserGroupNums.Count != 1)
        {
            return;
        }

        if (_clickedPermNode == null)
        {
            return;
        }

        var permType = (EnumPermType) _clickedPermNode.Tag;
        //If perm doesn't take a date, or the perm isn't AdjustmentTypeDeny, then the double click should not do anything so we just return.
        if (!GroupPermissions.PermTakesDates(permType) && permType != EnumPermType.AdjustmentTypeDeny)
        {
            return;
        }

        var perm = GroupPermissions.GetPerm(_listUserGroupNums.First(), (EnumPermType) _clickedPermNode.Tag);
        if (perm == null)
        {
            return;
        }

        DialogResult result;
        if (perm.PermType == EnumPermType.AdjustmentTypeDeny)
        {
            //Call an event that bubbles back up to the calling Form. The event returns a dialog result so we know how to continue here.
            result = AdjustmentTypeDenyPermissionChecked?.Invoke(sender, new SecurityEventArgs(perm)) ?? DialogResult.Cancel;
        }
        else
        {
            //Call an event that bubbles back up to the calling Form. The event returns a dialog result so we know how to continue here.
            result = GroupPermissionChecked?.Invoke(sender, new SecurityEventArgs(perm)) ?? DialogResult.Cancel;
        }

        if (result == DialogResult.Cancel)
        {
            return;
        }

        FillTreePerm();
    }

    private void treePermissions_MouseMove(object sender, MouseEventArgs e)
    {
        textXpos.Text = e.X.ToString();
    }
}