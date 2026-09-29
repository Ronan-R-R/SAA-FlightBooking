using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class PaymentsView : UserControl
{
    private readonly FlightBookingRepository _repository;
    private static readonly List<LookupItem> MethodChoices = EditField.Choices("Card", "EFT", "Cash");
    private static readonly List<LookupItem> StatusChoices = EditField.Choices("Pending", "Paid", "Refunded");

    public PaymentsView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await AsyncState.RunAsync(Loading, Grid, Empty, Error, ErrorText, async () =>
        {
            var data = await _repository.GetPaymentsAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var has = Grid.SelectedItem is Payment;
        EditButton.IsEnabled = has;
        DeleteButton.IsEnabled = has;
    }

    private static List<EditField> Fields(Payment p, IReadOnlyList<LookupItem> bookings) => new()
    {
        new EditField { Label = "Booking", Kind = FieldKind.Combo, Options = bookings, SelectedId = p.BookingId == 0 ? null : p.BookingId },
        new EditField { Label = "Amount", Kind = FieldKind.Decimal, Value = p.PaymentId == 0 ? "" : p.Amount.ToString(CultureInfo.InvariantCulture) },
        new EditField { Label = "Payment date", Kind = FieldKind.Date, Value = p.PaymentId == 0 ? DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : p.PaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
        new EditField { Label = "Method", Kind = FieldKind.Combo, Options = MethodChoices, SelectedId = EditField.ChoiceId(MethodChoices, p.Method) },
        new EditField { Label = "Status", Kind = FieldKind.Combo, Options = StatusChoices, SelectedId = EditField.ChoiceId(StatusChoices, p.Status) }
    };

    private static void Apply(Payment p, List<EditField> f)
    {
        p.BookingId = f[0].SelectedId!.Value;
        p.Amount = f[1].AsDecimal();
        p.PaymentDate = f[2].AsDateTime();
        p.Method = f[3].AsChoice();
        p.Status = f[4].AsChoice();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var bookings = await _repository.GetBookingLookupAsync();
        var p = new Payment();
        var fields = Fields(p, bookings);
        if (new EditDialog("Record payment", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(p, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.InsertPaymentAsync(p))) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Payment selected) return;
        var bookings = await _repository.GetBookingLookupAsync();
        var fields = Fields(selected, bookings);
        if (new EditDialog("Edit payment", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(selected, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.UpdatePaymentAsync(selected))) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Payment selected) return;
        if (!CrudUx.Confirm($"Delete payment #{selected.PaymentId} ({selected.BookingReference})? This cannot be undone.")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.DeletePaymentAsync(selected.PaymentId))) await LoadAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
