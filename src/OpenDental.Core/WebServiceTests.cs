using System;

namespace OpenDentBusiness
{
    ///<summary>These are only run from the Unit Testing framework</summary>
    public class WebServiceTests
    {
        private static string _dirtyString;
        private static string _newLineString;
        private static DateTime _dateTEntryTest;
        private static DateTime _dateTodayTest;

        public static string NewLineString
        {
            get
            {
                if (_newLineString == null)
                {
                    _newLineString = "Line1\rLine2\nLine3\r\nLine4";
                }

                return _newLineString;
            }
        }

        ///<summary>A helper object that can be used for simple unit tests.  It is safe to add to this class but do not remove.</summary>
        public class WebServiceTestObject
        {
            ///<summary>A simple string field that can be used as desired.</summary>
            public string ValueStr;

            ///<summary>ValueStr the second is specifically added for testing to see if "string" has the same problem as custom objects for XML escaping.
            ///There was a bug where the same Patient object was used in multiple fields within a parent object (Family object).
            ///There is a unit test for the aforementioned scenario and an additional unit test was created to test strings specifically.</summary>
            public string ValueStr2;
        }
    }
}