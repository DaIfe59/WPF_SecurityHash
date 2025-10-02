using System;
using System.Runtime.InteropServices;

namespace UserHash;

public static class CryptoAPI
{
    // Constants
    public const uint PROV_RSA_FULL = 1;
    public const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
    public const uint ALG_CLASS_HASH = 32768;
    public const uint ALG_TYPE_ANY = 0;
    public const uint ALG_SID_MD5 = 3;
    public const uint CALG_MD5 = (ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD5);
    public const uint ALG_CLASS_DATA_ENCRYPT = 24576;
    public const uint ALG_TYPE_STREAM = 2048;
    public const uint ALG_SID_RC4 = 1;
    public const uint CALG_RC4 = (ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_STREAM | ALG_SID_RC4);
    public const uint HP_HASHVAL = 2;
    public const uint CRYPT_EXPORTABLE = 1;

    // P/Invoke declarations
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptAcquireContext(
        out IntPtr phProv,
        string? pszContainer,
        string? pszProvider,
        uint dwProvType,
        uint dwFlags);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptCreateHash(
        IntPtr hProv,
        uint algid,
        IntPtr hKey,
        uint dwFlags,
        out IntPtr phHash);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptHashData(
        IntPtr hHash,
        byte[] pbData,
        uint dwDataLen,
        uint dwFlags);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptDeriveKey(
        IntPtr hProv,
        uint algid,
        IntPtr hBaseData,
        uint dwFlags,
        out IntPtr phKey);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptEncrypt(
        IntPtr hKey,
        IntPtr hHash,
        bool Final,
        uint dwFlags,
        byte[]? pbData,
        ref uint pdwDataLen,
        uint dwBufLen);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptDecrypt(
        IntPtr hKey,
        IntPtr hHash,
        bool Final,
        uint dwFlags,
        byte[] pbData,
        ref uint pdwDataLen);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptDestroyHash(IntPtr hHash);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptDestroyKey(IntPtr hKey);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();
}

