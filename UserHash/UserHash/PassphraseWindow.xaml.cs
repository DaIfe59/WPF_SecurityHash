using System.Windows;

namespace UserHash;

public partial class PassphraseWindow : Window
{
    public string Passphrase { get; private set; } = string.Empty;

    public PassphraseWindow()
    {
        InitializeComponent();
        OkButton.Click += (_, __) => OnOk();
        CancelButton.Click += (_, __) => { DialogResult = false; Close(); };
        ResetButton.Click += (_, __) => OnReset();
        PassphraseBox.KeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Enter) OnOk(); };
    }

    private void OnOk()
    {
        var passphrase = PassphraseBox.Password;
        if (string.IsNullOrWhiteSpace(passphrase))
        {
            InfoText.Text = "Введите парольную фразу.";
            return;
        }

        Passphrase = passphrase;
        DialogResult = true;
        Close();
    }

    private void OnReset()
    {
        var result = MessageBox.Show(
            "Это удалит все данные пользователей и создаст новый файл. Продолжить?",
            "Сброс данных",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var encryptedPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "users.dat");
                if (System.IO.File.Exists(encryptedPath))
                {
                    System.IO.File.Delete(encryptedPath);
                }
                MessageBox.Show("Файл данных удален. Перезапустите программу.", "Сброс", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = false;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка удаления файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

