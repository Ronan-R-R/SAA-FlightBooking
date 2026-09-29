namespace SAA.Data;

public sealed class Airport
{
    public int AirportId { get; set; }
    public string IataCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
}

public sealed class Passenger
{
    public int PassengerId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PassportNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public sealed class Flight
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = "";
    public int DepartureAirportId { get; set; }
    public int ArrivalAirportId { get; set; }
    public string DepartureCity { get; set; } = "";
    public string DepartureCode { get; set; } = "";
    public string ArrivalCity { get; set; } = "";
    public string ArrivalCode { get; set; } = "";
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string? Aircraft { get; set; }
    public int SeatCapacity { get; set; }
    public decimal BaseFare { get; set; }

    public string Route => $"{DepartureCode} -> {ArrivalCode}";
}

public sealed class Booking
{
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = "";
    public int PassengerId { get; set; }
    public string PassengerName { get; set; } = "";
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = "Pending";
}

public sealed class Ticket
{
    public int TicketId { get; set; }
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = "";
    public int PassengerId { get; set; }
    public string PassengerName { get; set; } = "";
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = "";
    public string SeatNumber { get; set; } = "";
    public string TicketClass { get; set; } = "Economy";
    public decimal Fare { get; set; }
    public DateTime IssuedDate { get; set; }
}

public sealed class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public string BookingReference { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Method { get; set; } = "Card";
    public string Status { get; set; } = "Pending";
}

/// <summary>Id + display text pair for foreign-key dropdowns.</summary>
public sealed class LookupItem
{
    public int Id { get; set; }
    public string Display { get; set; } = "";
    public override string ToString() => Display;
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

/// <summary>One traveller captured in the customer booking flow.</summary>
public sealed class PassengerInput
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PassportNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>A complete customer purchase: one flight, one or more travellers, one payment.</summary>
public sealed class BookingRequest
{
    public int FlightId { get; set; }
    public string TicketClass { get; set; } = "Economy";
    public string PaymentMethod { get; set; } = "Card";
    public decimal FarePerTicket { get; set; }
    public List<PassengerInput> Passengers { get; set; } = new();
}

/// <summary>Outcome of a confirmed customer booking.</summary>
public sealed class BookingResult
{
    public string BookingReference { get; init; } = "";
    public int TicketCount { get; init; }
    public decimal AmountPaid { get; init; }
    public IReadOnlyList<string> Seats { get; init; } = Array.Empty<string>();
}

/// <summary>Raised for database constraint violations, carrying a user-friendly message.</summary>
public sealed class DataException : Exception
{
    public DataException(string message) : base(message) { }
}
