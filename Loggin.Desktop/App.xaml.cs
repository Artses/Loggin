using System.Windows;
using Loggin.Desktop.Services;

namespace Loggin.Desktop;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            LocalLog.Error("Falha não tratada na interface.", args.Exception);
            System.Windows.MessageBox.Show("O Loggin encontrou um erro inesperado. Consulte os logs locais.", "Loggin",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}
