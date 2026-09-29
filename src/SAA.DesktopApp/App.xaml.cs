using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SAA.DesktopApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandledException;
        base.OnStartup(e);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            var log = Path.Combine(Path.GetTempPath(), "saa_desktop_error.log");
            File.WriteAllText(log, e.Exception.ToString());
        }
        catch { /* logging must never crash the handler */ }

        MessageBox.Show(e.Exception.Message, "SAA Flight Booking - Unexpected error",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
