using System;
using MySqlConnector;

namespace OpenDentBusiness;

public class OdSqlParameter(string parameterName, OdDbType dbType, object value)
{
    public OdDbType DbType { get; } = dbType;
    public string ParameterName { get; } = parameterName;
    public object Value { get; } = value;

    public MySqlDbType GetMySqlDbType()
    {
        if (DbType == OdDbType.Text)
        {
            return MySqlDbType.MediumText;
        }

        throw new ApplicationException("Type not found");
    }

    public MySqlParameter GetMySqlParameter()
    {
        return new MySqlParameter
        {
            ParameterName = "@" + ParameterName,
            Value = Value,
            MySqlDbType = GetMySqlDbType()
        };
    }
}