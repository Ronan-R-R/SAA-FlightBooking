using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class CustomerHomeView : UserControl
{
    private readonly FlightBookingRepository _repository;
    private readonly Action<int, int> _onSearch;

    public CustomerHomeView(FlightBookingRepository repository, Action<int, int> onSearch)
    {
        InitializeComponent();
        _repository = repository;
        _onSearch = onSearch;
        Loaded += async (_, _) => await LoadAirportsAsync();
    }

    private async Task LoadAirportsAsync()
    {
        try
        {
            var airports = await _repository.GetAirportLookupAsync();
            FromCombo.ItemsSource = airports;
            ToCombo.ItemsSource = airports;
        }
        catch (Exception ex)
        {
            ShowMessage($"Could not load airports: {ex.Message}");
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        if (FromCombo.SelectedItem is not LookupItem from || ToCombo.SelectedItem is not LookupItem to)
        {
            ShowMessage("Please choose both a departure and a destination airport.");
            return;
        }
        if (from.Id == to.Id)
        {
            ShowMessage("Departure and destination must be different.");
            return;
        }
        _onSearch(from.Id, to.Id);
    }

    private void ShowMessage(string text)
    {
        Message.Text = text;
        Message.Visibility = Visibility.Visible;
    }
}
