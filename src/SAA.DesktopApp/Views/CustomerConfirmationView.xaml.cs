using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class CustomerConfirmationView : UserControl
{
    private readonly Action _onDone;

    public CustomerConfirmationView(BookingResult result, Flight flight, Action onDone)
    {
        InitializeComponent();
        _onDone = onDone;

        Subtitle.Text = $"Thank you. {result.TicketCount} ticket(s) issued on flight {flight.FlightNumber}.";
        Reference.Text = result.BookingReference;
        Route.Text = $"{flight.DepartureCity} to {flight.ArrivalCity}";
        Seats.Text = result.Seats.Count > 0 ? string.Join(", ", result.Seats) : "-";
        Amount.Text = $"R {result.AmountPaid:N2}";
    }

    private void Done_Click(object sender, RoutedEventArgs e) => _onDone();
}
