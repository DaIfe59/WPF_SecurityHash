using System;
using System.Collections.Generic;

namespace UserHash;

public class UserAccount
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // empty means no password set yet
    public bool IsBlocked { get; set; }
    public bool EnforcePasswordConstraints { get; set; }
}

public class UserStore
{
    public List<UserAccount> Users { get; set; } = new();
}



