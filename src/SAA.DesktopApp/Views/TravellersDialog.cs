using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SAA.Data;

namespace SAA.DesktopApp.Views;

/// <summary>Manages the passengers linked to a booking (BookingPassengers).</summary>
public sealed class TravellersDialog : Window
{
    private readonly FlightBookingRepository _repository;
    private readonly Booking _booking;
    private readonly DataGrid _grid;
    private readonly Button _removeButton;

    public TravellersDialog(FlightBookingRepository repository, Booking booking)
    {
        _repository = repository;
        _booking = booking;

        Title = $"Travellers - {booking.BookingReference}";
        Width = 520;
        Height = 460;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF6, 0xF9));
        FontFamily = new FontFamily("Segoe UI");

        var root = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var heading = new TextBlock
        {
            Text = "Travellers on this booking",
            FontFamily = new FontFamily("Segoe UI Semibold"),
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0x1B, 0x27, 0x33))
        };
        Grid.SetRow(heading, 0);
        root.Children.Add(heading);

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 12) };
        var addButton = new Button { Content = "Add traveller", MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };
        addButton.Click += Add_Click;
        _removeButton = new Button { Content = "Remove", Style = (Style)FindResource("DangerButton"), MinWidth = 90, IsEnabled = false };
        _removeButton.Click += Remove_Click;
        toolbar.Children.Add(addButton);
        toolbar.Children.Add(_removeButton);
        Grid.SetRow(toolbar, 1);
        root.Children.Add(toolbar);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            SelectionMode = DataGridSelectionMode.Single,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE1, 0xE7, 0xEE)),
            BorderThickness = new Thickness(1)
        };
        _grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("FullName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        _grid.Columns.Add(new DataGridTextColumn { Header = "Passport", Binding = new System.Windows.Data.Binding("PassportNumber"), Width = 140 });
        _grid.SelectionChanged += (_, _) => _removeButton.IsEnabled = _grid.SelectedItem is Passenger;
        Grid.SetRow(_grid, 2);
        root.Children.Add(_grid);

        var close = new Button { Content = "Close", Style = (Style)FindResource("SecondaryButton"), MinWidth = 90, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0), IsCancel = true };
        Grid.SetRow(close, 3);
        root.Children.Add(close);

        Content = root;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _grid.ItemsSource = await _repository.GetTravellersAsync(_booking.BookingId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Could not load travellers", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var passengers = await _repository.GetPassengerLookupAsync();
        var field = new EditField { Label = "Passenger", Kind = FieldKind.Combo, Options = passengers };
        var fields = new List<EditField> { field };
        if (new EditDialog("Add traveller", fields) { Owner = this }.ShowDialog() != true) return;
        if (await CrudUx.TrySaveAsync(() => _repository.AddTravellerAsync(_booking.BookingId, field.SelectedId!.Value)))
            await LoadAsync();
    }

    private async void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is not Passenger p) return;
        if (!CrudUx.Confirm($"Remove {p.FullName} from this booking?")) return;
        if (await CrudUx.TrySaveAsync(() => _repository.RemoveTravellerAsync(_booking.BookingId, p.PassengerId)))
            await LoadAsync();
    }
}
