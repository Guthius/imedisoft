using MySqlConnector;

namespace OpenDentBusiness;

public class OdSqlParameter(string parameterName, object value)
{
    public MySqlParameter GetMySqlParameter()
    {
        return new MySqlParameter
        {
            ParameterName = "@" + parameterName,
            Value = value,
            MySqlDbType = MySqlDbType.MediumText
        };
    }
}