using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class LanguagePat : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long LanguagePatNum;

    ///<summary>FK to pref.PrefName. There are about 30 of these in use. This allows us to translate the value stored for templates like email, postcard, text, etc. Will be empty string if this is an eForm translation.></summary>
    public string PrefName;

    ///<summary>Three-letter language name or custom language name.  The custom language name is the full string name and is not necessarily supported by Microsoft.
    ///This will typically be matched to the patient's preferred language to select the appropriate translation.
    ///<br>Three-letter language name examples: eng (English), spa (Spanish), fra (French).</br>
    ///<br>Custom language name examples: Tahitian, American Sign Language, Morse Code.</br>
    ///The LanguagesUsedByPatients preference stores the three-letter names that the practice chooses to support.
    ///</summary>
    public string Language;

    public string Translation;

    ///<summary>FK to eformfielddef.EFormFieldDefNum. This is how eForms get translated. Once a def is converted to an eForm, this is not needed. The eForm fields have all the translated text. Will be 0 if this is a pref translation.</summary>
    public long EFormFieldDefNum;
    
    public LanguagePat Copy()
    {
        return (LanguagePat) MemberwiseClone();
    }
}