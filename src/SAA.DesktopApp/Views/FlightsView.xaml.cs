using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class FlightsView : UserControl
{
    private readonly FlightBookingRepository _repository;

    public FlightsView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await AsyncState.RunAsync(Loading, Grid, Empty, Error, ErrorText, async () =>
        {
            var data = await _repository.GetFlightsAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
