using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDental.User_Controls.SetupWizard;
using OpenDentBusiness;

namespace OpenDental
{
    public class SetupWizard
    {
        public abstract class SetupWizClass
        {
            public abstract string Name { get; }
            public abstract string GetDescript { get; }
            public abstract ODSetupCategory GetCategory { get; }
            public abstract ODSetupStatus GetStatus { get; }
            public abstract SetupWizControl SetupControl { get; }
        }

        #region Intro and Complete

        public class SetupIntro : SetupWizClass
        {
            public SetupIntro(string name, string descript)
            {
                Name = name;
                SetupControl = new UserControlSetupWizIntro(name, descript);
            }

            public override ODSetupCategory GetCategory => throw new Exception("This should not get called.");

            public override ODSetupStatus GetStatus => throw new Exception("This should not get called.");

            public override string Name { get; }

            public override SetupWizControl SetupControl { get; }

            public override string GetDescript => throw new Exception("This should not get called.");
        }


        public class SetupComplete : SetupWizClass
        {
            public SetupComplete(string name)
            {
                Name = name;
                SetupControl = new UserControlSetupWizComplete(name);
            }

            public override ODSetupCategory GetCategory => throw new Exception("This should not get called.");

            public override ODSetupStatus GetStatus => throw new Exception("This should not get called.");

            public override string Name { get; }

            public override string GetDescript => throw new Exception("This should not get called.");

            public override SetupWizControl SetupControl { get; }
        }

        #endregion

        #region PreSetup

        public class RegKeySetup : SetupWizClass
        {
            public override ODSetupCategory GetCategory => ODSetupCategory.PreSetup;

            public override string GetDescript
            {
                get
                {
                    var retVal = "Some items need to be set up before the program can be used effectively. "
                                 + "\r\nThis wizard's purpose is to help you quickly set those items up so that you can get started using the program.";
                    if (GetStatus != ODSetupStatus.Complete)
                    {
                        retVal += "\r\n\r\nIt looks like you have yet to enter your Registration Key. ";
                    }

                    retVal += "\r\nEntering your Registration Key is a necessary first step in order for the program to function.";
                    return retVal;
                }
            }

            public override ODSetupStatus GetStatus => GetSetupStatus();

            public ODSetupStatus GetSetupStatus(string regKey = null)
            {
                if (regKey == null)
                {
                    regKey = PrefC.GetString(PrefName.RegistrationKey);
                }

                if (string.IsNullOrEmpty(regKey))
                {
                    return ODSetupStatus.NotStarted;
                }

                return ODSetupStatus.Complete;
            }

            public override string Name => "Registration Key";

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizRegKey();
        }

        #endregion

        public class FeatureSetup : SetupWizClass
        {
            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override string GetDescript => "Turn features that your office uses on/off. Settings will affect all computers using the same database.";

            public override ODSetupStatus GetStatus => ODSetupStatus.Optional;

            public override string Name => "Basic Features";

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizFeatures();
        }

        public class ClinicSetup : SetupWizClass
        {
            public override string Name => "Clinics";

            public override string GetDescript =>
                "You have indicated that you will be using the Clinics feature. "
                + "Clinics can be used when you have multiple locations. Once clinics are set up, you can assign clinics throughout Open Dental. "
                + "If you follow basic guidelines, default clinic assignments for patient information should be accurate, thus reducing data entry.";

            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override ODSetupStatus GetStatus => GetSetupStatus();

            public ODSetupStatus GetSetupStatus(List<ClinicDto> listClinics = null)
            {
                if (listClinics == null)
                {
                    listClinics = Clinics.GetDeepCopy(true);
                }

                if (listClinics.Count == 0)
                {
                    return ODSetupStatus.NotStarted;
                }

                foreach (var clin in listClinics)
                {
                    if (string.IsNullOrEmpty(clin.Abbr) || string.IsNullOrEmpty(clin.Description) || string.IsNullOrEmpty(clin.PhoneNumber) || string.IsNullOrEmpty(clin.AddressLine1))
                    {
                        return ODSetupStatus.NeedsAttention;
                    }
                }

                return ODSetupStatus.Complete;
            }

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizClinic();
        }

        public class DefinitionSetup : SetupWizClass
        {
            public override string Name => "Definitions";

            public override string GetDescript =>
                "Definitions are an easy way to customize your software experience. Setup the colors, categories, and other customizable areas " +
                "within the program from this window.\r\n We've selected some of the definitions you may be interested in customizing for this Setup Wizard. " +
                "You may view the entire list of definitions by going to Setup -> Definitions from the main tool bar.";

            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;
            public override ODSetupStatus GetStatus => ODSetupStatus.Optional;
            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizDefinitions();
        }

        public class ProvSetup : SetupWizClass
        {
            public override string Name => "Providers";

            public override string GetDescript =>
                "Providers will show up in almost every part of OpenDental. " +
                "It is important that all provider information is up-to-date so that " +
                "claims, reports, procedures, fee schedules, and estimates will function correctly.";

            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override ODSetupStatus GetStatus => GetSetupStatus();

            public static ODSetupStatus GetSetupStatus(List<Provider> listProviders = null)
            {
                listProviders ??= Providers.GetDeepCopy(true);
                if (listProviders.Count == 0)
                {
                    return ODSetupStatus.NotStarted;
                }

                foreach (var prov in listProviders)
                {
                    var isDentist = IsPrimary(prov);
                    var isHyg = prov.IsSecondary;
                    if (((isDentist || isHyg) && string.IsNullOrEmpty(prov.Abbr))
                        || ((isDentist || isHyg) && string.IsNullOrEmpty(prov.LName))
                        || ((isDentist || isHyg) && string.IsNullOrEmpty(prov.FName))
                        || ((isDentist) && string.IsNullOrEmpty(prov.Suffix))
                        || ((isDentist) && string.IsNullOrEmpty(prov.SSN))
                        || ((isDentist) && string.IsNullOrEmpty(prov.NationalProvID))
                       )
                    {
                        return ODSetupStatus.NeedsAttention;
                    }
                }

                return ODSetupStatus.Complete;
            }

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizProvider();

            public static bool IsPrimary(Provider prov)
            {
                if (prov.IsSecondary)
                {
                    return false;
                }

                if (Defs.GetName(DefCat.ProviderSpecialties, prov.Specialty).ToLower() == "hygienist" ||
                    Defs.GetName(DefCat.ProviderSpecialties, prov.Specialty).ToLower() == "assistant" ||
                    Defs.GetName(DefCat.ProviderSpecialties, prov.Specialty).ToLower() == "labtech" ||
                    Defs.GetName(DefCat.ProviderSpecialties, prov.Specialty).ToLower() == "other" ||
                    Defs.GetName(DefCat.ProviderSpecialties, prov.Specialty).ToLower() == "notes" ||
                    Defs.GetName(DefCat.ProviderSpecialties, prov.Specialty).ToLower() == "none")
                {
                    return false;
                }

                return !prov.IsNotPerson;
            }
        }

        public class OperatorySetup : SetupWizClass
        {
            public override string Name => "Operatories";

            public override string GetDescript => "Operatories define locations in which appointments take place, and are used to organize appointment columns. Normally, every chair in your office will have an unique operatory. ";

            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override ODSetupStatus GetStatus => GetSetupStatus();

            public static ODSetupStatus GetSetupStatus(List<Operatory> listOperatories = null)
            {
                listOperatories ??= Operatories.GetDeepCopy(true);

                if (listOperatories.Count == 0)
                {
                    return ODSetupStatus.NotStarted;
                }

                foreach (var op in listOperatories)
                {
                    if (string.IsNullOrEmpty(op.OpName) || string.IsNullOrEmpty(op.Abbrev))
                    {
                        return ODSetupStatus.NeedsAttention;
                    }
                }

                return ODSetupStatus.Complete;
            }

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizOperatory();
        }

        public class EmployeeSetup : SetupWizClass
        {
            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override string GetDescript => "The Employee list is used to set up User profiles in Security and to set up Schedules.� This list also determines who can use the Time Clock.";

            public override ODSetupStatus GetStatus => GetSetupStatus();

            public static ODSetupStatus GetSetupStatus(List<Employee> listEmployees = null)
            {
                listEmployees ??= Employees.GetDeepCopy(true);

                if (listEmployees.Count == 0)
                {
                    return ODSetupStatus.NotStarted;
                }

                foreach (var employee in listEmployees)
                {
                    if (string.IsNullOrEmpty(employee.FName) || string.IsNullOrEmpty(employee.LName))
                    {
                        return ODSetupStatus.NeedsAttention;
                    }
                }

                return ODSetupStatus.Complete;
            }

            public override string Name => "Employees";

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizEmployee();
        }

        public class FeeSchedSetup : SetupWizClass
        {
            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override string GetDescript => "Fee Schedules determine the fees billed for each procedure.";
            
            public override ODSetupStatus GetStatus => GetSetupStatus();

            public static ODSetupStatus GetSetupStatus(List<FeeSched> listFeeScheds = null)
            {
                listFeeScheds ??= FeeScheds.GetDeepCopy(true);

                if (listFeeScheds.Count == 0)
                {
                    return ODSetupStatus.NotStarted;
                }

                var listFeeSchedNums = listFeeScheds.Select(x => x.FeeSchedNum).ToList();
                foreach (var schedNum in listFeeSchedNums)
                {
                    if (Fees.GetCountByFeeSchedNum(schedNum) <= 0)
                    {
                        return ODSetupStatus.NeedsAttention;
                    }
                }

                return ODSetupStatus.Complete;
            }

            public override string Name => "Fee Schedules";

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizFeeSched();
        }

        public class PrinterSetup : SetupWizClass
        {
            public override ODSetupCategory GetCategory => ODSetupCategory.Basic;

            public override string GetDescript =>
                "Set up print and scan options for the current workstation. " + 
                "You can leave all settings to the default, or you can control where specific items are are printed.";

            public override ODSetupStatus GetStatus => ODSetupStatus.Optional;

            public override string Name => "Printer/Scanner";

            public override SetupWizControl SetupControl { get; } = new UserControlSetupWizPrinter();
        }

        public static Color GetColor(ODSetupStatus stat)
        {
            switch (stat)
            {
                case ODSetupStatus.NotStarted:
                case ODSetupStatus.NeedsAttention:
                    return Color.FromArgb(255, 255, 204, 204);

                case ODSetupStatus.Complete:
                case ODSetupStatus.Optional:
                    return Color.FromArgb(255, 204, 255, 204);

                default:
                    return Color.White;
            }
        }
    }

    public enum ODSetupCategory
    {
        [Description("Misc Setup")]
        Misc,

        None,

        [Description("Pre-Setup")]
        PreSetup,

        [Description("Basic Setup")]
        Basic,

        [Description("Advanced Setup")]
        Advanced
    }

    public enum ODSetupStatus
    {
        /// <summary>User hasn't started this setup item.</summary>
        [Description("Needs Input")]
        NotStarted,

        /// <summary>User has left this setup item in an incomplete state.</summary>
        [Description("Needs Input")]
        NeedsAttention,

        /// <summary>Setup item has been considered and required elements have been filled in.</summary>
        [Description("OK")]
        Complete,

        /// <summary>Setup item is not required.</summary>
        [Description("Optional")]
        Optional
    }
}