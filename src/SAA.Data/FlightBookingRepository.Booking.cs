using System.Data;
using Microsoft.Data.SqlClient;

namespace SAA.Data;

/// <summary>
/// Customer booking flow. A purchase writes a Booking, its BookingPassengers,
/// a Ticket per traveller and one Payment inside a single transaction, so a
/// failure at any step rolls the whole purchase back and leaves no orphans.
/// </summary>
public sealed partial class FlightBookingRepository
{
    private static readonly char[] SeatColumns = { 'A', 'B', 'C', 'D', 'E', 'F' };

    public async Task<BookingResult> CreateWebBookingAsync(BookingRequest request, CancellationToken ct = default)
    {
        if (request.Passengers.Count == 0)
            throw new DataException("Add at least one traveller before confirming the booking.");

        try
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);

            var reference = "WEB" + DateTime.Now.ToString("yyMMddHHmmss");
            var passengerIds = new List<int>();
            foreach (var p in request.Passengers)
                passengerIds.Add(await UpsertPassengerAsync(conn, tx, p, ct));

            var bookingId = await InsertScalarAsync(conn, tx,
                "INSERT INTO dbo.Bookings (BookingReference, PassengerId, BookingDate, Status) " +
                "VALUES (@ref, @pid, SYSDATETIME(), N'Confirmed'); SELECT CAST(SCOPE_IDENTITY() AS int);",
                c =>
                {
                    c.Parameters.AddWithValue("@ref", reference);
                    c.Parameters.AddWithValue("@pid", passengerIds[0]);
                }, ct);

            foreach (var pid in passengerIds.Distinct())
                await ExecInTxAsync(conn, tx,
                    "INSERT INTO dbo.BookingPassengers (BookingId, PassengerId) VALUES (@bid, @pid);",
                    c => { c.Parameters.AddWithValue("@bid", bookingId); c.Parameters.AddWithValue("@pid", pid); }, ct);

            var seatStart = await InsertScalarAsync(conn, tx,
                "SELECT COUNT(*) FROM dbo.Tickets WHERE FlightId = @fid;",
                c => c.Parameters.AddWithValue("@fid", request.FlightId), ct);

            var seats = new List<string>();
            for (var i = 0; i < passengerIds.Count; i++)
            {
                var seat = SeatFor(seatStart + i);
                seats.Add(seat);
                await ExecInTxAsync(conn, tx,
                    "INSERT INTO dbo.Tickets (BookingId, PassengerId, FlightId, SeatNumber, TicketClass, Fare) " +
                    "VALUES (@bid, @pid, @fid, @seat, @class, @fare);",
                    c =>
                    {
                        c.Parameters.AddWithValue("@bid", bookingId);
                        c.Parameters.AddWithValue("@pid", passengerIds[i]);
                        c.Parameters.AddWithValue("@fid", request.FlightId);
                        c.Parameters.AddWithValue("@seat", seat);
                        c.Parameters.AddWithValue("@class", request.TicketClass);
                        c.Parameters.AddWithValue("@fare", request.FarePerTicket);
                    }, ct);
            }

            var amount = request.FarePerTicket * passengerIds.Count;
            await ExecInTxAsync(conn, tx,
                "INSERT INTO dbo.Payments (BookingId, Amount, PaymentDate, Method, Status) " +
                "VALUES (@bid, @amt, SYSDATETIME(), @method, N'Paid');",
                c =>
                {
                    c.Parameters.AddWithValue("@bid", bookingId);
                    c.Parameters.AddWithValue("@amt", amount);
                    c.Parameters.AddWithValue("@method", request.PaymentMethod);
                }, ct);

            await tx.CommitAsync(ct);
            return new BookingResult { BookingReference = reference, TicketCount = passengerIds.Count, AmountPaid = amount, Seats = seats };
        }
        catch (SqlException ex)
        {
            throw Translate(ex);
        }
    }

    /// <summary>Reuse an existing passenger (matched on passport) or create a new one.</summary>
    private static async Task<int> UpsertPassengerAsync(SqlConnection conn, SqlTransaction tx, PassengerInput p, CancellationToken ct)
    {
        var existing = await InsertScalarNullableAsync(conn, tx,
            "SELECT PassengerId FROM dbo.Passengers WHERE PassportNumber = @pass;",
            c => c.Parameters.AddWithValue("@pass", p.PassportNumber), ct);
        if (existing.HasValue) return existing.Value;

        return await InsertScalarAsync(conn, tx,
            "INSERT INTO dbo.Passengers (FirstName, LastName, PassportNumber, Email, Phone, DateOfBirth) " +
            "VALUES (@f, @l, @pass, @e, @ph, @dob); SELECT CAST(SCOPE_IDENTITY() AS int);",
            c =>
            {
                c.Parameters.AddWithValue("@f", p.FirstName);
                c.Parameters.AddWithValue("@l", p.LastName);
                c.Parameters.AddWithValue("@pass", p.PassportNumber);
                c.Parameters.AddWithValue("@e", p.Email);
                c.Parameters.AddWithValue("@ph", (object?)p.Phone ?? DBNull.Value);
                c.Parameters.AddWithValue("@dob", (object?)p.DateOfBirth ?? DBNull.Value);
            }, ct);
    }

    private static string SeatFor(int index)
    {
        var row = index / SeatColumns.Length + 1;
        var col = SeatColumns[index % SeatColumns.Length];
        return $"{row}{col}";
    }

    private static async Task ExecInTxAsync(SqlConnection conn, SqlTransaction tx, string sql, Action<SqlCommand> bind, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, conn, tx);
        bind(cmd);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> InsertScalarAsync(SqlConnection conn, SqlTransaction tx, string sql, Action<SqlCommand> bind, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, conn, tx);
        bind(cmd);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    private static async Task<int?> InsertScalarNullableAsync(SqlConnection conn, SqlTransaction tx, string sql, Action<SqlCommand> bind, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(sql, conn, tx);
        bind(cmd);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }
}
