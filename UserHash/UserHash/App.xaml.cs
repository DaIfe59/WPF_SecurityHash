using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace UserHash;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.DispatcherUnhandledException += (s, e) =>
        {
            MessageBox.Show($"Ошибка: {e.Exception.Message}", "Необработанное исключение", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Shutdown();
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            // Get passphrase for file encryption
            var passphraseWindow = new PassphraseWindow();
            var passphraseResult = passphraseWindow.ShowDialog();
            if (passphraseResult != true)
            {
                MessageBox.Show("Ввод парольной фразы отменен.", "Приложение", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            var repo = new UserRepository();
            if (!repo.InitializeEncryption(passphraseWindow.Passphrase))
            {
                MessageBox.Show("Ошибка инициализации шифрования.", "Приложение", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var store = repo.LoadOrCreate();
            if (store == null)
            {
                MessageBox.Show("Неверная парольная фраза или поврежденный файл.", "Приложение", MessageBoxButton.OK, MessageBoxImage.Error);
                repo.Cleanup();
                Shutdown();
                return;
            }

            // Verify ADMIN exists (validates passphrase correctness)
            var admin = repo.FindUser(store, "ADMIN");
            if (admin == null)
            {
                MessageBox.Show("Файл данных поврежден или неверная парольная фраза.", "Приложение", MessageBoxButton.OK, MessageBoxImage.Error);
                repo.Cleanup();
                Shutdown();
                return;
            }

            var login = new LoginWindow(repo, store);
            var result = login.ShowDialog();
            if (result != true || login.LoggedInUser == null)
            {
                MessageBox.Show("Вход отменен или не выполнен.", "Приложение", MessageBoxButton.OK, MessageBoxImage.Information);
                repo.Cleanup();
                Shutdown();
                return;
            }

            var main = new MainWindow();
            main.InitializeContext(repo, store, login.LoggedInUser);
            main.Show();
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            // Cleanup on exit
            Exit += (_, __) => repo.Cleanup();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске: {ex.Message}", "Приложение", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}

