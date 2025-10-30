using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UserHash;

public class UserRepository
{
    private readonly string _encryptedFilePath;
    private readonly string _tempFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };
    private FileEncryption? _encryption;

    public UserRepository(string? filePath = null)
    {
        _encryptedFilePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.dat");
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"users_temp_{Guid.NewGuid()}.json");
    }

    public bool InitializeEncryption(string passphrase)
    {
        _encryption = new FileEncryption();
        return _encryption.Initialize(passphrase);
    }

    public UserStore? LoadOrCreate()
    {
        if (_encryption == null) return null;

        if (!File.Exists(_encryptedFilePath))
        {
            // Create initial store with ADMIN
            var store = new UserStore
            {
                Users = new List<UserAccount>
                {
                    new UserAccount
                    {
                        UserName = "ADMIN",
                        PasswordHash = string.Empty,
                        IsBlocked = false,
                        EnforcePasswordConstraints = false
                    }
                }
            };
            if (!Save(store))
            {
                return null; // Failed to create encrypted file
            }
            return store;
        }

        // Decrypt to temp file
        if (!_encryption.DecryptFile(_encryptedFilePath, _tempFilePath))
        {
            return null; // Wrong passphrase or corruption
        }

        try
        {
            using var stream = File.OpenRead(_tempFilePath);
            var loaded = JsonSerializer.Deserialize<UserStore>(stream);
            return loaded ?? new UserStore();
        }
        catch
        {
            return null;
        }
    }

    public bool Save(UserStore store)
    {
        if (_encryption == null) return false;

        try
        {
            // Save to temp file first
            using (var stream = File.Create(_tempFilePath))
            {
                JsonSerializer.Serialize(stream, store, _jsonOptions);
            }

            // Encrypt temp file to final location
            return _encryption.EncryptFile(_tempFilePath, _encryptedFilePath);
        }
        catch
        {
            return false;
        }
    }

    public void Cleanup()
    {
        try
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
        catch { }
        
        _encryption?.Dispose();
    }

    public UserAccount? FindUser(UserStore store, string userName)
    {
        return store.Users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));
    }

    public bool AddUser(UserStore store, string userName)
    {
        if (FindUser(store, userName) != null) return false;
        store.Users.Add(new UserAccount
        {
            UserName = userName,
            PasswordHash = string.Empty,
            IsBlocked = false,
            EnforcePasswordConstraints = false
        });
        return true;
    }
}



