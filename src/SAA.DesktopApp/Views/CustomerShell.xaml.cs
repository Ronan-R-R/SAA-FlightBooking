using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class CustomerShell : UserControl
{
    private readonly FlightBookingRepository _repository;
    private readonly Action _onLogout;

    public CustomerShell(FlightBookingRepository repository, Action onLogout)
    {
        InitializeComponent();
        _repository = repository;
        _onLogout = onLogout;
        Loaded += (_, _) => ShowHome();
    }

    private void ShowHome() => Host.Content = new CustomerHomeView(_repository, ShowResults);

    private void ShowResults(int fromId, int toId) =>
        Host.Content = new CustomerResultsView(_repository, fromId, toId, ShowHome, OnSelect);

    private void OnSelect(Flight flight)
    {
        var dialog = new BookingDialog(_repository, flight) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Result is not null)
            Host.Content = new CustomerConfirmationView(dialog.Result, flight, ShowHome);
    }

    private void Brand_Click(object sender, RoutedEventArgs e) => ShowHome();
    private void Logout_Click(object sender, RoutedEventArgs e) => _onLogout();
}
