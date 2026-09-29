using System.Globalization;
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
            var data = await _repository.GetBookingsAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var has = Grid.SelectedItem is Booking;
        EditButton.IsEnabled = has;
        DeleteButton.IsEnabled = has;
        TravellersButton.IsEnabled = has;
    }

    private static readonly List<LookupItem> StatusChoices = EditField.Choices("Pending", "Confirmed", "Cancelled");

    private static List<EditField> Fields(Booking b, IReadOnlyList<LookupItem> passengers) => new()
    {
        new EditField { Label = "Booking reference", Value = b.BookingReference },
        new EditField { Label = "Booked by (passenger)", Kind = FieldKind.Combo, Options = passengers, SelectedId = b.PassengerId == 0 ? null : b.PassengerId },
        new EditField { Label = "Booking date", Kind = FieldKind.Date, Value = b.BookingId == 0 ? DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : b.BookingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
        new EditField { Label = "Status", Kind = FieldKind.Combo, Options = StatusChoices, SelectedId = EditField.ChoiceId(StatusChoices, b.Status) }
    };

    private static void Apply(Booking b, List<EditField> f)
    {
        b.BookingReference = f[0].Value;
        b.PassengerId = f[1].SelectedId!.Value;
        b.BookingDate = f[2].AsDateTime();
        b.Status = f[3].AsChoice();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var passengers = await _repository.GetPassengerLookupAsync();
        var b = new Booking();
        var fields = Fields(b, passengers);
        if (new EditDialog("Add booking", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(b, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.InsertBookingAsync(b))) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Booking selected) return;
        var passengers = await _repository.GetPassengerLookupAsync();
        var fields = Fields(selected, passengers);
        if (new EditDialog("Edit booking", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(selected, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.UpdateBookingAsync(selected))) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Booking selected) return;
        if (!CrudUx.Confirm($"Delete booking \"{selected.BookingReference}\"? This cannot be undone.")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.DeleteBookingAsync(selected.BookingId))) await LoadAsync();
    }

    private void Travellers_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Booking selected) return;
        new TravellersDialog(_repository, selected) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
