using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UserHash;

public partial class ManageUsersWindow : Window
{
    private readonly UserRepository _repo;
    private readonly UserStore _store;

    public ManageUsersWindow(UserRepository repo, UserStore store)
    {
        InitializeComponent();
        _repo = repo;
        _store = store;
        UsersGrid.ItemsSource = _store.Users.OrderBy(u => u.UserName).ToList();

        AddUserBtn.Click += (_, __) => OnAddUser();
        BlockUserBtn.Click += (_, __) => OnToggleBlock();
        TogglePolicyBtn.Click += (_, __) => OnTogglePolicy();
    }

    private UserAccount? SelectedUser => UsersGrid.SelectedItem as UserAccount;

    private void Refresh()
    {
        UsersGrid.ItemsSource = null;
        UsersGrid.ItemsSource = _store.Users.OrderBy(u => u.UserName).ToList();
    }

    private void OnAddUser()
    {
        var name = Microsoft.VisualBasic.Interaction.InputBox("Введите имя нового пользователя:", "Добавить пользователя", "");
        if (string.IsNullOrWhiteSpace(name)) return;
        if (string.Equals(name, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Имя ADMIN зарезервировано.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!_repo.AddUser(_store, name))
        {
            MessageBox.Show("Пользователь с таким именем уже существует.", "Добавление", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!_repo.Save(_store))
        {
            MessageBox.Show("Ошибка сохранения данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Refresh();
    }

    private void OnToggleBlock()
    {
        var user = SelectedUser;
        if (user == null) return;
        if (string.Equals(user.UserName, "ADMIN", System.StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Нельзя блокировать ADMIN.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        user.IsBlocked = !user.IsBlocked;
        if (!_repo.Save(_store))
        {
            MessageBox.Show("Ошибка сохранения данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Refresh();
    }

    private void OnTogglePolicy()
    {
        var user = SelectedUser;
        if (user == null) return;
        user.EnforcePasswordConstraints = !user.EnforcePasswordConstraints;
        if (!_repo.Save(_store))
        {
            MessageBox.Show("Ошибка сохранения данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Refresh();
    }
}



