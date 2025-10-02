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
}

