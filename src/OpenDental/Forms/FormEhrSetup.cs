using System;
using System.Drawing;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental
{
    public partial class FormEhrSetup : FormODBase
    {
        public FormEhrSetup()
        {
            InitializeComponent();
            InitializeLayoutManager();
            Lan.F(this);
        }

        private void FormEhrSetup_Load(object sender, EventArgs e)
        {
            LayoutMenu();
            if (PrefC.GetBool(PrefName.EhrEmergencyNow))
            {
                panelEmergencyNow.BackColor = Color.Red;
            }
            else
            {
                panelEmergencyNow.BackColor = SystemColors.Control;
            }

            if (!Security.IsAuthorized(EnumPermType.Setup, true))
            {
                //Hide all the buttons except Emergency Now and Close.
                //Unhiding all code system buttons since code systems can no longer be edited.
                butAllergies.Visible = false;
                //Forumularies will now be checked through New Crop
                //butFormularies.Visible=false;
                butVaccineDef.Visible = false;
                butDrugManufacturer.Visible = false;
                butDrugUnit.Visible = false;
                butInboundEmail.Visible = false;
                butReminderRules.Visible = false;
                butEducationalResources.Visible = false;
                menuMain.Enabled = false;
                butTimeSynch.Visible = false;
                butEhrTriggers.Visible = false;
            }
        }

        private void LayoutMenu()
        {
            menuMain.BeginUpdate();
            menuMain.Add(new MenuItemOD("Settings", menuItemSettings_Click));
            menuMain.EndUpdate();
        }

        private void menuItemSettings_Click(object sender, EventArgs e)
        {
            using FormEhrSettings FormES = new FormEhrSettings();
            FormES.ShowDialog();
        }

        private void butICD9s_Click(object sender, EventArgs e)
        {
            using FormIcd9s FormE = new FormIcd9s();
            FormE.ShowDialog();
        }

        private void butAllergies_Click(object sender, EventArgs e)
        {
            using FormAllergySetup FAS = new FormAllergySetup();
            FAS.ShowDialog();
        }
        
        private void butVaccineDef_Click(object sender, EventArgs e)
        {
            using FormVaccineDefSetup FormE = new FormVaccineDefSetup();
            FormE.ShowDialog();
        }

        private void butDrugManufacturer_Click(object sender, EventArgs e)
        {
            using FormDrugManufacturerSetup FormE = new FormDrugManufacturerSetup();
            FormE.ShowDialog();
        }

        private void butDrugUnit_Click(object sender, EventArgs e)
        {
            using FormDrugUnitSetup FormE = new FormDrugUnitSetup();
            FormE.ShowDialog();
        }

        private void butInboundEmail_Click(object sender, EventArgs e)
        {
            using FormEmailAddresses formEA = new FormEmailAddresses();
            formEA.ShowDialog();
        }

        private void butEmergencyNow_Click(object sender, EventArgs e)
        {
            if (PrefC.GetBool(PrefName.EhrEmergencyNow))
            {
                panelEmergencyNow.BackColor = SystemColors.Control;
                Prefs.UpdateBool(PrefName.EhrEmergencyNow, false);
            }
            else
            {
                panelEmergencyNow.BackColor = Color.Red;
                Prefs.UpdateBool(PrefName.EhrEmergencyNow, true);
            }

            DataValid.SetInvalid(InvalidType.Prefs);
        }

        private void butReminderRules_Click(object sender, EventArgs e)
        {
            using FormReminderRules FormRR = new FormReminderRules();
            FormRR.ShowDialog();
        }

        private void butEducationalResources_Click(object sender, EventArgs e)
        {
            using FormEduResourceSetup FormEDUSetup = new FormEduResourceSetup();
            FormEDUSetup.ShowDialog();
        }

        private void butRxNorm_Click(object sender, EventArgs e)
        {
            using FormRxNorms FormR = new FormRxNorms();
            FormR.ShowDialog();
        }

        private void butLoincs_Click(object sender, EventArgs e)
        {
            using FormLoincs FormL = new FormLoincs();
            FormL.ShowDialog();
        }

        private void butSnomeds_Click(object sender, EventArgs e)
        {
            using FormSnomeds FormS = new FormSnomeds();
            FormS.ShowDialog();
        }

        private void butTimeSynch_Click(object sender, EventArgs e)
        {
            using FormEhrTimeSynch formET = new FormEhrTimeSynch();
            formET.ShowDialog();
        }

        private void butPortalSetup_Click(object sender, EventArgs e)
        {
            using FormEServicesPatientPortal formESPatPortal = new FormEServicesPatientPortal();
            formESPatPortal.ShowDialog();
        }

        private void butCodeImport_Click(object sender, EventArgs e)
        {
            using FormCodeSystemsImport FormCSI = new FormCodeSystemsImport();
            FormCSI.ShowDialog();
        }

        private void butProviderKeys_Click(object sender, EventArgs e)
        {
            using FormEhrProviderKeys formK = new FormEhrProviderKeys();
            formK.ShowDialog();
        }

        private void butCdsTriggers_Click(object sender, EventArgs e)
        {
            using FormCdsTriggers FormET = new FormCdsTriggers();
            FormET.ShowDialog();
        }

        private void butOIDs_Click(object sender, EventArgs e)
        {
            using FormOIDRegistryInternal FormOIDI = new FormOIDRegistryInternal();
            FormOIDI.ShowDialog();
        }
    }
}