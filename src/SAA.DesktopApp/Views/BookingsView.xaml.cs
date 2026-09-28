using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class BookingsView : UserControl
{
    private readonly FlightBookingRepository _repository;

    public BookingsView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await AsyncState.RunAsync(Loading, Grid, Empty, Error, ErrorText, async () =>
        {
            var data = await _repository.GetBookingOverviewAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
