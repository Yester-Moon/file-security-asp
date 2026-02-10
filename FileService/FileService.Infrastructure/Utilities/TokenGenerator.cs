using System.Security.Cryptography;

namespace FileService.Infrastructure.Utilities;

public static class TokenGenerator
{
    public static string GenerateSecureToken(int byteLength = 32)
    {
        var bytes = new byte[byteLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public static string GenerateNumericCode(int length = 6)
    {
        var code = "";
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        
        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10;
            code += randomNumber;
        }
        
        return code;
    }
}
