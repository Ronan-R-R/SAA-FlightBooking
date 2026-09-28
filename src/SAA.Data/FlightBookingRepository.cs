using Microsoft.Data.SqlClient;

namespace SAA.Data;

/// <summary>
/// ADO.NET data access for the SAA Flight Booking database.
/// All queries are parameterised where they take input.
/// </summary>
public sealed class FlightBookingRepository
{
    private readonly string _connectionString;

    public FlightBookingRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A connection string is required.", nameof(connectionString));
        _connectionString = connectionString;
    }

    /// <summary>Opens a connection and returns the server version, proving connectivity.</summary>
    public async Task<string> TestConnectionAsync(CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        return conn.ServerVersion;
    }

    public async Task<IReadOnlyList<Passenger>> GetPassengersAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT PassengerId, FirstName, LastName, PassportNumber, Email, Phone, DateOfBirth
            FROM   dbo.Passengers
            ORDER BY LastName, FirstName;";

        var list = new List<Passenger>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new Passenger
            {
                PassengerId = r.GetInt32(0),
                FirstName = r.GetString(1),
                LastName = r.GetString(2),
                PassportNumber = r.GetString(3),
                Email = r.GetString(4),
                Phone = r.IsDBNull(5) ? null : r.GetString(5),
                DateOfBirth = r.IsDBNull(6) ? null : r.GetDateTime(6)
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<Flight>> GetFlightsAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT f.FlightId, f.FlightNumber,
                   dep.City, dep.IataCode, arr.City, arr.IataCode,
                   f.DepartureTime, f.ArrivalTime, f.Aircraft, f.SeatCapacity, f.BaseFare
            FROM   dbo.Flights f
            JOIN   dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
            JOIN   dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
            ORDER BY f.DepartureTime;";

        var list = new List<Flight>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new Flight
            {
                FlightId = r.GetInt32(0),
                FlightNumber = r.GetString(1),
                DepartureCity = r.GetString(2),
                DepartureCode = r.GetString(3),
                ArrivalCity = r.GetString(4),
                ArrivalCode = r.GetString(5),
                DepartureTime = r.GetDateTime(6),
                ArrivalTime = r.GetDateTime(7),
                Aircraft = r.IsDBNull(8) ? null : r.GetString(8),
                SeatCapacity = r.GetInt32(9),
                BaseFare = r.GetDecimal(10)
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<BookingOverview>> GetBookingOverviewAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT b.BookingReference,
                   p.FirstName + ' ' + p.LastName AS BookedBy,
                   b.Status,
                   pay.Amount,
                   pay.Status AS PaymentStatus
            FROM   dbo.Bookings b
            JOIN   dbo.Passengers p ON p.PassengerId = b.PassengerId
            LEFT JOIN dbo.Payments pay ON pay.BookingId = b.BookingId
            ORDER BY b.BookingDate;";

        var list = new List<BookingOverview>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new BookingOverview
            {
                BookingReference = r.GetString(0),
                BookedBy = r.GetString(1),
                Status = r.GetString(2),
                Amount = r.IsDBNull(3) ? null : r.GetDecimal(3),
                PaymentStatus = r.IsDBNull(4) ? null : r.GetString(4)
            });
        }
        return list;
    }

    /// <summary>Flights filtered by departure city (parameterised).</summary>
    public async Task<IReadOnlyList<Flight>> GetFlightsDepartingFromAsync(string city, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT f.FlightId, f.FlightNumber,
                   dep.City, dep.IataCode, arr.City, arr.IataCode,
                   f.DepartureTime, f.ArrivalTime, f.Aircraft, f.SeatCapacity, f.BaseFare
            FROM   dbo.Flights f
            JOIN   dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
            JOIN   dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
            WHERE  dep.City = @city
            ORDER BY f.DepartureTime;";

        var list = new List<Flight>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@city", System.Data.SqlDbType.NVarChar, 100).Value = city;
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new Flight
            {
                FlightId = r.GetInt32(0),
                FlightNumber = r.GetString(1),
                DepartureCity = r.GetString(2),
                DepartureCode = r.GetString(3),
                ArrivalCity = r.GetString(4),
                ArrivalCode = r.GetString(5),
                DepartureTime = r.GetDateTime(6),
                ArrivalTime = r.GetDateTime(7),
                Aircraft = r.IsDBNull(8) ? null : r.GetString(8),
                SeatCapacity = r.GetInt32(9),
                BaseFare = r.GetDecimal(10)
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<FlightRevenue>> GetRevenuePerFlightAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT f.FlightNumber,
                   dep.City + ' -> ' + arr.City AS Route,
                   COUNT(t.TicketId) AS TicketsIssued,
                   ISNULL(SUM(t.Fare), 0) AS Revenue
            FROM   dbo.Flights f
            JOIN   dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
            JOIN   dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
            LEFT JOIN dbo.Tickets t ON t.FlightId = f.FlightId
            GROUP BY f.FlightNumber, dep.City, arr.City
            ORDER BY Revenue DESC;";

        var list = new List<FlightRevenue>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            list.Add(new FlightRevenue
            {
                FlightNumber = r.GetString(0),
                Route = r.GetString(1),
                TicketsIssued = r.GetInt32(2),
                Revenue = r.GetDecimal(3)
            });
        }
        return list;
    }
}
