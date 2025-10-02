using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UserHash;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private UserRepository? _repo;
    private UserStore? _store;
    private UserAccount? _currentUser;

    public MainWindow()
    {
        InitializeComponent();
        MenuAbout.Click += (_, __) => ShowAbout();
        MenuExit.Click += (_, __) => Application.Current.Shutdown();
        MenuChangePassword.Click += (_, __) => OnChangePassword();
        MenuManageUsers.Click += (_, __) => OnManageUsers();
    }

    protected override void OnContentRendered(System.EventArgs e)
    {
        base.OnContentRendered(e);
        _currentUser = DataContext as UserAccount;
        StatusText.Text = _currentUser != null ? $"Пользователь: {_currentUser.UserName}" : "Не вошли";
        MenuAdmin.Visibility = (_currentUser != null && string.Equals(_currentUser.UserName, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
            ? Visibility.Visible : Visibility.Collapsed;
    }

    public void InitializeContext(UserRepository repo, UserStore store, UserAccount current)
    {
        _repo = repo;
        _store = store;
        _currentUser = current;
        DataContext = current;
        StatusText.Text = $"Пользователь: {current.UserName}";
        MenuAdmin.Visibility = string.Equals(current.UserName, "ADMIN", System.StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnChangePassword()
    {
        if (_currentUser == null || _repo == null || _store == null) return;
        var dlg = new ChangePasswordWindow(requireOldPassword: true, enforcePolicy: _currentUser.EnforcePasswordConstraints)
        {
            Owner = this
        };
        if (dlg.ShowDialog() == true)
        {
            var oldHash = PasswordHasher.ComputeSha256(dlg.OldPassword);
            if (!string.Equals(oldHash, _currentUser.PasswordHash, System.StringComparison.Ordinal))
            {
                MessageBox.Show("Неверный старый пароль.", "Смена пароля", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _currentUser.PasswordHash = PasswordHasher.ComputeSha256(dlg.NewPassword);
            if (!_repo.Save(_store))
            {
                MessageBox.Show("Ошибка сохранения данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Пароль успешно изменен.", "Смена пароля", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "Автор: <укажите ФИО>\nИндивидуальное задание: Режимы ADMIN/пользователь, управление пользователями, ограничения паролей.",
            "О программе",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void OnManageUsers()
    {
        if (_repo == null || _store == null) return;
        var wnd = new ManageUsersWindow(_repo, _store)
        {
            Owner = this
        };
        wnd.ShowDialog();
    }
}