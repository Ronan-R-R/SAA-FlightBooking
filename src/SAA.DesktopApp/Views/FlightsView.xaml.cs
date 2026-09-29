using System.Globalization;
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

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var has = Grid.SelectedItem is Flight;
        EditButton.IsEnabled = has;
        DeleteButton.IsEnabled = has;
    }

    private static string Dt(DateTime v) => v.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    private static List<EditField> Fields(Flight f, IReadOnlyList<LookupItem> airports) => new()
    {
        new EditField { Label = "Flight number", Value = f.FlightNumber },
        new EditField { Label = "Departure airport", Kind = FieldKind.Combo, Options = airports, SelectedId = f.DepartureAirportId == 0 ? null : f.DepartureAirportId },
        new EditField { Label = "Arrival airport", Kind = FieldKind.Combo, Options = airports, SelectedId = f.ArrivalAirportId == 0 ? null : f.ArrivalAirportId },
        new EditField { Label = "Departure time (yyyy-MM-dd HH:mm)", Kind = FieldKind.DateTime, Value = f.FlightId == 0 ? "" : Dt(f.DepartureTime) },
        new EditField { Label = "Arrival time (yyyy-MM-dd HH:mm)", Kind = FieldKind.DateTime, Value = f.FlightId == 0 ? "" : Dt(f.ArrivalTime) },
        new EditField { Label = "Aircraft (optional)", Value = f.Aircraft ?? "", Required = false },
        new EditField { Label = "Seat capacity", Kind = FieldKind.Integer, Value = f.FlightId == 0 ? "" : f.SeatCapacity.ToString(CultureInfo.InvariantCulture) },
        new EditField { Label = "Base fare", Kind = FieldKind.Decimal, Value = f.FlightId == 0 ? "" : f.BaseFare.ToString(CultureInfo.InvariantCulture) }
    };

    private static void Apply(Flight f, List<EditField> x)
    {
        f.FlightNumber = x[0].Value;
        f.DepartureAirportId = x[1].SelectedId!.Value;
        f.ArrivalAirportId = x[2].SelectedId!.Value;
        f.DepartureTime = x[3].AsDateTime();
        f.ArrivalTime = x[4].AsDateTime();
        f.Aircraft = x[5].AsTextOrNull();
        f.SeatCapacity = x[6].AsInt();
        f.BaseFare = x[7].AsDecimal();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var airports = await _repository.GetAirportLookupAsync();
        var f = new Flight();
        var fields = Fields(f, airports);
        if (new EditDialog("Add flight", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(f, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.InsertFlightAsync(f))) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Flight selected) return;
        var airports = await _repository.GetAirportLookupAsync();
        var fields = Fields(selected, airports);
        if (new EditDialog("Edit flight", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(selected, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.UpdateFlightAsync(selected))) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Flight selected) return;
        if (!CrudUx.Confirm($"Delete flight \"{selected.FlightNumber}\"? This cannot be undone.")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.DeleteFlightAsync(selected.FlightId))) await LoadAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
