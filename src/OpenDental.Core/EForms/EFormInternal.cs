using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Imedisoft.Core.Entities;
using OpenDentBusiness.Properties;

namespace OpenDentBusiness;

public class EFormInternal
{
    public static List<EFormDef> GetAllInternal()
    {
        return
        [
            GetEFormDef(EnumEFormInternalType.PatientRegistration),
            GetEFormDef(EnumEFormInternalType.MedicalHist),
            GetEFormDef(EnumEFormInternalType.Consent),
            GetEFormDef(EnumEFormInternalType.DentalHist),
            GetEFormDef(EnumEFormInternalType.HIPPA)
        ];
    }

    public static EFormDef GetEFormDef(EnumEFormInternalType internalType)
    {
        return internalType switch
        {
            EnumEFormInternalType.Demo => GetEFormFromResource(Resources.EFormDemo),
            EnumEFormInternalType.PatientRegistration => GetEFormFromResource(Resources.EFormPatientRegistration),
            EnumEFormInternalType.MedicalHist => GetEFormFromResource(Resources.EFormMedicalHistory),
            EnumEFormInternalType.Consent => GetEFormFromResource(Resources.EFormExtractionConsent),
            EnumEFormInternalType.DentalHist => GetEFormFromResource(Resources.EFormDentalHistory),
            EnumEFormInternalType.HIPPA => GetEFormFromResource(Resources.EFormHIPAA),
            _ => throw new ApplicationException("Invalid EnumEFormInternalType:" + internalType)
        };
    }

    private static EFormDef GetEFormFromResource(string xmlDoc)
    {
        var xmlSerializer = new XmlSerializer(typeof(EFormDef));

        using var textReader = new StringReader(xmlDoc);

        var eFormDef = (EFormDef) xmlSerializer.Deserialize(textReader);
        foreach (var fieldDef in eFormDef.ListEFormFieldDefs)
        {
            fieldDef.EFormDefNum = 0;
            fieldDef.EFormFieldDefNum = 0;

            const string pattern = @"(?<!\r)" + "\n";

            fieldDef.ValueLabel = Regex.Replace(fieldDef.ValueLabel, pattern, "\r\n");
        }

        eFormDef.EFormDefNum = 0;

        return eFormDef;
    }
}