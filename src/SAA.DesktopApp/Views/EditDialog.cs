using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public enum FieldKind { Text, Integer, Decimal, Date, DateTime, Combo }

/// <summary>One editable field in an <see cref="EditDialog"/>.</summary>
public sealed class EditField
{
    public string Label { get; init; } = "";
    public FieldKind Kind { get; init; } = FieldKind.Text;
    public bool Required { get; init; } = true;
    public string Value { get; set; } = "";
    public IReadOnlyList<LookupItem>? Options { get; init; }
    public int? SelectedId { get; set; }

    internal Control? Control;

    public int AsInt() => int.Parse(Value, CultureInfo.InvariantCulture);
    public decimal AsDecimal() => decimal.Parse(Value, CultureInfo.InvariantCulture);
    public DateTime AsDateTime() => DateTime.Parse(Value, CultureInfo.InvariantCulture);
    public DateTime? AsDateTimeOrNull() => string.IsNullOrWhiteSpace(Value) ? null : AsDateTime();
    public string? AsTextOrNull() => string.IsNullOrWhiteSpace(Value) ? null : Value.Trim();

    /// <summary>For a combo built from Choices(): the selected option's display text.</summary>
    public string AsChoice() => Options!.First(o => o.Id == SelectedId!.Value).Display;

    /// <summary>Builds fixed-value combo options (Id is positional, Display is the value).</summary>
    public static List<LookupItem> Choices(params string[] values)
    {
        var list = new List<LookupItem>(values.Length);
        for (var i = 0; i < values.Length; i++) list.Add(new LookupItem { Id = i, Display = values[i] });
        return list;
    }

    /// <summary>The Id of the option whose Display equals <paramref name="value"/>, or null.</summary>
    public static int? ChoiceId(IReadOnlyList<LookupItem> options, string value)
        => options.FirstOrDefault(o => o.Display == value)?.Id;
}

/// <summary>
/// Reusable modal form built from a list of <see cref="EditField"/>. Validates
/// required fields and numeric/date formats before returning DialogResult = true.
/// </summary>
public sealed class EditDialog : Window
{
    private readonly IReadOnlyList<EditField> _fields;
    private readonly TextBlock _errorText;

    public EditDialog(string title, IReadOnlyList<EditField> fields)
    {
        _fields = fields;
        Title = title;
        Width = 460;
        SizeToContent = SizeToContent.Height;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF6, 0xF9));
        FontFamily = new FontFamily("Segoe UI");

        var root = new StackPanel { Margin = new Thickness(22, 18, 22, 18) };

        var heading = new TextBlock { Text = title, FontFamily = new FontFamily("Segoe UI Semibold"), FontSize = 18 };
        heading.Foreground = new SolidColorBrush(Color.FromRgb(0x1B, 0x27, 0x33));
        root.Children.Add(heading);

        foreach (var f in _fields)
        {
            root.Children.Add(new TextBlock { Text = f.Label, Style = (Style)FindResource("FieldLabel") });
            f.Control = BuildControl(f);
            root.Children.Add(f.Control);
        }

        _errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 12, 0, 0),
            Visibility = Visibility.Collapsed
        };
        root.Children.Add(_errorText);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 18, 0, 0) };
        var cancel = new Button { Content = "Cancel", Style = (Style)FindResource("SecondaryButton"), Margin = new Thickness(0, 0, 8, 0), MinWidth = 90 };
        cancel.Click += (_, _) => { DialogResult = false; };
        var ok = new Button { Content = "Save", MinWidth = 90, IsDefault = true };
        ok.Click += OnSave;
        buttons.Children.Add(cancel);
        buttons.Children.Add(ok);
        root.Children.Add(buttons);

        Content = root;
    }

    private Control BuildControl(EditField f)
    {
        if (f.Kind == FieldKind.Combo)
        {
            var combo = new ComboBox { Style = (Style)FindResource("FieldCombo"), ItemsSource = f.Options };
            if (f.SelectedId.HasValue && f.Options is not null)
                combo.SelectedItem = f.Options.FirstOrDefault(o => o.Id == f.SelectedId.Value);
            return combo;
        }

        if (f.Kind == FieldKind.Date)
        {
            var picker = new DatePicker { Style = (Style)FindResource("FieldDate") };
            if (DateTime.TryParse(f.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                picker.SelectedDate = d;
            return picker;
        }

        var box = new TextBox { Style = (Style)FindResource("Field"), Text = f.Value };
        return box;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        foreach (var f in _fields)
        {
            switch (f.Kind)
            {
                case FieldKind.Combo:
                    var item = (LookupItem?)((ComboBox)f.Control!).SelectedItem;
                    if (f.Required && item is null) { Fail($"Please choose a value for \"{f.Label}\"."); return; }
                    f.SelectedId = item?.Id;
                    break;
                case FieldKind.Date:
                    var sd = ((DatePicker)f.Control!).SelectedDate;
                    if (f.Required && sd is null) { Fail($"Please choose a date for \"{f.Label}\"."); return; }
                    f.Value = sd?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "";
                    break;
                default:
                    var text = ((TextBox)f.Control!).Text.Trim();
                    if (f.Required && string.IsNullOrWhiteSpace(text)) { Fail($"\"{f.Label}\" is required."); return; }
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (f.Kind == FieldKind.Integer && !int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                        { Fail($"\"{f.Label}\" must be a whole number."); return; }
                        if (f.Kind == FieldKind.Decimal && !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                        { Fail($"\"{f.Label}\" must be a number (for example 1500.00)."); return; }
                        if (f.Kind == FieldKind.DateTime && !DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        { Fail($"\"{f.Label}\" must be a date and time (for example 2026-09-29 14:30)."); return; }
                    }
                    f.Value = text;
                    break;
            }
        }
        DialogResult = true;
    }

    private void Fail(string message)
    {
        _errorText.Text = message;
        _errorText.Visibility = Visibility.Visible;
    }
}
