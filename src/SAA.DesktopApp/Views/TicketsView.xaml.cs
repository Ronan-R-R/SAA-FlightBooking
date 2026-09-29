using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class TicketsView : UserControl
{
    private readonly FlightBookingRepository _repository;
    private static readonly List<LookupItem> ClassChoices = EditField.Choices("Economy", "Business", "First");

    public TicketsView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await AsyncState.RunAsync(Loading, Grid, Empty, Error, ErrorText, async () =>
        {
            var data = await _repository.GetTicketsAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var has = Grid.SelectedItem is Ticket;
        EditButton.IsEnabled = has;
        DeleteButton.IsEnabled = has;
    }

    private static List<EditField> Fields(Ticket t, IReadOnlyList<LookupItem> bookings,
        IReadOnlyList<LookupItem> passengers, IReadOnlyList<LookupItem> flights) => new()
    {
        new EditField { Label = "Booking", Kind = FieldKind.Combo, Options = bookings, SelectedId = t.BookingId == 0 ? null : t.BookingId },
        new EditField { Label = "Passenger (must be a traveller on the booking)", Kind = FieldKind.Combo, Options = passengers, SelectedId = t.PassengerId == 0 ? null : t.PassengerId },
        new EditField { Label = "Flight", Kind = FieldKind.Combo, Options = flights, SelectedId = t.FlightId == 0 ? null : t.FlightId },
        new EditField { Label = "Seat number", Value = t.SeatNumber },
        new EditField { Label = "Class", Kind = FieldKind.Combo, Options = ClassChoices, SelectedId = EditField.ChoiceId(ClassChoices, t.TicketClass) },
        new EditField { Label = "Fare", Kind = FieldKind.Decimal, Value = t.TicketId == 0 ? "" : t.Fare.ToString(CultureInfo.InvariantCulture) }
    };

    private static void Apply(Ticket t, List<EditField> f)
    {
        t.BookingId = f[0].SelectedId!.Value;
        t.PassengerId = f[1].SelectedId!.Value;
        t.FlightId = f[2].SelectedId!.Value;
        t.SeatNumber = f[3].Value;
        t.TicketClass = f[4].AsChoice();
        t.Fare = f[5].AsDecimal();
    }

    private async Task<(List<LookupItem> b, List<LookupItem> p, List<LookupItem> fl)> LookupsAsync()
    {
        var b = await _repository.GetBookingLookupAsync();
        var p = await _repository.GetPassengerLookupAsync();
        var fl = await _repository.GetFlightLookupAsync();
        return (b, p, fl);
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var (b, p, fl) = await LookupsAsync();
        var t = new Ticket();
        var fields = Fields(t, b, p, fl);
        if (new EditDialog("Issue ticket", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(t, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.InsertTicketAsync(t))) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Ticket selected) return;
        var (b, p, fl) = await LookupsAsync();
        var fields = Fields(selected, b, p, fl);
        if (new EditDialog("Edit ticket", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(selected, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.UpdateTicketAsync(selected))) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Ticket selected) return;
        if (!CrudUx.Confirm($"Delete ticket #{selected.TicketId} ({selected.PassengerName}, seat {selected.SeatNumber})? This cannot be undone.")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.DeleteTicketAsync(selected.TicketId))) await LoadAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
