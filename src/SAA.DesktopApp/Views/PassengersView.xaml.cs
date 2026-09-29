using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class PassengersView : UserControl
{
    private readonly FlightBookingRepository _repository;

    public PassengersView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        await AsyncState.RunAsync(Loading, Grid, Empty, Error, ErrorText, async () =>
        {
            var data = await _repository.GetPassengersAsync();
            Grid.ItemsSource = data;
            return data.Count > 0;
        });
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var has = Grid.SelectedItem is Passenger;
        EditButton.IsEnabled = has;
        DeleteButton.IsEnabled = has;
    }

    private static List<EditField> Fields(Passenger p) => new()
    {
        new EditField { Label = "First name", Value = p.FirstName },
        new EditField { Label = "Last name", Value = p.LastName },
        new EditField { Label = "Passport number", Value = p.PassportNumber },
        new EditField { Label = "Email", Value = p.Email },
        new EditField { Label = "Phone (optional)", Value = p.Phone ?? "", Required = false },
        new EditField { Label = "Date of birth (optional)", Kind = FieldKind.Date, Required = false,
                        Value = p.DateOfBirth?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "" }
    };

    private static void Apply(Passenger p, List<EditField> f)
    {
        p.FirstName = f[0].Value;
        p.LastName = f[1].Value;
        p.PassportNumber = f[2].Value;
        p.Email = f[3].Value;
        p.Phone = f[4].AsTextOrNull();
        p.DateOfBirth = f[5].AsDateTimeOrNull();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var p = new Passenger();
        var fields = Fields(p);
        if (new EditDialog("Add passenger", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(p, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.InsertPassengerAsync(p))) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Passenger selected) return;
        var fields = Fields(selected);
        if (new EditDialog("Edit passenger", fields) { Owner = Window.GetWindow(this) }.ShowDialog() != true) return;
        Apply(selected, fields);
        if (await CrudUx.TrySaveAsync(() => _repository.UpdatePassengerAsync(selected))) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is not Passenger selected) return;
        if (!CrudUx.Confirm($"Delete passenger \"{selected.FullName}\"? This cannot be undone.")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.DeletePassengerAsync(selected.PassengerId))) await LoadAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
