using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeBase;
using System.Linq;

namespace OpenDentBusiness.Eclaims;

public class CCDField
{
    private abstract class LengthRequirement
    {
        public abstract int CalcLength(CCDFieldInputter formData);
    }

    private class ConstLengthRequirement(int length) : LengthRequirement
    {
        public override int CalcLength(CCDFieldInputter formData)
        {
            return length;
        }
    }

    private class LengthFromAnotherField(string pOtherFieldId) : LengthRequirement
    {
        public override int CalcLength(CCDFieldInputter formData)
        {
            var lengthField = formData.GetFieldsById(pOtherFieldId).LastOrDefault();
            if (lengthField == null)
            {
                return -1;
            }

            if (!Regex.IsMatch(lengthField.Valuestr, "^[0-9]+$"))
            {
                throw new ODException(ToString() + ".CalcLength: Internal Error, cannot load field length from non-integer field value!");
            }

            return Convert.ToInt32(lengthField.Valuestr);
        }
    }

    private class ConstLengthWhenOtherFieldHasValue(string pOtherFieldId, string pOtherFieldValue, int pValueWhenExists) : LengthRequirement
    {
        public override int CalcLength(CCDFieldInputter formData)
        {
            var lengthField = formData.GetFieldById(pOtherFieldId);
            if (lengthField == null)
            {
                return -1;
            }

            return lengthField.Valuestr == pOtherFieldValue ? pValueWhenExists : 0;
        }
    }

    private abstract class ValueRequirement;

    private class DiscreteValueRequirement : ValueRequirement;

    private class RangeValueRequirement : ValueRequirement;

    private class RegexValueRequirement : ValueRequirement;

    public class ValueMap;

    private LengthRequirement _lengthRequirement;
    private readonly List<ValueRequirement> _valueRequirements = [];

    public readonly string FieldId;
    public readonly string MsgType;

    public string FieldName;
    public string FrenchFieldName;
    public string Format;
    public string Valuestr;

    public string GetFieldName(bool useFrench)
    {
        if (useFrench && FrenchFieldName != null)
        {
            return FrenchFieldName;
        }

        return FieldName;
    }

    public static bool IsValidId_v2(string str)
    {
        if (str.Length != 3 || str[0] < 'A' || str[0] > 'G' || str[1] < '0' || str[1] > '9' || str[2] < '0' || str[2] > '9')
        {
            return false;
        }

        var num = Convert.ToInt32(str.Substring(1, 2));
        if (num < 1)
        {
            return false;
        }

        return str[0] switch
        {
            'A' => num <= 8,
            'B' => num <= 2,
            'C' => num <= 11,
            'D' => num <= 10,
            'E' => num <= 6,
            'F' => num <= 15,
            'G' => num <= 30,
            _ => false
        };
    }

    public static bool IsValidId_v4(string str)
    {
        if (str.Length != 3 || str[0] < 'A' || str[0] > 'G' || str[1] < '0' || str[1] > '9' || str[2] < '0' || str[2] > '9')
        {
            return false;
        }

        var num = Convert.ToInt32(str.Substring(1, 2));
        if (num < 1)
        {
            return false;
        }

        return str[0] switch
        {
            'A' => num <= 11,
            'B' => num <= 8,
            'C' => num <= 19,
            'D' => num <= 11,
            'E' => num <= 20,
            'F' => num <= 49,
            'G' => num <= 62,
            _ => false
        };
    }

    public int GetRequiredLength(CCDFieldInputter formData)
    {
        return _lengthRequirement.CalcLength(formData);
    }

    public CCDField(string pFieldId, string msgType, bool isVersion02)
    {
        MsgType = msgType;
        pFieldId = pFieldId.ToUpper();
        if (isVersion02)
        {
            if (!IsValidId_v2(pFieldId))
            {
                if (IsValidId_v4(pFieldId))
                {
                    SetValuesUsingFieldId_v4(pFieldId);
                }
                else
                {
                    throw new ODException("Cannot construct version 2 field with invalid field id: " + pFieldId);
                }
            }
            else
            {
                SetValuesUsingFieldId_v2(pFieldId);
            }
        }
        else
        {
            if (!IsValidId_v4(pFieldId))
            {
                throw new ODException("Cannot construct version 4 field with invalid field id: " + pFieldId);
            }

            SetValuesUsingFieldId_v4(pFieldId);
        }

        FieldId = pFieldId;
        Format = Format.ToUpper();
        Valuestr = null;

        if (_valueRequirements.Count != 0)
        {
            return;
        }

        switch (Format)
        {
            case "N":
            case "A":
                _valueRequirements.Add(new RegexValueRequirement());
                break;

            case "AE":
            case "A/N":
            case "AE/N":
                break;

            case "D":
                _valueRequirements.Add(new RegexValueRequirement());
                break;

            default:
                throw new ODException(ToString() + ".CCDField: Internal error, unrecognized field format: " + Format);
        }
    }

    private void SetValuesUsingFieldId_v2(string pFieldId)
    {
        switch (pFieldId)
        {
            case "A01":
                FieldName = "Transaction Prefix";
                FrenchFieldName = "Pr�fixe de transaction";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(12);
                break;
            case "A02":
                //Provider's Sequence Number
                FieldName = "DENTAL OFFICE CLAIM REFERENCE NO";
                FrenchFieldName = "NO DE TRANSACTION DU CABINET";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "A03":
                FieldName = "Format Version Number";
                FrenchFieldName = "Nombre de version de format";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "A04":
                FieldName = "Transaction Code";
                FrenchFieldName = "Code de transaction";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "A05":
                FieldName = "Carrier Identification Number";
                FrenchFieldName = "Num�ro d'identification de porteur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "A06":
                FieldName = "Software System ID";
                FrenchFieldName = "Syst�me logiciel identification";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "A07":
                FieldName = "Message Length";
                FrenchFieldName = "Longueur de message";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "A08":
                FieldName = "E-Mail Flag";
                FrenchFieldName = "Drapeau E-Mail";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "B01":
                //CDA Provider Number
                FieldName = "UNIQUE ID NO";
                FrenchFieldName = "NO DU DENTISTE";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(9);
                break;
            case "B02":
                //Provider Office Number
                FieldName = "OFFICE NO";
                FrenchFieldName = "NO DU CABINET";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "C01":
                //Primary Policy/Plan Number
                FieldName = "POLICY#";
                FrenchFieldName = "NO DE POLICE";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "C02":
                //Subscriber Identification Number
                FieldName = "CERTIFICATE NO";
                FrenchFieldName = "NO DE CERTIFICAT";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(11);
                break;
            case "C03":
                FieldName = "Relationship Code";
                FrenchFieldName = "Code de rapport";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "C04":
                FieldName = "Patient's Sex";
                FrenchFieldName = "Le sexe du patient";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "C05":
                FieldName = "Patient's Birthday";
                FrenchFieldName = "L'anniversaire du patient";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "C06":
                FieldName = "Patient's Last Name";
                FrenchFieldName = "Le dernier nom du patient";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "C07":
                FieldName = "Patient's First Name";
                FrenchFieldName = "Le pr�nom du patient";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(15);
                break;
            case "C08":
                FieldName = "Patient's Middle Initial";
                FrenchFieldName = "L'initiale moyenne du patient";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "C09":
                FieldName = "Eligibility Exception Code";
                FrenchFieldName = "Code d'exception d'acceptabilit�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "C10":
                FieldName = "Name of School";
                FrenchFieldName = "Nom d'�cole";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "C11":
                FieldName = "DIVISION/SECTION NO";
                FrenchFieldName = "NO DE DIVISION/SECTION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "D01":
                FieldName = "Subscriber's Birthday";
                FrenchFieldName = "L'anniversaire de l'abonn�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "D02":
                FieldName = "Subscriber's Last Name";
                FrenchFieldName = "Le dernier nom de l'abonn�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "D03":
                FieldName = "Subscriber's First Name";
                FrenchFieldName = "Le pr�nom de l'abonn�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(15);
                break;
            case "D04":
                FieldName = "Subscriber's Middle Initial";
                FrenchFieldName = "L'initiale moyenne de l'abonn�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "D05":
                FieldName = "Subscriber's Address Line 1";
                FrenchFieldName = "Ligne 1 de l'adresse de l'abonn�";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "D06":
                FieldName = "Subscriber's Address Line 2";
                FrenchFieldName = "Ligne 2 de l'adresse de l'abonn�";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "D07":
                FieldName = "Subscriber's City";
                FrenchFieldName = "La ville de l'abonn�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(20);
                break;
            case "D08":
                FieldName = "Subscriber's Province/State Code";
                FrenchFieldName = "Code de la province/�tat de l'abonn�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(2);
                //Includes US states and Canadian provinces.
                //http://www.nrcan.gc.ca/earth-sciences/geography-boundary/geographical-name/translators/5782
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "D09":
                FieldName = "Subscriber's Postal/ZIP Code";
                FrenchFieldName = "Code du Postal/ZIP de l'abonn�";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "D10":
                FieldName = "Language of the Insured";
                FrenchFieldName = "Langue des assur�s";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "E01":
                FieldName = "Secondary Carrier Unique ID Number";
                FrenchFieldName = "Nombre unique d'identification de porteur secondaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "E02":
                FieldName = "Secondary Policy/Plan";
                FrenchFieldName = "Politique/plan secondaires";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "E03":
                FieldName = "Secondary Plan Subscriber ID";
                FrenchFieldName = "Identification secondaire d'abonn� de plan";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(11);
                break;
            case "E04":
                //Spouse/Significant Other Birtday
                FieldName = "Secondary Subscriber's Birthday";
                FrenchFieldName = "L'anniversaire de l'abonn� secondaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "E05":
                FieldName = "Secondary Division/Section Number";
                FrenchFieldName = "Nombre secondaire de Division/section";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "E06":
                FieldName = "Secondary Relationship Code";
                FrenchFieldName = "Code secondaire de rapport";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F01":
                FieldName = "Payee Code";
                FrenchFieldName = "Code de b�n�ficiaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F02":
                FieldName = "Accident Date";
                FrenchFieldName = "Date d'accidents";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F03":
                FieldName = "Predetermination Number";
                FrenchFieldName = "Nombre de pr�d�termination";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(14);
                break;
            case "F04":
                FieldName = "Date of Initial Placement Upper";
                FrenchFieldName = "Date de haut initial de placement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F05":
                FieldName = "Treatment Required for orthodontic";
                FrenchFieldName = "Traitement requis pour orthodontique";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F06":
                FieldName = "Number of Procedures Performed";
                FrenchFieldName = "Nombre de proc�dures ex�cut�es";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F07":
                FieldName = "Procedure Line Number";
                FrenchFieldName = "Ligne nombre de proc�d�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F08":
                FieldName = "Procedure Code";
                FrenchFieldName = "Code de proc�d�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "F09":
                FieldName = "Date of Service";
                FrenchFieldName = "Date de service";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F10":
                FieldName = "International Tooth,Sextant, Quad or Arch";
                FrenchFieldName = "Dent, sextant, quadruple ou vo�te international";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "F11":
                FieldName = "Tooth Surface";
                FrenchFieldName = "Surface de dent";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(5);
                _valueRequirements.Add(new RegexValueRequirement());
                break;
            case "F12":
                FieldName = "Dentist's Fee Claimed";
                FrenchFieldName = "Les honoraires du dentiste r�clam�s";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F13":
                FieldName = "Lab Procedure Fee # 1";
                FrenchFieldName = "Honoraires # 1 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F14":
                FieldName = "Unit of Time";
                FrenchFieldName = "Unit� de temps";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "F15":
                FieldName = "Is this an Initial Placement Upper";
                FrenchFieldName = "Est c'un premier haut de placement";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G01":
                //Transaction Reference Number
                FieldName = "CARRIER CLAIM NO";
                FrenchFieldName = "NO DE R�F�RENCE DE TRANSACTION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(14);
                break;
            case "G02":
                FieldName = "Employer Certified Flag";
                FrenchFieldName = "";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G03":
                FieldName = "Expected Payment Date";
                FrenchFieldName = "Date pr�vue de paiement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "G04":
                FieldName = "Total Amount of Service";
                FrenchFieldName = "Montant total de service";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G05":
                FieldName = "Response Status";
                FrenchFieldName = "Statut de r�ponse";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G06":
                FieldName = "Number of Error Codes";
                FrenchFieldName = "Nombre de codes d'erreur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G07":
                //Disposition message
                FieldName = "DISPOSITION";
                FrenchFieldName = "SP�CIFICATIONS";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(75);
                break;
            case "G08":
                FieldName = "Error Code";
                FrenchFieldName = "Code d'erreur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "G09":
                FieldName = "E-Mail Flag";
                FrenchFieldName = "Drapeau E-Mail";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G10":
                FieldName = "Number of Carrier Issued Procedure Codes";
                FrenchFieldName = "Le nombre de porteur a publi� des codes de proc�d�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G11":
                FieldName = "Number of Note Lines";
                FrenchFieldName = "Nombre de lignes de note";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G12":
                FieldName = "Eligible Amount";
                FrenchFieldName = "Quantit� �ligible";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G13":
                FieldName = "Deductible Amount";
                FrenchFieldName = "Quantit� d�ductible";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G14":
                FieldName = "Eligible Percentage";
                FrenchFieldName = "Pourcentage �ligible";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G15":
                FieldName = "Benefit Amount for the Procedure";
                FrenchFieldName = "Quantit� d'avantage pour le proc�d�";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G16":
                FieldName = "Explanation Note Number 1";
                FrenchFieldName = "Note num�ro 1 d'explication";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G17":
                FieldName = "Explanation Note Number 2";
                FrenchFieldName = "Note num�ro 2 d'explication";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G18":
                FieldName = "Reference to Line Number of the Submitted Procedure";
                FrenchFieldName = "R�f�rence � la ligne nombre du proc�d� soumis";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(7);
                _valueRequirements.Add(new RegexValueRequirement());
                break;
            case "G19":
                FieldName = "Additional Procedure Code";
                FrenchFieldName = "Code additionnel de proc�d�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G20":
                FieldName = "Eligible Amount for the Additional Procedure";
                FrenchFieldName = "Quantit� �ligible pour le proc�d� additionnel";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G21":
                FieldName = "Dedutible for the Additional Procedure";
                FrenchFieldName = "";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G22":
                FieldName = "Eligible Percentage";
                FrenchFieldName = "Pourcentage �ligible";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G23":
                FieldName = "Benefit Amount for the Additional Procedure";
                FrenchFieldName = "Quantit� d'avantage pour le proc�d� additionnel";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G24":
                FieldName = "Explanation Note Number 1 for the Additional Procedure";
                FrenchFieldName = "Note d'explication num�ro 1 pour le proc�d� additionnel";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G25":
                FieldName = "Explanation Note Number 2 for the Additional Procedure";
                FrenchFieldName = "Note d'explication num�ro 2 pour le proc�d� additionnel";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G26":
                FieldName = "Note Text";
                FrenchFieldName = "Noter le texte";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(75);
                break;
            case "G27":
                FieldName = "Language of the Insured";
                FrenchFieldName = "Langue des assur�s";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G28":
                FieldName = "Total Benefit Amount";
                FrenchFieldName = "Quantit� totale d'avantage";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G29":
                FieldName = "Deductible amount unallocated";
                FrenchFieldName = "La quantit� d�ductible a d�sassign�";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G30":
                //Transaction Validation Code
                FieldName = "VERIFICATION NO";
                FrenchFieldName = "CODE DE VALIDATION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            default:
                throw new ODException("Internal Error, unknown version 2 CCD field ID during construction: " + pFieldId);
        }
    }

    private void SetValuesUsingFieldId_v4(string pFieldId)
    {
        //Includes US states and Canadian provinces.
        //http://www.nrcan.gc.ca/earth-sciences/geography-boundary/geographical-name/translators/5782
        var stateCodes = new string[]
        {
            //Canadian province codes.
            "AB", //Alberta
            "BC", //Britich Columbia
            "MB", //Manitoba
            "NB", //New Brunswick
            "NL", //Newfoundland and Labrador
            "NS", //Nova Scotia
            "NT", //Northwest Territories
            "NU", //Nunavut
            "ON", //Ontario
            "PE", //Prince Edward Island
            "QC", //Quebec
            "SK", //Saskatchewan
            "YT", //Yukon
            //Traditional Canadian province codes which somehow made it into our application, but we are going to leave them because they are probably harmless.
            "LB", //Newfoundland and Labrador - This appeared in Canada Post publications (e.g., The Canadian Postal Code Directory) for the mainland section of the province of Newfoundland and Labrador.
            "NF", //Newfoundland and Labrador - Nfld. and later NF (the two-letter abbreviation used before the province's name changed to Newfoundland and Labrador) and T.-N. (French version, for Terre-Neuve)
            "PQ", //Quebec	- Que. and P.Q. (French version, for Province du Qu�bec); later, PQ evolved from P.Q. as the first two-letter non-punctuated abbreviation.
            //US state codes.
            "AK", "AL", "AR", "AZ", "CA", "CO", "CT", "DC", "DE", "FL",
            "GA", "HI", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA",
            "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE",
            "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI",
            "SC", "SD", "TX", "UT", "VA", "VT", "WA", "WI", "WV", "WY"
        };
        var languageCodes = new string[] {"A", "E", "F"};
        ValueMap valueMap;
        //Values in the following table were taken from the data dictionary of the CCD doc (about page 70).
        switch (pFieldId)
        {
            case "A01":
                FieldName = "Transaction Prefix";
                FrenchFieldName = "Pr�fixe de transaction";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(12);
                break;
            case "A02":
                //DENTAL OFFICE CLAIM REFERENCE NO
                FieldName = "DENTAL OFFICE CLAIM REFERENCE NO";
                FrenchFieldName = "NO DE TRANSACTION DU CABINET";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "A03":
                FieldName = "Format Version Number";
                FrenchFieldName = "Nombre de version de format";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "A04":
                FieldName = "Transaction Code";
                FrenchFieldName = "Code de transaction";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "A05":
                FieldName = "Carrier Identification Number";
                FrenchFieldName = "Num�ro d'identification de porteur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "A06":
                FieldName = "Software System ID";
                FrenchFieldName = "Syst�me logiciel identification";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "A07":
                FieldName = "Message Length";
                FrenchFieldName = "Longueur de message";
                Format = "N";
                if (MsgType == "09")
                {
                    //Attachment Transaction Format
                    _lengthRequirement = new ConstLengthRequirement(7);
                }
                else
                {
                    _lengthRequirement = new ConstLengthRequirement(5);
                }

                break;
            case "A08":
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(1);
                FieldName = "Materials Forwarded";
                FrenchFieldName = "Les mat�riaux ont exp�di�";
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "A09": //Not in version 2.
                FieldName = "Carrier Transaction Counter";
                FrenchFieldName = "Compteur de transaction de porteur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "A10": //Not in version 2.
                FieldName = "Encryption Method";
                FrenchFieldName = "M�thode de chiffrage";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "A11": //Not in version 2.
                FieldName = "Mailbox Indicator";
                FrenchFieldName = "Indicateur de bo�te aux lettres";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "B01":
                //CDA Provider Number
                FieldName = "UNIQUE ID NO";
                FrenchFieldName = "NO DU DENTISTE";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(9);
                break;
            case "B02":
                //Provider Office Number
                FieldName = "OFFICE NO";
                FrenchFieldName = "NO DU CABINET";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "B03": //Not in version 2.
                FieldName = "BILLING PROVIDER NUMBER";
                FrenchFieldName = "NOMBRE DE FOURNISSEUR DE FACTURATION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(9);
                break;
            case "B04": //Not in version 2.
                FieldName = "BILLING OFFICE NUMBER";
                FrenchFieldName = "NOMBRE D'OFFICE DE FACTURATION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "B05": //Not in version 2.
                FieldName = "Referring Provider Number";
                FrenchFieldName = "R�f�rence du nombre de fournisseur";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "B06": //Not in version 2.
                FieldName = "Referral Reason Code";
                FrenchFieldName = "Code compl�mentaire de r�f�rence";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "B07": //Not in version 2.
                FieldName = "Receiving Provider Number";
                FrenchFieldName = "R�ception du nombre de fournisseur";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(9);
                break;
            case "B08": //Not in version 2.
                FieldName = "Receiving Office Number";
                FrenchFieldName = "R�ception du nombre d'Office";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "C01":
                //Primary Policy/Plan Number
                FieldName = "POLICY#";
                FrenchFieldName = "NO DE POLICE";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(12);
                break;
            case "C02":
                //Subscriber Identification Number
                FieldName = "CERTIFICATE NO";
                FrenchFieldName = "NO DE CERTIFICAT";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(12);
                break;
            case "C03":
                FieldName = "Relationship Code";
                FrenchFieldName = "Code de rapport";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "C04":
                FieldName = "Patient's Sex";
                FrenchFieldName = "Le sexe du patient";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "C05":
                FieldName = "Patient's Birthday";
                FrenchFieldName = "L'anniversaire du patient";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "C06":
                FieldName = "Patient's Last Name";
                FrenchFieldName = "Le dernier nom du patient";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "C07":
                FieldName = "Patient's First Name";
                FrenchFieldName = "Le pr�nom du patient";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(15);
                break;
            case "C08":
                FieldName = "Patient's Middle Initial";
                FrenchFieldName = "L'initiale moyenne du patient";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "C09":
                FieldName = "Eligibility Exception Code";
                FrenchFieldName = "Code d'exception d'acceptabilit�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "C10":
                FieldName = "Name of School";
                FrenchFieldName = "Nom d'�cole";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "C11":
                FieldName = "DIVISION/SECTION NO";
                FrenchFieldName = "NO DE DIVISION/SECTION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "C12": //Not in version 2.
                FieldName = "Plan Flag";
                FrenchFieldName = "Drapeau de plan";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "C13": //Not in version 2.
                FieldName = "Band Number";
                FrenchFieldName = "Nombre de bande";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "C14": //Not in version 2.
                FieldName = "Family Number";
                FrenchFieldName = "Nombre de famille";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "C15": //Not in version 2.
                FieldName = "Missing Teeth";
                FrenchFieldName = "Dents absentes";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(11);
                break;
            case "C16": //Not in version 2.
                FieldName = "Eligibility Date";
                FrenchFieldName = "Date d'acceptabilit�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "C17": //Not in version 2.
                FieldName = "Primary Dependant Code";
                FrenchFieldName = "Code d�pendant primaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "C18": //Not in version 2.
                FieldName = "Plan Record Count";
                FrenchFieldName = "Plan Coun record";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                //valueMap=new ValueMap(new string[] {"A","N"},new string[] {"1"});
                //valueRequirements.Add(new DiscreteValuesBasedOnOtherField("C12",new ValueMap[] {valueMap}));
                break;
            case "C19": //Not in version 2.
                FieldName = "Plan Record";
                FrenchFieldName = "Disque de plan";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "D01":
                FieldName = "Subscriber's Birthday";
                FrenchFieldName = "L'anniversaire de l'abonn�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "D02":
                FieldName = "Subscriber's Last Name";
                FrenchFieldName = "Le dernier nom de l'abonn�";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "D03":
                FieldName = "Subscriber's First Name";
                FrenchFieldName = "Le pr�nom de l'abonn�";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(15);
                break;
            case "D04":
                FieldName = "Subscriber's Middle Initial";
                FrenchFieldName = "L'initiale moyenne de l'abonn�";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "D05":
                FieldName = "Subscriber's Address Line 1";
                FrenchFieldName = "Ligne 1 de l'adresse de l'abonn�";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "D06":
                FieldName = "Subscriber's Address Line 2";
                FrenchFieldName = "Ligne 2 de l'adresse de l'abonn�";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "D07":
                FieldName = "Subscriber's City";
                FrenchFieldName = "La ville de l'abonn�";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(20);
                break;
            case "D08":
                FieldName = "Subscriber's Province/State Code";
                FrenchFieldName = "Code de la province/�tat de l'abonn�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "D09":
                FieldName = "Subscriber's Postal/ZIP Code";
                FrenchFieldName = "Code du Postal/ZIP de l'abonn�";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(9);
                break;
            case "D10":
                FieldName = "Language of the Insured";
                FrenchFieldName = "Langue des assur�s";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "D11": //Not in version 2.
                FieldName = "Card Sequence/Version Number";
                FrenchFieldName = "Ordre de carte/nombre de version";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement()); //This field is optional.  Is set to 0 when not used.
                break;
            case "E01":
                FieldName = "Secondary Carrier Unique ID Number";
                FrenchFieldName = "Nombre unique d'identification de porteur secondaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "E02":
                FieldName = "Secondary Policy/Plan";
                FrenchFieldName = "Politique/plan secondaires";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(12);
                break;
            case "E03":
                FieldName = "Secondary Plan Subscriber ID";
                FrenchFieldName = "Identification secondaire d'abonn� de plan";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(12);
                break;
            case "E04":
                FieldName = "Secondary Subscriber's Birthday";
                FrenchFieldName = "L'anniversaire de l'abonn� secondaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "E05":
                FieldName = "Secondary Division/Section Number";
                FrenchFieldName = "Nombre secondaire de Division/section";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "E06":
                FieldName = "Secondary Relationship Code";
                FrenchFieldName = "Code secondaire de rapport";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "E07": //Not in version 2.
                FieldName = "Secondary Card Sequence/Version Number";
                FrenchFieldName = "Ordre de carte secondaire/nombre de version";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "E08": //Not in version 2.
                FieldName = "Secondary Subscriber's Last Name";
                FrenchFieldName = "Le dernier nom de l'abonn� secondaire";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(25);
                break;
            case "E09": //Not in version 2.
                FieldName = "Secondary Subscriber's First Name";
                FrenchFieldName = "Le pr�nom de l'abonn� secondaire";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(15);
                break;
            case "E10": //Not in version 2.
                FieldName = "Secondary Subscriber's Middle Initial";
                FrenchFieldName = "L'initiale moyenne de l'abonn� secondaire";
                Format = "AE";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "E11": //Not in version 2.
                FieldName = "Secondary Subscriber's Address Line 1";
                FrenchFieldName = "Ligne 1 de l'adresse de l'abonn� secondaire";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "E12": //Not in version 2.
                FieldName = "Secondary Subscriber's Address Line 2";
                FrenchFieldName = "Ligne 2 de l'adresse de l'abonn� secondaire";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(30);
                break;
            case "E13": //Not in version 2.
                FieldName = "Secondary Subscriber's City";
                FrenchFieldName = "La ville de l'abonn� secondaire";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(20);
                break;
            case "E14": //Not in version 2.
                FieldName = "Secondary Subscriber's Province/State Code";
                FrenchFieldName = "Code de la province/�tat de l'abonn� secondaire";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "E15": //Not in version 2.
                FieldName = "Secondary Subscriber's Postal/ZIP Code";
                FrenchFieldName = "Code du Postal/ZIP de l'abonn� secondaire";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(9);
                break;
            case "E16": //Not in version 2.
                FieldName = "Secondary Language";
                FrenchFieldName = "Langue secondaire";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "E17": //Not in version 2.
                FieldName = "Secondary Dependant Code";
                FrenchFieldName = "Code d�pendant secondaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "E18": //Not in version 2.
                FieldName = "Secondary Coverage";
                FrenchFieldName = "Assurance secondaire";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "E19": //Not in version 2.
                FieldName = "Secondary Carrier Transaction Counter";
                FrenchFieldName = "Compteur secondaire de transaction de porteur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "E20": //Not in version 2.
                FieldName = "Secondary Record Count";
                FrenchFieldName = "Compte record secondaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                //valueMap=new ValueMap(new string[] {"Y","O"},new string[] {"1"});
                //valueRequirements.Add(new DiscreteValuesBasedOnOtherField("E18",new ValueMap[]{valueMap}));
                break;
            case "F01":
                FieldName = "Payee Code";
                FrenchFieldName = "Code de b�n�ficiaire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F02":
                FieldName = "Accident Date";
                FrenchFieldName = "Date d'accidents";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F03":
                FieldName = "Predetermination Number";
                FrenchFieldName = "Nombre de pr�d�termination";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(14);
                break;
            case "F04":
                FieldName = "Date of Initial Placement Upper";
                FrenchFieldName = "Date de haut initial de placement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F05":
                FieldName = "Treatment Required for orthodontic";
                FrenchFieldName = "Traitement requis pour orthodontique";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F06":
                FieldName = "Number of Procedures Performed";
                FrenchFieldName = "Nombre de proc�dures ex�cut�es";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F07":
                FieldName = "Procedure Line Number";
                FrenchFieldName = "Ligne nombre de proc�d�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F08":
                FieldName = "Procedure Code";
                FrenchFieldName = "Code de proc�d�";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "F09":
                FieldName = "Date of Service";
                FrenchFieldName = "Date de service";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F10":
                FieldName = "International Tooth,Sextant, Quad or Arch";
                FrenchFieldName = "Dent, sextant, quadruple ou vo�te international";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "F11":
                FieldName = "Tooth Surface";
                FrenchFieldName = "Surface de dent";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(5);
                _valueRequirements.Add(new RegexValueRequirement());
                break;
            case "F12":
                FieldName = "Dentist's Fee Claimed";
                FrenchFieldName = "Les honoraires du dentiste r�clam�s";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F13":
                FieldName = "Lab Procedure Fee # 1";
                FrenchFieldName = "Honoraires # 1 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            //case "F14": // Does not exist in data dictionary!
            //	break;
            case "F15":
                FieldName = "Is this an Initial Placement Upper";
                FrenchFieldName = "Est c'un premier haut de placement";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F16": //Not in version 2.
                FieldName = "Procedure Type Codes";
                FrenchFieldName = "Type codes de proc�d�";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(5);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F17": //Not in version 2.
                FieldName = "Remarks Code";
                FrenchFieldName = "Code de remarques";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "F18": //Not in version 2.
                FieldName = "Is this an Initial Placement Lower";
                FrenchFieldName = "Est c'un premier placement inf�rieur";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F19": //Not in version 2.
                FieldName = "Date of Initial Placement Lower";
                FrenchFieldName = "La date du placement initial s'abaissent";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F20": //Not in version 2.
                FieldName = "Maxillary Prosthesis Material";
                FrenchFieldName = "Mat�riel maxillaire de proth�se";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement()); //This field is optional.  Is set to 0 when not used.
                break;
            case "F21": //Not in version 2.
                FieldName = "Mandibular Prosthesis Material";
                FrenchFieldName = "Mat�riel mandibulaire de proth�se";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement()); //This field is optional.  Is set to 0 when not used.
                break;
            case "F22": //Not in version 2.
                FieldName = "Extracted Teeth Count";
                FrenchFieldName = "Compte extrait de dents";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F23": //Not in version 2.
                FieldName = "Extracted Tooth Number";
                FrenchFieldName = "Nombre extrait de dent";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "F24": //Not in version 2.
                FieldName = "Extraction Date";
                FrenchFieldName = "Date d'extraction";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F25": //Not in version 2.
                FieldName = "Orthodontic Record Flag";
                FrenchFieldName = "Drapeau record orthodontique";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F26": //Not in version 2.
                FieldName = "First Examination Fee";
                FrenchFieldName = "Premiers honoraires d'examen";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F27": //Not in version 2.
                FieldName = "Diagnostic Phase Fee";
                FrenchFieldName = "Honoraires diagnostiques de phase";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F28": //Not in version 2.
                FieldName = "Initial Payment";
                FrenchFieldName = "Paiement initial";
                Format = "D";
                _lengthRequirement = new ConstLengthWhenOtherFieldHasValue("F25", "1", 6);
                break;
            case "F29": //Not in version 2.
                FieldName = "Payment Mode";
                FrenchFieldName = "Mode de paiement";
                Format = "N";
                _lengthRequirement = new ConstLengthWhenOtherFieldHasValue("F25", "1", 1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F30": //Not in version 2.
                FieldName = "Treatment Duration";
                FrenchFieldName = "Dur�e de traitement";
                Format = "N";
                _lengthRequirement = new ConstLengthWhenOtherFieldHasValue("F25", "1", 2);
                break;
            case "F31": //Not in version 2.
                FieldName = "Number of Anticipated Payments";
                FrenchFieldName = "Nombre de paiements pr�vus";
                Format = "N";
                _lengthRequirement = new ConstLengthWhenOtherFieldHasValue("F25", "1", 2);
                break;
            case "F32": //Not in version 2.
                FieldName = "Anticipated Payment Amount";
                FrenchFieldName = "Quantit� pr�vue de paiement";
                Format = "D";
                _lengthRequirement = new ConstLengthWhenOtherFieldHasValue("F25", "1", 6);
                break;
            case "F33": //Not in version 2.
                FieldName = "Reconciliation Date";
                FrenchFieldName = "Date de r�conciliation";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F34": //Not in version 2.
                FieldName = "Lab Procedure Code # 1";
                FrenchFieldName = "Code # 1 de proc�d� de laboratoire";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "F35": //Not in version 2.
                FieldName = "Lab Procedure Code # 2";
                FrenchFieldName = "Code # 2 de proc�d� de laboratoire";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "F36": //Not in version 2.
                FieldName = "Lab Procedure Fee # 2";
                FrenchFieldName = "Honoraires # 2 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F37": //Not in version 2.
                FieldName = "Estimated Treatment Start Date";
                FrenchFieldName = "Date estim�e de d�but de traitement";
                Format = "N";
                _lengthRequirement = new ConstLengthWhenOtherFieldHasValue("F25", "1", 8);
                break;
            case "F38": //Not in version 2.
                FieldName = "Current Reconciliation Page Number";
                FrenchFieldName = "Num�ro de page courant de r�conciliation";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F39": //Not in version 2.
                FieldName = "Diagnostic Code";
                FrenchFieldName = "Code diagnostique";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F40": //Not in version 2.
                FieldName = "Institution Code";
                FrenchFieldName = "Code d'�tablissement";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F41": //Not in version 2.
                FieldName = "Original DENTAL OFFICE CLAIM REFERENCE NO";
                FrenchFieldName = "Nombre d'ordre original d'Office";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "F42": //Not in version 2.
                FieldName = "Original Transaction Reference Number";
                FrenchFieldName = "Num�ro de r�f�rence original de transaction";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(14);
                break;
            case "F43": //Not in version 2.
                FieldName = "Attachment Source";
                FrenchFieldName = "Source d'attachement";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F44": //Not in version 2.
                FieldName = "Attachment Count";
                FrenchFieldName = "Compte d'attachement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "F45": //Not in version 2.
                FieldName = "Attachment Type";
                FrenchFieldName = "Type d'attachement";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(3);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "F46": //Not in version 2.
                FieldName = "Attachment Length";
                FrenchFieldName = "Longueur d'attachement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "F47": //Not in version 2.
                FieldName = "Attachment";
                FrenchFieldName = "Attachement";
                Format = "A/N";
                _lengthRequirement = new LengthFromAnotherField("F46");
                break;
            case "F48": //Not in version 2.
                FieldName = "Attachment File Date";
                FrenchFieldName = "Date de dossier d'attachement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "F49": //Not in version 2.
                FieldName = "Attachment Number";
                FrenchFieldName = "Nombre d'attachement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G01":
                //Transaction Reference Number
                FieldName = "CARRIER CLAIM NO";
                FrenchFieldName = "NO DE R�F�RENCE DE TRANSACTION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(14);
                break;
            case "G02":
                FieldName = "Eligible Amount for Lab Procedure Code #2";
                FrenchFieldName = "Quantit� �ligible pour le code #2 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G03":
                FieldName = "Expected Payment Date";
                FrenchFieldName = "Date pr�vue de paiement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "G04":
                FieldName = "Total Amount of Service";
                FrenchFieldName = "Montant total de service";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G05":
                FieldName = "Response Status";
                FrenchFieldName = "Statut de r�ponse";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G06":
                FieldName = "Number of Error Codes";
                FrenchFieldName = "Nombre de codes d'erreur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G07":
                //Disposition message
                FieldName = "DISPOSITION";
                FrenchFieldName = "SP�CIFICATIONS";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(75);
                break;
            case "G08":
                FieldName = "Error Code";
                FrenchFieldName = "Code d'erreur";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            //case "G09": // Does not exist
            //break;
            case "G10":
                FieldName = "Number of Carrier Issued Procedure Codes";
                FrenchFieldName = "Le nombre de porteur a publi� des codes de proc�d�";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G11":
                FieldName = "Number of Note Lines";
                FrenchFieldName = "Nombre de lignes de note";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G12":
                FieldName = "Eligible Amount";
                FrenchFieldName = "Quantit� �ligible";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G13":
                FieldName = "Deductible Amount";
                FrenchFieldName = "Quantit� d�ductible";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G14":
                FieldName = "Eligible Percentage";
                FrenchFieldName = "Pourcentage �ligible";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G15":
                FieldName = "Benefit Amount for the Procedure";
                FrenchFieldName = "Quantit� d'avantage pour le proc�d�";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G16":
                FieldName = "Explanation Note Number 1";
                FrenchFieldName = "Note num�ro 1 d'explication";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G17":
                FieldName = "Explanation Note Number 2";
                FrenchFieldName = "Note num�ro 2 d'explication";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G18":
                FieldName = "Reference to Line Number of the Submitted Procedure";
                FrenchFieldName = "R�f�rence � la ligne nombre du proc�d� soumis";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(7);
                _valueRequirements.Add(new RegexValueRequirement());
                break;
            case "G19":
                FieldName = "Additional Procedure Code";
                FrenchFieldName = "Code additionnel de proc�d�";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G20":
                FieldName = "Eligible Amount for the Additional Procedure";
                FrenchFieldName = "Quantit� �ligible pour le proc�d� additionnel";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G21":
                FieldName = "Dedutible for the Additional Procedure";
                FrenchFieldName = "";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G22":
                FieldName = "Eligible Percentage";
                FrenchFieldName = "Pourcentage �ligible";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G23":
                FieldName = "Benefit Amount for the Additional Procedure";
                FrenchFieldName = "Quantit� d'avantage pour le proc�d� additionnel";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G24":
                FieldName = "Explanation Note Number 1 for the Additional Procedure";
                FrenchFieldName = "Note d'explication num�ro 1 pour le proc�d� additionnel";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G25":
                FieldName = "Explanation Note Number 2 for the Additional Procedure";
                FrenchFieldName = "Note d'explication num�ro 2 pour le proc�d� additionnel";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                break;
            case "G26":
                FieldName = "Note Text";
                FrenchFieldName = "Noter le texte";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(75);
                break;
            case "G27":
                FieldName = "Language of the Insured";
                FrenchFieldName = "Langue des assur�s";
                Format = "A";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new DiscreteValueRequirement());
                break;
            case "G28":
                FieldName = "Total Benefit Amount";
                FrenchFieldName = "Quantit� totale d'avantage";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G29":
                FieldName = "Deductible amount unallocated";
                FrenchFieldName = "La quantit� d�ductible a d�sassign�";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G30":
                FieldName = "VERIFICATION NO";
                FrenchFieldName = "CODE DE VALIDATION";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "G31": //Not in version 2.
                FieldName = "Display Message Count";
                FrenchFieldName = "Compte de message d'affichage";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G32": //Not in version 2.
                FieldName = "Display Message";
                FrenchFieldName = "Message d'affichage";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(75);
                break;
            case "G33": //Not in version 2.
                FieldName = "PAYMENT ADJUSTMENT AMOUNT";
                FrenchFieldName = "MONTANT D'ADAPTATION DE PAIEMENT";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G34": //Not in version 2.
                FieldName = "PAYMENT REFERENCE";
                FrenchFieldName = "R�F�RENCE DE PAIEMENT";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "G35": //Not in version 2.
                FieldName = "PAYMENT DATE";
                FrenchFieldName = "DATE DE PAIEMENT";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(8);
                break;
            case "G36": //Not in version 2.
                FieldName = "PAYMENT AMOUNT";
                FrenchFieldName = "QUANTIT� DE PAIEMENT";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G37": //Not in version 2.
                FieldName = "Payment Detail Count";
                FrenchFieldName = "Compte de d�tail de paiement";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G38": //Not in version 2.
                FieldName = "Transaction Payment";
                FrenchFieldName = "Paiement de transaction";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G39": //Not in version 2.
                FieldName = "Embedded Transaction Length";
                FrenchFieldName = "Longueur incluse de transaction";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "G40": //Not in version 2.
                FieldName = "Embedded Transaction";
                FrenchFieldName = "Transaction incluse";
                Format = "AE/N";
                _lengthRequirement = new LengthFromAnotherField("G39");
                break;
            case "G41": //Not in version 2.
                FieldName = "Message Output Flag";
                FrenchFieldName = "Drapeau de rendement de message";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G42": //Not in version 2.
                FieldName = "Form ID";
                FrenchFieldName = "Former l'identification";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G43": //Not in version 2.
                FieldName = "Eligible Amount for Lab Procedure Code # 1";
                FrenchFieldName = "Quantit� �ligible pour le code # 1 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G44": //Not in version 2.
                FieldName = "Eligible Lab Amount for the Additional Procedure";
                FrenchFieldName = "Quantit� �ligible de laboratoire pour le proc�d� additionnel";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G45": //Not in version 2.
                FieldName = "Note Number";
                FrenchFieldName = "Noter le nombre";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "G46": //Not in version 2.
                FieldName = "Current Predetermination Page Number";
                FrenchFieldName = "Num�ro de page courant de pr�d�termination";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "G47": //Not in version 2.
                FieldName = "Last Predetermination Page Number";
                FrenchFieldName = "Num�ro de page courant de pr�d�termination";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                break;
            case "G48": //Not in version 2.
                FieldName = "E-Mail Office Number";
                FrenchFieldName = "Nombre d'Office d'E-mail";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(4);
                break;
            case "G49": //Not in version 2.
                //E-mail to
                FieldName = "TO";
                FrenchFieldName = "DESTINATAIRE";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(60);
                break;
            case "G50": //Not in version 2.
                //E-mail from
                FieldName = "FROM";
                FrenchFieldName = "EXP�DITEUR";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(60);
                break;
            case "G51": //Not in version 2.
                FieldName = "SUBJECT";
                FrenchFieldName = "OBJET";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(60);
                break;
            case "G52": //Not in version 2.
                FieldName = "Number of E-mail Note Lines";
                FrenchFieldName = "Nombre de lignes de note d'E-mail";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(2);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            case "G53": //Not in version 2.
                FieldName = "E-Mail Note Line";
                FrenchFieldName = "Ligne de note d'E-mail";
                Format = "AE/N";
                _lengthRequirement = new ConstLengthRequirement(75);
                break;
            case "G54": //Not in version 2.
                //Email reference number
                FieldName = "REFERENCE";
                FrenchFieldName = "R�F�RENCE";
                Format = "A/N";
                _lengthRequirement = new ConstLengthRequirement(10);
                break;
            case "G55": //Not in version 2.
                FieldName = "Total Payable";
                FrenchFieldName = "Payable total";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(7);
                break;
            case "G56": //Not in version 2.
                FieldName = "Deductible Amount for Lab Procedure Code # 1";
                FrenchFieldName = "Quantit� d�ductible pour le code # 1 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G57": //Not in version 2.
                FieldName = "Eligible Percentage for Lab Procedure # 1";
                FrenchFieldName = "Pourcentage �ligible pour le proc�d� # 1 de laboratoire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "G58": //Not in version 2.
                FieldName = "Benefit Amount for Lab Procedure Code #1";
                FrenchFieldName = "Quantit� d'avantage pour le code #1 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G59": //Not in version 2.
                FieldName = "Deductible Amount for Lab Procedure Code # 2";
                FrenchFieldName = "Quantit� d�ductible pour le code # 2 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(5);
                break;
            case "G60": //Not in version 2.
                FieldName = "Eligible Percentage for Lab Procedure Code # 2";
                FrenchFieldName = "Pourcentage �ligible pour le code # 2 de proc�d� de laboratoire";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(3);
                break;
            case "G61": //Not in version 2.
                FieldName = "Benefit Amount for Lab Procedure Code # 2";
                FrenchFieldName = "B�n�ficier la quantit� pour le code # 2 de proc�d� de laboratoire";
                Format = "D";
                _lengthRequirement = new ConstLengthRequirement(6);
                break;
            case "G62": //Not in version 2.
                FieldName = "Last Reconciliation Page Number";
                FrenchFieldName = "Dernier num�ro de page de r�conciliation";
                Format = "N";
                _lengthRequirement = new ConstLengthRequirement(1);
                _valueRequirements.Add(new RangeValueRequirement());
                break;
            default:
                throw new ODException("Internal Error, unknown version 4 CCD field ID during construction: " + pFieldId);
        }
    }
}