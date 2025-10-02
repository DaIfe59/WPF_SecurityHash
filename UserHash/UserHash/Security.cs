using System;
using System.Security.Cryptography;
using System.Text;

namespace UserHash;

public static class PasswordHasher
{
    public static string ComputeSha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}

public static class PasswordPolicy
{
    public static bool Validate(string password)
    {
        bool hasLatin = false;
        bool hasCyrillic = false;
        bool hasDigit = false;

        foreach (var ch in password)
        {
            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
            {
                hasLatin = true;
                continue;
            }

            // Cyrillic ranges: U+0400–U+04FF, U+0500–U+052F, U+2DE0–U+2DFF, U+A640–U+A69F
            if ((ch >= '\u0400' && ch <= '\u04FF') ||
                (ch >= '\u0500' && ch <= '\u052F') ||
                (ch >= '\u2DE0' && ch <= '\u2DFF') ||
                (ch >= '\uA640' && ch <= '\uA69F'))
            {
                hasCyrillic = true;
                continue;
            }

            if (char.IsDigit(ch))
            {
                hasDigit = true;
                continue;
            }
        }

        return hasLatin && hasCyrillic && hasDigit;
    }
}



