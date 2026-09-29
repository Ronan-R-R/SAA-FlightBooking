using Microsoft.Data.SqlClient;

namespace SAA.Data;

/// <summary>
/// Create, update and delete operations plus lookups for the management UI.
/// All statements are parameterised. SQL constraint violations are translated
/// into a DataException with a readable message.
/// </summary>
public sealed partial class FlightBookingRepository
{
    private static DataException Translate(SqlException ex) => ex.Number switch
    {
        547 => new DataException(
            "The operation breaks a database rule. A referenced record may be missing or out of range, "
            + "or this record is still used by other records (for example a booking that still has tickets or payments)."),
        2627 or 2601 => new DataException("A record with the same unique value already exists."),
        _ => new DataException("Database error: " + ex.Message)
    };

    private async Task<int> ExecAsync(string sql, Action<SqlCommand> bind, CancellationToken ct)
    {
        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            bind(cmd);
            return await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (SqlException ex) { throw Translate(ex); }
    }

    private async Task<List<T>> QueryAsync<T>(string sql, Func<SqlDataReader, T> map,
        Action<SqlCommand>? bind, CancellationToken ct)
    {
        var list = new List<T>();
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        bind?.Invoke(cmd);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct)) list.Add(map(r));
        return list;
    }

    private static object NullOr(object? v) => v ?? DBNull.Value;

    // ---------------- Airports ----------------
    public Task<List<Airport>> GetAirportsAsync(CancellationToken ct = default) => QueryAsync(
        "SELECT AirportId, IataCode, Name, City, Country FROM dbo.Airports ORDER BY City;",
        r => new Airport { AirportId = r.GetInt32(0), IataCode = r.GetString(1), Name = r.GetString(2), City = r.GetString(3), Country = r.GetString(4) },
        null, ct);

    public Task InsertAirportAsync(Airport a, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.Airports (IataCode, Name, City, Country) VALUES (@iata, @name, @city, @country);",
        c => { c.Parameters.AddWithValue("@iata", a.IataCode); c.Parameters.AddWithValue("@name", a.Name); c.Parameters.AddWithValue("@city", a.City); c.Parameters.AddWithValue("@country", a.Country); }, ct);

    public Task UpdateAirportAsync(Airport a, CancellationToken ct = default) => ExecAsync(
        "UPDATE dbo.Airports SET IataCode=@iata, Name=@name, City=@city, Country=@country WHERE AirportId=@id;",
        c => { c.Parameters.AddWithValue("@id", a.AirportId); c.Parameters.AddWithValue("@iata", a.IataCode); c.Parameters.AddWithValue("@name", a.Name); c.Parameters.AddWithValue("@city", a.City); c.Parameters.AddWithValue("@country", a.Country); }, ct);

    public Task DeleteAirportAsync(int id, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.Airports WHERE AirportId=@id;", c => c.Parameters.AddWithValue("@id", id), ct);

    // ---------------- Passengers ----------------
    public Task InsertPassengerAsync(Passenger p, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.Passengers (FirstName, LastName, PassportNumber, Email, Phone, DateOfBirth) VALUES (@f,@l,@pass,@e,@ph,@dob);",
        c => BindPassenger(c, p), ct);

    public Task UpdatePassengerAsync(Passenger p, CancellationToken ct = default) => ExecAsync(
        "UPDATE dbo.Passengers SET FirstName=@f, LastName=@l, PassportNumber=@pass, Email=@e, Phone=@ph, DateOfBirth=@dob WHERE PassengerId=@id;",
        c => { c.Parameters.AddWithValue("@id", p.PassengerId); BindPassenger(c, p); }, ct);

    public Task DeletePassengerAsync(int id, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.Passengers WHERE PassengerId=@id;", c => c.Parameters.AddWithValue("@id", id), ct);

    private static void BindPassenger(SqlCommand c, Passenger p)
    {
        c.Parameters.AddWithValue("@f", p.FirstName);
        c.Parameters.AddWithValue("@l", p.LastName);
        c.Parameters.AddWithValue("@pass", p.PassportNumber);
        c.Parameters.AddWithValue("@e", p.Email);
        c.Parameters.AddWithValue("@ph", NullOr(p.Phone));
        c.Parameters.AddWithValue("@dob", NullOr(p.DateOfBirth));
    }

    // ---------------- Flights ----------------
    public Task InsertFlightAsync(Flight f, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.Flights (FlightNumber, DepartureAirportId, ArrivalAirportId, DepartureTime, ArrivalTime, Aircraft, SeatCapacity, BaseFare) VALUES (@num,@dep,@arr,@dt,@at,@ac,@cap,@fare);",
        c => BindFlight(c, f), ct);

    public Task UpdateFlightAsync(Flight f, CancellationToken ct = default) => ExecAsync(
        "UPDATE dbo.Flights SET FlightNumber=@num, DepartureAirportId=@dep, ArrivalAirportId=@arr, DepartureTime=@dt, ArrivalTime=@at, Aircraft=@ac, SeatCapacity=@cap, BaseFare=@fare WHERE FlightId=@id;",
        c => { c.Parameters.AddWithValue("@id", f.FlightId); BindFlight(c, f); }, ct);

    public Task DeleteFlightAsync(int id, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.Flights WHERE FlightId=@id;", c => c.Parameters.AddWithValue("@id", id), ct);

    private static void BindFlight(SqlCommand c, Flight f)
    {
        c.Parameters.AddWithValue("@num", f.FlightNumber);
        c.Parameters.AddWithValue("@dep", f.DepartureAirportId);
        c.Parameters.AddWithValue("@arr", f.ArrivalAirportId);
        c.Parameters.AddWithValue("@dt", f.DepartureTime);
        c.Parameters.AddWithValue("@at", f.ArrivalTime);
        c.Parameters.AddWithValue("@ac", NullOr(f.Aircraft));
        c.Parameters.AddWithValue("@cap", f.SeatCapacity);
        c.Parameters.AddWithValue("@fare", f.BaseFare);
    }

    // ---------------- Bookings ----------------
    public Task<List<Booking>> GetBookingsAsync(CancellationToken ct = default) => QueryAsync(
        @"SELECT b.BookingId, b.BookingReference, b.PassengerId, p.FirstName + ' ' + p.LastName, b.BookingDate, b.Status
          FROM dbo.Bookings b JOIN dbo.Passengers p ON p.PassengerId = b.PassengerId
          ORDER BY b.BookingDate;",
        r => new Booking { BookingId = r.GetInt32(0), BookingReference = r.GetString(1), PassengerId = r.GetInt32(2), PassengerName = r.GetString(3), BookingDate = r.GetDateTime(4), Status = r.GetString(5) },
        null, ct);

    public Task InsertBookingAsync(Booking b, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.Bookings (BookingReference, PassengerId, BookingDate, Status) VALUES (@ref,@pid,@date,@status);",
        c => BindBooking(c, b), ct);

    public Task UpdateBookingAsync(Booking b, CancellationToken ct = default) => ExecAsync(
        "UPDATE dbo.Bookings SET BookingReference=@ref, PassengerId=@pid, BookingDate=@date, Status=@status WHERE BookingId=@id;",
        c => { c.Parameters.AddWithValue("@id", b.BookingId); BindBooking(c, b); }, ct);

    public Task DeleteBookingAsync(int id, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.Bookings WHERE BookingId=@id;", c => c.Parameters.AddWithValue("@id", id), ct);

    private static void BindBooking(SqlCommand c, Booking b)
    {
        c.Parameters.AddWithValue("@ref", b.BookingReference);
        c.Parameters.AddWithValue("@pid", b.PassengerId);
        c.Parameters.AddWithValue("@date", b.BookingDate);
        c.Parameters.AddWithValue("@status", b.Status);
    }

    // ---- Booking travellers (associative table) ----
    public Task<List<Passenger>> GetTravellersAsync(int bookingId, CancellationToken ct = default) => QueryAsync(
        @"SELECT p.PassengerId, p.FirstName, p.LastName, p.PassportNumber, p.Email, p.Phone, p.DateOfBirth
          FROM dbo.BookingPassengers bp JOIN dbo.Passengers p ON p.PassengerId = bp.PassengerId
          WHERE bp.BookingId=@bid ORDER BY p.LastName;",
        r => new Passenger { PassengerId = r.GetInt32(0), FirstName = r.GetString(1), LastName = r.GetString(2), PassportNumber = r.GetString(3), Email = r.GetString(4), Phone = r.IsDBNull(5) ? null : r.GetString(5), DateOfBirth = r.IsDBNull(6) ? null : r.GetDateTime(6) },
        c => c.Parameters.AddWithValue("@bid", bookingId), ct);

    public Task AddTravellerAsync(int bookingId, int passengerId, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.BookingPassengers (BookingId, PassengerId) VALUES (@bid,@pid);",
        c => { c.Parameters.AddWithValue("@bid", bookingId); c.Parameters.AddWithValue("@pid", passengerId); }, ct);

    public Task RemoveTravellerAsync(int bookingId, int passengerId, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.BookingPassengers WHERE BookingId=@bid AND PassengerId=@pid;",
        c => { c.Parameters.AddWithValue("@bid", bookingId); c.Parameters.AddWithValue("@pid", passengerId); }, ct);

    // ---------------- Tickets ----------------
    public Task<List<Ticket>> GetTicketsAsync(CancellationToken ct = default) => QueryAsync(
        @"SELECT t.TicketId, t.BookingId, b.BookingReference, t.PassengerId, p.FirstName + ' ' + p.LastName,
                 t.FlightId, f.FlightNumber, t.SeatNumber, t.TicketClass, t.Fare, t.IssuedDate
          FROM dbo.Tickets t
          JOIN dbo.Bookings b ON b.BookingId = t.BookingId
          JOIN dbo.Passengers p ON p.PassengerId = t.PassengerId
          JOIN dbo.Flights f ON f.FlightId = t.FlightId
          ORDER BY b.BookingReference, p.LastName;",
        r => new Ticket { TicketId = r.GetInt32(0), BookingId = r.GetInt32(1), BookingReference = r.GetString(2), PassengerId = r.GetInt32(3), PassengerName = r.GetString(4), FlightId = r.GetInt32(5), FlightNumber = r.GetString(6), SeatNumber = r.GetString(7), TicketClass = r.GetString(8), Fare = r.GetDecimal(9), IssuedDate = r.GetDateTime(10) },
        null, ct);

    public Task InsertTicketAsync(Ticket t, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.Tickets (BookingId, PassengerId, FlightId, SeatNumber, TicketClass, Fare) VALUES (@bid,@pid,@fid,@seat,@class,@fare);",
        c => BindTicket(c, t), ct);

    public Task UpdateTicketAsync(Ticket t, CancellationToken ct = default) => ExecAsync(
        "UPDATE dbo.Tickets SET BookingId=@bid, PassengerId=@pid, FlightId=@fid, SeatNumber=@seat, TicketClass=@class, Fare=@fare WHERE TicketId=@id;",
        c => { c.Parameters.AddWithValue("@id", t.TicketId); BindTicket(c, t); }, ct);

    public Task DeleteTicketAsync(int id, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.Tickets WHERE TicketId=@id;", c => c.Parameters.AddWithValue("@id", id), ct);

    private static void BindTicket(SqlCommand c, Ticket t)
    {
        c.Parameters.AddWithValue("@bid", t.BookingId);
        c.Parameters.AddWithValue("@pid", t.PassengerId);
        c.Parameters.AddWithValue("@fid", t.FlightId);
        c.Parameters.AddWithValue("@seat", t.SeatNumber);
        c.Parameters.AddWithValue("@class", t.TicketClass);
        c.Parameters.AddWithValue("@fare", t.Fare);
    }

    // ---------------- Payments ----------------
    public Task<List<Payment>> GetPaymentsAsync(CancellationToken ct = default) => QueryAsync(
        @"SELECT pay.PaymentId, pay.BookingId, b.BookingReference, pay.Amount, pay.PaymentDate, pay.Method, pay.Status
          FROM dbo.Payments pay JOIN dbo.Bookings b ON b.BookingId = pay.BookingId
          ORDER BY pay.PaymentDate;",
        r => new Payment { PaymentId = r.GetInt32(0), BookingId = r.GetInt32(1), BookingReference = r.GetString(2), Amount = r.GetDecimal(3), PaymentDate = r.GetDateTime(4), Method = r.GetString(5), Status = r.GetString(6) },
        null, ct);

    public Task InsertPaymentAsync(Payment p, CancellationToken ct = default) => ExecAsync(
        "INSERT INTO dbo.Payments (BookingId, Amount, PaymentDate, Method, Status) VALUES (@bid,@amt,@date,@method,@status);",
        c => BindPayment(c, p), ct);

    public Task UpdatePaymentAsync(Payment p, CancellationToken ct = default) => ExecAsync(
        "UPDATE dbo.Payments SET BookingId=@bid, Amount=@amt, PaymentDate=@date, Method=@method, Status=@status WHERE PaymentId=@id;",
        c => { c.Parameters.AddWithValue("@id", p.PaymentId); BindPayment(c, p); }, ct);

    public Task DeletePaymentAsync(int id, CancellationToken ct = default) => ExecAsync(
        "DELETE FROM dbo.Payments WHERE PaymentId=@id;", c => c.Parameters.AddWithValue("@id", id), ct);

    private static void BindPayment(SqlCommand c, Payment p)
    {
        c.Parameters.AddWithValue("@bid", p.BookingId);
        c.Parameters.AddWithValue("@amt", p.Amount);
        c.Parameters.AddWithValue("@date", p.PaymentDate);
        c.Parameters.AddWithValue("@method", p.Method);
        c.Parameters.AddWithValue("@status", p.Status);
    }

    // ---------------- Lookups for dropdowns ----------------
    public Task<List<LookupItem>> GetAirportLookupAsync(CancellationToken ct = default) => QueryAsync(
        "SELECT AirportId, City + ' (' + IataCode + ')' FROM dbo.Airports ORDER BY City;",
        r => new LookupItem { Id = r.GetInt32(0), Display = r.GetString(1) }, null, ct);

    public Task<List<LookupItem>> GetPassengerLookupAsync(CancellationToken ct = default) => QueryAsync(
        "SELECT PassengerId, FirstName + ' ' + LastName + ' (' + PassportNumber + ')' FROM dbo.Passengers ORDER BY LastName;",
        r => new LookupItem { Id = r.GetInt32(0), Display = r.GetString(1) }, null, ct);

    public Task<List<LookupItem>> GetFlightLookupAsync(CancellationToken ct = default) => QueryAsync(
        @"SELECT f.FlightId, f.FlightNumber + ' (' + dep.IataCode + '->' + arr.IataCode + ')'
          FROM dbo.Flights f JOIN dbo.Airports dep ON dep.AirportId=f.DepartureAirportId
          JOIN dbo.Airports arr ON arr.AirportId=f.ArrivalAirportId ORDER BY f.FlightNumber;",
        r => new LookupItem { Id = r.GetInt32(0), Display = r.GetString(1) }, null, ct);

    public Task<List<LookupItem>> GetBookingLookupAsync(CancellationToken ct = default) => QueryAsync(
        "SELECT BookingId, BookingReference FROM dbo.Bookings ORDER BY BookingReference;",
        r => new LookupItem { Id = r.GetInt32(0), Display = r.GetString(1) }, null, ct);

    /// <summary>Passengers listed on a given booking (valid ticket holders for that booking).</summary>
    public Task<List<LookupItem>> GetTravellerLookupAsync(int bookingId, CancellationToken ct = default) => QueryAsync(
        @"SELECT p.PassengerId, p.FirstName + ' ' + p.LastName
          FROM dbo.BookingPassengers bp JOIN dbo.Passengers p ON p.PassengerId=bp.PassengerId
          WHERE bp.BookingId=@bid ORDER BY p.LastName;",
        r => new LookupItem { Id = r.GetInt32(0), Display = r.GetString(1) },
        c => c.Parameters.AddWithValue("@bid", bookingId), ct);
}
