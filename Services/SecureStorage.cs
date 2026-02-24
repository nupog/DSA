using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace DeepSeekSurveyAnalyzer.Services;

public static class SecureStorage
{
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
            return string.Empty;
        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
        byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}