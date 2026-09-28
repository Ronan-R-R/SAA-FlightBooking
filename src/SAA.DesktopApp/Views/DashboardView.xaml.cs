using System.Windows;
using System.Windows.Controls;
using SAA.Data;

namespace SAA.DesktopApp.Views;

public partial class DashboardView : UserControl
{
    private readonly FlightBookingRepository _repository;

    public DashboardView(FlightBookingRepository repository)
    {
        InitializeComponent();
        _repository = repository;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Loading.Visibility = Visibility.Visible;
        Error.Visibility = Visibility.Collapsed;
        try
        {
            var passengers = await _repository.GetPassengersAsync();
            var flights = await _repository.GetFlightsAsync();
            var bookings = await _repository.GetBookingOverviewAsync();
            var revenue = await _repository.GetRevenuePerFlightAsync();

            PassengersCount.Text = passengers.Count.ToString();
            FlightsCount.Text = flights.Count.ToString();
            BookingsCount.Text = bookings.Count.ToString();
            RevenueTotal.Text = revenue.Sum(r => r.Revenue).ToString("N2");

            Loading.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Loading.Visibility = Visibility.Collapsed;
            ErrorText.Text = ex.Message;
            Error.Visibility = Visibility.Visible;
        }
    }
}
