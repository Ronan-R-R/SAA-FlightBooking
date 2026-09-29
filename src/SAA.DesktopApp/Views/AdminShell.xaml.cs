using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class AdminShell : UserControl
{
    private readonly FlightBookingRepository _repository;
    private readonly Action _onLogout;
    private readonly Dictionary<string, UserControl> _views = new();

    public AdminShell(FlightBookingRepository repository, Action onLogout)
    {
        InitializeComponent();
        _repository = repository;
        _onLogout = onLogout;
        Loaded += async (_, _) =>
        {
            ShowView("Dashboard");
            await UpdateConnectionStatusAsync();
        };
    }

    private void Nav_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Nav.SelectedItem is ListBoxItem item && item.Content is string name)
            ShowView(name);
    }

    private void ShowView(string name)
    {
        if (ContentHost is null) return;

        if (!_views.TryGetValue(name, out var view))
        {
            view = name switch
            {
                "Dashboard" => new DashboardView(_repository),
                "Airports" => new AirportsView(_repository),
                "Passengers" => new PassengersView(_repository),
                "Flights" => new FlightsView(_repository),
                "Bookings" => new BookingsView(_repository),
                "Tickets" => new TicketsView(_repository),
                "Payments" => new PaymentsView(_repository),
                "Revenue Report" => new RevenueView(_repository),
                "About" => new AboutView(),
                _ => new DashboardView(_repository)
            };
            _views[name] = view;
        }
        ContentHost.Content = view;
    }

    private async Task UpdateConnectionStatusAsync()
    {
        try
        {
            var version = await _repository.TestConnectionAsync();
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x3F, 0xB6, 0x50));
            StatusText.Text = $"Connected (SQL {version.Split('.')[0]}.x)";
        }
        catch
        {
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xD9, 0x53, 0x4F));
            StatusText.Text = "Database offline";
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e) => _onLogout();
}
