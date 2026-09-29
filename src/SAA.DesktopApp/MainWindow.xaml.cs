using System.Windows;
using SAA.Data;
using SAA.DesktopApp.Views;

namespace SAA.DesktopApp;

public partial class MainWindow : Window
{
    private readonly FlightBookingRepository _repository = new(DbConfig.Resolve());

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => ShowSelection();
    }

    private void ShowSelection() => RootHost.Content = new SelectionView(ShowCustomer, ShowAdmin);

    private void ShowCustomer() => RootHost.Content = new CustomerShell(_repository, ShowSelection);

    private void ShowAdmin() => RootHost.Content = new AdminShell(_repository, ShowSelection);
}
