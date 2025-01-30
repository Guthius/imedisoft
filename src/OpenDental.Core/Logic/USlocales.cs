using System.Collections.Generic;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class USlocales
{
    public static readonly List<StateAbbr> ListAll =
    [
        new("Alaska", "AK"),
        new("Alabama", "AL"),
        new("Arkansas", "AR"),
        new("Arizona", "AZ"),
        new("California", "CA"),
        new("Colorado", "CO"),
        new("Connecticut", "CT"),
        new("Delaware", "DE"),
        new("Florida", "FL"),
        new("Georgia", "GA"),
        new("Hawaii", "HI"),
        new("Iowa", "IA"),
        new("Idaho", "ID"),
        new("Illinois", "IL"),
        new("Indiana", "IN"),
        new("Kansas", "KS"),
        new("Kentucky", "KY"),
        new("Louisiana", "LA"),
        new("Massachussetts", "MA"),
        new("Maryland", "MD"),
        new("Maine", "ME"),
        new("Michigan", "MI"),
        new("Minnesota", "MN"),
        new("Missouri", "MO"),
        new("Mississippi", "MS"),
        new("Montana", "MT"),
        new("North Carolina", "NC"),
        new("North Dakota", "ND"),
        new("Nebraska", "NE"),
        new("New Hampshire", "NH"),
        new("New Jersey", "NJ"),
        new("New Mexico", "NM"),
        new("Nevada", "NV"),
        new("New York", "NY"),
        new("Ohio", "OH"),
        new("Oklahoma", "OK"),
        new("Oregon", "OR"),
        new("Pennsylvania", "PA"),
        new("Rhode Island", "RI"),
        new("South Carolina", "SC"),
        new("South Dakota", "SD"),
        new("Tennessee", "TN"),
        new("Texas", "TX"),
        new("Utah", "UT"),
        new("Virginia", "VA"),
        new("Vermont", "VT"),
        new("Washington", "WA"),
        new("Wisconsin", "WI"),
        new("West Virginia", "WV"),
        new("Wyoming", "WY"),
        
        // US Districts
        new("District of Columbia", "DC"),
        
        // US territories. Reference https://simple.wikipedia.org/wiki/U.S._postal_abbreviations
        new("American Samoa", "AS"),
        new("Federated States of Micronesia", "FM"),
        new("Guam", "GU"),
        new("Marshall Islands", "MH"),
        new("Northern Mariana Islands", "MP"),
        new("Palau", "PW"),
        new("Puerto Rico", "PR"),
        new("United States Minor Outlying Islands", "UM"),
        new("U.S. Virgin Islands", "VI")
    ];

    public static bool IsValidAbbr(string stateAbbr)
    {
        return ListAll.Exists(x => x.Abbr == stateAbbr.ToUpper());
    }
}