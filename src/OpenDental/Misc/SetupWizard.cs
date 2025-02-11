#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using Imedisoft.Core.Features.Providers;
using Imedisoft.Core.Features.Providers.Dtos;
using OpenDental.User_Controls.SetupWizard;
using OpenDentBusiness;

namespace OpenDental;

public class SetupWizard
{
    public abstract class SetupWizClass
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract ODSetupCategory Category { get; }
        public abstract ODSetupStatus Status { get; }
        public abstract SetupWizControl SetupControl { get; }
    }

    public class SetupIntro(string name, string descript) : SetupWizClass
    {
        public override ODSetupCategory Category => throw new Exception("This should not get called.");

        public override ODSetupStatus Status => throw new Exception("This should not get called.");

        public override string Name { get; } = name;

        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizIntro(name, descript);

        public override string Description => throw new Exception("This should not get called.");
    }

    public class SetupComplete(string name) : SetupWizClass
    {
        public override ODSetupCategory Category => throw new Exception("This should not get called.");

        public override ODSetupStatus Status => throw new Exception("This should not get called.");

        public override string Name { get; } = name;

        public override string Description => throw new Exception("This should not get called.");

        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizComplete(name);
    }

    public class RegKeySetup : SetupWizClass
    {
        public override ODSetupCategory Category => ODSetupCategory.PreSetup;

        public override string Description
        {
            get
            {
                var description =
                    "Some items need to be set up before the program can be used effectively.\r\n" +
                    "This wizard's purpose is to help you quickly set those items up so that you can get started using the program.";

                if (Status != ODSetupStatus.Complete)
                {
                    description += "\r\n\r\nIt looks like you have yet to enter your Registration Key. ";
                }

                description += "\r\nEntering your Registration Key is a necessary first step in order for the program to function.";

                return description;
            }
        }

        public override ODSetupStatus Status => GetSetupStatus();

        public ODSetupStatus GetSetupStatus(string? regKey = null)
        {
            regKey ??= PrefC.GetString(PrefName.RegistrationKey);

            return string.IsNullOrEmpty(regKey) ? ODSetupStatus.NotStarted : ODSetupStatus.Complete;
        }

        public override string Name => "Registration Key";

        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizRegKey();
    }

    public class FeatureSetup : SetupWizClass
    {
        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override string Description => "Turn features that your office uses on/off. Settings will affect all computers using the same database.";

        public override ODSetupStatus Status => ODSetupStatus.Optional;

        public override string Name => "Basic Features";

        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizFeatures();
    }

    public class ClinicSetup : SetupWizClass
    {
        public override string Name => "Clinics";

        public override string Description =>
            "You have indicated that you will be using the Clinics feature. "
            + "Clinics can be used when you have multiple locations. Once clinics are set up, you can assign clinics throughout Open Dental. "
            + "If you follow basic guidelines, default clinic assignments for patient information should be accurate, thus reducing data entry.";

        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override ODSetupStatus Status => GetSetupStatus();

        public ODSetupStatus GetSetupStatus(List<ClinicDto>? clinicDtos = null)
        {
            clinicDtos ??= Clinics.GetDeepCopy(true);

            if (clinicDtos.Count == 0)
            {
                return ODSetupStatus.NotStarted;
            }

            foreach (var clinicDto in clinicDtos)
            {
                if (string.IsNullOrEmpty(clinicDto.Abbr) ||
                    string.IsNullOrEmpty(clinicDto.Description) ||
                    string.IsNullOrEmpty(clinicDto.PhoneNumber) ||
                    string.IsNullOrEmpty(clinicDto.AddressLine1))
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

        public override string Description =>
            "Definitions are an easy way to customize your software experience. Setup the colors, categories, and other customizable areas " +
            "within the program from this window.\r\n We've selected some of the definitions you may be interested in customizing for this Setup Wizard. " +
            "You may view the entire list of definitions by going to Setup -> Definitions from the main tool bar.";

        public override ODSetupCategory Category => ODSetupCategory.Basic;
        public override ODSetupStatus Status => ODSetupStatus.Optional;
        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizDefinitions();
    }

    public class ProvSetup : SetupWizClass
    {
        public override string Name => "Providers";

        public override string Description =>
            "Providers will show up in almost every part of OpenDental. " +
            "It is important that all provider information is up-to-date so that " +
            "claims, reports, procedures, fee schedules, and estimates will function correctly.";

        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override ODSetupStatus Status => GetSetupStatus();

        public static ODSetupStatus GetSetupStatus(List<ProviderDto>? providers = null)
        {
            providers ??= Providers.GetDeepCopy(true);
            if (providers.Count == 0)
            {
                return ODSetupStatus.NotStarted;
            }

            foreach (var provider in providers)
            {
                var isDentist = IsPrimary(provider);
                var isHyg = provider.IsSecondary;

                if (((isDentist || isHyg) && string.IsNullOrEmpty(provider.Abbr)) ||
                    ((isDentist || isHyg) && string.IsNullOrEmpty(provider.LastName)) ||
                    ((isDentist || isHyg) && string.IsNullOrEmpty(provider.FirstName)) ||
                    (isDentist && string.IsNullOrEmpty(provider.Suffix)) ||
                    (isDentist && string.IsNullOrEmpty(provider.Ssn)) ||
                    (isDentist && string.IsNullOrEmpty(provider.NationalProviderId)))
                {
                    return ODSetupStatus.NeedsAttention;
                }
            }

            return ODSetupStatus.Complete;
        }

        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizProvider();

        public static bool IsPrimary(ProviderDto prov)
        {
            if (prov.IsSecondary)
            {
                return false;
            }

            var description = prov.Specialty.Description.ToLower();
            if (description is "hygienist" or "assistant" or "labtech" or "other" or "notes" or "none")
            {
                return false;
            }

            return !prov.IsNotPerson;
        }
    }

    public class OperatorySetup : SetupWizClass
    {
        public override string Name => "Operatories";

        public override string Description => "Operatories define locations in which appointments take place, and are used to organize appointment columns. Normally, every chair in your office will have an unique operatory. ";

        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override ODSetupStatus Status => GetSetupStatus();

        public static ODSetupStatus GetSetupStatus(List<Operatory>? operatories = null)
        {
            operatories ??= Operatories.GetDeepCopy(true);

            if (operatories.Count == 0)
            {
                return ODSetupStatus.NotStarted;
            }

            foreach (var op in operatories)
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
        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override string Description => "The Employee list is used to set up User profiles in Security and to set up Schedules.� This list also determines who can use the Time Clock.";

        public override ODSetupStatus Status => GetSetupStatus();

        public static ODSetupStatus GetSetupStatus(List<Employee>? employees = null)
        {
            employees ??= Employees.GetDeepCopy(true);

            if (employees.Count == 0)
            {
                return ODSetupStatus.NotStarted;
            }

            foreach (var employee in employees)
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
        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override string Description => "Fee Schedules determine the fees billed for each procedure.";

        public override ODSetupStatus Status => GetSetupStatus();

        public static ODSetupStatus GetSetupStatus(List<FeeSched>? feeScheds = null)
        {
            feeScheds ??= FeeScheds.GetDeepCopy(true);
            
            if (feeScheds.Count == 0)
            {
                return ODSetupStatus.NotStarted;
            }

            var feeSchedNums = feeScheds.Select(x => x.FeeSchedNum).ToList();
            foreach (var schedNum in feeSchedNums)
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
        public override ODSetupCategory Category => ODSetupCategory.Basic;

        public override string Description =>
            "Set up print and scan options for the current workstation. " +
            "You can leave all settings to the default, or you can control where specific items are are printed.";

        public override ODSetupStatus Status => ODSetupStatus.Optional;

        public override string Name => "Printer/Scanner";

        public override SetupWizControl SetupControl { get; } = new UserControlSetupWizPrinter();
    }

    public static Color GetColor(ODSetupStatus stat)
    {
        return stat switch
        {
            ODSetupStatus.NotStarted or ODSetupStatus.NeedsAttention => Color.FromArgb(255, 255, 204, 204),
            ODSetupStatus.Complete or ODSetupStatus.Optional => Color.FromArgb(255, 204, 255, 204),
            _ => Color.White
        };
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
    [Description("Needs Input")]
    NotStarted,

    [Description("Needs Input")]
    NeedsAttention,

    [Description("OK")]
    Complete,

    [Description("Optional")]
    Optional
}