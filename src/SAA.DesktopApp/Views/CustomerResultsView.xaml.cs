using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class CustomerResultsView : UserControl
{
    private readonly FlightBookingRepository _repository;
    private readonly int _fromId;
    private readonly int _toId;
    private readonly Action _onBack;
    private readonly Action<Flight> _onSelect;

    public CustomerResultsView(FlightBookingRepository repository, int fromId, int toId, Action onBack, Action<Flight> onSelect)
    {
        InitializeComponent();
        _repository = repository;
        _fromId = fromId;
        _toId = toId;
        _onBack = onBack;
        _onSelect = onSelect;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Loading.Visibility = Visibility.Visible;
        Results.Visibility = Visibility.Collapsed;
        Empty.Visibility = Visibility.Collapsed;
        Error.Visibility = Visibility.Collapsed;

        try
        {
            var all = await _repository.GetFlightsAsync();
            var matches = all.Where(f => f.DepartureAirportId == _fromId && f.ArrivalAirportId == _toId).ToList();

            RouteTitle.Text = matches.Count > 0
                ? $"{matches[0].DepartureCity} to {matches[0].ArrivalCity}"
                : "Flight results";

            Loading.Visibility = Visibility.Collapsed;
            if (matches.Count == 0)
            {
                Empty.Visibility = Visibility.Visible;
                return;
            }
            Results.ItemsSource = matches;
            Results.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            Loading.Visibility = Visibility.Collapsed;
            ErrorText.Text = ex.Message;
            Error.Visibility = Visibility.Visible;
        }
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: Flight flight })
            _onSelect(flight);
    }

    private void Back_Click(object sender, RoutedEventArgs e) => _onBack();
}
