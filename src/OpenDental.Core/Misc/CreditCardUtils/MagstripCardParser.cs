using System;
using CodeBase;

namespace OpenDentBusiness;

public class MagstripCardParser
{
    private const char TRACK_SEPARATOR = '?';
    private const char FIELD_SEPARATOR = '^';
    private readonly string _inputStripeStr;
    private string _track1Data;
    private bool _needsParsing;
    private bool _hasTrack1;
    private bool _hasTrack2;
    private bool _hasTrack3;
    private string _accountHolder;

    public MagstripCardParser(string trackString, EnumMagstripCardParseTrack enumMagstripCardParseTrack = EnumMagstripCardParseTrack.All)
    {
        _inputStripeStr = trackString;
        _needsParsing = true;
        Parse(enumMagstripCardParseTrack);
    }
        
    public string Track2 { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string AccountNumber { get; private set; }
    public int ExpirationMonth { get; private set; }
    public int ExpirationYear { get; private set; }
        

    protected void Parse(EnumMagstripCardParseTrack enumMagstripCardParseTrack = EnumMagstripCardParseTrack.All)
    {
        if (!_needsParsing)
        {
            return;
        }

        try
        {
            //Example: Track 1 Data Only
            //%B1234123412341234^CardUser/John^030510100000019301000000877000000?
            //Key off of the presence of "^" but not "="
            //Example: Track 2 Data Only
            //;1234123412341234=0305101193010877?
            //Key off of the presence of "=" but not "^"
            //Determine the presence of special characters
            string[] tracks = _inputStripeStr.Split(new char[] {TRACK_SEPARATOR}, StringSplitOptions.RemoveEmptyEntries);
            if (tracks.Length > 0)
            {
                //Explicitly set the track data based on the enum value passed in. If set to anything but All, we expect 1 and only 1 track to be present in trackString.
                if (enumMagstripCardParseTrack.In(EnumMagstripCardParseTrack.All, EnumMagstripCardParseTrack.TrackOne))
                {
                    _hasTrack1 = true;
                    _track1Data = tracks[0];
                }
                else if (enumMagstripCardParseTrack == EnumMagstripCardParseTrack.TrackTwo)
                {
                    _hasTrack2 = true;
                    Track2 = tracks[0];
                }
                else if (enumMagstripCardParseTrack == EnumMagstripCardParseTrack.TrackThree)
                {
                    _hasTrack3 = true;
                }
            }

            if (tracks.Length > 1 && enumMagstripCardParseTrack == EnumMagstripCardParseTrack.All)
            {
                _hasTrack2 = true;
                Track2 = tracks[1];
            }

            if (tracks.Length > 2 && enumMagstripCardParseTrack == EnumMagstripCardParseTrack.All)
            {
                _hasTrack3 = true;
            }

            if (_hasTrack1)
            {
                ParseTrack1();
            }

            if (_hasTrack2)
            {
                ParseTrack2();
            }

            if (_hasTrack3)
            {
                ParseTrack3();
            }
        }
        catch (MagstripCardParseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MagstripCardParseException(ex);
        }

        _needsParsing = false;
    }

    private void ParseTrack1()
    {
        if (String.IsNullOrEmpty(_track1Data))
        {
            throw new MagstripCardParseException("Track 1 data is empty.");
        }

        string[] parts = _track1Data.Split(new char[] {FIELD_SEPARATOR}, StringSplitOptions.None);
        if (parts.Length != 3)
        {
            throw new MagstripCardParseException("Missing last field separator (^) in track 1 data.");
        }

        AccountNumber = CreditCardUtils.StripNonDigits(parts[0]);
        if (!String.IsNullOrEmpty(parts[1]))
        {
            _accountHolder = parts[1].Trim();
        }

        if (!String.IsNullOrEmpty(_accountHolder))
        {
            int nameDelim = _accountHolder.IndexOf("/");
            if (nameDelim > -1)
            {
                LastName = _accountHolder.Substring(0, nameDelim);
                FirstName = _accountHolder.Substring(nameDelim + 1);
            }
        }

        //date format: YYMM
        string expDate = parts[2].Substring(0, 4);
        ExpirationYear = ParseExpireYear(expDate);
        ExpirationMonth = ParseExpireMonth(expDate);
    }

    private void ParseTrack2()
    {
        if (String.IsNullOrEmpty(Track2))
        {
            throw new MagstripCardParseException("Track 2 data is empty.");
        }

        if (Track2.StartsWith(";"))
        {
            Track2 = Track2.Substring(1);
        }

        //may have already parsed this info out if track 1 data present
        if (String.IsNullOrEmpty(AccountNumber) || (ExpirationMonth == 0 || ExpirationYear == 0))
        {
            //Track 2 only cards
            //Ex: ;1234123412341234=0305101193010877?
            int sepIndex = Track2.IndexOf('=');
            if (sepIndex < 0)
            {
                throw new MagstripCardParseException("Invalid track 2 data.");
            }

            string[] parts = Track2.Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new MagstripCardParseException("Missing field separator (=) in track 2 data.");
            }

            if (String.IsNullOrEmpty(AccountNumber))
            {
                AccountNumber = CreditCardUtils.StripNonDigits(parts[0]);
            }

            if (ExpirationMonth == 0 || ExpirationYear == 0)
            {
                //date format: YYMM
                string expDate = parts[1].Substring(0, 4);
                ExpirationYear = ParseExpireYear(expDate);
                ExpirationMonth = ParseExpireMonth(expDate);
            }
        }
    }

    private void ParseTrack3()
    {
        //not implemented
    }

    private int ParseExpireMonth(string s)
    {
        s = CreditCardUtils.StripNonDigits(s);
        if (!ValidateExpiration(s))
        {
            return 0;
        }

        if (s.Length > 4)
        {
            s = s.Substring(0, 4);
        }

        return int.Parse(s.Substring(2, 2));
    }

    private int ParseExpireYear(string s)
    {
        s = CreditCardUtils.StripNonDigits(s);
        if (!ValidateExpiration(s))
        {
            return 0;
        }

        if (s.Length > 4)
        {
            s = s.Substring(0, 4);
        }

        int y = int.Parse(s.Substring(0, 2));
        if (y > 80)
        {
            y += 1900;
        }
        else
        {
            y += 2000;
        }

        return y;
    }

    private bool ValidateExpiration(string s)
    {
        if (String.IsNullOrEmpty(s))
        {
            return false;
        }

        if (s.Length < 4)
        {
            return false;
        }

        return true;
    }
}

public class MagstripCardParseException : Exception
{
    public MagstripCardParseException(Exception cause)
        : base(cause.Message, cause)
    {
    }

    public MagstripCardParseException(string msg)
        : base(msg)
    {
    }

    public MagstripCardParseException(string msg, Exception cause)
        : base(msg, cause)
    {
    }
}

/// <summary>Represents the track(s) contained in the trackString passed into the MagstripCardParser.</summary>
public enum EnumMagstripCardParseTrack
{
    /// <summary>0</summary>
    All,

    /// <summary>1</summary>
    TrackOne,

    /// <summary>2</summary>
    TrackTwo,

    /// <summary>3</summary>
    TrackThree
}