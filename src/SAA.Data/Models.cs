namespace SAA.Data;

public sealed class Passenger
{
    public int PassengerId { get; init; }
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string PassportNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string? Phone { get; init; }
    public DateTime? DateOfBirth { get; init; }

    public string FullName => $"{FirstName} {LastName}";
}

public sealed class Flight
{
    public int FlightId { get; init; }
    public string FlightNumber { get; init; } = "";
    public string DepartureCity { get; init; } = "";
    public string DepartureCode { get; init; } = "";
    public string ArrivalCity { get; init; } = "";
    public string ArrivalCode { get; init; } = "";
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public string? Aircraft { get; init; }
    public int SeatCapacity { get; init; }
    public decimal BaseFare { get; init; }

    public string Route => $"{DepartureCode} -> {ArrivalCode}";
}

public sealed class BookingOverview
{
    public string BookingReference { get; init; } = "";
    public string BookedBy { get; init; } = "";
    public string Status { get; init; } = "";
    public decimal? Amount { get; init; }
    public string? PaymentStatus { get; init; }
}

public sealed class FlightRevenue
{
    public string FlightNumber { get; init; } = "";
    public string Route { get; init; } = "";
    public int TicketsIssued { get; init; }
    public decimal Revenue { get; init; }
}
