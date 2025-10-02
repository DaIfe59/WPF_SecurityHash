using System;
using System.Windows;

namespace UserHash;

public partial class ChangePasswordWindow : Window
{
    private readonly bool _requireOld;
    private readonly bool _enforcePolicy;

    public string NewPassword { get; private set; } = string.Empty;
    public string OldPassword { get; private set; } = string.Empty;

    public ChangePasswordWindow(bool requireOldPassword, bool enforcePolicy)
    {
        InitializeComponent();
        _requireOld = requireOldPassword;
        _enforcePolicy = enforcePolicy;

        if (!_requireOld)
        {
            OldPassLabel.Visibility = Visibility.Collapsed;
            OldPasswordBox.Visibility = Visibility.Collapsed;
        }

        OkButton.Click += (_, __) => OnOk();
        CancelButton.Click += (_, __) => { DialogResult = false; Close(); };
    }

    private void OnOk()
    {
        OldPassword = OldPasswordBox.Password;
        var newPass = NewPasswordBox.Password;
        var confirm = ConfirmPasswordBox.Password;

        if (string.IsNullOrEmpty(newPass))
        {
            InfoText.Text = "Введите новый пароль.";
            return;
        }

        if (newPass != confirm)
        {
            InfoText.Text = "Пароли не совпадают.";
            return;
        }

        if (_enforcePolicy && !PasswordPolicy.Validate(newPass))
        {
            InfoText.Text = "Пароль должен содержать латиницу, кириллицу и цифры.";
            return;
        }

        NewPassword = newPass;
        DialogResult = true;
        Close();
    }
}


