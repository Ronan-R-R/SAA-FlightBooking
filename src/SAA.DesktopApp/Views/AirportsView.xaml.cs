using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class AirportsView : UserControl
{
    private readonly FlightBookingRepository _repository;

    public AirportsView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await AsyncState.RunAsync(Loading, Grid, Empty, Error, ErrorText, async () =>
        {
            var data = await _repository.GetAirportsAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var has = Grid.SelectedItem is Airport;
        EditButton.IsEnabled = has;
        DeleteButton.IsEnabled = has;
    }

    private static List<EditField> Fields(Airport a) => new()
    {
        new EditField { Label = "IATA code (3 letters)", Value = a.IataCode },
        new EditField { Label = "Name", Value = a.Name },
        new EditField { Label = "City", Value = a.City },
        new EditField { Label = "Country", Value = a.Country }
    };

    private static void Apply(Airport a, List<EditField> f)
    {
        a.IataCode = f[0].Value;
        a.Name = f[1].Value;
        a.City = f[2].Value;
        a.Country = f[3].Value;
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var a = new Airport();
        var fields = Fields(a);
        if (new EditDialog("Add airport", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(a, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.InsertAirportAsync(a))) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Airport selected) return;
        var fields = Fields(selected);
        if (new EditDialog("Edit airport", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(selected, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.UpdateAirportAsync(selected))) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Airport selected) return;
        if (!CrudUx.Confirm($"Delete airport \"{selected.City} ({selected.IataCode})\"? This cannot be undone.")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.DeleteAirportAsync(selected.AirportId))) await LoadAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
