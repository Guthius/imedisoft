using System;
using System.Text;
using Imedisoft.Core.Entities;
using ODCrypt;

namespace OpenDentBusiness;

public class Authentication
{
    public static PasswordContainer GenerateLoginDetails(string input, HashTypes hashType)
    {
        var salt = GenerateSalt(hashType);
        var hash = GetHash(input, salt, hashType);
            
        return new PasswordContainer(hashType, salt, hash);
    }
        
    public static bool CheckPassword(Userod user, string password)
    {
        var passwordContainer = user.GetPasswordContainer();
        if (passwordContainer.HashType == HashTypes.None)
        {
            return password == "";
        }
            
        return CheckPassword(password, passwordContainer);
    }

    public static bool CheckPassword(string input, PasswordContainer password)
    {
        return CheckPassword(input, password.Salt, password.Hash, password.HashType);
    }

    public static bool CheckPassword(string input, string salt, string hash, HashTypes hashType)
    {
        salt ??= "";

        var key = GetHash(input, salt, hashType);
            
        return ConstantEquals(key, hash);
    }
        
    public static string GetHash(string input, string salt, HashTypes type)
    {
        return type switch
        {
            HashTypes.MD5 => HashPasswordMd5(salt + input),
            HashTypes.MD5_ECW => HashPasswordMd5(input),
            HashTypes.SHA3_512 => HashPasswordSha512(input, salt),
            HashTypes.None => input,
            _ => throw new ApplicationException("Hash Type not implimented: " + type)
        };
    }
        
    public static void UpdatePasswordUserod(Userod user, string input, HashTypes hashType = HashTypes.SHA3_512)
    {
        var isPasswordStrong = string.IsNullOrEmpty(Userods.IsPasswordStrong(input));
        var loginDetails = GenerateLoginDetails(input, hashType);
            
        try
        {
            Userods.UpdatePassword(user, loginDetails, isPasswordStrong);
        }
        catch
        {
            // ignored
        }
    }
        
    public static string HashPasswordMd5(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return "";
        }

        var bytes = Encoding.Unicode.GetBytes(input);
        var bytesHash = MD5.Hash(bytes);
            
        return Convert.ToBase64String(bytesHash);
    }
        
    public static PasswordContainer GenerateLoginDetailsSha512(string input)
    {
        var salt = GenerateSalt(HashTypes.SHA3_512);
        var hash = GetHash(input, salt, HashTypes.SHA3_512);
            
        return new PasswordContainer(HashTypes.SHA3_512, salt, hash);
    }
        
    public static string HashPasswordSha512(string input, string salt = "")
    {
        if (string.IsNullOrEmpty(input))
        {
            return "";
        }

        var bytes = Encoding.Unicode.GetBytes(salt + input);
        var bytesHash = Sha3.Hash(bytes);
            
        return Convert.ToBase64String(bytesHash);
    }
        
    public static bool ConstantEquals(string lhs, string rhs)
    {
        return CryptUtil.ConstantEquals(lhs, rhs);
    }
        
    public static string GenerateSalt(HashTypes hashType)
    {
        var size = hashType switch
        {
            HashTypes.SHA3_512 => 64,
            HashTypes.MD5 => 16,
            _ => 0
        };

        return GenerateSalt(size);
    }

    public static string GenerateSalt(int byteLength)
    {
        return CryptUtil.GenerateSalt(byteLength);
    }
        
    public static PasswordContainer DecodePass(string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return new PasswordContainer(HashTypes.None, "", "");
        }

        var parts = passwordHash.Split('$');
        var success = Enum.TryParse(parts[0], out HashTypes hashType);

        if (success && parts.Length == 3)
        {
            return new PasswordContainer(hashType, parts[1], parts[2]);
        }
            
        if (passwordHash.Length == 24 && passwordHash.EndsWith("==") && !passwordHash.Contains("$"))
        {
            return new PasswordContainer(HashTypes.MD5, "", passwordHash);
        }
                
        return new PasswordContainer(HashTypes.None, "", passwordHash);

    }
}

public struct PasswordContainer
{
    public HashTypes HashType;
    public string Salt;
    public string Hash;
        
    public PasswordContainer(HashTypes hashType, string salt, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            HashType = HashTypes.None;
            Salt = "";
        }
        else
        {
            HashType = hashType;
            Salt = salt;
        }

        Hash = passwordHash;
    }

    public override string ToString()
    {
        return string.Join("$", HashType.ToString(), Salt, Hash);
    }
}

public enum HashTypes
{
    None,
    MD5,
    MD5_ECW,
    SHA3_512
}