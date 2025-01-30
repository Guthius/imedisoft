using System;

namespace OpenDentBusiness;

[AttributeUsage(AttributeTargets.Field)]
public class CrudColumnAttribute : Attribute
{
    public bool IsPriKey { get; set; }
    public CrudSpecialColType SpecialType { get; set; } = CrudSpecialColType.None;
    public bool IsNotDbColumn { get; set; }
    public bool IsNotCemtColumn { get; set; }
}
    
[Flags]
public enum CrudSpecialColType
{
    None = 0,

    ///<summary>User not allowed to change.  Insert uses NOW(), Update exludes this column, Select treats this like a date.</summary>
    DateEntry = 1,

    ///<summary>Insert uses NOW(), Update and Select treat this like a Date.</summary>
    DateEntryEditable = 2,

    ///<summary>Is set and updated by MySQL.  Leave these columns completely out of Insert and Update statements.  The name of the column must be exactly: DateTStamp for the crud schema logic to work.</summary>
    TimeStamp = 4,

    ///<summary>Same C# type as Date, but the MySQL database uses datetime instead of date.</summary>
    DateT = 8,

    ///<summary>User not allowed to change.  Insert uses NOW(), Update exludes this column, Select treats this like a DateT.</summary>
    DateTEntry = 16,

    ///<summary>Insert uses NOW(), Update and Select treat this like a DateT.</summary>
    DateTEntryEditable = 32,

    ///<summary>Database type is tinyint signed.  C# type is int.  Range -128 to 127.  The validation does not check to make sure the db is signed.  The programmer must do that.  So far, only used for percent fields because -1 is required to be accepted.  For most other fields, such as enums and itemorders, the solution is to change the field in C# to a byte, indicating a range of 0-255.  It usually doesn't matter whether the database accepts values to 255 or only to 127.  The validation does not check that.</summary>
    TinyIntSigned = 64,

    ///<summary>We do not want this column updated except as part of a separate routine.  Warning! The logic fails if this is used on the last column in a table.</summary>
    ExcludeFromUpdate = 128,

    ///<summary>Instead of storing this enum as an int in the db, it is stored as a string. ONLY used when you would never use a query that searched by this field. This is used in approx 60 places, and we probably need to refactor most of them to store the enum in the normal way as a tinyint. It can cause extreme slowness to run queries matching strings. As usual, Jordan would be involved. We would add a new column and deprecate the old column in order to facilitate converting each value and to not crash queries, API, etc.</summary>
    [Obsolete("Talk to Jordan before using")]
    EnumAsString = 256,

    ///<summary>DEPRECATED. See TimeSpanLong. For most C# TimeSpans, the default db type is TimeOfDay.  But for the few that need to use negative values or values greater than 24 hours, they get marked as this special type.  Handled differently in MySQL vs Oracle.</summary>
    TimeSpanNeg = 512,

    ///<summary>MySql type is TEXT, MEDIUMTEXT, or LONGTEXT instead of VARCHAR. This attribute was formerly known as TextIsClob and was only used for Oracle, but the usage gradually changed into this current usage. If this is specified, then they get sent as actual MySqlParameters instead of strings. The crud generator will throw an exception if a column with this attribute is not one of the 3 listed above. We only use VARCHAR up to 255 characters, even though it can handle 65k. For anything over 255, use TEXT instead and set this attribute.</summary>
    IsText = 1024,

    ///<summary>If the text contains 50 or more consecutive new line characters, the insert and update crud calls will replace them with a single 
    ///new line.  If the control tries to display a very large number of new line characters, graphics memory errors may occur when trying to measure 
    ///the height of the string.  This attribute should only be used for large note fields that the user is allowed to type into manually.  Not for 
    ///base64 text or other programatically generated text. Additionally null characters will be removed from the text.</summary>
    CleanText = 2048,

    /// <summary>MySQL 5.5 limits TimeSpan to 838:59:59 which is 34 days 6 hours. Use this tag to store the timespan as a bigint/long in the DB as a number of ticks. 1 Tick = 1/10 ms.</summary>
    TimeSpanLong = 4096,

    ///<summary>Allows storing a double with more than two decimal places.</summary>
    Double = 8192
}