/* =====================================================================
   Script 08 - Join Tables and Reports (Task 10, PA0205)
   Demonstrates INNER JOIN, LEFT JOIN, GROUP BY, COUNT and SUM to
   produce business reports.
   ===================================================================== */

USE [SAA_FlightBooking];
GO

-- 10.1 Passenger bookings (INNER JOIN across booking, traveller, ticket, flight)
SELECT  b.BookingReference               AS [Reference],
        pax.FirstName + ' ' + pax.LastName AS [Passenger],
        f.FlightNumber                   AS [Flight],
        dep.City                         AS [From],
        arr.City                         AS [To],
        t.SeatNumber                     AS [Seat],
        t.TicketClass                    AS [Class],
        t.Fare                           AS [Fare (ZAR)]
FROM    dbo.Tickets t
JOIN    dbo.Bookings b   ON b.BookingId   = t.BookingId
JOIN    dbo.Passengers pax ON pax.PassengerId = t.PassengerId
JOIN    dbo.Flights f    ON f.FlightId    = t.FlightId
JOIN    dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
JOIN    dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
ORDER BY b.BookingReference, [Passenger];
GO

-- 10.2 Flight manifest: passengers booked on each flight
SELECT  f.FlightNumber                    AS [Flight],
        dep.City + ' -> ' + arr.City      AS [Route],
        pax.FirstName + ' ' + pax.LastName AS [Passenger],
        t.SeatNumber                      AS [Seat]
FROM    dbo.Flights f
JOIN    dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
JOIN    dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
LEFT JOIN dbo.Tickets t   ON t.FlightId   = f.FlightId
LEFT JOIN dbo.Passengers pax ON pax.PassengerId = t.PassengerId
ORDER BY f.FlightNumber, t.SeatNumber;
GO

-- 10.3 Revenue generated per flight (LEFT JOIN keeps flights with no tickets)
SELECT  f.FlightNumber                       AS [Flight],
        dep.City + ' -> ' + arr.City         AS [Route],
        COUNT(t.TicketId)                    AS [Tickets Issued],
        ISNULL(SUM(t.Fare), 0)               AS [Revenue (ZAR)]
FROM    dbo.Flights f
JOIN    dbo.Airports dep ON dep.AirportId = f.DepartureAirportId
JOIN    dbo.Airports arr ON arr.AirportId = f.ArrivalAirportId
LEFT JOIN dbo.Tickets t ON t.FlightId = f.FlightId
GROUP BY f.FlightNumber, dep.City, arr.City
ORDER BY [Revenue (ZAR)] DESC;
GO

-- 10.4 Tickets issued per passenger
SELECT  pax.FirstName + ' ' + pax.LastName AS [Passenger],
        COUNT(t.TicketId)                  AS [Tickets Issued],
        ISNULL(SUM(t.Fare), 0)             AS [Total Spent (ZAR)]
FROM    dbo.Passengers pax
LEFT JOIN dbo.Tickets t ON t.PassengerId = pax.PassengerId
GROUP BY pax.FirstName, pax.LastName
ORDER BY [Tickets Issued] DESC, [Passenger];
GO

-- 10.5 Overall booking summary
-- Pre-aggregated derived tables avoid join fan-out (counting each
-- traveller against each ticket) so totals per booking are accurate.
SELECT  b.BookingReference        AS [Reference],
        b.Status                  AS [Booking Status],
        ISNULL(bp.Travellers, 0)  AS [Travellers],
        ISNULL(tk.Tickets, 0)     AS [Tickets],
        ISNULL(tk.TicketValue, 0) AS [Ticket Value (ZAR)],
        ISNULL(pm.AmountPaid, 0)  AS [Amount Paid (ZAR)]
FROM    dbo.Bookings b
LEFT JOIN (SELECT BookingId, COUNT(*) AS Travellers
           FROM dbo.BookingPassengers GROUP BY BookingId) bp ON bp.BookingId = b.BookingId
LEFT JOIN (SELECT BookingId, COUNT(*) AS Tickets, SUM(Fare) AS TicketValue
           FROM dbo.Tickets GROUP BY BookingId) tk ON tk.BookingId = b.BookingId
LEFT JOIN (SELECT BookingId, SUM(Amount) AS AmountPaid
           FROM dbo.Payments GROUP BY BookingId) pm ON pm.BookingId = b.BookingId
ORDER BY b.BookingReference;
GO
