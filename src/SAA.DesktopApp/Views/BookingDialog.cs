using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SAA.Data;

namespace SAA.DesktopApp.Views;

/// <summary>
/// Customer booking dialog: pick a class and payment method, add one or more
/// travellers, then confirm. Confirming runs the transactional purchase.
/// </summary>
public sealed class BookingDialog : Window
{
    private readonly FlightBookingRepository _repository;
    private readonly Flight _flight;
    private readonly ObservableCollection<PassengerInput> _travellers = new();
    private readonly ComboBox _classCombo;
    private readonly ComboBox _methodCombo;
    private readonly TextBlock _total;
    private readonly Button _confirm;
    private readonly Button _removeButton;
    private readonly DataGrid _grid;

    public BookingResult? Result { get; private set; }

    public BookingDialog(FlightBookingRepository repository, Flight flight)
    {
        _repository = repository;
        _flight = flight;

        Title = $"Book {flight.FlightNumber}";
        Width = 560;
        SizeToContent = SizeToContent.Height;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF6, 0xF9));
        FontFamily = new FontFamily("Segoe UI");

        var root = new StackPanel { Margin = new Thickness(22, 18, 22, 18) };

        root.Children.Add(new TextBlock
        {
            Text = $"{flight.DepartureCity} to {flight.ArrivalCity}",
            FontFamily = new FontFamily("Segoe UI Semibold"),
            FontSize = 18,
            Foreground = new SolidColorBrush(Color.FromRgb(0x1B, 0x27, 0x33))
        });
        root.Children.Add(new TextBlock
        {
            Text = $"{flight.FlightNumber}  -  {flight.DepartureTime:ddd dd MMM HH:mm} to {flight.ArrivalTime:HH:mm}",
            Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x6B, 0x7B)),
            FontSize = 12,
            Margin = new Thickness(0, 2, 0, 0)
        });

        var options = new Grid { Margin = new Thickness(0, 14, 0, 0) };
        options.ColumnDefinitions.Add(new ColumnDefinition());
        options.ColumnDefinitions.Add(new ColumnDefinition());
        var classPanel = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
        classPanel.Children.Add(new TextBlock { Text = "Cabin class", Style = (Style)FindResource("FieldLabel") });
        _classCombo = new ComboBox { Style = (Style)FindResource("FieldCombo"), ItemsSource = new[] { "Economy", "Business", "First" }, SelectedIndex = 0 };
        _classCombo.SelectionChanged += (_, _) => UpdateTotal();
        classPanel.Children.Add(_classCombo);
        Grid.SetColumn(classPanel, 0);
        options.Children.Add(classPanel);

        var methodPanel = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
        methodPanel.Children.Add(new TextBlock { Text = "Payment method", Style = (Style)FindResource("FieldLabel") });
        _methodCombo = new ComboBox { Style = (Style)FindResource("FieldCombo"), ItemsSource = new[] { "Card", "EFT", "Cash" }, SelectedIndex = 0 };
        methodPanel.Children.Add(_methodCombo);
        Grid.SetColumn(methodPanel, 1);
        options.Children.Add(methodPanel);
        root.Children.Add(options);

        root.Children.Add(new TextBlock { Text = "Travellers", Style = (Style)FindResource("FieldLabel") });
        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            SelectionMode = DataGridSelectionMode.Single,
            Height = 140,
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xE1, 0xE7, 0xEE)),
            BorderThickness = new Thickness(1),
            ItemsSource = _travellers
        };
        _grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("FullName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        _grid.Columns.Add(new DataGridTextColumn { Header = "Passport", Binding = new System.Windows.Data.Binding("PassportNumber"), Width = 130 });
        root.Children.Add(_grid);

        var travellerButtons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        var addButton = new Button { Content = "Add traveller", Style = (Style)FindResource("SecondaryButton"), Margin = new Thickness(0, 0, 8, 0) };
        addButton.Click += Add_Click;
        _removeButton = new Button { Content = "Remove", Style = (Style)FindResource("DangerButton"), IsEnabled = false };
        _removeButton.Click += Remove_Click;
        _grid.SelectionChanged += (_, _) => _removeButton.IsEnabled = _grid.SelectedItem is PassengerInput;
        travellerButtons.Children.Add(addButton);
        travellerButtons.Children.Add(_removeButton);
        root.Children.Add(travellerButtons);

        var totalRow = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xF3, 0xF9)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14, 10, 14, 10),
            Margin = new Thickness(0, 16, 0, 0)
        };
        _total = new TextBlock { FontFamily = new FontFamily("Segoe UI Semibold"), FontSize = 15, Foreground = new SolidColorBrush(Color.FromRgb(0x0A, 0x25, 0x40)) };
        totalRow.Child = _total;
        root.Children.Add(totalRow);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 18, 0, 0) };
        var cancel = new Button { Content = "Cancel", Style = (Style)FindResource("SecondaryButton"), Margin = new Thickness(0, 0, 8, 0), MinWidth = 90, IsCancel = true };
        _confirm = new Button { Content = "Pay and confirm", MinWidth = 140 };
        _confirm.Click += Confirm_Click;
        buttons.Children.Add(cancel);
        buttons.Children.Add(_confirm);
        root.Children.Add(buttons);

        Content = root;
        UpdateTotal();
    }

    private decimal FarePerTicket() => (string)_classCombo.SelectedItem switch
    {
        "Business" => _flight.BaseFare * 2m,
        "First" => _flight.BaseFare * 3m,
        _ => _flight.BaseFare
    };

    private void UpdateTotal()
    {
        var fare = FarePerTicket();
        var count = _travellers.Count;
        _total.Text = count == 0
            ? $"Fare per traveller: R {fare:N2}  -  add a traveller to continue"
            : $"{count} traveller(s) x R {fare:N2}  =  R {fare * count:N2}";
        if (_confirm is not null) _confirm.IsEnabled = count > 0;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var fields = new List<EditField>
        {
            new() { Label = "First name" },
            new() { Label = "Last name" },
            new() { Label = "Passport number" },
            new() { Label = "Email" },
            new() { Label = "Phone (optional)", Required = false },
            new() { Label = "Date of birth (optional)", Kind = FieldKind.Date, Required = false }
        };
        if (new EditDialog("Add traveller", fields) { Owner = this }.ShowDialog() != true) return;

        _travellers.Add(new PassengerInput
        {
            FirstName = fields[0].Value,
            LastName = fields[1].Value,
            PassportNumber = fields[2].Value,
            Email = fields[3].Value,
            Phone = fields[4].AsTextOrNull(),
            DateOfBirth = fields[5].AsDateTimeOrNull()
        });
        UpdateTotal();
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (_grid.SelectedItem is PassengerInput p)
        {
            _travellers.Remove(p);
            UpdateTotal();
        }
    }

    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (_travellers.Count == 0) return;

        var request = new BookingRequest
        {
            FlightId = _flight.FlightId,
            TicketClass = (string)_classCombo.SelectedItem,
            PaymentMethod = (string)_methodCombo.SelectedItem,
            FarePerTicket = FarePerTicket(),
            Passengers = _travellers.ToList()
        };

        _confirm.IsEnabled = false;
        try
        {
            Result = await _repository.CreateWebBookingAsync(request);
            DialogResult = true;
        }
        catch (DataException ex)
        {
            MessageBox.Show(ex.Message, "Booking could not be completed", MessageBoxButton.OK, MessageBoxImage.Warning);
            _confirm.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            _confirm.IsEnabled = true;
        }
    }
}
