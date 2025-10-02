using System;
using System.Windows;

namespace UserHash;

public partial class LoginWindow : Window
{
    private readonly UserRepository _repo;
    private UserStore _store;
    private int _failedAttempts = 0;

    public UserAccount? LoggedInUser { get; private set; }

    public LoginWindow(UserRepository repo, UserStore store)
    {
        InitializeComponent();
        _repo = repo;
        _store = store;
        LoginButton.Click += (_, __) => AttemptLogin();
        CancelButton.Click += (_, __) => { DialogResult = false; Close(); };
    }

    private void AttemptLogin()
    {
        var userName = UserNameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(userName))
        {
            InfoText.Text = "Введите имя пользователя.";
            return;
        }

        var user = _repo.FindUser(_store, userName);
        if (user == null)
        {
            InfoText.Text = "Пользователь не найден. Повторите ввод или выйдите.";
            return;
        }

        if (user.IsBlocked)
        {
            MessageBox.Show("Учетная запись заблокирована.", "Вход", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // First login: empty password requires immediate change
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            var change = new ChangePasswordWindow(requireOldPassword: false, enforcePolicy: user.EnforcePasswordConstraints);
            var result = change.ShowDialog();
            if (result != true)
            {
                // per spec: allow exit when first login password set is cancelled
                DialogResult = false;
                Close();
                return;
            }
            var newHash = PasswordHasher.ComputeSha256(change.NewPassword);
            user.PasswordHash = newHash;
            if (!_repo.Save(_store))
        {
            MessageBox.Show("Ошибка сохранения данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
            LoggedInUser = user;
            DialogResult = true;
            Close();
            return;
        }

        var enteredHash = PasswordHasher.ComputeSha256(password);
        if (!string.Equals(enteredHash, user.PasswordHash, StringComparison.Ordinal))
        {
            _failedAttempts++;
            if (_failedAttempts >= 3)
            {
                MessageBox.Show("Три неверных попытки. Программа будет закрыта.", "Вход", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
                return;
            }
            InfoText.Text = $"Неверный пароль. Осталось попыток: {3 - _failedAttempts}";
            PasswordBox.Password = string.Empty;
            return;
        }

        LoggedInUser = user;
        DialogResult = true;
        Close();
    }
}



