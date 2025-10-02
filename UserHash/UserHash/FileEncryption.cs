using System;
using System.IO;
using System.Text;

namespace UserHash;

public class FileEncryption : IDisposable
{
    private IntPtr _hProv = IntPtr.Zero;
    private IntPtr _hKey = IntPtr.Zero;
    private bool _disposed = false;

    public bool Initialize(string passphrase)
    {
        try
        {
            // Acquire crypto context
            if (!CryptoAPI.CryptAcquireContext(out _hProv, null, null, 
                CryptoAPI.PROV_RSA_FULL, CryptoAPI.CRYPT_VERIFYCONTEXT))
            {
                return false;
            }

            // Create MD5 hash of passphrase
            if (!CryptoAPI.CryptCreateHash(_hProv, CryptoAPI.CALG_MD5, IntPtr.Zero, 0, out IntPtr hHash))
            {
                return false;
            }

            byte[] passphraseBytes = Encoding.UTF8.GetBytes(passphrase);
            if (!CryptoAPI.CryptHashData(hHash, passphraseBytes, (uint)passphraseBytes.Length, 0))
            {
                CryptoAPI.CryptDestroyHash(hHash);
                return false;
            }

            // Derive RC4 key from hash
            if (!CryptoAPI.CryptDeriveKey(_hProv, CryptoAPI.CALG_RC4, hHash, 
                CryptoAPI.CRYPT_EXPORTABLE, out _hKey))
            {
                CryptoAPI.CryptDestroyHash(hHash);
                return false;
            }

            CryptoAPI.CryptDestroyHash(hHash);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool EncryptFile(string inputPath, string outputPath)
    {
        if (_hKey == IntPtr.Zero) return false;

        try
        {
            byte[] data = File.ReadAllBytes(inputPath);
            uint dataLen = (uint)data.Length;
            uint bufferLen = dataLen + 1024; // Extra space for encryption

            Array.Resize(ref data, (int)bufferLen);

            if (!CryptoAPI.CryptEncrypt(_hKey, IntPtr.Zero, true, 0, data, ref dataLen, bufferLen))
            {
                return false;
            }

            // Write only the encrypted portion
            byte[] encryptedData = new byte[dataLen];
            Array.Copy(data, encryptedData, dataLen);
            File.WriteAllBytes(outputPath, encryptedData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool DecryptFile(string inputPath, string outputPath)
    {
        if (_hKey == IntPtr.Zero) return false;

        try
        {
            byte[] data = File.ReadAllBytes(inputPath);
            uint dataLen = (uint)data.Length;

            if (!CryptoAPI.CryptDecrypt(_hKey, IntPtr.Zero, true, 0, data, ref dataLen))
            {
                return false;
            }

            // Write only the decrypted portion
            byte[] decryptedData = new byte[dataLen];
            Array.Copy(data, decryptedData, dataLen);
            File.WriteAllBytes(outputPath, decryptedData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_hKey != IntPtr.Zero)
            {
                CryptoAPI.CryptDestroyKey(_hKey);
                _hKey = IntPtr.Zero;
            }
            if (_hProv != IntPtr.Zero)
            {
                CryptoAPI.CryptReleaseContext(_hProv, 0);
                _hProv = IntPtr.Zero;
            }
            _disposed = true;
        }
    }
}

